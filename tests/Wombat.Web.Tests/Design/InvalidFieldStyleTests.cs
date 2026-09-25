using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T236: an invalid field shows it on the control, not only in the message under it (DESIGN.md § Form system). Nothing in
/// <c>app.css</c> styled Blazor's <c>.invalid</c> class or <c>aria-invalid="true"</c>, so a refused agenda deferral's
/// reason box looked exactly like a valid one.
/// </summary>
/// <remarks>
/// bUnit lays nothing out and applies no stylesheet, so the page tests can only show a control carrying Blazor's marker.
/// These pin what the rule means: which markers it answers to, that it marks in the danger colour and with a stripe as
/// well (WCAG 1.4.1: not colour alone), that it changes only paint so the text does not move, and that a contrast theme,
/// which drops the shadow, keeps the stripe as a border.
/// <see cref="ShowsInvalid" /> matches a rendered control against the rule's own selectors, so the deferral form and the
/// activity form's tests show their refused fields are the ones it styles.
/// </remarks>
public sealed partial class InvalidFieldStyleTests
{
    private const string ForcedColours = "@media (forced-colors: active)";

    /// <summary>The markers of an invalid control: Blazor's two, on each control a form uses, and the hand-made one.</summary>
    private static readonly string[] Markers =
    [
        ".input-validation-error",
        ".form-control.invalid",
        ".form-select.invalid",
        "textarea.invalid",
        """.form-control[aria-invalid="true"]""",
        """.form-select[aria-invalid="true"]""",
        """textarea[aria-invalid="true"]""",
    ];

    /// <summary>
    /// Whether app.css's invalid-field rule styles <paramref name="control" />: it matches one of the rule's selectors.
    /// </summary>
    public static bool ShowsInvalid(IElement control) => control.Matches(InvalidRule(AppCssRules()).Selector);

    [Fact]
    public void TheRule_AnswersToBlazorsMarkers_AndToTheHandMadeOne()
    {
        Selectors(InvalidRule(AppCssRules()).Selector).Should().BeEquivalentTo(Markers,
            "Blazor marks an invalid input with .invalid and aria-invalid=\"true\", and a page's predicted refusal with "
            + ".input-validation-error: every one of them is shown");
    }

    [Fact]
    public void AnInvalidField_TakesTheDangerBorder_AndAStripe_SoNotByColourAlone()
    {
        // The rule is found by its danger border (InvalidRule), so a border in any other colour fails every test here.
        var declarations = InvalidRule(AppCssRules()).Declarations;

        // WCAG 1.4.1: a stripe down the left edge that no valid field has. An inset shadow, so the text stays put.
        var stripe = declarations.Should().ContainSingle(value => InsetStripe().IsMatch(value),
            "the stripe is a 'box-shadow: inset Npx 0 0 var(--danger-color)'").Which;
        int.Parse(InsetStripe().Match(stripe).Groups["px"].Value, CultureInfo.InvariantCulture).Should()
            .BeGreaterThanOrEqualTo(2, "a hairline beside the 1px border is not a shape anyone can tell apart");

        declarations.Where(value => RawColour().IsMatch(value)).Should()
            .BeEmpty("a colour is a token from :root (DESIGN.md § Design tokens)");
    }

    [Fact]
    public void AnInvalidField_ChangesOnlyPaint_SoTheTextStaysPut()
    {
        // The reason the stripe is a shadow (DESIGN.md § Form system): a field turns invalid while someone types in it, and
        // a wider border, a padding or any other box property would move the text under the cursor. Colour and shadow only.
        InvalidRule(AppCssRules()).Declarations.Select(Property).Should().BeEquivalentTo(["border-color", "box-shadow"],
            "an invalid field repaints its border and draws its stripe inside it, and its box keeps its size");
    }

    [Fact]
    public void InAContrastTheme_TheStripeIsTheBorder()
    {
        // Forced colours drop box-shadow and paint every border in one system colour: without this the stripe and the
        // colour are both gone, and an invalid field looks like any other.
        var rules = AppCssRules();
        var forced = rules.Should().ContainSingle(rule => rule.Media == ForcedColours && MarksAField(rule) && rule.Declarations.Any(value => BorderLeftWidth().IsMatch(value)),
            "app.css marks an invalid field in a contrast theme with a wider left border").Which;

        Selectors(forced.Selector).Should().BeEquivalentTo(Markers, "the same fields as the rule it stands in for");
        var width = forced.Declarations.Should().ContainSingle().Which;
        int.Parse(BorderLeftWidth().Match(width).Groups["px"].Value, CultureInfo.InvariantCulture).Should()
            .BeGreaterThanOrEqualTo(3, "the left border is a stripe, well past the 1px every control has");
    }

    [Fact]
    public void TheRule_ComesAfterTheControlsOwnBorders()
    {
        // .input-validation-error is as specific as .form-control and .form-select, so it wins only by coming later.
        var rules = AppCssRules();
        var invalid = rules.ToList().IndexOf(InvalidRule(rules));

        invalid.Should().BeGreaterThan(IndexOf(rules, rule => rule.Media is null && rule.Selector == ".form-control"));
        invalid.Should().BeGreaterThan(IndexOf(rules, rule => rule.Media is null && rule.Selector == ".form-select"));
    }

    [Fact]
    public void TheRule_MarksAnInvalidControl_AndNoValidOne()
    {
        // The markers as the pages render them, matched with the rule's own selectors, so a selector that matched nothing
        // would not pass.
        var document = new HtmlParser().ParseDocument("""
            <textarea id="refused-box" class="form-control invalid" aria-invalid="true"></textarea>
            <textarea id="retyped-box" class="form-control modified invalid"></textarea>
            <select id="refused-select" class="form-select invalid"></select>
            <input id="predicted" class="form-control input-validation-error" aria-invalid="true" type="date" />
            <select id="picker-refused" class="form-select" aria-invalid="true"></select>
            <textarea id="bare-box" class="invalid"></textarea>

            <input id="fixed" class="form-control modified valid" />
            <select id="picker-valid" class="form-select" aria-invalid="false"></select>
            <input id="untouched" class="form-control" />
            <input id="checkbox" class="form-check-input invalid" type="checkbox" />
            """);

        string[] marked = ["refused-box", "retyped-box", "refused-select", "predicted", "picker-refused", "bare-box"];
        string[] unmarked = ["fixed", "picker-valid", "untouched", "checkbox"];

        marked.Where(id => !ShowsInvalid(document.GetElementById(id)!)).Should().BeEmpty("each is invalid");
        unmarked.Where(id => ShowsInvalid(document.GetElementById(id)!)).Should()
            .BeEmpty("a valid, untouched or aria-invalid=\"false\" control is not marked, nor a native checkbox, which takes no border");
    }

    // ---- helpers ----

    private sealed record CssRule(string? Media, string Selector, IReadOnlyList<string> Declarations);

    /// <summary>
    /// The rule that marks an invalid field: the one top-level rule giving a border the danger colour on any of the
    /// markers. Found by what it does and by any one marker rather than all of them, so a selector dropped from it fails as
    /// a selector missing, not as a rule missing; and a danger border anywhere else in app.css (a card, a button) is not it.
    /// </summary>
    private static CssRule InvalidRule(IReadOnlyList<CssRule> rules)
        => rules.Should().ContainSingle(rule => rule.Media == null && MarksAField(rule) && rule.Declarations.Contains("border-color: var(--danger-color)"),
            "app.css gives an invalid field its danger border in one rule").Which;

    /// <summary>Whether a rule's selector names at least one marker of an invalid control.</summary>
    private static bool MarksAField(CssRule rule) => Selectors(rule.Selector).Intersect(Markers).Any();

    private static IReadOnlyList<string> Selectors(string selector)
        => selector.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>A declaration's property: <c>border-color</c> of <c>border-color: var(--danger-color)</c>.</summary>
    private static string Property(string declaration) => declaration.Split(':', 2)[0].Trim();

    /// <summary>Where a rule stands in app.css. It must be there: a missing rule's -1 would pass any "later than" check.</summary>
    private static int IndexOf(IReadOnlyList<CssRule> rules, Predicate<CssRule> match)
    {
        var index = rules.ToList().FindIndex(match);
        index.Should().BeGreaterThanOrEqualTo(0, "app.css defines the rule");
        return index;
    }

    /// <summary>
    /// Every rule of app.css, in order, comments removed: the at-rule block it sits in (<c>@media …</c>, or null at the
    /// top level), its selector (whitespace collapsed) and its declarations.
    /// </summary>
    private static IReadOnlyList<CssRule> AppCssRules()
    {
        var css = Comment().Replace(File.ReadAllText(Path.Combine(SolutionRoot(), "src", "Wombat.Web", "wwwroot", "app.css")), string.Empty);
        var blocks = AtBlock().Matches(css)
            .Select(block => (Start: block.Index, End: block.Index + block.Length, Condition: Collapse(block.Groups["condition"].Value)))
            .ToList();

        return Rule().Matches(css)
            .Select(rule => new CssRule(
                blocks.Where(block => rule.Index >= block.Start && rule.Index < block.End)
                    .Select(block => (string?)block.Condition)
                    .SingleOrDefault(),
                Collapse(rule.Groups["selector"].Value),
                rule.Groups["body"].Value
                    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(Collapse)
                    .ToList()))
            .Where(rule => !rule.Selector.StartsWith('@'))
            .ToList();
    }

    private static string Collapse(string text) => Whitespace().Replace(text, " ").Trim();

    private static string SolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && directory.GetFiles("Wombat.sln").Length == 0)
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not find Wombat.sln.");
    }

    [GeneratedRegex(@"^box-shadow: inset (?<px>\d+)px 0 0 var\(--danger-color\)$")]
    private static partial Regex InsetStripe();

    [GeneratedRegex(@"^border-left-width: (?<px>\d+)px$")]
    private static partial Regex BorderLeftWidth();

    [GeneratedRegex(@"#[0-9a-fA-F]{3,8}\b|\brgba?\(|\bhsla?\(")]
    private static partial Regex RawColour();

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex Comment();

    /// <summary>An at-rule block holding rules one level deep (<c>@media … { rule { … } … }</c>).</summary>
    [GeneratedRegex(@"(?<condition>@[^{};]+)\{(?:[^{}]|\{[^{}]*\})*\}")]
    private static partial Regex AtBlock();

    [GeneratedRegex(@"(?<selector>[^{}]+)\{(?<body>[^{}]*)\}")]
    private static partial Regex Rule();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
