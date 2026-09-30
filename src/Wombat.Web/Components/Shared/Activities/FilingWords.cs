using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Web.Components.Shared.Activities;

/// <summary>
/// Where filing a new activity leaves it, as Log an activity words it before the move (T342, flow 03; Spec § 1 and § 2):
/// the primary button, the check line above it, and whose inbox it lands in.
/// </summary>
/// <param name="MoveLabel">The move's own label ("Submit", "Log"; <see cref="WorkflowTransition.LabelFor" />, E4).</param>
/// <param name="LandingStateLabel">The state the activity is in once filed ("Requested", "Logged").</param>
/// <param name="HandOffFieldKey">
/// The <c>user</c> field whose person takes the next move (<see cref="MoveHandOff" />), or null when it goes to nobody by
/// name: the teaching log's Supervising consultant receives nothing, so the log's button stays "Log".
/// </param>
/// <param name="HandOffNoun">That field's label in lower case ("assessor"), for "the assessor you name".</param>
/// <param name="NobodyActsNext">
/// True when no move out of the landing state leads on towards credit (<see cref="Workflow.TransitionsLeadingOn" />): it
/// is finished once filed.
/// </param>
public sealed record FilingWords(
    string MoveLabel,
    string LandingStateLabel,
    string? HandOffFieldKey,
    string HandOffNoun,
    bool NobodyActsNext)
{
    /// <summary>The noun the Spec uses when no hand-off field is known: "The assessor you name fills this in".</summary>
    public const string DefaultNoun = "assessor";

    /// <summary>
    /// The words for filing a new activity of this type: by <paramref name="authorMove" />, the author's first move out of
    /// the initial state that leads on; or, when there is none (<paramref name="authorMove" /> null), by the create itself,
    /// which is then the filing (a type born in <c>requested</c>, or born terminal; T148).
    /// </summary>
    public static FilingWords For(Workflow workflow, FormSchema schema, WorkflowTransition? authorMove)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(schema);

        var landing = authorMove?.To ?? workflow.InitialState;
        var handOff = HandOffFieldFor(workflow, schema, landing);
        var noun = handOff is null
            ? DefaultNoun
            : schema.Sections.SelectMany(section => section.Fields)
                .FirstOrDefault(field => string.Equals(field.Key, handOff, StringComparison.Ordinal))?.Label?.Trim().ToLowerInvariant()
                ?? DefaultNoun;

        return new FilingWords(
            authorMove is null ? SubmitLabel : WorkflowTransition.LabelFor(authorMove.Key),
            workflow.StateLabel(landing),
            handOff,
            string.IsNullOrWhiteSpace(noun) ? DefaultNoun : noun,
            !workflow.TransitionsLeadingOn(landing).Any());
    }

    /// <summary>The label of the move a registrar files most things by (E4).</summary>
    public const string SubmitLabel = "Submit";

    /// <summary>
    /// The user field whose person takes the first move out of <paramref name="state" /> that leads on: the question
    /// <see cref="MoveHandOff" /> asks of a move's target, asked of a state.
    /// </summary>
    public static string? HandOffFieldFor(Workflow workflow, FormSchema schema, string state)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(schema);

        // MoveHandOff reads only the move's target, so any declared move pointed at the state asks it of the state.
        var probe = workflow.Transitions.FirstOrDefault();
        return probe is null ? null : MoveHandOff.NomineeFieldFor(workflow, schema, probe with { To = state });
    }

    /// <summary>
    /// The button: "Submit to Fatima Khumalo" only when the move hands the activity to the person a filled field names,
    /// else the move's label alone ("Submit", "Log"; C3, E4).
    /// </summary>
    public string ButtonLabel(string? handOffName) => MoveButtonLabel(MoveLabel, HandOffFieldKey, handOffName);

    /// <summary>The button while its move runs (Spec § 1, "Running"; T234).</summary>
    public string RunningLabel => Running(MoveLabel);

    /// <summary>
    /// A move's button, on Log an activity and the activity page alike (E4, C3): "Submit to Fatima Khumalo" only when the
    /// move has a hand-off field and it names someone, else the move's own label ("Submit", "Log", "Complete").
    /// </summary>
    public static string MoveButtonLabel(string moveLabel, string? handOffFieldKey, string? handOffName)
        => handOffFieldKey is not null && !string.IsNullOrWhiteSpace(handOffName)
            ? $"{moveLabel} to {handOffName}"
            : moveLabel;

    /// <summary>
    /// What a pressed move's button says while it runs (Spec § 1, "Running"; T234): "Submitting…", "Completing…",
    /// "Logging…"; for a label of more than one word, the first word's -ing form with the rest kept, "Recording
    /// discussion…", "Signing off…" (T350, note 2). "Working…" only for a first word it cannot form: none, or one that is
    /// not all letters.
    /// </summary>
    public static string Running(string moveLabel)
    {
        var label = (moveLabel ?? string.Empty).Trim();
        var space = label.IndexOf(' ', StringComparison.Ordinal);
        var word = space < 0 ? label : label[..space];
        var rest = space < 0 ? string.Empty : label[space..];
        if (word.Length == 0 || !word.All(char.IsLetter))
        {
            return "Working…";
        }

        return IngForm(word) + rest + "…";
    }

    // A verb's -ing form, its first letter's case kept. The table holds the verbs whose last consonant doubles, which no
    // spelling rule can tell from the letters alone ("submit" doubles, "visit" does not); cancel doubles as South African
    // English spells it. Beyond the seeds' moves it holds the ones a builder is likely to name (the T350 build review, D3
    // and G6: "Refering…"); a doubling verb outside it takes the plain -ing, the limit DESIGN.md states.
    private static readonly HashSet<string> DoublesItsLastConsonant = new(StringComparer.Ordinal)
    {
        "submit", "log", "cancel", "refer", "prefer", "defer", "confer", "transfer", "admit", "commit", "permit", "omit",
        "emit", "begin", "regret", "stop", "drop", "plan", "flag", "tag", "run", "set", "get", "put", "ship", "rerun",
        "reset", "upset", "equip"
    };

    private static string IngForm(string word)
    {
        var ing = word.ToLowerInvariant() switch
        {
            var lower when DoublesItsLastConsonant.Contains(lower) => lower + lower[^1] + "ing",
            var lower when lower.EndsWith("ie", StringComparison.Ordinal) => lower[..^2] + "ying",
            var lower when lower.EndsWith("ee", StringComparison.Ordinal) || lower.EndsWith("ye", StringComparison.Ordinal)
                || lower.EndsWith("oe", StringComparison.Ordinal) => lower + "ing",
            var lower when lower.Length > 2 && lower.EndsWith('e') => lower[..^1] + "ing",
            var lower => lower + "ing"
        };
        return char.IsUpper(word[0]) ? char.ToUpperInvariant(ing[0]) + ing[1..] : ing;
    }

    /// <summary>The check line's lead: "When you submit:", "When you log it:".</summary>
    public string CheckLead
        => string.Equals(MoveLabel, SubmitLabel, StringComparison.Ordinal)
            ? "When you submit:"
            : $"When you {MoveLabel.ToLowerInvariant()} it:";

    /// <summary>
    /// The check line after its lead (Spec § 1, "Check line"; C11): the target state by its label and the person by name,
    /// never a past tense or a pronoun.
    /// </summary>
    /// <param name="handOffName">The name the hand-off field holds now, or null.</param>
    /// <param name="creditsNothing">Whether the type's credit rules credit nothing.</param>
    /// <param name="daysLate">How many days after the encounter a filing today is, when that is late; otherwise null.</param>
    public string CheckText(string? handOffName, bool creditsNothing, int? daysLate)
    {
        var text = NobodyActsNext
            ? $"it is {LandingStateLabel} at once{(creditsNothing ? ", and credits nothing" : string.Empty)}. Nobody else acts on it."
            : HandOffFieldKey is null
                ? $"it is {LandingStateLabel} until the next step is taken."
                : string.IsNullOrWhiteSpace(handOffName)
                    ? $"it goes to the Activity inbox of the {HandOffNoun} you name, and stays {LandingStateLabel} until that {HandOffNoun} acts on it."
                    : $"it goes to {handOffName}'s Activity inbox and stays {LandingStateLabel} until {handOffName} acts on it.";

        return daysLate is int days
            ? $"{text} Filed today, {days} days after the encounter: it will be recorded as late."
            : text;
    }

    /// <summary>The result's second sentence once filed and handed to a person (Spec § 1, "Result").</summary>
    public static string InboxSentence(string name) => $"It is in {name}'s Activity inbox.";

    /// <summary>The result's second sentence after a draft is saved (C2).</summary>
    public const string NobodysInboxSentence = "It is in nobody's inbox until you submit it.";
}
