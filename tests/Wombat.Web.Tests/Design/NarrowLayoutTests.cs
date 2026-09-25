using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T226: the layout at narrow widths. The shell has a side gutter at phone width, an auto-fit grid never asks for more
/// than its container, a grid of inputs stacks its buttons where it is narrow, and every table is a styled
/// <c>clinic-table</c>.
/// </summary>
/// <remarks>
/// bUnit lays nothing out, so these pin the rules that produce the layout. The widths the rules give were measured in
/// Chrome against a static copy of each page (DESIGN.md § Layout grid and § Table system; the numbers are in app.css
/// beside the rules), and are checked in a browser.
/// </remarks>
public sealed partial class NarrowLayoutTests
{
    [Fact]
    public void TheShell_HasASideGutter_AtPhoneWidth()
    {
        // Under 641px the only horizontal padding the page had was none: the page header and the cards on
        // /msf/campaigns/{id} ran from x=0 to x=390 at 390px. ClinicAssist's comes from Bootstrap's px-4, which Wombat does
        // not load (T042 removed the class, and with it the only gutter below the desktop rule).
        var rules = Parse(File.ReadAllText(WebFile("Components", "Layout", "MainLayout.razor.css")));

        var gutter = rules.Should().ContainSingle(rule => rule.AtRule == string.Empty && Selectors(rule).Contains("article"),
            "the article's gutter is set outside any media query, so it holds at every width").Which;
        Selectors(gutter).Should().Contain(".top-row", "the signed-in name and Sign out keep the same gutter as the page");
        gutter.Declarations.Should().Contain(["padding-left: var(--space-md)", "padding-right: var(--space-md)"]);

        // The desktop rule still widens it, so it must come later: the two are equally specific.
        var desktop = rules.Should().ContainSingle(rule => rule.AtRule == "@media (min-width: 641px)" && Selectors(rule).Contains("article")).Which;
        desktop.Declarations.Should().Contain(declaration => declaration.StartsWith("padding-left:", StringComparison.Ordinal));
        desktop.Index.Should().BeGreaterThan(gutter.Index);
    }

    [Fact]
    public void EveryAutoFitGrid_AsksForNoMoreThanItsContainer()
    {
        // minmax(350px, 1fr) is 350px wide in a card with less room than that: the scale editor's Name input stuck out of
        // its card at 390px, and out of the one-third column of .details-grid at every width that has two columns.
        var grids = Parse(File.ReadAllText(WebFile("wwwroot", "app.css")))
            .SelectMany(rule => rule.Declarations
                .Where(declaration => declaration.StartsWith("grid-template-columns:", StringComparison.Ordinal)
                    && declaration.Contains("repeat(auto-", StringComparison.Ordinal))
                .Select(declaration => (rule.Selector, Declaration: declaration)))
            .ToList();

        grids.Select(grid => grid.Selector).Should().Contain([".form-grid", ".form-grid--wide", ".search-grid", ".dashboard-grid", ".check-grid"],
            "guard: the scan finds the auto-fit grids, or it proves nothing");
        grids.Where(grid => !CappedTrack().IsMatch(grid.Declaration))
            .Select(grid => $"{grid.Selector} {{ {grid.Declaration} }}")
            .Should().BeEmpty("an auto-fit column asks for min(its width, 100%), so a narrow container is not overflowed");
    }

    [Fact]
    public void AGridOfInputs_AnswersToItsOwnWidth_AndStacksItsButtons_WhereItIsNarrow()
    {
        var rules = Parse(File.ReadAllText(WebFile("wwwroot", "app.css")));

        // The container is the grid's own table container: at 1000px the scale editor's card shares its row, at 900px it
        // has the row to itself, so the viewport's width does not say how much room the grid has.
        Rule(rules, string.Empty, ".table-container:has(> .clinic-table--inputs)").Declarations
            .Should().Contain("container: inputs-grid / inline-size");

        // 44rem: the narrowest table in which, with its buttons on one line, each input still has the buttons' own 12rem
        // (measured: Label 201px in a 705px table). At 36rem one line left Label 131px at 1280px, and a 15px scrollbar
        // was enough to stack the buttons there again (T226 review).
        const string narrow = "@container inputs-grid (width < 44rem)";
        var stacked = Rule(rules, narrow, ".clinic-table--inputs .col-actions");
        stacked.Declarations.Should().Contain("width: 1%",
            "the buttons' column is as narrow as its widest button, rather than claim 12rem before the inputs grow");
        var padded = Rule(rules, narrow, ".clinic-table--inputs th, .clinic-table--inputs td");
        padded.Declarations.Should().Contain(["padding-left: var(--space-xs)", "padding-right: var(--space-xs)"],
            "the inputs' own padding spaces them; the cells' 8px sides were what left the table 11px wider than its card at 641px");

        // Everywhere else the buttons keep one line, and the narrow rules override equally specific ones, so come later.
        Rule(rules, string.Empty, ".clinic-table .col-actions").Declarations.Should().Contain("width: 12rem");
        stacked.Index.Should().BeGreaterThan(Rule(rules, string.Empty, ".clinic-table .col-actions").Index);
        padded.Index.Should().BeGreaterThan(Rule(rules, string.Empty, ".clinic-table--compact th, .clinic-table--compact td").Index);
    }

    [Fact]
    public void TheBuilder_AnswersToItsOwnCardsWidth_NotTheViewports()
    {
        // T266. The builder's two columns need 320px + 400px + the gap, 46.5rem, of the room its card gives them. They
        // collapsed at a viewport of 900px, but the sidebar, the gutter and the card's padding take 372px of the viewport,
        // so from 901px to 1115px the columns were wider than the card, and the page scrolled sideways (158px at 901px,
        // 59px at 1000px; measured in Chrome on a static copy, app.css has the numbers).
        var rules = Parse(File.ReadAllText(WebFile("wwwroot", "app.css")));

        Rule(rules, string.Empty, ".form-container:has(> .builder-two-col)").Declarations
            .Should().Contain("container: builder / inline-size");
        var two = Rule(rules, string.Empty, ".builder-two-col");
        two.Declarations.Should().Contain("grid-template-columns: minmax(320px, 1fr) minmax(400px, 1.4fr)");

        // The threshold is the two columns' own minimum: 320 + 400 + the 24px gap = 744px = 46.5rem.
        var one = Rule(rules, "@container builder (width < 46.5rem)", ".builder-two-col");
        one.Declarations.Should().Contain("grid-template-columns: minmax(0, 1fr)");
        one.Index.Should().BeGreaterThan(two.Index, "the two selectors are equally specific, so the narrow rule comes later");
        Rule(rules, string.Empty, ".builder-two-col > *").Declarations.Should().Contain("min-width: 0",
            "a column's content keeps to its track rather than widen it");

        rules.Where(rule => rule.AtRule.StartsWith("@media", StringComparison.Ordinal) && Selectors(rule).Contains(".builder-two-col"))
            .Should().BeEmpty("the viewport does not say how wide the builder's card is");
    }

    [Fact]
    public void TheMinimaByTrainingYear_GiveTheirPickerWhatTheRowLeaves()
    {
        // T266. With a 10rem floor on the picker's column the rows ran past the Add form's content edge at 641px and
        // 360px, and out of its card at 320px. The picker still asks for up to 18rem, and gets it from 1000px.
        var rules = Parse(File.ReadAllText(WebFile("wwwroot", "app.css")));

        Rule(rules, string.Empty, ".stage-minima").Declarations
            .Should().Contain("grid-template-columns: fit-content(12rem) minmax(0, 18rem) max-content");
    }

    [Fact]
    public void ADetailsGridOfOneCard_GivesItTheWholeRow_WhereThePageAsksForIt()
    {
        // T266. The grid's first column is its narrow third, so a lone card sat in it with the rest of the row empty: on
        // /msf/campaigns/{id} at 1000px, a 223px card with 447px of nothing beside it, and its table scrolled inside it.
        // Only where the page opts in (T266 review): unscoped, it also stretched a lone STAR and a lone trajectory chart,
        // and made My MSF reports' list jump to a third of the row when a report was selected.
        var rules = Parse(File.ReadAllText(WebFile("wwwroot", "app.css")));

        var grid = Rule(rules, string.Empty, ".details-grid");
        grid.Declarations.Should().Contain("grid-template-columns: 1fr 2fr");
        Rule(rules, string.Empty, ".details-grid--lone-spans > .detail-card:only-child").Declarations.Should().Contain("grid-column: 1 / -1");
        rules.Where(rule => Selectors(rule).Any(selector => selector.StartsWith(".details-grid >", StringComparison.Ordinal)
                && selector.Contains(":only-child", StringComparison.Ordinal)))
            .Should().BeEmpty("a lone card on every details grid is not what every page wants");
        File.ReadAllText(WebFile("Components", "Pages", "MultiSourceFeedback", "CampaignEdit.razor"))
            .Should().Contain("""<div class="details-grid details-grid--lone-spans">""");
    }

    [Fact]
    public void EveryTable_IsAClinicTable_WhoseClassesAppCssDefines()
    {
        // The entrustment decisions list was class="data-table", which app.css does not define: it rendered unstyled, and
        // 133px wider than the screen at 390px.
        var css = File.ReadAllText(WebFile("wwwroot", "app.css"));
        var web = WebFile();
        var tables = Directory.EnumerateFiles(web, "*.razor", SearchOption.AllDirectories)
            .SelectMany(path => TableTag().Matches(File.ReadAllText(path))
                .Select(tag => (File: Path.GetRelativePath(web, path), Classes: ClassesOf(tag.Value))))
            .ToList();

        tables.Should().HaveCountGreaterThanOrEqualTo(15, "guard: the scan finds the pages' tables (16), or it proves nothing");

        // A data table that only a screen reader reads (the trajectory chart's) is not a list, and is not styled as one.
        tables.Where(table => !table.Classes.Contains("clinic-table") && !table.Classes.Contains("visually-hidden"))
            .Select(table => $"{table.File}: class=\"{string.Join(' ', table.Classes)}\"")
            .Should().BeEmpty("a table is a .clinic-table (DESIGN.md § Table system)");
        tables.SelectMany(table => table.Classes
                .Where(name => !Regex.IsMatch(css, $@"\.{Regex.Escape(name)}(?![\w-])"))
                .Select(name => $"{table.File}: .{name}"))
            .Should().BeEmpty("a class app.css does not define styles nothing");
    }

    // ---- helpers ----

    /// <summary>One rule of a stylesheet: the at-rule it sits in ("" at the top level), its selector and declarations.</summary>
    private sealed record CssRule(string AtRule, string Selector, IReadOnlyList<string> Declarations, int Index);

    /// <summary>Every rule, in order, comments removed, whitespace collapsed; a rule inside an at-rule names it.</summary>
    private static IReadOnlyList<CssRule> Parse(string css)
    {
        css = Comment().Replace(css, string.Empty);
        var rules = new List<CssRule>();
        var open = new Stack<(bool IsAtRule, string Prelude)>();
        var buffer = new StringBuilder();

        foreach (var character in css)
        {
            if (character == '{')
            {
                var prelude = Collapse(buffer.ToString());
                open.Push((prelude.StartsWith('@'), prelude));
                buffer.Clear();
            }
            else if (character == '}')
            {
                var (isAtRule, prelude) = open.Pop();
                if (!isAtRule)
                {
                    var atRule = open.Where(block => block.IsAtRule).Select(block => block.Prelude).FirstOrDefault() ?? string.Empty;
                    var declarations = buffer.ToString()
                        .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(Collapse)
                        .ToList();
                    rules.Add(new CssRule(atRule, prelude, declarations, rules.Count));
                }

                buffer.Clear();
            }
            else
            {
                buffer.Append(character);
            }
        }

        open.Should().BeEmpty("the stylesheet's braces balance");
        return rules;
    }

    private static CssRule Rule(IReadOnlyList<CssRule> rules, string atRule, string selector)
        => rules.Should().ContainSingle(rule => rule.AtRule == atRule && rule.Selector == selector,
            $"app.css defines {selector} once{(atRule.Length == 0 ? string.Empty : $" in {atRule}")}").Which;

    private static IReadOnlyList<string> Selectors(CssRule rule)
        => rule.Selector.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static IReadOnlyList<string> ClassesOf(string tag)
    {
        var match = ClassAttribute().Match(tag);
        return match.Success
            ? match.Groups["value"].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(name => !name.Contains('@')).ToList()
            : [];
    }

    private static string Collapse(string text) => Whitespace().Replace(text, " ").Trim();

    private static string WebFile(params string[] parts)
        => Path.Combine([SolutionRoot(), "src", "Wombat.Web", .. parts]);

    private static string SolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && directory.GetFiles("Wombat.sln").Length == 0)
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not find Wombat.sln.");
    }

    /// <summary>An auto-fit or auto-fill track that asks for the lesser of its width and the whole container.</summary>
    [GeneratedRegex(@"^grid-template-columns: repeat\(auto-(fit|fill), minmax\(min\([0-9.]+(px|rem), 100%\), 1fr\)\)$")]
    private static partial Regex CappedTrack();

    /// <summary>An opening <c>&lt;table&gt;</c> tag, attributes and all.</summary>
    [GeneratedRegex(@"<table\b[^>]*>")]
    private static partial Regex TableTag();

    [GeneratedRegex(@"\sclass\s*=\s*""(?<value>[^""]*)""")]
    private static partial Regex ClassAttribute();

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex Comment();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
