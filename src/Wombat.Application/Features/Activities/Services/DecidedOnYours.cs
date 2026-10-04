using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Activities.Services;

/// <summary>
/// What was decided on the caller's own requests (T355, B1; E6; note 1): the read behind the Trainee's Home "Recent
/// decisions", the mirror for the subject of <see cref="DecidedByYou" />, which leaves the caller's own rows out and so
/// cannot be asked for them by a parameter.
/// </summary>
/// <remarks>
/// <para>
/// A decision on her request is an activity whose subject is the caller, whose latest move (by time, ties to the later
/// id, as the credit column reads the latest, T108) someone else made, and that is finished in its PINNED workflow or has
/// no move left (T297; D44, <see cref="ActivityCompletion" />). So a completion, a decline, a discussion, a sign-off and
/// an accept are decisions whatever their state is called (E6: the rule, not a list); a return to her draft has a move
/// left and is not; her own logged procedure was moved last by her and is not.
/// </para>
/// <para>
/// A create is never a decision, whoever makes it (flow 04's R1 fix): a supervisor who logs a procedure on her behalf, on
/// a type born terminal, recorded it and decided nothing. A system-managed type's record is never listed (MSF: "MSF
/// records are not listed", Spec R1): the release writes it, and nobody decided it.
/// </para>
/// <para>
/// "Finished or no move left" is a question for each pin's workflow in C#, not SQL, so every activity of hers someone else
/// moved last is read (a few columns each) and judged before the list is cut, as <see cref="DecidedByYou" /> does: one
/// registrar's programme, not a table. Each row is named by E7 over her whole list (note 1), as My activities names it
/// (<c>ListActivitiesBySubjectQuery</c>), so a row reads the same on Home and there; and carries its latest credit (T108)
/// and the count it made (<see cref="EpaCountLines" />, E5).
/// </para>
/// <para>
/// Her rows go through <c>WhereReadableBy</c>, whose subject arm admits them all: the same rows My activities lists.
/// </para>
/// </remarks>
public static class DecidedOnYours
{
    /// <param name="take">How many of the newest decisions to return (Home lists five).</param>
    /// <param name="today">Today on the South African calendar (T325): which window is the current one for the count line.</param>
    public static async Task<IReadOnlyList<ActivitySummaryDto>> ReadAsync(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        ClaimsPrincipal principal,
        int take,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(principal);

        var callerUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(callerUserId) || take <= 0)
        {
            return [];
        }

        var hers = dbContext.Set<Activity>()
            .AsNoTracking()
            .Where(activity => activity.SubjectUserId == callerUserId)
            .WhereReadableBy(principal);

        var lastMovedByOthers = await hers
            .Where(activity => !activity.ActivityType.SystemManaged)
            .Where(activity => activity.Transitions.Any())
            .Where(activity => activity.Transitions
                .OrderByDescending(transition => transition.OccurredOn)
                .ThenByDescending(transition => transition.Id)
                .Select(transition => transition.ActorUserId)
                .FirstOrDefault() != callerUserId)
            // A create is not a decision, whoever makes it (flow 04's R1 fix): see the remarks.
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
                // The other person's move is the latest, so the latest time is when they decided.
                DecidedOn = activity.Transitions.Max(transition => transition.OccurredOn)
            })
            .ToListAsync(cancellationToken);

        var workflows = await PinnedWorkflows.LoadAsync(
            dbContext, lastMovedByOthers.Select(row => (row.ActivityTypeId, row.SchemaVersion)), cancellationToken);

        var decided = lastMovedByOthers
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
            .Take(take)
            .ToList();

        if (decided.Count == 0)
        {
            return [];
        }

        var ids = decided.Select(row => row.Id).ToList();
        var activities = await dbContext.Set<Activity>()
            .AsNoTracking()
            .Where(activity => ids.Contains(activity.Id))
            .WhereReadableBy(principal)
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

        // In the list's order; a row deleted between the two reads is left out rather than failing Home.
        var rows = decided.Where(row => activities.ContainsKey(row.Id)).ToList();
        var facts = rows.Select(row => ActivityRowFacts.From(activities[row.Id], EpaOf(activities[row.Id])?.Code)).ToList();

        // E7 over her whole list (note 1): a name carries its nominee when ANY of her activities shares its type, EPA and
        // date, exactly as My activities names the row.
        var shared = await ActivityRowDetails.SharedKeysAsync(hers, facts, cancellationToken);
        var forms = await PinnedForms.LoadAsync(
            dbContext, facts.Select(fact => (fact.ActivityTypeId, fact.SchemaVersion)), cancellationToken);
        var (details, _) = await ActivityRowDetails.ResolveAsync(
            facts,
            fact => forms[(fact.ActivityTypeId, fact.SchemaVersion)],
            fact => shared.Contains(fact.CollisionKey),
            principal,
            users,
            cancellationToken);

        // Whether each pin can credit (T355, build review R4): its pinned version's credit rules, else the type's own, as a
        // pin resolves elsewhere (PinnedWorkflows). A rule set that does not parse credits nothing.
        var typeIds = activities.Values.Select(activity => activity.ActivityTypeId).Distinct().ToList();
        var pinnedRules = (await dbContext.Set<ActivityTypeVersion>()
                .AsNoTracking()
                .Where(version => typeIds.Contains(version.ActivityTypeId))
                .Select(version => new { version.ActivityTypeId, version.Version, version.CreditRulesJson })
                .ToListAsync(cancellationToken))
            .GroupBy(version => (version.ActivityTypeId, version.Version))
            .ToDictionary(group => group.Key, group => group.First().CreditRulesJson);
        bool CanCredit(Activity activity)
        {
            var rules = pinnedRules.TryGetValue((activity.ActivityTypeId, activity.SchemaVersion), out var pinned)
                ? pinned
                : activity.ActivityType.CreditRulesJson;
            try
            {
                return EncounterDatePolicy.CanCredit(rules);
            }
            catch (Exception)
            {
                return false;
            }
        }

        var countLines = await EpaCountLines.ReadAsync(
            dbContext,
            callerUserId,
            activities.Values
                .Where(activity => activity.EpaId is not null)
                .Select(activity => (activity.EpaId!.Value, activity.ObservedOn))
                .ToList(),
            today,
            cancellationToken);

        return rows
            .Select(row =>
            {
                var activity = activities[row.Id];
                var epa = EpaOf(activity);
                var detail = details[activity.Id];
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
                    // The latest transition that evaluated credit (T108): "Credits nothing." when it is none.
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
                    Shape = detail.Shape,
                    DecidedOn = row.DecidedOn,
                    IsFinished = row.IsFinished,
                    CountLine = activity.EpaId is int epaId &&
                                countLines.TryGetValue((epaId, activity.ObservedOn), out var line)
                        ? line
                        : null,
                    // By the pinned workflow, as File it again reads it (R4; E6: the rule, not a list).
                    Declined = ActivityDecline.MoveOf(row.Workflow, activity) is not null,
                    CanCredit = CanCredit(activity)
                };
            })
            .ToList();
    }
}
