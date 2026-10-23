using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Dashboards.InstitutionalAdmin;

public sealed record GetInstitutionalAdminDashboardSummaryQuery(ClaimsPrincipal Principal)
    : IRequest<InstitutionalAdminDashboardSummaryDto>;

/// <summary>
/// Her Home's read (T376, flow 12, Q7, Q8, K24; T291 items 4 and 8): everything for her institution, nothing national.
/// </summary>
/// <remarks>
/// <para>
/// The people are one directory read of her institution's accounts with their roles (<see
/// cref="IUserAdministrationService.ListUsersPageAsync" />, one page holding them all): the distinct account count, those
/// with no role included (K24), every role held and its count, and the waiting count, Pending trainee's. Until T376 Home
/// asked the directory for every holder of each of the ten roles in the country and counted hers, and had no account count.
/// </para>
/// <para>
/// Curriculum counts what her institution has adopted, through its active adoptions (T291 item 4): until T376 it showed
/// the College's whole catalogue, every speciality and sub-speciality in Wombat, as if hers.
/// </para>
/// </remarks>
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
        if (request.Principal.GetInstitutionId() is not int institutionId)
        {
            return new InstitutionalAdminDashboardSummaryDto([], 0, false, 0, 0, 0, [], null);
        }

        var people = await _userService.ListUsersPageAsync(
            new UserDirectoryFilter(institutionId, [], Role: null, NoRole: false, Status: null, Page: 1, PageSize: int.MaxValue),
            cancellationToken);

        var roleCounts = WombatRoles.All
            .Select(role => new RoleCountItem(role, people.Rows.Count(user => user.Roles.Contains(role, StringComparer.Ordinal))))
            .Where(item => item.Count > 0)
            .ToArray();
        var waiting = roleCounts.FirstOrDefault(item => item.Role == WombatRoles.PendingTrainee)?.Count ?? 0;
        var callerId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var onlyYours = people.TotalCount == 1 &&
                        people.Rows.Count == 1 &&
                        string.Equals(people.Rows[0].UserId, callerId, StringComparison.Ordinal);

        var adoptions = await (
                from adoption in _dbContext.Set<InstitutionCurriculumAdoption>().AsNoTracking()
                where adoption.IsActive && adoption.InstitutionId == institutionId
                join curriculum in _dbContext.Set<Curriculum>() on adoption.CurriculumId equals curriculum.Id
                join subSpeciality in _dbContext.Set<SubSpeciality>() on adoption.SubSpecialityId equals subSpeciality.Id
                join speciality in _dbContext.Set<Speciality>() on subSpeciality.SpecialityId equals speciality.Id
                join college in _dbContext.Set<College>() on speciality.CollegeId equals college.Id
                select new
                {
                    SpecialityId = speciality.Id,
                    SubSpecialityId = subSpeciality.Id,
                    Item = new AdoptedCurriculumItem(
                        speciality.Name, subSpeciality.Name, college.ShortCode, curriculum.Name, curriculum.Version)
                })
            .ToListAsync(cancellationToken);

        var institution = await _dbContext.Set<Institution>()
            .AsNoTracking()
            .Where(entity => entity.Id == institutionId)
            .Select(entity => new InstitutionCardItem(entity.Id, entity.Name, entity.ShortCode, entity.ContactEmail, entity.IsActive))
            .SingleOrDefaultAsync(cancellationToken);

        return new InstitutionalAdminDashboardSummaryDto(
            roleCounts,
            people.TotalCount,
            onlyYours,
            waiting,
            adoptions.Select(row => row.SpecialityId).Distinct().Count(),
            adoptions.Select(row => row.SubSpecialityId).Distinct().Count(),
            adoptions
                .Select(row => row.Item)
                .OrderBy(item => item.SpecialityName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.SubSpecialityName, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            institution);
    }
}
