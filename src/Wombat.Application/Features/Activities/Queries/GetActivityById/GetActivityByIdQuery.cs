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
/// <c>Principal</c> is both the read gate and the input to the writable set. Returns null when the
/// activity does not exist OR the caller may not read it — one answer for both, so the id space
/// cannot be walked (T101).
/// </remarks>
public sealed record GetActivityByIdQuery(int ActivityId, ClaimsPrincipal Principal) : IRequest<ActivityDetailDto?>;

public sealed class GetActivityByIdQueryHandler : IRequestHandler<GetActivityByIdQuery, ActivityDetailDto?>
{
    private readonly IActivityService _activityService;

    public GetActivityByIdQueryHandler(IActivityService activityService)
    {
        _activityService = activityService;
    }

    public Task<ActivityDetailDto?> Handle(GetActivityByIdQuery request, CancellationToken cancellationToken)
        => _activityService.GetDetailAsync(request.ActivityId, request.Principal, cancellationToken);
}
