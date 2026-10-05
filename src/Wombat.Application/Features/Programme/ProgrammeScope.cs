using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Programme;

/// <summary>What a programme scope is cut by: the whole institution, a speciality there, or a sub-speciality there.</summary>
public enum ProgrammeScopeKind
{
    /// <summary>A Committee member's or a Coordinator's: every registrar and request at the institution.</summary>
    Institution,

    /// <summary>A Speciality admin's: the institution's registrars and requests in the speciality's sub-specialities.</summary>
    Speciality,

    /// <summary>A Sub-speciality admin's: the institution's registrars and requests in the sub-speciality.</summary>
    SubSpeciality
}

/// <summary>
/// The programme a page reads, as the one role it reads as (T358, flow 06; E4).
/// </summary>
/// <param name="ActingRole">The role read as (a <see cref="WombatRoles" /> key), which the subtitle names.</param>
/// <param name="Kind">What the scope is cut by.</param>
/// <param name="Name">
/// What the subtitle calls it: the institution's name for <see cref="ProgrammeScopeKind.Institution" />, "Kgosi Kgari
/// Teaching Hospital"; else the specialities' or sub-specialities' names, "Paediatrics", joined by ", " in name order.
/// </param>
/// <param name="InstitutionId">The institution, which every kind is held to: speciality ids are national (T091).</param>
/// <param name="SpecialityIds">A Speciality admin's specialities; empty for the other kinds.</param>
/// <param name="SubSpecialityIds">
/// The sub-specialities whose registrars are read: a Speciality admin's specialities' sub-specialities, or a Sub-speciality
/// admin's own; empty for <see cref="ProgrammeScopeKind.Institution" />, which reads them all.
/// </param>
public sealed record ProgrammeScopeDto(
    string ActingRole,
    ProgrammeScopeKind Kind,
    string Name,
    int InstitutionId,
    IReadOnlyList<int> SpecialityIds,
    IReadOnlyList<int> SubSpecialityIds);

/// <summary>
/// Which role a programme page reads as, what that role's scope is, and the two narrowings every programme read applies:
/// trainee profiles, and activities by their stamps (T358, flow 06, lane A0).
/// </summary>
/// <remarks>
/// <para>
/// The scope is the one role's, never the union of the roles held (E4). A Speciality admin who also sits on the committee
/// reads Paediatrics as Speciality admin and the whole hospital as Committee member, each page saying which in its
/// subtitle. Access to a page is the union of the roles held, as everywhere; what the page reads is not.
/// </para>
/// <para>
/// Every kind is held to the caller's institution. Speciality and sub-speciality ids are national (College-owned, T091),
/// so on their own they would reach every adopting institution's registrars, as the staff dashboards' counts did until
/// T130 and T185.
/// </para>
/// <para>
/// A registrar who holds an oversight seat reads no peer's record (<see cref="TraineeScopeResolver.ActsAsTrainee" />,
/// T185), so someone in the programme as a trainee has no programme scope, whatever role they read as.
/// </para>
/// </remarks>
public static class ProgrammeScope
{
    /// <summary>
    /// The roles that read the programme's registrars: Programme trainees and the registrar page admit these (D5).
    /// </summary>
    public static readonly IReadOnlyList<string> RosterRoles =
    [
        WombatRoles.CommitteeMember,
        WombatRoles.SpecialityAdmin,
        WombatRoles.SubSpecialityAdmin,
        WombatRoles.Coordinator
    ];

    /// <summary>
    /// The roles that read what waits for the programme's assessors and may remind them: Waiting for assessors admits these
    /// (D5). Not the Committee member, who reads a registrar's waiting requests on the registrar page and sends no reminder.
    /// </summary>
    public static readonly IReadOnlyList<string> WaitingRoles =
    [
        WombatRoles.SpecialityAdmin,
        WombatRoles.SubSpecialityAdmin,
        WombatRoles.Coordinator
    ];

    /// <summary>
    /// The programme <paramref name="principal" /> reads as <paramref name="actingRole" />, or null when there is none to
    /// read.
    /// </summary>
    /// <remarks>
    /// Null, and so "not found" on every programme page, for someone in the programme as a trainee (T185), for a role the
    /// caller does not hold, for a role that is not one of <see cref="RosterRoles" />, and for a caller with no institution
    /// claim. Also for an institution claim that names no institution, and for a speciality or sub-speciality admin none of
    /// whose claimed specialities or sub-specialities exists: a scope that names nothing has nothing to read, and a page
    /// that named it would say whose programme it is reading while reading nobody's.
    /// </remarks>
    public static async Task<ProgrammeScopeDto?> ResolveAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        string actingRole,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(principal);

        if (string.IsNullOrEmpty(actingRole) ||
            !RosterRoles.Contains(actingRole, StringComparer.Ordinal) ||
            !principal.IsInRole(actingRole) ||
            TraineeScopeResolver.ActsAsTrainee(principal) ||
            principal.GetInstitutionId() is not int institutionId)
        {
            return null;
        }

        var institutionName = await dbContext.Set<Institution>()
            .AsNoTracking()
            .Where(institution => institution.Id == institutionId)
            .Select(institution => institution.Name)
            .SingleOrDefaultAsync(cancellationToken);
        if (institutionName is null)
        {
            return null;
        }

        switch (actingRole)
        {
            case WombatRoles.SpecialityAdmin:
            {
                var claimed = principal.GetSpecialityIds().ToArray();
                var specialities = await dbContext.Set<Speciality>()
                    .AsNoTracking()
                    .Where(speciality => claimed.Contains(speciality.Id))
                    .Select(speciality => new { speciality.Id, speciality.Name })
                    .ToListAsync(cancellationToken);
                if (specialities.Count == 0)
                {
                    return null;
                }

                var specialityIds = specialities.Select(speciality => speciality.Id).Order().ToArray();
                var subSpecialityIds = await dbContext.Set<SubSpeciality>()
                    .AsNoTracking()
                    .Where(subSpeciality => specialityIds.Contains(subSpeciality.SpecialityId))
                    .Select(subSpeciality => subSpeciality.Id)
                    .OrderBy(id => id)
                    .ToListAsync(cancellationToken);

                return new ProgrammeScopeDto(
                    actingRole,
                    ProgrammeScopeKind.Speciality,
                    NamesOf(specialities.Select(speciality => speciality.Name)),
                    institutionId,
                    specialityIds,
                    subSpecialityIds);
            }

            case WombatRoles.SubSpecialityAdmin:
            {
                var claimed = principal.GetSubSpecialityIds().ToArray();
                var subSpecialities = await dbContext.Set<SubSpeciality>()
                    .AsNoTracking()
                    .Where(subSpeciality => claimed.Contains(subSpeciality.Id))
                    .Select(subSpeciality => new { subSpeciality.Id, subSpeciality.Name })
                    .ToListAsync(cancellationToken);
                if (subSpecialities.Count == 0)
                {
                    return null;
                }

                return new ProgrammeScopeDto(
                    actingRole,
                    ProgrammeScopeKind.SubSpeciality,
                    NamesOf(subSpecialities.Select(subSpeciality => subSpeciality.Name)),
                    institutionId,
                    [],
                    subSpecialities.Select(subSpeciality => subSpeciality.Id).Order().ToArray());
            }

            default:
                return new ProgrammeScopeDto(actingRole, ProgrammeScopeKind.Institution, institutionName, institutionId, [], []);
        }
    }

    /// <summary>
    /// The trainee profiles in <paramref name="scope" />: at its institution, and for the two admins on a curriculum of a
    /// sub-speciality in it. Every profile, ended and erased included: a caller that lists the current registrars keeps
    /// them (<see cref="TraineeScopeResolver.KeepCurrentAsync" />, T238), and one that opens one registrar's record keeps
    /// the preferred profile (<see cref="TraineeScopeResolver.PreferredProfiles" />).
    /// </summary>
    public static IQueryable<TraineeProfile> Profiles(IApplicationDbContext dbContext, ProgrammeScopeDto scope)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(scope);

        var institutionId = scope.InstitutionId;
        var profiles = dbContext.Set<TraineeProfile>().Where(profile => profile.InstitutionId == institutionId);
        if (scope.Kind == ProgrammeScopeKind.Institution)
        {
            return profiles;
        }

        var subSpecialityIds = scope.SubSpecialityIds.ToArray();
        return profiles.Where(profile => subSpecialityIds.Contains(profile.Curriculum.SubSpecialityId));
    }

    /// <summary>
    /// The activities in <paramref name="scope" />, by the stamps every activity carries from its subject's programme
    /// (T101): the institution's, and for the two admins the speciality's or the sub-speciality's too. A null stamp is
    /// nobody's, so an activity whose subject had no programme is in no scope.
    /// </summary>
    /// <remarks>
    /// A narrowing, not a read gate: a reader still puts its rows through <c>ActivityReadScope.WhereReadableBy</c>, which
    /// asks the same stamps of the caller's whole authority (T185).
    /// </remarks>
    public static IQueryable<Activity> Activities(IQueryable<Activity> activities, ProgrammeScopeDto scope)
    {
        ArgumentNullException.ThrowIfNull(activities);
        ArgumentNullException.ThrowIfNull(scope);

        int? institutionId = scope.InstitutionId;
        var atInstitution = activities.Where(activity => activity.InstitutionId == institutionId);

        switch (scope.Kind)
        {
            case ProgrammeScopeKind.Speciality:
            {
                var specialityIds = scope.SpecialityIds.ToArray();
                return atInstitution.Where(activity =>
                    activity.SpecialityId != null && specialityIds.Contains(activity.SpecialityId.Value));
            }

            case ProgrammeScopeKind.SubSpeciality:
            {
                var subSpecialityIds = scope.SubSpecialityIds.ToArray();
                return atInstitution.Where(activity =>
                    activity.SubSpecialityId != null && subSpecialityIds.Contains(activity.SubSpecialityId.Value));
            }

            default:
                return atInstitution;
        }
    }

    private static string NamesOf(IEnumerable<string> names)
        => string.Join(", ", names.Order(StringComparer.Ordinal));
}
