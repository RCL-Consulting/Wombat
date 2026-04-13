using Wombat.Application.Common.Email;

namespace Wombat.Application.Common.Email.Templates;

public static class AssessorPendingNudgeEmail
{
    public static EmailMessage Build(string toEmail, string firstName, IReadOnlyList<(string ActivityTypeName, string TraineeName, int DaysWaiting)> pendingActivities)
    {
        const string subject = "Activities awaiting your assessment";

        var listHtml = string.Join("", pendingActivities.Select(a =>
            $"<li><strong>{System.Net.WebUtility.HtmlEncode(a.ActivityTypeName)}</strong> from {System.Net.WebUtility.HtmlEncode(a.TraineeName)} — waiting {a.DaysWaiting} days</li>"));

        var html = EmailTemplateBase.WrapHtml(subject, $"""
            <p>Hi {System.Net.WebUtility.HtmlEncode(firstName)},</p>
            <p>The following activities are waiting for your assessment:</p>
            <ul>{listHtml}</ul>
            <p>Please log in to Wombat to complete them.</p>
            """);

        var listText = string.Join("\n", pendingActivities.Select(a =>
            $"  - {a.ActivityTypeName} from {a.TraineeName} — waiting {a.DaysWaiting} days"));

        var text = $"""
            Hi {firstName},

            The following activities are waiting for your assessment:

            {listText}

            Please log in to Wombat to complete them.
            """;

        return new EmailMessage(
            To: toEmail,
            Subject: subject,
            HtmlBody: html,
            TextBody: text,
            Tags: ["nudge", "assessor-pending"]);
    }
}
