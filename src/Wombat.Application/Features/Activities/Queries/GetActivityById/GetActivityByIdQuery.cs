using System.Security.Claims;
using MediatR;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Users;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;

namespace Wombat.Application.Features.Activities.Queries.GetActivityById;

/// <summary>
/// Loads one activity together with the calling actor's writable field set and available
/// transitions (T070), and the name of everyone in its history (T142).
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
    private readonly IUserAdministrationService _users;

    public GetActivityByIdQueryHandler(IActivityService activityService, IUserAdministrationService users)
    {
        _activityService = activityService;
        _users = users;
    }

    public async Task<ActivityDetailDto?> Handle(GetActivityByIdQuery request, CancellationToken cancellationToken)
    {
        var detail = await _activityService.GetDetailAsync(request.ActivityId, request.Principal, cancellationToken);
        if (detail is null)
        {
            // Nothing is looked up for an activity the caller may not read: the lookup is behind the read gate.
            return null;
        }

        // The history's Actor column, by name, in one lookup. Here rather than in ActivityService.Map, which create and
        // transition share and whose results no page shows as history.
        var transitions = detail.Activity.Transitions;
        var names = await UserDisplayNames.ResolveAsync(
            _users, transitions.Select(transition => transition.ActorUserId), cancellationToken);

        return detail with
        {
            Activity = detail.Activity with
            {
                Transitions = transitions
                    .Select(transition => transition with { ActorName = names.NameOf(transition.ActorUserId) })
                    .ToList()
            }
        };
    }
}
