namespace Wombat.Application.Common.Email.Templates;

public static class EntrustmentDecisionExpiringReminderEmail
{
    public static EmailMessage Build(string toEmail, string epaCode, string epaTitle, DateOnly expiresOn, int daysRemaining)
    {
        var subject = $"Entrustment decision expiring soon: {epaCode}";

        var html = EmailTemplateBase.WrapHtml(subject, $"""
            <p>Your entrustment decision (STAR) for <strong>{System.Net.WebUtility.HtmlEncode(epaCode)} — {System.Net.WebUtility.HtmlEncode(epaTitle)}</strong> expires on <strong>{expiresOn:yyyy-MM-dd}</strong> ({daysRemaining} days from today).</p>
            <p>Speak to your programme coordinator about scheduling a committee review if you need to renew this authorisation before it lapses.</p>
            """);

        var text = $"""
            Your entrustment decision (STAR) for {epaCode} — {epaTitle} expires on {expiresOn:yyyy-MM-dd} ({daysRemaining} days from today).

            Speak to your programme coordinator about scheduling a committee review if you need to renew this authorisation before it lapses.
            """;

        return new EmailMessage(
            To: toEmail,
            Subject: subject,
            HtmlBody: html,
            TextBody: text,
            Tags: ["entrustment", "expiring"]);
    }
}
