using System.Globalization;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Web.Components.Shared.Activities;

namespace Wombat.Web.Components.Shared;

/// <summary>
/// The words My authorisations and the STAR register say about a STAR (T366, flow 09; boards R3-5, R3-6): its expiry on
/// the South African calendar (S3, T325), what ended it (Since, Why; the register's row line, S4, C22) and its
/// certificate's download (F4, C19, T272). One place, so the two pages say the same thing the same way.
/// </summary>
public static class StarWords
{
    /// <summary>The badge beside Expires shows from this many days before the expiry date.</summary>
    public const int ExpiringSoonDays = 30;

    /// <summary>A STAR with no expiry date (C18).</summary>
    public const string NoExpiry = "No expiry";

    /// <summary>The download's running words, on its button (C19).</summary>
    public const string Generating = "Generating…";

    /// <summary>The download's answer when the handler finds no certificate the reader may have (C19, T183).</summary>
    public const string CertificateNotFound = "The certificate could not be found.";

    /// <summary>The download's unexpected failure: fixed words, the failure itself logged (T272, T329).</summary>
    public const string CertificateFailed = "The certificate could not be made just now.";

    /// <summary>
    /// "Expires in 20 days", "Expires in 1 day", "Expires today": counted on the South African calendar, a STAR being
    /// valid through its expiry date (S3, T325). Null outside the last <see cref="ExpiringSoonDays" /> days, or once it
    /// has passed.
    /// </summary>
    public static string? ExpiresIn(DateOnly expiresOn, DateOnly today)
    {
        var days = expiresOn.DayNumber - today.DayNumber;
        return days switch
        {
            < 0 or > ExpiringSoonDays => null,
            0 => "Expires today",
            1 => "Expires in 1 day",
            _ => string.Create(CultureInfo.InvariantCulture, $"Expires in {days} days")
        };
    }

    /// <summary>
    /// The day a STAR stopped being in force: a revocation's South African day, or the day after an expiry date, since a
    /// STAR is valid through it (S3). Null for one still in force.
    /// </summary>
    public static DateOnly? Since(EntrustmentDecisionDto decision)
    {
        ArgumentNullException.ThrowIfNull(decision);
        return decision.Status switch
        {
            EntrustmentDecisionStatus.Revoked when decision.RevokedOn is { } revokedOn => ProgrammeCalendar.DateOf(Utc(revokedOn)),
            EntrustmentDecisionStatus.Expired when decision.ExpiresOn is { } expiresOn => expiresOn.AddDays(1),
            _ => null
        };
    }

    /// <summary>
    /// Why it is no longer in force, in the reviser's name and words (Q8, T367): "Revoked by Refilwe Mokoena: Staged on
    /// …"; "Its expiry date, 2026-11-02, passed."
    /// </summary>
    public static string Why(EntrustmentDecisionDto decision)
    {
        ArgumentNullException.ThrowIfNull(decision);
        return decision.Status switch
        {
            EntrustmentDecisionStatus.Revoked => Revoked(decision.RevokedByName, decision.RevocationReason),
            EntrustmentDecisionStatus.Expired when decision.ExpiresOn is { } expiresOn => $"Its expiry date, {Iso(expiresOn)}, passed.",
            _ => string.Empty
        };

        static string Revoked(string? by, string? reason)
        {
            var lead = by is null ? "Revoked" : $"Revoked by {by}";
            return string.IsNullOrWhiteSpace(reason) ? lead + "." : $"{lead}: {reason}";
        }
    }

    /// <summary>
    /// The register's row line under a revoked or superseded STAR (S4, C22): "Revoked 2026-10-13 at 09:12 SAST by Refilwe
    /// Mokoena: Staged on …"; "Superseded by STAR #18, issued 2026-10-13." (no supersession date is stored). Null for a
    /// row with nothing to add.
    /// </summary>
    public static string? RowLine(EntrustmentDecisionDto decision)
    {
        ArgumentNullException.ThrowIfNull(decision);
        switch (decision.Status)
        {
            case EntrustmentDecisionStatus.Revoked when decision.RevokedOn is { } revokedOn:
                var by = decision.RevokedByName is null ? string.Empty : $" by {decision.RevokedByName}";
                var reason = string.IsNullOrWhiteSpace(decision.RevocationReason) ? "." : $": {decision.RevocationReason}";
                return $"Revoked {At(revokedOn)}{by}{reason}";
            case EntrustmentDecisionStatus.Superseded when decision.SupersededByDecisionId is { } successor:
                var issued = decision.SupersededByIssuedOn is { } issuedOn ? $", issued {Iso(issuedOn)}" : string.Empty;
                return string.Create(CultureInfo.InvariantCulture, $"Superseded by STAR #{successor}{issued}.");
            default:
                return null;
        }
    }

    /// <summary>A card's download, its visible words inside its name (F4, WCAG 2.5.3): "Download certificate for PAED-001".</summary>
    public static string DownloadCard(string epaCode) => $"Download certificate for {epaCode}";

    /// <summary>
    /// No longer in force's download (F4): "Download certificate for PAED-002 (revoked)", "… (expired)".
    /// </summary>
    public static string DownloadEnded(EntrustmentDecisionDto decision)
    {
        ArgumentNullException.ThrowIfNull(decision);
        return $"{DownloadCard(decision.EpaCode)} ({decision.Status.ToString().ToLowerInvariant()})";
    }

    /// <summary>A date as every page prints one, "2026-10-13".</summary>
    public static string Iso(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>
    /// "2026-10-13 at 09:12 SAST": a stored UTC moment on the South African clock, through the one helper every activity
    /// page uses (ActivityMoments.When, "2026-10-13 09:12 SAST"), never an offset of its own (T366, R4).
    /// </summary>
    public static string At(DateTime utc)
    {
        var when = ActivityMoments.When(utc);
        var date = ActivityMoments.Date(utc);
        return $"{date} at {when[(date.Length + 1)..]}";
    }

    private static DateTime Utc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
