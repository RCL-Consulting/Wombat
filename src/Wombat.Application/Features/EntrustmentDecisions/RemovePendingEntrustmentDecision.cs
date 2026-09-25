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
    private readonly IUserAdministrationService _users;

    public RemovePendingEntrustmentDecisionCommandHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<Unit> Handle(RemovePendingEntrustmentDecisionCommand request, CancellationToken cancellationToken)
    {
        var review = await _dbContext.Set<CommitteeReview>()
            .Include(r => r.Panel)
                .ThenInclude(p => p.Members)
            .Include(r => r.AgendaLines)
            .SingleOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        // Authorise first: an unknown review and one the caller does not chair get the one refusal, before the review's
        // state is said (T194 item 1, T131). And a chair who may sit at the review now (T256).
        (review, _) = await CommitteeDecisionAuthorization.DemandChairedReviewAsync(
            request.Principal, review, _users, cancellationToken);
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

        // The first mutation. A line the chair's staging added to the agenda goes with the decision that put it there
        // (T131 slice 4): staging the EPA again runs the routing check again. A cadence line stays, due.
        _dbContext.Set<PendingEntrustmentDecision>().Remove(pending);
        if (review.RemoveChairLineFor(pending.EpaId) is { } chairLine)
        {
            _dbContext.Set<CommitteeAgendaLine>().Remove(chairLine);
        }

        // The review is marked modified, though nothing about it changes, as staging marks it (T213): its xmin token
        // (CommitteeReviewConfiguration) is then checked in this save, which is refused whole when the review changed
        // after it was read above. Without it a remove that read the review in progress could delete a staged decision
        // after the committee's decision was recorded, and the STARs the panel recorded would not be the ones ratifying
        // issues (D46). A record, stage, deferral or ratify that reads the review before this commits is refused the same
        // way. A staged row already gone, ratified or removed, is refused by its own delete.
        _dbContext.Set<CommitteeReview>().Entry(review).Property(r => r.State).IsModified = true;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            // Carried as the inner exception, so the audit pipeline still sees a refused save and writes its row alone
            // (T201).
            throw new InvalidOperationException(ReviewChanged, exception);
        }

        return Unit.Value;
    }

    /// <summary>
    /// The refusal when the review, or the staged decision, changed between being read and the save. Nothing is removed.
    /// </summary>
    public const string ReviewChanged =
        "This review changed while the staged decision was being removed: " + CommitteeReviewChanged.WhatChanges + ". " +
        "Nothing was removed. Reload the review.";
}
