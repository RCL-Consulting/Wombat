using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Domain.Activities;

namespace Wombat.Application.Features.Dashboards.Assessor;

public sealed record GetAssessorDashboardSummaryQuery(ClaimsPrincipal Principal) : IRequest<AssessorDashboardSummaryDto>;

public sealed class GetAssessorDashboardSummaryQueryHandler
    : IRequestHandler<GetAssessorDashboardSummaryQuery, AssessorDashboardSummaryDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly DashboardThresholds _thresholds;

    public GetAssessorDashboardSummaryQueryHandler(
        IApplicationDbContext dbContext,
        IOptions<DashboardThresholds> thresholds)
    {
        _dbContext = dbContext;
        _thresholds = thresholds.Value;
    }

    public async Task<AssessorDashboardSummaryDto> Handle(
        GetAssessorDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var userId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var dueCutoff = DateTime.UtcNow.AddDays(-_thresholds.AssessorDueDays);

        var pendingCount = await _dbContext.Set<Activity>()
            .AsNoTracking()
            .Where(a => a.CreatedByUserId != a.SubjectUserId &&
                        a.CurrentState == "requested")
            .Join(
                _dbContext.Set<ActivityTransition>().AsNoTracking(),
                a => a.Id,
                t => t.ActivityId,
                (a, t) => new { Activity = a, Transition = t })
            .Where(x => x.Transition.ActorUserId == userId ||
                        x.Activity.Transitions.Any(t => t.ActorUserId == userId))
            .Select(x => x.Activity.Id)
            .Distinct()
            .CountAsync(cancellationToken);

        // Simpler approach: get activities where this assessor has participated
        var assessorActivities = await _dbContext.Set<Activity>()
            .AsNoTracking()
            .Include(a => a.ActivityType)
            .Include(a => a.Transitions)
            .Where(a => a.Transitions.Any(t => t.ActorUserId == userId) ||
                        a.CreatedByUserId == userId)
            .OrderByDescending(a => a.UpdatedOn)
            .Take(50)
            .ToListAsync(cancellationToken);

        var pendingRequests = assessorActivities
            .Where(a => a.CurrentState == "requested")
            .ToList();

        var accepted = assessorActivities
            .Where(a => a.CurrentState == "accepted")
            .Select(a => new AcceptedActivityItem(
                a.Id,
                a.ActivityType.Name,
                a.SubjectUserId,
                a.UpdatedOn,
                a.UpdatedOn < dueCutoff))
            .ToList();

        var recentDecisions = assessorActivities
            .Where(a => a.CurrentState is "completed" or "declined" or "cancelled")
            .Take(10)
            .Select(a => new RecentDecisionItem(
                a.Id,
                a.ActivityType.Name,
                a.SubjectUserId,
                a.CurrentState,
                a.UpdatedOn))
            .ToList();

        return new AssessorDashboardSummaryDto(
            pendingRequests.Count,
            accepted,
            recentDecisions);
    }
}
