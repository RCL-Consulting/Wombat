using System.Globalization;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Curricula;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Components.Pages.MultiSourceFeedback;

/// <summary>
/// The words the campaign list and the campaign page share: how a campaign is named, what withdrawing it does, and its
/// state. One place, so the two pages say the same thing (T206, T217). Built as whole strings, because Razor drops a
/// space standing alone before an expression.
/// </summary>
public static class MsfCampaignText
{
    /// <summary>A campaign as a coordinator tells it from the others: "Sipho Dlamini (Annual MSF, closing 2029-03-21)".</summary>
    public static string Describe(string subjectName, string templateName, DateOnly closesOn)
        => $"{subjectName} ({templateName}, closing {closesOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)})";

    public const string WithdrawTitle = "Withdraw this campaign?";

    public const string WithdrawConfirmLabel = "Withdraw campaign";

    /// <summary>
    /// What withdrawing does, and that it cannot be undone (MsfCampaign.Withdraw, T202). A campaign that has closed has
    /// no link left to stop and no address left to remove: closing did both. Withdrawing it is the decision never to
    /// release its report, and it is told that instead (T199).
    /// </summary>
    public static string WithdrawBody(string describedCampaign, MsfCampaignState state)
        => HasClosed(state)
            ? $"Withdraw the campaign for {describedCampaign}? It has closed, and its report has not been released. A " +
              "withdrawn campaign is never released to the trainee and is never evidence on their record, and " +
              "withdrawing cannot be undone."
            : $"Withdraw the campaign for {describedCampaign}? It will take no more responses: any link already sent " +
              "stops working, and every respondent's email address is removed. A withdrawn campaign is never released " +
              "to the trainee, and withdrawing cannot be undone.";

    /// <summary>What a withdraw did, for a campaign that was in this state when it was asked about.</summary>
    public static string Withdrawn(string describedCampaign, MsfCampaignState state)
        => $"{WithdrawnLead(describedCampaign)} {WithdrawnDetail(state)}";

    /// <summary>A withdraw's result, bold (L4; build review D11): "The campaign for … has been withdrawn."</summary>
    public static string WithdrawnLead(string describedCampaign) => $"The campaign for {describedCampaign} has been withdrawn.";

    /// <summary>A withdraw's result after its bold lead (L4): what the withdraw did, plain.</summary>
    public static string WithdrawnDetail(MsfCampaignState state)
        => HasClosed(state)
            ? "Its report will never be released to the trainee."
            : "Its respondents' links no longer work, and their email addresses have been removed.";

    /// <summary>
    /// Closed to responses and not yet released: closing has already stopped every link and removed every address. The
    /// rule the withdraw command asks too, so a page worded for an open campaign cannot withdraw a closed one (T199 review).
    /// </summary>
    private static bool HasClosed(MsfCampaignState state) => MsfCampaignRules.WithdrawingForgoesRelease(state);

    /// <summary>
    /// The link to the campaign's own page, named by what it lets the coordinator do (T225): a draft or an open campaign
    /// is managed there (invitees, Open, Withdraw); any other is only shown there. Until T225 every row of the campaign
    /// list read "Edit", whatever the state.
    /// </summary>
    public static string CampaignLinkLabel(MsfCampaignState state)
        => state is MsfCampaignState.Draft or MsfCampaignState.Open ? "Manage" : "View campaign";

    /// <summary>
    /// The link to the campaign's report, named by what the coordinator goes there to do (T217, T225): one closed and
    /// under review is reviewed and released there; an open or a released one's report is there to read, and an open
    /// one is closed there. Null for a draft, which has no responses to report, and for a withdrawn campaign, which will
    /// never be released: neither page offers a report link on those.
    /// </summary>
    public static string? ReportLinkLabel(MsfCampaignState state)
        => state switch
        {
            MsfCampaignState.Closed or MsfCampaignState.UnderReview => "Review and release",
            MsfCampaignState.Open or MsfCampaignState.Released => "View report",
            _ => null
        };

    /// <summary>
    /// A row link's accessible name: its visible label first, then the campaign it acts on, since a column of identical
    /// "Manage" links is otherwise indistinguishable to a screen reader (DESIGN.md § Button system, T206, T225).
    /// </summary>
    /// <remarks>
    /// A label that opens a dialog ends in an ellipsis on screen ("Withdraw…", T369); the name says the action without it.
    /// </remarks>
    public static string RowLinkName(string label, string describedCampaign)
        => $"{label.TrimEnd('…')}: the campaign for {describedCampaign}";

    /// <summary>A row's action named by what it acts on: "Remove grace@example.org (Nurse)" for "Remove…" (T369, K2).</summary>
    public static string RowActionName(string label, string described) => $"{label.TrimEnd('…')} {described}";

    public const string RemoveInviteeTitle = "Remove this invitee?";

    public const string RemoveInviteeConfirmLabel = "Remove invitee";

    /// <summary>
    /// A draft invitee as the Remove button, the dialog and the result name them: "peer-1@example.test (Peer doctor)".
    /// (T247)
    /// </summary>
    public static string DescribeInvitee(string email, string group) => $"{email} ({group})";

    /// <summary>
    /// What removing a draft's invitee does, and how to undo it: add them again (T247). "No working link", not "no
    /// link": an open that failed, or was refused at its save, may have mailed them one that does not work (T184, T247
    /// review).
    /// </summary>
    public static string RemoveInviteeBody(string describedInvitee)
        => $"Remove {describedInvitee} from this campaign? They hold no working link, and will not be sent one when the " +
           "campaign opens. While the campaign is a draft, they can be added again.";

    /// <summary>What a remove did. (T247)</summary>
    public static string InviteeRemoved(string describedInvitee)
        => $"{describedInvitee} has been removed from this campaign, and will not be emailed a link when it opens.";

    /// <summary>
    /// Why Open campaign is disabled on a draft that invites nobody (T225, the T107 pattern): opening mails each invitee
    /// a link, and the handler refuses a campaign with none.
    /// </summary>
    public const string OpenNeedsInvitees = "add at least one invitee first. Opening the campaign emails each invitee a link to respond.";

    /// <summary>
    /// What an open campaign's page says of the links that did not reach their respondents (T251): how many, and what
    /// Resend does. It never says whose: the coordinator knows which address is whom, and an undelivered respondent is one
    /// who has not answered (T217).
    /// </summary>
    public static string LinksNotDelivered(int count)
        => (count == 1 ? "1 link was not delivered." : $"{count} links were not delivered.") +
           " Resend sends each of these respondents a new link; this page never says who they are.";

    /// <summary>The Resend button's label, with how many links it sends again. (T251)</summary>
    public static string ResendLinksLabel(int count) => count == 1 ? "Resend 1 link" : $"Resend {count} links";

    /// <summary>What a resend did: handed the links to the mail worker, which has not yet sent them. (T251)</summary>
    public static string LinksResent(int count)
        => count == 1 ? "1 new link is being sent." : $"{count} new links are being sent.";

    /// <summary>
    /// What an open campaign's page says of links whose mail has not been reported on yet (T251). The page does not
    /// refresh by itself, so it says how to find out.
    /// </summary>
    public static string LinksBeingSent(int count)
        => count == 1
            ? "1 link is still being sent. Reload this page to see whether it was delivered."
            : $"{count} links are still being sent. Reload this page to see whether they were delivered.";

    /// <summary>
    /// What an open reports (T251). Only that the links are on their way: the mail server answers after the request, and
    /// until T251 the page said that each respondent had been emailed, including while nobody had been.
    /// </summary>
    public const string CampaignOpened = "Campaign opened; links are being sent.";

    /// <summary>
    /// The campaign page's and the report's line once the registrar's programme has ended (T369, K14, R10): a release still
    /// records the evidence, but she can no longer read it, and no release mail goes (F4). Her name each time, never a
    /// pronoun (build-lanes § Never).
    /// </summary>
    public static string ProgrammeEnded(string subjectName, DateOnly endedOn)
        => $"{subjectName}'s programme ended on {endedOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}. A release " +
           $"still records the feedback as evidence on {subjectName}'s record, but {subjectName} can no longer open MSF " +
           "reports to read it.";

    // ─── Flow 10: the campaign list, New MSF campaign and a campaign's page (T369, lane K) ───────────────────────

    /// <summary>The list's heading and tab, and the menu's words (T280).</summary>
    public const string ListTitle = "MSF campaigns";

    public const string ListSubtitle = "Create and review multi-source feedback campaigns.";

    /// <summary>The list's caption (L2–L7): its order, and what it never shows (T217).</summary>
    public const string ListCaption =
        "MSF campaigns, newest first. Counted by respondent group: this list never shows who was invited or who responded.";

    /// <summary>The list's h2 while it loads; once loaded it carries its plain count (<see cref="ListHeading" />).</summary>
    public const string ListHeadingLoading = "Campaigns";

    /// <summary>The list's h2 with its plain count, "Campaigns (2)", as My activities' "All activities (2)".</summary>
    public static string ListHeading(int count) => $"Campaigns ({count.ToString(CultureInfo.InvariantCulture)})";

    public const string ListLoading = "Loading MSF campaigns.";

    /// <summary>The list's failed read (L9, T329): never the exception's text (T272).</summary>
    public const string ListLoadFailed =
        "Could not load MSF campaigns. Nothing has changed. Try again, or come back in a few minutes.";

    public const string ListEmptyTitle = "No MSF campaigns";

    public const string ListEmptyBody = "Create the first MSF campaign to start respondent outreach.";

    /// <summary>A row's Withdraw, which opens the dialog (L2).</summary>
    public const string WithdrawRowLabel = "Withdraw…";

    /// <summary>The campaign page's Withdraw, which opens the dialog (K1–K14).</summary>
    public const string WithdrawCampaignLabel = "Withdraw campaign…";

    /// <summary>Withdraw's safe button, which says what it keeps (L3, K10; DESIGN.md § ConfirmDialog).</summary>
    public const string KeepCampaignLabel = "Keep the campaign";

    /// <summary>A draft address's Remove, which opens the dialog (K2).</summary>
    public const string RemoveRowLabel = "Remove…";

    /// <summary>Remove's safe button (K3).</summary>
    public const string KeepInviteeLabel = "Keep the invitee";

    /// <summary>The first words of a refused action (K20–K22): "Not withdrawn." and then the refusal in its own words.</summary>
    public const string NotWithdrawn = "Not withdrawn.";

    public const string NotOpened = "Not opened.";

    public const string NotResent = "Not resent.";

    public const string NotRemoved = "Not removed.";

    public const string NotAdded = "Not added.";

    /// <summary>
    /// An action that failed for no reason a handler gave (T329): a fixed sentence, and the failure goes to the log. A
    /// handler's own refusal, written in Wombat's words, shows instead.
    /// </summary>
    public const string ActionFailed = "Something went wrong. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>What an add did (F11): the address left the result, which the list below shows.</summary>
    public const string InviteeAdded = "Invitee added.";

    // The campaign page (K1–K22).

    /// <summary>The campaign page's heading and tab once read: "MSF campaign for Lerato Molefe".</summary>
    public static string CampaignTitle(string subjectName) => $"MSF campaign for {subjectName}";

    /// <summary>The heading while the campaign is read, and after a failed read (K17, K18).</summary>
    public const string CampaignLoadingTitle = "MSF campaign";

    public const string CampaignLoading = "Loading the campaign.";

    public const string CampaignLoadFailed =
        "Could not load this campaign. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>A campaign this caller does not run, or none (K16): one answer for both (T113).</summary>
    public const string CampaignUnavailableTitle = "Campaign unavailable";

    public const string CampaignUnavailableBody = "The selected campaign could not be loaded.";

    public const string GoToCampaigns = "Go to MSF campaigns";

    /// <summary>The subtitle: "Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28".</summary>
    public static string CampaignSubtitle(string templateName, string kind, DateOnly opensOn, DateOnly closesOn)
        => $"{templateName} · {kind} · {Day(opensOn)} to {Day(closesOn)}";

    /// <summary>
    /// The status card's h2 (F7): what the state means. An open campaign's is its count, "8 of 9 invitees have
    /// responded.", singular where it counts one (T270).
    /// </summary>
    public static string StatusLine(MsfCampaignState state, int responded, int invited)
        => state switch
        {
            MsfCampaignState.Draft => DraftLine,
            MsfCampaignState.Open => invited == 1
                ? $"{responded.ToString(CultureInfo.InvariantCulture)} of 1 invitee has responded."
                : $"{responded.ToString(CultureInfo.InvariantCulture)} of {invited.ToString(CultureInfo.InvariantCulture)} invitees have responded.",
            MsfCampaignState.Closed or MsfCampaignState.UnderReview =>
                "Closed to responses, and every respondent's email address has been removed.",
            MsfCampaignState.Released => "The report has been released to the trainee.",
            _ => "Withdrawn. It takes no responses, any link sent no longer works, and every respondent's email address has " +
                 "been removed. It will never be released to the trainee."
        };

    public const string DraftLine = "Add invitees, then open the campaign to send anonymous response links.";

    /// <summary>
    /// What comes next, under the status line: an open campaign names its respondents' last day and where Close campaign
    /// is; one closed and under review where it is released, and that it can be withdrawn here (T199). It says each
    /// respondent "is emailed" a link: the mail leaves after the open (T251).
    /// </summary>
    public static string? StatusNote(MsfCampaignState state, DateOnly closesOn)
        => state switch
        {
            MsfCampaignState.Open =>
                $"Each respondent is emailed a link, which gives {Day(closesOn)} as their last day to respond. The campaign " +
                "closes by itself once that day has passed; to close it sooner, use Close campaign on its report.",
            MsfCampaignState.Closed or MsfCampaignState.UnderReview =>
                "Review the responses on its report, and release it to the trainee there. If it should never be released, " +
                "withdraw it here.",
            _ => null
        };

    public const string NobodyInvitedYet = "Nobody has been invited yet.";

    public const string NobodyInvited = "Nobody was invited.";

    /// <summary>Under a draft's "Nobody has been invited yet.": when the page stops listing anyone (T247).</summary>
    public const string DraftNeverLists =
        "Once the campaign opens, this page never lists who was invited or which of them responded.";

    /// <summary>A count table's caption, on a draft and once opened (T217 review).</summary>
    public static string CountsCaption(MsfCampaignState state)
        => state == MsfCampaignState.Draft
            ? "Counted by respondent group. Once the campaign opens, this page never lists who was invited or which of them responded."
            : "Counted by respondent group. This page never lists who was invited or which of them responded.";

    public const string CountsHeading = "Invitees by respondent group";

    public const string AllGroups = "All groups";

    /// <summary>Beside Open campaign on a draft that invites someone: who is mailed, and their last day (K2).</summary>
    public static string OpeningEmails(int invited, DateOnly closesOn)
        => invited == 1
            ? $"Opening emails the 1 invitee a link to respond, with {Day(closesOn)} as their last day."
            : $"Opening emails each of the {invited.ToString(CultureInfo.InvariantCulture)} invitees a link to respond, with {Day(closesOn)} as their last day.";

    /// <summary>
    /// Why Open campaign is unavailable on a draft whose trainee is not current (K13; T270, T284): the rule open applies,
    /// said before the press. Withdraw still works.
    /// </summary>
    public static string OpenNeedsCurrentTrainee(string subjectName)
        => $"{subjectName} is no longer in a programme. Withdraw this campaign.";

    /// <summary>Why Add invitee is unavailable on that draft (K13).</summary>
    public static string AddNeedsCurrentTrainee(string subjectName)
        => $"{subjectName} is no longer in a programme, so nobody can be invited.";

    public const string LinksHeading = "Links";

    /// <summary>What counts as not delivered (T251, round 3 S3): said wherever a link was not delivered or was resent.</summary>
    public const string LinksHourRule = "A link nothing was heard of for an hour counts as not delivered.";

    public const string AddressesHeading = "Addresses invited";

    public const string AddressesNote =
        "Listed only while the campaign is a draft, so that an address added by mistake can be removed. No invitee holds " +
        "a working link until the campaign opens.";

    public const string AddInviteeHeading = "Add an invitee";

    /// <summary>Above a form whose required fields carry the mark (M7).</summary>
    public const string RequiredMarkLine = "* marks a field you must fill in.";

    public const string LearnerInviteesNote =
        "Every invitee is in one respondent group, Learner. The context is named only on the coordinator's report.";

    /// <summary>The draft's addresses under their count, "9 addresses", "1 address".</summary>
    public static string AddressCount(int count)
        => count == 1 ? "1 address" : $"{count.ToString(CultureInfo.InvariantCulture)} addresses";

    public const string NoAddressYet = "No address yet.";

    /// <summary>A learner's line under their address (K15): the group, then where they were taught.</summary>
    public static string LearnerLine(string? teachingContext)
        => string.IsNullOrWhiteSpace(teachingContext) ? "Learner" : $"Learner · Teaching context: {teachingContext}";

    public const string AboutHeading = "About this campaign";

    /// <summary>About's Trainee (K1): the name, then the training year while they are current.</summary>
    public static string AboutTrainee(string subjectName, int? trainingYear)
        => trainingYear is { } year ? $"{subjectName}, training year {year.ToString(CultureInfo.InvariantCulture)}" : subjectName;

    /// <summary>About's Thresholds (K1, M4): "5 responses; 3 in a respondent group; 2 respondent groups reporting".</summary>
    public static string AboutThresholds(int minimumResponses, int minimumInGroup, int minimumGroups)
        => $"{Count(minimumResponses, "response", "responses")}; {minimumInGroup.ToString(CultureInfo.InvariantCulture)} in a " +
           $"respondent group; {Count(minimumGroups, "respondent group", "respondent groups")} reporting";

    /// <summary>Learner feedback's note under its thresholds (M4, K15).</summary>
    public const string LearnerGroupsFixed =
        "Learner feedback has one respondent group, so its groups-reporting minimum is fixed at 1.";

    /// <summary>About's Closed to responses (K9): the South African day it closed.</summary>
    public static string AboutClosed(DateTime closedOnUtc) => $"on {Day(ProgrammeCalendar.DateOf(closedOnUtc))}";

    /// <summary>About's Evidence (K1, K19): one or two EPAs by code, more by their number.</summary>
    public static string AboutEvidence(IReadOnlyList<string> codes)
        => codes.Count switch
        {
            0 => "Evidence for no EPA.",
            1 => $"Evidence for {codes[0]}",
            2 => $"Evidence for {codes[0]} and {codes[1]}",
            _ => $"Evidence for {codes.Count.ToString(CultureInfo.InvariantCulture)} EPAs"
        };

    /// <summary>An EPA as About lists it: "PAED-010 — Leading and operating within a clinical team".</summary>
    public static string EpaLine(string code, string title) => $"{code} — {title}";

    // New MSF campaign (C1–C8).

    public const string NewCampaignTitle = "New MSF campaign";

    public const string NewCampaignSubtitle =
        "One campaign for one current trainee: its questionnaire, its window, its thresholds and the EPAs it is evidence " +
        "for. Creating it makes a draft; nobody is emailed until you open it.";

    public const string NewCampaignLoading = "Loading the templates and trainees.";

    /// <summary>A failed read of the templates or the trainees (C6, T329): the circuit stays up, Try again reads both.</summary>
    public const string NewCampaignLoadFailed =
        "Could not load what this campaign needs. Nothing has changed. Try again, or come back in a few minutes.";

    public const string QuestionnaireHeading = "Questionnaire";

    public const string QuestionnaireNote = "Written once, used by every campaign that names it.";

    public const string NoTemplateYet = "No template yet";

    public const string NoTemplateHelp = "Write the first one below; it is chosen here once it is added.";

    public const string WriteQuestionnaire = "Write a new questionnaire";

    public const string ScaleQuestionHelp =
        "Answered on five points, from Well below expectations to Well above expectations.";

    /// <summary>What Add template did (C2): "Template created." in bold, then that the new template is the campaign's.</summary>
    public const string TemplateCreatedLead = "Template created.";

    public static string TemplateChosen(string templateName) => $"{templateName} is chosen as this campaign's template.";

    public const string TraineeWindowHeading = "Trainee and window";

    public const string ChooseTrainee = "Choose a trainee";

    public const string TraineeHelp = "Current trainees you can run a campaign for.";

    public const string OpensOnHelp = "Today, on the South African calendar.";

    public const string ClosesOnHelp = "The respondents' last day to answer, on the South African calendar.";

    public const string ThresholdsHeading = "Thresholds";

    public const string ThresholdsNote =
        "Nothing is shown of a respondent group that falls short, so no answer can be traced to its author.";

    public const string MinimumResponsesLabel = "Minimum responses";

    public const string MinimumResponsesHelp = "Responses needed in all before the report can be released.";

    public const string MinimumInGroupLabel = "Minimum responses in a respondent group";

    public const string MinimumInGroupHelp =
        "A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for " +
        "multi-source feedback.";

    public const string MinimumGroupsLabel = "Minimum respondent groups reporting";

    public const string MinimumGroupsHelp =
        "How many respondent groups must clear the threshold beside it before the report can be released. Two is the " +
        "College's answer: eight peer doctors alone are not multi-source feedback.";

    public const string EvidenceHeading = "Evidence";

    public const string EvidenceLegend = "Evidence for these EPAs";

    public const string SelectTraineeFirst = "Select a trainee first.";

    /// <summary>How many EPAs are ticked (C3): "2 of 15 ticked".</summary>
    public static string Ticked(int ticked, int of)
        => $"{ticked.ToString(CultureInfo.InvariantCulture)} of {of.ToString(CultureInfo.InvariantCulture)} ticked";

    /// <summary>Why Create campaign is unavailable while no template exists (C1), after "Create campaign:".</summary>
    public const string CreateNeedsTemplate = "write a questionnaire first.";

    public const string NotCreated = "Not created.";

    /// <summary>A refused create's what-to-do (C4, C8; M9), one field or several.</summary>
    public static string FixFields(int count)
        => count == 1
            ? "Fix the field below and create the campaign again."
            : $"Fix the {count.ToString(CultureInfo.InvariantCulture)} fields below and create the campaign again.";

    // Form messages in Wombat's words (T324): never a model property's name.

    public const string ChooseTemplateMessage = "Choose the questionnaire this campaign uses.";

    public const string ChooseTraineeMessage = "Choose the trainee this campaign is about.";

    public const string EnterOpensOnMessage = "Enter the day the campaign opens.";

    public const string EnterClosesOnMessage = "Enter the respondents' last day.";

    public const string ClosesBeforeOpensMessage = "Enter a last day on or after the day the campaign opens.";

    public const string MinimumResponsesRange = "Enter a number from 1 to 100.";

    /// <summary>F2's floor in the field's words (C8); the summary line names the field first.</summary>
    public const string MinimumInGroupRange = "Enter a number from 3 to 100.";

    public const string MinimumGroupsRange = "Enter a number from 1 to 6.";

    public const string SelectAnEpa = "Select at least one EPA this campaign is evidence for.";

    public const string TemplateNameMessage = "Enter the template's name.";

    public const string ScaleQuestionMessage = "Enter the scale question.";

    public const string CommentQuestionMessage = "Enter the comment question.";

    public const string TemplateNameTooLong = "Enter a name of 200 characters or fewer.";

    public const string QuestionTooLong = "Enter a question of 1,000 characters or fewer.";

    public const string TeachingContextMessage = "Say where this learner was taught.";

    public const string TeachingContextTooLong = "Enter a teaching context of 200 characters or fewer.";

    /// <summary>
    /// A handler's refusal in its own words (<see cref="MsfRefusals.IsRefusal" />: a validator's messages, an
    /// <see cref="UnauthorizedAccessException" />, or the campaign's own <see cref="MsfRefusedException" />), or null for
    /// any other failure, which a page logs and answers with <see cref="ActionFailed" /> (T329, T272): an exception's text
    /// is never printed. Any other <see cref="InvalidOperationException" /> is such a failure: the framework throws that
    /// type too (T369, build review R1).
    /// </summary>
    public static string? RefusalOf(Exception exception)
        => MsfRefusals.IsRefusal(exception) ? RefusalText.Of(exception) : null;

    private static string Count(int count, string one, string many)
        => count == 1 ? $"1 {one}" : $"{count.ToString(CultureInfo.InvariantCulture)} {many}";

    private static string Day(DateOnly day) => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>The state in words: "Under review", not the enum's "UnderReview" (T217).</summary>
    public static string State(MsfCampaignState state)
        => state switch
        {
            MsfCampaignState.Draft => "Draft",
            MsfCampaignState.Open => "Open",
            MsfCampaignState.Closed => "Closed",
            MsfCampaignState.UnderReview => "Under review",
            MsfCampaignState.Released => "Released",
            MsfCampaignState.Withdrawn => "Withdrawn",
            _ => state.ToString()
        };
}
