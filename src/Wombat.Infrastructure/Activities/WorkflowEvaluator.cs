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
            return WorkflowEvaluationResult.Deny(
                $"Transition '{transitionKey}' is not available from state '{activity.CurrentState}'.");
        }

        // The actor grammar itself lives in ActorRuleMatcher so that field-write authorization
        // (FieldPermissionEvaluator) evaluates 'field:', 'scope:' and combined rules identically. (T070)
        return ActorRuleMatcher.Matches(transition.Actor, activity, principal)
            ? WorkflowEvaluationResult.Allow()
            : WorkflowEvaluationResult.Deny("The current actor is not allowed to perform this transition.");
    }
}
