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
/// It is also the programme the trainee's curriculum is read against. Credit (<c>CreditTargetResolver.PickProfileAsync</c>),
/// the EPA and tool pickers, the progress page and the trajectory's ladders all pick <see cref="PreferredProfiles" />
/// (T185). Until then they broke ties by the latest programme start, so a trainee with two past profiles could be credited
/// against one programme while the export and the committee read the other.
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
    /// For a caller that has to know the scope of several trainees at once, where one query per trainee would be three
    /// round trips a trainee. Three queries at most, however many trainees are asked about. A list that only needs to
    /// know which rows the caller administers narrows in SQL instead (<see cref="AdministeredProfiles" />, T185).
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
    /// The trainee rung comes first (<see cref="ActsAsTrainee" />), as it does on the committee review
    /// (<c>CommitteeDecisionAuthorization.DemandReviewAccess</c>): someone who holds Trainee beside an oversight role,
    /// the Administrator role included, reads their own record here and nobody else's, whatever the other role would
    /// reach. Decided in T185; until then a Trainee who was also a CommitteeMember read every trainee at the hospital
    /// here while the review refused them. <see cref="ActsAsTrainee" /> names every surface that applies the rung, and
    /// the one that does not yet.
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

        var callerUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isSelf = !string.IsNullOrEmpty(callerUserId) &&
                     string.Equals(callerUserId, traineeUserId, StringComparison.Ordinal);

        if (ActsAsTrainee(principal))
        {
            return isSelf;
        }

        if (principal.IsAdministrator() || isSelf)
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
    /// Whether this caller is a trainee in the programme, and so reads and administers trainee records as one: their
    /// own record and nobody else's, whatever other role they hold, the Administrator role included. (T185)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Users hold several roles, and a registrar can sit on the committee as the trainees' representative, coordinate a
    /// rotation, or administer the system. The rule is that none of those seats reads, or acts on, a peer's record. It
    /// is asked FIRST, before any role that would admit the caller, by every surface that answers about other trainees:
    /// </para>
    /// <list type="bullet">
    /// <item><see cref="MayReadAsync" />, and so every trainee read that climbs it: progress, committee reviews listed
    /// for a trainee, entrustment decisions and standing, the certificate, MSF campaigns and coverage, the portfolio
    /// export;</item>
    /// <item>the committee review itself (<c>CommitteeDecisionAuthorization.DemandReviewAccess</c>), where the rung
    /// started;</item>
    /// <item>the entrustment admin list and revoking (<c>ListEntrustmentDecisionsForAdminQuery</c>,
    /// <c>EntrustmentDecisionAuthorization.DemandRevocationAccessAsync</c>), so every row the list shows is one the
    /// caller may revoke and download;</item>
    /// <item>the committee member's dashboard, which names each trainee beside their targets;</item>
    /// <item>the decisions-due page (<c>GetEntrustmentDecisionsDueQuery</c>, T131 slice 6), which names each trainee's
    /// standing on every EPA due in a period;</item>
    /// <item>committee scheduling (<c>CommitteeDecisionAuthorization.MayScheduleReviews</c>, T216): scheduling a review,
    /// previewing its agenda and the scheduling page's trainee picker, which such a caller is refused, or offered nobody,
    /// for every trainee, themselves included;</item>
    /// <item>the committee reviews page's list (<c>ListReviewsForPanelQuery</c>, T216), which names each review's trainee
    /// and outcome: such a caller is listed none, their own included, which are on My committee reviews;</item>
    /// <item>conducting a committee review (<c>CommitteeDecisionAuthorization.WorksOnPanel</c> and <c>HoldsSeat</c>, the
    /// T194 review): starting one, every chair's action, and resolving an appeal, which such a caller is refused, and not
    /// offered, on every review, their own included, whatever seat they hold on its panel.</item>
    /// </list>
    /// <para>
    /// NOT yet the activity read gate (<c>ActivityService.IsScopedOverseerOf</c> and <c>ActivityReadScope.WhereReadableBy</c>,
    /// which ask <see cref="IsOverseenBy" /> of the activity's stamps): a Trainee who also holds an oversight role still
    /// opens and lists a peer's activities one by one. Closing that is its own change, because every activity list and
    /// the parity test move with it; until then the portfolio export, which climbs <see cref="MayReadAsync" />, refuses
    /// what the per-activity gate admits.
    /// </para>
    /// </remarks>
    public static bool ActsAsTrainee(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        return principal.IsInRole(WombatRoles.Trainee);
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
    /// The query form of <see cref="IsAdministeredBy" />: the preferred profiles (<see cref="PreferredProfiles" />) of
    /// the trainees this caller administers, for a list that must be narrowed in SQL rather than loaded whole and judged
    /// row by row. (T185)
    /// </summary>
    /// <remarks>
    /// <para>
    /// A trainee's profile is here exactly when <see cref="IsAdministeredBy" /> holds for the scope
    /// <see cref="ResolveAsync" /> gives them. Each arm is the same arm, read at the same level: the sub-speciality from
    /// the profile's curriculum, the speciality from that sub-speciality's row, so a curriculum or sub-speciality that
    /// cannot be reached admits nobody through the arm that needs it, as a null level does there. A parity test holds
    /// the two to each other, caller by caller.
    /// </para>
    /// <para>
    /// Like <see cref="IsAdministeredBy" />, it has no Administrator arm. A caller who sees everything is not narrowed
    /// at all, and that is the list's decision, not this rule's.
    /// </para>
    /// </remarks>
    public static IQueryable<TraineeProfile> AdministeredProfiles(IApplicationDbContext dbContext, ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(principal);

        var preferred = PreferredProfiles(dbContext);
        if (principal.GetInstitutionId() is not int institutionId)
        {
            return preferred.Where(_ => false);
        }

        preferred = preferred.Where(profile => profile.InstitutionId == institutionId);
        if (principal.IsInstitutionalAdmin())
        {
            return preferred;
        }

        int[] specialityIds = principal.IsInRole(WombatRoles.SpecialityAdmin)
            ? principal.GetSpecialityIds().ToArray()
            : [];
        int[] subSpecialityIds = principal.IsInRole(WombatRoles.SubSpecialityAdmin)
            ? principal.GetSubSpecialityIds().ToArray()
            : [];

        var curricula = dbContext.Set<Curriculum>();
        var subSpecialities = dbContext.Set<SubSpeciality>();

        return preferred.Where(profile => curricula.Any(curriculum =>
            curriculum.Id == profile.CurriculumId &&
            (subSpecialityIds.Contains(curriculum.SubSpecialityId) ||
             subSpecialities.Any(subSpeciality =>
                 subSpeciality.Id == curriculum.SubSpecialityId &&
                 specialityIds.Contains(subSpeciality.SpecialityId)))));
    }

    /// <summary>
    /// <see cref="IsAdministeredBy" /> plus a Coordinator at the trainee's institution: the reach of the roles that
    /// schedule committee reviews, which a CommitteeMember does not. (T182)
    /// </summary>
    public static bool IsAdministeredOrCoordinatedBy(TraineeScope scope, ClaimsPrincipal principal)
        => IsAdministeredBy(scope, principal) || HoldsAtInstitution(scope, principal, WombatRoles.Coordinator);

    /// <summary>
    /// Oversight, T101's read ladder: <see cref="IsAdministeredOrCoordinatedBy" /> plus a CommitteeMember at the
    /// trainee's institution. (T113)
    /// </summary>
    /// <remarks>
    /// The single-activity read gate asks this of the scope stamped on the activity
    /// (<c>ActivityService.IsScopedOverseerOf</c>, T185). The list-shaped gate,
    /// <c>ActivityReadScope.WhereReadableBy</c>, is its SQL form over the same stamps, written apart because a list
    /// cannot call this per row; a parity test holds the two to each other for every caller and stamp.
    /// </remarks>
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
