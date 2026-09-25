using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Activities.Services;
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

        // Simpler approach: get activities where this assessor has participated.
        // Never the caller's own portfolio: a user who is also a Trainee created, and made the "create" move on, every
        // activity of their own, and a nominee is never the subject (NomineeGate), so none of those is assessor work.
        // Without this a logged procedure, which is born in its terminal state, would read as the caller's decision (T203).
        var assessorActivities = await _dbContext.Set<Activity>()
            .AsNoTracking()
            .Include(a => a.ActivityType)
            .Include(a => a.Transitions)
            .Where(a => a.SubjectUserId != userId &&
                        (a.Transitions.Any(t => t.ActorUserId == userId) ||
                         a.CreatedByUserId == userId))
            .OrderByDescending(a => a.UpdatedOn)
            .Take(50)
            .ToListAsync(cancellationToken);

        // T203: "done" is a terminal state of the activity's PINNED workflow (D44, ActivityCompletion), not the literal
        // "completed". A discussed reflective exercise, a recorded MSF row and a logged procedure are finished, so they
        // are decisions; and a teaching session finishes in "accepted", so a finished one is not work needing action.
        // The same pinned workflow names the state each decision left the activity in (T220).
        var workflows = await PinnedWorkflows.LoadAsync(
            _dbContext,
            assessorActivities.Select(a => (a.ActivityTypeId, a.SchemaVersion)),
            cancellationToken);
        var finishedStates = workflows.ToDictionary(pair => pair.Key, pair => ActivityCompletion.FinishedStates(pair.Value));
        bool IsFinished(Activity activity)
            => finishedStates[(activity.ActivityTypeId, activity.SchemaVersion)].Contains(activity.CurrentState);

        var pendingRequests = assessorActivities
            .Where(a => a.CurrentState == "requested" && !IsFinished(a))
            .ToList();

        var accepted = assessorActivities
            .Where(a => a.CurrentState == "accepted" && !IsFinished(a))
            .Select(a => new AcceptedActivityItem(
                a.Id,
                a.ActivityType.Name,
                a.SubjectUserId,
                PinnedWorkflows.StateLabel(workflows[(a.ActivityTypeId, a.SchemaVersion)], a.CurrentState),
                a.UpdatedOn,
                a.UpdatedOn < dueCutoff))
            .ToList();

        var recentDecisions = assessorActivities
            .Where(a => IsFinished(a) || a.CurrentState is "declined" or "cancelled")
            .Take(10)
            .Select(a => new RecentDecisionItem(
                a.Id,
                a.ActivityType.Name,
                a.SubjectUserId,
                a.CurrentState,
                PinnedWorkflows.StateLabel(workflows[(a.ActivityTypeId, a.SchemaVersion)], a.CurrentState),
                a.UpdatedOn))
            .ToList();

        return new AssessorDashboardSummaryDto(
            pendingRequests.Count,
            accepted,
            recentDecisions);
    }
}
