using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Services;
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
    private readonly IUserAdministrationService _users;

    public GetSpecialityAdminDashboardSummaryQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
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

        // Speciality and sub-speciality ids are national (College-owned, T091), so on their own they match every
        // adopting institution's trainees. This admin is scoped to their own institution, as ExportPortfolio and
        // ListTraineesForSpeciality already require (T130: the coverage card counts trainees, and counting other
        // institutions' trainees made "4 of 40 met" of a programme that has 9). A global Administrator sees all.
        var institutionId = request.Principal.GetInstitutionId();
        var isAdministrator = request.Principal.IsAdministrator();

        // Counted across the whole database until T101: a speciality admin's "pending review" tile reported every
        // institution's backlog. T101 counted by the activity's SpecialityId stamp, which is national too, so the tile
        // still counted every adopting institution's backlog in this speciality until T185 conjoined the institution
        // stamp, as the trainee counts below do. A null stamp belongs to nobody's programme and is counted by nobody.
        // T297: pending is awaiting a reviewer, read from each activity's pinned workflow (ActivityWaiting). Until T297 it
        // was the states keyed "submitted" or "in_review": no seed has "in_review", and every rated CPSA instrument waits
        // for its assessor in "requested", so a requested CBD was never pending (Step 3.53).
        var pendingReviewCount = 0;
        if (isAdministrator || institutionId is not null)
        {
            var awaitingReviewer = await ActivityWaiting.LoadAwaitingReviewerAsync(_dbContext, cancellationToken);
            var candidates = await awaitingReviewer.Narrow(_dbContext.Set<Activity>()
                    .AsNoTracking()
                    .Where(a => a.SpecialityId != null && specialityIds.Contains(a.SpecialityId.Value))
                    .Where(a => isAdministrator || a.InstitutionId == institutionId))
                .Select(a => new { a.ActivityTypeId, a.SchemaVersion, a.CurrentState })
                .ToListAsync(cancellationToken);
            pendingReviewCount = candidates.Count(a => awaitingReviewer.Waits(a.ActivityTypeId, a.SchemaVersion, a.CurrentState));
        }

        var traineeProfiles = !isAdministrator && institutionId is null
            ? []
            : await _dbContext.Set<TraineeProfile>()
                .AsNoTracking()
                .Where(p => subSpecialityIds.Contains(p.Curriculum.SubSpecialityId))
                .Where(p => isAdministrator || p.InstitutionId == institutionId)
                .ToListAsync(cancellationToken);

        // The current trainees (T238), as the committee dashboard reads them: an active profile is not enough. An erased
        // trainee's profile stayed active under a pseudonym until T258, and a profile can outlive its user's Trainee
        // role. Counted as active, either made the tile and every "n of m" below disagree with the committee's card, which
        // shares the list that draws them (EpaTargetCoverageList). Neither is counted as inactive either: the admin
        // trainees list is where a profile that outlived its trainee is seen, and ended.
        var current = await TraineeScopeResolver.KeepCurrentAsync(_dbContext, _users, traineeProfiles, cancellationToken);
        var activeCount = current.Count;
        var inactiveCount = traineeProfiles.Count(p => !p.IsActive);

        // Each EPA's target for the current period, through the same calculator as the trainee's own page (T130).
        var coverage = await CurriculumCoverageReader.ReadAsync(
            _dbContext,
            current,
            request.AsOf ?? QuotaCalendar.Today(),
            cancellationToken);

        return new SpecialityAdminDashboardSummaryDto(
            pendingReviewCount, activeCount, inactiveCount, coverage);
    }
}
