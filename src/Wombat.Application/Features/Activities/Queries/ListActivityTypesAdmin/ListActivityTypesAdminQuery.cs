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
            var scopedInstitutionId = request.Principal.GetInstitutionId();
            if (!scopedInstitutionId.HasValue)
            {
                return Array.Empty<ActivityTypeAdminListItemDto>();
            }

            // Materialise the ids of specialities and sub-specialities that belong to the caller's
            // institution so the EF query stays a single SELECT with IN(...) clauses.
            var allowedSpecialityIds = await _dbContext.Set<Speciality>()
                .Where(entity => entity.InstitutionId == scopedInstitutionId.Value)
                .Select(entity => entity.Id)
                .ToListAsync(cancellationToken);

            var allowedSubSpecialityIds = await _dbContext.Set<SubSpeciality>()
                .Where(entity => entity.Speciality.InstitutionId == scopedInstitutionId.Value)
                .Select(entity => entity.Id)
                .ToListAsync(cancellationToken);

            var institutionId = scopedInstitutionId.Value;
            query = query.Where(entity =>
                entity.Scope == ActivityScope.Global ||
                (entity.Scope == ActivityScope.Institution && entity.ScopeId == institutionId) ||
                (entity.Scope == ActivityScope.Speciality && entity.ScopeId != null && allowedSpecialityIds.Contains(entity.ScopeId.Value)) ||
                (entity.Scope == ActivityScope.SubSpeciality && entity.ScopeId != null && allowedSubSpecialityIds.Contains(entity.ScopeId.Value)));
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
