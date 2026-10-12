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
            DateTime.UtcNow,
            CommitteeQuorumFixture.ChairAndMember);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            decision.Amend(CommitteeDecisionCategory.OutcomeDeferred, "Changed", null));

        Assert.Contains("immutable", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Review_RecordsAndRatifies_GraduateDecision()
    {
        var review = new CommitteeReview
        {
            AcademicYear = 2029,
            Semester = 2,
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
            DateTime.UtcNow,
            CommitteeQuorumFixture.ChairAndMember, [], []);
        review.Ratify("chair-1", DateTime.UtcNow);

        Assert.Equal(CommitteeReviewState.Ratified, review.State);
        Assert.Equal(CommitteeDecisionCategory.Graduate, review.GetCurrentDecision()!.Category);
    }

    [Fact]
    public void Review_StateMachine_TransitionsThroughAppealToFinal()
    {
        var review = new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 1,
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
            DateTime.UtcNow,
            CommitteeQuorumFixture.ChairAndMember, [], []);

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
            null,
            CommitteeQuorumFixture.ChairAndExternal);

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
            AcademicYear = 2026,
            Semester = 1,
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
            DateTime.UtcNow,
            CommitteeQuorumFixture.ChairAndMember, [], []);
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
            null,
            CommitteeQuorumFixture.ChairAndExternal);

        Assert.Equal(CommitteeReviewState.Final, review.State);
        Assert.Equal(2, review.Decisions.Count);
        Assert.Equal(CommitteeAppealOutcome.Remitted, review.Appeals.Single().Outcome);
    }

    // ─── The sentence about the EPAs not decided (T361, D53) ─────────────────

    [Fact]
    public void RecordingTheDecision_StoresTheSentenceAboutTheEpasNotDecided_Trimmed_AndABlankOneAsNull()
    {
        var review = StartedReview();
        var decision = review.RecordDecision(
            CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, "chair-1", DateTime.UtcNow,
            CommitteeQuorumFixture.ChairAndMember, [], [], undecidedNote: "  To be decided at her final review.  ");

        Assert.Equal("To be decided at her final review.", decision.UndecidedNote);

        var blank = StartedReview().RecordDecision(
            CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, "chair-1", DateTime.UtcNow,
            CommitteeQuorumFixture.ChairAndMember, [], [], undecidedNote: "   ");
        Assert.Null(blank.UndecidedNote);
    }

    [Fact]
    public void RemittingTheDecision_CarriesTheSentenceAboutTheEpasNotDecidedForward()
    {
        // T361, E5: the replacement decision says what the original said about the EPAs left undecided.
        var review = StartedReview();
        review.RecordDecision(
            CommitteeDecisionCategory.InadequateProgressAdditionalTraining, "Not enough observed evidence.", null, "chair-1",
            DateTime.UtcNow, CommitteeQuorumFixture.ChairAndMember, [], [], undecidedNote: "Too early in the programme to decide.");
        review.Ratify("chair-1", DateTime.UtcNow);
        review.LodgeAppeal("The single DOPS reflects the start of the year.", "trainee-1", DateTime.UtcNow);

        review.ResolveAppeal(
            CommitteeAppealOutcome.Remitted, "external-1", DateTime.UtcNow.AddDays(1),
            CommitteeDecisionCategory.SatisfactoryWithObservations, "Progress is adequate.", null,
            CommitteeQuorumFixture.ChairAndExternal);

        Assert.Equal(2, review.Decisions.Count);
        Assert.Equal("Too early in the programme to decide.", review.GetCurrentDecision()!.UndecidedNote);
    }

    private static CommitteeReview StartedReview()
    {
        var review = new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 2,
            TraineeUserId = "trainee-1",
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 12, 31),
            ScheduledOn = new DateOnly(2026, 10, 4)
        };

        review.Start([], "chair-1", DateTime.UtcNow);
        return review;
    }
}
