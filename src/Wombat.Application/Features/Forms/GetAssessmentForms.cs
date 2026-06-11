using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Forms;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Forms;

public sealed record GetAssessmentFormsListQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<AssessmentFormDto>>;
public sealed record GetAssessmentFormByIdQuery(int Id, ClaimsPrincipal Principal) : IRequest<AssessmentFormDto?>;

public sealed class GetAssessmentFormsListQueryHandler : IRequestHandler<GetAssessmentFormsListQuery, IReadOnlyList<AssessmentFormDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAssessmentFormsListQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AssessmentFormDto>> Handle(GetAssessmentFormsListQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Set<AssessmentForm>().AsQueryable();

        if (!request.Principal.IsAdministrator())
        {
            var scopedInstitutionId = request.Principal.GetInstitutionId();
            if (!scopedInstitutionId.HasValue)
            {
                return Array.Empty<AssessmentFormDto>();
            }

            var institutionId = scopedInstitutionId.Value;
            // Forms are institution-owned via form.InstitutionId; speciality/sub-speciality now denote a
            // national discipline (T091), not ownership. A non-admin sees their institution's forms plus
            // global forms (all scope ids null); write attempts get blocked by the per-command scope guard.
            query = query.Where(form =>
                form.InstitutionId == institutionId ||
                (form.InstitutionId == null && form.SpecialityId == null && form.SubSpecialityId == null));
        }

        return await query
            .OrderBy(entity => entity.Name)
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
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetAssessmentFormByIdQueryHandler : IRequestHandler<GetAssessmentFormByIdQuery, AssessmentFormDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAssessmentFormByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AssessmentFormDto?> Handle(GetAssessmentFormByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await FormMappings.GetByIdAsync(_dbContext, request.Id, cancellationToken);
        if (dto is null)
        {
            return null;
        }

        if (request.Principal.IsAdministrator())
        {
            return dto;
        }

        var scopeInstitutionId = await FormMappings.ResolveFormScopeInstitutionAsync(_dbContext, dto.InstitutionId, dto.SpecialityId, dto.SubSpecialityId, cancellationToken);
        // Global form (no institution resolved): InstitutionalAdmin may still read.
        if (!scopeInstitutionId.HasValue)
        {
            return request.Principal.IsInstitutionalAdmin() ? dto : null;
        }

        return request.Principal.CanAccessInstitution(scopeInstitutionId.Value) ? dto : null;
    }
}
