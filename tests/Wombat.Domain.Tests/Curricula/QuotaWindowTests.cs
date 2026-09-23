using Wombat.Domain.Curricula;

namespace Wombat.Domain.Tests.Curricula;

/// <summary>
/// The College's D14: a registrar who starts part-way through a period is exempt for it and starts counting at
/// the next boundary. It applies per item, because the boundary differs by quota kind. What counts as "part-way"
/// is D42, which is provisional.
/// </summary>
public sealed class QuotaWindowTests
{
    private static readonly DateOnly Today = new(2026, 9, 23);

    [Fact]
    public void ASemesterWindowIsTheSemesterContainingTheDay()
    {
        var window = QuotaWindow.For(QuotaPeriod.Semester, Today, programmeStart: new DateOnly(2025, 1, 1));

        Assert.Equal(QuotaWindowStatus.Counting, window.Status);
        Assert.Equal(new DateOnly(2026, 7, 1), window.Start);
        Assert.Equal(new DateOnly(2026, 12, 31), window.End);
        Assert.Equal(new DateOnly(2026, 11, 30), window.NominalEnd);
        Assert.True(window.Covers(2026, 2));
        Assert.False(window.Covers(2026, 1));
    }

    [Fact]
    public void AnAcademicYearWindowCoversBothSemestersOfTheYear()
    {
        var window = QuotaWindow.For(QuotaPeriod.AcademicYear, Today, programmeStart: new DateOnly(2025, 1, 1));

        Assert.Equal(QuotaWindowStatus.Counting, window.Status);
        Assert.Equal(new DateOnly(2026, 1, 1), window.Start);
        Assert.Equal(new DateOnly(2026, 11, 30), window.NominalEnd);
        Assert.True(window.Covers(2026, 1));
        Assert.True(window.Covers(2026, 2));
        Assert.False(window.Covers(2025, 2));
        Assert.Equal(2026, window.AcademicYear);
    }

    // D42, semester targets: a start within the semester's first calendar month is a boundary start. The first
    // working day of January is never 1 January (a public holiday); in 2028 it is Monday the 3rd.
    [Theory]
    [InlineData("2026-01-01", QuotaWindowStatus.Counting)]
    [InlineData("2026-01-02", QuotaWindowStatus.Counting)]
    [InlineData("2028-01-03", QuotaWindowStatus.Counting)]
    [InlineData("2026-01-15", QuotaWindowStatus.Counting)]
    [InlineData("2026-01-31", QuotaWindowStatus.Counting)]
    [InlineData("2026-02-01", QuotaWindowStatus.ExemptPartialPeriod)]
    [InlineData("2026-03-14", QuotaWindowStatus.ExemptPartialPeriod)]
    [InlineData("2026-06-30", QuotaWindowStatus.ExemptPartialPeriod)]
    public void SemesterTarget_StartsWithinTheFirstMonthCount_LaterStartsAreExempt(string start, QuotaWindowStatus expected)
    {
        var programmeStart = DateOnly.Parse(start);
        var day = new DateOnly(programmeStart.Year, 6, 30);

        Assert.Equal(expected, QuotaWindow.For(QuotaPeriod.Semester, day, programmeStart).Status);
    }

    // D42, academic-year targets: only a start in semester 2 is "mid-year", which is the College's own boundary.
    [Theory]
    [InlineData("2026-01-15", QuotaWindowStatus.Counting)]
    [InlineData("2026-02-01", QuotaWindowStatus.Counting)]
    [InlineData("2026-04-01", QuotaWindowStatus.Counting)]
    [InlineData("2026-06-30", QuotaWindowStatus.Counting)]
    [InlineData("2026-07-01", QuotaWindowStatus.ExemptPartialPeriod)]
    [InlineData("2026-07-02", QuotaWindowStatus.ExemptPartialPeriod)]
    [InlineData("2026-07-15", QuotaWindowStatus.ExemptPartialPeriod)]
    public void YearTarget_OnlyASecondSemesterStartIsMidYear(string start, QuotaWindowStatus expected)
    {
        var programmeStart = DateOnly.Parse(start);
        var day = new DateOnly(programmeStart.Year, 11, 30);

        Assert.Equal(expected, QuotaWindow.For(QuotaPeriod.AcademicYear, day, programmeStart).Status);
    }

    [Fact]
    public void TheDevTraineesRealStartIsExemptForTheSemester_AndCountsFromNextJanuary()
    {
        var window = QuotaWindow.For(QuotaPeriod.Semester, Today, programmeStart: new DateOnly(2026, 9, 20));

        Assert.Equal(QuotaWindowStatus.ExemptPartialPeriod, window.Status);
        Assert.Equal(new AcademicPeriod(2027, 1), window.FirstCountedPeriod);
    }

    [Fact]
    public void AJulyStarterCountsSemesterItemsAtOnce_ButIsExemptFromAnnualItemsUntilJanuary()
    {
        var start = new DateOnly(2026, 7, 1);

        var semester = QuotaWindow.For(QuotaPeriod.Semester, Today, start);
        var annual = QuotaWindow.For(QuotaPeriod.AcademicYear, Today, start);

        Assert.Equal(QuotaWindowStatus.Counting, semester.Status);
        Assert.Equal(new AcademicPeriod(2026, 2), semester.FirstCountedPeriod);
        Assert.Equal(QuotaWindowStatus.ExemptPartialPeriod, annual.Status);
        Assert.Equal(new AcademicPeriod(2027, 1), annual.FirstCountedPeriod);
    }

    [Fact]
    public void AnAprilStarterIsExemptFromSemesterOne_ButHeldToTheYear()
    {
        var start = new DateOnly(2026, 4, 1);

        Assert.Equal(QuotaWindowStatus.ExemptPartialPeriod, QuotaWindow.For(QuotaPeriod.Semester, new DateOnly(2026, 5, 1), start).Status);
        Assert.Equal(QuotaWindowStatus.Counting, QuotaWindow.For(QuotaPeriod.Semester, new DateOnly(2026, 7, 1), start).Status);
        Assert.Equal(QuotaWindowStatus.Counting, QuotaWindow.For(QuotaPeriod.AcademicYear, new DateOnly(2026, 5, 1), start).Status);
    }

    [Fact]
    public void BeforeTheProgrammeStartsNothingIsExpected()
    {
        var window = QuotaWindow.For(QuotaPeriod.Semester, new DateOnly(2026, 2, 1), programmeStart: new DateOnly(2026, 3, 14));

        Assert.Equal(QuotaWindowStatus.NotStarted, window.Status);
        Assert.Equal(new AcademicPeriod(2026, 2), window.FirstCountedPeriod);
    }

    [Fact]
    public void AFutureOnTimeStartsFirstTargetIsTheWindowItStartsIn()
    {
        var window = QuotaWindow.For(QuotaPeriod.AcademicYear, new DateOnly(2026, 12, 1), programmeStart: new DateOnly(2027, 1, 15));

        Assert.Equal(QuotaWindowStatus.NotStarted, window.Status);
        Assert.Equal(new AcademicPeriod(2027, 1), window.FirstCountedPeriod);
    }

    [Fact]
    public void PrecedingIsThePreviousWindowOfTheSameKind_JudgedForTheSameTrainee()
    {
        var start = new DateOnly(2026, 3, 14);

        var semester = QuotaWindow.For(QuotaPeriod.Semester, Today, start);
        var previousSemester = semester.Preceding(start)!;
        Assert.True(previousSemester.Covers(2026, 1));
        Assert.Equal(QuotaWindowStatus.ExemptPartialPeriod, previousSemester.Status);

        var year = QuotaWindow.For(QuotaPeriod.AcademicYear, Today, start);
        var previousYear = year.Preceding(start)!;
        Assert.True(previousYear.Covers(2025, 1));
        Assert.True(previousYear.Covers(2025, 2));
        Assert.Equal(QuotaWindowStatus.NotStarted, previousYear.Status);
    }

    [Fact]
    public void IsTotalAtTheEdgesOfTheCalendar()
    {
        var latest = QuotaWindow.For(QuotaPeriod.Semester, new DateOnly(9999, 12, 31), programmeStart: new DateOnly(9999, 8, 15));
        Assert.Equal(QuotaWindowStatus.ExemptPartialPeriod, latest.Status);
        Assert.Null(latest.FirstCountedPeriod);

        var earliest = QuotaWindow.For(QuotaPeriod.AcademicYear, DateOnly.MinValue, DateOnly.MinValue);
        Assert.Equal(QuotaWindowStatus.Counting, earliest.Status);
        Assert.Null(earliest.Preceding(DateOnly.MinValue));
    }

    [Fact]
    public void AnUnrecognisedKindReadsAsAnAcademicYear()
    {
        // A value outside the enum can only come from a hand-edited row. The zero value's meaning is the safe
        // reading, and a reader must not throw.
        var window = QuotaWindow.For((QuotaPeriod)99, Today, new DateOnly(2025, 1, 1));

        Assert.Equal(2, window.Semesters.Count);
    }
}
