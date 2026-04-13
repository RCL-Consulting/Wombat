using Wombat.Application.Common.Email;

namespace Wombat.Application.Common.Email.Templates;

public static class StarDecisionEmail
{
    public static EmailMessage Build(string toEmail, string traineeName, bool approved, string? comments = null)
    {
        var decision = approved ? "approved" : "declined";
        var subject = $"STAR reflection {decision}";

        var commentsHtml = string.IsNullOrWhiteSpace(comments)
            ? string.Empty
            : $"<p><strong>Comments:</strong> {System.Net.WebUtility.HtmlEncode(comments)}</p>";

        var commentsText = string.IsNullOrWhiteSpace(comments)
            ? string.Empty
            : $"\nComments: {comments}";

        var html = EmailTemplateBase.WrapHtml(subject, $"""
            <p>Hi {System.Net.WebUtility.HtmlEncode(traineeName)},</p>
            <p>Your STAR reflection has been <strong>{decision}</strong> by the committee.</p>
            {commentsHtml}
            """);

        var text = $"""
            Hi {traineeName},

            Your STAR reflection has been {decision} by the committee.{commentsText}
            """;

        return new EmailMessage(
            To: toEmail,
            Subject: subject,
            HtmlBody: html,
            TextBody: text,
            Tags: ["star-decision", $"decision:{decision}"]);
    }
}
