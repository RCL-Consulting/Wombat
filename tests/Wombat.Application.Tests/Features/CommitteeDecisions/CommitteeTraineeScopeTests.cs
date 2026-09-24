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

namespace Wombat.Application.Tests.Features.CommitteeDecisions;

/// <summary>
/// A committee panel reviews only trainees at its own institution: scheduling checks the trainee and the caller, every
/// command on a scheduled review checks the trainee again, and the scheduling page's picker offers exactly the trainees
/// the scheduling handler accepts. (T182)
/// </summary>
/// <remarks>
/// <para>
/// Before T182 the trainee id on the scheduling form was free text checked against nothing, and only an
/// InstitutionalAdmin's panel was checked at all. A Coordinator at one hospital could put another hospital's trainee
/// before their panel; starting the review then froze that trainee's evidence, and ratifying it superseded their
/// entrustment decisions.
/// </para>
/// <para>
/// A refused command must leave the store as it was even after a save, because the audit pipeline saves the request's
/// DbContext from its catch: a mutation staged before the check would be committed by the refusal itself. Every refusal
/// below is followed by that save and a cleared change tracker, and the store is read back through a second context.
/// </para>
/// </remarks>
public sealed class CommitteeTraineeScopeTests
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;
    private const int Paediatrics = 1;
    private const int Surgery = 2;
    private const int GeneralPaediatrics = 11;
    private const int Neonatology = 12;
    private const int GeneralSurgery = 21;

    private const int PanelA = 10;
    private const int PanelB = 20;

    private const int EpaId = 7;
    private const int LevelId = 3;

    // Trainees. Each is at the institution of their preferred profile (TraineeScopeResolver, T113).
    private const string PaedsAtA = "paeds-a";
    private const string NeonatologyAtA = "neo-a";
    private const string SurgeryAtA = "surgery-a";
    private const string PaedsAtB = "paeds-b";
    private const string MovedFromAToB = "moved-a-to-b";
    private const string LeftA = "left-a";
    private const string NoProfile = "no-profile";

    private static readonly string[] EveryTrainee =
        [PaedsAtA, NeonatologyAtA, SurgeryAtA, PaedsAtB, MovedFromAToB, LeftA, NoProfile];

    private readonly string _databaseName = Guid.NewGuid().ToString();

    // ─── Scheduling ──────────────────────────────────────────────────────────

    public static TheoryData<string> SchedulersOfA => new()
    {
        "Coordinator", "InstitutionalAdmin", "SpecialityAdmin", "SubSpecialityAdmin"
    };

    [Theory]
    [MemberData(nameof(SchedulersOfA))]
    public async Task ASchedulerOfA_CannotPutBsTraineeBeforeAsPanel_AndNothingIsWritten(string role)
    {
        await using var db = await SeededDbAsync();

        var act = () => ScheduleAsync(db, Scheduler(role, InstitutionA), PanelA, PaedsAtB);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await ReviewCountAsync()).Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(SchedulersOfA))]
    public async Task ASchedulerOfA_CannotScheduleOnBsPanel_EvenForBsOwnTrainee(string role)
    {
        await using var db = await SeededDbAsync();

        var act = () => ScheduleAsync(db, Scheduler(role, InstitutionA), PanelB, PaedsAtB);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await ReviewCountAsync()).Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(SchedulersOfA))]
    public async Task ASchedulerOfA_CanScheduleAsOwnPaediatricTrainee(string role)
    {
        await using var db = await SeededDbAsync();

        var review = await ScheduleAsync(db, Scheduler(role, InstitutionA), PanelA, PaedsAtA);

        review.TraineeUserId.Should().Be(PaedsAtA);
        (await ReviewCountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task AnUnknownTrainee_ATraineeWithNoProfile_AndAnUnknownPanel_AreRefusedExactlyAsAnotherInstitutionsTraineeIs()
    {
        // The refusal is printed on the page. "Could not be found" for one and a scope refusal for another would let a
        // Coordinator walk ids and learn who trains where, and which panels exist.
        await using var db = await SeededDbAsync();
        var coordinator = TestPrincipals.Coordinator(InstitutionA);

        var outOfScope = await RefusalAsync(() => ScheduleAsync(db, coordinator, PanelA, PaedsAtB));
        var unknown = await RefusalAsync(() => ScheduleAsync(db, coordinator, PanelA, "nobody-by-this-id"));
        var noProfile = await RefusalAsync(() => ScheduleAsync(db, coordinator, PanelA, NoProfile));
        var unknownPanel = await RefusalAsync(() => ScheduleAsync(db, coordinator, 999, PaedsAtA));
        var otherPanel = await RefusalAsync(() => ScheduleAsync(db, coordinator, PanelB, PaedsAtA));

        outOfScope.Should().BeOfType<UnauthorizedAccessException>();
        foreach (var refusal in new[] { unknown, noProfile, unknownPanel, otherPanel })
        {
            refusal.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(outOfScope.Message);
        }

        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await ReviewCountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ASpecialityOrSubSpecialityAdmin_SchedulesOnlyTheTraineesTheyOversee()
    {
        // T113's overseer rule, not a new one: a speciality id is national, so it counts only together with the
        // institution, and it reaches only its own trainees.
        await using var db = await SeededDbAsync();
        var paediatricsAdmin = Scheduler("SpecialityAdmin", InstitutionA);
        var generalPaediatricsAdmin = Scheduler("SubSpecialityAdmin", InstitutionA);

        (await AcceptsAsync(db, paediatricsAdmin, PanelA, NeonatologyAtA)).Should().BeTrue();
        (await AcceptsAsync(db, paediatricsAdmin, PanelA, SurgeryAtA)).Should().BeFalse();
        (await AcceptsAsync(db, generalPaediatricsAdmin, PanelA, NeonatologyAtA)).Should().BeFalse();
        (await AcceptsAsync(db, TestPrincipals.Coordinator(InstitutionA), PanelA, SurgeryAtA)).Should().BeTrue();
    }

    [Fact]
    public async Task ATraineeWhoMovedToB_IsBsToSchedule_AndOneWhoLeftA_IsStillAs()
    {
        await using var db = await SeededDbAsync();

        (await AcceptsAsync(db, TestPrincipals.Coordinator(InstitutionA), PanelA, MovedFromAToB)).Should().BeFalse();
        (await AcceptsAsync(db, TestPrincipals.Coordinator(InstitutionB), PanelB, MovedFromAToB)).Should().BeTrue();
        (await AcceptsAsync(db, TestPrincipals.Coordinator(InstitutionA), PanelA, LeftA)).Should().BeTrue();
    }

    [Fact]
    public async Task AnAdministrator_PutsBeforeAPanel_OnlyATraineeAtItsInstitution_AndNothingElseIsWritten()
    {
        // A panel's members may act only on reviews of their own institution's trainees, so a review of anyone else
        // is one no member could run, and ratified by an Administrator it would still supersede the other
        // institution's decisions. The Administrator is waived only the overseer half.
        await using var db = await SeededDbAsync();
        var administrator = TestPrincipals.Administrator();

        (await AcceptsAsync(db, administrator, PanelA, PaedsAtA)).Should().BeTrue();
        (await AcceptsAsync(db, administrator, PanelA, SurgeryAtA)).Should().BeTrue("an Administrator oversees everyone");
        (await AcceptsAsync(db, administrator, PanelB, MovedFromAToB)).Should().BeTrue();

        foreach (var (panelId, traineeUserId) in new[]
                 {
                     (PanelA, PaedsAtB), (PanelA, MovedFromAToB), (PanelB, PaedsAtA), (PanelA, NoProfile),
                     (PanelA, "nobody-by-this-id")
                 })
        {
            var refusal = await RefusalAsync(() => ScheduleAsync(db, administrator, panelId, traineeUserId));

            refusal.Should().BeOfType<UnauthorizedAccessException>()
                .Which.Message.Should().Be(
                    "A panel reviews only trainees at its own institution, and this trainee does not train there.",
                    $"panel {panelId}, {traineeUserId}");
        }

        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await ReviewCountAsync()).Should().Be(3, "only the three accepted reviews were written");
    }

    [Fact]
    public async Task ASpecialityAdminWhoAlsoSitsOnTheCommittee_SchedulesOnlyTheirOwnSpecialitysTrainees()
    {
        // The CommitteeMember arm of the overseer rule reaches every trainee at the institution, but that role
        // schedules nothing. Someone holding both must not take the reach from one role and the right from the other.
        await using var db = await SeededDbAsync();
        var paediatricsAdminOnTheCommittee = AlsoCommitteeMember(Scheduler("SpecialityAdmin", InstitutionA));
        var generalPaediatricsAdminOnTheCommittee = AlsoCommitteeMember(Scheduler("SubSpecialityAdmin", InstitutionA));

        (await AcceptsAsync(db, paediatricsAdminOnTheCommittee, PanelA, PaedsAtA)).Should().BeTrue();
        (await AcceptsAsync(db, paediatricsAdminOnTheCommittee, PanelA, SurgeryAtA)).Should().BeFalse();
        (await AcceptsAsync(db, generalPaediatricsAdminOnTheCommittee, PanelA, PaedsAtA)).Should().BeTrue();
        (await AcceptsAsync(db, generalPaediatricsAdminOnTheCommittee, PanelA, NeonatologyAtA)).Should().BeFalse();
    }

    [Theory]
    [InlineData("SpecialityAdmin")]
    [InlineData("SubSpecialityAdmin")]
    public async Task APanelTheSchedulerCreatedThemselves_TakesTheTraineesTheyOversee(string role)
    {
        // The flow the institution rule would otherwise break: a SpecialityAdmin's panel used to carry no institution,
        // so once a panel reviewed only its own institution's trainees, it reviewed nobody's. It is stamped at create.
        await using var db = await SeededDbAsync();
        var scheduler = Scheduler(role, InstitutionA);
        var panel = await new CreateDecisionPanelCommandHandler(db).Handle(
            new CreateDecisionPanelCommand(
                "Paediatrics annual review",
                DecisionPanelScope.Speciality,
                InstitutionId: null,
                SpecialityId: Paediatrics,
                [new DecisionPanelMemberInput("chair-a", DecisionPanelMemberRole.Chair)],
                scheduler),
            CancellationToken.None);
        db.ChangeTracker.Clear();

        var offered = await PickerAsync(db, scheduler, panel.Id);
        var review = await ScheduleAsync(db, scheduler, panel.Id, PaedsAtA);

        offered.Select(trainee => trainee.UserId).Should().Contain(PaedsAtA).And.NotContain(SurgeryAtA);
        review.PanelId.Should().Be(panel.Id);
    }

    [Fact]
    public async Task ACommitteeMember_DoesNotScheduleReviews_EvenOfTheirOwnInstitutionsTrainee()
    {
        await using var db = await SeededDbAsync();

        (await AcceptsAsync(db, Member("member-a", InstitutionA), PanelA, PaedsAtA)).Should().BeFalse();
    }

    // ─── The picker is the gate ──────────────────────────────────────────────

    public static TheoryData<string, int> PickerCases()
    {
        var cases = new TheoryData<string, int>();
        foreach (var caller in new[]
                 {
                     "Coordinator of A", "InstitutionalAdmin of A", "SpecialityAdmin of A", "SubSpecialityAdmin of A",
                     "Coordinator of B", "CommitteeMember of A", "Trainee of A", "SpecialityAdmin on the committee of A",
                     "Administrator"
                 })
        {
            foreach (var panel in new[] { PanelA, PanelB, 999 })
            {
                cases.Add(caller, panel);
            }
        }

        return cases;
    }

    [Theory]
    [MemberData(nameof(PickerCases))]
    public async Task ThePicker_OffersExactlyTheTraineesTheHandlerAccepts(string caller, int panelId)
    {
        await using var db = await SeededDbAsync();
        var principal = Caller(caller);

        var offered = await PickerAsync(db, principal, panelId);

        var accepted = new List<string>();
        foreach (var traineeUserId in EveryTrainee)
        {
            if (await AcceptsAsync(db, principal, panelId, traineeUserId))
            {
                accepted.Add(traineeUserId);
            }
        }

        offered.Select(trainee => trainee.UserId).Should().BeEquivalentTo(accepted, $"{caller} on panel {panelId}");
    }

    [Fact]
    public async Task ThePicker_ForACoordinatorOfA_OffersAsTraineesByName()
    {
        // The case that makes the equivalence above worth having: without it an empty picker would pass it.
        await using var db = await SeededDbAsync();

        var offered = await PickerAsync(db, TestPrincipals.Coordinator(InstitutionA), PanelA);

        offered.Should().Equal(
            new SchedulableTraineeDto(LeftA, "Lindiwe Left"),
            new SchedulableTraineeDto(NeonatologyAtA, "Nandi Neonatal"),
            new SchedulableTraineeDto(PaedsAtA, "Palesa Paeds"),
            new SchedulableTraineeDto(SurgeryAtA, "Sipho Surgery"));
    }

    [Fact]
    public async Task ThePicker_ForAnAdministrator_OffersThePanelsOwnInstitutionsTrainees()
    {
        // An Administrator oversees every trainee, but a panel still reviews only its own institution's.
        await using var db = await SeededDbAsync();

        var offered = await PickerAsync(db, TestPrincipals.Administrator(), PanelB);

        offered.Select(trainee => trainee.UserId).Should().BeEquivalentTo(PaedsAtB, MovedFromAToB);
        foreach (var trainee in offered)
        {
            (await AcceptsAsync(db, TestPrincipals.Administrator(), PanelB, trainee.UserId)).Should().BeTrue();
        }
    }

    // ─── Acting on a scheduled review ────────────────────────────────────────

    public static TheoryData<string> ReviewCommands => new()
    {
        "Start", "Record", "Ratify", "Close", "ResolveAppeal", "Stage", "Remove", "Issue"
    };

    public static TheoryData<string, string> ReviewCommandsForTraineesNotAtThePanel()
    {
        var cases = new TheoryData<string, string>();
        // Not ResolveAppeal: an appeal body answers an appeal against its own ratified review wherever the trainee now
        // trains (TheAppealBody_AnswersTheAppealOfATraineeWhoHasMovedAway).
        foreach (var command in new[] { "Start", "Record", "Ratify", "Close", "Stage", "Remove", "Issue" })
        {
            foreach (var trainee in new[] { PaedsAtB, MovedFromAToB, NoProfile })
            {
                cases.Add(command, trainee);
            }
        }

        return cases;
    }

    [Theory]
    [MemberData(nameof(ReviewCommandsForTraineesNotAtThePanel))]
    public async Task APanelOfA_CannotActOnAReviewOfATraineeNotAtA_AndNothingChanges(string command, string traineeUserId)
    {
        // A review written before T182, or one whose trainee has since moved: the scheduling check alone would leave
        // it as a way in.
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewForAsync(db, command, traineeUserId);
        var before = await SnapshotAsync();

        var act = () => RunAsync(db, command, reviewId, traineeUserId, ActorFor(command));

        (await act.Should().ThrowAsync<UnauthorizedAccessException>())
            .Which.Message.Should().Contain("panel's institution");
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await SnapshotAsync()).Should().BeEquivalentTo(before);
    }

    [Theory]
    [MemberData(nameof(ReviewCommands))]
    public async Task APanelOfA_ActsOnAReviewOfAsOwnTrainee(string command)
    {
        // The control: the same fixture, the same actor, a trainee at the panel's institution. Without it the refusals
        // above could be the fixture failing rather than the check.
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewForAsync(db, command, PaedsAtA);
        var before = await SnapshotAsync();

        await RunAsync(db, command, reviewId, PaedsAtA, ActorFor(command));

        (await SnapshotAsync()).Should().NotBeEquivalentTo(before, "the command did its work");
    }

    [Theory]
    [MemberData(nameof(ReviewCommands))]
    public async Task AnAdministrator_IsNotRefused_AReviewOfATraineeNotAtThePanel(string command)
    {
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewForAsync(db, command, PaedsAtB);

        var act = () => RunAsync(db, command, reviewId, PaedsAtB, TestPrincipals.Administrator());

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task TheAppealBody_AnswersTheAppealOfATraineeWhoHasMovedAway()
    {
        // Lodging is unchecked because an appeal is the trainee's recourse; resolving it must be too, or a trainee who
        // moves after ratification is left with an appeal only an Administrator could answer. It reads no evidence and
        // supersedes no entrustment decision: it records the panel's answer on the panel's own review.
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewForAsync(db, "ResolveAppeal", MovedFromAToB);

        await RunAsync(db, "ResolveAppeal", reviewId, MovedFromAToB, ActorFor("ResolveAppeal"));

        await using var read = CreateDb();
        (await read.CommitteeReviews.SingleAsync(review => review.Id == reviewId)).State
            .Should().Be(CommitteeReviewState.Final);
        (await read.Set<CommitteeAppeal>().SingleAsync()).Outcome.Should().Be(CommitteeAppealOutcome.Dismissed);
    }

    [Fact]
    public async Task ARefusedRatify_IssuesNothing_AndSupersedesNothing()
    {
        // The consequence the task names: a ratified cross-institution review would supersede the trainee's current
        // entrustment decision. The staged decision stays staged and the current one stays current.
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewForAsync(db, "Ratify", PaedsAtB);

        var act = () => RunAsync(db, "Ratify", reviewId, PaedsAtB, ActorFor("Ratify"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        await SaveAndClearAsAuditPipelineWouldAsync(db);

        await using var read = CreateDb();
        (await read.Set<PendingEntrustmentDecision>().CountAsync(pending => pending.ReviewId == reviewId)).Should().Be(1);
        (await read.Set<EntrustmentDecision>().Where(decision => decision.TraineeUserId == PaedsAtB)
                .Select(decision => decision.Status).ToListAsync())
            .Should().Equal(EntrustmentDecisionStatus.Active);
    }

    // ─── The commands ────────────────────────────────────────────────────────

    private async Task<CommitteeReviewListItemDto> ScheduleAsync(
        ApplicationDbContext db, ClaimsPrincipal principal, int panelId, string traineeUserId)
        => await new ScheduleCommitteeReviewCommandHandler(db).Handle(
            new ScheduleCommitteeReviewCommand(
                traineeUserId,
                panelId,
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 12, 31),
                new DateOnly(2027, 1, 8),
                principal),
            CancellationToken.None);

    /// <summary>Whether the scheduling handler takes this trainee; a refusal is asserted to leave nothing behind.</summary>
    private async Task<bool> AcceptsAsync(ApplicationDbContext db, ClaimsPrincipal principal, int panelId, string traineeUserId)
    {
        var reviewsBefore = await ReviewCountAsync();
        try
        {
            await ScheduleAsync(db, principal, panelId, traineeUserId);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            await SaveAndClearAsAuditPipelineWouldAsync(db);
            (await ReviewCountAsync()).Should().Be(reviewsBefore, "a refused schedule writes nothing");
            return false;
        }
        catch (InvalidOperationException) when (principal.IsInRole(WombatRoles.Administrator) && panelId == 999)
        {
            return false;
        }
    }

    private static async Task<IReadOnlyList<SchedulableTraineeDto>> PickerAsync(
        ApplicationDbContext db, ClaimsPrincipal principal, int panelId)
        => await new ListSchedulableTraineesQueryHandler(db, new NamedUsers()).Handle(
            new ListSchedulableTraineesQuery(panelId, principal),
            CancellationToken.None);

    private static async Task RunAsync(
        ApplicationDbContext db, string command, int reviewId, string traineeUserId, ClaimsPrincipal principal)
    {
        _ = command switch
        {
            "Start" => await new StartCommitteeReviewCommandHandler(db).Handle(
                new StartCommitteeReviewCommand(reviewId, principal), CancellationToken.None),
            "Record" => await new RecordCommitteeDecisionCommandHandler(db).Handle(
                new RecordCommitteeDecisionCommand(reviewId, CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, principal),
                CancellationToken.None),
            "Ratify" => await new RatifyCommitteeDecisionCommandHandler(db).Handle(
                new RatifyCommitteeDecisionCommand(reviewId, principal), CancellationToken.None),
            "Close" => await new CloseFormativeReviewCommandHandler(db).Handle(
                new CloseFormativeReviewCommand(reviewId, principal), CancellationToken.None),
            "ResolveAppeal" => await new ResolveAppealCommandHandler(db).Handle(
                new ResolveAppealCommand(reviewId, CommitteeAppealOutcome.Dismissed, null, null, null, principal),
                CancellationToken.None),
            "Stage" => await new StagePendingEntrustmentDecisionCommandHandler(db).Handle(
                new StagePendingEntrustmentDecisionCommand(
                    reviewId, null, EpaId, LevelId, new DateOnly(2027, 1, 8), null, "Ready for indirect supervision.", [], principal),
                CancellationToken.None),
            "Remove" => (object)await new RemovePendingEntrustmentDecisionCommandHandler(db).Handle(
                new RemovePendingEntrustmentDecisionCommand(reviewId, await PendingIdAsync(db, reviewId), principal),
                CancellationToken.None),
            "Issue" => await new IssueEntrustmentDecisionCommandHandler(db).Handle(
                new IssueEntrustmentDecisionCommand(
                    traineeUserId, EpaId, LevelId, new DateOnly(2027, 1, 8), null, reviewId, "Ready.", [], principal),
                CancellationToken.None),
            _ => throw new ArgumentOutOfRangeException(nameof(command), command, null)
        };
    }

    /// <summary>The panel member each command admits: the chair for all but Start, which any member may do.</summary>
    private static ClaimsPrincipal ActorFor(string command)
        => command == "Start" ? Member("member-a", InstitutionA) : Member("chair-a", InstitutionA);

    private static async Task<int> PendingIdAsync(ApplicationDbContext db, int reviewId)
        => await db.Set<PendingEntrustmentDecision>()
            .Where(pending => pending.ReviewId == reviewId)
            .Select(pending => pending.Id)
            .SingleAsync();

    // ─── Fixture ─────────────────────────────────────────────────────────────

    /// <summary>A review on panel A in the state the command acts on, with what the command reads.</summary>
    private async Task<int> SeedReviewForAsync(ApplicationDbContext db, string command, string traineeUserId)
    {
        var now = new DateTime(2027, 1, 8, 9, 0, 0, DateTimeKind.Utc);
        var review = new CommitteeReview
        {
            TraineeUserId = traineeUserId,
            PanelId = PanelA,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 12, 31),
            ScheduledOn = new DateOnly(2027, 1, 8),
            IsFormative = command == "Close"
        };

        if (command != "Start")
        {
            review.Start([], "chair-a", now);
        }

        if (command is "Ratify" or "ResolveAppeal" or "Issue")
        {
            review.RecordDecision(CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, "chair-a", now);
        }

        if (command is "ResolveAppeal" or "Issue")
        {
            review.Ratify("chair-a", now);
        }

        if (command == "ResolveAppeal")
        {
            review.LodgeAppeal("The window missed my rotation.", traineeUserId, now);
        }

        db.CommitteeReviews.Add(review);
        await db.SaveChangesAsync();

        if (command is "Ratify" or "Remove")
        {
            db.Set<PendingEntrustmentDecision>().Add(PendingEntrustmentDecision.Stage(
                review.Id, EpaId, LevelId, new DateOnly(2027, 1, 8), null, "Ready.", "[]", "chair-a", now));
        }

        if (command == "Ratify")
        {
            // The trainee's current decision on the EPA, which a ratified decision would supersede.
            db.Set<EntrustmentDecision>().Add(EntrustmentDecision.Issue(
                traineeUserId, EpaId, 2, new DateOnly(2026, 1, 8), null, review.Id, "chair-a", "Earlier.", []));
        }

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return review.Id;
    }

    private async Task<ApplicationDbContext> SeededDbAsync()
    {
        var db = CreateDb();

        db.Institutions.AddRange(
            new Institution { Id = InstitutionA, Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = InstitutionB, Name = "B", ShortCode = "B", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Specialities.AddRange(
            new Speciality { Id = Paediatrics, CollegeId = 1, Name = "Paediatrics", IsActive = true },
            new Speciality { Id = Surgery, CollegeId = 1, Name = "Surgery", IsActive = true });
        db.SubSpecialities.AddRange(
            new SubSpeciality { Id = GeneralPaediatrics, SpecialityId = Paediatrics, Name = "General Paediatrics", IsActive = true },
            new SubSpeciality { Id = Neonatology, SpecialityId = Paediatrics, Name = "Neonatology", IsActive = true },
            new SubSpeciality { Id = GeneralSurgery, SpecialityId = Surgery, Name = "General Surgery", IsActive = true });
        db.Curricula.AddRange(
            new Curriculum { Id = 100, SubSpecialityId = GeneralPaediatrics, Name = "General Paediatrics", Version = "11.1" },
            new Curriculum { Id = 101, SubSpecialityId = Neonatology, Name = "Neonatology", Version = "1" },
            new Curriculum { Id = 200, SubSpecialityId = GeneralSurgery, Name = "General Surgery", Version = "1" });

        AddProfile(db, 1, PaedsAtA, InstitutionA, 100, isActive: true);
        AddProfile(db, 2, NeonatologyAtA, InstitutionA, 101, isActive: true);
        AddProfile(db, 3, SurgeryAtA, InstitutionA, 200, isActive: true);
        AddProfile(db, 4, PaedsAtB, InstitutionB, 100, isActive: true);
        AddProfile(db, 5, MovedFromAToB, InstitutionA, 100, isActive: false);
        AddProfile(db, 6, MovedFromAToB, InstitutionB, 100, isActive: true);
        AddProfile(db, 7, LeftA, InstitutionA, 100, isActive: false);

        db.DecisionPanels.AddRange(
            Panel(PanelA, InstitutionA, "chair-a", "member-a", "external-a"),
            Panel(PanelB, InstitutionB, "chair-b", "member-b", "external-b"));

        db.EntrustmentScales.Add(new EntrustmentScale { Id = 1, Name = "v11.1 rungs" });
        db.EntrustmentLevels.AddRange(
            new EntrustmentLevel { Id = 2, ScaleId = 1, Order = 2, Label = "Direct supervision" },
            new EntrustmentLevel { Id = LevelId, ScaleId = 1, Order = 3, Label = "Indirect supervision" });
        db.Epas.Add(new Epa { Id = EpaId, SubSpecialityId = GeneralPaediatrics, Code = "PAED-007", Title = "Triage", IsActive = true });
        // A STAR is granted only on an EPA of the trainee's curriculum (T167), so the General Paediatrics curriculum
        // every Stage, Issue and Ratify row's trainee follows holds the EPA they stage.
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 1000, CurriculumId = 100, EpaId = EpaId, RequiredCount = 1, MinimumLevelOrder = 3
        });

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private static DecisionPanel Panel(int id, int institutionId, string chair, string member, string external)
        => new()
        {
            Id = id,
            Name = $"Panel {id}",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = institutionId,
            CreatedOn = DateTime.UtcNow,
            Members =
            [
                new DecisionPanelMember { UserId = chair, Role = DecisionPanelMemberRole.Chair },
                new DecisionPanelMember { UserId = member, Role = DecisionPanelMemberRole.Member },
                new DecisionPanelMember { UserId = external, Role = DecisionPanelMemberRole.External }
            ]
        };

    private static void AddProfile(ApplicationDbContext db, int id, string userId, int institutionId, int curriculumId, bool isActive)
        => db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = id,
            UserId = userId,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            ProgrammeStartDate = new DateOnly(2024, 1, 15),
            ExpectedCompletionDate = new DateOnly(2028, 1, 15),
            IsActive = isActive
        });

    private static ClaimsPrincipal Scheduler(string role, int institutionId) => role switch
    {
        "Coordinator" => TestPrincipals.Coordinator(institutionId),
        "InstitutionalAdmin" => TestPrincipals.InstitutionalAdmin(institutionId),
        "SpecialityAdmin" => TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "spec-admin", institutionId, specialityId: Paediatrics),
        "SubSpecialityAdmin" => TestPrincipals.InRole(
            WombatRoles.SubSpecialityAdmin, "sub-admin", institutionId, subSpecialityId: GeneralPaediatrics),
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
    };

    private static ClaimsPrincipal Caller(string caller) => caller switch
    {
        "Coordinator of A" => Scheduler("Coordinator", InstitutionA),
        "InstitutionalAdmin of A" => Scheduler("InstitutionalAdmin", InstitutionA),
        "SpecialityAdmin of A" => Scheduler("SpecialityAdmin", InstitutionA),
        "SubSpecialityAdmin of A" => Scheduler("SubSpecialityAdmin", InstitutionA),
        "Coordinator of B" => Scheduler("Coordinator", InstitutionB),
        "CommitteeMember of A" => Member("member-a", InstitutionA),
        "Trainee of A" => TestPrincipals.Trainee(PaedsAtA, InstitutionA),
        "SpecialityAdmin on the committee of A" => AlsoCommitteeMember(Scheduler("SpecialityAdmin", InstitutionA)),
        "Administrator" => TestPrincipals.Administrator(),
        _ => throw new ArgumentOutOfRangeException(nameof(caller), caller, null)
    };

    /// <summary>The same person, sitting on the committee as well: the two-role case of the overseer rule.</summary>
    private static ClaimsPrincipal AlsoCommitteeMember(ClaimsPrincipal principal)
    {
        var identity = new ClaimsIdentity(principal.Claims, "test");
        identity.AddClaim(new Claim(ClaimTypes.Role, WombatRoles.CommitteeMember));
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal Member(string userId, int institutionId)
        => TestPrincipals.InRole(WombatRoles.CommitteeMember, userId, institutionId);

    // ─── The store ───────────────────────────────────────────────────────────

    private static async Task SaveAndClearAsAuditPipelineWouldAsync(ApplicationDbContext db)
    {
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    private async Task<int> ReviewCountAsync()
    {
        await using var read = CreateDb();
        return await read.CommitteeReviews.CountAsync();
    }

    private sealed record StoreSnapshot(
        IReadOnlyList<string> Reviews,
        int Evidence,
        IReadOnlyList<string> Decisions,
        IReadOnlyList<string> Appeals,
        IReadOnlyList<string> Pending,
        IReadOnlyList<string> EntrustmentDecisions);

    private async Task<StoreSnapshot> SnapshotAsync()
    {
        await using var read = CreateDb();
        return new StoreSnapshot(
            await read.CommitteeReviews.OrderBy(review => review.Id)
                .Select(review => $"{review.Id}:{review.State}:{review.StartedOn}:{review.RatifiedOn}:{review.FinalizedOn}")
                .ToListAsync(),
            await read.Set<CommitteeEvidence>().CountAsync(),
            await read.Set<CommitteeDecision>().OrderBy(decision => decision.Id)
                .Select(decision => $"{decision.Id}:{decision.Category}")
                .ToListAsync(),
            await read.Set<CommitteeAppeal>().OrderBy(appeal => appeal.Id)
                .Select(appeal => $"{appeal.Id}:{appeal.ResolvedOn}:{appeal.Outcome}")
                .ToListAsync(),
            await read.Set<PendingEntrustmentDecision>().OrderBy(pending => pending.Id)
                .Select(pending => $"{pending.Id}:{pending.Rationale}")
                .ToListAsync(),
            await read.Set<EntrustmentDecision>().OrderBy(decision => decision.Id)
                .Select(decision => $"{decision.Id}:{decision.Status}:{decision.SupersededByDecisionId}")
                .ToListAsync());
    }

    private static async Task<Exception> RefusalAsync(Func<Task> act)
    {
        try
        {
            await act();
        }
        catch (Exception exception)
        {
            return exception;
        }

        throw new InvalidOperationException("Expected a refusal.");
    }

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);

    /// <summary>Names every trainee in the fixture, and no one else.</summary>
    private sealed class NamedUsers : IUserAdministrationService
    {
        private static readonly UserIdentityDetails[] Users =
        [
            User(PaedsAtA, "Palesa", "Paeds", InstitutionA),
            User(NeonatologyAtA, "Nandi", "Neonatal", InstitutionA),
            User(SurgeryAtA, "Sipho", "Surgery", InstitutionA),
            User(PaedsAtB, "Bongani", "Paeds", InstitutionB),
            User(MovedFromAToB, "Mpho", "Moved", InstitutionB),
            User(LeftA, "Lindiwe", "Left", InstitutionA),
            User(NoProfile, "Noma", "Profile", InstitutionA)
        ];

        private static UserIdentityDetails User(string userId, string first, string last, int institutionId)
            => new(userId, $"{userId}@test", first, last, institutionId, [], [], [WombatRoles.Trainee]);

        public Task<IReadOnlyList<UserIdentityDetails>> ListAllUsersAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UserIdentityDetails>>(Users);

        public Task<UserIdentityDetails?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
            => Task.FromResult(Users.FirstOrDefault(user => user.UserId == userId));

        public Task<IReadOnlyList<UserIdentityDetails>> ListUsersInRoleAsync(string role, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public Task UpdateNamesAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public Task UpdateScopeAsync(string userId, int institutionId, IReadOnlyCollection<int> specialityIds, IReadOnlyCollection<int> subSpecialityIds, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public Task PromotePendingTraineeAsync(string userId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public Task AddRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public Task RemoveRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public Task ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public Task SetLockoutAsync(string userId, bool locked, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
