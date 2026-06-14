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
            var scopedInstitutionId = request.Principal.GetInstitutionId();

            if (scopedCollegeId.HasValue)
            {
                // CollegeAdmin: the sub-specialities their own College owns. (T091)
                query = query.Where(entity => entity.Speciality.CollegeId == scopedCollegeId.Value);
            }
            else if (request.Principal.IsInstitutionalAdmin() && scopedInstitutionId.HasValue)
            {
                // InstitutionalAdmin: the sub-specialities (disciplines) their institution has an
                // active adoption for. Mirrors GetSpecialitiesListQuery. (T092)
                var adoptedSubSpecialityIds = _dbContext.Set<InstitutionCurriculumAdoption>()
                    .Where(adoption => adoption.IsActive && adoption.InstitutionId == scopedInstitutionId.Value)
                    .Select(adoption => adoption.SubSpecialityId);

                query = query.Where(entity => adoptedSubSpecialityIds.Contains(entity.Id));
            }
            else
            {
                return Array.Empty<SubSpecialityDto>();
            }
        }

        return await query
            .OrderBy(entity => entity.Speciality.College.Name)
            .ThenBy(entity => entity.Speciality.Name)
            .ThenBy(entity => entity.Name)
            .Select(entity => new SubSpecialityDto(entity.Id, entity.SpecialityId, entity.Name, entity.Description, entity.IsActive, entity.DefaultEntrustmentScaleId))
            .ToListAsync(cancellationToken);
    }
}
