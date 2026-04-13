using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Application.Features.MultiSourceFeedback;

public sealed record ListMsfTemplatesQuery() : IRequest<IReadOnlyList<MsfTemplateDto>>;

public sealed class ListMsfTemplatesQueryHandler : IRequestHandler<ListMsfTemplatesQuery, IReadOnlyList<MsfTemplateDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListMsfTemplatesQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MsfTemplateDto>> Handle(ListMsfTemplatesQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Set<Wombat.Domain.MultiSourceFeedback.MsfTemplate>()
            .AsNoTracking()
            .Include(template => template.Questions)
            .OrderBy(template => template.Name)
            .Select(template => new MsfTemplateDto(
                template.Id,
                template.Name,
                template.SpecialityId,
                template.AllowPatientResponses,
                template.IsActive,
                template.Questions
                    .OrderBy(question => question.Order)
                    .Select(question => new MsfQuestionDto(question.Id, question.Order, question.Prompt, question.Type, question.ScaleId, question.Required))
                    .ToList()))
            .ToListAsync(cancellationToken);
    }
}
