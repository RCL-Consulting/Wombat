using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Users;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
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
        // The activities the caller can move now, each with its pinned workflow, which decided that and names the state the
        // row shows (T220). The one reading the Assessor's and the Trainee's cards share, so neither can disagree with this
        // page (T297); narrowed in SQL to the states that have a move out and the rows the caller could reach, where every
        // activity used to be loaded. With the transitions, for the credit column below.
        var actionable = await ActivityWaiting.LoadActionableAsync(
            _dbContext.Set<Activity>(), _dbContext, _workflowEvaluator, request.Principal, withTransitions: true, cancellationToken);

        // T137. The EPA each row is about, from the stamped column, in one read for the rows that survived the act
        // gate. An assessor with three requests from one trainee used to see three rows that differed only by id.
        // T231. With whether it is in force now, by the rule the activity's own picker labels by (EpaOptionLabel).
        var epaIds = actionable
            .Where(row => row.Activity.EpaId is not null)
            .Select(row => row.Activity.EpaId!.Value)
            .Distinct()
            .ToList();

        var epas = epaIds.Count == 0
            ? new Dictionary<int, (string Code, string Title, bool InForce)>()
            : (await _dbContext.Set<Epa>()
                    .AsNoTracking()
                    .Where(epa => epaIds.Contains(epa.Id))
                    .Select(epa => new { epa.Id, epa.Code, epa.Title, epa.IsActive })
                    .ToListAsync(cancellationToken))
                .ToDictionary(epa => epa.Id, epa => (epa.Code, epa.Title, InForce: epa.IsActive));

        // T142. Whose activity each row is, by name, in one lookup for the rows that survived the act gate. The column
        // used to print the subject's user id.
        var names = await UserDisplayNames.ResolveAsync(
            _users, actionable.Select(row => row.Activity.SubjectUserId), cancellationToken);

        return actionable
            .Select(row =>
            {
                var activity = row.Activity;
                (string Code, string Title, bool InForce)? epa =
                    activity.EpaId is int epaId && epas.TryGetValue(epaId, out var found) ? found : null;

                return new ActivitySummaryDto(
                    activity.Id,
                    activity.ActivityTypeId,
                    activity.ActivityType.Key,
                    activity.ActivityType.Name,
                    activity.SubjectUserId,
                    activity.CurrentState,
                    // The state in the words of the pinned workflow, which the act gate above just read (T220).
                    PinnedWorkflows.StateLabel(row.Workflow, activity.CurrentState),
                    activity.CreatedOn,
                    activity.UpdatedOn,
                    activity.EpaId,
                    epa?.Code,
                    epa?.Title,
                    epa?.InForce,
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
