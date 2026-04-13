using Wombat.Application.Common.Email;

namespace Wombat.Application.Common.Email.Templates;

public static class AssessmentRequestedEmail
{
    public static EmailMessage Build(string toEmail, string assessorName, string traineeName, string activityTypeName, string reviewUrl)
    {
        const string subject = "New assessment request";

        var html = EmailTemplateBase.WrapHtml(subject, $"""
            <p>Hi {System.Net.WebUtility.HtmlEncode(assessorName)},</p>
            <p><strong>{System.Net.WebUtility.HtmlEncode(traineeName)}</strong> has requested your assessment for
               <strong>{System.Net.WebUtility.HtmlEncode(activityTypeName)}</strong>.</p>
            <p><a class="btn" href="{System.Net.WebUtility.HtmlEncode(reviewUrl)}">Review request</a></p>
            """);

        var text = $"""
            Hi {assessorName},

            {traineeName} has requested your assessment for {activityTypeName}.

            Review the request:
            {reviewUrl}
            """;

        return new EmailMessage(
            To: toEmail,
            Subject: subject,
            HtmlBody: html,
            TextBody: text,
            Tags: ["assessment-requested"]);
    }
}
