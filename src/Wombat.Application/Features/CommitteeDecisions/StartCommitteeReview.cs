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
        // A review period is a clinical period: the panel is judging what the trainee DID between these
        // dates, so evidence falls in the window by its encounter date, not by when the form was filed.
        // An encounter in March that reached the system in September belongs to the March review. (T119,
        // decision D2 — and note this is a visible change to what a panel is shown.)
        //
        // Both bounds are DateOnly against a DateOnly column, inclusive at each end, which is what the
        // AddDays(1)-exclusive instant was emulating. The MSF window below already compared this way.
        var fromDate = review.ReviewPeriodFrom;
        var toDate = review.ReviewPeriodTo;

        var activities = await _dbContext.Set<Activity>()
            .AsNoTracking()
            .Include(activity => activity.ActivityType)
            .Where(activity =>
                activity.SubjectUserId == review.TraineeUserId &&
                activity.ObservedOn >= fromDate &&
                activity.ObservedOn <= toDate)
            .OrderByDescending(activity => activity.UpdatedOn)
            .ToListAsync(cancellationToken);

        var msfCampaigns = await _dbContext.Set<MsfCampaign>()
            .AsNoTracking()
            .Include(campaign => campaign.Template)
            .Include(campaign => campaign.Responses)
            // T121: so the campaign row can say what its evidence activities are FOR. A released
            // campaign now contributes both this row and one Activity row per covered EPA, and without
            // the coverage on the parent a panel sees one campaign and N unexplained siblings dated the
            // same minute. They are not duplicates; they are a report and its per-EPA claims, and the
            // summary below is where that is said.
            .Include(campaign => campaign.CoveredEpas)
                .ThenInclude(covered => covered.Epa)
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
            Summary = $"State: {campaign.State}; responses {campaign.Responses.Count}; " +
                      $"closes {campaign.ClosesOn:yyyy-MM-dd}.{DescribeCoverage(campaign)}",
            SourceRecordedOn = campaign.ReleasedOn ?? campaign.ClosedOn ?? campaign.OpenedOn ?? campaign.CreatedOn
        });

        return activityEvidence.Concat(msfEvidence).ToArray();
    }

    /// <summary>
    /// What a campaign declared itself evidence for, and whether that reached the portfolio. (T121)
    /// </summary>
    /// <remarks>
    /// A released campaign appears in this snapshot twice over: once here, and once per covered EPA as
    /// an ordinary <c>msf_cpsa</c> activity. That is deliberate — the panel wants both the report and
    /// the per-EPA claims — but it is only readable if the parent names the children, which is what this
    /// sentence does. An empty coverage set is worth printing too: it is the one case where a released
    /// campaign left no evidence at all.
    /// </remarks>
    private static string DescribeCoverage(MsfCampaign campaign)
    {
        var covered = campaign.CoveredEpas
            .Where(entry => entry.Epa is not null)
            .OrderBy(entry => entry.Epa.Code, StringComparer.Ordinal)
            .ToArray();

        if (covered.Length == 0)
        {
            return campaign.State == MsfCampaignState.Released
                ? " Names no EPA, so it recorded no evidence."
                : string.Empty;
        }

        if (campaign.State != MsfCampaignState.Released)
        {
            return $" Evidence for {string.Join(", ", covered.Select(entry => entry.Epa.Code))}, to be recorded on release.";
        }

        // Per EPA, never per campaign. A release records evidence for the EPAs still on the trainee's
        // curriculum and drops the rest, so a campaign can be half recorded - and a panel told
        // "recorded as one activity each" about an EPA whose activity was never written would go
        // looking for a record that does not exist. Released with nothing recorded is terminal, not
        // pending: Release refuses a second attempt.
        var recorded = covered.Where(entry => entry.RecordedOn is not null).Select(entry => entry.Epa.Code).ToArray();
        var missing = covered.Where(entry => entry.RecordedOn is null).Select(entry => entry.Epa.Code).ToArray();

        var sentence = recorded.Length > 0
            ? $" Evidence recorded for {string.Join(", ", recorded)}, one activity each."
            : string.Empty;

        return missing.Length > 0
            ? sentence + $" Also declared {string.Join(", ", missing)}, no longer on the trainee's " +
              "curriculum, so nothing was recorded for those."
            : sentence;
    }
}
