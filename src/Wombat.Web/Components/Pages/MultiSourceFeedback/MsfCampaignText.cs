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
