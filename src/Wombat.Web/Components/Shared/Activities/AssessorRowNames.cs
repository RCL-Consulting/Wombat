using Wombat.Application.Features.Activities.Dtos;

namespace Wombat.Web.Components.Shared.Activities;

/// <summary>
/// Each activity link's accessible name on an assessor's page, the Assessor's Home and the Activity inbox (T350, flow 04;
/// note 9, C11): one naming over "Waiting for you" and the decisions, so no two links on the page read the same. A link
/// reads its name and ", from &lt;registrar&gt;" (<see cref="ActivityRowNames.WaitingLinkWords" />); two that read the
/// same add their state, then ", waiting since 2026-09-30 08:12 SAST" for a waiting row or ", decided 2026-09-30 09:59
/// SAST" for a decision, then are numbered in list order, "(1 of 2)". Only those need an <c>aria-label</c>.
/// </summary>
/// <remarks>
/// Over the waiting rows alone this is <see cref="ActivityRowNames.Waiting" />, the way on's and WaitingList's own naming.
/// The two lists never hold one activity twice (a waiting one still has a move; a decided one has none), but a decided
/// link can read as a waiting one does: a registrar's second request on one EPA and date, one rated and one not.
/// </remarks>
public static class AssessorRowNames
{
    public static IReadOnlyDictionary<int, string> For(
        IEnumerable<ActivitySummaryDto> waiting, IEnumerable<ActivitySummaryDto> decided)
        => RowNames.Distinct(
            waiting.Concat(decided).DistinctBy(activity => activity.Id),
            activity => activity.Id,
            ActivityRowNames.WaitingLinkWords,
            activity => $", {activity.CurrentStateLabel}",
            activity => activity.DecidedOn is { } decidedOn
                ? $", decided {ActivityMoments.When(decidedOn)}"
                : $", waiting since {ActivityMoments.When(activity.UpdatedOn)}");
}
