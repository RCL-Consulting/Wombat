using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Components.Pages.CommitteeDecisions.Review;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.CommitteeDecisions.ReviewPage;

/// <summary>
/// T366 (flow 09, boards R3-3; round 3 F5, F6, F7, M2, M8, the nits; T323, T213, T237): the committee's Appeals section
/// and the reader's jump to it. The section draws lane T's <c>AppealEntry</c> for each appeal, then the resolve form (one
/// column, its Outcome two radios read whole at every width) or the who-may line. The form's quorum line sits beside
/// Resolve appeal, and its refusal after the form's actions, led "Resolve appeal was refused:". Remit and Dismiss each
/// confirm in a <c>ConfirmDialog</c> as built, whose Go back leaves the focus to the native dialog's return to Resolve
/// appeal (F6).
/// </summary>
public sealed class ReviewPageAppealTests : ReviewPageTestBase
{
    private const string Reason =
        "The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before " +
        "it was rated. I ask for reconsideration.";

    private const string Rationale =
        "The stalled Mini-CEX was the assessor's delay; progress is adequate, with observed assessments to follow.";

    private const string QuorumRule = "A committee decision needs at least two panel members present: the chair and at least one other.";

    // ─── The jump (F5) ──────────────────────────────────────────────────────────

    [Fact]
    public void UnderAppeal_EveryReadersBar_CarriesGoToTheAppeal_WhichMovesTheFocusToTheAppealsHeading()
    {
        // David Naidoo (4.44): a reader's bar, an outline small button at its right end.
        SignInAs(Naidoo);
        var cut = Render(UnderAppeal(resolves: false));

        cut.Find("#sitting-bar").ClassList.Should().Contain("sitting-bar--reader");
        var jump = cut.Find("#sitting-bar .sitting-bar-actions #sitting-go-to-appeal");
        Text(jump).Should().Be("Go to the appeal");
        jump.GetAttribute("type").Should().Be("button", "a button, never an #appeals link that <base href> sends Home");
        jump.ClassList.Should().Contain(["btn", "btn-sm", "btn-outline"]);
        cut.Find("#appeals-heading").GetAttribute("tabindex").Should().Be("-1");

        jump.Click();

        FocusedIds().Should().Equal("appeals-heading");
    }

    [Fact]
    public void UnderAppeal_TheAppealBody_IsOfferedTheJumpToo()
    {
        var cut = Render(UnderAppeal(resolves: true));

        cut.FindAll("#sitting-go-to-appeal").Should().ContainSingle();
    }

    [Theory]
    [InlineData(CommitteeReviewState.Ratified)]
    [InlineData(CommitteeReviewState.Final)]
    [InlineData(CommitteeReviewState.Decided)]
    public void OutsideAnAppeal_ThereIsNoJump(CommitteeReviewState state)
    {
        // Board 3 c "No appeal lodged" and the resolved frames: no jump once nothing is under appeal.
        SignInAs(Botha);
        var cut = Render(Review(state, callerChairs: false));

        cut.FindAll("#sitting-go-to-appeal").Should().BeEmpty();
    }

    // ─── The section: the entry, then the who-may line (4.44; F7, M2; T213, T237) ─

    [Fact]
    public void AMembersView_IsTheEntry_ThenTheWhoMayLine_AndNoForm()
    {
        SignInAs(Naidoo);
        var cut = Render(UnderAppeal(resolves: false));

        var section = cut.Find("#appeals");
        var entry = section.QuerySelector("ul.appeal-list > li.appeal-entry")!;
        Text(entry.QuerySelector("h3")!).Should().Be("Appeal by Nomsa Mahlangu Open");
        Text(entry).Should().Contain("Lodged 2026-10-13")
            .And.Contain("Heard by Thandi Zulu (chair) and John van Rensburg (external).")
            .And.Contain(Reason);

        var line = cut.Find("#appeal-body-note");
        line.ClassList.Should().Contain("appeals-who");
        Text(line).Should().Be(
            "Only the appeal body can resolve the appeal: the panel's chair, Thandi Zulu, and its external member, John van Rensburg.");
        line.PreviousElementSibling!.ClassList.Should().Contain("appeal-list", "the line follows the entry (board 3 order)");
        cut.FindAll("#appeals form").Should().BeEmpty();
        Buttons(cut).Should().NotContain("Resolve appeal");
    }

    [Fact]
    public void ACoordinatorHoldingTheExternalSeat_WhoCannotActNow_ReadsTheReadersView_AndTheLineLeavesThemOut()
    {
        // C13 as F7 and M2 redraw it: a Coordinator at Kgosi Kgari Teaching Hospital who also holds the panel's External
        // seat and cannot act now. The query neither offers them the form nor names them on the appeal body.
        SignInAs(VanRensburg, WombatRoles.Coordinator);
        var review = UnderAppeal(resolves: false) with
        {
            AppealBody = [new CommitteePersonDto(Chair, DecisionPanelMemberRole.Chair) { Name = "Thandi Zulu" }],
            PanelMembers =
            [
                Person(Chair, DecisionPanelMemberRole.Chair, "Thandi Zulu"),
                Person(Naidoo, DecisionPanelMemberRole.Member, "David Naidoo"),
                Person(Botha, DecisionPanelMemberRole.Member, "Sarah Botha"),
                Person(VanRensburg, DecisionPanelMemberRole.External, "John van Rensburg", maySit: false)
            ]
        };
        var cut = Render(review);

        cut.FindAll("#appeals form").Should().BeEmpty();
        Text(cut.Find("#appeal-body-note")).Should().Be("Only the appeal body can resolve the appeal: the panel's chair, Thandi Zulu.");
        Text(cut.Find("li.appeal-entry")).Should().Contain("Heard by Thandi Zulu (chair).");
        cut.FindAll("#sitting-go-to-appeal").Should().ContainSingle();
    }

    [Fact]
    public void WhenNobodyOnTheAppealBodyCanAct_TheLineSaysSo_AndTheEntryNamesNobody()
    {
        // Board 3 c (C12a).
        SignInAs(Botha);
        var cut = Render(UnderAppeal(resolves: false) with { AppealBody = [] });

        Text(cut.Find("#appeal-body-note")).Should().Be(
            "Only the appeal body, the panel's chair or an external member, can resolve the appeal, and none of them can " +
            "act now. A panel administrator must seat a chair who can.");
        Text(cut.Find("li.appeal-entry")).Should().NotContain("Heard by");
    }

    [Fact]
    public void NoAppealLodged_SaysSo()
    {
        SignInAs(Botha);
        var cut = Render(Review(CommitteeReviewState.Ratified, callerChairs: false));

        Text(cut.Find("#appeals")).Should().Be("Appeals No appeal has been lodged against this review.");
        cut.FindAll("#appeals ul.appeal-list").Should().BeEmpty();
    }

    [Fact]
    public void AResolvedAppeal_IsItsEntry_RemittedWithWhoResolvedIt()
    {
        var review = Review(CommitteeReviewState.Final, callerChairs: false) with
        {
            Appeals = [Appeal() with { ResolvedOn = Lodged.AddHours(6), ResolvedByUserId = Chair, Outcome = CommitteeAppealOutcome.Remitted, ResolvedByName = "Thandi Zulu" }]
        };
        var cut = Render(review);

        var entry = cut.Find("li.appeal-entry");
        Text(entry.QuerySelector("h3")!).Should().Be("Appeal by Nomsa Mahlangu Remitted");
        Text(entry).Should().Contain("Lodged 2026-10-13 · Resolved 2026-10-13 by Thandi Zulu")
            .And.Contain("Remitted: the appeal body replaced the decision. The replacement decision is above.")
            .And.NotContain("Heard by");
        cut.FindAll("#appeal-body-note").Should().BeEmpty();
    }

    // ─── The form: one column, the Outcome whole (T323; 4.45) ─────────────────────

    [Fact]
    public void TheResolveForm_IsOneColumn_ItsOutcomeTwoRadios_NoneChosen_EachSaidWhole()
    {
        SignInAs(VanRensburg);
        var cut = Render(UnderAppeal(resolves: true, callerChairs: false));

        var form = cut.Find("#appeals form.resolve-form");
        form.GetAttribute("aria-label").Should().Be("Resolve the appeal");
        form.PreviousElementSibling!.ClassList.Should().Contain("appeal-list", "the form follows the entry (board 3 order)");

        var outcome = cut.Find("fieldset#appeal-outcome");
        Text(outcome.QuerySelector("legend")!).Should().Be("Outcome * required");
        outcome.GetAttribute("aria-describedby").Should().Be("appeal-outcome-help appeal-outcome-message");
        var radios = outcome.QuerySelectorAll("input[type=radio]").ToArray();
        radios.Select(radio => radio.Id).Should().Equal("appeal-outcome-dismissed", "appeal-outcome-remitted");
        radios.Should().NotContain(radio => radio.HasAttribute("checked"), "the resolver has chosen nothing yet (T307)");
        outcome.QuerySelectorAll("label").Select(label => Text(label)).Should().Equal(
            "Dismissed: the decision stands", "Remitted: the appeal body replaces the decision");
        Text(cut.Find("#appeal-outcome-help")).Should().Be(CommitteeDecisionWording.AppealOutcomeHelp);
        Text(cut.Find("#appeal-quorum-line")).Should().BeEmpty("no quorum is asked until Remitted is chosen");
    }

    [Fact]
    public void ResolvingWithNoOutcome_SaysChooseAnOutcome_MarksTheGroup_AndSendsNothing()
    {
        SignInAs(VanRensburg);
        var cut = Render(UnderAppeal(resolves: true, callerChairs: false));

        Button(cut, "Resolve appeal").Click();

        cut.WaitForAssertion(() => Text(cut.Find("#appeal-outcome-message")).Should().Be("Choose an outcome."));
        cut.Find("#appeal-outcome .choice-group").ClassList.Should().Contain("is-invalid");
        Sender.Sent<ResolveAppealCommand>().Should().BeEmpty();
        cut.WaitForAssertion(() => ElementFocusCalls().Should().Be(1, "the first radio takes the focus"));

        Choose(cut, CommitteeAppealOutcome.Dismissed);
        cut.Find("#appeal-outcome .choice-group").ClassList.Should().NotContain("is-invalid");
    }

    // ─── The quorum line beside Resolve appeal (M8; 4.45, 4.46) ──────────────────

    [Fact]
    public void TheExternalResolving_ReadsTheQuorumMet_TheChairAndHimselfTickedAndLocked()
    {
        SignInAs(VanRensburg);
        var cut = Render(UnderAppeal(resolves: true, callerChairs: false));

        Choose(cut, CommitteeAppealOutcome.Remitted);

        Text(cut.Find("#appeal-category option")).Should().Be("Select a category…");
        var line = cut.Find("#appeal-quorum-line");
        line.GetAttribute("aria-live").Should().Be("polite");
        line.ClassList.Should().Contain("quorum-line");
        line.Closest(".decision-actions")!.QuerySelector("button")!.TextContent.Trim().Should().Be("Resolve appeal");
        Text(line).Should().Be("2 present: the chair and 1 other. The quorum is met.");
        cut.FindAll("#remit-present-0, #remit-present-3").Should().OnlyContain(box => box.HasAttribute("disabled"));
    }

    [Fact]
    public void TheChairResolvingAlone_ReadsTheQuorumShortBeforePressing_AndTickingAMemberMeetsIt()
    {
        var cut = Render(UnderAppeal(resolves: true));

        Choose(cut, CommitteeAppealOutcome.Remitted);
        Text(cut.Find("#appeal-quorum-line")).Should().Be($"Only the chair is ticked. {QuorumRule}");

        cut.Find("#remit-present-2").Change(true);

        Text(cut.Find("#appeal-quorum-line")).Should().Be("2 present: the chair and 1 other. The quorum is met.");
    }

    [Fact]
    public void AQuorumShort_IsRefusedAfterTheFormsActions_LedResolveAppealWasRefused_WithTheFocus_EverythingTypedKept()
    {
        // 4.46: Dr Zulu ticks only herself. Nothing is sent: the replacement would not hold a quorum.
        var cut = Render(UnderAppeal(resolves: true));
        Choose(cut, CommitteeAppealOutcome.Remitted);
        cut.Find("#appeal-category").Change(CommitteeDecisionCategory.SatisfactoryWithObservations.ToString());
        cut.Find("#appeal-rationale").Change(Rationale);

        Button(cut, "Resolve appeal").Click();

        var result = cut.Find("#appeals .action-result");
        Text(result).Should().Be($"Resolve appeal was refused: {QuorumRule}");
        result.PreviousElementSibling!.ClassList.Should().Contain("decision-actions", "the refusal follows the form's actions");
        result.Closest("form").Should().NotBeNull();
        cut.WaitForAssertion(() => ElementFocusCalls().Should().Be(1));
        Sender.Sent<ResolveAppealCommand>().Should().BeEmpty();
        cut.Find("#appeal-rationale").GetAttribute("value").Should().Be(Rationale);
        JSInterop.Invocations.Should().NotContain(invocation => invocation.Identifier == "wombatDialog.showModal");
    }

    [Fact]
    public void ARemitWithNoCategoryOrRationale_IsRefusedInOnePlace_NamingBoth()
    {
        SignInAs(VanRensburg);
        var cut = Render(UnderAppeal(resolves: true, callerChairs: false));
        Choose(cut, CommitteeAppealOutcome.Remitted);

        Button(cut, "Resolve appeal").Click();

        Text(cut.Find("#appeals .action-result")).Should().Be(
            "Resolve appeal was refused: Choose a progression category. Give the replacement rationale.");
        Sender.Sent<ResolveAppealCommand>().Should().BeEmpty();
    }

    [Fact]
    public void APanelThatCannotSeatAQuorum_KeepsResolveAppealReachable_ItsReasonInPlaceOfTheQuorumLine()
    {
        // Board 3 c (C12b).
        var cut = Render(UnderAppeal(resolves: true) with
        {
            PanelMembers = [Person(Chair, DecisionPanelMemberRole.Chair, "Thandi Zulu")]
        });
        Choose(cut, CommitteeAppealOutcome.Remitted);

        var resolve = Button(cut, "Resolve appeal");
        resolve.HasAttribute("disabled").Should().BeFalse();
        resolve.GetAttribute("aria-disabled").Should().Be("true");
        resolve.ClassList.Should().Contain("is-unavailable");
        resolve.GetAttribute("aria-describedby").Should().Be("remit-reason");
        Text(cut.Find("#remit-reason")).Should().StartWith("Resolve appeal: Only the chair can be recorded as present");
        Text(cut.Find("#appeal-quorum-line")).Should().BeEmpty();

        resolve.Click();

        FocusedIds().Should().Equal("remit-reason");
        Sender.Sent<ResolveAppealCommand>().Should().BeEmpty();
    }

    // ─── The dialogs (F6; 4.47, C14b, C14c) ─────────────────────────────────────

    [Fact]
    public void Remitting_ConfirmsInADialog_GoBackSendsNothing_AndKeepsWhatWasTyped()
    {
        var cut = Render(UnderAppeal(resolves: true));
        FillRemit(cut);

        Button(cut, "Resolve appeal").Click();

        JSInterop.Invocations.Should().Contain(invocation => invocation.Identifier == "wombatDialog.showModal");
        var dialog = Dialog(cut, "Remit Nomsa Mahlangu's appeal?");
        dialog.QuerySelectorAll(".dialog-body p").Select(paragraph => Text(paragraph)).Should().Equal(
            "Satisfactory with Observations replaces Inadequate Progress — Additional Training.",
            "Present: Thandi Zulu (chair), Sarah Botha",
            "This closes the 2026 S2 review. Nothing reopens it, and no STAR changes.");
        dialog.QuerySelectorAll("button").Select(button => Text(button)).Should().Equal("Go back", "Remit the appeal");

        DialogButton(cut, "Remit Nomsa Mahlangu's appeal?", "Go back").Click();

        Sender.Sent<ResolveAppealCommand>().Should().BeEmpty();
        JSInterop.Invocations.Should().Contain(invocation => invocation.Identifier == "wombatDialog.close");
        FocusedIds().Should().BeEmpty("the native dialog returns the focus to Resolve appeal, its opener (F6)");
        ElementFocusCalls().Should().Be(0);
        cut.Find("#appeal-rationale").GetAttribute("value").Should().Be(Rationale);
    }

    [Fact]
    public void Remitted_SendsTheReplacementAndWhoSat_ThenAppealResolvedHeadsTheSection()
    {
        var cut = Render(UnderAppeal(resolves: true));
        FillRemit(cut);
        Sender.Now<ResolveAppealCommand>(_ => Remitted());
        Sender.Now<GetCommitteeReviewByIdQuery>(_ => Remitted());

        Button(cut, "Resolve appeal").Click();
        DialogButton(cut, "Remit Nomsa Mahlangu's appeal?", "Remit the appeal").Click();

        var sent = Sender.Sent<ResolveAppealCommand>().Single();
        sent.Outcome.Should().Be(CommitteeAppealOutcome.Remitted);
        sent.RemittedCategory.Should().Be(CommitteeDecisionCategory.SatisfactoryWithObservations);
        sent.RemittedRationale.Should().Be(Rationale);
        sent.RemittedConditions.Should().Be("Two observed Mini-CEX and one DOPS before the next review.");
        sent.PresentUserIds.Should().Equal(Chair, Botha);

        var result = cut.Find("#appeals .action-result");
        Text(result).Should().Be("Appeal resolved.");
        result.PreviousElementSibling!.Id.Should().Be("appeals-heading", "the result heads the Appeals section");
        cut.FindAll("#appeals form").Should().BeEmpty();
        cut.FindAll("#sitting-go-to-appeal").Should().BeEmpty("the review is no longer under appeal");
    }

    [Fact]
    public void Dismissing_ConfirmsInItsOwnDialog_AndSendsNoReplacement()
    {
        var cut = Render(UnderAppeal(resolves: true));
        Choose(cut, CommitteeAppealOutcome.Dismissed);

        Button(cut, "Resolve appeal").Click();

        var dialog = Dialog(cut, "Dismiss Nomsa Mahlangu's appeal?");
        dialog.QuerySelectorAll(".dialog-body p").Select(paragraph => Text(paragraph)).Should().Equal(
            "Inadequate Progress — Additional Training stays in force.",
            "This closes the 2026 S2 review. Nothing reopens it.");
        dialog.QuerySelectorAll("button").Select(button => Text(button)).Should().Equal("Go back", "Dismiss the appeal");

        Sender.Now<ResolveAppealCommand>(_ => UnderAppeal(resolves: true) with { State = CommitteeReviewState.Final });
        DialogButton(cut, "Dismiss Nomsa Mahlangu's appeal?", "Dismiss the appeal").Click();

        var sent = Sender.Sent<ResolveAppealCommand>().Single();
        sent.Outcome.Should().Be(CommitteeAppealOutcome.Dismissed);
        sent.PresentUserIds.Should().BeNull();
        sent.RemittedRationale.Should().BeNull();
    }

    [Fact]
    public void ARefusalFromTheServer_SitsAfterTheFormsActions_WhileTheFormStands()
    {
        var cut = Render(UnderAppeal(resolves: true));
        Choose(cut, CommitteeAppealOutcome.Dismissed);
        // The handler raises the review's own rules as the committee's refusal (T366, R1).
        Sender.Now<ResolveAppealCommand>(_ => throw new CommitteeRefusedException("There is no open appeal to resolve."));

        Button(cut, "Resolve appeal").Click();
        DialogButton(cut, "Dismiss Nomsa Mahlangu's appeal?", "Dismiss the appeal").Click();

        var result = cut.Find("#appeals .action-result");
        Text(result).Should().Be("Resolve appeal was refused: There is no open appeal to resolve.");
        result.PreviousElementSibling!.ClassList.Should().Contain("decision-actions");
    }

    [Fact]
    public void ARefusalFromTheServer_HeadsTheSection_WhenTheReviewReadAgainNoLongerOffersTheForm()
    {
        // Resolved meanwhile from another tab: the page reads the review again (T260), the form goes, the refusal stays.
        var cut = Render(UnderAppeal(resolves: true));
        Choose(cut, CommitteeAppealOutcome.Dismissed);
        Sender.Now<ResolveAppealCommand>(_ => throw new CommitteeRefusedException("Only reviews under appeal can resolve an appeal."));
        Sender.Now<GetCommitteeReviewByIdQuery>(_ => Remitted());

        Button(cut, "Resolve appeal").Click();
        DialogButton(cut, "Dismiss Nomsa Mahlangu's appeal?", "Dismiss the appeal").Click();

        var result = cut.Find("#appeals .action-result");
        Text(result).Should().Be("Resolve appeal was refused: Only reviews under appeal can resolve an appeal.");
        result.PreviousElementSibling!.Id.Should().Be("appeals-heading");
        cut.WaitForAssertion(() => ElementFocusCalls().Should().Be(1));
    }

    [Fact]
    public void AResolverWhoMayNotSitNow_PassesThePagesQuorumCheck_TheHandlerRefuses_AndNothingTypedIsLost()
    {
        // T366, R2: the page counts only members who may sit (MaySit), and the handler's seat gate refuses a resolver who
        // may not (PanelSeat, T237). Mr van Rensburg's seat lapsed after the review was read: the page's count leaves him
        // out, so the chair and Sarah Botha meet the quorum and the remit is sent; the handler's refusal shows in its words
        // where a page refusal would, and the remit stays as typed for whoever may resolve it.
        SignInAs(VanRensburg);
        var review = UnderAppeal(resolves: true, callerChairs: false);
        var cut = Render(review with
        {
            PanelMembers = review.PanelMembers
                .Select(member => member.UserId == VanRensburg ? member with { MaySit = false } : member)
                .ToArray()
        });
        Choose(cut, CommitteeAppealOutcome.Remitted);
        cut.Find("#appeal-category").Change(CommitteeDecisionCategory.SatisfactoryWithObservations.ToString());
        cut.Find("#appeal-rationale").Change(Rationale);
        cut.Find("#remit-present-2").Change(true);
        Sender.Now<ResolveAppealCommand>(_ => throw new CommitteeRefusedException(PanelSeat.NotEligible));

        Button(cut, "Resolve appeal").Click();
        DialogButton(cut, "Remit Nomsa Mahlangu's appeal?", "Remit the appeal").Click();

        Sender.Sent<ResolveAppealCommand>().Single().PresentUserIds.Should().Equal(Chair, Botha);
        Text(cut.Find("#appeals .action-result")).Should().Be($"Resolve appeal was refused: {PanelSeat.NotEligible}");
        cut.Find("#appeal-rationale").GetAttribute("value").Should().Be(Rationale);
        cut.Find("#remit-present-2").HasAttribute("checked").Should().BeTrue();
    }

    [Fact]
    public void AFailureThatIsNoRefusal_IsAFixedSentence_AndGoesToTheLog()
    {
        // T366, R1 and D3: a database or concurrency failure is not a refusal, so its framework text is never printed, led
        // "Resolve appeal was refused:" or otherwise.
        var logger = new CapturingLogger<ReviewDetail>();
        Services.AddSingleton<ILogger<ReviewDetail>>(logger);
        var cut = Render(UnderAppeal(resolves: true));
        Choose(cut, CommitteeAppealOutcome.Dismissed);
        Sender.Now<ResolveAppealCommand>(_ => throw new InvalidOperationException("A second operation was started on this context instance."));

        Button(cut, "Resolve appeal").Click();
        DialogButton(cut, "Dismiss Nomsa Mahlangu's appeal?", "Dismiss the appeal").Click();

        Text(cut.Find("#appeals .action-result")).Should().Be(ReviewWords.ResolveFailed);
        cut.Markup.Should().NotContain("second operation");
        logger.Entries.Should().ContainSingle(entry => entry.Level == LogLevel.Error && entry.Exception is InvalidOperationException);
    }

    [Fact]
    public void AValidatorsRefusal_StillShowsItsWords()
    {
        var cut = Render(UnderAppeal(resolves: true));
        Choose(cut, CommitteeAppealOutcome.Dismissed);
        Sender.Now<ResolveAppealCommand>(_ => throw new FluentValidation.ValidationException(
            [new FluentValidation.Results.ValidationFailure("PresentUserIds", QuorumRule)]));

        Button(cut, "Resolve appeal").Click();
        DialogButton(cut, "Dismiss Nomsa Mahlangu's appeal?", "Dismiss the appeal").Click();

        Text(cut.Find("#appeals .action-result")).Should().Be($"Resolve appeal was refused: {QuorumRule}");
    }

    [Fact]
    public void AnEntrustmentOnlyReview_SaysTheReplacementDecisionReplacesTheCommittees()
    {
        var cut = Render(UnderAppeal(resolves: true, type: CommitteeReviewType.EntrustmentOnly));
        Choose(cut, CommitteeAppealOutcome.Remitted);
        cut.FindAll("#appeal-category").Should().BeEmpty();
        cut.Find("#remit-entrustment-only-note").Should().NotBeNull();
        cut.Find("#appeal-rationale").Change(Rationale);
        cut.Find("#remit-present-2").Change(true);

        Button(cut, "Resolve appeal").Click();

        Text(Dialog(cut, "Remit Nomsa Mahlangu's appeal?").QuerySelector(".dialog-body p")!)
            .Should().Be("The appeal body's decision replaces the committee's.");
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────────

    private static readonly DateTime Lodged = new(2026, 10, 13, 7, 30, 0, DateTimeKind.Utc);

    private static CommitteeAppealDto Appeal()
        => new(7, Lodged, "mahlangu", Reason, null, null, null) { LodgedByName = "Nomsa Mahlangu" };

    /// <summary>Dr Mahlangu's 2026 S2 review under appeal (4.44–4.47), its decision Inadequate Progress — Additional Training.</summary>
    private static CommitteeReviewDetailDto UnderAppeal(
        bool resolves, bool callerChairs = true, CommitteeReviewType type = CommitteeReviewType.AnnualProgression)
    {
        var decision = Decision(type == CommitteeReviewType.EntrustmentOnly ? null : CommitteeDecisionCategory.InadequateProgressAdditionalTraining);
        return Review(CommitteeReviewState.UnderAppeal, callerChairs, decisions: [decision], type: type) with
        {
            TraineeName = "Nomsa Mahlangu",
            Appeals = [Appeal()],
            CallerResolvesAppeals = resolves,
            AppealBody =
            [
                new CommitteePersonDto(Chair, DecisionPanelMemberRole.Chair) { Name = "Thandi Zulu" },
                new CommitteePersonDto(VanRensburg, DecisionPanelMemberRole.External) { Name = "John van Rensburg" }
            ]
        };
    }

    /// <summary>The review once remitted: Closed, the appeal resolved by the chair.</summary>
    private static CommitteeReviewDetailDto Remitted()
        => UnderAppeal(resolves: true) with
        {
            State = CommitteeReviewState.Final,
            Appeals = [Appeal() with { ResolvedOn = Lodged.AddHours(6), ResolvedByUserId = Chair, Outcome = CommitteeAppealOutcome.Remitted, ResolvedByName = "Thandi Zulu" }],
            AppealBody = []
        };

    private static void Choose(IRenderedFragment cut, CommitteeAppealOutcome outcome)
        => cut.Find($"#appeal-outcome-{outcome.ToString().ToLowerInvariant()}").Change(outcome.ToString());

    /// <summary>4.47: Remitted, Satisfactory with Observations, the rationale and conditions, Sarah Botha ticked.</summary>
    private static void FillRemit(IRenderedFragment cut)
    {
        Choose(cut, CommitteeAppealOutcome.Remitted);
        cut.Find("#appeal-category").Change(CommitteeDecisionCategory.SatisfactoryWithObservations.ToString());
        cut.Find("#appeal-rationale").Change(Rationale);
        cut.Find("#appeal-conditions").Change("Two observed Mini-CEX and one DOPS before the next review.");
        cut.Find("#remit-present-2").Change(true);
    }
}
