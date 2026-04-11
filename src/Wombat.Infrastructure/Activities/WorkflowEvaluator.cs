using System.Security.Claims;
using Wombat.Application.Common.Extensions;
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

        return EvaluateRule(transition.Actor, activity, principal)
            ? WorkflowEvaluationResult.Allow()
            : WorkflowEvaluationResult.Deny("The current actor is not allowed to perform this transition.");
    }

    private static bool EvaluateRule(ActorRule rule, Activity activity, ClaimsPrincipal principal)
    {
        return rule switch
        {
            SubjectUserActorRule => HasNameIdentifier(principal, activity.SubjectUserId),
            CreatorUserActorRule => HasNameIdentifier(principal, activity.CreatedByUserId),
            NamedRoleActorRule namedRole => principal.IsInRole(namedRole.Role),
            ScopeMatchActorRule scopeMatch => IsInActivityScope(scopeMatch.Scope, activity, principal),
            CombinedActorRule combined when combined.CombinationKind == ActorRuleCombinationKind.All
                => combined.Rules.All(child => EvaluateRule(child, activity, principal)),
            CombinedActorRule combined when combined.CombinationKind == ActorRuleCombinationKind.Any
                => combined.Rules.Any(child => EvaluateRule(child, activity, principal)),
            _ => false
        };
    }

    private static bool IsInActivityScope(string scope, Activity activity, ClaimsPrincipal principal)
    {
        if (activity.ActivityType is null)
        {
            return false;
        }

        return scope switch
        {
            "global" => activity.ActivityType.Scope == ActivityScope.Global,
            "institution" => activity.ActivityType.Scope == ActivityScope.Institution &&
                             activity.ActivityType.ScopeId == principal.GetInstitutionId(),
            "speciality" => activity.ActivityType.Scope == ActivityScope.Speciality &&
                            activity.ActivityType.ScopeId.HasValue &&
                            principal.IsInSpeciality(activity.ActivityType.ScopeId.Value),
            "subspeciality" or "sub_speciality" => activity.ActivityType.Scope == ActivityScope.SubSpeciality &&
                                                   activity.ActivityType.ScopeId.HasValue &&
                                                   principal.IsInSubSpeciality(activity.ActivityType.ScopeId.Value),
            _ => false
        };
    }

    private static bool HasNameIdentifier(ClaimsPrincipal principal, string userId)
        => string.Equals(
            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            userId,
            StringComparison.Ordinal);
}
