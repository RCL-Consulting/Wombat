using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
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

    /// <summary>
    /// The open binding review of this trainee for this period before a panel in <paramref name="panel" />'s seat, if any:
    /// a panel at the same institution sitting as the same College committee, or as none. Includes the panel itself, which
    /// is what the database's index allows one of. The lowest id first, so every caller names the same review.
    /// </summary>
    /// <remarks>
    /// The one read of "who holds the seat" (T364, Q3 and T309 item 1): the scheduling handler refuses a second review by
    /// it, and the schedule page's Already held (<see cref="GetSchedulingSeatQuery" />) says so before Create by it, so
    /// the page never offers what the handler refuses. A formative check-in holds no seat (<see cref="OpenBinding" />).
    /// </remarks>
    public static Task<SeatHolder?> HolderAsync(
        IApplicationDbContext dbContext,
        string traineeUserId,
        DecisionPanel panel,
        AcademicPeriod period,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(panel);

        var institutionId = panel.InstitutionId;
        var bodyKey = DecisionBody.NormalizeKey(panel.DecisionBodyKey);

        return dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .OpenBinding()
            .Where(entity => entity.TraineeUserId == traineeUserId &&
                             entity.Panel.InstitutionId == institutionId &&
                             entity.Panel.DecisionBodyKey == bodyKey &&
                             entity.AcademicYear == period.Year &&
                             entity.Semester == period.Semester)
            .OrderBy(entity => entity.Id)
            .Select(entity => new SeatHolder(entity.Id, entity.Panel.Name, entity.State, entity.ScheduledOn))
            .FirstOrDefaultAsync(cancellationToken);
    }
}

/// <summary>The open binding review that holds a trainee's seat for a period (<see cref="CommitteeReviewSeats.HolderAsync" />).</summary>
internal sealed record SeatHolder(int Id, string PanelName, CommitteeReviewState State, DateOnly ScheduledOn);
