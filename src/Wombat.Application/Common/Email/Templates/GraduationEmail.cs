namespace Wombat.Application.Common.Email.Templates;

/// <summary>
/// Tells a registrar that their programme was marked complete (T080), and where their record is now (T379, flow 13, Q12,
/// T311 item 4). Until T379 it said "Your committee has ratified your final entrustment decisions", which Wombat does not
/// require before a completion (Q13: the last ratified decision gates nothing), and named no page.
/// </summary>
public static class GraduationEmail
{
    /// <summary>
    /// What the graduate keeps (T379, NEW: no board draws an email): the record, read-only, and what they can still do with it.
    /// Words, not a promise of a certificate: My authorisations has one only for a STAR that was issued.
    /// </summary>
    public const string RecordKept =
        "Your record of the programme stays in Wombat, read-only. Sign in to read it on My progress, to export your " +
        "portfolio, and to download the certificate of any STAR issued to you.";

    /// <summary>The link's words, the page's title (Q23).</summary>
    public const string OpenMyProgress = "Open My progress";

    /// <param name="progressUrl">My progress under the configured base URL.</param>
    public static EmailMessage Build(string toEmail, string traineeName, string programmeName, DateOnly completedOn, string progressUrl)
    {
        var subject = $"Congratulations on completing {programmeName}";
        var encodedName = System.Net.WebUtility.HtmlEncode(traineeName);
        var encodedProgramme = System.Net.WebUtility.HtmlEncode(programmeName);
        var encodedUrl = System.Net.WebUtility.HtmlEncode(progressUrl);

        var html = EmailTemplateBase.WrapHtml(subject, $"""
            <p>Dear {encodedName},</p>
            <p>Congratulations on completing your training programme,
               <strong>{encodedProgramme}</strong>, as of {completedOn:yyyy-MM-dd}.</p>
            <p>{System.Net.WebUtility.HtmlEncode(RecordKept)}</p>
            <p><a class="btn" href="{encodedUrl}">{OpenMyProgress}</a></p>
            <p>We wish you every success in the next stage of your career.</p>
            """);

        var text = $"""
            Dear {traineeName},

            Congratulations on completing your training programme, {programmeName}, as of {completedOn:yyyy-MM-dd}.

            {RecordKept}

            {OpenMyProgress}: {progressUrl}

            We wish you every success in the next stage of your career.
            """;

        return new EmailMessage(
            To: toEmail,
            Subject: subject,
            HtmlBody: html,
            TextBody: text,
            Tags: ["graduation"]);
    }
}
