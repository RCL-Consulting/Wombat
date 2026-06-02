using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Institutions;

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

        var panel = await _dbContext.Set<DecisionPanel>()
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == request.PanelId, cancellationToken)
            ?? throw new InvalidOperationException("The decision panel could not be found.");

        // An InstitutionalAdmin may only schedule on panels within their institution. The
        // panel's effective institution is its InstitutionId when Institution-scoped, or its
        // Speciality's InstitutionId when Speciality-scoped. (T075 / F-4A-1)
        if (request.Principal.IsInstitutionalAdmin() && !request.Principal.IsAdministrator())
        {
            var resolvedInstitutionId = await ResolveInstitutionIdAsync(panel, cancellationToken);
            if (resolvedInstitutionId.HasValue && !request.Principal.CanAccessInstitution(resolvedInstitutionId.Value))
            {
                throw new UnauthorizedAccessException("You can only schedule reviews for panels in your institution.");
            }
        }

        var review = new CommitteeReview
        {
            TraineeUserId = request.TraineeUserId.Trim(),
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

    private async Task<int?> ResolveInstitutionIdAsync(DecisionPanel panel, CancellationToken cancellationToken)
        => panel.Scope switch
        {
            DecisionPanelScope.Institution => panel.InstitutionId,
            DecisionPanelScope.Speciality when panel.SpecialityId.HasValue => await _dbContext.Set<Speciality>()
                .Where(speciality => speciality.Id == panel.SpecialityId.Value)
                .Select(speciality => (int?)speciality.InstitutionId)
                .SingleOrDefaultAsync(cancellationToken),
            _ => null
        };
}
