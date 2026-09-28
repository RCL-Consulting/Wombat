namespace Wombat.Web.Security;

/// <summary>
/// A refused password as a page shows it (T339, flow 02): an optional first line saying what did not happen, the
/// sentences for everything but the rules, in the order sent, and the rules broken, in the one order, under
/// <see cref="PasswordRuleMessages.Heading" />.
/// </summary>
/// <param name="Headline">"Your password was not changed.", or null where the page says nothing before the reasons.</param>
/// <param name="Sentences">Every reason but a broken rule, each once.</param>
/// <param name="BrokenRules">The rules broken, each once, in the one order; empty when none was.</param>
public sealed record PasswordRefusal(string? Headline, IReadOnlyList<string> Sentences, IReadOnlyList<string> BrokenRules)
{
    /// <summary>Nothing refused.</summary>
    public static PasswordRefusal None { get; } = new(null, [], []);

    /// <summary>Whether there is anything to say.</summary>
    public bool Any => Sentences.Count > 0 || BrokenRules.Count > 0;

    /// <summary>
    /// The whole refusal as one list of sentences, in reading order: the headline, the reasons, and the heading followed by
    /// the rules broken. What a page shows when it runs them together in one line.
    /// </summary>
    public IReadOnlyList<string> Flat
    {
        get
        {
            if (!Any)
            {
                return [];
            }

            var sentences = new List<string>();
            if (Headline is not null)
            {
                sentences.Add(Headline);
            }

            sentences.AddRange(Sentences);
            if (BrokenRules.Count > 0)
            {
                sentences.Add(PasswordRuleMessages.Heading);
                sentences.AddRange(BrokenRules);
            }

            return sentences;
        }
    }
}
