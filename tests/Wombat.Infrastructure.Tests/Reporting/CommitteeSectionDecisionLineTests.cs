using FluentAssertions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Infrastructure.Reporting;

namespace Wombat.Infrastructure.Tests.Reporting;

/// <summary>
/// The portfolio prints a committee decision's outcome in words, and an entrustment-only review's decision, which records
/// no progression category, as what it is rather than as a blank or a raw value (T131 slice 5).
/// </summary>
public sealed class CommitteeSectionDecisionLineTests
{
    private static readonly IReadOnlyCollection<DecisionPanelMember> Quorum =
    [
        new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair },
        new DecisionPanelMember { UserId = "member-1", Role = DecisionPanelMemberRole.Member }
    ];

    [Fact]
    public void AProgressionDecision_PrintsItsCategoryInWords()
    {
        var decision = CommitteeDecision.Create(
            CommitteeDecisionCategory.InadequateProgressAdditionalTraining, "Support plan.", null, "chair-1", DateTime.UtcNow, Quorum);

        CommitteeSectionComponent.DecisionLine(decision).Should().Be("Inadequate Progress — Additional Training");
    }

    [Fact]
    public void AnEntrustmentOnlyReviewsDecision_PrintsThatItDecidedEntrustmentOnly()
    {
        var decision = CommitteeDecision.Create(null, "PAED-004 entrusted at 3a.", null, "chair-1", DateTime.UtcNow, Quorum);

        CommitteeSectionComponent.DecisionLine(decision).Should().Be("Entrustment decisions only");
    }

    [Theory]
    [InlineData(false, "Annual progression review")]
    [InlineData(true, "Formative check-in")]
    public void TheReviewsType_IsPrintedAsTheReviewIs_AFormativeCheckInAsOne(bool isFormative, string expected)
    {
        // T361, review R1 (T309 item 2): a formative check-in decides nothing whatever its stored type.
        var review = new CommitteeReview
        {
            AcademicYear = 2026, Semester = 2, ReviewType = CommitteeReviewType.AnnualProgression, IsFormative = isFormative
        };

        CommitteeSectionComponent.TypeLine(review).Should().Be(expected);
    }
}
