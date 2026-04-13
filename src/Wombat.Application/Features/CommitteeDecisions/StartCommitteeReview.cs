using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Activities;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record StartCommitteeReviewCommand(int ReviewId, ClaimsPrincipal Principal) : IRequest<CommitteeReviewDetailDto>;

public sealed class StartCommitteeReviewCommandValidator : AbstractValidator<StartCommitteeReviewCommand>
{
    public StartCommitteeReviewCommandValidator()
    {
        RuleFor(command => command.ReviewId).GreaterThan(0);
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class StartCommitteeReviewCommandHandler : IRequestHandler<StartCommitteeReviewCommand, CommitteeReviewDetailDto>
{
    private readonly IApplicationDbContext _dbContext;

    public StartCommitteeReviewCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CommitteeReviewDetailDto> Handle(StartCommitteeReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _dbContext.Set<CommitteeReview>()
            .Include(entity => entity.Panel)
                .ThenInclude(panel => panel.Members)
            .Include(entity => entity.Decisions)
            .Include(entity => entity.Appeals)
            .Include(entity => entity.EvidenceItems)
            .SingleOrDefaultAsync(entity => entity.Id == request.ReviewId, cancellationToken)
            ?? throw new InvalidOperationException("The committee review could not be found.");

        CommitteeDecisionAuthorization.DemandPanelAccess(request.Principal, review.Panel);

        var actorUserId = CommitteeDecisionAuthorization.GetRequiredUserId(request.Principal);
        var evidenceItems = await BuildEvidenceSnapshotAsync(review, cancellationToken);
        review.Start(evidenceItems, actorUserId, DateTime.UtcNow);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return review.ToDetailDto();
    }

    private async Task<IReadOnlyList<CommitteeEvidence>> BuildEvidenceSnapshotAsync(CommitteeReview review, CancellationToken cancellationToken)
    {
        var fromUtc = review.ReviewPeriodFrom.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtcExclusive = review.ReviewPeriodTo.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var activities = await _dbContext.Set<Activity>()
            .AsNoTracking()
            .Include(activity => activity.ActivityType)
            .Where(activity =>
                activity.SubjectUserId == review.TraineeUserId &&
                activity.CreatedOn >= fromUtc &&
                activity.CreatedOn < toUtcExclusive)
            .OrderByDescending(activity => activity.UpdatedOn)
            .ToListAsync(cancellationToken);

        var msfCampaigns = await _dbContext.Set<MsfCampaign>()
            .AsNoTracking()
            .Include(campaign => campaign.Template)
            .Include(campaign => campaign.Responses)
            .Where(campaign =>
                campaign.SubjectUserId == review.TraineeUserId &&
                campaign.ClosesOn >= review.ReviewPeriodFrom &&
                campaign.ClosesOn <= review.ReviewPeriodTo)
            .OrderByDescending(campaign => campaign.ClosesOn)
            .ToListAsync(cancellationToken);

        var activityEvidence = activities.Select(activity => new CommitteeEvidence
        {
            SourceType = CommitteeEvidenceSourceType.Activity,
            ActivityId = activity.Id,
            SourceLabel = $"{activity.ActivityType.Name} #{activity.Id}",
            Summary = $"State: {activity.CurrentState}; created {activity.CreatedOn:yyyy-MM-dd}; updated {activity.UpdatedOn:yyyy-MM-dd HH:mm} UTC.",
            SourceRecordedOn = activity.UpdatedOn
        });

        var msfEvidence = msfCampaigns.Select(campaign => new CommitteeEvidence
        {
            SourceType = CommitteeEvidenceSourceType.MsfCampaign,
            MsfCampaignId = campaign.Id,
            SourceLabel = $"{campaign.Template.Name} #{campaign.Id}",
            Summary = $"State: {campaign.State}; responses {campaign.Responses.Count}; closes {campaign.ClosesOn:yyyy-MM-dd}.",
            SourceRecordedOn = campaign.ReleasedOn ?? campaign.ClosedOn ?? campaign.OpenedOn ?? campaign.CreatedOn
        });

        return activityEvidence.Concat(msfEvidence).ToArray();
    }
}
