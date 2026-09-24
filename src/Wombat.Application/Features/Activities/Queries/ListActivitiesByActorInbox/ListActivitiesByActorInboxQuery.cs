using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Users;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Activities.Queries.ListActivitiesByActorInbox;

public sealed record ListActivitiesByActorInboxQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<ActivitySummaryDto>>;

public sealed class ListActivitiesByActorInboxQueryHandler : IRequestHandler<ListActivitiesByActorInboxQuery, IReadOnlyList<ActivitySummaryDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IWorkflowEvaluator _workflowEvaluator;
    private readonly IUserAdministrationService _users;

    public ListActivitiesByActorInboxQueryHandler(
        IApplicationDbContext dbContext,
        IWorkflowEvaluator workflowEvaluator,
        IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _workflowEvaluator = workflowEvaluator;
        _users = users;
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

        var actionable = activities
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
            .ToList();

        // T137. The EPA each row is about, from the stamped column, in one read for the rows that survived the act
        // gate. An assessor with three requests from one trainee used to see three rows that differed only by id.
        var epaIds = actionable
            .Where(activity => activity.EpaId is not null)
            .Select(activity => activity.EpaId!.Value)
            .Distinct()
            .ToList();

        var epas = epaIds.Count == 0
            ? new Dictionary<int, (string Code, string Title)>()
            : (await _dbContext.Set<Epa>()
                    .AsNoTracking()
                    .Where(epa => epaIds.Contains(epa.Id))
                    .Select(epa => new { epa.Id, epa.Code, epa.Title })
                    .ToListAsync(cancellationToken))
                .ToDictionary(epa => epa.Id, epa => (epa.Code, epa.Title));

        // T142. Whose activity each row is, by name, in one lookup for the rows that survived the act gate. The column
        // used to print the subject's user id.
        var names = await UserDisplayNames.ResolveAsync(
            _users, actionable.Select(activity => activity.SubjectUserId), cancellationToken);

        return actionable
            .Select(activity =>
            {
                (string Code, string Title)? epa =
                    activity.EpaId is int epaId && epas.TryGetValue(epaId, out var found) ? found : null;

                return new ActivitySummaryDto(
                    activity.Id,
                    activity.ActivityTypeId,
                    activity.ActivityType.Key,
                    activity.ActivityType.Name,
                    activity.SubjectUserId,
                    activity.CurrentState,
                    activity.CreatedOn,
                    activity.UpdatedOn,
                    activity.EpaId,
                    epa?.Code,
                    epa?.Title,
                    activity.ObservedOn,
                    activity.ObservedOnSource == ObservationDateSource.Declared,
                    // The same rule as the trainee's own list: the latest transition that evaluated credit (T108).
                    activity.Transitions
                        .Where(transition => transition.CreditedItemCount is not null)
                        .OrderByDescending(transition => transition.OccurredOn)
                        .ThenByDescending(transition => transition.Id)
                        .Select(transition => transition.CreditedItemCount)
                        .FirstOrDefault())
                {
                    SubjectName = names.NameOf(activity.SubjectUserId)
                };
            })
            .ToList();
    }
}
