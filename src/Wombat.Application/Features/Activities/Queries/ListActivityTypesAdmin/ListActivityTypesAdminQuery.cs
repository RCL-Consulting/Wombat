using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;

namespace Wombat.Application.Features.Activities.Queries.ListActivityTypesAdmin;

public sealed record ListActivityTypesAdminQuery() : IRequest<IReadOnlyList<ActivityTypeAdminListItemDto>>;

public sealed class ListActivityTypesAdminQueryHandler : IRequestHandler<ListActivityTypesAdminQuery, IReadOnlyList<ActivityTypeAdminListItemDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListActivityTypesAdminQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ActivityTypeAdminListItemDto>> Handle(ListActivityTypesAdminQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Set<ActivityType>()
            .AsNoTracking()
            .OrderBy(entity => entity.Name)
            .Select(entity => new ActivityTypeAdminListItemDto(
                entity.Id,
                entity.Key,
                entity.Name,
                entity.Description,
                entity.Scope,
                entity.ScopeId,
                entity.Version,
                entity.IsActive,
                entity.StagingSchemaJson != null && entity.StagingWorkflowJson != null && entity.StagingCreditRulesJson != null,
                entity.StagingUpdatedOn))
            .ToListAsync(cancellationToken);
    }
}
