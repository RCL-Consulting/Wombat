using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.EntrustmentDecisions;

public sealed record IssueEntrustmentDecisionCommand(
    string TraineeUserId,
    int EpaId,
    int AuthorisedLevelId,
    DateOnly IssuedOn,
    DateOnly? ExpiresOn,
    int CommitteeReviewId,
    string Rationale,
    IReadOnlyList<EntrustmentEvidenceLinkInput> EvidenceLinks,
    ClaimsPrincipal Principal) : IRequest<EntrustmentDecisionDto>;

public sealed class IssueEntrustmentDecisionCommandValidator : AbstractValidator<IssueEntrustmentDecisionCommand>
{
    public IssueEntrustmentDecisionCommandValidator()
    {
        RuleFor(command => command.TraineeUserId).NotEmpty();
        RuleFor(command => command.EpaId).GreaterThan(0);
        RuleFor(command => command.AuthorisedLevelId).GreaterThan(0);
        RuleFor(command => command.CommitteeReviewId).GreaterThan(0);
        RuleFor(command => command.Rationale).NotEmpty().MaximumLength(4000);
        RuleFor(command => command.Principal).NotNull();
        RuleFor(command => command)
            .Must(command => !command.ExpiresOn.HasValue || command.ExpiresOn.Value > command.IssuedOn)
            .WithMessage("An expiry date must be after the issue date.");
        RuleForEach(command => command.EvidenceLinks).ChildRules(link =>
        {
            link.RuleFor(l => l.SourceLabel).NotEmpty().MaximumLength(200);
            link.RuleFor(l => l.Summary).MaximumLength(2000);
        });
    }
}

public sealed class IssueEntrustmentDecisionCommandHandler : IRequestHandler<IssueEntrustmentDecisionCommand, EntrustmentDecisionDto>
{
    private readonly IApplicationDbContext _dbContext;

    public IssueEntrustmentDecisionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EntrustmentDecisionDto> Handle(IssueEntrustmentDecisionCommand request, CancellationToken cancellationToken)
    {
        var review = await _dbContext.Set<CommitteeReview>()
            .Include(entity => entity.Panel)
                .ThenInclude(panel => panel.Members)
            .SingleOrDefaultAsync(entity => entity.Id == request.CommitteeReviewId, cancellationToken)
            ?? throw new InvalidOperationException("The committee review could not be found.");

        if (review.State is not CommitteeReviewState.Ratified and not CommitteeReviewState.Final)
        {
            throw new InvalidOperationException("Entrustment decisions may only be issued against a ratified committee review.");
        }

        if (!string.Equals(review.TraineeUserId, request.TraineeUserId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The entrustment decision trainee does not match the committee review subject.");
        }

        EntrustmentDecisionAuthorization.DemandChairAccess(request.Principal, review.Panel);
        var chairUserId = EntrustmentDecisionAuthorization.GetRequiredUserId(request.Principal);

        var epa = await _dbContext.Set<Epa>().SingleOrDefaultAsync(e => e.Id == request.EpaId, cancellationToken)
            ?? throw new InvalidOperationException("The specified EPA could not be found.");
        _ = await _dbContext.Set<EntrustmentLevel>().SingleOrDefaultAsync(l => l.Id == request.AuthorisedLevelId, cancellationToken)
            ?? throw new InvalidOperationException("The specified entrustment level could not be found.");

        var evidenceLinks = (request.EvidenceLinks ?? Array.Empty<EntrustmentEvidenceLinkInput>())
            .Select(link => EntrustmentEvidenceLink.Create(
                link.SourceType,
                link.ActivityId,
                link.MsfCampaignId,
                link.CommitteeReviewId,
                link.SourceLabel,
                link.Summary ?? string.Empty,
                link.SourceRecordedOn))
            .ToList();

        var decision = EntrustmentDecision.Issue(
            request.TraineeUserId,
            request.EpaId,
            request.AuthorisedLevelId,
            request.IssuedOn,
            request.ExpiresOn,
            request.CommitteeReviewId,
            chairUserId,
            request.Rationale,
            evidenceLinks);

        var priorActive = await _dbContext.Set<EntrustmentDecision>()
            .Where(d => d.TraineeUserId == request.TraineeUserId
                && d.EpaId == request.EpaId
                && d.Status == EntrustmentDecisionStatus.Active)
            .ToListAsync(cancellationToken);

        _dbContext.Set<EntrustmentDecision>().Add(decision);
        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var prior in priorActive)
        {
            prior.SupersedeBy(decision.Id);
        }

        if (priorActive.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var stored = await _dbContext.Set<EntrustmentDecision>()
            .Include(d => d.Epa)
            .Include(d => d.AuthorisedLevel)
            .Include(d => d.EvidenceLinks)
            .SingleAsync(d => d.Id == decision.Id, cancellationToken);

        return stored.ToDto();
    }
}
