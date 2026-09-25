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

    /// <summary>A's Paediatrics panel: it covers one speciality, so it reviews only that speciality's trainees (T131).</summary>
    private const int PaediatricsPanelA = 30;

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

        // A trainee has one open binding review per panel and period (T131 slice 4), so the first one goes before the
        // second scheduler is asked about the same trainee on the same panel: this test is about scope alone.
        db.CommitteeReviews.RemoveRange(await db.CommitteeReviews.ToListAsync());
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
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
        var panel = await new CreateDecisionPanelCommandHandler(db, Committee).Handle(
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

    // ─── A trainee first (T216) ──────────────────────────────────────────────
    //
    // T185's rung: someone who holds Trainee beside a role that schedules, the Administrator's included, schedules nobody,
    // previews nobody's agenda, and is offered nobody. Each case has its control: the same sign-in without the Trainee
    // role is accepted, so the refusal is the rung's and not the fixture's.

    private const string TraineeRefusal =
        "You hold the Trainee role, so you cannot schedule a committee review or preview its agenda, including your own.";

    /// <summary>A registrar with no programme in the fixture, so every trainee in it is a peer.</summary>
    private const string Registrar = "registrar-a";

    public static TheoryData<string> RolesThatSchedule => new()
    {
        "Coordinator", "InstitutionalAdmin", "SpecialityAdmin", "SubSpecialityAdmin", "Administrator"
    };

    [Theory]
    [MemberData(nameof(RolesThatSchedule))]
    public async Task ATraineeWhoAlsoSchedules_IsRefusedAPeersReview_BeforeEitherIdIsLookedUp_AndNothingIsWritten(string role)
    {
        await using var db = await SeededDbAsync();
        var registrar = TraineeWho(role, Registrar);

        foreach (var (panelId, traineeUserId) in new[]
                 {
                     (PanelA, PaedsAtA), (PanelA, SurgeryAtA), (PanelB, PaedsAtB), (999, PaedsAtA), (PanelA, "nobody-by-this-id")
                 })
        {
            // One refusal for every pair, an unknown panel included: an Administrator who is also a trainee is not told
            // that a panel id names nothing, or where a trainee trains.
            var refusal = await RefusalAsync(() => ScheduleAsync(db, registrar, panelId, traineeUserId));

            refusal.Should().BeOfType<UnauthorizedAccessException>()
                .Which.Message.Should().Be(TraineeRefusal, $"{role}: panel {panelId}, {traineeUserId}");
        }

        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await ReviewCountAsync()).Should().Be(0);

        (await AcceptsAsync(db, WithoutTrainee(registrar), PanelA, PaedsAtA))
            .Should().BeTrue($"the same {role} without the Trainee role schedules the peer");
    }

    [Theory]
    [MemberData(nameof(RolesThatSchedule))]
    public async Task ATraineeWhoAlsoSchedules_CannotScheduleTheirOwnReview_Either(string role)
    {
        // A trainee does not choose which panel sits on them, for which period, or on which evidence.
        await using var db = await SeededDbAsync();
        var paedsWhoAlsoSchedules = TraineeWho(role, PaedsAtA);

        var refusal = await RefusalAsync(() => ScheduleAsync(db, paedsWhoAlsoSchedules, PanelA, PaedsAtA));

        refusal.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(TraineeRefusal);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await ReviewCountAsync()).Should().Be(0);

        (await AcceptsAsync(db, WithoutTrainee(paedsWhoAlsoSchedules), PanelA, PaedsAtA)).Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(RolesThatSchedule))]
    public async Task ATraineeWhoAlsoSchedules_PreviewsNoAgenda_APeersOrTheirOwn(string role)
    {
        // The preview names the trainee's standing on every EPA due: a peer's record.
        await using var db = await SeededDbAsync();
        var registrar = TraineeWho(role, Registrar);
        var paedsWhoAlsoSchedules = TraineeWho(role, PaedsAtA);

        foreach (var (principal, panelId) in new[]
                 {
                     (registrar, PanelA), (registrar, 999), (registrar, PanelB), (paedsWhoAlsoSchedules, PanelA)
                 })
        {
            var refusal = await RefusalAsync(() => PreviewAsync(db, principal, panelId, PaedsAtA));

            refusal.Should().BeOfType<UnauthorizedAccessException>()
                .Which.Message.Should().Be(TraineeRefusal, $"{role}: panel {panelId}");
        }

        var preview = await PreviewAsync(db, WithoutTrainee(registrar), PanelA, PaedsAtA);
        preview.PeriodLabel.Should().Be("2026 S2", $"the same {role} without the Trainee role previews the peer's agenda");
        preview.TraineeHasCurriculum.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(RolesThatSchedule))]
    public async Task ThePicker_OffersATraineeWhoAlsoSchedules_Nobody_ThemselvesIncluded(string role)
    {
        await using var db = await SeededDbAsync();
        var registrar = TraineeWho(role, Registrar);
        var paedsWhoAlsoSchedules = TraineeWho(role, PaedsAtA);

        foreach (var panelId in new[] { PanelA, PaediatricsPanelA, PanelB })
        {
            (await PickerAsync(db, registrar, panelId)).Should().BeEmpty($"{role} on panel {panelId}");
            (await PickerAsync(db, paedsWhoAlsoSchedules, panelId)).Should().BeEmpty($"{role} on panel {panelId}");
        }

        (await PickerAsync(db, WithoutTrainee(registrar), PanelA)).Select(trainee => trainee.UserId)
            .Should().Contain(PaedsAtA, $"the same {role} without the Trainee role is offered A's trainees");
    }

    private const string MayNotSchedule = "You are not allowed to schedule committee reviews.";

    [Fact]
    public async Task OnlyATraineeWhoAlsoHoldsARoleThatSchedules_IsToldTheTraineeRoleIsWhy()
    {
        // A trainee alone, or one who also sits on a committee, holds no role that schedules: dropping Trainee would not
        // let them schedule, so they are not told that it is the reason (T216 review).
        await using var db = await SeededDbAsync();

        foreach (var (caller, principal, expected) in new[]
                 {
                     ("a trainee alone", TestPrincipals.Trainee(PaedsAtA, InstitutionA), MayNotSchedule),
                     ("a trainee on the committee", TraineeWho("CommitteeMember", "member-a"), MayNotSchedule),
                     ("a committee member", Member("member-a", InstitutionA), MayNotSchedule),
                     ("a trainee who coordinates", TraineeWho("Coordinator", Registrar), TraineeRefusal)
                 })
        {
            var schedule = await RefusalAsync(() => ScheduleAsync(db, principal, PanelA, PaedsAtA));
            var preview = await RefusalAsync(() => PreviewAsync(db, principal, PanelA, PaedsAtA));

            schedule.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(expected, caller);
            preview.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(expected, caller);
        }

        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await ReviewCountAsync()).Should().Be(0);
    }

    // ─── The committee reviews page (T216) ───────────────────────────────────
    //
    // The page's list names each review's trainee and the committee's outcome, and the review itself refuses someone who
    // holds Trainee every review but their own once ratified (DemandReviewAccess). So such a caller is listed none, a
    // peer's or their own, whatever other role brings them to the page. Each case has its control without Trainee.

    public static TheoryData<string> RolesThatListReviews => new()
    {
        "Coordinator", "InstitutionalAdmin", "SpecialityAdmin", "SubSpecialityAdmin", "Administrator", "CommitteeMember"
    };

    [Theory]
    [MemberData(nameof(RolesThatListReviews))]
    public async Task TheReviewList_ListsATraineeWhoAlsoHoldsAnotherRole_NoReview_APeersOrTheirOwn(string role)
    {
        await using var db = await SeededDbAsync();

        // member-a sits on panel A, so every role's own reach lists panel A's reviews: a member's through the seat.
        var peers = await SeedReviewForAsync(db, "Ratify", PaedsAtA);
        var own = await SeedReviewForAsync(db, "Start", "member-a");
        var registrar = TraineeWho(role, "member-a");

        (await ListAsync(db, registrar)).Should().BeEmpty($"{role} who also holds Trainee");

        var control = await ListAsync(db, WithoutTrainee(registrar));
        control.Select(review => review.Id).Should().Contain(
            [peers, own], $"the same {role} without the Trainee role lists panel A's reviews");
        var peersRow = control.Single(review => review.Id == peers);
        peersRow.TraineeName.Should().Be("Palesa Paeds");
        peersRow.CurrentDecisionCategory.Should().Be(
            CommitteeDecisionCategory.SatisfactoryProgress, "the row shows the committee's outcome, which is why it is withheld");
    }

    [Theory]
    [MemberData(nameof(RolesThatListReviews))]
    public async Task TheReviewsPage_OffersScheduling_ExactlyToWhomTheHandlersAdmit_AndSaysWhyToATrainee(string role)
    {
        await using var db = await SeededDbAsync();
        var schedules = role != "CommitteeMember";

        foreach (var (holdsTrainee, principal) in new[]
                 {
                     (true, TraineeWho(role, Registrar)), (false, WithoutTrainee(TraineeWho(role, Registrar)))
                 })
        {
            var access = await new GetCommitteeReviewsAccessQueryHandler().Handle(
                new GetCommitteeReviewsAccessQuery(principal), CancellationToken.None);

            // The flag is the handlers' own first rule: the preview (which writes nothing) is refused by that rule exactly
            // when the page offers no scheduling.
            var refusedByTheRule = await PreviewRefusedByTheSchedulingRuleAsync(db, principal);
            access.MaySchedule.Should().Be(!refusedByTheRule, $"{role}, Trainee: {holdsTrainee}");
            access.MaySchedule.Should().Be(schedules && !holdsTrainee, $"{role}, Trainee: {holdsTrainee}");

            access.TraineeNote.Should().Be(
                !holdsTrainee ? null : schedules ? TraineeSchedulesAndListsNoReview : TraineeListsNoReview,
                $"{role}, Trainee: {holdsTrainee}");
        }
    }

    private const string TraineeSchedulesAndListsNoReview =
        "You hold the Trainee role, so you cannot schedule a committee review or preview its agenda, and this page lists " +
        "no one's reviews. Your own are on My committee reviews once they are ratified.";

    private const string TraineeListsNoReview =
        "You hold the Trainee role, so this page lists no one's committee reviews. Your own are on My committee reviews " +
        "once they are ratified.";

    /// <summary>Whether the agenda preview is refused by the rule that admits a caller to scheduling at all.</summary>
    private static async Task<bool> PreviewRefusedByTheSchedulingRuleAsync(ApplicationDbContext db, ClaimsPrincipal principal)
    {
        try
        {
            await PreviewAsync(db, principal, PanelA, PaedsAtA);
            return false;
        }
        catch (UnauthorizedAccessException refusal) when (refusal.Message is MayNotSchedule or TraineeRefusal)
        {
            return true;
        }
    }

    // ─── A panel that covers one speciality (T131 slice 3, T194 item 2) ──────

    [Theory]
    [MemberData(nameof(SchedulersOfA))]
    public async Task ASpecialityPanel_RefusesATraineeOfAnotherSpeciality_AndNothingIsWritten(string role)
    {
        // Before T131 a Paediatrics panel took a Surgery trainee at the same hospital: only the institution was checked.
        await using var db = await SeededDbAsync();
        var scheduler = Scheduler(role, InstitutionA);

        var act = () => ScheduleAsync(db, scheduler, PaediatricsPanelA, SurgeryAtA);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await ReviewCountAsync()).Should().Be(0);

        (await AcceptsAsync(db, scheduler, PaediatricsPanelA, PaedsAtA)).Should().BeTrue();
    }

    [Fact]
    public async Task AnAdministrator_IsToldThatTheSpecialityPanelCoversAnotherSpeciality()
    {
        await using var db = await SeededDbAsync();
        var administrator = TestPrincipals.Administrator();

        var refusal = await RefusalAsync(() => ScheduleAsync(db, administrator, PaediatricsPanelA, SurgeryAtA));

        refusal.Should().BeOfType<UnauthorizedAccessException>()
            .Which.Message.Should().Be("This panel covers one speciality, and this trainee's programme is in another.");
        (await AcceptsAsync(db, administrator, PaediatricsPanelA, NeonatologyAtA))
            .Should().BeTrue("neonatology is a paediatric sub-speciality, and the panel covers the speciality");
        (await AcceptsAsync(db, administrator, PaediatricsPanelA, PaedsAtB))
            .Should().BeFalse("a speciality id is national: the panel is still A's");
    }

    [Fact]
    public async Task ThePicker_ForASpecialityPanel_OffersOnlyThatSpecialitysTrainees()
    {
        await using var db = await SeededDbAsync();

        var offered = await PickerAsync(db, TestPrincipals.Coordinator(InstitutionA), PaediatricsPanelA);

        offered.Select(trainee => trainee.UserId).Should().BeEquivalentTo([PaedsAtA, NeonatologyAtA, LeftA]);
    }

    // ─── The picker is the gate ──────────────────────────────────────────────

    public static TheoryData<string, int> PickerCases()
    {
        var cases = new TheoryData<string, int>();
        foreach (var caller in new[]
                 {
                     "Coordinator of A", "InstitutionalAdmin of A", "SpecialityAdmin of A", "SubSpecialityAdmin of A",
                     "Coordinator of B", "CommitteeMember of A", "Trainee of A", "SpecialityAdmin on the committee of A",
                     "Administrator", "Trainee who coordinates at A", "Trainee who is an Administrator"
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

    // Not Issue: since T165 a STAR is issued only by ratifying, the command that issued one directly is gone.
    public static TheoryData<string> ReviewCommands => new()
    {
        "Start", "Record", "Ratify", "Close", "ResolveAppeal", "Stage", "Remove"
    };

    public static TheoryData<string, string> ReviewCommandsForTraineesNotAtThePanel()
    {
        var cases = new TheoryData<string, string>();
        // Not ResolveAppeal: an appeal body answers an appeal against its own ratified review wherever the trainee now
        // trains (TheAppealBody_AnswersTheAppealOfATraineeWhoHasMovedAway).
        foreach (var command in new[] { "Start", "Record", "Ratify", "Close", "Stage", "Remove" })
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
    public async Task AnAdministratorWhoChairsThePanel_IsNotRefused_AReviewOfATraineeNotAtThePanel(string command)
    {
        // Since T165 the chair's actions have no Administrator bypass, so an Administrator finishing a review stranded by a
        // move takes the chair first, which takes what any seat takes: the CommitteeMember role at the panel's institution
        // (PanelSeat; the Committee directory holds them so). The trainee check still waives them (CommitteeTraineeScope).
        await using var db = await SeededDbAsync();
        await SeatAsChairOfAAsync(db, AdministratorId);
        var reviewId = await SeedReviewForAsync(db, command, PaedsAtB);

        var act = () => RunAsync(db, command, reviewId, PaedsAtB, TestPrincipals.Administrator(AdministratorId));

        await act.Should().NotThrowAsync();
    }

    public static TheoryData<string> ChairsActions => new()
    {
        "Record", "Ratify", "Close", "Stage", "Remove"
    };

    [Theory]
    [MemberData(nameof(ChairsActions))]
    public async Task AnAdministratorNotOnThePanel_IsRefusedEveryChairsAction_AndNothingChanges(string command)
    {
        // T165 (D46): the Administrator bypass is gone from the one chair gate. A global Administrator keeps panel
        // administration and read access, but one who does not sit on the panel takes no part in its decision. The trainee
        // is the panel's own, so the refusal can only be the chair gate's.
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewForAsync(db, command, PaedsAtA);
        var before = await SnapshotAsync();

        var act = () => RunAsync(db, command, reviewId, PaedsAtA, TestPrincipals.Administrator(AdministratorId));

        (await act.Should().ThrowAsync<UnauthorizedAccessException>())
            .Which.Message.Should().Be(ChairRefusal(command));
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await SnapshotAsync()).Should().BeEquivalentTo(before);
    }

    [Theory]
    [MemberData(nameof(ChairsActions))]
    public async Task APanelMemberWhoIsNotTheChair_IsRefusedEveryChairsAction_AndNothingChanges(string command)
    {
        await using var db = await SeededDbAsync();
        var reviewId = await SeedReviewForAsync(db, command, PaedsAtA);
        var before = await SnapshotAsync();

        var act = () => RunAsync(db, command, reviewId, PaedsAtA, Member("member-a", InstitutionA));

        (await act.Should().ThrowAsync<UnauthorizedAccessException>())
            .Which.Message.Should().Be(ChairRefusal(command));
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await SnapshotAsync()).Should().BeEquivalentTo(before);
    }

    /// <summary>
    /// The chair gate's refusal for each action. Ratify, Stage and Remove look the review up and authorise first, with the
    /// one refusal that does not say whether the id names a review (T131, T194 item 1); Record and Close say the action
    /// is the chair's. Neither has an Administrator bypass (T165, D46).
    /// </summary>
    private static string ChairRefusal(string command)
        => command is "Ratify" or "Stage" or "Remove"
            ? "The committee review could not be found among the reviews you chair."
            : "Only the panel's chair can do this.";

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
                2026,
                2,
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

    /// <summary>The committee reviews page's list. (T216)</summary>
    private static async Task<IReadOnlyList<CommitteeReviewListItemDto>> ListAsync(ApplicationDbContext db, ClaimsPrincipal principal)
        => await new ListReviewsForPanelQueryHandler(db, new NamedUsers()).Handle(
            new ListReviewsForPanelQuery(principal),
            CancellationToken.None);

    /// <summary>The scheduling form's agenda preview, for the period <see cref="ScheduleAsync" /> schedules. (T216)</summary>
    private static async Task<CommitteeAgendaPreviewDto> PreviewAsync(
        ApplicationDbContext db, ClaimsPrincipal principal, int panelId, string traineeUserId)
        => await new PreviewCommitteeAgendaQueryHandler(db).Handle(
            new PreviewCommitteeAgendaQuery(traineeUserId, panelId, 2026, 2, principal, new DateOnly(2026, 9, 24)),
            CancellationToken.None);

    private static async Task RunAsync(
        ApplicationDbContext db, string command, int reviewId, string traineeUserId, ClaimsPrincipal principal)
    {
        _ = command switch
        {
            "Start" => await new StartCommitteeReviewCommandHandler(db).Handle(
                new StartCommitteeReviewCommand(reviewId, principal), CancellationToken.None),
            "Record" => await new RecordCommitteeDecisionCommandHandler(db, Committee).Handle(
                new RecordCommitteeDecisionCommand(
                    reviewId, CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null,
                    // The actor and one member present (T165): the chair records, and sat.
                    [principal.FindFirst(ClaimTypes.NameIdentifier)!.Value, "member-a"], principal),
                CancellationToken.None),
            "Ratify" => await new RatifyCommitteeDecisionCommandHandler(db).Handle(
                new RatifyCommitteeDecisionCommand(reviewId, principal), CancellationToken.None),
            "Close" => await new CloseFormativeReviewCommandHandler(db).Handle(
                new CloseFormativeReviewCommand(reviewId, principal), CancellationToken.None),
            "ResolveAppeal" => await new ResolveAppealCommandHandler(db, Committee).Handle(
                new ResolveAppealCommand(reviewId, CommitteeAppealOutcome.Dismissed, null, null, null, null, principal),
                CancellationToken.None),
            "Stage" => await new StagePendingEntrustmentDecisionCommandHandler(db).Handle(
                new StagePendingEntrustmentDecisionCommand(
                    reviewId, null, EpaId, LevelId, new DateOnly(2027, 1, 8), null, "Ready for indirect supervision.",
                    [await EvidenceLineIdAsync(db, reviewId)], principal),
                CancellationToken.None),
            "Remove" => (object)await new RemovePendingEntrustmentDecisionCommandHandler(db).Handle(
                new RemovePendingEntrustmentDecisionCommand(reviewId, await PendingIdAsync(db, reviewId), principal),
                CancellationToken.None),
            _ => throw new ArgumentOutOfRangeException(nameof(command), command, null)
        };
    }

    /// <summary>The panel member each command admits: the chair for all but Start, which any member may do.</summary>
    private static ClaimsPrincipal ActorFor(string command)
        => command == "Start" ? Member("member-a", InstitutionA) : Member("chair-a", InstitutionA);

    /// <summary>The review's one snapshot line, which a staged decision names as the evidence it rests on (D38, T131).</summary>
    private static async Task<int> EvidenceLineIdAsync(ApplicationDbContext db, int reviewId)
        => await db.Set<CommitteeEvidence>()
            .Where(line => line.ReviewId == reviewId)
            .Select(line => line.Id)
            .SingleAsync();

    private static async Task<int> PendingIdAsync(ApplicationDbContext db, int reviewId)
        => await db.Set<PendingEntrustmentDecision>()
            .Where(pending => pending.ReviewId == reviewId)
            .Select(pending => pending.Id)
            .SingleAsync();

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private const string AdministratorId = "admin-user";

    /// <summary>
    /// Who may sit on panel A (T165, PanelSeat): its members, and the Administrator, who the tests that seat them as chair
    /// have given the CommitteeMember role at A, as D46 requires of an Administrator who must act.
    /// </summary>
    private static FakeUserDirectory Committee
        => FakeUserDirectory.CommitteeMembersAt(InstitutionA, "chair-a", "member-a", "external-a", AdministratorId);

    /// <summary>Panel A's chair and one member, present at the decision the fixture records (T165).</summary>
    private static IReadOnlyCollection<DecisionPanelMember> PresentAtA =>
    [
        new DecisionPanelMember { UserId = "chair-a", Role = DecisionPanelMemberRole.Chair },
        new DecisionPanelMember { UserId = "member-a", Role = DecisionPanelMemberRole.Member }
    ];

    /// <summary>Makes this user panel A's chair in chair-a's place.</summary>
    private static async Task SeatAsChairOfAAsync(ApplicationDbContext db, string userId)
    {
        var chair = await db.Set<DecisionPanelMember>()
            .SingleAsync(member => member.PanelId == PanelA && member.Role == DecisionPanelMemberRole.Chair);
        chair.UserId = userId;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    /// <summary>A review on panel A in the state the command acts on, with what the command reads.</summary>
    private async Task<int> SeedReviewForAsync(ApplicationDbContext db, string command, string traineeUserId)
    {
        var now = new DateTime(2027, 1, 8, 9, 0, 0, DateTimeKind.Utc);
        var review = new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 2,
            TraineeUserId = traineeUserId,
            PanelId = PanelA,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 12, 31),
            ScheduledOn = new DateOnly(2027, 1, 8),
            IsFormative = command == "Close"
        };

        // One frozen line, which a staged decision names as the evidence it rests on (D38, T131).
        var line = new CommitteeEvidence
        {
            SourceType = CommitteeEvidenceSourceType.Activity,
            ActivityId = 900,
            EpaId = EpaId,
            SourceLabel = "Mini-CEX #900",
            Summary = "State: completed.",
            ObservedOn = new DateOnly(2026, 6, 1)
        };

        if (command != "Start")
        {
            // A binding review starts with the EPA on its agenda (T131 slice 4), so staging on it, the Administrator's
            // included, is not a chair's line and does not re-run routing for a trainee who has since moved.
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

        if (command is "Ratify" or "ResolveAppeal")
        {
            review.RecordDecision(CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, "chair-a", now, PresentAtA);
        }

        if (command == "ResolveAppeal")
        {
            review.Ratify("chair-a", now);
        }

        if (command == "ResolveAppeal")
        {
            review.LodgeAppeal("The window missed my rotation.", traineeUserId, now);
        }

        db.CommitteeReviews.Add(review);
        await db.SaveChangesAsync();

        // Recording demands a settled agenda (T131 slice 4), so a review the chair records has its one closing line staged.
        if (command is "Ratify" or "Remove" or "Record")
        {
            db.Set<PendingEntrustmentDecision>().Add(PendingEntrustmentDecision.Stage(
                review.Id, EpaId, LevelId, new DateOnly(2027, 1, 8), null, "Ready.", [line.Id], "chair-a", now));
        }

        if (command == "Ratify")
        {
            // The trainee's current decision on the EPA, which a ratified decision would supersede.
            db.Set<EntrustmentDecision>().Add(EntrustmentDecision.Issue(
                traineeUserId, EpaId, 2, new DateOnly(2026, 1, 8), null, review.Id, "chair-a", "Earlier.",
                [EntrustmentEvidenceLink.FromSnapshot(line)]));
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

        var paediatricsPanel = Panel(PaediatricsPanelA, InstitutionA, "chair-a", "member-a", "external-a");
        paediatricsPanel.Scope = DecisionPanelScope.Speciality;
        paediatricsPanel.SpecialityId = Paediatrics;
        db.DecisionPanels.AddRange(
            Panel(PanelA, InstitutionA, "chair-a", "member-a", "external-a"),
            Panel(PanelB, InstitutionB, "chair-b", "member-b", "external-b"),
            paediatricsPanel);

        db.EntrustmentScales.Add(new EntrustmentScale { Id = 1, Name = "v11.1 rungs" });
        db.EntrustmentLevels.AddRange(
            new EntrustmentLevel { Id = 2, ScaleId = 1, Order = 2, Label = "Direct supervision" },
            new EntrustmentLevel { Id = LevelId, ScaleId = 1, Order = 3, Label = "Indirect supervision" });
        db.Epas.Add(new Epa { Id = EpaId, SubSpecialityId = GeneralPaediatrics, Code = "PAED-007", Title = "Triage", IsActive = true });
        // A STAR is granted only on an EPA of the trainee's curriculum (T167), so the General Paediatrics curriculum
        // every Stage and Ratify row's trainee follows holds the EPA they stage.
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
        "Trainee who coordinates at A" => TraineeWho("Coordinator", Registrar),
        "Trainee who is an Administrator" => TraineeWho("Administrator", Registrar),
        _ => throw new ArgumentOutOfRangeException(nameof(caller), caller, null)
    };

    /// <summary>
    /// Someone who holds Trainee and <paramref name="role" /> on one sign-in at A, with the scope claims
    /// <see cref="Scheduler" /> gives that role. (T216)
    /// </summary>
    private static ClaimsPrincipal TraineeWho(string role, string userId) => role switch
    {
        "Coordinator" or "InstitutionalAdmin" or "Administrator" => TestPrincipals.InRoles(
            [WombatRoles.Trainee, role], userId, InstitutionA),
        "SpecialityAdmin" => TestPrincipals.InRoles(
            [WombatRoles.Trainee, WombatRoles.SpecialityAdmin], userId, InstitutionA, specialityId: Paediatrics),
        "SubSpecialityAdmin" => TestPrincipals.InRoles(
            [WombatRoles.Trainee, WombatRoles.SubSpecialityAdmin], userId, InstitutionA, subSpecialityId: GeneralPaediatrics),
        "CommitteeMember" => TestPrincipals.InRoles([WombatRoles.Trainee, WombatRoles.CommitteeMember], userId, InstitutionA),
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
    };

    /// <summary>The same sign-in, every claim kept but the Trainee role: the control for each T216 refusal.</summary>
    private static ClaimsPrincipal WithoutTrainee(ClaimsPrincipal principal)
        => new(new ClaimsIdentity(
            principal.Claims.Where(claim => !(claim.Type == ClaimTypes.Role && claim.Value == WombatRoles.Trainee)),
            "test"));

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
        IReadOnlyList<string> EntrustmentDecisions,
        IReadOnlyList<string> Attendees);

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
                .ToListAsync(),
            await read.Set<CommitteeDecisionAttendee>().OrderBy(attendee => attendee.Id)
                .Select(attendee => $"{attendee.DecisionId}:{attendee.UserId}:{attendee.Role}")
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
