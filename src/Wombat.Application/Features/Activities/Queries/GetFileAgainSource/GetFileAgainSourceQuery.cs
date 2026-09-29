using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Users;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Application.Features.Activities.Queries.GetFileAgainSource;

/// <summary>
/// What Log an activity prefills when a registrar files a declined request again (T342, B10, E5): the page's
/// <c>?from=&lt;id&gt;</c>. Nothing is saved; the page starts a new activity from it, and the create judges it as it
/// judges any other.
/// </summary>
/// <remarks>
/// <para>
/// Null, and the page then ignores <c>?from</c>, unless the caller is the source's SUBJECT and the source was DECLINED
/// (E5: a cancelled draft was her own choice, and she starts again). "Declined" is read from the workflow, not from a
/// state key: the source's current state is a dead end (no move leaves it) that is not terminal, and the move that put it
/// there was made by someone other than its subject and creator, under a move the author named its maker for: the move
/// the workflow records under that key has a <c>field:</c> arm and no arm of the author's (T342, R6). That is the CPSA
/// <c>decline</c>; it is not a <c>cancel</c>, which the author may make (the legacy Mini-CEX's assessor may cancel too,
/// and that is no decline of anything she asked), nor a programme admin's <c>reject</c> (a <c>role:</c> arm: nobody she
/// named), nor a completion, which is terminal. The source is read under its PINNED
/// workflow, the one that recorded the decline.
/// </para>
/// <para>
/// The copy is made for the type's CURRENT published version, which the new activity will be pinned to:
/// <list type="bullet">
///   <item>the fields the author may write in the initial state, computed as <c>/activities/new</c> computes them
///   (<see cref="IFieldPermissionEvaluator.GetWritableFieldKeys" /> with <c>ignoreStateGate: true</c>, for an activity in
///   the initial state with no data). A key the current version lacks is not a field of it, so it is not copied; an
///   assessor's field is not the author's, so it is not copied either;</item>
///   <item>minus the nominee the next move hands the activity to (<see cref="MoveHandOff.NomineeFieldFor(Workflow, FormSchema, WorkflowTransition)" />,
///   over the author's moves out of the initial state that lead on): she names someone else;</item>
///   <item>minus an EPA the Log page's own picker would not offer: a paused EPA (D48) or one the instrument may not be
///   filed on (T122). The picker is asked (<see cref="IActivityReferenceDataService.GetEpaOptionsAsync" />, with the
///   narrowing the page uses), so the copy cannot prefill a value the page would render as nothing.</item>
/// </list>
/// </para>
/// </remarks>
public sealed record GetFileAgainSourceQuery(int SourceActivityId, ClaimsPrincipal Principal)
    : IRequest<FileAgainSourceDto?>;

/// <summary>What Log an activity prefills from a declined request (T342, B10, E5).</summary>
/// <param name="SourceActivityId">The declined request.</param>
/// <param name="ActivityTypeId">Its type: the one Log an activity selects.</param>
/// <param name="ActivityTypeKey">That type's key.</param>
/// <param name="DataJson">
/// The prefill, a JSON object holding only the copied fields, for the type's current published version.
/// </param>
/// <param name="CopiedFieldKeys">The keys in <paramref name="DataJson" />, in schema order.</param>
/// <param name="EpaDropped">
/// True when the source named an EPA the page would not offer now (paused, or not for this instrument), so none is
/// copied: the notice's "The EPA, date and request are as you filed them" is then not true of the EPA.
/// </param>
/// <param name="DeclinedByName">
/// Who declined it, by name: "Copied from your request to Fatima Khumalo, which was declined." Their id when they have no
/// name on record (<see cref="UserDisplayNames.NameOf" />).
/// </param>
/// <param name="DeclinedOn">When the decline was recorded.</param>
public sealed record FileAgainSourceDto(
    int SourceActivityId,
    int ActivityTypeId,
    string ActivityTypeKey,
    string DataJson,
    IReadOnlyList<string> CopiedFieldKeys,
    bool EpaDropped,
    string DeclinedByName,
    DateTime DeclinedOn);

public sealed class GetFileAgainSourceQueryValidator : AbstractValidator<GetFileAgainSourceQuery>
{
    public GetFileAgainSourceQueryValidator()
    {
        RuleFor(query => query.SourceActivityId).GreaterThan(0);
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class GetFileAgainSourceQueryHandler : IRequestHandler<GetFileAgainSourceQuery, FileAgainSourceDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IFieldPermissionEvaluator _fieldPermissionEvaluator;
    private readonly IActivityReferenceDataService _referenceData;
    private readonly IUserAdministrationService _users;

    public GetFileAgainSourceQueryHandler(
        IApplicationDbContext dbContext,
        IFieldPermissionEvaluator fieldPermissionEvaluator,
        IActivityReferenceDataService referenceData,
        IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _fieldPermissionEvaluator = fieldPermissionEvaluator;
        _referenceData = referenceData;
        _users = users;
    }

    public async Task<FileAgainSourceDto?> Handle(GetFileAgainSourceQuery request, CancellationToken cancellationToken)
    {
        var callerId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(callerId))
        {
            return null;
        }

        // The read rule first (T101), then the narrower one below: only the subject files it again.
        var source = await _dbContext.Set<Activity>()
            .AsNoTracking()
            .WhereReadableBy(request.Principal)
            .Include(activity => activity.ActivityType)
                .ThenInclude(activityType => activityType.Versions)
            .Include(activity => activity.Transitions)
            .SingleOrDefaultAsync(activity => activity.Id == request.SourceActivityId, cancellationToken);

        // Only its subject files it again. Anyone else gets the answer a missing id gets.
        if (source is null || !string.Equals(source.SubjectUserId, callerId, StringComparison.Ordinal))
        {
            return null;
        }

        var decline = DeclineOf(source);
        if (decline is null)
        {
            return null;
        }

        var activityType = source.ActivityType;
        if (!activityType.IsActive ||
            activityType.SystemManaged ||
            activityType.Version <= 0 ||
            string.IsNullOrWhiteSpace(activityType.SchemaJson) ||
            string.IsNullOrWhiteSpace(activityType.WorkflowJson))
        {
            return null;
        }

        FormSchema schema;
        Workflow workflow;
        try
        {
            schema = FormSchemaParser.Parse(activityType.SchemaJson);
            workflow = WorkflowParser.Parse(activityType.WorkflowJson);
        }
        catch (Exception exception) when (exception is SchemaParseException or WorkflowParseException or JsonException)
        {
            return null;
        }

        var copied = await CopyAsync(source, activityType, schema, workflow, request.Principal, callerId, cancellationToken);

        var names = await UserDisplayNames.ResolveAsync(_users, [decline.ActorUserId], cancellationToken);

        return new FileAgainSourceDto(
            source.Id,
            activityType.Id,
            activityType.Key,
            copied.DataJson,
            copied.Keys,
            copied.EpaDropped,
            names.NameOf(decline.ActorUserId),
            decline.OccurredOn);
    }

    /// <summary>
    /// The move that declined the source, or null when it was not declined: see the remarks on the query. Read under the
    /// source's pinned workflow; a pinned version that cannot be read is not a decline anyone can act on.
    /// </summary>
    private static ActivityTransition? DeclineOf(Activity source)
    {
        var pinned = source.ActivityType.Versions.SingleOrDefault(version => version.Version == source.SchemaVersion);
        var workflow = pinned is null ? null : PinnedWorkflows.TryParse(pinned.WorkflowJson);
        if (workflow is null)
        {
            return null;
        }

        var state = workflow.States.FirstOrDefault(candidate =>
            string.Equals(candidate.Key, source.CurrentState, StringComparison.Ordinal));
        if (state is null || state.Terminal || workflow.HasOutgoingTransition(state.Key))
        {
            return null;
        }

        var last = source.Transitions
            .OrderBy(transition => transition.OccurredOn)
            .ThenBy(transition => transition.Id)
            .LastOrDefault();

        if (last is null ||
            !string.Equals(last.ToState, source.CurrentState, StringComparison.Ordinal) ||
            string.Equals(last.ActorUserId, source.SubjectUserId, StringComparison.Ordinal) ||
            string.Equals(last.ActorUserId, source.CreatedByUserId, StringComparison.Ordinal))
        {
            return null;
        }

        // R6: the recorded move must be one only the person she named may make.
        var move = workflow.Transitions.FirstOrDefault(transition =>
            string.Equals(transition.Key, last.TransitionKey, StringComparison.Ordinal) &&
            transition.From.Contains(last.FromState, StringComparer.Ordinal) &&
            string.Equals(transition.To, last.ToState, StringComparison.Ordinal));
        return move is not null && IsNamedPersonsMove(move.Actor) ? last : null;
    }

    /// <summary>
    /// Whether a move's actor rule admits someone the author named (a <c>field:</c> arm, alone or narrowed) and no arm of
    /// the author's own (<see cref="ActivityWaiting.IsAuthorArm" />).
    /// </summary>
    private static bool IsNamedPersonsMove(ActorRule rule)
    {
        var arms = rule is CombinedActorRule { CombinationKind: ActorRuleCombinationKind.Any } any ? any.Rules : [rule];
        return arms.Count > 0 && !arms.Any(ActivityWaiting.IsAuthorArm) && arms.Any(NamesAField);
    }

    private static bool NamesAField(ActorRule arm) => arm switch
    {
        FieldUserActorRule => true,
        CombinedActorRule { CombinationKind: ActorRuleCombinationKind.All } all => all.Rules.Any(NamesAField),
        _ => false
    };

    private async Task<(string DataJson, IReadOnlyList<string> Keys, bool EpaDropped)> CopyAsync(
        Activity source,
        ActivityType activityType,
        FormSchema schema,
        Workflow workflow,
        ClaimsPrincipal principal,
        string callerId,
        CancellationToken cancellationToken)
    {
        // The activity the page would create: in the initial state, with no data (DataJson's own "{}"), filed by the caller about themselves,
        // stamped with the scope the source was (the same subject). Never added to the context.
        var candidate = new Activity
        {
            ActivityTypeId = activityType.Id,
            ActivityType = activityType,
            SchemaVersion = activityType.Version,
            SubjectUserId = callerId,
            CreatedByUserId = callerId,
            CurrentState = workflow.InitialState,
            InstitutionId = source.InstitutionId,
            SpecialityId = source.SpecialityId,
            SubSpecialityId = source.SubSpecialityId
        };

        var writable = _fieldPermissionEvaluator.GetWritableFieldKeys(
            schema, workflow, candidate, principal, ignoreStateGate: true);

        var handOffFields = workflow.TransitionsLeadingOn(workflow.InitialState)
            .Select(transition => MoveHandOff.NomineeFieldFor(workflow, schema, transition))
            .OfType<string>()
            .ToHashSet(StringComparer.Ordinal);

        var narrowedEpaFields = CreditRuleFields.ResolveNarrowedEpaFieldKeys(
            activityType.CreditRulesJson, schema.EvidenceEpaField, activityType.WbaToolKey);

        JsonObject sourceData;
        try
        {
            sourceData = JsonNode.Parse(source.DataJson) as JsonObject ?? [];
        }
        catch (JsonException)
        {
            sourceData = [];
        }

        var copy = new JsonObject();
        var keys = new List<string>();
        var epaDropped = false;
        foreach (var field in schema.Sections.SelectMany(section => section.Fields))
        {
            if (!writable.Contains(field.Key) ||
                handOffFields.Contains(field.Key) ||
                !sourceData.TryGetPropertyValue(field.Key, out var value) ||
                value is null)
            {
                continue;
            }

            if (field.Type == FieldType.Epa &&
                !await IsOfferedAsync(field, value, narrowedEpaFields, activityType.WbaToolKey, principal, callerId, cancellationToken))
            {
                epaDropped = true;
                continue;
            }

            copy[field.Key] = value.DeepClone();
            keys.Add(field.Key);
        }

        return (copy.ToJsonString(), keys, epaDropped);
    }

    /// <summary>Whether Log an activity's EPA picker offers this value for this field, asked as the page asks it.</summary>
    private async Task<bool> IsOfferedAsync(
        FormField field,
        JsonNode value,
        IReadOnlySet<string> narrowedEpaFields,
        string? wbaToolKey,
        ClaimsPrincipal principal,
        string callerId,
        CancellationToken cancellationToken)
    {
        // Stored as a number by the form, or as its text by a hand-made payload; the picker's values are the id's text.
        string? stored = null;
        if (value is JsonValue scalar)
        {
            if (scalar.TryGetValue<int>(out var number))
            {
                stored = number.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (scalar.TryGetValue<string>(out var text))
            {
                stored = text.Trim();
            }
        }

        if (string.IsNullOrEmpty(stored))
        {
            return false;
        }

        // CurrentValue null: the picker appends a stored value, and this value is not stored on anything the page shows.
        var options = await _referenceData.GetEpaOptionsAsync(
            principal,
            new EpaOptionScope(callerId, narrowedEpaFields.Contains(field.Key), CurrentValue: null, WbaToolKey: wbaToolKey),
            cancellationToken);

        return options.Any(option => string.Equals(option.Value, stored, StringComparison.Ordinal));
    }
}
