namespace Wombat.Domain.Epas;

/// <summary>
/// Why a level comparison came out the way it did. The <em>basis</em> matters as much as the result:
/// a pass with no scale proof and a pass on a verified ladder are both "met", and only one of them is
/// trustworthy (T109).
/// </summary>
public enum LevelComparisonBasis
{
    /// <summary>The credit directive gates on no level at all, so there is nothing to compare.</summary>
    NoGate,

    /// <summary>Gated, but the activity carries no readable ordinal for the field named by the directive.</summary>
    ValueMissing,

    /// <summary>Both sides resolved to the same scale. The comparison means what it says.</summary>
    SameScale,

    /// <summary>
    /// At least one side has no known scale, so the ordinals were compared on trust — exactly as every
    /// comparison in Wombat worked before T109. Counted, so that a pass without proof is never silent.
    /// </summary>
    Unpinned,

    /// <summary>
    /// Both sides resolved, to <em>different</em> scales. The ordinals are not comparable and the minimum
    /// is refused. This is the defect T109 exists to stop: rung 4 of the six-rung CPSA ladder is "3b,
    /// still needs supervision", and rung 4 of a five-rung ladder is "Independent".
    /// </summary>
    ScaleMismatch
}

/// <summary>The outcome of comparing an achieved entrustment ordinal against a required one.</summary>
public readonly record struct LevelComparison(bool MinimumMet, LevelComparisonBasis Basis);

/// <summary>
/// The one place two entrustment ordinals are compared. Pure, so the whole truth table is testable
/// without a database.
/// </summary>
public static class EntrustmentLevelComparer
{
    /// <summary>The directive gates on no level: every matching completion meets the (absent) minimum.</summary>
    public static LevelComparison NotGated() => new(true, LevelComparisonBasis.NoGate);

    /// <summary>
    /// Compares an achieved ordinal against a required one, refusing rather than guessing when the two
    /// are known to sit on different ladders.
    /// </summary>
    /// <param name="providedOrder">The ordinal the assessment recorded, or null when none is readable.</param>
    /// <param name="providedScaleId">The scale that ordinal is expressed on, or null when unknown.</param>
    /// <param name="requiredOrder">The curriculum item's minimum for the trainee's stage.</param>
    /// <param name="requiredScaleId">The scale that minimum is expressed on, or null when unpinned.</param>
    /// <remarks>
    /// The null-means-no-constraint discipline is deliberate and load-bearing. A scale that cannot be
    /// resolved on either side degrades to <see cref="LevelComparisonBasis.Unpinned" />, which compares
    /// precisely as Wombat did before T109 — so nothing that credits today stops crediting because a
    /// ladder has not been pinned yet.
    /// </remarks>
    public static LevelComparison Compare(
        int? providedOrder,
        int? providedScaleId,
        int requiredOrder,
        int? requiredScaleId)
    {
        if (providedOrder is null)
        {
            return new LevelComparison(false, LevelComparisonBasis.ValueMissing);
        }

        if (providedScaleId.HasValue && requiredScaleId.HasValue)
        {
            return providedScaleId.Value == requiredScaleId.Value
                ? new LevelComparison(providedOrder.Value >= requiredOrder, LevelComparisonBasis.SameScale)
                : new LevelComparison(false, LevelComparisonBasis.ScaleMismatch);
        }

        return new LevelComparison(providedOrder.Value >= requiredOrder, LevelComparisonBasis.Unpinned);
    }
}
