using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Infrastructure.Activities;

public sealed class ActivityService : IActivityService
{
    private const string EmptyObjectJson = "{}";

    private readonly IApplicationDbContext _dbContext;
    private readonly ISchemaValidator _schemaValidator;
    private readonly IWorkflowEvaluator _workflowEvaluator;
    private readonly ICreditApplier _creditApplier;
    private readonly IFieldPermissionEvaluator _fieldPermissionEvaluator;

    public ActivityService(
        IApplicationDbContext dbContext,
        ISchemaValidator schemaValidator,
        IWorkflowEvaluator workflowEvaluator,
        ICreditApplier creditApplier,
        IFieldPermissionEvaluator fieldPermissionEvaluator)
    {
        _dbContext = dbContext;
        _schemaValidator = schemaValidator;
        _workflowEvaluator = workflowEvaluator;
        _creditApplier = creditApplier;
        _fieldPermissionEvaluator = fieldPermissionEvaluator;
    }

    public async Task<ActivityDto> CreateDraftAsync(CreateActivityInput input, CancellationToken cancellationToken = default)
    {
        var activityType = await _dbContext.Set<ActivityType>()
            .Include(entity => entity.Versions)
            .SingleOrDefaultAsync(entity => entity.Id == input.ActivityTypeId, cancellationToken)
            ?? throw new InvalidOperationException("The activity type could not be found.");

        if (activityType.Version <= 0 || string.IsNullOrWhiteSpace(activityType.SchemaJson) || string.IsNullOrWhiteSpace(activityType.WorkflowJson))
        {
            throw new InvalidOperationException("The selected activity type has not been published yet.");
        }

        var schema = FormSchemaParser.Parse(activityType.SchemaJson);
        var workflow = WorkflowParser.Parse(activityType.WorkflowJson);
        var submittedDataJson = NormalizeObjectJson(input.InitialDataJson);

        var utcNow = DateTime.UtcNow;
        var activity = new Activity
        {
            ActivityTypeId = activityType.Id,
            ActivityType = activityType,
            SchemaVersion = activityType.Version,
            SubjectUserId = input.SubjectUserId.Trim(),
            CreatedByUserId = input.CreatedByUserId.Trim(),
            CurrentState = workflow.InitialState,
            // Deliberately EMPTY while the writable set is computed below. See the note there.
            DataJson = EmptyObjectJson,
            CreatedOn = utcNow,
            UpdatedOn = utcNow
        };

        // T070: the creator may only supply the fields they own, so a trainee cannot pre-fill the
        // assessor's ratings and make `complete` satisfiable before the assessor ever sees the form.
        // Keys outside the writable set (including keys the schema does not declare at all) are
        // dropped silently rather than rejected — a create is not a patch, and rejecting would break
        // every caller that echoes a full form.
        //
        // The permission check runs against EMPTY data, not against what the caller submitted. A
        // `field:` rule reads its answer out of DataJson, so evaluating it against the caller's own
        // payload lets the caller grant themselves the rule: name yourself in `assessor_user_id`
        // and the assessor-owned sections unlock, self-rating and all. At creation nobody is bound
        // by a data field yet, so the only rules that can match here are the ones backed by real
        // columns (`subject`, `creator`), roles and scope.
        //
        // The state gate is ignored here because procedure_log and journal_club declare a terminal
        // initial state; gating on it would make those types uncreatable.
        var writableFieldKeys = _fieldPermissionEvaluator.GetWritableFieldKeys(
            schema,
            workflow,
            activity,
            input.Principal,
            ignoreStateGate: true);

        var normalizedDataJson = FilterToWritableKeys(submittedDataJson, writableFieldKeys);
        ThrowIfActorFieldNamesSubject(schema, workflow, normalizedDataJson, activity.SubjectUserId);
        activity.DataJson = normalizedDataJson;

        ThrowIfInvalid(_schemaValidator.Validate(schema, normalizedDataJson, SchemaValidationMode.Draft));

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
        var version = GetPinnedVersion(activity);
        var workflow = WorkflowParser.Parse(version.WorkflowJson);
        var currentState = workflow.States.Single(state => string.Equals(state.Key, activity.CurrentState, StringComparison.Ordinal));

        if (currentState.Terminal)
        {
            throw new InvalidOperationException("Terminal activities cannot be edited.");
        }

        if (!CanEditDraft(activity, input.ActorUserId))
        {
            throw new InvalidOperationException("The current actor is not allowed to edit this activity.");
        }

        var schema = FormSchemaParser.Parse(version.SchemaJson);
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
        var version = GetPinnedVersion(activity);
        var schema = FormSchemaParser.Parse(version.SchemaJson);
        var workflow = WorkflowParser.Parse(version.WorkflowJson);
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

        // The writable set is computed AFTER the transition gate above and BEFORE the merge below,
        // against the PRE-transition state and the PRE-patch data. Both orderings are load-bearing:
        // a patch must not be able to authorise its own transition, and the actor's permissions are
        // those of the state they are acting from, not the state they are moving to.
        var mergedDataJson = activity.DataJson;
        if (!string.IsNullOrWhiteSpace(input.DataPatchJson))
        {
            var writableFieldKeys = _fieldPermissionEvaluator.GetWritableFieldKeys(
                schema,
                workflow,
                activity,
                input.Principal);

            mergedDataJson = MergeWritableKeys(
                activity.DataJson,
                input.DataPatchJson,
                writableFieldKeys,
                activity.CurrentState);

            // The subject legitimately owns `assessor_user_id` while the request is still theirs to
            // edit, so the merge above will happily accept a patch that points it at themselves —
            // and from the next state on, every `field:assessor_user_id` rule would match them.
            ThrowIfActorFieldNamesSubject(schema, workflow, mergedDataJson, activity.SubjectUserId);
        }

        ThrowIfInvalid(_schemaValidator.Validate(
            schema,
            mergedDataJson,
            SchemaValidationMode.Submit,
            transition.RequiresFields));

        var record = activity.ApplyTransition(workflow, input.TransitionKey, input.ActorUserId, mergedDataJson, input.Note);

        var targetState = workflow.States.Single(state => string.Equals(state.Key, transition.To, StringComparison.Ordinal));
        if (targetState.Terminal && DeclaresCredit(version.CreditRulesJson))
        {
            // Stamp the outcome onto the transition that caused it. Until T108 this result was
            // discarded, which made "credited nothing" indistinguishable from "credited" at every
            // surface in the product: no return value read, no domain event, no log line.
            //
            // The `counts_for` gate is checked BEFORE the call, not after, and that is what stops the
            // signal crying wolf: a reflective note, journal club, procedure log, QI project, research
            // output or teaching session declares an empty `counts_for`, so it is never evaluated and
            // its transition stays null for ever.
            //
            // Both writes land in the SaveChangesAsync below, so the stamp is atomic with the credit
            // it describes.
            //
            // Stamped on the transition path only, and that is a GAP rather than a design.
            // RebuildCurriculumProgress deletes every progress row before replaying
            // (RebuildCurriculumProgressCommand.cs:30-32), so it re-credits properly — meaning a stale
            // zero left by a since-corrected curriculum is never cleared, and the warning banner stays
            // on for ever after the exact remediation it tells the reader to perform. Making the
            // rebuild stamp is the fix; recorded as T106 item 12.
            var credited = await _creditApplier.ApplyAsync(
                activity,
                new ActivityType
                {
                    CreditRulesJson = version.CreditRulesJson,
                    // The pinned schema, not the live one: it declares the `scale_key` of the field the
                    // credit directive gates on, and that is what binds the achieved ordinal to a ladder
                    // (T109). Pinning means the binding cannot drift under the activity.
                    SchemaJson = version.SchemaJson
                },
                cancellationToken);

            record.CreditedItemCount = credited.UpdatedRows.Count;
            record.CreditScaleMismatchCount = credited.ScaleMismatchCount;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(activity);
    }

    public async Task<ActivityDto> GetAsync(int activityId, CancellationToken cancellationToken = default)
        => Map(await LoadActivityAsync(activityId, cancellationToken));

    public async Task<ActivityDetailDto> GetDetailAsync(
        int activityId,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var activity = await LoadActivityAsync(activityId, cancellationToken);
        var version = GetPinnedVersion(activity);
        var schema = FormSchemaParser.Parse(version.SchemaJson);
        var workflow = WorkflowParser.Parse(version.WorkflowJson);

        var writableFieldKeys = _fieldPermissionEvaluator.GetWritableFieldKeys(schema, workflow, activity, principal);

        // Evaluated against the REAL loaded ActivityType, which LoadActivityAsync Includes. The Web
        // layer used to build a synthetic ActivityType for this, defaulting Scope to Global and
        // ScopeId to null, so every `scope:` rule evaluated false in the button list while the server
        // would have allowed the transition. (T070)
        var availableActions = workflow.Transitions
            .Where(transition => transition.From.Contains(activity.CurrentState, StringComparer.Ordinal))
            .Where(transition => _workflowEvaluator.Evaluate(workflow, activity, transition.Key, principal).Allowed)
            .Select(transition => new ActivityActionDto(transition.Key, transition.RequiresNote))
            .ToList();

        return new ActivityDetailDto(
            Map(activity),
            OrderBySchema(schema, writableFieldKeys),
            availableActions);
    }

    /// <summary>
    /// Whether the pinned version declares any credit at all — the anti-cry-wolf gate for the T108
    /// signal, and the same test <c>CreditApplier</c> makes first thing.
    /// </summary>
    /// <remarks>
    /// Blank rules count as "credits nothing" rather than an error: a type published without a credit
    /// block should still be completable, and before T108 an empty string here threw out of the
    /// terminal transition.
    /// </remarks>
    private static bool DeclaresCredit(string creditRulesJson)
        => !string.IsNullOrWhiteSpace(creditRulesJson) &&
           CreditRulesParser.Parse(creditRulesJson).CountsFor.Count > 0;

    /// <summary>Writable keys in schema declaration order, so the UI can render them predictably.</summary>
    private static IReadOnlyList<string> OrderBySchema(FormSchema schema, IReadOnlySet<string> writableFieldKeys)
        => schema.Sections
            .SelectMany(section => section.Fields)
            .Select(field => field.Key)
            .Where(writableFieldKeys.Contains)
            .ToList();

    private async Task<Activity> LoadActivityAsync(int activityId, CancellationToken cancellationToken)
    {
        return await _dbContext.Set<Activity>()
            .Include(entity => entity.ActivityType)
                .ThenInclude(activityType => activityType.Versions)
            .Include(entity => entity.Transitions)
            .SingleOrDefaultAsync(entity => entity.Id == activityId, cancellationToken)
            ?? throw new InvalidOperationException("The activity could not be found.");
    }

    private static ActivityTypeVersion GetPinnedVersion(Activity activity)
    {
        return activity.ActivityType.Versions.SingleOrDefault(entity => entity.Version == activity.SchemaVersion)
            ?? throw new InvalidOperationException(
                $"The published activity type version '{activity.SchemaVersion}' could not be found.");
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

    /// <summary>
    /// Refuses data in which a field that decides who may act on the activity names the activity's
    /// own subject.
    /// </summary>
    /// <remarks>
    /// A <c>field:</c> rule — <c>field:assessor_user_id</c> on the CPSA seeds — makes whoever that
    /// field names the actor for a transition and, since T070, the owner of the fields that
    /// transition fills in. The subject legitimately owns that field while the request is still
    /// theirs to edit, which is the whole escalation: name yourself, and you may rate yourself and
    /// take your own <c>complete</c>, awarding your own curriculum credit. Nothing else in the
    /// pipeline catches it — <c>SchemaValidator</c> treats a <c>user</c> field as a plain string
    /// and the assessor picker is a UI affordance, not a check.
    ///
    /// This is the narrow guard. The general one — validating a <c>user</c> value against the users
    /// the caller may legitimately nominate — is T102.
    /// </remarks>
    private static void ThrowIfActorFieldNamesSubject(
        FormSchema schema,
        Workflow workflow,
        string dataJson,
        string subjectUserId)
    {
        if (string.IsNullOrWhiteSpace(subjectUserId))
        {
            return;
        }

        var actorFieldNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var transition in workflow.Transitions)
        {
            ActorRuleMatcher.CollectUserFieldNames(transition.Actor, actorFieldNames);
        }

        foreach (var state in workflow.States)
        {
            ActorRuleMatcher.CollectUserFieldNames(state.EditableBy, actorFieldNames);
        }

        foreach (var section in schema.Sections)
        {
            ActorRuleMatcher.CollectUserFieldNames(section.EditableBy, actorFieldNames);
            foreach (var field in section.Fields)
            {
                ActorRuleMatcher.CollectUserFieldNames(field.EditableBy, actorFieldNames);
            }
        }

        foreach (var fieldName in actorFieldNames)
        {
            var value = ActorRuleMatcher.ReadUserFieldValue(dataJson, fieldName);
            if (string.Equals(value, subjectUserId, StringComparison.Ordinal))
            {
                var label = schema.Sections
                    .SelectMany(section => section.Fields)
                    .FirstOrDefault(field => string.Equals(field.Key, fieldName, StringComparison.Ordinal))
                    ?.Label ?? fieldName;

                throw new InvalidOperationException(
                    $"{label}: this decides who may act on the activity, so it cannot name the person the activity is about.");
            }
        }
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

    /// <summary>
    /// Shallow top-level merge restricted to the keys this actor owns (T070).
    /// </summary>
    /// <remarks>
    /// A key the actor may write is applied. A key they may not write is ignored when the patch
    /// carries the value already stored — full-form post-backs echo every field, including locked
    /// ones, and without that carve-out the rule would fire on untouched data. A key they may not
    /// write whose value actually differs throws: that is the case where an assessor's `complete`
    /// patch would otherwise rewrite <c>epa_id</c> or <c>assessor_user_id</c> and redirect which
    /// curriculum item gets credited.
    /// </remarks>
    private static string MergeWritableKeys(
        string currentJson,
        string patchJson,
        IReadOnlySet<string> writableFieldKeys,
        string currentState)
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
            if (writableFieldKeys.Contains(property.Name))
            {
                merged[property.Name] = property.Value.Clone();
                continue;
            }

            var isUnchanged = merged.TryGetValue(property.Name, out var storedValue) &&
                              JsonElement.DeepEquals(storedValue, property.Value);

            if (!isUnchanged)
            {
                throw new InvalidOperationException(
                    $"Field '{property.Name}' cannot be written in state '{currentState}' by the current actor.");
            }
        }

        return JsonSerializer.Serialize(merged);
    }

    /// <summary>
    /// Drops every key the actor does not own. Used at creation, where silently ignoring an
    /// unowned key is right (see <see cref="CreateDraftAsync" />).
    /// </summary>
    private static string FilterToWritableKeys(string dataJson, IReadOnlySet<string> writableFieldKeys)
    {
        using var document = JsonDocument.Parse(dataJson);

        var retained = document.RootElement.EnumerateObject()
            .Where(property => writableFieldKeys.Contains(property.Name))
            .ToDictionary(property => property.Name, property => property.Value.Clone(), StringComparer.Ordinal);

        return JsonSerializer.Serialize(retained);
    }

    private static ActivityDto Map(Activity activity)
    {
        var pinnedVersion = GetPinnedVersion(activity);

        return new ActivityDto(
            activity.Id,
            activity.ActivityTypeId,
            activity.ActivityType.Key,
            activity.ActivityType.Name,
            activity.SchemaVersion,
            pinnedVersion.SchemaJson,
            pinnedVersion.WorkflowJson,
            pinnedVersion.DisplayFieldsJson,
            pinnedVersion.CreditRulesJson,
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
                    entity.SnapshotJson,
                    entity.CreditedItemCount,
                    entity.CreditScaleMismatchCount))
                .ToList());
    }
}
