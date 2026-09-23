using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T122, the write-path half of the EPA→tool allow-list, driven through the real <see cref="ActivityService" />.
/// </summary>
/// <remarks>
/// <para>
/// The picker narrows what a trainee is offered; this gate is what actually refuses (D20). It checks at create; on
/// any move that CHANGES the credit target and can still lead to credit; and, for an unchanged target, only when the
/// author, before anyone else has acted, hands it on while still able to write its field. So an assessor's
/// completion that leaves the EPA alone is not re-checked, an allow-list edited after an encounter was filed never
/// strands it, and credit never re-litigates the list. Everything the gate does not know is let through (D21).
/// </para>
/// <para>
/// Every refusal is also checked against the audit trap. <c>AuditPipelineBehavior</c>'s catch saves the request's
/// shared DbContext, so a refusal thrown after any mutation would COMMIT that mutation under a failed command. Each
/// refusal therefore asserts the change tracker holds nothing Added, Modified or Deleted, then performs the audit
/// save itself and asserts it writes nothing (the <see cref="CreditPlannedBeforeTransitionTests" /> template).
/// </para>
/// <para>
/// Each call runs on its own DbContext over one InMemory store, as each request does in the product. That matters
/// here more than elsewhere: an allow-list is edited by an administrator in a different request, and a single shared
/// context would hand the gate its own stale tracked copy of the curriculum item.
/// </para>
/// </remarks>
public sealed class ToolPermissionGateTests
{
    private const string TraineeId = "trainee-1";
    private const string UnprofiledTraineeId = "trainee-2";
    private const string AssessorId = "assessor-1";

    private const int InstitutionId = 10;
    private const int CurriculumId = 3000;
    private const int OtherCurriculumId = 3001;

    // PAED-006's item permits Mini-CEX and CBD. PAED-005's item permits CBD, DOPS and MSF, and so forbids Mini-CEX.
    private const int PermittingEpaId = 6;
    private const int ForbiddingEpaId = 5;
    private const int UnlistedEpaId = 1;
    private const int MalformedListEpaId = 2;
    private const int OffCurriculumEpaId = 9;

    private const int PermittingItemId = 4006;
    private const int ForbiddingItemId = 4005;

    private const int MiniCexTypeId = 100;
    private const int CbdTypeId = 101;
    private const int UnkeyedTypeId = 102;
    private const int LegacyMiniCexTypeId = 103;
    private const int TerminalMiniCexTypeId = 104;
    private const int NonCreditingMiniCexTypeId = 105;
    private const int ResubmittableMiniCexTypeId = 106;
    private const int RecallableMiniCexTypeId = 107;
    private const int ReopenIntoAssessmentMiniCexTypeId = 108;
    private const int ManyItemsMiniCexTypeId = 109;
    private const int RoleSubmitMiniCexTypeId = 110;
    private const int SignOffMiniCexTypeId = 111;
    private const int WithdrawableMiniCexTypeId = 112;
    private const int ResubmittableLegacyMiniCexTypeId = 113;
    private const int MixedAcceptLegacyMiniCexTypeId = 114;
    private const int AssessorPickupMiniCexTypeId = 115;
    private const int AssessorEditableEpaMiniCexTypeId = 116;
    private const int TwoEpaMiniCexTypeId = 117;
    private const int LoopBackMiniCexTypeId = 118;
    private const int LockedCancelMiniCexTypeId = 119;

    /// <summary>The fragment every gate refusal carries, so a test can tell the gate apart from any other refusal.</summary>
    private const string GateRefusal = "cannot be used as evidence";

    // ---- 1 ----------------------------------------------------------------------------------------------------

    [Fact]
    public async Task APermittedTool_IsFiledSubmittedAndCompleted_AndCreditsExactlyOneProgressRow()
    {
        // The control for everything below: a gate that refused too much would pass every refusal test. CBD is on
        // PAED-005's list, so the whole CPSA path must run, and credit must land once on that item.
        var options = NewDatabase();
        await SeedAsync(options);

        var created = await CreateAsync(options, CbdTypeId, ForbiddingEpaId);
        await TransitionAsync(options, created.Id, "submit", TraineeId);
        var completed = await TransitionAsync(options, created.Id, "complete", AssessorId, """{ "overall_level": 4 }""");

        completed.CurrentState.Should().Be("completed");
        var row = (await ProgressRowsAsync(options)).Should().ContainSingle().Subject;
        row.CurriculumItemId.Should().Be(ForbiddingItemId);
        row.TraineeUserId.Should().Be(TraineeId);
        row.CountsSoFar.Should().Be(1);
        (await StoredAsync(options, created.Id)).Transitions
            .Single(transition => transition.TransitionKey == "complete").CreditedItemCount.Should().Be(1);
    }

    // ---- 2 ----------------------------------------------------------------------------------------------------

    [Fact]
    public async Task AForbiddenTool_IsRefusedAtCreate_WithAMessageLedByTheFieldLabel_AndNothingIsPersisted()
    {
        // Gating the create means a refused Submit on /activities/new fails before the draft exists, so no orphan
        // draft is left behind. The message is the one alert the page shows: it must name the instrument, the EPA
        // and the instruments the curriculum does accept, and lead with the field's label ("epa_id:" means nothing
        // to a registrar).
        var options = NewDatabase();
        await SeedAsync(options);

        var message = await ShouldBeRefusedAsync(options, service => service.CreateDraftAsync(
            new CreateActivityInput(MiniCexTypeId, TraineeId, TraineeId, RequestData(ForbiddingEpaId), Principal(TraineeId))));

        message.Should().StartWith("EPA observed: ", "the refusal is led by the field's label, not its key");
        message.Should().NotContain("epa_id");
        message.Should().Contain("Mini-CEX");
        message.Should().Contain("PAED-005");
        message.Should().Contain("CBD, DOPS or MSF");
        message.Should().Contain(GateRefusal);

        await using var verify = new ApplicationDbContext(options);
        (await verify.Activities.CountAsync()).Should().Be(0, "a refused create must leave no activity behind");
        (await verify.ActivityTransitions.CountAsync()).Should().Be(0);
    }

    // ---- 3 ----------------------------------------------------------------------------------------------------

    [Fact]
    public async Task ADraftFiledBeforeTheListForbadeItsTool_IsRefusedAtSubmit_AndStaysADraftWithItsDataUnchanged()
    {
        // D20 applies the rule in force at SUBMISSION. A draft filed while the item was unrestricted is caught when
        // the author submits it. The submit also carries an edit to a non-credit field, so "unchanged" proves the
        // patch was not merged into the tracked entity before the gate threw.
        var options = NewDatabase();
        await SeedAsync(options);
        await SetPermittedToolsAsync(options, ForbiddingItemId);

        var draft = await CreateAsync(options, MiniCexTypeId, ForbiddingEpaId);
        await SetPermittedToolsAsync(options, ForbiddingItemId, "cbd", "dops", "msf");

        var message = await ShouldBeRefusedAsync(options, service => service.TransitionAsync(new TransitionActivityInput(
            draft.Id, "submit", TraineeId, Principal(TraineeId), """{ "presenting_problem": "Revised before submitting" }""", null)));

        message.Should().Contain(GateRefusal).And.Contain("PAED-005");

        var stored = await StoredAsync(options, draft.Id);
        stored.CurrentState.Should().Be("draft");
        ReadString(stored.DataJson, "presenting_problem").Should().Be("Fever for three days");
        ReadInt(stored.DataJson, "epa_id").Should().Be(ForbiddingEpaId);
        stored.ObservedOn.Should().Be(new DateOnly(2026, 3, 10));
        stored.Transitions.Select(transition => transition.TransitionKey).Should().Equal("create");
    }

    // ---- 4 ----------------------------------------------------------------------------------------------------

    [Fact]
    public async Task AForbiddenDraft_CanStillBeCancelled_BecauseAMoveIntoADeadEndIsExempt()
    {
        // `cancelled` is a non-terminal dead end: nothing leaves it and it never reaches credit. Gating the move into
        // it would trap the trainee with a draft they can neither submit nor withdraw.
        var options = NewDatabase();
        await SeedAsync(options);
        await SetPermittedToolsAsync(options, ForbiddingItemId);

        var draft = await CreateAsync(options, MiniCexTypeId, ForbiddingEpaId);
        await SetPermittedToolsAsync(options, ForbiddingItemId, "cbd", "dops", "msf");

        (await ShouldBeRefusedAsync(options, service => service.TransitionAsync(new TransitionActivityInput(
            draft.Id, "submit", TraineeId, Principal(TraineeId), null, null))))
            .Should().Contain(GateRefusal, "guard: this draft really is forbidden");

        var cancelled = await TransitionAsync(options, draft.Id, "cancel", TraineeId);

        cancelled.CurrentState.Should().Be("cancelled");
    }

    // ---- 5 ----------------------------------------------------------------------------------------------------

    [Fact]
    public async Task ARequestSubmittedWhilePermitted_CanBeDeclinedByTheAssessor_AfterTheListForbidsItsTool()
    {
        // The assessor's decline changes no credit target and leads nowhere credit can follow, so it is never checked.
        var options = NewDatabase();
        await SeedAsync(options);
        await SetPermittedToolsAsync(options, ForbiddingItemId);

        var request = await CreateAsync(options, MiniCexTypeId, ForbiddingEpaId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await SetPermittedToolsAsync(options, ForbiddingItemId, "cbd", "dops", "msf");
        await AssertNowForbiddenAsync(options, MiniCexTypeId, ForbiddingEpaId);

        var declined =await TransitionAsync(options, request.Id, "decline", AssessorId, note: "Not observed by me.");

        declined.CurrentState.Should().Be("declined");
    }

    [Fact]
    public async Task ARequestSubmittedWhilePermitted_IsCompletedAndCredited_AfterTheListForbidsItsTool()
    {
        // D20: the encounter was filed legitimately under the rule in force at the time. The assessor's completion,
        // which echoes the EPA unchanged as a full-form post-back does, must neither be refused nor lose its credit.
        var options = NewDatabase();
        await SeedAsync(options);
        await SetPermittedToolsAsync(options, ForbiddingItemId);

        var request = await CreateAsync(options, MiniCexTypeId, ForbiddingEpaId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await SetPermittedToolsAsync(options, ForbiddingItemId, "cbd", "dops", "msf");
        await AssertNowForbiddenAsync(options, MiniCexTypeId, ForbiddingEpaId);

        var completed = await TransitionAsync(
            options, request.Id, "complete", AssessorId, $$"""{ "epa_id": {{ForbiddingEpaId}}, "overall_level": 4 }""");

        completed.CurrentState.Should().Be("completed");
        var row = (await ProgressRowsAsync(options)).Should().ContainSingle().Subject;
        row.CurriculumItemId.Should().Be(ForbiddingItemId);
        row.CountsSoFar.Should().Be(1, "credit never re-checks the allow-list (D20)");
    }

    // ---- 6 ----------------------------------------------------------------------------------------------------

    [Fact]
    public async Task UpdateDraft_SwitchingARequestToAForbiddenEpa_IsRefused_AndLeavesNothingToCommit()
    {
        // The bypass UpdateDraftAsync would otherwise open: it replaces the whole payload in any non-terminal state,
        // so a trainee could submit against a permitted EPA, switch to a forbidden one while the request sits with
        // the assessor, and have the assessor's unchanged completion credit it.
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, MiniCexTypeId, PermittingEpaId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);

        var message = await ShouldBeRefusedAsync(options, service => service.UpdateDraftAsync(
            new UpdateActivityDraftInput(request.Id, TraineeId, RequestData(ForbiddingEpaId), Principal(TraineeId))));

        message.Should().Contain(GateRefusal).And.Contain("PAED-005");

        var stored = await StoredAsync(options, request.Id);
        stored.CurrentState.Should().Be("requested");
        ReadInt(stored.DataJson, "epa_id").Should().Be(PermittingEpaId);
    }

    [Fact]
    public async Task UpdateDraft_ChangingOnlyANonCreditField_Passes_EvenThoughTheStoredEpaIsNowForbidden()
    {
        // Only a change of credit target is re-checked. Correcting the presenting problem on a request filed
        // legitimately must not be refused because the list was edited since.
        var options = NewDatabase();
        await SeedAsync(options);
        await SetPermittedToolsAsync(options, ForbiddingItemId);

        var request = await CreateAsync(options, MiniCexTypeId, ForbiddingEpaId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await SetPermittedToolsAsync(options, ForbiddingItemId, "cbd", "dops", "msf");
        await AssertNowForbiddenAsync(options, MiniCexTypeId, ForbiddingEpaId);

        await using (var db = new ApplicationDbContext(options))
        {
            await Service(db).UpdateDraftAsync(new UpdateActivityDraftInput(
                request.Id,
                TraineeId,
                RequestData(ForbiddingEpaId, presentingProblem: "Fever and a rash"),
                Principal(TraineeId)));
        }

        var stored = await StoredAsync(options, request.Id);
        ReadString(stored.DataJson, "presenting_problem").Should().Be("Fever and a rash");
        ReadInt(stored.DataJson, "epa_id").Should().Be(ForbiddingEpaId);
    }

    // ---- 7 ----------------------------------------------------------------------------------------------------

    [Fact]
    public async Task Submit_WithAPatchThatRepairsTheEpaToAPermittedOne_Passes_BecauseTheGateReadsTheMergedData()
    {
        // The refusal message tells the trainee to pick another EPA, and the page sends that pick in the same patch
        // as the submit. A gate reading the stored data would refuse the repair itself.
        var options = NewDatabase();
        await SeedAsync(options);
        await SetPermittedToolsAsync(options, ForbiddingItemId);

        var draft = await CreateAsync(options, MiniCexTypeId, ForbiddingEpaId);
        await SetPermittedToolsAsync(options, ForbiddingItemId, "cbd", "dops", "msf");
        await AssertNowForbiddenAsync(options, MiniCexTypeId, ForbiddingEpaId);

        var submitted = await TransitionAsync(
            options, draft.Id, "submit", TraineeId, $$"""{ "epa_id": {{PermittingEpaId}} }""");

        submitted.CurrentState.Should().Be("requested");
        ReadInt(submitted.DataJson, "epa_id").Should().Be(PermittingEpaId);
    }

    [Fact]
    public async Task Submit_WithAPatchThatChangesAPermittedEpaToAForbiddenOne_IsRefused()
    {
        // The mirror of the repair: the stored EPA is fine, the one being written is not.
        var options = NewDatabase();
        await SeedAsync(options);

        var draft = await CreateAsync(options, MiniCexTypeId, PermittingEpaId);

        var message = await ShouldBeRefusedAsync(options, service => service.TransitionAsync(new TransitionActivityInput(
            draft.Id, "submit", TraineeId, Principal(TraineeId), $$"""{ "epa_id": {{ForbiddingEpaId}} }""", null)));

        message.Should().Contain(GateRefusal).And.Contain("PAED-005");

        var stored = await StoredAsync(options, draft.Id);
        stored.CurrentState.Should().Be("draft");
        ReadInt(stored.DataJson, "epa_id").Should().Be(PermittingEpaId);
    }

    // ---- 8 ----------------------------------------------------------------------------------------------------

    [Fact]
    public async Task AnAssessorWhoIsAlsoTheCreator_ChangingTheEpaAtCompleteToAForbiddenOne_IsRefused()
    {
        // The creator owns the request fields, and in `requested` the bound assessor owns the state, so an
        // assessor-creator CAN rewrite epa_id on complete: the merge accepts it. A change of credit target is checked
        // on any move that can still lead to credit, whoever makes it.
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, MiniCexTypeId, PermittingEpaId, creator: AssessorId);
        await TransitionAsync(options, request.Id, "submit", AssessorId);

        var message = await ShouldBeRefusedAsync(options, service => service.TransitionAsync(new TransitionActivityInput(
            request.Id,
            "complete",
            AssessorId,
            Principal(AssessorId),
            $$"""{ "epa_id": {{ForbiddingEpaId}}, "overall_level": 4 }""",
            null)));

        message.Should().Contain(GateRefusal, "the merge accepted the change; the refusal must come from the gate")
            .And.Contain("PAED-005");

        var stored = await StoredAsync(options, request.Id);
        stored.CurrentState.Should().Be("requested");
        ReadInt(stored.DataJson, "epa_id").Should().Be(PermittingEpaId);
        (await ProgressRowsAsync(options)).Should().BeEmpty();
    }

    [Fact]
    public async Task AnAssessorWhoIsAlsoTheCreator_IsTheAuthor_SoTheirUnchangedCompletionIsReChecked_AndTheyCanRepairIt()
    {
        // Rule of the third review round: an unchanged target is re-checked while nobody but the author has acted and
        // the person moving it can correct it. An assessor who CREATED the activity is its author, nobody else has
        // acted, and they can write the EPA at `complete`, so the refusal is theirs to act on, and they do.
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, MiniCexTypeId, PermittingEpaId, creator: AssessorId);
        await TransitionAsync(options, request.Id, "submit", AssessorId);
        await SetPermittedToolsAsync(options, PermittingItemId, "cbd");

        var message = await ShouldBeRefusedAsync(options, service => service.TransitionAsync(new TransitionActivityInput(
            request.Id, "complete", AssessorId, Principal(AssessorId), """{ "overall_level": 4 }""", null)));
        message.Should().Contain(GateRefusal).And.Contain("PAED-006");

        var completed = await TransitionAsync(
            options, request.Id, "complete", AssessorId, $$"""{ "epa_id": {{UnlistedEpaId}}, "overall_level": 4 }""");
        completed.CurrentState.Should().Be("completed");
        (await ProgressRowsAsync(options)).Should().ContainSingle().Which.CurriculumItemId.Should().Be(4001);
    }

    [Fact]
    public async Task ReSendingTheSameEpaAsAString_IsNotAChangeOfTarget()
    {
        // A select posts "5" where the stored value is 5; the credit engine reads both as EPA 5. After a decline the
        // author may write the EPA again, and re-sending the same one must not count as a change that re-opens the
        // check (the assessor has acted, so an unchanged target is not re-litigated).
        var options = NewDatabase();
        await SeedAsync(options);
        await SetPermittedToolsAsync(options, ForbiddingItemId);

        var request = await CreateAsync(options, ResubmittableMiniCexTypeId, ForbiddingEpaId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await TransitionAsync(options, request.Id, "decline", AssessorId, note: "Please add the findings.");
        await SetPermittedToolsAsync(options, ForbiddingItemId, "cbd", "dops", "msf");
        await AssertNowForbiddenAsync(options, ResubmittableMiniCexTypeId, ForbiddingEpaId);

        var resubmitted = await TransitionAsync(
            options, request.Id, "submit", TraineeId, $$"""{ "epa_id": "{{ForbiddingEpaId}}" }""");

        resubmitted.CurrentState.Should().Be("requested");
    }

    // ---- 9 ----------------------------------------------------------------------------------------------------

    [Fact]
    public async Task ALegacyShapedKeyedType_IsRefusedAtCreate_BecauseItsCreateIsTheAuthorsSubmission()
    {
        // The legacy WBA shape starts in `requested`: there is no author transition, so the create is the only point
        // at which the author's choice of EPA can be checked.
        var options = NewDatabase();
        await SeedAsync(options);

        var message = await ShouldBeRefusedAsync(options, service => service.CreateDraftAsync(
            new CreateActivityInput(LegacyMiniCexTypeId, TraineeId, TraineeId, RequestData(ForbiddingEpaId), Principal(TraineeId))));

        message.Should().Contain(GateRefusal).And.Contain("Mini-CEX").And.Contain("PAED-005");

        await using var verify = new ApplicationDbContext(options);
        (await verify.Activities.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ALegacyShapedKeyedType_IsNotGatedOnTheAssessorsAcceptOrComplete_AfterTheListForbidsItsTool()
    {
        // `accept` and `complete` are the assessor's moves (field:assessor_user_id), neither changes the target, and
        // the create was already checked. Re-gating them would strand a request filed under the earlier rule.
        var options = NewDatabase();
        await SeedAsync(options);
        await SetPermittedToolsAsync(options, ForbiddingItemId);

        var request = await CreateAsync(options, LegacyMiniCexTypeId, ForbiddingEpaId);
        request.CurrentState.Should().Be("requested", "guard: this is the legacy shape");
        await SetPermittedToolsAsync(options, ForbiddingItemId, "cbd", "dops", "msf");
        await AssertNowForbiddenAsync(options, LegacyMiniCexTypeId, ForbiddingEpaId);

        var accepted =await TransitionAsync(options, request.Id, "accept", AssessorId);
        accepted.CurrentState.Should().Be("accepted");

        var completed = await TransitionAsync(options, request.Id, "complete", AssessorId, """{ "overall_level": 4 }""");
        completed.CurrentState.Should().Be("completed");
        (await ProgressRowsAsync(options)).Should().ContainSingle()
            .Which.CurriculumItemId.Should().Be(ForbiddingItemId);
    }

    // ---- 10 ---------------------------------------------------------------------------------------------------

    [Fact]
    public async Task AKeyedCreditingTypeWithATerminalInitialState_IsRefusedAtCreate()
    {
        // A type that is born finished has no later move to gate. The create is the whole of it.
        var options = NewDatabase();
        await SeedAsync(options);

        var message = await ShouldBeRefusedAsync(options, service => service.CreateDraftAsync(
            new CreateActivityInput(TerminalMiniCexTypeId, TraineeId, TraineeId, RequestData(ForbiddingEpaId), Principal(TraineeId))));

        message.Should().Contain(GateRefusal).And.Contain("PAED-005");

        await using var verify = new ApplicationDbContext(options);
        (await verify.Activities.CountAsync()).Should().Be(0);
    }

    // ---- 11: D21, permissive wherever the answer is unknown ----------------------------------------------------

    [Fact]
    public async Task D21_ATypeWithNoToolKey_IsAcceptedAgainstAnItemWhoseListWouldForbidAKeyedTool()
    {
        // Every builder-made type is unkeyed until an administrator chooses an instrument for it. Refusing those
        // would make every institution-built WBA unfileable against the catalogue.
        var options = NewDatabase();
        await SeedAsync(options);

        await AssertAcceptedAsync(options, UnkeyedTypeId, ForbiddingEpaId);
    }

    [Fact]
    public async Task D21_AnItemWithNoList_AcceptsAKeyedTool()
    {
        var options = NewDatabase();
        await SeedAsync(options);

        await AssertAcceptedAsync(options, MiniCexTypeId, UnlistedEpaId);
    }

    [Fact]
    public async Task D21_AnItemWhoseListIsMalformedJson_AcceptsAKeyedTool()
    {
        // Only direct SQL can store this. Refusing on it would be a refusal nobody can explain or act on.
        var options = NewDatabase();
        await SeedAsync(options);

        await AssertAcceptedAsync(options, MiniCexTypeId, MalformedListEpaId);
    }

    [Fact]
    public async Task D21_ASubjectWithNoTraineeProfile_IsAccepted()
    {
        // Nothing would be credited today, and refusing would turn the gate into a curriculum-membership check that
        // neither D20 nor D21 decided.
        var options = NewDatabase();
        await SeedAsync(options);

        await AssertAcceptedAsync(options, MiniCexTypeId, ForbiddingEpaId, subject: UnprofiledTraineeId);
    }

    [Fact]
    public async Task D21_AnEpaWithNoItemOnTheSubjectsCurriculum_IsAccepted_EvenIfAnotherCurriculumForbidsTheTool()
    {
        // The gate answers with the credit engine's resolver, scoped to the subject's own curriculum. An item on a
        // curriculum the trainee is not on can neither be credited nor forbid anything.
        var options = NewDatabase();
        await SeedAsync(options);

        await AssertAcceptedAsync(options, MiniCexTypeId, OffCurriculumEpaId);
    }

    [Fact]
    public async Task D21_AKeyedTypeWhosePinnedRulesCreditNothing_IsAccepted()
    {
        // msf_cpsa and every unrated instrument declare `counts_for: []`. They can credit no item, so there is
        // nothing to protect.
        var options = NewDatabase();
        await SeedAsync(options);

        await AssertAcceptedAsync(options, NonCreditingMiniCexTypeId, ForbiddingEpaId);
    }

    // ---- 12 ---------------------------------------------------------------------------------------------------

    [Fact]
    public async Task D20Boundary_AProfileCreatedAfterSubmission_IsNotReChecked_AndTheForbiddenEpaIsCredited()
    {
        // KNOWN BEHAVIOUR, pinned deliberately (design change 12). The gate evaluates the curriculum the subject is on
        // at the moment of the gated write, and a subject with no profile passes (D21). A profile created, or
        // re-pointed, between submission and completion is not re-checked when the assessor completes, because
        // credit never re-litigates (D20), so CreditApplier credits the forbidden EPA. If this test starts failing,
        // the D20 boundary has moved: that needs a decision in EPA-PROGRAMME, not a test edit.
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, MiniCexTypeId, ForbiddingEpaId, subject: UnprofiledTraineeId);
        await TransitionAsync(options, request.Id, "submit", UnprofiledTraineeId);

        await using (var admin = new ApplicationDbContext(options))
        {
            admin.TraineeProfiles.Add(new TraineeProfile
            {
                Id = 2,
                UserId = UnprofiledTraineeId,
                InstitutionId = InstitutionId,
                CurriculumId = CurriculumId,
                ProgrammeStartDate = new DateOnly(2025, 4, 14),
                ExpectedCompletionDate = new DateOnly(2029, 4, 13),
                IsActive = true
            });
            await admin.SaveChangesAsync();
        }

        // Guard: with the profile in place the gate does bite, so what follows is the boundary and not a gate that
        // is simply off for this trainee.
        await AssertNowForbiddenAsync(options, MiniCexTypeId, ForbiddingEpaId, subject: UnprofiledTraineeId);

        var completed =await TransitionAsync(options, request.Id, "complete", AssessorId, """{ "overall_level": 4 }""");

        completed.CurrentState.Should().Be("completed");
        var row = (await ProgressRowsAsync(options)).Should().ContainSingle().Subject;
        row.TraineeUserId.Should().Be(UnprofiledTraineeId);
        row.CurriculumItemId.Should().Be(ForbiddingItemId, "PAED-005 forbids Mini-CEX, and it is credited all the same");
        row.CountsSoFar.Should().Be(1);
    }

    // ---- 13 ---------------------------------------------------------------------------------------------------

    [Fact]
    public async Task AResubmissionFromDeclined_WithTheSameEpa_IsNotReChecked_BecauseTheAssessorHasActed()
    {
        // Settled by T122's review rounds. A resubmission after a decline re-submits a target that was checked at create
        // and again when the author handed it on, and the assessor has since acted on it. D20 never re-litigates an
        // unchanged target once somebody else has acted: the same rule that lets the assessor's completion credit
        // after a list edit, and that keeps a trainee's sign-off after assessment from stranding the encounter.
        var options = NewDatabase();
        await SeedAsync(options);
        await SetPermittedToolsAsync(options, ForbiddingItemId);

        var request = await CreateAsync(options, ResubmittableMiniCexTypeId, ForbiddingEpaId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await TransitionAsync(options, request.Id, "decline", AssessorId, note: "Please add the examination findings.");
        await SetPermittedToolsAsync(options, ForbiddingItemId, "cbd", "dops", "msf");
        await AssertNowForbiddenAsync(options, ResubmittableMiniCexTypeId, ForbiddingEpaId);

        var resubmitted = await TransitionAsync(options, request.Id, "submit", TraineeId);

        resubmitted.CurrentState.Should().Be("requested");
    }

    [Fact]
    public async Task AResubmissionFromDeclined_ThatChangesTheEpaToAForbiddenOne_IsRefused()
    {
        // A CHANGED target is checked on any move that can still lead to credit, whoever makes it.
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, ResubmittableMiniCexTypeId, PermittingEpaId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await TransitionAsync(options, request.Id, "decline", AssessorId, note: "Wrong EPA.");

        var message = await ShouldBeRefusedAsync(options, service => service.TransitionAsync(new TransitionActivityInput(
            request.Id, "submit", TraineeId, Principal(TraineeId), $$"""{ "epa_id": {{ForbiddingEpaId}} }""", null)));

        message.Should().Contain(GateRefusal).And.Contain("PAED-005");
        (await StoredAsync(options, request.Id)).CurrentState.Should().Be("declined");
    }

    // ---- 14: moves AWAY from credit are withdrawals (the review's finding, 2026-09-23) -----------------------------

    [Fact]
    public async Task ARecallIntoTheDraftPhase_IsNotASubmission_SoATraineeCanPullBackARequestWhoseEpaIsNowForbidden()
    {
        // Found by T122's first review round. A builder-made `recall` (requested -> draft, subject|creator) was refused by
        // the first cut of the gate, while the assessor's unchanged `complete` credited. In `requested` the author
        // cannot write the EPA, so the recall is not checked; the submit out of the draft that follows is.
        var options = NewDatabase();
        await SeedAsync(options);
        await SetPermittedToolsAsync(options, ForbiddingItemId);

        var request = await CreateAsync(options, RecallableMiniCexTypeId, ForbiddingEpaId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await SetPermittedToolsAsync(options, ForbiddingItemId, "cbd", "dops", "msf");
        await AssertNowForbiddenAsync(options, RecallableMiniCexTypeId, ForbiddingEpaId);

        var recalled = await TransitionAsync(options, request.Id, "recall", TraineeId);
        recalled.CurrentState.Should().Be("draft");

        // ...and the resubmission out of the draft phase is the gated move.
        var message = await ShouldBeRefusedAsync(options, service => service.TransitionAsync(new TransitionActivityInput(
            request.Id, "submit", TraineeId, Principal(TraineeId), null, null)));
        message.Should().Contain(GateRefusal);
    }

    [Fact]
    public async Task ACancelIntoAStateThatReopensOnlyIntoTheDraftPhase_IsAWithdrawal_AndIsNotGated()
    {
        // Cancelled from `requested`, where the author cannot write the EPA, so it is not checked. (The case where the
        // withdrawal test itself decides, a cancel out of the draft, is the next test.)
        var options = NewDatabase();
        await SeedAsync(options);
        await SetPermittedToolsAsync(options, ForbiddingItemId);

        var request = await CreateAsync(options, RecallableMiniCexTypeId, ForbiddingEpaId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await SetPermittedToolsAsync(options, ForbiddingItemId, "cbd", "dops", "msf");
        await AssertNowForbiddenAsync(options, RecallableMiniCexTypeId, ForbiddingEpaId);

        var cancelled = await TransitionAsync(options, request.Id, "cancel", TraineeId);
        cancelled.CurrentState.Should().Be("cancelled");
    }

    [Fact]
    public async Task ACancelOutOfTheDraftIntoAStateThatReopensOnlyIntoTheDraft_IsAWithdrawal_AndIsNotGated()
    {
        // The author can write the EPA in `draft` and nobody else has acted, so only the withdrawal test stands between
        // this cancel and a refusal: `cancelled` reaches credit only by coming back through `draft`, where it started.
        var options = NewDatabase();
        await SeedAsync(options);
        await SetPermittedToolsAsync(options, ForbiddingItemId);

        var draft = await CreateAsync(options, RecallableMiniCexTypeId, ForbiddingEpaId);
        await SetPermittedToolsAsync(options, ForbiddingItemId, "cbd", "dops", "msf");
        await AssertNowForbiddenAsync(options, RecallableMiniCexTypeId, ForbiddingEpaId);

        (await TransitionAsync(options, draft.Id, "cancel", TraineeId)).CurrentState.Should().Be("cancelled");
    }

    [Fact]
    public async Task ACancelOutOfTheDraftThatTheAssessorCanReopenIntoAssessment_StaysGated()
    {
        // The one builder shape deliberately left refused, pinned so a change to it is a decision. The author can write
        // the EPA in `draft` and nobody else has acted, and here the assessor can reopen a cancelled request straight
        // into `requested` and complete it with no further author move: credit can follow the cancel without coming
        // back through `draft`, so the cancel hands the target on and is checked.
        var options = NewDatabase();
        await SeedAsync(options);
        await SetPermittedToolsAsync(options, ForbiddingItemId);

        var draft = await CreateAsync(options, ReopenIntoAssessmentMiniCexTypeId, ForbiddingEpaId);
        await SetPermittedToolsAsync(options, ForbiddingItemId, "cbd", "dops", "msf");

        var message = await ShouldBeRefusedAsync(options, service => service.TransitionAsync(new TransitionActivityInput(
            draft.Id, "cancel", TraineeId, Principal(TraineeId), null, null)));
        message.Should().Contain(GateRefusal);
    }

    // ---- 16: the second review round's shapes ------------------------------------------------------------------

    [Fact]
    public async Task ARoleActorSubmit_IsTheAuthorHandingItOn_AndIsRefusedForADraftFiledBeforeTheListForbadeItsTool()
    {
        // The scenario runbooks build types whose submit is `role:Trainee`, not `subject`. The gate never reads the
        // actor rule: the mover is the subject, nobody else has acted, they can write the EPA in `draft`, and the submit
        // hands it on, so it is checked.
        var options = NewDatabase();
        await SeedAsync(options);
        await SetPermittedToolsAsync(options, ForbiddingItemId);

        var draft = await CreateAsync(options, RoleSubmitMiniCexTypeId, ForbiddingEpaId);
        await SetPermittedToolsAsync(options, ForbiddingItemId, "cbd", "dops", "msf");

        var message = await ShouldBeRefusedAsync(options, service => service.TransitionAsync(new TransitionActivityInput(
            draft.Id, "submit", TraineeId, TraineePrincipal(), null, null)));

        message.Should().Contain(GateRefusal);
        (await StoredAsync(options, draft.Id)).CurrentState.Should().Be("draft");
    }

    [Fact]
    public async Task ATraineesSignOffAfterAssessment_IsNotReChecked_AndTheAssessedEncounterCredits()
    {
        // The review's major finding. draft -submit-> requested -assess-> assessed -acknowledge(subject)-> completed.
        // The trainee's acknowledge is the move that reaches credit, but the encounter was checked when it was handed
        // over, and the assessor has rated it. Refusing the sign-off would strand an assessed encounter for ever.
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, SignOffMiniCexTypeId, PermittingEpaId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await TransitionAsync(options, request.Id, "assess", AssessorId, patch: """{ "overall_level": 4 }""");
        await SetPermittedToolsAsync(options, PermittingItemId, "cbd");
        await AssertNowForbiddenAsync(options, SignOffMiniCexTypeId, PermittingEpaId);

        var completed = await TransitionAsync(options, request.Id, "acknowledge", TraineeId);

        completed.CurrentState.Should().Be("completed");
        completed.Transitions.Last().CreditedItemCount.Should().Be(1);
    }

    [Fact]
    public async Task AWithdrawalIntoAHoldingState_IsNotGated_EvenThoughCreditCanFollowAResubmission()
    {
        // requested -withdraw-> withdrawn -resubmit-> requested. In `requested` the author cannot write the EPA (the
        // state belongs to the assessor), so a refusal there is nothing they could act on, and it is not checked.
        var options = NewDatabase();
        await SeedAsync(options);
        await SetPermittedToolsAsync(options, ForbiddingItemId);

        var request = await CreateAsync(options, WithdrawableMiniCexTypeId, ForbiddingEpaId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await SetPermittedToolsAsync(options, ForbiddingItemId, "cbd", "dops", "msf");
        await AssertNowForbiddenAsync(options, WithdrawableMiniCexTypeId, ForbiddingEpaId);

        (await TransitionAsync(options, request.Id, "withdraw", TraineeId)).CurrentState.Should().Be("withdrawn");
    }

    [Fact]
    public async Task AReassignThatStaysWithTheAssessor_IsNotGated_BecauseTheAuthorCannotCorrectTheTargetThere()
    {
        // A builder `reassign` (requested -> requested, subject|creator) after a list edit. If the named assessor has
        // left, refusing it would strand the request.
        var options = NewDatabase();
        await SeedAsync(options);
        await SetPermittedToolsAsync(options, ForbiddingItemId);

        var request = await CreateAsync(options, WithdrawableMiniCexTypeId, ForbiddingEpaId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await SetPermittedToolsAsync(options, ForbiddingItemId, "cbd", "dops", "msf");

        (await TransitionAsync(options, request.Id, "reassign", TraineeId)).CurrentState.Should().Be("requested");
    }

    [Fact]
    public async Task ACancelFromRequestedInTheReopenIntoAssessmentShape_IsNotGated_BecauseTheAuthorCannotWriteTheEpaThere()
    {
        // The cancel OUT OF THE DRAFT in that shape stays refused (above). From `requested` the author cannot write the
        // EPA, so refusing the cancel would name nothing they could correct, and would protect nothing.
        var options = NewDatabase();
        await SeedAsync(options);
        await SetPermittedToolsAsync(options, ForbiddingItemId);

        var request = await CreateAsync(options, ReopenIntoAssessmentMiniCexTypeId, ForbiddingEpaId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await SetPermittedToolsAsync(options, ForbiddingItemId, "cbd", "dops", "msf");

        (await TransitionAsync(options, request.Id, "cancel", TraineeId)).CurrentState.Should().Be("cancelled");
    }

    [Fact]
    public async Task ALegacyShapedResubmission_WithTheSameEpa_IsNotReChecked_AsInTheDraftInitialShape()
    {
        // The legacy shape is born in `requested`; a builder-added `resubmit: declined -> requested` returns to the
        // initial state. It is treated exactly like the draft-initial resubmission above: an unchanged target is not
        // re-checked, a changed one is.
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, ResubmittableLegacyMiniCexTypeId, PermittingEpaId);
        await TransitionAsync(options, request.Id, "decline", AssessorId, note: "Not observed by me.");
        await SetPermittedToolsAsync(options, PermittingItemId, "cbd");
        await AssertNowForbiddenAsync(options, ResubmittableLegacyMiniCexTypeId, PermittingEpaId);

        (await TransitionAsync(options, request.Id, "resubmit", TraineeId)).CurrentState.Should().Be("requested");
    }

    // ---- 17: the third review round's shapes ---------------------------------------------------------------------

    [Fact]
    public async Task ALegacyShapedAcceptWithAFallbackApprover_IsNotReChecked_BecauseTheAssessorCannotCorrectTheTarget()
    {
        // Round 3's major finding: `accept: field:assessor_user_id|role:Coordinator` out of the legacy `requested`
        // was re-checked after create and stranded the encounter, while a plain `field:` accept passed. The rule no
        // longer reads the actor rule's syntax: the assessor cannot write the EPA, so they are never refused for it.
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, MixedAcceptLegacyMiniCexTypeId, PermittingEpaId);
        await SetPermittedToolsAsync(options, PermittingItemId, "cbd");
        await AssertNowForbiddenAsync(options, MixedAcceptLegacyMiniCexTypeId, PermittingEpaId);

        (await TransitionAsync(options, request.Id, "accept", AssessorId)).CurrentState.Should().Be("accepted");
        var completed = await TransitionAsync(options, request.Id, "complete", AssessorId, """{ "overall_level": 4 }""");
        completed.CurrentState.Should().Be("completed");
    }

    [Fact]
    public async Task AnAssessorPickingADraftUp_IsNotRefused_BecauseTheyCannotCorrectIt_TheCreateWasTheLastCheck()
    {
        // The recorded boundary. A builder shape lets the named assessor take a draft straight out of `draft`. They
        // cannot write the EPA, so refusing them would strand the request; a list edited after the create therefore
        // applies only if the author moves the draft on first.
        var options = NewDatabase();
        await SeedAsync(options);

        var draft = await CreateAsync(options, AssessorPickupMiniCexTypeId, PermittingEpaId);
        await SetPermittedToolsAsync(options, PermittingItemId, "cbd");
        await AssertNowForbiddenAsync(options, AssessorPickupMiniCexTypeId, PermittingEpaId);

        (await TransitionAsync(options, draft.Id, "pick_up", AssessorId)).CurrentState.Should().Be("requested");
    }

    [Fact]
    public async Task AResubmissionRoutedBackThroughTheDraft_IsNotReChecked_EitherWay_AfterADecline()
    {
        // Round 3: reflective_note's shape offers two ways back after a decline, submit straight from `declined` and
        // revise -> draft -> submit. Both are after the assessor has acted, so neither re-checks an unchanged target.
        var options = NewDatabase();
        await SeedAsync(options);
        await SetPermittedToolsAsync(options, ForbiddingItemId);

        var request = await CreateAsync(options, ResubmittableMiniCexTypeId, ForbiddingEpaId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await TransitionAsync(options, request.Id, "decline", AssessorId, note: "Please add the findings.");
        await SetPermittedToolsAsync(options, ForbiddingItemId, "cbd", "dops", "msf");
        await AssertNowForbiddenAsync(options, ResubmittableMiniCexTypeId, ForbiddingEpaId);

        await TransitionAsync(options, request.Id, "revise", TraineeId);
        (await TransitionAsync(options, request.Id, "submit", TraineeId)).CurrentState.Should().Be("requested");
    }

    // ---- 18: the fourth review round's shapes --------------------------------------------------------------------

    [Fact]
    public async Task AnAssessorAllowedToCorrectTheEpa_IsNotRefusedForAnUnchangedTarget_AtCompletion()
    {
        // Round 4's first major finding: the rule asked what the MOVER could write but never whether the mover was the
        // author, so an assessor granted write access to the EPA field was refused at an unchanged completion. The
        // encounter was filed and submitted legitimately; only the author handing it on is re-checked.
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, AssessorEditableEpaMiniCexTypeId, PermittingEpaId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await SetPermittedToolsAsync(options, PermittingItemId, "cbd");
        await AssertNowForbiddenAsync(options, AssessorEditableEpaMiniCexTypeId, PermittingEpaId);

        var completed = await TransitionAsync(options, request.Id, "complete", AssessorId, """{ "overall_level": 4 }""");

        completed.CurrentState.Should().Be("completed");
        completed.Transitions.Last().CreditedItemCount.Should().Be(1);
    }

    [Fact]
    public async Task ATwoDirectiveRule_JudgesOnlyTheDirectiveWhoseTargetChanged_AndNamesOnlyItsField()
    {
        // Round 4's second major finding. A rule crediting the trainee's `epa_id` and the assessor's
        // `additional_epa_id`: after the list forbids the tool on the trainee's EPA, the assessor's completion must
        // be judged only on the target it actually sets, never on the trainee's unchanged field it cannot write.
        var options = NewDatabase();
        await SeedAsync(options);

        var request = await CreateAsync(options, TwoEpaMiniCexTypeId, PermittingEpaId);
        await TransitionAsync(options, request.Id, "submit", TraineeId);
        await SetPermittedToolsAsync(options, PermittingItemId, "cbd");

        // Setting the second EPA to a forbidden one is refused, and the message names only that field.
        var message = await ShouldBeRefusedAsync(options, service => service.TransitionAsync(new TransitionActivityInput(
            request.Id, "complete", AssessorId, Principal(AssessorId),
            $$"""{ "overall_level": 4, "additional_epa_id": {{ForbiddingEpaId}} }""", null)));
        message.Should().StartWith("Additional EPA observed:").And.Contain("PAED-005").And.NotContain("PAED-006");
        System.Text.RegularExpressions.Regex.Matches(message, GateRefusal).Count.Should().Be(1);

        // Setting it to one with no list passes, and the trainee's unchanged, now-forbidden EPA is not re-judged.
        var completed = await TransitionAsync(options, request.Id, "complete", AssessorId,
            $$"""{ "overall_level": 4, "additional_epa_id": {{UnlistedEpaId}} }""");
        completed.CurrentState.Should().Be("completed");
    }

    [Fact]
    public async Task AHandoverWhoseCreditPathLoopsBackThroughTheDraft_IsStillChecked_BecauseTheAuthorLosesWriteAccess()
    {
        // Round 4's third finding: draft -handover-> with_assessor -rate-> draft -finalize-> completed. Credit can
        // follow the handover only back through `draft`, so the reachability test alone called it a withdrawal, and by
        // the finalize the assessor had acted. The author loses write access to the EPA at the handover, so it is one.
        var options = NewDatabase();
        await SeedAsync(options);
        await SetPermittedToolsAsync(options, ForbiddingItemId);

        var draft = await CreateAsync(options, LoopBackMiniCexTypeId, ForbiddingEpaId);
        await SetPermittedToolsAsync(options, ForbiddingItemId, "cbd", "dops", "msf");

        var message = await ShouldBeRefusedAsync(options, service => service.TransitionAsync(new TransitionActivityInput(
            draft.Id, "handover", TraineeId, Principal(TraineeId), null, null)));
        message.Should().Contain(GateRefusal);
    }

    [Fact]
    public async Task ACancelOutOfTheDraftIntoAHoldingStateTheAuthorCannotEdit_IsJudged_TheRecordedResidual()
    {
        // Round 5's one finding, kept deliberately and pinned so a change to it is a decision. The author loses write
        // access to the EPA when the draft moves into a locked `cancelled`, and losing it is how a hand-on is
        // recognised (the loop-back shape above). Here only the author can reopen, so the refusal protects nothing,
        // but it lands on the author, who can act on it by re-pointing the EPA; the principle holds, and telling
        // "only the author can leave" apart from "someone else can" would mean reading actor-rule syntax again, which
        // is what four earlier rounds showed breaks.
        var options = NewDatabase();
        await SeedAsync(options);
        await SetPermittedToolsAsync(options, ForbiddingItemId);

        var draft = await CreateAsync(options, LockedCancelMiniCexTypeId, ForbiddingEpaId);
        await SetPermittedToolsAsync(options, ForbiddingItemId, "cbd", "dops", "msf");

        var message = await ShouldBeRefusedAsync(options, service => service.TransitionAsync(new TransitionActivityInput(
            draft.Id, "cancel", TraineeId, Principal(TraineeId), null, null)));
        message.Should().Contain(GateRefusal);
    }

    // ---- 15: the refusal stays short enough for the audit row -----------------------------------------------------

    [Fact]
    public async Task ARefusalOfManyItems_NamesTheFirstThree_AndSummarisesTheRest()
    {
        // The message is also the audit row's error text (varchar 2000). A rule set that credits many items can
        // refuse them all at once; naming every one could overflow the column and make the audit save fail too.
        var options = NewDatabase();
        await SeedAsync(options);

        var message = await ShouldBeRefusedAsync(options, service => service.CreateDraftAsync(
            new CreateActivityInput(ManyItemsMiniCexTypeId, TraineeId, TraineeId, RequestData(PermittingEpaId), Principal(TraineeId))));

        System.Text.RegularExpressions.Regex.Matches(message, GateRefusal).Count.Should().Be(3);
        message.Should().Contain("Mini-CEX cannot be used for 2 more EPAs this activity would credit either.");
        message.Length.Should().BeLessThan(2000);
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

    /// <summary>
    /// The guard for every test that proves something is NOT re-checked: a fresh filing of the same tool against the
    /// same EPA must be refused now. Without it, a list edit that did not take would let those tests pass for the
    /// wrong reason.
    /// </summary>
    private static async Task AssertNowForbiddenAsync(
        DbContextOptions<ApplicationDbContext> options,
        int activityTypeId,
        int epaId,
        string subject = TraineeId)
    {
        var message = await ShouldBeRefusedAsync(options, service => service.CreateDraftAsync(
            new CreateActivityInput(activityTypeId, subject, subject, RequestData(epaId), Principal(subject))));

        message.Should().Contain(GateRefusal, "guard: the edited list must really forbid this tool now");
    }

    /// <summary>A create and a submit that both succeed, leaving the request with the assessor.</summary>
    private static async Task AssertAcceptedAsync(
        DbContextOptions<ApplicationDbContext> options,
        int activityTypeId,
        int epaId,
        string subject = TraineeId)
    {
        var created = await CreateAsync(options, activityTypeId, epaId, subject: subject);
        var submitted = await TransitionAsync(options, created.Id, "submit", subject);

        submitted.CurrentState.Should().Be("requested");
        ReadInt(submitted.DataJson, "epa_id").Should().Be(epaId);
    }

    private static async Task<ActivityDto> CreateAsync(
        DbContextOptions<ApplicationDbContext> options,
        int activityTypeId,
        int epaId,
        string subject = TraineeId,
        string? creator = null)
    {
        var createdBy = creator ?? subject;

        await using var db = new ApplicationDbContext(options);
        return await Service(db).CreateDraftAsync(
            new CreateActivityInput(activityTypeId, subject, createdBy, RequestData(epaId), Principal(createdBy)));
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

    /// <summary>What an administrator's edit of one item's allow-list does, in its own request. No keys means null.</summary>
    private static async Task SetPermittedToolsAsync(
        DbContextOptions<ApplicationDbContext> options,
        int curriculumItemId,
        params string[] toolKeys)
    {
        await using var db = new ApplicationDbContext(options);
        var item = await db.CurriculumItems.SingleAsync(entity => entity.Id == curriculumItemId);
        item.PermittedToolsJson = CurriculumItem.NormalizePermittedToolsJson(toolKeys);
        await db.SaveChangesAsync();
    }

    private static async Task<Activity> StoredAsync(DbContextOptions<ApplicationDbContext> options, int activityId)
    {
        await using var db = new ApplicationDbContext(options);
        return await db.Activities
            .AsNoTracking()
            .Include(entity => entity.Transitions)
            .SingleAsync(entity => entity.Id == activityId);
    }

    private static async Task<List<CurriculumItemProgress>> ProgressRowsAsync(DbContextOptions<ApplicationDbContext> options)
    {
        await using var db = new ApplicationDbContext(options);
        return await db.CurriculumItemProgresses.AsNoTracking().ToListAsync();
    }

    private static ActivityService Service(ApplicationDbContext db)
        => new(db, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator());

    private static string RequestData(int epaId, string presentingProblem = "Fever for three days")
        => $$"""
            {
              "epa_id": {{epaId}},
              "assessor_user_id": "{{AssessorId}}",
              "observed_on": "2026-03-10",
              "presenting_problem": "{{presentingProblem}}"
            }
            """;

    private static int? ReadInt(string dataJson, string key)
    {
        using var document = JsonDocument.Parse(dataJson);
        if (!document.RootElement.TryGetProperty(key, out var value))
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.Number ? value.GetInt32() : int.Parse(value.GetString()!);
    }

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
            [new Claim(ClaimTypes.NameIdentifier, TraineeId), new Claim(ClaimTypes.Role, "Trainee")],
            "test",
            ClaimTypes.Name,
            ClaimTypes.Role));

    // ---- fixture ----------------------------------------------------------------------------------------------

    /// <summary>
    /// A CPSA-shaped request form. The EPA field's label differs from its key on purpose, so a message led by the
    /// key can never pass for one led by the label.
    /// </summary>
    private const string SchemaJson = """
        {
          "version": 1,
          "observation_date_field": "observed_on",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "epa_id", "type": "epa", "label": "EPA observed", "required": true },
                { "key": "assessor_user_id", "type": "user", "label": "Assessor", "required": true },
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

    /// <summary>The request form with the EPA field writable by the named assessor too, so they can correct it.</summary>
    private static readonly string AssessorEditableEpaSchemaJson = SchemaJson.Replace(
        """{ "key": "epa_id", "type": "epa", "label": "EPA observed", "required": true }""",
        """{ "key": "epa_id", "type": "epa", "label": "EPA observed", "required": true, "editable_by": "subject|creator|field:assessor_user_id" }""",
        StringComparison.Ordinal);

    /// <summary>The request form plus a second EPA the assessor sets in the assessment section.</summary>
    private static readonly string TwoEpaSchemaJson = SchemaJson.Replace(
        """{ "key": "overall_level", "type": "number", "label": "Supervision required for this encounter" }""",
        """{ "key": "overall_level", "type": "number", "label": "Supervision required for this encounter" }, { "key": "additional_epa_id", "type": "epa", "label": "Additional EPA observed" }""",
        StringComparison.Ordinal);

    private const string CreditsBothEpaFields = """
        {
          "counts_for": [
            { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1 },
            { "curriculum_item_match": { "epa_field": "additional_epa_id" }, "amount": 1 }
          ]
        }
        """;

    /// <summary>The recallable shape with a `cancelled` state the author cannot edit, though only the author reopens it.</summary>
    private static readonly string LockedCancelWorkflowJson = RecallableWorkflowJson.Replace(
        """{ "key": "cancelled", "label": "Cancelled" }""",
        """{ "key": "cancelled", "label": "Cancelled", "editable_by": "role:Coordinator" }""",
        StringComparison.Ordinal);

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

    /// <summary>The legacy WBA shape: born in `requested`, with no author transition at all.</summary>
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

    /// <summary>procedure_log's shape: an activity that is terminal from the moment it is created.</summary>
    private const string TerminalWorkflowJson = """
        {
          "version": 1,
          "initial_state": "logged",
          "states": [
            { "key": "logged", "label": "Logged", "terminal": true }
          ],
          "transitions": []
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

    /// <summary>The scenario runbooks' shape: submit and recall by `role:Trainee`, not by `subject`.</summary>
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

    /// <summary>The legacy shape with a fallback approver on `accept`.</summary>
    private const string MixedAcceptLegacyWorkflowJson = """
        {
          "version": 1,
          "initial_state": "requested",
          "states": [
            { "key": "requested", "label": "Requested" },
            { "key": "accepted", "label": "Accepted", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true },
            { "key": "cancelled", "label": "Cancelled" }
          ],
          "transitions": [
            { "key": "accept", "from": "requested", "to": "accepted", "actor": "field:assessor_user_id|role:Coordinator" },
            { "key": "cancel", "from": "requested", "to": "cancelled", "actor": "subject" },
            { "key": "complete", "from": "accepted", "to": "completed", "actor": "field:assessor_user_id", "requires_fields": ["overall_level"] }
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

    /// <summary>Five literal curriculum_item_id directives, every one on an item whose list forbids Mini-CEX.</summary>
    private const string CreditsFiveFixedItems = """
        {
          "counts_for": [
            { "curriculum_item_match": { "curriculum_item_id": 4011 }, "amount": 1 },
            { "curriculum_item_match": { "curriculum_item_id": 4012 }, "amount": 1 },
            { "curriculum_item_match": { "curriculum_item_id": 4013 }, "amount": 1 },
            { "curriculum_item_match": { "curriculum_item_id": 4014 }, "amount": 1 },
            { "curriculum_item_match": { "curriculum_item_id": 4015 }, "amount": 1 }
          ]
        }
        """;

    private const string CreditsTheEpaField = """
        {
          "counts_for": [
            { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1, "minimum_level_field": "overall_level" }
          ]
        }
        """;

    private const string CreditsNothing = """{ "counts_for": [] }""";

    private static async Task SeedAsync(DbContextOptions<ApplicationDbContext> options)
    {
        await using var db = new ApplicationDbContext(options);

        db.WbaTools.AddRange(
            new WbaTool { Key = "cbd", Name = "CBD" },
            new WbaTool { Key = "dops", Name = "DOPS" },
            new WbaTool { Key = "msf", Name = "MSF" },
            new WbaTool { Key = "mini_cex", Name = "Mini-CEX" });

        db.Epas.AddRange(
            new Epa { Id = ForbiddingEpaId, SubSpecialityId = 1, Code = "PAED-005", Title = "Providing neonatal care" },
            new Epa { Id = PermittingEpaId, SubSpecialityId = 1, Code = "PAED-006", Title = "Managing a child with fever" },
            new Epa { Id = UnlistedEpaId, SubSpecialityId = 1, Code = "PAED-001", Title = "Taking a paediatric history" },
            new Epa { Id = MalformedListEpaId, SubSpecialityId = 1, Code = "PAED-002", Title = "Examining a child" },
            new Epa { Id = OffCurriculumEpaId, SubSpecialityId = 1, Code = "PAED-009", Title = "Leading a resuscitation" },
            new Epa { Id = 11, SubSpecialityId = 1, Code = "PAED-011", Title = "Fixed item one" },
            new Epa { Id = 12, SubSpecialityId = 1, Code = "PAED-012", Title = "Fixed item two" },
            new Epa { Id = 13, SubSpecialityId = 1, Code = "PAED-013", Title = "Fixed item three" },
            new Epa { Id = 14, SubSpecialityId = 1, Code = "PAED-014", Title = "Fixed item four" },
            new Epa { Id = 15, SubSpecialityId = 1, Code = "PAED-015", Title = "Fixed item five" });

        db.CurriculumItems.AddRange(
            Item(ForbiddingItemId, CurriculumId, ForbiddingEpaId, """["cbd","dops","msf"]"""),
            Item(PermittingItemId, CurriculumId, PermittingEpaId, """["cbd","mini_cex"]"""),
            Item(4001, CurriculumId, UnlistedEpaId, null),
            Item(4002, CurriculumId, MalformedListEpaId, """["cbd", """),
            // On a curriculum the trainee is not on. It would forbid Mini-CEX if it applied.
            Item(4109, OtherCurriculumId, OffCurriculumEpaId, """["cbd"]"""),
            Item(4011, CurriculumId, 11, """["cbd"]"""),
            Item(4012, CurriculumId, 12, """["cbd"]"""),
            Item(4013, CurriculumId, 13, """["cbd"]"""),
            Item(4014, CurriculumId, 14, """["cbd"]"""),
            Item(4015, CurriculumId, 15, """["cbd"]"""));

        db.TraineeProfiles.Add(new TraineeProfile
        {
            Id = 1,
            UserId = TraineeId,
            InstitutionId = InstitutionId,
            CurriculumId = CurriculumId,
            ProgrammeStartDate = new DateOnly(2025, 4, 14),
            ExpectedCompletionDate = new DateOnly(2029, 4, 13),
            IsActive = true
        });

        db.ActivityTypes.AddRange(
            Type(MiniCexTypeId, "mini_cex_under_test", "mini_cex", CpsaWorkflowJson, CreditsTheEpaField),
            Type(CbdTypeId, "cbd_under_test", "cbd", CpsaWorkflowJson, CreditsTheEpaField),
            Type(UnkeyedTypeId, "unkeyed_under_test", null, CpsaWorkflowJson, CreditsTheEpaField),
            Type(LegacyMiniCexTypeId, "legacy_mini_cex_under_test", "mini_cex", LegacyWorkflowJson, CreditsTheEpaField),
            Type(TerminalMiniCexTypeId, "logged_mini_cex_under_test", "mini_cex", TerminalWorkflowJson, CreditsTheEpaField),
            Type(NonCreditingMiniCexTypeId, "non_crediting_mini_cex_under_test", "mini_cex", CpsaWorkflowJson, CreditsNothing),
            Type(ResubmittableMiniCexTypeId, "resubmittable_mini_cex_under_test", "mini_cex", ResubmittableWorkflowJson, CreditsTheEpaField),
            Type(RecallableMiniCexTypeId, "recallable_mini_cex_under_test", "mini_cex", RecallableWorkflowJson, CreditsTheEpaField),
            Type(ReopenIntoAssessmentMiniCexTypeId, "reopenable_mini_cex_under_test", "mini_cex", ReopenIntoAssessmentWorkflowJson, CreditsTheEpaField),
            Type(ManyItemsMiniCexTypeId, "many_items_mini_cex_under_test", "mini_cex", CpsaWorkflowJson, CreditsFiveFixedItems),
            Type(RoleSubmitMiniCexTypeId, "role_submit_mini_cex_under_test", "mini_cex", RoleSubmitWorkflowJson, CreditsTheEpaField),
            Type(SignOffMiniCexTypeId, "sign_off_mini_cex_under_test", "mini_cex", SignOffWorkflowJson, CreditsTheEpaField),
            Type(WithdrawableMiniCexTypeId, "withdrawable_mini_cex_under_test", "mini_cex", WithdrawableWorkflowJson, CreditsTheEpaField),
            Type(ResubmittableLegacyMiniCexTypeId, "resubmittable_legacy_mini_cex_under_test", "mini_cex", ResubmittableLegacyWorkflowJson, CreditsTheEpaField),
            Type(MixedAcceptLegacyMiniCexTypeId, "mixed_accept_legacy_mini_cex_under_test", "mini_cex", MixedAcceptLegacyWorkflowJson, CreditsTheEpaField),
            Type(AssessorPickupMiniCexTypeId, "assessor_pickup_mini_cex_under_test", "mini_cex", AssessorPickupWorkflowJson, CreditsTheEpaField),
            Type(AssessorEditableEpaMiniCexTypeId, "assessor_editable_mini_cex_under_test", "mini_cex", CpsaWorkflowJson, CreditsTheEpaField, AssessorEditableEpaSchemaJson),
            Type(TwoEpaMiniCexTypeId, "two_epa_mini_cex_under_test", "mini_cex", CpsaWorkflowJson, CreditsBothEpaFields, TwoEpaSchemaJson),
            Type(LoopBackMiniCexTypeId, "loop_back_mini_cex_under_test", "mini_cex", LoopBackWorkflowJson, CreditsTheEpaField),
            Type(LockedCancelMiniCexTypeId, "locked_cancel_mini_cex_under_test", "mini_cex", LockedCancelWorkflowJson, CreditsTheEpaField));

        await db.SaveChangesAsync();
    }

    private static CurriculumItem Item(int id, int curriculumId, int epaId, string? permittedToolsJson)
        => new()
        {
            Id = id,
            CurriculumId = curriculumId,
            EpaId = epaId,
            RequiredCount = 3,
            QuotaPeriod = QuotaPeriod.Semester,
            MinimumLevelOrder = 3,
            WindowMonths = 12,
            PermittedToolsJson = permittedToolsJson
        };

    private static ActivityType Type(int id, string key, string? wbaToolKey, string workflowJson, string creditRulesJson, string schemaJson = SchemaJson)
    {
        var publishedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        const string displayFieldsJson = """["epa_id","assessor_user_id"]""";

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
            CreditRulesJson = creditRulesJson,
            DisplayFieldsJson = displayFieldsJson,
            WbaToolKey = wbaToolKey,
            OwnerUserId = "admin-1",
            CreatedOn = publishedOn
        };

        activityType.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = id,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditRulesJson,
            DisplayFieldsJson = displayFieldsJson,
            PublishedByUserId = "admin-1",
            PublishedOn = publishedOn
        });

        return activityType;
    }
}
