using System.Security.Claims;
using System.Text.Json;
using Wombat.Application.Common.Extensions;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Infrastructure.Activities;

/// <summary>
/// Evaluates an <see cref="ActorRule"/> against an activity and the current principal.
/// </summary>
/// <remarks>
/// Lifted verbatim out of <see cref="WorkflowEvaluator"/> by T070 so that transition
/// authorization (<see cref="WorkflowEvaluator"/>) and field-write authorization
/// (<see cref="FieldPermissionEvaluator"/>) share one implementation of the actor grammar.
/// If the two ever diverge, a button the UI offers stops matching what the server allows.
///
/// This lives in Infrastructure rather than Domain because <c>scope:</c> rules need
/// <c>GetInstitutionId</c> / <c>IsInSpeciality</c> / <c>IsInSubSpeciality</c> from
/// <see cref="Wombat.Application.Common.Extensions.ClaimsPrincipalExtensions"/>, and Domain
/// may not depend on Application.
/// </remarks>
internal static class ActorRuleMatcher
{
    public static bool Matches(ActorRule rule, Activity activity, ClaimsPrincipal principal)
    {
        return rule switch
        {
            SubjectUserActorRule => HasNameIdentifier(principal, activity.SubjectUserId),
            CreatorUserActorRule => HasNameIdentifier(principal, activity.CreatedByUserId),
            NamedRoleActorRule namedRole => principal.IsInRole(namedRole.Role),
            ScopeMatchActorRule scopeMatch => IsInActivityScope(scopeMatch.Scope, activity, principal),
            FieldUserActorRule fieldUser => HasNameIdentifier(principal, GetStringFieldValue(activity.DataJson, fieldUser.Field)),
            CombinedActorRule combined when combined.CombinationKind == ActorRuleCombinationKind.All
                => combined.Rules.All(child => Matches(child, activity, principal)),
            CombinedActorRule combined when combined.CombinationKind == ActorRuleCombinationKind.Any
                => combined.Rules.Any(child => Matches(child, activity, principal)),
            _ => false
        };
    }

    /// <summary>
    /// Collects the names of every field a <c>field:</c> rule reads, anywhere in the rule tree.
    /// </summary>
    /// <remarks>
    /// These fields decide who may act on an activity, so their values are authorization input
    /// rather than ordinary form data. <see cref="ActivityService"/> uses this to refuse a value
    /// that would name the activity's own subject as its assessor.
    /// </remarks>
    public static void CollectUserFieldNames(ActorRule? rule, HashSet<string> into)
    {
        ArgumentNullException.ThrowIfNull(into);

        switch (rule)
        {
            case null:
                return;
            case FieldUserActorRule fieldUser:
                into.Add(fieldUser.Field);
                return;
            case CombinedActorRule combined:
                foreach (var child in combined.Rules)
                {
                    CollectUserFieldNames(child, into);
                }

                return;
        }
    }

    /// <summary>
    /// Reads a field as the actor grammar reads it — a JSON string, or empty for anything else.
    /// </summary>
    public static string ReadUserFieldValue(string dataJson, string fieldName)
        => GetStringFieldValue(dataJson, fieldName);

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

    private static string GetStringFieldValue(string dataJson, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(dataJson) || string.IsNullOrWhiteSpace(fieldName))
        {
            return string.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(dataJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object ||
                !document.RootElement.TryGetProperty(fieldName, out var value) ||
                value.ValueKind != JsonValueKind.String)
            {
                return string.Empty;
            }

            return value.GetString() ?? string.Empty;
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }
}
