using Wombat.Application.Common.Email;

namespace Wombat.Application.Common.Email.Templates;

public static class PasswordResetEmail
{
    public static EmailMessage Build(string toEmail, string resetUrl, int expiryMinutes = 60)
    {
        const string subject = "Reset your Wombat password";

        var html = EmailTemplateBase.WrapHtml(subject, $"""
            <p>We received a request to reset the password for your Wombat account.</p>
            <p><a class="btn" href="{System.Net.WebUtility.HtmlEncode(resetUrl)}">Reset password</a></p>
            <p>Or copy this link into your browser:<br><code>{System.Net.WebUtility.HtmlEncode(resetUrl)}</code></p>
            <p>This link expires in <strong>{expiryMinutes} minutes</strong>. If you did not request a password reset, you can ignore this email.</p>
            """);

        var text = $"""
            We received a request to reset the password for your Wombat account.

            Reset your password:
            {resetUrl}

            This link expires in {expiryMinutes} minutes. If you did not request a password reset, you can ignore this email.
            """;

        return new EmailMessage(
            To: toEmail,
            Subject: subject,
            HtmlBody: html,
            TextBody: text,
            Tags: ["password-reset"]);
    }
}
