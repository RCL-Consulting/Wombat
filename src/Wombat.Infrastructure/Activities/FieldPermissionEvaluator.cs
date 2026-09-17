using System.Collections.Frozen;
using System.Security.Claims;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Infrastructure.Activities;

/// <inheritdoc cref="IFieldPermissionEvaluator" />
public sealed class FieldPermissionEvaluator : IFieldPermissionEvaluator
{
    /// <summary>
    /// The rule applied when neither the state, the field nor its section declares
    /// <c>editable_by</c>. It reproduces the pre-T070 <c>CanEditDraft</c> test
    /// (subject or creator) exactly, so nothing already published changes behaviour.
    /// </summary>
    private static readonly ActorRule DefaultEditableBy = ActorRuleParser.Parse("subject|creator");

    public IReadOnlySet<string> GetWritableFieldKeys(
        FormSchema schema,
        Workflow workflow,
        Activity activity,
        ClaimsPrincipal principal,
        bool ignoreStateGate = false)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(activity);
        ArgumentNullException.ThrowIfNull(principal);

        if (!ignoreStateGate && !IsStateWritable(workflow, activity, principal))
        {
            return FrozenSet<string>.Empty;
        }

        var writableFieldKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var section in schema.Sections)
        {
            foreach (var field in section.Fields)
            {
                // Field beats section beats the subject|creator default. A section rule is the
                // ordinary way to declare ownership; a field rule is the escape hatch for one
                // field that does not follow its section.
                var rule = field.EditableBy ?? section.EditableBy ?? DefaultEditableBy;
                if (ActorRuleMatcher.Matches(rule, activity, principal))
                {
                    writableFieldKeys.Add(field.Key);
                }
            }
        }

        return writableFieldKeys;
    }

    private static bool IsStateWritable(Workflow workflow, Activity activity, ClaimsPrincipal principal)
    {
        var currentState = workflow.States.FirstOrDefault(state =>
            string.Equals(state.Key, activity.CurrentState, StringComparison.Ordinal));

        // An activity sitting in a state the pinned workflow no longer declares is not writable by
        // anyone. Denying is the safe reading: the alternative would hand out write access under a
        // state whose rule cannot be read.
        if (currentState is null || currentState.Terminal)
        {
            return false;
        }

        // A dead end is as final as a terminal state, whatever the flag says. The CPSA workflows
        // declare `declined` and `cancelled` non-terminal deliberately — marking them terminal
        // would fire CreditApplier on a refused or withdrawn request — but an activity that cannot
        // leave the state it is in is not editable by anyone either. Without this, the subject is
        // handed a live form on a dead activity with no action able to consume it.
        if (!HasOutgoingTransition(workflow, currentState.Key))
        {
            return false;
        }

        return ActorRuleMatcher.Matches(currentState.EditableBy ?? DefaultEditableBy, activity, principal);
    }

    private static bool HasOutgoingTransition(Workflow workflow, string stateKey)
        => workflow.Transitions.Any(transition =>
            transition.From.Contains(stateKey, StringComparer.Ordinal));
}
