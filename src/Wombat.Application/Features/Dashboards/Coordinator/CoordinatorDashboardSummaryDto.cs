namespace Wombat.Application.Features.Dashboards.Coordinator;

public sealed record CoordinatorDashboardSummaryDto(
    IReadOnlyList<StalledRequestItem> StalledRequests,
    IReadOnlyList<ExpiringInvitationItem> ExpiringInvitations);

public sealed record StalledRequestItem(
    int ActivityId,
    string ActivityTypeName,
    string SubjectName,
    DateTime SubmittedOn);

public sealed record ExpiringInvitationItem(
    int InvitationId,
    string Email,
    string TargetRole,
    DateOnly ExpiresOn);
