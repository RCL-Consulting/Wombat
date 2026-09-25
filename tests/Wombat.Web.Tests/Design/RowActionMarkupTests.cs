using System.Text.RegularExpressions;
using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T239: a list's actions column has a header, and each row's action names its row (DESIGN.md § Button system and
/// § Table system). Twenty-four pages had an actions column headed by an empty <c>&lt;th&gt;</c>, and most of them a
/// column of identical "Edit", "View" or "Open" buttons, each of which a screen reader announced by that word alone.
/// </summary>
/// <remarks>
/// A scan of the Razor source, since every list page would otherwise need its own fixture: the page tests render each
/// fixed page (MyReviews, the activity lists, the scales list), and this holds every page, and the next one, to the rule.
/// </remarks>
public sealed partial class RowActionMarkupTests
{
    [Fact]
    public void EveryTableHeader_HasText()
    {
        var headers = RazorFiles()
            .SelectMany(file => HeaderCell().Matches(file.Text)
                .Select(match => (file.Name, Header: match.Value, Content: match.Groups["content"].Value)))
            .ToList();

        headers.Should().HaveCountGreaterThanOrEqualTo(150, "guard: the scan finds the pages' header cells, or it proves nothing");
        Lines(headers.Where(header => !HasText(header.Content)).Select(header => $"{header.Name}: {Collapse(header.Header)}"))
            .Should().BeEmpty("a header names the cells under it; an actions column's is a .visually-hidden \"Actions\"");
    }

    [Fact]
    public void NoTableCell_IsItselfTheActionsCluster()
    {
        // A table cell made a flex box is no longer a table cell, and its border no longer meets its row's (T226).
        Lines(RazorFiles().SelectMany(file => ActionsCellTd().Matches(file.Text).Select(match => $"{file.Name}: {match.Value}")))
            .Should().BeEmpty("a cell's buttons sit in a div.actions-cell inside the <td>");
    }

    [Fact]
    public void EveryRowAction_NamesItsRow()
    {
        var actions = RazorFiles()
            .SelectMany(file => RowRegion().Matches(file.Text)
                .SelectMany(region => Control().Matches(region.Groups["body"].Value))
                .Select(control => (file.Name, Control: control)))
            .ToList();

        actions.Should().HaveCountGreaterThanOrEqualTo(40, "guard: the scan finds the rows' buttons and links, or it proves nothing");
        Lines(actions.Where(action => !NamesItsRow(action.Control)).Select(action => $"{action.Name}: {Collapse(action.Control.Value)}"))
            .Should().BeEmpty(
                "a row's button or button-styled link carries an aria-label, computed from its row, that contains its " +
                "visible label and names the row (\"Edit PAED-001\"); a column of identical \"Edit\" buttons is otherwise " +
                "indistinguishable");
    }

    [Theory]
    [InlineData("""<a class="btn btn-outline btn-sm" href="@($"/x/{item.Id}")">Edit</a>""", false)]
    [InlineData("""<button type="button" class="btn btn-sm" @onclick="() => RemoveAsync(item)">Remove</button>""", false)]
    [InlineData("""<button class="btn btn-sm">@(job.IsEnabled ? "Disable" : "Enable")</button>""", false)]
    [InlineData("""<a class="btn btn-sm" href="/x" aria-label="@($"Edit {item.Name}")">Edit</a>""", true)]
    [InlineData("""<button class="btn btn-sm" @onclick="() => Go()" aria-label="@($"Move level {level} up")">Up</button>""", true)]
    [InlineData("""<a href="/activities/@item.ActivityId">@item.ActivityTypeName</a>""", true)]
    [InlineData("""<a href="/activities/inbox">Open</a>""", false)]
    // T239 review: an aria-label had only to be there. A fixed one names every row alike, and one that drops the visible
    // label cannot be spoken by what a speech-input user sees (WCAG 2.5.3).
    [InlineData("""<a class="btn btn-sm" href="/x" aria-label="Detail">Detail</a>""", false)]
    [InlineData("""<button class="btn btn-sm" aria-label="@("Edit")">Edit</button>""", false)]
    [InlineData("""<button class="btn btn-sm" aria-label="@(item.Name)">Edit</button>""", false)]
    [InlineData("""<button class="btn btn-sm" aria-label="@($"Remove {item.Name}")">Delete</button>""", false)]
    [InlineData("""<button class="btn btn-sm" aria-label="@($"Editor of {item.Name}")">Edit</button>""", false)]
    [InlineData("""<button class="btn btn-sm" aria-label="@($"{(job.IsEnabled ? "Disable" : "Enable")} {job.Key}")">@(job.IsEnabled ? "Disable" : "Enable")</button>""", true)]
    [InlineData("""<button class="btn btn-sm" aria-label="@($"{(job.IsEnabled ? "Enable" : "Disable")} {job.Key}")">@(job.IsEnabled ? "Disable" : "Enable")</button>""", false)]
    [InlineData("""<a class="btn btn-sm" href="@($"/r/{id}")" aria-label="@($"Open review #{id}, open for {item.Period}")">@($"Open review #{id}")</a>""", true)]
    [InlineData("""<a class="btn btn-sm" href="@link" aria-label="@Text.RowLinkName(campaignLink, Describe(item))">@campaignLink</a>""", true)]
    [InlineData("""<a class="btn btn-sm" href="@link" aria-label="@Text.RowLinkName(otherLink, Describe(item))">@campaignLink</a>""", false)]
    public void TheScan_TellsANamedRowAction_FromABareOne(string markup, bool named)
    {
        // The rule the scan applies, shown catching each way a row's action goes unnamed, and passing each way of naming
        // one: a scan that passed everything would pass every page.
        var control = Control().Match(markup);

        control.Success.Should().BeTrue();
        NamesItsRow(control).Should().Be(named);
    }

    // A control names its row where it carries an aria-labelledby, or an aria-label computed from the row that contains the
    // visible label, or where it is a plain link (no .btn) whose own text is the row's words: a link that reads "Mini-CEX"
    // is named by what it says. A .btn is an action, and its words are the action's ("Edit", "Disable"), however they are
    // computed, so it needs the row named for it.
    private static bool NamesItsRow(Match control)
    {
        var attributes = control.Groups["attrs"].Value;
        if (attributes.Contains("aria-labelledby=", StringComparison.Ordinal))
        {
            return true;
        }

        if (AriaLabel().Match(attributes) is { Success: true } label)
        {
            var value = label.Groups["value"].Value;
            return IsComputedFromTheRow(value) && CarriesTheVisibleLabel(value, VisibleText(control.Groups["text"].Value));
        }

        return control.Groups["tag"].Value == "a"
               && !ButtonClass().IsMatch(attributes)
               && control.Groups["text"].Value.Contains('@');
    }

    // A label that names its row is computed from it: a Razor expression with an interpolation hole
    // (@($"Edit {item.Name}")) or a call given an argument (@Text.RowLinkName(link, Describe(item))). A fixed label
    // ("Detail", or @("Edit")) names every row alike, and a bare member (@(item.Name)) drops the action's own label.
    private static bool IsComputedFromTheRow(string value)
        => value.StartsWith('@') && (value.Contains('{') || CallWithArguments().IsMatch(value));

    // The name contains the words a sighted user sees, as WCAG 2.5.3 asks, so a speech-input user can say them: "Up" is in
    // "Move level 3 up". Where the visible label is itself computed (@(job.IsEnabled ? "Disable" : "Enable"), or an
    // interpolated @($"Open review #{id}")), the aria-label is built from the same expression.
    private static bool CarriesTheVisibleLabel(string value, string visible)
    {
        if (!visible.StartsWith('@'))
        {
            return visible.Length > 0
                   && Regex.IsMatch(value, $@"(?<!\w){Regex.Escape(visible)}(?!\w)", RegexOptions.IgnoreCase);
        }

        var expression = visible[1..];
        if (expression.StartsWith('(') && expression.EndsWith(')'))
        {
            expression = expression[1..^1];
        }

        if (expression.StartsWith("$\"", StringComparison.Ordinal) && expression.EndsWith('"'))
        {
            expression = expression[2..^1];
        }

        return expression.Length > 0 && value.Contains(expression, StringComparison.Ordinal);
    }

    // What a control's content reads: its markup and Razor comments dropped, its whitespace collapsed.
    private static string VisibleText(string content)
        => Collapse(Tag().Replace(RazorComment().Replace(content, string.Empty), string.Empty));

    // Every offender, one a line: a collection's failure message names only the first.
    private static string Lines(IEnumerable<string> offenders) => string.Join(Environment.NewLine, offenders);

    private static bool HasText(string content) => VisibleText(content).Length > 0;

    private static IEnumerable<(string Name, string Text)> RazorFiles()
    {
        var web = Path.Combine(SolutionRoot(), "src", "Wombat.Web");
        return Directory.EnumerateFiles(web, "*.razor", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                           && !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(path => (Path.GetRelativePath(web, path), File.ReadAllText(path)));
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

    private static string Collapse(string text) => Whitespace().Replace(text, " ").Trim();

    /// <summary>A header cell and what it holds. A quoted attribute value may hold a "&gt;" (a lambda's "=&gt;").</summary>
    [GeneratedRegex("""<th\b(?:"[^"]*"|[^">])*>(?<content>.*?)</th>""", RegexOptions.Singleline)]
    private static partial Regex HeaderCell();

    [GeneratedRegex("""<td\b[^>]*\bclass\s*=\s*"[^"]*\bactions-cell\b[^"]*"[^>]*>""")]
    private static partial Regex ActionsCellTd();

    /// <summary>A list's rows: a DataTable's <c>Row</c> fragment, or a hand-built table's <c>tbody</c>.</summary>
    [GeneratedRegex("""<(?<region>Row|tbody)\b[^>]*>(?<body>.*?)</\k<region>>""", RegexOptions.Singleline)]
    private static partial Regex RowRegion();

    /// <summary>A link or a button: its tag, its attributes and its content.</summary>
    [GeneratedRegex("""<(?<tag>a|button)\b(?<attrs>(?:"[^"]*"|[^">])*)>(?<text>.*?)</\k<tag>>""", RegexOptions.Singleline)]
    private static partial Regex Control();

    [GeneratedRegex("""\bclass\s*=\s*"[^"]*\bbtn\b""")]
    private static partial Regex ButtonClass();

    /// <summary>
    /// An aria-label and its value: a Razor expression, <c>@(…)</c> or <c>@Member.Call(…)</c>, whose parentheses balance
    /// and whose string literals hold quotes of their own, or else plain text up to the closing quote.
    /// </summary>
    [GeneratedRegex(@"\baria-label\s*=\s*""(?<value>@[\w.]*\((?>[^()]+|\((?<depth>)|\)(?<-depth>))*(?(depth)(?!))\)|[^""]*)""")]
    private static partial Regex AriaLabel();

    /// <summary>A Razor call given at least one argument: <c>@Text.RowLinkName(link, …)</c>.</summary>
    [GeneratedRegex(@"^@\w[\w.]*\(\s*[^)\s]")]
    private static partial Regex CallWithArguments();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex Tag();

    [GeneratedRegex(@"@\*.*?\*@", RegexOptions.Singleline)]
    private static partial Regex RazorComment();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
