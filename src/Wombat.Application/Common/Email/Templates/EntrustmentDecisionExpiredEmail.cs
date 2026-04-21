namespace Wombat.Application.Common.Email.Templates;

public static class EntrustmentDecisionExpiredEmail
{
    public static EmailMessage Build(string toEmail, string epaCode, string epaTitle, DateOnly expiredOn)
    {
        var subject = $"Entrustment decision expired: {epaCode}";

        var html = EmailTemplateBase.WrapHtml(subject, $"""
            <p>Your entrustment decision (STAR) for <strong>{System.Net.WebUtility.HtmlEncode(epaCode)} — {System.Net.WebUtility.HtmlEncode(epaTitle)}</strong> expired on <strong>{expiredOn:yyyy-MM-dd}</strong>.</p>
            <p>This authorisation is no longer current. Contact your programme coordinator to arrange a committee review if you need it renewed.</p>
            """);

        var text = $"""
            Your entrustment decision (STAR) for {epaCode} — {epaTitle} expired on {expiredOn:yyyy-MM-dd}.

            This authorisation is no longer current. Contact your programme coordinator to arrange a committee review if you need it renewed.
            """;

        return new EmailMessage(
            To: toEmail,
            Subject: subject,
            HtmlBody: html,
            TextBody: text,
            Tags: ["entrustment", "expired"]);
    }
}
