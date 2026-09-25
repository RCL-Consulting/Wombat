using System.Security.Claims;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Infrastructure.Activities;

public sealed class WorkflowEvaluator : IWorkflowEvaluator
{
    public WorkflowEvaluationResult Evaluate(
        Workflow workflow,
        Activity activity,
        string transitionKey,
        ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(activity);
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentException.ThrowIfNullOrWhiteSpace(transitionKey);

        var transition = workflow.Transitions.SingleOrDefault(candidate =>
            string.Equals(candidate.Key, transitionKey, StringComparison.Ordinal) &&
            candidate.From.Contains(activity.CurrentState, StringComparer.Ordinal));

        if (transition is null)
        {
            return WorkflowEvaluationResult.Deny(NotAvailableReason(workflow, transitionKey, activity.CurrentState));
        }

        // The actor grammar itself lives in ActorRuleMatcher so that field-write authorization
        // (FieldPermissionEvaluator) evaluates 'field:', 'scope:' and combined rules identically. (T070)
        return ActorRuleMatcher.Matches(transition.Actor, activity, principal)
            ? WorkflowEvaluationResult.Allow()
            : WorkflowEvaluationResult.Deny("The current actor is not allowed to perform this transition.");
    }

    /// <summary>
    /// The refusal of a move the workflow does not offer from a state: "Complete is not available while the activity is
    /// Draft." The move is named as its button is, and the state by its label (T189).
    /// </summary>
    /// <remarks>
    /// The one wording, shared with <c>ActivityService</c>'s own lookups. The page never offers such a move, so a person
    /// meets this only after the activity has moved on under an open page, or from a hand-made request.
    /// </remarks>
    internal static string NotAvailableReason(Workflow workflow, string transitionKey, string stateKey)
        => $"{WorkflowTransition.LabelFor(transitionKey)} is not available while the activity is {workflow.StateLabel(stateKey)}.";
}
