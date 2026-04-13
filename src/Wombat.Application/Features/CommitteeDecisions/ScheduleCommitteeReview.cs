using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record ScheduleCommitteeReviewCommand(
    string TraineeUserId,
    int PanelId,
    DateOnly ReviewPeriodFrom,
    DateOnly ReviewPeriodTo,
    DateOnly ScheduledOn,
    ClaimsPrincipal Principal) : IRequest<CommitteeReviewListItemDto>;

public sealed class ScheduleCommitteeReviewCommandValidator : AbstractValidator<ScheduleCommitteeReviewCommand>
{
    public ScheduleCommitteeReviewCommandValidator()
    {
        RuleFor(command => command.TraineeUserId).NotEmpty();
        RuleFor(command => command.PanelId).GreaterThan(0);
        RuleFor(command => command.Principal).NotNull();
        RuleFor(command => command.ReviewPeriodTo).GreaterThanOrEqualTo(command => command.ReviewPeriodFrom);
    }
}

public sealed class ScheduleCommitteeReviewCommandHandler : IRequestHandler<ScheduleCommitteeReviewCommand, CommitteeReviewListItemDto>
{
    private readonly IApplicationDbContext _dbContext;

    public ScheduleCommitteeReviewCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CommitteeReviewListItemDto> Handle(ScheduleCommitteeReviewCommand request, CancellationToken cancellationToken)
    {
        CommitteeDecisionAuthorization.DemandReviewScheduling(request.Principal);

        var panel = await _dbContext.Set<DecisionPanel>()
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == request.PanelId, cancellationToken)
            ?? throw new InvalidOperationException("The decision panel could not be found.");

        var review = new CommitteeReview
        {
            TraineeUserId = request.TraineeUserId.Trim(),
            PanelId = request.PanelId,
            ReviewPeriodFrom = request.ReviewPeriodFrom,
            ReviewPeriodTo = request.ReviewPeriodTo,
            ScheduledOn = request.ScheduledOn
        };

        _dbContext.Set<CommitteeReview>().Add(review);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CommitteeReviewListItemDto(
            review.Id,
            review.TraineeUserId,
            review.PanelId,
            panel.Name,
            review.ReviewPeriodFrom,
            review.ReviewPeriodTo,
            review.ScheduledOn,
            review.State,
            null,
            null);
    }
}
