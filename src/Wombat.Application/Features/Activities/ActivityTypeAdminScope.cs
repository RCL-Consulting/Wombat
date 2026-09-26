using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Activities;

/// <summary>
/// Who may write an activity type, and in which scopes they may put one (T300). The builder's commands refuse by
/// <see cref="MayWriteAsync" /> (<see cref="ActivityTypeScopeGuard" />), and its pages offer by the same two methods: the
/// editor's and the list's <c>CanWrite</c>, and the editor's Scope picker, which lists <see cref="WritableScopesAsync" />.
/// So a page offers exactly what the commands accept (DESIGN.md § Table system, T211), as <c>CurriculumAdminScope</c> does
/// for the curriculum pages.
/// </summary>
/// <remarks>
/// <para>
/// <b>The rule.</b> An Administrator writes every type. A Global type is the Administrator's alone. An Institution type is
/// its institution's: the InstitutionalAdmin of that institution writes it. A Speciality or SubSpeciality type is the
/// College's that owns the discipline, since disciplines are national (T091): that College's CollegeAdmin writes it. Each
/// role is judged only with its own claim, as <c>CanAccessInstitution</c> and <c>CanAccessCollege</c> judge it: every
/// signed-in user carries an institution claim (T113), and it makes nobody an InstitutionalAdmin.
/// </para>
/// <para>
/// <b>Decided with T300: the College writes its instruments in the builder.</b> The rule named the College as the author
/// of its disciplines' types from T091, but only an Administrator could open the builder, so the College could change
/// none of its own instruments. T300 admits the CollegeAdmin to both builder pages (<c>NationalCatalogueAccess</c>).
/// Rejected: narrowing the Speciality and SubSpeciality cases to the Administrator, so that a College instrument changes
/// only by a seed release. What follows: a seeded type that anyone other than the seeder publishes is handed over, and
/// <c>ActivityTypeSeedRefresher</c> (T103) no longer refreshes it from its seed folder, and a draft saved on one pauses
/// its refresh until the draft is published or discarded. That hand-over is intended, and it is why a scenario replay
/// saves no draft on a seeded type.
/// </para>
/// <para>
/// Before T300 the builder offered Save draft, Publish and every editor to every caller its policy admitted, and a new
/// type every scope, defaulting to Global, so an InstitutionalAdmin was refused on a College instrument's every save and
/// on a new type's first. The College, which the rule names as the author of its disciplines' types, was not admitted to
/// the builder at all.
/// </para>
/// </remarks>
public static class ActivityTypeAdminScope
{
    /// <summary>
    /// Whether the caller may write a type at this scope and target, given the College that owns the target when it is a
    /// speciality or sub-speciality (null otherwise, or when the target does not exist). The rule itself, without a read,
    /// for a caller that has looked the College up already: a list does it once for all its rows.
    /// </summary>
    public static bool MayWrite(ClaimsPrincipal principal, ActivityScope scope, int? scopeId, int? owningCollegeId)
    {
        if (principal.IsAdministrator())
        {
            return true;
        }

        return scope switch
        {
            ActivityScope.Global => false,
            ActivityScope.Institution => principal.IsInstitutionalAdmin()
                && principal.GetInstitutionId() is int institutionId
                && scopeId == institutionId,
            ActivityScope.Speciality or ActivityScope.SubSpeciality => owningCollegeId is int collegeId
                && principal.CanAccessCollege(collegeId),
            _ => false
        };
    }

    /// <summary>
    /// Whether the caller may write a type at this scope and target: <see cref="MayWrite" />, with the College that owns a
    /// speciality or sub-speciality read from the store. It never throws for a refusal; <see cref="ActivityTypeScopeGuard" />
    /// does, on false.
    /// </summary>
    public static async Task<bool> MayWriteAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        ActivityScope scope,
        int? scopeId,
        CancellationToken cancellationToken)
        => principal.IsAdministrator()
           || MayWrite(principal, scope, scopeId, await OwningCollegeIdAsync(dbContext, scope, scopeId, cancellationToken));

    /// <summary>
    /// The scopes the caller may put a type in, each with every target the caller may put it at, in the order the Scope
    /// picker offers them: Global, Institution, Speciality, SubSpeciality. Exactly the pairs <see cref="MayWriteAsync" />
    /// admits among the targets that exist. A scope with no target is left out, so an empty list means the caller may
    /// create no type at all.
    /// </summary>
    public static async Task<IReadOnlyList<ActivityTypeScopeChoiceDto>> WritableScopesAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        // Each role only with its own claim, as MayWrite reads them.
        var isAdministrator = principal.IsAdministrator();
        var institutionId = !isAdministrator && principal.IsInstitutionalAdmin() ? principal.GetInstitutionId() : null;
        var collegeId = !isAdministrator && principal.IsCollegeAdmin() ? principal.GetCollegeId() : null;

        var choices = new List<ActivityTypeScopeChoiceDto>();
        if (isAdministrator)
        {
            choices.Add(new ActivityTypeScopeChoiceDto(ActivityScope.Global, []));
        }

        if (isAdministrator || institutionId is not null)
        {
            AddIfAny(choices, ActivityScope.Institution, await dbContext.Set<Institution>()
                .AsNoTracking()
                .Where(entity => isAdministrator || entity.Id == institutionId)
                .OrderBy(entity => entity.Name)
                .Select(entity => new ActivityTypeScopeTargetDto(entity.Id, entity.Name))
                .ToListAsync(cancellationToken));
        }

        if (isAdministrator || collegeId is not null)
        {
            AddIfAny(choices, ActivityScope.Speciality, await dbContext.Set<Speciality>()
                .AsNoTracking()
                .Where(entity => isAdministrator || entity.CollegeId == collegeId)
                .OrderBy(entity => entity.Name)
                .Select(entity => new ActivityTypeScopeTargetDto(entity.Id, entity.Name))
                .ToListAsync(cancellationToken));

            AddIfAny(choices, ActivityScope.SubSpeciality, await dbContext.Set<SubSpeciality>()
                .AsNoTracking()
                .Where(entity => isAdministrator || entity.Speciality.CollegeId == collegeId)
                .OrderBy(entity => entity.Speciality.Name)
                .ThenBy(entity => entity.Name)
                .Select(entity => new ActivityTypeScopeTargetDto(entity.Id, entity.Speciality.Name + " / " + entity.Name))
                .ToListAsync(cancellationToken));
        }

        return choices;
    }

    /// <summary>
    /// The stored scope's target as a reader sees it: the institution's or the speciality's name, or "Speciality /
    /// Sub-speciality"; null for Global, or for a target that does not exist. <see cref="ScopeTargetsAsync" /> for one
    /// target, so the editor and the list name a target alike.
    /// </summary>
    internal static async Task<string?> ScopeTargetNameAsync(
        IApplicationDbContext dbContext,
        ActivityScope scope,
        int? scopeId,
        CancellationToken cancellationToken)
        => scopeId is null
            ? null
            : (await ScopeTargetsAsync(dbContext, [(scope, scopeId)], cancellationToken))(scope, scopeId).Name;

    /// <summary>The College that owns a speciality or sub-speciality target; null for any other scope, or no such target.</summary>
    private static async Task<int?> OwningCollegeIdAsync(
        IApplicationDbContext dbContext,
        ActivityScope scope,
        int? scopeId,
        CancellationToken cancellationToken)
    {
        if (scopeId is not int id)
        {
            return null;
        }

        return scope switch
        {
            ActivityScope.Speciality => await dbContext.Set<Speciality>()
                .Where(entity => entity.Id == id)
                .Select(entity => (int?)entity.CollegeId)
                .SingleOrDefaultAsync(cancellationToken),
            ActivityScope.SubSpeciality => await dbContext.Set<SubSpeciality>()
                .Where(entity => entity.Id == id)
                .Select(entity => (int?)entity.Speciality.CollegeId)
                .SingleOrDefaultAsync(cancellationToken),
            _ => null
        };
    }

    /// <summary>
    /// Each target among these by name, as <see cref="ScopeTargetNameAsync" /> names one, with the College that owns it
    /// where it is a speciality or sub-speciality: read in at most three queries, as a lookup the list judges and labels
    /// every row by (<see cref="MayWrite" />) without a read per row. A target not among them, or that does not exist,
    /// has neither.
    /// </summary>
    internal static async Task<Func<ActivityScope, int?, ScopeTarget>> ScopeTargetsAsync(
        IApplicationDbContext dbContext,
        IReadOnlyCollection<(ActivityScope Scope, int? ScopeId)> targets,
        CancellationToken cancellationToken)
    {
        var institutionIds = IdsIn(targets, ActivityScope.Institution);
        var specialityIds = IdsIn(targets, ActivityScope.Speciality);
        var subSpecialityIds = IdsIn(targets, ActivityScope.SubSpeciality);

        var institutions = new Dictionary<int, ScopeTarget>();
        if (institutionIds.Count > 0)
        {
            institutions = await dbContext.Set<Institution>()
                .Where(entity => institutionIds.Contains(entity.Id))
                .Select(entity => new { entity.Id, entity.Name })
                .ToDictionaryAsync(entity => entity.Id, entity => new ScopeTarget(entity.Name, null), cancellationToken);
        }

        var specialities = new Dictionary<int, ScopeTarget>();
        if (specialityIds.Count > 0)
        {
            specialities = await dbContext.Set<Speciality>()
                .Where(entity => specialityIds.Contains(entity.Id))
                .Select(entity => new { entity.Id, entity.Name, entity.CollegeId })
                .ToDictionaryAsync(entity => entity.Id, entity => new ScopeTarget(entity.Name, entity.CollegeId), cancellationToken);
        }

        var subSpecialities = new Dictionary<int, ScopeTarget>();
        if (subSpecialityIds.Count > 0)
        {
            subSpecialities = await dbContext.Set<SubSpeciality>()
                .Where(entity => subSpecialityIds.Contains(entity.Id))
                .Select(entity => new { entity.Id, Name = entity.Speciality.Name + " / " + entity.Name, entity.Speciality.CollegeId })
                .ToDictionaryAsync(entity => entity.Id, entity => new ScopeTarget(entity.Name, entity.CollegeId), cancellationToken);
        }

        return (scope, scopeId) =>
        {
            var byId = scope switch
            {
                ActivityScope.Institution => institutions,
                ActivityScope.Speciality => specialities,
                ActivityScope.SubSpeciality => subSpecialities,
                _ => null
            };

            return byId is not null && scopeId is int id && byId.TryGetValue(id, out var target) ? target : default;
        };
    }

    private static List<int> IdsIn(IEnumerable<(ActivityScope Scope, int? ScopeId)> targets, ActivityScope scope)
        => targets
            .Where(target => target.Scope == scope && target.ScopeId.HasValue)
            .Select(target => target.ScopeId!.Value)
            .Distinct()
            .ToList();

    private static void AddIfAny(List<ActivityTypeScopeChoiceDto> choices, ActivityScope scope, IReadOnlyList<ActivityTypeScopeTargetDto> targets)
    {
        if (targets.Count > 0)
        {
            choices.Add(new ActivityTypeScopeChoiceDto(scope, targets));
        }
    }
}

/// <summary>
/// A scope's target as <see cref="ActivityTypeAdminScope.ScopeTargetsAsync" /> reads it: its name as a reader sees it,
/// and the College that owns it where it is a speciality or sub-speciality. Both null for a target that does not exist.
/// </summary>
internal readonly record struct ScopeTarget(string? Name, int? OwningCollegeId);

/// <summary>
/// Refuses a save, discard or publish the caller may not make: <see cref="ActivityTypeAdminScope.MayWriteAsync" />, thrown
/// on false (T300). A read only, so every command asks it before its first mutation: the audit pipeline's catch saves the
/// request's context (T201).
/// </summary>
internal static class ActivityTypeScopeGuard
{
    public static async Task EnsureCallerCanWriteAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        ActivityScope scope,
        int? scopeId,
        CancellationToken cancellationToken)
    {
        if (await ActivityTypeAdminScope.MayWriteAsync(dbContext, principal, scope, scopeId, cancellationToken))
        {
            return;
        }

        throw new UnauthorizedAccessException(scope switch
        {
            ActivityScope.Global => "Only global administrators may edit a globally-scoped activity type.",
            ActivityScope.Institution => "You do not have permission to modify activity types in that institution.",
            ActivityScope.Speciality => "You do not have permission to modify activity types in that speciality.",
            ActivityScope.SubSpeciality => "You do not have permission to modify activity types in that sub-speciality.",
            _ => "Unknown activity-type scope."
        });
    }
}
