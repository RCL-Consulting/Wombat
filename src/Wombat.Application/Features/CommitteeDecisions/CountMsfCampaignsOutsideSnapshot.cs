using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// How many of the review's trainee's MSF campaigns closed inside its window and are not in its evidence snapshot:
/// those still awaiting release, and, once the review has started, those released after it did. (T173)
/// </summary>
/// <remarks>
/// <para>
/// The evidence snapshot takes released campaigns only (T138), and freezes at Start. A campaign that closed in the
/// window but was still under review at Start then reaches no snapshot: not this one, because it was unreleased, and
/// not the next, because its close date is in this window. Releasing it later does not change that. These counts are
/// how a panel learns of it. They are live, recomputed on every load, and they are not evidence: numbers, with no
/// campaign, respondent or response in them.
/// </para>
/// <para>
/// The rows are the ones the snapshot would take were they released: the same trainee and the same UTC close-day
/// window (<see cref="MsfCampaignReviewWindow" />). Of those, a campaign in
/// <see cref="MsfCampaignState.UnderReview" /> (the one state <c>MsfCampaign.Release</c> accepts) is awaiting release,
/// and a <see cref="MsfCampaignState.Released" /> one on a started review is outside the snapshot when no evidence item
/// of this review names it. Before Start a released campaign is not counted: Start will take it. A withdrawn campaign
/// is not awaiting anything and was never evidence.
/// </para>
/// <para>
/// Scoped exactly as the snapshot is. The readers are the snapshot's: the review ladder
/// (<see cref="CommitteeDecisionAuthorization.DemandReviewAccess" />) that the review and its other sibling queries on
/// the page climb, which refuses anyone with no claim on the review. The trainee is taken from the review, never from
/// the caller (T113).
/// </para>
/// <para>
/// One rung answers differently. A trainee admitted to their own ratified review is told nothing
/// (<see cref="MsfCampaignsOutsideSnapshotDto.None" />): T113 shows the subject a campaign only once it is released,
/// and the notice is the panel's. The page this feeds does not admit trainees at all; the handler holds the rule so
/// that no other caller can reach round it.
/// </para>
/// <para>
/// Not narrowed by <c>TraineeScopeResolver.MayReadAsync</c>, deliberately. An External panel member from another
/// institution passes the review ladder but not that one, and reads the frozen snapshot, released campaigns included,
/// through the review. Answering them zero would tell them nothing is missing when something is, which is the defect
/// this query exists to fix, while withholding less than the snapshot already shows them.
/// </para>
/// </remarks>
public sealed record CountMsfCampaignsOutsideSnapshotQuery(int ReviewId, ClaimsPrincipal Principal)
    : IRequest<MsfCampaignsOutsideSnapshotDto>;

/// <summary>
/// The window's MSF campaigns that are not in a review's evidence snapshot, by why. (T173)
/// </summary>
/// <param name="AwaitingRelease">Closed in the window and still under review.</param>
/// <param name="ReleasedAfterStart">
/// Closed in the window and released, but not in the snapshot because the review had started first. Always zero
/// before Start.
/// </param>
public sealed record MsfCampaignsOutsideSnapshotDto(int AwaitingRelease, int ReleasedAfterStart)
{
    public static readonly MsfCampaignsOutsideSnapshotDto None = new(0, 0);

    public int Total => AwaitingRelease + ReleasedAfterStart;
}

public sealed class CountMsfCampaignsOutsideSnapshotQueryValidator : AbstractValidator<CountMsfCampaignsOutsideSnapshotQuery>
{
    public CountMsfCampaignsOutsideSnapshotQueryValidator()
    {
        RuleFor(query => query.ReviewId).GreaterThan(0);
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class CountMsfCampaignsOutsideSnapshotQueryHandler
    : IRequestHandler<CountMsfCampaignsOutsideSnapshotQuery, MsfCampaignsOutsideSnapshotDto>
{
    private readonly IApplicationDbContext _dbContext;

    public CountMsfCampaignsOutsideSnapshotQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MsfCampaignsOutsideSnapshotDto> Handle(
        CountMsfCampaignsOutsideSnapshotQuery request,
        CancellationToken cancellationToken)
    {
        var review = await _dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .Include(entity => entity.Panel)
                .ThenInclude(panel => panel.Members)
            .SingleOrDefaultAsync(entity => entity.Id == request.ReviewId, cancellationToken)
            ?? throw new InvalidOperationException("The committee review could not be found.");

        CommitteeDecisionAuthorization.DemandReviewAccess(request.Principal, review);

        // The ladder's trainee arm comes first and admits only the trainee's own ratified review, so reaching here
        // in the role means this is the subject reading about themselves.
        if (request.Principal.IsInRole(WombatRoles.Trainee))
        {
            return MsfCampaignsOutsideSnapshotDto.None;
        }

        // Before Start there is no snapshot, and a released campaign is one Start will take, so only the unreleased
        // are outside it. After Start the snapshot is what it froze, and a released campaign it does not name was
        // released too late for it. Membership is judged for both states alike, so a campaign the snapshot does hold
        // is never reported missing from it. The state rule is the Where below and nothing else: whatever passes it
        // and is not under review is counted as released after Start.
        var reviewId = review.Id;
        var started = review.State != CommitteeReviewState.Scheduled;
        var counts = await _dbContext.Set<MsfCampaign>()
            .AsNoTracking()
            .ClosedInWindowOf(review)
            .Where(campaign =>
                campaign.State == MsfCampaignState.UnderReview ||
                (started && campaign.State == MsfCampaignState.Released))
            .Where(campaign => !_dbContext.Set<CommitteeEvidence>()
                .Any(item => item.ReviewId == reviewId && item.MsfCampaignId == campaign.Id))
            .GroupBy(campaign => campaign.State == MsfCampaignState.UnderReview)
            .Select(group => new { AwaitingRelease = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        return new MsfCampaignsOutsideSnapshotDto(
            counts.Where(entry => entry.AwaitingRelease).Sum(entry => entry.Count),
            counts.Where(entry => !entry.AwaitingRelease).Sum(entry => entry.Count));
    }
}
