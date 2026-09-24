using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Application.Audit;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <remarks>
/// <c>Rationale</c> and <c>Conditions</c> are redacted from the audit summary. They are the
/// committee's written judgement of a named trainee's progression — the most sensitive prose the
/// product holds — and the audit log is read through a different, coarser gate than the review
/// itself. <c>ReviewId</c>, <c>Category</c> and the actor stay in the clear, which is what makes
/// the entry useful: who decided what, and when. (T101)
/// </remarks>
public sealed record RecordCommitteeDecisionCommand(
    int ReviewId,
    CommitteeDecisionCategory Category,
    [property: Redact] string Rationale,
    [property: Redact] string? Conditions,
    ClaimsPrincipal Principal) : IRequest<CommitteeReviewDetailDto>;

public sealed class RecordCommitteeDecisionCommandValidator : AbstractValidator<RecordCommitteeDecisionCommand>
{
    public RecordCommitteeDecisionCommandValidator()
    {
        RuleFor(command => command.ReviewId).GreaterThan(0);
        RuleFor(command => command.Rationale).NotEmpty().MaximumLength(4000);
        RuleFor(command => command.Conditions).MaximumLength(4000);
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class RecordCommitteeDecisionCommandHandler : IRequestHandler<RecordCommitteeDecisionCommand, CommitteeReviewDetailDto>
{
    private readonly IApplicationDbContext _dbContext;

    public RecordCommitteeDecisionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CommitteeReviewDetailDto> Handle(RecordCommitteeDecisionCommand request, CancellationToken cancellationToken)
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
        await CommitteeTraineeScope.DemandTraineeAtPanelInstitutionAsync(_dbContext, request.Principal, review, cancellationToken);

        review.RecordDecision(
            request.Category,
            request.Rationale,
            request.Conditions,
            CommitteeDecisionAuthorization.GetRequiredUserId(request.Principal),
            DateTime.UtcNow);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return review.ToDetailDto();
    }
}
