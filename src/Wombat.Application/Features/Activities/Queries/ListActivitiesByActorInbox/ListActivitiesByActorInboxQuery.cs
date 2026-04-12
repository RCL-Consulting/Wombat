using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Application.Features.Activities.Queries.ListActivitiesByActorInbox;

public sealed record ListActivitiesByActorInboxQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<ActivitySummaryDto>>;

public sealed class ListActivitiesByActorInboxQueryHandler : IRequestHandler<ListActivitiesByActorInboxQuery, IReadOnlyList<ActivitySummaryDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IWorkflowEvaluator _workflowEvaluator;

    public ListActivitiesByActorInboxQueryHandler(IApplicationDbContext dbContext, IWorkflowEvaluator workflowEvaluator)
    {
        _dbContext = dbContext;
        _workflowEvaluator = workflowEvaluator;
    }

    public async Task<IReadOnlyList<ActivitySummaryDto>> Handle(ListActivitiesByActorInboxQuery request, CancellationToken cancellationToken)
    {
        var activities = await _dbContext.Set<Activity>()
            .AsNoTracking()
            .Include(activity => activity.ActivityType)
                .ThenInclude(activityType => activityType.Versions)
            .Include(activity => activity.Transitions)
            .OrderByDescending(activity => activity.UpdatedOn)
            .ToListAsync(cancellationToken);

        return activities
            .Where(activity =>
            {
                var pinnedVersion = activity.ActivityType.Versions.SingleOrDefault(version => version.Version == activity.SchemaVersion);
                if (pinnedVersion is null)
                {
                    return false;
                }

                var workflow = WorkflowParser.Parse(pinnedVersion.WorkflowJson);
                return workflow.Transitions.Any(transition =>
                    transition.From.Contains(activity.CurrentState, StringComparer.Ordinal) &&
                    _workflowEvaluator.Evaluate(workflow, activity, transition.Key, request.Principal).Allowed);
            })
            .Select(activity => new ActivitySummaryDto(
                activity.Id,
                activity.ActivityTypeId,
                activity.ActivityType.Key,
                activity.ActivityType.Name,
                activity.SubjectUserId,
                activity.CurrentState,
                activity.CreatedOn,
                activity.UpdatedOn))
            .ToList();
    }
}
