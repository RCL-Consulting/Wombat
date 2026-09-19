using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;

namespace Wombat.Application.Features.Activities.Queries.ListActivitiesBySubject;

/// <summary>
/// One trainee's activity list.
/// </summary>
/// <remarks>
/// <see cref="SubjectUserId" /> is whose list is being asked for; <see cref="Principal" /> is who is
/// asking. Both are needed: the query used to carry only the first, and since the subject id arrives
/// from the caller, anyone who could reach the handler could read any trainee's list by naming them.
/// (T101)
/// </remarks>
public sealed record ListActivitiesBySubjectQuery(string SubjectUserId, ClaimsPrincipal Principal)
    : IRequest<IReadOnlyList<ActivitySummaryDto>>;

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
            // Applied to the rows rather than to the subject, so that the answer to "may I see this
            // trainee?" is decided per activity by the stamps on it. A trainee who moved institution
            // keeps their old assessments out of their new overseers' sight, which is what the stamps
            // are for. Out of scope yields an empty list, not an error: this is a list, and a refusal
            // would confirm the trainee exists. (T101)
            .WhereReadableBy(request.Principal)
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
