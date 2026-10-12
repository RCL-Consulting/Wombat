using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T361 (flow 07, lane R; round 2 review 14, 15; round 3 items 9, 14, 15; T356): the shell's stacking and the focus's
/// clearance under the bars. In a file of its own so the lanes do not meet in one test file.
/// </summary>
public sealed partial class StylesheetRuleTests
{
    private const string Flow07Wide = "@media (min-width: 641px)";
    private const string Flow07Phone = "@media (max-width: 640.98px)";

    /// <summary>
    /// T356's cause (review 14): the sidebar is sticky with no z-index, so a positioned .table-container later in the page
    /// painted over the fixed top bar inside it, Sign out included. From 641px it is 10, above the chair's sitting bar's 1;
    /// below, the phone bar is not fixed and takes none.
    /// </summary>
    [Fact]
    public void TheSidebar_StacksAboveThePage_From641px()
    {
        var layout = Stylesheet.Parse(File.ReadAllText(Stylesheet.WebFile("Components", "Layout", "MainLayout.razor.css")));

        var sidebar = layout.Should().ContainSingle(rule => rule.AtRule == Flow07Wide && rule.Selector == ".sidebar").Which;
        sidebar.Value("position").Should().Be("sticky");
        sidebar.Value("z-index").Should().Be("10");

        layout.Where(rule => rule.AtRule != Flow07Wide && rule.Selectors.Contains(".sidebar"))
            .Should().NotContain(rule => rule.Declarations.Any(declaration => declaration.Property == "z-index"));

        // The top bar inside it keeps its own order within the sidebar, and nothing in app.css outranks the sidebar but the
        // error bar (1000, MainLayout's) and the top layer the dialogs open in.
        layout.Should().ContainSingle(rule => rule.AtRule == Flow07Wide && rule.Selector == ".account-row")
            .Which.Value("position").Should().Be("fixed");
        Stylesheet.AppCss().Rules
            .SelectMany(rule => rule.Declarations.Where(declaration => declaration.Property == "z-index")
                .Select(declaration => (rule.Selector, Value: int.Parse(declaration.Value, System.Globalization.CultureInfo.InvariantCulture))))
            .Should().OnlyContain(entry => entry.Value < 10, "the page's own stacking stays under the shell's sidebar");
    }

    /// <summary>
    /// Review 15 (WCAG 2.4.11): from 641px, html reserves the fixed top bar and the chair's stuck bar, measured into
    /// --sitting-bar-h by js/sticky-offset.js. Below 641px no bar is fixed, so nothing is reserved.
    /// </summary>
    [Fact]
    public void ScrollPadding_ClearsTheTopBarAndTheChairsStuckBar_From641px()
    {
        var css = Stylesheet.AppCss();

        css.RulesFor("html", Flow07Wide).Should().ContainSingle()
            .Which.Value("scroll-padding-top").Should().Be("calc(56px + var(--sitting-bar-h, 0px))");
        css.RulesFor("html").Should().NotContain(rule => rule.Declarations.Any(declaration => declaration.Property == "scroll-padding-top"));
    }

    /// <summary>
    /// The script measures the chair's bar alone, the one that sticks, and only from 641px; it is a file, so the CSP's
    /// script-src 'self' admits it, and it writes the property through the CSSOM.
    /// </summary>
    [Fact]
    public void TheStickyOffsetScript_MeasuresTheChairsBar_IntoTheCustomProperty()
    {
        var script = File.ReadAllText(Stylesheet.WebFile("wwwroot", "js", "sticky-offset.js"));

        script.Should().Contain("\".sitting-bar:not(.sitting-bar--reader)\"");
        script.Should().Contain("\"(min-width: 641px)\"");
        script.Should().Contain("setProperty(\"--sitting-bar-h\"");
        script.Should().Contain("ResizeObserver");
        script.Should().NotContain("eval(").And.NotContain("innerHTML");

        File.ReadAllText(Stylesheet.WebFile("Components", "App.razor"))
            .Should().Contain("<script src=\"@Assets[\"js/sticky-offset.js\"]\"></script>");
    }

    /// <summary>
    /// Round 3 item 9: the review's way back on flow 03's page. A 44px row below 641px, where it replaces the trail's back
    /// crumb: one back link, never two.
    /// </summary>
    [Fact]
    public void TheReturnLink_Is44pxOnAPhone_WhereTheTrailGoes()
    {
        var css = Stylesheet.AppCss();

        css.Cascaded(".return-link")["display"].Should().Be("inline-flex");
        css.CascadedIn(Flow07Phone, ".return-link")["min-height"].Should().Be("44px");
        css.CascadedIn(Flow07Phone, ".activity-page--from-review nav:has(> .breadcrumbs)")["display"].Should().Be("none");
        css.Cascaded(".activity-page--from-review nav:has(> .breadcrumbs)").Should().NotContainKey("display",
            "from 641px the trail stays under the return link");
    }

    /// <summary>
    /// Review D2 (Q9, round 3 item 20): below 641px every target the chair presses mid-sitting is 44px. Stage pending
    /// decision and Cancel, "Show the other N snapshot lines" and the appeal's buttons; and the snapshot and Present ticks,
    /// each a 2.75rem row whose label fills it. Every rule stays in the review sitting's own section.
    /// </summary>
    [Theory]
    [InlineData(".agenda-form .form-actions .btn")]
    [InlineData(".snapshot-pick-more")]
    [InlineData(".review-appeals .form-actions .btn")]
    public void TheSittingsButtons_Are44pxOnAPhone(string selector)
        => Stylesheet.AppCss().CascadedIn(Flow07Phone, selector)["min-height"].Should().Be("44px");

    [Theory]
    [InlineData(".review-page .snapshot-pick-line")]
    [InlineData(".review-page .check-grid")]
    public void TheSnapshotAndPresentTicks_AreRowsTheLabelFills_OnAPhone(string scope)
    {
        var css = Stylesheet.AppCss();

        css.CascadedIn(Flow07Phone, $"{scope} .form-check")["min-height"].Should().Be("2.75rem");
        var label = css.CascadedIn(Flow07Phone, $"{scope} .form-check label");
        label["min-height"].Should().Be("2.75rem");
        label["flex"].Should().Be("1");
        css.Cascaded($"{scope} .form-check label").Should().NotContainKey("min-height", "from 641px the ticks keep their size");
    }
}
