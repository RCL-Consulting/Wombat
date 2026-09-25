using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Application.Features.Activities.Dtos;

public sealed record ActivityTypeListItemDto(
    int Id,
    string Key,
    string Name,
    ActivityScope Scope,
    int? ScopeId,
    int Version,
    bool IsActive);

public sealed record ActivityTypeAdminListItemDto(
    int Id,
    string Key,
    string Name,
    string? Description,
    ActivityScope Scope,
    int? ScopeId,
    int PublishedVersion,
    bool IsActive,
    bool HasDraft,
    DateTime? DraftUpdatedOn);

public sealed record ActivityTypeVersionDto(
    int Version,
    DateTime PublishedOn,
    string PublishedByUserId);

public sealed record ActivityTypeEditorDto(
    int Id,
    string Key,
    string Name,
    string? Description,
    ActivityScope Scope,
    int? ScopeId,
    bool IsActive,
    // Which College-named instrument this type is, or null (T122). Live metadata like Name, not a staged
    // payload. /activities/new reads it from here to narrow the EPA picker the way the write path will check.
    string? WbaToolKey,
    int PublishedVersion,
    bool HasDraft,
    string DraftSchemaJson,
    string DraftWorkflowJson,
    string DraftCreditRulesJson,
    string DraftDisplayFieldsJson,
    string? PublishedSchemaJson,
    string? PublishedWorkflowJson,
    string? PublishedCreditRulesJson,
    string PublishedDisplayFieldsJson,
    string OwnerUserId,
    string? StagingUpdatedByUserId,
    DateTime? StagingUpdatedOn,
    IReadOnlyList<ActivityTypeVersionDto> Versions);

/// <summary>
/// One recorded workflow move. <paramref name="CreditedItemCount" /> is the T108 signal: <c>null</c>
/// when credit was never evaluated for this move (non-terminal, or a version that credits nothing by
/// design), <c>0</c> when it was evaluated and matched no curriculum item, otherwise the number of
/// curriculum items credited. <paramref name="DaysAfterEncounter" /> is set only on the move that filed the
/// activity, when its encounter date was stated and its pinned version can credit (T160, D15).
/// </summary>
/// <param name="FromStateLabel">
/// <paramref name="FromState" /> as the activity's PINNED workflow labels it (<c>PinnedWorkflows.StateLabel</c>), the key
/// only when that workflow does not declare it (T220). What the history prints; the key stays for logic.
/// </param>
/// <param name="ToStateLabel">The same, for <paramref name="ToState" />.</param>
/// <param name="TransitionLabel">
/// <paramref name="TransitionKey" /> as its button named the move (<c>PinnedWorkflows.TransitionLabel</c>): "Sign Off",
/// "Create" for the create row, the key only for a move the pinned workflow does not declare (T220).
/// </param>
public sealed record ActivityTransitionDto(
    int Id,
    string FromState,
    string ToState,
    string TransitionKey,
    // No defaults: a producer that forgets a label is a page that prints a key again (T220).
    string FromStateLabel,
    string ToStateLabel,
    string TransitionLabel,
    string ActorUserId,
    DateTime OccurredOn,
    string? Note,
    string SnapshotJson,
    int? CreditedItemCount,
    // No default: this is the only signal that a completion counted for volume but was refused its
    // supervision level, and a defaulted argument is one a future call site can silently drop (T109).
    int? CreditScaleMismatchCount,
    // No default either, for the same reason: it is the only record that a filing was late (T160).
    int? DaysAfterEncounter)
{
    /// <summary>
    /// Who made the move, by name (<see cref="Wombat.Application.Common.Users.UserDisplayNames.NameOf" />): the
    /// history table's Actor column (T142). Filled by <c>GetActivityByIdQuery</c>, the query that shows the history, in
    /// one lookup for every actor. Null from every other producer: <c>ActivityService.Map</c> is shared by create,
    /// transition and detail, and does no lookup.
    /// </summary>
    public string? ActorName { get; init; }
}

public sealed record ActivityDto(
    int Id,
    int ActivityTypeId,
    string ActivityTypeKey,
    string ActivityTypeName,
    // The type row's CURRENT instrument key (T122), not a pinned one: it is unversioned, and it is the key the
    // write path checks at submit, so the EPA picker narrows by the same value.
    string? WbaToolKey,
    int SchemaVersion,
    string SchemaJson,
    string WorkflowJson,
    string DisplayFieldsJson,
    // The pinned version's credit rules, carried for the same reason SchemaJson and WorkflowJson are:
    // the renderer has to know which EPA field the credit engine will actually read before it can
    // narrow that field's options to what would be credited (T108).
    string CreditRulesJson,
    string SubjectUserId,
    // The subject's institution as stamped at creation (T101): the institution a nominee field may name someone from
    // (T102). The picker reads it from here, off an activity the page has already been authorised to read.
    int? InstitutionId,
    string CreatedByUserId,
    string CurrentState,
    // CurrentState as the pinned workflow labels it, the key only when that workflow does not declare it (T220): what
    // the page's header and summary print, so they name the state as a refusal and a notice on the page do (T189).
    string CurrentStateLabel,
    string DataJson,
    int? EpaId,
    int? CurriculumItemId,
    // When the encounter happened (Activity.ObservedOn, T119), and whether anyone stated it. False means it is only
    // the day the activity was created, and the page must say so rather than show it as a clinical date (T161, D28,
    // T197; EncounterDate).
    DateOnly ObservedOn,
    bool ObservedOnDeclared,
    DateTime CreatedOn,
    DateTime UpdatedOn,
    IReadOnlyList<ActivityTransitionDto> Transitions);

/// <summary>
/// A workflow transition the current actor is allowed to perform from the activity's current state,
/// evaluated server-side against the real pinned <c>ActivityType</c> (T070).
/// </summary>
/// <param name="UnavailableReason">
/// Null when the actor can take the action from this page. Otherwise the action is shown disabled with this text
/// (T107, D33): the transition's validation would flag, against the stored data, a field this actor cannot write in
/// the current state, so pressing it could only fail. The server still refuses such a move on its own.
/// </param>
public sealed record ActivityActionDto(
    string TransitionKey,
    bool RequiresNote,
    string? UnavailableReason = null)
{
    public bool IsAvailable => UnavailableReason is null;

    /// <summary>
    /// The action's name on its button, from <see cref="WorkflowTransition.LabelFor" />: the name a refusal of the same
    /// move uses (T189), and the name the workflow history gives the move once it is made
    /// (<see cref="ActivityTransitionDto.TransitionLabel" />, T220), so all three call it the same thing.
    /// </summary>
    public string Label => WorkflowTransition.LabelFor(TransitionKey);
}

/// <summary>
/// An activity plus the two things that can only be worked out with a principal in hand:
/// which fields this actor may write right now, and which transitions they may perform.
/// </summary>
/// <remarks>
/// T070. <see cref="EditableFieldKeys"/> is in schema order. It is empty for a terminal activity,
/// for an actor the state's <c>editable_by</c> rule excludes, and for an unrelated user — which is
/// what keeps non-bound users read-only.
/// </remarks>
public sealed record ActivityDetailDto(
    ActivityDto Activity,
    IReadOnlyList<string> EditableFieldKeys,
    IReadOnlyList<ActivityActionDto> AvailableActions);

/// <summary>
/// One row of an activity list: enough to tell an activity from its siblings without opening it (T137).
/// </summary>
/// <param name="EpaId">The stamped <c>Activity.EpaId</c>: the EPA this activity is evidence for, or null.</param>
/// <param name="EpaCode">That EPA's code, or null when the activity is about no EPA.</param>
/// <param name="EpaTitle">That EPA's title, or null when the activity is about no EPA.</param>
/// <param name="ObservedOn">
/// When the encounter happened (<c>Activity.ObservedOn</c>, T119), not when the paperwork was filed.
/// </param>
/// <param name="ObservedOnDeclared">
/// False when nobody stated an encounter date and <paramref name="ObservedOn" /> is only the day the activity was
/// created. A list must not present that as a clinical fact.
/// </param>
/// <param name="CreditedItemCount">
/// The T108 outcome of the LATEST transition on which credit was evaluated: null when credit was never evaluated (the
/// activity is not complete, or its type credits nothing by design), 0 when it was evaluated and matched no curriculum
/// item, otherwise the number of items credited. The same three-valued contract as
/// <see cref="ActivityTransitionDto.CreditedItemCount" />, read from the same column. (T106 item 14)
/// </param>
public sealed record ActivitySummaryDto(
    int Id,
    int ActivityTypeId,
    string ActivityTypeKey,
    string ActivityTypeName,
    string SubjectUserId,
    string CurrentState,
    // CurrentState as the activity's pinned workflow labels it, the key only when that workflow does not declare it
    // (T220). What the lists print.
    string CurrentStateLabel,
    DateTime CreatedOn,
    DateTime UpdatedOn,
    int? EpaId,
    string? EpaCode,
    string? EpaTitle,
    DateOnly ObservedOn,
    bool ObservedOnDeclared,
    int? CreditedItemCount)
{
    /// <summary>
    /// Whose activity it is, by name (<see cref="Wombat.Application.Common.Users.UserDisplayNames.NameOf" />): the
    /// inbox's Subject column (T142). Filled by <c>ListActivitiesByActorInboxQuery</c> in one lookup for the page. Null
    /// on the subject's own list, which is all one person and does not show them.
    /// </summary>
    public string? SubjectName { get; init; }
}

public sealed record ActivityValidationErrorDto(
    string? FieldKey,
    string Message,
    string Rule);
