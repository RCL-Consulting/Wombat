using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Domain.Tests.CommitteeDecisions;

/// <summary>
/// What each appeal outcome does to the review's current decision (T307, D51). The appeal body has two effects, so it has
/// two outcomes: Dismissed leaves the appealed decision in force, and Remitted replaces it with the appeal body's own.
/// Before T307 there was a third, Upheld, which did what Dismissed did under a name that says the appeal succeeded.
/// </summary>
public sealed class AppealOutcomeTests
{
    private static readonly DateTime Now = new(2026, 9, 26, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void TheAppealBody_HasTwoOutcomes_DismissedAndRemitted()
    {
        Assert.Equal(
            [CommitteeAppealOutcome.Dismissed, CommitteeAppealOutcome.Remitted],
            Enum.GetValues<CommitteeAppealOutcome>());
    }

    [Fact]
    public void Dismissing_LeavesTheAppealedDecisionCurrent_AndClosesTheReview()
    {
        var review = AppealedReview();
        var appealed = review.GetCurrentDecision()!;

        review.ResolveAppeal(CommitteeAppealOutcome.Dismissed, "chair-1", Now);

        Assert.Same(appealed, review.GetCurrentDecision());
        Assert.Single(review.Decisions);
        Assert.Equal(CommitteeDecisionCategory.InadequateProgressAdditionalTraining, review.GetCurrentDecision()!.Category);
        Assert.Equal(CommitteeReviewState.Final, review.State);
        Assert.Equal(CommitteeAppealOutcome.Dismissed, review.Appeals.Single().Outcome);
    }

    [Fact]
    public void Remitting_MakesTheAppealBodysDecisionCurrent_WithItsConditions_AndClosesTheReview()
    {
        var review = AppealedReview();
        var appealed = review.GetCurrentDecision()!;
        // Nothing is persisted here, so every id is 0 and "supersedes the appealed decision" would pass for any decision.
        // The appealed one is given the id the store would have given it.
        typeof(CommitteeDecision).GetProperty(nameof(CommitteeDecision.Id))!.SetValue(appealed, 41);

        review.ResolveAppeal(
            CommitteeAppealOutcome.Remitted, "external-1", Now.AddDays(30), CommitteeDecisionCategory.SatisfactoryWithObservations,
            "Progress is adequate.", "Two observed Mini-CEX before the next review.", CommitteeQuorumFixture.ChairAndExternal);

        var replacement = review.GetCurrentDecision()!;
        Assert.NotSame(appealed, replacement);
        Assert.Equal(CommitteeDecisionCategory.SatisfactoryWithObservations, replacement.Category);
        Assert.Equal("Two observed Mini-CEX before the next review.", replacement.Conditions);
        Assert.Equal(41, replacement.SupersedesDecisionId);
        Assert.Equal(CommitteeReviewState.Final, review.State);
        Assert.Equal(CommitteeAppealOutcome.Remitted, review.Appeals.Single().Outcome);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)] // Upheld's old value: a stored or sent 1 is not an outcome any more.
    [InlineData(99)]
    public void AnUndefinedOutcome_IsRefused_AndLeavesTheAppealOpen(int value)
    {
        var review = AppealedReview();
        var appealed = review.GetCurrentDecision()!;

        var refusal = Assert.Throws<InvalidOperationException>(
            () => review.ResolveAppeal((CommitteeAppealOutcome)value, "chair-1", Now));

        Assert.Equal("An appeal is resolved as dismissed or remitted.", refusal.Message);
        Assert.Equal(CommitteeReviewState.UnderAppeal, review.State);
        Assert.Null(review.FinalizedOn);
        Assert.Same(appealed, review.GetCurrentDecision());
        Assert.Null(review.Appeals.Single().ResolvedOn);
        Assert.Null(review.Appeals.Single().Outcome);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void AnAppeal_RefusesAnUndefinedOutcome_Itself(int value)
    {
        var appeal = CommitteeAppeal.Lodge("The single DOPS reflects the start of the year.", "trainee-1", Now);

        Assert.Throws<InvalidOperationException>(() => appeal.Resolve((CommitteeAppealOutcome)value, "chair-1", Now));

        Assert.Null(appeal.ResolvedOn);
        Assert.Null(appeal.Outcome);
    }

    private static CommitteeReview AppealedReview()
    {
        var review = new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 2,
            TraineeUserId = "trainee-1",
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 12, 31),
            ScheduledOn = new DateOnly(2026, 9, 1)
        };

        review.Start([], "chair-1", Now);
        review.RecordDecision(
            CommitteeDecisionCategory.InadequateProgressAdditionalTraining, "Not enough observed evidence.", null, "chair-1",
            Now, CommitteeQuorumFixture.ChairAndMember, [], []);
        review.Ratify("chair-1", Now);
        review.LodgeAppeal("The single DOPS reflects the start of the year.", "trainee-1", Now);
        return review;
    }
}
