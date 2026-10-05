using FluentAssertions;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.Charts;

/// <summary>
/// T355: a rating trajectory's words, every phrase Spec § 1 and the R3-C-Trajectory board give it, for the cast: Lerato
/// Molefe's three PAED-001 ratings, Anele Dlamini's one, and a rating on another ladder. Dates ISO (decision D1).
/// </summary>
public sealed class TrajectoryWordsTests
{
    private static readonly DateOnly From = new(2026, 1, 1);
    private static readonly DateOnly To = new(2026, 12, 31);

    /// <summary>Molefe's minimum: 4 in training year 3, 5 from 2026-01-14 in training year 4.</summary>
    private static readonly IReadOnlyList<TrajectoryMinimumStepDto> MolefeSteps =
    [
        new(From, 3, 5, "4"),
        new(new DateOnly(2026, 1, 14), 4, 6, "5")
    ];

    [Fact]
    public void ThreeRatings_NameTheYear_TheirAssessors_AndTheirVerdicts()
    {
        var trajectory = Molefe(
            Point(1, new(2026, 9, 21), "5", "zulu", "Thandi Zulu", 4, "5", TrajectoryAgainstMinimum.AtOrAbove),
            Point(2, new(2026, 9, 22), "5", "naidoo", "David Naidoo", 4, "5", TrajectoryAgainstMinimum.AtOrAbove),
            Point(3, new(2026, 9, 24), "4", "patel", "Mohammed Patel", 4, "5", TrajectoryAgainstMinimum.Below));

        TrajectoryWords.Summary(trajectory, reviewWindow: false, subjectName: null).Should().Be(
            "3 ratings in the 2026 academic year, from Thandi Zulu, David Naidoo and Mohammed Patel. 2 at the minimum, 1 below.");
    }

    [Fact]
    public void OneRating_IsSoFar()
    {
        var trajectory = Molefe(Point(1, new(2026, 9, 23), "4", "naidoo", "David Naidoo", 3, "4", TrajectoryAgainstMinimum.AtOrAbove));

        TrajectoryWords.Summary(trajectory, reviewWindow: false, subjectName: null).Should().Be(
            "1 rating so far, from David Naidoo. At the minimum.");
    }

    /// <summary>
    /// T358 (flow 06; R2-Registrar r1): the registrar page names the registrar and the academic year it draws, even for one
    /// rating: "so far" is the registrar's own phrase, for a chart of their own year.
    /// </summary>
    [Fact]
    public void TheRegistrarPage_NamesTheRegistrarAndTheYear_EvenForOneRating()
    {
        var one = Molefe(Point(1, new(2026, 10, 1), "3a", "zulu", "Thandi Zulu", 1, "3a", TrajectoryAgainstMinimum.AtOrAbove));
        var two = Molefe(
            Point(1, new(2026, 9, 21), "5", "zulu", "Thandi Zulu", 4, "5", TrajectoryAgainstMinimum.AtOrAbove),
            Point(2, new(2026, 9, 24), "4", "patel", "Mohammed Patel", 4, "5", TrajectoryAgainstMinimum.Below));

        TrajectoryWords.Summary(one, reviewWindow: false, subjectName: "Nomsa Mahlangu").Should().Be(
            "Nomsa Mahlangu · 1 rating in the 2026 academic year, from Thandi Zulu. At the minimum.");
        TrajectoryWords.Summary(two, reviewWindow: false, subjectName: "Lerato Molefe").Should().Be(
            "Lerato Molefe · 2 ratings in the 2026 academic year, from Thandi Zulu and Mohammed Patel. 1 at the minimum, 1 below.");
    }

    [Fact]
    public void ARatingOnAnotherScale_IsCountedApart()
    {
        var trajectory = Molefe(
            Point(1, new(2026, 8, 10), "3b", "botha", "Sarah Botha", 3, "4", TrajectoryAgainstMinimum.NotComparable) with
            {
                OffLadder = true, OtherScaleName = "O-R Scale", OtherScaleRatingLabel = "Independent"
            },
            Point(2, new(2026, 9, 12), "3b", "patel", "Mohammed Patel", 3, "4", TrajectoryAgainstMinimum.Below));

        TrajectoryWords.Summary(trajectory, reviewWindow: false, subjectName: null).Should().Be(
            "2 ratings in the 2026 academic year, from Sarah Botha and Mohammed Patel. 1 on another scale, 1 below.");
        TrajectoryWords.RatingCell(trajectory.Points[0]).Should().Be("Independent on O-R Scale", "C11's [rating] on [scale]");
        TrajectoryWords.RatingCell(trajectory.Points[1]).Should().Be("3b");
        TrajectoryWords.AgainstMinimum(trajectory.Points[0]).Should().Be("Not on the ladder: counts towards the number, not the level");
    }

    [Fact]
    public void TheCommitteesChart_NamesTheTrainee_AndTheReviewsWindow()
    {
        var trajectory = Molefe(
            Point(1, new(2026, 9, 21), "5", "zulu", "Thandi Zulu", 4, "5", TrajectoryAgainstMinimum.AtOrAbove),
            Point(2, new(2026, 9, 22), "5", "naidoo", "David Naidoo", 4, "5", TrajectoryAgainstMinimum.AtOrAbove),
            Point(3, new(2026, 9, 24), "4", "patel", "Mohammed Patel", 4, "5", TrajectoryAgainstMinimum.Below));

        TrajectoryWords.Summary(trajectory, reviewWindow: true, subjectName: "Lerato Molefe").Should().Be(
            "Lerato Molefe · 3 ratings in the review window, 2026-01-01 to 2026-12-31, from Thandi Zulu, David Naidoo and " +
            "Mohammed Patel. 2 at the minimum, 1 below.");
    }

    [Fact]
    public void MoreThanThreeAssessors_AreCounted()
    {
        var trajectory = Molefe(
            Point(1, new(2026, 3, 1), "5", "a", "A One", 4, "5", TrajectoryAgainstMinimum.AtOrAbove),
            Point(2, new(2026, 4, 1), "5", "b", "B Two", 4, "5", TrajectoryAgainstMinimum.AtOrAbove),
            Point(3, new(2026, 5, 1), "5", "c", "C Three", 4, "5", TrajectoryAgainstMinimum.AtOrAbove),
            Point(4, new(2026, 6, 1), "5", "a", "A One", 4, "5", TrajectoryAgainstMinimum.AtOrAbove),
            Point(5, new(2026, 7, 1), "4", "d", "D Four", 4, "5", TrajectoryAgainstMinimum.Below));

        TrajectoryWords.Summary(trajectory, reviewWindow: false, subjectName: null).Should().Be(
            "5 ratings in the 2026 academic year, from 4 assessors. 4 at the minimum, 1 below.");
    }

    [Fact]
    public void TheTablesVerdict_SaysAtOrAbove_NeverAtIt()
    {
        // Note 14: the board's "At it (5, training year 4)" also covers a rating above, so it reads as the panel does.
        var at = Point(1, new(2026, 9, 21), "5", "zulu", "Thandi Zulu", 4, "5", TrajectoryAgainstMinimum.AtOrAbove);

        TrajectoryWords.AgainstMinimum(at).Should().Be("At or above (5, training year 4)");
        TrajectoryWords.AgainstMinimum(at with { AgainstMinimum = TrajectoryAgainstMinimum.Below, Rating = 5, RatingLabel = "4" })
            .Should().Be("Below (5, training year 4)");
        TrajectoryWords.AgainstMinimum(at with { AgainstMinimum = TrajectoryAgainstMinimum.NotGated, MinimumLabel = null })
            .Should().Be("No minimum");
        TrajectoryWords.RatingCell(at).Should().Be("5");
    }

    [Fact]
    public void TheKey_TheRegion_TheDrawing_AndTheEmpty()
    {
        var trajectory = Molefe(Point(1, new(2026, 9, 21), "5", "zulu", "Thandi Zulu", 4, "5", TrajectoryAgainstMinimum.AtOrAbove));

        TrajectoryWords.Key.Should().Be("How to read the chart");
        TrajectoryWords.KeyBelow(trajectory).Should().Be("Below the minimum: 4 until 2026-01-13, then 5 (training year 4)");
        TrajectoryWords.KeyExit(trajectory).Should().Be("Exit level 5");
        TrajectoryWords.KeyRating.Should().Be("A rating");
        TrajectoryWords.KeyOtherScale.Should().Be("Rated on another scale: counts towards the number, not the level");
        TrajectoryWords.RegionName("PAED-001").Should().Be("Rating chart for PAED-001");
        TrajectoryWords.SvgName(3).Should().Be("Chart of the 3 ratings in the table below.");
        TrajectoryWords.SvgName(1).Should().Be("Chart of the 1 rating in the table below.");
        TrajectoryWords.NoRating.Should().Be("No rating yet.");
        TrajectoryWords.NotPlotted.Should().Be("Multi-source feedback is not plotted.");
    }

    [Fact]
    public void TheKeysMinimum_MergesAStepThatKeepsTheLevel_AndIsAbsentWithNoMinimum()
    {
        var oneLevel = Molefe() with
        {
            MinimumSteps = [new(From, 3, 6, "4"), new(new DateOnly(2026, 1, 14), 4, 6, "4")]
        };

        TrajectoryWords.KeyBelow(oneLevel).Should().Be("Below the minimum: 4 (training year 4)");
        TrajectoryWords.KeyBelow(Molefe() with { MinimumSteps = [] }).Should().BeNull();
        TrajectoryWords.KeyExit(Molefe() with { ExitLevelLabel = null }).Should().BeNull();
    }

    private static EpaTrajectoryDto Molefe(params TrajectoryPointDto[] points)
        => new(2, "PAED-001", "Providing paediatric emergency care to children", true, 42,
            "CPSA Paediatric Entrustment Scale v11.1", [], points)
        {
            ExitLevelOrder = 6,
            ExitLevelLabel = "5",
            MinimumSteps = MolefeSteps,
            WindowFrom = From,
            WindowTo = To
        };

    private static TrajectoryPointDto Point(
        int activityId, DateOnly observedOn, string label, string assessorId, string assessorName, int year, string minimum,
        TrajectoryAgainstMinimum against)
        => new(activityId, observedOn, ObservedOnDeclared: true, Rating: 0, label, "Direct observation", assessorId)
        {
            AssessorName = assessorName,
            ActivityName = $"Mini-CEX (Paediatrics) · PAED-001 · {observedOn:yyyy-MM-dd}",
            TrainingYear = year,
            MinimumLabel = minimum,
            AgainstMinimum = against
        };
}
