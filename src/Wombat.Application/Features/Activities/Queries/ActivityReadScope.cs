using System.Security.Claims;
using Wombat.Application.Common.Extensions;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Activities.Queries;

/// <summary>
/// The read boundary for every query that returns activity rows in bulk, as a SQL-translatable
/// predicate over the scope stamps T101 put on <see cref="Activity" />.
/// </summary>
/// <remarks>
/// <para>
/// This is the list-shaped half of <c>ActivityService.IsReadableBy</c>, which guards a single
/// activity after it has been loaded with its transitions and its pinned DSL. A list cannot afford
/// that shape: honouring the creator, the past-actor and the declared-actor-rule arms would mean
/// materialising every activity in the database to decide which ones the caller may see. So this
/// filter is the part of the rule that is a column comparison — you are the subject, or you oversee
/// the programme the activity is stamped to — and it is deliberately NARROWER than the single-read
/// rule. Narrower is the safe direction: someone withheld from a list can still open the activity
/// by id, where the full rule runs.
/// </para>
/// <para>
/// A null stamp matches nobody but the subject, for the reason recorded on
/// <see cref="Activity.InstitutionId" />.
/// </para>
/// <para>
/// The defect this closes: these queries used to take no principal at all and filter on a
/// caller-supplied user id, so naming any other trainee returned their activity list. If you add a
/// query that reads activities for anyone but the signed-in user, put this filter on it rather than
/// re-deriving the rule here — the point of one implementation is that it cannot drift from the
/// single-activity gate. (T101)
/// </para>
/// </remarks>
public static class ActivityReadScope
{
    public static IQueryable<Activity> WhereReadableBy(this IQueryable<Activity> activities, ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(activities);
        ArgumentNullException.ThrowIfNull(principal);

        if (principal.IsAdministrator())
        {
            return activities;
        }

        var callerUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var hasCallerUserId = !string.IsNullOrEmpty(callerUserId);

        // Oversight is role-gated, not claim-gated: every user carries an institution claim,
        // trainees included, so the claim alone must never widen a read.
        var overseesInstitution =
            principal.IsInstitutionalAdmin() ||
            principal.IsInRole(WombatRoles.Coordinator) ||
            principal.IsInRole(WombatRoles.CommitteeMember);
        var overseenInstitutionId = overseesInstitution ? principal.GetInstitutionId() : null;
        var hasOverseenInstitution = overseenInstitutionId.HasValue;

        // The speciality arms need the caller's institution as well, because a Speciality is
        // College-owned and therefore a NATIONAL id: matching on it alone would let one hospital's
        // SpecialityAdmin list every paediatric trainee in the country. The oversight arms below are the
        // SQL form of TraineeScopeResolver.IsOverseenBy over the activity's stamps, the rule the
        // single-activity gate (ActivityService.IsScopedOverseerOf) calls since T185. They are written
        // apart because a list cannot call it per row; OverseerRuleParityTests holds them to it.
        var callerInstitutionId = principal.GetInstitutionId();
        var hasCallerInstitution = callerInstitutionId.HasValue;

        int[] overseenSpecialityIds = principal.IsInRole(WombatRoles.SpecialityAdmin)
            ? principal.GetSpecialityIds().ToArray()
            : [];

        int[] overseenSubSpecialityIds = principal.IsInRole(WombatRoles.SubSpecialityAdmin)
            ? principal.GetSubSpecialityIds().ToArray()
            : [];

        return activities.Where(activity =>
            (hasCallerUserId && activity.SubjectUserId == callerUserId) ||
            (hasOverseenInstitution && activity.InstitutionId == overseenInstitutionId) ||
            (hasCallerInstitution && activity.InstitutionId == callerInstitutionId &&
                activity.SpecialityId != null && overseenSpecialityIds.Contains(activity.SpecialityId.Value)) ||
            (hasCallerInstitution && activity.InstitutionId == callerInstitutionId &&
                activity.SubSpecialityId != null && overseenSubSpecialityIds.Contains(activity.SubSpecialityId.Value)));
    }
}
