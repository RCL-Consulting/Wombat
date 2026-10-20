using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T373 (flow 11, build-contracts § 0): the onboarding pages' shared rules, and the general phone rules round 3 asked for
/// (M8, M9, S16), written before the lanes so each uses them as given. Flows 08's and 10's scoped copies of the dialog,
/// header and stack-wide row-button rules fold into them. In a file of its own so the flows do not meet in one test file.
/// </summary>
public sealed partial class StylesheetRuleTests
{
    private const string Flow11Phone = "@media (max-width: 640.98px)";
    private const string Flow11Wide = "@media (max-width: 900px)";

    /// <summary>The four pages' column: its parts 24px apart, the two form pages 48rem; the count in muted text.</summary>
    [Fact]
    public void TheOnboardingPageColumn_SpacesItsPartsOnTheScale()
    {
        var css = Stylesheet.AppCss();

        var page = css.Computed(".inv-page");
        page["display"].Should().Be("flex");
        page["flex-direction"].Should().Be("column");
        page["gap"].Should().Be("var(--space-lg)");
        css.Computed(".inv-page--form")["max-width"].Should().Be("48rem");
        css.Computed(".inv-count")["color"].Should().Be("var(--muted-text)");
        css.Computed(".inv-when span")["display"].Should().Be("block");
    }

    /// <summary>
    /// M8: below 641px every ConfirmDialog in action mode shows its action on top and the safe button last on screen (the
    /// DOM has the safe button first, so it stays first in the tab order), each 44px and the row's width. One general rule:
    /// the top-layer dialog is outside any page wrapper, so no page's own class reaches it.
    /// </summary>
    [Fact]
    public void EveryDialog_OnAPhone_StacksItsButtons_TheActionOnTop()
    {
        var css = Stylesheet.AppCss();

        css.RulesFor(".dialog-card .form-actions", Flow11Phone).Should().ContainSingle()
            .Which.Value("flex-direction").Should().Be("column-reverse");
        var button = css.ComputedIn(Flow11Phone, ".dialog-card .form-actions .btn");
        button["min-height"].Should().Be("2.75rem");
        button["width"].Should().Be("100%");
    }

    /// <summary>
    /// M9: below 641px every page header's actions take the row and are 44px; every stand-alone button in the onboarding
    /// column is 44px. Flows 08 and 10 wrote the header rule under their own wrappers (`.committee-calendar`, `.msf-page`):
    /// they fold into this one.
    /// </summary>
    [Fact]
    public void EveryHeaderAction_OnAPhone_Is44px_AndTheRowsWidth()
    {
        var css = Stylesheet.AppCss();

        css.ComputedIn(Flow11Phone, ".header-container .actions-cell")["width"].Should().Be("100%");
        css.ComputedIn(Flow11Phone, ".header-container .actions-cell .btn")["min-height"].Should().Be("2.75rem");
        css.ComputedIn(Flow11Phone, ".inv-page .btn")["min-height"].Should().Be("2.75rem");
    }

    /// <summary>S16: a stack-wide table's row buttons are 44px wherever it is stacked, from 900px down, on every page.</summary>
    [Fact]
    public void AStackWideTablesButtons_Are44px_From900pxDown()
    {
        Stylesheet.AppCss().RulesFor(".clinic-table--stack-wide .btn", Flow11Wide).Should().ContainSingle()
            .Which.Value("min-height").Should().Be("2.75rem");
    }

    /// <summary>The scoped copies the general rules replace are gone, so the two cannot disagree.</summary>
    [Theory]
    [InlineData(".msf-page dialog .form-actions", Flow11Phone)]
    [InlineData(".msf-page dialog .btn", Flow11Phone)]
    [InlineData(".committee-calendar .header-container .actions-cell .btn", Flow11Phone)]
    [InlineData(".committee-calendar .header-container .actions-cell", Flow11Phone)]
    [InlineData(".committee-calendar .clinic-table--stack-wide .btn", Flow11Wide)]
    [InlineData(".msf-page .clinic-table--stack-wide td .actions-cell .btn", Flow11Wide)]
    public void AScopedCopy_FoldedIntoTheGeneralRule_IsGone(string selector, string media)
    {
        Stylesheet.AppCss().RulesFor(selector, media).Should().BeEmpty();
    }

    /// <summary>
    /// M7: a result, a notice or a refusal naming a 64-character address breaks inside it rather than widen the page at
    /// 390 (N21-390).
    /// </summary>
    [Theory]
    [InlineData(".alert")]
    [InlineData(".validation-message")]
    public void AnAlertOrAFieldsRefusal_BreaksALongAddress(string selector)
    {
        Stylesheet.AppCss().Computed(selector)["overflow-wrap"].Should().Be("anywhere");
    }

    /// <summary>S20: the loading skeleton's sizes are classes, so no page writes an inline style for them.</summary>
    [Fact]
    public void TheLoadingSkeletonsSizes_AreClasses()
    {
        var css = Stylesheet.AppCss();

        css.Computed(".skel-title")["height"].Should().Be("1.25rem");
        css.Computed(".skel-title")["width"].Should().Be("30%");
        css.Computed(".skel-line")["height"].Should().Be("1rem");
        css.Computed(".skel-short")["width"].Should().Be("60%");
        css.Computed(".loading-line")["color"].Should().Be("var(--muted-text)");
    }

    /// <summary>The two Lucide icons the wait card and its links draw (round 3 check, carried 6), in the icons' format.</summary>
    [Theory]
    [InlineData("hourglass")]
    [InlineData("arrow-right")]
    public void TheWaitCardsIcons_Exist(string name)
    {
        var svg = File.ReadAllText(Stylesheet.WebFile("wwwroot", "icons", $"{name}.svg"));

        svg.Should().StartWith("<svg id=\"i\" xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\">");
        svg.Should().Contain("<path d=\"");
    }
}
