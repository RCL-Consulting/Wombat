using System.Globalization;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Common.Email.Templates;

/// <summary>
/// The reminder a respondent who has not answered is sent two days before their last day to respond: whom the feedback
/// is about, on which questionnaire, the last day, and a new link. (T132, T206)
/// </summary>
/// <remarks>
/// <para>
/// Worded as the invitation is (<see cref="MsfInvitationEmail" />, T202), from the same content and the same shared
/// parts, so the two cannot drift: the trainee's name, the questionnaire, the window, and one deadline,
/// <see cref="Wombat.Domain.MultiSourceFeedback.MsfInvitation.LastDayToRespond" />, which is also the date the page the
/// link opens gives. Until T206 it named "a colleague" and the invitation's own expiry, a week after the campaign had
/// stopped taking responses.
/// </para>
/// <para>
/// The link is freshly issued (T132: the original token cannot be recovered from its hash). The one the invitation
/// carried keeps working until the last day to respond (T214), so a respondent who started on it can finish there. It
/// says both, and that the two take one response between them.
/// </para>
/// <para>
/// A learner (<see cref="MsfTemplateKind.LearnerFeedback" />, T164) is reminded, as they were invited, to give feedback
/// on the trainee's teaching, pooled with the other learners' rather than grouped by role.
/// </para>
/// </remarks>
public static class MsfExpiryReminderEmail
{
    public static EmailMessage Build(MsfInvitationEmailContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var trainee = content.TraineeName;
        var window = MsfInvitationEmail.Window(content);
        var lastDay = MsfInvitationEmail.LastDay(content);

        // The subject names neither kind: a learner was never asked for multi-source feedback, and the subject is all a
        // mailbox shows (T164 review). The questionnaire's name, and the body, say which it is.
        var subject = $"Reminder: feedback on {trainee} ({content.TemplateName}) is due by {lastDay}";
        var heading = content.Kind == MsfTemplateKind.LearnerFeedback ? "Learner feedback reminder" : "Multi-source feedback reminder";

        var html = EmailTemplateBase.WrapHtml(heading, $"""
            <p>You were asked to give {MsfInvitationEmail.Request(content.Kind, $"<strong>{Encode(trainee)}</strong>")}, and your response has not been received yet.</p>
            <p>Questionnaire: <strong>{Encode(content.TemplateName)}</strong><br>Feedback window: <strong>{window}</strong></p>
            <p>The last day to respond is <strong>{lastDay}</strong>.</p>
            <p><a class="btn" href="{Encode(content.ResponseUrl)}">Give feedback</a></p>
            <p>Or copy this link into your browser:<br><code>{Encode(content.ResponseUrl)}</code></p>
            <p>{Encode(LinksSentence(lastDay))}</p>
            <p>{Encode(MsfInvitationEmail.Anonymity(trainee, content.Kind))}</p>
            """);

        var text = $"""
            You were asked to give {MsfInvitationEmail.Request(content.Kind, trainee)}, and your response has not been received yet.

            Questionnaire: {content.TemplateName}
            Feedback window: {window}

            The last day to respond is {lastDay}.

            Give feedback:
            {content.ResponseUrl}

            {LinksSentence(lastDay)}

            {MsfInvitationEmail.Anonymity(trainee, content.Kind)}
            """;

        return new EmailMessage(
            To: content.RespondentEmail,
            Subject: subject,
            HtmlBody: html,
            TextBody: text,
            Tags: ["nudge", "msf-expiry", $"campaign:{content.CampaignId.ToString(CultureInfo.InvariantCulture)}"]);
    }

    /// <summary>What the reminder's link is to the invitation's, which still works until the last day (T214).</summary>
    private static string LinksSentence(string lastDay)
        => "Please use this link. If you have already started with the link in your original invitation, that one also " +
           $"works until {lastDay}. Both links are yours alone, and between them take one response.";

    private static string Encode(string value) => MsfInvitationEmail.Encode(value);
}
