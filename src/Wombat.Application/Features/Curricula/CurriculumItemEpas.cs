using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Curricula;

/// <summary>
/// Which EPAs a curriculum item may name (T195): the one rule the item editor's EPA pickers list and
/// <see cref="AddCurriculumItemCommandHandler" /> and <see cref="UpdateCurriculumItemCommandHandler" /> enforce.
/// </summary>
/// <remarks>
/// <para>
/// <b>The rule.</b> An item names an EPA of its curriculum's own sub-speciality. A national item (no owning institution)
/// names a national EPA. An institution-local item names a national EPA or one of its owning institution's local EPAs.
/// No item names another institution's local EPA.
/// </para>
/// <para>
/// <b>Whose local EPAs.</b> A new item's owner is the caller's institution when the caller adds as an InstitutionalAdmin
/// (<see cref="OwnerOfNewItem" />), so for an Add it is the caller's own institution. On an edit it is the stored item's
/// owner, which the Update handler has already required the caller to be able to act for. An Administrator editing
/// institution A's item may therefore name A's local EPAs, and not those of the institution the Administrator happens
/// to hold a claim for.
/// </para>
/// <para>
/// Before T195 neither handler looked at the EPA. A national item, which is part of the College's published curriculum
/// and shared by every institution that adopts it, could point at one institution's local EPA, and any item could point
/// at an EPA of another sub-speciality. The Add picker listed every local EPA of the sub-speciality to an Administrator.
/// </para>
/// <para>
/// The refusal names neither the EPA nor its owner. A local EPA belongs to its institution; telling a caller from
/// elsewhere what it is would leak what a scoped read hides. The rule is judged on the REQUESTED EPA on every save, not
/// only when it changes: there are no stored items to protect (W-007), and an item that names an EPA it may not name is
/// the defect, whichever save first shows it.
/// </para>
/// </remarks>
public static class CurriculumItemEpas
{
    /// <summary>
    /// The EPAs an item of a curriculum of <paramref name="curriculumSubSpecialityId" />, owned by
    /// <paramref name="itemOwningInstitutionId" /> (null for a national item), may name.
    /// </summary>
    public static Expression<Func<Epa, bool>> Nameable(int curriculumSubSpecialityId, int? itemOwningInstitutionId)
        => epa => epa.SubSpecialityId == curriculumSubSpecialityId
            && (epa.OwningInstitutionId == null
                || (itemOwningInstitutionId != null && epa.OwningInstitutionId == itemOwningInstitutionId));

    /// <summary>
    /// Who owns an item the caller adds: null (the national core) for an Administrator or a CollegeAdmin, the caller's
    /// institution for an InstitutionalAdmin adding a local extra to an adopted curriculum (T091 phase 3).
    /// </summary>
    public static int? OwnerOfNewItem(ClaimsPrincipal principal)
        => !principal.IsAdministrator() && !principal.IsCollegeAdmin() && principal.IsInstitutionalAdmin()
            ? principal.GetInstitutionId()
            : null;

    /// <summary>
    /// Whether the caller may write an item with this owner on a curriculum of <paramref name="collegeId" />: a national
    /// item is the College's, a local one its institution's.
    /// </summary>
    public static bool MayWrite(ClaimsPrincipal principal, int collegeId, int? itemOwningInstitutionId)
        => itemOwningInstitutionId is null
            ? principal.CanAccessCollege(collegeId)
            : principal.CanAccessInstitution(itemOwningInstitutionId.Value);

    /// <summary>The EPAs the picker offers such an item, by code. Inactive ones included, as the item editor marks them (T158).</summary>
    internal static async Task<IReadOnlyList<EpaDto>> ListAsync(
        IApplicationDbContext dbContext,
        int curriculumSubSpecialityId,
        int? itemOwningInstitutionId,
        CancellationToken cancellationToken)
        => await dbContext.Set<Epa>()
            .AsNoTracking()
            .Where(Nameable(curriculumSubSpecialityId, itemOwningInstitutionId))
            .OrderBy(entity => entity.Code)
            .ThenBy(entity => entity.Id)
            .Select(entity => new EpaDto(
                entity.Id,
                entity.SubSpecialityId,
                entity.SubSpeciality.Name,
                entity.SubSpeciality.Speciality.College.Name,
                entity.Code,
                entity.Title,
                entity.Description,
                entity.RequiredKnowledgeSkills,
                entity.Category,
                entity.IsActive,
                entity.CreatedOn))
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Refuses an EPA the item may not name, including one that does not exist. Called before either handler mutates
    /// anything: the audit pipeline commits a half-finished mutation when a handler throws.
    /// </summary>
    /// <param name="curriculum">Loaded with its sub-speciality, whose name the refusal gives.</param>
    internal static async Task EnsureNameableAsync(
        IApplicationDbContext dbContext,
        Curriculum curriculum,
        int? itemOwningInstitutionId,
        int epaId,
        CancellationToken cancellationToken)
    {
        var nameable = await dbContext.Set<Epa>()
            .AsNoTracking()
            .Where(entity => entity.Id == epaId)
            .Where(Nameable(curriculum.SubSpecialityId, itemOwningInstitutionId))
            .AnyAsync(cancellationToken);
        if (nameable)
        {
            return;
        }

        var subSpeciality = curriculum.SubSpeciality.Name;
        throw new InvalidOperationException(itemOwningInstitutionId is null
            ? $"This item cannot name the selected EPA. A national curriculum item names a national EPA of {subSpeciality}."
            : $"This item cannot name the selected EPA. An institution's own curriculum item names a national EPA of {subSpeciality}, or one of that institution's local EPAs of {subSpeciality}.");
    }
}
