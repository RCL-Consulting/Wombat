using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;

namespace Wombat.Application.Features.Activities.Queries.ListActivityTypes;

public sealed record ListActivityTypesQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<ActivityTypeListItemDto>>;

public sealed class ListActivityTypesQueryHandler : IRequestHandler<ListActivityTypesQuery, IReadOnlyList<ActivityTypeListItemDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListActivityTypesQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ActivityTypeListItemDto>> Handle(ListActivityTypesQuery request, CancellationToken cancellationToken)
    {
        var institutionId = request.Principal.GetInstitutionId();
        var specialityIds = request.Principal.GetSpecialityIds();
        var subSpecialityIds = request.Principal.GetSubSpecialityIds();

        return await _dbContext.Set<ActivityType>()
            .AsNoTracking()
            .Where(activityType =>
                activityType.Scope == ActivityScope.Global ||
                (activityType.Scope == ActivityScope.Institution && institutionId.HasValue && activityType.ScopeId == institutionId.Value) ||
                (activityType.Scope == ActivityScope.Speciality && specialityIds.Contains(activityType.ScopeId ?? 0)) ||
                (activityType.Scope == ActivityScope.SubSpeciality && subSpecialityIds.Contains(activityType.ScopeId ?? 0)))
            .OrderBy(activityType => activityType.Name)
            .Select(activityType => new ActivityTypeListItemDto(
                activityType.Id,
                activityType.Key,
                activityType.Name,
                activityType.Scope,
                activityType.ScopeId,
                activityType.Version,
                activityType.IsActive))
            .ToListAsync(cancellationToken);
    }
}
