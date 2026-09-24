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
    /// Reads a field as the actor grammar reads it — a JSON string, or empty for anything else.
    /// </summary>
    public static string ReadUserFieldValue(string dataJson, string fieldName)
        => GetStringFieldValue(dataJson, fieldName);

    /// <summary>
    /// Resolves a <c>scope:</c> token against the ACTIVITY's own stamped scope. (T101)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Until T101 this resolved against <c>activity.ActivityType.Scope</c>/<c>ScopeId</c> — the
    /// scope that decides which programme may OFFER the tool, not the one the assessment is ABOUT.
    /// The two diverge whenever a trainee files a tool published by another programme, and the
    /// consequence was backwards: <c>role:SpecialityAdmin+scope:speciality</c> matched an admin of
    /// the tool's speciality, who has no relationship to the trainee, and did not match the admin
    /// who actually oversees them. Oversight follows the trainee, so the subject's stamped scope is
    /// the right side of the comparison.
    /// </para>
    /// <para>
    /// A null stamp matches nothing. Activities created before their scope was stamped, or whose
    /// subject has no trainee profile, therefore satisfy no <c>scope:</c> rule — the same
    /// fail-closed direction the rest of the grammar takes (<c>_ =&gt; false</c> below).
    /// </para>
    /// <para>
    /// <c>global</c> is the one token that is not about the subject: it asks whether the tool itself
    /// is unrestricted, so it still reads the activity type.
    /// </para>
    /// </remarks>
    private static bool IsInActivityScope(string scope, Activity activity, ClaimsPrincipal principal)
    {
        return scope switch
        {
            "global" => activity.ActivityType is not null && activity.ActivityType.Scope == ActivityScope.Global,
            "institution" => activity.InstitutionId.HasValue &&
                             activity.InstitutionId == principal.GetInstitutionId(),
            // The speciality arms conjoin the institution for the same reason the read gate does: a
            // Speciality is College-owned and therefore NATIONAL, so the speciality claim alone would
            // make `role:SpecialityAdmin+scope:speciality` a country-wide grant. It must also stay in
            // step with ActivityService.IsScopedOverseerOf (TraineeScopeResolver.IsOverseenBy over the
            // activity's stamps, T185) — read has to remain a superset of act, and
            // narrowing one without the other produces a button that 404s.
            // NOTE the InstitutionId.HasValue guard on each arm: both sides are int?, and `null ==
            // null` is true, so without it an UNSTAMPED activity would match any principal who also
            // holds no institution claim.
            "speciality" => activity.SpecialityId.HasValue &&
                            activity.InstitutionId.HasValue &&
                            activity.InstitutionId == principal.GetInstitutionId() &&
                            principal.IsInSpeciality(activity.SpecialityId.Value),
            "subspeciality" or "sub_speciality" => activity.SubSpecialityId.HasValue &&
                                                   activity.InstitutionId.HasValue &&
                                                   activity.InstitutionId == principal.GetInstitutionId() &&
                                                   principal.IsInSubSpeciality(activity.SubSpecialityId.Value),
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
