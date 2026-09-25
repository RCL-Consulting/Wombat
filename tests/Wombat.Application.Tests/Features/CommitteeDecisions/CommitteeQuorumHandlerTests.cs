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
/// A committee decision is taken by a quorum: a panel holds the chair and at least one other, each of whom may sit on it;
/// the chair records who was present; the STARs staged at the review are fixed with the decision; only the panel's chair
/// ratifies a decision whose attendance holds the chair and one other; and an appeal that remits the decision records
/// its own quorum. (T165, D46)
/// </summary>
/// <remarks>
/// Every refusal below is followed by the save the audit pipeline makes from its catch and a cleared change tracker, and
/// the store is read back through a second context: a check that ran after a mutation would have the refusal commit it.
/// </remarks>
public sealed class CommitteeQuorumHandlerTests
{
    private const int InstitutionId = 1;
    private const int OtherInstitutionId = 2;
    private const int PanelId = 20;
    private const int ReviewId = 30;
    private const int EpaId = 7;
    private const int SecondEpaId = 8;
    private const int LevelId = 3;
    private const int EvidenceOnEpa = 40;
    private const int EvidenceOnSecondEpa = 41;
    private const string Trainee = "trainee-1";

    private readonly string _databaseName = Guid.NewGuid().ToString();

    /// <summary>
    /// The user store: the panel's three members, each an active committee member at the panel's institution, and the
    /// people who are not what a seat needs: deactivated, without the CommitteeMember role, or at another institution. An
    /// erased member's pseudonym and a forged id are in the store as nobody.
    /// </summary>
    private readonly FakeUserDirectory _directory =
        FakeUserDirectory.CommitteeMembersAt(InstitutionId, "chair-1", "member-1", "external-1")
            .With(new UserIdentityDetails(
                "deactivated-1", "gone@test", "Gone", "Away", InstitutionId, [], [], [WombatRoles.CommitteeMember],
                IsLockedOut: true, IsDeactivated: true))
            .With(new UserIdentityDetails(
                "assessor-1", "assessor@test", "Only", "Assessor", InstitutionId, [], [], [WombatRoles.Assessor]))
            .With(new UserIdentityDetails(
                "elsewhere-1", "moved@test", "Moved", "Elsewhere", OtherInstitutionId, [], [], [WombatRoles.CommitteeMember]));

    // ─── Panel composition ───────────────────────────────────────────────────

    public static TheoryData<string, DecisionPanelMemberInput[], string> PanelsThatCannotDecide => new()
    {
        { "one member", [Input("chair-1", DecisionPanelMemberRole.Chair)], DecisionPanelComposition.TooFewMembers },
        {
            "the chair twice",
            [Input("chair-1", DecisionPanelMemberRole.Chair), Input("chair-1", DecisionPanelMemberRole.Member)],
            DecisionPanelComposition.MemberListedTwice
        },
        {
            "one person, padded",
            [Input("chair-1", DecisionPanelMemberRole.Chair), Input(" chair-1 ", DecisionPanelMemberRole.Member)],
            DecisionPanelComposition.MemberListedTwice
        },
        {
            "no chair",
            [Input("member-1", DecisionPanelMemberRole.Member), Input("member-2", DecisionPanelMemberRole.External)],
            DecisionPanelComposition.ExactlyOneChair
        },
        {
            "two chairs",
            [Input("chair-1", DecisionPanelMemberRole.Chair), Input("chair-2", DecisionPanelMemberRole.Chair)],
            DecisionPanelComposition.ExactlyOneChair
        },
        {
            "a blank member",
            [Input("chair-1", DecisionPanelMemberRole.Chair), Input(" ", DecisionPanelMemberRole.Member)],
            "Every panel member must be named."
        },
        { "nobody", [], "'Members' must not be empty." }
    };

    [Theory]
    [MemberData(nameof(PanelsThatCannotDecide))]
    public void CreatingAPanel_ThatCannotDecideAsACommittee_IsRefused(string because, DecisionPanelMemberInput[] members, string message)
    {
        var result = new CreateDecisionPanelCommandValidator().Validate(new CreateDecisionPanelCommand(
            "Paediatrics CCC", DecisionPanelScope.Institution, InstitutionId, null, members, TestPrincipals.Administrator()));

        result.IsValid.Should().BeFalse(because);
        result.Errors.Select(error => error.ErrorMessage).Should().ContainSingle().Which.Should().Be(message, because);
    }

    [Theory]
    [MemberData(nameof(PanelsThatCannotDecide))]
    public void UpdatingAPanel_ToOneThatCannotDecideAsACommittee_IsRefused(string because, DecisionPanelMemberInput[] members, string message)
    {
        var result = new UpdateDecisionPanelCommandValidator().Validate(
            new UpdateDecisionPanelCommand(PanelId, members, TestPrincipals.Administrator()));

        result.IsValid.Should().BeFalse(because);
        result.Errors.Select(error => error.ErrorMessage).Should().ContainSingle().Which.Should().Be(message, because);
    }

    [Fact]
    public void APanel_OfTheChairAndOneOther_IsAccepted_OnCreateAndOnUpdate()
    {
        DecisionPanelMemberInput[] members =
            [Input("chair-1", DecisionPanelMemberRole.Chair), Input("external-1", DecisionPanelMemberRole.External)];

        new CreateDecisionPanelCommandValidator().Validate(new CreateDecisionPanelCommand(
                "Paediatrics CCC", DecisionPanelScope.Institution, InstitutionId, null, members, TestPrincipals.Administrator()))
            .IsValid.Should().BeTrue();
        new UpdateDecisionPanelCommandValidator().Validate(
                new UpdateDecisionPanelCommand(PanelId, members, TestPrincipals.Administrator()))
            .IsValid.Should().BeTrue();
    }

    // ─── Who may sit on a panel ──────────────────────────────────────────────

    /// <summary>
    /// Each is someone the panel form's picker never listed, and whom a forged request could name before T165: the
    /// quorum would have counted them.
    /// </summary>
    public static TheoryData<string, string> PeopleWhoMayNotSit => new()
    {
        { "a forged id", "nobody-at-all" },
        { "a deactivated committee member", "deactivated-1" },
        { "someone who holds no CommitteeMember role", "assessor-1" },
        { "a committee member at another institution", "elsewhere-1" }
    };

    [Theory]
    [MemberData(nameof(PeopleWhoMayNotSit))]
    public async Task CreatingAPanel_NamingSomeoneWhoMayNotSit_IsRefused_AndNothingIsWritten(string because, string userId)
    {
        await using var db = await SeededDbAsync();
        var panels = await db.DecisionPanels.CountAsync();

        var act = () => new CreateDecisionPanelCommandHandler(db, Directory()).Handle(
            new CreateDecisionPanelCommand(
                "Second CCC", DecisionPanelScope.Institution, InstitutionId, null,
                [Input("chair-1", DecisionPanelMemberRole.Chair), Input(userId, DecisionPanelMemberRole.Member)],
                TestPrincipals.Administrator()),
            CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>(because)).Which.Message.Should().Be(PanelSeat.NotEligible);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        await using var read = CreateDb();
        (await read.DecisionPanels.CountAsync()).Should().Be(panels, because);
    }

    [Theory]
    [MemberData(nameof(PeopleWhoMayNotSit))]
    public async Task UpdatingAPanel_ToSeatSomeoneWhoMayNotSit_IsRefused_AndTheMembersAreUnchanged(string because, string userId)
    {
        await using var db = await SeededDbAsync();
        var before = await PanelMembersAsync();

        var act = () => new UpdateDecisionPanelCommandHandler(db, Directory()).Handle(
            new UpdateDecisionPanelCommand(
                PanelId,
                [Input("chair-1", DecisionPanelMemberRole.Chair), Input(userId, DecisionPanelMemberRole.External)],
                TestPrincipals.Administrator()),
            CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>(because)).Which.Message.Should().Be(PanelSeat.NotEligible);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await PanelMembersAsync()).Should().Equal(before, because);
    }

    [Fact]
    public async Task UpdatingAPanel_WithEveryoneWhoMaySit_Succeeds()
    {
        // The control for the refusals above.
        await using var db = await SeededDbAsync();

        await new UpdateDecisionPanelCommandHandler(db, Directory()).Handle(
            new UpdateDecisionPanelCommand(
                PanelId,
                [Input("member-1", DecisionPanelMemberRole.Chair), Input("chair-1", DecisionPanelMemberRole.Member)],
                TestPrincipals.Administrator()),
            CancellationToken.None);

        (await PanelMembersAsync()).Should().Equal("chair-1:Member", "member-1:Chair");
    }

    [Fact]
    public async Task ThePanelPicker_ListsOnlyWhoMaySit_AtThePanelsInstitution()
    {
        var picker = new ListPanelMemberCandidatesQueryHandler(Directory());

        var forAdministrator = await picker.Handle(
            new ListPanelMemberCandidatesQuery(TestPrincipals.Administrator(), InstitutionId), CancellationToken.None);
        var forAdministratorBeforeChoosing = await picker.Handle(
            new ListPanelMemberCandidatesQuery(TestPrincipals.Administrator()), CancellationToken.None);
        var forInstitutionalAdmin = await picker.Handle(
            // Their own institution, whatever is asked: they manage panels nowhere else.
            new ListPanelMemberCandidatesQuery(TestPrincipals.InstitutionalAdmin(InstitutionId), OtherInstitutionId),
            CancellationToken.None);

        forAdministrator.Select(candidate => candidate.UserId).Should().BeEquivalentTo("chair-1", "member-1", "external-1");
        forAdministratorBeforeChoosing.Should().BeEmpty("an Administrator belongs to no institution until they name the panel's");
        forInstitutionalAdmin.Select(candidate => candidate.UserId).Should().BeEquivalentTo("chair-1", "member-1", "external-1");
    }

    // ─── Recording the decision, and who was present ─────────────────────────

    [Fact]
    public async Task TheChair_RecordsTheDecision_AndWhoWasPresent()
    {
        await using var db = await SeededDbAsync();

        var decided = await RecordAsync(db, Chair(), "chair-1", "external-1");

        decided.State.Should().Be(CommitteeReviewState.Decided);
        decided.Decisions.Should().ContainSingle().Which.Attendees.Select(person => $"{person.UserId}:{person.Role}")
            .Should().Equal("chair-1:Chair", "external-1:External");
        decided.QuorumShortfall.Should().BeNull();

        await using var read = CreateDb();
        var decisionId = await read.Set<CommitteeDecision>().Where(decision => decision.ReviewId == ReviewId).Select(decision => decision.Id).SingleAsync();
        (await read.Set<CommitteeDecisionAttendee>().CountAsync(attendee => attendee.DecisionId == decisionId)).Should().Be(2);
    }

    public static TheoryData<string, string[], string> AttendanceTheRecordRefuses => new()
    {
        { "the chair alone", ["chair-1"], CommitteeReview.QuorumRule },
        { "nobody", [], CommitteeReview.QuorumRule },
        { "someone not on the panel", ["chair-1", "stranger"], "Only members of this review's panel can be recorded as present." },
        { "without the chair", ["member-1", "external-1"], "The chair recording the decision must be recorded as present, as the chair." },
        { "a member twice", ["chair-1", "member-1", "member-1"], "A panel member is recorded as present more than once." }
    };

    [Theory]
    [MemberData(nameof(AttendanceTheRecordRefuses))]
    public async Task RecordingADecision_WithoutAQuorumOfThePanelPresent_IsRefused_AndNothingIsWritten(
        string because, string[] present, string message)
    {
        // Straight to the handler, past the validator, as a caller that skipped the page would come.
        await using var db = await SeededDbAsync();
        var before = await SnapshotAsync();

        var act = () => RecordAsync(db, Chair(), present);

        (await act.Should().ThrowAsync<InvalidOperationException>(because)).Which.Message.Should().Be(message);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await SnapshotAsync()).Should().BeEquivalentTo(before, because);
    }

    /// <summary>
    /// A panel member who may no longer sit, or the trainee under review, counted as the chair's second. Each is on the
    /// panel, so only the seat rule, read from the user store, can refuse them. (T165)
    /// </summary>
    public static TheoryData<string, string> PanelMembersWhoMayNotBePresent => new()
    {
        { "deactivated since joining", "deactivated-1" },
        { "no longer a committee member", "assessor-1" },
        { "moved to another institution", "elsewhere-1" },
        { "erased: the id is a pseudonym", "erased-7f3a" },
        { "the trainee under review", Trainee }
    };

    [Theory]
    [MemberData(nameof(PanelMembersWhoMayNotBePresent))]
    public async Task RecordingADecision_CountingSomeoneWhoMayNotSit_IsRefused_AndNothingIsWritten(string because, string userId)
    {
        await using var db = await SeededDbAsync(extraMember: userId);
        var before = await SnapshotAsync();

        var act = () => RecordAsync(db, Chair(), "chair-1", userId);

        (await act.Should().ThrowAsync<InvalidOperationException>(because)).Which.Message.Should().Be(
            userId == Trainee ? PanelSeat.TraineeUnderReview : PanelSeat.NotEligibleToBePresent, because);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await SnapshotAsync()).Should().BeEquivalentTo(before, because);
    }

    public static TheoryData<string[]> AttendanceThatCannotBeAQuorum => new()
    {
        Array.Empty<string>(),
        new[] { "chair-1" },
        new[] { "chair-1", "chair-1" },
        new[] { "chair-1", " " }
    };

    [Theory]
    [MemberData(nameof(AttendanceThatCannotBeAQuorum))]
    public void TheRecordCommandValidator_RefusesAnAttendanceThatCannotBeAQuorum(string[] present)
    {
        var result = new RecordCommitteeDecisionCommandValidator().Validate(
            new RecordCommitteeDecisionCommand(ReviewId, CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, present, Chair()));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void TheRecordCommandValidator_AcceptsTheChairAndOneOther()
        => new RecordCommitteeDecisionCommandValidator().Validate(
                new RecordCommitteeDecisionCommand(
                    ReviewId, CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, ["chair-1", "member-1"], Chair()))
            .IsValid.Should().BeTrue();

    // ─── The STARs staged with the decision ──────────────────────────────────

    [Fact]
    public async Task OnceTheDecisionIsRecorded_NoStarCanBeStaged_AndNothingIsWritten()
    {
        await using var db = await SeededDbAsync();
        await StageThroughTheHandlerAsync(db, EpaId);
        await RecordAsync(db, Chair(), "chair-1", "member-1");
        db.ChangeTracker.Clear();
        var before = await SnapshotAsync();

        var act = () => StageThroughTheHandlerAsync(db, SecondEpaId);

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(StagedStars.FixedWhenDecided);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await SnapshotAsync()).Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task OnceTheDecisionIsRecorded_AStagedStarCannotBeChanged_AndNothingIsWritten()
    {
        await using var db = await SeededDbAsync();
        var staged = await StageThroughTheHandlerAsync(db, EpaId);
        await RecordAsync(db, Chair(), "chair-1", "member-1");
        db.ChangeTracker.Clear();
        var before = await SnapshotAsync();

        // The update path: the same handler, naming the staged decision, with a new rationale and date.
        var act = () => StageThroughTheHandlerAsync(db, EpaId, pendingId: staged.Id, rationale: "Raised after the sitting.");

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(StagedStars.FixedWhenDecided);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await SnapshotAsync()).Should().BeEquivalentTo(before);
        await using var read = CreateDb();
        (await read.Set<PendingEntrustmentDecision>().SingleAsync()).Rationale.Should().Be("Ready for indirect supervision.");
    }

    [Fact]
    public async Task OnceTheDecisionIsRecorded_AStagedStarThatStillFits_CannotBeRemoved_AndNothingIsWritten()
    {
        await using var db = await SeededDbAsync();
        var staged = await StageThroughTheHandlerAsync(db, EpaId);
        await RecordAsync(db, Chair(), "chair-1", "member-1");
        db.ChangeTracker.Clear();
        var before = await SnapshotAsync();

        var act = () => RemoveAsync(db, staged.Id);

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(StagedStars.FixedWhenDecided);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await SnapshotAsync()).Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task OnceTheDecisionIsRecorded_AStagedStarThatNoLongerFits_CanStillBeRemoved_SoTheReviewCanBeRatified()
    {
        // T167's recovery: a STAR that no longer fits the trainee's curriculum can never be issued, and while it is staged
        // the review cannot be ratified at all. Removing it is the only change a decided review still admits.
        await using var db = await SeededDbAsync();
        var stale = await StageThroughTheHandlerAsync(db, SecondEpaId);
        var fitting = await StageThroughTheHandlerAsync(db, EpaId);
        await RecordAsync(db, Chair(), "chair-1", "member-1");
        (await db.Epas.SingleAsync(epa => epa.Id == SecondEpaId)).Deactivate(DateTime.MinValue);
        await SaveAndClearAsAuditPipelineWouldAsync(db);

        var listed = await new ListPendingEntrustmentDecisionsForReviewQueryHandler(db).Handle(
            new ListPendingEntrustmentDecisionsForReviewQuery(ReviewId, Chair()), CancellationToken.None);
        listed.Single(pending => pending.Id == stale.Id).NoLongerFits.Should().Contain("PAED-008");
        listed.Single(pending => pending.Id == fitting.Id).NoLongerFits.Should().BeNull();

        await RemoveAsync(db, stale.Id);
        await RatifyAsync(db, Chair());

        await using var read = CreateDb();
        (await read.Set<EntrustmentDecision>().Select(decision => decision.EpaId).ToListAsync()).Should().Equal(EpaId);
    }

    [Fact]
    public async Task Ratifying_IssuesExactlyTheStarsStagedWhenTheDecisionWasRecorded()
    {
        await using var db = await SeededDbAsync();
        await StageThroughTheHandlerAsync(db, EpaId);
        await RecordAsync(db, Chair(), "chair-1", "member-1");

        // After the sitting: an attempt to add a second STAR, refused.
        await ((Func<Task>)(() => StageThroughTheHandlerAsync(db, SecondEpaId))).Should().ThrowAsync<InvalidOperationException>();
        await SaveAndClearAsAuditPipelineWouldAsync(db);

        await RatifyAsync(db, Chair());

        await using var read = CreateDb();
        (await read.Set<EntrustmentDecision>().Select(decision => decision.EpaId).ToListAsync()).Should().Equal(EpaId);
    }

    // ─── Ratifying ───────────────────────────────────────────────────────────

    [Fact]
    public async Task TheChair_RatifiesADecisionTakenByAQuorum_AndTheStagedStarIsIssued()
    {
        // The control for the refusals below: the same fixture, a quorate attendance, and the chair.
        await using var db = await SeededDbAsync();
        await StageAsync(db);
        await RecordAsync(db, Chair(), "chair-1", "member-1");

        var ratified = await RatifyAsync(db, Chair());

        ratified.State.Should().Be(CommitteeReviewState.Ratified);
        await using var read = CreateDb();
        (await read.Set<EntrustmentDecision>().CountAsync(decision => decision.TraineeUserId == Trainee)).Should().Be(1);
    }

    [Fact]
    public async Task Ratifying_ADecisionWhoseAttendanceIsTheChairAlone_IsRefused_AndIssuesNoStar()
    {
        // A decision recorded before T165, or whose attendance was since cut to the chair: either way one person.
        await using var db = await SeededDbAsync();
        await StageAsync(db);
        await RecordAsync(db, Chair(), "chair-1", "member-1");
        db.Set<CommitteeDecisionAttendee>().RemoveRange(db.Set<CommitteeDecisionAttendee>().Where(attendee => attendee.UserId == "member-1"));
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        var before = await SnapshotAsync();

        var act = () => RatifyAsync(db, Chair());

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Be("Only the chair was recorded as present when this decision was recorded. " + CommitteeReview.QuorumRule);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await SnapshotAsync()).Should().BeEquivalentTo(before);

        await using var read = CreateDb();
        (await read.Set<EntrustmentDecision>().AnyAsync()).Should().BeFalse("a refused ratify issues no STAR");
        (await read.Set<PendingEntrustmentDecision>().CountAsync()).Should().Be(1, "the staged decision stays staged");
    }

    [Fact]
    public async Task AnAdministratorNotOnThePanel_CannotRatify_EvenAQuorateDecision_AndNoStarIsIssued()
    {
        await using var db = await SeededDbAsync();
        await StageAsync(db);
        await RecordAsync(db, Chair(), "chair-1", "member-1");
        db.ChangeTracker.Clear();
        var before = await SnapshotAsync();

        var act = () => RatifyAsync(db, TestPrincipals.Administrator());

        // Ratify authorises first, with the one refusal for a review the caller does not chair (T131, T194 item 1).
        (await act.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message
            .Should().Be("The committee review could not be found among the reviews you chair.");
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await SnapshotAsync()).Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task AnAdministratorNotOnThePanel_CannotRecordADecision()
    {
        await using var db = await SeededDbAsync();
        var before = await SnapshotAsync();

        var act = () => RecordAsync(db, TestPrincipals.Administrator(), "chair-1", "member-1");

        // The one refusal for a review the caller does not chair, which an unknown id gets too (T194 item 1).
        (await act.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message
            .Should().Be("The committee review could not be found among the reviews you chair.");
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await SnapshotAsync()).Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task AnAdministrator_StillReadsTheReview()
    {
        // D46 removes the Administrator from the decision, not from oversight.
        await using var db = await SeededDbAsync();
        await RecordAsync(db, Chair(), "chair-1", "member-1");

        var review = await new GetCommitteeReviewByIdQueryHandler(db, FakeUserDirectory.Empty).Handle(
            new GetCommitteeReviewByIdQuery(ReviewId, TestPrincipals.Administrator()), CancellationToken.None);

        review.Decisions.Should().ContainSingle().Which.Attendees.Should().HaveCount(2);
    }

    // ─── The appeal ──────────────────────────────────────────────────────────

    [Fact]
    public async Task AnAdministrator_CannotLodgeAnAppeal_OnTheTraineesBehalf()
    {
        // An appeal is the trainee's own. Before T165 an Administrator could lodge one, and with the appeal body's bypass
        // reopen and replace a ratified committee decision alone.
        await using var db = await RatifiedDbAsync();
        var before = await SnapshotAsync();

        var act = () => new LodgeAppealCommandHandler(db).Handle(
            new LodgeAppealCommand(ReviewId, "Lodged for the trainee.", TestPrincipals.Administrator()), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await SnapshotAsync()).Should().BeEquivalentTo(before);
    }

    [Theory]
    [InlineData(CommitteeAppealOutcome.Dismissed)]
    [InlineData(CommitteeAppealOutcome.Upheld)]
    [InlineData(CommitteeAppealOutcome.Remitted)]
    public async Task AnAdministratorNotOnThePanel_CannotResolveAnAppeal_AndNothingIsWritten(CommitteeAppealOutcome outcome)
    {
        await using var db = await AppealedDbAsync();
        var before = await SnapshotAsync();

        var act = () => ResolveAsync(db, TestPrincipals.Administrator(), outcome, "chair-1", "member-1");

        (await act.Should().ThrowAsync<UnauthorizedAccessException>())
            .Which.Message.Should().Be("The committee review could not be found among the reviews whose appeals you resolve.");
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await SnapshotAsync()).Should().BeEquivalentTo(before);
    }

    public static TheoryData<string, string[]?, string> RemittalsWithoutAQuorum => new()
    {
        { "no attendance sent", null, CommitteeReview.QuorumRule },
        { "the resolver alone", ["external-1"], CommitteeReview.QuorumRule },
        { "without the chair", ["external-1", "member-1"], "The chair must be recorded as present. " + CommitteeReview.QuorumRule },
        { "without the resolver", ["chair-1", "member-1"], "Whoever records the decision must be recorded as present." },
        { "the trainee counted", ["chair-1", "external-1", Trainee], "Only members of this review's panel can be recorded as present." },
        { "a stranger counted", ["external-1", "stranger"], "Only members of this review's panel can be recorded as present." }
    };

    [Theory]
    [MemberData(nameof(RemittalsWithoutAQuorum))]
    public async Task RemittingAnAppeal_WithoutAQuorumPresent_IsRefused_AndNothingIsWritten(
        string because, string[]? present, string message)
    {
        await using var db = await AppealedDbAsync();
        var before = await SnapshotAsync();

        var act = () => ResolveAsync(db, External(), CommitteeAppealOutcome.Remitted, present);

        (await act.Should().ThrowAsync<InvalidOperationException>(because)).Which.Message.Should().Be(message, because);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await SnapshotAsync()).Should().BeEquivalentTo(before, because);
    }

    [Fact]
    public async Task RemittingAnAppeal_RecordsWhoSatForTheReplacement_AndThePageNamesEachDecisionsOwnSitting()
    {
        // Before T165 the replacement was the resolver's alone, and every page showed the first sitting's attendance
        // beside it.
        await using var db = await AppealedDbAsync();

        await ResolveAsync(db, External(), CommitteeAppealOutcome.Remitted, "chair-1", "external-1");

        var users = new FakeUserDirectory(("chair-1", "Thandi Zulu"), ("member-1", "Priya Naidoo"), ("external-1", "Anna Botha"));
        var review = await new GetCommitteeReviewByIdQueryHandler(db, users).Handle(
            new GetCommitteeReviewByIdQuery(ReviewId, Chair()), CancellationToken.None);

        review.State.Should().Be(CommitteeReviewState.Final);
        review.Decisions.Select(decision => string.Join(", ", decision.Attendees.Select(person => person.Label)))
            .Should().Equal("Thandi Zulu (chair), Anna Botha (external)", "Thandi Zulu (chair), Priya Naidoo");
        review.Decisions[0].SupersedesDecisionId.Should().Be(review.Decisions[1].Id);
    }

    [Fact]
    public void TheResolveCommandValidator_AsksARemittalForAQuorum_AndNoOtherOutcome()
    {
        var validator = new ResolveAppealCommandValidator();

        validator.Validate(new ResolveAppealCommand(
                ReviewId, CommitteeAppealOutcome.Remitted, CommitteeDecisionCategory.SatisfactoryProgress, "Lifted.", null,
                ["external-1"], External()))
            .IsValid.Should().BeFalse();
        validator.Validate(new ResolveAppealCommand(
                ReviewId, CommitteeAppealOutcome.Remitted, CommitteeDecisionCategory.SatisfactoryProgress, "Lifted.", null,
                ["chair-1", "external-1"], External()))
            .IsValid.Should().BeTrue();
        validator.Validate(new ResolveAppealCommand(
                ReviewId, CommitteeAppealOutcome.Dismissed, null, null, null, null, External()))
            .IsValid.Should().BeTrue();
    }

    // ─── The review page's read ──────────────────────────────────────────────

    [Fact]
    public async Task TheReview_NamesItsPanelAndWhoWasPresent_InOneLookup()
    {
        await using var db = await SeededDbAsync();
        await RecordAsync(db, Chair(), "chair-1", "external-1");
        var users = new FakeUserDirectory(
            (Trainee, "Lerato Molefe"), ("chair-1", "Thandi Zulu"), ("member-1", "Priya Naidoo"), ("external-1", "Anna Botha"));

        var review = await new GetCommitteeReviewByIdQueryHandler(db, users).Handle(
            new GetCommitteeReviewByIdQuery(ReviewId, Chair()), CancellationToken.None);

        review.PanelMembers.Select(person => person.Label)
            .Should().Equal("Thandi Zulu (chair)", "Priya Naidoo", "Anna Botha (external)");
        review.Decisions.Single().Attendees.Select(person => person.Label).Should().Equal("Thandi Zulu (chair)", "Anna Botha (external)");
        review.TraineeName.Should().Be("Lerato Molefe");
        users.Lookups.Should().ContainSingle();
    }

    [Fact]
    public async Task TheReview_OffersAsPresentOnlyThoseWhoMaySit()
    {
        // The deactivated member is on the panel but may not sit, so the page must not offer them: the form cannot send
        // someone the server refuses.
        await using var db = await SeededDbAsync(extraMember: "deactivated-1");

        var review = await new GetCommitteeReviewByIdQueryHandler(db, Directory()).Handle(
            new GetCommitteeReviewByIdQuery(ReviewId, Chair()), CancellationToken.None);

        review.PanelMembers.Where(person => person.MaySit).Select(person => person.UserId)
            .Should().BeEquivalentTo("chair-1", "member-1", "external-1");
        review.PanelMembers.Single(person => person.UserId == "deactivated-1").MaySit.Should().BeFalse();
        review.PanelShortfall.Should().BeNull();
    }

    [Fact]
    public async Task APanelWithOnlyItsChairSeatable_SaysWhyNoDecisionCanBeRecorded()
    {
        await using var db = await SeededDbAsync();

        var review = await new GetCommitteeReviewByIdQueryHandler(db, FakeUserDirectory.CommitteeMembersAt(InstitutionId, "chair-1"))
            .Handle(new GetCommitteeReviewByIdQuery(ReviewId, Chair()), CancellationToken.None);

        review.PanelShortfall.Should().StartWith("Only the chair can be recorded as present");
    }

    [Fact]
    public async Task ADecidedReviewWithoutAQuorum_SaysWhyItCannotBeRatified()
    {
        await using var db = await SeededDbAsync();
        await RecordAsync(db, Chair(), "chair-1", "member-1");
        db.Set<CommitteeDecisionAttendee>().RemoveRange(db.Set<CommitteeDecisionAttendee>());
        await SaveAndClearAsAuditPipelineWouldAsync(db);

        var review = await new GetCommitteeReviewByIdQueryHandler(db, FakeUserDirectory.Empty).Handle(
            new GetCommitteeReviewByIdQuery(ReviewId, Chair()), CancellationToken.None);

        review.QuorumShortfall.Should().Be("Nobody was recorded as present when this decision was recorded. " + CommitteeReview.QuorumRule);
    }

    /// <summary>
    /// T213: the review page offers the chair's controls, and the resolve-appeal form, by what the query says the caller
    /// may do, and the query asks the predicates the handlers demand. Each principal below is let through, or refused, by
    /// the handler exactly as its flag says: recording is the chair's, and resolving the appeal body's.
    /// </summary>
    public static TheoryData<string, bool, bool> WhoMayDoWhat => new()
    {
        { "chair", true, true },
        { "member", false, false },
        { "external", false, true },
        { "administrator", false, false },
        { "coordinator", false, false }
    };

    [Theory]
    [MemberData(nameof(WhoMayDoWhat))]
    public async Task TheReview_SaysWhatTheCallerMayDo_ByThePredicatesTheHandlersDemand(
        string who, bool chairs, bool resolvesAppeals)
    {
        await using var db = await SeededDbAsync();

        var review = await new GetCommitteeReviewByIdQueryHandler(db, Directory()).Handle(
            new GetCommitteeReviewByIdQuery(ReviewId, Caller(who)), CancellationToken.None);

        review.CallerChairs.Should().Be(chairs, who);
        review.CallerResolvesAppeals.Should().Be(resolvesAppeals, who);
        review.TraineeElsewhere.Should().BeNull($"{who}: the trainee trains at the panel's institution");

        // The picker is the gate: the record handler lets through exactly those the flag offers the form to.
        db.ChangeTracker.Clear();
        var record = () => RecordAsync(db, Caller(who), "chair-1", "member-1");
        if (chairs)
        {
            await record.Should().NotThrowAsync(who);
        }
        else
        {
            await record.Should().ThrowAsync<UnauthorizedAccessException>(who);
        }
    }

    public static TheoryData<string, bool> WhoMayResolveAnAppeal => new()
    {
        { "chair", true },
        { "member", false },
        { "external", true },
        { "administrator", false },
        { "coordinator", false }
    };

    [Theory]
    [MemberData(nameof(WhoMayResolveAnAppeal))]
    public async Task TheResolveForm_IsOfferedToExactlyThoseTheResolveHandlerLetsThrough(string who, bool resolvesAppeals)
    {
        await using var db = await AppealedDbAsync();

        var review = await new GetCommitteeReviewByIdQueryHandler(db, Directory()).Handle(
            new GetCommitteeReviewByIdQuery(ReviewId, Caller(who)), CancellationToken.None);
        review.CallerResolvesAppeals.Should().Be(resolvesAppeals, who);

        db.ChangeTracker.Clear();
        var dismiss = () => ResolveAsync(db, Caller(who), CommitteeAppealOutcome.Dismissed);
        if (resolvesAppeals)
        {
            await dismiss.Should().NotThrowAsync(who);
        }
        else
        {
            await dismiss.Should().ThrowAsync<UnauthorizedAccessException>(who);
        }
    }

    /// <summary>
    /// T213 review. Starting a review and every chair's action also demand that its trainee still trains at the panel's
    /// institution, a global Administrator excepted (<c>CommitteeTraineeScope</c>). The query says when that fails for the
    /// caller, by the same predicate, so the page offers the chair none of the controls each click of which would refuse,
    /// and shows the sentence they refuse with instead.
    /// </summary>
    public static TheoryData<string, bool> WhoIsToldTheTraineeHasMoved => new()
    {
        { "chair", true },
        { "member", true },
        { "coordinator", true },
        { "administrator", false },
        { "administrator in the chair", false }
    };

    [Theory]
    [MemberData(nameof(WhoIsToldTheTraineeHasMoved))]
    public async Task WhenTheTraineeHasMoved_TheReviewSaysThePanelCannotAct_ByThePredicateTheChairsActionsDemand(
        string who, bool told)
    {
        await using var db = await SeededDbAsync();
        await MoveTraineeAsync(db);

        var review = await new GetCommitteeReviewByIdQueryHandler(db, Directory()).Handle(
            new GetCommitteeReviewByIdQuery(ReviewId, Caller(who)), CancellationToken.None);

        review.TraineeElsewhere.Should().Be(told ? TraineeMoved : null, who);
    }

    [Fact]
    public async Task WhenTheTraineeHasMoved_TheChairsRecordIsRefused_WithTheSentenceTheReviewShows_AndNothingIsWritten()
    {
        await using var db = await SeededDbAsync();
        await MoveTraineeAsync(db);
        var review = await new GetCommitteeReviewByIdQueryHandler(db, Directory()).Handle(
            new GetCommitteeReviewByIdQuery(ReviewId, Chair()), CancellationToken.None);
        review.CallerChairs.Should().BeTrue("the chair still holds the seat");
        var before = await SnapshotAsync();
        db.ChangeTracker.Clear();

        var record = () => RecordAsync(db, Chair(), "chair-1", "member-1");

        (await record.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(review.TraineeElsewhere);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await SnapshotAsync()).Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task WhenTheTraineeHasMoved_AnAdministratorInTheChair_IsToldNothing_AndMayRecord()
    {
        // The demand's bypass: a review stranded by a move is finished by an Administrator who takes the chair.
        await using var db = await SeededDbAsync();
        await MoveTraineeAsync(db);
        var administratorInTheChair = Caller("administrator in the chair");

        var review = await new GetCommitteeReviewByIdQueryHandler(db, Directory()).Handle(
            new GetCommitteeReviewByIdQuery(ReviewId, administratorInTheChair), CancellationToken.None);
        review.CallerChairs.Should().BeTrue();
        review.TraineeElsewhere.Should().BeNull();
        db.ChangeTracker.Clear();

        var record = () => RecordAsync(db, administratorInTheChair, "chair-1", "member-1");

        await record.Should().NotThrowAsync();
    }

    [Fact]
    public async Task WhenTheTraineeHasMoved_ARatifiedReview_SaysNothing_ForNoActionItGatesIsOpen()
    {
        // An appeal is not gated by the trainee check (CommitteeTraineeScope), and nothing else can be done now.
        await using var db = await RatifiedDbAsync();
        await MoveTraineeAsync(db);

        var review = await new GetCommitteeReviewByIdQueryHandler(db, Directory()).Handle(
            new GetCommitteeReviewByIdQuery(ReviewId, Chair()), CancellationToken.None);

        review.State.Should().Be(CommitteeReviewState.Ratified);
        review.TraineeElsewhere.Should().BeNull();
    }

    private const string TraineeMoved =
        "This review's trainee does not train at the panel's institution, so the panel cannot act on it.";

    private static async Task MoveTraineeAsync(ApplicationDbContext db)
    {
        (await db.Set<TraineeProfile>().SingleAsync(profile => profile.UserId == Trainee)).InstitutionId = OtherInstitutionId;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    private static ClaimsPrincipal Caller(string who) => who switch
    {
        "administrator in the chair" =>
            TestPrincipals.InRoles([WombatRoles.Administrator, WombatRoles.CommitteeMember], "chair-1", InstitutionId),
        "chair" => Chair(),
        "member" => TestPrincipals.InRole(WombatRoles.CommitteeMember, "member-1", InstitutionId),
        "external" => External(),
        "administrator" => TestPrincipals.Administrator(),
        "coordinator" => TestPrincipals.InRole(WombatRoles.Coordinator, "coordinator-1", InstitutionId),
        _ => throw new ArgumentOutOfRangeException(nameof(who), who, null)
    };

    [Fact]
    public void ACommandsAnswer_TakesItsNamesAndSeatsFromThePageItReplaces()
    {
        var loaded = Detail() with
        {
            TraineeName = "Lerato Molefe",
            CallerChairs = true,
            CallerResolvesAppeals = true,
            TraineeElsewhere = "Moved.",
            PanelMembers =
            [
                new CommitteePersonDto("chair-1", DecisionPanelMemberRole.Chair) { Name = "Thandi Zulu", MaySit = true },
                new CommitteePersonDto("member-1", DecisionPanelMemberRole.Member) { Name = "Priya Naidoo", MaySit = true }
            ]
        };
        var answered = Detail() with
        {
            PanelMembers =
            [
                new CommitteePersonDto("chair-1", DecisionPanelMemberRole.Chair),
                new CommitteePersonDto("member-1", DecisionPanelMemberRole.Member)
            ],
            Decisions =
            [
                new CommitteeDecisionDto(1, CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, DateTime.UtcNow, "chair-1", null)
                {
                    Attendees =
                    [
                        new CommitteePersonDto("chair-1", DecisionPanelMemberRole.Chair),
                        new CommitteePersonDto("member-1", DecisionPanelMemberRole.Member)
                    ]
                }
            ]
        };

        var named = answered.WithNamesFrom(loaded);

        named.TraineeName.Should().Be("Lerato Molefe");
        named.Decisions.Single().Attendees.Select(person => person.Label).Should().Equal("Thandi Zulu (chair)", "Priya Naidoo");
        named.PanelMembers.Select(person => person.DisplayName).Should().Equal("Thandi Zulu", "Priya Naidoo");
        named.PanelMembers.Should().OnlyContain(person => person.MaySit);

        // The mapper knows no caller, so what the caller may do is the loaded page's too (T213).
        answered.CallerChairs.Should().BeFalse();
        named.CallerChairs.Should().BeTrue();
        named.CallerResolvesAppeals.Should().BeTrue();
        answered.TraineeElsewhere.Should().BeNull();
        named.TraineeElsewhere.Should().Be("Moved.", "the trainee's institution does not change with the action");
    }

    // ─── Commands ────────────────────────────────────────────────────────────

    private Task<CommitteeReviewDetailDto> RecordAsync(ApplicationDbContext db, ClaimsPrincipal principal, params string[] present)
        => new RecordCommitteeDecisionCommandHandler(db, Directory()).Handle(
            new RecordCommitteeDecisionCommand(
                ReviewId, CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, present, principal),
            CancellationToken.None);

    private static Task<CommitteeReviewDetailDto> RatifyAsync(ApplicationDbContext db, ClaimsPrincipal principal)
        => new RatifyCommitteeDecisionCommandHandler(db).Handle(
            new RatifyCommitteeDecisionCommand(ReviewId, principal), CancellationToken.None);

    private Task<CommitteeReviewDetailDto> ResolveAsync(
        ApplicationDbContext db, ClaimsPrincipal principal, CommitteeAppealOutcome outcome, params string[]? present)
        => new ResolveAppealCommandHandler(db, Directory()).Handle(
            new ResolveAppealCommand(
                ReviewId,
                outcome,
                outcome == CommitteeAppealOutcome.Remitted ? CommitteeDecisionCategory.SatisfactoryProgress : null,
                outcome == CommitteeAppealOutcome.Remitted ? "Conditions lifted on appeal." : null,
                null,
                present,
                principal),
            CancellationToken.None);

    private static Task<PendingEntrustmentDecisionDto> StageThroughTheHandlerAsync(
        ApplicationDbContext db, int epaId, int? pendingId = null, string rationale = "Ready for indirect supervision.")
        => new StagePendingEntrustmentDecisionCommandHandler(db).Handle(
            new StagePendingEntrustmentDecisionCommand(
                ReviewId, pendingId, epaId, LevelId, new DateOnly(2026, 7, 2), null, rationale,
                [epaId == EpaId ? EvidenceOnEpa : EvidenceOnSecondEpa], Chair()),
            CancellationToken.None);

    private static Task RemoveAsync(ApplicationDbContext db, int pendingId)
        => new RemovePendingEntrustmentDecisionCommandHandler(db).Handle(
            new RemovePendingEntrustmentDecisionCommand(ReviewId, pendingId, Chair()), CancellationToken.None);

    private static async Task StageAsync(ApplicationDbContext db)
    {
        db.Set<PendingEntrustmentDecision>().Add(PendingEntrustmentDecision.Stage(
            ReviewId, EpaId, LevelId, new DateOnly(2026, 7, 2), null, "Ready for indirect supervision.", [EvidenceOnEpa], "chair-1",
            new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc)));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    private static ClaimsPrincipal Chair() => TestPrincipals.InRole(WombatRoles.CommitteeMember, "chair-1", InstitutionId);

    private static ClaimsPrincipal External() => TestPrincipals.InRole(WombatRoles.CommitteeMember, "external-1", InstitutionId);

    private static DecisionPanelMemberInput Input(string userId, DecisionPanelMemberRole role) => new(userId, role);

    private static CommitteeReviewDetailDto Detail()
        => new(
            ReviewId, Trainee, PanelId, "Paediatrics CCC", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30),
            new DateOnly(2026, 7, 2), CommitteeReviewState.Decided, null, null, null, null, null, [], [], [])
        {
            AcademicYear = 2026,
            Semester = 1
        };

    private IUserAdministrationService Directory() => _directory;

    // ─── Fixture ─────────────────────────────────────────────────────────────

    /// <param name="extraMember">A fourth panel member, as the panel holds them: someone who may not sit, for a refusal.</param>
    private async Task<ApplicationDbContext> SeededDbAsync(string? extraMember = null)
    {
        var db = CreateDb();

        db.Institutions.Add(new Institution { Id = InstitutionId, Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Specialities.Add(new Speciality { Id = 1, CollegeId = 1, Name = "Paediatrics", IsActive = true });
        db.SubSpecialities.Add(new SubSpeciality { Id = 11, SpecialityId = 1, Name = "General Paediatrics", IsActive = true });
        db.Curricula.Add(new Curriculum { Id = 100, SubSpecialityId = 11, Name = "General Paediatrics", Version = "11.1" });
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = Trainee,
            InstitutionId = InstitutionId,
            CurriculumId = 100,
            ProgrammeStartDate = new DateOnly(2024, 1, 15),
            ExpectedCompletionDate = new DateOnly(2028, 1, 15),
            IsActive = true
        });
        db.EntrustmentScales.Add(new EntrustmentScale { Id = 1, Name = "v11.1 rungs" });
        db.EntrustmentLevels.Add(new EntrustmentLevel { Id = LevelId, ScaleId = 1, Order = 3, Label = "3a" });
        db.Epas.Add(new Epa { Id = EpaId, SubSpecialityId = 11, Code = "PAED-007", Title = "Triage", IsActive = true });
        db.Epas.Add(new Epa { Id = SecondEpaId, SubSpecialityId = 11, Code = "PAED-008", Title = "Handover", IsActive = true });
        db.CurriculumItems.Add(new CurriculumItem { Id = 1000, CurriculumId = 100, EpaId = EpaId, RequiredCount = 1, MinimumLevelOrder = 3 });
        db.CurriculumItems.Add(new CurriculumItem { Id = 1001, CurriculumId = 100, EpaId = SecondEpaId, RequiredCount = 1, MinimumLevelOrder = 3 });

        var members = new List<DecisionPanelMember>
        {
            new() { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair },
            new() { UserId = "member-1", Role = DecisionPanelMemberRole.Member },
            new() { UserId = "external-1", Role = DecisionPanelMemberRole.External }
        };
        if (extraMember is not null)
        {
            members.Add(new DecisionPanelMember { UserId = extraMember, Role = DecisionPanelMemberRole.Member });
        }

        db.DecisionPanels.Add(new DecisionPanel
        {
            Id = PanelId,
            Name = "Paediatrics CCC",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = InstitutionId,
            CreatedOn = DateTime.UtcNow,
            Members = members
        });

        var review = new CommitteeReview
        {
            Id = ReviewId,
            AcademicYear = 2026,
            Semester = 1,
            PanelId = PanelId,
            TraineeUserId = Trainee,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 6, 30),
            ScheduledOn = new DateOnly(2026, 7, 2)
        };
        // One frozen line about each EPA, which a staged decision names as the evidence it rests on (D38, T131).
        review.Start(
            [
                new CommitteeEvidence
                {
                    Id = EvidenceOnEpa, SourceType = CommitteeEvidenceSourceType.Activity, ActivityId = 700, EpaId = EpaId,
                    SourceLabel = "Mini-CEX #700", Summary = "State: completed."
                },
                new CommitteeEvidence
                {
                    Id = EvidenceOnSecondEpa, SourceType = CommitteeEvidenceSourceType.Activity, ActivityId = 701, EpaId = SecondEpaId,
                    SourceLabel = "CbD #701", Summary = "State: completed."
                }
            ],
            "chair-1",
            new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc));
        db.CommitteeReviews.Add(review);

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private async Task<ApplicationDbContext> RatifiedDbAsync()
    {
        var db = await SeededDbAsync();
        await RecordAsync(db, Chair(), "chair-1", "member-1");
        await RatifyAsync(db, Chair());
        db.ChangeTracker.Clear();
        return db;
    }

    private async Task<ApplicationDbContext> AppealedDbAsync()
    {
        var db = await RatifiedDbAsync();
        await new LodgeAppealCommandHandler(db).Handle(
            new LodgeAppealCommand(ReviewId, "The conditions are disproportionate.", TestPrincipals.Trainee(Trainee, InstitutionId)),
            CancellationToken.None);
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

    private async Task<IReadOnlyList<string>> PanelMembersAsync()
    {
        await using var read = CreateDb();
        return await read.Set<DecisionPanelMember>()
            .Where(member => member.PanelId == PanelId)
            .OrderBy(member => member.UserId)
            .Select(member => $"{member.UserId}:{member.Role}")
            .ToListAsync();
    }

    private sealed record StoreSnapshot(
        string Review,
        IReadOnlyList<string> Decisions,
        IReadOnlyList<string> Attendees,
        IReadOnlyList<string> Appeals,
        IReadOnlyList<string> Pending,
        IReadOnlyList<string> EntrustmentDecisions);

    private async Task<StoreSnapshot> SnapshotAsync()
    {
        await using var read = CreateDb();
        var review = await read.CommitteeReviews.SingleAsync(entity => entity.Id == ReviewId);
        return new StoreSnapshot(
            $"{review.State}:{review.RatifiedOn}:{review.RatifiedByUserId}:{review.FinalizedOn}",
            await read.Set<CommitteeDecision>().OrderBy(decision => decision.Id)
                .Select(decision => $"{decision.Id}:{decision.Category}:{decision.DecidedByChairUserId}").ToListAsync(),
            await read.Set<CommitteeDecisionAttendee>().OrderBy(attendee => attendee.Id)
                .Select(attendee => $"{attendee.DecisionId}:{attendee.UserId}:{attendee.Role}").ToListAsync(),
            await read.Set<CommitteeAppeal>().OrderBy(appeal => appeal.Id)
                .Select(appeal => $"{appeal.Id}:{appeal.LodgedByUserId}:{appeal.Outcome}:{appeal.ResolvedOn}").ToListAsync(),
            await read.Set<PendingEntrustmentDecision>().OrderBy(pending => pending.Id)
                .Select(pending => $"{pending.Id}:{pending.EpaId}:{pending.AuthorisedLevelId}:{pending.Rationale}").ToListAsync(),
            await read.Set<EntrustmentDecision>().OrderBy(decision => decision.Id)
                .Select(decision => $"{decision.Id}:{decision.Status}").ToListAsync());
    }
}
