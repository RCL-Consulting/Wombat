using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Dashboards.InstitutionalAdmin;

public sealed record GetInstitutionalAdminDashboardSummaryQuery(ClaimsPrincipal Principal)
    : IRequest<InstitutionalAdminDashboardSummaryDto>;

public sealed class GetInstitutionalAdminDashboardSummaryQueryHandler
    : IRequestHandler<GetInstitutionalAdminDashboardSummaryQuery, InstitutionalAdminDashboardSummaryDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userService;

    public GetInstitutionalAdminDashboardSummaryQueryHandler(
        IApplicationDbContext dbContext,
        IUserAdministrationService userService)
    {
        _dbContext = dbContext;
        _userService = userService;
    }

    public async Task<InstitutionalAdminDashboardSummaryDto> Handle(
        GetInstitutionalAdminDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var institutionId = request.Principal.GetInstitutionId();

        var roleCounts = new List<RoleCountItem>();
        foreach (var role in WombatRoles.All)
        {
            var users = await _userService.ListUsersInRoleAsync(role, cancellationToken);
            var count = institutionId.HasValue
                ? users.Count(u => u.InstitutionId == institutionId.Value)
                : users.Count;
            if (count > 0)
            {
                roleCounts.Add(new RoleCountItem(role, count));
            }
        }

        // Specialities/sub-specialities are now national (College-owned) catalogue, not institution-scoped
        // (T091); report the national catalogue totals. A per-institution "adopted disciplines" count
        // arrives with adoption in phase 4.
        var specialityCount = await _dbContext.Set<Speciality>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var subSpecialityCount = await _dbContext.Set<SubSpeciality>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        return new InstitutionalAdminDashboardSummaryDto(roleCounts, specialityCount, subSpecialityCount);
    }
}
