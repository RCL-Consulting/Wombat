using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Activities.Queries.ListActivityTypesAdmin;

public sealed record ListActivityTypesAdminQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<ActivityTypeAdminListItemDto>>;

public sealed class ListActivityTypesAdminQueryHandler : IRequestHandler<ListActivityTypesAdminQuery, IReadOnlyList<ActivityTypeAdminListItemDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListActivityTypesAdminQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ActivityTypeAdminListItemDto>> Handle(ListActivityTypesAdminQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Set<ActivityType>().AsNoTracking();

        if (!request.Principal.IsAdministrator())
        {
            // Speciality/sub-speciality scopes are now national (College-owned) disciplines (T091), so a
            // non-admin sees the global + all national discipline types, plus their own institution's types.
            // Adoption-based narrowing of national types arrives in phase 4.
            var scopedInstitutionId = request.Principal.GetInstitutionId();
            query = query.Where(entity =>
                entity.Scope == ActivityScope.Global ||
                (entity.Scope == ActivityScope.Institution && scopedInstitutionId != null && entity.ScopeId == scopedInstitutionId.Value) ||
                entity.Scope == ActivityScope.Speciality ||
                entity.Scope == ActivityScope.SubSpeciality);
        }

        return await query
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
