using System.Security.Claims;
using MediatR;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Dashboards.Oversight;
using Wombat.Application.Features.Programme;
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
    private readonly IReminderRecipients _recipients;
    private readonly DashboardThresholds _thresholds;
    private readonly TimeProvider _clock;

    public GetSubSpecialityAdminDashboardSummaryQueryHandler(
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

    public async Task<SubSpecialityAdminDashboardSummaryDto> Handle(
        GetSubSpecialityAdminDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        // T358 (flow 06, Q1, Q3, E4): the Home reads as Sub-speciality admin, in that role's one scope, the institution
        // and the admin's sub-specialities (ProgrammeScope), by the readers of the two lists its cards preview. Until T358
        // the handler wrote the scope out twice itself, the sub-speciality stamp for the backlog and the curriculum's
        // sub-speciality for the trainees, which disagreed about a transferred trainee (the T101 review). The rules it kept
        // are the scope's: the institution conjoined to the national sub-speciality ids (T130, T185), a null stamp
        // nobody's, current registrars only (T238, T268).
        var scope = await ProgrammeScope.ResolveAsync(_dbContext, request.Principal, WombatRoles.SubSpecialityAdmin, cancellationToken);

        var waiting = await OversightHomeReads.WaitingAsync(
            _dbContext, _users, _recipients, _clock, _thresholds, request.Principal, scope, cancellationToken);
        var (registrars, coverage) = await OversightHomeReads.RosterAsync(
            _dbContext, _users, scope, request.AsOf ?? QuotaCalendar.Today(_clock), cancellationToken);

        return new SubSpecialityAdminDashboardSummaryDto(waiting, registrars, coverage);
    }
}
