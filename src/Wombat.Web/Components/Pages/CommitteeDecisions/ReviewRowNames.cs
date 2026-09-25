using System.Globalization;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Components.Pages.CommitteeDecisions;

/// <summary>
/// A committee review as a list row's action names it (T239): "the 2026 S2 review before Neonatal CCC", and on the
/// schedule, whose rows are many trainees', "the 2026 S2 review of Thandi Nkosi before Neonatal CCC".
/// </summary>
/// <remarks>
/// The name is built from what the row shows: the period it sits for, the panel, and the trainee where the list is more
/// than one trainee's, with "formative" where the review is. That can still repeat: a trainee has one open binding review
/// for a period before a panel, but once it is ratified another may be scheduled (an entrustment-only review after the
/// annual one), and formative reviews are not held to one. So a name two rows share adds the review's type as the Type
/// column says it, then the day each was scheduled for, then the review's number, as the committee's refusals name a
/// review ("Review #12"). Every name on one list is then different (<see cref="RowNames.Distinct{TItem, TKey}" />).
/// </remarks>
public static class ReviewRowNames
{
    /// <summary>Each review's name on one list, by review id; no two are the same.</summary>
    /// <param name="reviews">The list's rows.</param>
    /// <param name="withTrainee">Whether to name whose review it is: true on a list of many trainees' reviews.</param>
    public static IReadOnlyDictionary<int, string> For(IEnumerable<CommitteeReviewListItemDto> reviews, bool withTrainee)
        => RowNames.Distinct(
            reviews,
            review => review.Id,
            review => Base(review, withTrainee),
            review => $", {CommitteeDecisionWording.ReviewTypeShortLabel(review.ReviewType)}",
            review => $", scheduled {review.ScheduledOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}",
            review => $", review #{review.Id}");

    private static string Base(CommitteeReviewListItemDto review, bool withTrainee)
    {
        var formative = review.IsFormative ? "formative " : string.Empty;
        var trainee = withTrainee && !string.IsNullOrWhiteSpace(review.TraineeName) ? $" of {review.TraineeName}" : string.Empty;
        return $"the {review.PeriodLabel} {formative}review{trainee} before {review.PanelName}";
    }
}
