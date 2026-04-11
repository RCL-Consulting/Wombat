using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions.Queries.GetSubSpecialitiesForSpeciality;

public sealed class GetSubSpecialitiesForSpecialityQueryHandler : IRequestHandler<GetSubSpecialitiesForSpecialityQuery, IReadOnlyList<SubSpecialityDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSubSpecialitiesForSpecialityQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SubSpecialityDto>> Handle(GetSubSpecialitiesForSpecialityQuery request, CancellationToken cancellationToken)
        => await _dbContext.Set<SubSpeciality>()
            .Where(entity => entity.SpecialityId == request.SpecialityId)
            .OrderBy(entity => entity.Name)
            .Select(entity => new SubSpecialityDto(entity.Id, entity.SpecialityId, entity.Name, entity.Description, entity.IsActive))
            .ToListAsync(cancellationToken);
}
