using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Domain.Tests.CommitteeDecisions;

/// <summary>
/// T131 slice 5 (O4): a review that decides entrustment only records, ratifies and remits a decision with no progression
/// category, because its decision is its STARs; a progression review's decision always carries one. Every refusal leaves
/// the review as it was.
/// </summary>
public sealed class EntrustmentOnlyReviewTests
{
    private static readonly DateTime Now = new(2026, 9, 24, 9, 0, 0, DateTimeKind.Utc);

    // ─── Which types a review may take ───────────────────────────────────────

    [Fact]
    public void BeforeACollegeCommittee_EveryReviewIsEntrustmentOnly_InEitherSemester()
    {
        Assert.Equal([CommitteeReviewType.EntrustmentOnly], CommitteeReviewTypes.Allowed(sitsAsDecisionBody: true, semester: 1));
        Assert.Equal([CommitteeReviewType.EntrustmentOnly], CommitteeReviewTypes.Allowed(sitsAsDecisionBody: true, semester: 2));
        Assert.Equal(CommitteeReviewType.EntrustmentOnly, CommitteeReviewTypes.DefaultFor(sitsAsDecisionBody: true, semester: 2));
    }

    [Fact]
    public void AGeneralPanelsSemester1Sitting_MayDecideEntrustmentOnly_ButItsSemester2SittingDecidesProgression()
    {
        Assert.Equal(
            [CommitteeReviewType.AnnualProgression, CommitteeReviewType.PreGraduation, CommitteeReviewType.EntrustmentOnly],
            CommitteeReviewTypes.Allowed(sitsAsDecisionBody: false, semester: 1));
        Assert.Equal(
            [CommitteeReviewType.AnnualProgression, CommitteeReviewType.PreGraduation],
            CommitteeReviewTypes.Allowed(sitsAsDecisionBody: false, semester: 2));
        Assert.Equal(CommitteeReviewType.AnnualProgression, CommitteeReviewTypes.DefaultFor(sitsAsDecisionBody: false, semester: 1));
    }

    [Theory]
    [InlineData(CommitteeReviewType.AnnualProgression, 1)]
    [InlineData(CommitteeReviewType.PreGraduation, 2)]
    public void AProgressionReview_BeforeACollegeCommittee_IsRefused_NamingTheCommittee(CommitteeReviewType type, int semester)
    {
        var refusal = CommitteeReviewTypes.Refusal(type, "Neonatal team Clinical Competency Committee", semester);

        Assert.Equal(
            "The Neonatal team Clinical Competency Committee decides entrustment only: a review before it is an " +
            "entrustment-only review, which records no progression category. The trainee's progression is decided by " +
            "their general committee.",
            refusal);
    }

    [Fact]
    public void AnEntrustmentOnlyReview_AtAGeneralPanelsSemester2Sitting_IsRefused()
    {
        Assert.Equal(
            CommitteeReviewTypes.EntrustmentOnlyNotAtSemester2,
            CommitteeReviewTypes.Refusal(CommitteeReviewType.EntrustmentOnly, decisionBodyName: null, semester: 2));
        Assert.Null(CommitteeReviewTypes.Refusal(CommitteeReviewType.EntrustmentOnly, decisionBodyName: null, semester: 1));
        Assert.Null(CommitteeReviewTypes.Refusal(CommitteeReviewType.EntrustmentOnly, "Neonatal CCC", semester: 2));
        Assert.Equal(CommitteeReviewTypes.UnknownType, CommitteeReviewTypes.Refusal((CommitteeReviewType)99, null, 1));
    }

    [Fact]
    public void OnlyAnEntrustmentOnlyReview_DecidesNoProgression()
    {
        Assert.True(CommitteeReviewTypes.DecidesProgression(CommitteeReviewType.AnnualProgression));
        Assert.True(CommitteeReviewTypes.DecidesProgression(CommitteeReviewType.PreGraduation));
        Assert.False(CommitteeReviewTypes.DecidesProgression(CommitteeReviewType.EntrustmentOnly));
    }

    // ─── Recording and ratifying ─────────────────────────────────────────────

    [Fact]
    public void AnEntrustmentOnlyReview_RecordsItsDecisionWithNoCategory_AndIsRatified()
    {
        var review = StartedReview(CommitteeReviewType.EntrustmentOnly);

        var decision = review.RecordDecision(null, "PAED-004 entrusted at 3a.", null, "chair-1", Now, CommitteeQuorumFixture.ChairAndMember);
        review.Ratify("chair-1", Now);

        Assert.Null(decision.Category);
        Assert.Equal(CommitteeReviewState.Ratified, review.State);
    }

    [Fact]
    public void AnEntrustmentOnlyReview_RefusesAProgressionCategory_AndChangesNothing()
    {
        var review = StartedReview(CommitteeReviewType.EntrustmentOnly);

        var refusal = Assert.Throws<InvalidOperationException>(() => review.RecordDecision(
            CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, "chair-1", Now, CommitteeQuorumFixture.ChairAndMember));

        Assert.Equal(CommitteeReview.EntrustmentOnlyRecordsNoCategory, refusal.Message);
        Assert.Empty(review.Decisions);
        Assert.Equal(CommitteeReviewState.InProgress, review.State);
    }

    [Theory]
    [InlineData(CommitteeReviewType.AnnualProgression)]
    [InlineData(CommitteeReviewType.PreGraduation)]
    public void AProgressionReview_StillNeedsACategory_AndChangesNothingWithoutOne(CommitteeReviewType type)
    {
        var review = StartedReview(type);

        var refusal = Assert.Throws<InvalidOperationException>(() => review.RecordDecision(
            null, "On track.", null, "chair-1", Now, CommitteeQuorumFixture.ChairAndMember));

        Assert.Equal(CommitteeReview.ProgressionNeedsACategory, refusal.Message);
        Assert.Empty(review.Decisions);
        Assert.Equal(CommitteeReviewState.InProgress, review.State);
    }

    [Fact]
    public void AProgressionReview_RefusesACategoryThatIsNoneOfTheCategories()
    {
        var review = StartedReview(CommitteeReviewType.AnnualProgression);

        Assert.Throws<InvalidOperationException>(() => review.RecordDecision(
            (CommitteeDecisionCategory)99, "On track.", null, "chair-1", Now, CommitteeQuorumFixture.ChairAndMember));

        Assert.Empty(review.Decisions);
    }

    [Fact]
    public void Ratify_RefusesAProgressionReviewWhoseDecisionRecordsNoCategory_AndStampsNothing()
    {
        // Recording refuses it; a decision stored otherwise (a row written before this rule, or a review's type changed
        // under it) is what ratify's own branch is for.
        var review = StartedReview(CommitteeReviewType.AnnualProgression);
        review.RecordDecision(CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, "chair-1", Now, CommitteeQuorumFixture.ChairAndMember);
        ReplaceTheDecisionWith(review, category: null);

        var refusal = Assert.Throws<InvalidOperationException>(() => review.Ratify("chair-1", Now));

        Assert.StartsWith("This review decides the trainee's progression, but its decision records no progression category",
            refusal.Message, StringComparison.Ordinal);
        Assert.Equal(CommitteeReviewState.Decided, review.State);
        Assert.Null(review.RatifiedOn);
    }

    [Fact]
    public void Ratify_RefusesAnEntrustmentOnlyReviewWhoseDecisionRecordsACategory_AndStampsNothing()
    {
        var review = StartedReview(CommitteeReviewType.EntrustmentOnly);
        review.RecordDecision(null, "PAED-004 entrusted at 3a.", null, "chair-1", Now, CommitteeQuorumFixture.ChairAndMember);
        ReplaceTheDecisionWith(review, CommitteeDecisionCategory.SatisfactoryProgress);

        var refusal = Assert.Throws<InvalidOperationException>(() => review.Ratify("chair-1", Now));

        Assert.StartsWith("This review decides entrustment only, but its decision records a progression category",
            refusal.Message, StringComparison.Ordinal);
        Assert.Equal(CommitteeReviewState.Decided, review.State);
        Assert.Null(review.RatifiedOn);
    }

    // ─── An appeal that remits the decision ──────────────────────────────────

    [Fact]
    public void RemittingAnEntrustmentOnlyReview_ReplacesItsDecisionWithAnotherThatRecordsNoCategory()
    {
        var review = AppealedReview(CommitteeReviewType.EntrustmentOnly);
        var original = review.GetCurrentDecision()!;

        review.ResolveAppeal(
            CommitteeAppealOutcome.Remitted, "external-1", Now.AddDays(30), null, "Re-read on appeal: the level stands.", null,
            CommitteeQuorumFixture.ChairAndExternal);

        Assert.Equal(CommitteeReviewState.Final, review.State);
        var replacement = review.GetCurrentDecision()!;
        Assert.NotSame(original, replacement);
        Assert.Null(replacement.Category);
        Assert.Equal(CommitteeAppealOutcome.Remitted, review.Appeals.Single().Outcome);
    }

    [Fact]
    public void RemittingAnEntrustmentOnlyReview_WithAProgressionCategory_IsRefused_AndLeavesTheAppealOpen()
    {
        var review = AppealedReview(CommitteeReviewType.EntrustmentOnly);

        var refusal = Assert.Throws<InvalidOperationException>(() => review.ResolveAppeal(
            CommitteeAppealOutcome.Remitted, "external-1", Now, CommitteeDecisionCategory.SatisfactoryProgress, "Lifted.", null,
            CommitteeQuorumFixture.ChairAndExternal));

        Assert.Equal(CommitteeReview.EntrustmentOnlyRecordsNoCategory, refusal.Message);
        AssertAppealStillOpen(review);
    }

    [Fact]
    public void RemittingAProgressionReview_WithNoCategory_IsRefused_AndLeavesTheAppealOpen()
    {
        var review = AppealedReview(CommitteeReviewType.AnnualProgression);

        var refusal = Assert.Throws<InvalidOperationException>(() => review.ResolveAppeal(
            CommitteeAppealOutcome.Remitted, "external-1", Now, null, "Lifted.", null, CommitteeQuorumFixture.ChairAndExternal));

        Assert.Equal(CommitteeReview.ProgressionNeedsACategory, refusal.Message);
        AssertAppealStillOpen(review);
    }

    [Theory]
    [InlineData(CommitteeAppealOutcome.Upheld)]
    [InlineData(CommitteeAppealOutcome.Dismissed)]
    public void UpholdingOrDismissingAnEntrustmentOnlyReviewsAppeal_LeavesItsDecisionStanding(CommitteeAppealOutcome outcome)
    {
        var review = AppealedReview(CommitteeReviewType.EntrustmentOnly);

        review.ResolveAppeal(outcome, "chair-1", Now);

        Assert.Equal(CommitteeReviewState.Final, review.State);
        Assert.Null(Assert.Single(review.Decisions).Category);
    }

    // ─── An agenda with nothing on it ────────────────────────────────────────

    [Fact]
    public void AnEntrustmentOnlyReview_WithNothingOnItsAgenda_CannotBeRecorded()
    {
        // Its decision is what its agenda holds; with nothing on it, the decision would be a rationale about nothing.
        var review = StartedReview(CommitteeReviewType.EntrustmentOnly);

        var recording = Assert.Throws<InvalidOperationException>(() => review.EnsureAgendaSettled([]));

        Assert.Equal("The committee's decision cannot be recorded yet: " + CommitteeReview.NothingOnTheAgenda, recording.Message);
        Assert.Equal(CommitteeReview.NothingOnTheAgenda, review.EmptyAgendaRefusal());
        Assert.Equal(CommitteeReviewState.InProgress, review.State);
    }

    [Fact]
    public void AnEntrustmentOnlyReview_WhoseAgendaEmptiedAfterItWasDecided_IsStillRatified()
    {
        // T167's exception can remove the one staged STAR, and its chair's line, after the decision is recorded. The review
        // cannot go back to stage another, so ratify does not ask again: it would leave the review decided for good.
        var review = StartedReview(CommitteeReviewType.EntrustmentOnly);
        review.RecordDecision(null, "PAED-004 entrusted at 3a.", null, "chair-1", Now, CommitteeQuorumFixture.ChairAndMember);

        review.EnsureAgendaClosable([]);
        review.Ratify("chair-1", Now);

        Assert.Equal(CommitteeReviewState.Ratified, review.State);
    }

    [Fact]
    public void AnEntrustmentOnlyReview_WithAnOptionalLineNothingWasStagedOn_IsRecordedAndRatified()
    {
        // A line the College makes optional is still decided when the review closes: NotDecided. Only an empty agenda is
        // refused, never an agenda whose lines were left.
        var review = StartedReview(CommitteeReviewType.EntrustmentOnly, [OpportunisticLine()]);

        review.EnsureAgendaSettled([]);
        review.RecordDecision(null, "Nothing on PAED-009 this semester.", null, "chair-1", Now, CommitteeQuorumFixture.ChairAndMember);
        review.Ratify("chair-1", Now);
        review.CloseAgenda(new Dictionary<int, Wombat.Domain.EntrustmentDecisions.EntrustmentDecision>());

        Assert.Null(review.EmptyAgendaRefusal());
        Assert.Equal(CommitteeAgendaLineState.NotDecided, review.AgendaLines.Single().State);
    }

    [Theory]
    [InlineData(CommitteeReviewType.AnnualProgression)]
    [InlineData(CommitteeReviewType.PreGraduation)]
    public void AProgressionReview_WithNothingOnItsAgenda_IsNotRefusedForIt(CommitteeReviewType type)
    {
        // Its decision is its category.
        var review = StartedReview(type);

        review.EnsureAgendaSettled([]);

        Assert.Null(review.EmptyAgendaRefusal());
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    /// <summary>PAED-009, decided as opportunity allows (O7): an optional line at the 2026 S1 sitting.</summary>
    private static CommitteeAgendaLine OpportunisticLine()
        => CommitteeAgendaLine.ForCadence(
            9, 9, "PAED-009", "Adolescents", isOpportunistic: true,
            Wombat.Domain.Curricula.QuotaWindow.For(Wombat.Domain.Curricula.QuotaPeriod.Semester, Semester1.End, new DateOnly(2025, 1, 15)),
            Semester1);

    private static readonly Wombat.Domain.Curricula.AcademicPeriod Semester1 = new(2026, 1);

    private static void AssertAppealStillOpen(CommitteeReview review)
    {
        Assert.Equal(CommitteeReviewState.UnderAppeal, review.State);
        Assert.Single(review.Decisions);
        Assert.Null(review.Appeals.Single().ResolvedOn);
        Assert.Null(review.FinalizedOn);
    }

    /// <summary>Swaps the recorded decision for one with <paramref name="category" />, as a row stored otherwise would be.</summary>
    private static void ReplaceTheDecisionWith(CommitteeReview review, CommitteeDecisionCategory? category)
    {
        review.Decisions.Clear();
        review.Decisions.Add(CommitteeDecision.Create(
            category, "Stored otherwise.", null, "chair-1", Now, CommitteeQuorumFixture.ChairAndMember));
    }

    private static CommitteeReview StartedReview(CommitteeReviewType type, IReadOnlyList<CommitteeAgendaLine>? agenda = null)
    {
        var review = new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 1,
            TraineeUserId = "trainee-1",
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 6, 30),
            ScheduledOn = new DateOnly(2026, 7, 2),
            ReviewType = type
        };

        review.Start([], agenda ?? [], "chair-1", Now);
        return review;
    }

    private static CommitteeReview AppealedReview(CommitteeReviewType type)
    {
        var review = StartedReview(type);
        review.RecordDecision(
            type == CommitteeReviewType.EntrustmentOnly ? null : CommitteeDecisionCategory.SatisfactoryProgress,
            "Decided.", null, "chair-1", Now, CommitteeQuorumFixture.ChairAndMember);
        review.Ratify("chair-1", Now);
        review.LodgeAppeal("The level understates my supervised practice.", "trainee-1", Now);
        return review;
    }
}
