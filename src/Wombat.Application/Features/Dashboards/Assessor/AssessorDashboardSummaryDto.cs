namespace Wombat.Application.Features.Dashboards.Assessor;

public sealed record AssessorDashboardSummaryDto(
    int PendingRequestCount,
    IReadOnlyList<AcceptedActivityItem> AcceptedActivities,
    IReadOnlyList<RecentDecisionItem> RecentDecisions);

/// <param name="SubjectName">
/// Whose activity it is, by name (<see cref="Wombat.Application.Common.Users.UserDisplayNames.NameOf" />), the id only
/// when that user has no name on record (T250).
/// </param>
/// <param name="CurrentStateLabel">
/// The <c>accepted</c> state as the activity's pinned workflow labels it, the key only when that workflow does not declare
/// it: the badge's text when the activity is not overdue (T220).
/// </param>
public sealed record AcceptedActivityItem(
    int ActivityId,
    string ActivityTypeName,
    string SubjectName,
    string CurrentStateLabel,
    DateTime AcceptedOn,
    bool IsOverdue);

/// <param name="SubjectName">
/// Whose activity it is, by name (<see cref="Wombat.Application.Common.Users.UserDisplayNames.NameOf" />), the id only
/// when that user has no name on record (T250).
/// </param>
/// <param name="FinalState">The stored state key: the badge's colour class.</param>
/// <param name="FinalStateLabel">
/// The state as the activity's pinned workflow labels it, the key only when that workflow does not declare it: the
/// badge's text (T220).
/// </param>
public sealed record RecentDecisionItem(
    int ActivityId,
    string ActivityTypeName,
    string SubjectName,
    string FinalState,
    string FinalStateLabel,
    DateTime DecidedOn);
