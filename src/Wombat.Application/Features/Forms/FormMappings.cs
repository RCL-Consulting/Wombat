using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Forms;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Forms;

internal static class FormMappings
{
    public static Task<AssessmentFormDto?> GetByIdAsync(IApplicationDbContext dbContext, int id, CancellationToken cancellationToken)
        => dbContext.Set<AssessmentForm>()
            .Where(form => form.Id == id)
            .Select(form => new AssessmentFormDto(
                form.Id,
                form.Name,
                form.InstitutionId,
                form.SpecialityId,
                form.SubSpecialityId,
                form.ScaleId,
                form.Scale.Name,
                form.CanDelete,
                form.IsActive,
                form.Criteria
                    .OrderBy(criteria => criteria.Order)
                    .Select(criteria => new FormCriterionDto(criteria.Id, criteria.Order, criteria.Prompt, criteria.HelpText, criteria.IsRequired))
                    .ToList(),
                form.EpaLinks
                    .OrderBy(link => link.Epa.Code)
                    .Select(link => new FormEpaLinkDto(link.Id, link.EpaId, link.Epa.Code, link.Epa.Title))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

    /// <summary>
    /// Resolves the institution that an AssessmentForm scope-tuple belongs to.
    /// Returns null for fully-global forms (no institution / speciality / sub-speciality set) —
    /// callers should treat those as visible to InstitutionalAdmin and editable only by Administrator.
    /// </summary>
    // Speciality/sub-speciality scopes now reference national (College-owned) disciplines (T091) and no
    // longer resolve to an owning institution; only the explicit Institution scope does. Discipline-scoped
    // (and global) forms therefore resolve to null = Administrator-only to write, per the guard below.
    public static Task<int?> ResolveFormScopeInstitutionAsync(
        IApplicationDbContext dbContext,
        int? institutionId,
        int? specialityId,
        int? subSpecialityId,
        CancellationToken cancellationToken)
        => Task.FromResult(institutionId);

    /// <summary>
    /// Throws UnauthorizedAccessException unless the caller may write to the given form-scope tuple.
    /// Global forms (institution/speciality/subSpeciality all null) are Administrator-only.
    /// </summary>
    public static async Task EnsureCallerCanWriteAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        int? institutionId,
        int? specialityId,
        int? subSpecialityId,
        CancellationToken cancellationToken)
    {
        if (principal.IsAdministrator())
        {
            return;
        }

        if (!principal.IsInstitutionalAdmin())
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this assessment form.");
        }

        var scopeInstitutionId = await ResolveFormScopeInstitutionAsync(dbContext, institutionId, specialityId, subSpecialityId, cancellationToken);
        if (!scopeInstitutionId.HasValue)
        {
            throw new UnauthorizedAccessException("Only global administrators may edit a globally-scoped assessment form.");
        }

        if (!principal.CanAccessInstitution(scopeInstitutionId.Value))
        {
            throw new UnauthorizedAccessException("You do not have permission to modify assessment forms outside your institution.");
        }
    }
}
