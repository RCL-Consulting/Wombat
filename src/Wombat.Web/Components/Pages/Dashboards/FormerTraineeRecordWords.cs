using System.Globalization;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Dashboards.FormerTrainee;
using Wombat.Application.Features.EntrustmentDecisions;

namespace Wombat.Web.Components.Pages.Dashboards;

/// <summary>
/// The former trainee's record in words (T379, flow 13, lane R; R3-H01, R3-H02, R3-R01, R3-R07, R3-R09; Spec 3's Notice,
/// Empty and Row rows): Home's card "Your training record", My progress's "Your record:" line, and what My activities and
/// My authorisations say to a person whose programme has ended. Dates are ISO (D1); "you" and "your" to the former trainee
/// herself (N10).
/// </summary>
public static class FormerTraineeRecordWords
{
    /// <summary>Flow 01's card for a person with no role and no record to show (R3-H04, as built).</summary>
    public const string NoRoleTitle = "No role assigned";

    /// <summary>Its sentence, as built (T261).</summary>
    public const string NoRoleBody =
        "Your account holds no role at the moment, so there is nothing to show you here. An administrator can give you one.";

    /// <summary>The card's title (Q1).</summary>
    public const string CardTitle = "Your training record";

    /// <summary>The card's second sentence, after the end (Spec 3's Notice row).</summary>
    public const string KeptReadOnly = "Your record is kept, read-only: no target applies to you any more.";

    /// <summary>My authorisations' line with no STAR in force, though one was issued (S8; PROPOSED, drawn on no board).</summary>
    public const string NoneInForce = "None of your STARs is in force now; My authorisations lists them.";

    /// <summary>The STARs row as plain text, with no STAR ever issued (R3-H02, F6).</summary>
    public const string StarsPlain = "STARs";

    /// <summary>
    /// With no STAR ever issued: the STARs row's line on Home (R3-H02), and My authorisations' empty card for a person whose
    /// programme has ended (Spec 3's Empty row, in place of the built "No active authorisations yet").
    /// </summary>
    public const string NoStarIssued = "No STAR was issued to you, so there is no certificate to download.";

    /// <summary>My activities' line on Home (R3-H01).</summary>
    public const string MyActivitiesLine = "Every activity you filed or that was filed about you, read-only.";

    /// <summary>Export portfolio's line on Home (R3-H01).</summary>
    public const string ExportLine = "A PDF of your record. Anyone holding it can check it on Verify portfolio export.";

    /// <summary>My progress's "Your record:" line's label (R3-R01, S8).</summary>
    public const string RecordLineLabel = "Your record:";

    /// <summary>My activities' table caption, for everyone (S18: the shipped table had none).</summary>
    public const string MyActivitiesCaption = "Every activity filed by you or about you, with who has it now and its credit.";

    /// <summary>
    /// The card's lead: "You completed your programme on 2026-10-26." or "Your programme ended on 2026-10-24.", in My
    /// progress's words for the end (<see cref="QuotaText.ProgrammeEnded" />), so the two pages say it alike.
    /// </summary>
    public static string Ended(FormerTraineeHomeDto record)
    {
        ArgumentNullException.ThrowIfNull(record);

        return $"{QuotaText.ProgrammeEnded(new ProgrammeEndDto(record.Completed, record.EndedOn, record.EndedOn))}.";
    }

    /// <summary>
    /// My progress's line on Home: "Every period of your programme, to training year 4, and your standing against Annexure
    /// A." Without the training year where the record names none (a last day before the programme start).
    /// </summary>
    public static string MyProgressLine(FormerTraineeHomeDto record)
    {
        ArgumentNullException.ThrowIfNull(record);

        return record.TrainingYearAtEnd is { } year
            ? $"Every period of your programme, to training year {year.ToString(CultureInfo.InvariantCulture)}, and your standing against Annexure A."
            : "Every period of your programme, and your standing against Annexure A.";
    }

    /// <summary>
    /// My authorisations' line on Home where a STAR was issued: "Your 15 STARs in force, each with its certificate to
    /// download." ("Your 1 STAR in force, …"), or <see cref="NoneInForce" /> when none stands in force now (S8).
    /// </summary>
    public static string MyAuthorisationsLine(RecordStarsDto stars)
    {
        ArgumentNullException.ThrowIfNull(stars);

        return stars.InForce switch
        {
            0 => NoneInForce,
            1 => "Your 1 STAR in force, with its certificate to download.",
            var count => $"Your {count.ToString(CultureInfo.InvariantCulture)} STARs in force, each with its certificate to download."
        };
    }

    /// <summary>
    /// My activities' subtitle note for a person whose programme has ended: "Read-only: your programme ended on 2026-10-24,
    /// so nothing new can be filed." (R3-R07, S7), after the built subtitle.
    /// </summary>
    public static string ReadOnlyNote(DateOnly endedOn)
        => $"Read-only: your programme ended on {QuotaText.Iso(endedOn)}, so nothing new can be filed.";

    /// <summary>
    /// My activities' note row under an activity observed after the last day (R3-R07; T281): "Observed after your last day
    /// in the programme, 2026-10-24, so it counts towards nothing."
    /// </summary>
    public static string ObservedAfterEnd(DateOnly endedOn)
        => $"Observed after your last day in the programme, {QuotaText.Iso(endedOn)}, so it counts towards nothing.";
}
