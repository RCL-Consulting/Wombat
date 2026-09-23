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
