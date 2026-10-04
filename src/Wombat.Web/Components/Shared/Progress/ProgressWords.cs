using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Curricula;

namespace Wombat.Web.Components.Shared.Progress;

/// <summary>
/// The words of a registrar's progress (T355): the figures, a count cell, This period's lines, the Alerts, the index's
/// captions and cadence, the paused row, and Home's furthest-short row. One class, so Home, My progress and the EPA page
/// say a count the same way (R3).
/// </summary>
/// <remarks>
/// <para>
/// The built sentences are moved here from <c>MyProgress.razor</c> word for word where the boards keep them, with every
/// date ISO (T325; decision D1): "2 more by 2026-11-30", never "30 November 2026". A figure is a count against a target
/// for a named window ("1 of 3 this semester"), never "n / m", a percentage or a lifetime total, and a training year and
/// an academic year are never both called "year" (R3).
/// </para>
/// <para>
/// A yearly window is "the 2026 academic year" inside a sentence (C3, C11) and "2026 academic year" as a name
/// (<see cref="QuotaText.WindowName" />). Built as whole strings, because Razor drops a space standing alone before an
/// expression.
/// </para>
/// </remarks>
public static class ProgressWords
{
    /// <summary>"No decision cadence": said to a screen reader beside the dash of an item with none (notes 8, 9).</summary>
    public const string NoCadence = "No decision cadence";

    /// <summary>
    /// A paused EPA's row, and the Alert under its page's h1 (N6; D48). True of the engine: a completion while the EPA is
    /// paused credits nothing, the credit already earned is kept, and reactivating credits what was completed meanwhile.
    /// </summary>
    public const string PausedRow =
        "Paused by the College. It is not a target while it is paused. Its ratings and the credit it had earned are kept, " +
        "and what is completed on it meanwhile is credited if it is restored.";

    /// <summary>"1 of 3 this semester", "0 of 1 in 2026": the figure of an item's current window.</summary>
    public static string Figure(TraineeCurriculumProgressDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return $"{item.Current.Count} of {item.Current.Target} {WindowWords(item)}";
    }

    /// <summary>"this semester" or "in 2026": the words after a figure, by the item's window (as built).</summary>
    public static string WindowWords(TraineeCurriculumProgressDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return item.IsPerSemester ? "this semester" : $"in {item.Current.Start.Year}";
    }

    /// <summary>
    /// Whether the count cell draws a bar under its figure: only while the current window holds a target, and never on an
    /// ended programme's record (whose items carry <see cref="TraineeCurriculumProgressDto.Periods" />, R5). A waived,
    /// not-started or ended cell is one line, <see cref="CountMeta" />, with no figure and no bar (R3).
    /// </summary>
    public static bool HasBar(TraineeCurriculumProgressDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return item.Current.Applies && item.Periods is null;
    }

    /// <summary>
    /// The line under a count's figure and bar, or, where <see cref="HasBar" /> is false, the cell's one line:
    /// "2 more by 2026-11-30", "Target met for Semester 2, 2026.", "Target met for the 2026 academic year."; in December
    /// "3 more; encounters in December still count towards Semester 2, 2026." (D40, C3); waived "No target this semester
    /// · 2 recorded · targets start with semester 1, 2027"; not started "No target yet · targets start with semester 1,
    /// 2027"; and the ended forms as built.
    /// </summary>
    public static string CountMeta(TraineeCurriculumProgressDto item, DateOnly asOf)
    {
        ArgumentNullException.ThrowIfNull(item);
        var current = item.Current;

        if (current.Applies)
        {
            if (current.IsMet)
            {
                return $"Target met for {InSentence(item)}.";
            }

            // December: the College's year has ended, but December encounters still count (D40).
            return asOf > current.NominalEnd
                ? $"{current.Shortfall} more; encounters in December still count towards {InSentence(item)}."
                : $"{current.Shortfall} more by {QuotaText.Iso(current.NominalEnd)}";
        }

        var startsWith = current.FirstCountedName is { } first ? $" · targets start with {first}" : string.Empty;

        if (current.IsExempt)
        {
            return $"No target {WindowWords(item)} · {current.Count} recorded{startsWith}";
        }

        if (current.EndedPartWay)
        {
            // D49: the programme ended before this window's last month. Never "targets start with": they stopped.
            return $"No target {WindowWords(item)} · {current.Count} recorded · your programme ended part-way through";
        }

        if (current.IsAfterProgrammeEnd)
        {
            return $"No target: this is after your programme ended · {current.Count} recorded";
        }

        return $"No target yet{startsWith}";
    }

    /// <summary>
    /// Home's furthest-short row, under the EPA's name (C3): "0 of 3 this semester · 3 more by 2026-11-30"; in December
    /// "0 of 3 this semester · 3 more; encounters in December still count towards Semester 2, 2026.": the figure kept, and
    /// only "n more by" replaced.
    /// </summary>
    public static string ShortRow(TraineeCurriculumProgressDto item, DateOnly asOf)
    {
        ArgumentNullException.ThrowIfNull(item);
        return item.Current.Applies
            ? $"{Figure(item)} · {CountMeta(item, asOf)}"
            : CountMeta(item, asOf);
    }

    /// <summary>
    /// "At the minimum level when observed: 2 of 3": of the window's credited encounters, how many met the minimum of the
    /// training year each was observed in (the stored tally, T073). Null when nothing is credited in the window.
    /// </summary>
    public static string? AtMinimum(QuotaWindowDto window)
    {
        ArgumentNullException.ThrowIfNull(window);
        return window.Count == 0
            ? null
            : $"At the minimum level when observed: {window.MinimumLevelReachedCount} of {window.Count}";
    }

    /// <summary>
    /// "Last encounter 2026-09-23", or with <c>EncounterDate.Label</c>'s mark when nobody stated the date (T219): "Last
    /// encounter not recorded (created 2026-09-23)". Null when the window has none.
    /// </summary>
    public static string? LastEncounter(QuotaWindowDto window)
    {
        ArgumentNullException.ThrowIfNull(window);
        return window.LastObservedOn is { } last
            ? $"Last encounter {EncounterDate.Label(last, window.LastObservedOnDeclared)}"
            : null;
    }

    /// <summary>
    /// The window before this one, in words (as built; O2): "Semester 1, 2026: 0 of 3, 3 short". A window with no target
    /// says why: the programme started part-way through it (D14), or ended part-way through it before its last month
    /// (D49), or it is after the programme ended. A running programme reaches only the first; the others are worded so
    /// that no reader can ever make an ended programme read as a late start. An ended programme lists its periods with
    /// <see cref="EndedPeriodLine" /> instead (T252).
    /// </summary>
    public static string PreviousLine(QuotaWindowDto previous)
    {
        ArgumentNullException.ThrowIfNull(previous);

        if (previous.EndedPartWay)
        {
            return $"{previous.Name}: no target (your programme ended part-way through) · {previous.Count} recorded";
        }

        if (previous.IsAfterProgrammeEnd)
        {
            return $"{previous.Name}: no target (after your programme ended) · {previous.Count} recorded";
        }

        if (!previous.Applies)
        {
            return $"{previous.Name}: no target (you started part-way through) · {previous.Count} recorded";
        }

        return previous.IsMet
            ? $"{previous.Name}: {previous.Count} of {previous.Target}, met"
            : $"{previous.Name}: {previous.Count} of {previous.Target}, {previous.Shortfall} short";
    }

    /// <summary>
    /// One period of an ended programme, in the words the portfolio PDF prints it (as built; T169, T209, T252; R5). A
    /// closed period is met or short. One that has not closed by today is its count so far and nothing more: the trainee
    /// owes nothing, but an encounter observed before the end can still be credited. Either says how many of its
    /// encounters met the minimum level when observed. A waived period says why (D14, D49). Never "n more by".
    /// </summary>
    public static string EndedPeriodLine(QuotaWindowDto period, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(period);

        if (period.IsExempt)
        {
            return $"no target (you started part-way through) · {period.Count} recorded";
        }

        if (period.EndedPartWay)
        {
            return $"no target (your programme ended part-way through) · {period.Count} recorded";
        }

        if (period.IsAfterProgrammeEnd)
        {
            return $"no target (after your programme ended) · {period.Count} recorded";
        }

        if (!period.Applies)
        {
            return "no target (before your programme started)";
        }

        var standing = period.HasClosedBy(today)
            ? (period.IsMet ? ", met" : $", {period.Shortfall} short")
            : (period.IsMet ? " so far, met" : " so far");
        var atMinimum = period.Count > 0
            ? $"; {period.MinimumLevelReachedCount} at the minimum level when observed"
            : string.Empty;

        return $"{period.Count} of {period.Target}{standing}{atMinimum}";
    }

    /// <summary>
    /// This period's semester figure and its label: ("1 of 10", "EPAs met this semester"), or, where no semester target
    /// applies, ("none apply yet", "Semester targets") (C2).
    /// </summary>
    public static (string Value, string Label) SemesterFigure(TraineeCurriculumProgressSummaryDto summary)
    {
        ArgumentNullException.ThrowIfNull(summary);
        return summary.SemesterTargetsApplying == 0
            ? (TargetsLine(0, 0, "this semester"), "Semester targets")
            : ($"{summary.SemesterTargetsMet} of {summary.SemesterTargetsApplying}", "EPAs met this semester");
    }

    /// <summary>This period's yearly figure: ("0 of 5", "EPAs met in 2026"), or ("none apply yet", "Yearly targets").</summary>
    public static (string Value, string Label) YearFigure(TraineeCurriculumProgressSummaryDto summary)
    {
        ArgumentNullException.ThrowIfNull(summary);
        return summary.YearTargetsApplying == 0
            ? (TargetsLine(0, 0, $"in {summary.AsOf.Year}"), "Yearly targets")
            : ($"{summary.YearTargetsMet} of {summary.YearTargetsApplying}", $"EPAs met in {summary.AsOf.Year}");
    }

    /// <summary>"3 of 10 EPAs met this semester", or "none apply yet" (as built).</summary>
    public static string TargetsLine(int met, int applying, string when)
        => applying == 0
            ? "none apply yet"
            : $"{met} of {applying} EPAs met {when}";

    /// <summary>
    /// This period's ends line: "Semester 2, 2026 ends on 2026-11-30."; in December, "Semester 2, 2026 counts encounters
    /// observed in December." (D40, C11).
    /// </summary>
    public static string EndsLine(TraineeCurriculumProgressSummaryDto summary)
    {
        ArgumentNullException.ThrowIfNull(summary);
        return summary.IsAfterTeachingYear
            ? $"{summary.CurrentSemesterName} counts encounters observed in December."
            : $"{summary.CurrentSemesterName} ends on {QuotaText.Iso(summary.CurrentSemesterNominalEnd)}.";
    }

    /// <summary>
    /// "Training year 4 — it sets the minimum level each encounter is judged against.": Home's and This period's line
    /// (T306), the built sentence with its full stop.
    /// </summary>
    public static string TrainingYearLine(int stage)
        => $"Training year {stage} — it sets the minimum level each encounter is judged against.";

    /// <summary>My progress's subtitle: "Training year 4 · Semester 2, 2026"; the semester alone before the programme starts.</summary>
    public static string Subtitle(TraineeCurriculumProgressSummaryDto summary)
    {
        ArgumentNullException.ThrowIfNull(summary);
        return summary.TraineeStage is { } stage
            ? $"Training year {stage} · {summary.CurrentSemesterName}"
            : summary.CurrentSemesterName;
    }

    /// <summary>
    /// The December Alert (as built, dates ISO): "The 2026 academic year ended on 2026-11-30. Encounters observed in
    /// December still count towards semester 2, 2026 and your 2026 yearly targets. Semester 1, 2027 starts on
    /// 2027-01-01." Null outside December, and on an ended programme's record.
    /// </summary>
    public static string? DecemberNotice(TraineeCurriculumProgressSummaryDto summary)
    {
        ArgumentNullException.ThrowIfNull(summary);
        if (!summary.IsAfterTeachingYear || summary.Ended is not null)
        {
            return null;
        }

        var year = summary.AsOf.Year;
        return $"The {year} academic year ended on {QuotaText.Iso(new AcademicPeriod(year, 2).NominalEnd)}. " +
               $"Encounters observed in December still count towards semester 2, {year} and your {year} yearly targets. " +
               $"Semester 1, {year + 1} starts on {QuotaText.Iso(new AcademicPeriod(year + 1, 1).Start)}.";
    }

    /// <summary>
    /// The College's D14, per kind (as built, <c>MyProgress.razor</c>, dates ISO; C2): the not-started Alert, "Your
    /// programme starts on 2027-01-15. Your first semester targets are for semester 1, 2027. …", or the part-way one,
    /// "You started the programme on 2026-08-18. That was part-way through the semester, …". The reader sets a start only
    /// for a kind the curriculum holds and does not apply today, and D42 usually waives only one of the two kinds, so each
    /// sentence speaks for its own kind. Null when neither kind is waived, and on an ended programme's record.
    /// </summary>
    public static string? StartNotice(TraineeCurriculumProgressSummaryDto summary)
    {
        ArgumentNullException.ThrowIfNull(summary);
        if (summary.Ended is not null || (summary.SemesterTargetsStart is null && summary.YearTargetsStart is null))
        {
            return null;
        }

        var sentences = new List<string>();
        if (summary.ProgrammeNotStarted)
        {
            sentences.Add($"Your programme starts on {QuotaText.Iso(summary.ProgrammeStartDate)}.");
            if (summary.SemesterTargetsStart is { } firstSemester)
            {
                sentences.Add($"Your first semester targets are for {firstSemester.Name}.");
            }

            if (summary.YearTargetsStart is { } firstYear)
            {
                sentences.Add($"Your first yearly targets are for {firstYear.Name}.");
            }

            return string.Join(" ", sentences);
        }

        sentences.Add($"You started the programme on {QuotaText.Iso(summary.ProgrammeStartDate)}.");
        if (summary.SemesterTargetsStart is { } semesterStart)
        {
            sentences.Add(
                $"That was part-way through the semester, so under the College's rule no semester target applies until {semesterStart.Name}, " +
                $"which starts on {QuotaText.Iso(semesterStart.StartsOn)}. Encounters on those EPAs before then stay in your portfolio as " +
                "evidence, but do not count towards a later semester's target.");
        }

        if (summary.YearTargetsStart is { } yearStart)
        {
            sentences.Add(
                $"That was in the second half of the academic year, so under the College's rule no yearly target applies until {yearStart.Name}, " +
                $"which starts on {QuotaText.Iso(yearStart.StartsOn)}. Encounters on those EPAs before then stay in your portfolio as " +
                "evidence, but do not count towards a later year's target.");
        }

        if (summary.SemesterTargetsStart is null && summary.HasSemesterItems)
        {
            sentences.Add("Your semester targets apply now.");
        }

        if (summary.YearTargetsStart is null && summary.HasYearItems)
        {
            sentences.Add("Your yearly targets apply now.");
        }

        return string.Join(" ", sentences);
    }

    /// <summary>The index's group caption: "Each semester · 10 EPAs", "Once a year · 7 EPAs" (C7).</summary>
    public static string GroupCaption(QuotaPeriod kind, int count)
        => $"{(kind == QuotaPeriod.Semester ? "Each semester" : "Once a year")} · {Epas(count)}";

    /// <summary>The paused group's caption: "No longer in use · 1 EPA" (Spec § 6).</summary>
    public static string PausedCaption(int count) => $"No longer in use · {Epas(count)}";

    /// <summary>The window column's heading, the window's name: "Semester 2, 2026", "2026 academic year".</summary>
    public static string WindowHeading(TraineeCurriculumProgressDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return item.Current.Name;
    }

    /// <summary>
    /// When a committee decides the EPA, in Q4's words, which name their own frame (notes 8, 9): "Decided each semester",
    /// "Decided once a year", "Decided as opportunity allows"; null where the item has no cadence (KGK-001), which the
    /// index shows as a dash with <see cref="NoCadence" /> beside it for a screen reader. So PAED-003, observed each
    /// semester, reads "Decided once a year" inside the "Each semester" group.
    /// </summary>
    public static string? Cadence(TraineeCurriculumProgressDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return item.DecisionCadence switch
        {
            null => null,
            _ when item.DecisionIsOpportunistic => "Decided as opportunity allows",
            QuotaPeriod.Semester => "Decided each semester",
            _ => "Decided once a year"
        };
    }

    /// <summary>A local item's badge: "Kgosi Kgari Teaching Hospital's own"; null for the College's item.</summary>
    public static string? LocalBadge(TraineeCurriculumProgressDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return item.OwningInstitutionName is { } owner ? $"{owner}'s own" : null;
    }

    /// <summary>"Semester 2, 2026" or "the 2026 academic year": a window inside a sentence (C3, C11).</summary>
    private static string InSentence(TraineeCurriculumProgressDto item)
        => item.IsPerSemester ? item.Current.Name : $"the {item.Current.Name}";

    private static string Epas(int count) => count == 1 ? "1 EPA" : $"{count} EPAs";
}
