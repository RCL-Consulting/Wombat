using Bunit;
using FluentAssertions;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.Charts;

public sealed class TrajectoryChartTests : TestContext
{
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
}
