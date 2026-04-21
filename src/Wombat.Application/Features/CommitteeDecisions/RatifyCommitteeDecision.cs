using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;

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
        var chairUserId = CommitteeDecisionAuthorization.GetRequiredUserId(request.Principal);
        var utcNow = DateTime.UtcNow;

        review.Ratify(chairUserId, utcNow);

        var pending = await _dbContext.Set<PendingEntrustmentDecision>()
            .Where(p => p.ReviewId == review.Id)
            .ToListAsync(cancellationToken);

        if (pending.Count > 0)
        {
            await IssuePendingDecisionsAsync(review, pending, chairUserId, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return review.ToDetailDto();
    }

    private async Task IssuePendingDecisionsAsync(
        CommitteeReview review,
        IReadOnlyList<PendingEntrustmentDecision> pending,
        string chairUserId,
        CancellationToken cancellationToken)
    {
        foreach (var pendingDecision in pending)
        {
            var evidenceInputs = EntrustmentDecisionMappings.DeserializeEvidenceLinks(pendingDecision.EvidenceLinksJson);
            var evidenceLinks = evidenceInputs
                .Select(input => EntrustmentEvidenceLink.Create(
                    input.SourceType,
                    input.ActivityId,
                    input.MsfCampaignId,
                    input.CommitteeReviewId,
                    input.SourceLabel,
                    input.Summary ?? string.Empty,
                    input.SourceRecordedOn))
                .ToList();

            var decision = EntrustmentDecision.Issue(
                review.TraineeUserId,
                pendingDecision.EpaId,
                pendingDecision.AuthorisedLevelId,
                pendingDecision.IssuedOn,
                pendingDecision.ExpiresOn,
                review.Id,
                chairUserId,
                pendingDecision.Rationale,
                evidenceLinks);

            _dbContext.Set<EntrustmentDecision>().Add(decision);
            _dbContext.Set<PendingEntrustmentDecision>().Remove(pendingDecision);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var pendingDecision in pending)
        {
            var issued = await _dbContext.Set<EntrustmentDecision>()
                .Where(d => d.IssuedByCommitteeReviewId == review.Id
                    && d.EpaId == pendingDecision.EpaId
                    && d.TraineeUserId == review.TraineeUserId
                    && d.IssuedOn == pendingDecision.IssuedOn
                    && d.Status == EntrustmentDecisionStatus.Active)
                .OrderByDescending(d => d.Id)
                .FirstAsync(cancellationToken);

            var priorActive = await _dbContext.Set<EntrustmentDecision>()
                .Where(d => d.TraineeUserId == review.TraineeUserId
                    && d.EpaId == pendingDecision.EpaId
                    && d.Status == EntrustmentDecisionStatus.Active
                    && d.Id != issued.Id)
                .ToListAsync(cancellationToken);

            foreach (var prior in priorActive)
            {
                prior.SupersedeBy(issued.Id);
            }
        }
    }
}
