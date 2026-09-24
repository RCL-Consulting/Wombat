using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.CommitteeDecisions;
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
            .SingleOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        // Authorise first: an unknown review and one the caller does not chair get the one refusal, before the review's
        // state is said (T194 item 1, T131).
        review = CommitteeDecisionAuthorization.DemandChairedReview(request.Principal, review);
        await CommitteeTraineeScope.DemandTraineeAtPanelInstitutionAsync(_dbContext, request.Principal, review, cancellationToken);

        if (review.State is not CommitteeReviewState.InProgress and not CommitteeReviewState.Decided)
        {
            throw new InvalidOperationException(StagedStars.FixedWhenDecided);
        }

        var pending = await _dbContext.Set<PendingEntrustmentDecision>()
            .SingleOrDefaultAsync(p => p.Id == request.PendingId && p.ReviewId == request.ReviewId, cancellationToken)
            ?? throw new InvalidOperationException("The pending entrustment decision could not be found for this review.");

        // T165: once the decision is recorded, the staged STARs are the committee's and fixed with it. The one exception is
        // a STAR ratifying would refuse because it no longer fits the trainee's curriculum (T167): it can never be issued,
        // and while it is staged the review cannot be ratified at all. Judged by the rule ratifying refuses by, and before
        // anything is removed.
        if (review.State == CommitteeReviewState.Decided &&
            !(await StarCurriculum.RefusalsForStagedAsync(_dbContext, review.TraineeUserId, [pending], cancellationToken))
                .ContainsKey(pending.Id))
        {
            throw new InvalidOperationException(StagedStars.FixedWhenDecided);
        }

        _dbContext.Set<PendingEntrustmentDecision>().Remove(pending);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            // The row was gone by the save: a ratify issued it, or another remove took it. Carried as the inner
            // exception, so the audit pipeline still sees a refused save and writes its row alone (T201).
            throw new InvalidOperationException(AlreadyGone, exception);
        }

        return Unit.Value;
    }

    /// <summary>The refusal when the staged decision was ratified or removed between being read and the save.</summary>
    public const string AlreadyGone =
        "This staged decision was ratified or removed while it was being removed. Nothing was changed. Reload the review.";
}
