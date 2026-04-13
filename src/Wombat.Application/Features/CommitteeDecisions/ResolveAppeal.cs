using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record ResolveAppealCommand(
    int ReviewId,
    CommitteeAppealOutcome Outcome,
    CommitteeDecisionCategory? RemittedCategory,
    string? RemittedRationale,
    string? RemittedConditions,
    ClaimsPrincipal Principal) : IRequest<CommitteeReviewDetailDto>;

public sealed class ResolveAppealCommandValidator : AbstractValidator<ResolveAppealCommand>
{
    public ResolveAppealCommandValidator()
    {
        RuleFor(command => command.ReviewId).GreaterThan(0);
        RuleFor(command => command.Principal).NotNull();
        When(command => command.Outcome == CommitteeAppealOutcome.Remitted, () =>
        {
            RuleFor(command => command.RemittedCategory).NotNull();
            RuleFor(command => command.RemittedRationale).NotEmpty().MaximumLength(4000);
        });
        RuleFor(command => command.RemittedConditions).MaximumLength(4000);
    }
}

public sealed class ResolveAppealCommandHandler : IRequestHandler<ResolveAppealCommand, CommitteeReviewDetailDto>
{
    private readonly IApplicationDbContext _dbContext;

    public ResolveAppealCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CommitteeReviewDetailDto> Handle(ResolveAppealCommand request, CancellationToken cancellationToken)
    {
        var review = await _dbContext.Set<CommitteeReview>()
            .Include(entity => entity.Panel)
                .ThenInclude(panel => panel.Members)
            .Include(entity => entity.Decisions)
            .Include(entity => entity.Appeals)
            .Include(entity => entity.EvidenceItems)
            .SingleOrDefaultAsync(entity => entity.Id == request.ReviewId, cancellationToken)
            ?? throw new InvalidOperationException("The committee review could not be found.");

        CommitteeDecisionAuthorization.DemandAppealResolverAccess(request.Principal, review.Panel);
        review.ResolveAppeal(
            request.Outcome,
            CommitteeDecisionAuthorization.GetRequiredUserId(request.Principal),
            DateTime.UtcNow,
            request.RemittedCategory,
            request.RemittedRationale,
            request.RemittedConditions);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return review.ToDetailDto();
    }
}
