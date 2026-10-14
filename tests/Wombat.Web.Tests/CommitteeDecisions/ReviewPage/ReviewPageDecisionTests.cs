using Bunit;
using FluentAssertions;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Web.Tests.CommitteeDecisions.ReviewPage;

/// <summary>
/// T361 (flow 07, round 3): the decision, Record and Ratify, Start and Close. 1280 decide frames 1–11; 1280 more frames
/// 7–11; 1280 start frame 5; the 390 decide frames; round 3 items 7, 11, 16, 21, 22; E1, E4; T324. Every dialog's words,
/// its safe button, and what it sends.
/// </summary>
public sealed class ReviewPageDecisionTests : ReviewPageTestBase
{
    // ─── The form (7, 16, 21) ───────────────────────────────────────────────────

    [Fact]
    public void TheForm_AsksForCategoryRationaleTheSentenceConditionsAndPresent_InThatOrder()
    {
        var cut = Render(Review());

        Text(cut.Find("#decision h2")).Should().Be("Decision");
        cut.FindAll("#decision label").Select(label => Text(label)).Take(4).Should().Equal(
            "Category* required", "Rationale* required", "About the EPAs not decided (optional)", "Conditions");
        Text(cut.Find("#decision-undecided-help")).Should().Be("The registrar sees this once the review is ratified.");
        cut.Find("#decision-undecided").GetAttribute("aria-describedby").Should().Be("decision-undecided-help");
        Text(cut.Find("#decision-present-help")).Should().Be(
            "Tick every panel member at this sitting. A committee decision needs at least two panel members present: the " +
            "chair and at least one other.");
        cut.FindAll("#decision-category option").Select(Text).First().Should().Be("Select a category…");
        cut.Find("#decision-present-0").HasAttribute("disabled").Should().BeTrue("the chair is ticked and locked");
    }

    [Fact]
    public void OnlyTheChairTicked_RecordIsUnavailable_NeverNativelyDisabled_TheQuorumLineIsItsReason_AndThePressMovesTheFocusThere()
    {
        // 1280 decide frame 1 (4.23).
        var cut = Render(Review());

        var record = cut.Find("#record-decision");
        Text(record).Should().Be("Record decision…");
        record.HasAttribute("disabled").Should().BeFalse();
        record.GetAttribute("aria-disabled").Should().Be("true");
        record.ClassList.Should().Contain("is-unavailable");
        record.GetAttribute("aria-describedby").Should().Be("quorum-line");
        var quorum = cut.Find("#quorum-line");
        Text(quorum).Should().Be(
            "Only the chair is ticked. A committee decision needs at least two panel members present: the chair and at least one other.");
        quorum.GetAttribute("aria-live").Should().Be("polite");
        quorum.ClassList.Should().Contain("quorum-line");

        record.Click();

        FocusedIds().Should().Contain("quorum-line");
        Sender.Sent<RecordCommitteeDecisionCommand>().Should().BeEmpty();
        JSInterop.Invocations.Should().NotContain(invocation => invocation.Identifier == "wombatDialog.showModal");
    }

    [Fact]
    public void ThreeTicked_TheQuorumIsMet_AndRecordIsAvailable_WithTheHintBesideIt()
    {
        // 1280 decide frame 5 (4.24, 4.25).
        var cut = Render(Review(), [Pending(1, "5", [12, 13])]);

        TickAll(cut);

        Text(cut.Find("#quorum-line")).Should().Be("4 present: the chair and 3 others. The quorum is met.");
        var record = cut.Find("#record-decision");
        record.HasAttribute("aria-disabled").Should().BeFalse();
        record.ClassList.Should().NotContain("is-unavailable");
        record.GetAttribute("aria-describedby").Should().Be("record-hint");
        Text(cut.Find("#record-hint")).Should().Be(
            "Recording fixes the 1 staged decision, and records the 3 EPAs not staged as not decided.");
    }

    [Fact]
    public void TwoPresent_SaysTheChairAndOneOther()
    {
        var cut = Render(Review());

        cut.Find("#decision-present-1").Change(true);

        Text(cut.Find("#quorum-line")).Should().Be("2 present: the chair and 1 other. The quorum is met.");
    }

    [Fact]
    public void APanelThatCannotSitAQuorum_SaysSoInTheQuorumLine_AndTickingCannotCureIt()
    {
        // 1280 decide frame 2 (7).
        var review = Review() with
        {
            PanelMembers =
            [
                Person(Chair, DecisionPanelMemberRole.Chair, "Thandi Zulu"),
                Person(Botha, DecisionPanelMemberRole.Member, "Sarah Botha", maySit: false)
            ]
        };
        var cut = Render(review);

        Text(cut.Find("#quorum-line")).Should().Be(review.PanelShortfall);
        cut.Find("#record-decision").GetAttribute("aria-disabled").Should().Be("true");
        Text(cut.Find(".sitting-bar-present")).Should().Be("Present: the chair · no other member may sit");
    }

    [Fact]
    public void AnEntrustmentOnlyReview_WithNothingStaged_KeepsRecordReachable_WithE4sRefusalBesideIt()
    {
        // 1280 more frame 11; 390 more frame 9 (E4).
        var cut = Render(Review(type: CommitteeReviewType.EntrustmentOnly));
        cut.Find("#decision-present-1").Change(true);

        cut.FindAll("#decision-category").Should().BeEmpty("an entrustment-only review records no category");
        var record = cut.Find("#record-decision");
        record.GetAttribute("aria-disabled").Should().Be("true");
        record.GetAttribute("aria-describedby").Should().Be("record-reason");
        Text(cut.Find("#record-reason")).Should().Be(
            "This review decides entrustment only, so stage at least one decision before recording it.");
        Text(cut.Find("#quorum-line")).Should().Be("2 present: the chair and 1 other. The quorum is met.");

        record.Click();
        FocusedIds().Should().Contain("record-reason");
        Text(cut.Find("#entrustment-only-note")).Should().Contain("staged above");
    }

    // ─── Refused before any dialog (T324; 4.24) ─────────────────────────────────

    [Fact]
    public void NoCategory_IsRefusedAtTheFormsTop_TheCategoryMarked_EverythingTypedKept()
    {
        var cut = Render(Review());
        TickAll(cut);
        cut.Find("#decision-rationale").Change("On track for completion.");
        cut.Find("#decision-undecided").Change("Typed sentence.");

        cut.Find("#record-decision").Click();

        Text(cut.Find("#decision .action-result")).Should().Be(
            "Not recorded. This review decides the trainee's progression, so its decision records a progression category. Choose one.");
        cut.Find("#decision-category").GetAttribute("aria-invalid").Should().Be("true");
        Text(cut.Find("#decision-category-message")).Should().Be("Choose a progression category.");
        cut.Find("#decision-rationale").GetAttribute("value").Should().Be("On track for completion.");
        cut.Find("#decision-undecided").GetAttribute("value").Should().Be("Typed sentence.");
        cut.Find("#decision-present-1").HasAttribute("checked").Should().BeTrue();
        JSInterop.Invocations.Should().NotContain(invocation => invocation.Identifier == "wombatDialog.showModal");
        ElementFocusCalls().Should().BeGreaterThan(0, "the summary takes the focus");
    }

    [Fact]
    public void NoRationale_IsRefusedWithTheFieldsMessage_NeverTheModelsPropertyName()
    {
        var cut = Render(Review());
        TickAll(cut);
        cut.Find("#decision-category").Change(CommitteeDecisionCategory.SatisfactoryProgress.ToString());

        cut.Find("#record-decision").Click();

        Text(cut.Find("#decision .action-result")).Should().Be("Not recorded. Give the committee's rationale.");
        Text(cut.Find("#decision-rationale-message")).Should().Be("Give the committee's rationale.");
        Page(cut).Should().NotContain("The Rationale field is required.");
    }

    // ─── The Record dialog (E1, 22) ─────────────────────────────────────────────

    [Fact]
    public void Record_SendsTheCategoryTheRationaleTheSentenceAndWhoSat_AndTheDecisionRisesAboveTheAgenda()
    {
        var agenda = Agenda(
        [
            Line(1, "Providing paediatric emergency care to children", CommitteeAgendaLineStatus.Staged, staged: true, evidence: 3),
            Line(2, "Managing common paediatric presentations"),
            Line(8, "Evaluating and managing neurodevelopmental and behavioural presentations in children",
                CommitteeAgendaLineStatus.AsOpportunityAllows, annual: true)
        ]);
        var cut = Render(Review(agenda: agenda), [Pending(1, "5", [12, 13])]);
        TickAll(cut);
        cut.Find("#decision-category").Change(CommitteeDecisionCategory.SatisfactoryProgress.ToString());
        cut.Find("#decision-rationale").Change("On track for completion.");
        cut.Find("#decision-undecided").Change("To be decided at the final review.");

        cut.Find("#record-decision").Click();

        var dialog = Dialog(cut, "Record Satisfactory Progress for Lerato Molefe?");
        dialog.QuerySelectorAll("p").Select(p => Text(p)).Should().Equal(
            "Fixes 1 staged decision: PAED-001 at 5.",
            "Records 2 EPAs as not decided: PAED-002 and PAED-008.",
            "Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)",
            "Afterwards only a decision that no longer fits the trainee's curriculum can be removed.");
        dialog.QuerySelectorAll("button").Select(button => Text(button)).Should().Equal("Not yet", "Record decision");

        var decided = Review(CommitteeReviewState.Decided, agenda: agenda, decisions: [Decision(undecidedNote: "To be decided at the final review.")]);
        Sender.On<RecordCommitteeDecisionCommand>(_ => decided);
        Sender.Now<GetCommitteeReviewByIdQuery>(_ => decided);
        DialogButton(cut, "Record Satisfactory Progress for Lerato Molefe?", "Record decision").Click();

        var sent = Sender.Sent<RecordCommitteeDecisionCommand>().Should().ContainSingle().Which;
        (sent.Category, sent.Rationale, sent.UndecidedNote).Should().Be(
            (CommitteeDecisionCategory.SatisfactoryProgress, "On track for completion.", "To be decided at the final review."));
        sent.PresentUserIds.Should().Equal(Chair, Naidoo, Botha, VanRensburg);

        cut.WaitForAssertion(() => Text(cut.Find("#decision .action-result")).Should().Be("Decision recorded."));
        var sections = cut.FindAll(".review-page > section").Select(section => section.Id).ToArray();
        Array.IndexOf(sections, "decision").Should().BeLessThan(Array.IndexOf(sections, "agenda"), "the decision rises above the agenda");
        Text(cut.Find("#decision article")).Should().Contain("About the EPAs not decided: To be decided at the final review.");
    }

    [Fact]
    public void ARecordRefusedByTheServer_IsShownAtTheFormsTop_InItsOwnWords()
    {
        var cut = Render(Review());
        TickAll(cut);
        cut.Find("#decision-category").Change(CommitteeDecisionCategory.SatisfactoryProgress.ToString());
        cut.Find("#decision-rationale").Change("On track.");
        Sender.On<RecordCommitteeDecisionCommand>(_ => throw new InvalidOperationException("This review changed meanwhile."));

        cut.Find("#record-decision").Click();
        DialogButton(cut, "Record Satisfactory Progress for Lerato Molefe?", "Record decision").Click();

        cut.WaitForAssertion(() => Text(cut.Find("#decision .action-result")).Should().Be("Not recorded. This review changed meanwhile."));
        cut.Find("#decision .action-result .alert").GetAttribute("role").Should().Be("alert");
        cut.Find("#decision-rationale").GetAttribute("value").Should().Be("On track.");
    }

    // ─── Ratify (E1, 22) ────────────────────────────────────────────────────────

    [Fact]
    public void Ratify_ConfirmsWithTheStarsItIssues_AndTheLinesNotDecided_NotYetSafe_NoMailPromised()
    {
        var agenda = Agenda(
        [
            Line(1, "Providing paediatric emergency care to children", CommitteeAgendaLineStatus.Staged, staged: true, evidence: 3),
            Line(2, "Managing common paediatric presentations", CommitteeAgendaLineStatus.NotDecided, readsNotDecided: true),
            Line(8, "Evaluating and managing neurodevelopmental and behavioural presentations in children",
                CommitteeAgendaLineStatus.NotDecided, annual: true, readsNotDecided: true),
            Line(10, "Leading and operating within a clinical team", CommitteeAgendaLineStatus.Staged, staged: true, evidence: 2)
        ]);
        var cut = Render(Review(CommitteeReviewState.Decided, agenda: agenda),
            [Pending(1, "5", [12, 13]), Pending(10, "4", [21, 22], new DateOnly(2026, 10, 25))]);

        Button(cut, "Ratify…").Click();

        var dialog = Dialog(cut, "Ratify and issue 2 STARs to Lerato Molefe?");
        dialog.QuerySelectorAll("li").Select(li => Text(li)).Should().Equal(
            "PAED-001 at 5, issued 2026-10-05", "PAED-010 at 4, issued 2026-10-05, expires 2026-10-25");
        dialog.QuerySelectorAll("p").Select(p => Text(p)).Should().Equal(
            "PAED-002 and 1 other were not decided at this sitting.",
            "Ratifying cannot be undone: nothing on this review can change afterwards.");
        Text(dialog).Should().NotContainEquivalentOf("mail");
        dialog.QuerySelectorAll("button").Select(button => Text(button)).Should().Equal("Not yet", "Ratify and issue 2 STARs");
    }

    [Fact]
    public void Ratifying_PutsDecisionRatifiedUnderTheBar_TakingTheFocus()
    {
        var cut = Render(Review(CommitteeReviewState.Decided), [Pending(1, "5", [12])]);
        var ratified = Review(CommitteeReviewState.Ratified, callerChairs: false);
        Sender.On<RatifyCommitteeDecisionCommand>(_ => ratified);

        Button(cut, "Ratify…").Click();
        Sender.Now<GetCommitteeReviewByIdQuery>(_ => ratified);
        DialogButton(cut, "Ratify and issue 1 STAR to Lerato Molefe?", "Ratify and issue 1 STAR").Click();

        cut.WaitForAssertion(() => Text(cut.Find("#sitting-bar + .action-result")).Should().Be("Decision ratified."));
        ElementFocusCalls().Should().BeGreaterThan(0);
    }

    // ─── Start (11, 22; E2; T173) and Close ─────────────────────────────────────

    [Fact]
    public void Start_ConfirmsWithWhatItFreezes_CountedByThePreview_NotYetSafe()
    {
        var cut = Render(Review(CommitteeReviewState.Scheduled));

        Button(cut, "Start review…").Click();

        var dialog = Dialog(cut, "Start Lerato Molefe's review for 2026 S2?");
        dialog.QuerySelectorAll("p").Select(p => Text(p)).Should().Equal(
            "Starting freezes the evidence: the 9 items observed from 2026-01-01 to 2026-12-31 (8 activities and the MSF " +
            "campaign report) become this review's snapshot, each in the state it is in now. Anything finished later is not added.");
        dialog.QuerySelectorAll("button").Select(button => Text(button)).Should().Equal("Not yet", "Start review");
        Sender.Sent<GetStartPreviewQuery>().Should().ContainSingle().Which.ReviewId.Should().Be(ReviewId);
    }

    [Fact]
    public void Start_SaysT173sSentence_WhenAClosedCampaignAwaitsRelease()
    {
        Sender.On<GetStartPreviewQuery>(_ => new StartPreviewDto(8, 8, 0, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 1));
        var cut = Render(Review(CommitteeReviewState.Scheduled));

        Button(cut, "Start review…").Click();

        Dialog(cut, "Start Lerato Molefe's review for 2026 S2?").QuerySelectorAll("p").Select(p => Text(p)).Should().Equal(
            "Starting freezes the evidence: the 8 items observed from 2026-01-01 to 2026-12-31 (8 activities) become this " +
            "review's snapshot, each in the state it is in now. Anything finished later is not added.",
            "Starting now leaves out 1 feedback campaign that closed in this window and is not yet released.");
    }

    [Fact]
    public void Starting_PutsReviewStartedAtopTheAgenda()
    {
        var cut = Render(Review(CommitteeReviewState.Scheduled));
        var started = Review();
        Sender.On<StartCommitteeReviewCommand>(_ => started);

        Button(cut, "Start review…").Click();
        Sender.Now<GetCommitteeReviewByIdQuery>(_ => started);
        DialogButton(cut, "Start Lerato Molefe's review for 2026 S2?", "Start review").Click();

        cut.WaitForAssertion(() => Text(cut.Find("#agenda h2 + .action-result")).Should().Be("Review started."));
        Text(cut.Find(".sitting-bar-state")).Should().Be("In progress");
    }

    [Fact]
    public void Close_ConfirmsThatNothingIsDecided_NotYetSafe_AndClosedSaysSoUnderTheBar()
    {
        var review = Review() with { IsFormative = true, Agenda = null };
        var cut = Render(review);
        var closed = Review(CommitteeReviewState.Final, decisions: []) with { IsFormative = true, Agenda = null };
        Sender.On<CloseFormativeReviewCommand>(_ => closed);

        Button(cut, "Close review…").Click();

        var dialog = Dialog(cut, "Close Lerato Molefe's formative check-in?");
        Text(dialog.QuerySelector("p")!).Should().Be("It records no decision and issues no STAR. Nothing on it can change once it is closed.");
        dialog.QuerySelectorAll("button").Select(button => Text(button)).Should().Equal("Not yet", "Close review");

        Sender.Now<GetCommitteeReviewByIdQuery>(_ => closed);
        DialogButton(cut, "Close Lerato Molefe's formative check-in?", "Close review").Click();

        cut.WaitForAssertion(() => Text(cut.Find("#sitting-bar + .action-result")).Should().Be("Formative review closed."));
    }

    // ─── The recorded decision ──────────────────────────────────────────────────

    [Fact]
    public void ARecordedDecision_ShowsItsCategoryRationaleSentenceConditionsAndWhoSat()
    {
        SignInAs(Naidoo);
        var decision = Decision() with { Conditions = "At least two Mini-CEX before the next sitting." };
        var cut = Render(Review(CommitteeReviewState.Ratified, callerChairs: false, decisions: [decision]));

        cut.Find("#decision article").QuerySelectorAll("h3, p").Select(element => Text(element)).Should().Equal(
            "Satisfactory Progress",
            "On track for completion.",
            "About the EPAs not decided: Not at a decision point this year.",
            "Conditions: At least two Mini-CEX before the next sitting.",
            "Present: Thandi Zulu (chair), David Naidoo");
    }

    [Fact]
    public void OutcomeDeferred_IsAProgressionCategory_ShownAsAnyOther()
    {
        // 1280 decide frame 11 (4.32): not a deferral; unrelated to D53.
        var cut = Render(Review(CommitteeReviewState.Ratified, callerChairs: false,
            decisions: [Decision(CommitteeDecisionCategory.OutcomeDeferred, "Too early in the programme to decide.")]));

        Text(cut.Find("#decision article h3")).Should().Be("Outcome Deferred");
        Text(cut.Find(".sitting-bar-count")).Should().StartWith("Outcome Deferred · ");
    }

    [Fact]
    public void AMemberWhileInProgress_ReadsThatNoDecisionIsRecordedYet()
    {
        SignInAs(Naidoo);
        var cut = Render(Review(callerChairs: false));

        Text(cut.Find("#no-decision-note")).Should().Be("No decision has been recorded yet.");
        cut.FindAll("#record-decision").Should().BeEmpty();
    }

    private static void TickAll(IRenderedComponent<Wombat.Web.Components.Pages.CommitteeDecisions.ReviewDetail> cut)
    {
        cut.Find("#decision-present-1").Change(true);
        cut.Find("#decision-present-2").Change(true);
        cut.Find("#decision-present-3").Change(true);
    }
}
