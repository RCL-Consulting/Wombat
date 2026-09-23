using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;

namespace Wombat.Domain.Tests.Curricula;

public sealed class AcademicPeriodTests
{
    [Theory]
    [InlineData("2026-01-01", 2026, 1)]
    [InlineData("2026-03-10", 2026, 1)]
    // June is semester 1: the register's reading of D13's "boundary in June", which the College has not yet
    // confirmed. If these rows change, SecondSemesterStartMonth/Day changed, and every database needs a rebuild.
    [InlineData("2026-06-15", 2026, 1)]
    [InlineData("2026-06-16", 2026, 1)]
    [InlineData("2026-06-30", 2026, 1)]
    [InlineData("2026-07-01", 2026, 2)]
    [InlineData("2026-11-30", 2026, 2)]
    // December is outside the January–November teaching year but still has to land somewhere: it folds
    // into semester 2 of the same year (D40), never into next year's semester 1.
    [InlineData("2026-12-01", 2026, 2)]
    [InlineData("2026-12-31", 2026, 2)]
    [InlineData("0001-01-01", 1, 1)]
    [InlineData("9999-12-31", 9999, 2)]
    public void Containing_PutsEveryDateInExactlyOneSemester(string date, int year, int semester)
    {
        var period = AcademicPeriod.Containing(DateOnly.Parse(date));

        Assert.Equal(new AcademicPeriod(year, semester), period);
        Assert.True(period.Contains(DateOnly.Parse(date)));
    }

    [Fact]
    public void TheTwoSemestersTileTheCalendarYearWithNoGapAndNoOverlap()
    {
        var first = new AcademicPeriod(2026, 1);
        var second = new AcademicPeriod(2026, 2);

        Assert.Equal(new DateOnly(2026, 1, 1), first.Start);
        Assert.Equal(new DateOnly(2026, 6, 30), first.End);
        Assert.Equal(first.End.AddDays(1), second.Start);
        Assert.Equal(new DateOnly(2026, 12, 31), second.End);
        Assert.Equal(new AcademicPeriod(2027, 1).Start, second.End.AddDays(1));
    }

    [Fact]
    public void EveryDayOfAYearResolvesToTheSemesterThatContainsIt()
    {
        // Exhaustive rather than sampled: a leap year, so 29 February and both boundaries are all covered.
        for (var day = new DateOnly(2028, 1, 1); day.Year == 2028; day = day.AddDays(1))
        {
            var period = AcademicPeriod.Containing(day);
            Assert.True(period.Start <= day && day <= period.End, $"{day} resolved to {period}.");
        }
    }

    [Fact]
    public void SemesterTwoEndsOnTheThirtyFirstForCounting_ButOnTheThirtiethOfNovemberForTheReader()
    {
        var second = new AcademicPeriod(2026, 2);

        Assert.Equal(new DateOnly(2026, 12, 31), second.End);
        Assert.Equal(new DateOnly(2026, 11, 30), second.NominalEnd);
        Assert.Equal(new AcademicPeriod(2026, 1).End, new AcademicPeriod(2026, 1).NominalEnd);
    }

    [Fact]
    public void NextAndPrevious_WalkAcrossTheYearBoundary_AndStopAtTheEdgesOfTime()
    {
        Assert.Equal(new AcademicPeriod(2026, 2), new AcademicPeriod(2026, 1).Next());
        Assert.Equal(new AcademicPeriod(2027, 1), new AcademicPeriod(2026, 2).Next());
        Assert.Null(new AcademicPeriod(9999, 2).Next());

        Assert.Equal(new AcademicPeriod(2026, 1), new AcademicPeriod(2026, 2).Previous());
        Assert.Equal(new AcademicPeriod(2025, 2), new AcademicPeriod(2026, 1).Previous());
        Assert.Null(new AcademicPeriod(1, 1).Previous());

        Assert.Equal(new DateOnly(9999, 12, 31), new AcademicPeriod(9999, 2).End);
    }

    [Theory]
    [InlineData(2026, 0)]
    [InlineData(2026, 3)]
    [InlineData(0, 1)]
    [InlineData(10000, 1)]
    public void Constructor_RefusesAPeriodThatDoesNotExist(int year, int semester)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new AcademicPeriod(year, semester));
    }

    [Theory]
    [InlineData("2026-06-30T21:59:59Z", "2026-06-30")]
    [InlineData("2026-06-30T22:00:00Z", "2026-07-01")]
    [InlineData("2026-12-31T23:30:00Z", "2027-01-01")]
    public void ProgrammeCalendar_GivesTheSouthAfricanDateOfAUtcInstant(string utc, string expected)
    {
        // UTC+2 all year, no daylight saving: the calendar the clinician types an encounter date in.
        var instant = DateTime.Parse(utc, null, System.Globalization.DateTimeStyles.AdjustToUniversal);

        Assert.Equal(DateOnly.Parse(expected), ProgrammeCalendar.DateOf(instant));
    }

    [Fact]
    public void APeriodIsNotATrainingYear()
    {
        // D17. A registrar who started on 1 March 2025 is in training year 2 from 1 March 2026. On 10 March
        // 2026 they are in training year 2 AND in semester 1 of 2026, which began while they were still in
        // training year 1. The two concepts disagree by construction. This test pins that they are computed
        // independently, so nobody "fixes" one by deriving it from the other.
        var profile = new TraineeProfile { ProgrammeStartDate = new DateOnly(2025, 3, 1) };
        var encounter = new DateOnly(2026, 3, 10);

        Assert.Equal(2, profile.GetStage(encounter));
        Assert.Equal(new AcademicPeriod(2026, 1), AcademicPeriod.Containing(encounter));
        Assert.Equal(1, profile.GetStage(AcademicPeriod.Containing(encounter).Start));
    }
}
