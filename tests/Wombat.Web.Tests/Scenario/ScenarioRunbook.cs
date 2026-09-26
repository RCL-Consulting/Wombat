using System.Text;
using System.Text.RegularExpressions;

namespace Wombat.Web.Tests.Scenario;

/// <summary>
/// One <c>### Step</c> block of the scenario runbook: its heading to the next heading.
/// </summary>
/// <param name="File">The act or appendix file, by name.</param>
/// <param name="Line">The heading's line, from 1.</param>
/// <param name="Level">The heading's level; the step format writes <c>###</c>.</param>
/// <param name="Id">The step's number: <c>3.4</c>, or <c>A.1.2</c> in the appendix.</param>
/// <param name="Fields">The field names the block carries, in the order written, <c>Note</c> included.</param>
/// <param name="Route">The <c>Route:</c> value with its continuation lines, or null when the block has no Route.</param>
/// <param name="RouteTokens">The routes <see cref="ScenarioRunbook.RouteTokens" /> takes from the first Route value.</param>
internal sealed record ScenarioStep(
    string File,
    int Line,
    int Level,
    string Id,
    IReadOnlyList<string> Fields,
    string? Route,
    IReadOnlyList<string> RouteTokens)
{
    /// <summary>How a failure names the step: <c>act-3-operations.md:120 Step 3.4</c>.</summary>
    public string Where => $"{File}:{Line} Step {Id}";
}

/// <summary>A template <c>coverage.md</c> lists under <c>## Not played</c>, with its reason.</summary>
internal sealed record NotPlayedRow(int Line, string Template, string Reason);

/// <summary>A template <c>coverage.md</c> lists under <c>## Pages</c>.</summary>
internal sealed record PagesRow(int Line, string Template);

/// <summary>
/// The scenario runbook (T293) as T294's check reads it: the step blocks of the act and appendix files, and
/// <c>coverage.md</c>'s <c>## Pages</c> and <c>## Not played</c> tables. The contract is README.md § "The step format".
/// </summary>
/// <remarks>
/// <para>
/// <b>A step block</b> runs from a heading <c>Step &lt;id&gt; — label</c> to the next heading of any level, outside
/// fenced code. A field is a line at column 0 of the form <c>Name:</c> or <c>Name (…):</c>, where Name is one capitalised
/// word (<c>Actual (2026-09-30, T295): …</c> is the field Actual). An indented line continues the field above it; a blank
/// line ends it.
/// </para>
/// <para>
/// <b>A table row</b> is a line beginning with <c>|</c> that is not a table's header (the row above a <c>|---|</c>
/// separator) or the separator itself. A section runs from its <c>##</c> heading to the next <c>#</c> or <c>##</c>
/// heading, so tables under <c>###</c> sub-headings are part of it. A row's templates are the backticked spans beginning
/// with <c>/</c> in the first cell that has one; in <c>## Not played</c> the cells after it are the reason.
/// </para>
/// </remarks>
internal sealed partial class ScenarioRunbook
{
    public const string CoverageFile = "coverage.md";

    private ScenarioRunbook(
        IReadOnlyList<string> stepFiles,
        IReadOnlyList<ScenarioStep> steps,
        bool hasCoverage,
        bool hasPagesSection,
        IReadOnlyList<PagesRow> pages,
        IReadOnlyList<NotPlayedRow> notPlayed,
        IReadOnlyList<int> notPlayedRowsWithoutTemplate)
    {
        StepFiles = stepFiles;
        Steps = steps;
        HasCoverage = hasCoverage;
        HasPagesSection = hasPagesSection;
        Pages = pages;
        NotPlayed = notPlayed;
        NotPlayedRowsWithoutTemplate = notPlayedRowsWithoutTemplate;
    }

    /// <summary>The act and appendix files read, by name.</summary>
    public IReadOnlyList<string> StepFiles { get; }

    public IReadOnlyList<ScenarioStep> Steps { get; }

    /// <summary>Whether there was a <c>coverage.md</c> to read.</summary>
    public bool HasCoverage { get; }

    public bool HasPagesSection { get; }

    public IReadOnlyList<PagesRow> Pages { get; }

    public IReadOnlyList<NotPlayedRow> NotPlayed { get; }

    /// <summary>The lines of <c>## Not played</c> rows that name no template in backticks.</summary>
    public IReadOnlyList<int> NotPlayedRowsWithoutTemplate { get; }

    /// <summary>
    /// The runbook in <paramref name="folder" />: every <c>act-*.md</c> and <c>appendix*.md</c>, in name order, and
    /// <c>coverage.md</c>. A file that is not there yet is read as absent, not as an error, so each rule can say what is
    /// missing.
    /// </summary>
    public static ScenarioRunbook Load(string folder)
    {
        var stepFiles = Directory.Exists(folder)
            ? Directory.EnumerateFiles(folder, "*.md")
                .Where(path => IsStepFile(Path.GetFileName(path)))
                .OrderBy(path => Path.GetFileName(path), StringComparer.Ordinal)
                .Select(path => (Path.GetFileName(path), File.ReadAllText(path)))
                .ToList()
            : [];

        var coveragePath = Path.Combine(folder, CoverageFile);
        var coverage = File.Exists(coveragePath) ? File.ReadAllText(coveragePath) : null;

        return Parse(stepFiles, coverage);
    }

    /// <summary>Whether a file of the runbook's folder holds steps: the acts and the appendix.</summary>
    public static bool IsStepFile(string fileName)
        => fileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase) &&
           (fileName.StartsWith("act-", StringComparison.OrdinalIgnoreCase) ||
            fileName.StartsWith("appendix", StringComparison.OrdinalIgnoreCase));

    /// <param name="stepFiles">Each act or appendix file, by name, with its text.</param>
    /// <param name="coverage"><c>coverage.md</c>'s text, or null when there is none.</param>
    public static ScenarioRunbook Parse(IEnumerable<(string File, string Text)> stepFiles, string? coverage)
    {
        var files = stepFiles.ToList();
        var steps = files.SelectMany(file => ParseSteps(file.File, file.Text)).ToList();

        if (coverage is null)
        {
            return new ScenarioRunbook(files.Select(f => f.File).ToList(), steps, false, false, [], [], []);
        }

        var lines = Lines(coverage);
        var pagesSection = Section(lines, PagesHeading());
        var notPlayedSection = Section(lines, NotPlayedHeading());

        var pages = pagesSection is null
            ? []
            : TableRows(lines, pagesSection.Value)
                .SelectMany(row => TemplateCell(row.Cells) is { } cell
                    ? Templates(row.Cells[cell]).Select(template => new PagesRow(row.Line, template))
                    : [])
                .ToList();

        var notPlayed = new List<NotPlayedRow>();
        var withoutTemplate = new List<int>();
        if (notPlayedSection is not null)
        {
            foreach (var row in TableRows(lines, notPlayedSection.Value))
            {
                if (TemplateCell(row.Cells) is not { } cell)
                {
                    withoutTemplate.Add(row.Line);
                    continue;
                }

                var reason = string.Join(" ", row.Cells.Skip(cell + 1).Where(c => c.Length > 0));
                notPlayed.AddRange(Templates(row.Cells[cell]).Select(template => new NotPlayedRow(row.Line, template, reason)));
            }
        }

        return new ScenarioRunbook(
            files.Select(f => f.File).ToList(), steps, true, pagesSection is not null, pages, notPlayed, withoutTemplate);
    }

    /// <summary>The step blocks of one act or appendix file.</summary>
    public static IReadOnlyList<ScenarioStep> ParseSteps(string file, string text)
    {
        var steps = new List<ScenarioStep>();
        var lines = Lines(text);

        (int Line, int Level, string Id)? heading = null;
        var fields = new List<string>();

        // The first Route field's value, continuation lines included. A second Route is a format error the order rule
        // reports; its routes are not read.
        StringBuilder? route = null;
        var readingRoute = false;
        var inFence = false;

        void Close()
        {
            if (heading is { } open)
            {
                var value = route?.ToString();
                steps.Add(new ScenarioStep(
                    file, open.Line, open.Level, open.Id, fields.ToList(), value, value is null ? [] : RouteTokens(value)));
            }

            heading = null;
            fields.Clear();
            route = null;
            readingRoute = false;
        }

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];

            if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
            {
                inFence = !inFence;
                readingRoute = false;
                continue;
            }

            if (inFence)
            {
                continue;
            }

            if (Heading().Match(line) is { Success: true } h)
            {
                Close();
                if (StepHeading().Match(h.Groups["text"].Value) is { Success: true } step)
                {
                    heading = (i + 1, h.Groups["hashes"].Length, step.Groups["id"].Value);
                }

                continue;
            }

            if (heading is null)
            {
                continue;
            }

            if (Field().Match(line) is { Success: true } field)
            {
                var name = field.Groups["name"].Value;
                fields.Add(name);
                readingRoute = name == "Route" && route is null;
                if (readingRoute)
                {
                    route = new StringBuilder(line[field.Length..].Trim());
                }

                continue;
            }

            var isContinuation = line.Length > 0 && char.IsWhiteSpace(line[0]) && line.Trim().Length > 0;
            if (isContinuation && readingRoute)
            {
                route!.Append(' ').Append(line.Trim());
            }
            else if (!isContinuation)
            {
                // A blank line, or text at column 0 that is not a field, ends the field above it.
                readingRoute = false;
            }
        }

        Close();
        return steps;
    }

    /// <summary>
    /// The routes a <c>Route:</c> value names, in order.
    /// </summary>
    /// <remarks>
    /// The rule, so that a step can say more than its routes and still be read exactly:
    /// <list type="number">
    /// <item>A value beginning <c>n/a</c> names no route, whatever follows it.</item>
    /// <item>
    /// A route begins with <c>/</c> at the start of the value or after whitespace, a comma, <c>→</c>, <c>(</c> or a
    /// backtick, so <c>n/a</c>, <c>and/or</c> and <c>https://…</c> are not routes. A route inside a parenthetical is a
    /// route: it names a page as much as any other.
    /// </item>
    /// <item>
    /// It runs to the next whitespace, comma, <c>→</c> or backtick, except inside <c>{…}</c>, where a constraint may carry a
    /// comma (<c>{n:range(1,10)}</c>).
    /// </item>
    /// <item>
    /// A query string or fragment is cut (<c>?tab=history</c>, <c>#notes</c>), outside <c>{…}</c>, where <c>?</c> marks an
    /// optional parameter. Then trailing <c>)</c>, <c>.</c>, <c>;</c>, <c>:</c> and <c>!</c> are trimmed: a template ends
    /// in a letter, a digit, <c>/</c> or <c>}</c>, never in punctuation that closes a sentence or a parenthetical.
    /// </item>
    /// </list>
    /// What is left is compared exactly, case included, with the templates and the mapped endpoints.
    /// </remarks>
    public static IReadOnlyList<string> RouteTokens(string value)
    {
        var trimmed = value.Trim();
        if (IsNotApplicable(trimmed))
        {
            return [];
        }

        return RouteToken().Matches(trimmed)
            .Select(match => Clean(match.Value))
            .Where(token => token.Length > 0)
            .ToList();
    }

    /// <summary>Whether a Route value says the step visits no page.</summary>
    public static bool IsNotApplicable(string routeValue)
        => routeValue.TrimStart().StartsWith("n/a", StringComparison.OrdinalIgnoreCase);

    private static string Clean(string token)
    {
        var depth = 0;
        var end = token.Length;
        for (var i = 0; i < token.Length && end == token.Length; i++)
        {
            depth += token[i] switch { '{' => 1, '}' => -1, _ => 0 };
            if (depth <= 0 && token[i] is '?' or '#')
            {
                end = i;
            }
        }

        return token[..end].TrimEnd(')', '.', ';', ':', '!');
    }

    private static List<string> Lines(string text) => text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n').ToList();

    /// <summary>The lines after <paramref name="heading" />'s line, up to the next <c>#</c> or <c>##</c> heading.</summary>
    private static (int Start, int End)? Section(List<string> lines, Regex heading)
    {
        var start = lines.FindIndex(line => heading.IsMatch(line));
        if (start < 0)
        {
            return null;
        }

        var end = lines.FindIndex(start + 1, line => TopHeading().IsMatch(line));
        return (start + 1, end < 0 ? lines.Count : end);
    }

    /// <summary>The data rows of every table in a section, each with its line (from 1) and its trimmed cells.</summary>
    private static IEnumerable<(int Line, IReadOnlyList<string> Cells)> TableRows(List<string> lines, (int Start, int End) section)
    {
        var inFence = false;
        for (var i = section.Start; i < section.End; i++)
        {
            var line = lines[i].Trim();
            if (line.StartsWith("```", StringComparison.Ordinal))
            {
                inFence = !inFence;
                continue;
            }

            if (inFence || !line.StartsWith('|') || Separator().IsMatch(line))
            {
                continue;
            }

            var isHeader = i + 1 < section.End && Separator().IsMatch(lines[i + 1].Trim());
            if (isHeader)
            {
                continue;
            }

            yield return (i + 1, Cells(line));
        }
    }

    private static IReadOnlyList<string> Cells(string row)
    {
        var inner = row.Trim().Trim('|');
        return inner.Split('|').Select(cell => cell.Trim()).ToList();
    }

    private static int? TemplateCell(IReadOnlyList<string> cells)
    {
        for (var i = 0; i < cells.Count; i++)
        {
            if (Templates(cells[i]).Count > 0)
            {
                return i;
            }
        }

        return null;
    }

    private static IReadOnlyList<string> Templates(string cell)
        => BacktickSpan().Matches(cell)
            .Select(span => span.Groups["text"].Value.Trim())
            .Where(text => text.StartsWith('/'))
            .ToList();

    [GeneratedRegex(@"^(?<hashes>#{1,6})\s+(?<text>.*)$")]
    private static partial Regex Heading();

    [GeneratedRegex(@"^#{1,2}\s")]
    private static partial Regex TopHeading();

    /// <summary>A heading's text that opens a step: <c>Step 3.4 — …</c>, <c>Step A.1.2 — …</c>.</summary>
    [GeneratedRegex(@"^Step\s+(?<id>[A-Za-z0-9]+(?:\.[A-Za-z0-9]+)*)")]
    private static partial Regex StepHeading();

    [GeneratedRegex(@"^(?<name>[A-Z][A-Za-z]*)(?:[ \t]+\([^)]*\))?:(?=\s|$)")]
    private static partial Regex Field();

    [GeneratedRegex(@"(?<=^|[\s,→(`])/(?:\{[^}\s]*\}|[^\s,→`{}])*")]
    private static partial Regex RouteToken();

    [GeneratedRegex(@"^##\s+Pages\b", RegexOptions.IgnoreCase)]
    private static partial Regex PagesHeading();

    [GeneratedRegex(@"^##\s+Not played\b", RegexOptions.IgnoreCase)]
    private static partial Regex NotPlayedHeading();

    [GeneratedRegex(@"^\|?\s*:?-+:?\s*(\|\s*:?-+:?\s*)*\|?$")]
    private static partial Regex Separator();

    [GeneratedRegex("`(?<text>[^`]+)`")]
    private static partial Regex BacktickSpan();
}
