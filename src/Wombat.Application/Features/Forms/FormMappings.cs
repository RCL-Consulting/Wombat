using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Forms;

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
}
