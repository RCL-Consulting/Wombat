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
/// It names one deadline: <see cref="LastDayToRespond" />. The invitation's own expiry is written a week after the window
/// closes (<c>AddMsfInvitationCommandHandler</c>), but the auto-close job closes the campaign the day after the window
/// does, and a closed campaign takes no response. Printing the expiry told a respondent the link worked for a week it
/// does not (T202 review).
/// </para>
/// </remarks>
public static class MsfInvitationEmail
{
    public static EmailMessage Build(MsfInvitationEmailContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var trainee = content.TraineeName;
        var window = $"{Date(content.OpensOn)} to {Date(content.ClosesOn)}";
        var lastDay = Date(LastDayToRespond(content));
        var subject = $"Feedback request: {trainee} ({content.TemplateName}, {window})";

        // T164: a learner is asked about the trainee's teaching, not as a colleague, and is grouped with the other
        // learners rather than by role.
        var learner = content.Kind == MsfTemplateKind.LearnerFeedback;
        var heading = learner ? "Learner feedback request" : "Multi-source feedback request";
        var askedHtml = learner
            ? $"You have been asked to give feedback on the teaching of <strong>{Encode(trainee)}</strong>, a trainee who has taught you."
            : $"You have been asked to give multi-source feedback on <strong>{Encode(trainee)}</strong>, a trainee you have worked with.";
        var askedText = learner
            ? $"You have been asked to give feedback on the teaching of {trainee}, a trainee who has taught you."
            : $"You have been asked to give multi-source feedback on {trainee}, a trainee you have worked with.";
        var grouping = learner ? "together with the other learners' feedback" : "grouped by respondent role";

        var html = EmailTemplateBase.WrapHtml(heading, $"""
            <p>{askedHtml}</p>
            <p>Questionnaire: <strong>{Encode(content.TemplateName)}</strong><br>Feedback window: <strong>{window}</strong></p>
            <p><a class="btn" href="{Encode(content.ResponseUrl)}">Give feedback</a></p>
            <p>Or copy this link into your browser:<br><code>{Encode(content.ResponseUrl)}</code></p>
            <p>The link is yours alone and can be used once. The last day to respond is <strong>{lastDay}</strong>.</p>
            <p>Your name and email address are never shown to {Encode(trainee)}. They see the feedback only after the campaign has closed and a coordinator has reviewed and released it, {grouping}.</p>
            """);

        var text = $"""
            {askedText}

            Questionnaire: {content.TemplateName}
            Feedback window: {window}

            Give feedback:
            {content.ResponseUrl}

            The link is yours alone and can be used once. The last day to respond is {lastDay}.

            Your name and email address are never shown to {trainee}. They see the feedback only after the campaign has closed and a coordinator has reviewed and released it, {grouping}.
            """;

        return new EmailMessage(
            To: content.RespondentEmail,
            Subject: subject,
            HtmlBody: html,
            TextBody: text,
            Tags: ["msf-invite", $"campaign:{content.CampaignId.ToString(CultureInfo.InvariantCulture)}"]);
    }

    /// <summary>
    /// The last day the link takes a response: the earlier of the day the feedback window closes and the invitation's
    /// own expiry. A link is refused once its expiry has passed, and once its campaign has closed, which the auto-close
    /// job does the day after <see cref="MsfInvitationEmailContent.ClosesOn" />.
    /// </summary>
    private static DateOnly LastDayToRespond(MsfInvitationEmailContent content)
    {
        ArgumentNullException.ThrowIfNull(content);
        return content.ExpiresOn < content.ClosesOn ? content.ExpiresOn : content.ClosesOn;
    }

    private static string Date(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string Encode(string value) => WebUtility.HtmlEncode(value);
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
