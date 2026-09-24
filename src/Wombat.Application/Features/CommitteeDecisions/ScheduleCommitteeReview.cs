using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record ScheduleCommitteeReviewCommand(
    string TraineeUserId,
    int PanelId,
    DateOnly ReviewPeriodFrom,
    DateOnly ReviewPeriodTo,
    DateOnly ScheduledOn,
    ClaimsPrincipal Principal,
    bool IsFormative = false,
    CommitteeReviewType ReviewType = CommitteeReviewType.AnnualProgression) : IRequest<CommitteeReviewListItemDto>;

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

        var traineeUserId = request.TraineeUserId.Trim();

        var panel = await _dbContext.Set<DecisionPanel>()
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == request.PanelId, cancellationToken);

        if (panel is null)
        {
            // Anyone but an Administrator is refused exactly as for another institution's panel, so that the refusal
            // does not say which panel ids exist. (T182)
            throw request.Principal.IsAdministrator()
                ? new InvalidOperationException("The decision panel could not be found.")
                : new UnauthorizedAccessException(CommitteeTraineeScope.NotSchedulable);
        }

        // The trainee must train at the panel's institution, and anyone but an Administrator must oversee them; before
        // T182 only an InstitutionalAdmin's panel was checked, and the trainee not at all. Nothing is written until this
        // passes.
        await CommitteeTraineeScope.DemandSchedulableAsync(
            _dbContext, request.Principal, panel, traineeUserId, cancellationToken);

        var review = new CommitteeReview
        {
            TraineeUserId = traineeUserId,
            PanelId = request.PanelId,
            ReviewPeriodFrom = request.ReviewPeriodFrom,
            ReviewPeriodTo = request.ReviewPeriodTo,
            ScheduledOn = request.ScheduledOn,
            IsFormative = request.IsFormative,
            ReviewType = request.ReviewType
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
            null,
            review.IsFormative,
            review.ReviewType);
    }
}
