using System.Globalization;

namespace Wombat.Web.Components.Shared;

/// <summary>
/// Flow 13's addresses (T379, graduation and exit, build-contracts § 0), so no page writes one by hand: the record's pages
/// a former trainee reaches from Home and My progress (lane R), the export and its staff route with the pages that offer it
/// (lane E, and lane P's profile header), Verify (lanes E and V), and the trainee's two staff pages the export's trail
/// returns to (lane E). The menu's items keep their own literals, which a test holds equal to these.
/// </summary>
public static class PortfolioLinks
{
    /// <summary>My progress: <c>/portfolio/progress</c>.</summary>
    public const string MyProgress = "/portfolio/progress";

    /// <summary>My authorisations, the STARs and their certificates: <c>/portfolio/authorisations</c>.</summary>
    public const string MyAuthorisations = "/portfolio/authorisations";

    /// <summary>My activities: <c>/activities/mine</c>.</summary>
    public const string MyActivities = "/activities/mine";

    /// <summary>The person's own export: <c>/portfolio/export</c>.</summary>
    public const string Export = "/portfolio/export";

    /// <summary>
    /// Verify portfolio export: <c>/portfolio/verify</c>. A static page (<c>[ExcludeFromInteractiveRouting]</c>), so a link to
    /// it is a plain <c>&lt;a href&gt;</c>, a full navigation (S13).
    /// </summary>
    public const string Verify = "/portfolio/verify";

    /// <summary>Trainees: <c>/admin/trainees</c>.</summary>
    public const string Trainees = "/admin/trainees";

    /// <summary>Programme trainees: <c>/programme/trainees</c>.</summary>
    public const string ProgrammeTrainees = "/programme/trainees";

    /// <summary>
    /// The staff export of one registrar's portfolio, by their user id: <c>/portfolio/export/{userId}</c> (Q4, F1). Offered
    /// only where the caller's DTO says <c>MayExport</c>.
    /// </summary>
    public static string ExportFor(string traineeUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(traineeUserId);
        return $"{Export}/{Uri.EscapeDataString(traineeUserId)}";
    }

    /// <summary>A trainee's profile, the Institutional admin's: <c>/admin/trainees/edit?id={profileId}</c> (M4's crumb).</summary>
    public static string TraineeProfile(int profileId)
        => $"/admin/trainees/edit?id={profileId.ToString(CultureInfo.InvariantCulture)}";

    /// <summary>A registrar's programme page, the four programme roles': <c>/programme/trainees/{profileId}</c> (M4's crumb).</summary>
    public static string ProgrammeTrainee(int profileId)
        => $"{ProgrammeTrainees}/{profileId.ToString(CultureInfo.InvariantCulture)}";
}
