using System.Security.Claims;
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
/// T131 slice 5 (O4): a review before a panel sitting as a College committee decides entrustment only. It is scheduled as
/// one, records and ratifies its decision with no progression category, and an appeal against it is remitted with none;
/// a general panel's progression review still needs one, at each step.
/// </summary>
/// <remarks>
/// A miniature of the v11.1 shape: PAED-001 is decided each semester by the trainee's general committee, PAED-004 and 005
/// each semester by the neonatal CCC. Every refusal is followed by the save the audit pipeline makes from its catch and a
/// cleared tracker, and the store is read back through a second context: a check that ran after a mutation would have the
/// refusal commit it.
/// </remarks>
public sealed class EntrustmentOnlyReviewHandlerTests
{
    private const int InstitutionA = 1;
    private const int GeneralPanel = 10;
    private const int NeonatalPanel = 11;
    private const int CurriculumId = 100;
    private const string Trainee = "trainee-a";

    /// <summary>A trainee on a curriculum with no neonatal EPA: the neonatal CCC decides nothing for them.</summary>
    private const string TraineeWithoutNeonatal = "trainee-b";
    private const string Neonatal = "neonatal";
    private const string NeonatalCcc = "Neonatal team Clinical Competency Committee";
    private const int Level = 3;

    private static readonly (string Code, int Id)[] Epas = [("PAED-001", 1), ("PAED-004", 4), ("PAED-005", 5)];

    private readonly string _databaseName = Guid.NewGuid().ToString();

    // ─── Scheduling: what the review decides ─────────────────────────────────

    [Fact]
    public async Task AReviewBeforeTheNeonatalCcc_IsEntrustmentOnly_WhenNoTypeIsAsked()
    {
        await using var db = await SeededDbAsync();

        var review = await ScheduleAsync(db, NeonatalPanel, semester: 1, type: null);

        review.ReviewType.Should().Be(CommitteeReviewType.EntrustmentOnly);
        review.DecidesProgression.Should().BeFalse();
        await using var read = CreateDb();
        (await read.CommitteeReviews.SingleAsync()).ReviewType.Should().Be(CommitteeReviewType.EntrustmentOnly);
    }

    [Theory]
    [InlineData(CommitteeReviewType.AnnualProgression, 1)]
    [InlineData(CommitteeReviewType.PreGraduation, 2)]
    public async Task AProgressionReviewBeforeTheNeonatalCcc_IsRefused_NamingIt_AndWritesNothing(CommitteeReviewType type, int semester)
    {
        await using var db = await SeededDbAsync();

        var act = () => ScheduleAsync(db, NeonatalPanel, semester, type);

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message
            .Should().StartWith($"The {NeonatalCcc} decides entrustment only");
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        await using var read = CreateDb();
        (await read.CommitteeReviews.CountAsync()).Should().Be(0);
        (await read.CommitteeAgendaLines.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task AGeneralPanel_MaySitEntrustmentOnlyInSemester1_ButNotInSemester2_WhereItDecidesProgression()
    {
        await using var db = await SeededDbAsync();

        var semester1 = await ScheduleAsync(db, GeneralPanel, semester: 1, CommitteeReviewType.EntrustmentOnly);
        var act = () => ScheduleAsync(db, GeneralPanel, semester: 2, CommitteeReviewType.EntrustmentOnly);

        semester1.ReviewType.Should().Be(CommitteeReviewType.EntrustmentOnly);
        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message
            .Should().Be(CommitteeReviewTypes.EntrustmentOnlyNotAtSemester2);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        await using var read = CreateDb();
        (await read.CommitteeReviews.Select(review => review.Id).ToListAsync()).Should().Equal(semester1.Id);
    }

    [Fact]
    public async Task AGeneralPanelsReview_IsAnnualProgression_WhenNoTypeIsAsked()
    {
        await using var db = await SeededDbAsync();

        var review = await ScheduleAsync(db, GeneralPanel, semester: 2, type: null);

        review.ReviewType.Should().Be(CommitteeReviewType.AnnualProgression);
        review.DecidesProgression.Should().BeTrue();
    }

    [Fact]
    public void TheScheduleValidator_RefusesATypeThatIsNoneOfTheTypes()
    {
        var result = new ScheduleCommitteeReviewCommandValidator().Validate(new ScheduleCommitteeReviewCommand(
            Trainee, NeonatalPanel, 2026, 1, new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), new DateOnly(2026, 7, 2),
            TestPrincipals.Coordinator(InstitutionA), ReviewType: (CommitteeReviewType)99));

        result.Errors.Select(error => error.ErrorMessage).Should().Contain(CommitteeReviewTypes.UnknownType);
    }

    [Fact]
    public async Task AnEntrustmentOnlyReview_BeforeAPanelThatDecidesNothingOnTheTraineesCurriculum_IsRefused_AndWritesNothing()
    {
        // Its decision is what its agenda holds, and nothing on this trainee's curriculum could ever be put there: the review
        // could never be recorded. The preview says so first (picker = gate).
        await using var db = await SeededDbAsync();
        await AddTraineeWithoutNeonatalEpasAsync(db);

        var preview = await new PreviewCommitteeAgendaQueryHandler(db, FakeUserDirectory.TraineesOf(db)).Handle(
            new PreviewCommitteeAgendaQuery(TraineeWithoutNeonatal, NeonatalPanel, 2026, 1, TestPrincipals.Coordinator(InstitutionA)),
            CancellationToken.None);
        var act = () => ScheduleAsync(db, NeonatalPanel, semester: 1, type: null, TraineeWithoutNeonatal);

        preview.TraineeHasCurriculum.Should().BeTrue();
        preview.PanelDecidesAnything.Should().BeFalse();
        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(
            "Neonatal CCC decides no EPA on this trainee's curriculum, so an entrustment-only review before it would have " +
            "nothing to decide.");
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        await using (var read = CreateDb())
        {
            (await read.CommitteeReviews.CountAsync()).Should().Be(0);
        }

        // The control: the general panel decides PAED-001 for the same trainee, so an entrustment-only sitting may be held
        // before it, and a progression review before either panel is never refused for it.
        (await ScheduleAsync(db, GeneralPanel, semester: 1, CommitteeReviewType.EntrustmentOnly, TraineeWithoutNeonatal))
            .ReviewType.Should().Be(CommitteeReviewType.EntrustmentOnly);
        (await new PreviewCommitteeAgendaQueryHandler(db, FakeUserDirectory.TraineesOf(db)).Handle(
                new PreviewCommitteeAgendaQuery(TraineeWithoutNeonatal, GeneralPanel, 2026, 2, TestPrincipals.Coordinator(InstitutionA)),
                CancellationToken.None))
            .PanelDecidesAnything.Should().BeTrue();
    }

    // ─── Recording and ratifying ─────────────────────────────────────────────

    [Fact]
    public async Task AReviewBeforeTheNeonatalCcc_RecordsAndRatifiesWithNoCategory_AndIssuesItsStar()
    {
        await using var db = await SeededDbAsync();
        var review = await ScheduleAsync(db, NeonatalPanel, semester: 1, type: null);
        await StartWithEvidenceAsync(db, review.Id);
        await StageAsync(db, review.Id, EpaIdOf("PAED-004"));
        await DeferAsync(db, review.Id, "PAED-005", "The neonatal rotation has not started.");

        var decided = await RecordAsync(db, review.Id, NeonatalChair(), category: null, "chair-n", "member-n");
        var ratified = await RatifyAsync(db, review.Id, NeonatalChair());

        decided.State.Should().Be(CommitteeReviewState.Decided);
        ratified.State.Should().Be(CommitteeReviewState.Ratified);
        ratified.DecidesProgression.Should().BeFalse();
        ratified.Decisions.Should().ContainSingle().Which.Category.Should().BeNull();

        await using var read = CreateDb();
        (await read.Set<CommitteeDecision>().SingleAsync()).Category.Should().BeNull();
        var star = await read.EntrustmentDecisions.SingleAsync();
        star.EpaId.Should().Be(EpaIdOf("PAED-004"));
        star.IssuedByCommitteeReviewId.Should().Be(review.Id);
        (await read.CommitteeAgendaLines.SingleAsync(line => line.EpaCode == "PAED-004")).State
            .Should().Be(CommitteeAgendaLineState.Decided);
    }

    [Fact]
    public async Task AnEntrustmentOnlyReview_RefusesAProgressionCategory_AndWritesNothing()
    {
        await using var db = await SeededDbAsync();
        var review = await ScheduleAsync(db, NeonatalPanel, semester: 1, type: null);
        await StartWithEvidenceAsync(db, review.Id);
        await StageAsync(db, review.Id, EpaIdOf("PAED-004"));
        await DeferAsync(db, review.Id, "PAED-005", "The neonatal rotation has not started.");

        var act = () => RecordAsync(db, review.Id, NeonatalChair(), CommitteeDecisionCategory.SatisfactoryProgress, "chair-n", "member-n");

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message
            .Should().Be(CommitteeReview.EntrustmentOnlyRecordsNoCategory);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        await using var read = CreateDb();
        (await read.CommitteeReviews.SingleAsync()).State.Should().Be(CommitteeReviewState.InProgress);
        (await read.Set<CommitteeDecision>().CountAsync()).Should().Be(0);
        (await read.Set<CommitteeDecisionAttendee>().CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task AGeneralAnnualProgressionReview_StillNeedsACategory_AndWritesNothingWithoutOne()
    {
        await using var db = await SeededDbAsync();
        var review = await ScheduleAsync(db, GeneralPanel, semester: 1, CommitteeReviewType.AnnualProgression);
        await StartWithEvidenceAsync(db, review.Id);
        await StageAsync(db, review.Id, EpaIdOf("PAED-001"));

        var act = () => RecordAsync(db, review.Id, GeneralChair(), category: null, "chair-a", "member-a");

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message
            .Should().Be(CommitteeReview.ProgressionNeedsACategory);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        await using var read = CreateDb();
        (await read.CommitteeReviews.SingleAsync()).State.Should().Be(CommitteeReviewState.InProgress);
        (await read.Set<CommitteeDecision>().CountAsync()).Should().Be(0);

        // The control: with a category the same review is recorded and ratified.
        await RecordAsync(db, review.Id, GeneralChair(), CommitteeDecisionCategory.SatisfactoryProgress, "chair-a", "member-a");
        (await RatifyAsync(db, review.Id, GeneralChair())).State.Should().Be(CommitteeReviewState.Ratified);
    }

    [Fact]
    public async Task Ratify_RefusesAProgressionReviewWhoseStoredDecisionRecordsNoCategory_AndIssuesNoStar()
    {
        // Recording refuses it, so the row is written as one stored before the rule would be: ratify's own branch holds.
        await using var db = await SeededDbAsync();
        var review = await ScheduleAsync(db, GeneralPanel, semester: 1, CommitteeReviewType.AnnualProgression);
        await StartWithEvidenceAsync(db, review.Id);
        await StageAsync(db, review.Id, EpaIdOf("PAED-001"));
        await RecordAsync(db, review.Id, GeneralChair(), CommitteeDecisionCategory.SatisfactoryProgress, "chair-a", "member-a");
        var stored = await db.Set<CommitteeDecision>().SingleAsync();
        db.Entry(stored).Property(decision => decision.Category).CurrentValue = null;
        await SaveAndClearAsAuditPipelineWouldAsync(db);

        var act = () => RatifyAsync(db, review.Id, GeneralChair());

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message
            .Should().StartWith("This review decides the trainee's progression, but its decision records no progression category");
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        await using var read = CreateDb();
        var unratified = await read.CommitteeReviews.SingleAsync();
        unratified.State.Should().Be(CommitteeReviewState.Decided);
        unratified.RatifiedOn.Should().BeNull();
        (await read.EntrustmentDecisions.CountAsync()).Should().Be(0);
        (await read.PendingEntrustmentDecisions.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task AnEntrustmentOnlyReview_WithNothingOnItsAgenda_IsNotRecorded_UntilADecisionIsStagedOnIt()
    {
        // PAED-004 and 005 route to the neonatal CCC but carry no cadence here, so nothing is due and the agenda starts
        // empty: the panel still decides them, so the review is scheduled, and the chair stages the one it re-decides.
        await using var db = await SeededDbAsync();
        await ClearCadenceAsync(db, "PAED-004", "PAED-005");
        var review = await ScheduleAsync(db, NeonatalPanel, semester: 1, type: null);
        await StartWithEvidenceAsync(db, review.Id);

        var agenda = await AgendaAsync(db, review.Id);
        var act = () => RecordAsync(db, review.Id, NeonatalChair(), category: null, "chair-n", "member-n");

        agenda.Lines.Should().BeEmpty();
        agenda.RecordBlockedReason.Should().Be(CommitteeReview.NothingOnTheAgenda, "the page shows Record disabled with it");
        agenda.RatifyBlockedReason.Should().BeNull();
        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message
            .Should().Be("The committee's decision cannot be recorded yet: " + CommitteeReview.NothingOnTheAgenda);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        await using (var read = CreateDb())
        {
            (await read.CommitteeReviews.SingleAsync()).State.Should().Be(CommitteeReviewState.InProgress);
            (await read.Set<CommitteeDecision>().CountAsync()).Should().Be(0);
            (await read.Set<CommitteeDecisionAttendee>().CountAsync()).Should().Be(0);
        }

        // Staging PAED-004 puts the chair's line on the agenda; then the decision is recorded and ratified, with its STAR.
        await StageAsync(db, review.Id, EpaIdOf("PAED-004"));
        (await AgendaAsync(db, review.Id)).RecordBlockedReason.Should().BeNull();
        await RecordAsync(db, review.Id, NeonatalChair(), category: null, "chair-n", "member-n");
        (await RatifyAsync(db, review.Id, NeonatalChair())).State.Should().Be(CommitteeReviewState.Ratified);
        await using var after = CreateDb();
        (await after.EntrustmentDecisions.SingleAsync()).EpaId.Should().Be(EpaIdOf("PAED-004"));
    }

    [Fact]
    public async Task AProgressionReview_WithNothingOnItsAgenda_IsRecordedWithItsCategory()
    {
        // Its decision is its category: an empty agenda blocks nothing.
        await using var db = await SeededDbAsync();
        await ClearCadenceAsync(db, "PAED-001");
        var review = await ScheduleAsync(db, GeneralPanel, semester: 1, CommitteeReviewType.AnnualProgression);
        await StartWithEvidenceAsync(db, review.Id);

        (await AgendaAsync(db, review.Id)).RecordBlockedReason.Should().BeNull();
        await RecordAsync(db, review.Id, GeneralChair(), CommitteeDecisionCategory.SatisfactoryProgress, "chair-a", "member-a");
        (await RatifyAsync(db, review.Id, GeneralChair())).State.Should().Be(CommitteeReviewState.Ratified);
    }

    // ─── The panel's committee, under a review still open ────────────────────

    [Fact]
    public async Task AGeneralPanel_IsNotTaggedAsTheNeonatalCcc_WhileAReviewBeforeItIsOpen_NamingIt_AndNothingIsWritten()
    {
        // Its open progression review would sit before the neonatal CCC, and Start would add the committee's EPAs to it. A
        // ratified review before the panel does not stand in the way.
        await using var db = await SeededDbAsync();
        await RatifiedGeneralReviewAsync(db);
        var open = await ScheduleAsync(db, GeneralPanel, semester: 2, CommitteeReviewType.AnnualProgression);
        await SetBodyAsync(db, NeonatalPanel, null, InstitutionalAdmin());

        var act = () => SetBodyAsync(db, GeneralPanel, Neonatal, InstitutionalAdmin());

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(
            $"Review #{open.Id} (2026 S2, scheduled) still sits before this panel. What a review decides, and its agenda, " +
            "follow the College committee the panel sat as when it was scheduled, so the panel keeps what it sits as until " +
            "its open reviews are ratified, or closed if formative.");
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await StoredBodiesAsync()).Should().Equal(new Dictionary<int, string?> { [GeneralPanel] = null, [NeonatalPanel] = null });

        // The control: once the open review is ratified, the panel takes the committee.
        await StartWithEvidenceAsync(db, open.Id);
        await StageAsync(db, open.Id, EpaIdOf("PAED-001"));
        await StageAsync(db, open.Id, EpaIdOf("PAED-004"));
        await StageAsync(db, open.Id, EpaIdOf("PAED-005"));
        await RecordAsync(db, open.Id, GeneralChair(), CommitteeDecisionCategory.SatisfactoryProgress, "chair-a", "member-a");
        await RatifyAsync(db, open.Id, GeneralChair());
        (await SetBodyAsync(db, GeneralPanel, Neonatal, InstitutionalAdmin())).DecisionBodyKey.Should().Be(Neonatal);
    }

    [Fact]
    public async Task TheNeonatalCcc_IsNotMadeGeneral_WhileAnEntrustmentOnlyReviewBeforeItIsOpen_ButKeepsItsOwnCommittee()
    {
        // Made general, the panel would hold an entrustment-only review at a semester-2 sitting, which the rule allows no
        // general panel. Saving the committee it already sits as changes nothing, and is not refused.
        await using var db = await SeededDbAsync();
        var open = await ScheduleAsync(db, NeonatalPanel, semester: 2, type: null);
        await StartWithEvidenceAsync(db, open.Id);

        var act = () => SetBodyAsync(db, NeonatalPanel, null, InstitutionalAdmin());

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message
            .Should().StartWith($"Review #{open.Id} (2026 S2, in progress) still sits before this panel.");
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await StoredBodiesAsync())[NeonatalPanel].Should().Be(Neonatal);
        (await SetBodyAsync(db, NeonatalPanel, Neonatal, InstitutionalAdmin())).DecisionBodyKey.Should().Be(Neonatal);
    }

    // ─── An appeal that remits the decision ──────────────────────────────────

    [Fact]
    public async Task ARemittedAppeal_OnAnEntrustmentOnlyReview_ReplacesItsDecisionWithOneThatRecordsNoCategory()
    {
        await using var db = await AppealedDbAsync(NeonatalPanel);
        var originalId = await db.Set<CommitteeDecision>().Select(decision => decision.Id).SingleAsync();

        var resolved = await ResolveAsync(db, NeonatalPanel, NeonatalChair(), category: null, "chair-n", "member-n");

        resolved.State.Should().Be(CommitteeReviewState.Final);
        resolved.Decisions.Should().HaveCount(2);
        resolved.Decisions[0].Category.Should().BeNull();
        resolved.Decisions[0].SupersedesDecisionId.Should().Be(originalId);
        await using var read = CreateDb();
        (await read.Set<CommitteeDecision>().Select(decision => decision.Category).ToListAsync()).Should().AllSatisfy(
            category => category.Should().BeNull());
        (await read.CommitteeAppeals.SingleAsync()).Outcome.Should().Be(CommitteeAppealOutcome.Remitted);
    }

    [Fact]
    public async Task ARemittedAppeal_OnAnEntrustmentOnlyReview_WithAProgressionCategory_IsRefused_AndWritesNothing()
    {
        await using var db = await AppealedDbAsync(NeonatalPanel);

        var act = () => ResolveAsync(db, NeonatalPanel, NeonatalChair(), CommitteeDecisionCategory.SatisfactoryProgress, "chair-n", "member-n");

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message
            .Should().Be(CommitteeReview.EntrustmentOnlyRecordsNoCategory);
        await AssertTheAppealIsStillOpenAsync(db);
    }

    [Fact]
    public async Task ARemittedAppeal_OnAProgressionReview_WithNoCategory_IsRefused_AndWritesNothing()
    {
        // The validator used to demand the category; it cannot see the review's type, so the domain does now.
        await using var db = await AppealedDbAsync(GeneralPanel);

        var act = () => ResolveAsync(db, GeneralPanel, GeneralChair(), category: null, "chair-a", "member-a");

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message
            .Should().Be(CommitteeReview.ProgressionNeedsACategory);
        await AssertTheAppealIsStillOpenAsync(db);
    }

    // ─── What the trainee reads ──────────────────────────────────────────────

    [Fact]
    public async Task TheTrainee_ReadsTheRatifiedEntrustmentOnlyReview_AsDecided_WithNoCategory()
    {
        await using var db = await RatifiedDbAsync(NeonatalPanel);
        var trainee = TestPrincipals.Trainee(Trainee, InstitutionA);

        var listed = (await new ListReviewsForTraineeQueryHandler(db).Handle(
            new ListReviewsForTraineeQuery(Trainee, trainee), CancellationToken.None)).Should().ContainSingle().Subject;
        var detail = await new GetCommitteeReviewByIdQueryHandler(db, FakeUserDirectory.Empty).Handle(
            new GetCommitteeReviewByIdQuery(listed.Id, trainee), CancellationToken.None);

        listed.ReviewType.Should().Be(CommitteeReviewType.EntrustmentOnly);
        listed.CurrentDecisionCategory.Should().BeNull();
        listed.HasDecision.Should().BeTrue();
        CommitteeDecisionWording.OutcomeLabel(listed).Should().Be(CommitteeDecisionWording.EntrustmentOnlyOutcome);
        detail.DecidesProgression.Should().BeFalse();
        detail.Decisions.Should().ContainSingle().Which.Category.Should().BeNull();
        detail.Agenda!.Lines.Single(line => line.EpaCode == "PAED-004").Status.Should().Be(CommitteeAgendaLineStatus.Decided);
    }

    // ─── The requests ────────────────────────────────────────────────────────

    private static async Task<CommitteeReviewListItemDto> ScheduleAsync(
        ApplicationDbContext db, int panelId, int semester, CommitteeReviewType? type, string trainee = Trainee)
    {
        var scheduled = await new ScheduleCommitteeReviewCommandHandler(db, FakeUserDirectory.TraineesOf(db)).Handle(
            new ScheduleCommitteeReviewCommand(
                trainee, panelId, 2026, semester, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), new DateOnly(2026, 7, 2),
                TestPrincipals.Coordinator(InstitutionA), ReviewType: type),
            CancellationToken.None);
        db.ChangeTracker.Clear();
        return scheduled;
    }

    private static async Task<CommitteeAgendaDto> AgendaAsync(ApplicationDbContext db, int reviewId)
    {
        var agenda = await new GetCommitteeAgendaQueryHandler(db).Handle(
            new GetCommitteeAgendaQuery(reviewId, await ChairOfAsync(db, reviewId)), CancellationToken.None);
        db.ChangeTracker.Clear();
        return agenda;
    }

    private static async Task<DecisionPanelDetailDto> SetBodyAsync(
        ApplicationDbContext db, int panelId, string? bodyKey, ClaimsPrincipal principal)
    {
        var result = await new SetDecisionPanelBodyCommandHandler(db).Handle(
            new SetDecisionPanelBodyCommand(panelId, bodyKey, principal), CancellationToken.None);
        db.ChangeTracker.Clear();
        return result;
    }

    private async Task<Dictionary<int, string?>> StoredBodiesAsync()
    {
        await using var read = CreateDb();
        return await read.DecisionPanels.ToDictionaryAsync(panel => panel.Id, panel => panel.DecisionBodyKey);
    }

    private static ClaimsPrincipal InstitutionalAdmin() => TestPrincipals.InstitutionalAdmin(InstitutionA);

    /// <summary>A general S1 progression review, decided on PAED-001 and ratified: it no longer follows its panel's tag.</summary>
    private static async Task RatifiedGeneralReviewAsync(ApplicationDbContext db)
    {
        var review = await ScheduleAsync(db, GeneralPanel, semester: 1, CommitteeReviewType.AnnualProgression);
        await StartWithEvidenceAsync(db, review.Id);
        await StageAsync(db, review.Id, EpaIdOf("PAED-001"));
        await RecordAsync(db, review.Id, GeneralChair(), CommitteeDecisionCategory.SatisfactoryProgress, "chair-a", "member-a");
        await RatifyAsync(db, review.Id, GeneralChair());
    }

    /// <summary>Takes the decision cadence off these EPAs' items: still decided by their panel, never due.</summary>
    private static async Task ClearCadenceAsync(ApplicationDbContext db, params string[] codes)
    {
        var epaIds = codes.Select(EpaIdOf).ToArray();
        foreach (var item in await db.CurriculumItems.Where(item => epaIds.Contains(item.EpaId)).ToListAsync())
        {
            item.DecisionCadence = null;
        }

        await SaveAndClearAsAuditPipelineWouldAsync(db);
    }

    /// <summary>
    /// <see cref="TraineeWithoutNeonatal" />, at institution A on a curriculum holding only PAED-001, which the general panel
    /// decides.
    /// </summary>
    private static async Task AddTraineeWithoutNeonatalEpasAsync(ApplicationDbContext db)
    {
        db.Curricula.Add(new Curriculum { Id = CurriculumId + 1, SubSpecialityId = 11, Name = "General Paediatrics (no neonatal)", Version = "11.1" });
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 300,
            CurriculumId = CurriculumId + 1,
            EpaId = EpaIdOf("PAED-001"),
            RequiredCount = 1,
            MinimumLevelOrder = 3,
            DecisionCadence = QuotaPeriod.Semester
        });
        db.TraineeProfiles.Add(new TraineeProfile
        {
            Id = 2, UserId = TraineeWithoutNeonatal, InstitutionId = InstitutionA, CurriculumId = CurriculumId + 1, IsActive = true,
            ProgrammeStartDate = new DateOnly(2025, 1, 15), ExpectedCompletionDate = new DateOnly(2029, 1, 14)
        });

        await SaveAndClearAsAuditPipelineWouldAsync(db);
    }

    /// <summary>Starts the review, then writes one snapshot line per EPA: the window held no activity to freeze.</summary>
    private static async Task StartWithEvidenceAsync(ApplicationDbContext db, int reviewId)
    {
        var chair = await ChairOfAsync(db, reviewId);
        await new StartCommitteeReviewCommandHandler(db).Handle(new StartCommitteeReviewCommand(reviewId, chair), CancellationToken.None);
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

        await SaveAndClearAsAuditPipelineWouldAsync(db);
    }

    private static async Task StageAsync(ApplicationDbContext db, int reviewId, int epaId)
    {
        var evidence = await db.CommitteeEvidenceItems.Where(line => line.ReviewId == reviewId && line.EpaId == epaId)
            .Select(line => line.Id).ToArrayAsync();
        await new StagePendingEntrustmentDecisionCommandHandler(db).Handle(
            new StagePendingEntrustmentDecisionCommand(
                reviewId, null, epaId, Level, new DateOnly(2026, 7, 2), null, "Consistent.", evidence, await ChairOfAsync(db, reviewId)),
            CancellationToken.None);
        db.ChangeTracker.Clear();
    }

    private static async Task DeferAsync(ApplicationDbContext db, int reviewId, string epaCode, string reason)
    {
        var lineId = await db.CommitteeAgendaLines.Where(line => line.ReviewId == reviewId && line.EpaCode == epaCode)
            .Select(line => line.Id).SingleAsync();
        await new DeferAgendaLineCommandHandler(db).Handle(
            new DeferAgendaLineCommand(reviewId, lineId, reason, await ChairOfAsync(db, reviewId)), CancellationToken.None);
        db.ChangeTracker.Clear();
    }

    private static async Task<CommitteeReviewDetailDto> RecordAsync(
        ApplicationDbContext db, int reviewId, ClaimsPrincipal chair, CommitteeDecisionCategory? category, params string[] present)
    {
        var recorded = await new RecordCommitteeDecisionCommandHandler(db, FakeUserDirectory.CommitteeMembersAt(InstitutionA, present)).Handle(
            new RecordCommitteeDecisionCommand(reviewId, category, "Decided on the evidence named.", null, present, chair),
            CancellationToken.None);
        db.ChangeTracker.Clear();
        return recorded;
    }

    private static async Task<CommitteeReviewDetailDto> RatifyAsync(ApplicationDbContext db, int reviewId, ClaimsPrincipal chair)
    {
        var ratified = await new RatifyCommitteeDecisionCommandHandler(db).Handle(
            new RatifyCommitteeDecisionCommand(reviewId, chair), CancellationToken.None);
        db.ChangeTracker.Clear();
        return ratified;
    }

    private static async Task<CommitteeReviewDetailDto> ResolveAsync(
        ApplicationDbContext db, int panelId, ClaimsPrincipal resolver, CommitteeDecisionCategory? category, params string[] present)
    {
        var reviewId = await db.CommitteeReviews.Where(review => review.PanelId == panelId).Select(review => review.Id).SingleAsync();
        var resolved = await new ResolveAppealCommandHandler(db, FakeUserDirectory.CommitteeMembersAt(InstitutionA, present)).Handle(
            new ResolveAppealCommand(
                reviewId, CommitteeAppealOutcome.Remitted, category, "Re-read on appeal.", null, present, resolver),
            CancellationToken.None);
        db.ChangeTracker.Clear();
        return resolved;
    }

    private static async Task<ClaimsPrincipal> ChairOfAsync(ApplicationDbContext db, int reviewId)
        => await db.CommitteeReviews.AsNoTracking().Where(review => review.Id == reviewId).Select(review => review.PanelId)
               .SingleAsync() == NeonatalPanel
            ? NeonatalChair()
            : GeneralChair();

    private static ClaimsPrincipal GeneralChair() => TestPrincipals.InRole(WombatRoles.CommitteeMember, "chair-a", InstitutionA);

    private static ClaimsPrincipal NeonatalChair() => TestPrincipals.InRole(WombatRoles.CommitteeMember, "chair-n", InstitutionA);

    private static int EpaIdOf(string code) => Epas.Single(epa => epa.Code == code).Id;

    private async Task AssertTheAppealIsStillOpenAsync(ApplicationDbContext db)
    {
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        await using var read = CreateDb();
        (await read.CommitteeReviews.SingleAsync()).State.Should().Be(CommitteeReviewState.UnderAppeal);
        (await read.Set<CommitteeDecision>().CountAsync()).Should().Be(1);
        var appeal = await read.CommitteeAppeals.SingleAsync();
        appeal.ResolvedOn.Should().BeNull();
        appeal.Outcome.Should().BeNull();
    }

    private static async Task SaveAndClearAsAuditPipelineWouldAsync(ApplicationDbContext db)
    {
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    /// <summary>
    /// A ratified review before <paramref name="panelId" />: entrustment-only before the neonatal CCC, with PAED-004
    /// decided and PAED-005 deferred; annual progression before the general panel, with PAED-001 decided.
    /// </summary>
    private async Task<ApplicationDbContext> RatifiedDbAsync(int panelId)
    {
        var db = await SeededDbAsync();
        if (panelId == NeonatalPanel)
        {
            var review = await ScheduleAsync(db, NeonatalPanel, semester: 1, type: null);
            await StartWithEvidenceAsync(db, review.Id);
            await StageAsync(db, review.Id, EpaIdOf("PAED-004"));
            await DeferAsync(db, review.Id, "PAED-005", "The neonatal rotation has not started.");
            await RecordAsync(db, review.Id, NeonatalChair(), category: null, "chair-n", "member-n");
            await RatifyAsync(db, review.Id, NeonatalChair());
        }
        else
        {
            var review = await ScheduleAsync(db, GeneralPanel, semester: 1, CommitteeReviewType.AnnualProgression);
            await StartWithEvidenceAsync(db, review.Id);
            await StageAsync(db, review.Id, EpaIdOf("PAED-001"));
            await RecordAsync(db, review.Id, GeneralChair(), CommitteeDecisionCategory.SatisfactoryProgress, "chair-a", "member-a");
            await RatifyAsync(db, review.Id, GeneralChair());
        }

        return db;
    }

    private async Task<ApplicationDbContext> AppealedDbAsync(int panelId)
    {
        var db = await RatifiedDbAsync(panelId);
        var reviewId = await db.CommitteeReviews.Select(review => review.Id).SingleAsync();
        await new LodgeAppealCommandHandler(db).Handle(
            new LodgeAppealCommand(reviewId, "The level understates my supervised practice.", TestPrincipals.Trainee(Trainee, InstitutionA)),
            CancellationToken.None);
        db.ChangeTracker.Clear();
        return db;
    }

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);

    private async Task<ApplicationDbContext> SeededDbAsync()
    {
        var db = CreateDb();

        db.Institutions.Add(new Institution { Id = InstitutionA, Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Specialities.Add(new Speciality { Id = 1, CollegeId = 1, Name = "Paediatrics", IsActive = true });
        db.SubSpecialities.Add(new SubSpeciality { Id = 11, SpecialityId = 1, Name = "General Paediatrics", IsActive = true });
        db.Curricula.Add(new Curriculum { Id = CurriculumId, SubSpecialityId = 11, Name = "General Paediatrics", Version = "11.1" });
        db.DecisionBodies.Add(new DecisionBody { Key = Neonatal, Name = NeonatalCcc });
        db.EntrustmentScales.Add(new EntrustmentScale { Id = 1, Name = "CPSA ladder" });
        db.EntrustmentLevels.AddRange(Enumerable.Range(1, 5).Select(order =>
            new EntrustmentLevel { Id = order, ScaleId = 1, Order = order, Label = order.ToString(System.Globalization.CultureInfo.InvariantCulture) }));

        foreach (var (code, id) in Epas)
        {
            db.Epas.Add(new Epa { Id = id, SubSpecialityId = 11, Code = code, Title = $"EPA {id}", IsActive = true });
        }

        db.CurriculumItems.AddRange(Item(1, body: null), Item(4, Neonatal), Item(5, Neonatal));

        db.TraineeProfiles.Add(new TraineeProfile
        {
            Id = 1, UserId = Trainee, InstitutionId = InstitutionA, CurriculumId = CurriculumId, IsActive = true,
            ProgrammeStartDate = new DateOnly(2025, 1, 15), ExpectedCompletionDate = new DateOnly(2029, 1, 14)
        });

        db.DecisionPanels.Add(Panel(GeneralPanel, "General CCC", body: null, "chair-a", "member-a"));
        db.DecisionPanels.Add(Panel(NeonatalPanel, "Neonatal CCC", Neonatal, "chair-n", "member-n"));

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private static CurriculumItem Item(int epaId, string? body)
        => new()
        {
            Id = 100 + epaId,
            CurriculumId = CurriculumId,
            EpaId = epaId,
            RequiredCount = 1,
            MinimumLevelOrder = 3,
            DecisionCadence = QuotaPeriod.Semester,
            DecisionBodyKey = body
        };

    private static DecisionPanel Panel(int id, string name, string? body, string chair, string member)
        => new()
        {
            Id = id,
            Name = name,
            Scope = DecisionPanelScope.Institution,
            InstitutionId = InstitutionA,
            DecisionBodyKey = body,
            CreatedOn = DateTime.UtcNow,
            Members =
            [
                new DecisionPanelMember { UserId = chair, Role = DecisionPanelMemberRole.Chair },
                new DecisionPanelMember { UserId = member, Role = DecisionPanelMemberRole.Member }
            ]
        };
}
