using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Application.Features.Activities.Queries.ListActivityTypes;

/// <summary>
/// Which group of the Log page's instrument picker a type belongs to, and whether it credits anything (T342, B8, Q1).
/// </summary>
/// <remarks>
/// <para>
/// Read from the type's PUBLISHED schema, workflow and credit rules, the version a new activity is pinned to. In order:
/// <list type="number">
///   <item><see cref="ActivityTypeShape.Rated" /> when the schema has a <c>scale</c> field: the seven rated KGK
///   instruments.</item>
///   <item><see cref="ActivityTypeShape.LoggedByYou" /> when nobody else acts on it: the initial state is itself
///   terminal (a type born terminal, whose create is the whole record), or a move out of the initial state that the
///   author may take (an actor rule with a <c>subject</c> or <c>creator</c> arm) goes straight to a state marked
///   <c>terminal</c>. Marked terminal, not merely a dead end: every seeded type cancels into a dead end
///   (<c>cancelled</c> is deliberately non-terminal), and "a move by the author into a dead end" would put every one of
///   them here.</item>
///   <item><see cref="ActivityTypeShape.DiscussedOrReviewed" /> otherwise: someone else takes the move that finishes it.</item>
/// </list>
/// </para>
/// <para>
/// "Credits nothing" is an empty <c>counts_for</c> (<see cref="EncounterDatePolicy.CanCredit" />, the rule the date
/// gate uses). A published version that does not parse (it parsed when it was published, so this is a corrupt row)
/// reads as <see cref="ActivityTypeShape.DiscussedOrReviewed" /> crediting nothing: a menu label is no place to fail.
/// </para>
/// </remarks>
public static class ActivityTypeShapes
{
    public static (ActivityTypeShape Shape, bool CreditsNothing) Of(
        string? schemaJson,
        string? workflowJson,
        string? creditRulesJson)
    {
        bool creditsNothing;
        try
        {
            creditsNothing = !EncounterDatePolicy.CanCredit(creditRulesJson);
        }
        catch (Exception)
        {
            creditsNothing = true;
        }

        FormSchema schema;
        Workflow workflow;
        try
        {
            if (string.IsNullOrWhiteSpace(schemaJson) || string.IsNullOrWhiteSpace(workflowJson))
            {
                return (ActivityTypeShape.DiscussedOrReviewed, creditsNothing);
            }

            schema = FormSchemaParser.Parse(schemaJson);
            workflow = WorkflowParser.Parse(workflowJson);
        }
        catch (Exception)
        {
            return (ActivityTypeShape.DiscussedOrReviewed, creditsNothing);
        }

        return (Of(schema, workflow), creditsNothing);
    }

    /// <summary>The group, from a parsed schema and workflow. See the remarks on the type.</summary>
    public static ActivityTypeShape Of(FormSchema schema, Workflow workflow)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(workflow);

        if (schema.Sections.SelectMany(section => section.Fields).Any(field => field.Type == FieldType.Scale))
        {
            return ActivityTypeShape.Rated;
        }

        var terminal = workflow.States
            .Where(state => state.Terminal)
            .Select(state => state.Key)
            .ToHashSet(StringComparer.Ordinal);

        var loggedByYou = terminal.Contains(workflow.InitialState) ||
                          workflow.Transitions.Any(transition =>
                              transition.From.Contains(workflow.InitialState, StringComparer.Ordinal) &&
                              terminal.Contains(transition.To) &&
                              TheAuthorMayTake(transition.Actor));

        return loggedByYou ? ActivityTypeShape.LoggedByYou : ActivityTypeShape.DiscussedOrReviewed;
    }

    /// <summary>
    /// Whether the rule has a <c>subject</c> or <c>creator</c> arm: in an <c>Any</c>, any arm; in an <c>All</c>, any
    /// conjunct, the others being further conditions on the same person.
    /// </summary>
    private static bool TheAuthorMayTake(ActorRule rule)
        => rule switch
        {
            SubjectUserActorRule or CreatorUserActorRule => true,
            CombinedActorRule combined => combined.Rules.Any(TheAuthorMayTake),
            _ => false
        };
}
