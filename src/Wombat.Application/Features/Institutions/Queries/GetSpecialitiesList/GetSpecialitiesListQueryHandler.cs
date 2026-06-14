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
            var scopedInstitutionId = request.Principal.GetInstitutionId();

            if (scopedCollegeId.HasValue)
            {
                // CollegeAdmin: the national specialities their own College owns. (T091)
                query = query.Where(entity => entity.CollegeId == scopedCollegeId.Value);
            }
            else if (request.Principal.IsInstitutionalAdmin() && scopedInstitutionId.HasValue)
            {
                // InstitutionalAdmin: the national specialities their institution has an active
                // adoption for — the disciplines they actually train, and so can invite assessors/
                // trainees into and scope activity types / panels / forms against. Without this an
                // InstitutionalAdmin (who has no College claim) saw an empty list and could not issue
                // the Assessor/Trainee invitations that require speciality scope. (T092)
                var adoptedSpecialityIds =
                    from adoption in _dbContext.Set<InstitutionCurriculumAdoption>()
                    where adoption.IsActive && adoption.InstitutionId == scopedInstitutionId.Value
                    join subSpeciality in _dbContext.Set<SubSpeciality>() on adoption.SubSpecialityId equals subSpeciality.Id
                    select subSpeciality.SpecialityId;

                query = query.Where(entity => adoptedSpecialityIds.Contains(entity.Id));
            }
            else
            {
                return Array.Empty<SpecialityDto>();
            }
        }

        return await query
            .OrderBy(entity => entity.College.Name)
            .ThenBy(entity => entity.Name)
            .Select(entity => new SpecialityDto(entity.Id, entity.CollegeId, entity.Name, entity.Description, entity.IsActive))
            .ToListAsync(cancellationToken);
    }
}
