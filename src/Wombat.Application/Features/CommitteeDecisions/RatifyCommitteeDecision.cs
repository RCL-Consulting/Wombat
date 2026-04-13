using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record RatifyCommitteeDecisionCommand(int ReviewId, ClaimsPrincipal Principal) : IRequest<CommitteeReviewDetailDto>;

public sealed class RatifyCommitteeDecisionCommandValidator : AbstractValidator<RatifyCommitteeDecisionCommand>
{
    public RatifyCommitteeDecisionCommandValidator()
    {
        RuleFor(command => command.ReviewId).GreaterThan(0);
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class RatifyCommitteeDecisionCommandHandler : IRequestHandler<RatifyCommitteeDecisionCommand, CommitteeReviewDetailDto>
{
    private readonly IApplicationDbContext _dbContext;

    public RatifyCommitteeDecisionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CommitteeReviewDetailDto> Handle(RatifyCommitteeDecisionCommand request, CancellationToken cancellationToken)
    {
        var review = await _dbContext.Set<CommitteeReview>()
            .Include(entity => entity.Panel)
                .ThenInclude(panel => panel.Members)
            .Include(entity => entity.Decisions)
            .Include(entity => entity.Appeals)
            .Include(entity => entity.EvidenceItems)
            .SingleOrDefaultAsync(entity => entity.Id == request.ReviewId, cancellationToken)
            ?? throw new InvalidOperationException("The committee review could not be found.");

        CommitteeDecisionAuthorization.DemandChairAccess(request.Principal, review.Panel);
        review.Ratify(CommitteeDecisionAuthorization.GetRequiredUserId(request.Principal), DateTime.UtcNow);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return review.ToDetailDto();
    }
}
