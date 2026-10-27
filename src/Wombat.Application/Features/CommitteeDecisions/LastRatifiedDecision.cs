using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// A trainee's last ratified committee decision, as the trainee profile's End this programme names it before the last day
/// (T379, flow 13, Q13): "The last committee decision: Graduate (programme complete), Pre-graduation review, ratified
/// 2026-10-26." It gates nothing: an end is recorded whatever the committee decided, or whether it has decided at all.
/// </summary>
/// <param name="Category">The progression category in words (<see cref="CommitteeDecisionWording.CategoryLabel" />); null for
/// an entrustment-only review, which records none.</param>
/// <param name="ReviewType">The review's type in words (<see cref="CommitteeDecisionWording.ReviewTypeLabel" />).</param>
/// <param name="RatifiedOn">The day it was ratified, on the South African calendar.</param>
public sealed record LastRatifiedDecisionDto(string? Category, string ReviewType, DateOnly RatifiedOn);

/// <summary>Reads <see cref="LastRatifiedDecisionDto" />. It checks no scope: its caller has (the profile's read).</summary>
public static class LastRatifiedDecision
{
    /// <summary>
    /// The latest ratified review of <paramref name="traineeUserId" />'s, by the moment it was ratified then by id, with its
    /// current decision (a remitted appeal's replacement included); null when none was ever ratified. A review under appeal
    /// or final was ratified first, so it counts; a withdrawn one never was. A formative review is left out though
    /// <see cref="CommitteeReview.Close" /> stamps its <c>RatifiedOn</c>: it decides nothing (T379's B1).
    /// </summary>
    public static async Task<LastRatifiedDecisionDto?> ReadAsync(
        IApplicationDbContext dbContext,
        string traineeUserId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(traineeUserId);

        var review = await dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .Include(entity => entity.Decisions)
            .Where(entity => entity.TraineeUserId == traineeUserId && entity.RatifiedOn != null && !entity.IsFormative)
            .OrderByDescending(entity => entity.RatifiedOn)
            .ThenByDescending(entity => entity.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (review?.RatifiedOn is not { } ratifiedOn)
        {
            return null;
        }

        var category = review.GetCurrentDecision()?.Category;
        return new LastRatifiedDecisionDto(
            category is { } value ? CommitteeDecisionWording.CategoryLabel(value) : null,
            CommitteeDecisionWording.ReviewTypeLabel(review.ReviewType),
            ProgrammeCalendar.DateOf(ratifiedOn));
    }
}
