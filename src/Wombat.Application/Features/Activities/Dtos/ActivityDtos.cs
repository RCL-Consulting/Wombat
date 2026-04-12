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

public sealed record ActivityTransitionDto(
    int Id,
    string FromState,
    string ToState,
    string TransitionKey,
    string ActorUserId,
    DateTime OccurredOn,
    string? Note,
    string SnapshotJson);

public sealed record ActivityDto(
    int Id,
    int ActivityTypeId,
    string ActivityTypeKey,
    string ActivityTypeName,
    int SchemaVersion,
    string SchemaJson,
    string WorkflowJson,
    string DisplayFieldsJson,
    string SubjectUserId,
    string CreatedByUserId,
    string CurrentState,
    string DataJson,
    int? EpaId,
    int? CurriculumItemId,
    DateTime CreatedOn,
    DateTime UpdatedOn,
    IReadOnlyList<ActivityTransitionDto> Transitions);

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
