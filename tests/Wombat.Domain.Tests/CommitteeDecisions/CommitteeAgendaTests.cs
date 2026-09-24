using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Domain.Tests.CommitteeDecisions;

/// <summary>
/// A review's agenda (T131 slice 4): which lines are closing (Decisions 5 and 9, O7), and how the agenda moves as the
/// chair stages, defers and ratifies (Decision 6).
/// </summary>
public sealed class CommitteeAgendaTests
{
    private static readonly DateOnly OnTime = new(2025, 1, 15);
    private static readonly AcademicPeriod Semester1 = new(2026, 1);
    private static readonly AcademicPeriod Semester2 = new(2026, 2);

    // ---- Closing (Decisions 5 and 9, O7) ------------------------------------------------------------------------------

    [Fact]
    public void ASemesterLine_IsClosingAtEitherSemestersSitting()
    {
        foreach (var sitting in new[] { Semester1, Semester2 })
        {
            var line = Cadence(QuotaPeriod.Semester, sitting);

            Assert.True(line.IsClosing);
            Assert.False(line.IsPartialPeriod);
            Assert.Equal(2026, line.WindowYear);
            Assert.Equal(sitting.Semester, line.WindowSemester);
            Assert.Equal(sitting.ToString(), line.WindowLabel);
            Assert.Equal(CommitteeAgendaLineOrigin.Cadence, line.Origin);
            Assert.Equal(CommitteeAgendaLineState.Due, line.State);
        }
    }

    [Fact]
    public void AnAnnualLine_IsOptionalAtTheSemester1Sitting_AndClosingAtTheSemester2Sitting()
    {
        var atSemester1 = Cadence(QuotaPeriod.AcademicYear, Semester1);
        var atSemester2 = Cadence(QuotaPeriod.AcademicYear, Semester2);

        Assert.False(atSemester1.IsClosing);
        Assert.True(atSemester2.IsClosing);
        Assert.Null(atSemester1.WindowSemester);
        Assert.Equal("2026", atSemester1.WindowLabel);
        Assert.Equal(new DateOnly(2026, 12, 31), atSemester1.WindowEnd);
    }

    [Fact]
    public void AnEpaDecidedAsOpportunityAllows_IsNeverClosing()
    {
        Assert.False(Cadence(QuotaPeriod.Semester, Semester1, opportunistic: true).IsClosing);
        Assert.False(Cadence(QuotaPeriod.AcademicYear, Semester2, opportunistic: true).IsClosing);
    }

    [Fact]
    public void AWindowTheTraineeJoinedPartWayThrough_KeepsItsLine_LabelledAndOptional()
    {
        // A start after the semester's first month exempts the semester (D42), and a start in semester 2 the year.
        var lateInSemester1 = new DateOnly(2026, 3, 1);
        var semesterLine = Cadence(QuotaPeriod.Semester, Semester1, programmeStart: lateInSemester1);
        Assert.True(semesterLine.IsPartialPeriod);
        Assert.False(semesterLine.IsClosing);

        var annualForAnAprilStarter = Cadence(QuotaPeriod.AcademicYear, Semester2, programmeStart: new DateOnly(2026, 4, 1));
        Assert.False(annualForAnAprilStarter.IsPartialPeriod);
        Assert.True(annualForAnAprilStarter.IsClosing, "an April starter has eight months for an annual EPA");

        var annualForASemester2Starter = Cadence(QuotaPeriod.AcademicYear, Semester2, programmeStart: new DateOnly(2026, 8, 3));
        Assert.True(annualForASemester2Starter.IsPartialPeriod);
        Assert.False(annualForASemester2Starter.IsClosing);
    }

    [Fact]
    public void AWindowTheTraineeHadNotStarted_IsNotDue()
    {
        var window = QuotaWindow.For(QuotaPeriod.Semester, Semester1.End, new DateOnly(2026, 8, 3));

        Assert.False(CommitteeAgendaLine.IsDueAt(window, Semester1));
        Assert.Throws<ArgumentException>(() => CommitteeAgendaLine.ForCadence(1, 1, "PAED-001", "Ward round", false, window, Semester1));
    }

    [Fact]
    public void AWindowThatDoesNotHoldTheSitting_IsNotDue()
    {
        var semester1Window = QuotaWindow.For(QuotaPeriod.Semester, Semester1.End, OnTime);

        Assert.False(CommitteeAgendaLine.IsDueAt(semester1Window, Semester2));
    }

    [Fact]
    public void AChairsLine_IsNeverClosing()
    {
        var line = CommitteeAgendaLine.ForChair(1, 1, "PAED-020", "Research", QuotaWindow.For(QuotaPeriod.Semester, Semester2.End, OnTime));

        Assert.Equal(CommitteeAgendaLineOrigin.Chair, line.Origin);
        Assert.False(line.IsClosing);
        Assert.Equal(2, line.WindowSemester);
    }

    // ---- Deferring ----------------------------------------------------------------------------------------------------

    [Fact]
    public void Deferring_NeedsAReason_AndReinstatingClearsIt()
    {
        var line = Cadence(QuotaPeriod.Semester, Semester1);

        Assert.Throws<InvalidOperationException>(() => line.Defer("   "));
        Assert.Equal(CommitteeAgendaLineState.Due, line.State);

        line.Defer("  Rotation moved.  ");
        Assert.Equal(CommitteeAgendaLineState.Deferred, line.State);
        Assert.Equal("Rotation moved.", line.DeferralReason);
        Assert.False(line.BlocksRatify(staged: false), "a deferred line lets the review through");
        Assert.Throws<InvalidOperationException>(() => line.Defer("Again."));

        line.Reinstate();
        Assert.Equal(CommitteeAgendaLineState.Due, line.State);
        Assert.Null(line.DeferralReason);
        Assert.Throws<InvalidOperationException>(() => line.Reinstate());
    }

    // ---- The review's agenda ------------------------------------------------------------------------------------------

    [Fact]
    public void CadenceLines_ArePlannedOnlyBeforeTheReviewStarts_AndPlanningOnlyAdds()
    {
        var review = Review(Semester1);
        review.AddCadenceLines([Cadence(QuotaPeriod.Semester, Semester1, epaId: 1, code: "PAED-001")]);
        review.AgendaLineFor(1)!.Defer("Kept as it stands.");

        // Start plans again: PAED-001 keeps its line and state, PAED-002 joins.
        review.Start(
            [],
            [Cadence(QuotaPeriod.Semester, Semester1, epaId: 1, code: "PAED-001"), Cadence(QuotaPeriod.Semester, Semester1, epaId: 2, code: "PAED-002")],
            "chair-1",
            DateTime.UtcNow);

        Assert.Equal(new[] { 1, 2 }, review.AgendaLines.Select(line => line.EpaId).Order());
        Assert.Equal(CommitteeAgendaLineState.Deferred, review.AgendaLineFor(1)!.State);
        Assert.Throws<InvalidOperationException>(() => review.AddCadenceLines([Cadence(QuotaPeriod.Semester, Semester1, epaId: 3, code: "PAED-003")]));
    }

    [Fact]
    public void AFormativeReview_CarriesNoAgenda()
    {
        var review = Review(Semester1, formative: true);

        Assert.Throws<InvalidOperationException>(() => review.AddCadenceLines([Cadence(QuotaPeriod.Semester, Semester1)]));
        review.AddCadenceLines([]);
        review.Start([], [], "chair-1", DateTime.UtcNow);
        Assert.Empty(review.AgendaLines);
    }

    [Fact]
    public void AChairsLine_IsAddedOnlyWhileTheReviewSits_AndOnlyForAnEpaNotOnTheAgenda()
    {
        var review = Review(Semester1);
        var chairs = CommitteeAgendaLine.ForChair(9, 9, "PAED-009", "Adolescents", QuotaWindow.For(QuotaPeriod.Semester, Semester1.End, OnTime));
        Assert.Throws<InvalidOperationException>(() => review.AddChairLine(chairs));

        review.Start([], [Cadence(QuotaPeriod.Semester, Semester1, epaId: 1, code: "PAED-001")], "chair-1", DateTime.UtcNow);
        review.AddChairLine(chairs);
        Assert.Throws<InvalidOperationException>(() => review.AddChairLine(
            CommitteeAgendaLine.ForChair(1, 1, "PAED-001", "Ward round", QuotaWindow.For(QuotaPeriod.Semester, Semester1.End, OnTime))));
        Assert.Throws<ArgumentException>(() => review.AddChairLine(Cadence(QuotaPeriod.Semester, Semester1, epaId: 5, code: "PAED-005")));

        Assert.Null(review.RemoveChairLineFor(1));
        Assert.Same(chairs, review.RemoveChairLineFor(9));
        Assert.Null(review.AgendaLineFor(9));
    }

    [Fact]
    public void Ratify_IsRefusedWhileAClosingLineIsNeitherStagedNorDeferred_AndNamesThem()
    {
        var review = StartedReviewWithFourLines();

        var refusal = Assert.Throws<InvalidOperationException>(() => review.EnsureAgendaClosable([]));
        Assert.Equal(
            "This review cannot be ratified: PAED-001 and PAED-002 must be decided at this sitting. Stage a decision on " +
            "each, or defer it with a reason.",
            refusal.Message);

        review.EnsureAgendaClosable([1, 2]);
        review.AgendaLineFor(2)!.Defer("Not observed this semester.");
        review.EnsureAgendaClosable([1]);
        Assert.Equal(
            "PAED-001 must be decided at this sitting. Stage a decision on it, or defer it with a reason.",
            CommitteeReview.OutstandingClosingLinesReason(review.OutstandingClosingLines([]).Select(line => line.EpaCode).ToArray()));
    }

    [Fact]
    public void ClosingTheAgenda_DecidesTheStagedLines_LeavesTheDeferredOnes_AndClosesTheOptionalOnesUndecided()
    {
        var review = StartedReviewWithFourLines();
        review.AgendaLineFor(2)!.Defer("Not observed this semester.");
        var star = EntrustmentDecision.Issue(
            "trainee-1", 1, 3, new DateOnly(2026, 7, 2), null, 30, "chair-1", "Consistent.",
            [EntrustmentEvidenceLink.FromSnapshot(new CommitteeEvidence { Id = 7, SourceType = CommitteeEvidenceSourceType.Activity, ActivityId = 5, SourceLabel = "Mini-CEX #5", Summary = "State: completed." })]);

        review.CloseAgenda(new Dictionary<int, EntrustmentDecision> { [1] = star });

        Assert.Equal(CommitteeAgendaLineState.Decided, review.AgendaLineFor(1)!.State);
        Assert.Same(star, review.AgendaLineFor(1)!.EntrustmentDecision);
        Assert.Equal(CommitteeAgendaLineState.Deferred, review.AgendaLineFor(2)!.State);
        Assert.Equal(CommitteeAgendaLineState.NotDecided, review.AgendaLineFor(3)!.State);
        Assert.Equal(CommitteeAgendaLineState.NotDecided, review.AgendaLineFor(8)!.State);
    }

    [Fact]
    public void ClosingTheAgenda_WithAClosingLineOutstanding_ChangesNothing()
    {
        var review = StartedReviewWithFourLines();

        Assert.Throws<InvalidOperationException>(() => review.CloseAgenda(new Dictionary<int, EntrustmentDecision>()));
        Assert.All(review.AgendaLines, line => Assert.Equal(CommitteeAgendaLineState.Due, line.State));
    }

    // ---- Fixture ------------------------------------------------------------------------------------------------------

    /// <summary>A semester-1 sitting: PAED-001 and 002 closing (semester), 003 due by year end, 008 as opportunity allows.</summary>
    private static CommitteeReview StartedReviewWithFourLines()
    {
        var review = Review(Semester1);
        review.Start(
            [],
            [
                Cadence(QuotaPeriod.Semester, Semester1, epaId: 1, code: "PAED-001"),
                Cadence(QuotaPeriod.Semester, Semester1, epaId: 2, code: "PAED-002"),
                Cadence(QuotaPeriod.AcademicYear, Semester1, epaId: 3, code: "PAED-003"),
                Cadence(QuotaPeriod.Semester, Semester1, epaId: 8, code: "PAED-008", opportunistic: true)
            ],
            "chair-1",
            DateTime.UtcNow);
        return review;
    }

    private static CommitteeReview Review(AcademicPeriod period, bool formative = false)
        => new()
        {
            AcademicYear = period.Year,
            Semester = period.Semester,
            TraineeUserId = "trainee-1",
            PanelId = 20,
            ReviewPeriodFrom = period.Start,
            ReviewPeriodTo = period.End,
            ScheduledOn = period.End,
            IsFormative = formative
        };

    private static CommitteeAgendaLine Cadence(
        QuotaPeriod cadence,
        AcademicPeriod sitting,
        bool opportunistic = false,
        DateOnly? programmeStart = null,
        int epaId = 1,
        string code = "PAED-001")
        => CommitteeAgendaLine.ForCadence(
            epaId + 100,
            epaId,
            code,
            $"EPA {epaId}",
            opportunistic,
            QuotaWindow.For(cadence, sitting.End, programmeStart ?? OnTime),
            sitting);
}
