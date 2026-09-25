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
/// The chair takes the chair's actions only from a seat they may sit in now: an active committee member at the panel's
/// institution who is not a trainee, and never the trainee under review (<c>PanelSeat.SittingAt</c>, T237's one rule).
/// Every chair's action demands it (<c>CommitteeDecisionAuthorization.DemandChairedReviewAsync</c>), and the review page
/// reads the same predicate (<c>CommitteeReviewDetailDto.CallerChairs</c>) and says why when it fails
/// (<c>CommitteeReviewDetailDto.ChairCannotAct</c>). (T256, D46)
/// </summary>
/// <remarks>
/// <para>
/// Until T256 the chair's gate read the caller's claims alone, and a circuit's claims are frozen for its life: a chair who
/// had lost the CommitteeMember role, moved to another institution or been deactivated kept every chair's action that
/// records no attendance. Ratify issued the staged STARs as the committee's. Recording refused them only because it
/// records the chair as present.
/// </para>
/// <para>
/// Each chair here signs in as they did before the change: their claims still say CommitteeMember at the panel's
/// institution, and only the user store says they may not sit. Each refused request is followed by the save the audit
/// pipeline makes from its catch and a cleared change tracker, and the store is read back through a second context: a
/// check that ran after a mutation would have the refusal commit it. The control runs the same request past a chair who
/// may sit, and reads back what the action wrote: so each refusal is the gate's, not a fixture the action could not run on.
/// </para>
/// <para>
/// Since T279 a seat admits its holder to the review itself only while they may sit at it, so a chair who may not reads
/// it only through another rung of the read ladder. The seat's own refusal and the page's note are said to such a chair
/// who also coordinates at the panel's institution; a chair who reads it no other way is given the one refusal.
/// </para>
/// </remarks>
public sealed class ChairMaySitTests
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;
    private const int Paediatrics = 1;
    private const int GeneralPaediatrics = 11;
    private const int PanelId = 10;
    private const int EpaId = 7;
    private const int LevelId = 3;

    private const string ChairUserId = "chair-a";
    private const string ChairName = "Thandi Zulu";
    private const string TraineeUserId = "paeds-a";

    private readonly string _databaseName = Guid.NewGuid().ToString();

    /// <summary>Every chair's action: the requests <c>DemandChairedReviewAsync</c> gates.</summary>
    public static TheoryData<string> ChairsActions => new()
    {
        "Record", "Ratify", "Close", "Stage", "Remove", "Defer", "Reinstate"
    };

    /// <summary>Why the chair may not sit at the review now, as the user store says it.</summary>
    public static TheoryData<string> ChairsWhoMayNotSit => new()
    {
        "is no longer a committee member",
        "moved to another institution",
        "was deactivated",
        "was given the Trainee role since signing in",
        "was erased: the id names no account"
    };

    public static TheoryData<string, string> EveryActionOfEveryChairWhoMayNotSit
    {
        get
        {
            var data = new TheoryData<string, string>();
            foreach (var action in new[] { "Record", "Ratify", "Close", "Stage", "Remove", "Defer", "Reinstate" })
            {
                foreach (var condition in new[]
                         {
                             "is no longer a committee member",
                             "moved to another institution",
                             "was deactivated",
                             "was given the Trainee role since signing in",
                             "was erased: the id names no account"
                         })
                {
                    data.Add(action, condition);
                }
            }

            return data;
        }
    }

    // ─── The chair's actions ─────────────────────────────────────────────────

    [Theory]
    [MemberData(nameof(EveryActionOfEveryChairWhoMayNotSit))]
    public async Task AChairWhoMayNotSitNow_ButReadsTheReviewAsACoordinator_IsRefusedTheAction_WithTheSeatsRefusal_AndNothingIsWritten(
        string action, string condition)
    {
        // The seat's own sentence is said only to a chair who may still read the review, here through the coordinator's
        // rung (T279): it tells them nothing the review page does not.
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewForAsync(db, action);
        var before = await SnapshotAsync();
        var because = $"{action}: the chair {condition}";

        var act = () => RunAsync(db, action, reviewId, DirectoryWhereTheChair(condition), ChairWhoAlsoCoordinates());

        (await act.Should().ThrowAsync<UnauthorizedAccessException>(because))
            .Which.Message.Should().Be(PanelSeat.MayNotChairFromSeat, because);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await SnapshotAsync()).Should().Equal(before, because);
    }

    [Theory]
    [MemberData(nameof(EveryActionOfEveryChairWhoMayNotSit))]
    public async Task AChairWhoMayNotSitNow_AndReadsTheReviewNoOtherWay_IsRefusedTheAction_WithTheOneRefusal_AndNothingIsWritten(
        string action, string condition)
    {
        // Since T279 a seat admits its holder to the review only while they may sit at it, so a chair who may not, and
        // holds no other role that reads it, cannot read it. Told the seat's sentence, they would learn that the id names a
        // review of their panel (T194 item 1): they are given the one refusal instead.
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewForAsync(db, action);
        var before = await SnapshotAsync();
        var because = $"{action}: the chair {condition}";

        var act = () => RunAsync(db, action, reviewId, DirectoryWhereTheChair(condition));

        (await act.Should().ThrowAsync<UnauthorizedAccessException>(because))
            .Which.Message.Should().Be("The committee review could not be found among the reviews you chair.", because);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await SnapshotAsync()).Should().Equal(before, because);
    }

    [Theory]
    [MemberData(nameof(ChairsActions))]
    public async Task TheControl_AChairWhoMaySit_TakesTheAction_AndItIsWritten(string action)
    {
        // The same request, fixture and review as the refusal above, past a chair the user store lets sit: it must run to
        // the end and write what the action writes. Not merely "no refusal": any other exception (a fixture that no longer
        // satisfies the action) would leave the refusals above proving nothing about the gate.
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewForAsync(db, action);

        var act = () => RunAsync(db, action, reviewId, DirectoryWhereTheChair("may sit"));

        await act.Should().NotThrowAsync(action);
        db.ChangeTracker.Clear();
        await using var read = CreateDb();
        var review = await read.CommitteeReviews.SingleAsync(entity => entity.Id == reviewId);
        var staged = await read.Set<PendingEntrustmentDecision>().CountAsync(pending => pending.ReviewId == reviewId);
        var stars = await read.Set<EntrustmentDecision>().CountAsync();
        var lines = await read.CommitteeAgendaLines.Where(line => line.ReviewId == reviewId).Select(line => line.State).ToListAsync();

        switch (action)
        {
            case "Record":
                review.State.Should().Be(CommitteeReviewState.Decided, action);
                (await read.Set<CommitteeDecisionAttendee>().Select(attendee => attendee.UserId).ToListAsync())
                    .Should().BeEquivalentTo([ChairUserId, "member-a"], action);
                break;
            case "Ratify":
                review.State.Should().Be(CommitteeReviewState.Ratified, action);
                review.RatifiedByUserId.Should().Be(ChairUserId, action);
                stars.Should().Be(1, "ratifying issues the staged decision as a STAR");
                break;
            case "Close":
                review.State.Should().Be(CommitteeReviewState.Final, action);
                break;
            case "Stage":
                staged.Should().Be(1, action);
                break;
            case "Remove":
                staged.Should().Be(0, action);
                break;
            case "Defer":
                lines.Should().Equal([CommitteeAgendaLineState.Deferred], action);
                break;
            case "Reinstate":
                lines.Should().Equal([CommitteeAgendaLineState.Due], action);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    [Theory]
    [MemberData(nameof(ChairsActions))]
    public async Task SomeoneWhoHoldsNoChairSeat_StillGetsTheOneRefusal_NotTheSeats(string action)
    {
        // The seat's refusal is said only to its holder, who can read the review: anyone else is told the review is not
        // among those they chair, whether or not the id names one (T194 item 1).
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewForAsync(db, action);
        var member = TestPrincipals.InRole(WombatRoles.CommitteeMember, "member-a", InstitutionA);

        var act = () => RunAsync(db, action, reviewId, DirectoryWhereTheChair("was deactivated"), member);

        (await act.Should().ThrowAsync<UnauthorizedAccessException>(action))
            .Which.Message.Should().Be("The committee review could not be found among the reviews you chair.");
    }

    [Fact]
    public async Task AChairWhoMayNotSit_RatifiesNothing_SoNoStarIsIssued()
    {
        // The case T256 names first: ratifying issues the staged STARs as the committee's.
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewAsync(db, CommitteeReviewState.Decided);

        var ratify = () => RunAsync(db, "Ratify", reviewId, DirectoryWhereTheChair("is no longer a committee member"));

        await ratify.Should().ThrowAsync<UnauthorizedAccessException>();
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        await using var read = CreateDb();
        (await read.Set<EntrustmentDecision>().CountAsync()).Should().Be(0);
        (await read.Set<PendingEntrustmentDecision>().CountAsync()).Should().Be(1, "the staged decision is still staged");
        (await read.CommitteeReviews.SingleAsync(review => review.Id == reviewId)).State.Should().Be(CommitteeReviewState.Decided);
    }

    // ─── What the review page is told ────────────────────────────────────────

    public static TheoryData<CommitteeReviewState?> StatesWithAChairsActionOpenOrToCome => new()
    {
        CommitteeReviewState.Scheduled, CommitteeReviewState.InProgress, CommitteeReviewState.Decided, null
    };

    [Theory]
    [MemberData(nameof(StatesWithAChairsActionOpenOrToCome))]
    public async Task ThePage_OffersAChairWhoMayNotSit_NoneOfTheChairsControls_AndSaysWhy_InTheWordsTheClickWouldBeRefusedWith(
        CommitteeReviewState? state)
    {
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewAsync(db, state);

        // A chair who may not sit reads the review only through another rung since T279: here, as a coordinator.
        foreach (var condition in ConditionsWhereTheChairMayNotSit)
        {
            var review = await ReadAsync(db, reviewId, ChairWhoAlsoCoordinates(), DirectoryWhereTheChair(condition));

            review.CallerChairs.Should().BeFalse($"{state}: the chair {condition}");
            review.ChairCannotAct.Should().Be(PanelSeat.MayNotChairFromSeat, $"{state}: the chair {condition}");
        }
    }

    [Theory]
    [MemberData(nameof(StatesWithAChairsActionOpenOrToCome))]
    public async Task AChairWhoMayNotSit_AndReadsTheReviewNoOtherWay_IsRefusedThePage_WithTheOneRefusal(CommitteeReviewState? state)
    {
        // T279: the seat admits its holder to the review only while they may sit at it.
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewAsync(db, state);

        foreach (var condition in ConditionsWhereTheChairMayNotSit)
        {
            var read = () => ReadAsync(db, reviewId, Chair(), DirectoryWhereTheChair(condition));

            (await read.Should().ThrowAsync<UnauthorizedAccessException>($"{state}: the chair {condition}"))
                .Which.Message.Should().Be("The committee review could not be found among the reviews you can view.");
        }
    }

    [Theory]
    [MemberData(nameof(StatesWithAChairsActionOpenOrToCome))]
    public async Task ThePage_TellsEveryoneElse_ThatTheChairCannotAct_NamingThem(CommitteeReviewState? state)
    {
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewAsync(db, state);
        var member = TestPrincipals.InRole(WombatRoles.CommitteeMember, "member-a", InstitutionA);

        var review = await ReadAsync(db, reviewId, member, DirectoryWhereTheChair("moved to another institution"));

        review.CallerChairs.Should().BeFalse();
        review.ChairCannotAct.Should().Be(PanelSeat.ChairCannotAct(ChairName)).And.Contain(ChairName);
    }

    [Theory]
    [MemberData(nameof(StatesWithAChairsActionOpenOrToCome))]
    public async Task TheControl_AChairWhoMaySit_IsOfferedTheControls_AndNobodyIsToldAnything(CommitteeReviewState? state)
    {
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewAsync(db, state);
        var member = TestPrincipals.InRole(WombatRoles.CommitteeMember, "member-a", InstitutionA);

        var asChair = await ReadAsync(db, reviewId, Chair(), DirectoryWhereTheChair("may sit"));
        var asMember = await ReadAsync(db, reviewId, member, DirectoryWhereTheChair("may sit"));

        asChair.CallerChairs.Should().BeTrue();
        asChair.ChairCannotAct.Should().BeNull();
        asMember.CallerChairs.Should().BeFalse();
        asMember.ChairCannotAct.Should().BeNull();
    }

    [Fact]
    public async Task OnceRatified_NoChairsActionIsOpen_SoThePageSaysNothingAboutTheChair()
    {
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewAsync(db, CommitteeReviewState.Ratified);

        var review = await ReadAsync(db, reviewId, ChairWhoAlsoCoordinates(), DirectoryWhereTheChair("was deactivated"));

        review.CallerChairs.Should().BeFalse();
        review.ChairCannotAct.Should().BeNull();
    }

    [Fact]
    public void ACommandsAnswer_KeepsWhatThePageWasToldAboutTheChair()
    {
        // The mapper knows no caller and reads no store (T213): what the page loaded with carries over.
        var loaded = Detail() with { ChairCannotAct = PanelSeat.MayNotChairFromSeat };

        Detail().WithNamesFrom(loaded).ChairCannotAct.Should().Be(PanelSeat.MayNotChairFromSeat);
        Detail().ChairCannotAct.Should().BeNull();
    }

    // ─── Requests ────────────────────────────────────────────────────────────

    private static readonly string[] ConditionsWhereTheChairMayNotSit =
    [
        "is no longer a committee member",
        "moved to another institution",
        "was deactivated",
        "was given the Trainee role since signing in",
        "was erased: the id names no account"
    ];

    /// <summary>
    /// The user store, in which the panel's member and external member may sit, and the chair as
    /// <paramref name="condition" /> says. The chair's claims never change: they signed in before it did.
    /// </summary>
    private static FakeUserDirectory DirectoryWhereTheChair(string condition)
    {
        var directory = new FakeUserDirectory((ChairUserId, ChairName), ("member-a", "Priya Naidoo"))
            .WithCommitteeMembers(InstitutionA, "member-a", "external-a")
            .WithTrainees(TraineeUserId);

        return condition switch
        {
            "may sit" => directory.WithCommitteeMembers(InstitutionA, ChairUserId),
            "is no longer a committee member" => directory.With(Chair(InstitutionA, [WombatRoles.Assessor])),
            "moved to another institution" => directory.With(Chair(InstitutionB, [WombatRoles.CommitteeMember])),
            "was deactivated" => directory.With(Chair(InstitutionA, [WombatRoles.CommitteeMember]) with
            {
                IsLockedOut = true,
                IsDeactivated = true
            }),
            "was given the Trainee role since signing in" =>
                directory.With(Chair(InstitutionA, [WombatRoles.CommitteeMember, WombatRoles.Trainee])),
            "was erased: the id names no account" => directory,
            _ => throw new ArgumentOutOfRangeException(nameof(condition), condition, null)
        };

        static UserIdentityDetails Chair(int institutionId, string[] roles)
            => new(ChairUserId, "chair@test", "Thandi", "Zulu", institutionId, [], [], roles);
    }

    /// <summary>The chair as they signed in: CommitteeMember at the panel's institution.</summary>
    private static ClaimsPrincipal Chair() => TestPrincipals.InRole(WombatRoles.CommitteeMember, ChairUserId, InstitutionA);

    /// <summary>
    /// The chair as they signed in, holding Coordinator at the panel's institution too: a role that reads every review of
    /// the institution's panels whether or not its holder may sit on them (T279).
    /// </summary>
    private static ClaimsPrincipal ChairWhoAlsoCoordinates()
        => TestPrincipals.InRoles([WombatRoles.CommitteeMember, WombatRoles.Coordinator], ChairUserId, InstitutionA);

    private static async Task RunAsync(
        ApplicationDbContext db, string action, int reviewId, IUserAdministrationService users, ClaimsPrincipal? caller = null)
    {
        var principal = caller ?? Chair();
        _ = action switch
        {
            "Record" => (object)await new RecordCommitteeDecisionCommandHandler(db, users).Handle(
                new RecordCommitteeDecisionCommand(
                    reviewId, CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, [ChairUserId, "member-a"], principal),
                CancellationToken.None),
            "Ratify" => await new RatifyCommitteeDecisionCommandHandler(db, users).Handle(
                new RatifyCommitteeDecisionCommand(reviewId, principal), CancellationToken.None),
            "Close" => await new CloseFormativeReviewCommandHandler(db, users).Handle(
                new CloseFormativeReviewCommand(reviewId, principal), CancellationToken.None),
            "Stage" => await new StagePendingEntrustmentDecisionCommandHandler(db, users).Handle(
                new StagePendingEntrustmentDecisionCommand(
                    reviewId, null, EpaId, LevelId, new DateOnly(2027, 1, 8), null, "Ready.",
                    [await EvidenceLineIdAsync(db, reviewId)], principal),
                CancellationToken.None),
            "Remove" => await new RemovePendingEntrustmentDecisionCommandHandler(db, users).Handle(
                new RemovePendingEntrustmentDecisionCommand(reviewId, await PendingIdAsync(db, reviewId), principal),
                CancellationToken.None),
            "Defer" => await new DeferAgendaLineCommandHandler(db, users).Handle(
                new DeferAgendaLineCommand(reviewId, await AgendaLineIdAsync(db, reviewId), "Not observed this semester.", principal),
                CancellationToken.None),
            "Reinstate" => await new ReinstateAgendaLineCommandHandler(db, users).Handle(
                new ReinstateAgendaLineCommand(reviewId, await AgendaLineIdAsync(db, reviewId), principal),
                CancellationToken.None),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };
    }

    private static Task<CommitteeReviewDetailDto> ReadAsync(
        ApplicationDbContext db, int reviewId, ClaimsPrincipal caller, IUserAdministrationService users)
        => new GetCommitteeReviewByIdQueryHandler(db, users).Handle(
            new GetCommitteeReviewByIdQuery(reviewId, caller, new DateOnly(2027, 1, 8)), CancellationToken.None);

    private static Task<int> EvidenceLineIdAsync(ApplicationDbContext db, int reviewId)
        => db.CommitteeEvidenceItems.Where(line => line.ReviewId == reviewId).Select(line => line.Id).SingleAsync();

    private static Task<int> PendingIdAsync(ApplicationDbContext db, int reviewId)
        => db.Set<PendingEntrustmentDecision>().Where(pending => pending.ReviewId == reviewId).Select(pending => pending.Id).SingleAsync();

    private static Task<int> AgendaLineIdAsync(ApplicationDbContext db, int reviewId)
        => db.CommitteeAgendaLines.Where(line => line.ReviewId == reviewId).Select(line => line.Id).SingleAsync();

    private static CommitteeReviewDetailDto Detail()
        => new(
            1, TraineeUserId, PanelId, "Paediatrics CCC", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31),
            new DateOnly(2027, 1, 8), CommitteeReviewState.Decided, null, null, null, null, null, [], [], [])
        {
            AcademicYear = 2026,
            Semester = 2
        };

    // ─── Fixture ─────────────────────────────────────────────────────────────

    /// <summary>
    /// The review each action is taken on, in a state that action takes: so only the gate stands in the way. Recording
    /// needs its agenda line staged first (T131 slice 4), or it refuses whoever records it.
    /// </summary>
    private static Task<int> SeedReviewForAsync(ApplicationDbContext db, string action) => action switch
    {
        "Record" => SeedReviewAsync(db, CommitteeReviewState.InProgress, stage: true),
        "Ratify" => SeedReviewAsync(db, CommitteeReviewState.Decided),
        "Close" => SeedReviewAsync(db, state: null),
        "Stage" => SeedReviewAsync(db, CommitteeReviewState.InProgress),
        "Remove" => SeedReviewAsync(db, CommitteeReviewState.InProgress, stage: true),
        "Defer" => SeedReviewAsync(db, CommitteeReviewState.InProgress),
        "Reinstate" => SeedReviewAsync(db, CommitteeReviewState.InProgress, deferLine: true),
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
    };

    /// <summary>
    /// The panel's review of the paediatric trainee at A, in <paramref name="state" />; null for a formative review in
    /// progress, which holds no agenda. A binding review in progress or later holds one agenda line and one frozen evidence
    /// line; a decided one has the line staged, as recording demands.
    /// </summary>
    private static async Task<int> SeedReviewAsync(
        ApplicationDbContext db, CommitteeReviewState? state, bool stage = false, bool deferLine = false)
    {
        var now = new DateTime(2027, 1, 8, 9, 0, 0, DateTimeKind.Utc);
        var review = new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 2,
            TraineeUserId = TraineeUserId,
            PanelId = PanelId,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 12, 31),
            ScheduledOn = new DateOnly(2027, 1, 8),
            IsFormative = state is null
        };

        var line = new CommitteeEvidence
        {
            SourceType = CommitteeEvidenceSourceType.Activity,
            ActivityId = 900,
            EpaId = EpaId,
            SourceLabel = "Mini-CEX #900",
            Summary = "State: completed.",
            ObservedOn = new DateOnly(2026, 6, 1)
        };

        if (state is not CommitteeReviewState.Scheduled)
        {
            IReadOnlyList<CommitteeAgendaLine> agenda = review.IsFormative
                ? []
                :
                [
                    CommitteeAgendaLine.ForCadence(
                        1000, EpaId, "PAED-007", "Triage", isOpportunistic: false,
                        QuotaWindow.For(QuotaPeriod.Semester, new DateOnly(2026, 12, 31), new DateOnly(2024, 1, 15)),
                        new AcademicPeriod(2026, 2))
                ];
            review.Start([line], agenda, ChairUserId, now);
        }

        if (deferLine)
        {
            review.AgendaLines.Single().Defer("Not observed this semester.");
        }

        db.CommitteeReviews.Add(review);
        await db.SaveChangesAsync();

        if (stage || state is CommitteeReviewState.Decided or CommitteeReviewState.Ratified)
        {
            db.Set<PendingEntrustmentDecision>().Add(PendingEntrustmentDecision.Stage(
                review.Id, EpaId, LevelId, new DateOnly(2027, 1, 8), null, "Ready.", [line.Id], ChairUserId, now));
        }

        if (state is CommitteeReviewState.Decided or CommitteeReviewState.Ratified)
        {
            review.RecordDecision(
                CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, ChairUserId, now,
                [
                    new DecisionPanelMember { UserId = ChairUserId, Role = DecisionPanelMemberRole.Chair },
                    new DecisionPanelMember { UserId = "member-a", Role = DecisionPanelMemberRole.Member }
                ], [EpaId], []);
        }

        if (state is CommitteeReviewState.Ratified)
        {
            review.Ratify(ChairUserId, now);
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
        db.Specialities.Add(new Speciality { Id = Paediatrics, CollegeId = 1, Name = "Paediatrics", IsActive = true });
        db.SubSpecialities.Add(new SubSpeciality
        {
            Id = GeneralPaediatrics, SpecialityId = Paediatrics, Name = "General Paediatrics", IsActive = true
        });
        db.Curricula.Add(new Curriculum { Id = 100, SubSpecialityId = GeneralPaediatrics, Name = "General Paediatrics", Version = "11.1" });
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = TraineeUserId,
            InstitutionId = InstitutionA,
            CurriculumId = 100,
            ProgrammeStartDate = new DateOnly(2024, 1, 15),
            ExpectedCompletionDate = new DateOnly(2028, 1, 15),
            IsActive = true
        });

        db.DecisionPanels.Add(new DecisionPanel
        {
            Id = PanelId,
            Name = "Paediatrics CCC",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = InstitutionA,
            CreatedOn = DateTime.UtcNow,
            Members =
            [
                new DecisionPanelMember { UserId = ChairUserId, Role = DecisionPanelMemberRole.Chair },
                new DecisionPanelMember { UserId = "member-a", Role = DecisionPanelMemberRole.Member },
                new DecisionPanelMember { UserId = "external-a", Role = DecisionPanelMemberRole.External }
            ]
        });

        db.EntrustmentScales.Add(new EntrustmentScale { Id = 1, Name = "v11.1 rungs" });
        db.EntrustmentLevels.Add(new EntrustmentLevel { Id = LevelId, ScaleId = 1, Order = 3, Label = "Indirect supervision" });
        db.Epas.Add(new Epa { Id = EpaId, SubSpecialityId = GeneralPaediatrics, Code = "PAED-007", Title = "Triage", IsActive = true });
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 1000, CurriculumId = 100, EpaId = EpaId, RequiredCount = 1, MinimumLevelOrder = 3
        });

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);

    private static async Task SaveAndClearAsAuditPipelineWouldAsync(ApplicationDbContext db)
    {
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    /// <summary>Everything a chair's action writes: the review, its decisions, staged STARs, agenda lines and STARs.</summary>
    private async Task<IReadOnlyList<string>> SnapshotAsync()
    {
        await using var read = CreateDb();
        var rows = new List<string>();
        rows.AddRange(await read.CommitteeReviews.OrderBy(review => review.Id)
            .Select(review => $"review {review.Id}:{review.State}:{review.RatifiedOn}:{review.RatifiedByUserId}:{review.FinalizedOn}")
            .ToListAsync());
        rows.AddRange(await read.Set<CommitteeDecision>().OrderBy(decision => decision.Id)
            .Select(decision => $"decision {decision.Id}:{decision.Category}:{decision.DecidedByChairUserId}")
            .ToListAsync());
        rows.AddRange(await read.Set<CommitteeDecisionAttendee>().OrderBy(attendee => attendee.Id)
            .Select(attendee => $"attendee {attendee.DecisionId}:{attendee.UserId}")
            .ToListAsync());
        rows.AddRange(await read.Set<PendingEntrustmentDecision>().OrderBy(pending => pending.Id)
            .Select(pending => $"pending {pending.Id}:{pending.EpaId}:{pending.AuthorisedLevelId}:{pending.Rationale}")
            .ToListAsync());
        rows.AddRange(await read.Set<CommitteeAgendaLine>().OrderBy(line => line.Id)
            .Select(line => $"line {line.Id}:{line.State}:{line.DeferralReason}")
            .ToListAsync());
        rows.AddRange(await read.Set<EntrustmentDecision>().OrderBy(decision => decision.Id)
            .Select(decision => $"star {decision.Id}:{decision.Status}")
            .ToListAsync());
        return rows;
    }
}
