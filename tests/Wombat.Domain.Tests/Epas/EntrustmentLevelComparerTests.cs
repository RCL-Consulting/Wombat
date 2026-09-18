using Wombat.Domain.Epas;

namespace Wombat.Domain.Tests.Epas;

/// <summary>
/// The whole truth table for T109's comparison. The cases that matter most are the ones that must NOT
/// change: an unpinned comparison has to behave exactly as Wombat did before this existed.
/// </summary>
public sealed class EntrustmentLevelComparerTests
{
    [Fact]
    public void NotGated_MeetsTheMinimum()
    {
        var result = EntrustmentLevelComparer.NotGated();

        Assert.True(result.MinimumMet);
        Assert.Equal(LevelComparisonBasis.NoGate, result.Basis);
    }

    [Fact]
    public void Compare_WhenNoOrdinalWasRecorded_DoesNotMeetTheMinimum()
    {
        var result = EntrustmentLevelComparer.Compare(providedOrder: null, providedScaleId: 1, requiredOrder: 3, requiredScaleId: 1);

        Assert.False(result.MinimumMet);
        Assert.Equal(LevelComparisonBasis.ValueMissing, result.Basis);
    }

    [Theory]
    [InlineData(4, 3, true)]
    [InlineData(3, 3, true)]
    [InlineData(2, 3, false)]
    public void Compare_OnTheSameScale_ComparesOrdinals(int provided, int required, bool expected)
    {
        var result = EntrustmentLevelComparer.Compare(provided, providedScaleId: 7, required, requiredScaleId: 7);

        Assert.Equal(expected, result.MinimumMet);
        Assert.Equal(LevelComparisonBasis.SameScale, result.Basis);
    }

    [Fact]
    public void Compare_OnDifferentScales_RefusesEvenWhenTheOrdinalWouldPass()
    {
        // The T109 case exactly. Order 4 on the six-rung CPSA ladder is rung "3b" — still needs
        // supervision — while order 4 on a five-rung ladder is "Independent". 4 >= 4 is arithmetically
        // true and clinically false.
        var result = EntrustmentLevelComparer.Compare(providedOrder: 4, providedScaleId: 6, requiredOrder: 4, requiredScaleId: 5);

        Assert.False(result.MinimumMet);
        Assert.Equal(LevelComparisonBasis.ScaleMismatch, result.Basis);
    }

    [Fact]
    public void Compare_OnDifferentScales_RefusesRegardlessOfDirection()
    {
        var result = EntrustmentLevelComparer.Compare(providedOrder: 6, providedScaleId: 5, requiredOrder: 1, requiredScaleId: 6);

        Assert.False(result.MinimumMet);
        Assert.Equal(LevelComparisonBasis.ScaleMismatch, result.Basis);
    }

    [Theory]
    [InlineData(null, 3)]
    [InlineData(3, null)]
    [InlineData(null, null)]
    public void Compare_WhenEitherSideIsUnpinned_ComparesExactlyAsBeforeT109(int? providedScaleId, int? requiredScaleId)
    {
        var passing = EntrustmentLevelComparer.Compare(4, providedScaleId, 3, requiredScaleId);
        Assert.True(passing.MinimumMet);
        Assert.Equal(LevelComparisonBasis.Unpinned, passing.Basis);

        var failing = EntrustmentLevelComparer.Compare(2, providedScaleId, 3, requiredScaleId);
        Assert.False(failing.MinimumMet);
        Assert.Equal(LevelComparisonBasis.Unpinned, failing.Basis);
    }
}
