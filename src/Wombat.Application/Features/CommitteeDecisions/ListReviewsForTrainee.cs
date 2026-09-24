using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// One trainee's committee reviews in the states a trainee may see (ratified, under appeal, final), for the trainee
/// themselves or anyone who may read about them (<see cref="TraineeScopeResolver.MayReadAsync" />); empty for anyone
/// else, never a refusal. Until T113 it answered on the caller-supplied id alone.
/// </summary>
public sealed record ListReviewsForTraineeQuery(string TraineeUserId, ClaimsPrincipal Principal)
    : IRequest<IReadOnlyList<CommitteeReviewListItemDto>>;

public sealed class ListReviewsForTraineeQueryHandler : IRequestHandler<ListReviewsForTraineeQuery, IReadOnlyList<CommitteeReviewListItemDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListReviewsForTraineeQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CommitteeReviewListItemDto>> Handle(ListReviewsForTraineeQuery request, CancellationToken cancellationToken)
    {
        if (!await TraineeScopeResolver.MayReadAsync(_dbContext, request.Principal, request.TraineeUserId, cancellationToken))
        {
            return [];
        }

        return await _dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .Include(review => review.Panel)
            .Include(review => review.Decisions)
            .Where(review =>
                review.TraineeUserId == request.TraineeUserId &&
                (review.State == CommitteeReviewState.Ratified ||
                 review.State == CommitteeReviewState.UnderAppeal ||
                 review.State == CommitteeReviewState.Final))
            .OrderByDescending(review => review.ScheduledOn)
            .Select(review => new CommitteeReviewListItemDto(
                review.Id,
                review.TraineeUserId,
                review.PanelId,
                review.Panel.Name,
                review.ReviewPeriodFrom,
                review.ReviewPeriodTo,
                review.ScheduledOn,
                review.State,
                review.Decisions.OrderByDescending(decision => decision.DecidedOn).Select(decision => (CommitteeDecisionCategory?)decision.Category).FirstOrDefault(),
                review.RatifiedOn,
                review.IsFormative,
                review.ReviewType)
            {
                AcademicYear = review.AcademicYear,
                Semester = review.Semester
            })
            .ToListAsync(cancellationToken);
    }
}
