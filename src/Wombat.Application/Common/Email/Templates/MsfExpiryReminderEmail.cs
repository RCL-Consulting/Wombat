using Wombat.Application.Common.Email;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Common.Email.Templates;

public static class MsfExpiryReminderEmail
{
    /// <param name="kind">What the questionnaire collects (T164): a learner was asked about the trainee's teaching.</param>
    public static EmailMessage Build(string toEmail, string responseUrl, DateOnly expiresOn, MsfTemplateKind kind = MsfTemplateKind.Msf)
    {
        // A learner was never asked for multi-source feedback, and the subject is all a mailbox shows (T164 review).
        var subject = kind == MsfTemplateKind.LearnerFeedback
            ? "Your learner feedback link expires soon"
            : "Your MSF feedback link expires soon";
        var invited = kind == MsfTemplateKind.LearnerFeedback
            ? "You were invited to give feedback on the teaching of a trainee who has taught you, via Wombat."
            : "You were invited to provide multi-source feedback on a colleague via Wombat.";

        // The link below is freshly issued, which retires the one sent when the campaign opened
        // (T132 — the original token cannot be recovered from its hash). Say so, or a respondent
        // who kept the first email will click a dead link and conclude the system is broken.
        var html = EmailTemplateBase.WrapHtml(subject, $"""
            <p>{invited}</p>
            <p>Your response link expires on <strong>{expiresOn:yyyy-MM-dd}</strong>.</p>
            <p><a class="btn" href="{System.Net.WebUtility.HtmlEncode(responseUrl)}">Complete feedback</a></p>
            <p>Or copy this link: <code>{System.Net.WebUtility.HtmlEncode(responseUrl)}</code></p>
            <p>This link replaces the one in your original invitation. Please use this one.</p>
            """);

        var text = $"""
            {invited}

            Your response link expires on {expiresOn:yyyy-MM-dd}.

            Complete feedback: {responseUrl}

            This link replaces the one in your original invitation. Please use this one.
            """;

        return new EmailMessage(
            To: toEmail,
            Subject: subject,
            HtmlBody: html,
            TextBody: text,
            Tags: ["nudge", "msf-expiry"]);
    }
}
