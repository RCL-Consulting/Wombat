namespace Wombat.Domain.Activities.Workflow;

/// <summary>
/// Where a move leaves the activity, and the sentence that says so once it is made (T342, B2, C3, E4).
/// </summary>
/// <param name="TransitionKey">The move.</param>
/// <param name="TargetStateKey">The state it moves to.</param>
/// <param name="TargetStateLabel">That state as the workflow labels it (<see cref="Workflow.StateLabel" />).</param>
/// <param name="TargetIsTerminal">Whether that state is marked <c>terminal</c>: where credit fires.</param>
/// <param name="TargetIsFinal">
/// Whether no move leaves that state: a terminal state, or a dead end such as the CPSA workflows' <c>declined</c> and
/// <c>cancelled</c>, which are deliberately not terminal (<see cref="Workflow.HasOutgoingTransition" />).
/// </param>
/// <remarks>
/// <para>
/// The DSL declares no label for a move, let alone a past tense, and a past tense made from the key ("Record
/// Discussioned") would be wrong. So the sentence is built from the target state's label, which the workflow does declare
/// (flow 03's Spec § 1, the result region):
/// <list type="bullet">
///   <item><b>A move that ends the activity</b> (<see cref="TargetIsFinal" />) reads as the state it ends in: "Logged.",
///   "Cancelled.", "Declined.", "Completed.", "Signed off.", "Discussed.". Nothing more can happen to it, so its state is
///   the whole of the news.</item>
///   <item><b>A move that leads on</b> says where the activity now is: "It is now Requested.". A <c>submit</c> is named
///   first, "Submitted. It is now Requested.", the Spec's words for the one such move a registrar makes; it is the only
///   past tense this class knows, written out, not derived. When the state it goes to is itself labelled "Submitted"
///   (the demo teaching session, QI project, research output and reflective note), the sentence is "Submitted." alone,
///   not "Submitted. It is now Submitted." (T342, R5).</item>
/// </list>
/// The page adds its second sentence (whose inbox it is in) from the hand-off (<see cref="MoveHandOff" />).
/// </para>
/// </remarks>
public sealed record MoveOutcome(
    string TransitionKey,
    string TargetStateKey,
    string TargetStateLabel,
    bool TargetIsTerminal,
    bool TargetIsFinal)
{
    /// <summary>The key of the one move whose past tense the Spec gives: a registrar's <c>submit</c>.</summary>
    public const string SubmitTransitionKey = "submit";

    /// <summary>The first sentence of the result region once the move is made. See the remarks on the type.</summary>
    public string ResultSentence
        => TargetIsFinal
            ? $"{TargetStateLabel}."
            : string.Equals(TransitionKey, SubmitTransitionKey, StringComparison.Ordinal)
                ? string.Equals(TargetStateLabel.Trim(), SubmittedWord, StringComparison.OrdinalIgnoreCase)
                    ? $"{SubmittedWord}."
                    : $"{SubmittedWord}. It is now {TargetStateLabel}."
                : $"It is now {TargetStateLabel}.";

    private const string SubmittedWord = "Submitted";

    /// <summary>The outcome of a move the workflow declares.</summary>
    public static MoveOutcome For(Workflow workflow, WorkflowTransition transition)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(transition);

        var target = workflow.States.FirstOrDefault(state => string.Equals(state.Key, transition.To, StringComparison.Ordinal));
        return new MoveOutcome(
            transition.Key,
            transition.To,
            workflow.StateLabel(transition.To),
            target?.Terminal ?? false,
            !workflow.HasOutgoingTransition(transition.To));
    }

    /// <summary>
    /// The outcome of the move <paramref name="transitionKey" />, taken from <paramref name="fromState" /> when given,
    /// or null when the workflow declares no such move.
    /// </summary>
    public static MoveOutcome? For(Workflow workflow, string transitionKey, string? fromState = null)
    {
        ArgumentNullException.ThrowIfNull(workflow);

        var transition = workflow.Transitions.FirstOrDefault(candidate =>
            string.Equals(candidate.Key, transitionKey, StringComparison.Ordinal) &&
            (fromState is null || candidate.From.Contains(fromState, StringComparer.Ordinal)));

        return transition is null ? null : For(workflow, transition);
    }
}
