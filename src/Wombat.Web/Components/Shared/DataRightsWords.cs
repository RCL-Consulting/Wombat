using Wombat.Application.Features.DataRights;
using Wombat.Domain.DataRights;
using Wombat.Web.Components.Shared.Activities;

namespace Wombat.Web.Components.Shared;

/// <summary>
/// The words the three data-rights pages share (T387, flow 14, build-contracts § 0): a request's type and status as a
/// person reads them (T324), its moment in SAST (T325, Q9), its page's title (Q12), and where its page lives. Each page
/// keeps its own sentences in its own words class; only what two pages say alike is here.
/// </summary>
public static class DataRightsWords
{
    /// <summary>The queue.</summary>
    public const string QueueLink = "/admin/data-rights";

    /// <summary>My data rights.</summary>
    public const string MineLink = "/account/data-rights";

    /// <summary>A request's page: <c>/admin/data-rights/{id}</c>.</summary>
    public static string RequestLink(Guid id) => $"/admin/data-rights/{id}";

    /// <summary>A Completed Access or Export's download: <c>/account/data-rights/download/{id}</c> (Program.cs).</summary>
    public static string DownloadLink(Guid id) => $"/account/data-rights/download/{id}";

    /// <summary>"Export", "Rectification": never the enum's name printed (T324).</summary>
    public static string Type(DataRightsRequestType type) => DataRightsLabels.Type(type);

    /// <summary>"Submitted", "Approved", "Withdrawn".</summary>
    public static string Status(DataRightsRequestStatus status) => DataRightsLabels.Status(status);

    /// <summary>A stored UTC moment as every data-rights page prints it, to the minute: "2026-10-28 00:01 SAST" (Q9).</summary>
    public static string When(DateTime utc) => ActivityMoments.When(utc);

    /// <summary>A day: "2026-10-28".</summary>
    public static string Day(DateOnly day) => day.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>A stored UTC moment's South African day: "2026-10-28" (the answer's "Approved on ‹day›").</summary>
    public static string DayOf(DateTime utc) => ActivityMoments.Date(utc);

    /// <summary>A request's page title, h1 and tab (Q12): "Erasure request from Sipho Ndlovu", the live name or the pseudonym.</summary>
    public static string Title(DataRightsRequestType type, string requesterName) => $"{Type(type)} request from {requesterName}";

    /// <summary>A correction's record: "Account › Last name", "Other › Home address" (<see cref="DataRightsRecords.Label" />).</summary>
    public static string Record(string targetType) => DataRightsRecords.Label(targetType);
}
