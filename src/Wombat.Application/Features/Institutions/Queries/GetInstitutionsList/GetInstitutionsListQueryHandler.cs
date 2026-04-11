using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;

public sealed class GetInstitutionsListQueryHandler : IRequestHandler<GetInstitutionsListQuery, IReadOnlyList<InstitutionDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetInstitutionsListQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<InstitutionDto>> Handle(GetInstitutionsListQuery request, CancellationToken cancellationToken)
        => await _dbContext.Set<Institution>()
            .OrderBy(entity => entity.Name)
            .Select(entity => new InstitutionDto(entity.Id, entity.Name, entity.ShortCode, entity.ContactEmail, entity.IsActive, entity.CreatedOn))
            .ToListAsync(cancellationToken);
}
