using System.Security.Claims;
using MediatR;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;

namespace Wombat.Application.Features.Activities.Queries.GetActivityById;

/// <summary>
/// Loads one activity together with the calling actor's writable field set and available
/// transitions (T070).
/// </summary>
/// <remarks>
/// IMPORTANT: <c>Principal</c> is used ONLY to compute the writable set and the action
/// list. This query still performs NO read authorization — any authenticated user who knows an
/// activity id can load it, exactly as before T070. Do not read the ClaimsPrincipal here as a gate
/// that is not there; read scoping is T101.
/// </remarks>
public sealed record GetActivityByIdQuery(int ActivityId, ClaimsPrincipal Principal) : IRequest<ActivityDetailDto>;

public sealed class GetActivityByIdQueryHandler : IRequestHandler<GetActivityByIdQuery, ActivityDetailDto>
{
    private readonly IActivityService _activityService;

    public GetActivityByIdQueryHandler(IActivityService activityService)
    {
        _activityService = activityService;
    }

    public Task<ActivityDetailDto> Handle(GetActivityByIdQuery request, CancellationToken cancellationToken)
        => _activityService.GetDetailAsync(request.ActivityId, request.Principal, cancellationToken);
}
