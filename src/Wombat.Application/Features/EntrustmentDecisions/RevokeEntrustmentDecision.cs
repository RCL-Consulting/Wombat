using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Application.Features.EntrustmentDecisions;

public sealed record RevokeEntrustmentDecisionCommand(
    int DecisionId,
    string Reason,
    ClaimsPrincipal Principal) : IRequest<EntrustmentDecisionDto>;

public sealed class RevokeEntrustmentDecisionCommandValidator : AbstractValidator<RevokeEntrustmentDecisionCommand>
{
    public RevokeEntrustmentDecisionCommandValidator()
    {
        RuleFor(command => command.DecisionId).GreaterThan(0);
        RuleFor(command => command.Reason).NotEmpty().MaximumLength(1000);
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class RevokeEntrustmentDecisionCommandHandler : IRequestHandler<RevokeEntrustmentDecisionCommand, EntrustmentDecisionDto>
{
    private readonly IApplicationDbContext _dbContext;

    public RevokeEntrustmentDecisionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EntrustmentDecisionDto> Handle(RevokeEntrustmentDecisionCommand request, CancellationToken cancellationToken)
    {
        var decision = await _dbContext.Set<EntrustmentDecision>()
            .Include(d => d.Epa)
            .Include(d => d.AuthorisedLevel)
            .Include(d => d.EvidenceLinks)
            .SingleOrDefaultAsync(d => d.Id == request.DecisionId, cancellationToken)
            ?? throw new InvalidOperationException("The entrustment decision could not be found.");

        var review = await _dbContext.Set<CommitteeReview>()
            .Include(entity => entity.Panel)
                .ThenInclude(panel => panel.Members)
            .SingleOrDefaultAsync(entity => entity.Id == decision.IssuedByCommitteeReviewId, cancellationToken)
            ?? throw new InvalidOperationException("The issuing committee review could not be found.");

        EntrustmentDecisionAuthorization.DemandRevocationAccess(request.Principal, review.Panel);
        var actorUserId = EntrustmentDecisionAuthorization.GetRequiredUserId(request.Principal);

        decision.Revoke(request.Reason, actorUserId, DateTime.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return decision.ToDto();
    }
}
