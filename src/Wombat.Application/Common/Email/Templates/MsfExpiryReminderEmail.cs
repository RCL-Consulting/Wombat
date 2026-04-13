using Wombat.Application.Common.Email;

namespace Wombat.Application.Common.Email.Templates;

public static class MsfExpiryReminderEmail
{
    public static EmailMessage Build(string toEmail, string responseUrl, DateOnly expiresOn)
    {
        const string subject = "Your MSF feedback link expires soon";

        var html = EmailTemplateBase.WrapHtml(subject, $"""
            <p>You were invited to provide multi-source feedback on a colleague via Wombat.</p>
            <p>Your response link expires on <strong>{expiresOn:yyyy-MM-dd}</strong>.</p>
            <p><a class="btn" href="{System.Net.WebUtility.HtmlEncode(responseUrl)}">Complete feedback</a></p>
            <p>Or copy this link: <code>{System.Net.WebUtility.HtmlEncode(responseUrl)}</code></p>
            """);

        var text = $"""
            You were invited to provide multi-source feedback on a colleague via Wombat.

            Your response link expires on {expiresOn:yyyy-MM-dd}.

            Complete feedback: {responseUrl}
            """;

        return new EmailMessage(
            To: toEmail,
            Subject: subject,
            HtmlBody: html,
            TextBody: text,
            Tags: ["nudge", "msf-expiry"]);
    }
}
