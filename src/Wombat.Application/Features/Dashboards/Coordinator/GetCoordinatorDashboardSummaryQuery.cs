using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Dashboards.Oversight;
using Wombat.Application.Features.Programme;
using Wombat.Domain.Identity;
using Wombat.Domain.Invitations;

namespace Wombat.Application.Features.Dashboards.Coordinator;

/// <param name="AsOf">The day to read for. Defaults to today in South Africa; tests pin it.</param>
public sealed record GetCoordinatorDashboardSummaryQuery(ClaimsPrincipal Principal, DateOnly? AsOf = null)
    : IRequest<CoordinatorDashboardSummaryDto>;

public sealed class GetCoordinatorDashboardSummaryQueryHandler
    : IRequestHandler<GetCoordinatorDashboardSummaryQuery, CoordinatorDashboardSummaryDto>
{
    /// <summary>How many South African days ahead an invitation is "nearing expiry": its rule line's "the next 3 days".</summary>
    public const int ExpiryDays = 3;

    /// <summary>How many expiring invitations the card lists, soonest first.</summary>
    public const int InvitationsListed = 10;

    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;
    private readonly IReminderRecipients _recipients;
    private readonly DashboardThresholds _thresholds;
    private readonly TimeProvider _clock;

    public GetCoordinatorDashboardSummaryQueryHandler(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        IReminderRecipients recipients,
        IOptions<DashboardThresholds> thresholds,
        TimeProvider clock)
    {
        _dbContext = dbContext;
        _users = users;
        _recipients = recipients;
        _thresholds = thresholds.Value;
        _clock = clock;
    }

    public async Task<CoordinatorDashboardSummaryDto> Handle(
        GetCoordinatorDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        // "Today" is the South African day (T325): until T358 the card read DateTime.UtcNow, so between midnight and 02:00
        // an invitation expiring "today" had already expired, and dates were "d MMM".
        var today = request.AsOf ?? QuotaCalendar.Today(_clock);

        // T358 (flow 06, E3, E4, E5): the Home reads as Coordinator, in the institution's scope. Waiting for assessors is
        // the page's own read, its first five (the Stalled requests it replaces listed any activity awaiting a reviewer,
        // a role-held review included, past the old stall days); Nothing filed is Programme trainees' filter, its first
        // five, by the rule the weekly digest reads (E5).
        var scope = await ProgrammeScope.ResolveAsync(_dbContext, request.Principal, WombatRoles.Coordinator, cancellationToken);
        var waiting = await OversightHomeReads.WaitingAsync(
            _dbContext, _users, _recipients, _clock, _thresholds, request.Principal, scope, cancellationToken);
        var nothingFiled = await OversightHomeReads.NothingFiledAsync(_dbContext, _users, scope, today, cancellationToken);

        // The invitations keep their scope as before: the Coordinator's institution.
        var institutionId = request.Principal.GetInstitutionId();
        var expiryCutoff = today.AddDays(ExpiryDays);
        var expiringQuery = _dbContext.Set<Invitation>()
            .AsNoTracking()
            .Where(i => i.UsedOn == null && i.RevokedOn == null &&
                        i.ExpiresOn >= today && i.ExpiresOn <= expiryCutoff);

        if (institutionId.HasValue)
        {
            expiringQuery = expiringQuery.Where(i => i.InstitutionId == institutionId.Value);
        }

        var expiring = (await expiringQuery
                .OrderBy(i => i.ExpiresOn)
                .ThenBy(i => i.Id)
                .Take(InvitationsListed)
                .Select(i => new { i.Id, i.Email, i.TargetRole, i.ExpiresOn })
                .ToListAsync(cancellationToken))
            .Select(i => new ExpiringInvitationItem(i.Id, i.Email, i.TargetRole, i.ExpiresOn)
            {
                TargetRoleLabel = WombatRoleLabels.For(i.TargetRole)
            })
            .ToList();

        return new CoordinatorDashboardSummaryDto(waiting, nothingFiled, expiring);
    }
}
