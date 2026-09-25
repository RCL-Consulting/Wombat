using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// A trainee's seat before a committee: what a panel sits as at its institution, the College committee it carries
/// (<see cref="DecisionPanel.DecisionBodyKey" />) or none, the trainee's general committee. A trainee has one open binding
/// review per period in each seat (T131 slice 4 review): the scheduling handler refuses a second
/// (<see cref="ScheduleCommitteeReviewCommandHandler" />), and the decisions-due page points to the first instead of
/// offering Schedule (T131 slice 6 review). Both ask these, so the page offers nothing the handler refuses.
/// </summary>
internal static class CommitteeReviewSeats
{
    /// <summary>The binding reviews still open: scheduled, in progress, or decided and awaiting ratify.</summary>
    public static IQueryable<CommitteeReview> OpenBinding(this IQueryable<CommitteeReview> reviews)
    {
        ArgumentNullException.ThrowIfNull(reviews);
        return reviews.Where(review => !review.IsFormative &&
                                       (review.State == CommitteeReviewState.Scheduled ||
                                        review.State == CommitteeReviewState.InProgress ||
                                        review.State == CommitteeReviewState.Decided));
    }

    /// <summary>Whether panels carrying these two bodies, at one institution, are the same seat.</summary>
    public static bool SameSeat(string? bodyKey, string? otherBodyKey)
        => string.Equals(DecisionBody.NormalizeKey(bodyKey), DecisionBody.NormalizeKey(otherBodyKey), StringComparison.Ordinal);
}
