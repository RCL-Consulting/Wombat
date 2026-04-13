using Wombat.Application.Common.Email;

namespace Wombat.Application.Common.Email.Templates;

public static class DraftNudgeEmail
{
    public static EmailMessage Build(string toEmail, string firstName, IReadOnlyList<(string ActivityTypeName, int DaysOld)> staleActivities)
    {
        const string subject = "You have draft activities waiting";

        var listHtml = string.Join("", staleActivities.Select(a =>
            $"<li><strong>{System.Net.WebUtility.HtmlEncode(a.ActivityTypeName)}</strong> — draft for {a.DaysOld} days</li>"));

        var html = EmailTemplateBase.WrapHtml(subject, $"""
            <p>Hi {System.Net.WebUtility.HtmlEncode(firstName)},</p>
            <p>The following activities have been in draft for a while and may need your attention:</p>
            <ul>{listHtml}</ul>
            <p>Please log in to Wombat to submit or discard them.</p>
            """);

        var listText = string.Join("\n", staleActivities.Select(a =>
            $"  - {a.ActivityTypeName} — draft for {a.DaysOld} days"));

        var text = $"""
            Hi {firstName},

            The following activities have been in draft for a while:

            {listText}

            Please log in to Wombat to submit or discard them.
            """;

        return new EmailMessage(
            To: toEmail,
            Subject: subject,
            HtmlBody: html,
            TextBody: text,
            Tags: ["nudge", "draft-reminder"]);
    }
}
