using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions.Queries.GetSpecialityById;

public sealed class GetSpecialityByIdQueryHandler : IRequestHandler<GetSpecialityByIdQuery, SpecialityDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSpecialityByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SpecialityDto?> Handle(GetSpecialityByIdQuery request, CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Set<Speciality>()
            .Where(entity => entity.Id == request.Id)
            .Select(entity => new SpecialityDto(entity.Id, entity.CollegeId, entity.Name, entity.Description, entity.IsActive))
            .SingleOrDefaultAsync(cancellationToken);

        if (projection is null)
        {
            return null;
        }

        // 404 (null), not 403, for an out-of-college speciality so other Colleges' rows aren't leaked.
        return request.Principal.CanAccessCollege(projection.CollegeId) ? projection : null;
    }
}
