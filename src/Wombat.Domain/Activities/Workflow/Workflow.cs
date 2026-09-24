namespace Wombat.Domain.Activities.Workflow;

public sealed record Workflow(
    int Version,
    string InitialState,
    IReadOnlyList<WorkflowState> States,
    IReadOnlyList<WorkflowTransition> Transitions)
{
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
