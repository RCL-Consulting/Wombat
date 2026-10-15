using System.Globalization;

namespace Wombat.Web.Components.Shared;

/// <summary>
/// The MSF pages' addresses (T369, flow 10, build-contracts § 0), so no page writes one by hand. The menu's items
/// (<c>NavItems.MsfReports</c>, <c>NavItems.MsfCampaigns</c>) and Home's action keep their own literals.
/// </summary>
public static class MsfLinks
{
    /// <summary>The Coordinator's list of campaigns: <c>/msf/campaigns</c>.</summary>
    public const string Campaigns = "/msf/campaigns";

    /// <summary>The create form: <c>/msf/campaigns/new</c>.</summary>
    public const string NewCampaign = "/msf/campaigns/new";

    /// <summary>The programme's MSF coverage: <c>/msf/coverage</c>.</summary>
    public const string Coverage = "/msf/coverage";

    /// <summary>The registrar's released reports: <c>/msf/my-reports</c>. The release mail links here too (T369, F4).</summary>
    public const string MyReports = "/msf/my-reports";

    /// <summary>One campaign's page: <c>/msf/campaigns/12</c>.</summary>
    public static string Campaign(int campaignId) => Invariant($"{Campaigns}/{campaignId}");

    /// <summary>One campaign's report, the Coordinator's: <c>/msf/reports/12</c>.</summary>
    public static string Report(int campaignId) => Invariant($"/msf/reports/{campaignId}");

    /// <summary>One released report, the registrar's own copy: <c>/msf/my-reports/12</c>.</summary>
    public static string MyReport(int campaignId) => Invariant($"{MyReports}/{campaignId}");

    private static string Invariant(FormattableString value) => value.ToString(CultureInfo.InvariantCulture);
}
