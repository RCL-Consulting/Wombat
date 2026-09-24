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

        var scopes = await ResolveManyAsync(dbContext, [traineeUserId], cancellationToken);
        return scopes.GetValueOrDefault(traineeUserId);
    }

    /// <summary>
    /// Where each of these trainees trains, keyed by user id; a trainee who holds no profile is absent. The same answer
    /// <see cref="ResolveAsync" /> gives for each one, which is one call of this. (T183)
    /// </summary>
    /// <remarks>
    /// For a list that has to judge every row it shows by <see cref="IsAdministeredBy" />: the committee's entrustment
    /// decisions, say, where one query per trainee would be three round trips a row. Three queries at most, however
    /// many trainees are asked about.
    /// </remarks>
    public static async Task<IReadOnlyDictionary<string, TraineeScope>> ResolveManyAsync(
        IApplicationDbContext dbContext,
        IEnumerable<string> traineeUserIds,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(traineeUserIds);

        var userIds = traineeUserIds
            .Where(userId => !string.IsNullOrWhiteSpace(userId))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (userIds.Length == 0)
        {
            return new Dictionary<string, TraineeScope>(StringComparer.Ordinal);
        }

        return await ResolveProfilesAsync(
            dbContext,
            PreferredProfiles(dbContext).Where(entity => userIds.Contains(entity.UserId)),
            cancellationToken);
    }

    /// <summary>
    /// Where each trainee trains, for every trainee whose preferred profile is at this institution, or for every
    /// trainee with a profile when the institution is null. The set form of <see cref="ResolveAsync" />. (T182)
    /// </summary>
    /// <remarks>
    /// For a picker that must offer exactly the trainees a single-trainee check would accept: the committee scheduling
    /// page lists the trainees its handler would take, and the handler asks <see cref="ResolveAsync" /> about the one
    /// it is given. The two see the same scope for the same trainee because both read <see cref="PreferredProfiles" />
    /// through the one assembly, <see cref="ResolveProfilesAsync" />. Three queries at most, whatever the number of
    /// trainees.
    /// </remarks>
    public static async Task<IReadOnlyDictionary<string, TraineeScope>> ResolveAllAsync(
        IApplicationDbContext dbContext,
        int? institutionId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        var preferred = PreferredProfiles(dbContext);
        if (institutionId is int onlyInstitutionId)
        {
            preferred = preferred.Where(entity => entity.InstitutionId == onlyInstitutionId);
        }

        return await ResolveProfilesAsync(dbContext, preferred, cancellationToken);
    }

    /// <summary>
    /// The scope of each of these preferred profiles, keyed by user id: the one assembly every resolve reads, one
    /// level at a time, as <see cref="ResolveAsync" /> explains. A curriculum or sub-speciality that cannot be reached
    /// nulls that level for the trainees that name it and nothing else; the institution survives both.
    /// </summary>
    private static async Task<IReadOnlyDictionary<string, TraineeScope>> ResolveProfilesAsync(
        IApplicationDbContext dbContext,
        IQueryable<TraineeProfile> preferredProfiles,
        CancellationToken cancellationToken)
    {
        var profiles = await preferredProfiles
            .Select(entity => new { entity.UserId, entity.InstitutionId, entity.CurriculumId })
            .ToListAsync(cancellationToken);

        var curriculumIds = profiles.Select(profile => profile.CurriculumId).Distinct().ToArray();
        var subSpecialityByCurriculum = curriculumIds.Length == 0
            ? new Dictionary<int, int>()
            : await dbContext.Set<Curriculum>()
                .Where(entity => curriculumIds.Contains(entity.Id))
                .Select(entity => new { entity.Id, entity.SubSpecialityId })
                .ToDictionaryAsync(entity => entity.Id, entity => entity.SubSpecialityId, cancellationToken);

        var subSpecialityIds = subSpecialityByCurriculum.Values.Distinct().ToArray();
        var specialityBySubSpeciality = subSpecialityIds.Length == 0
            ? new Dictionary<int, int>()
            : await dbContext.Set<SubSpeciality>()
                .Where(entity => subSpecialityIds.Contains(entity.Id))
                .Select(entity => new { entity.Id, entity.SpecialityId })
                .ToDictionaryAsync(entity => entity.Id, entity => entity.SpecialityId, cancellationToken);

        var scopes = new Dictionary<string, TraineeScope>(StringComparer.Ordinal);
        foreach (var profile in profiles)
        {
            int? subSpecialityId = subSpecialityByCurriculum.TryGetValue(profile.CurriculumId, out var found)
                ? found
                : null;

            int? specialityId = subSpecialityId is int knownSubSpeciality &&
                                specialityBySubSpeciality.TryGetValue(knownSubSpeciality, out var speciality)
                ? speciality
                : null;

            scopes[profile.UserId] = new TraineeScope(profile.InstitutionId, specialityId, subSpecialityId);
        }

        return scopes;
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

    // ─── Who stands over a trainee ──────────────────────────────────────────
    //
    // Three nested predicates, each the one before plus one institution-wide role:
    //     IsAdministeredBy  ⊂  IsAdministeredOrCoordinatedBy  ⊂  IsOverseenBy
    // EVERY arm of each requires the trainee's institution, the speciality ones included: a Speciality is College-owned
    // and therefore a NATIONAL id, so IsInSpeciality on its own would reach every such trainee in the country.
    // Ask the narrowest predicate that grants the right in hand, never "holds role X" and a wider predicate separately:
    // users hold several roles, and one role's reach would stand in for another's scope. A SpecialityAdmin for surgery
    // who also sits on the committee oversees every trainee at the hospital, but administers, and schedules, only the
    // surgical ones. (T101, T113, T182, T183)

    /// <summary>
    /// Administration: an InstitutionalAdmin at the trainee's institution, or a Speciality/SubSpecialityAdmin there of
    /// the trainee's own speciality/sub-speciality. Asked by what only an administrator may do: revoking, say. (T183)
    /// </summary>
    public static bool IsAdministeredBy(TraineeScope scope, ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(principal);

        if (principal.GetInstitutionId() != scope.InstitutionId)
        {
            return false;
        }

        if (principal.IsInstitutionalAdmin())
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

    /// <summary>
    /// <see cref="IsAdministeredBy" /> plus a Coordinator at the trainee's institution: the reach of the roles that
    /// schedule committee reviews, which a CommitteeMember does not. (T182)
    /// </summary>
    public static bool IsAdministeredOrCoordinatedBy(TraineeScope scope, ClaimsPrincipal principal)
        => IsAdministeredBy(scope, principal) || HoldsAtInstitution(scope, principal, WombatRoles.Coordinator);

    /// <summary>
    /// Oversight, T101's read ladder: <see cref="IsAdministeredOrCoordinatedBy" /> plus a CommitteeMember at the
    /// trainee's institution. Must agree with <c>ActivityService.IsScopedOverseerOf</c> and
    /// <c>ActivityReadScope.WhereReadableBy</c>. (T113)
    /// </summary>
    public static bool IsOverseenBy(TraineeScope scope, ClaimsPrincipal principal)
        => IsAdministeredOrCoordinatedBy(scope, principal) ||
           HoldsAtInstitution(scope, principal, WombatRoles.CommitteeMember);

    /// <summary>Whether the caller holds this role at the trainee's institution: the institution-wide arms.</summary>
    private static bool HoldsAtInstitution(TraineeScope scope, ClaimsPrincipal principal, string role)
        => principal.GetInstitutionId() == scope.InstitutionId && principal.IsInRole(role);

    private static bool HoldsOversightRole(ClaimsPrincipal principal)
        => principal.IsInstitutionalAdmin() ||
           principal.IsInRole(WombatRoles.Coordinator) ||
           principal.IsInRole(WombatRoles.CommitteeMember) ||
           principal.IsInRole(WombatRoles.SpecialityAdmin) ||
           principal.IsInRole(WombatRoles.SubSpecialityAdmin);
}
