using FluentAssertions;
using Wombat.Application.Features.EntrustmentDecisions;

namespace Wombat.Application.Tests.Features.EntrustmentDecisions;

/// <summary>
/// The whole truth table of the rule the entrustment standing judges a level by (T166): a verdict only on the item's
/// own ladder, "not comparable" wherever either ladder is unknown or they differ.
/// </summary>
public sealed class EntrustmentStandingJudgeTests
{
    private const int Cpsa = 42;
    private const int OrScale = 43;

    [Theory]
    [InlineData(4, Cpsa, 4, Cpsa, EntrustmentStandingStatus.AtOrAbove)]
    [InlineData(5, Cpsa, 4, Cpsa, EntrustmentStandingStatus.AtOrAbove)]
    [InlineData(3, Cpsa, 4, Cpsa, EntrustmentStandingStatus.Below)]
    [InlineData(5, OrScale, 4, Cpsa, EntrustmentStandingStatus.NotComparable)]
    [InlineData(1, OrScale, 4, Cpsa, EntrustmentStandingStatus.NotComparable)]
    [InlineData(5, Cpsa, 4, null, EntrustmentStandingStatus.NotComparable)]
    [InlineData(5, null, 4, Cpsa, EntrustmentStandingStatus.NotComparable)]
    [InlineData(5, null, 4, null, EntrustmentStandingStatus.NotComparable)]
    public void ALevel_IsJudgedOnlyOnTheItemsLadder(
        int achieved,
        int? achievedScale,
        int required,
        int? requiredScale,
        EntrustmentStandingStatus expected)
        => EntrustmentStanding.Judge(achieved, achievedScale, required, requiredScale).Should().Be(expected);

    [Theory]
    [InlineData(Cpsa)]
    [InlineData(null)]
    public void NoLevel_IsNoDecision_WhateverTheLadder(int? requiredScale)
        => EntrustmentStanding.Judge(null, null, 4, requiredScale).Should().Be(EntrustmentStandingStatus.NoDecision);
}
