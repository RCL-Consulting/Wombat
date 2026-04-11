using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Activities.Services;

public interface ICreditApplier
{
    Task<IReadOnlyList<CurriculumItemProgress>> ApplyAsync(
        Activity completedActivity,
        ActivityType activityType,
        CancellationToken cancellationToken = default);
}
