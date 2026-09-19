namespace Wombat.Application.Features.Dashboards.Trainee;

public sealed record TraineeDashboardSummaryDto(
    IReadOnlyList<CurriculumProgressItem> CurriculumProgress,
    IReadOnlyList<ActivityInboxItem> Inbox,
    IReadOnlyList<RecentActivityItem> RecentActivities,
    IReadOnlyList<UpcomingDeadlineItem> UpcomingDeadlines,
    bool IsPendingTrainee);

public sealed record CurriculumProgressItem(
    string EpaTitle,
    int CompletedCount,
    int RequiredCount,
    bool IsComplete,
    int EffectiveMinimumLevelOrder,
    /// <summary>
    /// <see cref="EffectiveMinimumLevelOrder" /> rendered as the rung a clinician reads — "3a", not "3"
    /// (T100). Falls back to the ordinal as text when the curriculum item is unpinned.
    /// </summary>
    string EffectiveMinimumLevelLabel,
    int MinimumLevelReachedCount,
    int? TraineeStage);

public sealed record ActivityInboxItem(
    int ActivityId,
    string ActivityTypeName,
    string CurrentState,
    DateTime UpdatedOn);

public sealed record RecentActivityItem(
    int ActivityId,
    string ActivityTypeName,
    string CurrentState,
    DateTime CreatedOn);

public sealed record UpcomingDeadlineItem(
    int ActivityId,
    string ActivityTypeName,
    string FieldLabel,
    DateOnly DueDate);
