using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record ListReviewsForPanelQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<CommitteeReviewListItemDto>>;

public sealed class ListReviewsForPanelQueryHandler : IRequestHandler<ListReviewsForPanelQuery, IReadOnlyList<CommitteeReviewListItemDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListReviewsForPanelQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CommitteeReviewListItemDto>> Handle(ListReviewsForPanelQuery request, CancellationToken cancellationToken)
    {
        var userId = CommitteeDecisionAuthorization.GetRequiredUserId(request.Principal);

        return await _dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .Include(review => review.Panel)
                .ThenInclude(panel => panel.Members)
            .Include(review => review.Decisions)
            .Where(review => review.Panel.Members.Any(member => member.UserId == userId))
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
                review.RatifiedOn))
            .ToListAsync(cancellationToken);
    }
}
