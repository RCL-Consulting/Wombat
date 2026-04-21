using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record ListReviewsForTraineeQuery(string TraineeUserId) : IRequest<IReadOnlyList<CommitteeReviewListItemDto>>;

public sealed class ListReviewsForTraineeQueryHandler : IRequestHandler<ListReviewsForTraineeQuery, IReadOnlyList<CommitteeReviewListItemDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListReviewsForTraineeQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CommitteeReviewListItemDto>> Handle(ListReviewsForTraineeQuery request, CancellationToken cancellationToken)
    {
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
                review.IsFormative))
            .ToListAsync(cancellationToken);
    }
}
