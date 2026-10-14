using System.Security.Claims;
using System.Text.RegularExpressions;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Components.Pages.CommitteeDecisions.Review;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// Her review's page, <c>/committee/my-reviews/{id}</c> (T366, flow 09; boards R3-2 a–c; round 3 F6, F8, M3, S5, S9, S12,
/// S13; T308): header, Decisions, Appeal, Agenda; the lodge block while Ratified; the review read again after a lodge, done
/// or refused; flow 01's not found for anything that is not hers; "Committee review" while it loads or fails.
/// </summary>
public sealed partial class MyReviewPageTests : TestContext
{
    private const int ReviewId = 4;
    private const string TraineeId = "mahlangu";
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    private const string Reason =
        "The single DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it " +
        "was rated. I ask for reconsideration.";

    private readonly RecordingSender _sender = new();

    public MyReviewPageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("mahlangu@test");
        auth.SetRoles(WombatRoles.Trainee);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, TraineeId));
        Services.AddSingleton<IScopedSender>(_sender);
    }

    // ─── Reading (4.41, 4.43 before) ─────────────────────────────────────────

    [Fact]
    public void ARatifiedReview_ReadsHeaderDecisionsAppealAgenda_InThatOrder()
    {
        var cut = Open(Review(CommitteeReviewState.Ratified));

        cut.Find("h1").TextContent.Trim().Should().Be("2026 S2 annual progression review");
        TabTitle.Of(this, cut).Should().Be("2026 S2 annual progression review · Wombat");
        cut.Find(".page-subtitle").TextContent.Trim().Should().Be("Paed Annual Review Panel");
        cut.Find(".review-page").ClassList.Should().Contain("my-review-page");

        cut.FindAll(".review-page > section").Select(section => string.IsNullOrEmpty(section.Id) ? section.GetAttribute("aria-label") : section.Id).Should().Equal(
            "About this review", "decisions", "appeal", "agenda");
        cut.FindAll(".review-page h2").Select(heading => heading.TextContent.Trim()).Should().Equal("Decisions", "Appeal", "Agenda");

        cut.FindAll(".detail-list dt").Select(term => term.TextContent.Trim()).Should().Equal(
            "Panel", "Sits for", "Evidence window", "Type", "State");
        Text(cut.Find("#my-review-period")).Should().Be("2026 S2");
        Text(cut.Find("#my-review-window")).Should().Be("2026-01-01 to 2026-12-31");
        Text(cut.Find("#my-review-type")).Should().Be("Annual progression review");
        var state = cut.Find("#my-review-state .badge");
        Text(state).Should().Be("Ratified");
        state.ClassList.Should().Contain("badge-completed");
    }

    [Fact]
    public void TheDecision_SaysItsRationale_TheSentenceAboutTheEpasNotDecided_AndWhoSat()
    {
        var cut = Open(Review(CommitteeReviewState.Ratified));

        var record = cut.Find("#decisions .decision-history .decision-record");
        Text(record.QuerySelector("h3")!).Should().Be("Inadequate Progress — Additional Training");
        Text(record).Should()
            .Contain("One DOPS in the year and a Mini-CEX that stalled.")
            .And.Contain("About the EPAs not decided: Not enough observed evidence yet this year.")
            .And.Contain("Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)");
    }

    [Fact]
    public void TheLodgeBlock_NamesTheAppealBody_AndEveryReaderTruly_ThenTheReasonWithItsCounter()
    {
        var cut = Open(Review(CommitteeReviewState.Ratified));

        Text(cut.Find(".lodge-block h3")).Should().Be("What happens next");
        Text(cut.Find("#lodge-heard-by")).Should().Be(
            "Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external).");
        Text(cut.Find(".lodge-next")).Should().Contain("Either outcome is final: nothing reopens the review, and it cannot be appealed again.");
        Text(cut.Find("#lodge-readers")).Should().Be(
            "Who reads your reason: everyone who can open this review on the committee's side: the panel's members, the " +
            "coordinators and institutional administrators of Kgosi Kgari Teaching Hospital, your programme's speciality and " +
            "sub-speciality administrators, and Wombat's system administrators.");

        var box = cut.Find("textarea#appeal-reason");
        Text(cut.Find("label[for='appeal-reason']")).Should().Be("Why you are appealing * required");
        box.GetAttribute("aria-required").Should().Be("true");
        box.HasAttribute("maxlength").Should().BeFalse("a long paste is never cut silently: the counter shows it, and Lodge refuses it");
        box.GetAttribute("aria-describedby").Should().Be("appeal-reason-help appeal-reason-count");
        Text(cut.Find("#appeal-reason-help.field-help")).Should().Be("Up to 4,000 characters.");
        Text(cut.Find("#appeal-reason-count.field-help")).Should().Be("0 of 4,000 characters");
        cut.Find("#lodge-appeal").TextContent.Trim().Should().Be("Lodge appeal");
        cut.Find("#lodge-appeal").HasAttribute("disabled").Should().BeFalse();
        cut.Markup.Should().NotContain("Deferred").And.NotContain("Upheld");
    }

    [Fact]
    public void TheCounter_IsCountedInTheBrowser_AndAgreesWithWhatIsKept()
    {
        var cut = Open(Review(CommitteeReviewState.Ratified));

        JSInterop.Invocations.Should().Contain(invocation => invocation.Identifier == "import"
            && (string)invocation.Arguments[0]! == "./js/review-appeal.js");

        cut.Find("#appeal-reason").Change(Reason);

        Text(cut.Find("#appeal-reason-count")).Should().Be("153 of 4,000 characters");
    }

    [Fact]
    public void TheAgenda_IsReadOnly_EachLineSayingWhatTheCommitteeDid_NeverDeferred()
    {
        var cut = Open(Review(CommitteeReviewState.Ratified) with
        {
            Agenda = Agenda(
                Line(1, "PAED-001", CommitteeAgendaLineStatus.Decided, starId: 7),
                Line(2, "PAED-002", CommitteeAgendaLineStatus.NotDecided),
                Line(3, "PAED-003", CommitteeAgendaLineStatus.NotDecided, semester: null))
        });

        Text(cut.Find("#agenda caption")).Should().Be(
            "The EPAs this review was there to decide for 2026 S2, and what the committee did with each.");
        cut.FindAll("#agenda thead th").Select(header => Text(header)).Should().Equal("EPA", "Window", "Outcome");
        Text(cut.Find("#my-agenda-line-1")).Should().Contain("PAED-001").And.Contain("Decided").And.Contain("STAR #7.");
        cut.Find("#my-agenda-line-1 .badge").ClassList.Should().Contain("badge-completed");
        foreach (var line in new[] { "#my-agenda-line-2", "#my-agenda-line-3" })
        {
            Text(cut.Find($"{line} .badge")).Should().Be("Not decided at this sitting");
            Text(cut.Find(line)).Should().Contain("Not enough observed evidence yet this year.");
        }

        Text(cut.Find("#my-agenda-line-3 td[data-label='Window']")).Should().Be("2026");
        cut.FindAll("#agenda button, #agenda a").Should().BeEmpty("her agenda is read-only, and a STAR has no download here (Q6)");
        Text(cut.Find("#agenda")).Should().NotContain("Deferred").And.NotContain("reason");
    }

    // ─── Lodging (4.43; F6, M3, S5) ──────────────────────────────────────────

    [Fact]
    public void LodgeAppeal_WithTheReasonBlank_IsRefusedUnderTheField_WhichTakesTheFocus_AndNothingIsSent()
    {
        var cut = Open(Review(CommitteeReviewState.Ratified));
        var focusesBefore = FocusCalls().Count;

        cut.Find("#lodge-appeal").Click();

        Text(cut.Find("#appeal-reason-message.validation-message")).Should().Be("Give your reason for appealing.");
        var box = cut.Find("#appeal-reason");
        box.GetAttribute("aria-invalid").Should().Be("true");
        box.ClassList.Should().Contain("input-validation-error");
        box.GetAttribute("aria-describedby").Should().Be("appeal-reason-help appeal-reason-count appeal-reason-message");
        FocusCalls().Should().HaveCount(focusesBefore + 1, "the field takes the focus");
        JSInterop.Invocations.Should().NotContain(invocation => invocation.Identifier == "wombatDialog.showModal");
        _sender.Received.OfType<LodgeAppealCommand>().Should().BeEmpty();
    }

    [Fact]
    public void LodgeAppeal_OverTheLimit_IsRefusedUnderTheField()
    {
        var cut = Open(Review(CommitteeReviewState.Ratified));

        cut.Find("#appeal-reason").Change(new string('a', 4001));
        cut.Find("#lodge-appeal").Click();

        Text(cut.Find("#appeal-reason-message")).Should().Be("Give your reason in 4,000 characters or fewer.");
        Text(cut.Find("#appeal-reason-count")).Should().Be("4,001 of 4,000 characters");
        _sender.Received.OfType<LodgeAppealCommand>().Should().BeEmpty();
    }

    [Fact]
    public void LodgeAppeal_OpensTheDialog_KeepEditingFirst_SayingWhatIsAppealed_WhoHearsIt_AndWhoReadsIt()
    {
        var cut = Open(Review(CommitteeReviewState.Ratified));
        cut.Find("#appeal-reason").Change(Reason);

        cut.Find("#lodge-appeal").Click();

        JSInterop.Invocations.Should().Contain(invocation => invocation.Identifier == "wombatDialog.showModal");
        var dialog = cut.Find("dialog");
        Text(dialog.QuerySelector("h2")!).Should().Be("Lodge your appeal?");
        Text(dialog.QuerySelector(".dialog-body")!).Should()
            .Contain("Against the 2026 S2 annual progression review's decision, Inadequate Progress — Additional Training.")
            .And.Contain("Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external). Either outcome is final.")
            .And.Contain("Kgosi Kgari Teaching Hospital");
        dialog.QuerySelectorAll(".form-actions button").Select(button => Text(button)).Should().Equal(
            ["Keep editing", "Lodge appeal"], "the safe button first, which the native dialog focuses on open");
    }

    [Fact]
    public void KeepEditing_ClosesTheDialog_SendsNothing_AndKeepsTheReason()
    {
        var cut = Open(Review(CommitteeReviewState.Ratified));
        cut.Find("#appeal-reason").Change(Reason);
        cut.Find("#lodge-appeal").Click();

        DialogButton(cut, "Keep editing").Click();

        JSInterop.Invocations.Should().Contain(invocation => invocation.Identifier == "wombatDialog.close");
        _sender.Received.OfType<LodgeAppealCommand>().Should().BeEmpty();
        cut.Find("#appeal-reason").GetAttribute("value").Should().Be(Reason, "F6: the text is kept");
        Text(cut.Find("#appeal-reason-count")).Should().Be("153 of 4,000 characters");
    }

    [Fact]
    public void Lodged_ThePageReadsTheReviewAgain_AppealLodgedTakesTheFocus_UnderAppeal_HerOpenEntry_NoForm()
    {
        var reads = 0;
        _sender
            .On<GetMyCommitteeReviewQuery>(_ => ++reads == 1 ? Review(CommitteeReviewState.Ratified) : UnderAppeal())
            .On<LodgeAppealCommand>(_ => Unnamed(UnderAppeal()));
        var cut = Render();

        cut.Find("#appeal-reason").Change(Reason);
        cut.Find("#lodge-appeal").Click();
        DialogButton(cut, "Lodge appeal").Click();

        cut.WaitForAssertion(() => Text(cut.Find("#appeal-result")).Should().Be("Appeal lodged."));
        _sender.Received.OfType<LodgeAppealCommand>().Should().ContainSingle()
            .Which.Should().Match<LodgeAppealCommand>(command => command.ReviewId == ReviewId && command.Reason == Reason);
        reads.Should().Be(2, "the review is read again after the lodge (T260, S5)");

        var result = cut.FindComponent<ActionResult>();
        FocusCalls().Last().Arguments[0].Should().BeOfType<ElementReference>().Which.Id.Should().Be(result.Instance.Element.Id);
        Text(cut.Find("#my-review-state")).Should().Be("Under appeal");
        cut.Find("#my-review-state .badge").ClassList.Should().Contain("badge-accepted");
        cut.FindAll(".lodge-block, #appeal-reason, #lodge-appeal").Should().BeEmpty("one open appeal at a time");

        var entry = cut.Find("#appeal ul.appeal-list > li.appeal-entry");
        Text(entry.QuerySelector(".appeal-entry-head")!).Should().Be("Your appeal Open");
        entry.QuerySelector(".appeal-entry-head .badge")!.ClassList.Should().Contain("badge-accepted");
        Text(entry.QuerySelector(".appeal-entry-meta")!).Should().Be("Lodged 2026-10-13");
        Text(entry.QuerySelector(".appeal-entry-outcome")!).Should().Be(
            "Heard by Thandi Zulu (chair) and John van Rensburg (external).");
        Text(entry.QuerySelector(".appeal-entry-reason")!).Should().Be(Reason);
        cut.FindAll("#agenda").Should().HaveCount(1, "the agenda stays in view");
    }

    [Fact]
    public void ARefusedLodge_ShowsTheReviewsOwnWords_ThenThePageReadsTheReviewAgain()
    {
        // M3, S5: a second tab lodged first, so this review is Under appeal.
        var reads = 0;
        _sender
            .On<GetMyCommitteeReviewQuery>(_ => ++reads == 1 ? Review(CommitteeReviewState.Ratified) : UnderAppeal())
            .On<LodgeAppealCommand>(_ => throw new CommitteeRefusedException("Only ratified reviews can be appealed."));
        var cut = Render();

        cut.Find("#appeal-reason").Change(Reason);
        cut.Find("#lodge-appeal").Click();
        DialogButton(cut, "Lodge appeal").Click();

        cut.WaitForAssertion(() => Text(cut.Find("#appeal-result")).Should().Be("Only ratified reviews can be appealed."));
        cut.Find("#appeal-result .alert").ClassList.Should().Contain("alert-danger");
        reads.Should().Be(2);
        var result = cut.FindComponent<ActionResult>();
        FocusCalls().Last().Arguments[0].Should().BeOfType<ElementReference>().Which.Id.Should().Be(result.Instance.Element.Id);
        Text(cut.Find("#my-review-state")).Should().Be("Under appeal");
        cut.FindAll(".lodge-block").Should().BeEmpty("the form is gone once the page has read the review again");
        cut.FindAll("li.appeal-entry").Should().ContainSingle("the open entry from the other tab");
        cut.Find("#appeal > .action-result").Should().NotBeNull("the refusal heads her Appeal section");
    }

    [Fact]
    public void AnUnexpectedFailure_IsAFixedSentence_NeverTheExceptionsText()
    {
        _sender
            .On<GetMyCommitteeReviewQuery>(_ => Review(CommitteeReviewState.Ratified))
            .On<LodgeAppealCommand>(_ => throw new InvalidOperationException("Sequence contains no elements"));
        var cut = Render();

        cut.Find("#appeal-reason").Change(Reason);
        cut.Find("#lodge-appeal").Click();
        DialogButton(cut, "Lodge appeal").Click();

        cut.WaitForAssertion(() => Text(cut.Find("#appeal-result")).Should().Be(
            "Your appeal could not be lodged. Nothing has changed. Try again, or come back in a few minutes."));
        cut.Markup.Should().NotContain("Sequence contains");
        cut.Find("#appeal-reason").GetAttribute("value").Should().Be(Reason, "what she typed is kept to try again");
    }

    [Fact]
    public void WhenTheReadAgainFails_TheAnswerIsNamedFromWhatThePageHeld()
    {
        // The command's answer comes from the lookup-free mapper, which names nobody (T142, T165).
        var reads = 0;
        _sender
            .On<GetMyCommitteeReviewQuery>(_ => ++reads == 1 ? Review(CommitteeReviewState.Ratified) : throw new TimeoutException("read again"))
            .On<LodgeAppealCommand>(_ => Unnamed(UnderAppeal()));
        var cut = Render();

        cut.Find("#appeal-reason").Change(Reason);
        cut.Find("#lodge-appeal").Click();
        DialogButton(cut, "Lodge appeal").Click();

        cut.WaitForAssertion(() => Text(cut.Find("#appeal-result")).Should().Be("Appeal lodged."));
        Text(cut.Find(".decision-record")).Should().Contain("Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)");
        cut.FindAll("#my-agenda-line-2").Should().HaveCount(1, "lodging changes no agenda line");
    }

    // ─── Outcomes (4.48; the dismissal; C17) ─────────────────────────────────

    [Fact]
    public void ClosedAfterARemit_BothDecisionsNewestFirst_TheFirstMarkedReplaced_HerEntryRemitted_NoForm()
    {
        var cut = Open(Closed(CommitteeAppealOutcome.Remitted));

        var records = cut.FindAll(".decision-history .decision-record").ToList();
        records.Select(record => Text(record.QuerySelector("h3")!)).Should().Equal(
            "Satisfactory with Observations", "Inadequate Progress — Additional Training");
        records[0].QuerySelectorAll(".decision-replaced").Should().BeEmpty();
        Text(records[0]).Should().Contain("Conditions: Two observed Mini-CEX and one DOPS before the next review.")
            .And.Contain("Present: Thandi Zulu (chair), Sarah Botha");
        Text(records[1].QuerySelector(".decision-replaced")!).Should().Be("Replaced on appeal by the decision above.");
        Text(records[1]).Should().Contain("Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)");

        var entry = cut.Find("li.appeal-entry");
        Text(entry.QuerySelector(".appeal-entry-head")!).Should().Be("Your appeal Remitted");
        entry.QuerySelector(".badge")!.ClassList.Should().Contain("badge-completed");
        Text(entry.QuerySelector(".appeal-entry-meta")!).Should().Be("Lodged 2026-10-13 · Resolved 2026-10-13 by Thandi Zulu");
        Text(entry.QuerySelector(".appeal-entry-outcome")!).Should().Be(
            "Remitted: the appeal body replaced the decision. The decision above is theirs, and it is final.");
        Text(cut.Find("#appeal-closed")).Should().Be("This review is closed and cannot be appealed again.");
        cut.FindAll(".lodge-block").Should().BeEmpty();
        Text(cut.Find("#my-review-state")).Should().Be("Closed");
    }

    [Fact]
    public void ClosedAfterADismissal_TheDecisionStandsAlone_HerEntryDismissed()
    {
        var cut = Open(Closed(CommitteeAppealOutcome.Dismissed));

        cut.FindAll(".decision-record").Should().ContainSingle();
        var entry = cut.Find("li.appeal-entry");
        Text(entry.QuerySelector(".appeal-entry-head")!).Should().Be("Your appeal Dismissed");
        entry.QuerySelector(".badge")!.ClassList.Should().Contain("badge-completed");
        Text(entry.QuerySelector(".appeal-entry-outcome")!).Should().Be("Dismissed: the decision stands, and it is final.");
    }

    [Fact]
    public void AnEntrustmentOnlyReview_Remitted_SaysWhatItDecided_AndItsTwoHeadingsDiffer()
    {
        var review = Closed(CommitteeAppealOutcome.Remitted) with
        {
            ReviewType = CommitteeReviewType.EntrustmentOnly,
            Agenda = Agenda(Line(4, "PAED-004", CommitteeAgendaLineStatus.Decided, starId: 91)),
            Decisions =
            [
                Decision(42, null, "The appeal body's rationale.", supersedes: 41),
                Decision(41, null, "The committee's rationale.")
            ]
        };

        var cut = Open(review);

        cut.Find("h1").TextContent.Trim().Should().Be("2026 S2 entrustment-only review");
        var records = cut.FindAll(".decision-record").ToList();
        records.Select(record => Text(record.QuerySelector("h3")!)).Should().Equal(
            "Entrustment decisions only", "Entrustment decisions only, replaced on appeal");
        records[0].QuerySelectorAll(".decision-note").Select(note => Text(note)).Should().Equal(
            CommitteeDecisionWording.EntrustmentOnlyForTheTrainee,
            CommitteeDecisionWording.EntrustmentOnlyRemittedForTheTrainee,
            CommitteeDecisionWording.EntrustmentOnlySeeTheAgenda);
        records[1].QuerySelectorAll(".decision-note").Select(note => Text(note)).Should().Equal(
            CommitteeDecisionWording.EntrustmentOnlyForTheTrainee);
        Text(records[1].QuerySelector(".decision-replaced")!).Should().Be("Replaced on appeal by the decision above.");
    }

    // ─── Other kinds of review (5.8, 4.51) ───────────────────────────────────

    [Fact]
    public void APreGraduationReview_IsHeadedByItsType_AndOffersTheLodgeBlock()
    {
        var cut = Open(Review(CommitteeReviewState.Ratified) with { ReviewType = CommitteeReviewType.PreGraduation });

        cut.Find("h1").TextContent.Trim().Should().Be("2026 S2 pre-graduation review");
        Text(cut.Find("#my-review-type")).Should().Be("Pre-graduation review");
        cut.FindAll(".lodge-block").Should().ContainSingle();
    }

    [Fact]
    public void AFormativeCheckIn_RecordsNoDecision_AndHasNoAppealAndNoAgenda()
    {
        var cut = Open(Review(CommitteeReviewState.Final) with { IsFormative = true, Decisions = [] });

        cut.Find("h1").TextContent.Trim().Should().Be("2026 S2 formative check-in");
        Text(cut.Find("#my-review-type")).Should().Be("Formative check-in");
        Text(cut.Find("#formative-note")).Should().Be(
            "No binding decision. A formative check-in records no decision and issues no STAR.");
        cut.FindAll("#appeal, #agenda, .lodge-block").Should().BeEmpty();
    }

    // ─── Not found, loading, failure (F8, S12, T329) ─────────────────────────

    [Fact]
    public void AReviewThatIsNotHers_IsFlow01sPageNotFound_WordForWord()
    {
        _sender.On<GetMyCommitteeReviewQuery>(_ => null);

        var cut = RenderComponent<MyReview>(parameters => parameters.Add(page => page.ReviewId, ReviewId));
        cut.WaitForState(() => cut.FindAll(".system-panel").Count == 1);

        cut.Find("h1").TextContent.Trim().Should().Be("Page not found");
        TabTitle.Of(this, cut).Should().Be("Page not found · Wombat");
        Text(cut.Find(".system-panel")).Should().Be("There is no page at this address. Check the address, or start again from Home. Go to Home");
        cut.FindAll(".review-page, dialog").Should().BeEmpty();
    }

    [Fact]
    public void WhileItLoads_TheHeaderReadsCommitteeReview_AndAStatusLineSaysSo()
    {
        var pending = new TaskCompletionSource<CommitteeReviewDetailDto?>();
        _sender.OnAsync<GetMyCommitteeReviewQuery>(_ => pending.Task);

        var cut = RenderComponent<MyReview>(parameters => parameters.Add(page => page.ReviewId, ReviewId));

        cut.Find("h1").TextContent.Trim().Should().Be("Committee review");
        cut.Find("p.visually-hidden[role='status']").TextContent.Should().Be("Loading your review.");
        cut.FindAll(".lodge-block, #lodge-appeal").Should().BeEmpty();
        cut.FindAll(".skeleton").Should().NotBeEmpty();
    }

    [Fact]
    public void AFailedRead_IsFlow07sFixedSentence_WithTryAgain_TheHeaderStillCommitteeReview()
    {
        var reads = 0;
        _sender.On<GetMyCommitteeReviewQuery>(_ => ++reads == 1
            ? throw new InvalidOperationException("Npgsql: the connection was reset")
            : Review(CommitteeReviewState.Ratified));

        var cut = RenderComponent<MyReview>(parameters => parameters.Add(page => page.ReviewId, ReviewId));
        cut.WaitForState(() => cut.FindAll(".alert-danger").Count == 1);

        cut.Find("h1").TextContent.Trim().Should().Be("Committee review");
        Text(cut.Find(".alert-danger")).Should().Contain("The review could not be loaded. Nothing on it has changed.");
        cut.Markup.Should().NotContain("Npgsql");

        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Try again").Click();
        cut.WaitForState(() => cut.FindAll("#lodge-appeal").Count == 1);
        cut.Find("h1").TextContent.Trim().Should().Be("2026 S2 annual progression review");
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private IRenderedComponent<MyReview> Open(CommitteeReviewDetailDto review)
    {
        _sender.On<GetMyCommitteeReviewQuery>(_ => review);
        return Render();
    }

    private IRenderedComponent<MyReview> Render()
    {
        var cut = RenderComponent<MyReview>(parameters => parameters.Add(page => page.ReviewId, ReviewId));
        cut.WaitForState(() => cut.FindAll(".review-page:not([aria-busy])").Count == 1);
        return cut;
    }

    private IReadOnlyList<JSRuntimeInvocation> FocusCalls()
        => JSInterop.Invocations.Where(invocation => invocation.Identifier == FocusIdentifier).ToList();

    private static AngleSharp.Dom.IElement DialogButton(IRenderedFragment cut, string label)
        => cut.FindAll("dialog .form-actions button").Single(button => button.TextContent.Trim() == label);

    private static CommitteeReviewDetailDto Review(CommitteeReviewState state)
        => new(
            ReviewId, TraineeId, 20, "Paed Annual Review Panel", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31),
            new DateOnly(2026, 10, 13), state, Utc(9), "zulu", Utc(12), "zulu", null,
            [Decision(41, CommitteeDecisionCategory.InadequateProgressAdditionalTraining, "One DOPS in the year and a Mini-CEX that stalled.",
                attendees: ["zulu:Thandi Zulu:Chair", "botha:Sarah Botha:Member", "rensburg:John van Rensburg:External"])],
            [], [])
        {
            AcademicYear = 2026,
            Semester = 2,
            TraineeName = "Nomsa Mahlangu",
            PanelInstitutionName = "Kgosi Kgari Teaching Hospital",
            AppealBody = [Person("zulu", "Thandi Zulu", DecisionPanelMemberRole.Chair), Person("rensburg", "John van Rensburg", DecisionPanelMemberRole.External)],
            Agenda = Agenda(Line(2, "PAED-002", CommitteeAgendaLineStatus.NotDecided))
        };

    private static CommitteeReviewDetailDto UnderAppeal()
        => Review(CommitteeReviewState.UnderAppeal) with
        {
            Appeals = [new CommitteeAppealDto(9, Utc(14), TraineeId, Reason, null, null, null) { LodgedByName = "Nomsa Mahlangu" }]
        };

    private static CommitteeReviewDetailDto Closed(CommitteeAppealOutcome outcome)
    {
        var review = Review(CommitteeReviewState.Final) with
        {
            AppealBody = [],
            Appeals =
            [
                new CommitteeAppealDto(9, Utc(14), TraineeId, Reason, Utc(16), "zulu", outcome)
                {
                    LodgedByName = "Nomsa Mahlangu",
                    ResolvedByName = "Thandi Zulu"
                }
            ]
        };

        return outcome == CommitteeAppealOutcome.Dismissed
            ? review
            : review with
            {
                Decisions =
                [
                    Decision(42, CommitteeDecisionCategory.SatisfactoryWithObservations,
                        "The stalled Mini-CEX was the assessor's delay.", supersedes: 41,
                        conditions: "Two observed Mini-CEX and one DOPS before the next review.",
                        attendees: ["zulu:Thandi Zulu:Chair", "botha:Sarah Botha:Member"]),
                    review.Decisions[0]
                ]
            };
    }

    /// <summary>As the command's lookup-free mapper answers: nobody named.</summary>
    private static CommitteeReviewDetailDto Unnamed(CommitteeReviewDetailDto review)
        => review with
        {
            TraineeName = null,
            PanelInstitutionName = null,
            AppealBody = [],
            Agenda = null,
            Appeals = review.Appeals.Select(appeal => appeal with { LodgedByName = null }).ToArray(),
            Decisions = review.Decisions
                .Select(decision => decision with
                {
                    Attendees = decision.Attendees.Select(person => person with { Name = null }).ToArray()
                })
                .ToArray()
        };

    /// <param name="attendees">"id:Name:Role" each.</param>
    private static CommitteeDecisionDto Decision(
        int id,
        CommitteeDecisionCategory? category,
        string rationale,
        int? supersedes = null,
        string? conditions = null,
        string[]? attendees = null)
        => new(id, category, rationale, conditions, Utc(id - 30), "zulu", supersedes)
        {
            UndecidedNote = "Not enough observed evidence yet this year.",
            Attendees = (attendees ?? ["zulu:Thandi Zulu:Chair", "botha:Sarah Botha:Member"])
                .Select(attendee => attendee.Split(':'))
                .Select(parts => Person(parts[0], parts[1], Enum.Parse<DecisionPanelMemberRole>(parts[2])))
                .ToArray()
        };

    private static CommitteePersonDto Person(string userId, string name, DecisionPanelMemberRole role)
        => new(userId, role) { Name = name };

    private static CommitteeAgendaDto Agenda(params CommitteeAgendaLineDto[] lines)
        => new(ReviewId, 2026, 2, "2026 S2", false, lines, []);

    private static CommitteeAgendaLineDto Line(int epaId, string code, CommitteeAgendaLineStatus status, int? starId = null, int? semester = 2)
        => new(
            100 + epaId, epaId, code, $"EPA {epaId}", CommitteeAgendaLineOrigin.Cadence, 2026, semester,
            semester is int s ? $"2026 S{s}" : "2026", true, false,
            status == CommitteeAgendaLineStatus.Decided ? CommitteeAgendaLineState.Decided : CommitteeAgendaLineState.NotDecided,
            status, false, status == CommitteeAgendaLineStatus.NotDecided, starId, 0);

    // 2026-10-13 at the given hour, UTC: the South African day is the same.
    private static DateTime Utc(int hour) => new DateTime(2026, 10, 13, 0, 0, 0, DateTimeKind.Utc).AddHours(Math.Clamp(hour, 0, 21));

    private static string Text(AngleSharp.Dom.IElement element) => Whitespace().Replace(element.TextContent, " ").Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    /// <summary>Answers what a test registers, records every request, and refuses anything unregistered.</summary>
    private sealed class RecordingSender : IScopedSender
    {
        private readonly Dictionary<Type, Func<object, Task<object?>>> _answers = [];

        public List<object> Received { get; } = [];

        public RecordingSender On<TRequest>(Func<TRequest, object?> answer)
        {
            _answers[typeof(TRequest)] = request => Task.FromResult(answer((TRequest)request));
            return this;
        }

        public RecordingSender OnAsync<TRequest>(Func<TRequest, Task<CommitteeReviewDetailDto?>> answer)
        {
            _answers[typeof(TRequest)] = async request => await answer((TRequest)request);
            return this;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Received.Add(request);
            return _answers.TryGetValue(request.GetType(), out var answer)
                ? (TResponse)(await answer(request))!
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
