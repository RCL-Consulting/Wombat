using System.Security.Claims;
using MediatR;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Dashboards.Oversight;
using Wombat.Application.Features.Programme;
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
    private readonly TimeProvider _clock;

    public GetCommitteeMemberDashboardSummaryQueryHandler(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        TimeProvider clock)
    {
        _dbContext = dbContext;
        _users = users;
        _clock = clock;
    }

    public async Task<CommitteeMemberDashboardSummaryDto> Handle(
        GetCommitteeMemberDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        // T358 (flow 06, Q1, E4): the Home reads as Committee member, in that role's scope, the institution's current
        // registrars, by Programme trainees' own reader, so the card and the page its foot opens list the same people in
        // the same order. The rules the card kept before are the scope's: every current registrar at the member's own
        // institution whatever the member's sub-speciality claims (T113, T290: the external member, who holds none, saw
        // nobody, Step 2.37), never another institution's (T130), current only (T238, T268), and nobody's for a registrar
        // on the committee as the trainees' representative (T185), who reads no peer. Only the five first are named here.
        var scope = await ProgrammeScope.ResolveAsync(_dbContext, request.Principal, WombatRoles.CommitteeMember, cancellationToken);
        var (registrars, coverage) = await OversightHomeReads.RosterAsync(
            _dbContext, _users, scope, request.AsOf ?? QuotaCalendar.Today(_clock), cancellationToken);

        return new CommitteeMemberDashboardSummaryDto(registrars, coverage);
    }
}
