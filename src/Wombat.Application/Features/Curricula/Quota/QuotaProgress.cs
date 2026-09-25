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
    public static DateOnly Today() => Today(TimeProvider.System);

    /// <summary>Today on the South African calendar, by <paramref name="clock" />, so a test can pin 22:30 UTC.</summary>
    public static DateOnly Today(TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return ProgrammeCalendar.DateOf(clock.GetUtcNow().UtcDateTime);
    }
}

/// <summary>One stored semester bucket, reduced to what the quota reads.</summary>
/// <param name="LastObservedOnDeclared">Whether <paramref name="LastObservedOn" /> was stated (T219).</param>
public sealed record QuotaProgressRow(
    int CurriculumItemId,
    int AcademicYear,
    int Semester,
    int CountsSoFar,
    int MinimumLevelReachedCount,
    DateOnly? LastObservedOn,
    bool LastObservedOnDeclared);

/// <summary>What a trainee has, against what is asked, in one window.</summary>
/// <param name="Target">The item's <see cref="CurriculumItem.RequiredCount" />, which is a target per window (D18).</param>
/// <param name="Count">Credited encounters in the window's semesters. It is shown even when the target is waived.</param>
/// <param name="LastObservedOn">The latest encounter credited in the window's semesters.</param>
/// <param name="LastObservedOnDeclared">
/// Whether somebody stated that date, rather than it being the day a form was created (T219). False when
/// <paramref name="LastObservedOn" /> is null.
/// </param>
public sealed record QuotaWindowTally(
    QuotaWindow Window,
    int Target,
    int Count,
    int MinimumLevelReachedCount,
    DateOnly? LastObservedOn,
    bool LastObservedOnDeclared)
{
    /// <summary>
    /// The target applies: the trainee is not exempt (D14 at the start, D49 at the end), not yet started, nor past the
    /// programme's end.
    /// </summary>
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
/// <para>
/// Pure: dates in, figures out. Every progress reader goes through it: the trainee's progress page, the
/// trainee dashboard, the committee and coverage dashboards, and the portfolio export. So a committee member sees the
/// number the trainee sees. Before T130 the same arithmetic was written five times, in three different ways.
/// </para>
/// <para>
/// <b>Which windows hold a target</b> is <see cref="QuotaWindow.For" />'s, with both ends of the programme: D14 and D42
/// at the start, D49 at the end (T209). Every reader passes the programme's actual end (<c>TraineeProfile.EndedOn</c>),
/// which is null while it runs, so no reader can hold a graduate's cut-short last period, or a period after they left,
/// to a target.
/// </para>
/// </remarks>
public static class QuotaProgressCalculator
{
    /// <param name="programmeEnd">The day the programme actually ended (<c>TraineeProfile.EndedOn</c>), or null while it runs.</param>
    public static ItemQuotaProgress For(
        int curriculumItemId,
        QuotaPeriod quotaPeriod,
        int requiredCount,
        IEnumerable<QuotaProgressRow> rows,
        DateOnly programmeStart,
        DateOnly? programmeEnd,
        DateOnly asOf)
    {
        var itemRows = rows.Where(row => row.CurriculumItemId == curriculumItemId).ToList();

        var window = QuotaWindow.For(quotaPeriod, asOf, programmeStart, programmeEnd);
        var current = Tally(window, requiredCount, itemRows);

        var preceding = window.Preceding(programmeStart, programmeEnd);
        var previous = preceding is null ? null : Tally(preceding, requiredCount, itemRows);

        return new ItemQuotaProgress(current, previous);
    }

    /// <summary>
    /// Every window of the item's kind from the one containing <paramref name="asOf" /> back to the one containing
    /// <paramref name="from" />, newest first: what a report covering a span, not a day, prints (the portfolio export,
    /// T169).
    /// </summary>
    /// <remarks>
    /// The first window is <see cref="For" />'s <c>Current</c> and the second its <c>Previous</c>, tallied by the same
    /// code, so a span ending today opens with exactly what the progress page shows. A window that lies wholly before
    /// the programme start is not listed, just as the progress page drops a previous window the trainee had not
    /// started, and the walk stops there. The window containing <paramref name="asOf" /> is listed unless it ended
    /// before <paramref name="from" />: a span that starts after <paramref name="asOf" />'s window has no window to
    /// report, and listing that one would print a period the span does not cover.
    /// </remarks>
    public static IReadOnlyList<QuotaWindowTally> Since(
        int curriculumItemId,
        QuotaPeriod quotaPeriod,
        int requiredCount,
        IEnumerable<QuotaProgressRow> rows,
        DateOnly programmeStart,
        DateOnly? programmeEnd,
        DateOnly from,
        DateOnly asOf)
    {
        var itemRows = rows.Where(row => row.CurriculumItemId == curriculumItemId).ToList();

        var window = QuotaWindow.For(quotaPeriod, asOf, programmeStart, programmeEnd);
        if (window.End < from)
        {
            return [];
        }

        var tallies = new List<QuotaWindowTally> { Tally(window, requiredCount, itemRows) };

        for (var preceding = window.Preceding(programmeStart, programmeEnd);
             preceding is not null && preceding.End >= from && preceding.Status != QuotaWindowStatus.NotStarted;
             preceding = preceding.Preceding(programmeStart, programmeEnd))
        {
            tallies.Add(Tally(preceding, requiredCount, itemRows));
        }

        return tallies;
    }

    private static QuotaWindowTally Tally(QuotaWindow window, int target, IReadOnlyList<QuotaProgressRow> rows)
    {
        var covered = rows.Where(row => window.Covers(row.AcademicYear, row.Semester)).ToList();

        // An academic year is two semester rows. Its last encounter is the later of theirs, and on a tie it is stated
        // if either row's is: the rule CurriculumItemProgress.NoteEncounter applies within one row (T219).
        var lastObservedOn = covered.Max(row => row.LastObservedOn);
        var lastObservedOnDeclared = lastObservedOn is { } last &&
                                     covered.Any(row => row.LastObservedOn == last && row.LastObservedOnDeclared);

        return new QuotaWindowTally(
            window,
            target,
            covered.Sum(row => row.CountsSoFar),
            covered.Sum(row => row.MinimumLevelReachedCount),
            lastObservedOn,
            lastObservedOnDeclared);
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

    /// <summary>
    /// The first window a trainee is held to: "semester 1, 2027" or "the 2027 academic year". Null when there is none
    /// (<see cref="QuotaWindow.FirstCountedPeriodFor" />).
    /// </summary>
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

    /// <summary>
    /// How the trainee's own programme ended, as a clause with no full stop, for each surface that says so to them (T252):
    /// "You completed your programme on 30 June 2026", "Your programme ended on 20 August 2026", or, for a profile ended
    /// before Wombat recorded the day (T209), "Your programme has ended". One sentence, so My progress, its standing panel
    /// and the trainee dashboard cannot word the same end differently.
    /// </summary>
    public static string ProgrammeEnded(ProgrammeEndDto ended)
    {
        ArgumentNullException.ThrowIfNull(ended);

        return ended switch
        {
            { EndedOn: { } on, Completed: true } => $"You completed your programme on {LongDate(on)}",
            { EndedOn: { } on } => $"Your programme ended on {LongDate(on)}",
            _ => "Your programme has ended"
        };
    }

    private static string MonthRange(DateOnly start, DateOnly end)
        => $"{start.ToString("MMMM", English)} to {end.ToString("MMMM", English)}";
}
