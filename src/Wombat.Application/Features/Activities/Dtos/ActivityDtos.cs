using Wombat.Domain.Activities;

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
/// curriculum items credited.
/// </summary>
public sealed record ActivityTransitionDto(
    int Id,
    string FromState,
    string ToState,
    string TransitionKey,
    string ActorUserId,
    DateTime OccurredOn,
    string? Note,
    string SnapshotJson,
    int? CreditedItemCount,
    // No default: this is the only signal that a completion counted for volume but was refused its
    // supervision level, and a defaulted argument is one a future call site can silently drop (T109).
    int? CreditScaleMismatchCount);

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
    string DataJson,
    int? EpaId,
    int? CurriculumItemId,
    DateTime CreatedOn,
    DateTime UpdatedOn,
    IReadOnlyList<ActivityTransitionDto> Transitions);

/// <summary>
/// A workflow transition the current actor is allowed to perform from the activity's current state,
/// evaluated server-side against the real pinned <c>ActivityType</c> (T070). The display label is the
/// caller's business — <c>ActivityWorkflowActions</c> title-cases the key.
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

public sealed record ActivitySummaryDto(
    int Id,
    int ActivityTypeId,
    string ActivityTypeKey,
    string ActivityTypeName,
    string SubjectUserId,
    string CurrentState,
    DateTime CreatedOn,
    DateTime UpdatedOn);

public sealed record ActivityValidationErrorDto(
    string? FieldKey,
    string Message,
    string Rule);
