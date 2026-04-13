namespace Wombat.Application.Features.Dashboards.InstitutionalAdmin;

public sealed record InstitutionalAdminDashboardSummaryDto(
    IReadOnlyList<RoleCountItem> UsersByRole,
    int SpecialityCount,
    int SubSpecialityCount);

public sealed record RoleCountItem(string Role, int Count);
