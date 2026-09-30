using System.Security.Claims;
using MediatR;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;

namespace Wombat.Application.Features.Activities.Queries.ListWaitingForYou;

/// <summary>
/// What waits on the caller as someone else's assessor, reviewer or admin, oldest first (T350, note 5): the Activity
/// inbox's "Waiting for you", the activity page's way on after a move, and the other-role line on Home. The read is
/// <see cref="WaitingForYou" />, which the Assessor's Home makes too, so none of them can disagree (T297).
/// </summary>
/// <remarks>
/// It replaces <c>ListActivitiesByActorInboxQuery</c> (T342), which listed the same rows newest first and kept the caller's
/// own subject rows that the dashboard then left out: two reads of one list (T350, E5).
/// </remarks>
public sealed record ListWaitingForYouQuery(ClaimsPrincipal Principal) : IRequest<WaitingForYouDto>;

public sealed class ListWaitingForYouQueryHandler : IRequestHandler<ListWaitingForYouQuery, WaitingForYouDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IWorkflowEvaluator _workflowEvaluator;
    private readonly IUserAdministrationService _users;
    private readonly DashboardThresholds _thresholds;
    private readonly TimeProvider _clock;

    public ListWaitingForYouQueryHandler(
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

    public Task<WaitingForYouDto> Handle(ListWaitingForYouQuery request, CancellationToken cancellationToken)
        => WaitingForYou.ReadAsync(
            _dbContext, _workflowEvaluator, _users, _clock, _thresholds, request.Principal, cancellationToken);
}
