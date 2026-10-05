using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
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
        // A committee member oversees every current trainee at their own institution: the rule every other committee
        // surface applies (ActivityReadScope, TraineeScopeResolver.IsOverseenBy, T113). Until T290 this card read the
        // member's sub-speciality claims instead, so the external member, who holds none, saw nobody, and the others saw
        // only the sub-speciality their assessor profile happened to write onto them (Step 2.37). The institution
        // filter stays: before T130 its absence named other institutions' trainees. A global Administrator sees every
        // institution.
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
            .Where(p => p.IsActive)
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

        // Names, not user ids (the old card printed the id in the name column), and only for the trainees listed. The
        // contact, not the display name, because ties are broken by surname, and "Pieter du Plessis" cannot be split.
        var contacts = coverage.Trainees.Count == 0
            ? new Dictionary<string, UserContact>(StringComparer.Ordinal)
            : await _users.GetContactsAsync(
                coverage.Trainees.Select(trainee => trainee.TraineeUserId).ToList(), cancellationToken);

        // The reader's order is fewest targets met first, as a share of those that apply: the trainees a committee needs
        // to see. Equal shares are then read by surname and first name (T298), never by user id, a GUID no reader can
        // follow; the id only separates two people of one name. A trainee with no name on record reads as the id, as
        // UserDisplayNames does.
        var trainees = coverage.Trainees
            .Select(trainee => (
                Coverage: trainee,
                Contact: contacts.TryGetValue(trainee.TraineeUserId, out var contact) ? contact : null))
            .OrderBy(row => (double)row.Coverage.TargetsMet / row.Coverage.TargetsApplying)
            .ThenBy(row => row.Contact?.LastName ?? row.Coverage.TraineeUserId, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(row => row.Contact?.FirstName ?? string.Empty, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(row => row.Coverage.TraineeUserId, StringComparer.Ordinal)
            .Select(row => new TraineeTargetsItem(
                row.Coverage.TraineeUserId,
                NameOf(row.Contact, row.Coverage.TraineeUserId),
                row.Coverage.SemesterTargetsMet,
                row.Coverage.SemesterTargetsApplying,
                row.Coverage.YearTargetsMet,
                row.Coverage.YearTargetsApplying))
            .ToList();

        return new CommitteeMemberDashboardSummaryDto(
            coverage.CurrentSemesterName,
            coverage.CurrentSemesterMonths,
            trainees,
            coverage.Epas,
            coverage.ExemptTraineeCount);
    }

    private static string NameOf(UserContact? contact, string userId)
    {
        var name = contact is null ? string.Empty : $"{contact.FirstName} {contact.LastName}".Trim();
        return name.Length == 0 ? userId : name;
    }
}
