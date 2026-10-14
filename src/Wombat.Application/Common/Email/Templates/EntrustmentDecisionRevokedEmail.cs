namespace Wombat.Application.Common.Email.Templates;

/// <summary>
/// Tells a trainee that a STAR of theirs was revoked: what, when and by whom, and where to read it. (T367, T319)
/// </summary>
/// <remarks>
/// It carries no reason. Mail is outside Wombat's access control, and the reason is the most damaging thing written about
/// a trainee (T101 redacts it even from the audit log); she reads it on My authorisations and on the certificate.
/// </remarks>
public static class EntrustmentDecisionRevokedEmail
{
    /// <param name="revokedBy">The reviser's name; null when the account has no name on record, and the mail then names nobody.</param>
    public static EmailMessage Build(
        string toEmail,
        string epaCode,
        string epaTitle,
        string levelLabel,
        DateOnly revokedOn,
        string? revokedBy,
        string authorisationsUrl)
    {
        var subject = $"Entrustment decision revoked: {epaCode}";
        var by = string.IsNullOrWhiteSpace(revokedBy) ? string.Empty : $" by {revokedBy}";

        var html = EmailTemplateBase.WrapHtml(subject, $"""
            <p>Your entrustment decision (STAR) for <strong>{Encode(epaCode)} — {Encode(epaTitle)}</strong> at level {Encode(levelLabel)} was revoked on <strong>{revokedOn:yyyy-MM-dd}</strong>{Encode(by)}.</p>
            <p>It is no longer in force. My authorisations lists it under No longer in force, with the reason, and its certificate states it.</p>
            <p><a class="btn" href="{Encode(authorisationsUrl)}">Open My authorisations</a></p>
            """);

        var text = $"""
            Your entrustment decision (STAR) for {epaCode} — {epaTitle} at level {levelLabel} was revoked on {revokedOn:yyyy-MM-dd}{by}.

            It is no longer in force. My authorisations lists it under No longer in force, with the reason, and its certificate states it.

            {authorisationsUrl}
            """;

        return new EmailMessage(
            To: toEmail,
            Subject: subject,
            HtmlBody: html,
            TextBody: text,
            Tags: ["entrustment", "revoked"]);
    }

    private static string Encode(string value) => System.Net.WebUtility.HtmlEncode(value);
}
