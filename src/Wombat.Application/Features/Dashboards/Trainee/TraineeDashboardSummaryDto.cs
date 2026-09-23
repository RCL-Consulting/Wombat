using Wombat.Application.Features.Curricula.Quota;

namespace Wombat.Application.Features.Dashboards.Trainee;

/// <param name="CurriculumTargets">
/// The trainee's curriculum progress for the current period: the same read model the progress page uses (T130).
/// Null for a pending trainee or one with no active profile.
/// </param>
public sealed record TraineeDashboardSummaryDto(
    TraineeCurriculumProgressSummaryDto? CurriculumTargets,
    IReadOnlyList<ActivityInboxItem> Inbox,
    IReadOnlyList<RecentActivityItem> RecentActivities,
    IReadOnlyList<UpcomingDeadlineItem> UpcomingDeadlines,
    bool IsPendingTrainee);

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
