using System.Globalization;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T198: a table column's width is a class in <c>app.css</c>, never a style on its cells (DESIGN.md § Table system).
/// The entrustment scale editor's headers carried <c>style="width:6rem"</c> and <c>"width:12rem"</c>.
/// </summary>
/// <remarks>
/// bUnit lays nothing out, so the page tests can only show a cell carrying its class. These pin what the classes mean:
/// the declarations that let a long-text column wrap inside a word down to a floor, keep a narrow column narrow, and let
/// a row's buttons wrap before the table scrolls. Without them the classes are names, and the curriculum items table is
/// back to about 30px to spare at 1280px. The widths they produce were measured in a browser (DESIGN.md § Table system).
/// </remarks>
public sealed partial class TableColumnClassTests
{
    [Fact]
    public void NoTableCell_CarriesAStyleOfItsOwn()
    {
        var web = Path.Combine(SolutionRoot(), "src", "Wombat.Web");
        var cells = Directory.EnumerateFiles(web, "*.razor", SearchOption.AllDirectories)
            .SelectMany(path => TableCell().Matches(File.ReadAllText(path))
                .Select(cell => (File: Path.GetRelativePath(web, path), Tag: cell.Value)))
            .ToList();

        cells.Should().HaveCountGreaterThanOrEqualTo(400, "guard: the scan finds the tables' cells (507), or it proves nothing");

        cells.Where(cell => StyleAttribute().IsMatch(cell.Tag))
            .Select(cell => $"{cell.File}: {cell.Tag}")
            .Should().BeEmpty("a column's width is .col-wrap, .col-fit or .col-actions in app.css, not a style on its cells");
    }

    [Fact]
    public void TheScan_FindsAStyledCell_AndPassesAClassedOne()
    {
        // The scan's own rule, shown failing, so a scan that matched nothing would not pass every page.
        const string styled = """<th style="width:6rem;">Order</th><td class="col-fit" style="width:12rem">1</td>""";
        const string classed = """<th class="col-fit">Order</th><td class="col-wrap" colspan="9">x</td><tbody>""";

        TableCell().Matches(styled).Count(cell => StyleAttribute().IsMatch(cell.Value)).Should().Be(2);
        TableCell().Matches(classed).Should().HaveCount(2, "<tbody> is not a cell");
        TableCell().Matches(classed).Count(cell => StyleAttribute().IsMatch(cell.Value)).Should().Be(0);
    }

    [Fact]
    public void ALongTextColumn_BreaksInsideAWord_DownToAFloor_AndAsksForAShareOfTheTable()
    {
        var rules = AppCssRules();

        Declarations(rules, ".clinic-table .col-wrap").Should().Contain("overflow-wrap: anywhere",
            "a column's longest word sets the table's narrowest width unless it may break inside it");

        // A share, not a length: a column held at a rem width stays at it on a wide screen while every other column grows.
        Declarations(rules, ".clinic-table th.col-wrap").Should().ContainSingle(value => PercentWidth().IsMatch(value));
        Declarations(rules, ".clinic-table th.col-wrap--wide").Should().ContainSingle(value => PercentWidth().IsMatch(value));
        IndexOf(rules, ".clinic-table th.col-wrap--wide").Should().BeGreaterThan(IndexOf(rules, ".clinic-table th.col-wrap"),
            "the two selectors are equally specific, so the wide share must come later to win");

        // The floor (T198 review). Break-anywhere makes a column's narrowest width one letter, and a share applies only
        // where there is room, so below the table's narrowest width the EPA and Tools columns fell to 30px, one letter a
        // line, and the table scrolled anyway. A floor in rem: a share is what gives way.
        var floor = RemMinWidth(Declarations(rules, ".clinic-table th.col-wrap"));
        floor.Should().BeGreaterThan(0, "a long-text column has a floor");
        RemMinWidth(Declarations(rules, ".clinic-table th.col-wrap--wide")).Should()
            .BeGreaterThan(floor, "the wide column's floor is the higher");
    }

    [Fact]
    public void ANarrowColumn_IsAsNarrowAsItsContent_AndARowsButtons_WrapBeforeTheTableScrolls()
    {
        var rules = AppCssRules();

        Declarations(rules, ".clinic-table .col-fit").Should().Contain(new[] { "white-space: nowrap", "width: 1%" });

        // A row's buttons ask for a length, which a table gives before the columns beside it grow, and wrap when it cannot
        // (T198 review). Held on one line at every width, they made the scale editor's table wider than its card at 390px
        // and near 700 and 1000px, with Remove out of view.
        Declarations(rules, ".clinic-table .col-actions").Should().ContainSingle(value => RemWidth().IsMatch(value));
        rules.Where(rule => rule.Selector.Contains(".actions-cell", StringComparison.Ordinal)
                && rule.Declarations.Contains("flex-wrap: nowrap"))
            .Select(rule => rule.Selector)
            .Should().BeEmpty("a row's buttons wrap when the table is short of room, rather than widen it");
        Declarations(rules, ".actions-cell").Should().Contain("flex-wrap: wrap");
    }

    [Fact]
    public void AGridOfInputs_CentresItsRows_AndTakesNoHoverTint()
    {
        var rules = AppCssRules();

        Declarations(rules, ".clinic-table--inputs td").Should().Contain("vertical-align: middle");
        IndexOf(rules, ".clinic-table--inputs td").Should().BeGreaterThan(IndexOf(rules, ".clinic-table td"),
            "it overrides the table's top alignment, which is as specific");

        // Every row is a form, and tinting a form as the pointer crosses it is noise (DESIGN.md, as for .is-editing).
        Declarations(rules, ".clinic-table--inputs tbody tr:hover").Should().Contain("background-color: transparent");
        IndexOf(rules, ".clinic-table--inputs tbody tr:hover").Should().BeGreaterThan(IndexOf(rules, ".clinic-table tbody tr:hover"),
            "it overrides the list's hover tint, which is as specific");
    }

    // ---- helpers ----

    /// <summary>Every rule of app.css, in order, comments removed: its selector (whitespace collapsed) and declarations.</summary>
    private static IReadOnlyList<(string Selector, IReadOnlyList<string> Declarations)> AppCssRules()
    {
        var css = Comment().Replace(File.ReadAllText(Path.Combine(SolutionRoot(), "src", "Wombat.Web", "wwwroot", "app.css")), string.Empty);
        return Rule().Matches(css)
            .Select(rule => (
                Selector: Whitespace().Replace(rule.Groups["selector"].Value, " ").Trim(),
                Declarations: (IReadOnlyList<string>)rule.Groups["body"].Value
                    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(declaration => Whitespace().Replace(declaration, " "))
                    .ToList()))
            .ToList();
    }

    private static IReadOnlyList<string> Declarations(
        IReadOnlyList<(string Selector, IReadOnlyList<string> Declarations)> rules, string selector)
        => rules.Should().ContainSingle(rule => rule.Selector == selector, $"app.css defines {selector} once").Which.Declarations;

    /// <summary>Where a rule stands in app.css. It must be there: a missing rule's -1 would pass any "later than" check.</summary>
    private static int IndexOf(IReadOnlyList<(string Selector, IReadOnlyList<string> Declarations)> rules, string selector)
    {
        var index = rules.ToList().FindIndex(rule => rule.Selector == selector);
        index.Should().BeGreaterThanOrEqualTo(0, $"app.css defines {selector}");
        return index;
    }

    /// <summary>The value, in rem, of a rule's one <c>min-width</c> declaration.</summary>
    private static double RemMinWidth(IReadOnlyList<string> declarations)
    {
        var declaration = declarations.Should()
            .ContainSingle(value => RemMinWidthDeclaration().IsMatch(value), "a floor is a length in rem").Which;
        return double.Parse(RemMinWidthDeclaration().Match(declaration).Groups["rem"].Value, CultureInfo.InvariantCulture);
    }

    private static string SolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && directory.GetFiles("Wombat.sln").Length == 0)
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not find Wombat.sln.");
    }

    /// <summary>An opening <c>&lt;th&gt;</c> or <c>&lt;td&gt;</c> tag, attributes and all.</summary>
    [GeneratedRegex(@"<t[hd]\b[^>]*>")]
    private static partial Regex TableCell();

    [GeneratedRegex(@"\sstyle\s*=")]
    private static partial Regex StyleAttribute();

    [GeneratedRegex(@"^width: \d+(\.\d+)?%$")]
    private static partial Regex PercentWidth();

    [GeneratedRegex(@"^width: \d+(\.\d+)?rem$")]
    private static partial Regex RemWidth();

    [GeneratedRegex(@"^min-width: (?<rem>\d+(\.\d+)?)rem$")]
    private static partial Regex RemMinWidthDeclaration();

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex Comment();

    [GeneratedRegex(@"(?<selector>[^{}]+)\{(?<body>[^{}]*)\}")]
    private static partial Regex Rule();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
