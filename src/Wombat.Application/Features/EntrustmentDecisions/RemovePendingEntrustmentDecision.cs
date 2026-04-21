using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Application.Features.EntrustmentDecisions;

public sealed record RemovePendingEntrustmentDecisionCommand(
    int ReviewId,
    int PendingId,
    ClaimsPrincipal Principal) : IRequest<Unit>;

public sealed class RemovePendingEntrustmentDecisionCommandValidator : AbstractValidator<RemovePendingEntrustmentDecisionCommand>
{
    public RemovePendingEntrustmentDecisionCommandValidator()
    {
        RuleFor(command => command.ReviewId).GreaterThan(0);
        RuleFor(command => command.PendingId).GreaterThan(0);
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class RemovePendingEntrustmentDecisionCommandHandler
    : IRequestHandler<RemovePendingEntrustmentDecisionCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public RemovePendingEntrustmentDecisionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(RemovePendingEntrustmentDecisionCommand request, CancellationToken cancellationToken)
    {
        var review = await _dbContext.Set<CommitteeReview>()
            .Include(r => r.Panel)
                .ThenInclude(p => p.Members)
            .SingleOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken)
            ?? throw new InvalidOperationException("The committee review could not be found.");

        if (review.State is not CommitteeReviewState.InProgress and not CommitteeReviewState.Decided)
        {
            throw new InvalidOperationException("Pending entrustment decisions may only be removed on an in-progress or decided review.");
        }

        EntrustmentDecisionAuthorization.DemandChairAccess(request.Principal, review.Panel);

        var pending = await _dbContext.Set<PendingEntrustmentDecision>()
            .SingleOrDefaultAsync(p => p.Id == request.PendingId && p.ReviewId == request.ReviewId, cancellationToken)
            ?? throw new InvalidOperationException("The pending entrustment decision could not be found for this review.");

        _dbContext.Set<PendingEntrustmentDecision>().Remove(pending);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
