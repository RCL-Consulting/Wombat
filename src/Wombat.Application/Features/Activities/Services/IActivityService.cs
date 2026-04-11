using Wombat.Application.Features.Activities.Dtos;

namespace Wombat.Application.Features.Activities.Services;

public interface IActivityService
{
    Task<ActivityDto> CreateDraftAsync(CreateActivityInput input, CancellationToken cancellationToken = default);
    Task<ActivityDto> UpdateDraftAsync(UpdateActivityDraftInput input, CancellationToken cancellationToken = default);
    Task<ActivityDto> TransitionAsync(TransitionActivityInput input, CancellationToken cancellationToken = default);
    Task<ActivityDto> GetAsync(int activityId, CancellationToken cancellationToken = default);
}
