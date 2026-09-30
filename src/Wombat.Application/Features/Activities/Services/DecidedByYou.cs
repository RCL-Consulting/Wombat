using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Activities.Services;

/// <summary>
/// What the caller decided (T350, round 1, Q1; note 6): the read behind <c>ListDecidedByYouQuery</c>, the Activity
/// inbox's "Decided by you", whose first page of five is the Assessor's Home "Recent decisions"
/// (<c>GetAssessorDashboardSummaryQuery</c>). One read, so the card and the section it links to cannot disagree (T297).
/// </summary>
/// <remarks>
/// <para>
/// A decision is an activity the caller moved last (the latest transition by time, ties to the later id, as the credit
/// column reads the latest, T108) that is finished or has no move left, each judged by its PINNED workflow (T297; D44,
/// <see cref="ActivityCompletion" />): not the literal "declined" and "cancelled", which counted a request the trainee
/// withdrew after the caller had moved it and left out a dead end of any other name. A review the caller returned to
/// draft has a move left, so it is back with its author and is not a decision. With no workflow, nothing can move it, so
/// it is where the caller left it.
/// </para>
/// <para>
/// The caller's own subject rows are left out: a user who is also a trainee made the create move on every activity of
/// their own, and a logged procedure is born terminal, so it would read as their decision (T203). For the same reason
/// an activity whose last move is its create is left out whoever created it: a supervisor or coordinator who logs a
/// procedure on a registrar's behalf recorded it, and decided nothing (T350 build review, R1).
/// </para>
/// <para>
/// "Finished or no move left" is a question for each pin's workflow in C#, not SQL, so every activity the caller moved
/// last is read (a few columns each) and judged before the page is cut and the total counted (note 6): an assessor's
/// scale, not a table's. Only the page's rows are then loaded whole and named.
/// </para>
/// </remarks>
public static class DecidedByYou
{
    /// <summary>The page size the inbox's "Decided by you" opens with.</summary>
    public const int DefaultPageSize = 20;

    /// <summary>The largest page served, as My activities serves (<c>ListActivitiesBySubjectQuery.MaxPageSize</c>).</summary>
    public const int MaxPageSize = 100;

    /// <param name="page">The page to serve, from 1; brought within the list's pages, so a page past the end is the last.</param>
    /// <param name="pageSize">Rows per page, brought within 1 and <see cref="MaxPageSize" />.</param>
    public static async Task<ActivityListPageDto> ReadAsync(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        ClaimsPrincipal principal,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(principal);

        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        var callerUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(callerUserId))
        {
            return new ActivityListPageDto([], 1, pageSize, 0);
        }

        var lastMoved = await dbContext.Set<Activity>()
            .AsNoTracking()
            .Where(activity => activity.SubjectUserId != callerUserId)
            // Implied by the next test, and cheaper: only the activities the caller ever moved are asked who moved last.
            .Where(activity => activity.Transitions.Any(transition => transition.ActorUserId == callerUserId))
            .Where(activity => activity.Transitions
                .OrderByDescending(transition => transition.OccurredOn)
                .ThenByDescending(transition => transition.Id)
                .Select(transition => transition.ActorUserId)
                .FirstOrDefault() == callerUserId)
            // A create is not a decision, whoever makes it (T350 build review, R1): see the remarks.
            .Where(activity => activity.Transitions
                .OrderByDescending(transition => transition.OccurredOn)
                .ThenByDescending(transition => transition.Id)
                .Select(transition => transition.TransitionKey)
                .FirstOrDefault() != Workflow.CreateTransitionKey)
            .Select(activity => new
            {
                activity.Id,
                activity.ActivityTypeId,
                activity.SchemaVersion,
                activity.CurrentState,
                // The caller's move is the latest, so the latest time is when they made it.
                DecidedOn = activity.Transitions.Max(transition => transition.OccurredOn)
            })
            .ToListAsync(cancellationToken);

        var workflows = await PinnedWorkflows.LoadAsync(
            dbContext, lastMoved.Select(row => (row.ActivityTypeId, row.SchemaVersion)), cancellationToken);

        var decided = lastMoved
            .Select(row =>
            {
                var workflow = workflows[(row.ActivityTypeId, row.SchemaVersion)];
                var isFinished = ActivityCompletion.FinishedStates(workflow).Contains(row.CurrentState);
                var hasMoveLeft = workflow?.HasOutgoingTransition(row.CurrentState) ?? false;
                return (row.Id, row.DecidedOn, Workflow: workflow, IsFinished: isFinished, IsDecision: isFinished || !hasMoveLeft);
            })
            .Where(row => row.IsDecision)
            .OrderByDescending(row => row.DecidedOn)
            .ThenByDescending(row => row.Id)
            .ToList();

        var pageCount = Math.Max(1, (decided.Count + pageSize - 1) / pageSize);
        page = Math.Clamp(page, 1, pageCount);
        var slice = decided.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        if (slice.Count == 0)
        {
            return new ActivityListPageDto([], page, pageSize, decided.Count);
        }

        var ids = slice.Select(row => row.Id).ToList();
        var activities = await dbContext.Set<Activity>()
            .AsNoTracking()
            .Where(activity => ids.Contains(activity.Id))
            .Include(activity => activity.ActivityType)
            .Include(activity => activity.Transitions)
            .ToDictionaryAsync(activity => activity.Id, cancellationToken);

        var epaIds = activities.Values
            .Where(activity => activity.EpaId is not null)
            .Select(activity => activity.EpaId!.Value)
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

        // In the page's order; a row deleted between the two reads is left out rather than failing the page.
        var rows = slice.Where(row => activities.ContainsKey(row.Id)).ToList();
        var facts = rows.Select(row => ActivityRowFacts.From(activities[row.Id], EpaOf(activities[row.Id])?.Code)).ToList();
        var forms = await PinnedForms.LoadAsync(
            dbContext, facts.Select(fact => (fact.ActivityTypeId, fact.SchemaVersion)), cancellationToken);
        // Named as the waiting rows are (WaitingForYou), without E7's nominee suffix, which would be the caller's own name.
        var (details, names) = await ActivityRowDetails.ResolveAsync(
            facts,
            fact => forms[(fact.ActivityTypeId, fact.SchemaVersion)],
            _ => false,
            principal,
            users,
            cancellationToken,
            alsoNamed: rows.Select(row => activities[row.Id].SubjectUserId));

        var items = rows
            .Select(row =>
            {
                var activity = activities[row.Id];
                var epa = EpaOf(activity);
                return new ActivitySummaryDto(
                    activity.Id,
                    activity.ActivityTypeId,
                    activity.ActivityType.Key,
                    activity.ActivityType.Name,
                    activity.SubjectUserId,
                    activity.CurrentState,
                    // The state the decision left it in, as its pinned workflow names it (T220).
                    PinnedWorkflows.StateLabel(row.Workflow, activity.CurrentState),
                    activity.CreatedOn,
                    activity.UpdatedOn,
                    activity.EpaId,
                    epa?.Code,
                    epa?.Title,
                    epa?.InForce,
                    activity.ObservedOn,
                    activity.ObservedOnSource == ObservationDateSource.Declared,
                    // The latest transition that evaluated credit (T108): the Credit column.
                    activity.Transitions
                        .Where(transition => transition.CreditedItemCount is not null)
                        .OrderByDescending(transition => transition.OccurredOn)
                        .ThenByDescending(transition => transition.Id)
                        .Select(transition => transition.CreditedItemCount)
                        .FirstOrDefault())
                {
                    SubjectName = names.NameOf(activity.SubjectUserId),
                    DisplayName = details[activity.Id].DisplayName,
                    DecidedOn = row.DecidedOn,
                    IsFinished = row.IsFinished
                };
            })
            .ToList();

        return new ActivityListPageDto(items, page, pageSize, decided.Count);
    }
}
