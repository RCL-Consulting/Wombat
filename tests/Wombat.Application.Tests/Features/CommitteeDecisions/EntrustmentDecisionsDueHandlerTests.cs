using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.CommitteeDecisions;

/// <summary>
/// T131 slice 6: the decisions-due page lists, per trainee at the caller's institution, which EPAs are due in a period and
/// where each decision stands, from the agenda lines, the STARs and each EPA's cadence; "missed" is computed.
/// </summary>
/// <remarks>
/// The curriculum is the agenda tests' miniature of v11.1: PAED-001, 002, 004 and 005 decided each semester (004 and 005 by
/// the neonatal committee), 003 annually, 008 annually as opportunity allows, 020 with no cadence, 030 another
/// institution's local item, 040's EPA deactivated. A Surgery trainee at A follows a curriculum of one EPA. Today is
/// 24 September 2026: semester 1 has ended, semester 2 and the year have not.
/// </remarks>
public sealed class EntrustmentDecisionsDueHandlerTests
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;
    private const int GeneralPanel = 10;
    private const int NeonatalPanel = 11;
    private const int PanelAtB = 20;
    private const int PaedsCurriculum = 100;
    private const int SurgeryCurriculum = 200;
    private const int Paediatrics = 1;
    private const int Surgery = 2;
    private const int GeneralPaediatrics = 11;
    private const int GeneralSurgery = 21;
    private const string Trainee = "trainee-a";
    private const string LateStarter = "trainee-late";
    private const string FutureStarter = "trainee-future";
    private const string Leaver = "trainee-left";
    private const string SurgeryTrainee = "trainee-surgery";
    private const string TraineeAtB = "trainee-b";
    private const string Neonatal = "neonatal";
    private const int Level = 3;

    private static readonly DateOnly Today = new(2026, 9, 24);

    private readonly string _databaseName = Guid.NewGuid().ToString();

    // ---- What is due, with nothing on record ------------------------------------------------------------------------

    [Fact]
    public async Task WithNothingOnRecord_EachEpaDueByCadence_IsNotScheduled_AndAnOpportunisticOneIsOptional()
    {
        await using var db = await SeededDbAsync();

        var due = await DueAsync(db, 2026, 2);

        due.PeriodLabel.Should().Be("2026 S2");
        due.InstitutionName.Should().Be("A");
        Rows(due, Trainee).Should().Equal(
            ("PAED-001", "2026 S2", EntrustmentDecisionDueStatus.NotScheduled),
            ("PAED-002", "2026 S2", EntrustmentDecisionDueStatus.NotScheduled),
            ("PAED-003", "2026", EntrustmentDecisionDueStatus.NotScheduled),
            ("PAED-004", "2026 S2", EntrustmentDecisionDueStatus.NotScheduled),
            ("PAED-005", "2026 S2", EntrustmentDecisionDueStatus.NotScheduled),
            ("PAED-008", "2026", EntrustmentDecisionDueStatus.AsOpportunityAllows));
        Row(due, Trainee, "PAED-001").TraineeName.Should().Be("Ada Trainee");
        Row(due, Trainee, "PAED-001").ReviewId.Should().BeNull();
        Row(due, Trainee, "PAED-004").SchedulePanelId.Should().Be(GeneralPanel, "no neonatal panel covers the trainee");
        Row(due, Trainee, "PAED-004").SchedulePanelName.Should().Be("General CCC");
    }

    [Fact]
    public async Task ForAPeriodThatHasEnded_WhatNothingDecidedIsMissed_ButNotAnAnnualEpaBeforeYearEnd_NorAnOptionalOne()
    {
        await using var db = await SeededDbAsync();

        var due = await DueAsync(db, 2026, 1);

        Status(due, Trainee, "PAED-001").Should().Be(EntrustmentDecisionDueStatus.Missed);
        Status(due, Trainee, "PAED-003").Should().Be(EntrustmentDecisionDueStatus.DueByYearEnd, "its window is the whole year");
        Status(due, Trainee, "PAED-008").Should().Be(EntrustmentDecisionDueStatus.AsOpportunityAllows, "never missed (O7)");
        Status(due, LateStarter, "PAED-001").Should().Be(
            EntrustmentDecisionDueStatus.PartialPeriod, "they joined semester 1 part-way through (Decision 9)");
        Status(due, LateStarter, "PAED-003").Should().Be(
            EntrustmentDecisionDueStatus.DueByYearEnd, "a start in semester 1 is on time for the year");
    }

    [Fact]
    public async Task AnAnnualEpa_IsMissedOnlyOnceItsYearHasEnded()
    {
        await using var db = await SeededDbAsync();

        var due = await DueAsync(db, 2026, 2, today: new DateOnly(2027, 1, 5));

        Status(due, Trainee, "PAED-003").Should().Be(EntrustmentDecisionDueStatus.Missed);
        Status(due, Trainee, "PAED-001").Should().Be(EntrustmentDecisionDueStatus.Missed);
        Status(due, Trainee, "PAED-008").Should().Be(EntrustmentDecisionDueStatus.AsOpportunityAllows);
    }

    [Fact]
    public async Task OnlyItemsWithACadence_OnTheTraineesCurriculum_InForce_AreDue()
    {
        await using var db = await SeededDbAsync();

        var codes = (await DueAsync(db, 2026, 2)).Items
            .Where(item => item.TraineeUserId == Trainee)
            .Select(item => item.EpaCode);

        codes.Should().NotContain(["PAED-020", "PAED-030", "PAED-040"],
            "020 has no cadence, 030 is another institution's local item, and 040's EPA is deactivated");
    }

    [Fact]
    public async Task ATraineeWhoHadNotStartedByThePeriodsEnd_OwesNothingInIt()
    {
        await using var db = await SeededDbAsync();

        (await DueAsync(db, 2026, 2)).Items.Should().NotContain(item => item.TraineeUserId == FutureStarter);
        (await DueAsync(db, 2027, 1, today: new DateOnly(2027, 3, 1))).Items
            .Should().Contain(item => item.TraineeUserId == FutureStarter);
    }

    [Fact]
    public async Task ATraineeWhoseProgrammeHasEnded_IsNotListed()
    {
        await using var db = await SeededDbAsync();

        (await DueAsync(db, 2026, 2)).Items.Should().NotContain(item => item.TraineeUserId == Leaver);
    }

    [Fact]
    public async Task AnErasedTraineesPseudonym_AndAProfileThatOutlivedItsTrainee_AreNotListed()
    {
        // T238. An erasure left the profile active at A under a pseudonym until T258 (ErasureExecutor), and a
        // profile can outlive its user's Trainee role. Listed, each would owe decisions, the first named by its bare
        // pseudonym, and each would be offered a Schedule link the scheduling handler refuses.
        await using var db = await SeededDbAsync();
        db.TraineeProfiles.AddRange(
            Profile(7, "deleted_user_7c0ffee1", InstitutionA, new DateOnly(2025, 1, 15)),
            Profile(8, "former-a", InstitutionA, new DateOnly(2025, 1, 15)));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var directory = Names()
            .WithTrainees(Trainee, LateStarter, FutureStarter, Leaver, SurgeryTrainee, TraineeAtB)
            .With(new UserIdentityDetails("former-a", "former-a@test", "Fezile", "Former", InstitutionA, [], [], [WombatRoles.Assessor]));

        var due = await new GetEntrustmentDecisionsDueQueryHandler(db, directory).Handle(
            new GetEntrustmentDecisionsDueQuery(2026, 2, null, TestPrincipals.Coordinator(InstitutionA), Today),
            CancellationToken.None);

        due!.Items.Select(item => item.TraineeUserId).Distinct()
            .Should().BeEquivalentTo([Trainee, LateStarter, SurgeryTrainee]);
    }

    // ---- Where a decision stands ------------------------------------------------------------------------------------

    [Fact]
    public async Task AStarFromASittingInTheWindow_DecidesIt_AndADeferredLine_ReadsDeferredEvenOnceTheWindowHasEnded()
    {
        await using var db = await SeededDbAsync();
        var review = await DecideAsync(db, 2026, 1, "PAED-001");
        var star = await db.EntrustmentDecisions.AsNoTracking().SingleAsync();

        var due = await DueAsync(db, 2026, 1);

        var decided = Row(due, Trainee, "PAED-001");
        (decided.Status, decided.ReviewId, decided.EntrustmentDecisionId).Should().Be(
            (EntrustmentDecisionDueStatus.Decided, review, star.Id));
        decided.MayOpenReview.Should().BeTrue("a Coordinator reads the reviews of their institution's panels");
        var deferred = Row(due, Trainee, "PAED-002");
        (deferred.Status, deferred.ReviewId).Should().Be((EntrustmentDecisionDueStatus.Deferred, review));
        Status(due, Trainee, "PAED-003").Should().Be(
            EntrustmentDecisionDueStatus.DueByYearEnd, "optional at semester 1, and closed Not decided there");
    }

    [Fact]
    public async Task ASemesterDecision_DecidesOnlyItsSemester_AndAnAnnualOne_TheWholeYear()
    {
        await using var db = await SeededDbAsync();
        var review = await DecideAsync(db, 2026, 1, "PAED-001", "PAED-003");

        var due = await DueAsync(db, 2026, 2);

        Status(due, Trainee, "PAED-001").Should().Be(EntrustmentDecisionDueStatus.NotScheduled, "semester 2 is its own window");
        var annual = Row(due, Trainee, "PAED-003");
        (annual.Status, annual.ReviewId).Should().Be((EntrustmentDecisionDueStatus.Decided, review));
    }

    [Fact]
    public async Task ASupersededStar_StillDecidesItsWindow()
    {
        await using var db = await SeededDbAsync();
        var first = await DecideAsync(db, 2026, 1, "PAED-001");
        await DecideAsync(db, 2026, 2, "PAED-001");
        var stars = await db.EntrustmentDecisions.AsNoTracking().OrderBy(star => star.Id).ToListAsync();
        stars.Select(star => star.Status).Should().Equal(EntrustmentDecisionStatus.Superseded, EntrustmentDecisionStatus.Active);

        var semester1 = Row(await DueAsync(db, 2026, 1), Trainee, "PAED-001");
        var semester2 = Row(await DueAsync(db, 2026, 2), Trainee, "PAED-001");

        (semester1.Status, semester1.ReviewId, semester1.EntrustmentDecisionId).Should().Be(
            (EntrustmentDecisionDueStatus.Decided, first, stars[0].Id));
        (semester2.Status, semester2.EntrustmentDecisionId).Should().Be((EntrustmentDecisionDueStatus.Decided, stars[1].Id));
    }

    [Fact]
    public async Task ARevokedStar_ReadsRevokedReDecide_NotMissed_AndNamesTheStarAndItsReview()
    {
        await using var db = await SeededDbAsync();
        var review = await DecideAsync(db, 2026, 1, "PAED-001");
        var star = await db.EntrustmentDecisions.SingleAsync();
        star.Revoke("Issued in error.", "admin-user", DateTime.UtcNow);
        await SaveAndClearAsync(db);

        var row = Row(await DueAsync(db, 2026, 1), Trainee, "PAED-001");

        (row.Status, row.ReviewId, row.EntrustmentDecisionId).Should().Be(
            (EntrustmentDecisionDueStatus.Revoked, review, star.Id));
        row.IsOutstanding.Should().BeTrue();
    }

    [Fact]
    public async Task AnExpiredStar_StillDecidesItsWindow_AndIsNotPlannedAgain()
    {
        // Decision 10: expiry is informational. The decision was taken in its window; its lapsing neither calls the window
        // undecided nor puts the EPA back on a sitting's agenda (T131 slice 6 review).
        await using var db = await SeededDbAsync();
        var review = await DecideAsync(db, 2026, 1, "PAED-001", expiresOn: new DateOnly(2026, 8, 1));
        var star = await db.EntrustmentDecisions.SingleAsync();
        star.MarkExpired(new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc));
        await SaveAndClearAsync(db);

        var row = Row(await DueAsync(db, 2026, 1), Trainee, "PAED-001");
        (row.Status, row.ReviewId, row.EntrustmentDecisionId).Should().Be(
            (EntrustmentDecisionDueStatus.Decided, review, star.Id));

        var remediation = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        (await PlannedCodesAsync(db, remediation.Id)).Should().NotContain("PAED-001");
    }

    [Fact]
    public async Task ADeferral_OvertakenByALaterSittingsDecision_SinceRevoked_ReadsRevoked()
    {
        // Review 1 (2026 S1) defers PAED-002; a remediation sitting in the same period decides it; that STAR is revoked.
        await using var db = await SeededDbAsync();
        var first = await DecideAsync(db, 2026, 1, "PAED-001");
        var second = await DecideAsync(db, 2026, 1, "PAED-002");
        var star = await RevokeAsync(db, "PAED-002");

        var row = Row(await DueAsync(db, 2026, 1), Trainee, "PAED-002");

        second.Should().BeGreaterThan(first);
        (row.Status, row.ReviewId, row.EntrustmentDecisionId).Should().Be(
            (EntrustmentDecisionDueStatus.Revoked, second, star.Id), "the latest thing a sitting did was decide it");
    }

    [Fact]
    public async Task ARevokedDecision_ThenALaterSittingsDeferral_ReadsDeferred()
    {
        await using var db = await SeededDbAsync();
        await DecideAsync(db, 2026, 1, "PAED-002");
        await RevokeAsync(db, "PAED-002");
        var second = await DecideAsync(db, 2026, 1, "PAED-001");

        var row = Row(await DueAsync(db, 2026, 1), Trainee, "PAED-002");

        (row.Status, row.ReviewId).Should().Be(
            (EntrustmentDecisionDueStatus.Deferred, second), "the sitting after the revocation deferred it, with a reason");
    }

    [Fact]
    public async Task ASupersededStar_WhoseSuccessorInTheWindowWasRevoked_DecidesNothing_AndIsPlannedAgain()
    {
        // STAR 1 on PAED-001 at a 2026 S1 sitting; a second S1 sitting issues STAR 2, superseding it; STAR 2 is revoked.
        // The window's decision was STAR 2's, and the trainee holds no standing STAR on the EPA.
        await using var db = await SeededDbAsync();
        await DecideAsync(db, 2026, 1, "PAED-001");
        var second = await DecideAsync(db, 2026, 1, "PAED-001");
        var revoked = await RevokeAsync(db, "PAED-001");
        (await db.EntrustmentDecisions.AsNoTracking().OrderBy(star => star.Id).Select(star => star.Status).ToListAsync())
            .Should().Equal(EntrustmentDecisionStatus.Superseded, EntrustmentDecisionStatus.Revoked);

        var row = Row(await DueAsync(db, 2026, 1), Trainee, "PAED-001");

        (row.Status, row.ReviewId, row.EntrustmentDecisionId).Should().Be(
            (EntrustmentDecisionDueStatus.Revoked, second, revoked.Id));
        row.SchedulePanelId.Should().Be(GeneralPanel, "a revoked decision offers Schedule");

        var third = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        (await PlannedCodesAsync(db, third.Id)).Should().Contain("PAED-001", "nothing decides its window now");
    }

    [Fact]
    public async Task ASupersededStar_WhoseSuccessorInTheWindowStands_IsDecidedByTheSuccessor()
    {
        await using var db = await SeededDbAsync();
        await DecideAsync(db, 2026, 1, "PAED-001");
        var second = await DecideAsync(db, 2026, 1, "PAED-001");
        var successor = await db.EntrustmentDecisions.AsNoTracking().OrderBy(star => star.Id).LastAsync();

        var row = Row(await DueAsync(db, 2026, 1), Trainee, "PAED-001");

        (row.Status, row.ReviewId, row.EntrustmentDecisionId).Should().Be(
            (EntrustmentDecisionDueStatus.Decided, second, successor.Id));
    }

    [Fact]
    public async Task AnEpaOnAnOpenReviewsAgenda_IsScheduled_AndNamesThatReview()
    {
        await using var db = await SeededDbAsync();
        var review = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 2);

        var due = await DueAsync(db, 2026, 2);

        var row = Row(due, Trainee, "PAED-001");
        (row.Status, row.ReviewId).Should().Be((EntrustmentDecisionDueStatus.Scheduled, review.Id));
        Status(due, Trainee, "PAED-008").Should().Be(
            EntrustmentDecisionDueStatus.Scheduled, "an optional line on an open review's agenda is scheduled too");
        Status(due, LateStarter, "PAED-001").Should().Be(EntrustmentDecisionDueStatus.NotScheduled, "only Ada's review is open");
    }

    [Fact]
    public async Task AStarNoAgendaLineRecords_StillDecidesItsWindow()
    {
        // T215: agenda lines are the record of what a sitting did; the STAR is the fact. A STAR ratified before agendas
        // existed has no line.
        await using var db = await SeededDbAsync();
        var review = await DecideAsync(db, 2026, 1, "PAED-001");
        db.CommitteeAgendaLines.RemoveRange(await db.CommitteeAgendaLines.ToListAsync());
        await SaveAndClearAsync(db);

        var row = Row(await DueAsync(db, 2026, 1), Trainee, "PAED-001");

        (row.Status, row.ReviewId).Should().Be((EntrustmentDecisionDueStatus.Decided, review));

        // The planner reads the page's rule, so a sitting for the period never plans what the page calls decided.
        var remediation = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        (await PlannedCodesAsync(db, remediation.Id)).Should().NotContain("PAED-001");
    }

    [Fact]
    public async Task ALineStillDueOnAReviewTheTraineeStranded_SaysNothingAtTheirNewInstitution_ButTheirStarsGoWithThem()
    {
        await using var db = await SeededDbAsync();
        await DecideAsync(db, 2026, 1, "PAED-001");
        await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 2);
        await MoveAsync(db, Trainee, InstitutionB);

        var semester2 = await DueAsync(db, 2026, 2, TestPrincipals.InstitutionalAdmin(InstitutionB));
        var semester1 = await DueAsync(db, 2026, 1, TestPrincipals.InstitutionalAdmin(InstitutionB));

        var row = Row(semester2, Trainee, "PAED-001");
        row.Status.Should().Be(EntrustmentDecisionDueStatus.NotScheduled, "A's open review holds it, but that review is stranded");
        row.SchedulePanelId.Should().Be(PanelAtB);
        Status(semester1, Trainee, "PAED-001").Should().Be(EntrustmentDecisionDueStatus.Decided, "a decision goes with the trainee");
        Status(semester1, Trainee, "PAED-002").Should().Be(
            EntrustmentDecisionDueStatus.Missed, "A's deferral says nothing about where the decision stands at B");
    }

    // ---- Where Schedule leads ----------------------------------------------------------------------------------------

    [Fact]
    public async Task TheSchedulePeriod_IsTheWindowsLastSemester_SoAnAnnualEpaSchedulesTheYearsLastSitting()
    {
        await using var db = await SeededDbAsync();

        var due = await DueAsync(db, 2026, 1);

        var annual = Row(due, Trainee, "PAED-003");
        (annual.SchedulePeriodKey, annual.SchedulePeriodLabel).Should().Be(("2026-2", "2026 S2"), "Decision 5");
        (Row(due, Trainee, "PAED-001").SchedulePeriodKey, Row(due, Trainee, "PAED-001").SchedulePeriodLabel)
            .Should().Be(("2026-1", "2026 S1"));
    }

    [Fact]
    public async Task ALineDeferredOnAReviewStillOpen_NamesThatReviewAsHoldingTheSeat()
    {
        await using var db = await SeededDbAsync();
        var review = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 2);
        await StartWithEvidenceAsync(db, review.Id);
        await DeferLineAsync(db, review.Id, "PAED-001");

        var due = await DueAsync(db, 2026, 2);

        var row = Row(due, Trainee, "PAED-001");
        (row.Status, row.ReviewId, row.HoldingReviewId, row.MayOpenHoldingReview).Should().Be(
            (EntrustmentDecisionDueStatus.Deferred, review.Id, review.Id, true), "its chair can reinstate it there");
        Row(due, LateStarter, "PAED-001").HoldingReviewId.Should().BeNull("only Ada's seat is held");
    }

    [Fact]
    public async Task ADecisionRevokedAfterAnOpenReviewWasPlannedWithoutIt_PointsToThatReview()
    {
        await using var db = await SeededDbAsync();
        var decidedAt = await DecideAsync(db, 2026, 2, "PAED-001");
        var open = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 2);
        (await PlannedCodesAsync(db, open.Id)).Should().NotContain("PAED-001");
        await RevokeAsync(db, "PAED-001");

        var row = Row(await DueAsync(db, 2026, 2), Trainee, "PAED-001");

        (row.Status, row.ReviewId, row.HoldingReviewId, row.MayOpenHoldingReview).Should().Be(
            (EntrustmentDecisionDueStatus.Revoked, decidedAt, open.Id, true));
    }

    [Fact]
    public async Task AnOpenReviewHoldsOnlyItsOwnSeat_AndOnlyForItsPeriod()
    {
        await using var db = await SeededDbAsync(withNeonatalPanel: true);
        var general = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 2);

        var semester2 = await DueAsync(db, 2026, 2);
        Row(semester2, Trainee, "PAED-001").HoldingReviewId.Should().Be(general.Id);
        Row(semester2, Trainee, "PAED-004").HoldingReviewId.Should().BeNull("the neonatal CCC is another seat");
        Row(await DueAsync(db, 2026, 1), Trainee, "PAED-001").HoldingReviewId.Should().BeNull("it sits for 2026 S2");

        var neonatal = await ScheduleAsync(db, NeonatalPanel, Trainee, 2026, 2);
        Row(await DueAsync(db, 2026, 2), Trainee, "PAED-004").HoldingReviewId.Should().Be(neonatal.Id);
    }

    [Fact]
    public async Task ANeonatalReview_DoesNotHoldTheGeneralSeat_NorDoesAFormativeReviewHoldAny()
    {
        await using var db = await SeededDbAsync(withNeonatalPanel: true);
        var neonatal = await ScheduleAsync(db, NeonatalPanel, Trainee, 2026, 2);
        await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 2, formative: true);

        var due = await DueAsync(db, 2026, 2);

        Row(due, Trainee, "PAED-004").HoldingReviewId.Should().Be(neonatal.Id);
        Row(due, Trainee, "PAED-001").HoldingReviewId.Should().BeNull(
            "the neonatal CCC is another seat, and a formative review is not a binding one: scheduling is not refused");
    }

    [Fact]
    public async Task AHoldingReview_IsLinkedOnlyWhereItsReadLadderAdmitsTheCaller()
    {
        await using var db = await SeededDbAsync();
        var open = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 2);

        var specialityAdmin = Row(await DueAsync(db, 2026, 2, SpecialityAdmin(Paediatrics)), Trainee, "PAED-001");

        (specialityAdmin.HoldingReviewId, specialityAdmin.MayOpenHoldingReview).Should().Be(
            (open.Id, false), "a SpecialityAdmin who does not sit on the panel cannot open its reviews");
    }

    [Fact]
    public async Task WhereANeonatalPanelCoversTheTrainee_Epas4And5_AreScheduledBeforeIt()
    {
        await using var db = await SeededDbAsync(withNeonatalPanel: true);

        var due = await DueAsync(db, 2026, 2);

        (Row(due, Trainee, "PAED-004").SchedulePanelId, Row(due, Trainee, "PAED-004").SchedulePanelName)
            .Should().Be((NeonatalPanel, "Neonatal CCC"));
        Row(due, Trainee, "PAED-001").SchedulePanelId.Should().Be(GeneralPanel);
    }

    [Fact]
    public async Task TheSummary_CountsTheTraineesOnEachEpa_ByWhereTheyStand()
    {
        await using var db = await SeededDbAsync();
        await DecideAsync(db, 2026, 1, "PAED-001");

        var due = await DueAsync(db, 2026, 1);

        due.TraineeCount.Should().Be(3, "Ada, the late starter and the Surgery trainee owe decisions in 2026 S1");
        var paed001 = due.ByEpa.Should().ContainSingle(summary => summary.EpaCode == "PAED-001").Subject;
        (paed001.Due, paed001.Decided, paed001.Optional).Should().Be((2, 1, 1), "Ada's is decided; the late starter's is a partial period");
        var paed002 = due.ByEpa.Single(summary => summary.EpaCode == "PAED-002");
        (paed002.Due, paed002.Deferred, paed002.Optional).Should().Be((2, 1, 1));
        var paed003 = due.ByEpa.Single(summary => summary.EpaCode == "PAED-003");
        paed003.ToSchedule.Should().Be(2, "due by year end for both");
        due.ByEpa.Select(summary => summary.EpaCode).Should().BeInAscendingOrder(StringComparer.Ordinal);
    }

    // ---- Scope ------------------------------------------------------------------------------------------------------

    [Fact]
    public async Task InstitutionBsQuery_IgnoresInstitutionIdA()
    {
        await using var db = await SeededDbAsync();

        var due = await DueAsync(db, 2026, 2, TestPrincipals.InstitutionalAdmin(InstitutionB), institutionId: InstitutionA);

        due.InstitutionId.Should().Be(InstitutionB);
        due.Items.Select(item => item.TraineeUserId).Distinct().Should().Equal(TraineeAtB);
    }

    [Fact]
    public async Task AnAdministrator_MustNameAnInstitution()
    {
        await using var db = await SeededDbAsync();

        var act = () => DueAsync(db, 2026, 2, TestPrincipals.Administrator());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Choose the institution whose decisions due to show.");
    }

    [Fact]
    public async Task AnAdministrator_NamingAnInstitution_SeesItsTrainees_AndAnUnknownOneIsNull()
    {
        await using var db = await SeededDbAsync();

        var atB = await DueAsync(db, 2026, 2, TestPrincipals.Administrator(), institutionId: InstitutionB);

        atB.Items.Select(item => item.TraineeUserId).Distinct().Should().Equal(TraineeAtB);
        (await DueNullableAsync(db, 2026, 2, TestPrincipals.Administrator(), institutionId: 999)).Should().BeNull();
    }

    [Fact]
    public async Task ACoordinator_SeesTheTraineesOfTheirInstitution()
    {
        await using var db = await SeededDbAsync();

        (await DueAsync(db, 2026, 2)).Items.Select(item => item.TraineeUserId).Distinct()
            .Should().BeEquivalentTo([Trainee, LateStarter, SurgeryTrainee]);
    }

    [Fact]
    public async Task ASpecialityAdmin_SeesOnlyTheirOwnSpecialitysTrainees()
    {
        await using var db = await SeededDbAsync();

        var surgery = await DueAsync(db, 2026, 2, SpecialityAdmin(Surgery));
        var paediatrics = await DueAsync(db, 2026, 2, SpecialityAdmin(Paediatrics));

        surgery.Items.Select(item => item.TraineeUserId).Distinct().Should().Equal(SurgeryTrainee);
        paediatrics.Items.Select(item => item.TraineeUserId).Distinct().Should().BeEquivalentTo([Trainee, LateStarter]);
    }

    [Fact]
    public async Task ASubSpecialityAdmin_SeesOnlyTheirOwnSubSpecialitysTrainees()
    {
        await using var db = await SeededDbAsync();

        var surgery = await DueAsync(db, 2026, 2, SubSpecialityAdmin(GeneralSurgery));
        var paediatrics = await DueAsync(db, 2026, 2, SubSpecialityAdmin(GeneralPaediatrics));

        surgery.Items.Select(item => item.TraineeUserId).Distinct().Should().Equal(SurgeryTrainee);
        paediatrics.Items.Select(item => item.TraineeUserId).Distinct().Should().BeEquivalentTo([Trainee, LateStarter]);
    }

    [Theory]
    [InlineData(WombatRoles.Coordinator)]
    [InlineData(WombatRoles.InstitutionalAdmin)]
    [InlineData(WombatRoles.Administrator)]
    public async Task ATraineeWhoAlsoSchedulesReviews_IsShownNobodysDecisions(string role)
    {
        // T185: a registrar who coordinates, administers, or holds Administrator reads no peer's record here, their own
        // institution's trainees included, as though there were none to show.
        await using var db = await SeededDbAsync();
        await DecideAsync(db, 2026, 1, "PAED-001");
        var registrar = TestPrincipals.InRoles([WombatRoles.Trainee, role], Trainee, InstitutionA);

        var due = await DueNullableAsync(db, 2026, 1, registrar, institutionId: InstitutionA);

        due.Should().BeNull();
    }

    [Theory]
    [InlineData(WombatRoles.CommitteeMember)]
    [InlineData(WombatRoles.Trainee)]
    [InlineData(WombatRoles.Assessor)]
    public async Task ARoleThatSchedulesNoReview_IsShownNoTrainee(string role)
    {
        // The reach of the roles that schedule reviews, with no CommitteeMember arm (Decisions: the design's "no
        // CommitteeMember arm").
        await using var db = await SeededDbAsync();

        var due = await DueNullableAsync(db, 2026, 2, TestPrincipals.InRole(role, "someone", InstitutionA));

        (due?.Items ?? []).Should().BeEmpty();
    }

    [Fact]
    public async Task AReviewLink_IsOfferedOnlyWhereTheReviewsReadLadderAdmitsTheCaller()
    {
        await using var db = await SeededDbAsync();
        var review = await DecideAsync(db, 2026, 1, "PAED-001");

        var coordinator = Row(await DueAsync(db, 2026, 1), Trainee, "PAED-001");
        var specialityAdmin = Row(await DueAsync(db, 2026, 1, SpecialityAdmin(Paediatrics)), Trainee, "PAED-001");

        (coordinator.ReviewId, coordinator.MayOpenReview).Should().Be((review, true));
        (specialityAdmin.ReviewId, specialityAdmin.MayOpenReview).Should().Be(
            (review, false), "a SpecialityAdmin who does not sit on the panel cannot open its reviews");
    }

    [Fact]
    public async Task TheValidator_RefusesASemesterOtherThan1Or2()
    {
        var result = await new GetEntrustmentDecisionsDueQueryValidator().ValidateAsync(
            new GetEntrustmentDecisionsDueQuery(2026, 3, null, TestPrincipals.Coordinator(InstitutionA)));

        result.IsValid.Should().BeFalse();
    }

    // ---- The status, read on its own --------------------------------------------------------------------------------

    private static readonly DateOnly OnTime = new(2025, 1, 15);

    private static QuotaWindow SemesterWindow(int semester, DateOnly? start = null)
        => QuotaWindow.For(QuotaPeriod.Semester, new AcademicPeriod(2026, semester).End, start ?? OnTime);

    private static QuotaWindow YearWindow() => QuotaWindow.For(QuotaPeriod.AcademicYear, new AcademicPeriod(2026, 1).End, OnTime);

    private static CommitteeSitting Sitting(int reviewId, int semester = 1) => new(2026, semester, reviewId);

    private static AgendaLineStanding LineAt(
        CommitteeAgendaLineState state, CommitteeReviewState review, int reviewId = 1, bool starDecides = false)
        => new(state, review, Sitting(reviewId), starDecides);

    private static StarStanding StarAt(bool decides, int reviewId = 1) => new(decides, Sitting(reviewId));

    private static EntrustmentDecisionDueStatus Read(
        AgendaLineStanding[] lines,
        StarStanding[] stars,
        QuotaWindow? window = null,
        int semester = 1,
        bool opportunistic = false,
        DateOnly? today = null)
        => CommitteeAgendaStatus.DecisionDue(
            lines, stars, opportunistic, window ?? SemesterWindow(semester), new AcademicPeriod(2026, semester), today ?? Today);

    [Fact]
    public void AStandingStar_DecidesAheadOfEverything()
    {
        Read([LineAt(CommitteeAgendaLineState.Due, CommitteeReviewState.InProgress, 2)], [StarAt(false), StarAt(true)])
            .Should().Be(EntrustmentDecisionDueStatus.Decided);
    }

    [Fact]
    public void AnOpenAgenda_ComesFirst_ThenTheLaterOfADeferralAndALostDecision()
    {
        Read(
                [
                    LineAt(CommitteeAgendaLineState.Deferred, CommitteeReviewState.Ratified, 1),
                    LineAt(CommitteeAgendaLineState.Due, CommitteeReviewState.Scheduled, 3)
                ],
                [StarAt(false, reviewId: 2)])
            .Should().Be(EntrustmentDecisionDueStatus.Scheduled);
        Read([LineAt(CommitteeAgendaLineState.Deferred, CommitteeReviewState.Ratified, 1)], [StarAt(false, reviewId: 2)])
            .Should().Be(EntrustmentDecisionDueStatus.Revoked, "a later sitting decided it, and that decision was revoked");
        Read([LineAt(CommitteeAgendaLineState.Deferred, CommitteeReviewState.Ratified, 2)], [StarAt(false, reviewId: 1)])
            .Should().Be(EntrustmentDecisionDueStatus.Deferred, "the sitting after the revocation deferred it");
        Read(
                [
                    LineAt(CommitteeAgendaLineState.Deferred, CommitteeReviewState.Ratified, 1),
                    LineAt(CommitteeAgendaLineState.Decided, CommitteeReviewState.Ratified, 2, starDecides: false)
                ],
                [])
            .Should().Be(EntrustmentDecisionDueStatus.Revoked, "a Decided line whose STAR was lost is a later decision too");
    }

    [Fact]
    public void ALostDecision_ReadsRevoked_AheadOfMissed()
    {
        Read([], [StarAt(false)]).Should().Be(EntrustmentDecisionDueStatus.Revoked);
        Read([], []).Should().Be(EntrustmentDecisionDueStatus.Missed, "semester 1 has ended");
    }

    [Fact]
    public void ADueLineOnAReviewNoLongerOpen_DoesNotReadAsScheduled()
    {
        Read([LineAt(CommitteeAgendaLineState.Due, CommitteeReviewState.Ratified)], []).Should().Be(EntrustmentDecisionDueStatus.Missed);
        Read([LineAt(CommitteeAgendaLineState.NotDecided, CommitteeReviewState.Ratified)], [], semester: 2)
            .Should().Be(EntrustmentDecisionDueStatus.NotScheduled);
    }

    // ---- Whether a STAR decides its window --------------------------------------------------------------------------

    private static DecisionWindowStar Star(int id, EntrustmentDecisionStatus status, int? supersededBy = null, int semester = 1)
        => new(id, Trainee, 1, status, supersededBy, Sitting(id, semester));

    private static bool Decides(DecisionWindowStar star, params DecisionWindowStar[] others)
        => CommitteeAgendaStatus.StarDecides(star, SemesterWindow(1), others.Append(star).ToDictionary(entry => entry.Id));

    [Fact]
    public void AnActiveOrExpiredStar_DecidesItsWindow_AndARevokedOneDoesNot()
    {
        Decides(Star(1, EntrustmentDecisionStatus.Active)).Should().BeTrue();
        Decides(Star(1, EntrustmentDecisionStatus.Expired)).Should().BeTrue("expiry is informational (Decision 10)");
        Decides(Star(1, EntrustmentDecisionStatus.Revoked)).Should().BeFalse();
    }

    [Fact]
    public void ASupersededStar_DecidesItsWindow_UnlessASuccessorInsideItWasRevoked()
    {
        Decides(Star(1, EntrustmentDecisionStatus.Superseded, 2), Star(2, EntrustmentDecisionStatus.Revoked, semester: 2))
            .Should().BeTrue("its successor decided another window");
        Decides(Star(1, EntrustmentDecisionStatus.Superseded, 99))
            .Should().BeTrue("a successor not among the year's STARs sat for another year");
        Decides(Star(1, EntrustmentDecisionStatus.Superseded, 2), Star(2, EntrustmentDecisionStatus.Active))
            .Should().BeTrue();
        Decides(Star(1, EntrustmentDecisionStatus.Superseded, 2), Star(2, EntrustmentDecisionStatus.Revoked))
            .Should().BeFalse("the window's decision was replaced, and the replacement revoked");
        Decides(
                Star(1, EntrustmentDecisionStatus.Superseded, 2),
                Star(2, EntrustmentDecisionStatus.Superseded, 3),
                Star(3, EntrustmentDecisionStatus.Revoked))
            .Should().BeFalse("the chain is followed to the window's last decision");
        Decides(
                Star(1, EntrustmentDecisionStatus.Superseded, 2),
                Star(2, EntrustmentDecisionStatus.Superseded, 3),
                Star(3, EntrustmentDecisionStatus.Active, semester: 2))
            .Should().BeTrue("the window's last decision was superseded by another window's");
    }

    [Fact]
    public void WhatCannotBeMissed_SaysWhy()
    {
        Read([], [], opportunistic: true).Should().Be(EntrustmentDecisionDueStatus.AsOpportunityAllows);
        Read([], [], window: SemesterWindow(1, new DateOnly(2026, 3, 2))).Should().Be(EntrustmentDecisionDueStatus.PartialPeriod);
        Read([], [], window: YearWindow()).Should().Be(EntrustmentDecisionDueStatus.DueByYearEnd);
        Read([], [], window: YearWindow(), semester: 2).Should().Be(
            EntrustmentDecisionDueStatus.NotScheduled, "semester 2 is the year's last: an annual EPA closes there");
        Read([], [], window: YearWindow(), semester: 2, today: new DateOnly(2027, 1, 1)).Should().Be(EntrustmentDecisionDueStatus.Missed);
    }

    // ---- The requests -----------------------------------------------------------------------------------------------

    private static async Task<EntrustmentDecisionsDueDto> DueAsync(
        ApplicationDbContext db, int year, int semester, ClaimsPrincipal? principal = null, int? institutionId = null,
        DateOnly? today = null)
        => (await DueNullableAsync(db, year, semester, principal, institutionId, today))
           ?? throw new InvalidOperationException("The query returned nothing.");

    private static async Task<EntrustmentDecisionsDueDto?> DueNullableAsync(
        ApplicationDbContext db, int year, int semester, ClaimsPrincipal? principal = null, int? institutionId = null,
        DateOnly? today = null)
    {
        var due = await new GetEntrustmentDecisionsDueQueryHandler(db, Names().WithTraineesOf(db)).Handle(
            new GetEntrustmentDecisionsDueQuery(
                year, semester, institutionId, principal ?? TestPrincipals.Coordinator(InstitutionA), today ?? Today),
            CancellationToken.None);
        db.ChangeTracker.Clear();
        return due;
    }

    private static FakeUserDirectory Names() => new(
        (Trainee, "Ada Trainee"),
        (LateStarter, "Lerato Late"),
        (FutureStarter, "Fikile Future"),
        (SurgeryTrainee, "Sipho Surgeon"),
        (TraineeAtB, "Bongi Bee"));

    private static IEnumerable<(string Code, string Window, EntrustmentDecisionDueStatus Status)> Rows(
        EntrustmentDecisionsDueDto due, string trainee)
        => due.Items.Where(item => item.TraineeUserId == trainee).Select(item => (item.EpaCode, item.WindowLabel, item.Status));

    private static EntrustmentDecisionDueDto Row(EntrustmentDecisionsDueDto due, string trainee, string code)
        => due.Items.Should().ContainSingle(item => item.TraineeUserId == trainee && item.EpaCode == code).Subject;

    private static EntrustmentDecisionDueStatus Status(EntrustmentDecisionsDueDto due, string trainee, string code)
        => Row(due, trainee, code).Status;

    private static ClaimsPrincipal SpecialityAdmin(int specialityId)
        => TestPrincipals.InRole(WombatRoles.SpecialityAdmin, $"speciality-admin-{specialityId}", InstitutionA, specialityId);

    private static ClaimsPrincipal SubSpecialityAdmin(int subSpecialityId)
        => TestPrincipals.InRole(
            WombatRoles.SubSpecialityAdmin, $"sub-speciality-admin-{subSpecialityId}", InstitutionA, subSpecialityId: subSpecialityId);

    /// <summary>Revokes Ada's active STAR on the EPA, and returns it.</summary>
    private static async Task<EntrustmentDecision> RevokeAsync(ApplicationDbContext db, string code)
    {
        var epaId = EpaIdOf(code);
        var star = await db.EntrustmentDecisions.SingleAsync(entity =>
            entity.TraineeUserId == Trainee && entity.EpaId == epaId && entity.Status == EntrustmentDecisionStatus.Active);
        star.Revoke("Issued in error.", "admin-user", DateTime.UtcNow);
        await SaveAndClearAsync(db);
        return star;
    }

    /// <summary>The chair defers the review's line on the EPA, leaving the review open.</summary>
    private static async Task DeferLineAsync(ApplicationDbContext db, int reviewId, string code)
    {
        var epaId = EpaIdOf(code);
        var lineId = await db.CommitteeAgendaLines
            .Where(line => line.ReviewId == reviewId && line.EpaId == epaId)
            .Select(line => line.Id)
            .SingleAsync();
        await new DeferAgendaLineCommandHandler(db).Handle(
            new DeferAgendaLineCommand(reviewId, lineId, "Evidence still to come.", Chair()), CancellationToken.None);
        db.ChangeTracker.Clear();
    }

    /// <summary>The codes the planner put on the review's agenda.</summary>
    private static async Task<IReadOnlyList<string>> PlannedCodesAsync(ApplicationDbContext db, int reviewId)
        => await db.CommitteeAgendaLines.AsNoTracking()
            .Where(line => line.ReviewId == reviewId)
            .Select(line => line.EpaCode)
            .ToListAsync();

    /// <summary>
    /// A binding review of Ada before the general panel for the period: STARs staged on <paramref name="codes" />, every
    /// other closing line deferred, the decision recorded with a quorum and ratified. Returns the review's id.
    /// </summary>
    private static async Task<int> DecideAsync(
        ApplicationDbContext db, int year, int semester, string code, string? second = null, DateOnly? expiresOn = null)
    {
        var review = await ScheduleAsync(db, GeneralPanel, Trainee, year, semester);
        await StartWithEvidenceAsync(db, review.Id);
        await StageAsync(db, review.Id, EpaIdOf(code), expiresOn);
        if (second is not null)
        {
            await StageAsync(db, review.Id, EpaIdOf(second), expiresOn);
        }

        await DeferAllOutstandingAsync(db, review.Id);
        await RecordDecisionAsync(db, review.Id);
        await RatifyAsync(db, review.Id);
        return review.Id;
    }

    private static async Task<CommitteeReviewListItemDto> ScheduleAsync(
        ApplicationDbContext db, int panelId, string trainee, int year, int semester, bool formative = false)
    {
        var scheduled = await new ScheduleCommitteeReviewCommandHandler(db, FakeUserDirectory.TraineesOf(db)).Handle(
            new ScheduleCommitteeReviewCommand(
                trainee, panelId, year, semester, new DateOnly(year, 1, 1), new DateOnly(year, 12, 31), new DateOnly(2026, 7, 2),
                TestPrincipals.Coordinator(InstitutionA), IsFormative: formative),
            CancellationToken.None);
        db.ChangeTracker.Clear();
        return scheduled;
    }

    /// <summary>Starts the review, then writes one snapshot line per EPA: the window held no activity to freeze.</summary>
    private static async Task StartWithEvidenceAsync(ApplicationDbContext db, int reviewId)
    {
        await new StartCommitteeReviewCommandHandler(db).Handle(new StartCommitteeReviewCommand(reviewId, Chair()), CancellationToken.None);
        db.ChangeTracker.Clear();

        foreach (var (code, epaId) in Epas)
        {
            db.CommitteeEvidenceItems.Add(new CommitteeEvidence
            {
                ReviewId = reviewId,
                SourceType = CommitteeEvidenceSourceType.Activity,
                ActivityId = 1000 + epaId,
                EpaId = epaId,
                EpaCode = code,
                SourceLabel = $"Mini-CEX #{1000 + epaId}",
                Summary = "State: completed.",
                ObservedOn = new DateOnly(2026, 2, 10)
            });
        }

        await SaveAndClearAsync(db);
    }

    private static async Task StageAsync(ApplicationDbContext db, int reviewId, int epaId, DateOnly? expiresOn)
    {
        var evidence = await db.CommitteeEvidenceItems.Where(line => line.ReviewId == reviewId && line.EpaId == epaId)
            .Select(line => line.Id).ToArrayAsync();
        await new StagePendingEntrustmentDecisionCommandHandler(db).Handle(
            new StagePendingEntrustmentDecisionCommand(
                reviewId, null, epaId, Level, new DateOnly(2026, 7, 2), expiresOn, "Consistent.", evidence, Chair()),
            CancellationToken.None);
        db.ChangeTracker.Clear();
    }

    /// <summary>Defers every closing line nothing is staged on, as the chair does for the EPAs the sitting will not decide.</summary>
    private static async Task DeferAllOutstandingAsync(ApplicationDbContext db, int reviewId)
    {
        var agenda = await new GetCommitteeAgendaQueryHandler(db).Handle(
            new GetCommitteeAgendaQuery(reviewId, TestPrincipals.Administrator(), Today), CancellationToken.None);
        db.ChangeTracker.Clear();

        foreach (var line in agenda.OutstandingClosingLines)
        {
            await new DeferAgendaLineCommandHandler(db).Handle(
                new DeferAgendaLineCommand(reviewId, line.Id, "Not decided at this sitting.", Chair()), CancellationToken.None);
            db.ChangeTracker.Clear();
        }
    }

    /// <summary>Records the decision with the whole general panel present: a quorum of active committee members (T165).</summary>
    private static async Task RecordDecisionAsync(ApplicationDbContext db, int reviewId)
    {
        string[] members = ["chair-a", "member-a"];
        await new RecordCommitteeDecisionCommandHandler(db, FakeUserDirectory.CommitteeMembersAt(InstitutionA, members)).Handle(
            new RecordCommitteeDecisionCommand(
                reviewId, CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, members, Chair()),
            CancellationToken.None);
        db.ChangeTracker.Clear();
    }

    private static async Task RatifyAsync(ApplicationDbContext db, int reviewId)
    {
        await new RatifyCommitteeDecisionCommandHandler(db).Handle(
            new RatifyCommitteeDecisionCommand(reviewId, Chair()), CancellationToken.None);
        db.ChangeTracker.Clear();
    }

    private static ClaimsPrincipal Chair() => TestPrincipals.InRole(WombatRoles.CommitteeMember, "chair-a", InstitutionA);

    /// <summary>The trainee moves institution: their preferred profile is now at <paramref name="institutionId" />.</summary>
    private static async Task MoveAsync(ApplicationDbContext db, string trainee, int institutionId)
    {
        (await db.TraineeProfiles.SingleAsync(profile => profile.UserId == trainee)).InstitutionId = institutionId;
        await SaveAndClearAsync(db);
    }

    private static int EpaIdOf(string code) => Epas.Single(epa => epa.Code == code).Id;

    private static async Task SaveAndClearAsync(ApplicationDbContext db)
    {
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    // ---- Fixture ----------------------------------------------------------------------------------------------------

    private static readonly (string Code, int Id)[] Epas =
    [
        ("PAED-001", 1), ("PAED-002", 2), ("PAED-003", 3), ("PAED-004", 4), ("PAED-005", 5), ("PAED-008", 8),
        ("PAED-020", 20), ("PAED-030", 30), ("PAED-040", 40)
    ];

    private async Task<ApplicationDbContext> SeededDbAsync(bool withNeonatalPanel = false)
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);

        db.Institutions.AddRange(
            new Institution { Id = InstitutionA, Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = InstitutionB, Name = "B", ShortCode = "B", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Specialities.AddRange(
            new Speciality { Id = Paediatrics, CollegeId = 1, Name = "Paediatrics", IsActive = true },
            new Speciality { Id = Surgery, CollegeId = 1, Name = "Surgery", IsActive = true });
        db.SubSpecialities.AddRange(
            new SubSpeciality { Id = 11, SpecialityId = Paediatrics, Name = "General Paediatrics", IsActive = true },
            new SubSpeciality { Id = 21, SpecialityId = Surgery, Name = "General Surgery", IsActive = true });
        db.Curricula.AddRange(
            new Curriculum { Id = PaedsCurriculum, SubSpecialityId = 11, Name = "General Paediatrics", Version = "11.1" },
            new Curriculum { Id = SurgeryCurriculum, SubSpecialityId = 21, Name = "General Surgery", Version = "1" });
        db.DecisionBodies.Add(new DecisionBody { Key = Neonatal, Name = "Neonatal team Clinical Competency Committee" });
        db.EntrustmentScales.Add(new EntrustmentScale { Id = 1, Name = "CPSA ladder" });
        db.EntrustmentLevels.AddRange(Enumerable.Range(1, 5).Select(order =>
            new EntrustmentLevel { Id = order, ScaleId = 1, Order = order, Label = order.ToString(System.Globalization.CultureInfo.InvariantCulture) }));

        foreach (var (code, id) in Epas)
        {
            db.Epas.Add(new Epa { Id = id, SubSpecialityId = 11, Code = code, Title = $"EPA {id}", IsActive = code != "PAED-040" });
        }

        db.Epas.Add(new Epa { Id = 50, SubSpecialityId = 21, Code = "SURG-001", Title = "Theatre list", IsActive = true });

        db.CurriculumItems.AddRange(
            Item(1, QuotaPeriod.Semester),
            Item(2, QuotaPeriod.Semester),
            Item(3, QuotaPeriod.AcademicYear),
            Item(4, QuotaPeriod.Semester, Neonatal),
            Item(5, QuotaPeriod.Semester, Neonatal),
            Item(8, QuotaPeriod.AcademicYear, opportunistic: true),
            Item(20, cadence: null),
            Item(30, QuotaPeriod.Semester, owningInstitutionId: InstitutionB),
            Item(40, QuotaPeriod.Semester),
            new CurriculumItem
            {
                Id = 150, CurriculumId = SurgeryCurriculum, EpaId = 50, RequiredCount = 1, MinimumLevelOrder = 3,
                DecisionCadence = QuotaPeriod.Semester
            });

        db.TraineeProfiles.AddRange(
            Profile(1, Trainee, InstitutionA, new DateOnly(2025, 1, 15)),
            Profile(2, LateStarter, InstitutionA, new DateOnly(2026, 3, 2)),
            Profile(3, FutureStarter, InstitutionA, new DateOnly(2027, 2, 1)),
            Profile(4, Leaver, InstitutionA, new DateOnly(2024, 1, 15), isActive: false),
            Profile(5, SurgeryTrainee, InstitutionA, new DateOnly(2025, 1, 15), SurgeryCurriculum),
            Profile(6, TraineeAtB, InstitutionB, new DateOnly(2025, 1, 15)));

        db.DecisionPanels.Add(Panel(GeneralPanel, "General CCC", InstitutionA, null, "chair-a", "member-a"));
        db.DecisionPanels.Add(Panel(PanelAtB, "B's CCC", InstitutionB, null, "chair-b"));
        if (withNeonatalPanel)
        {
            db.DecisionPanels.Add(Panel(NeonatalPanel, "Neonatal CCC", InstitutionA, Neonatal, "chair-n", "member-n"));
        }

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private static TraineeProfile Profile(
        int id, string userId, int institutionId, DateOnly start, int curriculumId = PaedsCurriculum, bool isActive = true)
        => new()
        {
            Id = id,
            UserId = userId,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            IsActive = isActive,
            ProgrammeStartDate = start,
            ExpectedCompletionDate = start.AddYears(4)
        };

    private static CurriculumItem Item(
        int epaId, QuotaPeriod? cadence, string? body = null, bool opportunistic = false, int? owningInstitutionId = null)
        => new()
        {
            Id = 100 + epaId,
            CurriculumId = PaedsCurriculum,
            EpaId = epaId,
            RequiredCount = 1,
            MinimumLevelOrder = 3,
            DecisionCadence = cadence,
            DecisionBodyKey = body,
            DecisionIsOpportunistic = opportunistic,
            OwningInstitutionId = owningInstitutionId
        };

    private static DecisionPanel Panel(int id, string name, int institutionId, string? body, string chair, string? member = null)
        => new()
        {
            Id = id,
            Name = name,
            Scope = DecisionPanelScope.Institution,
            InstitutionId = institutionId,
            DecisionBodyKey = body,
            CreatedOn = DateTime.UtcNow,
            Members = member is null
                ? [new DecisionPanelMember { UserId = chair, Role = DecisionPanelMemberRole.Chair }]
                :
                [
                    new DecisionPanelMember { UserId = chair, Role = DecisionPanelMemberRole.Chair },
                    new DecisionPanelMember { UserId = member, Role = DecisionPanelMemberRole.Member }
                ]
        };
}
