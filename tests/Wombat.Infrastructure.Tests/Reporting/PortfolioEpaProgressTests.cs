using System.Globalization;
using System.Security.Claims;
using System.Xml.Linq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using Wombat.Application.Features.Activities.Commands.RebuildCurriculumProgress;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Features.Reporting;
using Wombat.Application.Features.Trainees;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Reporting;

namespace Wombat.Infrastructure.Tests.Reporting;

/// <summary>
/// T169: the portfolio export's summary counts an activity as complete when it is in a terminal state of its PINNED
/// workflow (D44), and a "Progress per EPA" section prints the progress page's figures: each curriculum item's target
/// and count for every period the export covers, and the entrustment trajectory's rated observations.
/// </summary>
/// <remarks>
/// The figures are asserted on the loaded data (<c>LoadPortfolioDataAsync</c>, internal for tests since T113), and the
/// words on the section's own helpers. What reaches the page is asserted on the document rendered as SVG, whose text
/// elements are what a reader reads (<c>PortfolioPdfService.ComposeDocument</c>; the PDF's bytes hold the text only in
/// compressed, font-encoded streams), and as a change in the PDF's bytes when only one thing changes. Every fixture is
/// saved and the change tracker cleared before the read, so a missing <c>Include</c> is not hidden by navigation fix-up.
/// Renders run one at a time (<see cref="QuestPdfRenderingCollection" />).
/// </remarks>
[Collection(QuestPdfRenderingCollection.Name)]
public sealed class PortfolioEpaProgressTests
{
    static PortfolioEpaProgressTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    /// <summary>08:00 UTC on 23 September 2026: semester 2 of 2026 on the South African calendar.</summary>
    private static readonly DateTimeOffset Now = new(2026, 9, 23, 8, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly Today = new(2026, 9, 23);

    private const string Trainee = "trainee-1";

    // ─── The summary ─────────────────────────────────────────────────────────

    [Fact]
    public async Task TheSummary_CountsEveryTerminalStateOfThePinnedWorkflowAsComplete()
    {
        // The verification's trio: a discussed reflective exercise, a recorded MSF row and a logged procedure each
        // printed "0 completed" when only the literal state `completed` counted. A draft, a cancelled exercise and a
        // declined Mini-CEX are not finished and stay out: every CPSA seed makes those non-terminal dead ends.
        await using var db = SeededDb();

        var data = await LoadAsync(db);

        data.TypeSummaries.Should().Equal(
            new PortfolioTypeSummary("mini_cex_cpsa", Total: 4, Complete: 3),
            new PortfolioTypeSummary("msf_cpsa", Total: 1, Complete: 1),
            new PortfolioTypeSummary("procedure_log", Total: 1, Complete: 1),
            new PortfolioTypeSummary("reflective_exercise_cpsa", Total: 3, Complete: 1));
    }

    [Fact]
    public async Task TheSummary_ReadsThePinnedVersionsWorkflow_NotTheTypesCurrentOne()
    {
        // A type republished with a different finishing state: an activity pinned to v1 finished where v1 finishes,
        // and "done" is not a finish under v2. A type with no workflow finishes in `completed`.
        await using var db = SeededDb();
        var republished = AddType(db, 20, "ward_review", Schema("ward_review"), FinishesInSignedOff, version: 2);
        AddVersion(db, republished, 1, FinishesInDone);
        AddVersion(db, republished, 2, FinishesInSignedOff);
        AddActivity(db, 201, republished, "done", schemaVersion: 1);
        AddActivity(db, 202, republished, "done", schemaVersion: 2);

        var unworkflowed = AddType(db, 21, "legacy_log", Schema("legacy_log"), workflowJson: null, version: 1);
        AddVersion(db, unworkflowed, 1, workflowJson: string.Empty);
        AddActivity(db, 203, unworkflowed, "completed");
        AddActivity(db, 204, unworkflowed, "open");
        Save(db);

        var data = await LoadAsync(db);

        data.TypeSummaries.Should().Contain(new PortfolioTypeSummary("ward_review", Total: 2, Complete: 1));
        data.TypeSummaries.Should().Contain(new PortfolioTypeSummary("legacy_log", Total: 2, Complete: 1));
    }

    // ─── The per-EPA section: targets ────────────────────────────────────────

    [Fact]
    public async Task TheEpaSection_MatchesTheProgressPage_ForTheSameTraineeAndDay()
    {
        await using var db = SeededDb();

        var progress = (await LoadAsync(db)).EpaProgress;
        var page = await new GetCurriculumProgressForTraineeQueryHandler(db).Handle(
            new GetCurriculumProgressForTraineeQuery(Trainee, SubjectPrincipal(), Today), CancellationToken.None);

        progress.AsOf.Should().Be(Today, "an open-ended export is read as of today");
        page.Should().NotBeNull();
        progress.Targets!.TraineeStage.Should().Be(page!.TraineeStage);

        // Institution 2's local item shares the national curriculum row and is no target of this trainee's. PAED-009 is
        // no item at all, and is listed after the curriculum for its rated evidence.
        progress.Rows.Select(row => row.EpaCode).Should().Equal("PAED-001", "PAED-002", "PAED-009");
        progress.Rows[2].Item.Should().BeNull();
        progress.Rows[2].NoTarget.Should().Be(PortfolioNoTargetReason.NotInCurriculum);
        progress.Programme.State.Should().Be(PortfolioProgrammeState.Active);

        foreach (var pageItem in page.Items)
        {
            var row = progress.Rows.Single(entry => entry.EpaId == pageItem.EpaId);
            row.Item!.Current.Should().Be(pageItem.Current);
            row.Item.Previous.Should().Be(pageItem.Previous);
            row.Item.EffectiveMinimumLevelLabel.Should().Be(pageItem.EffectiveMinimumLevelLabel);
            row.Periods[0].Should().Be(pageItem.Current, "the section opens with the page's current period");
            row.Periods[1].Should().Be(pageItem.Previous, "and then the page's previous one");
        }
    }

    [Fact]
    public async Task TheEpaSection_ListsEveryPeriodTheExportCovers()
    {
        await using var db = SeededDb();

        // The whole portfolio: back to the programme's first semester, 2025.
        var whole = (await LoadAsync(db)).EpaProgress;
        var paed001 = whole.Rows.Single(row => row.EpaCode == "PAED-001");
        paed001.Periods.Select(period => period.Name).Should().Equal(
            "Semester 2, 2026", "Semester 1, 2026", "Semester 2, 2025", "Semester 1, 2025");
        paed001.Periods.Select(period => period.Count).Should().Equal(1, 2, 3, 1);
        paed001.Periods.Select(period => period.IsMet).Should().Equal(false, false, true, false);
        whole.Rows.Single(row => row.EpaCode == "PAED-002").Periods.Select(period => (period.Name, period.Count))
            .Should().Equal(("2026 academic year", 0), ("2025 academic year", 1));

        // An annual export: that year's two semesters and that year, nothing before.
        var annual = (await LoadAsync(db, new DateOnly(2026, 1, 1), new DateOnly(2026, 11, 30))).EpaProgress;
        annual.Rows.Single(row => row.EpaCode == "PAED-001").Periods.Select(period => period.Name)
            .Should().Equal("Semester 2, 2026", "Semester 1, 2026");
        annual.Rows.Single(row => row.EpaCode == "PAED-002").Periods.Select(period => period.Name)
            .Should().Equal("2026 academic year");
    }

    [Fact]
    public async Task TheEpaSection_IsReadOnTheExportsLastDay_UnlessThatIsInTheFuture()
    {
        await using var db = SeededDb();

        var lastYear = (await LoadAsync(db, toDate: new DateOnly(2025, 12, 31))).EpaProgress;
        lastYear.AsOf.Should().Be(new DateOnly(2025, 12, 31));
        var paed001 = lastYear.Rows.Single(row => row.EpaCode == "PAED-001");
        paed001.Item!.Current.Name.Should().Be("Semester 2, 2025");
        paed001.Periods.Select(period => period.Name).Should().Equal("Semester 2, 2025", "Semester 1, 2025");

        // A period that has not happened has nothing to report, so a future end reads today.
        (await LoadAsync(db, toDate: new DateOnly(2027, 6, 30))).EpaProgress.AsOf.Should().Be(Today);
    }

    [Fact]
    public async Task TheEpaSection_ForACompletedTrainee_ReadsTheCoversProgramme_AsOnTheCompletionDay()
    {
        // The graduation export (T169 review). The progress page reads active profiles only, and completing a programme
        // deactivates the profile, so the section printed "no active training programme" under a cover naming it. It
        // reads the cover's programme instead, as on the day it was completed: no period after that is listed.
        await using var db = SeededDb();
        db.TraineeProfiles.Single().Complete(new DateOnly(2026, 5, 15), Today);
        Save(db);

        var progress = (await LoadAsync(db)).EpaProgress;

        progress.Programme.Should().Be(new PortfolioProgramme(PortfolioProgrammeState.Completed, new DateOnly(2026, 5, 15)));
        progress.AsOf.Should().Be(new DateOnly(2026, 5, 15));
        progress.Targets.Should().NotBeNull();
        progress.Rows.Select(row => (row.EpaCode, row.NoTarget)).Should().Equal(
            ("PAED-001", (PortfolioNoTargetReason?)null),
            ("PAED-002", null),
            ("PAED-009", PortfolioNoTargetReason.NotInCurriculum));
        progress.Rows[0].Periods.Select(period => period.Name).Should().Equal(
            "Semester 1, 2026", "Semester 2, 2025", "Semester 1, 2025");
        EpaProgressSectionComponent.Introduction(progress).Should().StartWith(
            "The trainee completed the programme on 15 May 2026. Targets as on 15 May 2026 (training year 2).");

        // Read on the export's last day when that is earlier still.
        (await LoadAsync(db, toDate: new DateOnly(2025, 12, 31))).EpaProgress.AsOf.Should().Be(new DateOnly(2025, 12, 31));
    }

    [Fact]
    public async Task TheEpaSection_ForAGraduateWhoseLastPeriodWasCutShort_PrintsItAsExempt_NotShort()
    {
        // T209, D49. Completed on 15 May 2026, before June, the last month of semester 1: that semester and the 2026
        // academic year hold no target. Before T209 they printed as "2 of 3, 1 short" and "0 of 1, 1 short".
        await using var db = SeededDb();
        db.TraineeProfiles.Single().Complete(new DateOnly(2026, 5, 15), Today);
        Save(db);

        var data = await LoadAsync(db);
        var progress = data.EpaProgress;

        var semester = progress.Rows.Single(row => row.EpaCode == "PAED-001");
        semester.Periods.Select(period => (period.Name, period.Status)).Should().Equal(
            ("Semester 1, 2026", QuotaWindowStatus.ExemptProgrammeEnded),
            ("Semester 2, 2025", QuotaWindowStatus.Counting),
            ("Semester 1, 2025", QuotaWindowStatus.Counting));
        semester.Periods.Select(period => EpaProgressSectionComponent.PeriodLine(period, progress.Today, progress.Programme.IsActive))
            .Should().Equal(
                "no target (the programme ended part-way through), 2 recorded",
                "3 of 3, met; 3 at the minimum level when observed",
                "1 of 3, 2 short; 1 at the minimum level when observed");

        var year = progress.Rows.Single(row => row.EpaCode == "PAED-002");
        year.Periods.Select(period => EpaProgressSectionComponent.PeriodLine(period, progress.Today, progress.Programme.IsActive))
            .Should().Equal(
                "no target (the programme ended part-way through), 0 recorded",
                "1 of 1, met; 1 at the minimum level when observed");

        EpaProgressSectionComponent.Introduction(progress).Should().StartWith(
            "The trainee completed the programme on 15 May 2026. Targets as on 15 May 2026 (training year 2). " +
            "A period the programme ended in before that period's last month has no target. " +
            "No period that began after the programme ended is listed: it is outside the programme.");

        RenderedText(data).Should().ContainInConsecutiveOrder(
            "PAED-001", "Acute admission",
            "Target:", "3 per semester (6 a year), minimum 3a in training year 2",
            "Semester 1, 2026:", "no target (the programme ended part-way through), 2 recorded");
    }

    [Fact]
    public async Task TheEpaSection_ForAGraduateWhoFinishedInTheLastMonth_HoldsThatPeriodToItsTarget()
    {
        // D49: an end in June leaves semester 1 whole. It is short, and says so.
        await using var db = SeededDb();
        db.TraineeProfiles.Single().Complete(new DateOnly(2026, 6, 1), Today);
        Save(db);

        var progress = (await LoadAsync(db)).EpaProgress;

        var last = progress.Rows.Single(row => row.EpaCode == "PAED-001").Periods[0];
        last.Status.Should().Be(QuotaWindowStatus.Counting);
        EpaProgressSectionComponent.PeriodLine(last, progress.Today, progress.Programme.IsActive)
            .Should().Be("2 of 3, 1 short; 2 at the minimum level when observed");
    }

    [Fact]
    public async Task TheEpaSection_ForADeactivatedTrainee_IsReadAsOnTheirLastDay_AndListsNoPeriodAfterIt()
    {
        // T209. Deactivated on 20 August 2026, in semester 2's first month. Before T209 the day was not recorded, so the
        // section read today and listed the periods after the end as if the trainee were still held to them. It is now
        // read as on the last day, like a graduate's: semester 2 is exempt (D49), and nothing after it is listed.
        await using var db = SeededDb();
        db.TraineeProfiles.Single().Deactivate(new DateOnly(2026, 8, 20), Today);
        Save(db);

        var progress = (await LoadAsync(db)).EpaProgress;

        progress.Programme.Should().Be(new PortfolioProgramme(PortfolioProgrammeState.Inactive, new DateOnly(2026, 8, 20)));
        progress.AsOf.Should().Be(new DateOnly(2026, 8, 20));
        var semester = progress.Rows.Single(row => row.EpaCode == "PAED-001");
        semester.Periods.Select(period => (period.Name, period.Status)).Should().Equal(
            ("Semester 2, 2026", QuotaWindowStatus.ExemptProgrammeEnded),
            ("Semester 1, 2026", QuotaWindowStatus.Counting),
            ("Semester 2, 2025", QuotaWindowStatus.Counting),
            ("Semester 1, 2025", QuotaWindowStatus.Counting));
        EpaProgressSectionComponent.PeriodLine(semester.Periods[0], progress.Today, progress.Programme.IsActive)
            .Should().Be("no target (the programme ended part-way through), 1 recorded");
        EpaProgressSectionComponent.Introduction(progress).Should().StartWith(
            "The trainee's programme was ended without being completed, on 20 August 2026. Targets as on 20 August 2026 " +
            "(training year 2). A period the programme ended in before that period's last month has no target. " +
            "No period that began after the programme ended is listed: it is outside the programme.");

        // A later export window still reads the last day, never a period after it.
        (await LoadAsync(db, toDate: new DateOnly(2027, 3, 1))).EpaProgress.AsOf.Should().Be(new DateOnly(2026, 8, 20));
    }

    [Fact]
    public async Task TheEpaSection_ForALastDayRecordedAfterTheFact_CountsOnlyTheEncountersUpToIt()
    {
        // T281, as the lifecycle check found it (T252): the last day is recorded weeks after it, by which time encounters
        // after it had credited semester 2. They credit nothing on the ended programme, so recording the day takes their
        // credit back, and the export counts only what was observed up to it. The tallies here are the Mini-CEXes' own
        // credit, not the fixture's hand-written rows.
        await using var db = SeededDb();
        db.CurriculumItemProgresses.RemoveRange(db.CurriculumItemProgresses);
        var miniCex = db.ActivityTypes.Single(type => type.Key == "mini_cex_cpsa");
        AddCompletedMiniCex(db, 120, miniCex, new DateOnly(2026, 8, 5));
        AddCompletedMiniCex(db, 121, miniCex, new DateOnly(2026, 9, 10));
        Save(db);

        await new RebuildCurriculumProgressCommandHandler(db, new CreditApplier(db))
            .Handle(new RebuildCurriculumProgressCommand(Administrator(), Trainee), CancellationToken.None);
        db.ChangeTracker.Clear();
        (await LoadAsync(db)).EpaProgress.Rows.Single(row => row.EpaCode == "PAED-001").Periods[0].Count
            .Should().Be(3, "guard: while the programme runs, the Mini-CEXes of 5 August, 12 August and 10 September count");

        await new DeactivateTraineeProfileCommandHandler(db, new CreditApplier(db), new TraineeCreditLock(db), new FixedClock(Now))
            .Handle(new DeactivateTraineeProfileCommand(1, new DateOnly(2026, 8, 8), Administrator()), CancellationToken.None);
        db.ChangeTracker.Clear();

        var progress = (await LoadAsync(db)).EpaProgress;
        progress.AsOf.Should().Be(new DateOnly(2026, 8, 8));
        var semester = progress.Rows.Single(row => row.EpaCode == "PAED-001").Periods[0];
        semester.Name.Should().Be("Semester 2, 2026");
        semester.Count.Should().Be(1, "only the encounter of 5 August is inside the programme");
        EpaProgressSectionComponent.PeriodLine(semester, progress.Today, progress.Programme.IsActive)
            .Should().Be("no target (the programme ended part-way through), 1 recorded");
    }

    [Fact]
    public async Task TheEpaSection_ForATraineeDeactivatedBeforeTheDayWasRecorded_KeepsTheTargets_AndOwesNothingMoreInARunningPeriod()
    {
        // Deactivated before T209 recorded the day (IsActive cleared, no DeactivatedOn): the targets are read as on today
        // and the introduction says periods after the programme ended are listed too. A period still running is a count
        // so far, with nothing "more by" a date the trainee is no longer held to.
        await using var db = SeededDb();
        db.TraineeProfiles.Single().IsActive = false;
        Save(db);

        var progress = (await LoadAsync(db)).EpaProgress;

        progress.Programme.State.Should().Be(PortfolioProgrammeState.Inactive);
        progress.AsOf.Should().Be(Today);
        progress.Rows.Select(row => row.EpaCode).Should().Equal("PAED-001", "PAED-002", "PAED-009");
        EpaProgressSectionComponent.Introduction(progress).Should().StartWith(
            "The trainee's programme is inactive: it was ended without being completed");
        EpaProgressSectionComponent.PeriodLine(progress.Rows[0].Periods[0], progress.Today, progress.Programme.IsActive)
            .Should().Be("1 of 3 so far; 1 at the minimum level when observed");
    }

    [Fact]
    public async Task TheEpaSection_WithNoProgramme_HasNoTargets_AndKeepsTheRatedEvidence()
    {
        await using var db = SeededDb();
        db.TraineeProfiles.Remove(db.TraineeProfiles.Single());
        Save(db);

        var progress = (await LoadAsync(db)).EpaProgress;

        progress.Programme.State.Should().Be(PortfolioProgrammeState.None);
        progress.Targets.Should().BeNull();
        progress.Rows.Select(row => row.EpaCode).Should().Equal("PAED-001", "PAED-009");
        progress.Rows.Should().OnlyContain(row =>
            row.Item == null && row.NoTarget == PortfolioNoTargetReason.NoProgramme && row.Ratings.Observations > 0);
        EpaProgressSectionComponent.Introduction(progress).Should().StartWith("The trainee has no training programme");
    }

    [Fact]
    public async Task AnEpaDeactivatedAfterItWasRated_SaysSo_RatherThanThatItIsNotInTheCurriculum()
    {
        // T158: a deactivated EPA's item is no target, so the progress reader leaves it out, and its rated evidence gets a
        // row after the curriculum. The row said "not in the trainee's curriculum", which is false. Another institution's
        // local item is still not this trainee's, deactivated or not.
        await using var db = SeededDb();
        var miniCex = db.ActivityTypes.Single(type => type.Key == "mini_cex_cpsa");
        AddRated(db, 110, miniCex, "completed", epaId: 2, "assessor-a", rating: 3, new DateOnly(2026, 7, 1));
        AddRated(db, 111, miniCex, "completed", epaId: 3, "assessor-a", rating: 3, new DateOnly(2026, 7, 2));
        db.Epas.Single(epa => epa.Id == 2).Deactivate(DateTime.MinValue);
        db.Epas.Single(epa => epa.Id == 3).Deactivate(DateTime.MinValue);
        Save(db);

        var progress = (await LoadAsync(db)).EpaProgress;

        progress.Rows.Select(row => (row.EpaCode, row.NoTarget)).Should().Equal(
            ("PAED-001", (PortfolioNoTargetReason?)null),
            ("LOCAL-2", PortfolioNoTargetReason.NotInCurriculum),
            ("PAED-002", PortfolioNoTargetReason.EpaDeactivated),
            ("PAED-009", PortfolioNoTargetReason.NotInCurriculum));
    }

    [Fact]
    public async Task TheEpaSection_ReadsTheProgrammeTheCoverNames()
    {
        // Two active profiles (T169 review). The cover takes TraineeScopeResolver's preferred profile, the highest id; the
        // progress reader on its own takes the latest programme start. They disagreed here, so the cover named one
        // programme while the section read the other's curriculum. The section reads the cover's.
        await using var db = SeededDb();
        db.Curricula.Add(new Curriculum
        {
            Id = 2, SubSpecialityId = 1, Name = "Older programme", Version = "10.0",
            EffectiveFrom = new DateOnly(2020, 1, 1), IsActive = true
        });
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 20, CurriculumId = 2, EpaId = 9, RequiredCount = 2, QuotaPeriod = QuotaPeriod.Semester,
            MinimumLevelOrder = 3, WindowMonths = 36, ScaleId = PinnedScaleId
        });
        db.TraineeProfiles.Add(new TraineeProfile
        {
            Id = 2, UserId = Trainee, InstitutionId = 1, CurriculumId = 2,
            ProgrammeStartDate = new DateOnly(2024, 1, 1), ExpectedCompletionDate = new DateOnly(2028, 1, 1),
            IsActive = true
        });
        Save(db);

        var data = await LoadAsync(db);

        data.ProgrammeName.Should().Be("Older programme");
        data.EpaProgress.Targets!.ProgrammeStartDate.Should().Be(new DateOnly(2024, 1, 1));
        data.EpaProgress.Rows.Where(row => row.Item is not null).Select(row => row.EpaCode).Should().Equal("PAED-009");
    }

    [Fact]
    public async Task AnExportThatStartsAfterToday_ListsNoPeriod()
    {
        // An open-ended export from 1 January 2027, read on 23 September 2026, covers no period yet (T169 review). It
        // listed semester 2 of 2026.
        await using var db = SeededDb();

        var progress = (await LoadAsync(db, fromDate: new DateOnly(2027, 1, 1))).EpaProgress;

        progress.AsOf.Should().Be(Today);
        var targeted = progress.Rows.Where(row => row.Item is not null).ToList();
        targeted.Should().HaveCount(2).And.OnlyContain(row => row.Periods.Count == 0);
        EpaProgressSectionComponent.DetailLines(targeted[0], progress).Should().Equal(
            ("Target", "3 per semester (6 a year), minimum 3a in training year 2"),
            ("Periods", "none within this export's dates"),
            ("Rated observations", "none in this export"));
    }

    [Fact]
    public async Task ARowsLines_ReadAsTheCommitteeReadsThem()
    {
        // The whole portfolio, read on 23 September 2026: semester 2 of 2026 is still running, so it is a count so far
        // and what is still to do; every earlier semester is closed, so it is met or short.
        await using var db = SeededDb();
        var progress = (await LoadAsync(db)).EpaProgress;

        EpaProgressSectionComponent.DetailLines(progress.Rows.Single(row => row.EpaCode == "PAED-001"), progress)
            .Should().Equal(
                ("Target", "3 per semester (6 a year), minimum 3a in training year 2"),
                ("Semester 2, 2026", "1 of 3 so far; 2 more by 30 November 2026; 1 at the minimum level when observed"),
                ("Semester 1, 2026", "2 of 3, 1 short; 2 at the minimum level when observed"),
                ("Semester 2, 2025", "3 of 3, met; 3 at the minimum level when observed"),
                ("Semester 1, 2025", "1 of 3, 2 short; 1 at the minimum level when observed"),
                ("Rated observations", "2 from 2 assessors; latest 3a, encounter 2026-08-12"));

        // No curriculum item, so no pinned ladder: the rating is the bare ordinal, as the progress page's chart draws it.
        EpaProgressSectionComponent.DetailLines(progress.Rows.Single(row => row.EpaCode == "PAED-009"), progress)
            .Should().Equal(
                ("Target", "none: this EPA is not in the trainee's curriculum"),
                ("Rated observations", "1 from 1 assessor; latest 3, encounter 2026-05-05"));
    }

    // ─── The per-EPA section: rated observations ─────────────────────────────

    [Fact]
    public async Task TheEpaSection_CountsTheTrajectorysRatedObservations()
    {
        await using var db = SeededDb();

        var progress = (await LoadAsync(db)).EpaProgress;
        var trajectories = await new GetEpaTrajectoryForTraineeQueryHandler(db).Handle(
            new GetEpaTrajectoryForTraineeQuery(Trainee, SubjectPrincipal()), CancellationToken.None);

        // Two completed Mini-CEX on PAED-001 by two assessors. The declined one is not an observation (D44), and the
        // recorded MSF names no assessor (D36), so neither is drawn or counted.
        var paed001 = progress.Rows.Single(row => row.EpaCode == "PAED-001").Ratings;
        paed001.Observations.Should().Be(2);
        paed001.DistinctAssessors.Should().Be(2);
        paed001.LatestLabel.Should().Be("3a", "the latest is 12 August, rated at ordinal 3, rung 3a on the pinned ladder");
        paed001.LatestObservedOn.Should().Be("2026-08-12");
        paed001.LatestOffLadder.Should().BeFalse();

        progress.Rows.Single(row => row.EpaCode == "PAED-002").Ratings.Should().Be(PortfolioEpaRatings.None);
        progress.Rows.Single(row => row.EpaCode == "PAED-009").Ratings.Observations.Should().Be(1);

        // The same figures the progress page's chart draws.
        foreach (var trajectory in trajectories)
        {
            var ratings = progress.Rows.Single(row => row.EpaId == trajectory.EpaId).Ratings;
            ratings.Observations.Should().Be(trajectory.Points.Count);
            ratings.LatestLabel.Should().Be(trajectory.Points[^1].RatingLabel);
        }
    }

    [Fact]
    public async Task TheEpaSection_CountsOnlyTheRatedObservationsInsideTheExportsDates()
    {
        await using var db = SeededDb();

        var paed001 = (await LoadAsync(db, new DateOnly(2026, 6, 1), new DateOnly(2026, 12, 31))).EpaProgress
            .Rows.Single(row => row.EpaCode == "PAED-001").Ratings;

        paed001.Observations.Should().Be(1, "the March observation is before the export's first day");
        paed001.DistinctAssessors.Should().Be(1);
    }

    [Fact]
    public async Task TheLatestRatingsDate_SaysWhenNobodyStatedIt()
    {
        // T137's words, T161's rule: a date nobody stated is only the filing day.
        await using var db = SeededDb();
        db.Activities.Single(activity => activity.Id == 101).ObservedOnSource = ObservationDateSource.CreatedOn;
        Save(db);

        var paed001 = (await LoadAsync(db)).EpaProgress.Rows.Single(row => row.EpaCode == "PAED-001").Ratings;

        paed001.LatestObservedOn.Should().Be("not recorded (created 2026-08-12)");
    }

    // ─── The printed PDF ─────────────────────────────────────────────────────

    [Fact]
    public async Task ThePdf_WithTheEpaSection_IsByteForByteDeterministic()
    {
        await using var db = SeededDb();
        var service = Service(db);

        var first = await service.GenerateAsync(Request(), CancellationToken.None);
        var second = await service.GenerateAsync(Request(), CancellationToken.None);

        first.PdfBytes.Should().NotBeEmpty();
        second.PdfBytes.Should().Equal(first.PdfBytes);
    }

    [Fact]
    public async Task ThePdf_PrintsTheEpaSection()
    {
        // Only a progress figure changes, and nothing but the per-EPA section prints progress figures.
        await using var db = SeededDb();
        var service = Service(db);
        var before = await service.GenerateAsync(Request(), CancellationToken.None);

        db.CurriculumItemProgresses.Single(row => row.CurriculumItemId == 1 && row.AcademicYear == 2025 && row.Semester == 1)
            .CountsSoFar = 2;
        Save(db);
        var after = await service.GenerateAsync(Request(), CancellationToken.None);

        after.ContentHash.Should().NotBe(before.ContentHash);
    }

    [Fact]
    public async Task ThePdf_MarksAnUndatedActivityLine()
    {
        // The logged procedure is unrated, so its date appears on its activity line and nowhere else.
        await using var db = SeededDb();
        var service = Service(db);
        var dated = await service.GenerateAsync(Request(), CancellationToken.None);

        db.Activities.Single(activity => activity.Id == 109).ObservedOnSource = ObservationDateSource.CreatedOn;
        Save(db);
        var undated = await service.GenerateAsync(Request(), CancellationToken.None);

        undated.ContentHash.Should().NotBe(dated.ContentHash);
    }

    [Fact]
    public async Task ThePdf_PrintsTheSummarysCompleteCount()
    {
        // Only the summary's complete count changes, from one finished reflective exercise to two: the pinned version is
        // rewritten in place so that `cancelled` is terminal too. Reflective exercises are unrated, so neither the
        // trajectory nor the per-EPA section reads them, and each activity line prints its state, which is unchanged.
        // Both counts are above zero, so the figure is the only difference: not its colour.
        await using var db = SeededDb();
        var service = Service(db);
        var before = await service.GenerateAsync(Request(), CancellationToken.None);

        var pinned = db.ActivityTypeVersions.Single(version => version.ActivityTypeId == 12 && version.Version == 1);
        pinned.WorkflowJson = pinned.WorkflowJson.Replace(
            "\"label\": \"Cancelled\"", "\"label\": \"Cancelled\", \"terminal\": true", StringComparison.Ordinal);
        Save(db);
        (await LoadAsync(db)).TypeSummaries.Should().Contain(
            new PortfolioTypeSummary("reflective_exercise_cpsa", Total: 3, Complete: 2));
        var after = await service.GenerateAsync(Request(), CancellationToken.None);

        after.ContentHash.Should().NotBe(before.ContentHash);
    }

    [Fact]
    public async Task ThePrintedSummary_ShowsTheDiscussedRecordedAndLoggedEvidenceAsComplete()
    {
        // The verification's trio, as printed: each type's line reads "N total," then "M complete" (T169 review: the
        // count was asserted on the data only, and a constant "0 complete" passed every test).
        await using var db = SeededDb();

        var text = RenderedText(await LoadAsync(db));

        text.Should().ContainInConsecutiveOrder(
            "mini_cex_cpsa: 4 total,", "3 complete",
            "msf_cpsa: 1 total,", "1 complete",
            "procedure_log: 1 total,", "1 complete",
            "reflective_exercise_cpsa: 3 total,", "1 complete");
    }

    [Fact]
    public async Task ThePrintedSection_ReadsAsItsLines()
    {
        // What a committee reads on the page: the section's title, each row's EPA, and its labelled lines in order.
        await using var db = SeededDb();

        var text = RenderedText(await LoadAsync(db));

        text.Should().Contain("Progress per EPA");
        text.Should().ContainInConsecutiveOrder(
            "PAED-001", "Acute admission",
            "Target:", "3 per semester (6 a year), minimum 3a in training year 2",
            "Semester 2, 2026:", "1 of 3 so far; 2 more by 30 November 2026; 1 at the minimum level when observed",
            "Semester 1, 2026:", "2 of 3, 1 short; 2 at the minimum level when observed");
        text.Should().ContainInConsecutiveOrder(
            "PAED-009", "Not in this curriculum",
            "Target:", "none: this EPA is not in the trainee's curriculum",
            "Rated observations:", "1 from 1 assessor; latest 3, encounter 2026-05-05");
    }

    // ─── The section's wording ───────────────────────────────────────────────

    private static readonly DateOnly ProgrammeStart = new(2025, 1, 1);

    [Theory]
    // A closed period is met or short, as the progress page's previous period is.
    [InlineData("2026-01-01", "2026-09-23", 2, 3, 2, true, "2 of 3, 1 short; 2 at the minimum level when observed")]
    [InlineData("2026-01-01", "2026-09-23", 3, 3, 1, true, "3 of 3, met; 1 at the minimum level when observed")]
    [InlineData("2026-01-01", "2026-07-01", 2, 3, 2, true, "2 of 3, 1 short; 2 at the minimum level when observed")]
    // A running period is a count so far, and what is still to do by the College's last day. Never "short".
    [InlineData("2026-07-01", "2026-09-23", 1, 3, 0, true, "1 of 3 so far; 2 more by 30 November 2026; 0 at the minimum level when observed")]
    [InlineData("2026-07-01", "2026-09-23", 0, 3, 0, true, "0 of 3 so far; 3 more by 30 November 2026")]
    [InlineData("2026-01-01", "2026-06-30", 2, 3, 2, true, "2 of 3 so far; 1 more by 30 June 2026; 2 at the minimum level when observed")]
    [InlineData("2026-07-01", "2026-09-23", 3, 3, 3, true, "3 of 3 so far, met; 3 at the minimum level when observed")]
    // December: the College's year is over, but December encounters still count (D40), up to and including the 31st.
    [InlineData("2026-07-01", "2026-12-10", 1, 3, 1, true, "1 of 3 so far; 2 more, and encounters in December still count towards Semester 2, 2026; 1 at the minimum level when observed")]
    [InlineData("2026-07-01", "2026-12-31", 1, 3, 1, true, "1 of 3 so far; 2 more, and encounters in December still count towards Semester 2, 2026; 1 at the minimum level when observed")]
    [InlineData("2026-07-01", "2027-01-01", 1, 3, 1, true, "1 of 3, 2 short; 1 at the minimum level when observed")]
    // A programme that has ended owes nothing more in a running period.
    [InlineData("2026-07-01", "2026-09-23", 1, 3, 1, false, "1 of 3 so far; 1 at the minimum level when observed")]
    public void APeriodLine_SaysWhetherThePeriodHasEnded(
        string inPeriod, string today, int count, int target, int reached, bool programmeActive, string expected)
    {
        var period = Period(QuotaPeriod.Semester, Day(inPeriod), ProgrammeStart, count, target, reached);

        EpaProgressSectionComponent.PeriodLine(period, Day(today), programmeActive).Should().Be(expected);
    }

    [Fact]
    public void APeriodLine_ForAnAcademicYear_RunsToTheEndOfDecember()
    {
        var year = Period(QuotaPeriod.AcademicYear, new DateOnly(2026, 3, 1), ProgrammeStart, count: 0, target: 1, reached: 0);

        EpaProgressSectionComponent.PeriodLine(year, new DateOnly(2026, 9, 23), programmeActive: true)
            .Should().Be("0 of 1 so far; 1 more by 30 November 2026");
        EpaProgressSectionComponent.PeriodLine(year, new DateOnly(2027, 1, 1), programmeActive: true)
            .Should().Be("0 of 1, 1 short");
    }

    [Fact]
    public void APeriodLine_ForAWaivedOrUnstartedPeriod_SaysSo()
    {
        var waived = Period(QuotaPeriod.Semester, new DateOnly(2026, 9, 1), new DateOnly(2026, 8, 15), count: 2, target: 3, reached: 2);
        EpaProgressSectionComponent.PeriodLine(waived, new DateOnly(2027, 3, 1), programmeActive: true)
            .Should().Be("no target (started part-way through), 2 recorded");

        var unstarted = Period(QuotaPeriod.Semester, new DateOnly(2026, 8, 1), new DateOnly(2027, 1, 1), count: 0, target: 3, reached: 0);
        EpaProgressSectionComponent.PeriodLine(unstarted, new DateOnly(2026, 8, 1), programmeActive: true)
            .Should().Be("no target yet; targets start with semester 1, 2027");
    }

    [Fact]
    public void APeriodLine_ForAPeriodTheProgrammeEndedInOrAfter_SaysSo_AndIsNeverShort()
    {
        // D49 (T209). Read long after both periods closed, when a counting period would be "met" or "short".
        var end = new DateOnly(2026, 3, 14);
        var endedIn = Period(QuotaPeriod.Semester, new DateOnly(2026, 2, 1), ProgrammeStart, count: 1, target: 3, reached: 1, end);
        var after = Period(QuotaPeriod.Semester, new DateOnly(2026, 8, 1), ProgrammeStart, count: 2, target: 3, reached: 2, end);

        EpaProgressSectionComponent.PeriodLine(endedIn, new DateOnly(2027, 3, 1), programmeActive: false)
            .Should().Be("no target (the programme ended part-way through), 1 recorded");
        EpaProgressSectionComponent.PeriodLine(after, new DateOnly(2027, 3, 1), programmeActive: false)
            .Should().Be("no target (after the programme ended), 2 recorded");

        var endedInLastMonth = Period(QuotaPeriod.Semester, new DateOnly(2026, 2, 1), ProgrammeStart, count: 1, target: 3, reached: 1, new DateOnly(2026, 6, 1));
        EpaProgressSectionComponent.PeriodLine(endedInLastMonth, new DateOnly(2027, 3, 1), programmeActive: false)
            .Should().Be("1 of 3, 2 short; 1 at the minimum level when observed");
    }

    [Fact]
    public void TheTargetLine_IsTheProgressPagesTarget()
    {
        var semester = Item(QuotaPeriod.Semester, target: 3);
        var year = Item(QuotaPeriod.AcademicYear, target: 1);

        EpaProgressSectionComponent.TargetLine(semester, trainingYear: 2)
            .Should().Be("3 per semester (6 a year), minimum 3a in training year 2");
        EpaProgressSectionComponent.TargetLine(year, trainingYear: null)
            .Should().Be("1 per academic year, minimum 3a");
    }

    [Fact]
    public void TheRatingsLine_CountsAssessors_AndMarksAnOffLadderOrUndatedLatestRating()
    {
        EpaProgressSectionComponent.RatingsLine(PortfolioEpaRatings.None).Should().Be("none in this export");
        EpaProgressSectionComponent.RatingsLine(new PortfolioEpaRatings(4, 2, "3a", false, "2026-08-12"))
            .Should().Be("4 from 2 assessors; latest 3a, encounter 2026-08-12");
        EpaProgressSectionComponent.RatingsLine(
                new PortfolioEpaRatings(1, 1, "4", true, "not recorded (created 2026-08-12)"))
            .Should().Be("1 from 1 assessor; latest 4 (on a different scale), encounter not recorded (created 2026-08-12)");
    }

    [Fact]
    public void ARowWithNoTarget_SaysWhy()
    {
        EpaProgressSectionComponent.NoTargetLine(PortfolioNoTargetReason.NoProgramme)
            .Should().Be("none: the trainee has no training programme");
        EpaProgressSectionComponent.NoTargetLine(PortfolioNoTargetReason.NotInCurriculum)
            .Should().Be("none: this EPA is not in the trainee's curriculum");
        EpaProgressSectionComponent.NoTargetLine(PortfolioNoTargetReason.EpaDeactivated)
            .Should().Be("none: this EPA has been deactivated, so it is no longer a target");
    }

    [Fact]
    public async Task TheIntroduction_NamesTheDay_TheReach_AndTheRunningPeriodRule()
    {
        await using var db = SeededDb();

        var whole = EpaProgressSectionComponent.Introduction((await LoadAsync(db)).EpaProgress);
        whole.Should().StartWith("Targets as on the trainee's progress page on 23 September 2026 (training year 2).");
        whole.Should().Contain("back to the start of the programme");
        whole.Should().Contain("A period that has not ended by 23 September 2026 shows its count so far.");

        var annual = EpaProgressSectionComponent.Introduction(
            (await LoadAsync(db, new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30))).EpaProgress);
        annual.Should().StartWith("Targets as on the trainee's progress page on 30 June 2026 (training year 2).");
        annual.Should().Contain("back to the one containing 1 January 2026");
        annual.Should().Contain("A period that has not ended by 23 September 2026 shows its count so far.");
    }

    private static DateOnly Day(string iso) => DateOnly.ParseExact(iso, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";

    /// <summary>Every run of text the portfolio draws, in page order, as ActivitiesSectionEncounterDateTests reads it (T161).</summary>
    private static IReadOnlyList<string> RenderedText(PortfolioData data)
        => PortfolioPdfService.ComposeDocument(data).GenerateSvg()
            .SelectMany(page => XDocument.Parse(page).Descendants(Svg + "text"))
            .Select(text => text.Value.Trim())
            .ToList();

    private static QuotaWindowDto Period(
        QuotaPeriod kind, DateOnly inPeriod, DateOnly programmeStart, int count, int target, int reached,
        DateOnly? programmeEnd = null)
        => QuotaWindowDto.From(new QuotaWindowTally(
            QuotaWindow.For(kind, inPeriod, programmeStart, programmeEnd), target, count, reached, LastObservedOn: null, LastObservedOnDeclared: false));

    private static TraineeCurriculumProgressDto Item(QuotaPeriod kind, int target)
    {
        var current = Period(kind, new DateOnly(2026, 8, 1), ProgrammeStart, count: 0, target, reached: 0);
        return new TraineeCurriculumProgressDto(
            1, 1, "PAED-001", "Acute admission", kind, target, current, Previous: null,
            EffectiveMinimumLevelOrder: 3, EffectiveMinimumLevelLabel: "3a", TrainingYearChangedOn: null);
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private const int PinnedScaleId = 42;

    private static PortfolioPdfService Service(ApplicationDbContext db)
        => new(db, new MsfAggregationService(), new FixedClock(Now));

    private static PortfolioExportRequest Request(DateOnly? fromDate = null, DateOnly? toDate = null)
        => new(Trainee, fromDate, toDate, SubjectPrincipal());

    private static Task<PortfolioData> LoadAsync(ApplicationDbContext db, DateOnly? fromDate = null, DateOnly? toDate = null)
        => Service(db).LoadPortfolioDataAsync(Request(fromDate, toDate), CancellationToken.None);

    /// <summary>
    /// A trainee at institution 1 on a curriculum shared with institution 2, started 1 January 2025, with credit in four
    /// semesters and one activity in each finishing state the summary must read.
    /// </summary>
    private static ApplicationDbContext SeededDb()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        db.Set<WombatIdentityUser>().Add(new WombatIdentityUser { Id = Trainee, FirstName = "Lerato", LastName = "Molefe" });
        db.Institutions.Add(new Institution { Id = 1, Name = "Host Academic Hospital", ShortCode = "HOST" });
        db.Institutions.Add(new Institution { Id = 2, Name = "Other Academic Hospital", ShortCode = "OTHR" });
        db.Specialities.Add(new Speciality { Id = 1, CollegeId = 1, Name = "Paediatrics" });
        db.SubSpecialities.Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "General Paediatrics" });
        db.Curricula.Add(new Curriculum
        {
            Id = 1, SubSpecialityId = 1, Name = "CPSA Paediatrics", Version = "11.1",
            EffectiveFrom = new DateOnly(2025, 1, 1), IsActive = true
        });

        db.EntrustmentScales.Add(new EntrustmentScale { Id = PinnedScaleId, Name = "CPSA Paediatric Entrustment Scale v11.1", SeedKey = "cpsa:scale:v11.1" });
        var labels = new[] { "1", "2", "3a", "3b", "4", "5" };
        for (var order = 1; order <= labels.Length; order++)
        {
            db.EntrustmentLevels.Add(new EntrustmentLevel
            {
                Id = 4200 + order, ScaleId = PinnedScaleId, Order = order, Label = labels[order - 1]
            });
        }

        db.Epas.Add(new Epa { Id = 1, SubSpecialityId = 1, Code = "PAED-001", Title = "Acute admission", IsActive = true });
        db.Epas.Add(new Epa { Id = 2, SubSpecialityId = 1, Code = "PAED-002", Title = "Ward round", IsActive = true });
        db.Epas.Add(new Epa { Id = 3, SubSpecialityId = 1, OwningInstitutionId = 2, Code = "LOCAL-2", Title = "Someone else's extra", IsActive = true });
        db.Epas.Add(new Epa { Id = 9, SubSpecialityId = 1, Code = "PAED-009", Title = "Not in this curriculum", IsActive = true });

        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 1, CurriculumId = 1, EpaId = 1, RequiredCount = 3, QuotaPeriod = QuotaPeriod.Semester,
            MinimumLevelOrder = 3, WindowMonths = 36, ScaleId = PinnedScaleId
        });
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 2, CurriculumId = 1, EpaId = 2, RequiredCount = 1, QuotaPeriod = QuotaPeriod.AcademicYear,
            MinimumLevelOrder = 3, WindowMonths = 36, ScaleId = PinnedScaleId
        });
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 3, CurriculumId = 1, EpaId = 3, OwningInstitutionId = 2, RequiredCount = 1,
            QuotaPeriod = QuotaPeriod.Semester, MinimumLevelOrder = 3, WindowMonths = 36
        });

        db.TraineeProfiles.Add(new TraineeProfile
        {
            Id = 1, UserId = Trainee, InstitutionId = 1, CurriculumId = 1,
            ProgrammeStartDate = new DateOnly(2025, 1, 1), ExpectedCompletionDate = new DateOnly(2029, 1, 1),
            IsActive = true
        });

        AddProgress(db, itemId: 1, 2025, 1, counts: 1);
        AddProgress(db, itemId: 1, 2025, 2, counts: 3);
        AddProgress(db, itemId: 1, 2026, 1, counts: 2);
        AddProgress(db, itemId: 1, 2026, 2, counts: 1);
        AddProgress(db, itemId: 2, 2025, 2, counts: 1);
        AddProgress(db, itemId: 3, 2026, 2, counts: 4);

        var miniCex = AddSeedType(db, 11, "mini_cex_cpsa");
        var reflective = AddSeedType(db, 12, "reflective_exercise_cpsa");
        var msf = AddSeedType(db, 13, "msf_cpsa");
        var procedureLog = AddSeedType(db, 14, "procedure_log");

        AddRated(db, 101, miniCex, "completed", epaId: 1, "assessor-a", rating: 3, new DateOnly(2026, 8, 12));
        AddRated(db, 102, miniCex, "completed", epaId: 1, "assessor-b", rating: 4, new DateOnly(2026, 3, 10));
        AddRated(db, 103, miniCex, "declined", epaId: 1, "assessor-c", rating: 5, new DateOnly(2026, 8, 20));
        AddRated(db, 104, miniCex, "completed", epaId: 9, "assessor-a", rating: 3, new DateOnly(2026, 5, 5));
        AddActivity(db, 105, reflective, "discussed");
        AddActivity(db, 106, reflective, "draft");
        AddActivity(db, 107, reflective, "cancelled");
        AddActivity(db, 108, msf, "recorded", epaId: 1, dataJson: """{ "epa_id": 1, "overall_level": 4 }""");
        AddActivity(db, 109, procedureLog, "logged");

        Save(db);
        return db;
    }

    private static void Save(ApplicationDbContext db)
    {
        db.SaveChanges();
        db.ChangeTracker.Clear();
    }

    private static void AddProgress(ApplicationDbContext db, int itemId, int year, int semester, int counts)
        => db.CurriculumItemProgresses.Add(new CurriculumItemProgress
        {
            CurriculumItemId = itemId,
            TraineeUserId = Trainee,
            AcademicYear = year,
            Semester = semester,
            CountsSoFar = counts,
            MinimumLevelReachedCount = counts,
            LastUpdated = new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc)
        });

    private static ActivityType AddSeedType(ApplicationDbContext db, int id, string key)
    {
        var workflow = ReadSeedFile(key, "workflow.json");
        var type = AddType(db, id, key, ReadSeedFile(key, "schema.json"), workflow, version: 1);
        type.CreditRulesJson = ReadSeedFile(key, "credit.json");
        AddVersion(db, type, 1, workflow, type.SchemaJson, type.CreditRulesJson);
        return type;
    }

    private static ActivityType AddType(
        ApplicationDbContext db, int id, string key, string schemaJson, string? workflowJson, int version)
    {
        var type = new ActivityType
        {
            Id = id,
            Key = key,
            Name = key,
            Version = version,
            IsActive = true,
            OwnerUserId = "seed-system",
            CreatedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = """{ "counts_for": [] }"""
        };
        db.ActivityTypes.Add(type);
        return type;
    }

    private static void AddVersion(
        ApplicationDbContext db,
        ActivityType type,
        int version,
        string workflowJson,
        string? schemaJson = null,
        string? creditRulesJson = null)
        => db.ActivityTypeVersions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = type.Id,
            Version = version,
            SchemaJson = schemaJson ?? type.SchemaJson!,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditRulesJson ?? """{ "counts_for": [] }""",
            PublishedByUserId = "seed-system",
            PublishedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

    /// <summary>A Mini-CEX as <c>ActivityService</c> leaves it: the EPA stamped from <c>evidence_epa_field</c> (T137).</summary>
    private static void AddRated(
        ApplicationDbContext db, int id, ActivityType type, string state, int epaId, string assessor, int rating, DateOnly observedOn)
        => AddActivity(
            db, id, type, state, epaId: epaId, observedOn: observedOn,
            dataJson: $$"""
                { "epa_id": {{epaId}}, "assessor_user_id": "{{assessor}}", "observed_on": "{{observedOn:yyyy-MM-dd}}",
                  "overall_level": {{rating}} }
                """);

    /// <summary>A Mini-CEX filed and completed two days after its encounter, with the history credit is recorded against.</summary>
    private static void AddCompletedMiniCex(ApplicationDbContext db, int id, ActivityType type, DateOnly observedOn)
    {
        AddRated(db, id, type, "completed", epaId: 1, "assessor-a", rating: 3, observedOn);
        var activity = db.Activities.Local.Single(entity => entity.Id == id);
        var filedOn = observedOn.AddDays(2).ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc);
        activity.Transitions.Add(new ActivityTransition
        {
            ActivityId = id, FromState = "draft", ToState = "requested", TransitionKey = "submit",
            ActorUserId = Trainee, OccurredOn = filedOn.AddMinutes(-5)
        });
        activity.Transitions.Add(new ActivityTransition
        {
            ActivityId = id, FromState = "requested", ToState = "completed", TransitionKey = "complete",
            ActorUserId = "assessor-a", OccurredOn = filedOn
        });
    }

    private static ClaimsPrincipal Administrator()
        => new(new ClaimsIdentity([new Claim(ClaimTypes.Role, WombatRoles.Administrator)], "test"));

    private static void AddActivity(
        ApplicationDbContext db,
        int id,
        ActivityType type,
        string state,
        int? schemaVersion = null,
        int? epaId = null,
        DateOnly? observedOn = null,
        string dataJson = "{}")
    {
        var on = observedOn ?? new DateOnly(2026, 4, 1);
        db.Activities.Add(new Activity
        {
            Id = id,
            ActivityTypeId = type.Id,
            SchemaVersion = schemaVersion ?? type.Version,
            SubjectUserId = Trainee,
            CreatedByUserId = Trainee,
            CurrentState = state,
            DataJson = dataJson,
            EpaId = epaId,
            InstitutionId = 1,
            CreatedOn = on.ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc),
            UpdatedOn = on.ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc),
            ObservedOn = on,
            ObservedOnSource = ObservationDateSource.Declared
        });
    }

    private static string ReadSeedFile(string key, string fileName)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", key, fileName));

    private static string Schema(string key) => $$"""
        {
          "version": 1,
          "sections": [
            { "key": "{{key}}", "title": "Notes", "fields": [ { "key": "notes", "type": "longtext", "label": "Notes" } ] }
          ]
        }
        """;

    /// <summary>Finishes in <c>done</c>.</summary>
    private const string FinishesInDone = """
        {
          "version": 1,
          "initial_state": "open",
          "states": [
            { "key": "open", "label": "Open" },
            { "key": "done", "label": "Done", "terminal": true }
          ],
          "transitions": [
            { "key": "finish", "from": "open", "to": "done", "actor": "subject|creator" }
          ]
        }
        """;

    /// <summary>Passes through <c>done</c>, which is no longer a finish, and finishes in <c>signed_off</c>.</summary>
    private const string FinishesInSignedOff = """
        {
          "version": 1,
          "initial_state": "open",
          "states": [
            { "key": "open", "label": "Open" },
            { "key": "done", "label": "Done" },
            { "key": "signed_off", "label": "Signed off", "terminal": true }
          ],
          "transitions": [
            { "key": "park", "from": "open", "to": "done", "actor": "subject|creator" },
            { "key": "sign_off", "from": "done", "to": "signed_off", "actor": "subject|creator" }
          ]
        }
        """;

    private static ClaimsPrincipal SubjectPrincipal()
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, Trainee)], "test"));

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
