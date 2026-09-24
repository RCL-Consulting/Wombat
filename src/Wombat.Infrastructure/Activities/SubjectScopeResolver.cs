using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Infrastructure.Identity;

namespace Wombat.Infrastructure.Activities;

/// <summary>
/// The one answer to "where does this subject train?" (T101), shared by the stamp <see cref="ActivityService" /> writes
/// at creation and the nominee picker on the create page (T102), so the picker cannot offer the people of one profile's
/// institution while the server judges against another's.
/// </summary>
internal static class SubjectScopeResolver
{
    /// <summary>
    /// Where the subject trains, read once at creation and stamped onto the activity. (T101)
    /// </summary>
    /// <remarks>
    /// <para>
    /// The profile half is <see cref="TraineeScopeResolver.ResolveAsync" /> in Application, the one resolver every
    /// trainee-scoped read and the MSF rules also use (T113): the active profile, else the most recent, each level of
    /// profile -> curriculum -> sub-speciality -> speciality degrading on its own. A trainee who has graduated or been
    /// withdrawn keeps their most recent profile, so activities logged afterwards still carry a scope.
    /// </para>
    /// <para>
    /// What is added here is the identity-row fallback for a subject with no profile at all, which only an activity
    /// needs: an activity can be about someone who is not (yet) an admitted trainee. Null throughout for a subject with
    /// neither, which withholds scoped oversight rather than granting it.
    /// </para>
    /// </remarks>
    public static async Task<(int? InstitutionId, int? SpecialityId, int? SubSpecialityId)> ResolveAsync(
        IApplicationDbContext dbContext,
        string subjectUserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(subjectUserId))
        {
            return (null, null, null);
        }

        var scope = await TraineeScopeResolver.ResolveAsync(dbContext, subjectUserId, cancellationToken);

        return scope is null
            ? await ResolveScopeFromIdentityAsync(dbContext, subjectUserId, cancellationToken)
            : (scope.InstitutionId, scope.SpecialityId, scope.SubSpecialityId);
    }

    /// <summary>
    /// The fallback for a subject who is not an admitted trainee: their own identity record. (T101)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Not every activity is about a trainee. An invited user is given an institution and speciality
    /// scopes by <c>InvitedUserProvisioner</c> at acceptance, while a <c>TraineeProfile</c> is created
    /// only later by <c>AdmitTrainee</c> — and nothing stops them filing a reflective note in between.
    /// </para>
    /// <para>
    /// Without this, such an activity was stamped with nothing, and the seeded reflective-note family
    /// offers only <c>role:SpecialityAdmin+scope:speciality</c> out of <c>submitted</c>. A null stamp
    /// matches no <c>scope:</c> rule for anybody, and neither <see cref="WorkflowEvaluator" /> nor
    /// <c>ActivityService.TransitionAsync</c> has an Administrator bypass — so the row was frozen in <c>submitted</c>
    /// for ever, unreadable by the admin who should have acted on it. Reading the same facts the login
    /// claims are issued from is not a guess; it is the same answer one step earlier.
    /// </para>
    /// <para>
    /// A user holding several speciality scopes yields null rather than an arbitrary pick: the column
    /// holds one id, and choosing between them would be inventing an answer. That leaves the residual
    /// frozen-row case for a subject with no institution at all — see T116.
    /// </para>
    /// </remarks>
    private static async Task<(int? InstitutionId, int? SpecialityId, int? SubSpecialityId)> ResolveScopeFromIdentityAsync(
        IApplicationDbContext dbContext,
        string subjectUserId,
        CancellationToken cancellationToken)
    {
        var institutionId = await dbContext.Set<WombatIdentityUser>()
            .Where(entity => entity.Id == subjectUserId)
            .Select(entity => entity.InstitutionId)
            .FirstOrDefaultAsync(cancellationToken);

        var specialityIds = await dbContext.Set<WombatIdentityUserSpecialityScope>()
            .Where(entity => entity.UserId == subjectUserId)
            .Select(entity => entity.SpecialityId)
            .Distinct()
            .Take(2)
            .ToListAsync(cancellationToken);

        var subSpecialityIds = await dbContext.Set<WombatIdentityUserSubSpecialityScope>()
            .Where(entity => entity.UserId == subjectUserId)
            .Select(entity => entity.SubSpecialityId)
            .Distinct()
            .Take(2)
            .ToListAsync(cancellationToken);

        return (
            institutionId,
            specialityIds.Count == 1 ? specialityIds[0] : null,
            subSpecialityIds.Count == 1 ? subSpecialityIds[0] : null);
    }
}
