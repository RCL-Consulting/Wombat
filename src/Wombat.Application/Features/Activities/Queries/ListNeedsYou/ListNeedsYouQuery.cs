using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Activities.Queries.ListNeedsYou;

/// <summary>
/// Needs you (T342, B6): the caller's own work that waits on them, their drafts and the work returned to them, most
/// recently updated first. Home's Needs you card and My activities' Needs you section both send it, so they cannot
/// disagree (T297's rule, carried over from the Activity inbox card).
/// </summary>
/// <remarks>
/// <para>
/// A row is one the caller is the subject or the creator of, on which they may make a move that leads on
/// (<c>Workflow.TransitionsLeadingOn</c>) by the author's arms of its rule (<see cref="ActorArms.Author" />):
/// submit a draft, file a returned reflection again, log a teaching session. A request they may only cancel is with its
/// assessor, so it is not here (Step 3.12), and a declined one has no move left. The work that waits on them for
/// someone else is the Activity inbox's (<c>ListActivitiesByActorInboxQuery</c>, <see cref="ActorArms.NotAuthor" />).
/// </para>
/// <para>
/// Each row carries what My activities' rows carry: who has it (the caller), the nominee, the return and the name, read
/// by the same code (<see cref="ActivityRowDetails" />).
/// </para>
/// </remarks>
public sealed record ListNeedsYouQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<ActivitySummaryDto>>;

public sealed class ListNeedsYouQueryHandler : IRequestHandler<ListNeedsYouQuery, IReadOnlyList<ActivitySummaryDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IWorkflowEvaluator _workflowEvaluator;
    private readonly IUserAdministrationService _users;

    public ListNeedsYouQueryHandler(
        IApplicationDbContext dbContext,
        IWorkflowEvaluator workflowEvaluator,
        IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _workflowEvaluator = workflowEvaluator;
        _users = users;
    }

    public Task<IReadOnlyList<ActivitySummaryDto>> Handle(ListNeedsYouQuery request, CancellationToken cancellationToken)
        => NeedsYou.ReadAsync(_dbContext, _workflowEvaluator, _users, request.Principal, cancellationToken);
}

/// <summary>
/// The read behind <see cref="ListNeedsYouQuery" />, which Home's trainee summary makes too
/// (<c>GetTraineeDashboardSummaryQuery</c>): one read, so Home's Needs you card and My activities' Needs you section list
/// the same rows in the same words (T297's rule as flow 03 restates it; T342, lane D).
/// </summary>
public static class NeedsYou
{
    public static async Task<IReadOnlyList<ActivitySummaryDto>> ReadAsync(
        IApplicationDbContext dbContext,
        IWorkflowEvaluator workflowEvaluator,
        IUserAdministrationService users,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        var callerUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(callerUserId))
        {
            return [];
        }

        // The caller's own rows, and only those they may read: a row they created for someone they no longer oversee is
        // not theirs to open, so it is not theirs to finish (T101's read rule, conjoined).
        var readable = dbContext.Set<Activity>().WhereReadableBy(principal);
        var own = readable.Where(activity => activity.SubjectUserId == callerUserId || activity.CreatedByUserId == callerUserId);

        var actionable = await ActivityWaiting.LoadActionableAsync(
            own, dbContext, workflowEvaluator, principal, withTransitions: true, cancellationToken, ActorArms.Author);
        if (actionable.Count == 0)
        {
            return [];
        }

        var epaIds = actionable
            .Where(row => row.Activity.EpaId is not null)
            .Select(row => row.Activity.EpaId!.Value)
            .Distinct()
            .ToList();
        var epas = epaIds.Count == 0
            ? new Dictionary<int, (string Code, string Title, bool InForce)>()
            : (await dbContext.Set<Epa>()
                    .AsNoTracking()
                    .Where(epa => epaIds.Contains(epa.Id))
                    .Select(epa => new { epa.Id, epa.Code, epa.Title, epa.IsActive })
                    .ToListAsync(cancellationToken))
                .ToDictionary(epa => epa.Id, epa => (epa.Code, epa.Title, InForce: epa.IsActive));
        (string Code, string Title, bool InForce)? EpaOf(Activity activity)
            => activity.EpaId is int epaId && epas.TryGetValue(epaId, out var found) ? found : null;

        var facts = actionable.Select(row => ActivityRowFacts.From(row.Activity, EpaOf(row.Activity)?.Code)).ToList();

        // The pinned forms name each row's segments (E9); the workflow already decided the row, and names its state.
        var forms = await PinnedForms.LoadAsync(
            dbContext, facts.Select(fact => (fact.ActivityTypeId, fact.SchemaVersion)), cancellationToken);
        var shared = await ActivityRowDetails.SharedKeysAsync(readable, facts, cancellationToken);
        var (details, _) = await ActivityRowDetails.ResolveAsync(
            facts,
            fact => forms[(fact.ActivityTypeId, fact.SchemaVersion)],
            fact => shared.Contains(fact.CollisionKey),
            principal,
            users,
            cancellationToken);

        return actionable
            .Select(row =>
            {
                var activity = row.Activity;
                var epa = EpaOf(activity);
                var detail = details[activity.Id];
                return new ActivitySummaryDto(
                    activity.Id,
                    activity.ActivityTypeId,
                    activity.ActivityType.Key,
                    activity.ActivityType.Name,
                    activity.SubjectUserId,
                    activity.CurrentState,
                    PinnedWorkflows.StateLabel(row.Workflow, activity.CurrentState),
                    activity.CreatedOn,
                    activity.UpdatedOn,
                    activity.EpaId,
                    epa?.Code,
                    epa?.Title,
                    epa?.InForce,
                    activity.ObservedOn,
                    activity.ObservedOnSource == ObservationDateSource.Declared,
                    // Credit is evaluated only on the way into a terminal state, and nothing here is finished.
                    activity.Transitions
                        .Where(transition => transition.CreditedItemCount is not null)
                        .OrderByDescending(transition => transition.OccurredOn)
                        .ThenByDescending(transition => transition.Id)
                        .Select(transition => transition.CreditedItemCount)
                        .FirstOrDefault())
                {
                    Holder = detail.Holder,
                    NomineeName = detail.NomineeName,
                    Returned = detail.Returned,
                    DisplayName = detail.DisplayName,
                    DisplayNameHasNominee = detail.DisplayNameHasNominee,
                    Shape = detail.Shape
                };
            })
            .ToList();
    }
}
