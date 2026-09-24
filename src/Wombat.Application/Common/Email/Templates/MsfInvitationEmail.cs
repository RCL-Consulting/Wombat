using System.Globalization;
using System.Net;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Common.Email.Templates;

/// <summary>
/// The invitation a respondent is sent when an MSF campaign opens: whom the feedback is about, on which questionnaire,
/// over which window, and the respondent's own link. (T202)
/// </summary>
/// <remarks>
/// <para>
/// Until T202 the invitation named only the template ("MSF request: Annual MSF"), so two campaigns on one template, for
/// two trainees or for one trainee in two semesters, sent invitations that could not be told apart, and none said whom
/// the respondent was being asked to rate.
/// </para>
/// <para>
/// Naming the trainee is what MSF requires, not a leak of it. Its anonymity runs the other way: the RESPONDENT is hidden
/// from the trainee (T021: "ten respondents rate one trainee ... individual respondents must not be de-anonymisable").
/// A respondent cannot rate someone they have not been told about. What the email says about anonymity is only what
/// the product guarantees whatever a campaign's thresholds are set to: the trainee never sees a name or an address,
/// sees nothing before a coordinator releases it, and sees it grouped by respondent role.
/// </para>
/// <para>
/// It names one deadline: <see cref="MsfInvitation.LastDayToRespond" />, the date the page the link opens gives too
/// (T205). The invitation's own expiry is written a week after the window closes (<c>AddMsfInvitationCommandHandler</c>),
/// but the auto-close job closes the campaign the day after the window does, and a closed campaign takes no response.
/// Printing the expiry told a respondent the link worked for a week it does not (T202 review).
/// </para>
/// </remarks>
public static class MsfInvitationEmail
{
    public static EmailMessage Build(MsfInvitationEmailContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var trainee = content.TraineeName;
        var window = Window(content);
        var lastDay = LastDay(content);
        var subject = $"Feedback request: {trainee} ({content.TemplateName}, {window})";

        // T164: a learner is asked about the trainee's teaching, not as a colleague, and is grouped with the other
        // learners rather than by role.
        var heading = content.Kind == MsfTemplateKind.LearnerFeedback ? "Learner feedback request" : "Multi-source feedback request";
        var askedHtml = $"You have been asked to give {Request(content.Kind, $"<strong>{Encode(trainee)}</strong>")}.";
        var askedText = $"You have been asked to give {Request(content.Kind, trainee)}.";

        var html = EmailTemplateBase.WrapHtml(heading, $"""
            <p>{askedHtml}</p>
            <p>Questionnaire: <strong>{Encode(content.TemplateName)}</strong><br>Feedback window: <strong>{window}</strong></p>
            <p><a class="btn" href="{Encode(content.ResponseUrl)}">Give feedback</a></p>
            <p>Or copy this link into your browser:<br><code>{Encode(content.ResponseUrl)}</code></p>
            <p>The link is yours alone and can be used once. The last day to respond is <strong>{lastDay}</strong>.</p>
            <p>{Encode(Anonymity(trainee, content.Kind))}</p>
            """);

        var text = $"""
            {askedText}

            Questionnaire: {content.TemplateName}
            Feedback window: {window}

            Give feedback:
            {content.ResponseUrl}

            The link is yours alone and can be used once. The last day to respond is {lastDay}.

            {Anonymity(trainee, content.Kind)}
            """;

        return new EmailMessage(
            To: content.RespondentEmail,
            Subject: subject,
            HtmlBody: html,
            TextBody: text,
            Tags: ["msf-invite", $"campaign:{content.CampaignId.ToString(CultureInfo.InvariantCulture)}"]);
    }

    /// <summary>The feedback window, as the invitation and the reminder both give it.</summary>
    internal static string Window(MsfInvitationEmailContent content)
        => $"{Date(content.OpensOn)} to {Date(content.ClosesOn)}";

    /// <summary>
    /// The one deadline, <see cref="MsfInvitation.LastDayToRespond" />, as the invitation, the reminder and the page all
    /// give it.
    /// </summary>
    internal static string LastDay(MsfInvitationEmailContent content)
        => Date(MsfInvitation.LastDayToRespond(content.ClosesOn, content.ExpiresOn));

    /// <summary>
    /// What the respondent is asked to give, as the invitation and the reminder both say it, about
    /// <paramref name="trainee" /> as the caller writes the name (encoded and emphasised in HTML). A learner is asked
    /// about the trainee's teaching, not for multi-source feedback on a colleague (T164).
    /// </summary>
    internal static string Request(MsfTemplateKind kind, string trainee)
        => kind == MsfTemplateKind.LearnerFeedback
            ? $"feedback on the teaching of {trainee}, a trainee who has taught you"
            : $"multi-source feedback on {trainee}, a trainee you have worked with";

    /// <summary>
    /// What the invitation and the reminder both promise about anonymity, as plain text. A learner's answers are shown
    /// together with the other learners', not grouped by role (T164).
    /// </summary>
    internal static string Anonymity(string trainee, MsfTemplateKind kind)
        => $"Your name and email address are never shown to {trainee}. They see the feedback only after the campaign " +
           "has closed and a coordinator has reviewed and released it, " +
           (kind == MsfTemplateKind.LearnerFeedback ? "together with the other learners' feedback." : "grouped by respondent role.");

    internal static string Date(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    internal static string Encode(string value) => WebUtility.HtmlEncode(value);
}

/// <summary>What one respondent's invitation says. (T202)</summary>
/// <param name="Kind">What the questionnaire collects (T164): a learner is asked about the trainee's teaching.</param>
public sealed record MsfInvitationEmailContent(
    int CampaignId,
    string RespondentEmail,
    string TraineeName,
    string TemplateName,
    DateOnly OpensOn,
    DateOnly ClosesOn,
    DateOnly ExpiresOn,
    string ResponseUrl,
    MsfTemplateKind Kind = MsfTemplateKind.Msf);
