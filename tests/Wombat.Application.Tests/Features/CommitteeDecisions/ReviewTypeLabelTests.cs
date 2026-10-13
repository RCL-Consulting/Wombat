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
        CommitteeDecisionWording.ReviewTypeShortLabel(type, isFormative: true).Should().Be("Formative check-in", "T364, C8: the list's Type reads as the review's");
        CommitteeDecisionWording.ReviewTypeLabel(type, isFormative: false).Should().Be(CommitteeDecisionWording.ReviewTypeLabel(type));
        CommitteeDecisionWording.ReviewTypeShortLabel(type, isFormative: false).Should().Be(CommitteeDecisionWording.ReviewTypeShortLabel(type));
    }

    [Fact]
    public void EachType_HasItsHelp_ForThePanelAndThePeriod()
    {
        // T364, S1 and C1: the schedule page's Review type radios say what each type decides.
        CommitteeDecisionWording.ReviewTypeHelp(CommitteeReviewType.AnnualProgression, "General CCC", null, 2)
            .Should().Be("The semester-2 sitting decides the trainee's progression for the year.");
        CommitteeDecisionWording.ReviewTypeHelp(CommitteeReviewType.AnnualProgression, "General CCC", null, 1)
            .Should().Be("Decides the trainee's progression for the year.");
        CommitteeDecisionWording.ReviewTypeHelp(CommitteeReviewType.PreGraduation, "General CCC", null, 2)
            .Should().Be("The final review before the programme ends: it decides the trainee's progression, and whether they may graduate.");
        CommitteeDecisionWording.ReviewTypeHelp(CommitteeReviewType.EntrustmentOnly, "General CCC", null, 1)
            .Should().Be("Decides entrustment only: it records no progression category.");
        CommitteeDecisionWording.ReviewTypeHelp(
                CommitteeReviewType.EntrustmentOnly, "Neonatal CCC", "Neonatal team Clinical Competency Committee", 2)
            .Should().Be("Neonatal CCC sits as the Neonatal team Clinical Competency Committee, which decides entrustment only: " +
                         "a review before it records no progression category.");
        CommitteeDecisionWording.FormativeCheckInHelp.Should().Be(
            "Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. " +
            "It takes no seat for the period.");
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
