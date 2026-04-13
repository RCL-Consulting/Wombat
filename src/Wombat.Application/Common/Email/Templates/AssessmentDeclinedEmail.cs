using Wombat.Application.Common.Email;

namespace Wombat.Application.Common.Email.Templates;

public static class AssessmentDeclinedEmail
{
    public static EmailMessage Build(string toEmail, string traineeName, string assessorName, string activityTypeName, string? reason = null)
    {
        const string subject = "Assessment request declined";

        var reasonHtml = string.IsNullOrWhiteSpace(reason)
            ? string.Empty
            : $"<p><strong>Reason:</strong> {System.Net.WebUtility.HtmlEncode(reason)}</p>";

        var reasonText = string.IsNullOrWhiteSpace(reason)
            ? string.Empty
            : $"\nReason: {reason}";

        var html = EmailTemplateBase.WrapHtml(subject, $"""
            <p>Hi {System.Net.WebUtility.HtmlEncode(traineeName)},</p>
            <p><strong>{System.Net.WebUtility.HtmlEncode(assessorName)}</strong> has declined your assessment request for
               <strong>{System.Net.WebUtility.HtmlEncode(activityTypeName)}</strong>.</p>
            {reasonHtml}
            <p>You may submit a new request to another assessor.</p>
            """);

        var text = $"""
            Hi {traineeName},

            {assessorName} has declined your assessment request for {activityTypeName}.{reasonText}

            You may submit a new request to another assessor.
            """;

        return new EmailMessage(
            To: toEmail,
            Subject: subject,
            HtmlBody: html,
            TextBody: text,
            Tags: ["assessment-declined"]);
    }
}
