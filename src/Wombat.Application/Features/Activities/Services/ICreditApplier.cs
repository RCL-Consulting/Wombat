using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Activities.Services;

/// <summary>
/// What one application of the credit rules did. The counters are per-call, not cumulative: they describe
/// this completion, so the caller can stamp the outcome onto the transition that caused it (T109).
/// </summary>
/// <param name="UpdatedRows">The progress rows this call created or incremented.</param>
/// <param name="ScaleMismatchCount">
/// Curriculum items that counted toward volume but were refused the minimum, because the assessment's
/// entrustment ladder and the item's were both known and different.
/// </param>
/// <param name="UnverifiedLevelCount">
/// Curriculum items whose level comparison was made without scale proof on at least one side.
/// </param>
public sealed record CreditApplicationResult(
    IReadOnlyList<CurriculumItemProgress> UpdatedRows,
    int ScaleMismatchCount,
    int UnverifiedLevelCount)
{
    public static CreditApplicationResult Empty { get; } = new([], 0, 0);
}

public interface ICreditApplier
{
    /// <param name="activityType">
    /// Carries the <em>pinned</em> version's <c>CreditRulesJson</c> and <c>SchemaJson</c>. The schema is
    /// required as well as the rules because the scale an achieved ordinal sits on is declared by the
    /// <c>scale_key</c> of the field the directive names, and pinning means that answer cannot drift for
    /// the life of the activity (T109).
    /// </param>
    Task<CreditApplicationResult> ApplyAsync(
        Activity completedActivity,
        ActivityType activityType,
        CancellationToken cancellationToken = default);
}
