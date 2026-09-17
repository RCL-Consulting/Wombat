using System.Security.Claims;
using Wombat.Application.Features.Activities.Dtos;

namespace Wombat.Application.Features.Activities.Services;

public interface IActivityService
{
    Task<ActivityDto> CreateDraftAsync(CreateActivityInput input, CancellationToken cancellationToken = default);
    Task<ActivityDto> UpdateDraftAsync(UpdateActivityDraftInput input, CancellationToken cancellationToken = default);
    Task<ActivityDto> TransitionAsync(TransitionActivityInput input, CancellationToken cancellationToken = default);
    Task<ActivityDto> GetAsync(int activityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// The activity plus this actor's writable field set and available transitions (T070).
    /// </summary>
    /// <remarks>
    /// The principal is used ONLY to compute the writable set and the action list. This call performs
    /// no read authorization — any authenticated user who knows an activity id may load it, exactly as
    /// before. Read scoping is T101.
    /// </remarks>
    Task<ActivityDetailDto> GetDetailAsync(int activityId, ClaimsPrincipal principal, CancellationToken cancellationToken = default);
}
