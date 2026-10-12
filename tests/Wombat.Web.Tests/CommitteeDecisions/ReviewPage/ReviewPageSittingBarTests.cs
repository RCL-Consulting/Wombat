using Bunit;
using FluentAssertions;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.CommitteeDecisions.ReviewPage;

/// <summary>
/// T361 (flow 07, round 3): the header and the sitting bar, board by board. 1280 start frames 1–4 and the 390 start
/// frames; 1280 more frames 3–6, 9; E2, E7, E8; round 3 items 14, 19, 21, 28; the settled Refresh.
/// </summary>
public sealed class ReviewPageSittingBarTests : ReviewPageTestBase
{
    // ─── The header (the title rule; 19; E6; T309 item 2) ─────────────────────────

    [Fact]
    public void TheHeading_NamesTheTraineeAndThePeriod_TheSubtitleTheTypePanelAndTrainingYear_AndTheTabSaysTheSame()
    {
        var cut = Render(Review(CommitteeReviewState.Scheduled));

        Text(cut.Find("h1")).Should().Be("Lerato Molefe · 2026 S2");
        TabTitle.Of(this, cut).Should().Be("Lerato Molefe · 2026 S2 · Wombat");
        Text(cut.Find(".page-subtitle span")).Should().Be("Annual progression review · Paed Annual Review Panel · Training year 4");
    }

    [Theory]
    [InlineData(WombatRoles.CommitteeMember)]
    [InlineData(WombatRoles.Coordinator)]
    [InlineData(WombatRoles.SpecialityAdmin)]
    [InlineData(WombatRoles.SubSpecialityAdmin)]
    public void TheProgressLink_IsInTheSubtitle_ForTheFourRolesTheRegistrarPageAdmits(string role)
    {
        SignInAs(Naidoo, role);
        var cut = Render(Review(CommitteeReviewState.Scheduled, callerChairs: false));

        var link = cut.Find(".page-subtitle a.review-progress-link");
        Text(link).Should().Be("Lerato Molefe's progress");
        link.GetAttribute("href").Should().Be("/programme/trainees/7");
    }

    [Theory]
    [InlineData(WombatRoles.InstitutionalAdmin)]
    [InlineData(WombatRoles.Administrator)]
    public void TheProgressLink_IsNotDrawn_ForARoleTheRegistrarPageDoesNotAdmit(string role)
    {
        // Nolwazi Mbatha's frames (4.26): flow 06's registrar page does not admit an institutional admin.
        SignInAs(Mbatha, role);
        var cut = Render(Review(CommitteeReviewState.Decided, callerChairs: false));

        cut.FindAll("a.review-progress-link").Should().BeEmpty();
    }

    [Fact]
    public void TheProgressLink_IsNotDrawn_WhenTheTraineeTrainsElsewhere()
    {
        var cut = Render(Review(CommitteeReviewState.Scheduled) with
        {
            TraineeProfileId = null,
            TraineeElsewhere = "This review's trainee does not train at the panel's institution, so the panel cannot act on it."
        });

        cut.FindAll("a.review-progress-link").Should().BeEmpty();
    }

    [Fact]
    public void AFormativeReview_IsAFormativeCheckIn_InItsSubtitle()
    {
        var cut = Render(Review(CommitteeReviewState.InProgress) with { IsFormative = true });

        Text(cut.Find(".page-subtitle span")).Should().StartWith("Formative check-in · Paed Annual Review Panel");
    }

    // ─── Scheduled (frames 1–5; E2; 28) ───────────────────────────────────────────

    [Fact]
    public void Scheduled_TheChairsBar_HasTheCountTheModeAndTheWindow_AndStart()
    {
        var cut = Render(Review(CommitteeReviewState.Scheduled));

        var bar = cut.Find("#sitting-bar");
        bar.ClassList.Should().Equal("sitting-bar");
        Text(bar.QuerySelector("h2")!).Should().Be("The sitting");
        bar.QuerySelector("h2")!.ClassList.Should().Contain("visually-hidden");
        Text(bar.QuerySelector(".sitting-bar-state")!).Should().Be("Scheduled");
        Text(bar.QuerySelector(".sitting-bar-count")!).Should().Be("4 EPAs on the agenda");
        Text(bar.QuerySelector(".sitting-bar-mode")!).Should().Be("Summative · evidence 2026-01-01 to 2026-12-31");
        Buttons(cut).Should().Contain("Start review…");
        cut.FindAll("#sitting-refresh").Should().BeEmpty("the chair's bar acts; it does not follow");
    }

    [Fact]
    public void Scheduled_AMemberWhoMaySit_IsOfferedStart_InAStaticBarWithTheActionStripe_AndRefresh()
    {
        SignInAs(Naidoo);
        var cut = Render(Review(CommitteeReviewState.Scheduled, callerChairs: false));

        cut.Find("#sitting-bar").ClassList.Should().BeEquivalentTo("sitting-bar", "sitting-bar--reader", "sitting-bar--starts");
        Buttons(cut).Should().Contain("Start review…");
        cut.FindAll("#sitting-sentence").Should().BeEmpty();
        Text(cut.Find("#sitting-as-of")).Should().Be("As of 14:02 SAST");
    }

    [Fact]
    public void Scheduled_AReaderTheHandlerRefuses_ReadsWhoCanStart_InStartsPlace()
    {
        SignInAs(Mbatha, WombatRoles.InstitutionalAdmin);
        var cut = Render(Review(CommitteeReviewState.Scheduled, callerChairs: false) with { CallerMayStart = false });

        Buttons(cut).Should().NotContain("Start review…");
        Text(cut.Find("#sitting-sentence")).Should().Be(
            "Only the coordinators of the panel's institution, and those of its members who are active committee members " +
            "there, can start this review.");
        cut.Find("#sitting-bar").ClassList.Should().BeEquivalentTo("sitting-bar", "sitting-bar--reader");
    }

    [Theory]
    [InlineData(Chair)]
    [InlineData(Naidoo)]
    public void Scheduled_WhileTheChairCannotAct_NobodyIsOfferedStart_AndTheBarSaysWhy(string reader)
    {
        // C5, Q11, E2: Start is never offered while the chair cannot act, to the chair, a member, anyone.
        SignInAs(reader);
        const string cannot = "The panel's chair, Thandi Zulu, cannot take the chair's actions now: the reason. A panel administrator must seat a chair who can.";
        var cut = Render(Review(CommitteeReviewState.Scheduled, callerChairs: false) with
        {
            CallerMayStart = true,
            ChairCannotAct = cannot,
            CallerChairs = false
        });

        Buttons(cut).Should().NotContain("Start review…");
        Text(cut.Find("#sitting-sentence")).Should().Be(cannot);
    }

    [Fact]
    public void Scheduled_WithTheTraineeElsewhere_NobodyIsOfferedStart_AndTheBarSaysWhy()
    {
        const string elsewhere = "This review's trainee does not train at the panel's institution, so the panel cannot act on it.";
        var cut = Render(Review(CommitteeReviewState.Scheduled) with { TraineeElsewhere = elsewhere, CallerChairs = false });

        Buttons(cut).Should().NotContain("Start review…");
        Text(cut.Find("#sitting-sentence")).Should().Be(elsewhere);
    }

    // ─── In progress: the chair's bar (E8) and a member's (4.22) ──────────────────

    [Fact]
    public void InProgress_TheChairsBar_CountsTheStaged_AndPresentFromTheTicks_AndOffersGoToTheDecision()
    {
        var cut = Render(Review(), [Pending(1, "5", [12, 13])]);

        var bar = cut.Find("#sitting-bar");
        Text(bar.QuerySelector(".sitting-bar-state")!).Should().Be("In progress");
        Text(bar.QuerySelector(".sitting-bar-count")!).Should().Be("1 staged");
        Text(bar.QuerySelector(".sitting-bar-present")!).Should().Be("Present: the chair · one more member needed");
        bar.QuerySelector(".sitting-bar-present")!.HasAttribute("aria-live").Should().BeFalse("only the quorum line is live (21)");
        Buttons(cut).Should().Contain("Go to the decision");

        cut.Find("#decision-present-1").Change(true);
        cut.Find("#decision-present-2").Change(true);
        Text(cut.Find("#sitting-bar .sitting-bar-present")).Should().Be("3 present · quorum met");
    }

    [Fact]
    public void InProgress_WithNothingStaged_TheCountIsNoneStaged()
    {
        var cut = Render(Review());

        Text(cut.Find(".sitting-bar-count")).Should().Be("None staged");
    }

    [Fact]
    public void GoToTheDecision_MovesTheFocusToTheDecisionsHeading()
    {
        var cut = Render(Review());

        Button(cut, "Go to the decision").Click();

        FocusedIds().Should().Contain("decision-heading");
        cut.Find("#decision-heading").GetAttribute("tabindex").Should().Be("-1");
    }

    [Fact]
    public void InProgress_AMember_ReadsAStaticBar_WhoMayAct_AsOf_AndRefresh_AndNoControl()
    {
        SignInAs(Naidoo);
        var cut = Render(Review(callerChairs: false), [Pending(1, "5", [12, 13])]);

        var bar = cut.Find("#sitting-bar");
        bar.ClassList.Should().BeEquivalentTo("sitting-bar", "sitting-bar--reader");
        Text(bar.QuerySelector(".sitting-bar-count")!).Should().Be("1 staged");
        Text(cut.Find("#sitting-sentence")).Should().Be(
            "Only the panel's chair, Thandi Zulu, can stage entrustment decisions and record the committee's decision.");
        Text(cut.Find("#sitting-as-of")).Should().Be("As of 14:02 SAST");
        cut.Find("#sitting-as-of").GetAttribute("role").Should().Be("status");
        Buttons(cut).Should().Contain("Refresh").And.NotContain(["Go to the decision", "Stage", "Remove…", "Record decision…"]);
        cut.FindAll(".sitting-bar-present").Should().BeEmpty();
    }

    [Fact]
    public void Refresh_ReReadsInPlace_AndSaysWhatChanged()
    {
        // 1280 more frame 4 (21): "As of 14:31 SAST. Now Decided."; the bar's sentence follows (T260).
        SignInAs(Naidoo);
        var cut = Render(Review(callerChairs: false));

        Clock.Value = Now.AddMinutes(29);
        Sender.Now<GetCommitteeReviewByIdQuery>(_ => Review(CommitteeReviewState.Decided, callerChairs: false));
        Button(cut, "Refresh").Click();

        cut.WaitForAssertion(() => Text(cut.Find("#sitting-as-of")).Should().Be("As of 14:31 SAST. Now Decided."));
        Text(cut.Find("#sitting-sentence")).Should().Be("Only the panel's chair, Thandi Zulu, can ratify the committee's decision.");
        Text(cut.Find(".sitting-bar-state")).Should().Be("Decided");
        Text(cut.Find(".sitting-bar-count")).Should().Be("Satisfactory Progress · no decision to issue");
        ElementFocusCalls().Should().Be(0, "the focus stays on Refresh");
        FocusedIds().Should().BeEmpty();
    }

    [Fact]
    public void Refresh_WithNothingChanged_SaysNoChange()
    {
        SignInAs(Naidoo);
        var cut = Render(Review(callerChairs: false));

        Clock.Value = Now.AddMinutes(29);
        Button(cut, "Refresh").Click();

        cut.WaitForAssertion(() => Text(cut.Find("#sitting-as-of")).Should().Be("As of 14:31 SAST. No change."));
    }

    [Fact]
    public void Refresh_WithAnotherDecisionStaged_SaysHowManyNow()
    {
        SignInAs(Naidoo);
        var cut = Render(Review(callerChairs: false));

        Sender.Now<ListPendingEntrustmentDecisionsForReviewQuery>(_ => new[] { Pending(1, "5", [12]) });
        Button(cut, "Refresh").Click();

        cut.WaitForAssertion(() => Text(cut.Find("#sitting-as-of")).Should().Be("As of 14:02 SAST. Now 1 staged."));
    }

    [Fact]
    public void ReadOnAnotherDay_TheAsOfLineGivesTheDate()
    {
        // E7: the page left open overnight reads "As of 2026-10-04 14:02 SAST" at its next render; the same day, the time.
        var readUtc = new DateTime(2026, 10, 4, 12, 2, 0, DateTimeKind.Utc);

        Wombat.Web.Components.Pages.CommitteeDecisions.Review.ReviewWords.AsOf(readUtc, new DateOnly(2026, 10, 5), null)
            .Should().Be("As of 2026-10-04 14:02 SAST");
        Wombat.Web.Components.Pages.CommitteeDecisions.Review.ReviewWords.AsOf(readUtc, new DateOnly(2026, 10, 4), null)
            .Should().Be("As of 14:02 SAST");
        Wombat.Web.Components.Pages.CommitteeDecisions.Review.ReviewWords.AsOf(new DateTime(2026, 10, 4, 22, 30, 0, DateTimeKind.Utc), new DateOnly(2026, 10, 5), null)
            .Should().Be("As of 00:30 SAST", "22:30 UTC is already tomorrow in South Africa (T325)");
    }

    // ─── Decided, ratified, formative, closed ────────────────────────────────────

    [Fact]
    public void Decided_TheChairsBar_CountsWhatRatifyIssues_AndOffersRatify()
    {
        var cut = Render(Review(CommitteeReviewState.Decided), [Pending(1, "5", [12]), Pending(10, "4", [21])]);

        Text(cut.Find(".sitting-bar-count")).Should().Be("Satisfactory Progress · 2 decisions to issue");
        Buttons(cut).Should().Contain("Ratify…");
    }

    [Fact]
    public void Decided_RatifyShortOfAQuorum_IsUnavailable_NeverNativelyDisabled_WithItsReasonBesideIt()
    {
        var cut = Render(Review(CommitteeReviewState.Decided) with { QuorumShortfall = "Only the chair was recorded as present." });

        var ratify = cut.Find("#sitting-ratify");
        ratify.HasAttribute("disabled").Should().BeFalse();
        ratify.GetAttribute("aria-disabled").Should().Be("true");
        ratify.ClassList.Should().Contain("is-unavailable");
        ratify.GetAttribute("aria-describedby").Should().Be("ratify-reason");
        Text(cut.Find("#ratify-reason")).Should().Be("Only the chair was recorded as present.");

        ratify.Click();
        FocusedIds().Should().Contain("ratify-reason");
    }

    [Fact]
    public void Decided_TheAdministrator_ReadsWhoRatifies_AndNoControl()
    {
        SignInAs(Mbatha, WombatRoles.InstitutionalAdmin);
        var cut = Render(Review(CommitteeReviewState.Decided, callerChairs: false), [Pending(1, "5", [12])]);

        Text(cut.Find("#sitting-sentence")).Should().Be("Only the panel's chair, Thandi Zulu, can ratify the committee's decision.");
        Buttons(cut).Should().NotContain(["Ratify…", "Remove…"]);
        cut.FindAll("#agenda th").Select(Text).Should().NotContain("Action");
    }

    [Fact]
    public void Ratified_TheBarCountsTheStarsIssued_AndOffersNothing()
    {
        var agenda = Agenda(
        [
            Line(1, "Providing paediatric emergency care to children", CommitteeAgendaLineStatus.Decided, CommitteeAgendaLineState.Decided, starId: 1),
            Line(2, "Managing common paediatric presentations", CommitteeAgendaLineStatus.NotDecided, CommitteeAgendaLineState.NotDecided, readsNotDecided: true)
        ]);
        var cut = Render(Review(CommitteeReviewState.Ratified, callerChairs: false, agenda: agenda));

        Text(cut.Find(".sitting-bar-count")).Should().Be("Satisfactory Progress · 1 STAR issued");
        cut.FindAll("#sitting-refresh").Should().BeEmpty("a ratified review no longer changes");
        cut.FindAll("#sitting-bar .sitting-bar-actions").Should().BeEmpty();
    }

    [Fact]
    public void Formative_InProgress_TheChairsBar_SaysTheMode_AndOffersCloseAlone()
    {
        var cut = Render(Review() with { IsFormative = true, Agenda = null });

        Text(cut.Find(".sitting-bar-mode")).Should().Be("Formative: an interim check-in, no binding decision");
        cut.FindAll(".sitting-bar-count").Should().BeEmpty();
        Buttons(cut).Should().Contain("Close review…").And.NotContain("Go to the decision");
        cut.FindAll("#agenda").Should().BeEmpty("a formative review has no agenda");
        cut.FindAll("#decision").Should().BeEmpty("nor a decision");
    }

    [Fact]
    public void Formative_AMember_ReadsThatOnlyTheChairCanClose()
    {
        SignInAs(Naidoo);
        var cut = Render(Review(callerChairs: false) with { IsFormative = true, Agenda = null });

        Text(cut.Find("#sitting-sentence")).Should().Be("Only the panel's chair, Thandi Zulu, can close this review.");
    }

    [Fact]
    public void Formative_Closed_SaysNoDecisionRecorded()
    {
        var cut = Render(Review(CommitteeReviewState.Final, callerChairs: false, decisions: []) with { IsFormative = true, Agenda = null });

        Text(cut.Find(".sitting-bar-state")).Should().Be("Closed");
        Text(cut.Find(".sitting-bar-count")).Should().Be("Formative check-in · no decision recorded");
    }

    // ─── Stacking (14, E8) ───────────────────────────────────────────────────────

    [Fact]
    public void OnlyTheChairsBar_IsTheOneThatSticks()
    {
        var chair = Render(Review());
        chair.Find("#sitting-bar").ClassList.Should().NotContain("sitting-bar--reader");
    }
}
