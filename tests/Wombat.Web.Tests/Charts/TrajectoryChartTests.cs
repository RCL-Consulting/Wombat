using Bunit;
using FluentAssertions;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.Charts;

public sealed class TrajectoryChartTests : TestContext
{

    // ---- T123 defect 1: the axis comes from the scale, not from the data ----

    private static readonly TrajectoryChart.Rung[] CpsaLadder =
    [
        new(1, "1"), new(2, "2"), new(3, "3a"), new(4, "3b"), new(5, "4"), new(6, "5")
    ];

    private static readonly TrajectoryChart.Rung[] WordLadder =
    [
        new(1, "Observe only"), new(2, "Direct supervision"), new(3, "Indirect supervision"),
        new(4, "Independent"), new(5, "Supervises others")
    ];

    [Fact]
    public void Rungs_DrawTheWholeLadder_EvenWhenNoRatingReachesTheTop()
    {
        // The defect: a six-rung trainee whose ratings all sit at 3 saw a 1-5 axis -- a ladder with
        // its top two rungs missing -- because the axis grew to fit the DATA, not the SCALE.
        var points = new[] { new TrajectoryChart.ChartPoint(new DateOnly(2026, 3, 10), 3) };

        var cut = RenderComponent<TrajectoryChart>(parameters => parameters
            .Add(p => p.Points, points)
            .Add(p => p.Rungs, CpsaLadder));

        cut.FindAll("g.trajectory-chart-grid line").Count.Should().Be(6);
    }

    [Fact]
    public void Rungs_LabelTheAxisWithTheRung_NotTheOrdinal()
    {
        // On this ladder ordinal 5 is the College's rung "4" and ordinal 6 is rung "5". An axis of
        // bare ordinals labels tick 5 as "5" and means rung 4.
        var points = new[] { new TrajectoryChart.ChartPoint(new DateOnly(2026, 3, 10), 3) };

        var cut = RenderComponent<TrajectoryChart>(parameters => parameters
            .Add(p => p.Points, points)
            .Add(p => p.Rungs, CpsaLadder));

        var labels = cut.FindAll("text.trajectory-chart-y-label")
            .Select(node => node.TextContent)
            .ToArray();

        // Rendered in ladder order, bottom rung first.
        labels.Should().Equal("1", "2", "3a", "3b", "4", "5");
    }

    [Fact]
    public void NoRungs_LeavesEveryExistingAxisExpressionUntouched()
    {
        // Every legacy trajectory lands here: curriculum 2 pins nothing, and the generic tools'
        // scale_key resolves to nothing (T110). The chart must render exactly as it did before.
        var points = new[]
        {
            new TrajectoryChart.ChartPoint(new DateOnly(2026, 1, 1), 2),
            new TrajectoryChart.ChartPoint(new DateOnly(2026, 2, 1), 3)
        };

        var cut = RenderComponent<TrajectoryChart>(parameters => parameters
            .Add(p => p.Points, points));

        cut.FindAll("g.trajectory-chart-grid line").Count.Should().Be(5);
        cut.FindAll("text.trajectory-chart-y-label")
            .Select(node => node.TextContent)
            .Should().Equal("1", "2", "3", "4", "5");
        cut.FindAll("circle.is-off-scale").Should().BeEmpty();
    }

    [Fact]
    public void LongRungLabels_FallBackToTheOrdinalOnTheAxis_ButNotInTheAccessibleTable()
    {
        // D29: the axis has a 40px margin. "Indirect supervision" would overwrite the plot, so the
        // axis shows the ordinal -- but the screen-reader table must never be poorer than the picture.
        var points = new[] { new TrajectoryChart.ChartPoint(new DateOnly(2026, 3, 10), 3) };

        var cut = RenderComponent<TrajectoryChart>(parameters => parameters
            .Add(p => p.Points, points)
            .Add(p => p.Rungs, WordLadder));

        cut.FindAll("text.trajectory-chart-y-label")
            .Select(node => node.TextContent)
            .Should().Equal("1", "2", "3", "4", "5");

        cut.Find("table.visually-hidden").TextContent.Should().Contain("Indirect supervision");
    }

    [Fact]
    public void ARatingThatIsNotARungOnTheAxisLadder_IsKeptHollowAndLeftOutOfTheLine()
    {
        // D30. A five-rung rating charted against a six-rung axis is the T109 defect drawn as a
        // picture. Dropping the point would erase evidence the trainee did not mis-record.
        var points = new[]
        {
            new TrajectoryChart.ChartPoint(new DateOnly(2026, 1, 1), 3),
            new TrajectoryChart.ChartPoint(new DateOnly(2026, 2, 1), 4),
            new TrajectoryChart.ChartPoint(new DateOnly(2026, 3, 1), 9)
        };

        var cut = RenderComponent<TrajectoryChart>(parameters => parameters
            .Add(p => p.Points, points)
            .Add(p => p.Rungs, CpsaLadder));

        cut.FindAll("circle.trajectory-chart-dot").Count.Should().Be(3, "the observation is real and stays on the page");
        cut.FindAll("circle.is-off-scale").Count.Should().Be(1);

        // Two on-ladder points remain, so the line joins exactly those two.
        cut.Find("polyline.trajectory-chart-line").GetAttribute("points")!
            .Split(' ').Should().HaveCount(2);

        var table = cut.Find("table.visually-hidden").TextContent;
        table.Should().Contain("not a rung on this scale");
    }

    [Fact]
    public void ASingleOnLadderPoint_DrawsNoLine_EvenWhenOtherPointsAreOffLadder()
    {
        var points = new[]
        {
            new TrajectoryChart.ChartPoint(new DateOnly(2026, 1, 1), 3),
            new TrajectoryChart.ChartPoint(new DateOnly(2026, 2, 1), 9)
        };

        var cut = RenderComponent<TrajectoryChart>(parameters => parameters
            .Add(p => p.Points, points)
            .Add(p => p.Rungs, CpsaLadder));

        cut.FindAll("polyline.trajectory-chart-line").Should().BeEmpty();
    }

    [Fact]
    public void RungLabelsAreHtmlEncodedOnTheAxis()
    {
        // The axis label is emitted as a raw MarkupString and its content is now an
        // EntrustmentLevel.Label an administrator can type, where it used to be an int.
        var points = new[] { new TrajectoryChart.ChartPoint(new DateOnly(2026, 3, 10), 1) };
        var ladder = new[] { new TrajectoryChart.Rung(1, "<x>") };

        var cut = RenderComponent<TrajectoryChart>(parameters => parameters
            .Add(p => p.Points, points)
            .Add(p => p.Rungs, ladder));

        cut.Markup.Should().NotContain("<x>");
        cut.Markup.Should().Contain("&lt;x&gt;");
    }

    [Fact]
    public void ANonContiguousLadder_DrawsOneTickPerRung_NotOnePerInteger()
    {
        // The grid used to be an integer stepper from Min to Max. Nothing guarantees a scale's
        // Orders are contiguous -- an administrator can delete a rung from the middle.
        var points = new[] { new TrajectoryChart.ChartPoint(new DateOnly(2026, 3, 10), 1) };
        var ladder = new[]
        {
            new TrajectoryChart.Rung(1, "1"),
            new TrajectoryChart.Rung(4, "3b"),
            new TrajectoryChart.Rung(6, "5")
        };

        var cut = RenderComponent<TrajectoryChart>(parameters => parameters
            .Add(p => p.Points, points)
            .Add(p => p.Rungs, ladder));

        cut.FindAll("g.trajectory-chart-grid line").Count.Should().Be(3);
        cut.FindAll("text.trajectory-chart-y-label")
            .Select(node => node.TextContent)
            .Should().Equal("1", "3b", "5");
    }

    [Fact]
    public void EmptyPoints_RendersEmptyPlaceholder()
    {
        var cut = RenderComponent<TrajectoryChart>(parameters => parameters
            .Add(p => p.Points, Array.Empty<TrajectoryChart.ChartPoint>()));

        cut.Markup.Should().NotContain("<svg");
        cut.Markup.Should().Contain("No observations to plot.");
    }

    [Fact]
    public void SinglePoint_RendersOneDotAndNoLine()
    {
        var points = new[]
        {
            new TrajectoryChart.ChartPoint(new DateOnly(2026, 2, 1), 3)
        };

        var cut = RenderComponent<TrajectoryChart>(parameters => parameters
            .Add(p => p.Points, points));

        cut.FindAll("circle.trajectory-chart-dot").Count.Should().Be(1);
        cut.FindAll("polyline.trajectory-chart-line").Should().BeEmpty();
    }

    [Fact]
    public void MultiplePoints_RendersDotsAndLine()
    {
        var points = new[]
        {
            new TrajectoryChart.ChartPoint(new DateOnly(2026, 1, 1), 2, 0, "Direct observation"),
            new TrajectoryChart.ChartPoint(new DateOnly(2026, 2, 1), 3, 1, "Conversation"),
            new TrajectoryChart.ChartPoint(new DateOnly(2026, 3, 1), 5, 2, "Direct observation")
        };

        var cut = RenderComponent<TrajectoryChart>(parameters => parameters
            .Add(p => p.Points, points));

        cut.FindAll("circle.trajectory-chart-dot").Count.Should().Be(3);
        cut.FindAll("polyline.trajectory-chart-line").Count.Should().Be(1);

        // Y grid lines = MaxRating - MinRating + 1 => 5 ticks for 1..5
        cut.FindAll("g.trajectory-chart-grid line").Count.Should().Be(5);
    }

    [Fact]
    public void AxisGrowsToFitRatingsAboveFive()
    {
        // Regression (T098): MaxRating defaulted to a hard 5, so a six-rung scale plotted its top
        // rung off the top of the chart. The axis now grows to fit the data.
        var points = new[]
        {
            new TrajectoryChart.ChartPoint(new DateOnly(2026, 1, 1), 3),
            new TrajectoryChart.ChartPoint(new DateOnly(2026, 2, 1), 6)
        };

        var cut = RenderComponent<TrajectoryChart>(parameters => parameters
            .Add(p => p.Points, points));

        // 1..6 => 6 ticks
        cut.FindAll("g.trajectory-chart-grid line").Count.Should().Be(6);
    }

    [Fact]
    public void AxisStaysAtFiveForAFiveRungScale()
    {
        // The familiar 1-5 axis must be unchanged for existing five-rung data.
        var points = new[]
        {
            new TrajectoryChart.ChartPoint(new DateOnly(2026, 1, 1), 2),
            new TrajectoryChart.ChartPoint(new DateOnly(2026, 2, 1), 4)
        };

        var cut = RenderComponent<TrajectoryChart>(parameters => parameters
            .Add(p => p.Points, points));

        cut.FindAll("g.trajectory-chart-grid line").Count.Should().Be(5);
    }

    [Fact]
    public void RendersFirstAndLastDateLabels()
    {
        var points = new[]
        {
            new TrajectoryChart.ChartPoint(new DateOnly(2026, 1, 1), 2),
            new TrajectoryChart.ChartPoint(new DateOnly(2026, 3, 1), 5)
        };

        var cut = RenderComponent<TrajectoryChart>(parameters => parameters
            .Add(p => p.Points, points));

        cut.Markup.Should().Contain("2026-01-01");
        cut.Markup.Should().Contain("2026-03-01");
    }

    [Fact]
    public void UsesAriaLabelForAccessibility()
    {
        var points = new[]
        {
            new TrajectoryChart.ChartPoint(new DateOnly(2026, 1, 1), 3)
        };

        var cut = RenderComponent<TrajectoryChart>(parameters => parameters
            .Add(p => p.Points, points)
            .Add(p => p.AriaLabel, "Rating trajectory for EPA-07"));

        cut.Find("svg").GetAttribute("aria-label").Should().Be("Rating trajectory for EPA-07");
        cut.Find("svg").GetAttribute("role").Should().Be("img");
    }

    [Fact]
    public void RendersVisuallyHiddenTableFallbackForScreenReaders()
    {
        var points = new[]
        {
            new TrajectoryChart.ChartPoint(new DateOnly(2026, 1, 1), 2, 0, "Direct observation"),
            new TrajectoryChart.ChartPoint(new DateOnly(2026, 2, 1), 4, 1, "Conversation")
        };

        var cut = RenderComponent<TrajectoryChart>(parameters => parameters
            .Add(p => p.Points, points)
            .Add(p => p.AriaLabel, "Rating trajectory for EPA-07"));

        var table = cut.Find("table.visually-hidden");
        table.QuerySelector("caption")!.TextContent.Should().Be("Rating trajectory for EPA-07");

        var rows = table.QuerySelectorAll("tbody tr");
        rows.Length.Should().Be(2);
        rows[0].TextContent.Should().Contain("2026-01-01").And.Contain("2").And.Contain("Direct observation");
        rows[1].TextContent.Should().Contain("2026-02-01").And.Contain("4").And.Contain("Conversation");
    }
}
