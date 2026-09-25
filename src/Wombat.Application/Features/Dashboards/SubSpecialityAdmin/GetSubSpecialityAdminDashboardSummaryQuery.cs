using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Dashboards.SubSpecialityAdmin;

/// <param name="AsOf">The day to read targets for. Defaults to today in South Africa; tests pin it.</param>
public sealed record GetSubSpecialityAdminDashboardSummaryQuery(ClaimsPrincipal Principal, DateOnly? AsOf = null)
    : IRequest<SubSpecialityAdminDashboardSummaryDto>;

public sealed class GetSubSpecialityAdminDashboardSummaryQueryHandler
    : IRequestHandler<GetSubSpecialityAdminDashboardSummaryQuery, SubSpecialityAdminDashboardSummaryDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public GetSubSpecialityAdminDashboardSummaryQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<SubSpecialityAdminDashboardSummaryDto> Handle(
        GetSubSpecialityAdminDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var subSpecialityIds = request.Principal.GetSubSpecialityIds();

        // Speciality and sub-speciality ids are national (College-owned, T091), so on their own they match every
        // adopting institution's trainees. This admin is scoped to their own institution, as ExportPortfolio and
        // ListTraineesForSpeciality already require (T130: the coverage card counts trainees, and counting other
        // institutions' trainees made "4 of 40 met" of a programme that has 9). A global Administrator sees all.
        var institutionId = request.Principal.GetInstitutionId();
        var isAdministrator = request.Principal.IsAdministrator();

        // Counted across the whole database until T101: a sub-speciality admin's "pending review" tile reported every
        // institution's backlog. T101 counted by the activity's SubSpecialityId stamp, which is national too, so the
        // tile still counted every adopting institution's backlog in this sub-speciality until T185 conjoined the
        // institution stamp, as the trainee counts below do. A null stamp belongs to nobody's programme and is counted
        // by nobody.
        var pendingReviewCount = !isAdministrator && institutionId is null
            ? 0
            : await _dbContext.Set<Activity>()
                .AsNoTracking()
                .Where(a => a.CurrentState == "submitted" || a.CurrentState == "in_review")
                .Where(a => a.SubSpecialityId != null && subSpecialityIds.Contains(a.SubSpecialityId.Value))
                .Where(a => isAdministrator || a.InstitutionId == institutionId)
                .CountAsync(cancellationToken);

        var traineeProfiles = !isAdministrator && institutionId is null
            ? []
            : await _dbContext.Set<TraineeProfile>()
                .AsNoTracking()
                .Where(p => subSpecialityIds.Contains(p.Curriculum.SubSpecialityId))
                .Where(p => isAdministrator || p.InstitutionId == institutionId)
                .ToListAsync(cancellationToken);

        // The current trainees (T238), as the committee dashboard reads them: an active profile is not enough. An erased
        // trainee's profile stays active under a pseudonym no account holds, and a profile can outlive its user's Trainee
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

        return new SubSpecialityAdminDashboardSummaryDto(
            pendingReviewCount, activeCount, inactiveCount, coverage);
    }
}
