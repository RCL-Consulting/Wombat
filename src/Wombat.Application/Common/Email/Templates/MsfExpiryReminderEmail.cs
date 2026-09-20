using Wombat.Application.Common.Email;

namespace Wombat.Application.Common.Email.Templates;

public static class MsfExpiryReminderEmail
{
    public static EmailMessage Build(string toEmail, string responseUrl, DateOnly expiresOn)
    {
        const string subject = "Your MSF feedback link expires soon";

        // The link below is freshly issued, which retires the one sent when the campaign opened
        // (T132 — the original token cannot be recovered from its hash). Say so, or a respondent
        // who kept the first email will click a dead link and conclude the system is broken.
        var html = EmailTemplateBase.WrapHtml(subject, $"""
            <p>You were invited to provide multi-source feedback on a colleague via Wombat.</p>
            <p>Your response link expires on <strong>{expiresOn:yyyy-MM-dd}</strong>.</p>
            <p><a class="btn" href="{System.Net.WebUtility.HtmlEncode(responseUrl)}">Complete feedback</a></p>
            <p>Or copy this link: <code>{System.Net.WebUtility.HtmlEncode(responseUrl)}</code></p>
            <p>This link replaces the one in your original invitation. Please use this one.</p>
            """);

        var text = $"""
            You were invited to provide multi-source feedback on a colleague via Wombat.

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
