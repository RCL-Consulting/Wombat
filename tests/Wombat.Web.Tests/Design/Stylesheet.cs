using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// A first-party stylesheet read as its rules, for the Design tests that hold <c>app.css</c> to DESIGN.md (T322, T328,
/// T335 flow 01). Comments are dropped and whitespace collapsed; a quoted string (a data URI, a font's address) is read
/// whole, so a <c>;</c> or a brace inside one splits nothing.
/// </summary>
internal sealed partial class Stylesheet
{
    private Stylesheet(IReadOnlyList<CssRule> rules) => Rules = rules;

    /// <summary>Every rule, in order: the innermost at-rule it sits in ("" at the top level), its selector, its declarations.</summary>
    /// <remarks>
    /// An at-rule that holds declarations itself (<c>@font-face</c>) is a rule too, its prelude standing as the selector.
    /// </remarks>
    public IReadOnlyList<CssRule> Rules { get; }

    public static Stylesheet AppCss() => Load(WebFile("wwwroot", "app.css"));

    public static Stylesheet Load(string path) => new(ParseRules(File.ReadAllText(path)));

    /// <summary>The custom properties <c>:root</c> declares, by name (<c>--text-color</c>), and the value each is given.</summary>
    public IReadOnlyDictionary<string, string> Root()
        => Rules.Where(rule => rule.AtRule.Length == 0 && rule.Selector == ":root")
            .SelectMany(rule => rule.Declarations)
            .Where(declaration => declaration.Property.StartsWith("--", StringComparison.Ordinal))
            .GroupBy(declaration => declaration.Property)
            .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal);

    /// <summary>The top-level rules naming <paramref name="selector" /> as one of their selectors, in order.</summary>
    public IReadOnlyList<CssRule> RulesFor(string selector, string atRule = "")
        => Rules.Where(rule => rule.AtRule == atRule && rule.Selectors.Contains(selector)).ToList();

    /// <summary>
    /// What the rules for these selectors give one element that carries them all, in document order: a later declaration
    /// of a property wins, as the cascade has it for selectors of one specificity (<c>.btn</c> then <c>.btn-primary</c>).
    /// </summary>
    public IReadOnlyDictionary<string, string> Computed(params string[] selectors)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var rule in Rules.Where(rule => rule.AtRule.Length == 0 && rule.Selectors.Intersect(selectors).Any()))
        {
            foreach (var declaration in rule.Declarations)
            {
                result[declaration.Property] = declaration.Value;
            }
        }

        return result;
    }

    /// <summary>
    /// As <see cref="Computed" />, where <paramref name="atRule" /> applies (<c>@media (max-width: 640.98px)</c>): the
    /// top-level rules and that at-rule's, in document order, so a later rule of one specificity wins, as in the cascade.
    /// </summary>
    public IReadOnlyDictionary<string, string> ComputedIn(string atRule, params string[] selectors)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var rule in Rules.Where(rule => (rule.AtRule.Length == 0 || rule.AtRule == atRule) && rule.Selectors.Intersect(selectors).Any()))
        {
            foreach (var declaration in rule.Declarations)
            {
                result[declaration.Property] = declaration.Value;
            }
        }

        return result;
    }

    /// <summary>
    /// What the cascade gives one element that <paramref name="selectors" /> all match (list every selector in the sheet
    /// that matches it, the base rule's too): per property, <c>!important</c> first, then the higher specificity, then the
    /// later rule. <see cref="Computed" /> reads source order alone, so it let <c>.details-list--stacked &gt; div</c> win on
    /// paper while the later <c>.details-list &gt; div</c> won in the browser (T339 step F review). <c>margin</c> and
    /// <c>padding</c> are read as their four sides, so a shorthand and a side compete as they do in the browser.
    /// </summary>
    public IReadOnlyDictionary<string, string> Cascaded(params string[] selectors) => CascadedIn(null, selectors);

    /// <summary>As <see cref="Cascaded" />, with the top-level rules and those in <paramref name="atRule" />, when given.</summary>
    public IReadOnlyDictionary<string, string> CascadedIn(string? atRule, params string[] selectors)
    {
        var winners = new Dictionary<string, (bool Important, (int, int, int) Specificity, int Index, string Value)>(StringComparer.Ordinal);
        foreach (var rule in Rules.Where(rule => rule.AtRule.Length == 0 || rule.AtRule == atRule))
        {
            var matched = rule.Selectors.Where(selectors.Contains).ToList();
            if (matched.Count == 0)
            {
                continue;
            }

            var specificity = matched.Select(Specificity).Max();
            foreach (var declaration in rule.Declarations)
            {
                foreach (var (property, value) in Longhands(declaration))
                {
                    var candidate = (declaration.Important, specificity, rule.Index, value);
                    if (!winners.TryGetValue(property, out var current)
                        || (candidate.Important, candidate.specificity, candidate.Index).CompareTo((current.Important, current.Specificity, current.Index)) > 0)
                    {
                        winners[property] = candidate;
                    }
                }
            }
        }

        return winners.ToDictionary(pair => pair.Key, pair => pair.Value.Value, StringComparer.Ordinal);
    }

    /// <summary>
    /// A selector's specificity, as (ids, classes/attributes/pseudo-classes, types/pseudo-elements). <c>:is()</c>,
    /// <c>:not()</c> and <c>:has()</c> count their most specific argument; <c>:where()</c> counts nothing.
    /// </summary>
    public static (int Ids, int Classes, int Types) Specificity(string selector)
    {
        int ids = 0, classes = 0, types = 0;
        var i = 0;
        while (i < selector.Length)
        {
            var character = selector[i];
            if (character == '#')
            {
                ids++;
                i = SkipName(selector, i + 1);
            }
            else if (character == '.')
            {
                classes++;
                i = SkipName(selector, i + 1);
            }
            else if (character == '[')
            {
                classes++;
                i = selector.IndexOf(']', i) + 1;
            }
            else if (character == ':' && i + 1 < selector.Length && selector[i + 1] == ':')
            {
                types++;
                i = SkipArguments(selector, SkipName(selector, i + 2), out _);
            }
            else if (character == ':')
            {
                var end = SkipName(selector, i + 1);
                var name = selector[(i + 1)..end];
                i = SkipArguments(selector, end, out var arguments);
                if (name is "is" or "not" or "has" && arguments is not null)
                {
                    var most = SplitOutsideQuotes(arguments, ',').Select(argument => Specificity(argument.Trim())).Max();
                    ids += most.Ids;
                    classes += most.Classes;
                    types += most.Types;
                }
                else if (name != "where")
                {
                    classes++;
                }
            }
            else if (char.IsLetter(character))
            {
                types++;
                i = SkipName(selector, i);
            }
            else
            {
                i++;
            }
        }

        return (ids, classes, types);
    }

    private static int SkipName(string selector, int i)
    {
        while (i < selector.Length && (char.IsLetterOrDigit(selector[i]) || selector[i] is '-' or '_' or '\\'))
        {
            i++;
        }

        return i;
    }

    private static int SkipArguments(string selector, int i, out string? arguments)
    {
        arguments = null;
        if (i >= selector.Length || selector[i] != '(')
        {
            return i;
        }

        var depth = 0;
        for (var j = i; j < selector.Length; j++)
        {
            depth += selector[j] switch { '(' => 1, ')' => -1, _ => 0 };
            if (depth == 0)
            {
                arguments = selector[(i + 1)..j];
                return j + 1;
            }
        }

        throw new FormatException($"'{selector}' has an unclosed parenthesis.");
    }

    private static IEnumerable<(string Property, string Value)> Longhands(CssDeclaration declaration)
    {
        if (declaration.Property is not ("margin" or "padding"))
        {
            yield return (declaration.Property, declaration.Value);
            yield break;
        }

        var values = SplitOutsideQuotes(declaration.Value, ' ').Where(value => value.Length > 0).ToArray();
        var (top, right, bottom, left) = values.Length switch
        {
            1 => (values[0], values[0], values[0], values[0]),
            2 => (values[0], values[1], values[0], values[1]),
            3 => (values[0], values[1], values[2], values[1]),
            _ => (values[0], values[1], values[2], values[3])
        };
        yield return (declaration.Property, declaration.Value);
        yield return ($"{declaration.Property}-top", top);
        yield return ($"{declaration.Property}-right", right);
        yield return ($"{declaration.Property}-bottom", bottom);
        yield return ($"{declaration.Property}-left", left);
    }

    public static string WebFile(params string[] parts) => Path.Combine([SolutionRoot(), "src", "Wombat.Web", .. parts]);

    public static string SolutionFile(params string[] parts) => Path.Combine([SolutionRoot(), .. parts]);

    private static string SolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && directory.GetFiles("Wombat.sln").Length == 0)
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not find Wombat.sln.");
    }

    /// <summary>The rules of <paramref name="css" />, in order.</summary>
    public static IReadOnlyList<CssRule> Parse(string css) => ParseRules(css);

    /// <summary><paramref name="css" /> with its comments removed.</summary>
    public static string WithoutComments(string css) => Comment().Replace(css, string.Empty);

    private static List<CssRule> ParseRules(string css)
    {
        css = Comment().Replace(css, string.Empty);
        var rules = new List<CssRule>();
        var open = new Stack<(bool IsAtRule, string Prelude)>();
        var buffer = new StringBuilder();
        char? quote = null;

        foreach (var character in css)
        {
            if (quote is not null)
            {
                buffer.Append(character);
                if (character == quote)
                {
                    quote = null;
                }
            }
            else if (character is '"' or '\'')
            {
                quote = character;
                buffer.Append(character);
            }
            else if (character == '{')
            {
                var prelude = Collapse(buffer.ToString());
                open.Push((prelude.StartsWith('@'), prelude));
                buffer.Clear();
            }
            else if (character == '}')
            {
                var (isAtRule, prelude) = open.Pop();
                var body = buffer.ToString();
                if (!isAtRule || body.Trim().Length > 0)
                {
                    var atRule = open.Where(block => block.IsAtRule).Select(block => block.Prelude).FirstOrDefault() ?? string.Empty;
                    rules.Add(new CssRule(atRule, prelude, Declarations(body), rules.Count));
                }

                buffer.Clear();
            }
            else
            {
                buffer.Append(character);
            }
        }

        if (open.Count > 0)
        {
            throw new InvalidOperationException("The stylesheet's braces do not balance.");
        }

        return rules;
    }

    private static List<CssDeclaration> Declarations(string body)
        => SplitOutsideQuotes(body, ';')
            .Select(Collapse)
            .Where(text => text.Length > 0)
            .Select(text =>
            {
                var colon = text.IndexOf(':', StringComparison.Ordinal);
                var property = text[..colon].Trim();
                var value = text[(colon + 1)..].Trim();
                var important = value.EndsWith("!important", StringComparison.Ordinal);
                return new CssDeclaration(property, important ? value[..^"!important".Length].Trim() : value, important);
            })
            .ToList();

    /// <summary>Splits on <paramref name="separator" /> where it stands outside quotes and outside parentheses.</summary>
    public static List<string> SplitOutsideQuotes(string text, char separator)
    {
        var parts = new List<string>();
        var current = new StringBuilder();
        char? quote = null;
        var depth = 0;
        foreach (var character in text)
        {
            if (quote is not null)
            {
                if (character == quote)
                {
                    quote = null;
                }
            }
            else if (character is '"' or '\'')
            {
                quote = character;
            }
            else if (character == '(')
            {
                depth++;
            }
            else if (character == ')')
            {
                depth--;
            }
            else if (character == separator && depth == 0)
            {
                parts.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(character);
        }

        parts.Add(current.ToString());
        return parts;
    }

    private static string Collapse(string text) => Whitespace().Replace(text, " ").Trim();

    public static double Rem(string length)
    {
        var match = RemLength().Match(length);
        return match.Success
            ? double.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture)
            : throw new FormatException($"'{length}' is not a length in rem.");
    }

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex Comment();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    [GeneratedRegex(@"^(?<value>[0-9]*\.?[0-9]+)rem$")]
    private static partial Regex RemLength();
}

/// <summary>One rule of a stylesheet: the at-rule it sits in ("" at the top level), its selector and declarations.</summary>
internal sealed record CssRule(string AtRule, string Selector, IReadOnlyList<CssDeclaration> Declarations, int Index)
{
    /// <summary>The selector list, split where a comma stands outside parentheses (<c>:is(a, b)</c> is one).</summary>
    public IReadOnlyList<string> Selectors { get; } = Stylesheet.SplitOutsideQuotes(Selector, ',')
        .Select(part => part.Trim())
        .Where(part => part.Length > 0)
        .ToList();

    /// <summary>The last value this rule gives <paramref name="property" />, or null.</summary>
    public string? Value(string property) => Declarations.LastOrDefault(declaration => declaration.Property == property)?.Value;
}

internal sealed record CssDeclaration(string Property, string Value, bool Important)
{
    public override string ToString() => $"{Property}: {Value}{(Important ? " !important" : string.Empty)}";
}
