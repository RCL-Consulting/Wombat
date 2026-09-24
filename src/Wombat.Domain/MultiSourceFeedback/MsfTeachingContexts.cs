using System.Text.RegularExpressions;

namespace Wombat.Domain.MultiSourceFeedback;

/// <summary>
/// The one reading of a learner invitation's teaching context: how it is stored, and when two are the same. (T164)
/// </summary>
public static partial class MsfTeachingContexts
{
    /// <summary>The longest context stored; the column's length.</summary>
    public const int MaximumLength = 200;

    /// <summary>
    /// The context as stored: trimmed, each run of whitespace one space. Null when nothing is left.
    /// </summary>
    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Whitespace().Replace(value.Trim(), " ");
    }

    /// <summary>
    /// The distinct contexts among <paramref name="contexts" />, each once, ordered for reading. Two are the same when
    /// they normalise to the same text ignoring case ("Ward round" and "ward  round"); blanks are no context.
    /// </summary>
    /// <remarks>
    /// Of several spellings of one context, the ordinal first is shown ("Ward round" before "ward round"), so the answer
    /// does not depend on the order the responses were read in.
    /// </remarks>
    public static IReadOnlyList<string> Distinct(IEnumerable<string?> contexts)
    {
        ArgumentNullException.ThrowIfNull(contexts);

        return contexts
            .Select(Normalize)
            .OfType<string>()
            .GroupBy(context => context.ToUpperInvariant(), StringComparer.Ordinal)
            .Select(group => group.Min(StringComparer.Ordinal)!)
            .OrderBy(context => context, StringComparer.OrdinalIgnoreCase)
            .ThenBy(context => context, StringComparer.Ordinal)
            .ToArray();
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
