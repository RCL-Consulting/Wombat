namespace Wombat.Application.Features.Dashboards.Coordinator;

public sealed record CoordinatorDashboardSummaryDto(
    IReadOnlyList<StalledRequestItem> StalledRequests,
    IReadOnlyList<ExpiringInvitationItem> ExpiringInvitations);

/// <summary>
/// An activity awaiting a reviewer that nobody has moved for <c>DashboardThresholds.CoordinatorStallDays</c> (T297):
/// its row links to the activity's page.
/// </summary>
/// <param name="SubjectName">Whose activity it is, by name, the email or the id only when no name is on record (T250).</param>
/// <param name="LastMovedOn">When the activity last moved: how long it has waited.</param>
public sealed record StalledRequestItem(
    int ActivityId,
    string ActivityTypeName,
    string SubjectName,
    DateTime LastMovedOn);

public sealed record ExpiringInvitationItem(
    int InvitationId,
    string Email,
    string TargetRole,
    DateOnly ExpiresOn);
