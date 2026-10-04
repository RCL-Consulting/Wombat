using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Application.Features.Activities.Services;

/// <summary>
/// Whether a request was DECLINED, read from its pinned workflow, not from a state key (T342, R6, E5; T355, build review
/// R4): the one rule File it again (<c>GetFileAgainSourceQuery</c>) and Home's Recent decisions
/// (<see cref="DecidedOnYours" />) share, so a row offers File it again, and carries no count line, exactly where the Log
/// page will copy it.
/// </summary>
/// <remarks>
/// Its current state is a dead end (no move leaves it) that is not terminal, and the move that put it there was made by
/// someone other than its subject and creator, under a move the author named its maker for: the move the workflow records
/// under that key has a <c>field:</c> arm and no arm of the author's. That is the CPSA <c>decline</c>, whatever a
/// builder's type calls its state; it is not a <c>cancel</c>, which the author may make (the legacy Mini-CEX's assessor may
/// cancel too, and that is no decline of anything she asked), nor a programme admin's <c>reject</c> (a <c>role:</c> arm:
/// nobody she named), nor a completion, which is terminal.
/// </remarks>
public static class ActivityDecline
{
    /// <summary>
    /// The move that declined <paramref name="source" />, or null when it was not declined. <paramref name="workflow" /> is
    /// its PINNED workflow, the one that recorded the decline; null (a pin that cannot be read) is no decline anyone can
    /// act on. <paramref name="source" />'s transitions must be loaded.
    /// </summary>
    public static ActivityTransition? MoveOf(Workflow? workflow, Activity source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (workflow is null)
        {
            return null;
        }

        var state = workflow.States.FirstOrDefault(candidate =>
            string.Equals(candidate.Key, source.CurrentState, StringComparison.Ordinal));
        if (state is null || state.Terminal || workflow.HasOutgoingTransition(state.Key))
        {
            return null;
        }

        var last = source.Transitions
            .OrderBy(transition => transition.OccurredOn)
            .ThenBy(transition => transition.Id)
            .LastOrDefault();

        if (last is null ||
            !string.Equals(last.ToState, source.CurrentState, StringComparison.Ordinal) ||
            string.Equals(last.ActorUserId, source.SubjectUserId, StringComparison.Ordinal) ||
            string.Equals(last.ActorUserId, source.CreatedByUserId, StringComparison.Ordinal))
        {
            return null;
        }

        // R6: the recorded move must be one only the person she named may make.
        var move = workflow.Transitions.FirstOrDefault(transition =>
            string.Equals(transition.Key, last.TransitionKey, StringComparison.Ordinal) &&
            transition.From.Contains(last.FromState, StringComparer.Ordinal) &&
            string.Equals(transition.To, last.ToState, StringComparison.Ordinal));
        return move is not null && IsNamedPersonsMove(move.Actor) ? last : null;
    }

    /// <summary>
    /// Whether a move's actor rule admits someone the author named (a <c>field:</c> arm, alone or narrowed) and no arm of
    /// the author's own (<see cref="ActivityWaiting.IsAuthorArm" />).
    /// </summary>
    private static bool IsNamedPersonsMove(ActorRule rule)
    {
        var arms = rule is CombinedActorRule { CombinationKind: ActorRuleCombinationKind.Any } any ? any.Rules : [rule];
        return arms.Count > 0 && !arms.Any(ActivityWaiting.IsAuthorArm) && arms.Any(NamesAField);
    }

    private static bool NamesAField(ActorRule arm) => arm switch
    {
        FieldUserActorRule => true,
        CombinedActorRule { CombinationKind: ActorRuleCombinationKind.All } all => all.Rules.Any(NamesAField),
        _ => false
    };
}
