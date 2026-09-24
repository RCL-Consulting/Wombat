using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// Which MSF campaigns belong to a committee review's window: the review's trainee's, closed on one of its days.
/// (T138, T173)
/// </summary>
/// <remarks>
/// <para>
/// One definition, read by the evidence snapshot (<see cref="StartCommitteeReviewCommandHandler" />, released
/// campaigns) and by the live notice beside it (<see cref="CountMsfCampaignsOutsideSnapshotQueryHandler" />, the
/// campaigns the snapshot does not hold). The notice counts exactly what the snapshot would have taken had they been
/// released in time, so the two must not select on different days; they share this rather than each spelling the
/// bounds out.
/// </para>
/// <para>
/// A campaign falls in the window by the day it actually closed, not the day it was scheduled to (T138). That is the
/// date a release stamps on its per-EPA evidence activities as <c>ObservedOn</c>
/// (<c>ReleaseMsfCampaign.EvidenceCompleteOn</c>), so the report and its per-EPA claims land in the same review.
/// Windowing by <c>ClosesOn</c> split them whenever the two dates fell either side of a boundary, and the auto-close
/// job makes that routine: it closes a campaign on the day after <c>ClosesOn</c> at the earliest, so every
/// auto-closed campaign scheduled for a window's last day was split. <c>ObservedOn</c> is <c>ClosedOn</c>'s UTC date,
/// hence UTC bounds: the review's inclusive days become a half-open UTC range. A campaign that never closed has no
/// <c>ClosedOn</c> and falls in no window.
/// </para>
/// </remarks>
internal static class MsfCampaignReviewWindow
{
    public static IQueryable<MsfCampaign> ClosedInWindowOf(this IQueryable<MsfCampaign> campaigns, CommitteeReview review)
    {
        ArgumentNullException.ThrowIfNull(campaigns);
        ArgumentNullException.ThrowIfNull(review);

        var traineeUserId = review.TraineeUserId;
        var closedFrom = review.ReviewPeriodFrom.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var closedBefore = review.ReviewPeriodTo.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        return campaigns.Where(campaign =>
            campaign.SubjectUserId == traineeUserId &&
            campaign.ClosedOn >= closedFrom &&
            campaign.ClosedOn < closedBefore);
    }
}
