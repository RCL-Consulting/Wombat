using Wombat.Domain.Activities.Schema;

namespace Wombat.Domain.Activities.Workflow;

/// <summary>
/// Whom a move hands the activity to: the <c>user</c> field whose person takes the next move (T342, B2, C3, E4).
/// </summary>
/// <remarks>
/// <para>
/// A move's button reads "Submit to Fatima Khumalo" only when the move puts the activity where its next move belongs to
/// the person a filled user field names. So this asks the workflow, not the move's own actor: of the moves out of the
/// move's target state that lead on towards credit (<see cref="Workflow.TransitionsLeadingOn" />, so a withdrawal or a
/// decline into a dead end does not count), the first whose actor rule has a <c>field:</c> arm naming a <c>user</c>
/// field of the schema. The Mini-CEX's <c>submit</c> goes to <c>requested</c>, whose <c>complete</c> is
/// <c>field:assessor_user_id</c>; the reflection's and the portfolio review's <c>submit</c> likewise (their
/// <c>record_discussion</c> and <c>sign_off</c>). A move into a state no move leaves hands nothing on: the teaching log's
/// <c>log</c>, whose Supervising consultant is a user field that receives nothing, and every <c>cancel</c>.
/// </para>
/// <para>
/// It returns the field's key, not a person. Whether the field is filled, and whom it names, is read by the caller from
/// whatever data it holds: the server from the stored data (<c>ActivityActionDto.HandsToName</c>), the Log page from the
/// values being typed, resolving the name from the picker's own options without a round trip.
/// </para>
/// <para>
/// A rule arm that is a role or a scope (a Demo type's <c>role:Coordinator</c>) names nobody, so the button stays the
/// move's own label; a <c>field:</c> arm combined with one (<c>field:assessor_user_id|role:Coordinator</c>) still names
/// the field's person, who can take the next move. Declaration order throughout, so the answer is stable.
/// </para>
/// </remarks>
public static class MoveHandOff
{
    /// <summary>The user field whose person the move hands the activity to, or null when it hands it to nobody by name.</summary>
    public static string? NomineeFieldFor(Workflow workflow, FormSchema schema, WorkflowTransition transition)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(transition);

        var userFields = schema.Sections
            .SelectMany(section => section.Fields)
            .Where(field => field.Type == FieldType.User)
            .Select(field => field.Key)
            .ToHashSet(StringComparer.Ordinal);

        if (userFields.Count == 0)
        {
            return null;
        }

        foreach (var next in workflow.TransitionsLeadingOn(transition.To))
        {
            var named = FieldArms(next.Actor).FirstOrDefault(userFields.Contains);
            if (named is not null)
            {
                return named;
            }
        }

        return null;
    }

    /// <summary>
    /// <see cref="NomineeFieldFor(Workflow, FormSchema, WorkflowTransition)" /> for the move <paramref name="transitionKey" />,
    /// taken from <paramref name="fromState" /> when given; null when the workflow declares no such move.
    /// </summary>
    public static string? NomineeFieldFor(Workflow workflow, FormSchema schema, string transitionKey, string? fromState = null)
    {
        ArgumentNullException.ThrowIfNull(workflow);

        var transition = workflow.Transitions.FirstOrDefault(candidate =>
            string.Equals(candidate.Key, transitionKey, StringComparison.Ordinal) &&
            (fromState is null || candidate.From.Contains(fromState, StringComparer.Ordinal)));

        return transition is null ? null : NomineeFieldFor(workflow, schema, transition);
    }

    /// <summary>The fields a rule's <c>field:</c> arms name, in declaration order.</summary>
    private static IEnumerable<string> FieldArms(ActorRule rule)
        => rule switch
        {
            FieldUserActorRule field => [field.Field],
            CombinedActorRule combined => combined.Rules.SelectMany(FieldArms),
            _ => []
        };
}
