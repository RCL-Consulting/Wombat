using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Dashboards.SpecialityAdmin;

/// <param name="AsOf">The day to read targets for. Defaults to today in South Africa; tests pin it.</param>
public sealed record GetSpecialityAdminDashboardSummaryQuery(ClaimsPrincipal Principal, DateOnly? AsOf = null)
    : IRequest<SpecialityAdminDashboardSummaryDto>;

public sealed class GetSpecialityAdminDashboardSummaryQueryHandler
    : IRequestHandler<GetSpecialityAdminDashboardSummaryQuery, SpecialityAdminDashboardSummaryDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSpecialityAdminDashboardSummaryQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SpecialityAdminDashboardSummaryDto> Handle(
        GetSpecialityAdminDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var specialityIds = request.Principal.GetSpecialityIds();

        // Get sub-speciality IDs that belong to the admin's specialities
        var subSpecialityIds = await _dbContext.Set<SubSpeciality>()
            .AsNoTracking()
            .Where(ss => specialityIds.Contains(ss.SpecialityId))
            .Select(ss => ss.Id)
            .ToListAsync(cancellationToken);

        var pendingReviewCount = await _dbContext.Set<Activity>()
            .AsNoTracking()
            .Where(a => a.CurrentState == "submitted" || a.CurrentState == "in_review")
            // Counted across the whole database until T101: a speciality admin's "pending review"
            // tile reported every institution's backlog. Activities carry their own SpecialityId
            // stamp now, so the count answers for the specialities this admin is scoped to. A null
            // stamp belongs to nobody's programme and is counted by nobody.
            .Where(a => a.SpecialityId != null && specialityIds.Contains(a.SpecialityId.Value))
            .CountAsync(cancellationToken);

        // Speciality and sub-speciality ids are national (College-owned, T091), so on their own they match every
        // adopting institution's trainees. This admin is scoped to their own institution, as ExportPortfolio and
        // ListTraineesForSpeciality already require (T130: the coverage card counts trainees, and counting other
        // institutions' trainees made "4 of 40 met" of a programme that has 9). A global Administrator sees all.
        var institutionId = request.Principal.GetInstitutionId();
        var isAdministrator = request.Principal.IsAdministrator();

        var traineeProfiles = !isAdministrator && institutionId is null
            ? []
            : await _dbContext.Set<TraineeProfile>()
                .AsNoTracking()
                .Where(p => subSpecialityIds.Contains(p.Curriculum.SubSpecialityId))
                .Where(p => isAdministrator || p.InstitutionId == institutionId)
                .ToListAsync(cancellationToken);

        var activeCount = traineeProfiles.Count(p => p.IsActive);
        var inactiveCount = traineeProfiles.Count(p => !p.IsActive);

        // Each EPA's target for the current period, through the same calculator as the trainee's own page (T130).
        var coverage = await CurriculumCoverageReader.ReadAsync(
            _dbContext,
            traineeProfiles.Where(p => p.IsActive).ToList(),
            request.AsOf ?? QuotaCalendar.Today(),
            cancellationToken);

        return new SpecialityAdminDashboardSummaryDto(
            pendingReviewCount, activeCount, inactiveCount, coverage);
    }
}
