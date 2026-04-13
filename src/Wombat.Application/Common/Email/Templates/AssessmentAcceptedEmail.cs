using Wombat.Application.Common.Email;

namespace Wombat.Application.Common.Email.Templates;

public static class AssessmentAcceptedEmail
{
    public static EmailMessage Build(string toEmail, string traineeName, string assessorName, string activityTypeName, string activityUrl)
    {
        const string subject = "Assessment request accepted";

        var html = EmailTemplateBase.WrapHtml(subject, $"""
            <p>Hi {System.Net.WebUtility.HtmlEncode(traineeName)},</p>
            <p><strong>{System.Net.WebUtility.HtmlEncode(assessorName)}</strong> has accepted your assessment request for
               <strong>{System.Net.WebUtility.HtmlEncode(activityTypeName)}</strong>.</p>
            <p><a class="btn" href="{System.Net.WebUtility.HtmlEncode(activityUrl)}">View activity</a></p>
            """);

        var text = $"""
            Hi {traineeName},

            {assessorName} has accepted your assessment request for {activityTypeName}.

            View activity:
            {activityUrl}
            """;

        return new EmailMessage(
            To: toEmail,
            Subject: subject,
            HtmlBody: html,
            TextBody: text,
            Tags: ["assessment-accepted"]);
    }
}
