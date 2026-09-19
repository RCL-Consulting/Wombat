using System.Security.Claims;
using Wombat.Application.Features.Activities.Dtos;

namespace Wombat.Application.Features.Activities.Services;

public interface IActivityService
{
    Task<ActivityDto> CreateDraftAsync(CreateActivityInput input, CancellationToken cancellationToken = default);
    Task<ActivityDto> UpdateDraftAsync(UpdateActivityDraftInput input, CancellationToken cancellationToken = default);
    Task<ActivityDto> TransitionAsync(TransitionActivityInput input, CancellationToken cancellationToken = default);

    /// <summary>
    /// The activity plus this actor's writable field set and available transitions (T070), or null
    /// when this actor may not read it (T101).
    /// </summary>
    /// <remarks>
    /// Null means either "no such activity" or "not yours" — deliberately the same answer, so that
    /// walking the id space discloses nothing. Callers must render it as not-found and must not
    /// translate it into a message that distinguishes the two cases.
    /// </remarks>
    Task<ActivityDetailDto?> GetDetailAsync(int activityId, ClaimsPrincipal principal, CancellationToken cancellationToken = default);
}
