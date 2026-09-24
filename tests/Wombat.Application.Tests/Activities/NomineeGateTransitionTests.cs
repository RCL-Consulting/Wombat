using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T102, the nominee gate on transitions, driven through the real <see cref="ActivityService" />: which moves judge a
/// nominee field, and which leave it alone.
/// </summary>
/// <remarks>
/// <para>
/// A <b>changed</b> nominee is judged on every move, whoever makes it, including a move into a dead end: a
/// <c>field:</c> value grants read, an inbox row and nudges in every state. An <b>unchanged</b> nominee is judged only
/// at the author's hand-on (<c>UnchangedFieldsHandedOn</c>, the clause T122's D20 gate shares): the move can still reach
/// credit, the mover is the subject or the creator, nobody else has acted, the mover can write the field now, and the
/// move hands it on. So a draft whose assessor has since lost eligibility is refused at the trainee's submit, while the
/// trainee can still pick someone else, and never on a move that would strand the encounter on someone who cannot.
/// </para>
/// <para>
/// Every test that proves a stale nominee is NOT judged first proves the nominee really is ineligible now (a fresh
/// filing naming them is refused), so a loss of eligibility that did not take cannot make it pass for the wrong reason.
/// The builder shapes are the ones T122's review rounds found; several of them exist here only to pin one conjunct of
/// the hand-on clause each (see the mutation table in the T102 task file).
/// </para>
/// <para>
/// Every refusal is checked against the audit trap: <c>AuditPipelineBehavior</c>'s catch saves the request's shared
/// DbContext, so the change tracker must hold nothing Added, Modified or Deleted, and that save must write nothing.
/// Each call runs on its own DbContext over one InMemory store, as each request does in the product: an assessor's
/// role, institution or lockout is changed by an administrator in a different request.
/// </para>
/// <para>
/// None of these types credits anything, so the T122 gate never runs and every refusal here is the nominee gate's.
/// </para>
/// </remarks>
public sealed class NomineeGateTransitionTests
{
    private const string TraineeId = "trainee-1";
    private const string AssessorId = "assessor-1";
    private const string ReplacementAssessorId = "assessor-2";
    private const string CoordinatorId = "coordinator-1";

    // Three people nobody may name on institution 10's activities, one for each eligibility condition.
    private const string RegistrarId = "registrar-2";
    private const string ElsewhereAssessorId = "assessor-elsewhere";
    private const string DeactivatedAssessorId = "assessor-deactivated";

    private const int InstitutionId = 10;
    private const int OtherInstitutionId = 20;

    private const int CpsaTypeId = 200;
    private const int LegacyTypeId = 201;
    private const int ResubmittableTypeId = 202;
    private const int RecallableTypeId = 203;
    private const int SignOffTypeId = 204;
    private const int WithdrawableTypeId = 205;
    private const int LockedCancelTypeId = 206;
    private const int ReopenIntoAssessmentTypeId = 207;
    private const int LoopBackTypeId = 208;
    private const int AssessorReassignTypeId = 209;
    private const int AuthorKeepsEditingTypeId = 210;
    private const int AssessorPickupTypeId = 211;
    private const int RoleSubmitTypeId = 212;
    private const int ResubmittableLegacyTypeId = 213;

    /// <summary>The nominee field's label. Deliberately not "Assessor", so it cannot be confused with the role named later in the refusal.</summary>
    private const string NomineeLabel = "Assessing consultant";

    /// <summary>The fragment every nominee refusal carries, so a test can tell the gate apart from any other refusal.</summary>
    private const string NomineeRefusal = "cannot be named here";

    /// <summary>How the refusal names the stale assessor while they are still in the activity's institution.</summary>
    private const string AssessorName = "First assessor-1";

    public enum EligibilityLoss
    {
        LosesTheAssessorRole,
        MovesToAnotherInstitution,
        IsDeactivated
    }

    // ---- 1. The author's hand-on judges an unchanged, stale nominee ---------------------------------------------

    [Theory]
    [InlineData(EligibilityLoss.LosesTheAssessorRole, AssessorName)]
    [InlineData(EligibilityLoss.MovesToAnotherInstitution, "that person")]
    [InlineData(EligibilityLoss.IsDeactivated, AssessorName)]
    public async Task ADraftSavedWhileItsAssessorWasEligible_IsRefusedAtTheTraineesSubmit_OnceTheyAreNot(
        EligibilityLoss loss, string expectedName)
    {
        // The submit is the author's hand-on: the trainee can still write the field and pick someone else, and after it
        // the request sits with the nominee. The submit also edits a non-nominee field, so "unchanged" below proves the
        // patch was not merged into the tracked entity before the gate threw. The refusal names the person only while
        // they belong to the trainee's institution, so a move elsewhere is reported as "that person".
        var options = NewDatabase();
        await SeedAsync(options);

        var draft = await CreateAsync(options, CpsaTypeId);
        await LoseEligibilityAsync(options, AssessorId, loss);

        var message = await RefusedTransitionAsync(
            options, draft.Id, "submit", TraineeId, """{ "presenting_problem": "Revised before submitting" }""");

        message.Should().StartWith($"{NomineeLabel}: {expectedName} {NomineeRefusal}.");

        var stored = await StoredAsync(options, draft.Id);
        stored.CurrentState.Should().Be("draft");
        ReadString(stored.DataJson, "assessor_user_id").Should().Be(AssessorId);
        ReadString(stored.DataJson, "presenting_problem").Should().Be("Fever for three days");
        stored.Transitions.Select(transition => transition.TransitionKey).Should().Equal("create");
    }

    [Theory]
    [InlineData(EligibilityLoss.LosesTheAssessorRole)]
    [InlineData(EligibilityLoss.MovesToAnotherInstitution)]
    [InlineData(EligibilityLoss.IsDeactivated)]
    public async Task TheSameSubmit_Passes_WhenItsPatchRepairsTheNominee(EligibilityLoss loss)
    {
        // The refusal tells the trainee to choose someone else, and the page sends that choice in the same patch as the
        // submit. The gate reads the merged data, so the repair is what it judges.
        var options = NewDatabase();
        await SeedAsync(options);

        var draft = await CreateAsync(options, CpsaTypeId);
        await LoseEligibilityAsync(options, AssessorId, loss);

        var submitted = await TransitionAsync(
            options, draft.Id, "submit", TraineeId, $$"""{ "assessor_user_id": "{{ReplacementAssessorId}}" }""");

        submitted.CurrentState.Should().Be("requested");
        ReadString(submitted.DataJson, "assessor_user_id").Should().Be(ReplacementAssessorId);
    }

    [Fact]
    public async Task ADraftWhoseAssessorIsStillEligible_IsSubmittedAndCompleted()
    {
        // The control for everything in this file: a gate that refused too much would pass every refusal test.
        var options = NewDatabase();
        await SeedAsync(options);

        var draft = await CreateAsync(options, CpsaTypeId);
        (await TransitionAsync(options, draft.Id, "submit", TraineeId)).CurrentState.Should().Be("requested");

        var completed = await TransitionAsync(options, draft.Id, "complete", AssessorId, """{ "overall_level": 4 }""");
        completed.CurrentState.Should().Be("completed");
    }

    [Theory]
    [InlineData("0b6f3d2e-5a41-4c1e-9d2f-7e8a1c3b5d90", "that person")]
    [InlineData(RegistrarId, "First registrar-2")]
    public async Task ADraftWrittenBeforeT102_NamingSomeoneWhoWasNeverEligible_IsRefusedAtSubmit_AndCanBeRepaired(
        string storedNominee, string expectedName)
    {
        // Before T102 nothing checked the value, so a stored draft may name any id at all: one that is nobody, or a
        // colleague the picker never offered. The create that wrote it is gone, so the author's submit is the only point
        // at which it can be caught while the trainee can still fix it. An id that resolves to nobody at the institution
        // is reported as "that person"; one that does is named.
        var options = NewDatabase();
        await SeedAsync(options);

        var draftId = await InsertPreT102DraftAsync(options, CpsaTypeId, storedNominee);

        var message = await RefusedTransitionAsync(options, draftId, "submit", TraineeId);
        message.Should().StartWith($"{NomineeLabel}: {expectedName} {NomineeRefusal}.");
        (await StoredAsync(options, draftId)).CurrentState.Should().Be("draft");

        var repaired = await TransitionAsync(
            options, draftId, "submit", TraineeId, $$"""{ "assessor_user_id": "{{AssessorId}}" }""");
        repaired.CurrentState.Should().Be("requested");
    }

    [Fact]
    public async Task ACreatorFilingOnBehalfOfTheTrainee_IsTheAuthorToo_SoTheirSubmitIsJudged()
    {
        // "The author" is the subject OR the creator. A coordinator who filed the request for the trainee hands it on
        // exactly as the trainee would, and can re-pick just as well.
        var options = NewDatabase();
        await SeedAsync(options);

        var draft = await CreateAsync(options, CpsaTypeId, creator: CoordinatorId);
        await LoseEligibilityAsync(options, AssessorId, EligibilityLoss.LosesTheAssessorRole);

        (await RefusedTransitionAsync(options, draft.Id, "submit", CoordinatorId)).Should().Contain(NomineeRefusal);
        (await RefusedTransitionAsync(options, draft.Id, "submit", TraineeId)).Should().Contain(NomineeRefusal);
    }

    [Fact]
    public async Task ARoleActorSubmit_IsStillTheAuthorsHandOn_AndIsJudged()
    {
        // The scenario runbooks build types whose submit is `role:Trainee`, not `subject`. The clause never reads the
        // actor rule: the mover is the subject, and that is what makes it the author's hand-on.
        var options = NewDatabase();
        await SeedAsync(options);

        var draft = await CreateAsync(options, RoleSubmitTypeId);
        await LoseEligibilityAsync(options, AssessorId, EligibilityLoss.IsDeactivated);

        var message = await ShouldBeRefusedAsync(options, service => service.TransitionAsync(
            new TransitionActivityInput(draft.Id, "submit", TraineeId, TraineePrincipal(), null, null)));

        message.Should().Contain(NomineeRefusal);
        (await StoredAsync(options, draft.Id)).CurrentState.Should().Be("draft");
    }

    [Fact]
    public async Task ARecallIntoTheAuthorWritableDraft_IsNotJudged_ButTheResubmissionOutOfItIs()
    {
        // In `requested` the trainee cannot write the nominee (the state is the assessor's), so the recall is not theirs
        // to be refused for, and pulling a request back from a departed assessor is exactly what they need to do. Only
        // the trainee has acted, so the submit out of the draft that follows is a hand-on again, and is judged.
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, RecallableTypeId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await LoseEligibilityAsync(options, AssessorId, EligibilityLoss.MovesToAnotherInstitution);
        await AssertNowIneligibleAsync(options, AssessorId);

        (await TransitionAsync(options, request.Id, "recall", TraineeId)).CurrentState.Should().Be("draft");

        (await RefusedTransitionAsync(options, request.Id, "submit", TraineeId)).Should().Contain(NomineeRefusal);

        var resubmitted = await TransitionAsync(
            options, request.Id, "submit", TraineeId, $$"""{ "assessor_user_id": "{{ReplacementAssessorId}}" }""");
        resubmitted.CurrentState.Should().Be("requested");
    }

    // ---- 2. An unchanged, stale nominee does not block a move that is not the author's hand-on ------------------

    [Theory]
    [InlineData(EligibilityLoss.LosesTheAssessorRole)]
    [InlineData(EligibilityLoss.MovesToAnotherInstitution)]
    [InlineData(EligibilityLoss.IsDeactivated)]
    public async Task TheAssessorsOwnCompletion_IsNotBlocked_ByTheirLossOfEligibilityAfterTheSubmit(EligibilityLoss loss)
    {
        // The request was handed on legitimately. Re-judging the nominee at completion would strand an observed
        // encounter on the one person who cannot fix it. Act-time enforcement is deliberately not T102's (D20-style).
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, CpsaTypeId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await LoseEligibilityAsync(options, AssessorId, loss);
        await AssertNowIneligibleAsync(options, AssessorId);

        var completed = await TransitionAsync(options, request.Id, "complete", AssessorId, """{ "overall_level": 4 }""");

        completed.CurrentState.Should().Be("completed");
    }

    [Fact]
    public async Task TheAssessorsDecline_IsNotBlocked_ByTheirLossOfEligibility()
    {
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, CpsaTypeId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await LoseEligibilityAsync(options, AssessorId, EligibilityLoss.LosesTheAssessorRole);
        await AssertNowIneligibleAsync(options, AssessorId);

        var declined = await TransitionAsync(options, request.Id, "decline", AssessorId, note: "I have left the unit.");

        declined.CurrentState.Should().Be("declined");
    }

    [Fact]
    public async Task ATraineesSignOffAfterAssessment_IsNotBlocked_ByTheAssessorsLossOfEligibility()
    {
        // draft -submit-> requested -assess-> assessed -acknowledge(subject)-> completed. The trainee's acknowledge is an
        // author's move that reaches credit, but the assessor has acted, so the submit was the last check. Refusing the
        // sign-off would strand an assessed encounter for ever.
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, SignOffTypeId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await TransitionAsync(options, request.Id, "assess", AssessorId, """{ "overall_level": 4 }""");
        await LoseEligibilityAsync(options, AssessorId, EligibilityLoss.IsDeactivated);
        await AssertNowIneligibleAsync(options, AssessorId);

        var completed = await TransitionAsync(options, request.Id, "acknowledge", TraineeId);

        completed.CurrentState.Should().Be("completed");
    }

    [Fact]
    public async Task AResubmissionAfterADecline_IsNotBlocked_EitherWayBack()
    {
        // reflective_note's shape offers two ways back after a decline: submit straight from `declined`, and
        // revise -> draft -> submit. The assessor has acted, so neither is the author's hand-on.
        var options = NewDatabase();
        await SeedAsync(options);

        var direct = await CreateAsync(options, ResubmittableTypeId);
        var viaDraft = await CreateAsync(options, ResubmittableTypeId);
        foreach (var request in new[] { direct, viaDraft })
        {
            await TransitionAsync(options, request.Id, "submit", TraineeId);
            await TransitionAsync(options, request.Id, "decline", AssessorId, note: "Please add the findings.");
        }

        await LoseEligibilityAsync(options, AssessorId, EligibilityLoss.MovesToAnotherInstitution);
        await AssertNowIneligibleAsync(options, AssessorId);

        (await TransitionAsync(options, direct.Id, "submit", TraineeId)).CurrentState.Should().Be("requested");

        await TransitionAsync(options, viaDraft.Id, "revise", TraineeId);
        (await TransitionAsync(options, viaDraft.Id, "submit", TraineeId)).CurrentState.Should().Be("requested");
    }

    [Fact]
    public async Task EchoingTheStoredNomineeInAPatch_IsNotAChange_SoAResubmissionThatEchoesIt_IsNotJudged()
    {
        // A full-form post-back echoes every field. Sending the stored nominee back must not count as naming them anew,
        // or every resubmission after an assessor left would be refused for a choice the trainee did not make today.
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, ResubmittableTypeId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await TransitionAsync(options, request.Id, "decline", AssessorId, note: "Please add the findings.");
        await LoseEligibilityAsync(options, AssessorId, EligibilityLoss.LosesTheAssessorRole);
        await AssertNowIneligibleAsync(options, AssessorId);

        var resubmitted = await TransitionAsync(
            options,
            request.Id,
            "submit",
            TraineeId,
            $$"""{ "assessor_user_id": "{{AssessorId}}", "presenting_problem": "Findings added" }""");

        resubmitted.CurrentState.Should().Be("requested");
    }

    [Fact]
    public async Task TheAuthorsCancelIntoADeadEnd_IsNotBlocked_FromTheDraftOrFromRequested()
    {
        // `cancelled` is a non-terminal dead end: nothing leaves it. A trainee whose draft names a departed assessor must
        // be able to withdraw it, and so must one whose request is sitting with them.
        var options = NewDatabase();
        await SeedAsync(options);

        var draft = await CreateAsync(options, CpsaTypeId);
        var request = await CreateAsync(options, CpsaTypeId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await LoseEligibilityAsync(options, AssessorId, EligibilityLoss.LosesTheAssessorRole);

        (await RefusedTransitionAsync(options, draft.Id, "submit", TraineeId))
            .Should().Contain(NomineeRefusal, "guard: this draft really is refused at its hand-on");

        (await TransitionAsync(options, draft.Id, "cancel", TraineeId)).CurrentState.Should().Be("cancelled");
        (await TransitionAsync(options, request.Id, "cancel", TraineeId)).CurrentState.Should().Be("cancelled");
    }

    [Fact]
    public async Task ACancelOutOfTheDraftIntoAStateThatReopensOnlyIntoTheDraft_IsAWithdrawal_AndIsNotBlocked()
    {
        // The author can write the nominee in `draft`, nobody else has acted, and `cancelled` can still reach credit (via
        // reopen). What stands between this cancel and a refusal is only the hand-on test: credit can follow only back
        // through `draft`, and the author can still write the field in `cancelled`.
        var options = NewDatabase();
        await SeedAsync(options);

        var draft = await CreateAsync(options, RecallableTypeId);
        await LoseEligibilityAsync(options, AssessorId, EligibilityLoss.IsDeactivated);
        await AssertNowIneligibleAsync(options, AssessorId);

        (await TransitionAsync(options, draft.Id, "cancel", TraineeId)).CurrentState.Should().Be("cancelled");
    }

    [Fact]
    public async Task AReopenOutOfAnAuthorWritableCancelledIntoTheDraft_IsJudged_AConservativeResidual_TheAuthorCanRepairItInTheSamePatch()
    {
        // Pinned so a change to it is a decision. Out of `cancelled` the author can still write the nominee, only the
        // author has acted, and `draft` reaches credit without coming back through `cancelled`, so by the clause as
        // specified the reopen is a hand-on, although the author keeps the draft and its submit would be judged anyway.
        // It strands nobody: the refusal lands on the author, who can re-pick in the reopen's own patch. Telling "the
        // author still holds the next state" apart from "someone else can take it on" would mean reading actor-rule
        // syntax, which T122's review rounds showed breaks. (With a locked `cancelled`, the author cannot write there,
        // so the reopen is not judged and the submit out of the draft is.)
        var options = NewDatabase();
        await SeedAsync(options);

        var draft = await CreateAsync(options, RecallableTypeId);
        (await TransitionAsync(options, draft.Id, "cancel", TraineeId)).CurrentState.Should().Be("cancelled");
        await LoseEligibilityAsync(options, AssessorId, EligibilityLoss.LosesTheAssessorRole);

        (await RefusedTransitionAsync(options, draft.Id, "reopen", TraineeId)).Should().Contain(NomineeRefusal);
        (await StoredAsync(options, draft.Id)).CurrentState.Should().Be("cancelled");

        var reopened = await TransitionAsync(
            options, draft.Id, "reopen", TraineeId, $$"""{ "assessor_user_id": "{{ReplacementAssessorId}}" }""");
        reopened.CurrentState.Should().Be("draft");
        (await TransitionAsync(options, draft.Id, "submit", TraineeId)).CurrentState.Should().Be("requested");
    }

    [Fact]
    public async Task AReopenOutOfALockedCancelled_IsNotJudged_ButTheSubmitOutOfTheDraftIs()
    {
        // The locked variant of the residual above: in a `cancelled` only a Coordinator may edit, the author cannot write
        // the nominee, so the reopen is not theirs to be refused for. The draft it lands in is, and its submit is judged.
        var options = NewDatabase();
        await SeedAsync(options);

        var draft = await CreateAsync(options, LockedCancelTypeId);
        (await TransitionAsync(options, draft.Id, "cancel", TraineeId)).CurrentState.Should().Be("cancelled");
        await LoseEligibilityAsync(options, AssessorId, EligibilityLoss.LosesTheAssessorRole);
        await AssertNowIneligibleAsync(options, AssessorId);

        (await TransitionAsync(options, draft.Id, "reopen", TraineeId)).CurrentState.Should().Be("draft");
        (await RefusedTransitionAsync(options, draft.Id, "submit", TraineeId)).Should().Contain(NomineeRefusal);
    }

    [Fact]
    public async Task AWithdrawalIntoAHoldingState_IsNotBlocked_BecauseTheAuthorCannotWriteTheNomineeInRequested()
    {
        // requested -withdraw-> withdrawn -resubmit-> requested. In `requested` the author cannot write the nominee, so
        // a refusal there would name nothing they could correct.
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, WithdrawableTypeId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await LoseEligibilityAsync(options, AssessorId, EligibilityLoss.LosesTheAssessorRole);
        await AssertNowIneligibleAsync(options, AssessorId);

        (await TransitionAsync(options, request.Id, "withdraw", TraineeId)).CurrentState.Should().Be("withdrawn");
    }

    [Fact]
    public async Task AnAuthorsReassignThatStaysWithTheAssessor_IsNotBlocked_BecauseTheAuthorCannotWriteTheNomineeThere()
    {
        // A builder `reassign` (requested -> requested, subject|creator) sends the request on without the author being
        // able to change who it names. Credit follows it without coming back anywhere, so "can the author write the field
        // now" is the only conjunct that keeps it from being judged.
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, WithdrawableTypeId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await LoseEligibilityAsync(options, AssessorId, EligibilityLoss.MovesToAnotherInstitution);
        await AssertNowIneligibleAsync(options, AssessorId);

        (await TransitionAsync(options, request.Id, "reassign", TraineeId)).CurrentState.Should().Be("requested");
    }

    [Fact]
    public async Task ACancelFromRequestedIntoALockedCancelled_IsNotBlocked_BecauseTheAuthorCannotWriteTheNomineeInRequested()
    {
        // The author loses write access at this move (`cancelled` is the Coordinator's to edit), which would make it a
        // hand-on, but they never had it in `requested` either, so there is nothing a refusal could ask them to fix.
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, LockedCancelTypeId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await LoseEligibilityAsync(options, AssessorId, EligibilityLoss.IsDeactivated);
        await AssertNowIneligibleAsync(options, AssessorId);

        (await TransitionAsync(options, request.Id, "cancel", TraineeId)).CurrentState.Should().Be("cancelled");
    }

    [Fact]
    public async Task AnAssessorAllowedToReassign_CompletesWithTheNomineeUnchanged_AfterLosingEligibility()
    {
        // Here the named assessor may write the nominee field, and only the trainee has acted before them: every conjunct
        // of the hand-on clause holds except that the mover is not the author. The encounter was handed on legitimately;
        // only the author handing it on is re-checked.
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, AssessorReassignTypeId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await LoseEligibilityAsync(options, AssessorId, EligibilityLoss.LosesTheAssessorRole);
        await AssertNowIneligibleAsync(options, AssessorId);

        var completed = await TransitionAsync(options, request.Id, "complete", AssessorId, """{ "overall_level": 4 }""");

        completed.CurrentState.Should().Be("completed");
    }

    [Fact]
    public async Task AnAssessorPickingUpADraft_IsNotJudged_BecauseTheyAreNotTheAuthor_TheCreateWasTheLastCheck()
    {
        // The recorded boundary, as with D20: if someone other than the author acts first, the create was the last check.
        var options = NewDatabase();
        await SeedAsync(options);

        var draft = await CreateAsync(options, AssessorPickupTypeId);
        await LoseEligibilityAsync(options, AssessorId, EligibilityLoss.MovesToAnotherInstitution);
        await AssertNowIneligibleAsync(options, AssessorId);

        (await TransitionAsync(options, draft.Id, "pick_up", AssessorId)).CurrentState.Should().Be("requested");
    }

    // ---- 3. The legacy requested-born shape: the create is the author's hand-on ---------------------------------

    [Fact]
    public async Task ALegacyShapedCreate_NamingAnIneligibleAssessor_IsRefused_AndNothingIsPersisted()
    {
        // Born in `requested`, the legacy WBA shape has no author transition: the create IS the submission.
        var options = NewDatabase();
        await SeedAsync(options);

        var message = await ShouldBeRefusedAsync(options, service => service.CreateDraftAsync(new CreateActivityInput(
            LegacyTypeId, TraineeId, TraineeId, RequestData(RegistrarId), Principal(TraineeId))));

        message.Should().StartWith($"{NomineeLabel}: First registrar-2 {NomineeRefusal}.");

        await using var verify = new ApplicationDbContext(options);
        (await verify.Activities.CountAsync()).Should().Be(0);
        (await verify.ActivityTransitions.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ALegacyShapedRequest_IsNotBlocked_AtTheAssessorsAcceptOrComplete_AfterTheyLoseEligibility()
    {
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, LegacyTypeId);
        request.CurrentState.Should().Be("requested", "guard: this is the legacy shape");
        await LoseEligibilityAsync(options, AssessorId, EligibilityLoss.LosesTheAssessorRole);
        await AssertNowIneligibleAsync(options, AssessorId);

        (await TransitionAsync(options, request.Id, "accept", AssessorId)).CurrentState.Should().Be("accepted");
        var completed = await TransitionAsync(options, request.Id, "complete", AssessorId, """{ "overall_level": 4 }""");
        completed.CurrentState.Should().Be("completed");
    }

    [Fact]
    public async Task ALegacyShapedRequest_CanBeCancelledByTheTrainee_WithAStaleNominee_ButNotWhileNamingAnIneligibleOne()
    {
        // The legacy `requested` is writable by the trainee (it declares no editable_by), and the cancel leads into a dead
        // end. An unchanged stale nominee is not judged there; a CHANGED one is, dead end or not.
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, LegacyTypeId);
        await LoseEligibilityAsync(options, AssessorId, EligibilityLoss.IsDeactivated);
        await AssertNowIneligibleAsync(options, AssessorId);

        var message = await RefusedTransitionAsync(
            options, request.Id, "cancel", TraineeId, $$"""{ "assessor_user_id": "{{ElsewhereAssessorId}}" }""");
        message.Should().StartWith($"{NomineeLabel}: that person {NomineeRefusal}.");
        ReadString((await StoredAsync(options, request.Id)).DataJson, "assessor_user_id").Should().Be(AssessorId);

        (await TransitionAsync(options, request.Id, "cancel", TraineeId)).CurrentState.Should().Be("cancelled");
    }

    [Fact]
    public async Task ALegacyShapedResubmission_IsNotBlockedByAStaleNominee_ButAChangedOneIsJudged()
    {
        // A builder-added `resubmit: declined -> requested` returns to the legacy initial state after the assessor has
        // acted, so an unchanged nominee is not re-litigated; a changed one is judged like any other change.
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, ResubmittableLegacyTypeId);
        await TransitionAsync(options, request.Id, "decline", AssessorId, note: "Not observed by me.");
        await LoseEligibilityAsync(options, AssessorId, EligibilityLoss.LosesTheAssessorRole);
        await AssertNowIneligibleAsync(options, AssessorId);

        (await TransitionAsync(options, request.Id, "resubmit", TraineeId)).CurrentState.Should().Be("requested");

        await TransitionAsync(options, request.Id, "decline", AssessorId, note: "Still not observed by me.");

        (await RefusedTransitionAsync(
                options, request.Id, "resubmit", TraineeId, $$"""{ "assessor_user_id": "{{RegistrarId}}" }"""))
            .Should().Contain(NomineeRefusal);

        var resubmitted = await TransitionAsync(
            options, request.Id, "resubmit", TraineeId, $$"""{ "assessor_user_id": "{{ReplacementAssessorId}}" }""");
        resubmitted.CurrentState.Should().Be("requested");
        ReadString(resubmitted.DataJson, "assessor_user_id").Should().Be(ReplacementAssessorId);
    }

    // ---- 4. Builder shapes in which the author's move IS a hand-on ---------------------------------------------

    [Fact]
    public async Task AHandoverWhoseCreditPathLoopsBackThroughTheDraft_IsJudged_BecauseTheAuthorLosesWriteAccess()
    {
        // draft -handover-> with_assessor -rate-> draft -finalize-> completed. Credit can follow the handover only back
        // through `draft`, so reachability alone would call it a withdrawal. The author loses write access to the
        // nominee at the handover, and losing it is how the hand-on is recognised.
        var options = NewDatabase();
        await SeedAsync(options);

        var draft = await CreateAsync(options, LoopBackTypeId);
        await LoseEligibilityAsync(options, AssessorId, EligibilityLoss.LosesTheAssessorRole);

        (await RefusedTransitionAsync(options, draft.Id, "handover", TraineeId)).Should().Contain(NomineeRefusal);

        var handedOver = await TransitionAsync(
            options, draft.Id, "handover", TraineeId, $$"""{ "assessor_user_id": "{{ReplacementAssessorId}}" }""");
        handedOver.CurrentState.Should().Be("with_assessor");
    }

    [Fact]
    public async Task ASubmitIntoAStateTheAuthorCanStillEdit_IsJudged_BecauseCreditCanFollowWithoutThem()
    {
        // The author keeps write access in `requested` here, so "loses write access" does not make this a hand-on. The
        // assessor can complete straight out of `requested` with no further author move, and that does.
        var options = NewDatabase();
        await SeedAsync(options);

        var draft = await CreateAsync(options, AuthorKeepsEditingTypeId);
        await LoseEligibilityAsync(options, AssessorId, EligibilityLoss.IsDeactivated);

        var message = await RefusedTransitionAsync(options, draft.Id, "submit", TraineeId);

        message.Should().StartWith($"{NomineeLabel}: {AssessorName} {NomineeRefusal}.");
        (await StoredAsync(options, draft.Id)).CurrentState.Should().Be("draft");
    }

    [Fact]
    public async Task ACancelOutOfTheDraftIntoALockedCancelled_IsJudged_TheRecordedResidual()
    {
        // T122's fifth-round residual, shared here because the clause is shared: the author loses write access when the
        // draft moves into a `cancelled` only a Coordinator may edit, so the move counts as a hand-on, even though only
        // the author can reopen it. The refusal lands on the author, who can act on it.
        var options = NewDatabase();
        await SeedAsync(options);

        var draft = await CreateAsync(options, LockedCancelTypeId);
        await LoseEligibilityAsync(options, AssessorId, EligibilityLoss.LosesTheAssessorRole);

        (await RefusedTransitionAsync(options, draft.Id, "cancel", TraineeId)).Should().Contain(NomineeRefusal);
    }

    [Fact]
    public async Task ACancelOutOfTheDraftThatTheAssessorCanReopenIntoAssessment_IsJudged()
    {
        // The assessor can reopen a cancelled request straight into `requested` and complete it with no further author
        // move, so credit can follow this cancel without coming back through `draft`: it hands the nominee on.
        var options = NewDatabase();
        await SeedAsync(options);

        var draft = await CreateAsync(options, ReopenIntoAssessmentTypeId);
        await LoseEligibilityAsync(options, AssessorId, EligibilityLoss.MovesToAnotherInstitution);

        (await RefusedTransitionAsync(options, draft.Id, "cancel", TraineeId)).Should().Contain(NomineeRefusal);
    }

    // ---- 5. A changed nominee is judged on every move, whoever makes it ----------------------------------------

    [Theory]
    [InlineData(RegistrarId, "First registrar-2")]
    [InlineData(ElsewhereAssessorId, "that person")]
    [InlineData(DeactivatedAssessorId, "First assessor-deactivated")]
    public async Task ASubmitWhosePatchNamesSomeoneIneligible_IsRefused_AndTheStoredNomineeIsKept(
        string nominee, string expectedName)
    {
        // The mirror of the repair: the stored nominee is fine, the one being written is not.
        var options = NewDatabase();
        await SeedAsync(options);

        var draft = await CreateAsync(options, CpsaTypeId);

        var message = await RefusedTransitionAsync(
            options, draft.Id, "submit", TraineeId, $$"""{ "assessor_user_id": "{{nominee}}" }""");

        message.Should().StartWith($"{NomineeLabel}: {expectedName} {NomineeRefusal}.");
        var stored = await StoredAsync(options, draft.Id);
        stored.CurrentState.Should().Be("draft");
        ReadString(stored.DataJson, "assessor_user_id").Should().Be(AssessorId);
    }

    [Fact]
    public async Task ACancelIntoADeadEnd_ThatChangesTheNominee_IsJudged_RefusedForSomeoneIneligible_AndPassingForSomeoneEligible()
    {
        // Unlike the EPA→tool gate, a dead end is no exemption for a CHANGED nominee: a `field:` value grants read of the
        // whole record in every state, cancelled included.
        var options = NewDatabase();
        await SeedAsync(options);

        var draft = await CreateAsync(options, CpsaTypeId);

        var message = await RefusedTransitionAsync(
            options, draft.Id, "cancel", TraineeId, $$"""{ "assessor_user_id": "{{RegistrarId}}" }""");
        message.Should().StartWith($"{NomineeLabel}: First registrar-2 {NomineeRefusal}.");

        var stored = await StoredAsync(options, draft.Id);
        stored.CurrentState.Should().Be("draft");
        ReadString(stored.DataJson, "assessor_user_id").Should().Be(AssessorId);

        var cancelled = await TransitionAsync(
            options, draft.Id, "cancel", TraineeId, $$"""{ "assessor_user_id": "{{ReplacementAssessorId}}" }""");
        cancelled.CurrentState.Should().Be("cancelled");
        ReadString(cancelled.DataJson, "assessor_user_id").Should().Be(ReplacementAssessorId);
    }

    [Fact]
    public async Task AnAssessorReassigningToSomeoneIneligible_IsRefused_ThoughTheyAreNotTheAuthor()
    {
        // A non-author's move is never the author's hand-on, but a changed value is judged whoever writes it: otherwise
        // an assessor allowed to reassign could hand the trainee's record to anyone.
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, AssessorReassignTypeId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);

        var message = await RefusedTransitionAsync(
            options, request.Id, "reassign", AssessorId, $$"""{ "assessor_user_id": "{{ElsewhereAssessorId}}" }""");

        message.Should().StartWith($"{NomineeLabel}: that person {NomineeRefusal}.");
        var stored = await StoredAsync(options, request.Id);
        ReadString(stored.DataJson, "assessor_user_id").Should().Be(AssessorId);
        stored.Transitions.Select(transition => transition.TransitionKey).Should().Equal("create", "submit");
    }

    [Fact]
    public async Task AnAssessorWhoHasLostEligibility_CanReassignToAnEligibleColleague_WhoCanThenComplete()
    {
        // The departing assessor's way out: the change is judged, and it names someone eligible. Their own staleness is
        // not judged, because they are not the author.
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, AssessorReassignTypeId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await LoseEligibilityAsync(options, AssessorId, EligibilityLoss.MovesToAnotherInstitution);
        await AssertNowIneligibleAsync(options, AssessorId);

        var reassigned = await TransitionAsync(
            options, request.Id, "reassign", AssessorId, $$"""{ "assessor_user_id": "{{ReplacementAssessorId}}" }""");
        reassigned.CurrentState.Should().Be("requested");
        ReadString(reassigned.DataJson, "assessor_user_id").Should().Be(ReplacementAssessorId);

        var completed = await TransitionAsync(
            options, request.Id, "complete", ReplacementAssessorId, """{ "overall_level": 4 }""");
        completed.CurrentState.Should().Be("completed");
    }

    [Fact]
    public async Task AResubmissionAfterADecline_ThatChangesTheNomineeToSomeoneIneligible_IsRefused()
    {
        // The assessor has acted, so an unchanged nominee would not be judged here; a changed one still is.
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, ResubmittableTypeId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await TransitionAsync(options, request.Id, "decline", AssessorId, note: "Ask someone on the ward.");

        var message = await RefusedTransitionAsync(
            options, request.Id, "submit", TraineeId, $$"""{ "assessor_user_id": "{{DeactivatedAssessorId}}" }""");

        message.Should().Contain(NomineeRefusal);
        (await StoredAsync(options, request.Id)).CurrentState.Should().Be("declined");
    }

    // ---- helpers ----------------------------------------------------------------------------------------------

    /// <summary>
    /// Runs one refused request on its own context, asserts it left nothing for the audit save to commit, performs
    /// that save, and returns the refusal message.
    /// </summary>
    private static async Task<string> ShouldBeRefusedAsync(
        DbContextOptions<ApplicationDbContext> options,
        Func<ActivityService, Task> act)
    {
        await using var db = new ApplicationDbContext(options);
        var service = Service(db);

        var attempt = async () => await act(service);
        var thrown = await attempt.Should().ThrowAsync<InvalidOperationException>();

        db.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(entry => $"{entry.Metadata.ClrType.Name}: {entry.State}")
            .Should().BeEmpty("the audit pipeline's catch saves this context, so anything dirty here would be committed");

        // What the audit pipeline's catch does next: save the same context.
        (await db.SaveChangesAsync()).Should().Be(0);

        return thrown.Which.Message;
    }

    private static Task<string> RefusedTransitionAsync(
        DbContextOptions<ApplicationDbContext> options,
        int activityId,
        string transitionKey,
        string actorUserId,
        string? patch = null,
        string? note = null)
        => ShouldBeRefusedAsync(options, service => service.TransitionAsync(
            new TransitionActivityInput(activityId, transitionKey, actorUserId, Principal(actorUserId), patch, note)));

    /// <summary>
    /// The guard for every test that proves a stale nominee is NOT judged: a fresh filing naming them must be refused
    /// now. Without it, a loss of eligibility that did not take would let those tests pass for the wrong reason.
    /// </summary>
    private static async Task AssertNowIneligibleAsync(DbContextOptions<ApplicationDbContext> options, string userId)
    {
        var message = await ShouldBeRefusedAsync(options, service => service.CreateDraftAsync(
            new CreateActivityInput(CpsaTypeId, TraineeId, TraineeId, RequestData(userId), Principal(TraineeId))));

        message.Should().Contain(NomineeRefusal, "guard: the nominee must really be ineligible now");
    }

    /// <summary>What an administrator's change to the assessor does, in its own request.</summary>
    private static async Task LoseEligibilityAsync(
        DbContextOptions<ApplicationDbContext> options,
        string userId,
        EligibilityLoss loss)
    {
        await using var db = new ApplicationDbContext(options);
        var user = await db.Users.SingleAsync(entity => entity.Id == userId);

        switch (loss)
        {
            case EligibilityLoss.LosesTheAssessorRole:
                var assessorRoleId = await db.Roles
                    .Where(role => role.NormalizedName == WombatRoles.Assessor.ToUpperInvariant())
                    .Select(role => role.Id)
                    .SingleAsync();
                db.UserRoles.RemoveRange(await db.UserRoles
                    .Where(userRole => userRole.UserId == userId && userRole.RoleId == assessorRoleId)
                    .ToListAsync());
                break;
            case EligibilityLoss.MovesToAnotherInstitution:
                user.InstitutionId = OtherInstitutionId;
                break;
            case EligibilityLoss.IsDeactivated:
                // What an administrator's lock and an erasure write, as opposed to a brute-force lockout.
                user.LockoutEnd = UserDeactivation.IndefiniteLockoutEnd;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(loss), loss, null);
        }

        (await db.SaveChangesAsync()).Should().BeGreaterThan(0, "guard: the change must actually be written");
    }

    private static async Task<ActivityDto> CreateAsync(
        DbContextOptions<ApplicationDbContext> options,
        int activityTypeId,
        string nominee = AssessorId,
        string? creator = null)
    {
        var createdBy = creator ?? TraineeId;

        await using var db = new ApplicationDbContext(options);
        return await Service(db).CreateDraftAsync(
            new CreateActivityInput(activityTypeId, TraineeId, createdBy, RequestData(nominee), Principal(createdBy)));
    }

    private static async Task<ActivityDto> TransitionAsync(
        DbContextOptions<ApplicationDbContext> options,
        int activityId,
        string transitionKey,
        string actorUserId,
        string? patch = null,
        string? note = null)
    {
        await using var db = new ApplicationDbContext(options);
        return await Service(db).TransitionAsync(
            new TransitionActivityInput(activityId, transitionKey, actorUserId, Principal(actorUserId), patch, note));
    }

    /// <summary>
    /// A draft as it could have been stored before T102: written straight to the table, naming whatever id it names,
    /// with the create recorded as the trainee's.
    /// </summary>
    private static async Task<int> InsertPreT102DraftAsync(
        DbContextOptions<ApplicationDbContext> options,
        int activityTypeId,
        string nominee)
    {
        await using var db = new ApplicationDbContext(options);

        var createdOn = new DateTime(2026, 3, 10, 8, 0, 0, DateTimeKind.Utc);
        var dataJson = JsonSerializer.Serialize(JsonDocument.Parse(RequestData(nominee)).RootElement);

        var activity = new Activity
        {
            ActivityTypeId = activityTypeId,
            SchemaVersion = 1,
            SubjectUserId = TraineeId,
            CreatedByUserId = TraineeId,
            CurrentState = "draft",
            DataJson = dataJson,
            InstitutionId = InstitutionId,
            ObservedOn = new DateOnly(2026, 3, 10),
            CreatedOn = createdOn,
            UpdatedOn = createdOn
        };
        activity.Transitions.Add(new ActivityTransition
        {
            FromState = "draft",
            ToState = "draft",
            TransitionKey = "create",
            ActorUserId = TraineeId,
            OccurredOn = createdOn,
            SnapshotJson = dataJson
        });

        db.Activities.Add(activity);
        await db.SaveChangesAsync();
        return activity.Id;
    }

    private static async Task<Activity> StoredAsync(DbContextOptions<ApplicationDbContext> options, int activityId)
    {
        await using var db = new ApplicationDbContext(options);
        return await db.Activities
            .AsNoTracking()
            .Include(entity => entity.Transitions)
            .SingleAsync(entity => entity.Id == activityId);
    }

    private static ActivityService Service(ApplicationDbContext db)
        => new(db, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator());

    private static string RequestData(string nominee)
        => $$"""
            {
              "assessor_user_id": "{{nominee}}",
              "observed_on": "2026-03-10",
              "presenting_problem": "Fever for three days"
            }
            """;

    private static string? ReadString(string dataJson, string key)
    {
        using var document = JsonDocument.Parse(dataJson);
        return document.RootElement.TryGetProperty(key, out var value) ? value.GetString() : null;
    }

    private static DbContextOptions<ApplicationDbContext> NewDatabase()
        => new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static ClaimsPrincipal Principal(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));

    /// <summary>The trainee holding the Trainee role, for a `role:Trainee` actor rule.</summary>
    private static ClaimsPrincipal TraineePrincipal()
        => new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, TraineeId), new Claim(ClaimTypes.Role, WombatRoles.Trainee)],
            "test",
            ClaimTypes.Name,
            ClaimTypes.Role));

    // ---- fixture ----------------------------------------------------------------------------------------------

    private static async Task SeedAsync(DbContextOptions<ApplicationDbContext> options)
    {
        await using var db = new ApplicationDbContext(options);

        // The trainee has no profile: the create stamps the institution from their Identity row, which is the same
        // answer SubjectScopeResolver gives a profiled trainee at that institution.
        NomineeSeed.AddUser(db, TraineeId, InstitutionId, WombatRoles.Trainee);
        NomineeSeed.AddUser(db, AssessorId, InstitutionId, WombatRoles.Assessor);
        NomineeSeed.AddUser(db, ReplacementAssessorId, InstitutionId, WombatRoles.Assessor);
        NomineeSeed.AddUser(db, CoordinatorId, InstitutionId, WombatRoles.Coordinator);
        NomineeSeed.AddUser(db, RegistrarId, InstitutionId, WombatRoles.Trainee);
        NomineeSeed.AddUser(db, ElsewhereAssessorId, OtherInstitutionId, WombatRoles.Assessor);
        NomineeSeed.AddUser(db, DeactivatedAssessorId, InstitutionId, UserDeactivation.IndefiniteLockoutEnd, WombatRoles.Assessor);

        db.ActivityTypes.AddRange(
            Type(CpsaTypeId, "cpsa_shaped", CpsaWorkflowJson),
            Type(LegacyTypeId, "legacy_shaped", LegacyWorkflowJson),
            Type(ResubmittableTypeId, "resubmittable", ResubmittableWorkflowJson),
            Type(RecallableTypeId, "recallable", RecallableWorkflowJson),
            Type(SignOffTypeId, "sign_off", SignOffWorkflowJson),
            Type(WithdrawableTypeId, "withdrawable", WithdrawableWorkflowJson),
            Type(LockedCancelTypeId, "locked_cancel", LockedCancelWorkflowJson),
            Type(ReopenIntoAssessmentTypeId, "reopen_into_assessment", ReopenIntoAssessmentWorkflowJson),
            Type(LoopBackTypeId, "loop_back", LoopBackWorkflowJson),
            Type(AssessorReassignTypeId, "assessor_reassign", AssessorReassignWorkflowJson, AssessorWritableNomineeSchemaJson),
            Type(AuthorKeepsEditingTypeId, "author_keeps_editing", AuthorKeepsEditingWorkflowJson),
            Type(AssessorPickupTypeId, "assessor_pickup", AssessorPickupWorkflowJson),
            Type(RoleSubmitTypeId, "role_submit", RoleSubmitWorkflowJson),
            Type(ResubmittableLegacyTypeId, "resubmittable_legacy", ResubmittableLegacyWorkflowJson));

        await db.SaveChangesAsync();
    }

    private static ActivityType Type(int id, string key, string workflowJson, string schemaJson = SchemaJson)
    {
        var publishedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        const string displayFieldsJson = """["assessor_user_id"]""";

        var activityType = new ActivityType
        {
            Id = id,
            Key = key,
            Name = key,
            Scope = ActivityScope.Institution,
            ScopeId = InstitutionId,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = CreditsNothing,
            DisplayFieldsJson = displayFieldsJson,
            OwnerUserId = "admin-1",
            CreatedOn = publishedOn
        };

        activityType.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = id,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = CreditsNothing,
            DisplayFieldsJson = displayFieldsJson,
            PublishedByUserId = "admin-1",
            PublishedOn = publishedOn
        });

        return activityType;
    }

    /// <summary>Credits nothing, so the EPA→tool gate never runs and every refusal here is the nominee gate's.</summary>
    private const string CreditsNothing = """{ "counts_for": [] }""";

    /// <summary>A CPSA-shaped request form: the trainee names the assessor; the assessor's section is theirs.</summary>
    private const string SchemaJson = """
        {
          "version": 1,
          "observation_date_field": "observed_on",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "assessor_user_id", "type": "user", "label": "Assessing consultant", "required": true },
                { "key": "observed_on", "type": "date", "label": "Date observed", "required": true },
                { "key": "presenting_problem", "type": "text", "label": "Presenting problem" }
              ]
            },
            {
              "key": "assessment",
              "title": "Entrustment",
              "editable_by": "field:assessor_user_id",
              "fields": [
                { "key": "overall_level", "type": "number", "label": "Supervision required for this encounter" }
              ]
            }
          ]
        }
        """;

    /// <summary>The request form with the nominee writable by the named assessor too, so they can hand it on.</summary>
    private static readonly string AssessorWritableNomineeSchemaJson = SchemaJson.Replace(
        """{ "key": "assessor_user_id", "type": "user", "label": "Assessing consultant", "required": true }""",
        """{ "key": "assessor_user_id", "type": "user", "label": "Assessing consultant", "required": true, "editable_by": "subject|creator|field:assessor_user_id" }""",
        StringComparison.Ordinal);

    /// <summary>The CPSA seeds' shape: draft, the author's submit, the assessor's complete or decline, and cancel.</summary>
    private const string CpsaWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true },
            { "key": "declined", "label": "Declined" },
            { "key": "cancelled", "label": "Cancelled" }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id", "requires_fields": ["overall_level"] },
            { "key": "decline", "from": "requested", "to": "declined", "actor": "field:assessor_user_id", "requires_note": true },
            { "key": "cancel", "from": ["draft", "requested"], "to": "cancelled", "actor": "subject|creator" }
          ]
        }
        """;

    /// <summary>The CPSA shape with an assessor's `reassign` that keeps the request in `requested`.</summary>
    private static readonly string AssessorReassignWorkflowJson = CpsaWorkflowJson.Replace(
        """{ "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator" },""",
        """{ "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator" }, { "key": "reassign", "from": "requested", "to": "requested", "actor": "field:assessor_user_id" },""",
        StringComparison.Ordinal);

    /// <summary>The CPSA shape with `requested` still editable by the author, alongside the assessor.</summary>
    private static readonly string AuthorKeepsEditingWorkflowJson = CpsaWorkflowJson.Replace(
        """{ "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" }""",
        """{ "key": "requested", "label": "Requested", "editable_by": "subject|creator|field:assessor_user_id" }""",
        StringComparison.Ordinal);

    /// <summary>The legacy WBA shape: born in `requested`, with no author transition but a cancel.</summary>
    private const string LegacyWorkflowJson = """
        {
          "version": 1,
          "initial_state": "requested",
          "states": [
            { "key": "requested", "label": "Requested" },
            { "key": "accepted", "label": "Accepted", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true },
            { "key": "declined", "label": "Declined" },
            { "key": "cancelled", "label": "Cancelled" }
          ],
          "transitions": [
            { "key": "accept", "from": "requested", "to": "accepted", "actor": "field:assessor_user_id" },
            { "key": "decline", "from": "requested", "to": "declined", "actor": "field:assessor_user_id", "requires_note": true },
            { "key": "cancel", "from": ["requested", "accepted"], "to": "cancelled", "actor": "subject|field:assessor_user_id" },
            { "key": "complete", "from": "accepted", "to": "completed", "actor": "field:assessor_user_id", "requires_fields": ["overall_level"] }
          ]
        }
        """;

    /// <summary>The legacy shape with a builder-added resubmission back into the initial `requested`.</summary>
    private const string ResubmittableLegacyWorkflowJson = """
        {
          "version": 1,
          "initial_state": "requested",
          "states": [
            { "key": "requested", "label": "Requested" },
            { "key": "accepted", "label": "Accepted", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true },
            { "key": "declined", "label": "Declined" }
          ],
          "transitions": [
            { "key": "accept", "from": "requested", "to": "accepted", "actor": "field:assessor_user_id" },
            { "key": "decline", "from": "requested", "to": "declined", "actor": "field:assessor_user_id", "requires_note": true },
            { "key": "resubmit", "from": "declined", "to": "requested", "actor": "subject" },
            { "key": "complete", "from": "accepted", "to": "completed", "actor": "field:assessor_user_id", "requires_fields": ["overall_level"] }
          ]
        }
        """;

    /// <summary>reflective_note's shape: `declined` is live, because the author may resubmit out of it.</summary>
    private const string ResubmittableWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true },
            { "key": "declined", "label": "Declined" },
            { "key": "cancelled", "label": "Cancelled" }
          ],
          "transitions": [
            { "key": "submit", "from": ["draft", "declined"], "to": "requested", "actor": "subject" },
            { "key": "revise", "from": "declined", "to": "draft", "actor": "subject" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id", "requires_fields": ["overall_level"] },
            { "key": "decline", "from": "requested", "to": "declined", "actor": "field:assessor_user_id", "requires_note": true },
            { "key": "cancel", "from": ["draft", "requested", "declined"], "to": "cancelled", "actor": "subject" }
          ]
        }
        """;

    /// <summary>CPSA's shape plus a `recall` back into the draft, and a `cancelled` that reopens only into the draft.</summary>
    private const string RecallableWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true },
            { "key": "cancelled", "label": "Cancelled" }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator" },
            { "key": "recall", "from": "requested", "to": "draft", "actor": "subject|creator" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id", "requires_fields": ["overall_level"] },
            { "key": "cancel", "from": ["draft", "requested"], "to": "cancelled", "actor": "subject|creator" },
            { "key": "reopen", "from": "cancelled", "to": "draft", "actor": "subject|creator" }
          ]
        }
        """;

    /// <summary>The recallable shape with a `cancelled` state the author cannot edit, though only the author reopens it.</summary>
    private static readonly string LockedCancelWorkflowJson = RecallableWorkflowJson.Replace(
        """{ "key": "cancelled", "label": "Cancelled" }""",
        """{ "key": "cancelled", "label": "Cancelled", "editable_by": "role:Coordinator" }""",
        StringComparison.Ordinal);

    /// <summary>A `cancelled` the assessor can reopen straight into assessment, skipping the draft phase.</summary>
    private const string ReopenIntoAssessmentWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true },
            { "key": "cancelled", "label": "Cancelled" }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id", "requires_fields": ["overall_level"] },
            { "key": "cancel", "from": ["draft", "requested"], "to": "cancelled", "actor": "subject|creator" },
            { "key": "reopen", "from": "cancelled", "to": "requested", "actor": "field:assessor_user_id" }
          ]
        }
        """;

    /// <summary>The scenario runbooks' shape: submit by `role:Trainee`, not by `subject`.</summary>
    private const string RoleSubmitWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "role:Trainee" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id", "requires_fields": ["overall_level"] }
          ]
        }
        """;

    /// <summary>A trainee sign-off after assessment: the trainee's move is the one that reaches credit.</summary>
    private const string SignOffWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" },
            { "key": "assessed", "label": "Assessed" },
            { "key": "completed", "label": "Completed", "terminal": true }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator" },
            { "key": "assess", "from": "requested", "to": "assessed", "actor": "field:assessor_user_id", "requires_fields": ["overall_level"] },
            { "key": "acknowledge", "from": "assessed", "to": "completed", "actor": "subject" }
          ]
        }
        """;

    /// <summary>A holding state instead of a recall to draft, plus an author's reassign that stays with the assessor.</summary>
    private const string WithdrawableWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" },
            { "key": "withdrawn", "label": "Withdrawn" },
            { "key": "completed", "label": "Completed", "terminal": true }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator" },
            { "key": "reassign", "from": "requested", "to": "requested", "actor": "subject|creator" },
            { "key": "withdraw", "from": "requested", "to": "withdrawn", "actor": "subject|creator" },
            { "key": "resubmit", "from": "withdrawn", "to": "requested", "actor": "subject|creator" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id", "requires_fields": ["overall_level"] }
          ]
        }
        """;

    /// <summary>A draft the named assessor may pick up without waiting for the author's submit.</summary>
    private const string AssessorPickupWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator" },
            { "key": "pick_up", "from": "draft", "to": "requested", "actor": "field:assessor_user_id" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id", "requires_fields": ["overall_level"] }
          ]
        }
        """;

    /// <summary>A handover whose way to credit comes back through the draft after the assessor has rated it.</summary>
    private const string LoopBackWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "with_assessor", "label": "With assessor", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true }
          ],
          "transitions": [
            { "key": "handover", "from": "draft", "to": "with_assessor", "actor": "subject|creator" },
            { "key": "rate", "from": "with_assessor", "to": "draft", "actor": "field:assessor_user_id", "requires_fields": ["overall_level"] },
            { "key": "finalize", "from": "draft", "to": "completed", "actor": "subject", "requires_fields": ["overall_level"] }
          ]
        }
        """;
}
