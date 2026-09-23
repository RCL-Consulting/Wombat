using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Epas;

/// <summary>A workplace-based assessment instrument the College names (T122).</summary>
/// <param name="Key">What an activity type's <c>WbaToolKey</c> and a curriculum item's tool list store.</param>
/// <param name="Name">The College's display name: what a refusal message and the admin pickers print.</param>
/// <param name="Description">Page 8's one-sentence definition, or null for the three instruments it does not define.</param>
public sealed record WbaToolDto(string Key, string Name, string? Description);

/// <summary>
/// The instrument vocabulary, for the builder's "This tool is" picker and the curriculum item editor's tool list.
/// </summary>
/// <remarks>
/// A MediatR query rather than an <c>IActivityReferenceDataService</c> method: the admin pages already load their
/// reference data this way, and a new interface member would break the stub seven bUnit test classes share.
/// </remarks>
public sealed record GetWbaToolsQuery : IRequest<IReadOnlyList<WbaToolDto>>;

public sealed class GetWbaToolsQueryHandler : IRequestHandler<GetWbaToolsQuery, IReadOnlyList<WbaToolDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetWbaToolsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<WbaToolDto>> Handle(GetWbaToolsQuery request, CancellationToken cancellationToken)
        => await _dbContext.Set<WbaTool>()
            .AsNoTracking()
            .OrderBy(tool => tool.Name)
            .Select(tool => new WbaToolDto(tool.Key, tool.Name, tool.Description))
            .ToListAsync(cancellationToken);
}
