using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions.Queries.GetSpecialitiesForInstitution;

public sealed class GetSpecialitiesForInstitutionQueryHandler : IRequestHandler<GetSpecialitiesForInstitutionQuery, IReadOnlyList<SpecialityDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSpecialitiesForInstitutionQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SpecialityDto>> Handle(GetSpecialitiesForInstitutionQuery request, CancellationToken cancellationToken)
    {
        if (!request.Principal.CanAccessCollege(request.CollegeId))
        {
            return Array.Empty<SpecialityDto>();
        }

        return await _dbContext.Set<Speciality>()
            .Where(entity => entity.CollegeId == request.CollegeId)
            .OrderBy(entity => entity.Name)
            .Select(entity => new SpecialityDto(entity.Id, entity.CollegeId, entity.Name, entity.Description, entity.IsActive))
            .ToListAsync(cancellationToken);
    }
}
