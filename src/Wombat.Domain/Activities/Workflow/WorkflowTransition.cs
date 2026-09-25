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
}
