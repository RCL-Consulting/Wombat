using MediatR;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;

namespace Wombat.Application.Features.Activities.Queries.GetActivityById;

public sealed record GetActivityByIdQuery(int ActivityId) : IRequest<ActivityDto>;

public sealed class GetActivityByIdQueryHandler : IRequestHandler<GetActivityByIdQuery, ActivityDto>
{
    private readonly IActivityService _activityService;

    public GetActivityByIdQueryHandler(IActivityService activityService)
    {
        _activityService = activityService;
    }

    public Task<ActivityDto> Handle(GetActivityByIdQuery request, CancellationToken cancellationToken)
        => _activityService.GetAsync(request.ActivityId, cancellationToken);
}
