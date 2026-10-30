using Wombat.Application.Features.DataRights;
using Wombat.Domain.DataRights;

namespace Wombat.Application.Common.Email.Templates;

/// <summary>What a data-rights notice tells (T387, Q5): which way the request went.</summary>
public enum DataRightsNoticeOutcome
{
    /// <summary>Approved: an Access or Export is completed with it, and can be downloaded; a Rectification is being corrected.</summary>
    Approved,

    /// <summary>Rejected: the answer, with the note, is on My data rights.</summary>
    Rejected,

    /// <summary>A Rectification completed: every correction recorded.</summary>
    Completed,

    /// <summary>An Erasure approved and carried out: no link, since the account is gone (F4).</summary>
    Erased
}

/// <summary>
/// Tells a requester that their data-rights request was decided, or completed (T319; T387, Q5, M3, F4). Sent after the
/// command's save through <c>AccountNotices</c>, so a refused decision mails nobody and a failed send never fails it.
/// </summary>
/// <remarks>
/// The request's type, the day and the outcome, and a link to My data rights, where the answer is read: never the
/// decision note (mail is outside Wombat's access control) and never who decided (My data rights names nobody either).
/// An erasure's confirmation carries no link: the account is gone, and it is sent to the address read before the erasure
/// ran (<c>AccountNotices.SendToAddressAsync</c>). The words are NEW (T387): no board draws a mail.
/// </remarks>
public static class DataRightsDecisionEmail
{
    public const string DecidedSubject = "Your data rights request has been decided";
    public const string CompletedSubject = "Your data rights request is complete";
    public const string ErasedSubject = "Your personal data has been erased from Wombat";

    /// <param name="type">The request's type, said as the pages say it (DataRightsLabels.Type).</param>
    /// <param name="onDay">The South African day of the decision or the completion.</param>
    /// <param name="myDataRightsUrl">My data rights under the configured base URL; ignored for <see cref="DataRightsNoticeOutcome.Erased" />.</param>
    public static EmailMessage Build(
        string toEmail,
        DataRightsRequestType type,
        DataRightsNoticeOutcome outcome,
        DateOnly onDay,
        string myDataRightsUrl)
    {
        var subject = Subject(outcome);
        var lines = Lines(type, outcome, onDay);
        var withLink = outcome != DataRightsNoticeOutcome.Erased;

        var paragraphs = string.Join(Environment.NewLine, lines.Select(line => $"<p>{Encode(line)}</p>"));
        var html = EmailTemplateBase.WrapHtml(subject, withLink
            ? $"""
              {paragraphs}
              <p><a class="btn" href="{Encode(myDataRightsUrl)}">Open My data rights</a></p>
              """
            : paragraphs);

        var text = string.Join(Environment.NewLine + Environment.NewLine, lines) +
                   (withLink ? Environment.NewLine + Environment.NewLine + myDataRightsUrl : string.Empty);

        return new EmailMessage(
            To: toEmail,
            Subject: subject,
            HtmlBody: html,
            TextBody: text,
            Tags: ["data-rights", outcome.ToString().ToLowerInvariant()]);
    }

    public static string Subject(DataRightsNoticeOutcome outcome) => outcome switch
    {
        DataRightsNoticeOutcome.Completed => CompletedSubject,
        DataRightsNoticeOutcome.Erased => ErasedSubject,
        _ => DecidedSubject
    };

    /// <summary>The notice's sentences, in order (NEW, T387).</summary>
    public static IReadOnlyList<string> Lines(DataRightsRequestType type, DataRightsNoticeOutcome outcome, DateOnly onDay)
    {
        var day = onDay.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        var label = DataRightsLabels.Type(type);
        return outcome switch
        {
            DataRightsNoticeOutcome.Approved when type == DataRightsRequestType.Rectification =>
            [
                $"Your Rectification request was approved on {day}.",
                "It is being corrected. You will be emailed again when it is complete; read the answer on My data rights."
            ],
            DataRightsNoticeOutcome.Approved =>
            [
                $"Your {label} request was approved on {day}.",
                "Read the answer, and download your data, on My data rights."
            ],
            DataRightsNoticeOutcome.Rejected =>
            [
                $"Your {label} request was rejected on {day}.",
                "Read the answer on My data rights."
            ],
            DataRightsNoticeOutcome.Completed =>
            [
                $"Your {label} request was completed on {day}.",
                "Read what was corrected on My data rights."
            ],
            _ =>
            [
                $"Your Erasure request was approved on {day}, and your personal data has been erased from Wombat.",
                "Your account is closed, and you can no longer sign in. Your training records are kept under a name that identifies nobody."
            ]
        };
    }

    private static string Encode(string value) => System.Net.WebUtility.HtmlEncode(value);
}
