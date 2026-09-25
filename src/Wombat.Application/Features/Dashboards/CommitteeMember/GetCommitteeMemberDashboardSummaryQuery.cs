using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Common.Users;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Dashboards.CommitteeMember;

/// <param name="AsOf">The day to read targets for. Defaults to today in South Africa; tests pin it.</param>
public sealed record GetCommitteeMemberDashboardSummaryQuery(ClaimsPrincipal Principal, DateOnly? AsOf = null)
    : IRequest<CommitteeMemberDashboardSummaryDto>;

public sealed class GetCommitteeMemberDashboardSummaryQueryHandler
    : IRequestHandler<GetCommitteeMemberDashboardSummaryQuery, CommitteeMemberDashboardSummaryDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public GetCommitteeMemberDashboardSummaryQueryHandler(
        IApplicationDbContext dbContext,
        IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<CommitteeMemberDashboardSummaryDto> Handle(
        GetCommitteeMemberDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var subSpecialityIds = request.Principal.GetSubSpecialityIds();

        // A sub-speciality id is national (College-owned, T091), so on its own it matches every adopting
        // institution's trainees. A committee member oversees their own institution: the rule every other
        // committee surface already applies (ActivityReadScope, TraineeScopeResolver.IsOverseenBy). Before T130
        // this card leaked only a few user ids above an 80% threshold; once it listed every trainee by name, the
        // missing institution filter became a disclosure. A global Administrator sees every institution.
        //
        // The trainee rung first (TraineeScopeResolver.ActsAsTrainee, T185): a registrar who sits on the committee as
        // the trainees' representative is a trainee in the programme, and this card names each of their peers beside
        // the targets they have met, which is their progress. They see it empty, as a member with no institution does.
        var institutionId = request.Principal.GetInstitutionId();
        var isAdministrator = request.Principal.IsAdministrator();
        if (TraineeScopeResolver.ActsAsTrainee(request.Principal) || (!isAdministrator && institutionId is null))
        {
            var empty = await CurriculumCoverageReader.ReadAsync(_dbContext, [], request.AsOf ?? QuotaCalendar.Today(), cancellationToken);
            return new CommitteeMemberDashboardSummaryDto(empty.CurrentSemesterName, empty.CurrentSemesterMonths, [], [], 0);
        }

        var activeProfiles = await _dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .Where(p => p.IsActive && subSpecialityIds.Contains(p.Curriculum.SubSpecialityId))
            .Where(p => isAdministrator || p.InstitutionId == institutionId)
            .ToListAsync(cancellationToken);

        // Current trainees only (T238): an active profile is not enough. An erased trainee's profile stayed active under a
        // pseudonym no account holds until T258, a profile can outlive its user's Trainee role, and an administrator can
        // lock the account (T268); none is a trainee for the committee to weigh, and each would be named here, the first
        // by its bare pseudonym. The speciality and sub-speciality dashboards keep the same trainees, by the same call,
        // since the three draw one card.
        var traineeProfiles = await TraineeScopeResolver.KeepCurrentAsync(_dbContext, _users, activeProfiles, cancellationToken);

        var coverage = await CurriculumCoverageReader.ReadAsync(
            _dbContext, traineeProfiles, request.AsOf ?? QuotaCalendar.Today(), cancellationToken);

        // Names, not user ids (the old card printed the id in the name column), and only for the trainees listed.
        var names = await UserDisplayNames.ResolveAsync(
            _users, coverage.Trainees.Select(trainee => trainee.TraineeUserId), cancellationToken);

        var trainees = coverage.Trainees
            .Select(trainee => new TraineeTargetsItem(
                trainee.TraineeUserId,
                names.NameOf(trainee.TraineeUserId),
                trainee.SemesterTargetsMet,
                trainee.SemesterTargetsApplying,
                trainee.YearTargetsMet,
                trainee.YearTargetsApplying))
            .ToList();

        return new CommitteeMemberDashboardSummaryDto(
            coverage.CurrentSemesterName,
            coverage.CurrentSemesterMonths,
            trainees,
            coverage.Epas,
            coverage.ExemptTraineeCount);
    }
}
