using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
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
    {
        var query = _dbContext.Set<SubSpeciality>().AsQueryable();

        if (!request.Principal.IsAdministrator())
        {
            var scopedCollegeId = request.Principal.GetCollegeId();
            if (!scopedCollegeId.HasValue)
            {
                return Array.Empty<SubSpecialityDto>();
            }

            query = query.Where(entity => entity.Speciality.CollegeId == scopedCollegeId.Value);
        }

        return await query
            .OrderBy(entity => entity.Speciality.College.Name)
            .ThenBy(entity => entity.Speciality.Name)
            .ThenBy(entity => entity.Name)
            .Select(entity => new SubSpecialityDto(entity.Id, entity.SpecialityId, entity.Name, entity.Description, entity.IsActive, entity.DefaultEntrustmentScaleId))
            .ToListAsync(cancellationToken);
    }
}
