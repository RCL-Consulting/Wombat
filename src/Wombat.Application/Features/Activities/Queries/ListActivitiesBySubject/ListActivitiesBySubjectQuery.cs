using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;

namespace Wombat.Application.Features.Activities.Queries.ListActivitiesBySubject;

public sealed record ListActivitiesBySubjectQuery(string SubjectUserId) : IRequest<IReadOnlyList<ActivitySummaryDto>>;

public sealed class ListActivitiesBySubjectQueryHandler : IRequestHandler<ListActivitiesBySubjectQuery, IReadOnlyList<ActivitySummaryDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListActivitiesBySubjectQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ActivitySummaryDto>> Handle(ListActivitiesBySubjectQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Set<Activity>()
            .AsNoTracking()
            .Include(activity => activity.ActivityType)
            .Where(activity => activity.SubjectUserId == request.SubjectUserId)
            .OrderByDescending(activity => activity.UpdatedOn)
            .Select(activity => new ActivitySummaryDto(
                activity.Id,
                activity.ActivityTypeId,
                activity.ActivityType.Key,
                activity.ActivityType.Name,
                activity.SubjectUserId,
                activity.CurrentState,
                activity.CreatedOn,
                activity.UpdatedOn))
            .ToListAsync(cancellationToken);
    }
}
