using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record LodgeAppealCommand(int ReviewId, string Reason, ClaimsPrincipal Principal) : IRequest<CommitteeReviewDetailDto>;

public sealed class LodgeAppealCommandValidator : AbstractValidator<LodgeAppealCommand>
{
    public LodgeAppealCommandValidator()
    {
        RuleFor(command => command.ReviewId).GreaterThan(0);
        RuleFor(command => command.Reason).NotEmpty().MaximumLength(4000);
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class LodgeAppealCommandHandler : IRequestHandler<LodgeAppealCommand, CommitteeReviewDetailDto>
{
    private readonly IApplicationDbContext _dbContext;

    public LodgeAppealCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CommitteeReviewDetailDto> Handle(LodgeAppealCommand request, CancellationToken cancellationToken)
    {
        var review = await _dbContext.Set<CommitteeReview>()
            .Include(entity => entity.Panel)
                .ThenInclude(panel => panel.Members)
            .Include(entity => entity.Decisions)
            .Include(entity => entity.Appeals)
            .Include(entity => entity.EvidenceItems)
            .SingleOrDefaultAsync(entity => entity.Id == request.ReviewId, cancellationToken)
            ?? throw new InvalidOperationException("The committee review could not be found.");

        CommitteeDecisionAuthorization.DemandTraineeSelfAccess(request.Principal, review.TraineeUserId);
        review.LodgeAppeal(request.Reason, CommitteeDecisionAuthorization.GetRequiredUserId(request.Principal), DateTime.UtcNow);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return review.ToDetailDto();
    }
}
