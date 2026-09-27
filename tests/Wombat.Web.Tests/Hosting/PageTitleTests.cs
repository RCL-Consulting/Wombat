using System.Text.RegularExpressions;
using FluentAssertions;
using Wombat.Web.Tests.Navigation;

namespace Wombat.Web.Tests.Hosting;

/// <summary>
/// Every page's tab title is "<c>Stem · Wombat</c>", its stem in sentence case and the same words as the page's heading
/// (T190; D10 of the flow 01 review, T335).
/// </summary>
/// <remarks>
/// <para>
/// Before T190 the tabs read "Dashboard — Wombat", "Sign in - Wombat", and most pages had no suffix; capitals were mixed
/// ("My Activities" beside "My progress"); and a title often named the page differently from its heading ("Profile" on
/// the page headed "My account"). The redesigned shell names a page four times: its nav label, its heading, its
/// breadcrumb and its tab. They are the same words (the flow 01 token sheet's type rule).
/// </para>
/// <para>
/// This reads each routable component's source, found by reflection over <c>[Route]</c> as the router finds pages. A
/// bUnit render of every page would need every page's services; the source says what every branch of a page can title
/// it, which a render of one state does not. A title computed at run time is held here to the same expression as its
/// heading, and its literal words to sentence case; what it reads after a load is held by that page's own tests (the
/// builder's <c>ActivityTypeBuilderAccessTests</c>, which T190's note asked for: its tab was empty after the load).
/// </para>
/// <para>
/// So a page writes its stem in one of three ways, each of which this can read: literal text
/// (<c>&lt;PageTitle&gt;Audit log · Wombat&lt;/PageTitle&gt;</c>), a member (<c>@Heading · Wombat</c>, the member its
/// <c>PageHeader</c> takes too), or a choice of two literals (<c>@(IsNew ? "Create EPA" : "Edit EPA") · Wombat</c>). A
/// title after a refused post starts "Error: " (WCAG's technique G88; the change password page).
/// </para>
/// </remarks>
public sealed class PageTitleTests
{
    /// <summary>What every tab title ends with: U+00B7, a space each side.</summary>
    public const string Suffix = " · Wombat";

    private const string ErrorPrefix = "Error: ";

    /// <summary>
    /// The words a stem may capitalise after its first: acronyms and proper nouns. "College" names the College, the
    /// national body (CMSA constituent college); "a college" is not one.
    /// </summary>
    private static readonly HashSet<string> ProperWords = new(StringComparer.Ordinal)
    {
        "EPA", "EPAs", "MSF", "SSO", "STAR", "CPSA", "PDF", "Mini-CEX", "DOPS", "CbD", "College", "Wombat",
    };

    /// <summary>
    /// Pages whose heading the source cannot compare with the title, each with the reason and the test that holds it.
    /// </summary>
    private static readonly Dictionary<string, string> HeadingNotComparable = new(StringComparer.Ordinal)
    {
        ["Admin.Institutions.MySpecialitiesRedirect"] =
            "it has no heading: it redirects as it opens, to the College's specialities or the colleges",
        ["MultiSourceFeedback.MsfRespond"] =
            "each state has its own <h2>; MsfRespondPageHostingTests holds each state's tab to its heading",
    };

    /// <summary>
    /// Pages whose stem is a member this cannot read the words of, each with the reason. None since the placeholder page
    /// went with the nav's placeholders (T335, flow 01; the review's S22e).
    /// </summary>
    private static readonly Dictionary<string, string> WordsNotReadable = new(StringComparer.Ordinal);

    [Fact]
    public void EveryExemption_NamesARoutablePage()
    {
        var names = RoutablePages().Select(page => page.Name).ToHashSet(StringComparer.Ordinal);

        HeadingNotComparable.Keys.Concat(WordsNotReadable.Keys)
            .Where(name => !names.Contains(name))
            .Should().BeEmpty("an exemption for a page that has gone, or moved, is taken out");
    }

    [Fact]
    public void TheScan_FindsEveryRoutablePagesSource()
    {
        var pages = RoutablePages().ToList();

        pages.Should().HaveCountGreaterThanOrEqualTo(60, "guard: the scan finds the pages");
        pages.Where(page => !File.Exists(page.Path)).Select(page => page.Path).Should().BeEmpty(
            "every routable component's source sits where its namespace says");
    }

    [Fact]
    public void EveryPage_TitlesItsTab_StemThenTheSuffix()
    {
        var failures = new List<string>();
        foreach (var page in CheckedPages())
        {
            var titles = PageTitles(page.Markup).ToList();
            if (titles.Count == 0)
            {
                failures.Add($"{page.Name}: no <PageTitle>");
                continue;
            }

            foreach (var title in titles)
            {
                if (!title.EndsWith(Suffix, StringComparison.Ordinal))
                {
                    failures.Add($"{page.Name}: \"{title}\" does not end \"{Suffix}\"");
                }
                else if (Stem(title).Trim().Length == 0 || Stem(title) != Stem(title).Trim())
                {
                    failures.Add($"{page.Name}: \"{title}\" has no stem, or spaces around it");
                }
                else if (Stem(title).Contains("Wombat", StringComparison.Ordinal))
                {
                    failures.Add($"{page.Name}: \"{title}\" names Wombat twice");
                }
            }
        }

        string.Join(Environment.NewLine, failures).Should().BeEmpty();
    }

    [Fact]
    public void EveryPagesStem_IsInSentenceCase()
    {
        var failures = new List<string>();
        foreach (var page in CheckedPages())
        {
            foreach (var title in PageTitles(page.Markup))
            {
                var stem = Parse(Stem(title), page.Source);
                if (stem is null)
                {
                    failures.Add(
                        $"{page.Name}: \"{title}\": write the stem as literal text, @Member or @(cond ? \"A\" : \"B\")");
                    continue;
                }

                if (stem.Words is null)
                {
                    if (!WordsNotReadable.ContainsKey(page.Name))
                    {
                        failures.Add($"{page.Name}: \"{title}\": its member's words cannot be read; make it an expression-bodied " +
                            "string member (private string Heading => …;)");
                    }

                    continue;
                }

                failures.AddRange(stem.Words.SelectMany(words => CaseFaults(words.Text, words.StartsTheStem))
                    .Select(fault => $"{page.Name}: \"{title}\": {fault}"));
            }
        }

        string.Join(Environment.NewLine, failures).Should().BeEmpty();
    }

    [Fact]
    public void EveryPagesStem_IsItsHeading()
    {
        var failures = new List<string>();
        foreach (var page in CheckedPages().Where(page => !HeadingNotComparable.ContainsKey(page.Name)))
        {
            var headings = Headings(page.Markup).Select(heading => Parse(heading, page.Source)).ToList();
            if (headings.Count == 0 || headings.Any(heading => heading is null))
            {
                failures.Add(
                    $"{page.Name}: no heading this can read (a PageHeader Title, an <h1>, or an auth card's <h2>)");
                continue;
            }

            var said = headings.SelectMany(heading => heading!.Alternatives).ToHashSet(StringComparer.Ordinal);
            foreach (var title in PageTitles(page.Markup))
            {
                var stem = Parse(Stem(title), page.Source);
                foreach (var alternative in stem?.Alternatives ?? [])
                {
                    var words = alternative.StartsWith(ErrorPrefix, StringComparison.Ordinal)
                        ? alternative[ErrorPrefix.Length..]
                        : alternative;
                    if (!said.Contains(words))
                    {
                        failures.Add($"{page.Name}: the tab says \"{words}\", the heading \"{string.Join("\" or \"", said)}\"");
                    }
                }
            }
        }

        string.Join(Environment.NewLine, failures).Should().BeEmpty();
    }

    /// <summary>The rule's own words: a sentence-case stem passes, Title Case and a stray capital do not.</summary>
    [Theory]
    [InlineData("Activity inbox", true)]
    [InlineData("SSO mappings", true)]
    [InlineData("My MSF reports", true)]
    [InlineData("Create EPA", true)]
    [InlineData("Sub-specialities", true)]
    [InlineData("Activity Inbox", false)]
    [InlineData("Sub-Specialities", false)]
    [InlineData("activity inbox", false)]
    [InlineData("SSO Group Mappings", false)]
    public void SentenceCase_IsJudgedWordByWord(string stem, bool passes)
    {
        var faults = CaseFaults(stem, startsTheStem: true).ToList();

        (faults.Count == 0).Should().Be(passes, string.Join("; ", faults));
    }

    // ---- reading the source ----

    /// <summary>A routable component: its name under <c>Components.Pages</c>, its source, and what it renders.</summary>
    private sealed record Page(string Name, string Path, string Source, string Markup);

    /// <summary>
    /// A stem as the source writes it: what it can read (its alternatives), and its literal words, null when they are a
    /// member's this cannot read.
    /// </summary>
    private sealed record ParsedStem(IReadOnlyList<string> Alternatives, IReadOnlyList<(string Text, bool StartsTheStem)>? Words);

    private static IEnumerable<Page> RoutablePages()
    {
        var web = System.IO.Path.Combine(PageAccess.SolutionRoot(), "src", "Wombat.Web");
        foreach (var type in PageAccess.Pages.Select(page => page.Page).Distinct())
        {
            // A Razor component's namespace is its folder under the project (no page sets @namespace).
            var relative = type.FullName!["Wombat.Web.".Length..].Replace('.', System.IO.Path.DirectorySeparatorChar)
                + ".razor";
            var path = System.IO.Path.Combine(web, relative);
            var source = File.Exists(path) ? File.ReadAllText(path) : string.Empty;
            yield return new Page(type.FullName["Wombat.Web.Components.Pages.".Length..], path, source, MarkupOf(source));
        }
    }

    /// <summary>Every routable page: since T335's failure pages and Home followed the rule, none is set aside.</summary>
    private static IEnumerable<Page> CheckedPages() => RoutablePages();

    /// <summary>The source before <c>@code</c>, without Razor comments: what the page renders.</summary>
    private static string MarkupOf(string source)
    {
        var markup = Regex.Replace(source, @"@\*.*?\*@", string.Empty, RegexOptions.Singleline);
        var code = markup.IndexOf("@code", StringComparison.Ordinal);
        return code < 0 ? markup : markup[..code];
    }

    private static IEnumerable<string> PageTitles(string markup) =>
        Regex.Matches(markup, @"<PageTitle>(.*?)</PageTitle>", RegexOptions.Singleline)
            .Select(match => Collapse(match.Groups[1].Value));

    /// <summary>The title without its suffix; a title without one is judged whole, so each rule fails on its own.</summary>
    private static string Stem(string title) =>
        title.EndsWith(Suffix, StringComparison.Ordinal) ? title[..^Suffix.Length] : title;

    /// <summary>
    /// What heads the page: each <c>PageHeader</c>'s Title and each <c>&lt;h1&gt;</c>; on a page with neither, each
    /// <c>&lt;h2&gt;</c>, which heads the account pages' card (DESIGN.md § Account / auth page).
    /// </summary>
    private static IEnumerable<string> Headings(string markup)
    {
        var headings = new List<string>();
        foreach (Match header in Regex.Matches(markup, @"<PageHeader\b"))
        {
            var title = Regex.Match(markup[header.Index..], @"\sTitle=""");
            if (title.Success)
            {
                headings.Add(ReadAttribute(markup, header.Index + title.Index + title.Length));
            }
        }

        headings.AddRange(Elements(markup, "h1"));
        if (headings.Count == 0)
        {
            headings.AddRange(Elements(markup, "h2"));
        }

        return headings;
    }

    private static IEnumerable<string> Elements(string markup, string tag) =>
        Regex.Matches(markup, $@"<{tag}\b[^>]*>(.*?)</{tag}>", RegexOptions.Singleline)
            .Select(match => Collapse(match.Groups[1].Value));

    /// <summary>An attribute's value from its opening quote to its closing one, stepping over a <c>@( … )</c>'s strings.</summary>
    private static string ReadAttribute(string markup, int start)
    {
        var end = start;
        if (markup[start..].StartsWith("@(", StringComparison.Ordinal))
        {
            end = ClosingParen(markup, start + 1) + 1;
        }

        end = markup.IndexOf('"', end);
        return Collapse(markup[start..end]);
    }

    private static int ClosingParen(string text, int open)
    {
        var depth = 0;
        for (var i = open; i < text.Length; i++)
        {
            switch (text[i])
            {
                case '"':
                    i = text.IndexOf('"', i + 1);
                    break;
                case '(':
                    depth++;
                    break;
                case ')' when --depth == 0:
                    return i;
            }
        }

        throw new InvalidOperationException($"Unbalanced parentheses in: {text[open..]}");
    }

    /// <summary>
    /// Reads a stem or a heading: literal text; <c>@Member</c>, whose literal words are read from the member's body;
    /// <c>@(cond ? "A" : "B")</c>; or any other <c>@( … )</c> whose literal words are its strings. Null for a mix of
    /// text and code, which this cannot read.
    /// </summary>
    private static ParsedStem? Parse(string written, string source)
    {
        if (!written.Contains('@'))
        {
            return new ParsedStem([written], [(written, true)]);
        }

        var member = Regex.Match(written, @"^@([A-Za-z_][A-Za-z0-9_]*)$");
        if (member.Success)
        {
            var body = Regex.Match(source, $@"\bstring\??\s+{member.Groups[1].Value}\s*=>(.*?);", RegexOptions.Singleline);
            return new ParsedStem(["@" + member.Groups[1].Value], body.Success ? LiteralWords(body.Groups[1].Value) : null);
        }

        if (written.StartsWith("@(", StringComparison.Ordinal) && ClosingParen(written, 1) == written.Length - 1)
        {
            var expression = written[2..^1];
            var choice = Regex.Match(expression, @"^[^?""]+\?\s*""([^""]*)""\s*:\s*""([^""]*)""\s*$");
            if (!choice.Success)
            {
                return new ParsedStem(["@" + Regex.Replace(expression, @"\s+", string.Empty)], LiteralWords(expression));
            }

            string[] either = [choice.Groups[1].Value, choice.Groups[2].Value];
            return new ParsedStem(either, either.Select(text => (text, true)).ToList());
        }

        return null;
    }

    /// <summary>
    /// The literal text in C# code: each string's text, an interpolated string's text between its holes. A piece that
    /// opens its string starts the stem, so its first word is capitalised.
    /// </summary>
    private static List<(string Text, bool StartsTheStem)> LiteralWords(string code)
    {
        var words = new List<(string, bool)>();
        foreach (Match literal in Regex.Matches(code, @"(\$?)""((?:[^""\\]|\\.)*)"""))
        {
            var text = literal.Groups[2].Value;
            if (literal.Groups[1].Value.Length == 0)
            {
                words.Add((text, true));
                continue;
            }

            var pieces = Regex.Split(text, @"\{[^}]*\}");
            words.AddRange(pieces.Select((piece, index) => (piece, index == 0)));
        }

        return words;
    }

    /// <summary>
    /// Sentence case: the stem's first word is capitalised, and no later word is unless it is an acronym or a proper
    /// noun. A hyphenated word is judged part by part ("Sub-specialities", not "Sub-Specialities").
    /// </summary>
    private static IEnumerable<string> CaseFaults(string text, bool startsTheStem)
    {
        if (startsTheStem && text.StartsWith(ErrorPrefix, StringComparison.Ordinal))
        {
            text = text[ErrorPrefix.Length..];
        }

        var words = Regex.Matches(text, @"[\p{L}\p{N}][\p{L}\p{N}'’-]*").Select(match => match.Value).ToList();
        for (var index = 0; index < words.Count; index++)
        {
            var word = words[index];
            if (ProperWords.Contains(word))
            {
                continue;
            }

            var parts = word.Split('-');
            var first = startsTheStem && index == 0;
            var fine = parts.Select((part, at) => ProperWords.Contains(part)
                    || (first && at == 0
                        ? char.IsUpper(part[0]) && part[1..] == part[1..].ToLowerInvariant()
                        : part == part.ToLowerInvariant()))
                .All(ok => ok);
            if (!fine)
            {
                yield return first
                    ? $"its first word \"{word}\" should be capitalised, and only its first letter"
                    : $"\"{word}\" should not be capitalised (sentence case; acronyms and proper nouns only)";
            }
        }
    }

    private static string Collapse(string text) => Regex.Replace(text, @"\s+", " ").Trim();
}
