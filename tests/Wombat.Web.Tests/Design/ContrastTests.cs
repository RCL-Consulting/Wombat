using System.Globalization;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T322, T335 flow 01: every colour pair of the design system meets WCAG 2.1 AA, computed from <c>app.css</c>'s
/// <c>:root</c>, and DESIGN.md § Design tokens states the figures this computes.
/// </summary>
/// <remarks>
/// <para>
/// Until T322 the semantic colours were text on their own tints (success 2.55:1, warning 2.42:1, danger 3.57:1), white on
/// the success and danger buttons was 2.87:1 and 3.82:1, the focus ring 2.99:1 on the page and an input's border 1.49:1.
/// The round-2 review of flow 01 found the token sheet's own figures slightly off where a translucent colour met a
/// gradient (N2). So nothing here is a figure copied from anywhere: each is computed.
/// </para>
/// <para>
/// The method is DESIGN.md's: WCAG 2.1 relative luminance on the token values. A translucent colour is blended over the
/// ground it actually sits on, unrounded, before it is measured. The sidebar's gradient is sampled at 21 points from its
/// start to its end, and the worst sample is the one that must pass; the tables print its start, middle and end. Text
/// needs 4.5:1; a control's edge, the focus ring and a meaningful mark 3:1 (WCAG 1.4.3, 1.4.11).
/// </para>
/// </remarks>
public sealed partial class ContrastTests
{
    private const double Text = 4.5;
    private const double Mark = 3.0;

    // ---- the two tables ----

    [Theory]
    [MemberData(nameof(PageRows))]
    [MemberData(nameof(DarkChromeRows))]
    public void EveryPair_MeetsItsMinimum(string label)
    {
        var palette = Palette.FromAppCss();
        var row = AllRows().Single(row => row.Label == label);

        foreach (var measure in row.Measures.Where(measure => measure.Minimum is not null))
        {
            var worst = measure.Worst(palette);
            if (row.Forbidden)
            {
                worst.Should().BeLessThan(measure.Minimum!.Value,
                    $"'{label}' is recorded as forbidden because it fails; if the tokens now pass it, DESIGN.md should say so");
            }
            else
            {
                worst.Should().BeGreaterThanOrEqualTo(measure.Minimum!.Value, $"'{label}' needs {measure.Minimum}:1");
            }
        }
    }

    [Theory]
    [MemberData(nameof(PageRows))]
    [MemberData(nameof(DarkChromeRows))]
    public void DesignMd_StatesTheComputedFigures(string label)
    {
        var palette = Palette.FromAppCss();
        var row = AllRows().Single(row => row.Label == label);
        var line = row.MarkdownLine(palette);

        File.ReadAllLines(Stylesheet.SolutionFile("execution", "architecture", "DESIGN.md"))
            .Where(text => text.StartsWith($"| {label} |", StringComparison.Ordinal))
            .Should().ContainSingle("DESIGN.md § Design tokens has one row for the pair").Which
            .Should().Be(line, "the row states the figures computed from app.css");
    }

    public static TheoryData<string> PageRows() => [.. Page().Select(row => row.Label)];

    public static TheoryData<string> DarkChromeRows() => [.. DarkChrome().Select(row => row.Label)];

    private static IEnumerable<Row> AllRows() => Page().Concat(DarkChrome());

    private static readonly string[] Plain = ["--surface-color", "--background-color", "--header-bg"];
    private static readonly string[] Tints = ["--success-bg", "--warning-bg", "--danger-bg", "--info-bg"];

    /// <summary>DESIGN.md's page table: each pair on a solid ground.</summary>
    private static IEnumerable<Row> Page()
    {
        static Row On(string label, string foreground, string[] grounds, double? minimum)
            => new(label, [.. grounds.Select(ground => Measure.Solid(foreground, ground, minimum))]);

        yield return On("`--text-color` on surface · background · header-bg", "--text-color", Plain, Text);
        yield return On("`--text-color` on the success · warning · danger · info tints", "--text-color", Tints, Text);
        yield return On("`--muted-text` on surface · background · header-bg", "--muted-text", Plain, Text);
        yield return On("`--muted-text` on the success · warning · danger · info tints", "--muted-text", Tints, Text);
        yield return On("`--link-color` on surface · background · header-bg", "--link-color", Plain, Text);
        yield return On("`--link-color` on the success · warning · danger · info tints", "--link-color", Tints, Text);
        yield return On("`--link-hover` on surface", "--link-hover", ["--surface-color"], Text);
        yield return On("`--primary-color` on surface", "--primary-color", ["--surface-color"], Text);
        yield return new Row("`--on-fill` on secondary · success · danger (the filled buttons)",
            [Measure.Solid("--on-fill", "--secondary-color", Text), Measure.Solid("--on-fill", "--success-color", Text),
             Measure.Solid("--on-fill", "--danger-color", Text)]);
        yield return On("`--secondary-color` (an outline button's label and edge) on surface · background", "--secondary-color",
            ["--surface-color", "--background-color"], Text);
        yield return On("`--danger-color` (validation text) on surface · background", "--danger-color",
            ["--surface-color", "--background-color"], Text);
        yield return On("`--input-border` on surface · background · header-bg", "--input-border", Plain, Mark);
        yield return On("`--input-border` on the success · warning · danger · info tints", "--input-border", Tints, Mark);
        yield return On("`--focus-ring` on surface · background · header-bg", "--focus-ring", Plain, Mark);
        yield return On("`--focus-ring` on the success · warning · danger · info tints", "--focus-ring", Tints, Mark);
        yield return new Row("Tint edges and icons: success · warning · danger · info (`--secondary-color`) on their tints",
            [Measure.Solid("--success-color", "--success-bg", Mark), Measure.Solid("--warning-color", "--warning-bg", Mark),
             Measure.Solid("--danger-color", "--danger-bg", Mark), Measure.Solid("--secondary-color", "--info-bg", Mark)]);
        yield return new Row("Neutral badge: text · edge on header-bg",
            [Measure.Solid("--text-color", "--header-bg", Text), Measure.Solid("--input-border", "--header-bg", Mark)]);
        yield return new Row("Status dots on surface: success · warning · danger",
            [Measure.Solid("--success-color", "--surface-color", Mark), Measure.Solid("--warning-color", "--surface-color", Mark),
             Measure.Solid("--danger-color", "--surface-color", Mark)]);
        yield return new Row("Progress fill on its hover-bg track: secondary · success (complete)",
            [Measure.Solid("--secondary-color", "--hover-bg", Mark), Measure.Solid("--success-color", "--hover-bg", Mark)]);
        yield return On("Reconnect bar: secondary on its header-bg track", "--secondary-color", ["--header-bg"], Mark);
        yield return new Row("Error bar: text · the warning edge and icon on warning-bg",
            [Measure.Solid("--text-color", "--warning-bg", Text), Measure.Solid("--warning-color", "--warning-bg", Mark)]);
        yield return On("`--border-color` on surface (a hairline, never a control's edge)", "--border-color",
            ["--surface-color"], null);
    }

    /// <summary>
    /// DESIGN.md's dark-chrome table: each pair over the sidebar's gradient (start · middle · end), a translucent token
    /// blended over it first.
    /// </summary>
    private static IEnumerable<Row> DarkChrome()
    {
        yield return new Row("`--nav-text` on the gradient", [Measure.OverGradient("--nav-text", null, Text)]);
        yield return new Row("`--nav-text-strong` on the gradient (the brand, the role name, the signed-out bar's text)",
            [Measure.OverGradient("--nav-text-strong", null, Text)]);
        yield return new Row("`--nav-group-label` on the gradient (group headings, \"Acting as\", the switch's and the toggle's edges)",
            [Measure.OverGradient("--nav-group-label", null, Text)]);
        yield return new Row("`--nav-text-strong` on `--nav-active-bg` over the gradient (the current item)",
            [Measure.OverGradient("--nav-text-strong", "--nav-active-bg", Text)]);
        yield return new Row("`--nav-text` on `--nav-active-bg` over the gradient",
            [Measure.OverGradient("--nav-text", "--nav-active-bg", Text)], Forbidden: true);
        yield return new Row("`--nav-text-strong` on `--nav-hover-bg` over the gradient (a hovered item)",
            [Measure.OverGradient("--nav-text-strong", "--nav-hover-bg", Text)]);
        yield return new Row("`--nav-focus-ring` on the gradient", [Measure.OverGradient("--nav-focus-ring", null, Mark)]);
        yield return new Row("`--nav-focus-ring` on the current item (`--nav-active-bg` over the gradient)",
            [Measure.OverGradient("--nav-focus-ring", "--nav-active-bg", Mark)]);
        yield return new Row("`--nav-text-strong` on `--nav-brand-bg` over the gradient's start (the brand cell)",
            [Measure.OverStart("--nav-text-strong", "--nav-brand-bg", Text)]);
        yield return new Row("Phone bar (the gradient's start, solid): `--nav-text-strong` · `--nav-group-label` · `--nav-focus-ring`",
            [Measure.OverStart("--nav-text-strong", null, Text), Measure.OverStart("--nav-group-label", null, Text),
             Measure.OverStart("--nav-focus-ring", null, Mark)]);
        yield return new Row("Phone menu foot (the gradient's end, solid): `--nav-text-strong` · `--nav-group-label` · `--nav-focus-ring`",
            [Measure.OverEnd("--nav-text-strong", null, Text), Measure.OverEnd("--nav-group-label", null, Mark),
             Measure.OverEnd("--nav-focus-ring", null, Mark)]);
    }

    // ---- the components, as app.css's rules paint them ----

    [Theory]
    [InlineData("success")]
    [InlineData("warning")]
    [InlineData("danger")]
    [InlineData("info")]
    public void AnAlert_IsBodyTextOnItsTint_WithTheSemanticColourOnItsEdgeAndIcon(string kind)
    {
        var css = Stylesheet.AppCss();
        var palette = Palette.FromAppCss();
        var alert = css.Computed(".alert", $".alert-{kind}");
        var ground = palette.Resolve(alert["background-color"]);

        palette.Ratio(palette.Resolve(alert["color"]), ground).Should().BeGreaterThanOrEqualTo(Text,
            "an alert's words are read on its tint (T322: the semantic colour on its own tint was 2.42 to 3.57:1)");
        alert["color"].Should().Be("var(--text-color)", "every alert's words are body text on the tint (DESIGN.md § Alerts)");
        palette.Ratio(palette.Resolve(ColourIn(alert["border-color"])), ground).Should().BeGreaterThanOrEqualTo(Mark,
            "the edge is the kind's cue");

        var icon = css.Computed($".alert-{kind}::before");
        palette.Ratio(palette.Resolve(icon["background-color"]), ground).Should().BeGreaterThanOrEqualTo(Mark,
            "the icon is a meaningful mark");
    }

    [Theory]
    [InlineData("badge-draft")]
    [InlineData("badge-submitted")]
    [InlineData("badge-accepted")]
    [InlineData("badge-completed")]
    [InlineData("badge-declined")]
    [InlineData("badge-standing-met")]
    [InlineData("badge-standing-below")]
    [InlineData("badge-standing-none")]
    public void ABadge_IsReadable_OnItsTint(string badge)
    {
        var palette = Palette.FromAppCss();
        var style = Stylesheet.AppCss().Computed(".badge", $".{badge}");

        palette.Ratio(palette.Resolve(style["color"]), palette.Resolve(Background(style))).Should()
            .BeGreaterThanOrEqualTo(Text, $".{badge}'s words are 0.75rem text");
    }

    [Theory]
    [InlineData(".validation-message")]
    [InlineData(".text-danger")]
    public void DangerText_IsReadable_OnSurfaceAndPage(string selector)
    {
        var palette = Palette.FromAppCss();
        var colour = palette.Resolve(Stylesheet.AppCss().Computed(selector)["color"]);

        palette.Ratio(colour, palette.Token("--surface-color")).Should().BeGreaterThanOrEqualTo(Text);
        palette.Ratio(colour, palette.Token("--background-color")).Should().BeGreaterThanOrEqualTo(Text);
    }

    [Theory]
    [InlineData(".validation-summary-errors")]
    [InlineData(".field-warning")]
    public void AFormLevelMessage_IsReadable_OnItsTint(string selector)
    {
        var palette = Palette.FromAppCss();
        var style = Stylesheet.AppCss().Computed(selector);

        palette.Ratio(palette.Resolve(style["color"]), palette.Resolve(Background(style))).Should().BeGreaterThanOrEqualTo(Text);
    }

    [Theory]
    [InlineData(".btn-primary")]
    [InlineData(".btn-success")]
    [InlineData(".btn-danger")]
    public void AFilledButton_TakesOnFill_AtTextContrast(string variant)
    {
        var palette = Palette.FromAppCss();
        var style = Stylesheet.AppCss().Computed(".btn", variant);

        style["color"].Should().Be("var(--on-fill)", "text on a filled button is --on-fill (DESIGN.md § Button system)");
        palette.Ratio(palette.Resolve(style["color"]), palette.Resolve(Background(style))).Should()
            .BeGreaterThanOrEqualTo(Text, "T322: white on the success and danger fills was 2.87 and 3.82:1");
    }

    [Fact]
    public void AnOutlineButton_IsFilledWithTheSurface()
    {
        // S16: transparent, it read 4.37:1 on header-bg and 4.32:1 on a success tint, and passed by 0.07 on the error bar.
        var palette = Palette.FromAppCss();
        var style = Stylesheet.AppCss().Computed(".btn", ".btn-outline");

        Background(style).Should().Be("var(--surface-color)", "an outline button carries its own ground wherever it sits");
        palette.Ratio(palette.Resolve(style["color"]), palette.Token("--surface-color")).Should().BeGreaterThanOrEqualTo(Text);
        palette.Ratio(palette.Resolve(ColourIn(style["border-color"])), palette.Token("--background-color")).Should()
            .BeGreaterThanOrEqualTo(Mark, "its edge is the control's boundary on the page");
    }

    [Fact]
    public void TheActiveTab_TakesOnFill()
    {
        var palette = Palette.FromAppCss();
        var style = Stylesheet.AppCss().Computed(".tab-bar-tab.is-active");

        palette.Ratio(palette.Resolve(style["color"]), palette.Resolve(Background(style))).Should().BeGreaterThanOrEqualTo(Text);
    }

    [Fact]
    public void TheFocusRing_ShowsOnSurfaceAndPage()
    {
        var palette = Palette.FromAppCss();
        var ring = palette.Resolve(ColourIn(Stylesheet.AppCss().Computed(":focus-visible")["outline"]));

        palette.Ratio(ring, palette.Token("--surface-color")).Should().BeGreaterThanOrEqualTo(Mark);
        palette.Ratio(ring, palette.Token("--background-color")).Should().BeGreaterThanOrEqualTo(Mark,
            "T322: the old ring was 2.99:1 on the page");
    }

    [Theory]
    [InlineData(".form-control")]
    [InlineData(".form-select")]
    [InlineData(".search-input")]
    public void AControlsEdge_ShowsOnSurfaceAndPage(string control)
    {
        var palette = Palette.FromAppCss();
        var edge = palette.Resolve(ColourIn(Stylesheet.AppCss().Computed(control)["border"]));

        palette.Ratio(edge, palette.Token("--surface-color")).Should().BeGreaterThanOrEqualTo(Mark,
            "T322: an input's edge was 1.49:1 on white");
        palette.Ratio(edge, palette.Token("--background-color")).Should().BeGreaterThanOrEqualTo(Mark);
    }

    [Theory]
    [InlineData("ok")]
    [InlineData("warn")]
    [InlineData("err")]
    public void AStatusDot_ShowsOnSurface(string state)
    {
        var palette = Palette.FromAppCss();
        var dot = palette.Resolve(Background(Stylesheet.AppCss().Computed($".status-dot.{state}")));

        palette.Ratio(dot, palette.Token("--surface-color")).Should().BeGreaterThanOrEqualTo(Mark);
    }

    [Theory]
    [InlineData(".progress-bar-fill")]
    [InlineData(".progress-bar-fill.is-complete")]
    public void AProgressFill_ShowsOnItsTrack(string fill)
    {
        var css = Stylesheet.AppCss();
        var palette = Palette.FromAppCss();

        palette.Ratio(palette.Resolve(Background(css.Computed(fill))), palette.Resolve(Background(css.Computed(".progress-bar"))))
            .Should().BeGreaterThanOrEqualTo(Mark, "a complete bar's green was 2.73:1 on its track");
    }

    // ---- the shell's own stylesheets (the review of the t335 branch) ----
    //
    // The two tables measure the tokens; nothing here read the rules the shell's scoped stylesheets paint with, so a rule
    // that paired the wrong tokens passed. These compute each pair from the rule that paints it, over the ground it sits
    // on: the gradient sampled at 21 points for the sidebar, its start for the phone bar, its end for the open menu's foot.

    private const string Phone = "@media (max-width: 640.98px)";

    [Fact]
    public void TheCurrentNavItem_ItsWordsAndItsBar_ReadOnItsFill_TheWholeGradientDown()
    {
        var palette = Palette.FromAppCss();
        var item = Scoped("NavItemLink.razor.css");
        var lit = item.Computed(".nav-link", ".nav-link.active");

        lit["color"].Should().Be("var(--nav-text-strong)", "--nav-text on the fill is the forbidden pair (3.66:1 at the start)");
        WorstOverGradient(palette, lit["color"], lit["background-color"]).Should().BeGreaterThanOrEqualTo(Text);
        WorstOverGradient(palette, ColourIn(lit["box-shadow"]), lit["background-color"]).Should().BeGreaterThanOrEqualTo(Mark,
            "the 3px bar is the state's cue (D4)");

        var rest = item.Computed(".nav-link");
        WorstOverGradient(palette, rest["color"], layer: null).Should().BeGreaterThanOrEqualTo(Text, "an item at rest");
        var hovered = item.Computed(".nav-link", ".nav-link:hover");
        WorstOverGradient(palette, hovered["color"], hovered["background-color"]).Should().BeGreaterThanOrEqualTo(Text, "a hovered item");
    }

    [Fact]
    public void ThePhoneBar_ItsBrandTheRoleAndTheToggle_ReadOnItsGround()
    {
        var palette = Palette.FromAppCss();
        var layout = Scoped("MainLayout.razor.css");
        var ground = palette.Resolve(layout.ComputedIn(Phone, ".sidebar")["background-color"]);
        ground.Should().Be(palette.Token("--sidebar-gradient-start"), "guard: the bar is the gradient's start, solid (R2-Tokens)");

        foreach (var words in new[] { ".brand", ".phone-role", ".phone-role-label", ".nav-toggle-label" })
        {
            palette.Ratio(palette.Resolve(layout.ComputedIn(Phone, words)["color"]), ground).Should().BeGreaterThanOrEqualTo(Text, words);
        }

        palette.Ratio(palette.Resolve(ColourIn(layout.ComputedIn(Phone, ".nav-toggle-label")["border"])), ground)
            .Should().BeGreaterThanOrEqualTo(Mark, "the toggle's edge is the control's boundary");
    }

    [Fact]
    public void TheRoleSwitch_ReadsOnTheGradient_AtRestAndHovered()
    {
        var palette = Palette.FromAppCss();
        var nav = Scoped("NavMenu.razor.css");

        var sw = nav.Computed(".role-switch");
        WorstOverGradient(palette, sw["color"], layer: null).Should().BeGreaterThanOrEqualTo(Text, "Switch to …, Change role");
        WorstOverGradient(palette, ColourIn(sw["border"]), layer: null).Should().BeGreaterThanOrEqualTo(Mark, "the switch's edge");
        var hovered = nav.Computed(".role-switch", ".role-switch:hover");
        WorstOverGradient(palette, hovered["color"], hovered["background-color"]).Should().BeGreaterThanOrEqualTo(Text);

        var option = nav.Computed(".role-option");
        WorstOverGradient(palette, option["color"], layer: null).Should().BeGreaterThanOrEqualTo(Text, "an option of Change role");
        var hoveredOption = nav.Computed(".role-option", ".role-option:hover");
        WorstOverGradient(palette, hoveredOption["color"], hoveredOption["background-color"]).Should().BeGreaterThanOrEqualTo(Text);

        foreach (var words in new[] { ".role-now-label", ".role-now-name", ".nav-group-heading" })
        {
            WorstOverGradient(palette, nav.Computed(words)["color"], layer: null).Should().BeGreaterThanOrEqualTo(Text, words);
        }
    }

    [Fact]
    public void TheOpenMenusFoot_TheNameAndSignOut_ReadOnItsGround()
    {
        var palette = Palette.FromAppCss();
        var layout = Scoped("MainLayout.razor.css");
        var ground = palette.Resolve(layout.ComputedIn(Phone, ".account-row")["background-color"]);
        ground.Should().Be(palette.Token("--sidebar-gradient-end"), "guard: the foot is the gradient's end, solid (D9)");

        palette.Ratio(palette.Resolve(layout.ComputedIn(Phone, ".account-link")["color"]), ground).Should().BeGreaterThanOrEqualTo(Text,
            "the name, which the bar at 641px paints --text-color on white");
        var signOut = layout.ComputedIn(Phone, ".sign-out-button");
        signOut["background-color"].Should().Be("transparent", "the button sits on the foot's own ground");
        palette.Ratio(palette.Resolve(signOut["color"]), ground).Should().BeGreaterThanOrEqualTo(Text);
        palette.Ratio(palette.Resolve(signOut["border-color"]), ground).Should().BeGreaterThanOrEqualTo(Mark);
        palette.Ratio(palette.Resolve(ColourIn(layout.ComputedIn(Phone, ".account-link[aria-current=\"page\"]")["box-shadow"])), ground)
            .Should().BeGreaterThanOrEqualTo(Mark, "My account's underline on the foot");
    }

    private static Stylesheet Scoped(string file) => Stylesheet.Load(Stylesheet.WebFile("Components", "Layout", file));

    /// <summary>
    /// The worst ratio of <paramref name="foreground" /> over the sidebar's gradient, sampled at 21 points, with
    /// <paramref name="layer" /> (a translucent fill) over it first; each a colour or a <c>var()</c>.
    /// </summary>
    private static double WorstOverGradient(Palette palette, string foreground, string? layer)
        => Enumerable.Range(0, 21).Select(step => step / 20.0).Min(t =>
        {
            var ground = layer is null ? palette.Gradient(t) : palette.Resolve(layer).Over(palette.Gradient(t));
            return palette.Ratio(palette.Resolve(foreground).Over(ground), ground);
        });

    // ---- the method itself ----

    [Fact]
    public void TheMethod_ReproducesWcagsOwnFigures()
    {
        var palette = Palette.FromAppCss();

        palette.Ratio(Rgb.Parse("rgb(0 0 0)"), Rgb.Parse("rgb(255 255 255)")).Should().BeApproximately(21, 0.001);
        palette.Ratio(Rgb.Parse("rgb(119 119 119)"), Rgb.Parse("rgb(255 255 255)")).Should().BeApproximately(4.48, 0.005,
            "#777 on white is the textbook near-miss");
        Rgb.Parse("rgb(255 255 255 / .32)").Over(Rgb.Parse("rgb(5 39 103)")).Hex.Should().Be("#556C98",
            "the round-3 sheet's swatch for the current item at the gradient's start");
        Rgb.Parse("#201657").Should().Be(new Rgb(32, 22, 87, 1));
    }

    // ---- helpers ----

    /// <summary>A background declared either way: <c>background-color</c>, or a <c>background</c> that is one colour.</summary>
    private static string Background(IReadOnlyDictionary<string, string> style)
        => style.TryGetValue("background-color", out var colour) ? colour
            : style.TryGetValue("background", out var shorthand) ? shorthand
            : throw new InvalidOperationException("The rule sets no background.");

    /// <summary>The colour in a shorthand (<c>border: 1px solid var(--input-border)</c>): its <c>var()</c> or colour function.</summary>
    private static string ColourIn(string value)
    {
        var match = ColourToken().Match(value);
        return match.Success ? match.Value : throw new FormatException($"'{value}' names no colour.");
    }

    [GeneratedRegex(@"var\(--[\w-]+\)|rgba?\([^)]*\)|#[0-9a-fA-F]{3,8}\b")]
    private static partial Regex ColourToken();

    private sealed record Row(string Label, IReadOnlyList<Measure> Measures, bool Forbidden = false)
    {
        public string MarkdownLine(Palette palette)
        {
            var minimums = Measures.Select(measure => measure.Minimum).ToList();
            var needs = minimums.All(minimum => minimum is null) ? "none"
                : minimums.Distinct().Count() == 1 ? Format(minimums[0]!.Value)
                : string.Join(" · ", minimums.Select(minimum => minimum is null ? "none" : Format(minimum.Value)));
            var figures = string.Join(" · ", Measures.SelectMany(measure => measure.Figures(palette)).Select(Figure));
            return $"| {Label} | {needs} | {figures}{(Forbidden ? " (forbidden)" : string.Empty)} |";
        }

        private static string Format(double minimum) => minimum.ToString("0.0##", CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');

        private static string Figure(double ratio) => ratio.ToString("0.00", CultureInfo.InvariantCulture);

        public override string ToString() => Label;
    }

    /// <summary>
    /// One pair: a foreground token on a ground. The ground is a token, or the sidebar's gradient at some point, with an
    /// optional translucent layer over it (the current item's fill); a translucent foreground is blended over the ground.
    /// </summary>
    private sealed record Measure(string Foreground, Func<Palette, double, Rgb> Ground, double[] Printed, double[] Sampled, double? Minimum)
    {
        public static Measure Solid(string foreground, string ground, double? minimum)
            => new(foreground, (palette, _) => palette.Token(ground), [0], [0], minimum);

        public static Measure OverGradient(string foreground, string? layer, double minimum)
            => new(foreground, (palette, t) => Layered(palette, layer, palette.Gradient(t)), [0, 0.5, 1],
                [.. Enumerable.Range(0, 21).Select(step => step / 20.0)], minimum);

        public static Measure OverStart(string foreground, string? layer, double minimum)
            => new(foreground, (palette, _) => Layered(palette, layer, palette.Gradient(0)), [0], [0], minimum);

        public static Measure OverEnd(string foreground, string? layer, double minimum)
            => new(foreground, (palette, _) => Layered(palette, layer, palette.Gradient(1)), [1], [1], minimum);

        public IEnumerable<double> Figures(Palette palette) => Printed.Select(t => Ratio(palette, t));

        public double Worst(Palette palette) => Sampled.Min(t => Ratio(palette, t));

        private double Ratio(Palette palette, double t)
        {
            var ground = Ground(palette, t);
            return palette.Ratio(palette.Token(Foreground).Over(ground), ground);
        }

        private static Rgb Layered(Palette palette, string? layer, Rgb ground)
            => layer is null ? ground : palette.Token(layer).Over(ground);
    }

    /// <summary>app.css's <c>:root</c>, its colours resolved through <c>var()</c>, and WCAG 2.1's arithmetic on them.</summary>
    private sealed partial class Palette(IReadOnlyDictionary<string, string> root)
    {
        public static Palette FromAppCss() => new(Stylesheet.AppCss().Root());

        public Rgb Token(string name)
            => root.TryGetValue(name, out var value) ? Resolve(value) : throw new InvalidOperationException($":root does not define {name}.");

        public Rgb Resolve(string value)
        {
            var reference = VarReference().Match(value.Trim());
            return reference.Success ? Token(reference.Groups["name"].Value) : Rgb.Parse(value);
        }

        /// <summary>
        /// The sidebar's gradient at <paramref name="t" /> (0 its start, 1 its end), interpolated in sRGB as a browser
        /// interpolates a gradient of legacy colours, and not rounded.
        /// </summary>
        public Rgb Gradient(double t)
        {
            var start = Token("--sidebar-gradient-start");
            var end = Token("--sidebar-gradient-end");
            return new Rgb(start.R + ((end.R - start.R) * t), start.G + ((end.G - start.G) * t), start.B + ((end.B - start.B) * t), 1);
        }

        public double Ratio(Rgb one, Rgb other)
        {
            var (lighter, darker) = (Math.Max(one.Luminance, other.Luminance), Math.Min(one.Luminance, other.Luminance));
            return (lighter + 0.05) / (darker + 0.05);
        }

        [GeneratedRegex(@"^var\((?<name>--[\w-]+)\)$")]
        private static partial Regex VarReference();
    }

    private sealed partial record Rgb(double R, double G, double B, double A)
    {
        public double Luminance => (0.2126 * Linear(R)) + (0.7152 * Linear(G)) + (0.0722 * Linear(B));

        public string Hex => $"#{Channel(R):X2}{Channel(G):X2}{Channel(B):X2}";

        /// <summary>This colour painted over <paramref name="ground" />: itself if opaque, else the blend at its alpha.</summary>
        public Rgb Over(Rgb ground)
            => new((R * A) + (ground.R * (1 - A)), (G * A) + (ground.G * (1 - A)), (B * A) + (ground.B * (1 - A)), 1);

        public static Rgb Parse(string value)
        {
            value = value.Trim();
            var hex = HexColour().Match(value);
            if (hex.Success)
            {
                var digits = hex.Groups["digits"].Value;
                if (digits.Length == 3)
                {
                    digits = string.Concat(digits.Select(digit => $"{digit}{digit}"));
                }

                return new Rgb(Convert.ToInt32(digits[..2], 16), Convert.ToInt32(digits[2..4], 16), Convert.ToInt32(digits[4..6], 16), 1);
            }

            var function = RgbFunction().Match(value);
            if (!function.Success)
            {
                throw new FormatException($"'{value}' is not a colour this test reads (rgb(), rgba() or #hex).");
            }

            var parts = function.Groups["body"].Value.Replace(",", " ", StringComparison.Ordinal).Replace("/", " ", StringComparison.Ordinal)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var alpha = parts.Length == 4 ? Number(parts[3]) : 1;
            return new Rgb(Number(parts[0]), Number(parts[1]), Number(parts[2]), alpha);
        }

        private static double Number(string text)
            => text.EndsWith('%')
                ? double.Parse(text[..^1], CultureInfo.InvariantCulture) / 100
                : double.Parse(text, CultureInfo.InvariantCulture);

        private static int Channel(double value) => (int)Math.Round(value, MidpointRounding.AwayFromZero);

        private static double Linear(double channel)
        {
            var c = channel / 255;
            return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }

        [GeneratedRegex(@"^#(?<digits>[0-9a-fA-F]{6}|[0-9a-fA-F]{3})$")]
        private static partial Regex HexColour();

        [GeneratedRegex(@"^rgba?\((?<body>[^)]*)\)$")]
        private static partial Regex RgbFunction();
    }
}
