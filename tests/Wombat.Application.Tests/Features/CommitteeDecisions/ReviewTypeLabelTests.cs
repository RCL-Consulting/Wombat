using FluentAssertions;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Tests.Features.CommitteeDecisions;

/// <summary>
/// T361 (T309 item 2): a formative review reads as a formative check-in, whatever type it was stored with; and the
/// who-may-act sentence a member reads during the sitting names no deferral (D53).
/// </summary>
public sealed class ReviewTypeLabelTests
{
    [Theory]
    [InlineData(CommitteeReviewType.AnnualProgression)]
    [InlineData(CommitteeReviewType.PreGraduation)]
    [InlineData(CommitteeReviewType.EntrustmentOnly)]
    public void AFormativeReview_ReadsFormativeCheckIn_WhateverItsStoredType(CommitteeReviewType type)
    {
        CommitteeDecisionWording.ReviewTypeLabel(type, isFormative: true).Should().Be("Formative check-in");
        CommitteeDecisionWording.ReviewTypeShortLabel(type, isFormative: true).Should().Be("Formative");
        CommitteeDecisionWording.ReviewTypeLabel(type, isFormative: false).Should().Be(CommitteeDecisionWording.ReviewTypeLabel(type));
        CommitteeDecisionWording.ReviewTypeShortLabel(type, isFormative: false).Should().Be(CommitteeDecisionWording.ReviewTypeShortLabel(type));
    }

    [Fact]
    public void TheDetailDto_CarriesTheLabel()
    {
        Detail(isFormative: true).ReviewTypeLabel.Should().Be("Formative check-in");
        Detail(isFormative: false).ReviewTypeLabel.Should().Be("Annual progression review");
    }

    [Fact]
    public void TheWhoMayActSentence_NamesTheChair_AndNoDeferral()
    {
        CommitteeDecisionWording.OnlyTheChairSits("Thandi Zulu").Should().Be(
            "Only the panel's chair, Thandi Zulu, can stage entrustment decisions and record the committee's decision.");
        CommitteeDecisionWording.OnlyTheChairSits(null).Should().Be(
            "Only the panel's chair can stage entrustment decisions and record the committee's decision.");
    }

    private static CommitteeReviewDetailDto Detail(bool isFormative)
        => new(
            1, "trainee-1", 2, "Paed Annual Review Panel", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31),
            new DateOnly(2026, 10, 4), CommitteeReviewState.Scheduled, null, null, null, null, null, [], [], [],
            isFormative, CommitteeReviewType.AnnualProgression)
        {
            AcademicYear = 2026,
            Semester = 2
        };
}
