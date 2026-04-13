using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record GetCommitteeReviewByIdQuery(int ReviewId, ClaimsPrincipal Principal) : IRequest<CommitteeReviewDetailDto>;

public sealed class GetCommitteeReviewByIdQueryHandler : IRequestHandler<GetCommitteeReviewByIdQuery, CommitteeReviewDetailDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCommitteeReviewByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CommitteeReviewDetailDto> Handle(GetCommitteeReviewByIdQuery request, CancellationToken cancellationToken)
    {
        var review = await _dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .Include(entity => entity.Panel)
                .ThenInclude(panel => panel.Members)
            .Include(entity => entity.Decisions)
            .Include(entity => entity.Appeals)
            .Include(entity => entity.EvidenceItems)
            .SingleOrDefaultAsync(entity => entity.Id == request.ReviewId, cancellationToken)
            ?? throw new InvalidOperationException("The committee review could not be found.");

        if (request.Principal.IsInRole(WombatRoles.Trainee))
        {
            var userId = CommitteeDecisionAuthorization.GetRequiredUserId(request.Principal);
            if (!string.Equals(review.TraineeUserId, userId, StringComparison.Ordinal))
            {
                throw new UnauthorizedAccessException("You can only view your own committee reviews.");
            }

            if (review.State is not (CommitteeReviewState.Ratified or CommitteeReviewState.UnderAppeal or CommitteeReviewState.Final))
            {
                throw new UnauthorizedAccessException("This review is not yet visible to the trainee.");
            }
        }
        else if (!request.Principal.IsInRole(WombatRoles.Administrator) && !request.Principal.IsInRole(WombatRoles.Coordinator))
        {
            CommitteeDecisionAuthorization.DemandPanelAccess(request.Principal, review.Panel);
        }

        return review.ToDetailDto();
    }
}
