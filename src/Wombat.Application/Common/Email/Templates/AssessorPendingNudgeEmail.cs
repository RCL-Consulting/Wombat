using System.Globalization;
using Wombat.Application.Common.Email;

namespace Wombat.Application.Common.Email.Templates;

/// <summary>
/// "Activities awaiting your assessment": the nightly nudge's mail (<c>AssessorPendingNudgeJob</c>), listing every request
/// that has waited on one assessor, and since T358 a staff member's reminder about one of them (<see cref="BuildReminder" />,
/// flow 06; Q4, C4; review 3).
/// </summary>
/// <remarks>
/// The two are one template, so an assessor reads a reminder as the nudge they already know, one request long. Only the
/// tags tell them apart in the mail log (<c>nudge</c>, <c>assessor-pending</c>; <c>reminder</c>, <c>assessor-reminder</c>).
/// A request's wait is whole days rounded down, as the waiting lists count it, and under a day reads "waiting less than a
/// day", never "waiting 0 days" (<see cref="WaitingPhrase" />).
/// </remarks>
public static class AssessorPendingNudgeEmail
{
    /// <summary>The subject both mails carry.</summary>
    public const string Subject = "Activities awaiting your assessment";

    public static EmailMessage Build(string toEmail, string firstName, IReadOnlyList<(string ActivityTypeName, string TraineeName, int DaysWaiting)> pendingActivities)
        => Compose(toEmail, firstName, pendingActivities, ["nudge", "assessor-pending"]);

    /// <summary>
    /// A reminder about one waiting request, sent by a member of staff (T358, flow 06; C4): the nudge's mail with the one
    /// line "Mini-CEX (Paediatrics) from Nomsa Mahlangu — waiting 8 days", tagged as a reminder.
    /// </summary>
    public static EmailMessage BuildReminder(string toEmail, string firstName, string activityTypeName, string traineeName, int daysWaiting)
        => Compose(toEmail, firstName, [(activityTypeName, traineeName, daysWaiting)], ["reminder", "assessor-reminder"]);

    /// <summary>
    /// A request's wait as the mails say it (T358, review 3), whole days as the lists count them: "waiting less than a
    /// day", "waiting 1 day", "waiting 8 days".
    /// </summary>
    public static string WaitingPhrase(int days) => days switch
    {
        < 1 => "waiting less than a day",
        1 => "waiting 1 day",
        _ => $"waiting {days.ToString(CultureInfo.InvariantCulture)} days"
    };

    private static EmailMessage Compose(
        string toEmail,
        string firstName,
        IReadOnlyList<(string ActivityTypeName, string TraineeName, int DaysWaiting)> pendingActivities,
        IReadOnlyList<string> tags)
    {
        var listHtml = string.Join("", pendingActivities.Select(a =>
            $"<li><strong>{System.Net.WebUtility.HtmlEncode(a.ActivityTypeName)}</strong> from {System.Net.WebUtility.HtmlEncode(a.TraineeName)} — {WaitingPhrase(a.DaysWaiting)}</li>"));

        var html = EmailTemplateBase.WrapHtml(Subject, $"""
            <p>Hi {System.Net.WebUtility.HtmlEncode(firstName)},</p>
            <p>The following activities are waiting for your assessment:</p>
            <ul>{listHtml}</ul>
            <p>Please log in to Wombat to complete them.</p>
            """);

        var listText = string.Join("\n", pendingActivities.Select(a =>
            $"  - {a.ActivityTypeName} from {a.TraineeName} — {WaitingPhrase(a.DaysWaiting)}"));

        var text = $"""
            Hi {firstName},

            The following activities are waiting for your assessment:

            {listText}

            Please log in to Wombat to complete them.
            """;

        return new EmailMessage(
            To: toEmail,
            Subject: Subject,
            HtmlBody: html,
            TextBody: text,
            Tags: [.. tags]);
    }
}
