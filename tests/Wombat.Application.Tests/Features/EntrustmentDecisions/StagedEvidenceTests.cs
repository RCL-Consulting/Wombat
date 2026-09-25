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

namespace Wombat.Application.Tests.Features.EntrustmentDecisions;

/// <summary>
/// Every staged entrustment decision names the lines of its own review's frozen evidence snapshot it rests on, and
/// ratifying issues no STAR without them (D38, T131 slice 1).
/// </summary>
/// <remarks>
/// <para>
/// The College's words: the decision is taken by the committee "drawing on the standard assessment information sources
/// ... never by a single assessor and never from a single form". Before T131 the page staged every decision with no
/// evidence at all, the handler accepted free-text links, and ratifying copied whatever they said onto the STAR.
/// </para>
/// <para>
/// Every refusal is followed by the save the audit pipeline makes from its catch and a cleared tracker, and the store is
/// read back after it: a check that ran after a mutation would have the refusal commit it.
/// </para>
/// </remarks>
public sealed class StagedEvidenceTests
{
    private const int Institution = 1;
    private const int PanelId = 20;
    private const int OtherPanelId = 21;
    private const int ReviewId = 30;
    private const int OtherReviewId = 31;
    private const int ScheduledReviewId = 32;
    private const int Epa7 = 7;
    private const int Epa8 = 8;
    private const int Level3 = 3;
    private const int Level4 = 4;

    // This review's snapshot.
    private const int MiniCexOnEpa7 = 501;
    private const int CbdOnEpa8 = 502;
    private const int MsfCampaignLine = 503;
    private const int SupervisorReportLine = 504;

    // Another review's snapshot, of the same trainee.
    private const int OtherReviewsLine = 601;

    private static readonly DateTime FrozenRecordedOn = new(2026, 2, 10, 9, 15, 0, DateTimeKind.Utc);

    private readonly string _databaseName = Guid.NewGuid().ToString();

    // ---- The command's own rules -------------------------------------------------------------------------------------

    [Fact]
    public void TheValidator_RefusesADecisionThatNamesNoEvidence_OrNamesAnItemTwice()
    {
        var validator = new StagePendingEntrustmentDecisionCommandValidator();

        var none = validator.Validate(Command([]));
        none.IsValid.Should().BeFalse();
        none.Errors.Select(error => error.ErrorMessage).Should().Contain(StagedEvidence.NoneNamed);

        validator.Validate(Command([MiniCexOnEpa7, MiniCexOnEpa7])).IsValid.Should().BeFalse();
        validator.Validate(Command([0])).IsValid.Should().BeFalse();
        validator.Validate(Command([MiniCexOnEpa7])).IsValid.Should().BeTrue();
    }

    // ---- Stage -------------------------------------------------------------------------------------------------------

    [Fact]
    public async Task Stage_KeepsTheNamedLines()
    {
        await using var db = await SeededDbAsync();

        var staged = await StageAsync(db, Epa7, MiniCexOnEpa7, MsfCampaignLine);

        staged.EvidenceItemIds.Should().Equal(MiniCexOnEpa7, MsfCampaignLine);
    }

    [Fact]
    public async Task Stage_ALineOfAnotherReview_AndAnIdThatNamesNothing_AreRefusedAlike_AndNothingIsWritten()
    {
        await using var db = await SeededDbAsync();

        var another = await RefusalAsync<InvalidOperationException>(() => StageAsync(db, Epa7, MiniCexOnEpa7, OtherReviewsLine));
        var nothing = await RefusalAsync<InvalidOperationException>(() => StageAsync(db, Epa7, 99_999));

        another.Should().Be(StagedEvidence.NotInThisSnapshot);
        nothing.Should().Be(another, "the reply must not say whether an id names another review's line");
        await SaveAndClearAsync(db);
        (await PendingCountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Stage_ASupervisorReportLine_IsRefused_AndNothingIsWritten()
    {
        await using var db = await SeededDbAsync();

        var refusal = await RefusalAsync<InvalidOperationException>(() => StageAsync(db, Epa7, MiniCexOnEpa7, SupervisorReportLine));

        refusal.Should().Be(StagedEvidence.SupervisorReportNamed);
        await SaveAndClearAsync(db);
        (await PendingCountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Stage_NoEvidence_IsRefusedByTheHandlerToo()
    {
        // The validator runs in the pipeline; the handler does not lean on it.
        await using var db = await SeededDbAsync();

        var refusal = await RefusalAsync<InvalidOperationException>(() => StageAsync(db, Epa7));

        refusal.Should().Be(StagedEvidence.NoneNamed);
        await SaveAndClearAsync(db);
        (await PendingCountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Stage_TheSameEpaTwice_IsRefusedByName_AndTheFirstStandsAsItWas()
    {
        await using var db = await SeededDbAsync();
        var first = await StageAsync(db, Epa7, MiniCexOnEpa7);

        var refusal = await RefusalAsync<InvalidOperationException>(() => StageAsync(db, Epa7, CbdOnEpa8));

        refusal.Should().Contain("EPA-07").And.Contain("already staged");
        await SaveAndClearAsync(db);
        await using var read = CreateDb();
        var stored = await read.PendingEntrustmentDecisions.SingleAsync();
        stored.Id.Should().Be(first.Id);
        stored.EvidenceItemIds.Should().Equal(MiniCexOnEpa7);
    }

    [Fact]
    public async Task Stage_EditingTheStagedDecision_ReplacesWhatItRestsOn()
    {
        await using var db = await SeededDbAsync();
        var first = await StageAsync(db, Epa7, MiniCexOnEpa7);

        var edited = await EditAsync(db, Epa7, first.Id, MiniCexOnEpa7, MsfCampaignLine);

        edited.Id.Should().Be(first.Id);
        await using var read = CreateDb();
        (await read.PendingEntrustmentDecisions.SingleAsync()).EvidenceItemIds.Should().Equal(MiniCexOnEpa7, MsfCampaignLine);
    }

    /// <summary>
    /// The edit path loads the staged row tracked, so a refusal after <see cref="PendingEntrustmentDecision.Update" />
    /// would have the audit pipeline's save commit the refused ids. Every check runs before it. (T131 review)
    /// </summary>
    [Fact]
    public async Task Stage_AnEditNamingALineOfAnotherReview_OrASupervisorReport_IsRefused_AndTheStagedDecisionStandsAsItWas()
    {
        await using var db = await SeededDbAsync();
        var first = await StageAsync(db, Epa7, MiniCexOnEpa7);
        db.ChangeTracker.Clear();

        var another = await RefusalAsync<InvalidOperationException>(() => EditAsync(db, Epa7, first.Id, OtherReviewsLine));
        await SaveAndClearAsync(db);
        var report = await RefusalAsync<InvalidOperationException>(() => EditAsync(db, Epa7, first.Id, CbdOnEpa8, SupervisorReportLine));
        await SaveAndClearAsync(db);

        another.Should().Be(StagedEvidence.NotInThisSnapshot);
        report.Should().Be(StagedEvidence.SupervisorReportNamed);
        await using var read = CreateDb();
        var stored = await read.PendingEntrustmentDecisions.SingleAsync();
        stored.EvidenceItemIds.Should().Equal(MiniCexOnEpa7);
        stored.Rationale.Should().Be("Consistent across the window.");
    }

    // ---- Authorise first, one refusal (T194 item 1) ------------------------------------------------------------------

    public static TheoryData<string> ChairCommands => new() { "Stage", "Remove", "Ratify" };

    [Theory]
    [MemberData(nameof(ChairCommands))]
    public async Task AnUnknownReview_AndOneTheCallerDoesNotChair_GetTheOneRefusal_BeforeItsStateIsSaid(string command)
    {
        await using var db = await SeededDbAsync();
        var chairOfAnotherPanel = TestPrincipals.InRole(WombatRoles.CommitteeMember, "chair-other", Institution);

        var notChaired = await RefusalAsync<UnauthorizedAccessException>(() => RunAsync(db, command, ReviewId, chairOfAnotherPanel));
        var notStarted = await RefusalAsync<UnauthorizedAccessException>(() => RunAsync(db, command, ScheduledReviewId, chairOfAnotherPanel));
        var unknown = await RefusalAsync<UnauthorizedAccessException>(() => RunAsync(db, command, 999, chairOfAnotherPanel));

        notChaired.Should().Be("The committee review could not be found among the reviews you chair.");
        notStarted.Should().Be(notChaired, "a scheduled review's state is not told to someone who does not chair it");
        unknown.Should().Be(notChaired, "the reply must not say whether an id names a review");
    }

    [Theory]
    [MemberData(nameof(ChairCommands))]
    public async Task AMemberWhoDoesNotChair_GetsTheOneRefusal_AndTheChairIsNotRefused(string command)
    {
        await using var db = await SeededDbAsync();
        var member = TestPrincipals.InRole(WombatRoles.CommitteeMember, "member-1", Institution);
        if (command is "Remove" or "Ratify")
        {
            await StageAsync(db, Epa7, MiniCexOnEpa7);
        }

        if (command == "Ratify")
        {
            await RecordDecisionAsync(db);
        }

        var refusal = await RefusalAsync<UnauthorizedAccessException>(() => RunAsync(db, command, ReviewId, member));

        refusal.Should().Be("The committee review could not be found among the reviews you chair.");
        await RunAsync(db, command, ReviewId, Chair());
    }

    // ---- Ratify ------------------------------------------------------------------------------------------------------

    [Fact]
    public async Task Ratify_BuildsEachLinkFromTheFrozenLine_AndNamesTheLine()
    {
        await using var db = await SeededDbAsync();
        await StageAsync(db, Epa7, MiniCexOnEpa7, MsfCampaignLine);
        await RecordDecisionAsync(db);

        await RatifyAsync(db);

        await using var read = CreateDb();
        var star = await read.EntrustmentDecisions.Include(decision => decision.EvidenceLinks).SingleAsync();
        star.EvidenceLinks.Should().HaveCount(2);

        var frozenActivity = await read.Set<CommitteeEvidence>().SingleAsync(line => line.Id == MiniCexOnEpa7);
        var activity = star.EvidenceLinks.Single(link => link.CommitteeEvidenceId == MiniCexOnEpa7);
        activity.SourceType.Should().Be(EntrustmentEvidenceSourceType.Activity);
        activity.ActivityId.Should().Be(frozenActivity.ActivityId);
        activity.MsfCampaignId.Should().BeNull();
        activity.SourceLabel.Should().Be(frozenActivity.SourceLabel);
        activity.Summary.Should().Be(frozenActivity.Summary);
        activity.SourceRecordedOn.Should().Be(frozenActivity.SourceRecordedOn);

        var frozenCampaign = await read.Set<CommitteeEvidence>().SingleAsync(line => line.Id == MsfCampaignLine);
        var campaign = star.EvidenceLinks.Single(link => link.CommitteeEvidenceId == MsfCampaignLine);
        campaign.SourceType.Should().Be(EntrustmentEvidenceSourceType.MsfCampaign);
        campaign.MsfCampaignId.Should().Be(frozenCampaign.MsfCampaignId);
        campaign.ActivityId.Should().BeNull();
        campaign.SourceLabel.Should().Be(frozenCampaign.SourceLabel);
        campaign.Summary.Should().Be(frozenCampaign.Summary);

        (await read.PendingEntrustmentDecisions.CountAsync()).Should().Be(0);
    }

    public static TheoryData<string, string> WaysAStagedDecisionRestsOnNothing => new()
    {
        { "names nothing", "[]" },
        { "names a line of another review", $"[{OtherReviewsLine}]" },
        { "cannot be read", "{\"not\":\"ids\"}" }
    };

    /// <summary>
    /// A staged row that rests on no evidence of this review: written before T131, or by hand. Ratifying would issue a
    /// STAR on nothing and supersede the trainee's current one, so it is refused before anything is changed.
    /// </summary>
    [Theory]
    [MemberData(nameof(WaysAStagedDecisionRestsOnNothing))]
    public async Task Ratify_WithAStagedDecisionThatRestsOnNothing_IsRefused_TheReviewStaysDecided_AndNoStarIsIssued(
        string way, string storedIds)
    {
        await using var db = await SeededDbAsync();
        var prior = await AddPriorStarAsync(db, Epa7);
        await StageAsync(db, Epa7, MiniCexOnEpa7);
        var ungrounded = await StageAsync(db, Epa8, CbdOnEpa8);
        await RecordDecisionAsync(db);
        await SetStoredIdsAsync(ungrounded.Id, storedIds);
        db.ChangeTracker.Clear(); // the ratify request reads the store, as a new request's context would

        var refusal = await RefusalAsync<InvalidOperationException>(() => RatifyAsync(db));

        refusal.Should().StartWith("This review cannot be ratified: 1 staged entrustment decision names no evidence", way)
            .And.Contain("EPA-08").And.NotContain("EPA-07");
        // T213: the decision is recorded, so the staged decisions are fixed (D46): the refusal no longer tells the chair to
        // remove it and stage it again, which neither handler allows on a decided review.
        refusal.Should().EndWith(StagedEvidence.UngroundedOnceRecorded, way).And.NotContain("Remove", way);
        await SaveAndClearAsync(db);
        await using var read = CreateDb();
        (await read.CommitteeReviews.SingleAsync(review => review.Id == ReviewId)).State.Should().Be(CommitteeReviewState.Decided);
        (await read.PendingEntrustmentDecisions.CountAsync()).Should().Be(2);
        var stars = await read.EntrustmentDecisions.ToListAsync();
        stars.Should().ContainSingle().Which.Id.Should().Be(prior);
        stars[0].Status.Should().Be(EntrustmentDecisionStatus.Active, "the trainee's current STAR is not superseded");
    }

    /// <summary>
    /// Two staged decisions on one EPA would each supersede the trainee's prior STAR, and the second
    /// <see cref="EntrustmentDecision.SupersedeBy" /> would throw after the first mutation. The table's unique index keeps
    /// them out on PostgreSQL; the check stays for a row that got past it, and EF InMemory, which has no unique index, is
    /// where it can be shown. (T131 review)
    /// </summary>
    [Fact]
    public async Task Ratify_TwoStagedDecisionsOnOneEpa_IsRefused_TheReviewStaysDecided_AndNoStarIsIssued()
    {
        await using var db = await SeededDbAsync();
        var prior = await AddPriorStarAsync(db, Epa7);
        await StageAsync(db, Epa7, MiniCexOnEpa7);
        db.PendingEntrustmentDecisions.Add(PendingEntrustmentDecision.Stage(
            ReviewId, Epa7, Level3, new DateOnly(2026, 7, 2), null, "A second.", [CbdOnEpa8], "chair-1", DateTime.UtcNow));
        await SaveAndClearAsync(db);
        await RecordDecisionAsync(db);
        db.ChangeTracker.Clear();

        var refusal = await RefusalAsync<InvalidOperationException>(() => RatifyAsync(db));

        // T213 review: the decision is recorded, so the staged decisions are fixed (D46) and the chair can remove neither;
        // the refusal says who can look into it, as the no-evidence refusal does.
        refusal.Should().Be(RatifyCommitteeDecisionCommandHandler.TwoStagedOnOneEpa)
            .And.Contain("two entrustment decisions are staged on one EPA")
            .And.EndWith("ask an administrator to look into it.")
            .And.NotContain("Remove");
        await SaveAndClearAsync(db);
        await using var read = CreateDb();
        (await read.CommitteeReviews.SingleAsync(review => review.Id == ReviewId)).State.Should().Be(CommitteeReviewState.Decided);
        (await read.PendingEntrustmentDecisions.CountAsync()).Should().Be(2);
        var stars = await read.EntrustmentDecisions.ToListAsync();
        stars.Should().ContainSingle().Which.Id.Should().Be(prior);
        stars[0].Status.Should().Be(EntrustmentDecisionStatus.Active);
    }

    [Fact]
    public async Task Ratify_IssuesAndSupersedesInOneSave()
    {
        await using var db = await SeededDbAsync();
        var prior = await AddPriorStarAsync(db, Epa7);
        await StageAsync(db, Epa7, MiniCexOnEpa7);
        await RecordDecisionAsync(db);

        await RatifyAsync(db);

        await using var read = CreateDb();
        var stars = await read.EntrustmentDecisions.OrderBy(decision => decision.Id).ToListAsync();
        stars.Should().HaveCount(2);
        stars[0].Id.Should().Be(prior);
        stars[0].Status.Should().Be(EntrustmentDecisionStatus.Superseded);
        stars[0].SupersededByDecisionId.Should().Be(stars[1].Id);
        stars[1].Status.Should().Be(EntrustmentDecisionStatus.Active);
    }

    // ---- The commands ------------------------------------------------------------------------------------------------

    private static StagePendingEntrustmentDecisionCommand Command(IReadOnlyList<int> evidenceItemIds, int? pendingId = null, int epaId = Epa7)
        => new(ReviewId, pendingId, epaId, Level3, new DateOnly(2026, 7, 2), null, "Consistent across the window.",
            evidenceItemIds, Chair());

    private static Task<PendingEntrustmentDecisionDto> StageAsync(ApplicationDbContext db, int epaId, params int[] evidenceItemIds)
        => new StagePendingEntrustmentDecisionCommandHandler(db).Handle(
            Command(evidenceItemIds, epaId: epaId), CancellationToken.None);

    private static Task<PendingEntrustmentDecisionDto> EditAsync(
        ApplicationDbContext db, int epaId, int pendingId, params int[] evidenceItemIds)
        => new StagePendingEntrustmentDecisionCommandHandler(db).Handle(
            Command(evidenceItemIds, pendingId, epaId), CancellationToken.None);

    /// <summary>Records the decision with a quorate sitting: the chair and the panel's other member, both seatable (T165).</summary>
    private static Task RecordDecisionAsync(ApplicationDbContext db)
        => new RecordCommitteeDecisionCommandHandler(db, FakeUserDirectory.CommitteeMembersAt(Institution, "chair-1", "member-1")).Handle(
            new RecordCommitteeDecisionCommand(
                ReviewId, CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, ["chair-1", "member-1"], Chair()),
            CancellationToken.None);

    private static Task RatifyAsync(ApplicationDbContext db)
        => new RatifyCommitteeDecisionCommandHandler(db).Handle(
            new RatifyCommitteeDecisionCommand(ReviewId, Chair()), CancellationToken.None);

    private static async Task RunAsync(ApplicationDbContext db, string command, int reviewId, ClaimsPrincipal principal)
    {
        switch (command)
        {
            case "Stage":
                await new StagePendingEntrustmentDecisionCommandHandler(db).Handle(
                    Command([MiniCexOnEpa7]) with { ReviewId = reviewId, Principal = principal }, CancellationToken.None);
                break;
            case "Remove":
                var pendingId = await db.PendingEntrustmentDecisions.Select(pending => pending.Id).FirstOrDefaultAsync();
                await new RemovePendingEntrustmentDecisionCommandHandler(db).Handle(
                    new RemovePendingEntrustmentDecisionCommand(reviewId, pendingId == 0 ? 1 : pendingId, principal),
                    CancellationToken.None);
                break;
            case "Ratify":
                await new RatifyCommitteeDecisionCommandHandler(db).Handle(
                    new RatifyCommitteeDecisionCommand(reviewId, principal), CancellationToken.None);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(command), command, null);
        }
    }

    private static ClaimsPrincipal Chair() => TestPrincipals.InRole(WombatRoles.CommitteeMember, "chair-1", Institution);

    private static async Task<string> RefusalAsync<TException>(Func<Task> act)
        where TException : Exception
        => (await act.Should().ThrowExactlyAsync<TException>()).Which.Message;

    // ---- The store ---------------------------------------------------------------------------------------------------

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(_databaseName).Options);

    private static async Task SaveAndClearAsync(ApplicationDbContext db)
    {
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    private async Task<int> PendingCountAsync()
    {
        await using var read = CreateDb();
        return await read.PendingEntrustmentDecisions.CountAsync();
    }

    /// <summary>Writes a staged row's ids as a row written before T131, or by hand, would hold them.</summary>
    private async Task SetStoredIdsAsync(int pendingId, string storedIds)
    {
        await using var write = CreateDb();
        var pending = await write.PendingEntrustmentDecisions.SingleAsync(row => row.Id == pendingId);
        write.Entry(pending).Property(row => row.EvidenceItemIdsJson).CurrentValue = storedIds;
        await write.SaveChangesAsync();
    }

    /// <summary>The trainee's current STAR on an EPA, from an earlier sitting, which ratifying supersedes.</summary>
    private async Task<int> AddPriorStarAsync(ApplicationDbContext db, int epaId)
    {
        var star = EntrustmentDecision.Issue(
            "trainee-1", epaId, Level3, new DateOnly(2025, 12, 1), null, OtherReviewId, "chair-1", "Earlier sitting.",
            StarEvidence.One(OtherReviewsLine, 201));
        db.EntrustmentDecisions.Add(star);
        await SaveAndClearAsync(db);
        return star.Id;
    }

    private async Task<ApplicationDbContext> SeededDbAsync()
    {
        var db = CreateDb();
        var now = new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc);

        db.Institutions.Add(new Institution { Id = Institution, Name = "Host", ShortCode = "HST", IsActive = true, CreatedOn = now });
        db.Specialities.Add(new Speciality { Id = 5, CollegeId = 1, Name = "Paediatrics", IsActive = true });
        db.SubSpecialities.Add(new SubSpeciality { Id = 9, SpecialityId = 5, Name = "General paediatrics", IsActive = true });
        db.EntrustmentScales.Add(new EntrustmentScale { Id = 1, Name = "Ladder" });
        db.EntrustmentLevels.AddRange(
            new EntrustmentLevel { Id = Level3, ScaleId = 1, Order = 3, Label = "3a" },
            new EntrustmentLevel { Id = Level4, ScaleId = 1, Order = 4, Label = "4" });
        db.Epas.AddRange(
            new Epa { Id = Epa7, SubSpecialityId = 9, Code = "EPA-07", Title = "Triage", IsActive = true },
            new Epa { Id = Epa8, SubSpecialityId = 9, Code = "EPA-08", Title = "Admission", IsActive = true });
        db.Curricula.Add(new Curriculum { Id = 40, SubSpecialityId = 9, Name = "Paediatrics", Version = "11.1" });
        db.CurriculumItems.AddRange(
            new CurriculumItem { Id = 41, CurriculumId = 40, EpaId = Epa7, RequiredCount = 1, MinimumLevelOrder = 3 },
            new CurriculumItem { Id = 42, CurriculumId = 40, EpaId = Epa8, RequiredCount = 1, MinimumLevelOrder = 3 });
        db.TraineeProfiles.Add(new TraineeProfile
        {
            UserId = "trainee-1", InstitutionId = Institution, CurriculumId = 40, IsActive = true,
            ProgrammeStartDate = new DateOnly(2025, 1, 15), ExpectedCompletionDate = new DateOnly(2029, 1, 14)
        });

        db.DecisionPanels.AddRange(
            new DecisionPanel
            {
                Id = PanelId, Name = "Paediatrics CCC", Scope = DecisionPanelScope.Institution, InstitutionId = Institution,
                CreatedOn = now,
                Members =
                [
                    new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair },
                    new DecisionPanelMember { UserId = "member-1", Role = DecisionPanelMemberRole.Member }
                ]
            },
            new DecisionPanel
            {
                Id = OtherPanelId, Name = "Another CCC", Scope = DecisionPanelScope.Institution, InstitutionId = Institution,
                CreatedOn = now,
                Members = [new DecisionPanelMember { UserId = "chair-other", Role = DecisionPanelMemberRole.Chair }]
            });

        var review = Review(ReviewId);
        review.Start(
            [
                new CommitteeEvidence
                {
                    Id = MiniCexOnEpa7, SourceType = CommitteeEvidenceSourceType.Activity, ActivityId = 101, EpaId = Epa7,
                    EpaCode = "EPA-07", SourceLabel = "Mini-CEX #101", Summary = "State: completed; rated 3a.",
                    SourceRecordedOn = FrozenRecordedOn, ObservedOn = new DateOnly(2026, 2, 10)
                },
                new CommitteeEvidence
                {
                    Id = CbdOnEpa8, SourceType = CommitteeEvidenceSourceType.Activity, ActivityId = 102, EpaId = Epa8,
                    EpaCode = "EPA-08", SourceLabel = "CbD #102", Summary = "State: completed.",
                    SourceRecordedOn = FrozenRecordedOn, ObservedOn = new DateOnly(2026, 3, 1)
                },
                new CommitteeEvidence
                {
                    Id = MsfCampaignLine, SourceType = CommitteeEvidenceSourceType.MsfCampaign, MsfCampaignId = 50,
                    SourceLabel = "Annual MSF #50", Summary = "State: Released; responses 8; closed 2026-04-30.",
                    SourceRecordedOn = new DateTime(2026, 5, 2, 8, 0, 0, DateTimeKind.Utc), SourceState = "Released"
                },
                new CommitteeEvidence
                {
                    Id = SupervisorReportLine, SourceType = CommitteeEvidenceSourceType.SupervisorReport, SupervisorReportId = 3,
                    SourceLabel = "Supervisor report #3", Summary = "Rotation report."
                }
            ],
            "chair-1",
            now);

        var other = Review(OtherReviewId);
        other.Start(
            [
                new CommitteeEvidence
                {
                    Id = OtherReviewsLine, SourceType = CommitteeEvidenceSourceType.Activity, ActivityId = 201, EpaId = Epa7,
                    EpaCode = "EPA-07", SourceLabel = "Mini-CEX #201", Summary = "State: completed."
                }
            ],
            "chair-1",
            now);

        db.CommitteeReviews.AddRange(review, other, Review(ScheduledReviewId));
        await SaveAndClearAsync(db);
        return db;
    }

    private static CommitteeReview Review(int id)
        => new()
        {
            AcademicYear = 2026,
            Semester = 1,
            Id = id,
            PanelId = PanelId,
            TraineeUserId = "trainee-1",
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 6, 30),
            ScheduledOn = new DateOnly(2026, 7, 2)
        };
}
