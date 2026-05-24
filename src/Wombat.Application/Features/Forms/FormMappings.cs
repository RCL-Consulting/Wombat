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
    public static async Task<int?> ResolveFormScopeInstitutionAsync(
        IApplicationDbContext dbContext,
        int? institutionId,
        int? specialityId,
        int? subSpecialityId,
        CancellationToken cancellationToken)
    {
        if (institutionId.HasValue)
        {
            return institutionId.Value;
        }
        if (specialityId.HasValue)
        {
            return await dbContext.Set<Speciality>()
                .Where(entity => entity.Id == specialityId.Value)
                .Select(entity => (int?)entity.InstitutionId)
                .SingleOrDefaultAsync(cancellationToken);
        }
        if (subSpecialityId.HasValue)
        {
            return await dbContext.Set<SubSpeciality>()
                .Where(entity => entity.Id == subSpecialityId.Value)
                .Select(entity => (int?)entity.Speciality.InstitutionId)
                .SingleOrDefaultAsync(cancellationToken);
        }
        return null;
    }

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
