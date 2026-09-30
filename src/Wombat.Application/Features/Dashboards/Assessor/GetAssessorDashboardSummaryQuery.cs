using System.Security.Claims;
using MediatR;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Activities.Services;

namespace Wombat.Application.Features.Dashboards.Assessor;

public sealed record GetAssessorDashboardSummaryQuery(ClaimsPrincipal Principal) : IRequest<AssessorDashboardSummaryDto>;

/// <summary>
/// The Assessor's Home: what waits on the caller, and what the caller recently decided, each read from the activity's
/// PINNED workflow (T297; "finished" since T203), never from a state's key.
/// </summary>
/// <remarks>
/// Since T350 (note 5, note 6; E5) both are the Activity inbox's own reads, made here with the same code: the waiting rows
/// are <see cref="WaitingForYou" />'s, all of them, and the decisions <see cref="DecidedByYou" />'s first page. Until T350
/// this handler read both itself: the inbox listed the same waiting rows newest first and with the caller's own subject
/// rows, which only this card left out, and the decisions were read fifty at a time until ten were found.
/// </remarks>
public sealed class GetAssessorDashboardSummaryQueryHandler
    : IRequestHandler<GetAssessorDashboardSummaryQuery, AssessorDashboardSummaryDto>
{
    /// <summary>How many recent decisions Home lists: the decided read's first page, at this size (T350, R1).</summary>
    public const int DecisionsListed = 5;

    private readonly IApplicationDbContext _dbContext;
    private readonly IWorkflowEvaluator _workflowEvaluator;
    private readonly IUserAdministrationService _users;
    private readonly DashboardThresholds _thresholds;
    private readonly TimeProvider _clock;

    public GetAssessorDashboardSummaryQueryHandler(
        IApplicationDbContext dbContext,
        IWorkflowEvaluator workflowEvaluator,
        IUserAdministrationService users,
        IOptions<DashboardThresholds> thresholds,
        TimeProvider clock)
    {
        _dbContext = dbContext;
        _workflowEvaluator = workflowEvaluator;
        _users = users;
        _thresholds = thresholds.Value;
        _clock = clock;
    }

    public async Task<AssessorDashboardSummaryDto> Handle(
        GetAssessorDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var waiting = await WaitingForYou.ReadAsync(
            _dbContext, _workflowEvaluator, _users, _clock, _thresholds, request.Principal, cancellationToken);
        var decisions = await DecidedByYou.ReadAsync(
            _dbContext, _users, request.Principal, page: 1, DecisionsListed, cancellationToken);

        return new AssessorDashboardSummaryDto(waiting, decisions);
    }
}
