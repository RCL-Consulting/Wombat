using FluentAssertions;
using Wombat.Application.Features.Curricula.Quota;

namespace Wombat.Application.Tests.Features.Curricula;

/// <summary>T355: the EPA page's chart is drawn over the academic year containing a day, December included (D40).</summary>
public sealed class TrajectoryWindowTests
{
    [Theory]
    [InlineData(2026, 1, 1)]
    [InlineData(2026, 10, 3)]
    [InlineData(2026, 11, 30)]
    [InlineData(2026, 12, 7)]
    [InlineData(2026, 12, 31)]
    public void TheAcademicYearOfADay_RunsFromJanuaryToDecember(int year, int month, int day)
        => TrajectoryWindow.AcademicYearOf(new DateOnly(year, month, day))
            .Should().Be((new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)),
                "December's encounters count into the year whose College calendar ended on 30 November");

    [Fact]
    public void TheFirstOfJanuary_StartsTheNextYear()
        => TrajectoryWindow.AcademicYearOf(new DateOnly(2027, 1, 1))
            .Should().Be((new DateOnly(2027, 1, 1), new DateOnly(2027, 12, 31)));
}
