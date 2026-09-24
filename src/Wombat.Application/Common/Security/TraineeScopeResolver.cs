using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Common.Security;

/// <summary>
/// Where a trainee trains: the institution on their preferred <see cref="TraineeProfile" />, and the speciality and
/// sub-speciality of that profile's curriculum. Either of the last two is null when its row cannot be reached.
/// </summary>
public sealed record TraineeScope(int InstitutionId, int? SpecialityId, int? SubSpecialityId);

/// <summary>
/// The one answer to "where does this trainee train, and may this caller read about them?" (T101, T113)
/// </summary>
/// <remarks>
/// <para>
/// There were four copies of the first half: <c>SubjectScopeResolver</c> (the stamp every activity carries),
/// <c>ExportPortfolio</c>, <c>PortfolioPdfService</c> and <c>MsfCampaignRules</c>. The first three broke ties by
/// <c>IsActive</c> then <c>Id</c>; the MSF copy broke them by <c>ProgrammeStartDate</c>, so a trainee with no current
/// profile and two past ones could resolve to one institution for their activities and to another for their feedback
/// campaigns. They
/// all read this now. <c>SubjectScopeResolver</c> keeps its identity-row fallback for a subject with no profile, layered
/// on top of this in Infrastructure; nothing here guesses from identity, because every row these callers guard is about
/// a trainee by definition.
/// </para>
/// <para>
/// This is the trainee's organisational home for AUTHORIZATION. Which programme their curriculum credit is read against
/// is a different question with its own tie-break (<c>CreditTargetResolver.PickProfileAsync</c>, the quota reader);
/// the two agree for a trainee with one profile, which is every trainee today.
/// </para>
/// </remarks>
public static class TraineeScopeResolver
{
    /// <summary>
    /// Each trainee's preferred profile, one row per trainee: the active one, else the most recent. "Most recent" is
    /// the highest id.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The database holds at most one ACTIVE profile per trainee (a unique index on <c>UserId</c> filtered on
    /// <c>IsActive</c>), so the id only decides between past profiles: a trainee who has completed or left more than
    /// one programme and has no current one. That is exactly where the MSF copy disagreed, choosing the latest
    /// programme START rather than the latest profile.
    /// </para>
    /// <para>
    /// Written as "no profile of the same trainee ranks above this one" rather than as an ordering, so that the same
    /// definition serves a single lookup and a set-based filter: a list of campaigns can be narrowed to the trainees who
    /// train at one institution in SQL, without materialising every campaign to resolve its subject one by one. The
    /// rank is the pair (<c>IsActive</c>, <c>Id</c>), compared lexicographically, both descending. Ids are unique, so
    /// exactly one profile per trainee survives.
    /// </para>
    /// <para>
    /// This is the tie-break the activity scope stamp has used since T101. Change it here or nowhere.
    /// </para>
    /// </remarks>
    public static IQueryable<TraineeProfile> PreferredProfiles(IApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        var profiles = dbContext.Set<TraineeProfile>();

        return profiles.Where(profile => !profiles.Any(other =>
            other.UserId == profile.UserId &&
            ((other.IsActive && !profile.IsActive) ||
             (other.IsActive == profile.IsActive && other.Id > profile.Id))));
    }

    /// <summary>
    /// Where this trainee trains, or null when they hold no profile at all.
    /// </summary>
    /// <remarks>
    /// Resolved one level at a time, NOT as a single join through TraineeProfile -> Curriculum -> SubSpeciality. Those
    /// navigations are required, so one query would be an INNER join: a curriculum row that has gone missing would take
    /// the institution down with it, even though the institution sits on the profile itself. Each level degrades on its
    /// own instead, and the institution survives the other two failing.
    /// </remarks>
    public static async Task<TraineeScope?> ResolveAsync(
        IApplicationDbContext dbContext,
        string traineeUserId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        if (string.IsNullOrWhiteSpace(traineeUserId))
        {
            return null;
        }

        var profile = await PreferredProfiles(dbContext)
            .Where(entity => entity.UserId == traineeUserId)
            .Select(entity => new { entity.InstitutionId, entity.CurriculumId })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return null;
        }

        var subSpecialityId = await dbContext.Set<Curriculum>()
            .Where(entity => entity.Id == profile.CurriculumId)
            .Select(entity => (int?)entity.SubSpecialityId)
            .FirstOrDefaultAsync(cancellationToken);

        var specialityId = subSpecialityId is null
            ? null
            : await dbContext.Set<SubSpeciality>()
                .Where(entity => entity.Id == subSpecialityId.Value)
                .Select(entity => (int?)entity.SpecialityId)
                .FirstOrDefaultAsync(cancellationToken);

        return new TraineeScope(profile.InstitutionId, specialityId, subSpecialityId);
    }

    /// <summary>
    /// Whether this caller may read what the programme holds about this trainee: their curriculum progress, their
    /// committee reviews, their entrustment decisions, their released feedback. (T113)
    /// </summary>
    /// <remarks>
    /// <para>
    /// T101's read ladder, lifted to a trainee rather than an activity: the trainee themselves, a global Administrator,
    /// or someone who oversees the programme the trainee is on (<see cref="IsOverseenBy" />). A trainee with no profile
    /// has no organisational home, so no scoped role can be held over them; only the first two rungs reach them, and an
    /// id that names nobody lands in the same place.
    /// </para>
    /// <para>
    /// A query refused here returns what it returns for a trainee with nothing on record, an empty list or null, never
    /// a refusal. The id is one the caller typed; a refusal that differed from "nothing" would confirm that someone by
    /// that id trains somewhere.
    /// </para>
    /// </remarks>
    public static async Task<bool> MayReadAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        string traineeUserId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (principal.IsAdministrator())
        {
            return true;
        }

        var callerUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(callerUserId) &&
            string.Equals(callerUserId, traineeUserId, StringComparison.Ordinal))
        {
            return true;
        }

        // Every user carries an institution claim, trainees included, so nobody without an oversight role is worth a
        // lookup: the answer for them is no whoever the trainee is.
        if (!HoldsOversightRole(principal))
        {
            return false;
        }

        var scope = await ResolveAsync(dbContext, traineeUserId, cancellationToken);
        return scope is not null && IsOverseenBy(scope, principal);
    }

    /// <summary>
    /// Programme oversight: the roles that supervise a trainee, each at the level of the tree it is scoped to. Mirrors
    /// <c>ActivityService.IsScopedOverseerOf</c>, which answers the same question from an activity's stamped scope, and
    /// <c>ActivityReadScope.WhereReadableBy</c>, its list-shaped half. The three must agree. (T101)
    /// </summary>
    /// <remarks>
    /// EVERY arm requires the institution, the speciality ones included: a Speciality is College-owned and therefore a
    /// NATIONAL id, so <c>IsInSpeciality</c> on its own would make one hospital's SpecialityAdmin an overseer of every
    /// paediatric trainee in the country.
    /// </remarks>
    public static bool IsOverseenBy(TraineeScope scope, ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(principal);

        if (principal.GetInstitutionId() != scope.InstitutionId)
        {
            return false;
        }

        if (principal.IsInstitutionalAdmin() ||
            principal.IsInRole(WombatRoles.Coordinator) ||
            principal.IsInRole(WombatRoles.CommitteeMember))
        {
            return true;
        }

        if (scope.SpecialityId is int specialityId &&
            principal.IsInRole(WombatRoles.SpecialityAdmin) &&
            principal.IsInSpeciality(specialityId))
        {
            return true;
        }

        return scope.SubSpecialityId is int subSpecialityId &&
               principal.IsInRole(WombatRoles.SubSpecialityAdmin) &&
               principal.IsInSubSpeciality(subSpecialityId);
    }

    private static bool HoldsOversightRole(ClaimsPrincipal principal)
        => principal.IsInstitutionalAdmin() ||
           principal.IsInRole(WombatRoles.Coordinator) ||
           principal.IsInRole(WombatRoles.CommitteeMember) ||
           principal.IsInRole(WombatRoles.SpecialityAdmin) ||
           principal.IsInRole(WombatRoles.SubSpecialityAdmin);
}
