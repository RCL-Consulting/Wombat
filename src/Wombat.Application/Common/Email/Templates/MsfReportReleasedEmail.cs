using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Common.Email.Templates;

/// <summary>
/// Tells a trainee that an MSF report about her was released: which questionnaire, when and by whom, and where to read it.
/// (T369, round 1 Q8, F4)
/// </summary>
/// <remarks>
/// It carries none of the feedback: no rating, no comment, no group, no count, and not the releasing clinician's narrative
/// or level. Mail is outside Wombat's access control (<see cref="AccountNotices" />), and the report is read on MSF reports,
/// beside the anonymity rules that govern it. Sent only while she is a current trainee: once her programme has ended she can
/// no longer open MSF reports, so there is nothing to send her to (F4).
/// </remarks>
public static class MsfReportReleasedEmail
{
    /// <param name="releasedBy">The releaser's name; null when the account has no name on record, and the mail then names nobody.</param>
    /// <param name="myReportsUrl">MSF reports, <c>/msf/my-reports</c>, under the configured base URL.</param>
    public static EmailMessage Build(
        string toEmail,
        string templateName,
        MsfTemplateKind kind,
        DateOnly releasedOn,
        string? releasedBy,
        string myReportsUrl)
    {
        var what = MsfEvidenceKinds.Describe(kind).ToLowerInvariant();
        var subject = $"MSF report released: {templateName}";
        var by = string.IsNullOrWhiteSpace(releasedBy) ? string.Empty : $" by {releasedBy}";

        var html = EmailTemplateBase.WrapHtml(subject, $"""
            <p>Your {Encode(what)} report, <strong>{Encode(templateName)}</strong>, was released to you on <strong>{releasedOn:yyyy-MM-dd}</strong>{Encode(by)}.</p>
            <p>Read it on MSF reports. This email holds none of the feedback.</p>
            <p><a class="btn" href="{Encode(myReportsUrl)}">Open MSF reports</a></p>
            """);

        var text = $"""
            Your {what} report, {templateName}, was released to you on {releasedOn:yyyy-MM-dd}{by}.

            Read it on MSF reports. This email holds none of the feedback.

            {myReportsUrl}
            """;

        return new EmailMessage(
            To: toEmail,
            Subject: subject,
            HtmlBody: html,
            TextBody: text,
            Tags: ["msf", "released"]);
    }

    private static string Encode(string value) => System.Net.WebUtility.HtmlEncode(value);
}
