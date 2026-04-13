using Wombat.Application.Common.Email;

namespace Wombat.Application.Common.Email.Templates;

public static class InvitationEmail
{
    public static EmailMessage Build(string toEmail, string targetRole, string registrationUrl, DateOnly expiresOn)
    {
        const string subject = "Your Wombat invitation";

        var html = EmailTemplateBase.WrapHtml(subject, $"""
            <p>You have been invited to register for <strong>Wombat</strong> as <strong>{System.Net.WebUtility.HtmlEncode(targetRole)}</strong>.</p>
            <p><a class="btn" href="{System.Net.WebUtility.HtmlEncode(registrationUrl)}">Complete registration</a></p>
            <p>Or copy this link into your browser:<br><code>{System.Net.WebUtility.HtmlEncode(registrationUrl)}</code></p>
            <p>This link expires on <strong>{expiresOn:yyyy-MM-dd}</strong>.</p>
            """);

        var text = $"""
            You have been invited to register for Wombat as {targetRole}.

            Complete registration:
            {registrationUrl}

            This link expires on {expiresOn:yyyy-MM-dd}.
            """;

        return new EmailMessage(
            To: toEmail,
            Subject: subject,
            HtmlBody: html,
            TextBody: text,
            Tags: ["invitation", $"role:{targetRole}"]);
    }
}
