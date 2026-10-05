using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Persistence;
using Wombat.Application.Common.Users;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Programme.Waiting;
using Wombat.Domain.Activities;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Programme.Commands.SendActivityReminder;

/// <summary>What became of a reminder (T358, flow 06; C4; E1; D7).</summary>
public enum ReminderOutcome
{
    /// <summary>Recorded, and one mail handed to the sender.</summary>
    Sent,

    /// <summary>
    /// Someone has reminded the assessor about this request today already, on the South African calendar: one reminder per
    /// request per day, whoever sends it (round 3's settled rule; D6 when two meet the unique index).
    /// </summary>
    RemindedToday,

    /// <summary>The nominee's account is deactivated: an administrator's lock or an erasure.</summary>
    Deactivated,

    /// <summary>The nominee has no email address in Wombat.</summary>
    NoEmail,

    /// <summary>The nominee field names no account: erased or deleted.</summary>
    NoAccount,

    /// <summary>
    /// The request moved since the list was read: no longer waiting for a named person, in another state, or saved since
    /// (any save changes its <c>UpdatedOn</c>, a new nominee among them).
    /// </summary>
    MovedMeanwhile,

    /// <summary>
    /// No such request, or one the reader may not read, or one outside the scope of the role sent as, or the reader's own
    /// (D7): never the moved-meanwhile words, which would tell its state.
    /// </summary>
    NotFound
}

/// <summary>
/// A member of staff reminds the assessor a waiting request names (T358, flow 06; Q4, C4, E1; review 1–3): one email,
/// the nudge's "Activities awaiting your assessment" listing this request, and a record of who sent it when.
/// </summary>
/// <param name="ActingRole">The role sent as, which must be one of <see cref="ProgrammeScope.WaitingRoles" />.</param>
/// <param name="ExpectedState">The row's <c>CurrentState</c> as the list read it.</param>
/// <param name="ExpectedUpdatedOn">The row's <c>UpdatedOn</c> as the list read it.</param>
/// <remarks>
/// <para>
/// <b>It moves nothing</b> (C4c). It is not a move: no transition, no state change, no <c>UpdatedOn</c>, so the wait
/// is not restarted, and the registrar is not told. The activity is never read tracked, so the save cannot reach it.
/// </para>
/// <para>
/// <b>It refuses by answering, never by throwing</b> (build-lanes § the audit pipeline): <c>AuditPipelineBehavior</c>
/// commits a failed handler's staged rows, so every check comes first and the one row is staged only once nothing can
/// refuse it. Two senders in the same instant both pass the check and meet the unique index on (request, South African
/// day): the second's row is detached, so the audit write does not send it again, and the answer is
/// <see cref="ReminderOutcome.RemindedToday" /> (D6). The mail is handed over only after the save.
/// </para>
/// <para>
/// Opting out of digest emails does not refuse it (E1): a reminder about one request is "email about one particular
/// thing", which the account page says is still sent.
/// </para>
/// </remarks>
public sealed record SendActivityReminderCommand(
    ClaimsPrincipal Principal,
    string ActingRole,
    int ActivityId,
    string ExpectedState,
    DateTime ExpectedUpdatedOn) : IRequest<SendActivityReminderResult>;

/// <summary>What a reminder did, and the words the page needs to say so (<c>ReminderWords.Result</c>).</summary>
/// <param name="AssessorName">Whom it was for: the request's nominee, by name.</param>
/// <param name="ActivityName">The request as the list names it: "Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01".</param>
/// <param name="SubjectName">Whose request it is.</param>
/// <param name="WaitedDays">Its wait in whole days, as the mail says it.</param>
/// <param name="CurrentStateLabel">Its state as its pinned workflow labels it.</param>
/// <param name="MovedOn">When it last moved (UTC): set for <see cref="ReminderOutcome.MovedMeanwhile" />.</param>
/// <param name="Reminder">The reminder sent, or today's that blocked this one.</param>
public sealed record SendActivityReminderResult(
    ReminderOutcome Outcome,
    string? AssessorName,
    string? ActivityName,
    string? SubjectName,
    int? WaitedDays,
    string? CurrentStateLabel,
    DateTime? MovedOn,
    ActivityReminderDto? Reminder)
{
    /// <summary>
    /// For <see cref="ReminderOutcome.MovedMeanwhile" />: whether it still waits for a named person (another nominee, or
    /// only saved since), so the page does not say it "waits for nobody" when it waits for someone (T358, lane A1).
    /// </summary>
    public bool StillWaiting { get; init; }

    /// <summary>The answer for a request the reader cannot reach (D7).</summary>
    public static SendActivityReminderResult NotFound { get; } = new(ReminderOutcome.NotFound, null, null, null, null, null, null, null);
}

/// <summary>
/// The command's shape only: whether the reminder may be sent is the handler's, answered as an outcome, never thrown
/// (the audit pipeline's trap).
/// </summary>
public sealed class SendActivityReminderCommandValidator : AbstractValidator<SendActivityReminderCommand>
{
    public SendActivityReminderCommandValidator()
    {
        RuleFor(command => command.Principal).NotNull();
        RuleFor(command => command.ActingRole).NotEmpty();
        RuleFor(command => command.ActivityId).GreaterThan(0);
        RuleFor(command => command.ExpectedState).NotEmpty();
    }
}

public sealed class SendActivityReminderCommandHandler : IRequestHandler<SendActivityReminderCommand, SendActivityReminderResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;
    private readonly IReminderRecipients _recipients;
    private readonly IEmailSender _emailSender;
    private readonly TimeProvider _clock;

    public SendActivityReminderCommandHandler(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        IReminderRecipients recipients,
        IEmailSender emailSender,
        TimeProvider clock)
    {
        _dbContext = dbContext;
        _users = users;
        _recipients = recipients;
        _emailSender = emailSender;
        _clock = clock;
    }

    public async Task<SendActivityReminderResult> Handle(SendActivityReminderCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // ---- Every check first: nothing is staged until nothing can refuse (the audit pipeline's trap). ----

        if (!ProgrammeScope.WaitingRoles.Contains(request.ActingRole, StringComparer.Ordinal))
        {
            return SendActivityReminderResult.NotFound;
        }

        var scope = await ProgrammeScope.ResolveAsync(_dbContext, request.Principal, request.ActingRole, cancellationToken);
        if (scope is null)
        {
            return SendActivityReminderResult.NotFound;
        }

        var callerUserId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(callerUserId))
        {
            return SendActivityReminderResult.NotFound;
        }

        // The list's own narrowing, untracked: the scope of the role sent as (E4), the read rule (T101), and never the
        // reader's own request (E4). Not found otherwise (D7), so the answer tells nothing of a request out of reach.
        var activity = await ProgrammeScope.Activities(_dbContext.Set<Activity>().AsNoTracking(), scope)
            .WhereReadableBy(request.Principal)
            .Where(candidate => candidate.Id == request.ActivityId && candidate.SubjectUserId != callerUserId)
            .Include(candidate => candidate.ActivityType)
            .SingleOrDefaultAsync(cancellationToken);
        if (activity is null)
        {
            return SendActivityReminderResult.NotFound;
        }

        var awaiting = await ActivityWaiting.LoadAwaitingReviewerAsync(_dbContext, cancellationToken);
        var workflow = awaiting.WorkflowOf(activity.ActivityTypeId, activity.SchemaVersion);
        var holder = ActivityHolders.Resolve(
            workflow, activity.CurrentState, activity.SubjectUserId, activity.CreatedByUserId, activity.DataJson, null, callerUserId);
        var waitsForAPerson = awaiting.Waits(activity.ActivityTypeId, activity.SchemaVersion, activity.CurrentState) &&
                              holder is { Kind: ActivityHolderKind.Person, UserId: not null };

        // The names the answer gives, read the way the list reads them, in one lookup.
        var epaCode = activity.EpaId is int epaId
            ? await _dbContext.Set<Epa>().AsNoTracking().Where(epa => epa.Id == epaId).Select(epa => epa.Code).FirstOrDefaultAsync(cancellationToken)
            : null;
        var fact = new ActivityRowFacts(
            activity.Id, activity.ActivityTypeId, activity.SchemaVersion, activity.ActivityType.Name, activity.SubjectUserId,
            activity.CreatedByUserId, activity.CurrentState, activity.EpaId, epaCode, activity.ObservedOn,
            activity.ObservedOnSource == ObservationDateSource.Declared, activity.DataJson, null);
        var forms = await PinnedForms.LoadAsync(_dbContext, [(activity.ActivityTypeId, activity.SchemaVersion)], cancellationToken);
        var form = forms[(activity.ActivityTypeId, activity.SchemaVersion)];
        var today = QuotaCalendar.Today(_clock);
        var todays = await _dbContext.Set<ActivityReminder>()
            .AsNoTracking()
            .Where(reminder => reminder.ActivityId == activity.Id && reminder.SentOnDay == today)
            .OrderBy(reminder => reminder.SentOn)
            .FirstOrDefaultAsync(cancellationToken);
        var (details, names) = await ActivityRowDetails.ResolveAsync(
            [fact], _ => form, _ => false, request.Principal, _users, cancellationToken,
            alsoNamed: [activity.SubjectUserId, todays?.SentByUserId, callerUserId]);

        var now = _clock.GetUtcNow().UtcDateTime;
        var waitedDays = Math.Max(0, (int)(now - activity.UpdatedOn).TotalDays);
        var assessorName = holder.UserId is { } holderId ? names.NameOf(holderId) : null;
        var answer = new SendActivityReminderResult(
            ReminderOutcome.Sent,
            assessorName,
            details[activity.Id].DisplayName,
            names.NameOf(activity.SubjectUserId),
            waitedDays,
            PinnedWorkflows.StateLabel(form.Workflow, activity.CurrentState),
            null,
            null);

        if (!waitsForAPerson ||
            !string.Equals(activity.CurrentState, request.ExpectedState, StringComparison.Ordinal) ||
            activity.UpdatedOn.Ticks != request.ExpectedUpdatedOn.Ticks)
        {
            return answer with
            {
                Outcome = ReminderOutcome.MovedMeanwhile,
                AssessorName = waitsForAPerson ? assessorName : null,
                MovedOn = activity.UpdatedOn,
                StillWaiting = waitsForAPerson
            };
        }

        if (todays is not null)
        {
            return answer with { Outcome = ReminderOutcome.RemindedToday, Reminder = ReminderOf(todays, names) };
        }

        var assessorUserId = holder.UserId!;
        var accounts = await _recipients.LoadAsync([assessorUserId], cancellationToken);
        var recipient = accounts.GetValueOrDefault(assessorUserId);
        if (ReminderRecipientRules.RefusalFor(recipient) is { } refusal)
        {
            return answer with { Outcome = refusal };
        }

        // ---- Nothing can refuse it now but the unique index: stage the one row. ----

        var row = ActivityReminder.Record(activity.Id, callerUserId, assessorUserId, now);
        _dbContext.Set<ActivityReminder>().Add(row);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (PostgresErrors.IsUniqueViolation(exception))
        {
            // D6: another sender's row landed between the check and the save. Detached, so the audit write that follows
            // this answer does not send it again.
            _dbContext.Set<ActivityReminder>().Entry(row).State = EntityState.Detached;
            var landed = await _dbContext.Set<ActivityReminder>()
                .AsNoTracking()
                .Where(reminder => reminder.ActivityId == activity.Id && reminder.SentOnDay == today)
                .OrderBy(reminder => reminder.SentOn)
                .FirstOrDefaultAsync(cancellationToken);
            var landedNames = landed is null
                ? names
                : await UserDisplayNames.ResolveAsync(_users, [landed.SentByUserId], cancellationToken);
            return answer with
            {
                Outcome = ReminderOutcome.RemindedToday,
                Reminder = landed is null ? null : ReminderOf(landed, landedNames)
            };
        }

        await _emailSender.SendAsync(
            AssessorPendingNudgeEmail.BuildReminder(
                recipient!.Email!, recipient.FirstName, activity.ActivityType.Name, names.NameOf(activity.SubjectUserId), waitedDays),
            cancellationToken);

        return answer with { Reminder = ReminderOf(row, names) };
    }

    private static ActivityReminderDto ReminderOf(ActivityReminder reminder, UserDisplayNames names)
        => new(reminder.SentOn, reminder.SentOnDay, names.NameOf(reminder.SentByUserId));
}
