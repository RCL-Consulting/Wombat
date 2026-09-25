using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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
/// T131 slice 4: a review sits for an academic period and carries an agenda of the EPAs due by cadence and routed to its
/// panel; a closing line must be staged or deferred before ratify; each line keeps its own state.
/// </summary>
/// <remarks>
/// The curriculum mirrors the v11.1 shape in miniature: PAED-001, 002, 004 and 005 are decided each semester (004 and 005
/// by the neonatal committee), 003 annually, 008 annually as opportunity allows, 020 has no cadence, 030 is another
/// institution's local item, and 040's EPA is deactivated. Every refusal here is followed by the save the audit pipeline
/// makes and a cleared tracker, and nothing it tried to write may be found.
/// </remarks>
public sealed class CommitteeAgendaHandlerTests
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;
    private const int GeneralPanel = 10;
    private const int NeonatalPanel = 11;
    private const int SpecialityPanel = 12;
    private const int NeonatalPanelAtB = 21;
    private const int PanelAtB = 20;
    private const int CurriculumId = 100;
    private const string Trainee = "trainee-a";
    private const string LateStarter = "trainee-late";
    private const string Neonatal = "neonatal";
    private const int Level = 3;

    private static readonly DateOnly Today = new(2026, 9, 24);

    private const string NotSchedulable =
        "A review can only be scheduled on a panel of your institution that covers the trainee's programme, for a " +
        "trainee at that institution whose programme you oversee.";

    private readonly string _databaseName = Guid.NewGuid().ToString();

    // ---- Planning ---------------------------------------------------------------------------------------------------

    [Fact]
    public async Task ABindingReview_IsScheduledForItsPeriod_WithTheEpasDueThatRouteToItsPanel()
    {
        await using var db = await SeededDbAsync();

        var scheduled = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);

        scheduled.AcademicYear.Should().Be(2026);
        scheduled.Semester.Should().Be(1);
        var agenda = await AgendaAsync(db, scheduled.Id);
        agenda.PeriodLabel.Should().Be("2026 S1");
        agenda.Lines.Select(line => line.EpaCode).Should().Equal(
            ["PAED-001", "PAED-002", "PAED-003", "PAED-004", "PAED-005", "PAED-008"],
            "no neonatal panel covers the trainee, so 004 and 005 fall back to the general panel; 020 has no cadence, 030 is " +
            "another institution's, and 040's EPA is deactivated");
        agenda.Lines.Where(line => line.IsClosing).Select(line => line.EpaCode)
            .Should().Equal("PAED-001", "PAED-002", "PAED-004", "PAED-005");
        Line(agenda, "PAED-001").WindowLabel.Should().Be("2026 S1");
        Line(agenda, "PAED-001").Status.Should().Be(CommitteeAgendaLineStatus.Due);
        Line(agenda, "PAED-003").WindowLabel.Should().Be("2026");
        Line(agenda, "PAED-003").Status.Should().Be(CommitteeAgendaLineStatus.DueByYearEnd);
        Line(agenda, "PAED-008").Status.Should().Be(CommitteeAgendaLineStatus.DueByYearEnd, "an annual EPA, before the year's last sitting");
        agenda.RoutedElsewhere.Should().BeEmpty();
    }

    [Fact]
    public async Task WhereANeonatalPanelCoversTheTrainee_TheGeneralAgendaLeavesOutEpas4And5_AndTheNeonatalReviewHoldsThem()
    {
        await using var db = await SeededDbAsync(withNeonatalPanel: true);

        var general = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        var neonatal = await ScheduleAsync(db, NeonatalPanel, Trainee, 2026, 1, principal: TestPrincipals.Coordinator(InstitutionA));

        var generalAgenda = await AgendaAsync(db, general.Id);
        generalAgenda.Lines.Select(line => line.EpaCode).Should().Equal("PAED-001", "PAED-002", "PAED-003", "PAED-008");
        generalAgenda.RoutedElsewhere.Select(line => (line.EpaCode, line.PanelName, line.Status)).Should().Equal(
            ("PAED-004", "Neonatal CCC", CommitteeAgendaElsewhereStatus.OnAgenda),
            ("PAED-005", "Neonatal CCC", CommitteeAgendaElsewhereStatus.OnAgenda));

        (await AgendaAsync(db, neonatal.Id, NeonatalChair())).Lines.Select(line => line.EpaCode)
            .Should().Equal("PAED-004", "PAED-005");
    }

    [Fact]
    public async Task AnotherInstitutionsNeonatalPanel_TakesNothingFromTheTraineesGeneralPanel()
    {
        await using var db = await SeededDbAsync();
        db.DecisionPanels.Add(Panel(21, "B's neonatal CCC", InstitutionB, Neonatal, "chair-bn"));
        await SaveAndClearAsync(db);

        var general = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);

        (await AgendaAsync(db, general.Id)).Lines.Select(line => line.EpaCode).Should().Contain(["PAED-004", "PAED-005"]);
    }

    [Fact]
    public async Task AWithdrawnReview_ReadsItsLinesStillDue_AsNotDecided_AndNothingOnItIsOutstanding()
    {
        // T258: an erasure withdraws a review still open. What it still held due was not decided, and nothing on it keeps a
        // review from being ratified that nobody will ratify.
        await using var db = await SeededDbAsync();
        var scheduled = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        (await AgendaAsync(db, scheduled.Id)).OutstandingClosingLines.Should().NotBeEmpty();

        var review = await db.CommitteeReviews.SingleAsync(entity => entity.Id == scheduled.Id);
        review.Withdraw(CommitteeReview.WithdrawnTraineeErased, DateTime.UtcNow);
        await SaveAndClearAsync(db);

        var agenda = await AgendaAsync(db, scheduled.Id);
        agenda.Lines.Should().NotBeEmpty()
            .And.OnlyContain(line => line.Status == CommitteeAgendaLineStatus.NotDecided && !line.BlocksRatify);
        agenda.OutstandingClosingLines.Should().BeEmpty();
        agenda.RatifyBlockedReason.Should().BeNull();
    }

    [Fact]
    public async Task AFormativeReview_CarriesNoAgenda()
    {
        await using var db = await SeededDbAsync();

        var formative = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1, formative: true);
        await StartAsync(db, formative.Id);

        (await db.CommitteeAgendaLines.CountAsync()).Should().Be(0);
        (await AgendaAsync(db, formative.Id)).IsFormative.Should().BeTrue();
    }

    [Fact]
    public async Task AtTheSemester2Sitting_AnAnnualEpaIsClosing_ButOneDecidedAsOpportunityAllowsIsNot()
    {
        await using var db = await SeededDbAsync();

        var agenda = await AgendaAsync(db, (await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 2)).Id);

        Line(agenda, "PAED-003").IsClosing.Should().BeTrue("semester 2 is the year's last sitting");
        Line(agenda, "PAED-003").Status.Should().Be(CommitteeAgendaLineStatus.Due);
        Line(agenda, "PAED-008").IsClosing.Should().BeFalse("EPA 8 is decided as opportunity allows (O7)");
        Line(agenda, "PAED-008").Status.Should().Be(CommitteeAgendaLineStatus.AsOpportunityAllows);
        Line(agenda, "PAED-001").WindowLabel.Should().Be("2026 S2");
    }

    [Fact]
    public async Task ATraineeWhoJoinedPartWayThroughTheSemester_HasItsLinesLabelledAndOptional()
    {
        await using var db = await SeededDbAsync();

        var agenda = await AgendaAsync(db, (await ScheduleAsync(db, GeneralPanel, LateStarter, 2026, 1)).Id);

        Line(agenda, "PAED-001").IsPartialPeriod.Should().BeTrue();
        Line(agenda, "PAED-001").IsClosing.Should().BeFalse("a partial period is never closing (Decision 9)");
        Line(agenda, "PAED-001").Status.Should().Be(CommitteeAgendaLineStatus.PartialPeriod);
        Line(agenda, "PAED-003").IsPartialPeriod.Should().BeFalse("a start in semester 1 is on time for the year");
        agenda.OutstandingClosingLines.Should().BeEmpty();
    }

    [Fact]
    public async Task AnEpaDecidedInItsWindow_IsNotPlannedAgainInThatWindow()
    {
        await using var db = await SeededDbAsync();
        var first = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        await StartWithEvidenceAsync(db, first.Id);
        await StageAsync(db, first.Id, EpaIdOf("PAED-001"));
        await StageAsync(db, first.Id, EpaIdOf("PAED-003"));
        await DeferAllOutstandingAsync(db, first.Id);
        await RecordDecisionAsync(db, first.Id);
        await RatifyAsync(db, first.Id);

        var preview = await PreviewAsync(db, GeneralPanel, Trainee, 2026, 2);

        preview.DecidedInWindow.Should().Equal("PAED-003");
        preview.Lines.Select(line => line.EpaCode).Should().NotContain("PAED-003")
            .And.Contain("PAED-001", "a semester EPA decided in semester 1 is due again in semester 2");
        (await AgendaAsync(db, (await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 2)).Id)).Lines
            .Select(line => line.EpaCode).Should().Equal(preview.Lines.Select(line => line.EpaCode), "the preview is the agenda");
    }

    [Fact]
    public async Task Start_PlansAgain_AddingWhatHasComeDueSince_AndKeepingTheLinesAlreadyPlanned()
    {
        await using var db = await SeededDbAsync();
        var review = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        var planned = await db.CommitteeAgendaLines.AsNoTracking().Select(line => new { line.Id, line.EpaId }).ToListAsync();

        var item = await db.CurriculumItems.SingleAsync(entity => entity.EpaId == EpaIdOf("PAED-020"));
        item.DecisionCadence = QuotaPeriod.Semester;
        await SaveAndClearAsync(db);

        await StartAsync(db, review.Id);

        var lines = await db.CommitteeAgendaLines.AsNoTracking().ToListAsync();
        lines.Select(line => line.EpaCode).Should().Contain("PAED-020");
        lines.Where(line => line.EpaCode != "PAED-020").Select(line => new { line.Id, line.EpaId })
            .Should().BeEquivalentTo(planned, "Start only adds");
    }

    [Fact]
    public async Task ARevokedStar_DecidesNothing_SoItsEpaIsPlannedAgain_AndAnotherPanelsReadsAsUndecided()
    {
        // T131 slice 6's rule, which the agenda shares: a window is decided by a STAR still Active or Superseded.
        await using var db = await SeededDbAsync(withNeonatalPanel: true);
        var general = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        await StartWithEvidenceAsync(db, general.Id);
        await StageAsync(db, general.Id, EpaIdOf("PAED-003"));
        await DeferAllOutstandingAsync(db, general.Id);
        await RecordDecisionAsync(db, general.Id);
        await RatifyAsync(db, general.Id);
        var neonatal = await ScheduleAsync(db, NeonatalPanel, Trainee, 2026, 1);
        await StartWithEvidenceAsync(db, neonatal.Id);
        await StageAsync(db, neonatal.Id, EpaIdOf("PAED-004"));
        await DeferAllOutstandingAsync(db, neonatal.Id);
        await RecordDecisionAsync(db, neonatal.Id);
        await RatifyAsync(db, neonatal.Id);

        (await PreviewAsync(db, GeneralPanel, Trainee, 2026, 2)).DecidedInWindow.Should().Equal("PAED-003");
        Elsewhere(await AgendaAsync(db, general.Id), "PAED-004").Status.Should().Be(CommitteeAgendaElsewhereStatus.Decided);

        foreach (var star in await db.EntrustmentDecisions.ToListAsync())
        {
            star.Revoke("Issued in error.", "admin-user", DateTime.UtcNow);
        }

        await SaveAndClearAsync(db);

        var preview = await PreviewAsync(db, GeneralPanel, Trainee, 2026, 2);
        preview.DecidedInWindow.Should().BeEmpty();
        preview.Lines.Select(line => line.EpaCode).Should().Contain("PAED-003", "its year's decision was revoked, so it is due again");
        var agenda = await AgendaAsync(db, general.Id);
        Elsewhere(agenda, "PAED-004").Status.Should().Be(
            CommitteeAgendaElsewhereStatus.Missed, "semester 1 has ended, and its only decision was revoked");
        Elsewhere(agenda, "PAED-005").Status.Should().Be(CommitteeAgendaElsewhereStatus.Deferred);
    }

    [Fact]
    public async Task AReviewTheTraineeHasLeft_ShowsItsInstitutionNothingOfTheirNewOne()
    {
        // The trainee moves from A to B, where a neonatal CCC sits. A's stranded review plans nothing more, and its page
        // names none of B's panels and says nothing of where B's decisions stand (T182).
        await using var db = await SeededDbAsync();
        db.DecisionPanels.Add(Panel(NeonatalPanelAtB, "B's neonatal CCC", InstitutionB, Neonatal, "chair-bn"));
        await SaveAndClearAsync(db);
        var review = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        await StartWithEvidenceAsync(db, review.Id);
        await MoveAsync(db, Trainee, InstitutionB);

        var agenda = await AgendaAsync(db, review.Id, TestPrincipals.InstitutionalAdmin(InstitutionA));

        agenda.RoutedElsewhere.Should().BeEmpty();
        agenda.Lines.Select(line => line.EpaCode).Should().Contain(["PAED-004", "PAED-005"], "the review's own frozen lines stay");
    }

    [Fact]
    public async Task AtTheTraineesNewInstitution_TheirStarsStand_ButAReviewTheyStrandedSaysNothing_AndDoesNotHoldTheirSeat()
    {
        await using var db = await SeededDbAsync();
        db.DecisionPanels.Add(Panel(NeonatalPanelAtB, "B's neonatal CCC", InstitutionB, Neonatal, "chair-bn"));
        await SaveAndClearAsync(db);

        // At A, which has no neonatal panel: PAED-001 and PAED-004 decided; PAED-005 deferred, then due again at a second
        // sitting for the same semester, still open when the trainee leaves.
        var first = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        await StartWithEvidenceAsync(db, first.Id);
        await StageAsync(db, first.Id, EpaIdOf("PAED-001"));
        await StageAsync(db, first.Id, EpaIdOf("PAED-004"));
        await DeferAllOutstandingAsync(db, first.Id);
        await RecordDecisionAsync(db, first.Id);
        await RatifyAsync(db, first.Id);
        var second = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        (await AgendaAsync(db, second.Id)).Lines.Select(line => line.EpaCode).Should().Contain("PAED-005");
        await MoveAsync(db, Trainee, InstitutionB);

        var coordinatorAtB = TestPrincipals.Coordinator(InstitutionB);
        var preview = await PreviewAsync(db, PanelAtB, Trainee, 2026, 1, coordinatorAtB);

        preview.DecidedInWindow.Should().Contain("PAED-001", "a STAR is the trainee's, and goes with them");
        Elsewhere(preview, "PAED-004").Status.Should().Be(CommitteeAgendaElsewhereStatus.Decided);
        Elsewhere(preview, "PAED-005").Status.Should().Be(
            CommitteeAgendaElsewhereStatus.Missed,
            "A's open review can act on nothing now (T182), and its deferral was A's; semester 1 has ended at B");
        (await ScheduleAsync(db, PanelAtB, Trainee, 2026, 1, principal: coordinatorAtB)).Id
            .Should().NotBe(second.Id, "a review stranded at A does not hold the trainee's seat at B");
    }

    // ---- A STAR no agenda line records (T215) -----------------------------------------------------------------------

    [Fact]
    public async Task AnActiveStarNoAgendaLineRecords_DecidesItsWindow_SoItsEpaIsNotPlannedAgain_AndIsNamedAlreadyDecided()
    {
        // On dev, reviews 2 and 3 were ratified before agendas existed, so no line records their STARs, and review 4
        // planned PAED-001 and PAED-006 as Due although both were active in the window. The STAR is the fact.
        await using var db = await SeededDbAsync();
        await DecideAsync(db, GeneralPanel, 2026, 2, "PAED-001", "PAED-003");
        await ForgetAgendaLinesAsync(db);
        (await StatusesAsync(db)).Should().Equal(EntrustmentDecisionStatus.Active, EntrustmentDecisionStatus.Active);

        var preview = await PreviewAsync(db, GeneralPanel, Trainee, 2026, 2);

        preview.DecidedInWindow.Should().Equal("PAED-001", "PAED-003");
        preview.Lines.Select(line => line.EpaCode).Should().NotContain(["PAED-001", "PAED-003"]).And.Contain("PAED-002");

        var second = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 2);
        var scheduled = await AgendaAsync(db, second.Id);
        scheduled.Lines.Select(line => line.EpaCode).Should().Equal(preview.Lines.Select(line => line.EpaCode), "the preview is the agenda");
        scheduled.DecidedInWindow.Should().Equal("PAED-001", "PAED-003");

        await StartAsync(db, second.Id);
        (await AgendaAsync(db, second.Id)).Lines.Select(line => line.EpaCode)
            .Should().NotContain(["PAED-001", "PAED-003"], "Start plans again by the same rule");
    }

    [Fact]
    public async Task ASupersededStarNoAgendaLineRecords_StillDecidesItsWindow_WhenItsSuccessorSatForALaterOne()
    {
        await using var db = await SeededDbAsync();
        await DecideAsync(db, GeneralPanel, 2026, 1, "PAED-001");
        await DecideAsync(db, GeneralPanel, 2026, 2, "PAED-001");
        await ForgetAgendaLinesAsync(db);
        (await StatusesAsync(db)).Should().Equal(EntrustmentDecisionStatus.Superseded, EntrustmentDecisionStatus.Active);

        var preview = await PreviewAsync(db, GeneralPanel, Trainee, 2026, 1);

        preview.DecidedInWindow.Should().Contain("PAED-001", "it was superseded by a decision for semester 2, a later window");
        preview.Lines.Select(line => line.EpaCode).Should().NotContain("PAED-001");
    }

    [Fact]
    public async Task ARevokedStarNoAgendaLineRecords_DecidesNothing_NorDoesOneSupersededInItsWindowByOneSinceRevoked()
    {
        // PAED-001's one STAR is revoked. PAED-002's first STAR is superseded at a second sitting for the same semester,
        // whose STAR is revoked: the window's decision was the second, and it has none now.
        await using var db = await SeededDbAsync();
        await DecideAsync(db, GeneralPanel, 2026, 1, "PAED-001", "PAED-002");
        await DecideAsync(db, GeneralPanel, 2026, 1, "PAED-002");
        await ForgetAgendaLinesAsync(db);
        await RevokeActiveAsync(db, "PAED-001");
        await RevokeActiveAsync(db, "PAED-002");
        (await StatusesAsync(db)).Should().Equal(
            EntrustmentDecisionStatus.Revoked, EntrustmentDecisionStatus.Superseded, EntrustmentDecisionStatus.Revoked);

        var preview = await PreviewAsync(db, GeneralPanel, Trainee, 2026, 1);

        preview.DecidedInWindow.Should().BeEmpty();
        preview.Lines.Where(line => line.EpaCode is "PAED-001" or "PAED-002").Select(line => (line.EpaCode, line.Status))
            .Should().Equal(("PAED-001", CommitteeAgendaLineStatus.Due), ("PAED-002", CommitteeAgendaLineStatus.Due));
    }

    [Fact]
    public async Task AnotherPanelsStarNoAgendaLineRecords_ReadsDecided_UntilItIsRevoked()
    {
        await using var db = await SeededDbAsync(withNeonatalPanel: true);
        await DecideAsync(db, NeonatalPanel, 2026, 1, "PAED-004");
        await ForgetAgendaLinesAsync(db);

        var preview = await PreviewAsync(db, GeneralPanel, Trainee, 2026, 1);
        Elsewhere(preview, "PAED-004").Status.Should().Be(CommitteeAgendaElsewhereStatus.Decided);
        Elsewhere(preview, "PAED-005").Status.Should().Be(
            CommitteeAgendaElsewhereStatus.Missed, "semester 1 has ended, and nothing records a decision or a deferral");

        await RevokeActiveAsync(db, "PAED-004");

        Elsewhere(await PreviewAsync(db, GeneralPanel, Trainee, 2026, 1), "PAED-004").Status
            .Should().Be(CommitteeAgendaElsewhereStatus.Missed, "a revoked STAR decides nothing");
    }

    [Fact]
    public async Task TheAgendaCard_NamesWhatAStarAlreadyDecided_ButNotAnEpaTheReviewHoldsALineFor()
    {
        await using var db = await SeededDbAsync();
        var first = await DecideAsync(db, GeneralPanel, 2026, 1, "PAED-001", "PAED-003");

        (await AgendaAsync(db, first)).DecidedInWindow.Should().BeEmpty("its own Decided lines say what this sitting did with both");

        var second = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        (await AgendaAsync(db, second.Id)).DecidedInWindow.Should().Equal("PAED-001", "PAED-003");

        var formative = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1, formative: true);
        (await AgendaAsync(db, formative.Id)).DecidedInWindow.Should().BeEmpty("a formative review carries no agenda");
    }

    [Fact]
    public async Task AnAnnualEpasStarNoAgendaLineRecords_FromTheFirstSemestersSitting_DecidesTheYear_SoTheSecondSemesterDoesNotPlanIt()
    {
        // PAED-003 is decided annually: a STAR from the semester-1 sitting decides its 2026 window, which semester 2 closes.
        // PAED-001 is decided each semester, so the same sitting's STAR says nothing of semester 2's window.
        await using var db = await SeededDbAsync();
        await DecideAsync(db, GeneralPanel, 2026, 1, "PAED-001", "PAED-003");
        await ForgetAgendaLinesAsync(db);

        var preview = await PreviewAsync(db, GeneralPanel, Trainee, 2026, 2);

        preview.DecidedInWindow.Should().Equal("PAED-003");
        preview.Lines.Select(line => line.EpaCode).Should().NotContain("PAED-003");
        preview.Lines.Where(line => line.EpaCode == "PAED-001").Select(line => (line.WindowLabel, line.Status))
            .Should().Equal([("2026 S2", CommitteeAgendaLineStatus.Due)], "semester 2 is a window of its own");

        var second = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 2);
        await StartAsync(db, second.Id);
        var agenda = await AgendaAsync(db, second.Id);
        agenda.Lines.Select(line => line.EpaCode).Should().NotContain("PAED-003", "Start plans the year by the same rule");
        agenda.DecidedInWindow.Should().Equal("PAED-003");
    }

    [Fact]
    public async Task AStarNoAgendaLineRecords_FromTheTraineesPreviousInstitution_StillDecidesItsWindowAtTheirNewOne()
    {
        // A STAR is the trainee's, and goes with them, whether or not a line records it: B's panels do not decide again what
        // A's sitting decided in the window.
        await using var db = await SeededDbAsync();
        db.DecisionPanels.Add(Panel(NeonatalPanelAtB, "B's neonatal CCC", InstitutionB, Neonatal, "chair-bn"));
        await SaveAndClearAsync(db);
        await DecideAsync(db, GeneralPanel, 2026, 1, "PAED-001", "PAED-004");
        await ForgetAgendaLinesAsync(db);
        await MoveAsync(db, Trainee, InstitutionB);

        var preview = await PreviewAsync(db, PanelAtB, Trainee, 2026, 1, TestPrincipals.Coordinator(InstitutionB));

        preview.DecidedInWindow.Should().Contain("PAED-001");
        preview.Lines.Select(line => line.EpaCode).Should().NotContain("PAED-001").And.Contain("PAED-002");
        Elsewhere(preview, "PAED-004").Status.Should().Be(CommitteeAgendaElsewhereStatus.Decided, "A's general panel decided it");
    }

    // ---- What changes while a review is open (T235) -----------------------------------------------------------------

    [Fact]
    public async Task AClosingLine_WhoseWindowAnotherSittingDecides_ReadsDecidedElsewhere_AndBlocksNeitherRecordNorRatify()
    {
        // The semester-2 review holds PAED-003, an annual EPA, as closing. A late semester-1 sitting, still open when the
        // second started, stages PAED-003 and ratifies. Before T235 the line stayed Due, and Record refused with "PAED-003
        // must be decided at this sitting" while the decisions-due page showed PAED-003 as Decided.
        await using var db = await SeededDbAsync();
        var second = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 2);
        await StartWithEvidenceAsync(db, second.Id);
        Line(await AgendaAsync(db, second.Id), "PAED-003").BlocksRatify.Should().BeTrue("guard: the year's last sitting holds it as closing");

        await DecideAsync(db, GeneralPanel, 2026, 1, "PAED-003");

        var agenda = await AgendaAsync(db, second.Id);
        var line = Line(agenda, "PAED-003");
        line.State.Should().Be(CommitteeAgendaLineState.Due, "nothing is written when the agenda is read");
        line.IsClosing.Should().BeTrue();
        line.Status.Should().Be(CommitteeAgendaLineStatus.DecidedElsewhere);
        line.BlocksRatify.Should().BeFalse();
        agenda.OutstandingClosingLines.Select(outstanding => outstanding.EpaCode)
            .Should().Equal("PAED-001", "PAED-002", "PAED-004", "PAED-005");
        (await DueStatusAsync(db, "PAED-003"))
            .Should().Be(EntrustmentDecisionDueStatus.Decided, "the page and the agenda read one predicate");

        // A refused recording (the semester lines are still open) is refused for them alone, and settles nothing.
        var tooSoon = () => RecordDecisionAsync(db, second.Id);
        (await tooSoon.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(
            "The committee's decision cannot be recorded yet: PAED-001, PAED-002, PAED-004 and PAED-005 must be decided at " +
            "this sitting. Stage a decision on each, or defer it with a reason.");
        await SaveAndClearAsync(db);
        (await db.CommitteeReviews.SingleAsync(review => review.Id == second.Id)).State.Should().Be(CommitteeReviewState.InProgress);
        (await LineStateAsync(db, second.Id, "PAED-003")).Should().Be(CommitteeAgendaLineState.Due);

        await DeferAllOutstandingAsync(db, second.Id);
        await RecordDecisionAsync(db, second.Id);

        (await LineStateAsync(db, second.Id, "PAED-003"))
            .Should().Be(CommitteeAgendaLineState.DecidedElsewhere, "recording the decision settles the agenda (D46)");

        await RatifyAsync(db, second.Id);

        (await db.CommitteeReviews.SingleAsync(review => review.Id == second.Id)).State.Should().Be(CommitteeReviewState.Ratified);
        var ratified = Line(await AgendaAsync(db, second.Id), "PAED-003");
        ratified.Status.Should().Be(CommitteeAgendaLineStatus.DecidedElsewhere);
        ratified.EntrustmentDecisionId.Should().BeNull("the STAR is the other sitting's");
        (await db.EntrustmentDecisions.CountAsync(star => star.EpaId == EpaIdOf("PAED-003"))).Should().Be(1);
    }

    [Fact]
    public async Task ALineSettledAsDecidedElsewhere_StaysSettled_WhenTheOtherSittingsStarIsRevokedBeforeRatify()
    {
        // D46: the decision recorded fixes the agenda. The revocation is the decisions-due page's to show, and the next
        // sitting's to plan.
        await using var db = await SeededDbAsync();
        var second = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 2);
        await StartWithEvidenceAsync(db, second.Id);
        await DecideAsync(db, GeneralPanel, 2026, 1, "PAED-003");
        await DeferAllOutstandingAsync(db, second.Id);
        await RecordDecisionAsync(db, second.Id);

        await RevokeActiveAsync(db, "PAED-003");
        (await AgendaAsync(db, second.Id)).RatifyBlockedReason.Should().BeNull();
        await RatifyAsync(db, second.Id);

        (await LineStateAsync(db, second.Id, "PAED-003")).Should().Be(CommitteeAgendaLineState.DecidedElsewhere);
        (await DueStatusAsync(db, "PAED-003"))
            .Should().Be(EntrustmentDecisionDueStatus.Revoked);
        (await PreviewAsync(db, GeneralPanel, Trainee, 2026, 2)).Lines.Select(line => line.EpaCode)
            .Should().Contain("PAED-003", "the next sitting plans it again");
    }

    [Fact]
    public async Task Ratify_ReadsTheAgendaAgain_SettlingALineDecidedElsewhereSinceTheRecording_AndARefusedRatifySettlesNothing()
    {
        // Recorded with PAED-008 (annual, as opportunity allows) still open and optional. A late semester-1 sitting then
        // decides it. Ratify reads the agenda again, before its first mutation, and settles the line DecidedElsewhere, not
        // NotDecided. First, a ratify refused for another line (T167's exception: PAED-001's staged decision removed after
        // the recording) writes nothing.
        await using var db = await SeededDbAsync();
        var second = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 2);
        await StartWithEvidenceAsync(db, second.Id);
        var staged = await StageAsync(db, second.Id, EpaIdOf("PAED-001"));
        await DeferAllOutstandingAsync(db, second.Id);
        await RecordDecisionAsync(db, second.Id);
        (await LineStateAsync(db, second.Id, "PAED-008")).Should().Be(CommitteeAgendaLineState.Due, "guard: undecided when recorded");

        await DecideAsync(db, GeneralPanel, 2026, 1, "PAED-008");
        (await db.Epas.SingleAsync(epa => epa.Id == EpaIdOf("PAED-001"))).Deactivate(DateTime.MinValue);
        await SaveAndClearAsync(db);
        await RemoveAsync(db, second.Id, staged.Id);

        var ratify = () => RatifyAsync(db, second.Id);

        (await ratify.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(
            "This review cannot be ratified: PAED-001 must be decided at this sitting. Stage a decision on it, or defer it " +
            "with a reason.");
        await SaveAndClearAsync(db);
        (await db.CommitteeReviews.SingleAsync(review => review.Id == second.Id)).State.Should().Be(CommitteeReviewState.Decided);
        (await LineStateAsync(db, second.Id, "PAED-008")).Should().Be(CommitteeAgendaLineState.Due, "a refused ratify settles nothing");
        (await db.EntrustmentDecisions.CountAsync()).Should().Be(1, "only the semester-1 sitting's");
        Line(await AgendaAsync(db, second.Id), "PAED-008").Status.Should().Be(CommitteeAgendaLineStatus.DecidedElsewhere);

        await DeferAsync(db, second.Id, LineIdOf(await AgendaAsync(db, second.Id), "PAED-001"), "The EPA was withdrawn.");
        await RatifyAsync(db, second.Id);

        (await LineStateAsync(db, second.Id, "PAED-008")).Should().Be(CommitteeAgendaLineState.DecidedElsewhere);
        (await LineStateAsync(db, second.Id, "PAED-001")).Should().Be(CommitteeAgendaLineState.Deferred);
        (await db.CommitteeReviews.SingleAsync(review => review.Id == second.Id)).State.Should().Be(CommitteeReviewState.Ratified);
    }

    [Fact]
    public async Task AClosingLineWhoseStagedDecisionWasRemovedAfterRecording_ButWhichAnotherSittingDecided_IsNotDeferred_AndRatifies()
    {
        // T167's exception reopens PAED-003's closing line on a decided review; a semester-1 sitting has decided its year by
        // then. It keeps nothing from being ratified, so the one change a decided agenda takes is refused for it.
        await using var db = await SeededDbAsync();
        var second = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 2);
        await StartWithEvidenceAsync(db, second.Id);
        var staged = await StageAsync(db, second.Id, EpaIdOf("PAED-003"));
        await DeferAllOutstandingAsync(db, second.Id);
        await RecordDecisionAsync(db, second.Id);
        await DecideAsync(db, GeneralPanel, 2026, 1, "PAED-003");
        (await db.Epas.SingleAsync(epa => epa.Id == EpaIdOf("PAED-003"))).Deactivate(DateTime.MinValue);
        await SaveAndClearAsync(db);
        await RemoveAsync(db, second.Id, staged.Id);

        var agenda = await AgendaAsync(db, second.Id);
        Line(agenda, "PAED-003").Status.Should().Be(CommitteeAgendaLineStatus.DecidedElsewhere);
        agenda.RatifyBlockedReason.Should().BeNull();

        var defer = () => DeferAsync(db, second.Id, LineIdOf(agenda, "PAED-003"), "Withdrawn.");
        (await defer.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(DeferAgendaLineCommandHandler.FixedWhenDecided);
        await SaveAndClearAsync(db);
        (await LineStateAsync(db, second.Id, "PAED-003")).Should().Be(CommitteeAgendaLineState.Due);

        await RatifyAsync(db, second.Id);

        (await LineStateAsync(db, second.Id, "PAED-003")).Should().Be(CommitteeAgendaLineState.DecidedElsewhere);
    }

    [Fact]
    public async Task AnEpaStartLeftOffBecauseAStarDecidedIt_IsNamedWhenThatStarIsRevokedWhileTheReviewSits_AndIsNotAddedToTheAgenda()
    {
        await using var db = await SeededDbAsync();
        await DecideAsync(db, GeneralPanel, 2026, 1, "PAED-003", "PAED-008");
        var second = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 2);
        await StartWithEvidenceAsync(db, second.Id);
        var started = await AgendaAsync(db, second.Id);
        started.DecidedInWindow.Should().Equal("PAED-003", "PAED-008");
        started.NoLongerDecided.Should().BeEmpty();

        await RevokeActiveAsync(db, "PAED-003");
        await RevokeActiveAsync(db, "PAED-008");

        var sitting = await AgendaAsync(db, second.Id);
        sitting.NoLongerDecided.Should().Equal(
            new CommitteeAgendaEpaDto(EpaIdOf("PAED-003"), "PAED-003", "EPA 3"),
            new CommitteeAgendaEpaDto(EpaIdOf("PAED-008"), "PAED-008", "EPA 8"));
        sitting.DecidedInWindow.Should().BeEmpty();
        sitting.Lines.Select(line => line.EpaCode).Should().NotContain(["PAED-003", "PAED-008"]);
        (await db.CommitteeAgendaLines.AnyAsync(line => line.ReviewId == second.Id && (line.EpaCode == "PAED-003" || line.EpaCode == "PAED-008")))
            .Should().BeFalse("the agenda Start planned is not changed under the chair (D46)");

        // Staging it adds the chair's line, as for any EPA that routes here, and it is named no longer.
        await StageAsync(db, second.Id, EpaIdOf("PAED-003"));
        var staged = await AgendaAsync(db, second.Id);
        Line(staged, "PAED-003").Origin.Should().Be(CommitteeAgendaLineOrigin.Chair);
        Line(staged, "PAED-003").Status.Should().Be(CommitteeAgendaLineStatus.Staged);
        staged.NoLongerDecided.Select(epa => epa.EpaCode).Should().Equal("PAED-008");

        // Once the decision is recorded nothing more is staged, so nothing is named: the decisions-due page says it.
        await DeferAllOutstandingAsync(db, second.Id);
        await RecordDecisionAsync(db, second.Id);
        (await AgendaAsync(db, second.Id)).NoLongerDecided.Should().BeEmpty();
        (await DueStatusAsync(db, "PAED-008"))
            .Should().Be(EntrustmentDecisionDueStatus.Revoked);
    }

    [Fact]
    public async Task ADecisionRevokedBeforeStart_IsNotNamed_ForStartPlansIt()
    {
        await using var db = await SeededDbAsync();
        await DecideAsync(db, GeneralPanel, 2026, 1, "PAED-003");
        var second = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 2);
        (await AgendaAsync(db, second.Id)).Lines.Select(line => line.EpaCode).Should().NotContain("PAED-003", "guard: decided when scheduled");

        await RevokeActiveAsync(db, "PAED-003");

        (await AgendaAsync(db, second.Id)).NoLongerDecided.Should().BeEmpty("a scheduled review is planned again when it starts");
        await StartAsync(db, second.Id);
        var line = Line(await AgendaAsync(db, second.Id), "PAED-003");
        line.Origin.Should().Be(CommitteeAgendaLineOrigin.Cadence);
        line.Status.Should().Be(CommitteeAgendaLineStatus.Due);
    }

    [Fact]
    public async Task AReviewWhosePanelNoLongerCoversTheTraineesSpeciality_StillReadsItsLinesAgainstTheWindowRule()
    {
        // The Paediatrics CCC's semester-2 review holds PAED-003 as closing. The general panel's late semester-1 sitting
        // decides it, and then the trainee changes speciality at the same hospital: the Paediatrics CCC plans nothing more
        // for them, but a line another sitting decided must still not block Record or Ratify, as the decisions-due page
        // calls it decided. Nothing here is another institution's.
        await using var db = await SeededDbAsync();
        db.DecisionPanels.Add(Panel(SpecialityPanel, "Paediatrics CCC", InstitutionA, null, "chair-s", "member-s", specialityId: 1));
        db.Specialities.Add(new Speciality { Id = 2, CollegeId = 1, Name = "Surgery", IsActive = true });
        db.SubSpecialities.Add(new SubSpeciality { Id = 21, SpecialityId = 2, Name = "General Surgery", IsActive = true });
        db.Curricula.Add(new Curriculum { Id = 200, SubSpecialityId = 21, Name = "General Surgery", Version = "1" });
        await SaveAndClearAsync(db);
        var second = await ScheduleAsync(db, SpecialityPanel, Trainee, 2026, 2);
        await StartWithEvidenceAsync(db, second.Id);
        await DecideAsync(db, GeneralPanel, 2026, 1, "PAED-003");

        (await db.TraineeProfiles.SingleAsync(profile => profile.UserId == Trainee)).CurriculumId = 200;
        await SaveAndClearAsync(db);

        var agenda = await AgendaAsync(db, second.Id, TestPrincipals.InstitutionalAdmin(InstitutionA));
        agenda.RoutedElsewhere.Should().BeEmpty("guard: the panel no longer covers the trainee, so it plans nothing");
        var line = Line(agenda, "PAED-003");
        line.Status.Should().Be(CommitteeAgendaLineStatus.DecidedElsewhere);
        line.BlocksRatify.Should().BeFalse();

        await DeferAllOutstandingAsync(db, second.Id);
        await RecordDecisionAsync(db, second.Id);
        (await LineStateAsync(db, second.Id, "PAED-003")).Should().Be(CommitteeAgendaLineState.DecidedElsewhere);

        await RatifyAsync(db, second.Id);
        (await db.CommitteeReviews.SingleAsync(review => review.Id == second.Id)).State.Should().Be(CommitteeReviewState.Ratified);
    }

    [Fact]
    public async Task AReviewTheTraineeLeftForAnotherInstitution_DoesNotSayThatASittingThereDecidedItsLine()
    {
        // T182: A's stranded review says nothing of what B did. B's sitting decides PAED-003's year after the trainee moves;
        // A's line on it reads as it did, since "decided elsewhere" would tell A what B decided. A's chair can act on
        // nothing for the trainee now (CommitteeTraineeScope).
        await using var db = await SeededDbAsync();
        var panelAtB = await db.DecisionPanels.Include(panel => panel.Members).SingleAsync(panel => panel.Id == PanelAtB);
        panelAtB.Members.Add(new DecisionPanelMember { UserId = "member-b", Role = DecisionPanelMemberRole.Member });
        await SaveAndClearAsync(db);
        var second = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 2);
        await StartWithEvidenceAsync(db, second.Id);
        await MoveAsync(db, Trainee, InstitutionB);

        var atB = await ScheduleAsync(db, PanelAtB, Trainee, 2026, 1, principal: TestPrincipals.Coordinator(InstitutionB));
        await StartWithEvidenceAsync(db, atB.Id);
        await StageAsync(db, atB.Id, EpaIdOf("PAED-003"));
        await DeferAllOutstandingAsync(db, atB.Id);
        await RecordDecisionAsync(db, atB.Id);
        await RatifyAsync(db, atB.Id);
        (await db.EntrustmentDecisions.CountAsync(star => star.EpaId == EpaIdOf("PAED-003") && star.Status == EntrustmentDecisionStatus.Active))
            .Should().Be(1, "guard: B's sitting decided PAED-003's year");

        foreach (var reader in new[] { TestPrincipals.InstitutionalAdmin(InstitutionA), TestPrincipals.Administrator() })
        {
            var line = Line(await AgendaAsync(db, second.Id, reader), "PAED-003");
            line.Status.Should().Be(CommitteeAgendaLineStatus.Due);
            line.State.Should().Be(CommitteeAgendaLineState.Due);
        }
    }

    // ---- Scheduling -------------------------------------------------------------------------------------------------

    [Fact]
    public async Task ASecondOpenBindingReview_BeforeThePanelForThePeriod_IsRefusedNamingTheFirst_AndWritesNothing()
    {
        await using var db = await SeededDbAsync();
        var first = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);

        var second = () => ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);

        (await second.Should().ThrowAsync<InvalidOperationException>()).Which.Message
            .Should().StartWith($"Review #{first.Id} already puts this trainee before General CCC for 2026 S1 (scheduled, scheduled 2026-07-02).");
        await SaveAndClearAsync(db);
        (await db.CommitteeReviews.CountAsync()).Should().Be(1);
        (await db.CommitteeAgendaLines.CountAsync()).Should().Be(6, "only the first review's agenda");

        // A formative sitting, another period, and another panel are each a different review.
        await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1, formative: true);
        await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 2);
        (await db.CommitteeReviews.CountAsync()).Should().Be(3);
    }

    [Fact]
    public async Task ASecondGeneralPanel_CannotSeatTheTraineeForAPeriodTheFirstHolds_ButANeonatalPanelAndAnotherPeriodCan()
    {
        // An EPA with no College committee routes to every eligible general panel: the institution-wide one and the
        // Paediatrics one would each hold PAED-001 and 002 as closing for 2026 S1, and each ratify a STAR on them.
        await using var db = await SeededDbAsync(withNeonatalPanel: true);
        db.DecisionPanels.Add(Panel(SpecialityPanel, "Paediatrics CCC", InstitutionA, null, "chair-s", specialityId: 1));
        await SaveAndClearAsync(db);
        var first = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);

        var second = () => ScheduleAsync(db, SpecialityPanel, Trainee, 2026, 1);

        (await second.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(
            $"Review #{first.Id} already puts this trainee before General CCC for 2026 S1 (scheduled, scheduled 2026-07-02). " +
            "A trainee has one binding review for each period before the panels that decide the same EPAs, until it is " +
            "ratified: open that review, or ratify it before scheduling another.");
        await SaveAndClearAsync(db);
        (await db.CommitteeReviews.CountAsync()).Should().Be(1);
        (await db.CommitteeAgendaLines.CountAsync(line => line.Review.PanelId == SpecialityPanel)).Should().Be(0);

        // The neonatal CCC is another seat; another period, and a formative sitting, are other reviews.
        await ScheduleAsync(db, NeonatalPanel, Trainee, 2026, 1);
        await ScheduleAsync(db, SpecialityPanel, Trainee, 2026, 2);
        await ScheduleAsync(db, SpecialityPanel, Trainee, 2026, 1, formative: true);
        (await db.CommitteeReviews.CountAsync()).Should().Be(4);
    }

    [Fact]
    public async Task OnceTheFirstIsRatified_AnotherSittingForThePeriodCanBeScheduled()
    {
        await using var db = await SeededDbAsync();
        var first = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        await StartWithEvidenceAsync(db, first.Id);
        await DeferAllOutstandingAsync(db, first.Id);
        await RecordDecisionAsync(db, first.Id);
        await RatifyAsync(db, first.Id);

        var remediation = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);

        remediation.Id.Should().NotBe(first.Id);
    }

    [Fact]
    public async Task ThePreview_IsRefusedToAnyoneWhoMayNotScheduleTheTrainee_AsSchedulingIs()
    {
        await using var db = await SeededDbAsync();

        var fromB = () => PreviewAsync(db, GeneralPanel, Trainee, 2026, 1, TestPrincipals.Coordinator(InstitutionB));
        (await fromB.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(NotSchedulable);

        var unknownPanel = () => PreviewAsync(db, 999, Trainee, 2026, 1);
        (await unknownPanel.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(NotSchedulable);
    }

    [Fact]
    public void TheSchedulingValidator_RefusesASemesterThatIsNotOneOrTwo()
    {
        var validator = new ScheduleCommitteeReviewCommandValidator();
        var command = new ScheduleCommitteeReviewCommand(
            Trainee, GeneralPanel, 2026, 3, new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), new DateOnly(2026, 7, 2),
            TestPrincipals.Coordinator(InstitutionA));

        validator.Validate(command).Errors.Select(error => error.ErrorMessage)
            .Should().Contain("A review sits for semester 1 or semester 2 of an academic year.");
        validator.Validate(command with { Semester = 2 }).IsValid.Should().BeTrue();
    }

    // ---- Staging ----------------------------------------------------------------------------------------------------

    [Fact]
    public async Task StagingPaed004AtTheGeneralReview_WhereTheNeonatalPanelDecidesIt_IsRefused_AndWritesNothing()
    {
        await using var db = await SeededDbAsync(withNeonatalPanel: true);
        var review = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        await StartWithEvidenceAsync(db, review.Id);

        var stage = () => StageAsync(db, review.Id, EpaIdOf("PAED-004"));

        (await stage.Should().ThrowAsync<InvalidOperationException>()).Which.Message
            .Should().Be("PAED-004 is not on this review's agenda: Neonatal CCC decides it for this trainee. Stage it at a review before that panel.");
        await SaveAndClearAsync(db);
        (await db.PendingEntrustmentDecisions.CountAsync()).Should().Be(0);
        (await db.CommitteeAgendaLines.AnyAsync(line => line.EpaCode == "PAED-004")).Should().BeFalse();
    }

    [Fact]
    public async Task StagingAnEpaThatRoutesHere_ButIsNotOnTheAgenda_AddsTheChairsLine_AndRemovingTheDecisionTakesItOff()
    {
        await using var db = await SeededDbAsync();
        var review = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        await StartWithEvidenceAsync(db, review.Id);

        var staged = await StageAsync(db, review.Id, EpaIdOf("PAED-020"));

        var line = Line(await AgendaAsync(db, review.Id), "PAED-020");
        line.Origin.Should().Be(CommitteeAgendaLineOrigin.Chair);
        line.WindowLabel.Should().Be("2026 S1", "an EPA with no cadence is decided in the sitting's semester");
        line.IsClosing.Should().BeFalse();
        line.Status.Should().Be(CommitteeAgendaLineStatus.Staged);

        await RemoveAsync(db, review.Id, staged.Id);
        (await AgendaAsync(db, review.Id)).Lines.Select(agendaLine => agendaLine.EpaCode).Should().NotContain("PAED-020");

        // A cadence line stays on the agenda, due, when its staged decision is removed.
        var onTheAgenda = await StageAsync(db, review.Id, EpaIdOf("PAED-001"));
        await RemoveAsync(db, review.Id, onTheAgenda.Id);
        Line(await AgendaAsync(db, review.Id), "PAED-001").Status.Should().Be(CommitteeAgendaLineStatus.Due);
    }

    [Fact]
    public async Task AnEpaDeferredAtTheReview_CannotBeStaged_UntilItIsReinstated()
    {
        await using var db = await SeededDbAsync();
        var review = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        await StartWithEvidenceAsync(db, review.Id);
        var lineId = LineIdOf(await AgendaAsync(db, review.Id), "PAED-002");
        await DeferAsync(db, review.Id, lineId, "Rotation moved to next semester.");

        var stage = () => StageAsync(db, review.Id, EpaIdOf("PAED-002"));
        (await stage.Should().ThrowAsync<InvalidOperationException>()).Which.Message
            .Should().Be("PAED-002 is deferred at this review. Reinstate it to stage a decision on it.");
        await SaveAndClearAsync(db);
        (await db.PendingEntrustmentDecisions.CountAsync()).Should().Be(0);

        await ReinstateAsync(db, review.Id, lineId);
        await StageAsync(db, review.Id, EpaIdOf("PAED-002"));
        Line(await AgendaAsync(db, review.Id), "PAED-002").Status.Should().Be(CommitteeAgendaLineStatus.Staged);
    }

    [Fact]
    public async Task AReviewStartedWithNoAgenda_StagesThroughTheChairsLine_AndRatifiesWithNothingOutstanding()
    {
        // How the reviews started before agendas existed behave (dev reviews 1 and 2): no lines, so nothing is closing.
        await using var db = await SeededDbAsync();
        var review = new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 2,
            TraineeUserId = Trainee,
            PanelId = GeneralPanel,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 12, 31),
            ScheduledOn = new DateOnly(2026, 9, 24)
        };
        review.Start([], "chair-a", DateTime.UtcNow);
        db.CommitteeReviews.Add(review);
        await SaveAndClearAsync(db);
        await AddEvidenceAsync(db, review.Id);

        await StageAsync(db, review.Id, EpaIdOf("PAED-001"));
        await RecordDecisionAsync(db, review.Id);
        await RatifyAsync(db, review.Id);

        var agenda = await AgendaAsync(db, review.Id);
        Line(agenda, "PAED-001").Origin.Should().Be(CommitteeAgendaLineOrigin.Chair);
        Line(agenda, "PAED-001").Status.Should().Be(CommitteeAgendaLineStatus.Decided);
        agenda.Lines.Should().ContainSingle();
    }

    // ---- Deferring and reinstating ----------------------------------------------------------------------------------

    public static TheoryData<string> NotTheChair => new() { "member", "another panel's chair", "unknown review" };

    [Theory]
    [MemberData(nameof(NotTheChair))]
    public async Task OnlyTheChair_DefersALine_AndEveryoneElseGetsTheOneRefusal_WithNothingWritten(string who)
    {
        await using var db = await SeededDbAsync();
        var review = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        await StartWithEvidenceAsync(db, review.Id);
        var lineId = LineIdOf(await AgendaAsync(db, review.Id), "PAED-001");

        var (reviewId, principal) = who switch
        {
            "member" => (review.Id, TestPrincipals.InRole(WombatRoles.CommitteeMember, "member-a", InstitutionA)),
            "another panel's chair" => (review.Id, TestPrincipals.InRole(WombatRoles.CommitteeMember, "chair-b", InstitutionB)),
            _ => (review.Id + 100, Chair())
        };

        var defer = () => DeferAsync(db, reviewId, lineId, "Not observed.", principal);

        (await defer.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message
            .Should().Be("The committee review could not be found among the reviews you chair.");
        await SaveAndClearAsync(db);
        (await db.CommitteeAgendaLines.SingleAsync(line => line.Id == lineId)).State.Should().Be(CommitteeAgendaLineState.Due);
    }

    [Fact]
    public async Task ADeferral_NeedsAReason()
    {
        await using var db = await SeededDbAsync();
        var review = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        await StartWithEvidenceAsync(db, review.Id);
        var lineId = LineIdOf(await AgendaAsync(db, review.Id), "PAED-001");

        new DeferAgendaLineCommandValidator().Validate(new DeferAgendaLineCommand(review.Id, lineId, "", Chair()))
            .Errors.Select(error => error.ErrorMessage).Should().Contain(CommitteeAgendaLine.DeferralReasonRequired);

        var blank = () => DeferAsync(db, review.Id, lineId, "   ");
        (await blank.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(CommitteeAgendaLine.DeferralReasonRequired);
        await SaveAndClearAsync(db);
        (await db.CommitteeAgendaLines.SingleAsync(line => line.Id == lineId)).State.Should().Be(CommitteeAgendaLineState.Due);
    }

    [Fact]
    public async Task ALineWithADecisionStaged_CannotBeDeferred()
    {
        await using var db = await SeededDbAsync();
        var review = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        await StartWithEvidenceAsync(db, review.Id);
        await StageAsync(db, review.Id, EpaIdOf("PAED-001"));
        var lineId = LineIdOf(await AgendaAsync(db, review.Id), "PAED-001");

        var defer = () => DeferAsync(db, review.Id, lineId, "Changed our minds.");

        (await defer.Should().ThrowAsync<InvalidOperationException>()).Which.Message
            .Should().Be("A decision on PAED-001 is staged at this review. Remove it to defer the EPA instead.");
        await SaveAndClearAsync(db);
        (await db.CommitteeAgendaLines.SingleAsync(line => line.Id == lineId)).DeferralReason.Should().BeNull();
    }

    [Fact]
    public async Task ALine_IsDeferredOnlyWhileTheReviewSits()
    {
        await using var db = await SeededDbAsync();
        var review = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        var lineId = LineIdOf(await AgendaAsync(db, review.Id), "PAED-001");

        var beforeStart = () => DeferAsync(db, review.Id, lineId, "Too early.");

        (await beforeStart.Should().ThrowAsync<InvalidOperationException>()).Which.Message
            .Should().Be("An agenda line can be changed only while the review is in progress.");
    }

    [Fact]
    public async Task OnceTheDecisionIsRecorded_NoLineIsDeferredOrReinstated_AndNothingIsWritten()
    {
        // T165: the deferrals are part of the decision the panel records, as the staged decisions are, and are fixed with
        // it. The chair alone, after the sitting, neither puts off an EPA the committee left optional nor takes back a
        // deferral the committee made.
        await using var db = await SeededDbAsync();
        var review = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        await StartWithEvidenceAsync(db, review.Id);
        await StageAsync(db, review.Id, EpaIdOf("PAED-001"));
        await DeferAllOutstandingAsync(db, review.Id);
        await RecordDecisionAsync(db, review.Id);
        var agenda = await AgendaAsync(db, review.Id);

        var deferOptional = () => DeferAsync(db, review.Id, LineIdOf(agenda, "PAED-003"), "After the sitting.");
        var deferStaged = () => DeferAsync(db, review.Id, LineIdOf(agenda, "PAED-001"), "After the sitting.");
        var reinstate = () => ReinstateAsync(db, review.Id, LineIdOf(agenda, "PAED-002"));

        (await deferOptional.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(DeferAgendaLineCommandHandler.FixedWhenDecided);
        (await deferStaged.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(DeferAgendaLineCommandHandler.FixedWhenDecided);
        (await reinstate.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(DeferAgendaLineCommandHandler.FixedWhenDecided);
        await SaveAndClearAsync(db);
        var after = await AgendaAsync(db, review.Id);
        Line(after, "PAED-003").State.Should().Be(CommitteeAgendaLineState.Due);
        Line(after, "PAED-003").DeferralReason.Should().BeNull();
        Line(after, "PAED-001").Status.Should().Be(CommitteeAgendaLineStatus.Staged);
        Line(after, "PAED-002").State.Should().Be(CommitteeAgendaLineState.Deferred);
        Line(after, "PAED-002").DeferralReason.Should().Be("Not decided at this sitting.");
    }

    [Fact]
    public async Task ATraineeWhoHasMovedAway_HasNoLineDeferredByTheirOldPanel()
    {
        await using var db = await SeededDbAsync();
        var review = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        await StartWithEvidenceAsync(db, review.Id);
        var lineId = LineIdOf(await AgendaAsync(db, review.Id), "PAED-001");
        (await db.TraineeProfiles.SingleAsync(profile => profile.UserId == Trainee)).InstitutionId = InstitutionB;
        await SaveAndClearAsync(db);

        var defer = () => DeferAsync(db, review.Id, lineId, "Moved.");

        (await defer.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message
            .Should().Be("This review's trainee does not train at the panel's institution, so the panel cannot act on it.");
    }

    // ---- Ratifying --------------------------------------------------------------------------------------------------

    [Fact]
    public async Task Record_IsRefusedWhileAClosingLineIsNeitherStagedNorDeferred_AndWritesNothing()
    {
        // The decision fixes the agenda with the staged decisions (T165), so recording it with a closing line still open
        // would leave a review nothing could ratify: the committee settles its agenda first.
        await using var db = await SeededDbAsync();
        var review = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        await StartWithEvidenceAsync(db, review.Id);
        await StageAsync(db, review.Id, EpaIdOf("PAED-001"));

        var record = () => RecordDecisionAsync(db, review.Id);

        (await record.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(
            "The committee's decision cannot be recorded yet: PAED-002, PAED-004 and PAED-005 must be decided at this " +
            "sitting. Stage a decision on each, or defer it with a reason.");
        await SaveAndClearAsync(db);
        (await db.CommitteeReviews.SingleAsync()).State.Should().Be(CommitteeReviewState.InProgress);
        (await db.CommitteeDecisions.CountAsync()).Should().Be(0);
        (await db.PendingEntrustmentDecisions.CountAsync()).Should().Be(1);
        (await db.CommitteeAgendaLines.AllAsync(line => line.State == CommitteeAgendaLineState.Due)).Should().BeTrue();
        (await AgendaAsync(db, review.Id)).RatifyBlockedReason.Should().Be(
            "PAED-002, PAED-004 and PAED-005 must be decided at this sitting. Stage a decision on each, or defer it with a reason.",
            "the page's disabled Record and Ratify say what the handlers refuse");
    }

    [Fact]
    public async Task Ratify_IsRefusedWhileAClosingLineIsNeitherStagedNorDeferred_AndWritesNothing_UntilTheChairDefersIt()
    {
        // Recording demands a settled agenda, so a closing line is open on a decided review only when the decision staged
        // on it was removed afterwards because it no longer fits the trainee's curriculum (T165's one exception, T167).
        // Ratify refuses the review then, and deferring that line, the one change a decided agenda still takes, lets it
        // through.
        await using var db = await SeededDbAsync();
        var review = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        await StartWithEvidenceAsync(db, review.Id);
        var staged = await StageAsync(db, review.Id, EpaIdOf("PAED-001"));
        await DeferAllOutstandingAsync(db, review.Id);
        await RecordDecisionAsync(db, review.Id);
        (await db.Epas.SingleAsync(epa => epa.Id == EpaIdOf("PAED-001"))).Deactivate(DateTime.MinValue);
        await SaveAndClearAsync(db);
        await RemoveAsync(db, review.Id, staged.Id);

        var ratify = () => RatifyAsync(db, review.Id);

        (await ratify.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(
            "This review cannot be ratified: PAED-001 must be decided at this sitting. Stage a decision on it, or defer it " +
            "with a reason.");
        await SaveAndClearAsync(db);
        (await db.CommitteeReviews.SingleAsync()).State.Should().Be(CommitteeReviewState.Decided);
        (await db.EntrustmentDecisions.CountAsync()).Should().Be(0);
        (await db.CommitteeAgendaLines.SingleAsync(line => line.EpaCode == "PAED-001")).State.Should().Be(CommitteeAgendaLineState.Due);
        var blocked = await AgendaAsync(db, review.Id);
        blocked.RatifyBlockedReason.Should().Be(
            "PAED-001 must be decided at this sitting. Stage a decision on it, or defer it with a reason.",
            "the page's disabled Ratify says what the handler refuses");

        await DeferAsync(db, review.Id, LineIdOf(blocked, "PAED-001"), "The EPA was withdrawn from the curriculum.");
        await RatifyAsync(db, review.Id);

        (await db.CommitteeReviews.SingleAsync()).State.Should().Be(CommitteeReviewState.Ratified);
        (await db.EntrustmentDecisions.CountAsync()).Should().Be(0);
        Line(await AgendaAsync(db, review.Id), "PAED-001").DeferralReason.Should().Be("The EPA was withdrawn from the curriculum.");
    }

    [Fact]
    public async Task TheAgenda_ReportsEachLinesOwnState_BeforeAndAfterRatify()
    {
        await using var db = await SeededDbAsync();
        var review = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        await StartWithEvidenceAsync(db, review.Id);
        await StageAsync(db, review.Id, EpaIdOf("PAED-001"));
        var before = await AgendaAsync(db, review.Id);
        await DeferAsync(db, review.Id, LineIdOf(before, "PAED-002"), "Rotation moved to next semester.");

        var sitting = await AgendaAsync(db, review.Id);
        Line(sitting, "PAED-001").Status.Should().Be(CommitteeAgendaLineStatus.Staged);
        Line(sitting, "PAED-001").IsStaged.Should().BeTrue();
        Line(sitting, "PAED-001").EvidenceCount.Should().Be(1);
        Line(sitting, "PAED-002").Status.Should().Be(CommitteeAgendaLineStatus.Deferred);
        Line(sitting, "PAED-002").DeferralReason.Should().Be("Rotation moved to next semester.");
        Line(sitting, "PAED-003").Status.Should().Be(CommitteeAgendaLineStatus.DueByYearEnd);
        Line(sitting, "PAED-004").Status.Should().Be(CommitteeAgendaLineStatus.Due);
        Line(sitting, "PAED-004").BlocksRatify.Should().BeTrue();
        sitting.OutstandingClosingLines.Select(line => line.EpaCode).Should().Equal("PAED-004", "PAED-005");

        await DeferAsync(db, review.Id, LineIdOf(sitting, "PAED-004"), "The neonatal rotation has not started.");
        await DeferAsync(db, review.Id, LineIdOf(sitting, "PAED-005"), "The neonatal rotation has not started.");
        await RecordDecisionAsync(db, review.Id);
        await RatifyAsync(db, review.Id);

        var ratified = await AgendaAsync(db, review.Id);
        var star = await db.EntrustmentDecisions.AsNoTracking().SingleAsync();
        Line(ratified, "PAED-001").State.Should().Be(CommitteeAgendaLineState.Decided);
        Line(ratified, "PAED-001").Status.Should().Be(CommitteeAgendaLineStatus.Decided);
        Line(ratified, "PAED-001").EntrustmentDecisionId.Should().Be(star.Id);
        star.EpaId.Should().Be(EpaIdOf("PAED-001"));
        Line(ratified, "PAED-002").State.Should().Be(CommitteeAgendaLineState.Deferred);
        Line(ratified, "PAED-002").DeferralReason.Should().Be("Rotation moved to next semester.");
        Line(ratified, "PAED-003").State.Should().Be(CommitteeAgendaLineState.NotDecided);
        Line(ratified, "PAED-008").State.Should().Be(CommitteeAgendaLineState.NotDecided);
        ratified.OutstandingClosingLines.Should().BeEmpty();
    }

    [Fact]
    public async Task AnAnnualLineAtTheSemester1Sitting_DoesNotBlockRatify()
    {
        await using var db = await SeededDbAsync();
        var review = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        await StartWithEvidenceAsync(db, review.Id);
        await DeferAllOutstandingAsync(db, review.Id);
        await RecordDecisionAsync(db, review.Id);

        await RatifyAsync(db, review.Id);

        (await db.CommitteeReviews.SingleAsync()).State.Should().Be(CommitteeReviewState.Ratified);
        (await db.CommitteeAgendaLines.SingleAsync(line => line.EpaCode == "PAED-003")).State
            .Should().Be(CommitteeAgendaLineState.NotDecided);
    }

    [Fact]
    public async Task AnAnnualLineAtTheSemester2Sitting_BlocksRatify()
    {
        await using var db = await SeededDbAsync();
        var review = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 2);
        await StartWithEvidenceAsync(db, review.Id);
        foreach (var code in new[] { "PAED-001", "PAED-002", "PAED-004", "PAED-005" })
        {
            await DeferAsync(db, review.Id, LineIdOf(await AgendaAsync(db, review.Id), code), "Not yet.");
        }

        // The decision cannot be recorded, and so the review cannot be ratified, while PAED-003 is open (T165 fixes the
        // agenda when the decision is recorded).
        var record = () => RecordDecisionAsync(db, review.Id);
        (await record.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Contain("PAED-003 must be decided at this sitting");
        (await AgendaAsync(db, review.Id)).RatifyBlockedReason.Should().StartWith("PAED-003 must be decided at this sitting");
    }

    // ---- Reading ----------------------------------------------------------------------------------------------------

    [Fact]
    public async Task AnotherInstitutionsAdmin_CannotReadTheAgenda_AndTheTraineeReadsItOnlyOnceRatified()
    {
        await using var db = await SeededDbAsync();
        var review = await ScheduleAsync(db, GeneralPanel, Trainee, 2026, 1);
        await StartWithEvidenceAsync(db, review.Id);

        var fromB = () => AgendaAsync(db, review.Id, TestPrincipals.InstitutionalAdmin(InstitutionB));
        await fromB.Should().ThrowAsync<UnauthorizedAccessException>();
        var traineeTooEarly = () => AgendaAsync(db, review.Id, TestPrincipals.Trainee(Trainee, InstitutionA));
        await traineeTooEarly.Should().ThrowAsync<UnauthorizedAccessException>();

        await DeferAllOutstandingAsync(db, review.Id);
        await RecordDecisionAsync(db, review.Id);
        await RatifyAsync(db, review.Id);

        var seen = await AgendaAsync(db, review.Id, TestPrincipals.Trainee(Trainee, InstitutionA));
        Line(seen, "PAED-001").DeferralReason.Should().Be("Not decided at this sitting.", "O6: the trainee sees why, after ratify");
        (await AgendaAsync(db, review.Id, TestPrincipals.InstitutionalAdmin(InstitutionA))).Lines.Should().HaveCount(6);
    }

    // ---- The period and "missed" ------------------------------------------------------------------------------------

    [Fact]
    public async Task AReviewScheduledInTheYearsLastMonth_SitsForSemester2_AndItsWindowRunsToTheYearsEnd()
    {
        var december = new DateOnly(2026, 12, 5);

        var current = CommitteeReviewPeriods.Current(december);
        current.Key.Should().Be("2026-2");
        current.WindowFrom.Should().Be(
            new DateOnly(2026, 1, 1), "the six annual EPAs a semester-2 sitting closes are judged on the whole year");
        current.WindowTo.Should().Be(new DateOnly(2026, 12, 31), "December folds into semester 2 (D13)");
        current.Label.Should().Be("2026 S2 · 1 Jul to 31 Dec 2026", "the label names the period, not the window");
        var firstSemester = CommitteeReviewPeriods.Around(december).Single(period => period.Key == "2026-1");
        (firstSemester.WindowFrom, firstSemester.WindowTo).Should().Be((new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30)));
        CommitteeReviewPeriods.Around(december).Select(period => period.Key)
            .Should().Equal("2025-1", "2025-2", "2026-1", "2026-2", "2027-1", "2027-2");

        await using var db = await SeededDbAsync(withNeonatalPanel: true);
        var review = await ScheduleAsync(db, GeneralPanel, Trainee, current.AcademicYear, current.Semester);

        var inDecember = await AgendaAsync(db, review.Id, today: new DateOnly(2026, 12, 20));
        Line(inDecember, "PAED-001").WindowLabel.Should().Be("2026 S2");
        inDecember.RoutedElsewhere.Should().OnlyContain(line => line.Status == CommitteeAgendaElsewhereStatus.NotYetDecided,
            "semester 2's window runs to 31 December, so nothing is missed in December");

        var inJanuary = await AgendaAsync(db, review.Id, today: new DateOnly(2027, 1, 4));
        inJanuary.RoutedElsewhere.Should().OnlyContain(line => line.Status == CommitteeAgendaElsewhereStatus.Missed);
    }

    [Fact]
    public void Missed_IsComputedFromTheWindow_AndNeverSaidOfAnOptionalDecision()
    {
        var end = new DateOnly(2026, 6, 30);
        var after = new DateOnly(2026, 7, 1);
        AgendaLineStanding[] none = [];

        CommitteeAgendaStatus.Elsewhere(none, [], mayBeMissed: true, end, end).Should().Be(CommitteeAgendaElsewhereStatus.NotYetDecided);
        CommitteeAgendaStatus.Elsewhere(none, [], mayBeMissed: true, end, after).Should().Be(CommitteeAgendaElsewhereStatus.Missed);
        CommitteeAgendaStatus.Elsewhere(none, [], mayBeMissed: false, end, after)
            .Should().Be(CommitteeAgendaElsewhereStatus.NotYetDecided, "opportunistic and partial-period decisions are never missed");

        CommitteeAgendaStatus.Elsewhere([Standing(CommitteeAgendaLineState.Deferred, CommitteeReviewState.Ratified, 1)], [], true, end, after)
            .Should().Be(CommitteeAgendaElsewhereStatus.Deferred);
        CommitteeAgendaStatus.Elsewhere([Standing(CommitteeAgendaLineState.Due, CommitteeReviewState.InProgress, 1)], [], true, end, after)
            .Should().Be(CommitteeAgendaElsewhereStatus.OnAgenda, "a sitting still open has it");
        CommitteeAgendaStatus.Elsewhere(
                [
                    Standing(CommitteeAgendaLineState.Deferred, CommitteeReviewState.Ratified, 1),
                    Standing(CommitteeAgendaLineState.Decided, CommitteeReviewState.Ratified, 2, starDecides: true)
                ],
                [],
                true, end, after)
            .Should().Be(CommitteeAgendaElsewhereStatus.Decided);
    }

    [Fact]
    public void ADeferral_StandsOnlyWhileNoLaterSittingHasDecidedIt_WhateverBecameOfThatDecision()
    {
        // T131 slice 6 review: the latest thing a sitting did about the EPA is the word. Review 1 deferred it and review 2
        // decided it on a STAR since revoked: the deferral is overtaken. Review 1's STAR was revoked and review 2 deferred it:
        // the deferral is the latest word.
        var end = new DateOnly(2026, 6, 30);
        var after = new DateOnly(2026, 7, 1);

        CommitteeAgendaStatus.Elsewhere(
                [
                    Standing(CommitteeAgendaLineState.Deferred, CommitteeReviewState.Ratified, 1),
                    Standing(CommitteeAgendaLineState.Decided, CommitteeReviewState.Ratified, 2, starDecides: false)
                ],
                [],
                true, end, after)
            .Should().Be(CommitteeAgendaElsewhereStatus.Missed, "a later sitting decided it, and that decision was revoked");
        CommitteeAgendaStatus.Elsewhere(
                [
                    Standing(CommitteeAgendaLineState.Decided, CommitteeReviewState.Ratified, 1, starDecides: false),
                    Standing(CommitteeAgendaLineState.Deferred, CommitteeReviewState.Ratified, 2)
                ],
                [],
                true, end, after)
            .Should().Be(CommitteeAgendaElsewhereStatus.Deferred, "the sitting after the revocation deferred it");
        CommitteeAgendaStatus.Elsewhere(
                [
                    Standing(CommitteeAgendaLineState.Deferred, CommitteeReviewState.Ratified, 9, semester: 1),
                    Standing(CommitteeAgendaLineState.Decided, CommitteeReviewState.Ratified, 3, semester: 2, starDecides: false)
                ],
                [],
                true, new DateOnly(2026, 12, 31), new DateOnly(2027, 1, 4))
            .Should().Be(CommitteeAgendaElsewhereStatus.Missed, "a sitting for semester 2 sat after one for semester 1, whatever the ids");
    }

    private static AgendaLineStanding Standing(
        CommitteeAgendaLineState state, CommitteeReviewState review, int reviewId, int semester = 1, bool starDecides = false)
        => new(state, review, new CommitteeSitting(2026, semester, reviewId), starDecides);

    // ---- What the agenda does not hold ------------------------------------------------------------------------------

    [Fact]
    public void AnAgendaLine_HoldsNoUserId_SoErasureHasNothingOfItToPseudonymise()
    {
        typeof(CommitteeAgendaLine).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.PropertyType == typeof(string))
            .Select(property => property.Name)
            .Should().NotContain(name => name.EndsWith("UserId", StringComparison.Ordinal), "who deferred is the audit row's");

        File.ReadAllText(Path.Combine(SolutionRoot(), "src", "Wombat.Infrastructure", "DataRights", "ErasureExecutor.cs"))
            .Should().NotContain("CommitteeAgendaLine", "ErasureExecutor does not change for the agenda");
    }

    /// <summary>
    /// A date built from parts, a month counted or read, or a month named: each is a calendar of its own. Month names are
    /// matched whole and capitalised, as a date literal or a format would spell them.
    /// </summary>
    private const string CalendarLiteral =
        @"new DateOnly\(|AddMonths\(|\.Month\b|\b(January|February|March|April|May|June|July|August|September|October|" +
        @"November|December|Jan|Feb|Mar|Apr|Jun|Jul|Aug|Sep|Sept|Oct|Nov|Dec)\b";

    [Fact]
    public void TheCalendarGrep_CatchesEachKindOfLiteral()
    {
        new[] { "new DateOnly(2026, 7, 1)", "today.AddMonths(6)", "if (date.Month == 12)", "\"1 July\"", "\"Dec\"" }
            .Should().OnlyContain(text => Regex.IsMatch(text, CalendarLiteral));
        new[] { "AcademicPeriod.Containing(today)", "period.Start", "QuotaWindow.For(cadence, end, start)", "Mayor", "Decided" }
            .Should().NotContain(text => Regex.IsMatch(text, CalendarLiteral));
    }

    [Fact]
    public void TheCommitteeFeature_WritesNoCalendarOfItsOwn()
    {
        // The brief's grep: cadence is read against AcademicPeriod and QuotaWindow, never a second calendar.
        var folder = Path.Combine(SolutionRoot(), "src", "Wombat.Application", "Features", "CommitteeDecisions");
        var offending = Directory.EnumerateFiles(folder, "*.cs")
            .SelectMany(path => File.ReadLines(path).Select((text, index) => (Path.GetFileName(path), index + 1, text)))
            .Where(line => !line.text.TrimStart().StartsWith("//", StringComparison.Ordinal))
            .Where(line => Regex.IsMatch(line.text, CalendarLiteral))
            .Select(line => $"{line.Item1}:{line.Item2}")
            .ToArray();

        offending.Should().BeEmpty();
    }

    // ---- The requests -----------------------------------------------------------------------------------------------

    private static async Task<CommitteeReviewListItemDto> ScheduleAsync(
        ApplicationDbContext db, int panelId, string trainee, int year, int semester, bool formative = false,
        ClaimsPrincipal? principal = null)
    {
        var scheduled = await new ScheduleCommitteeReviewCommandHandler(db, FakeUserDirectory.TraineesOf(db)).Handle(
            new ScheduleCommitteeReviewCommand(
                trainee, panelId, year, semester, new DateOnly(year, 1, 1), new DateOnly(year, 12, 31), new DateOnly(2026, 7, 2),
                principal ?? TestPrincipals.Coordinator(InstitutionA), formative),
            CancellationToken.None);
        db.ChangeTracker.Clear();
        return scheduled;
    }

    private static async Task<CommitteeAgendaPreviewDto> PreviewAsync(
        ApplicationDbContext db, int panelId, string trainee, int year, int semester, ClaimsPrincipal? principal = null)
        => await new PreviewCommitteeAgendaQueryHandler(db, FakeUserDirectory.TraineesOf(db)).Handle(
            new PreviewCommitteeAgendaQuery(trainee, panelId, year, semester, principal ?? TestPrincipals.Coordinator(InstitutionA), Today),
            CancellationToken.None);

    private static async Task StartAsync(ApplicationDbContext db, int reviewId)
    {
        var chair = await ChairOfAsync(db, reviewId);
        await new StartCommitteeReviewCommandHandler(db).Handle(new StartCommitteeReviewCommand(reviewId, chair), CancellationToken.None);
        db.ChangeTracker.Clear();
    }

    /// <summary>Starts the review, then writes one snapshot line per EPA: the window held no activity to freeze.</summary>
    private static async Task StartWithEvidenceAsync(ApplicationDbContext db, int reviewId)
    {
        await StartAsync(db, reviewId);
        await AddEvidenceAsync(db, reviewId);
    }

    private static async Task AddEvidenceAsync(ApplicationDbContext db, int reviewId)
    {
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

    private static async Task<PendingEntrustmentDecisionDto> StageAsync(ApplicationDbContext db, int reviewId, int epaId)
    {
        var evidence = await db.CommitteeEvidenceItems.Where(line => line.ReviewId == reviewId && line.EpaId == epaId)
            .Select(line => line.Id).ToArrayAsync();
        var staged = await new StagePendingEntrustmentDecisionCommandHandler(db, FakeUserDirectory.PanelMembersOf(db)).Handle(
            new StagePendingEntrustmentDecisionCommand(
                reviewId, null, epaId, Level, new DateOnly(2026, 7, 2), null, "Consistent.", evidence, await ChairOfAsync(db, reviewId)),
            CancellationToken.None);
        db.ChangeTracker.Clear();
        return staged;
    }

    private static async Task RemoveAsync(ApplicationDbContext db, int reviewId, int pendingId)
    {
        await new RemovePendingEntrustmentDecisionCommandHandler(db, FakeUserDirectory.PanelMembersOf(db)).Handle(
            new RemovePendingEntrustmentDecisionCommand(reviewId, pendingId, await ChairOfAsync(db, reviewId)), CancellationToken.None);
        db.ChangeTracker.Clear();
    }

    private static async Task DeferAsync(ApplicationDbContext db, int reviewId, int lineId, string reason, ClaimsPrincipal? principal = null)
    {
        await new DeferAgendaLineCommandHandler(db, FakeUserDirectory.PanelMembersOf(db)).Handle(
            new DeferAgendaLineCommand(reviewId, lineId, reason, principal ?? await ChairOfAsync(db, reviewId)), CancellationToken.None);
        db.ChangeTracker.Clear();
    }

    private static async Task ReinstateAsync(ApplicationDbContext db, int reviewId, int lineId)
    {
        await new ReinstateAgendaLineCommandHandler(db, FakeUserDirectory.PanelMembersOf(db)).Handle(
            new ReinstateAgendaLineCommand(reviewId, lineId, await ChairOfAsync(db, reviewId)), CancellationToken.None);
        db.ChangeTracker.Clear();
    }

    /// <summary>Defers every closing line nothing is staged on, as the chair does for the EPAs the sitting will not decide.</summary>
    private static async Task DeferAllOutstandingAsync(ApplicationDbContext db, int reviewId)
    {
        foreach (var line in (await AgendaAsync(db, reviewId)).OutstandingClosingLines)
        {
            await DeferAsync(db, reviewId, line.Id, "Not decided at this sitting.");
        }
    }

    /// <summary>
    /// Records the committee's decision with the whole panel present, each an active committee member at its institution:
    /// a quorum (T165, PanelSeat). A progression review's decision records a category; the neonatal CCC's reviews are
    /// entrustment-only and record none (T131 slice 5).
    /// </summary>
    private static async Task RecordDecisionAsync(ApplicationDbContext db, int reviewId)
    {
        var panel = await db.CommitteeReviews.AsNoTracking()
            .Where(review => review.Id == reviewId)
            .Select(review => new
            {
                review.Panel.InstitutionId,
                review.ReviewType,
                Members = review.Panel.Members.Select(member => member.UserId).ToList()
            })
            .SingleAsync();

        await new RecordCommitteeDecisionCommandHandler(db, FakeUserDirectory.CommitteeMembersAt(panel.InstitutionId, [.. panel.Members])).Handle(
            new RecordCommitteeDecisionCommand(
                reviewId,
                panel.ReviewType == CommitteeReviewType.EntrustmentOnly ? null : CommitteeDecisionCategory.SatisfactoryProgress,
                "On track.", null, panel.Members, await ChairOfAsync(db, reviewId)),
            CancellationToken.None);
        db.ChangeTracker.Clear();
    }

    private static async Task RatifyAsync(ApplicationDbContext db, int reviewId)
    {
        await new RatifyCommitteeDecisionCommandHandler(db, FakeUserDirectory.PanelMembersOf(db)).Handle(
            new RatifyCommitteeDecisionCommand(reviewId, await ChairOfAsync(db, reviewId)), CancellationToken.None);
        db.ChangeTracker.Clear();
    }

    /// <summary>
    /// A binding sitting of the panel for the period that stages a STAR on each EPA named, defers every other closing line,
    /// records the decision with a quorum and ratifies it. Returns the review's id.
    /// </summary>
    private static async Task<int> DecideAsync(ApplicationDbContext db, int panelId, int year, int semester, params string[] codes)
    {
        var review = await ScheduleAsync(db, panelId, Trainee, year, semester);
        await StartWithEvidenceAsync(db, review.Id);
        foreach (var code in codes)
        {
            await StageAsync(db, review.Id, EpaIdOf(code));
        }

        await DeferAllOutstandingAsync(db, review.Id);
        await RecordDecisionAsync(db, review.Id);
        await RatifyAsync(db, review.Id);
        return review.Id;
    }

    /// <summary>
    /// Deletes every agenda line, as on dev, where reviews 2 and 3 were ratified before agendas existed (T215): their STARs
    /// stand, and no line records them.
    /// </summary>
    private static async Task ForgetAgendaLinesAsync(ApplicationDbContext db)
    {
        db.CommitteeAgendaLines.RemoveRange(await db.CommitteeAgendaLines.ToListAsync());
        await SaveAndClearAsync(db);
        (await db.CommitteeAgendaLines.CountAsync()).Should().Be(0);
    }

    /// <summary>Revokes the trainee's active STAR on the EPA.</summary>
    private static async Task RevokeActiveAsync(ApplicationDbContext db, string code)
    {
        var epaId = EpaIdOf(code);
        var star = await db.EntrustmentDecisions.SingleAsync(entity =>
            entity.TraineeUserId == Trainee && entity.EpaId == epaId && entity.Status == EntrustmentDecisionStatus.Active);
        star.Revoke("Issued in error.", "admin-user", DateTime.UtcNow);
        await SaveAndClearAsync(db);
    }

    /// <summary>The stored state of the review's line on the EPA.</summary>
    private static async Task<CommitteeAgendaLineState> LineStateAsync(ApplicationDbContext db, int reviewId, string code)
        => await db.CommitteeAgendaLines.AsNoTracking()
            .Where(line => line.ReviewId == reviewId && line.EpaCode == code)
            .Select(line => line.State)
            .SingleAsync();

    /// <summary>
    /// Where the trainee's decision on the EPA stands for 2026 S2 on the decisions-due page, as institution A's coordinator
    /// reads it (T131 slice 6).
    /// </summary>
    private static async Task<EntrustmentDecisionDueStatus> DueStatusAsync(ApplicationDbContext db, string code)
    {
        var due = await new GetEntrustmentDecisionsDueQueryHandler(db, new FakeUserDirectory((Trainee, "Ada Trainee")).WithTraineesOf(db)).Handle(
            new GetEntrustmentDecisionsDueQuery(2026, 2, null, TestPrincipals.Coordinator(InstitutionA), Today),
            CancellationToken.None);
        db.ChangeTracker.Clear();
        return (due ?? throw new InvalidOperationException("The query returned nothing."))
            .Items.Single(item => item.TraineeUserId == Trainee && item.EpaCode == code).Status;
    }

    /// <summary>The status of each of the trainee's STARs, in the order they were issued.</summary>
    private static async Task<IReadOnlyList<EntrustmentDecisionStatus>> StatusesAsync(ApplicationDbContext db)
        => await db.EntrustmentDecisions.AsNoTracking()
            .Where(star => star.TraineeUserId == Trainee)
            .OrderBy(star => star.Id)
            .Select(star => star.Status)
            .ToListAsync();

    private static async Task<CommitteeAgendaDto> AgendaAsync(
        ApplicationDbContext db, int reviewId, ClaimsPrincipal? principal = null, DateOnly? today = null)
    {
        var agenda = await new GetCommitteeAgendaQueryHandler(db).Handle(
            new GetCommitteeAgendaQuery(reviewId, principal ?? TestPrincipals.Administrator(), today ?? Today),
            CancellationToken.None);
        db.ChangeTracker.Clear();
        return agenda;
    }

    private static async Task<ClaimsPrincipal> ChairOfAsync(ApplicationDbContext db, int reviewId)
        => await db.CommitteeReviews.AsNoTracking().Where(review => review.Id == reviewId).Select(review => review.PanelId)
               .SingleOrDefaultAsync() switch
        {
            NeonatalPanel => NeonatalChair(),
            SpecialityPanel => TestPrincipals.InRole(WombatRoles.CommitteeMember, "chair-s", InstitutionA),
            PanelAtB => TestPrincipals.InRole(WombatRoles.CommitteeMember, "chair-b", InstitutionB),
            NeonatalPanelAtB => TestPrincipals.InRole(WombatRoles.CommitteeMember, "chair-bn", InstitutionB),
            _ => Chair()
        };

    private static ClaimsPrincipal Chair() => TestPrincipals.InRole(WombatRoles.CommitteeMember, "chair-a", InstitutionA);

    private static ClaimsPrincipal NeonatalChair() => TestPrincipals.InRole(WombatRoles.CommitteeMember, "chair-n", InstitutionA);

    private static CommitteeAgendaElsewhereDto Elsewhere(CommitteeAgendaDto agenda, string code)
        => agenda.RoutedElsewhere.Should().ContainSingle(line => line.EpaCode == code).Subject;

    private static CommitteeAgendaElsewhereDto Elsewhere(CommitteeAgendaPreviewDto preview, string code)
        => preview.RoutedElsewhere.Should().ContainSingle(line => line.EpaCode == code).Subject;

    /// <summary>The trainee moves institution: their preferred profile is now at <paramref name="institutionId" />.</summary>
    private static async Task MoveAsync(ApplicationDbContext db, string trainee, int institutionId)
    {
        (await db.TraineeProfiles.SingleAsync(profile => profile.UserId == trainee)).InstitutionId = institutionId;
        await SaveAndClearAsync(db);
    }

    private static CommitteeAgendaLineDto Line(CommitteeAgendaDto agenda, string code)
        => agenda.Lines.Should().ContainSingle(line => line.EpaCode == code).Subject;

    private static int LineIdOf(CommitteeAgendaDto agenda, string code) => Line(agenda, code).Id;

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
        db.Specialities.Add(new Speciality { Id = 1, CollegeId = 1, Name = "Paediatrics", IsActive = true });
        db.SubSpecialities.Add(new SubSpeciality { Id = 11, SpecialityId = 1, Name = "General Paediatrics", IsActive = true });
        db.Curricula.Add(new Curriculum { Id = CurriculumId, SubSpecialityId = 11, Name = "General Paediatrics", Version = "11.1" });
        db.DecisionBodies.Add(new DecisionBody { Key = Neonatal, Name = "Neonatal team Clinical Competency Committee" });
        db.EntrustmentScales.Add(new EntrustmentScale { Id = 1, Name = "CPSA ladder" });
        db.EntrustmentLevels.AddRange(Enumerable.Range(1, 5).Select(order =>
            new EntrustmentLevel { Id = order, ScaleId = 1, Order = order, Label = order.ToString(System.Globalization.CultureInfo.InvariantCulture) }));

        foreach (var (code, id) in Epas)
        {
            db.Epas.Add(new Epa { Id = id, SubSpecialityId = 11, Code = code, Title = $"EPA {id}", IsActive = code != "PAED-040" });
        }

        db.CurriculumItems.AddRange(
            Item(1, QuotaPeriod.Semester),
            Item(2, QuotaPeriod.Semester),
            Item(3, QuotaPeriod.AcademicYear),
            Item(4, QuotaPeriod.Semester, Neonatal),
            Item(5, QuotaPeriod.Semester, Neonatal),
            Item(8, QuotaPeriod.AcademicYear, opportunistic: true),
            Item(20, cadence: null),
            Item(30, QuotaPeriod.Semester, owningInstitutionId: InstitutionB),
            Item(40, QuotaPeriod.Semester));

        db.TraineeProfiles.AddRange(
            new TraineeProfile
            {
                Id = 1, UserId = Trainee, InstitutionId = InstitutionA, CurriculumId = CurriculumId, IsActive = true,
                ProgrammeStartDate = new DateOnly(2025, 1, 15), ExpectedCompletionDate = new DateOnly(2029, 1, 14)
            },
            new TraineeProfile
            {
                Id = 2, UserId = LateStarter, InstitutionId = InstitutionA, CurriculumId = CurriculumId, IsActive = true,
                ProgrammeStartDate = new DateOnly(2026, 3, 2), ExpectedCompletionDate = new DateOnly(2030, 3, 1)
            });

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

    private static CurriculumItem Item(
        int epaId, QuotaPeriod? cadence, string? body = null, bool opportunistic = false, int? owningInstitutionId = null)
        => new()
        {
            Id = 100 + epaId,
            CurriculumId = CurriculumId,
            EpaId = epaId,
            RequiredCount = 1,
            MinimumLevelOrder = 3,
            DecisionCadence = cadence,
            DecisionBodyKey = body,
            DecisionIsOpportunistic = opportunistic,
            OwningInstitutionId = owningInstitutionId
        };

    private static DecisionPanel Panel(
        int id, string name, int institutionId, string? body, string chair, string? member = null, int? specialityId = null)
        => new()
        {
            Id = id,
            Name = name,
            Scope = specialityId is null ? DecisionPanelScope.Institution : DecisionPanelScope.Speciality,
            SpecialityId = specialityId,
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

    private static string SolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && directory.GetFiles("Wombat.sln").Length == 0)
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not find Wombat.sln.");
    }
}
