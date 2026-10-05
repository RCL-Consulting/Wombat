using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Programme.Commands.SendActivityReminder;
using Wombat.Application.Features.Programme.Waiting;
using Wombat.Web.Components.Shared.Activities;

namespace Wombat.Web.Components.Shared.Programme;

/// <summary>
/// Every phrase of Send a reminder (T358, flow 06; C4; E1; round 3 items 22, 23, 29; R2-Waiting w2–w8, R2-Registrar
/// r2d, r2r, r2x): the button and its name, the dialog, the result and each refusal, the "Reminded …" record and the "No
/// reminder: …" line. One place, so the list's cell and the registrar page cannot word one thing two ways.
/// </summary>
/// <remarks>
/// <para>
/// No third-person pronoun (round 3 check 1): every sentence names the assessor and the registrar, or says "the person
/// this request names". The reader is addressed as "you", as every Wombat page does.
/// </para>
/// <para>
/// The mail's own words come from the mail (<see cref="AssessorPendingNudgeEmail" />): its subject and its days phrase
/// ("waiting less than a day", "waiting 1 day", "waiting 8 days"), so the dialog says exactly what the assessor will read.
/// Dates are ISO and times South African with the zone (<see cref="ActivityMoments.When" />).
/// </para>
/// </remarks>
public static class ReminderWords
{
    /// <summary>The button's words. Its accessible name is <see cref="ButtonName" />.</summary>
    public const string Button = "Send a reminder";

    /// <summary>The dialog's safe button, which has the focus when it opens.</summary>
    public const string Cancel = "Don't send";

    /// <summary>The dialog's confirm.</summary>
    public const string Confirm = "Send the reminder";

    /// <summary>The confirm while the reminder is on its way (w2b).</summary>
    public const string Sending = "Sending…";

    /// <summary>
    /// The result when the send itself failed (not a refusal): fixed words, the failure to the log (T329). Not in the
    /// boards; the build's (T358, lane A1).
    /// </summary>
    public const string Failed =
        "Not sent. Something went wrong, and no reminder was sent. Try again, or come back in a few minutes.";

    /// <summary>
    /// The button's accessible name (2.4.4): "Send Thandi Zulu a reminder about Mini-CEX (Paediatrics) · PAED-004 ·
    /// 2026-10-01, from Nomsa Mahlangu". <paramref name="linkName" /> is the row link's own name where
    /// <c>ActivityRowNames.Waiting</c> gave it one, so two rows that read the same say "(1 of 2)" here too (round 3 item 29).
    /// </summary>
    public static string ButtonName(ActivitySummaryDto item, string? linkName)
    {
        ArgumentNullException.ThrowIfNull(item);
        return $"Send {AssessorOf(item)} a reminder about {linkName ?? ActivityRowNames.WaitingLinkWords(item)}";
    }

    /// <summary>The dialog's title: "Send Thandi Zulu a reminder?".</summary>
    public static string DialogTitle(ActivitySummaryDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return $"Send {AssessorOf(item)} a reminder?";
    }

    /// <summary>
    /// What the mail is: "Thandi Zulu gets one email, "Activities awaiting your assessment", listing this request:
    /// Mini-CEX (Paediatrics) from Nomsa Mahlangu — waiting 8 days."
    /// </summary>
    public static string DialogMail(ActivitySummaryDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return $"{AssessorOf(item)} gets one email, \"{AssessorPendingNudgeEmail.Subject}\", listing this request: " +
               $"{item.ActivityTypeName} from {item.SubjectName} — {AssessorPendingNudgeEmail.WaitingPhrase(item.WaitedDays ?? 0)}.";
    }

    /// <summary>
    /// What it does not do (C4c): "It moves nothing: the request stays Requested, its wait is not restarted, and Nomsa
    /// Mahlangu is not told."
    /// </summary>
    public static string DialogMovesNothing(ActivitySummaryDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return $"It moves nothing: the request stays {item.CurrentStateLabel}, its wait is not restarted, and " +
               $"{item.SubjectName} is not told.";
    }

    /// <summary>The dialog's body: what the mail is, then what it does not do (w2).</summary>
    public static string DialogBody(ActivitySummaryDto item) => $"{DialogMail(item)} {DialogMovesNothing(item)}";

    /// <summary>Whether the answer is a refusal: anything but sent.</summary>
    public static bool IsRefusal(SendActivityReminderResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.Outcome != ReminderOutcome.Sent;
    }

    /// <summary>The result's lead, its strong first words: "Reminder sent to Thandi Zulu." or "Not sent.".</summary>
    public static string Lead(SendActivityReminderResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.Outcome == ReminderOutcome.Sent ? $"Reminder sent to {result.AssessorName}." : "Not sent.";
    }

    /// <summary>
    /// The rest of the result (w3, w5–w8): what the mail listed and that nothing moved, or why it was not sent.
    /// </summary>
    public static string Detail(SendActivityReminderResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.Outcome switch
        {
            ReminderOutcome.Sent =>
                $"It lists {result.ActivityName}, from {result.SubjectName}, {AssessorPendingNudgeEmail.WaitingPhrase(result.WaitedDays ?? 0)}. " +
                $"The request is still {result.CurrentStateLabel}; its wait is unchanged.",
            ReminderOutcome.RemindedToday =>
                $"{result.Reminder?.SentByName ?? "Someone"} reminded {result.AssessorName} today already.",
            ReminderOutcome.Deactivated =>
                $"{Possessive(result.AssessorName)} account is deactivated, so Wombat sends {result.AssessorName} no email. " +
                "The request still waits.",
            ReminderOutcome.NoEmail =>
                $"{result.AssessorName} has no email address in Wombat. Ask your institutional admin to add one.",
            ReminderOutcome.NoAccount =>
                "Wombat has no account for the person this request names; it was erased or deleted, so there is nobody to " +
                "email. The request still waits.",
            ReminderOutcome.MovedMeanwhile => Moved(result),
            _ => "This request is no longer on your list."
        };
    }

    /// <summary>
    /// The whole result, as one sentence run: "Reminder sent to Thandi Zulu. It lists Mini-CEX (Paediatrics) · PAED-004 ·
    /// 2026-10-01, from Nomsa Mahlangu, waiting 8 days. The request is still Requested; its wait is unchanged.", or "Not
    /// sent. " and the reason.
    /// </summary>
    public static string Result(SendActivityReminderResult result) => $"{Lead(result)} {Detail(result)}";

    /// <summary>The record of the last reminder (w3, w4, r2r): "Reminded 2026-10-04 by Pieter Smit".</summary>
    public static string Reminded(ActivityReminderDto reminder)
    {
        ArgumentNullException.ThrowIfNull(reminder);
        return $"Reminded {QuotaText.Iso(reminder.SentOnDay)} by {reminder.SentByName}";
    }

    /// <summary>
    /// Why the row has no button (E3's w9), as the list was read: "No reminder: Fatima Khumalo's account is
    /// deactivated.", "No reminder: Thandi Zulu has no email address in Wombat.", "No reminder: the person this request
    /// names has no account."; null when a reminder may be sent.
    /// </summary>
    public static string? CannotRemind(ActivitySummaryDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return item.CannotRemind switch
        {
            ReminderOutcome.Deactivated => $"No reminder: {Possessive(AssessorOf(item))} account is deactivated.",
            ReminderOutcome.NoEmail => $"No reminder: {AssessorOf(item)} has no email address in Wombat.",
            ReminderOutcome.NoAccount => "No reminder: the person this request names has no account.",
            _ => null
        };
    }

    /// <summary>
    /// Moved meanwhile (w8): "Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01 moved at 2026-10-04 08:12 SAST: it is now
    /// Completed and waits for nobody."; when it still waits for a named person (saved since, or handed to another),
    /// "… it is now Requested, with Mohammed Patel." D7's not found never says this, which would tell a state.
    /// </summary>
    private static string Moved(SendActivityReminderResult result)
    {
        var when = result.MovedOn is { } moved ? $" moved at {ActivityMoments.When(moved)}" : " moved";
        return result.StillWaiting
            ? $"{result.ActivityName}{when}: it is now {result.CurrentStateLabel}, with {result.AssessorName}."
            : $"{result.ActivityName}{when}: it is now {result.CurrentStateLabel} and waits for nobody.";
    }

    private static string AssessorOf(ActivitySummaryDto item) => item.Holder?.Name ?? item.NomineeName ?? "the assessor";

    private static string Possessive(string? name) => string.IsNullOrEmpty(name) ? "The assessor's" : $"{name}'s";
}
