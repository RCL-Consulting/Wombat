using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Curricula;

/// <summary>A committee the College names as deciding an EPA (T131).</summary>
/// <param name="Key">What a curriculum item's <c>DecisionBodyKey</c> stores.</param>
/// <param name="Name">The College's name for it, which the pickers print.</param>
public sealed record DecisionBodyDto(string Key, string Name);

/// <summary>The national list of decision bodies, for the curriculum item editor's "Decided by" select.</summary>
/// <remarks>
/// National reference data with no institution in it, like <c>GetWbaToolsQuery</c>, so it takes no principal: the page
/// that asks is already behind the national-catalogue policy.
/// </remarks>
public sealed record GetDecisionBodiesQuery : IRequest<IReadOnlyList<DecisionBodyDto>>;

public sealed class GetDecisionBodiesQueryHandler : IRequestHandler<GetDecisionBodiesQuery, IReadOnlyList<DecisionBodyDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetDecisionBodiesQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<DecisionBodyDto>> Handle(GetDecisionBodiesQuery request, CancellationToken cancellationToken)
        => await _dbContext.Set<DecisionBody>()
            .AsNoTracking()
            .OrderBy(body => body.Name)
            .Select(body => new DecisionBodyDto(body.Key, body.Name))
            .ToListAsync(cancellationToken);
}
