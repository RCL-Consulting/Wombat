using System.Globalization;
using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T355 (flow 05, lane D; Q5, E1, R4; notes 6, 13; C7): the EPA page and the trajectory's new form. In a file of its own so
/// the lanes do not meet in one test file.
/// </summary>
public sealed partial class StylesheetRuleTests
{
    private const string Wide900 = "@container (min-width: 900px)";

    [Fact]
    public void TheChartsTwoDrawings_AreChosenByTheCardsWidth_NeverByTheViewport()
    {
        var css = Stylesheet.AppCss();

        // E1: the figure is the container; below 900px of it the 326 drawing shows, from 900px the 900 drawing.
        css.Cascaded(".trajectory-figure")["container-type"].Should().Be("inline-size");
        css.Cascaded(".trajectory-figure .trajectory-chart--wide")["display"].Should().Be("none");
        css.Cascaded(".trajectory-figure .trajectory-chart--narrow")["display"].Should().Be("block");
        css.CascadedIn(Wide900, ".trajectory-figure .trajectory-chart--wide")["display"].Should().Be("block");
        css.CascadedIn(Wide900, ".trajectory-figure .trajectory-chart--narrow")["display"].Should().Be("none");

        css.Rules.Where(rule => rule.AtRule.StartsWith("@media", StringComparison.Ordinal))
            .SelectMany(rule => rule.Selectors)
            .Where(selector => selector.Contains("trajectory-chart--wide", StringComparison.Ordinal)
                || selector.Contains("trajectory-chart--narrow", StringComparison.Ordinal))
            .Should().BeEmpty("the viewport never chooses a drawing: the card does (E1)");
    }

    [Fact]
    public void TheChart_IsNeverScaled_AndItsRegionScrollsWhereTheCardIsNarrower()
    {
        var css = Stylesheet.AppCss();

        var sized = css.Cascaded(".trajectory-chart", ".trajectory-chart.trajectory-chart--sized");
        sized["max-width"].Should().Be("none", "each drawing keeps its own size (Q5)");
        sized["height"].Should().Be("auto");
        css.Cascaded(".trajectory-figure")["overflow-x"].Should().Be("auto");
    }

    [Fact]
    public void TheChartsRegion_ShowsTheFocusRing()
    {
        var ring = Stylesheet.AppCss().Cascaded(".trajectory-figure:focus-visible");

        ring["outline"].Should().Be("2px solid var(--focus-ring)");
        ring["outline-offset"].Should().Be("2px");
    }

    [Fact]
    public void NothingInTheChart_IsUnder12px_AndTheRungsAndMonthsAre13pxAt900()
    {
        var css = Stylesheet.AppCss();
        var sizes = css.Rules
            .Where(rule => rule.Selectors.Any(selector => selector.Contains("trajectory-chart", StringComparison.Ordinal)))
            .Select(rule => rule.Value("font-size"))
            .Where(size => size is not null)
            .Select(size => double.Parse(size!.Replace("px", string.Empty, StringComparison.Ordinal), CultureInfo.InvariantCulture))
            .ToList();

        sizes.Should().NotBeEmpty();
        sizes.Should().OnlyContain(size => size >= 12, "nothing under 11px, and the notes at 12px (note 13)");
        css.Cascaded(".trajectory-chart .trajectory-chart-y-label")["font-size"].Should().Be("13px");
        css.Cascaded(".trajectory-chart .trajectory-chart-x-label")["font-size"].Should().Be("13px");
        css.Cascaded(".trajectory-chart .trajectory-chart-x-label", ".trajectory-chart--narrow .trajectory-chart-x-label")["font-size"]
            .Should().Be("12px");
    }

    [Fact]
    public void TheMinimumsLabel_IsInTextColour_NeverTheWarningColour()
    {
        // C7: the warning colour marks the edge; words beside it are text-color (DESIGN.md: never words in warning-color).
        Stylesheet.AppCss().Cascaded(".trajectory-chart .trajectory-chart-minimum-label")["fill"].Should().Be("var(--text-color)");
        Stylesheet.AppCss().Cascaded(".trajectory-chart-minimum")["stroke"].Should().Be("var(--warning-color)");
    }

    [Fact]
    public void TheChartsTableCaption_WinsOverTheBuiltCaption_ByComingAfterIt()
    {
        // C7: .trajectory-table caption ties .clinic-table caption (0,1,1) and must win by order.
        var css = Stylesheet.AppCss();

        css.Cascaded(".clinic-table caption", ".trajectory-table caption")["color"].Should().Be("var(--text-color)");
        css.Cascaded(".clinic-table caption", ".trajectory-table caption")["caption-side"].Should().Be("top");
    }

    [Fact]
    public void TheOldChartsRules_AreGone_NotLeftDeadBesideTheNewOnes()
    {
        // Note 6: the grid, the dots and the off-scale ring of the scaled chart are edited away.
        var selectors = Stylesheet.AppCss().Rules.SelectMany(rule => rule.Selectors).ToList();

        selectors.Should().NotContain(".trajectory-chart-grid line");
        selectors.Should().NotContain(".trajectory-chart-dot");
        selectors.Should().NotContain(".trajectory-chart-dot.is-off-scale");
    }

    [Fact]
    public void TheEpaPagesGrid_IsObservationsWide_AndOneColumnAt900pxAndBelow()
    {
        var css = Stylesheet.AppCss();

        css.Cascaded(".details-grid", ".details-grid--reverse")["grid-template-columns"].Should().Be("2fr 1fr");
        css.CascadedIn("@media (max-width: 900px)", ".details-grid", ".details-grid--reverse")["grid-template-columns"]
            .Should().Be("minmax(0, 1fr)");
    }
}
