using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Colleges;

public sealed record GetCollegesListQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<CollegeDto>>;
public sealed record GetCollegeByIdQuery(int Id, ClaimsPrincipal Principal) : IRequest<CollegeDto?>;

public sealed class GetCollegesListQueryHandler : IRequestHandler<GetCollegesListQuery, IReadOnlyList<CollegeDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCollegesListQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CollegeDto>> Handle(GetCollegesListQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Set<College>().AsQueryable();

        if (!request.Principal.IsAdministrator())
        {
            // A CollegeAdmin sees only their own College; anyone else routed here sees nothing.
            var scopedCollegeId = request.Principal.GetCollegeId();
            if (!scopedCollegeId.HasValue)
            {
                return Array.Empty<CollegeDto>();
            }

            query = query.Where(entity => entity.Id == scopedCollegeId.Value);
        }

        return await query
            .OrderBy(entity => entity.Name)
            .Select(entity => new CollegeDto(entity.Id, entity.Name, entity.ShortCode, entity.Description, entity.IsActive, entity.CreatedOn))
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetCollegeByIdQueryHandler : IRequestHandler<GetCollegeByIdQuery, CollegeDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCollegeByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CollegeDto?> Handle(GetCollegeByIdQuery request, CancellationToken cancellationToken)
    {
        // 404 (null), not 403, for out-of-scope ids so other Colleges' existence isn't leaked.
        if (!request.Principal.CanAccessCollege(request.Id))
        {
            return null;
        }

        return await _dbContext.Set<College>()
            .Where(entity => entity.Id == request.Id)
            .Select(entity => new CollegeDto(entity.Id, entity.Name, entity.ShortCode, entity.Description, entity.IsActive, entity.CreatedOn))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
