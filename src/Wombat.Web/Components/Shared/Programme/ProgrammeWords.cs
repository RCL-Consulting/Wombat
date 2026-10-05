using System.Globalization;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Programme;
using Wombat.Application.Features.Programme.Filing;
using Wombat.Application.Features.Programme.Trainees;
using Wombat.Domain.Identity;

namespace Wombat.Web.Components.Shared.Programme;

/// <summary>
/// The words of the programme's registrars (T358, flow 06, lane A2; R2-Trainees, R2-Home): Programme trainees' subtitle,
/// figures, headings, rule lines, captions, filters and states, and Home's Registrars and Nothing filed cards. One class,
/// so the page and the cards say a registrar's figures the same way, and the same way My progress does (flow 05's
/// <c>ProgressWords</c>).
/// </summary>
/// <remarks>
/// Dates ISO (T325; flow 05's D1). A figure is "n of m" over its window, never "n / m", a percentage or a lifetime total;
/// a training year is "Training year 3", never the bare "year" (Q10). Every count has its singular. No pronoun names a
/// person (round-3-check 1).
/// </remarks>
public static class ProgrammeWords
{
    // ─── The page ───────────────────────────────────────────────────────────

    /// <summary>Programme trainees' title and h1.</summary>
    public const string PageTitle = "Programme trainees";

    /// <summary>The list's heading while nothing is filtered and nothing is counted yet (loading, empty).</summary>
    public const string ListTitle = "Current registrars";

    /// <summary>
    /// The page's subtitle (E4: what was read, as which role): "Current registrars at Kgosi Kgari Teaching Hospital, read
    /// as Committee member · Semester 2, 2026"; "Current registrars in Paediatrics, read as Sub-speciality admin · …".
    /// </summary>
    public static string Subtitle(ProgrammeScopeDto scope, string semester)
    {
        ArgumentNullException.ThrowIfNull(scope);
        return $"Current registrars {ScopePhrase(scope)}, read as {WombatRoleLabels.For(scope.ActingRole)} · {semester}";
    }

    /// <summary>"at Kgosi Kgari Teaching Hospital" for the institution, "in Paediatrics" for a speciality or sub-speciality.</summary>
    public static string ScopePhrase(ProgrammeScopeDto scope)
    {
        ArgumentNullException.ThrowIfNull(scope);
        return scope.Kind == ProgrammeScopeKind.Institution ? $"at {scope.Name}" : $"in {scope.Name}";
    }

    /// <summary>The semester figure and its caption, My progress's: ("0 of 10", "EPAs met this semester").</summary>
    public static (string Value, string Label) SemesterFigure(ProgrammeTraineeRowDto row)
    {
        ArgumentNullException.ThrowIfNull(row);
        return row.SemesterApplying == 0
            ? ("none apply yet", "Semester targets")
            : ($"{row.SemesterMet} of {row.SemesterApplying}", "EPAs met this semester");
    }

    /// <summary>The yearly figure and its caption: ("0 of 5", "EPAs met in 2026").</summary>
    public static (string Value, string Label) YearFigure(ProgrammeTraineeRowDto row)
    {
        ArgumentNullException.ThrowIfNull(row);
        return row.YearApplying == 0
            ? ("none apply yet", "Yearly targets")
            : ($"{row.YearMet} of {row.YearApplying}", $"EPAs met in {row.AcademicYear.ToString(CultureInfo.InvariantCulture)}");
    }

    /// <summary>The yearly column's header: "In 2026".</summary>
    public static string YearColumn(ProgrammeRosterRead read)
    {
        ArgumentNullException.ThrowIfNull(read);
        return $"In {read.CurrentSemesterEnd.Year.ToString(CultureInfo.InvariantCulture)}";
    }

    /// <summary>"Training year 3"; before the programme starts, "Programme not started".</summary>
    public static string TrainingYear(ProgrammeTraineeRowDto row)
    {
        ArgumentNullException.ThrowIfNull(row);
        return row.TrainingYear is int year
            ? $"Training year {year.ToString(CultureInfo.InvariantCulture)}"
            : NotStarted;
    }

    /// <summary>The Training year cell: "3"; before the programme starts, "Not started".</summary>
    public static string TrainingYearCell(ProgrammeTraineeRowDto row)
    {
        ArgumentNullException.ThrowIfNull(row);
        return row.TrainingYear is int year ? year.ToString(CultureInfo.InvariantCulture) : "Not started";
    }

    private const string NotStarted = "Programme not started";

    /// <summary>The exempt row's badge (c6, t7).</summary>
    public const string Exempt = "Exempt this period";

    /// <summary>The exempt cell's data-label, where three figure cells give way to one (t7).</summary>
    public const string ExemptCellLabel = "This period";

    /// <summary>Why a registrar is exempt, in words (review 9): "Started part-way through the period" / "Starts on 2027-01-15".</summary>
    public static string ExemptReason(ProgrammeTraineeRowDto row)
    {
        ArgumentNullException.ThrowIfNull(row);
        return row.Exemption is { } exemption ? ExemptReason(exemption, row.ProgrammeStartDate) : string.Empty;
    }

    /// <inheritdoc cref="ExemptReason(ProgrammeTraineeRowDto)" />
    public static string ExemptReason(ProgrammeExemption exemption, DateOnly programmeStart) => exemption switch
    {
        ProgrammeExemption.NotStarted => $"Starts on {QuotaText.Iso(programmeStart)}",
        _ => "Started part-way through the period"
    };

    /// <summary>
    /// Furthest short's codes (D11): "PAED-001, PAED-002, PAED-003"; "Every target met" when nothing applying is short.
    /// </summary>
    public static string FurthestShortCodes(ProgrammeTraineeRowDto row)
    {
        ArgumentNullException.ThrowIfNull(row);
        return row.FurthestShort.Count == 0
            ? EveryTargetMet
            : string.Join(", ", row.FurthestShort.Select(epa => epa.EpaCode));
    }

    /// <summary>A registrar whose every applying target is met (D11).</summary>
    public const string EveryTargetMet = "Every target met";

    /// <summary>
    /// Furthest short's line under the codes (D11): "0 of 3 each this semester" when the three share one figure and window;
    /// else each "PAED-008: 0 of 1 in 2026", joined by " · ". Empty when nothing is short.
    /// </summary>
    public static string FurthestShortLine(ProgrammeTraineeRowDto row)
    {
        ArgumentNullException.ThrowIfNull(row);
        var shortfalls = row.FurthestShort;
        if (shortfalls.Count == 0)
        {
            return string.Empty;
        }

        var first = shortfalls[0];
        if (shortfalls.Count == 1)
        {
            return Figure(first);
        }

        return shortfalls.All(epa => epa.Count == first.Count && epa.Target == first.Target && epa.QuotaPeriod == first.QuotaPeriod)
            ? $"{first.Count} of {first.Target} each {WindowWords(first)}"
            : string.Join(" · ", shortfalls.Select(epa => $"{epa.EpaCode}: {Figure(epa)}"));
    }

    /// <summary>
    /// The Short on column's cell (t2): ("1 of 3 this semester", "2 more by 2026-11-30"); empty when the registrar is not short
    /// on the EPA asked.
    /// </summary>
    public static (string Value, string Meta) ShortOnCell(ProgrammeTraineeRowDto row)
    {
        ArgumentNullException.ThrowIfNull(row);
        return row.ShortOn is { } epa
            ? (Figure(epa), $"{epa.Short.ToString(CultureInfo.InvariantCulture)} more by {QuotaText.Iso(epa.WindowEnd)}")
            : (string.Empty, string.Empty);
    }

    /// <summary>"2026-09-12", or "Nothing filed yet".</summary>
    public static string LastFiled(ProgrammeTraineeRowDto row)
    {
        ArgumentNullException.ThrowIfNull(row);
        return row.LastFiledOn is { } on ? QuotaText.Iso(on) : NothingFiledYet;
    }

    private const string NothingFiledYet = "Nothing filed yet";

    /// <summary>The roster's order, under Home's Registrars card and over the list (c1, t1).</summary>
    public const string RosterLine = "Fewest met first, then by surname.";

    /// <summary>
    /// The list's heading, which counts the answer (D9): "5 current registrars"; one filter in its own words, "5 of 5 current
    /// registrars are short on PAED-002", "1 of 5 current registrars has filed nothing in 30 days", "2 of 5 current
    /// registrars are in training year 4"; several, "2 of 5 current registrars match these filters"; none matching, "0 of 5
    /// current registrars".
    /// </summary>
    public static string ListHeading(ProgrammeRosterRead read, ProgrammeTraineesFilter filter, int matchCount)
    {
        ArgumentNullException.ThrowIfNull(read);
        ArgumentNullException.ThrowIfNull(filter);

        var registrars = read.CurrentCount == 1 ? "current registrar" : "current registrars";
        if (!filter.IsFiltered)
        {
            return $"{Count(read.CurrentCount)} {registrars}";
        }

        var counted = $"{Count(matchCount)} of {Count(read.CurrentCount)} {registrars}";
        if (matchCount == 0)
        {
            return counted;
        }

        var one = matchCount == 1;
        if (filter.Count > 1)
        {
            return $"{counted} {(one ? "matches" : "match")} these filters";
        }

        if (filter.ShortOnEpaId is not null)
        {
            return $"{counted} {(one ? "is" : "are")} short on {EpaCodeAsked(read)}";
        }

        if (filter.TrainingYear is int year)
        {
            return $"{counted} {(one ? "is" : "are")} in training year {year.ToString(CultureInfo.InvariantCulture)}";
        }

        return $"{counted} {(one ? "has" : "have")} filed nothing in {FilingMoments.WindowDays} days";
    }

    /// <summary>
    /// The rule line under the heading (D9, t1–t7): the order, and what the figures are read against. A Short on list names
    /// the EPA and its target; Nothing filed says what counts; several filters say what was asked first.
    /// </summary>
    public static string ListRule(ProgrammeRosterRead read, ProgrammeTraineesFilter filter)
    {
        ArgumentNullException.ThrowIfNull(read);
        ArgumentNullException.ThrowIfNull(filter);

        var asked = filter.Count > 1 ? $"{Asked(filter, read.ShortOn)} " : string.Empty;

        if (filter.ShortOnEpaId is not null)
        {
            var epa = read.ShortOn is { } option
                ? $"{option.EpaCode} — {option.EpaTitle}, {QuotaText.TargetPhrase(option.QuotaPeriod, option.Target)}. "
                : string.Empty;
            return $"{asked}{epa}Furthest from its target first, then fewest EPAs met, then by surname.";
        }

        if (filter.NothingFiled)
        {
            return $"{asked}{FiledWhat} Longest without first.";
        }

        return read.ExemptCount > 0
            ? $"{asked}{RosterLine} {ExemptCount(read.ExemptCount)} exempt this period, not counted, listed last."
            : $"{asked}{RosterLine} {read.CurrentSemesterName} ends on {QuotaText.Iso(read.CurrentSemesterEnd)}.";
    }

    // The window is the last 30 days, and a registrar admitted less than 30 days ago is not listed at all (E5): "or since
    // admission if that is later" read as a window from admission (T358, build review G1).
    private const string FiledWhat =
        "Nothing filed (a draft is not filed) in the last 30 days; a recorded MSF counts. A registrar admitted less than 30 " +
        "days ago is not listed.";

    /// <summary>"1 registrar", "2 registrars": the one exemption wording's count (Spec § Dashboard layout grid).</summary>
    public static string ExemptCount(int exempt) => exempt == 1 ? "1 registrar" : $"{Count(exempt)} registrars";

    /// <summary>
    /// The table's caption (round 3 item 33): "Current registrars, fewest EPAs met first"; with an exempt registrar,
    /// "…, one exempt this period"; "Current registrars short on PAED-002, furthest from its target first"; "Current
    /// registrars with nothing filed in 30 days"; "Current registrars in training year 4, fewest EPAs met first"; several
    /// filters, "Current registrars matching these filters".
    /// </summary>
    public static string Caption(ProgrammeTraineesFilter filter, EpaFilterOptionDto? epa, int exemptCount = 0)
    {
        ArgumentNullException.ThrowIfNull(filter);

        if (filter.Count > 1)
        {
            return "Current registrars matching these filters";
        }

        if (filter.ShortOnEpaId is not null)
        {
            return $"Current registrars short on {epa?.EpaCode ?? "the EPA asked"}, furthest from its target first";
        }

        if (filter.NothingFiled)
        {
            return $"Current registrars with nothing filed in {FilingMoments.WindowDays} days";
        }

        var year = filter.TrainingYear is int trainingYear
            ? $" in training year {trainingYear.ToString(CultureInfo.InvariantCulture)}"
            : string.Empty;
        var exempt = exemptCount switch
        {
            0 => string.Empty,
            1 => ", one exempt this period",
            _ => $", {Count(exemptCount)} exempt this period"
        };
        return $"Current registrars{year}, fewest EPAs met first{exempt}";
    }

    /// <summary>One no-match pattern for both lists (round 3 item 33).</summary>
    public const string NoMatch = "No registrar matches these filters.";

    /// <summary>
    /// What was asked, under the no-match sentence and in a several-filter rule line: "Short on PAED-001, training year 4,
    /// nothing filed in 30 days."
    /// </summary>
    public static string Asked(ProgrammeTraineesFilter filter, EpaFilterOptionDto? epa)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var parts = new List<string>(3);
        if (filter.ShortOnEpaId is not null)
        {
            parts.Add($"short on {epa?.EpaCode ?? "the EPA asked"}");
        }

        if (filter.TrainingYear is int year)
        {
            parts.Add($"training year {year.ToString(CultureInfo.InvariantCulture)}");
        }

        if (filter.NothingFiled)
        {
            parts.Add($"nothing filed in {FilingMoments.WindowDays} days");
        }

        if (parts.Count == 0)
        {
            return string.Empty;
        }

        var sentence = string.Join(", ", parts) + ".";
        return char.ToUpperInvariant(sentence[0]) + sentence[1..];
    }

    /// <summary>The empty state's title: no current registrar in the scope (t5), where no filter form is drawn.</summary>
    public const string EmptyTitle = "No current registrars";

    /// <summary>The empty state's line.</summary>
    public const string EmptyBody = "They appear here once they are admitted to the programme.";

    /// <summary>The page's status while it loads.</summary>
    public const string Loading = "Loading Programme trainees.";

    /// <summary>The page's load error (t9): fixed words, never the exception (T329).</summary>
    public const string LoadFailed =
        "Could not load Programme trainees. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>The load error's strong part, before "Nothing has changed.".</summary>
    public const string LoadFailedLead = "Could not load Programme trainees.";

    /// <summary>The load error's rest.</summary>
    public const string LoadFailedRest = "Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>The form's name (<c>aria-label</c>).</summary>
    public const string FilterFormName = "Filter Programme trainees";

    public const string ShortOnLabel = "Short on";
    public const string AnyEpa = "Any EPA";
    public const string YearLabel = "Training year";
    public const string AnyYear = "Any";
    public const string FiledLabel = "Nothing filed in 30 days";
    public const string Show = "Show";
    public const string ClearFilters = "Clear filters";

    /// <summary>A Short on option: "PAED-002 — Managing common paediatric presentations".</summary>
    public static string EpaOption(EpaFilterOptionDto epa)
    {
        ArgumentNullException.ThrowIfNull(epa);
        return $"{epa.EpaCode} — {epa.EpaTitle}";
    }

    /// <summary>The pager's name.</summary>
    public const string PagerName = "Current registrars, pages";

    // ─── Home ───────────────────────────────────────────────────────────────

    /// <summary>The Registrars card's title (round 3 item 33).</summary>
    public const string RegistrarsCard = "Registrars";

    /// <summary>The Nothing filed card's title.</summary>
    public const string NothingFiledCard = "Nothing filed in 30 days";

    /// <summary>A count badge in words (review 31): "5 registrars", "1 registrar".</summary>
    public static string Badge(int count) => count == 1 ? "1 registrar" : $"{Count(count)} registrars";

    /// <summary>Past five rows (c11): "3 more in Programme trainees.", "1 more in Programme trainees.".</summary>
    public static string More(int beyond) => $"{Count(beyond)} more in Programme trainees.";

    /// <summary>The Registrars card with nobody to show (c7); the foot stays.</summary>
    public const string RosterEmpty = "No current registrars. They appear here once they are admitted to the programme.";

    /// <summary>The Registrars card's foot.</summary>
    public const string OpenTrainees = "Open Programme trainees";

    /// <summary>
    /// The Nothing filed card's rule line (E5; k1–k6): the last 30 days, and nobody admitted since (T358, build review G1).
    /// </summary>
    public const string FiledRule =
        "Current registrars with nothing filed (a draft is not filed) in the last 30 days. A registrar admitted less than 30 " +
        "days ago is not listed.";

    /// <summary>A Nothing filed row's meta (k4): "Last filed 2026-09-12 · Training year 2"; "Nothing filed yet · Training year 2".</summary>
    public static string FiledRowMeta(ProgrammeTraineeRowDto row)
    {
        ArgumentNullException.ThrowIfNull(row);
        var filed = row.LastFiledOn is { } on ? $"Last filed {QuotaText.Iso(on)}" : NothingFiledYet;
        return $"{filed} · {TrainingYear(row)}";
    }

    /// <summary>The Nothing filed card with nobody listed (k1, k2); its foot stays.</summary>
    public const string FiledEmpty = "Every current registrar has filed something in the last 30 days.";

    /// <summary>The Nothing filed card's foot.</summary>
    public const string OpenFiled = "Open in Programme trainees";

    // ─── Helpers ────────────────────────────────────────────────────────────

    /// <summary>"1 of 3 this semester", "0 of 1 in 2026": a shortfall's figure over its window.</summary>
    public static string Figure(EpaShortfallDto epa)
    {
        ArgumentNullException.ThrowIfNull(epa);
        return $"{epa.Count} of {epa.Target} {WindowWords(epa)}";
    }

    private static string WindowWords(EpaShortfallDto epa)
        => epa.IsPerSemester ? "this semester" : $"in {epa.AcademicYear.ToString(CultureInfo.InvariantCulture)}";

    private static string EpaCodeAsked(ProgrammeRosterRead read) => read.ShortOn?.EpaCode ?? "the EPA asked";

    private static string Count(int value) => value.ToString(CultureInfo.InvariantCulture);
}
