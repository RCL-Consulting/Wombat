using System.Globalization;
using Wombat.Domain.MultiSourceFeedback;

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

    /// <summary>What withdrawing does, and that it cannot be undone (MsfCampaign.Withdraw, T202).</summary>
    public static string WithdrawBody(string describedCampaign)
        => $"Withdraw the campaign for {describedCampaign}? It will take no more responses: any link already sent stops " +
           "working, and every respondent's email address is removed. A withdrawn campaign is never released to the " +
           "trainee, and withdrawing cannot be undone.";

    /// <summary>What a withdraw did.</summary>
    public static string Withdrawn(string describedCampaign)
        => $"The campaign for {describedCampaign} has been withdrawn. Its respondents' links no longer work, and their " +
           "email addresses have been removed.";

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
    public static string RowLinkName(string label, string describedCampaign)
        => $"{label}: the campaign for {describedCampaign}";

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

    /// <summary>
    /// The state's badge (DESIGN.md § Badges): a draft is grey, an open campaign blue, one closed and under review amber,
    /// a released one green, and a withdrawn one red. (T217)
    /// </summary>
    public static string StateBadge(MsfCampaignState state)
        => state switch
        {
            MsfCampaignState.Open => "badge-submitted",
            MsfCampaignState.Closed or MsfCampaignState.UnderReview => "badge-accepted",
            MsfCampaignState.Released => "badge-completed",
            MsfCampaignState.Withdrawn => "badge-declined",
            _ => "badge-draft"
        };
}
