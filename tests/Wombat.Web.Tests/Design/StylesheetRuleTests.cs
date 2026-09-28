using System.Text.RegularExpressions;
using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T335 flow 01's token and component rules, and T322's and T328's stylesheet halves: colours, radii, shadows and motion
/// come from <c>:root</c>; the focus ring, the badges, the buttons' heights and reduced motion are as DESIGN.md states.
/// </summary>
public sealed partial class StylesheetRuleTests
{
    // ---- tokens ----

    [Fact]
    public void Root_DeclaresTheLightScheme()
        => Stylesheet.AppCss().RulesFor(":root").Should().Contain(rule => rule.Value("color-scheme") == "light",
            "dark mode is later (W-011); until then the browser's own controls and scrollbars are the light ones");

    [Fact]
    public void NoColour_IsWrittenOutsideRoot()
    {
        // DESIGN.md § Non-negotiables. A colour inside a quoted data URI (the select's chevron, a mask's icon) is paint for
        // an image, not a colour of the page, and a system colour in a forced-colours block is the theme's own.
        var found = Stylesheet.AppCss().Rules
            .Where(rule => rule.Selector != ":root" && !rule.AtRule.StartsWith("@media (forced-colors", StringComparison.Ordinal))
            .SelectMany(rule => rule.Declarations.Select(declaration => (rule.Selector, Declaration: declaration)))
            .Where(item => RawColour().IsMatch(QuotedString().Replace(item.Declaration.Value, string.Empty)))
            .Select(item => $"{item.Selector} {{ {item.Declaration} }}")
            .ToList();

        found.Should().BeEmpty("every colour is a token from :root (T322: lightyellow, the dialog's backdrop and the sign-in page's wash were literals)");
    }

    [Fact]
    public void EveryTokenAStylesheetReads_IsDefined()
    {
        // T328: app.css read --text-muted, which :root never defined, so the SSO divider's words fell back to the inherited
        // colour. --accent-color and --info-color were removed in T335; a stylesheet still reading either fails here.
        var appCss = Stylesheet.WebFile("wwwroot", "app.css");
        var files = Directory.EnumerateFiles(Stylesheet.WebFile("Components"), "*.razor.css", SearchOption.AllDirectories).Prepend(appCss);
        var defined = new HashSet<string>(Definitions(File.ReadAllText(appCss)), StringComparer.Ordinal);

        var undefined = files
            .SelectMany(file =>
            {
                var css = CssComment().Replace(File.ReadAllText(file), string.Empty);
                var own = Definitions(css).ToHashSet(StringComparer.Ordinal);
                return VarRead().Matches(css).Select(match => match.Groups["name"].Value)
                    .Where(name => !defined.Contains(name) && !own.Contains(name))
                    .Select(name => $"{Path.GetFileName(file)}: {name}");
            })
            .Distinct()
            .ToList();

        undefined.Should().BeEmpty("a var() naming nothing falls back silently");
    }

    [Fact]
    public void EveryRadius_IsATokenOnTheScale()
    {
        var root = Stylesheet.AppCss().Root();
        root.Should().Contain(new Dictionary<string, string>
        {
            ["--radius-sm"] = "4px",
            ["--radius-md"] = "6px",
            ["--radius-lg"] = "8px",
            ["--radius-xl"] = "12px",
            ["--radius-pill"] = "999px",
        });

        Stylesheet.AppCss().Rules.Where(rule => rule.Selector != ":root")
            .SelectMany(rule => rule.Declarations.Where(declaration => declaration.Property == "border-radius").Select(declaration => $"{rule.Selector} {{ {declaration} }}"))
            .Where(text => !RadiusValue().IsMatch(text))
            .Should().BeEmpty("a radius is --radius-sm/-md/-lg/-xl/-pill, or 50% for a dot (N3)");
    }

    [Theory]
    [InlineData(".detail-card", "var(--shadow-raised)")]
    [InlineData(".form-container", "var(--shadow-raised)")]
    [InlineData(".account-form-container", "var(--shadow-raised)")]
    [InlineData(".shadow", "var(--shadow-raised)")]
    [InlineData(".dialog-card", "var(--shadow-dialog)")]
    public void ASurface_CastsItsRolesShadow(string selector, string shadow)
        => Stylesheet.AppCss().Computed(selector)["box-shadow"].Should().Be(shadow);

    [Fact]
    public void TheShadowTokens_KeepShadowColour()
    {
        var root = Stylesheet.AppCss().Root();
        root["--shadow-color"].Should().Be("rgb(0 0 0 / 0.1)");
        root["--shadow-raised"].Should().Be("0 1px 3px var(--shadow-color)");
        root["--shadow-dialog"].Should().Be("0 10px 25px rgb(0 0 0 / 0.25)");
        root["--shadow-bar"].Should().Be("0 -1px 2px rgb(0 0 0 / 0.2)");
    }

    [Fact]
    public void ALink_DarkensUnderThePointer_UnlessAClassColoursIt()
    {
        // As specific as "a" and no more: the brand on the sidebar's gradient, a nav item and a button's variant each keep
        // their own colour under the pointer, where "a:hover" would have turned the white brand dark blue on dark blue.
        Stylesheet.AppCss().RulesFor("a:where(:hover)").Should().ContainSingle().Which
            .Value("color").Should().Be("var(--link-hover)");
        Stylesheet.AppCss().Rules.Should().NotContain(rule => rule.Selectors.Contains("a:hover"));
    }

    // ---- focus ----

    [Fact]
    public void EveryFocusableElement_ShowsTheRing_AFocusableRegionIncluded()
    {
        // T328: Decisions Due's scrollable summary (.table-container[tabindex="0"]) showed the browser's 1px ring, because
        // the ring was a list of classes. It is now every element's, so a region, a tab and a checkbox get it too.
        var ring = Stylesheet.AppCss().RulesFor(":focus-visible").Should().ContainSingle().Which;

        ring.Value("outline").Should().Be("2px solid var(--focus-ring)");
        ring.Value("outline-offset").Should().Be("2px");
    }

    [Fact]
    public void NoRule_TakesTheRingAway_ButTheScriptFocusedHeadings()
    {
        // FocusOnNavigate focuses the h1 with tabindex="-1" for the screen reader; no keyboard reaches it. Nothing else may.
        Stylesheet.AppCss().Rules
            .Where(rule => rule.Declarations.Any(declaration => declaration.Property == "outline" && declaration.Value is "none" or "0"))
            .Select(rule => rule.Selector)
            .Should().BeEquivalentTo(["h1[tabindex=\"-1\"]:focus"]);
    }

    // ---- components ----

    [Fact]
    public void TheHeadersActions_StandOnTheHeadingsBaseline()
    {
        // T335, flow 01: at flex-start, Home's "Log an activity" (and every page's header action) sat at the top of the
        // header, 16px above the heading, which keeps the browser's own top margin.
        Stylesheet.AppCss().Computed(".header-container").Should().Contain("align-items", "baseline");
    }

    [Fact]
    public void TheHeader_IsSpacedAsTheBoardsDrawIt()
    {
        // The review of the t335 branch: the heading has no margin of its own (it kept the browser's top margin and 16px
        // under it), the subtitle keeps its own small gap, the rule is 12px under them, and the page begins 24px under the
        // rule (it was 16px and 32px). A dashboard card's title keeps no top margin either (an h2's is the browser's).
        var css = Stylesheet.AppCss();

        var header = css.Computed(".header-container");
        header["padding-bottom"].Should().Be("0.75rem");
        header["margin-bottom"].Should().Be("var(--space-lg)");
        css.Computed(".header-container h1")["margin"].Should().Be("0");
        css.Computed(".page-subtitle")["margin"].Should().Be("var(--space-xs) 0 0", "the subtitle's own small gap under the heading");
        css.Computed(".dashboard-card-title")["margin-top"].Should().Be("0");
    }

    [Fact]
    public void ABadge_KeepsItsPill_InAFlexRow()
    {
        // T328: on the dashboards' lists a badge beside a wrapping link stretched to the row's height.
        var badge = Stylesheet.AppCss().Computed(".badge");

        badge.Should().Contain(new Dictionary<string, string>
        {
            ["flex"] = "none",
            ["align-self"] = "center",
            ["white-space"] = "nowrap",
            ["border-radius"] = "var(--radius-pill)",
            ["color"] = "var(--text-color)",
        });
    }

    [Fact]
    public void Buttons_ShareOneHeight_AndSmallOnesStayTargets()
    {
        var css = Stylesheet.AppCss();
        var button = css.Computed(".btn");

        Stylesheet.Rem(button["min-height"]).Should().Be(2.25, "header actions share 36px, a <button> and an <a> alike");
        button["border"].Should().Be("1px solid transparent",
            "every button carries the outline's 1px edge, so a filled one stands exactly as tall as an outline one");
        button["box-sizing"].Should().Be("border-box",
            "as a browser sizes a <button>: an <a class=\"btn\"> sized its content box, and stood 46px beside a 36px <button>");
        Stylesheet.Rem(css.Computed(".btn", ".btn-sm")["min-height"]).Should().BeGreaterThanOrEqualTo(1.75, "T086: 24px and more");
        Stylesheet.Rem(css.Computed(".btn", ".btn-xs")["min-height"]).Should().BeGreaterThanOrEqualTo(1.75, "T086: 24px and more");
    }

    [Fact]
    public void ThePasswordToggle_IsATarget()
    {
        // T328: 45x23px, under T086's floor, on the sign-in card and the user page.
        // Since T339 (flow 02) it is a button beside the field and as tall as it (38px), where it was a word inside the
        // field's end (.password-toggle-btn).
        Stylesheet.Rem(Stylesheet.AppCss().Computed(".btn", ".btn-outline", ".password-toggle")["min-height"]).Should().Be(2.375);
    }

    [Fact]
    public void AnInvalidField_TakesTheSheetsStripe()
        => Stylesheet.AppCss().Rules.Should().Contain(rule => rule.AtRule.Length == 0 && rule.Selectors.Contains(".input-validation-error")
            && rule.Value("box-shadow") == "inset 4px 0 0 var(--danger-color)");

    [Fact]
    public void AValidationMessage_CarriesAnIcon()
    {
        var icon = Stylesheet.AppCss().Computed(".validation-message::before");

        icon["background-color"].Should().Be("currentColor", "the icon is the message's own danger colour");
        icon["mask"].Should().StartWith("url(\"data:image/svg+xml,", "an inline Lucide glyph: no Bootstrap Icons font");
    }

    // ---- motion ----

    private const string ReducedMotion = "@media (prefers-reduced-motion: reduce)";

    [Fact]
    public void EveryTransition_RunsAtTheMotionToken()
    {
        // The reduced-motion block is the one place a duration is written out: it sets every one to 0.
        Stylesheet.AppCss().Root()["--motion-fast"].Should().Be("150ms");
        Stylesheet.AppCss().Rules
            .Where(rule => rule.AtRule != ReducedMotion)
            .SelectMany(rule => rule.Declarations.Where(declaration => declaration.Property.StartsWith("transition", StringComparison.Ordinal))
                .Select(declaration => $"{rule.Selector} {{ {declaration} }}"))
            .Where(text => Duration().IsMatch(text))
            .Should().BeEmpty("a transition's duration is var(--motion-fast), never a number of its own (S17)");
    }

    [Fact]
    public void ReducedMotion_StopsTheMotion_AndStillsTheSkeleton()
    {
        var css = Stylesheet.AppCss();

        var everything = css.RulesFor("*", ReducedMotion).Should().ContainSingle().Which;
        everything.Selectors.Should().BeEquivalentTo(["*", "*::before", "*::after"]);
        everything.Declarations.Should().Contain(
        [
            new CssDeclaration("transition-duration", "0s", Important: true),
            new CssDeclaration("animation-duration", "0.01ms", Important: true),
            new CssDeclaration("animation-iteration-count", "1", Important: true),
        ], "every transition goes to 0, and every animation jumps to where it ends");

        var skeleton = css.RulesFor(".skeleton", ReducedMotion).Should().ContainSingle().Which;
        skeleton.Value("animation").Should().Be("none");
        skeleton.Value("background").Should().Be("var(--header-bg)", "a still header-bg block (S17)");
    }

    // ---- helpers ----

    private static IEnumerable<string> Definitions(string css) => Definition().Matches(css).Select(match => match.Groups["name"].Value);

    [GeneratedRegex(@"#[0-9a-fA-F]{3,8}\b|\b(rgba?|hsla?|hwb|lab|lch|oklab|oklch)\(|\b(white|black|red|green|blue|yellow|lightyellow|orange|gray|grey|silver)\b", RegexOptions.IgnoreCase)]
    private static partial Regex RawColour();

    [GeneratedRegex(@"""[^""]*""|'[^']*'")]
    private static partial Regex QuotedString();

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex CssComment();

    [GeneratedRegex(@"var\(\s*(?<name>--[\w-]+)")]
    private static partial Regex VarRead();

    [GeneratedRegex(@"(?<![\w-])(?<name>--[\w-]+)\s*:")]
    private static partial Regex Definition();

    [GeneratedRegex(@"border-radius: (var\(--radius-(sm|md|lg|xl|pill)\)|50%|0)( !important)? \}$")]
    private static partial Regex RadiusValue();

    [GeneratedRegex(@"(?<![\w.-])\d*\.?\d+m?s\b")]
    private static partial Regex Duration();
}
