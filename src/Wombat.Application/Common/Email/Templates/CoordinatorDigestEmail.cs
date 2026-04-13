using Wombat.Application.Common.Email;

namespace Wombat.Application.Common.Email.Templates;

public static class CoordinatorDigestEmail
{
    public static EmailMessage Build(
        string toEmail,
        string firstName,
        IReadOnlyList<string> inactiveTrainees,
        IReadOnlyList<string> msfCampaignsNeedingReview,
        IReadOnlyList<string> committeeReviewsThisWeek)
    {
        const string subject = "Your weekly Wombat digest";

        var sections = new List<string>();

        if (inactiveTrainees.Count > 0)
        {
            var list = string.Join("", inactiveTrainees.Select(t =>
                $"<li>{System.Net.WebUtility.HtmlEncode(t)}</li>"));
            sections.Add($"<h2>Trainees at risk (no activities in 30 days)</h2><ul>{list}</ul>");
        }

        if (msfCampaignsNeedingReview.Count > 0)
        {
            var list = string.Join("", msfCampaignsNeedingReview.Select(c =>
                $"<li>{System.Net.WebUtility.HtmlEncode(c)}</li>"));
            sections.Add($"<h2>MSF campaigns needing review</h2><ul>{list}</ul>");
        }

        if (committeeReviewsThisWeek.Count > 0)
        {
            var list = string.Join("", committeeReviewsThisWeek.Select(r =>
                $"<li>{System.Net.WebUtility.HtmlEncode(r)}</li>"));
            sections.Add($"<h2>Committee reviews scheduled this week</h2><ul>{list}</ul>");
        }

        if (sections.Count == 0)
        {
            sections.Add("<p>No items requiring attention this week.</p>");
        }

        var bodyHtml = $"<p>Hi {System.Net.WebUtility.HtmlEncode(firstName)},</p>" +
                       "<p>Here is your weekly summary:</p>" +
                       string.Join("", sections);

        var html = EmailTemplateBase.WrapHtml(subject, bodyHtml);

        var textSections = new List<string>();
        if (inactiveTrainees.Count > 0)
            textSections.Add("Trainees at risk:\n" + string.Join("\n", inactiveTrainees.Select(t => $"  - {t}")));
        if (msfCampaignsNeedingReview.Count > 0)
            textSections.Add("MSF campaigns needing review:\n" + string.Join("\n", msfCampaignsNeedingReview.Select(c => $"  - {c}")));
        if (committeeReviewsThisWeek.Count > 0)
            textSections.Add("Committee reviews this week:\n" + string.Join("\n", committeeReviewsThisWeek.Select(r => $"  - {r}")));

        var text = $"""
            Hi {firstName},

            Here is your weekly summary:

            {(textSections.Count > 0 ? string.Join("\n\n", textSections) : "No items requiring attention this week.")}
            """;

        return new EmailMessage(
            To: toEmail,
            Subject: subject,
            HtmlBody: html,
            TextBody: text,
            Tags: ["digest", "coordinator-weekly"]);
    }
}
