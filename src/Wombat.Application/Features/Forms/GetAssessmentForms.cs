using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Forms;

namespace Wombat.Application.Features.Forms;

public sealed record GetAssessmentFormsListQuery() : IRequest<IReadOnlyList<AssessmentFormDto>>;
public sealed record GetAssessmentFormByIdQuery(int Id) : IRequest<AssessmentFormDto?>;

public sealed class GetAssessmentFormsListQueryHandler : IRequestHandler<GetAssessmentFormsListQuery, IReadOnlyList<AssessmentFormDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAssessmentFormsListQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AssessmentFormDto>> Handle(GetAssessmentFormsListQuery request, CancellationToken cancellationToken)
        => await _dbContext.Set<AssessmentForm>()
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

public sealed class GetAssessmentFormByIdQueryHandler : IRequestHandler<GetAssessmentFormByIdQuery, AssessmentFormDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAssessmentFormByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AssessmentFormDto?> Handle(GetAssessmentFormByIdQuery request, CancellationToken cancellationToken)
        => FormMappings.GetByIdAsync(_dbContext, request.Id, cancellationToken);
}
