using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T358 (flow 06, lane B; round 3 item 21; R3-768): the oversight Homes' rows. In a file of its own so the lanes do not
/// meet in one test file.
/// </summary>
public sealed partial class StylesheetRuleTests
{
    private const string DashboardOneColumn = "@media (max-width: 900px)";

    /// <summary>
    /// A registrar's row is the name, then two 10rem figure columns; an EPA's the name, then one. At 900px and below, where
    /// the dashboard grid has one column, both are one column, so from 641 to 900px a figure never squeezes the name.
    /// </summary>
    [Fact]
    public void TheRosterAndCoverageRows_AreGridsOfFixedFigureColumns_AndOneColumnAt900px()
    {
        var css = Stylesheet.AppCss();

        var roster = css.Cascaded(".roster-row");
        roster["display"].Should().Be("grid");
        roster["grid-template-columns"].Should().Be("minmax(0, 1fr) 10rem 10rem");
        css.Cascaded(".coverage-row")["grid-template-columns"].Should().Be("minmax(0, 1fr) 10rem");
        css.Cascaded(".roster-row > .roster-exempt")["grid-column"].Should().Be("2 / -1", "the badge and why take both figure columns");

        css.CascadedIn(DashboardOneColumn, ".roster-row")["grid-template-columns"].Should().Be("minmax(0, 1fr)");
        css.CascadedIn(DashboardOneColumn, ".coverage-row")["grid-template-columns"].Should().Be("minmax(0, 1fr)");
        css.CascadedIn(DashboardOneColumn, ".roster-row > .roster-exempt")["grid-column"].Should().Be("auto");
    }

    /// <summary>Rows are ruled between, never above the first, as the Trainee's Furthest short rows are.</summary>
    [Fact]
    public void TheRosterAndCoverageRows_AreRuledBetween()
    {
        var css = Stylesheet.AppCss();

        css.Cascaded(".roster-row")["border-top"].Should().Be("1px solid var(--border-color)");
        css.Cascaded(".roster-row", ".roster-row:first-child")["border-top"].Should().Be("0");
        css.Cascaded(".coverage-row", ".coverage-row:first-child")["border-top"].Should().Be("0");
    }
}
