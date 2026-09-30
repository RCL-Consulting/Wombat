using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Activities.Services;

/// <summary>
/// What waits on the caller as someone else's assessor, reviewer or admin (T350, note 5; round 1, Q1, Q4, E5): the read
/// behind <c>ListWaitingForYouQuery</c>, which the Assessor's Home (<c>GetAssessorDashboardSummaryQuery</c>), the
/// activity page's way on and the other-role line make too. One read, so Home's "Waiting for you", the Activity inbox and
/// the way on list the same rows in the same order and words (T297).
/// </summary>
/// <remarks>
/// <para>
/// A row is an activity a move of which, one that leads on, the caller may make now by the arms of its actor rule that
/// are not the author's (<see cref="ActivityWaiting.LoadActionableAsync" />, <see cref="ActorArms.NotAuthor" />): a
/// <c>field:</c> arm naming them, a <c>role:</c> or a <c>scope:</c> arm they hold. Less the caller's own subject rows,
/// which moved in from the Assessor's dashboard (E5): an assessor who is also a trainee finds their own requests on My
/// activities, not here. Oldest first by <see cref="Activity.UpdatedOn" />, then id, so the one that has waited longest
/// leads, where the inbox used to list newest first.
/// </para>
/// <para>
/// <b>The one clock</b> (note 10; round 1, correction c; Spec § 7). A row has waited since its <c>UpdatedOn</c>, the
/// clock the Coordinator's stall and the assessor nudge read too, and it counts whole days as the nudge counts them,
/// rounded down. The activity page's status card says "since" from the newest move's time
/// (<see cref="ActivityHolderDto.Since" />). The two agree whenever the last write was a move, which is every write an
/// activity waiting on someone else receives in the shipped workflows; they part only after a data write without a move
/// (a save), which bumps <c>UpdatedOn</c> and so restarts the row's wait.
/// </para>
/// <para>
/// Overdue is <c>now − UpdatedOn ≥ AssessorDueDays × 24 h</c> (round 2, E1), "now" from the <see cref="TimeProvider" />,
/// so a whole-day count of 7 always carries it and the rule line's "7 days" is the same number.
/// </para>
/// <para>
/// Each row's name ("Type · EPA · date") is read by <see cref="ActivityRowDetails.ResolveAsync" />, the code My
/// activities names its rows by, with <c>sharesTheRest</c> false: E7's nominee suffix is the trainee's way to tell two
/// of her own requests apart, and on this list the nominee is the reader, so it would append their own name (note 5).
/// Two rows that read the same are told apart on the page instead (<c>ActivityRowNames.Waiting</c>, note 9).
/// </para>
/// </remarks>
public static class WaitingForYou
{
    public static async Task<WaitingForYouDto> ReadAsync(
        IApplicationDbContext dbContext,
        IWorkflowEvaluator workflowEvaluator,
        IUserAdministrationService users,
        TimeProvider clock,
        DashboardThresholds thresholds,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(workflowEvaluator);
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(thresholds);
        ArgumentNullException.ThrowIfNull(principal);

        var callerUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var dueDays = thresholds.AssessorDueDays;

        // With the transitions: the row's name reads the newest move, and the credit column the latest evaluation (T108).
        var actionable = (await ActivityWaiting.LoadActionableAsync(
                dbContext.Set<Activity>(), dbContext, workflowEvaluator, principal, withTransitions: true, cancellationToken,
                ActorArms.NotAuthor))
            .Where(row => row.Activity.SubjectUserId != callerUserId)
            .OrderBy(row => row.Activity.UpdatedOn)
            .ThenBy(row => row.Activity.Id)
            .ToList();
        if (actionable.Count == 0)
        {
            return new WaitingForYouDto([], 0, dueDays);
        }

        // T137, T231: the EPA each row is about, from the stamped column, with whether it is in force now, in one read.
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
        var forms = await PinnedForms.LoadAsync(
            dbContext, facts.Select(fact => (fact.ActivityTypeId, fact.SchemaVersion)), cancellationToken);
        // Whose each row is, by name, in the same lookup as the names the details read (T142's one lookup).
        var (details, names) = await ActivityRowDetails.ResolveAsync(
            facts,
            fact => forms[(fact.ActivityTypeId, fact.SchemaVersion)],
            _ => false,
            principal,
            users,
            cancellationToken,
            alsoNamed: actionable.Select(row => row.Activity.SubjectUserId));

        var now = clock.GetUtcNow().UtcDateTime;
        var due = TimeSpan.FromDays(dueDays);
        var items = actionable
            .Select(row =>
            {
                var activity = row.Activity;
                var epa = EpaOf(activity);
                var waited = now - activity.UpdatedOn;
                return new ActivitySummaryDto(
                    activity.Id,
                    activity.ActivityTypeId,
                    activity.ActivityType.Key,
                    activity.ActivityType.Name,
                    activity.SubjectUserId,
                    activity.CurrentState,
                    // In the words of the pinned workflow the act gate just read (T220).
                    PinnedWorkflows.StateLabel(row.Workflow, activity.CurrentState),
                    activity.CreatedOn,
                    activity.UpdatedOn,
                    activity.EpaId,
                    epa?.Code,
                    epa?.Title,
                    epa?.InForce,
                    activity.ObservedOn,
                    activity.ObservedOnSource == ObservationDateSource.Declared,
                    // The same rule as every list: the latest transition that evaluated credit (T108).
                    activity.Transitions
                        .Where(transition => transition.CreditedItemCount is not null)
                        .OrderByDescending(transition => transition.OccurredOn)
                        .ThenByDescending(transition => transition.Id)
                        .Select(transition => transition.CreditedItemCount)
                        .FirstOrDefault())
                {
                    SubjectName = names.NameOf(activity.SubjectUserId),
                    DisplayName = details[activity.Id].DisplayName,
                    IsOverdue = waited >= due,
                    WaitedDays = Math.Max(0, (int)waited.TotalDays)
                };
            })
            .ToList();

        return new WaitingForYouDto(items, items.Count(item => item.IsOverdue), dueDays);
    }
}
