using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Activities;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Programme.Waiting;

/// <summary>
/// What waits for a named assessor in one programme, read as one role (T358, flow 06; Q3, C3, C7, C11; E3, E4; review
/// 12): the read behind Waiting for assessors, Home's card of that name, and the registrar page's section. One read, so
/// the card and the page it opens cannot disagree (E4).
/// </summary>
/// <remarks>
/// <para>
/// <b>Which rows.</b> An activity awaiting a reviewer by its PINNED workflow (<see cref="ActivityWaiting.LoadAwaitingReviewerAsync" />,
/// the predicate the Coordinator's stalled card and the nightly nudge read, T297), whose next move is one named person's
/// (<see cref="ActivityHolders.Resolve" /> is <see cref="ActivityHolderKind.Person" />: every arm that is not the author's
/// is a <c>field:</c> arm, and they all name the same user). So a SpecialityAdmin's review of a teaching session, the
/// Coordinator's MSF release and a request whose nominee field is empty are never listed (E3): role-held work is the
/// reader's own queue, and "Waiting for assessors" names an assessor or nobody.
/// </para>
/// <para>
/// <b>Whose.</b> The scope of the role read as (<see cref="ProgrammeScope.Activities" />), never the union of the roles
/// held (E4), and through the list read rule (<see cref="ActivityReadScope.WhereReadableBy" />, T101) as well. Less the
/// reader's own requests (E4): a Coordinator who is also a registrar finds those on My activities.
/// </para>
/// <para>
/// <b>The clock</b> is <see cref="WaitingForYou" />'s (flow 04): oldest first by <see cref="Activity.UpdatedOn" />, then
/// id; whole days since <c>UpdatedOn</c>, rounded down; overdue at <c>AssessorDueDays</c> × 24 h. A reminder never
/// touches <c>UpdatedOn</c>, so sending one restarts no wait (C4c).
/// </para>
/// <para>
/// <b>The reminders.</b> Each row carries its newest reminder, whether one was sent today on the South African calendar
/// (the same-day block every staff member meets, round 3), and whether its nominee cannot be reminded at all
/// (<see cref="ReminderRecipientRules" />, read from <see cref="IReminderRecipients" />), so the deactivated row says so
/// before anyone presses (E3's w9).
/// </para>
/// </remarks>
public static class WaitingForAssessorsReader
{
    public static async Task<WaitingForAssessorsDto> ReadAsync(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        IReminderRecipients recipients,
        TimeProvider clock,
        DashboardThresholds thresholds,
        ClaimsPrincipal principal,
        ProgrammeScopeDto scope,
        WaitingForAssessorsFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(recipients);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(thresholds);
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(filter);

        pageSize = Math.Max(1, pageSize);
        var mayRemind = ProgrammeScope.WaitingRoles.Contains(scope.ActingRole, StringComparer.Ordinal);
        var callerUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var dueDays = thresholds.AssessorDueDays;

        var awaiting = await ActivityWaiting.LoadAwaitingReviewerAsync(dbContext, cancellationToken);

        var query = ProgrammeScope.Activities(awaiting.Narrow(dbContext.Set<Activity>().AsNoTracking()), scope)
            .WhereReadableBy(principal)
            // E4: the reader's own requests are not listed, whatever arm would admit them.
            .Where(activity => activity.SubjectUserId != callerUserId);
        if (filter.SubjectUserId is { } subjectUserId)
        {
            query = query.Where(activity => activity.SubjectUserId == subjectUserId);
        }

        // With the transitions: the credit column, as every list reads it (T108).
        var candidates = await query
            .Include(activity => activity.ActivityType)
            .Include(activity => activity.Transitions)
            .ToListAsync(cancellationToken);

        // The exact test per pin (WaitingStates.Waits), then the named-person test (E3), then the oldest.
        var waiting = candidates
            .Select(activity => (Activity: activity, Workflow: awaiting.WorkflowOf(activity.ActivityTypeId, activity.SchemaVersion)))
            .Where(row => awaiting.Waits(row.Activity.ActivityTypeId, row.Activity.SchemaVersion, row.Activity.CurrentState))
            .Select(row => (row.Activity, row.Workflow, Holder: ActivityHolders.Resolve(
                row.Workflow, row.Activity.CurrentState, row.Activity.SubjectUserId, row.Activity.CreatedByUserId,
                row.Activity.DataJson, null, callerUserId)))
            .Where(row => row.Holder.Kind == ActivityHolderKind.Person && row.Holder.UserId is not null)
            .OrderBy(row => row.Activity.UpdatedOn)
            .ThenBy(row => row.Activity.Id)
            .ToList();

        if (waiting.Count == 0)
        {
            return new WaitingForAssessorsDto(
                scope, [], 0, 0, 0, 0, [], dueDays, thresholds.AssessorNudgeDays, 1, pageSize, mayRemind) { Filter = filter };
        }

        var activityIds = waiting.Select(row => row.Activity.Id).ToArray();
        var reminders = (await dbContext.Set<ActivityReminder>()
                .AsNoTracking()
                .Where(reminder => activityIds.Contains(reminder.ActivityId))
                .ToListAsync(cancellationToken))
            .GroupBy(reminder => reminder.ActivityId)
            .ToDictionary(group => group.Key, group => group.ToList());

        // T137, T231: the EPA each row is about, from the stamped column, with whether it is in force now, in one read.
        var epaIds = waiting
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

        var facts = waiting.Select(row => ActivityRowFacts.From(row.Activity, EpaOf(row.Activity)?.Code)).ToList();
        var forms = await PinnedForms.LoadAsync(
            dbContext, facts.Select(fact => (fact.ActivityTypeId, fact.SchemaVersion)), cancellationToken);
        // Every name the rows show, in one lookup (T142): the holders and nominees (the details), whose each row is, and
        // who sent each row's last reminder. sharesTheRest false, as the assessor's list: the page tells two rows that
        // read the same apart by their state and their wait (ActivityRowNames.Waiting, C11).
        var lastReminders = waiting.ToDictionary(
            row => row.Activity.Id,
            row => reminders.TryGetValue(row.Activity.Id, out var sent)
                ? sent.OrderByDescending(reminder => reminder.SentOn).ThenByDescending(reminder => reminder.Id).First()
                : null);
        var (details, names) = await ActivityRowDetails.ResolveAsync(
            facts,
            fact => forms[(fact.ActivityTypeId, fact.SchemaVersion)],
            _ => false,
            principal,
            users,
            cancellationToken,
            alsoNamed: waiting.Select(row => row.Activity.SubjectUserId)
                .Concat(lastReminders.Values.Where(reminder => reminder is not null).Select(reminder => reminder!.SentByUserId)));

        var holderIds = waiting.Select(row => row.Holder.UserId!).Distinct(StringComparer.Ordinal).ToList();
        var accounts = await recipients.LoadAsync(holderIds, cancellationToken);

        var now = clock.GetUtcNow().UtcDateTime;
        var today = QuotaCalendar.Today(clock);
        var due = TimeSpan.FromDays(dueDays);
        var rows = waiting
            .Select(row =>
            {
                var activity = row.Activity;
                var epa = EpaOf(activity);
                var waited = now - activity.UpdatedOn;
                var detail = details[activity.Id];
                var last = lastReminders[activity.Id];
                return new ActivitySummaryDto(
                    activity.Id,
                    activity.ActivityTypeId,
                    activity.ActivityType.Key,
                    activity.ActivityType.Name,
                    activity.SubjectUserId,
                    activity.CurrentState,
                    // In the words of the pinned workflow the waiting test just read (T220).
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
                    DisplayName = detail.DisplayName,
                    Holder = detail.Holder,
                    NomineeName = detail.NomineeName ?? detail.Holder.Name,
                    Shape = detail.Shape,
                    IsOverdue = waited >= due,
                    WaitedDays = Math.Max(0, (int)waited.TotalDays),
                    LastReminder = last is null
                        ? null
                        : new ActivityReminderDto(last.SentOn, last.SentOnDay, names.NameOf(last.SentByUserId)),
                    RemindedToday = reminders.TryGetValue(activity.Id, out var sent) && sent.Any(reminder => reminder.SentOnDay == today),
                    CannotRemind = ReminderRecipientRules.RefusalFor(accounts.GetValueOrDefault(row.Holder.UserId!))
                };
            })
            .ToList();

        // Review 12: the With filter's names, from every row's nominee, by surname then first name (T298's order).
        var nominees = rows
            .Select(row => row.Holder!)
            .DistinctBy(holder => holder.UserId, StringComparer.Ordinal)
            .Select(holder => (
                Option: new NomineeOptionDto(holder.UserId!, holder.Name ?? names.NameOf(holder.UserId!)),
                Account: accounts.GetValueOrDefault(holder.UserId!)))
            .OrderBy(entry => entry.Account?.LastName ?? entry.Option.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(entry => entry.Account?.FirstName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(entry => entry.Option.UserId, StringComparer.Ordinal)
            .Select(entry => entry.Option)
            .ToList();

        var match = rows
            .Where(row => !filter.OverdueOnly || row.IsOverdue)
            .Where(row => filter.WithUserId is null || string.Equals(row.Holder!.UserId, filter.WithUserId, StringComparison.Ordinal))
            .ToList();

        var lastPage = Math.Max(1, (match.Count + pageSize - 1) / pageSize);
        var pageNumber = Math.Clamp(page, 1, lastPage);

        return new WaitingForAssessorsDto(
            scope,
            match.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
            match.Count,
            match.Count(row => row.IsOverdue),
            rows.Count,
            rows.Count(row => row.IsOverdue),
            nominees,
            dueDays,
            thresholds.AssessorNudgeDays,
            pageNumber,
            pageSize,
            mayRemind)
        {
            Filter = filter
        };
    }
}
