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
}
