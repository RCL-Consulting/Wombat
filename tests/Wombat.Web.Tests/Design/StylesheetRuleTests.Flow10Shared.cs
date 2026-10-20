using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T369 (flow 10, build-contracts § 0): the MSF pages' shared rules, written before the lanes so each uses them as given.
/// In a file of its own so the flows do not meet in one test file.
/// </summary>
public sealed partial class StylesheetRuleTests
{
    private const string Flow10Phone = "@media (max-width: 640.98px)";

    /// <summary>The page column: its sections 24px apart, the report's measure 60rem.</summary>
    [Fact]
    public void TheMsfPageColumn_SpacesItsSectionsOnTheScale()
    {
        var css = Stylesheet.AppCss();

        var page = css.Computed(".msf-page");
        page["display"].Should().Be("flex");
        page["flex-direction"].Should().Be("column");
        page["gap"].Should().Be("var(--space-lg)");
        css.Computed(".msf-page--measure")["max-width"].Should().Be("60rem");
    }

    /// <summary>
    /// M10: a ConfirmDialog on an MSF page, below 641px, shows its action on top and the safe button last on screen (the
    /// DOM has the safe button first, so it stays first in the tab order), each button 44px and full width. Since T373
    /// (flow 11, M8) that is the general rule on every dialog, which this section's scoped copy folded into
    /// (<see cref="EveryDialog_OnAPhone_StacksItsButtons_TheActionOnTop" />).
    /// </summary>
    [Fact]
    public void AnMsfDialog_OnAPhone_StacksItsButtons_TheActionOnTop()
    {
        var css = Stylesheet.AppCss();

        css.RulesFor(".dialog-card .form-actions", Flow10Phone).Should().ContainSingle()
            .Which.Value("flex-direction").Should().Be("column-reverse");
        var button = css.ComputedIn(Flow10Phone, ".dialog-card .form-actions .btn");
        button["min-height"].Should().Be("2.75rem");
        button["width"].Should().Be("100%");
    }

    /// <summary>
    /// T369, build review D1: the page column's reset cancels the element defaults and .alert's margin, and no class's.
    /// At (0,2,1) it beat every single class, so "Campaigns (2)" and "Your released reports" sat on their tables and New
    /// MSF campaign's card titles on their grids. Each title is cascaded against the reset's selector that reaches an h2
    /// nested in the column, as the browser has it.
    /// </summary>
    [Theory]
    [InlineData("h2", ".list-section-title", "var(--space-md)")]
    [InlineData("h2", ".msf-card-title", "var(--space-md)")]
    [InlineData("h2", ".section-title", "var(--space-sm)")]
    public void ATitleInsideTheMsfPage_KeepsItsBottomMargin(string element, string title, string margin)
    {
        var css = Stylesheet.AppCss();
        var nested = NestedReset(css, element);

        css.Cascaded([element, title, nested])["margin-bottom"].Should().Be(margin);
    }

    /// <summary>D1: the reset still wins over what it is for, h2's and .alert's own margins.</summary>
    [Fact]
    public void TheMsfPageReset_StillCancelsTheElementDefaults()
    {
        var css = Stylesheet.AppCss();

        foreach (var element in new[] { "h2", "p", ".alert" })
        {
            css.Cascaded([element, NestedReset(css, element)])["margin-bottom"].Should().Be("0", element);
        }
    }

    /// <summary>The page column reset's one selector that reaches <paramref name="element" /> nested in the column.</summary>
    private static string NestedReset(Stylesheet css, string element)
        => css.Rules.Single(rule => rule.AtRule.Length == 0 && rule.Selectors.Contains(".msf-page > :not(dialog)"))
            .Selectors.Where(selector => selector != ".msf-page > :not(dialog)")
            .Should().ContainSingle(selector => selector.Contains($"{(element.StartsWith('.') ? "" : ".msf-page ")}{element}", StringComparison.Ordinal))
            .Subject;

    /// <summary>
    /// T369, build review D2 and D9: a stacked list's row actions are written once, in the shared section: a full-width
    /// column below 641px, and 44px buttons from 641 to 900px, where a list with both stack classes is stacked too (the
    /// campaign list and her reports). The 44px from 900px down is the general stack-wide rule since T373 (flow 11, S16).
    /// </summary>
    [Fact]
    public void AStackedMsfListsRowActions_AreWrittenOnce_44pxTo900px()
    {
        var css = Stylesheet.AppCss();

        css.RulesFor(".msf-page td .actions-cell", Flow10Phone).Should().ContainSingle()
            .Which.Value("flex-direction").Should().Be("column");
        css.RulesFor(".msf-page td .actions-cell .btn", Flow10Phone).Should().ContainSingle()
            .Which.Value("width").Should().Be("100%");
        css.RulesFor(".clinic-table--stack-wide .btn", "@media (max-width: 900px)").Should().ContainSingle()
            .Which.Value("min-height").Should().Be("2.75rem");
    }

    /// <summary>The header's actions stack at 390 (New campaign, then MSF coverage), each a full-width 44px button.</summary>
    [Fact]
    public void TheMsfHeadersActions_StackOnAPhone()
    {
        var css = Stylesheet.AppCss();

        css.RulesFor(".msf-header .actions-cell", Flow10Phone).Should().ContainSingle()
            .Which.Value("flex-direction").Should().Be("column");
        var button = css.ComputedIn(Flow10Phone, ".msf-header .actions-cell .btn");
        button["min-height"].Should().Be("2.75rem");
        button["width"].Should().Be("100%");
    }

    /// <summary>A link in running text: a 24px target from 641px, 44px below (flow 08's rule; round 3 S15).</summary>
    [Fact]
    public void AnMsfInlineLink_Is24pxThen44px()
    {
        var css = Stylesheet.AppCss();

        css.Computed(".msf-inline-link")["min-height"].Should().Be("24px");
        css.ComputedIn(Flow10Phone, ".msf-inline-link")["min-height"].Should().Be("2.75rem");
    }
}
