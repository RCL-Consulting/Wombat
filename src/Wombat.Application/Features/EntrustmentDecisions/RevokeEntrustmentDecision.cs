using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Audit;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Application.Features.EntrustmentDecisions;

/// <remarks>
/// <c>Reason</c> is redacted from the audit summary. The AuditPipelineBehavior audits every request
/// whose type name ends in "Command" and AuditPayloadSerializer writes its properties into
/// SummaryJson; withdrawing an entrustment is the most damaging thing that can be written about a
/// trainee and the reason text usually says why — a concern, an incident, a performance judgement.
/// The decision id and the actor stay in the clear, so the audit row still proves who revoked what
/// and when; the narrative is read from the decision, under its own access control. (T101)
/// </remarks>
public sealed record RevokeEntrustmentDecisionCommand(
    int DecisionId,
    [property: Redact] string Reason,
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
