using System.Globalization;

namespace Wombat.Domain.Activities.Workflow;

public sealed record WorkflowTransition(
    string Key,
    IReadOnlyList<string> From,
    string To,
    ActorRule Actor,
    bool RequiresNote,
    IReadOnlyList<string> RequiresFields,
    TransitionValidation Validation)
{
    /// <summary>
    /// The name a person sees for a move: its key split into words at '_' and '-', in title case, so <c>sign_off</c> is
    /// "Sign Off". A key that is nothing but separators is named as it is.
    /// </summary>
    /// <remarks>
    /// The DSL declares no label for a transition, so its button on the activity's page is named from the key, and a
    /// refusal of the move names it by the same text (T189), and so does the activity's history once it is made
    /// (<see cref="Workflow.TransitionLabel" />, T220). This is the one implementation of all three. Invariant culture,
    /// so the name does not depend on the server's locale.
    /// </remarks>
    public static string LabelFor(string transitionKey)
    {
        var words = string.Join(" ", transitionKey.Split('-', '_')).Trim();
        return string.IsNullOrWhiteSpace(words)
            ? transitionKey
            : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(words);
    }

    /// <summary>
    /// A move as an instruction about the activity, in lower case, for running text: "complete it", "decline it", "return
    /// it", "record the discussion", "sign it off". The page joins them: "Complete it, or decline it with a note …"
    /// (T342). Null for a key that is nothing but separators.
    /// </summary>
    /// <remarks>
    /// Built from the key's words, as <see cref="LabelFor" /> is, since the DSL declares no words for a move. A one-word
    /// key is a verb and takes "it". A longer key is a verb and what it acts on: "it" goes between a verb and its particle
    /// (<c>sign_off</c>: "sign it off"), and any other object is the activity's own, "the discussion"
    /// (<c>record_discussion</c>), so no phrase reads "record discussion it". Invariant culture, as the label.
    /// </remarks>
    public static string? ImperativeFor(string transitionKey)
    {
        ArgumentNullException.ThrowIfNull(transitionKey);

        var words = transitionKey
            .Split(['-', '_', ' '], StringSplitOptions.RemoveEmptyEntries)
            .Select(word => word.ToLower(CultureInfo.InvariantCulture))
            .ToList();

        return words.Count switch
        {
            0 => null,
            1 => $"{words[0]} it",
            _ when words.Count == 2 && Particles.Contains(words[1]) => $"{words[0]} it {words[1]}",
            _ => $"{words[0]} the {string.Join(" ", words.Skip(1))}"
        };
    }

    // The particles a two-word move key ends in, where "it" goes before them: "sign it off", "hand it back".
    private static readonly HashSet<string> Particles =
        new(["off", "on", "up", "back", "out", "down", "in", "over", "away"], StringComparer.Ordinal);
}
