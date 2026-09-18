namespace Wombat.Domain.Curricula;

public sealed class CurriculumItemProgress
{
    public int Id { get; set; }
    public int CurriculumItemId { get; set; }
    public string TraineeUserId { get; set; } = string.Empty;
    public int CountsSoFar { get; set; }
    public int MinimumLevelReachedCount { get; set; }

    /// <summary>
    /// The entrustment scale the comparisons behind <see cref="MinimumLevelReachedCount" /> were actually
    /// made on, or null when no scale-verified comparison has ever contributed to it (T109).
    /// </summary>
    /// <remarks>
    /// Deliberately not redundant with <c>CurriculumItem.ScaleId</c>. If an administrator later re-pins the
    /// item, this row still records what the stored tally was computed against — which is the only thing
    /// that makes a now-stale tally detectable rather than merely wrong.
    /// </remarks>
    public int? MinimumLevelScaleId { get; set; }

    /// <summary>
    /// Completions that counted toward <see cref="CountsSoFar" /> but were refused the minimum because the
    /// assessment's ladder and the curriculum item's ladder were both known and different (T109).
    /// </summary>
    public int ScaleMismatchCount { get; set; }

    /// <summary>
    /// Completions whose level comparison was made without scale proof on at least one side — the
    /// pre-T109 behaviour, preserved. The exposure meter: a non-zero value says this tally rests on
    /// ordinals nobody has verified are on the same ladder.
    /// </summary>
    public int UnverifiedLevelCount { get; set; }

    public int? LastActivityId { get; set; }
    public DateTime LastUpdated { get; set; }
    public string CreditedActivityKeysJson { get; set; } = "[]";

    public CurriculumItem CurriculumItem { get; set; } = null!;
}
