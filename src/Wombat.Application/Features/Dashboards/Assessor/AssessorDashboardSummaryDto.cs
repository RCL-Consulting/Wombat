namespace Wombat.Application.Features.Dashboards.Assessor;

public sealed record AssessorDashboardSummaryDto(
    int PendingRequestCount,
    IReadOnlyList<AcceptedActivityItem> AcceptedActivities,
    IReadOnlyList<RecentDecisionItem> RecentDecisions);

public sealed record AcceptedActivityItem(
    int ActivityId,
    string ActivityTypeName,
    string SubjectName,
    DateTime AcceptedOn,
    bool IsOverdue);

public sealed record RecentDecisionItem(
    int ActivityId,
    string ActivityTypeName,
    string SubjectName,
    string FinalState,
    DateTime DecidedOn);
