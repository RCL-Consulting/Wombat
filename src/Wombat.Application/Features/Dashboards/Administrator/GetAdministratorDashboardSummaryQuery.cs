using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Dashboards.Administrator;

public sealed record GetAdministratorDashboardSummaryQuery(ClaimsPrincipal Principal)
    : IRequest<AdministratorDashboardSummaryDto>;

public sealed class GetAdministratorDashboardSummaryQueryHandler
    : IRequestHandler<GetAdministratorDashboardSummaryQuery, AdministratorDashboardSummaryDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userService;

    public GetAdministratorDashboardSummaryQueryHandler(
        IApplicationDbContext dbContext,
        IUserAdministrationService userService)
    {
        _dbContext = dbContext;
        _userService = userService;
    }

    public async Task<AdministratorDashboardSummaryDto> Handle(
        GetAdministratorDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        // DB health check via a lightweight count query
        var dbHealthy = true;
        try
        {
            await _dbContext.Set<Institution>()
                .AsNoTracking()
                .CountAsync(cancellationToken);
        }
        catch
        {
            dbHealthy = false;
        }

        // Total distinct user count across all roles
        var allUserIds = new HashSet<string>();
        foreach (var role in WombatRoles.All)
        {
            var users = await _userService.ListUsersInRoleAsync(role, cancellationToken);
            foreach (var user in users)
            {
                allUserIds.Add(user.UserId);
            }
        }

        return new AdministratorDashboardSummaryDto(dbHealthy, allUserIds.Count);
    }
}
