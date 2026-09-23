using System.Globalization;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Curricula.Quota;

/// <summary>
/// The date "today" means for the quota: the calendar date in South Africa (T130). Taking it from UTC showed the
/// previous semester for the first two hours of 1 January and 1 July. Same calendar as the undated-encounter
/// fallback; see <see cref="ProgrammeCalendar" />.
/// </summary>
public static class QuotaCalendar
{
    public static DateOnly Today() => ProgrammeCalendar.DateOf(DateTime.UtcNow);
}

/// <summary>One stored semester bucket, reduced to what the quota reads.</summary>
public sealed record QuotaProgressRow(
    int CurriculumItemId,
    int AcademicYear,
    int Semester,
    int CountsSoFar,
    int MinimumLevelReachedCount,
    DateOnly? LastObservedOn);

/// <summary>What a trainee has, against what is asked, in one window.</summary>
/// <param name="Target">The item's <see cref="CurriculumItem.RequiredCount" />, which is a target per window (D18).</param>
/// <param name="Count">Credited encounters in the window's semesters. It is shown even when the target is waived.</param>
public sealed record QuotaWindowTally(
    QuotaWindow Window,
    int Target,
    int Count,
    int MinimumLevelReachedCount,
    DateOnly? LastObservedOn)
{
    /// <summary>The target applies: the trainee is neither exempt (D14) nor not yet started.</summary>
    public bool Applies => Window.Status == QuotaWindowStatus.Counting;

    public bool IsMet => Applies && Count >= Target;

    public int Shortfall => Applies ? Math.Max(0, Target - Count) : 0;

    /// <summary>For a progress bar: 0 to 100, capped. Zero when the target does not apply.</summary>
    public int PercentOfTarget => !Applies
        ? 0
        : Target <= 0 ? 100 : Math.Min(100, Count * 100 / Target);
}

/// <summary>An item's current window and the one before it, so a closed period's result survives the boundary.</summary>
public sealed record ItemQuotaProgress(QuotaWindowTally Current, QuotaWindowTally? Previous);

/// <summary>
/// The one place a curriculum item's stored semester buckets are read against its target (T130).
/// </summary>
/// <remarks>
/// Pure: dates in, figures out. Every progress reader goes through it: the trainee's progress page, the
/// trainee dashboard, and the committee and coverage dashboards. So a committee member sees the number the
/// trainee sees. Before T130 the same arithmetic was written five times, in three different ways.
/// </remarks>
public static class QuotaProgressCalculator
{
    public static ItemQuotaProgress For(
        int curriculumItemId,
        QuotaPeriod quotaPeriod,
        int requiredCount,
        IEnumerable<QuotaProgressRow> rows,
        DateOnly programmeStart,
        DateOnly asOf)
    {
        var itemRows = rows.Where(row => row.CurriculumItemId == curriculumItemId).ToList();

        var window = QuotaWindow.For(quotaPeriod, asOf, programmeStart);
        var current = Tally(window, requiredCount, itemRows);

        var preceding = window.Preceding(programmeStart);
        var previous = preceding is null ? null : Tally(preceding, requiredCount, itemRows);

        return new ItemQuotaProgress(current, previous);
    }

    private static QuotaWindowTally Tally(QuotaWindow window, int target, IReadOnlyList<QuotaProgressRow> rows)
    {
        var covered = rows.Where(row => window.Covers(row.AcademicYear, row.Semester)).ToList();

        return new QuotaWindowTally(
            window,
            target,
            covered.Sum(row => row.CountsSoFar),
            covered.Sum(row => row.MinimumLevelReachedCount),
            covered.Max(row => row.LastObservedOn));
    }
}

/// <summary>
/// How a window is named to a reader. The College's calendar runs January to November, so semester 2 is
/// "July to November" even though December encounters are counted in it (D40).
/// </summary>
public static class QuotaText
{
    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-GB");

    /// <summary>"Semester 2, 2026" or "2026 academic year".</summary>
    public static string WindowName(QuotaWindow window)
        => window.Kind == QuotaPeriod.Semester
            ? SemesterName(window.Semesters[0])
            : $"{window.AcademicYear} academic year";

    /// <summary>"Semester 2, 2026".</summary>
    public static string SemesterName(AcademicPeriod period) => $"Semester {period.Semester}, {period.Year}";

    /// <summary>"July to November": the window's months on the College's calendar.</summary>
    public static string Months(QuotaWindow window) => MonthRange(window.Start, window.NominalEnd);

    /// <summary>"January to June".</summary>
    public static string Months(AcademicPeriod period) => MonthRange(period.Start, period.NominalEnd);

    /// <summary>The first window a trainee is held to: "semester 1, 2027" or "the 2027 academic year".</summary>
    public static string? FirstCountedName(QuotaWindow window)
        => window.FirstCountedPeriod is not { } first
            ? null
            : window.Kind == QuotaPeriod.Semester
                ? $"semester {first.Semester}, {first.Year}"
                : $"the {first.Year} academic year";

    /// <summary>"January to June, July to November": the two semesters on the College's calendar.</summary>
    public static string SemesterSplit(int year)
        => $"{Months(new AcademicPeriod(year, 1))}, {Months(new AcademicPeriod(year, 2))}";

    /// <summary>"January to November": the academic year on the College's calendar.</summary>
    public static string AcademicYearMonths(int year)
        => MonthRange(new AcademicPeriod(year, 1).Start, new AcademicPeriod(year, 2).NominalEnd);

    /// <summary>"23 September 2026", in English whatever the server's culture, like every other quota label.</summary>
    public static string LongDate(DateOnly date) => date.ToString("d MMMM yyyy", English);

    /// <summary>"23 Sep 2026".</summary>
    public static string ShortDate(DateOnly date) => date.ToString("d MMM yyyy", English);

    /// <summary>"three per semester" / "one per academic year".</summary>
    public static string TargetPhrase(QuotaPeriod kind, int target)
        => kind == QuotaPeriod.Semester ? $"{target} per semester" : $"{target} per academic year";

    private static string MonthRange(DateOnly start, DateOnly end)
        => $"{start.ToString("MMMM", English)} to {end.ToString("MMMM", English)}";
}
