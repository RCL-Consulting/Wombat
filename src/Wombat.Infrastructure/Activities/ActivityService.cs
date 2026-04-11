using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Infrastructure.Activities;

public sealed class ActivityService : IActivityService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ISchemaValidator _schemaValidator;
    private readonly IWorkflowEvaluator _workflowEvaluator;
    private readonly ICreditApplier _creditApplier;

    public ActivityService(
        IApplicationDbContext dbContext,
        ISchemaValidator schemaValidator,
        IWorkflowEvaluator workflowEvaluator,
        ICreditApplier creditApplier)
    {
        _dbContext = dbContext;
        _schemaValidator = schemaValidator;
        _workflowEvaluator = workflowEvaluator;
        _creditApplier = creditApplier;
    }

    public async Task<ActivityDto> CreateDraftAsync(CreateActivityInput input, CancellationToken cancellationToken = default)
    {
        var activityType = await _dbContext.Set<ActivityType>()
            .SingleOrDefaultAsync(entity => entity.Id == input.ActivityTypeId, cancellationToken)
            ?? throw new InvalidOperationException("The activity type could not be found.");

        var schema = FormSchemaParser.Parse(activityType.SchemaJson);
        var workflow = WorkflowParser.Parse(activityType.WorkflowJson);
        var normalizedDataJson = NormalizeObjectJson(input.InitialDataJson);
        ThrowIfInvalid(_schemaValidator.Validate(schema, normalizedDataJson, SchemaValidationMode.Draft));

        var utcNow = DateTime.UtcNow;
        var activity = new Activity
        {
            ActivityTypeId = activityType.Id,
            ActivityType = activityType,
            SchemaVersion = activityType.Version,
            SubjectUserId = input.SubjectUserId.Trim(),
            CreatedByUserId = input.CreatedByUserId.Trim(),
            CurrentState = workflow.InitialState,
            DataJson = normalizedDataJson,
            CreatedOn = utcNow,
            UpdatedOn = utcNow
        };

        activity.Transitions.Add(new ActivityTransition
        {
            FromState = workflow.InitialState,
            ToState = workflow.InitialState,
            TransitionKey = "create",
            ActorUserId = input.CreatedByUserId.Trim(),
            OccurredOn = utcNow,
            SnapshotJson = normalizedDataJson
        });

        _dbContext.Set<Activity>().Add(activity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(activity);
    }

    public async Task<ActivityDto> UpdateDraftAsync(UpdateActivityDraftInput input, CancellationToken cancellationToken = default)
    {
        var activity = await LoadActivityAsync(input.ActivityId, cancellationToken);
        var workflow = WorkflowParser.Parse(activity.ActivityType.WorkflowJson);
        var currentState = workflow.States.Single(state => string.Equals(state.Key, activity.CurrentState, StringComparison.Ordinal));

        if (currentState.Terminal)
        {
            throw new InvalidOperationException("Terminal activities cannot be edited.");
        }

        if (!CanEditDraft(activity, input.ActorUserId))
        {
            throw new InvalidOperationException("The current actor is not allowed to edit this activity.");
        }

        var schema = FormSchemaParser.Parse(activity.ActivityType.SchemaJson);
        var normalizedDataJson = NormalizeObjectJson(input.NewDataJson);
        ThrowIfInvalid(_schemaValidator.Validate(schema, normalizedDataJson, SchemaValidationMode.Draft));

        activity.DataJson = normalizedDataJson;
        activity.UpdatedOn = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(activity);
    }

    public async Task<ActivityDto> TransitionAsync(TransitionActivityInput input, CancellationToken cancellationToken = default)
    {
        var activity = await LoadActivityAsync(input.ActivityId, cancellationToken);
        var schema = FormSchemaParser.Parse(activity.ActivityType.SchemaJson);
        var workflow = WorkflowParser.Parse(activity.ActivityType.WorkflowJson);
        var transition = workflow.Transitions.SingleOrDefault(candidate =>
            string.Equals(candidate.Key, input.TransitionKey, StringComparison.Ordinal) &&
            candidate.From.Contains(activity.CurrentState, StringComparer.Ordinal))
            ?? throw new InvalidOperationException(
                $"Transition '{input.TransitionKey}' is not available from state '{activity.CurrentState}'.");

        var decision = _workflowEvaluator.Evaluate(workflow, activity, input.TransitionKey, input.Principal);
        if (!decision.Allowed)
        {
            throw new InvalidOperationException(decision.Reason ?? "The current actor is not allowed to perform this transition.");
        }

        if (transition.RequiresNote && string.IsNullOrWhiteSpace(input.Note))
        {
            throw new InvalidOperationException($"Transition '{input.TransitionKey}' requires a note.");
        }

        var mergedDataJson = string.IsNullOrWhiteSpace(input.DataPatchJson)
            ? activity.DataJson
            : MergeJsonObjects(activity.DataJson, input.DataPatchJson);

        ThrowIfInvalid(_schemaValidator.Validate(
            schema,
            mergedDataJson,
            SchemaValidationMode.Submit,
            transition.RequiresFields));

        activity.ApplyTransition(input.TransitionKey, input.ActorUserId, mergedDataJson, input.Note);

        var targetState = workflow.States.Single(state => string.Equals(state.Key, transition.To, StringComparison.Ordinal));
        if (targetState.Terminal)
        {
            await _creditApplier.ApplyAsync(activity, activity.ActivityType, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(activity);
    }

    public async Task<ActivityDto> GetAsync(int activityId, CancellationToken cancellationToken = default)
        => Map(await LoadActivityAsync(activityId, cancellationToken));

    private async Task<Activity> LoadActivityAsync(int activityId, CancellationToken cancellationToken)
    {
        return await _dbContext.Set<Activity>()
            .Include(entity => entity.ActivityType)
            .Include(entity => entity.Transitions)
            .SingleOrDefaultAsync(entity => entity.Id == activityId, cancellationToken)
            ?? throw new InvalidOperationException("The activity could not be found.");
    }

    private static bool CanEditDraft(Activity activity, string actorUserId)
        => string.Equals(activity.SubjectUserId, actorUserId, StringComparison.Ordinal) ||
           string.Equals(activity.CreatedByUserId, actorUserId, StringComparison.Ordinal);

    private static void ThrowIfInvalid(IReadOnlyList<ActivityValidationErrorDto> validationErrors)
    {
        if (validationErrors.Count == 0)
        {
            return;
        }

        var message = string.Join("; ", validationErrors.Select(error =>
            error.FieldKey is null
                ? error.Message
                : $"{error.FieldKey}: {error.Message}"));

        throw new InvalidOperationException(message);
    }

    private static string NormalizeObjectJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Activity data must be a JSON object.");
        }

        return JsonSerializer.Serialize(document.RootElement);
    }

    private static string MergeJsonObjects(string currentJson, string patchJson)
    {
        using var currentDocument = JsonDocument.Parse(currentJson);
        using var patchDocument = JsonDocument.Parse(patchJson);

        if (currentDocument.RootElement.ValueKind != JsonValueKind.Object ||
            patchDocument.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Activity data and patch data must both be JSON objects.");
        }

        var merged = currentDocument.RootElement.EnumerateObject()
            .ToDictionary(property => property.Name, property => property.Value.Clone(), StringComparer.Ordinal);

        foreach (var property in patchDocument.RootElement.EnumerateObject())
        {
            merged[property.Name] = property.Value.Clone();
        }

        return JsonSerializer.Serialize(merged);
    }

    private static ActivityDto Map(Activity activity)
    {
        return new ActivityDto(
            activity.Id,
            activity.ActivityTypeId,
            activity.ActivityType.Key,
            activity.ActivityType.Name,
            activity.SchemaVersion,
            activity.SubjectUserId,
            activity.CreatedByUserId,
            activity.CurrentState,
            activity.DataJson,
            activity.EpaId,
            activity.CurriculumItemId,
            activity.CreatedOn,
            activity.UpdatedOn,
            activity.Transitions
                .OrderBy(entity => entity.OccurredOn)
                .Select(entity => new ActivityTransitionDto(
                    entity.Id,
                    entity.FromState,
                    entity.ToState,
                    entity.TransitionKey,
                    entity.ActorUserId,
                    entity.OccurredOn,
                    entity.Note,
                    entity.SnapshotJson))
                .ToList());
    }
}
