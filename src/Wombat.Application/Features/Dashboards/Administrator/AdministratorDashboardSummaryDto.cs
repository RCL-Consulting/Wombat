namespace Wombat.Application.Features.Dashboards.Administrator;

public sealed record AdministratorDashboardSummaryDto(
    bool DatabaseHealthy,
    int TotalUserCount);
