using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Domain.Tests.CommitteeDecisions;

public sealed class CommitteeReviewTests
{
    [Fact]
    public void Decision_IsImmutable()
    {
        var decision = CommitteeDecision.Create(
            CommitteeDecisionCategory.SatisfactoryProgress,
            "Clear evidence of progress.",
            null,
            "chair-1",
            DateTime.UtcNow);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            decision.Amend(CommitteeDecisionCategory.OutcomeDeferred, "Changed", null));

        Assert.Contains("immutable", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Review_RecordsAndRatifies_GraduateDecision()
    {
        var review = new CommitteeReview
        {
            TraineeUserId = "trainee-1",
            ReviewPeriodFrom = new DateOnly(2029, 1, 1),
            ReviewPeriodTo = new DateOnly(2029, 12, 31),
            ScheduledOn = new DateOnly(2029, 11, 18)
        };

        review.Start([], "chair-1", DateTime.UtcNow);
        review.RecordDecision(
            CommitteeDecisionCategory.Graduate,
            "All 15 EPAs met or exceeded; recommend award and programme completion.",
            null,
            "chair-1",
            DateTime.UtcNow);
        review.Ratify("chair-1", DateTime.UtcNow);

        Assert.Equal(CommitteeReviewState.Ratified, review.State);
        Assert.Equal(CommitteeDecisionCategory.Graduate, review.GetCurrentDecision()!.Category);
    }

    [Fact]
    public void Review_StateMachine_TransitionsThroughAppealToFinal()
    {
        var review = new CommitteeReview
        {
            TraineeUserId = "trainee-1",
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 3, 31),
            ScheduledOn = new DateOnly(2026, 4, 1)
        };

        review.Start(
            [
                new CommitteeEvidence
                {
                    SourceType = CommitteeEvidenceSourceType.Activity,
                    ActivityId = 10,
                    SourceLabel = "Mini-CEX #10",
                    Summary = "State: completed.",
                    SourceRecordedOn = DateTime.UtcNow
                }
            ],
            "chair-1",
            DateTime.UtcNow);

        Assert.Equal(CommitteeReviewState.InProgress, review.State);

        review.RecordDecision(
            CommitteeDecisionCategory.SatisfactoryWithObservations,
            "Borderline but safe progression.",
            "Focus on feedback turnaround.",
            "chair-1",
            DateTime.UtcNow);

        Assert.Equal(CommitteeReviewState.Decided, review.State);

        review.Ratify("chair-1", DateTime.UtcNow);
        Assert.Equal(CommitteeReviewState.Ratified, review.State);

        review.LodgeAppeal("The remediation wording is disproportionate.", "trainee-1", DateTime.UtcNow);
        Assert.Equal(CommitteeReviewState.UnderAppeal, review.State);

        review.ResolveAppeal(
            CommitteeAppealOutcome.Remitted,
            "external-1",
            DateTime.UtcNow,
            CommitteeDecisionCategory.SatisfactoryProgress,
            "Appeal upheld in part; remediation removed.",
            null);

        Assert.Equal(CommitteeReviewState.Final, review.State);
        Assert.Equal(2, review.Decisions.Count);
        Assert.Equal(CommitteeDecisionCategory.SatisfactoryProgress, review.GetCurrentDecision()!.Category);
        Assert.Single(review.Appeals);
        Assert.Equal(CommitteeAppealOutcome.Remitted, review.Appeals.Single().Outcome);
    }

    [Fact]
    public void ResolveAppeal_RemittedWithoutReplacement_ThrowsAndLeavesAppealUnresolved()
    {
        var review = new CommitteeReview
        {
            TraineeUserId = "trainee-1",
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 3, 31),
            ScheduledOn = new DateOnly(2026, 4, 1)
        };

        review.Start([], "chair-1", DateTime.UtcNow);
        review.RecordDecision(
            CommitteeDecisionCategory.InadequateProgressAdditionalTraining,
            "Insufficient evidence volume.",
            null,
            "chair-1",
            DateTime.UtcNow);
        review.Ratify("chair-1", DateTime.UtcNow);
        review.LodgeAppeal("Evidence reflects start-of-year skill.", "trainee-1", DateTime.UtcNow);

        // F-4F-1: a Remitted resolution missing the replacement decision must throw WITHOUT mutating
        // anything — the appeal stays open and the review stays UnderAppeal so the operator can retry.
        Assert.Throws<InvalidOperationException>(() =>
            review.ResolveAppeal(CommitteeAppealOutcome.Remitted, "external-1", DateTime.UtcNow));

        var appeal = review.Appeals.Single();
        Assert.Null(appeal.ResolvedOn);
        Assert.Null(appeal.Outcome);
        Assert.Equal(CommitteeReviewState.UnderAppeal, review.State);
        Assert.Single(review.Decisions);

        // A subsequent correct resolution still succeeds (no stranded state).
        review.ResolveAppeal(
            CommitteeAppealOutcome.Remitted,
            "external-1",
            DateTime.UtcNow,
            CommitteeDecisionCategory.InadequateProgressAdditionalTraining,
            "Referral upheld; re-review window reduced to 3 months.",
            null);

        Assert.Equal(CommitteeReviewState.Final, review.State);
        Assert.Equal(2, review.Decisions.Count);
        Assert.Equal(CommitteeAppealOutcome.Remitted, review.Appeals.Single().Outcome);
    }
}
