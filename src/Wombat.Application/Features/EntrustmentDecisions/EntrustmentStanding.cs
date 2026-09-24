using Wombat.Domain.Epas;

namespace Wombat.Application.Features.EntrustmentDecisions;

/// <summary>
/// Where one level stands against one required level on a curriculum item's ladder. (T166)
/// </summary>
public enum EntrustmentStandingStatus
{
    /// <summary>At or above the required level, on the item's own ladder.</summary>
    AtOrAbove = 1,

    /// <summary>Below the required level, on the item's own ladder.</summary>
    Below = 2,

    /// <summary>There is no level to compare: no active entrustment decision, or no rating.</summary>
    NoDecision = 3,

    /// <summary>
    /// There is a level, but not on the item's ladder, or the item names no ladder or the rating's is unknown. Never
    /// coerced into a verdict (T109).
    /// </summary>
    NotComparable = 4
}

/// <summary>
/// The one rule the entrustment standing judges a level by. Pure, so its whole truth table is testable without a
/// database. (T166)
/// </summary>
/// <remarks>
/// <para>
/// It asks <see cref="EntrustmentLevelComparer" />, the one place two entrustment ordinals are compared, and accepts
/// only a <see cref="LevelComparisonBasis.SameScale" /> answer. The credit engine goes further and compares an unpinned
/// side on trust (<see cref="LevelComparisonBasis.Unpinned" />), so that nothing that credited before T109 stopped
/// crediting. That reason does not reach a committee's view: a standing that said "at target" of a level on no known
/// ladder would be a verdict nobody established. So an unpinned item, or a rating whose ladder cannot be resolved, is
/// "not comparable", exactly as a level on another ladder is.
/// </para>
/// <para>
/// An entrustment decision always has a known ladder (its <c>EntrustmentLevel.ScaleId</c>), so for a decision only the
/// item can be the unknown side. On the CPSA v11.1 catalogue every item is pinned by its seeder, and every rated CPSA
/// instrument declares that ladder by name.
/// </para>
/// </remarks>
public static class EntrustmentStanding
{
    /// <summary>
    /// Judges <paramref name="achievedOrder" /> on <paramref name="achievedScaleId" /> against
    /// <paramref name="requiredOrder" /> on <paramref name="requiredScaleId" />, the curriculum item's pin.
    /// </summary>
    public static EntrustmentStandingStatus Judge(
        int? achievedOrder,
        int? achievedScaleId,
        int requiredOrder,
        int? requiredScaleId)
    {
        if (achievedOrder is null)
        {
            return EntrustmentStandingStatus.NoDecision;
        }

        var comparison = EntrustmentLevelComparer.Compare(achievedOrder, achievedScaleId, requiredOrder, requiredScaleId);
        if (comparison.Basis != LevelComparisonBasis.SameScale)
        {
            return EntrustmentStandingStatus.NotComparable;
        }

        return comparison.MinimumMet ? EntrustmentStandingStatus.AtOrAbove : EntrustmentStandingStatus.Below;
    }
}
