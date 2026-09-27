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
