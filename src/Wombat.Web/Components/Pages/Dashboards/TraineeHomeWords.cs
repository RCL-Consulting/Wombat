using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.EntrustmentDecisions;

namespace Wombat.Web.Components.Pages.Dashboards;

/// <summary>
/// The Trainee's Home in words (T355, R1; Spec § 1, Home's rows): the targets card's notices and empties, Recent
/// decisions' empty and its File it again name, and My authorisations' rows and empties. Dates are ISO (T325; D1).
/// </summary>
public static class TraineeHomeWords
{
    /// <summary>Your targets, for a trainee with no curriculum yet (as built).</summary>
    public const string NoCurriculum =
        "No curriculum assigned yet. Once you are admitted, your targets for each period appear here.";

    /// <summary>Your targets, for a curriculum none of whose EPAs is in use (T158; as built).</summary>
    public const string NoEpaInUse =
        "No EPA on your curriculum is in use at the moment, so no target applies to you.";

    /// <summary>Recent decisions, when nobody has decided anything on her requests yet.</summary>
    public const string NoDecisions = "No decisions yet.";

    /// <summary>The words of the File it again link (E5), which begin its accessible name.</summary>
    private const string FileAgain = "File it again, to someone else";

    /// <summary>
    /// "Every target is met for Semester 2, 2026 and the 2026 academic year.", naming only the kinds that apply (note 15:
    /// "… for Semester 2, 2026." for a curriculum of semester items alone). Null unless at least one target applies and
    /// every target that applies is met.
    /// </summary>
    public static string? AllMet(TraineeCurriculumProgressSummaryDto summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        var applying = summary.SemesterTargetsApplying + summary.YearTargetsApplying;
        if (applying == 0 ||
            summary.SemesterTargetsMet < summary.SemesterTargetsApplying ||
            summary.YearTargetsMet < summary.YearTargetsApplying)
        {
            return null;
        }

        var windows = new List<string>(2);
        if (summary.SemesterTargetsApplying > 0)
        {
            windows.Add(summary.CurrentSemesterName);
        }

        if (summary.YearTargetsApplying > 0 && YearWindowName(summary) is { } year)
        {
            // "the" before a yearly window's name (Spec § 1, new words): "the 2026 academic year".
            windows.Add($"the {year}");
        }

        return $"Every target is met for {string.Join(" and ", windows)}.";
    }

    /// <summary>
    /// "No semester target this semester: you started part-way through it. Semester targets begin with Semester 1,
    /// 2027.", and its yearly twin "No yearly target this academic year: you started part-way through it. Yearly targets
    /// begin with the 2027 academic year." when the start waives that kind too (D14, D42). Null when the start waives
    /// neither, and before the programme starts (<see cref="NotStarted" /> says that).
    /// </summary>
    public static string? PartWay(TraineeCurriculumProgressSummaryDto summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        if (summary.ProgrammeNotStarted)
        {
            return null;
        }

        var sentences = new List<string>(4);
        if (summary.SemesterTargetsStart is { } semester)
        {
            sentences.Add("No semester target this semester: you started part-way through it.");
            sentences.Add($"Semester targets begin with {Capitalised(semester.Name)}.");
        }

        if (summary.YearTargetsStart is { } year)
        {
            sentences.Add("No yearly target this academic year: you started part-way through it.");
            sentences.Add($"Yearly targets begin with {year.Name}.");
        }

        return sentences.Count == 0 ? null : string.Join(" ", sentences);
    }

    /// <summary>"Your programme starts on 2027-01-15." Null once it has started.</summary>
    public static string? NotStarted(TraineeCurriculumProgressSummaryDto summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        return summary.ProgrammeNotStarted ? $"Your programme starts on {QuotaText.Iso(summary.ProgrammeStartDate)}." : null;
    }

    /// <summary>
    /// "Your programme ended on 2026-10-01, so no target applies to you any more. Your progress in each period is kept on
    /// My progress, read-only." (T252; A.4.6): the end in <see cref="QuotaText.ProgrammeEnded" />'s words, which read ISO
    /// (D1, lane A1).
    /// </summary>
    public static string Ended(ProgrammeEndDto ended)
    {
        ArgumentNullException.ThrowIfNull(ended);

        return $"{QuotaText.ProgrammeEnded(ended)}, so no target applies to you any more. " +
               "Your progress in each period is kept on My progress, read-only.";
    }

    /// <summary>
    /// My authorisations with no STAR: "No STAR yet. When the committee issues one, it shows here against training year 3's
    /// level."; "No STAR yet." alone once the programme has ended, or with no training year to name.
    /// </summary>
    public static string NoStar(int? stage, bool ended)
        => ended || stage is null
            ? "No STAR yet."
            : $"No STAR yet. When the committee issues one, it shows here against training year {stage}'s level.";

    /// <summary>
    /// A STAR row's line, under the EPA's name: "4, below training year 4's level of 5 · expires 2026-10-23"; "5, at or
    /// above training year 4's level of 5"; on another ladder "Independent on O-R Scale", which is not compared (T109).
    /// Where the item names no level for the year its exit level stands in, and the line says so ("below its exit level of
    /// 3a"), as the panel does. The expiry is added whenever the STAR has one.
    /// </summary>
    /// <exception cref="ArgumentException">The EPA has no STAR.</exception>
    public static string StarRow(EpaStandingDto epa, int targetYear)
    {
        ArgumentNullException.ThrowIfNull(epa);
        var decision = epa.Decision ?? throw new ArgumentException("A STAR row needs a STAR.", nameof(epa));

        var level = epa.YearTargetIsExitLevel
            ? $"its exit level of {epa.YearTargetLabel}"
            : $"training year {targetYear}'s level of {epa.YearTargetLabel}";

        var line = epa.YearStatus switch
        {
            EntrustmentStandingStatus.Below => $"{decision.LevelLabel}, below {level}",
            EntrustmentStandingStatus.AtOrAbove => $"{decision.LevelLabel}, at or above {level}",
            _ => decision.OtherLadderName is { } ladder ? $"{decision.LevelLabel} on {ladder}" : decision.LevelLabel
        };

        return decision.ExpiresOn is { } expires ? $"{line} · expires {QuotaText.Iso(expires)}" : line;
    }

    /// <summary>
    /// The STARs Home lists (R1): each below its level, or expiring within 30 days of <paramref name="today" />, by code.
    /// </summary>
    public static IReadOnlyList<EpaStandingDto> StarRows(EntrustmentStandingDto standing, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(standing);

        return standing.Epas
            .Where(epa => epa.Decision is { } decision &&
                          (epa.YearStatus == EntrustmentStandingStatus.Below ||
                           (decision.ExpiresOn is { } expires && expires >= today && expires <= today.AddDays(30))))
            .OrderBy(epa => epa.EpaCode, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    /// The accessible name of a declined row's link (E5): "File it again, to someone else: Mini-CEX (Paediatrics) ·
    /// PAED-002 · 2026-09-13", so each such link names its row.
    /// </summary>
    public static string FileAgainName(ActivitySummaryDto item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return $"{FileAgain}: {item.DisplayName ?? item.ActivityTypeName}";
    }

    /// <summary>"2026 academic year": the window a yearly item counts towards today, or null with no yearly item.</summary>
    private static string? YearWindowName(TraineeCurriculumProgressSummaryDto summary)
        => summary.Items.FirstOrDefault(item => !item.IsPerSemester)?.Current.Name;

    /// <summary>"semester 1, 2027" (<c>QuotaText.FirstCountedName</c>) as Home writes it: "Semester 1, 2027".</summary>
    private static string Capitalised(string name)
        => name.Length == 0 ? name : char.ToUpperInvariant(name[0]) + name[1..];

}
