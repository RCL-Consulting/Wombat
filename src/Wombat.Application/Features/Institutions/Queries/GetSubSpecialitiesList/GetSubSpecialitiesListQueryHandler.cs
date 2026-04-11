using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions.Queries.GetSubSpecialitiesList;

public sealed class GetSubSpecialitiesListQueryHandler : IRequestHandler<GetSubSpecialitiesListQuery, IReadOnlyList<SubSpecialityDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSubSpecialitiesListQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SubSpecialityDto>> Handle(GetSubSpecialitiesListQuery request, CancellationToken cancellationToken)
        => await _dbContext.Set<SubSpeciality>()
            .OrderBy(entity => entity.Speciality.Institution.Name)
            .ThenBy(entity => entity.Speciality.Name)
            .ThenBy(entity => entity.Name)
            .Select(entity => new SubSpecialityDto(entity.Id, entity.SpecialityId, entity.Name, entity.Description, entity.IsActive))
            .ToListAsync(cancellationToken);
}
