using Wombat.Domain.Curricula;

namespace Wombat.Domain.Tests.Curricula;

/// <summary>
/// D49 (T209, provisional): the mirror of D14 at the programme's end. A period that completion or deactivation cuts short
/// before its last month is exempt, as a late start's first period is. One that ends in its last month, or later, holds its
/// full target. Periods after the end are outside the programme, never short.
/// </summary>
/// <remarks>
/// The trainee started on 1 January 2025, so every 2026 window would count without an end. The last month is June for
/// semester 1 and November for semester 2 and the academic year, on the College's calendar (D13, D40).
/// </remarks>
public sealed class QuotaWindowProgrammeEndTests
{
    private static readonly DateOnly Start = new(2025, 1, 1);

    [Theory]
    // Semester 1 (January to June): the first month, mid-period and the day before its last month are exempt.
    [InlineData("2026-03-31", "2026-01-10", QuotaWindowStatus.ExemptProgrammeEnded)]
    [InlineData("2026-03-31", "2026-03-14", QuotaWindowStatus.ExemptProgrammeEnded)]
    [InlineData("2026-03-31", "2026-05-31", QuotaWindowStatus.ExemptProgrammeEnded)]
    // The last month, from its first day, holds the full target.
    [InlineData("2026-03-31", "2026-06-01", QuotaWindowStatus.Counting)]
    [InlineData("2026-03-31", "2026-06-30", QuotaWindowStatus.Counting)]
    // Semester 2 (July to November, December folded in): an end on its first day is in its first month.
    [InlineData("2026-09-23", "2026-07-01", QuotaWindowStatus.ExemptProgrammeEnded)]
    [InlineData("2026-09-23", "2026-10-31", QuotaWindowStatus.ExemptProgrammeEnded)]
    [InlineData("2026-09-23", "2026-11-01", QuotaWindowStatus.Counting)]
    [InlineData("2026-09-23", "2026-11-30", QuotaWindowStatus.Counting)]
    // December is after the College's year: the semester was served in full.
    [InlineData("2026-09-23", "2026-12-15", QuotaWindowStatus.Counting)]
    // An end in the next semester leaves this one whole.
    [InlineData("2026-03-31", "2026-07-01", QuotaWindowStatus.Counting)]
    public void ASemesterTheProgrammeEndsIn_IsExemptUnlessItEndsInItsLastMonth(string day, string end, QuotaWindowStatus expected)
    {
        var window = QuotaWindow.For(QuotaPeriod.Semester, Day(day), Start, Day(end));

        Assert.Equal(expected, window.Status);
    }

    [Theory]
    // The academic year's last month is November, as D49 words it for every period: an end in semester 2 before November
    // still waives the year. (D42 is not mirrored here: it holds an April start to the year.)
    [InlineData("2026-01-10", QuotaWindowStatus.ExemptProgrammeEnded)]
    [InlineData("2026-06-30", QuotaWindowStatus.ExemptProgrammeEnded)]
    [InlineData("2026-10-31", QuotaWindowStatus.ExemptProgrammeEnded)]
    [InlineData("2026-11-01", QuotaWindowStatus.Counting)]
    [InlineData("2026-12-31", QuotaWindowStatus.Counting)]
    public void AnAcademicYearTheProgrammeEndsIn_IsExemptUnlessItEndsInNovemberOrLater(string end, QuotaWindowStatus expected)
    {
        var window = QuotaWindow.For(QuotaPeriod.AcademicYear, Day(end), Start, Day(end));

        Assert.Equal(expected, window.Status);
    }

    [Fact]
    public void WindowsAfterTheEndAreOutsideTheProgramme_WhateverDayIsAsked()
    {
        var end = new DateOnly(2026, 5, 15);

        Assert.Equal(QuotaWindowStatus.AfterProgrammeEnd, QuotaWindow.For(QuotaPeriod.Semester, new DateOnly(2026, 9, 23), Start, end).Status);
        Assert.Equal(QuotaWindowStatus.AfterProgrammeEnd, QuotaWindow.For(QuotaPeriod.Semester, new DateOnly(2027, 3, 1), Start, end).Status);
        Assert.Equal(QuotaWindowStatus.AfterProgrammeEnd, QuotaWindow.For(QuotaPeriod.AcademicYear, new DateOnly(2027, 3, 1), Start, end).Status);

        // The window the end falls in is judged by D49, even when the day asked about is after the end.
        Assert.Equal(QuotaWindowStatus.ExemptProgrammeEnded, QuotaWindow.For(QuotaPeriod.Semester, new DateOnly(2026, 6, 20), Start, end).Status);
        Assert.Equal(QuotaWindowStatus.ExemptProgrammeEnded, QuotaWindow.For(QuotaPeriod.AcademicYear, new DateOnly(2026, 9, 23), Start, end).Status);
    }

    [Fact]
    public void WindowsBeforeTheOneTheProgrammeEndedIn_StillCount()
    {
        var end = new DateOnly(2026, 5, 15);

        Assert.Equal(QuotaWindowStatus.Counting, QuotaWindow.For(QuotaPeriod.Semester, new DateOnly(2025, 9, 1), Start, end).Status);
        Assert.Equal(QuotaWindowStatus.Counting, QuotaWindow.For(QuotaPeriod.AcademicYear, new DateOnly(2025, 9, 1), Start, end).Status);
    }

    [Fact]
    public void WithNoEnd_TheRuleIsTheOneBeforeD49()
    {
        // A running programme: null is the default, and every caller that reads only active profiles relies on it.
        Assert.Equal(QuotaWindowStatus.Counting, QuotaWindow.For(QuotaPeriod.Semester, new DateOnly(2026, 3, 1), Start).Status);
        Assert.Equal(QuotaWindowStatus.Counting, QuotaWindow.For(QuotaPeriod.Semester, new DateOnly(2026, 3, 1), Start, programmeEnd: null).Status);
    }

    [Fact]
    public void PrecedingCarriesTheEnd()
    {
        var end = new DateOnly(2026, 5, 15);

        var afterward = QuotaWindow.For(QuotaPeriod.Semester, new DateOnly(2027, 3, 1), Start, end);
        var semester2 = afterward.Preceding(Start, end)!;
        var semester1 = semester2.Preceding(Start, end)!;
        var earlier = semester1.Preceding(Start, end)!;

        Assert.Equal((new AcademicPeriod(2026, 2), QuotaWindowStatus.AfterProgrammeEnd), (semester2.Semesters[0], semester2.Status));
        Assert.Equal((new AcademicPeriod(2026, 1), QuotaWindowStatus.ExemptProgrammeEnded), (semester1.Semesters[0], semester1.Status));
        Assert.Equal((new AcademicPeriod(2025, 2), QuotaWindowStatus.Counting), (earlier.Semesters[0], earlier.Status));
    }

    [Fact]
    public void ALateStartAndAnEarlyEndInOneWindow_ReadsAsTheLateStart()
    {
        // Both waive the target. The start is judged first, as it always was, so "started part-way through" is said.
        var window = QuotaWindow.For(QuotaPeriod.Semester, new DateOnly(2026, 5, 1), new DateOnly(2026, 3, 1), new DateOnly(2026, 5, 1));

        Assert.Equal(QuotaWindowStatus.ExemptPartialPeriod, window.Status);
    }

    [Fact]
    public void ThereIsNoFirstCountedWindow_WhenTheEndCameBeforeIt()
    {
        // Started in March (semester 1 waived, D42): the first semester target would be semester 2.
        var start = new DateOnly(2026, 3, 1);

        Assert.Equal(new AcademicPeriod(2026, 2), QuotaWindow.FirstCountedPeriodFor(QuotaPeriod.Semester, start));
        Assert.Equal(new AcademicPeriod(2026, 2), QuotaWindow.FirstCountedPeriodFor(QuotaPeriod.Semester, start, new DateOnly(2026, 11, 15)));
        Assert.Null(QuotaWindow.FirstCountedPeriodFor(QuotaPeriod.Semester, start, new DateOnly(2026, 9, 30)));
        Assert.Null(QuotaWindow.FirstCountedPeriodFor(QuotaPeriod.Semester, start, new DateOnly(2026, 5, 31)));

        var waived = QuotaWindow.For(QuotaPeriod.Semester, new DateOnly(2026, 5, 1), start, new DateOnly(2026, 9, 30));
        Assert.Equal(QuotaWindowStatus.ExemptPartialPeriod, waived.Status);
        Assert.Null(waived.FirstCountedPeriod);
    }

    [Theory]
    [InlineData(QuotaPeriod.Semester, "2026-01-01", "2026-06-01")]
    [InlineData(QuotaPeriod.Semester, "2026-07-01", "2026-11-01")]
    [InlineData(QuotaPeriod.AcademicYear, "2026-01-01", "2026-11-01")]
    public void TheEarliestFullPeriodEnd_IsTheFirstDayOfTheWindowsLastMonthOnTheCollegesCalendar(
        QuotaPeriod kind, string inWindow, string expected)
    {
        var window = QuotaWindow.For(kind, Day(inWindow), Start);

        Assert.Equal(Day(expected), QuotaWindow.EarliestFullPeriodEnd(window.Semesters));
    }

    [Fact]
    public void IsTotal_ForAnEndBeforeTheStart_AndAtTheEdgesOfTheCalendar()
    {
        // A programme start moved after its recorded end (the edit form allows it) must not throw, and holds nobody to
        // anything.
        var start = new DateOnly(2026, 8, 1);
        var end = new DateOnly(2026, 3, 1);
        foreach (var day in new[] { new DateOnly(2026, 2, 1), new DateOnly(2026, 5, 1), new DateOnly(2026, 9, 1), new DateOnly(2027, 2, 1) })
        {
            Assert.NotEqual(QuotaWindowStatus.Counting, QuotaWindow.For(QuotaPeriod.Semester, day, start, end).Status);
            Assert.NotEqual(QuotaWindowStatus.Counting, QuotaWindow.For(QuotaPeriod.AcademicYear, day, start, end).Status);
        }

        Assert.Equal(
            QuotaWindowStatus.ExemptProgrammeEnded,
            QuotaWindow.For(QuotaPeriod.Semester, new DateOnly(9999, 12, 31), new DateOnly(9999, 7, 1), new DateOnly(9999, 10, 1)).Status);
        Assert.Equal(
            QuotaWindowStatus.Counting,
            QuotaWindow.For(QuotaPeriod.AcademicYear, DateOnly.MinValue, DateOnly.MinValue, DateOnly.MaxValue).Status);
    }

    private static DateOnly Day(string iso) => DateOnly.Parse(iso, System.Globalization.CultureInfo.InvariantCulture);
}
