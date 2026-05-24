using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions.Queries.GetInstitutionById;

public sealed class GetInstitutionByIdQueryHandler : IRequestHandler<GetInstitutionByIdQuery, InstitutionDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetInstitutionByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<InstitutionDto?> Handle(GetInstitutionByIdQuery request, CancellationToken cancellationToken)
    {
        if (!request.Principal.CanAccessInstitution(request.Id))
        {
            return null;
        }

        return await _dbContext.Set<Institution>()
            .Where(entity => entity.Id == request.Id)
            .Select(entity => new InstitutionDto(entity.Id, entity.Name, entity.ShortCode, entity.ContactEmail, entity.IsActive, entity.CreatedOn))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
