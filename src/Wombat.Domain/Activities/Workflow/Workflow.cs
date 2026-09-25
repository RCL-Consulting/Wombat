namespace Wombat.Domain.Activities.Workflow;

public sealed record Workflow(
    int Version,
    string InitialState,
    IReadOnlyList<WorkflowState> States,
    IReadOnlyList<WorkflowTransition> Transitions)
{
    /// <summary>
    /// The key of the history row a create writes (<c>ActivityService</c>): from the initial state to itself. It is no
    /// move a workflow declares, and <see cref="TransitionLabel" /> names it as though it were one.
    /// </summary>
    public const string CreateTransitionKey = "create";

    /// <summary>
    /// The label this workflow declares for a state, or the key itself when it declares none (T189).
    /// </summary>
    /// <remarks>
    /// A refusal names a state by this, never by key, as <c>NewActivity</c>'s notices do, and so does every page, list,
    /// dashboard, committee snapshot and portfolio export that shows an activity's state, from its PINNED version (T220),
    /// so a refusal and the page beside it name the state alike: "Awaiting supervisor", not "submitted". The parser
    /// requires a label on every declared state, so the key is what a state this workflow does not declare is called: an
    /// activity whose stored state its pinned version has lost.
    /// </remarks>
    public string StateLabel(string stateKey)
        => States.FirstOrDefault(state => string.Equals(state.Key, stateKey, StringComparison.Ordinal))?.Label is { } label &&
           !string.IsNullOrWhiteSpace(label)
            ? label.Trim()
            : stateKey;

    /// <summary>
    /// The name a recorded move is shown by in an activity's history (T220): <see cref="WorkflowTransition.LabelFor" />,
    /// the name its button had and a refusal of it uses, for a move this workflow declares and for the create row
    /// (<see cref="CreateTransitionKey" />); the key itself for a move it does not declare.
    /// </summary>
    /// <remarks>
    /// The DSL declares no label for a move, so a label is only ever the key in words. Asked of the activity's PINNED
    /// version, which recorded every move on it, so the key is shown only for a row the version cannot account for.
    /// </remarks>
    public string TransitionLabel(string transitionKey)
        => string.Equals(transitionKey, CreateTransitionKey, StringComparison.Ordinal) ||
           Transitions.Any(transition => string.Equals(transition.Key, transitionKey, StringComparison.Ordinal))
            ? WorkflowTransition.LabelFor(transitionKey)
            : transitionKey;

    /// <summary>
    /// Whether any transition leaves this state. A non-terminal state with none is a dead end.
    /// </summary>
    /// <remarks>
    /// Field-write authorization treats a dead end as final. The CPSA workflows declare <c>declined</c> and
    /// <c>cancelled</c> non-terminal deliberately, since marking them terminal would fire credit on a refused or
    /// withdrawn request. Lifted out of <c>FieldPermissionEvaluator</c> so there is one copy of the test.
    /// </remarks>
    public bool HasOutgoingTransition(string stateKey)
        => Transitions.Any(transition => transition.From.Contains(stateKey, StringComparer.Ordinal));

    /// <summary>
    /// The moves out of <paramref name="stateKey" /> that lead on towards credit: those whose target can reach a terminal
    /// state without coming back through this one. In declaration order.
    /// </summary>
    /// <remarks>
    /// <see cref="CanReachTerminal" />'s withdrawal test, asked of every move out of a state. A move into a dead end is
    /// not listed, nor one into a state whose only way on is back through this one (a builder-made <c>cancelled</c> with
    /// a <c>reopen</c>), nor a self-transition. A move straight into a terminal state is. The author of a new activity
    /// who may take none of these moves has filed it by creating it: for a type born in <c>requested</c> the create is
    /// the submission, and <c>/activities/new</c> must not send the author's only other move, the withdrawal (T148).
    /// </remarks>
    public IEnumerable<WorkflowTransition> TransitionsLeadingOn(string stateKey)
        => Transitions.Where(transition =>
            transition.From.Contains(stateKey, StringComparer.Ordinal) &&
            CanReachTerminal(transition.To, avoidingState: stateKey));

    /// <summary>
    /// Whether a recorded move left the initial state by one of its <see cref="TransitionsLeadingOn" />: for a
    /// draft-born type, the move that filed the activity, or a later one that filed it again after it came back (T160).
    /// </summary>
    /// <remarks>
    /// Asked of a history row (its from state, to state and key) rather than of a declared transition, so the write
    /// path's entity rows and the page's DTO rows get one answer. The row must match a declared move by key and by
    /// target, so the create row, from the initial state to itself, never matches: no move that leads on is a
    /// self-transition.
    /// </remarks>
    public bool LeftInitialStateLeadingOn(string fromState, string toState, string transitionKey)
        => string.Equals(fromState, InitialState, StringComparison.Ordinal) &&
           TransitionsLeadingOn(InitialState).Any(transition =>
               string.Equals(transition.Key, transitionKey, StringComparison.Ordinal) &&
               string.Equals(transition.To, toState, StringComparison.Ordinal));

    /// <summary>
    /// Whether an activity in this state can still reach a terminal state, the only place credit fires, by any
    /// sequence of transitions. Optionally without passing through <paramref name="avoidingState" />, which excludes
    /// that state as a starting point too.
    /// </summary>
    /// <remarks>
    /// The EPA→tool gate (T122) asks this twice. A move into a state that can never reach a terminal one is never
    /// checked. A move into a state that can reach credit only by coming back through the state it left is a
    /// withdrawal (or a no-op), and an unchanged target is not re-checked on it. It is a reachability test rather than
    /// <see cref="HasOutgoingTransition" />, so a builder-made <c>cancelled</c> state with a <c>reopen</c> back to where
    /// it came from counts as a withdrawal too. Total: an unknown state reaches nothing.
    /// </remarks>
    public bool CanReachTerminal(string stateKey, string? avoidingState = null)
    {
        if (avoidingState is not null && string.Equals(stateKey, avoidingState, StringComparison.Ordinal))
        {
            return false;
        }

        var terminal = States
            .Where(state => state.Terminal)
            .Select(state => state.Key)
            .ToHashSet(StringComparer.Ordinal);

        var visited = new HashSet<string>(StringComparer.Ordinal) { stateKey };
        var frontier = new Queue<string>([stateKey]);
        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            if (terminal.Contains(current))
            {
                return true;
            }

            foreach (var next in Transitions
                         .Where(transition => transition.From.Contains(current, StringComparer.Ordinal))
                         .Select(transition => transition.To))
            {
                if ((avoidingState is null || !string.Equals(next, avoidingState, StringComparison.Ordinal)) &&
                    visited.Add(next))
                {
                    frontier.Enqueue(next);
                }
            }
        }

        return false;
    }
}
