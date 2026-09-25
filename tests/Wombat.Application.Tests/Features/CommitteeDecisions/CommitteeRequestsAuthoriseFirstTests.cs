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
/// Every committee command, and every read of a committee review, authorises before anything else: an id that names
/// nothing and one out of the caller's reach get the same refusal, before any state check, and nothing is written. (T194
/// item 1)
/// </summary>
/// <remarks>
/// <para>
/// The committee pages print a refusal. Until T194 an unknown review id was "could not be found" while one out of reach
/// had a sentence of its own ("You are not a member of this decision panel", "Only the panel's chair can do this",
/// "This review is not yet visible to the trainee"), and a state check could come first. So anyone who could open a
/// committee page could walk ids and learn which reviews exist, which are formative, and how far each has got; and a
/// trainee could learn that a review of theirs was under way. T131 fixed staging, removing, ratifying and the agenda's
/// lines; T194 finishes starting, recording, closing, the appeals and the review's reads.
/// </para>
/// <para>
/// Each request is run twice by an outsider, on an id that names nothing and on a real review they may not act on, in a
/// state its own check would refuse. Both must give the one refusal, which says the review is not among those the caller
/// may act on. The control runs the same review past someone the gate admits, who is told about the state instead: so
/// the outsider's refusal is the gate's, not the state's. Every refusal is followed by the audit pipeline's save and a
/// cleared tracker.
/// </para>
/// </remarks>
public sealed class CommitteeRequestsAuthoriseFirstTests
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;
    private const int Paediatrics = 1;
    private const int Surgery = 2;
    private const int GeneralPaediatrics = 11;
    private const int GeneralSurgery = 21;

    private const int PanelA = 10;
    private const int PanelB = 20;
    private const int PaediatricsPanelA = 30;

    private const int EpaId = 7;
    private const int LevelId = 3;
    private const int UnknownId = 999;

    private const string PaedsAtA = "paeds-a";
    private const string SurgeryAtA = "surgery-a";
    private const string PaedsAtB = "paeds-b";

    /// <summary>A Coordinator at A who also sits as an External member on B's panel (T194 item 3).</summary>
    private const string CoordinatorOfA = "coord-a";

    private readonly string _databaseName = Guid.NewGuid().ToString();

    // ─── A review id ─────────────────────────────────────────────────────────

    public static TheoryData<string> ReviewRequests => new()
    {
        // Commands.
        "Start", "Record", "Ratify", "Close", "ResolveAppeal", "LodgeAppeal", "Stage", "Remove", "Defer", "Reinstate",
        // The review page's reads, which share one ladder.
        "GetById", "Agenda", "MsfOutsideSnapshot", "Sampling", "PendingDecisions", "StarEpaOptions"
    };

    [Theory]
    [MemberData(nameof(ReviewRequests))]
    public async Task AnUnknownReview_AndOneOutOfReach_GetTheOneRefusal_BeforeItsState_AndNothingIsWritten(string request)
    {
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewAsync(db, StateTheGateComesBefore(request));
        var before = await SnapshotAsync();
        var outsider = OutsiderFor(request);

        var unknown = await RefusalAsync(() => RunAsync(db, request, UnknownId, outsider));
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        var outOfReach = await RefusalAsync(() => RunAsync(db, request, reviewId, outsider));
        await SaveAndClearAsAuditPipelineWouldAsync(db);

        unknown.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(OneRefusal(request));
        outOfReach.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(OneRefusal(request));
        (await SnapshotAsync()).Should().BeEquivalentTo(before);
    }

    [Theory]
    [MemberData(nameof(ReviewRequests))]
    public async Task TheControl_SomeoneTheGateAdmits_IsToldTheReviewsState_OrReadsIt(string request)
    {
        // The same review, in the same state, past the gate: a command's own state check now speaks, and a read answers.
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewAsync(db, StateTheGateComesBefore(request));
        var before = await SnapshotAsync();

        var act = () => RunAsync(db, request, reviewId, InsiderFor(request));

        if (IsRead(request))
        {
            await act.Should().NotThrowAsync();
            return;
        }

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().NotBe(OneRefusal(request));
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await SnapshotAsync()).Should().BeEquivalentTo(before);
    }

    public static TheoryData<string> ReadsOfAReview => new()
    {
        "GetById", "Agenda", "MsfOutsideSnapshot", "Sampling", "PendingDecisions", "StarEpaOptions"
    };

    [Theory]
    [MemberData(nameof(ReadsOfAReview))]
    public async Task ATrainee_IsRefusedTheirOwnReviewBeforeRatification_AsAnUnknownId(string request)
    {
        // Until T194 the ladder told a trainee "This review is not yet visible to the trainee", which said a review of
        // theirs was under way before the panel had said anything.
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewAsync(db, CommitteeReviewState.InProgress);
        var ownTrainee = TestPrincipals.Trainee(PaedsAtA, InstitutionA);

        var own = await RefusalAsync(() => RunAsync(db, request, reviewId, ownTrainee));
        var unknown = await RefusalAsync(() => RunAsync(db, request, UnknownId, ownTrainee));

        own.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(unknown.Message);
        unknown.Message.Should().Be("The committee review could not be found among the reviews you can view.");
    }

    [Fact]
    public async Task ATrainee_IsRefusedAnAppealOnTheirOwnReviewBeforeRatification_AsAnUnknownId_AndNothingIsWritten()
    {
        // Until T194 their own scheduled review drew "Only ratified reviews can be appealed", naming its state.
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewAsync(db, CommitteeReviewState.Decided);
        var before = await SnapshotAsync();
        var ownTrainee = TestPrincipals.Trainee(PaedsAtA, InstitutionA);

        var own = await RefusalAsync(() => RunAsync(db, "LodgeAppeal", reviewId, ownTrainee));
        await SaveAndClearAsAuditPipelineWouldAsync(db);

        own.Should().BeOfType<UnauthorizedAccessException>()
            .Which.Message.Should().Be("The committee review could not be found among your own ratified reviews.");
        (await SnapshotAsync()).Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task ACallerWhoIsNoTrainee_IsToldSo_BeforeTheReviewIsLookedUp()
    {
        // Who may appeal at all is a role, which says nothing about an id: the same sentence for every id.
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewAsync(db, CommitteeReviewState.Ratified);
        var chair = Member("chair-a", InstitutionA);

        var real = await RefusalAsync(() => RunAsync(db, "LodgeAppeal", reviewId, chair));
        var unknown = await RefusalAsync(() => RunAsync(db, "LodgeAppeal", UnknownId, chair));

        real.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be("Only trainees can lodge appeals.");
        unknown.Message.Should().Be(real.Message);
    }

    [Fact]
    public async Task AnInstitutionalAdmin_ReadsAScheduledReview_ButIsRefusedStart_AsAnUnknownId_AndIsNotOfferedIt()
    {
        // An InstitutionalAdmin reads every review at their institution (T075), but starting one is for the panel's
        // members and the institution's coordinators. The page offers Start by the predicate the handler demands.
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewAsync(db, CommitteeReviewState.Scheduled);
        var admin = TestPrincipals.InstitutionalAdmin(InstitutionA);

        var detail = await new GetCommitteeReviewByIdQueryHandler(db, FakeUserDirectory.Empty).Handle(
            new GetCommitteeReviewByIdQuery(reviewId, admin), CancellationToken.None);
        var refusal = await RefusalAsync(() => RunAsync(db, "Start", reviewId, admin));
        await SaveAndClearAsAuditPipelineWouldAsync(db);

        detail.CallerMayStart.Should().BeFalse();
        refusal.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(OneRefusal("Start"));
        (await ReviewStateAsync(reviewId)).Should().Be(CommitteeReviewState.Scheduled);
    }

    [Theory]
    [InlineData("member-a")]
    [InlineData("Coordinator of A")]
    [InlineData("Administrator")]
    public async Task EveryoneStartAdmits_IsOfferedStart(string who)
    {
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewAsync(db, CommitteeReviewState.Scheduled);
        var caller = who switch
        {
            "Coordinator of A" => TestPrincipals.Coordinator(InstitutionA),
            "Administrator" => TestPrincipals.Administrator(),
            _ => Member(who, InstitutionA)
        };

        var detail = await new GetCommitteeReviewByIdQueryHandler(db, FakeUserDirectory.Empty).Handle(
            new GetCommitteeReviewByIdQuery(reviewId, caller), CancellationToken.None);
        await RunAsync(db, "Start", reviewId, caller);

        detail.CallerMayStart.Should().BeTrue();
        (await ReviewStateAsync(reviewId)).Should().Be(CommitteeReviewState.InProgress);
    }

    // ─── The trainee rung on conducting a review (T194 review) ──────────────

    public static TheoryData<string> RequestsAPanelSeatAdmits => new()
    {
        "Start", "Record", "Ratify", "Close", "ResolveAppeal", "Stage", "Remove", "Defer", "Reinstate",
        "GetById", "Agenda", "MsfOutsideSnapshot", "Sampling", "PendingDecisions", "StarEpaOptions"
    };

    [Theory]
    [MemberData(nameof(RequestsAPanelSeatAdmits))]
    public async Task SomeoneTheGateAdmits_IsRefused_OnceTheyAlsoHoldTrainee_AndNothingIsWritten(string request)
    {
        // The control's caller, seat, review and state, with the Trainee role beside them (T185's rung). The read ladder
        // always asked it first; until the T194 review Start, the chair's actions and the appeal body did not, so a
        // registrar who coordinated, or sat on the panel, could conduct a peer's review the page would not show them.
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewAsync(db, StateTheGateComesBefore(request));
        var before = await SnapshotAsync();

        var refusal = await RefusalAsync(() => RunAsync(db, request, reviewId, AlsoATrainee(InsiderFor(request))));
        await SaveAndClearAsAuditPipelineWouldAsync(db);

        refusal.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(OneRefusal(request));
        (await SnapshotAsync()).Should().BeEquivalentTo(before);
    }

    [Theory]
    [InlineData("Trainee who coordinates at A, on a peer's review")]
    [InlineData("Trainee who coordinates at A, on their own review")]
    [InlineData("Trainee seated as a member of A's panel")]
    [InlineData("Trainee who is an Administrator")]
    public async Task ATraineeWhoStartWouldOtherwiseAdmit_StartsNoScheduledReview_AndNothingIsWritten(string who)
    {
        // Start froze the evidence and the agenda and handed back the whole review, the read ladder's refusal included.
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewAsync(db, CommitteeReviewState.Scheduled);
        var before = await SnapshotAsync();
        var caller = who switch
        {
            "Trainee who coordinates at A, on a peer's review" =>
                TestPrincipals.InRoles([WombatRoles.Trainee, WombatRoles.Coordinator], "registrar-a", InstitutionA),
            "Trainee who coordinates at A, on their own review" =>
                TestPrincipals.InRoles([WombatRoles.Trainee, WombatRoles.Coordinator], PaedsAtA, InstitutionA),
            "Trainee seated as a member of A's panel" =>
                TestPrincipals.InRoles([WombatRoles.Trainee, WombatRoles.CommitteeMember], "member-a", InstitutionA),
            "Trainee who is an Administrator" =>
                TestPrincipals.InRoles([WombatRoles.Trainee, WombatRoles.Administrator], "registrar-admin", null),
            _ => throw new ArgumentOutOfRangeException(nameof(who), who, null)
        };

        var refusal = await RefusalAsync(() => RunAsync(db, "Start", reviewId, caller));
        await SaveAndClearAsAuditPipelineWouldAsync(db);

        refusal.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(OneRefusal("Start"));
        (await SnapshotAsync()).Should().BeEquivalentTo(before);
        (await ReviewStateAsync(reviewId)).Should().Be(CommitteeReviewState.Scheduled);
    }

    [Theory]
    [InlineData(DecisionPanelMemberRole.External)]
    [InlineData(DecisionPanelMemberRole.Chair)]
    public async Task ATraineeSeatedOnThePanelThatReviewsThem_IsNotOfferedTheirOwnAppeal_NorResolvesIt(
        DecisionPanelMemberRole seat)
    {
        // A trainee may hold CommitteeMember and sit on a panel (PanelSeat), and the page shows them their own review
        // once it is ratified. Until the T194 review a seat on the appeal body offered them the resolve form on their own
        // appeal, and the handler took it: dismissed, or remitted with a replacement decision and two others named present.
        await using var db = await SeededDbAsync();
        await SeatAsync(db, PanelA, PaedsAtA, seat);
        var reviewId = await SeedReviewAsync(db, CommitteeReviewState.UnderAppeal);
        var before = await SnapshotAsync();
        var ownTrainee = TestPrincipals.InRoles([WombatRoles.Trainee, WombatRoles.CommitteeMember], PaedsAtA, InstitutionA);
        var users = FakeUserDirectory.CommitteeMembersAt(InstitutionA, "chair-a", "member-a", "external-a", PaedsAtA);

        var detail = await new GetCommitteeReviewByIdQueryHandler(db, FakeUserDirectory.Empty).Handle(
            new GetCommitteeReviewByIdQuery(reviewId, ownTrainee), CancellationToken.None);
        var dismissed = await RefusalAsync(() => new ResolveAppealCommandHandler(db, users).Handle(
            new ResolveAppealCommand(reviewId, CommitteeAppealOutcome.Dismissed, null, null, null, null, ownTrainee),
            CancellationToken.None));
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        var remitted = await RefusalAsync(() => new ResolveAppealCommandHandler(db, users).Handle(
            new ResolveAppealCommand(
                reviewId, CommitteeAppealOutcome.Remitted, CommitteeDecisionCategory.SatisfactoryProgress,
                "Remitted by the trainee.", null, ["chair-a", "member-a"], ownTrainee),
            CancellationToken.None));
        await SaveAndClearAsAuditPipelineWouldAsync(db);

        detail.CallerResolvesAppeals.Should().BeFalse();
        detail.CallerChairs.Should().BeFalse();
        detail.CallerMayStart.Should().BeFalse();
        dismissed.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(OneRefusal("ResolveAppeal"));
        remitted.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(OneRefusal("ResolveAppeal"));
        (await SnapshotAsync()).Should().BeEquivalentTo(before);
        (await ReviewStateAsync(reviewId)).Should().Be(CommitteeReviewState.UnderAppeal);
    }

    [Fact]
    public async Task AnInstitutionalAdminSeatedOnAnotherInstitutionsPanel_ReadsTheReviewStartAdmitsThemTo()
    {
        // A member of A's panel who moved to B and administers it there. Start admits them as a member (WorksOnPanel);
        // until the T194 review the read ladder's InstitutionalAdmin arm asked only their own institution, and refused
        // them the review they had just started.
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewAsync(db, CommitteeReviewState.Scheduled);
        var moved = TestPrincipals.InstitutionalAdmin(InstitutionB, "member-a");

        var detail = await new GetCommitteeReviewByIdQueryHandler(db, FakeUserDirectory.Empty).Handle(
            new GetCommitteeReviewByIdQuery(reviewId, moved), CancellationToken.None);
        await RunAsync(db, "Start", reviewId, moved);
        var started = await new GetCommitteeReviewByIdQueryHandler(db, FakeUserDirectory.Empty).Handle(
            new GetCommitteeReviewByIdQuery(reviewId, moved), CancellationToken.None);

        detail.CallerMayStart.Should().BeTrue();
        started.State.Should().Be(CommitteeReviewState.InProgress);
    }

    /// <summary>The same sign-in with the Trainee role beside its own.</summary>
    private static ClaimsPrincipal AlsoATrainee(ClaimsPrincipal principal)
    {
        var identity = new ClaimsIdentity(principal.Claims, "test");
        identity.AddClaim(new Claim(ClaimTypes.Role, WombatRoles.Trainee));
        return new ClaimsPrincipal(identity);
    }

    private static async Task SeatAsync(ApplicationDbContext db, int panelId, string userId, DecisionPanelMemberRole role)
    {
        var panel = await db.DecisionPanels.Include(entity => entity.Members).SingleAsync(entity => entity.Id == panelId);
        if (role == DecisionPanelMemberRole.Chair)
        {
            panel.Members.Single(member => member.Role == DecisionPanelMemberRole.Chair).Role = DecisionPanelMemberRole.Member;
        }

        panel.Members.Add(new DecisionPanelMember { UserId = userId, Role = role });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    /// <summary>
    /// A state in which the request's own check would refuse even someone the gate admits, so that an outsider refused
    /// with the one sentence was refused before the state was looked at. Reads have no state check, and any state serves.
    /// </summary>
    private static CommitteeReviewState? StateTheGateComesBefore(string request) => request switch
    {
        "Start" => CommitteeReviewState.InProgress,          // already started
        "Record" => CommitteeReviewState.Scheduled,          // not yet in progress
        "Ratify" => CommitteeReviewState.InProgress,         // not yet decided
        "Close" => CommitteeReviewState.InProgress,          // binding, so not closed without a decision
        "ResolveAppeal" => CommitteeReviewState.Ratified,    // no appeal open
        "LodgeAppeal" => CommitteeReviewState.UnderAppeal,   // an appeal already open
        "Stage" or "Defer" or "Reinstate" => null,           // formative: no entrustment decisions, no agenda
        "Remove" => CommitteeReviewState.Ratified,           // the staged decisions are fixed
        _ => CommitteeReviewState.Scheduled
    };

    /// <summary>Who each request refuses: a committee member of another institution's panel, or another trainee.</summary>
    private static ClaimsPrincipal OutsiderFor(string request)
        => request == "LodgeAppeal" ? TestPrincipals.Trainee(PaedsAtB, InstitutionB) : Member("chair-b", InstitutionB);

    /// <summary>Who each request admits on panel A's review.</summary>
    private static ClaimsPrincipal InsiderFor(string request) => request switch
    {
        "Start" or "GetById" or "Agenda" or "MsfOutsideSnapshot" or "Sampling" or "PendingDecisions" or "StarEpaOptions"
            => Member("member-a", InstitutionA),
        "ResolveAppeal" => Member("external-a", InstitutionA),
        "LodgeAppeal" => TestPrincipals.Trainee(PaedsAtA, InstitutionA),
        _ => Member("chair-a", InstitutionA)
    };

    private static bool IsRead(string request)
        => request is "GetById" or "Agenda" or "MsfOutsideSnapshot" or "Sampling" or "PendingDecisions" or "StarEpaOptions";

    /// <summary>The one refusal each request gives an id it will not act on, whether or not the id names a review.</summary>
    private static string OneRefusal(string request) => request switch
    {
        "Start" => "The committee review could not be found among the reviews you can start.",
        "ResolveAppeal" => "The committee review could not be found among the reviews whose appeals you resolve.",
        "LodgeAppeal" => "The committee review could not be found among your own ratified reviews.",
        _ when IsRead(request) => "The committee review could not be found among the reviews you can view.",
        _ => "The committee review could not be found among the reviews you chair."
    };

    private static async Task RunAsync(ApplicationDbContext db, string request, int reviewId, ClaimsPrincipal principal)
    {
        var users = FakeUserDirectory.CommitteeMembersAt(InstitutionA, "chair-a", "member-a", "external-a");
        _ = request switch
        {
            "Start" => await new StartCommitteeReviewCommandHandler(db).Handle(
                new StartCommitteeReviewCommand(reviewId, principal), CancellationToken.None),
            "Record" => await new RecordCommitteeDecisionCommandHandler(db, users).Handle(
                new RecordCommitteeDecisionCommand(
                    reviewId, CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null,
                    ["chair-a", "member-a"], principal),
                CancellationToken.None),
            "Ratify" => await new RatifyCommitteeDecisionCommandHandler(db).Handle(
                new RatifyCommitteeDecisionCommand(reviewId, principal), CancellationToken.None),
            "Close" => await new CloseFormativeReviewCommandHandler(db).Handle(
                new CloseFormativeReviewCommand(reviewId, principal), CancellationToken.None),
            "ResolveAppeal" => await new ResolveAppealCommandHandler(db, users).Handle(
                new ResolveAppealCommand(reviewId, CommitteeAppealOutcome.Dismissed, null, null, null, null, principal),
                CancellationToken.None),
            "LodgeAppeal" => await new LodgeAppealCommandHandler(db).Handle(
                new LodgeAppealCommand(reviewId, "The window missed my rotation.", principal), CancellationToken.None),
            "Stage" => await new StagePendingEntrustmentDecisionCommandHandler(db).Handle(
                new StagePendingEntrustmentDecisionCommand(
                    reviewId, null, EpaId, LevelId, new DateOnly(2027, 1, 8), null, "Ready.", [1], principal),
                CancellationToken.None),
            "Remove" => await new RemovePendingEntrustmentDecisionCommandHandler(db).Handle(
                new RemovePendingEntrustmentDecisionCommand(reviewId, 1, principal), CancellationToken.None),
            "Defer" => await new DeferAgendaLineCommandHandler(db).Handle(
                new DeferAgendaLineCommand(reviewId, 1, "Not yet observed.", principal), CancellationToken.None),
            "Reinstate" => await new ReinstateAgendaLineCommandHandler(db).Handle(
                new ReinstateAgendaLineCommand(reviewId, 1, principal), CancellationToken.None),
            "GetById" => await new GetCommitteeReviewByIdQueryHandler(db, FakeUserDirectory.Empty).Handle(
                new GetCommitteeReviewByIdQuery(reviewId, principal), CancellationToken.None),
            "Agenda" => await new GetCommitteeAgendaQueryHandler(db).Handle(
                new GetCommitteeAgendaQuery(reviewId, principal), CancellationToken.None),
            "MsfOutsideSnapshot" => await new CountMsfCampaignsOutsideSnapshotQueryHandler(db).Handle(
                new CountMsfCampaignsOutsideSnapshotQuery(reviewId, principal), CancellationToken.None),
            "Sampling" => await new GetSamplingConcentrationWarningsQueryHandler(db).Handle(
                new GetSamplingConcentrationWarningsQuery(reviewId, principal), CancellationToken.None),
            "PendingDecisions" => await new ListPendingEntrustmentDecisionsForReviewQueryHandler(db).Handle(
                new ListPendingEntrustmentDecisionsForReviewQuery(reviewId, principal), CancellationToken.None),
            "StarEpaOptions" => (object)await new ListStarEpaOptionsForReviewQueryHandler(db).Handle(
                new ListStarEpaOptionsForReviewQuery(reviewId, principal), CancellationToken.None),
            _ => throw new ArgumentOutOfRangeException(nameof(request), request, null)
        };
    }

    // ─── A panel id ──────────────────────────────────────────────────────────

    public static TheoryData<string> PanelRequests => new()
    {
        "UpdatePanel", "SetPanelBody", "Schedule", "PreviewAgenda"
    };

    [Theory]
    [MemberData(nameof(PanelRequests))]
    public async Task AnUnknownPanel_AndAnotherInstitutionsPanel_GetTheOneRefusal_AndNothingIsWritten(string request)
    {
        // Already so for these since T182 and T131 slice 3; held here with the rest.
        await using var db = await SeededDbAsync();
        var before = await PanelsSnapshotAsync();
        var caller = TestPrincipals.InstitutionalAdmin(InstitutionA);

        var unknown = await RefusalAsync(() => RunOnPanelAsync(db, request, UnknownId, caller));
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        var otherInstitutions = await RefusalAsync(() => RunOnPanelAsync(db, request, PanelB, caller));
        await SaveAndClearAsAuditPipelineWouldAsync(db);

        otherInstitutions.Should().BeOfType<UnauthorizedAccessException>();
        unknown.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(otherInstitutions.Message);
        (await PanelsSnapshotAsync()).Should().BeEquivalentTo(before);
    }

    private static async Task RunOnPanelAsync(ApplicationDbContext db, string request, int panelId, ClaimsPrincipal principal)
    {
        var members = FakeUserDirectory.CommitteeMembersAt(InstitutionA, "chair-a", "member-a")
            .WithCommitteeMembers(InstitutionB, "chair-b", "member-b");
        _ = request switch
        {
            "UpdatePanel" => await new UpdateDecisionPanelCommandHandler(db, members).Handle(
                new UpdateDecisionPanelCommand(
                    panelId,
                    [
                        new DecisionPanelMemberInput("chair-a", DecisionPanelMemberRole.Chair),
                        new DecisionPanelMemberInput("member-a", DecisionPanelMemberRole.Member)
                    ],
                    principal),
                CancellationToken.None),
            "SetPanelBody" => await new SetDecisionPanelBodyCommandHandler(db).Handle(
                new SetDecisionPanelBodyCommand(panelId, null, principal), CancellationToken.None),
            "Schedule" => await new ScheduleCommitteeReviewCommandHandler(db).Handle(
                new ScheduleCommitteeReviewCommand(
                    PaedsAtA, panelId, 2026, 2, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31),
                    new DateOnly(2027, 1, 8), principal),
                CancellationToken.None),
            "PreviewAgenda" => (object)await new PreviewCommitteeAgendaQueryHandler(db).Handle(
                new PreviewCommitteeAgendaQuery(PaedsAtA, panelId, 2026, 2, principal, new DateOnly(2026, 9, 24)),
                CancellationToken.None),
            _ => throw new ArgumentOutOfRangeException(nameof(request), request, null)
        };
    }

    // ─── An entrustment decision id ──────────────────────────────────────────

    [Fact]
    public async Task Revoke_AnUnknownDecision_AndAnotherInstitutionsDecision_GetTheOneRefusal_AndNothingIsWritten()
    {
        // T183's rule, held here with the rest of the committee's commands.
        await using var db = await SeededDbAsync();
        var decisionAtB = await SeedDecisionAsync(db, PaedsAtB, PanelB);
        var caller = TestPrincipals.InstitutionalAdmin(InstitutionA);

        var unknown = await RefusalAsync(() => RevokeAsync(db, UnknownId, caller));
        var otherInstitutions = await RefusalAsync(() => RevokeAsync(db, decisionAtB, caller));
        await SaveAndClearAsAuditPipelineWouldAsync(db);

        otherInstitutions.Should().BeOfType<UnauthorizedAccessException>();
        unknown.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(otherInstitutions.Message);
        await using var read = CreateDb();
        (await read.Set<EntrustmentDecision>().SingleAsync(decision => decision.Id == decisionAtB)).Status
            .Should().Be(EntrustmentDecisionStatus.Active);
    }

    private static Task<EntrustmentDecisionDto> RevokeAsync(ApplicationDbContext db, int decisionId, ClaimsPrincipal principal)
        => new RevokeEntrustmentDecisionCommandHandler(db).Handle(
            new RevokeEntrustmentDecisionCommand(decisionId, "Concern raised.", principal), CancellationToken.None);

    // ─── The panel form offers what creating a panel accepts (T194) ─────────

    public static TheoryData<string> PanelAdministrators => new()
    {
        "Administrator", "InstitutionalAdmin of A", "SpecialityAdmin of A (Paediatrics)",
        "SubSpecialityAdmin of A (General Surgery)", "SubSpecialityAdmin of A (General Surgery) with a Paediatrics claim",
        "SpecialityAdmin with no institution", "Coordinator of A"
    };

    [Theory]
    [MemberData(nameof(PanelAdministrators))]
    public async Task ThePanelForm_OffersAScopeAndSpeciality_ExactlyWhenCreatingThatPanelIsAccepted(string who)
    {
        // Picker = gate: the form's offer is read from the rule creating a panel demands (PanelReachAsync), and for every
        // caller, scope and speciality the two agree. Until T194 the form decided from roles, and a Speciality or
        // SubSpecialityAdmin was offered the Speciality scope with no speciality to choose.
        await using var db = await SeededDbAsync();
        var caller = PanelCaller(who);

        var options = await new GetDecisionPanelFormOptionsQueryHandler(db).Handle(
            new GetDecisionPanelFormOptionsQuery(caller), CancellationToken.None);

        (await CreatesAsync(db, caller, DecisionPanelScope.Institution, null))
            .Should().Be(options.MayCreateInstitutionWide, "the institution-wide scope");
        foreach (var speciality in new[] { Paediatrics, Surgery })
        {
            var offered = options.Specialities is null || options.Specialities.Any(offer => offer.Id == speciality);
            (await CreatesAsync(db, caller, DecisionPanelScope.Speciality, speciality))
                .Should().Be(offered, $"speciality {speciality}");
        }
    }

    [Theory]
    [InlineData("SpecialityAdmin of A (Paediatrics)", Paediatrics, "Paediatrics")]
    [InlineData("SubSpecialityAdmin of A (General Surgery)", Surgery, "Surgery")]
    public async Task ASpecialityOrSubSpecialityAdmin_IsOfferedOnlyTheirOwnSpeciality_AndNoInstitutionWidePanel(
        string who, int specialityId, string name)
    {
        await using var db = await SeededDbAsync();

        var options = await new GetDecisionPanelFormOptionsQueryHandler(db).Handle(
            new GetDecisionPanelFormOptionsQuery(PanelCaller(who)), CancellationToken.None);

        options.MayCreateInstitutionWide.Should().BeFalse();
        options.Specialities.Should().ContainSingle().Which.Should().Match<Wombat.Application.Features.Institutions.SpecialityDto>(
            speciality => speciality.Id == specialityId && speciality.Name == name);
        options.MayCreateAny.Should().BeTrue();
    }

    [Fact]
    public async Task ASpecialityClaim_CountsOnlyWithTheSpecialityAdminRole_SoItOffersAndAdmitsNoOtherPanel()
    {
        // Scope claims are issued per user, not per role (WombatUserClaimsPrincipalFactory): a General Surgery
        // SubSpecialityAdmin who was also given a Paediatrics speciality scope, with an Assessor invitation say, holds a
        // Paediatrics speciality claim. It is the SpecialityAdmin role's scope, and lends this caller nothing: they are
        // offered, may create and may change Surgery's panels only.
        await using var db = await SeededDbAsync();
        var caller = PanelCaller("SubSpecialityAdmin of A (General Surgery) with a Paediatrics claim");
        var before = await PanelsSnapshotAsync();

        var options = await new GetDecisionPanelFormOptionsQueryHandler(db).Handle(
            new GetDecisionPanelFormOptionsQuery(caller), CancellationToken.None);
        var update = await RefusalAsync(() => RunOnPanelAsync(db, "UpdatePanel", PaediatricsPanelA, caller));
        await SaveAndClearAsAuditPipelineWouldAsync(db);

        options.MayCreateInstitutionWide.Should().BeFalse();
        options.Specialities!.Select(speciality => speciality.Id).Should().Equal(Surgery);
        update.Should().BeOfType<UnauthorizedAccessException>();
        (await PanelsSnapshotAsync()).Should().BeEquivalentTo(before);
        (await CreatesAsync(db, caller, DecisionPanelScope.Speciality, Paediatrics)).Should().BeFalse();
        (await CreatesAsync(db, caller, DecisionPanelScope.Speciality, Surgery)).Should().BeTrue();
    }

    [Theory]
    [InlineData("SpecialityAdmin with no institution")]
    [InlineData("Coordinator of A")]
    public async Task SomeoneWhoMayCreateNoPanel_IsOfferedNone(string who)
    {
        await using var db = await SeededDbAsync();

        var options = await new GetDecisionPanelFormOptionsQueryHandler(db).Handle(
            new GetDecisionPanelFormOptionsQuery(PanelCaller(who)), CancellationToken.None);

        options.MayCreateInstitutionWide.Should().BeFalse();
        options.Specialities.Should().BeEmpty();
        options.MayCreateAny.Should().BeFalse();
    }

    private static ClaimsPrincipal PanelCaller(string who) => who switch
    {
        "Administrator" => TestPrincipals.Administrator(),
        "InstitutionalAdmin of A" => TestPrincipals.InstitutionalAdmin(InstitutionA),
        "SpecialityAdmin of A (Paediatrics)" => TestPrincipals.InRole(
            WombatRoles.SpecialityAdmin, "spec-admin", InstitutionA, specialityId: Paediatrics),
        "SubSpecialityAdmin of A (General Surgery)" => TestPrincipals.InRole(
            WombatRoles.SubSpecialityAdmin, "sub-admin", InstitutionA, subSpecialityId: GeneralSurgery),
        "SubSpecialityAdmin of A (General Surgery) with a Paediatrics claim" => TestPrincipals.InRole(
            WombatRoles.SubSpecialityAdmin, "sub-admin-paeds-claim", InstitutionA, Paediatrics, GeneralSurgery),
        "SpecialityAdmin with no institution" => TestPrincipals.InRole(
            WombatRoles.SpecialityAdmin, "spec-admin-nowhere", null, specialityId: Paediatrics),
        "Coordinator of A" => TestPrincipals.Coordinator(InstitutionA),
        _ => throw new ArgumentOutOfRangeException(nameof(who), who, null)
    };

    /// <summary>Whether creating this panel at A is accepted; a refusal is asserted to have written nothing.</summary>
    private async Task<bool> CreatesAsync(
        ApplicationDbContext db, ClaimsPrincipal caller, DecisionPanelScope scope, int? specialityId)
    {
        var panelsBefore = (await PanelsSnapshotAsync()).Count;
        try
        {
            await new CreateDecisionPanelCommandHandler(
                    db, FakeUserDirectory.CommitteeMembersAt(InstitutionA, "chair-a", "member-a")).Handle(
                new CreateDecisionPanelCommand(
                    $"{scope} {specialityId}",
                    scope,
                    // Anyone but an Administrator creates at their own institution; an Administrator names it.
                    caller.IsInRole(WombatRoles.Administrator) || scope == DecisionPanelScope.Institution ? InstitutionA : null,
                    specialityId,
                    [
                        new DecisionPanelMemberInput("chair-a", DecisionPanelMemberRole.Chair),
                        new DecisionPanelMemberInput("member-a", DecisionPanelMemberRole.Member)
                    ],
                    caller),
                CancellationToken.None);
            db.ChangeTracker.Clear();
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            await SaveAndClearAsAuditPipelineWouldAsync(db);
            (await PanelsSnapshotAsync()).Should().HaveCount(panelsBefore, "a refused create writes nothing");
            return false;
        }
    }

    // ─── The scheduling page's panel list (T194 item 3) ──────────────────────

    [Fact]
    public async Task AnExternalMemberFromAnotherInstitution_IsNotOfferedThatPanelToScheduleOn_ThoughTheyListIt()
    {
        // A Coordinator of A who sits as an External member on B's panel. They schedule nobody before it: a panel reviews
        // its own institution's trainees, and a Coordinator oversees only their own institution's.
        await using var db = await SeededDbAsync();
        var coordinator = TestPrincipals.Coordinator(InstitutionA, CoordinatorOfA);

        var listed = await PanelIdsAsync(db, coordinator, forScheduling: false);
        var offered = await PanelIdsAsync(db, coordinator, forScheduling: true);

        listed.Should().Contain(PanelB, "the panels page lists a panel they sit on");
        offered.Should().NotContain(PanelB);
        offered.Should().BeEquivalentTo(new[] { PanelA, PaediatricsPanelA });
        (await TraineesOfferedAsync(db, coordinator, PanelB)).Should().BeEmpty("the picker offers nobody on it");
    }

    [Fact]
    public async Task ASurgerySpecialityAdmin_IsNotOfferedThePaediatricsPanelToScheduleOn()
    {
        await using var db = await SeededDbAsync();
        var surgeryAdmin = TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "surgery-admin", InstitutionA, specialityId: Surgery);

        (await PanelIdsAsync(db, surgeryAdmin, forScheduling: true)).Should().Equal(PanelA);
        (await PanelIdsAsync(db, surgeryAdmin, forScheduling: false)).Should().Contain(PaediatricsPanelA);
    }

    public static TheoryData<string, bool> Schedulers
    {
        get
        {
            var data = new TheoryData<string, bool>();
            foreach (var who in new[]
                     {
                         "Administrator", "InstitutionalAdmin of A", "Coordinator of A who sits on B's panel",
                         "SpecialityAdmin of A (Paediatrics)", "SubSpecialityAdmin of A (General Surgery)",
                         "CommitteeMember of A", "Trainee who coordinates at A"
                     })
            {
                data.Add(who, false);
                data.Add(who, true);
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(Schedulers))]
    public async Task TheSchedulingPanelList_OffersAPanel_ExactlyWhenItsTraineePickerOffersSomeone(
        string who, bool paedsAtAIsErased)
    {
        // Picker = gate, one level up: the panel list and the trainee picker ask the one scheduling rule, and apply the
        // one filter to it, with or without an erased trainee in reach (T194 review).
        await using var db = await SeededDbAsync();
        var directory = paedsAtAIsErased ? WithPaedsAtAErased : EveryoneNamed;
        var caller = who switch
        {
            "Coordinator of A who sits on B's panel" => TestPrincipals.Coordinator(InstitutionA, CoordinatorOfA),
            "CommitteeMember of A" => Member("member-a", InstitutionA),
            "Trainee who coordinates at A" => TestPrincipals.InRoles(
                [WombatRoles.Trainee, WombatRoles.Coordinator], "registrar-a", InstitutionA),
            _ => PanelCaller(who)
        };

        var offered = await PanelIdsAsync(db, caller, forScheduling: true, directory);

        foreach (var panelId in new[] { PanelA, PanelB, PaediatricsPanelA })
        {
            offered.Contains(panelId).Should().Be(
                (await TraineesOfferedAsync(db, caller, panelId, directory)).Count > 0, $"panel {panelId}");
        }
    }

    [Fact]
    public async Task APanelWhoseOnlySchedulableTraineeWasErased_IsNotOffered_AsItsPickerOffersNobody()
    {
        // An erased trainee's profile keeps its institution and programme under a pseudonym that names no account
        // (ErasureExecutor), so the scheduling rule still accepts it. The picker leaves it out (T182); until the T194
        // review the panel list counted it, and offered the Paediatrics panel with nobody to choose.
        await using var db = await SeededDbAsync();
        var admin = TestPrincipals.InstitutionalAdmin(InstitutionA);

        var offered = await PanelIdsAsync(db, admin, forScheduling: true, WithPaedsAtAErased);

        offered.Should().Equal(PanelA);
        (await TraineesOfferedAsync(db, admin, PaediatricsPanelA, WithPaedsAtAErased)).Should().BeEmpty();
        (await TraineesOfferedAsync(db, admin, PanelA, WithPaedsAtAErased)).Select(trainee => trainee.UserId)
            .Should().Equal(SurgeryAtA);
    }

    private static FakeUserDirectory EveryoneNamed
        => new((PaedsAtA, "Palesa Paeds"), (SurgeryAtA, "Sipho Surgery"), (PaedsAtB, "Bongani Paeds"));

    /// <summary>A directory in which the paediatric trainee at A has no account: their profile was erased.</summary>
    private static FakeUserDirectory WithPaedsAtAErased
        => new((SurgeryAtA, "Sipho Surgery"), (PaedsAtB, "Bongani Paeds"));

    private static async Task<IReadOnlyList<int>> PanelIdsAsync(
        ApplicationDbContext db, ClaimsPrincipal caller, bool forScheduling, FakeUserDirectory? directory = null)
        => (await new ListDecisionPanelsQueryHandler(db, directory ?? EveryoneNamed).Handle(
                new ListDecisionPanelsQuery(caller, forScheduling), CancellationToken.None))
            .Select(panel => panel.Id)
            .ToArray();

    private static async Task<IReadOnlyList<SchedulableTraineeDto>> TraineesOfferedAsync(
        ApplicationDbContext db, ClaimsPrincipal caller, int panelId, FakeUserDirectory? directory = null)
        => await new ListSchedulableTraineesQueryHandler(db, directory ?? EveryoneNamed)
            .Handle(new ListSchedulableTraineesQuery(panelId, caller), CancellationToken.None);

    // ─── Fixture ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Panel A's review of a paediatric trainee at A, in <paramref name="state" />; null for a formative review in progress,
    /// which takes no entrustment decision and holds no agenda. A binding review in progress or later holds one agenda line
    /// and one frozen evidence line.
    /// </summary>
    private async Task<int> SeedReviewAsync(ApplicationDbContext db, CommitteeReviewState? state)
    {
        var now = new DateTime(2027, 1, 8, 9, 0, 0, DateTimeKind.Utc);
        var review = new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 2,
            TraineeUserId = PaedsAtA,
            PanelId = PanelA,
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
            review.Start([line], agenda, "chair-a", now);
        }

        db.CommitteeReviews.Add(review);
        await db.SaveChangesAsync();

        if (state is CommitteeReviewState.Decided or CommitteeReviewState.Ratified or CommitteeReviewState.UnderAppeal)
        {
            // Recording demands a settled agenda (T131 slice 4): the one closing line is staged first.
            db.Set<PendingEntrustmentDecision>().Add(PendingEntrustmentDecision.Stage(
                review.Id, EpaId, LevelId, new DateOnly(2027, 1, 8), null, "Ready.", [line.Id], "chair-a", now));
            review.RecordDecision(
                CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, "chair-a", now,
                [
                    new DecisionPanelMember { UserId = "chair-a", Role = DecisionPanelMemberRole.Chair },
                    new DecisionPanelMember { UserId = "member-a", Role = DecisionPanelMemberRole.Member }
                ], [], []);
        }

        if (state is CommitteeReviewState.Ratified or CommitteeReviewState.UnderAppeal)
        {
            review.Ratify("chair-a", now);
        }

        if (state is CommitteeReviewState.UnderAppeal)
        {
            review.LodgeAppeal("The window missed my rotation.", PaedsAtA, now);
        }

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        (await ReviewStateAsync(review.Id)).Should().Be(state ?? CommitteeReviewState.InProgress, "the fixture's state");
        return review.Id;
    }

    private static async Task<int> SeedDecisionAsync(ApplicationDbContext db, string traineeUserId, int panelId)
    {
        var review = new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 2,
            TraineeUserId = traineeUserId,
            PanelId = panelId,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 12, 31),
            ScheduledOn = new DateOnly(2027, 1, 8)
        };
        var line = new CommitteeEvidence
        {
            SourceType = CommitteeEvidenceSourceType.Activity,
            ActivityId = 901,
            EpaId = EpaId,
            SourceLabel = "Mini-CEX #901",
            Summary = "State: completed.",
            ObservedOn = new DateOnly(2026, 6, 1)
        };
        review.Start([line], "chair-b", new DateTime(2027, 1, 8, 9, 0, 0, DateTimeKind.Utc));
        db.CommitteeReviews.Add(review);
        await db.SaveChangesAsync();

        var decision = EntrustmentDecision.Issue(
            traineeUserId, EpaId, LevelId, new DateOnly(2027, 1, 8), null, review.Id, "chair-b", "Ready.",
            [EntrustmentEvidenceLink.FromSnapshot(line)]);
        db.Set<EntrustmentDecision>().Add(decision);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return decision.Id;
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
            new SubSpeciality { Id = GeneralSurgery, SpecialityId = Surgery, Name = "General Surgery", IsActive = true });
        db.Curricula.AddRange(
            new Curriculum { Id = 100, SubSpecialityId = GeneralPaediatrics, Name = "General Paediatrics", Version = "11.1" },
            new Curriculum { Id = 200, SubSpecialityId = GeneralSurgery, Name = "General Surgery", Version = "1" });

        AddProfile(db, 1, PaedsAtA, InstitutionA, 100);
        AddProfile(db, 2, SurgeryAtA, InstitutionA, 200);
        AddProfile(db, 3, PaedsAtB, InstitutionB, 100);

        var paediatricsPanel = Panel(PaediatricsPanelA, InstitutionA, "chair-a", "member-a", "external-a");
        paediatricsPanel.Scope = DecisionPanelScope.Speciality;
        paediatricsPanel.SpecialityId = Paediatrics;
        db.DecisionPanels.AddRange(
            Panel(PanelA, InstitutionA, "chair-a", "member-a", "external-a"),
            // B's panel seats a Coordinator of A as its External member: the case T194 item 3 names.
            Panel(PanelB, InstitutionB, "chair-b", "member-b", CoordinatorOfA),
            paediatricsPanel);

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

    private static void AddProfile(ApplicationDbContext db, int id, string userId, int institutionId, int curriculumId)
        => db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = id,
            UserId = userId,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            ProgrammeStartDate = new DateOnly(2024, 1, 15),
            ExpectedCompletionDate = new DateOnly(2028, 1, 15),
            IsActive = true
        });

    private static ClaimsPrincipal Member(string userId, int institutionId)
        => TestPrincipals.InRole(WombatRoles.CommitteeMember, userId, institutionId);

    // ─── The store ───────────────────────────────────────────────────────────

    private static async Task SaveAndClearAsAuditPipelineWouldAsync(ApplicationDbContext db)
    {
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    private async Task<CommitteeReviewState> ReviewStateAsync(int reviewId)
    {
        await using var read = CreateDb();
        return await read.CommitteeReviews.Where(review => review.Id == reviewId).Select(review => review.State).SingleAsync();
    }

    private async Task<IReadOnlyList<string>> SnapshotAsync()
    {
        await using var read = CreateDb();
        var rows = new List<string>();
        rows.AddRange(await read.CommitteeReviews.OrderBy(review => review.Id)
            .Select(review => $"review {review.Id}:{review.State}:{review.StartedOn}:{review.RatifiedOn}:{review.FinalizedOn}")
            .ToListAsync());
        rows.AddRange(await read.Set<CommitteeDecision>().OrderBy(decision => decision.Id)
            .Select(decision => $"decision {decision.Id}:{decision.Category}")
            .ToListAsync());
        rows.AddRange(await read.Set<CommitteeAppeal>().OrderBy(appeal => appeal.Id)
            .Select(appeal => $"appeal {appeal.Id}:{appeal.ResolvedOn}:{appeal.Outcome}")
            .ToListAsync());
        rows.AddRange(await read.Set<PendingEntrustmentDecision>().OrderBy(pending => pending.Id)
            .Select(pending => $"pending {pending.Id}:{pending.EpaId}")
            .ToListAsync());
        rows.AddRange(await read.Set<CommitteeAgendaLine>().OrderBy(line => line.Id)
            .Select(line => $"line {line.Id}:{line.State}:{line.DeferralReason}")
            .ToListAsync());
        rows.AddRange(await read.Set<EntrustmentDecision>().OrderBy(decision => decision.Id)
            .Select(decision => $"star {decision.Id}:{decision.Status}")
            .ToListAsync());
        return rows;
    }

    private async Task<IReadOnlyList<string>> PanelsSnapshotAsync()
    {
        await using var read = CreateDb();
        return await read.DecisionPanels.OrderBy(panel => panel.Id)
            .Select(panel => $"{panel.Id}:{panel.DecisionBodyKey}:" +
                             string.Join(",", panel.Members.OrderBy(member => member.UserId).Select(member => member.UserId)))
            .ToListAsync();
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
}
