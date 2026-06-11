using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions.Queries.GetSpecialitiesList;

public sealed class GetSpecialitiesListQueryHandler : IRequestHandler<GetSpecialitiesListQuery, IReadOnlyList<SpecialityDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSpecialitiesListQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SpecialityDto>> Handle(GetSpecialitiesListQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Set<Speciality>().AsQueryable();

        if (!request.Principal.IsAdministrator())
        {
            var scopedCollegeId = request.Principal.GetCollegeId();
            if (!scopedCollegeId.HasValue)
            {
                return Array.Empty<SpecialityDto>();
            }

            query = query.Where(entity => entity.CollegeId == scopedCollegeId.Value);
        }

        return await query
            .OrderBy(entity => entity.College.Name)
            .ThenBy(entity => entity.Name)
            .Select(entity => new SpecialityDto(entity.Id, entity.CollegeId, entity.Name, entity.Description, entity.IsActive))
            .ToListAsync(cancellationToken);
    }
}
