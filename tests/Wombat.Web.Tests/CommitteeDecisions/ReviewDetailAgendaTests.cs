using System.Security.Claims;
using System.Text.RegularExpressions;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;
using Wombat.Web.Tests.Design;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// The review page's agenda (T131 slice 4): above the staging form, each line with its window, a state badge, the
/// evidence about it and its action; Ratify disabled with the closing lines still outstanding named (T107); what another
/// panel still owes, read-only, and a warning on the progression decision while it does (O8).
/// </summary>
public sealed partial class ReviewDetailAgendaTests : TestContext
{
    private readonly RecordingSender _sender = new();

    public ReviewDetailAgendaTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("chair@test");
        auth.SetRoles(WombatRoles.CommitteeMember);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "chair-1"));
        Services.AddSingleton<IScopedSender>(_sender);
    }

    [Fact]
    public void TheAgenda_ListsEachLine_WithItsWindow_ItsStateBadge_AndTheEvidenceAboutIt()
    {
        var cut = Render(CommitteeReviewState.InProgress, Agenda(
            Due(1, "PAED-001"),
            Staged(2, "PAED-002"),
            Line(3, "PAED-003", CommitteeAgendaLineStatus.DueByYearEnd, CommitteeAgendaLineState.Due, window: (2026, null)),
            Line(4, "PAED-004", CommitteeAgendaLineStatus.Deferred, CommitteeAgendaLineState.Deferred, reason: "Rotation moved.")));

        var rows = cut.FindAll("#agenda-heading ~ .table-container tbody tr");
        rows.Should().HaveCount(4);
        Row(cut, 1).QuerySelector(".badge")!.ClassList.Should().Contain("badge-draft");
        Text(Row(cut, 1)).Should().Contain("PAED-001 — EPA 1").And.Contain("2026 S1").And.Contain("Due")
            .And.Contain("Must be decided at this sitting, or deferred with a reason.").And.Contain("2 items");
        Row(cut, 2).QuerySelector(".badge")!.ClassList.Should().Contain("badge-submitted");
        Text(Row(cut, 3)).Should().Contain("2026").And.Contain("Due by year end");
        Row(cut, 4).QuerySelector(".badge")!.ClassList.Should().Contain("badge-accepted");
        Text(Row(cut, 4)).Should().Contain("Reason: Rotation moved.");

        var caption = cut.Find("#agenda-heading ~ .table-container caption").TextContent;
        caption.Should().Be("4 EPAs for 2026 S1, 1 still to stage or defer");

        // The card sits above the staging form.
        var cards = cut.FindAll("section.detail-card h3").Select(heading => heading.TextContent.Trim()).ToList();
        cards.IndexOf("Agenda").Should().BeLessThan(cards.IndexOf("Pending entrustment decisions"));
    }

    [Fact]
    public void EachLine_OffersWhatItsStateAllows()
    {
        var cut = Render(CommitteeReviewState.InProgress, Agenda(
            Due(1, "PAED-001"),
            Staged(2, "PAED-002"),
            Line(4, "PAED-004", CommitteeAgendaLineStatus.Deferred, CommitteeAgendaLineState.Deferred, reason: "Later.")));

        Buttons(Row(cut, 1)).Should().Equal("Stage", "Defer");
        Buttons(Row(cut, 2)).Should().BeEmpty("a staged line is removed from the staged list below, not here");
        Buttons(Row(cut, 4)).Should().Equal("Reinstate");
    }

    [Fact]
    public void Stage_ChoosesTheLinesEpaInTheStagingForm()
    {
        var cut = Render(CommitteeReviewState.InProgress, Agenda(Due(1, "PAED-001"), Due(2, "PAED-002")));

        ButtonIn(Row(cut, 2), "Stage").Click();

        cut.Find("#pending-epa").GetAttribute("value").Should().Be("2");
    }

    [Fact]
    public void Defer_AsksWhy_SendsTheReason_AndReadsTheAgendaAgain()
    {
        var cut = Render(CommitteeReviewState.InProgress, Agenda(Due(1, "PAED-001")));
        _sender.On<DeferAgendaLineCommand>(_ => Unit.Value)
            .On<GetCommitteeAgendaQuery>(_ => Agenda(Line(1, "PAED-001", CommitteeAgendaLineStatus.Deferred, CommitteeAgendaLineState.Deferred, reason: "Not observed.")));

        ButtonIn(Row(cut, 1), "Defer").Click();
        // The deferral's own fieldset: the decision form above it has one too, for who was present (T165).
        cut.Find("#deferral-reason").Closest("fieldset")!.QuerySelector("legend")!.TextContent.Should().Contain("Defer PAED-001");
        cut.Find("#deferral-reason").Change("Not observed.");
        cut.Find("#deferral-reason").Closest("form")!.Submit();

        cut.WaitForState(() => cut.Markup.Contains("PAED-001 deferred."));
        var command = _sender.Received.OfType<DeferAgendaLineCommand>().Should().ContainSingle().Subject;
        command.LineId.Should().Be(101);
        command.Reason.Should().Be("Not observed.");
        _sender.Received.OfType<GetCommitteeAgendaQuery>().Should().ContainSingle();
        Text(Row(cut, 1)).Should().Contain("Deferred").And.Contain("Reason: Not observed.");
        cut.FindAll("#deferral-reason").Should().BeEmpty("the form closes once the line is deferred");
    }

    [Fact]
    public void Defer_MovesTheFocusToTheReason_AsStageMovesItToTheEpa()
    {
        var cut = Render(CommitteeReviewState.InProgress, Agenda(Due(1, "PAED-001")));

        ButtonIn(Row(cut, 1), "Defer").Click();

        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<Microsoft.AspNetCore.Components.ElementReference>()
            .Which.ShouldBeElementReferenceTo(cut.Find("#deferral-reason")));
    }

    [Fact]
    public void Reinstate_SendsTheCommand()
    {
        var cut = Render(CommitteeReviewState.InProgress, Agenda(
            Line(4, "PAED-004", CommitteeAgendaLineStatus.Deferred, CommitteeAgendaLineState.Deferred, reason: "Later.")));
        _sender.On<ReinstateAgendaLineCommand>(_ => Unit.Value).On<GetCommitteeAgendaQuery>(_ => Agenda(Due(4, "PAED-004")));

        ButtonIn(Row(cut, 4), "Reinstate").Click();

        cut.WaitForState(() => cut.Markup.Contains("PAED-004 reinstated"));
        _sender.Received.OfType<ReinstateAgendaLineCommand>().Single().LineId.Should().Be(104);
    }

    [Fact]
    public void OnceTheDecisionIsRecorded_OnlyALineThatBlocksRatify_OffersAnything_AndThatIsDefer()
    {
        // The agenda is fixed with the recorded decision (T165). The one line still open is a closing line whose staged
        // decision was removed afterwards because it no longer fits the curriculum: deferring it is the way through.
        var cut = Render(CommitteeReviewState.Decided, Agenda(
            Due(1, "PAED-001"),
            Staged(2, "PAED-002"),
            Line(3, "PAED-003", CommitteeAgendaLineStatus.DueByYearEnd, CommitteeAgendaLineState.Due, window: (2026, null)),
            Line(4, "PAED-004", CommitteeAgendaLineStatus.Deferred, CommitteeAgendaLineState.Deferred, reason: "Later.")));

        Buttons(Row(cut, 1)).Should().Equal("Defer");
        Buttons(Row(cut, 2)).Should().BeEmpty();
        Buttons(Row(cut, 3)).Should().BeEmpty("an optional line is not deferred after the decision is recorded");
        Buttons(Row(cut, 4)).Should().BeEmpty("a deferral is not taken back after the decision is recorded");
    }

    [Fact]
    public void Record_IsDisabled_WhileAClosingLineIsNeitherStagedNorDeferred_AndSaysWhich()
    {
        // The decision fixes the agenda (T165), so it is recorded only once every closing line is staged or deferred.
        var cut = Render(CommitteeReviewState.InProgress, Agenda(Due(1, "PAED-001"), Staged(2, "PAED-002")), seatsAQuorum: true);

        var record = RecordButton(cut);
        record.HasAttribute("disabled").Should().BeTrue();
        record.GetAttribute("aria-describedby").Should().Be("record-reason");
        Text(cut.Find("#record-reason")).Should().Be(
            "Record decision: PAED-001 must be decided at this sitting. Stage a decision on it, or defer it with a reason.");
    }

    [Fact]
    public void Record_IsOffered_OnceEveryClosingLineIsStagedOrDeferred()
    {
        var cut = Render(CommitteeReviewState.InProgress, Agenda(
            Staged(1, "PAED-001"),
            Line(3, "PAED-003", CommitteeAgendaLineStatus.DueByYearEnd, CommitteeAgendaLineState.Due, window: (2026, null))),
            seatsAQuorum: true);

        var record = RecordButton(cut);
        record.HasAttribute("disabled").Should().BeFalse();
        record.GetAttribute("type").Should().Be("submit");
        cut.FindAll("#record-reason").Should().BeEmpty();
    }

    [Fact]
    public void Ratify_IsDisabled_WhileAClosingLineIsNeitherStagedNorDeferred_AndSaysWhich()
    {
        var cut = Render(CommitteeReviewState.Decided, Agenda(Due(1, "PAED-001"), Due(5, "PAED-005"), Staged(2, "PAED-002")));

        var ratify = RatifyButton(cut);
        ratify.HasAttribute("disabled").Should().BeTrue();
        ratify.GetAttribute("aria-describedby").Should().Be("ratify-reason");
        cut.Find("#ratify-reason").TextContent.Should().Be(
            "Ratify: PAED-001 and PAED-005 must be decided at this sitting. Stage a decision on each, or defer it with a reason.");
    }

    [Fact]
    public void Ratify_IsEnabled_OnceEveryClosingLineIsStagedOrDeferred()
    {
        var cut = Render(CommitteeReviewState.Decided, Agenda(
            Staged(1, "PAED-001"),
            Line(3, "PAED-003", CommitteeAgendaLineStatus.DueByYearEnd, CommitteeAgendaLineState.Due, window: (2026, null))));

        RatifyButton(cut).HasAttribute("disabled").Should().BeFalse();
        cut.FindAll("#ratify-reason").Should().BeEmpty();
    }

    [Fact]
    public void WhatAnotherPanelDecides_IsListedReadOnly_AndTheProgressionDecisionWarnsWhileItIsUndecided()
    {
        var agenda = Agenda(Due(1, "PAED-001")) with
        {
            RoutedElsewhere =
            [
                new CommitteeAgendaElsewhereDto(4, "PAED-004", "Newborn", 11, "Neonatal CCC", "2026 S1", CommitteeAgendaElsewhereStatus.NotYetDecided),
                new CommitteeAgendaElsewhereDto(5, "PAED-005", "Sick newborn", 11, "Neonatal CCC", "2026 S1", CommitteeAgendaElsewhereStatus.Missed)
            ]
        };

        var cut = Render(CommitteeReviewState.InProgress, agenda);

        Text(cut.Find("#agenda-elsewhere-4")).Should().Contain("PAED-004 — Newborn").And.Contain("Neonatal CCC · 2026 S1").And.Contain("Not yet decided");
        cut.Find("#agenda-elsewhere-5 .badge").ClassList.Should().Contain("badge-declined");
        cut.Find("#agenda-elsewhere-5 .badge").TextContent.Should().Be("Missed");
        cut.Find("#agenda-elsewhere-4").QuerySelectorAll("button").Should().BeEmpty();
        cut.Find("#sitting-order-warning").TextContent.Should().Be(
            "PAED-004 and PAED-005 are decided by Neonatal CCC and not yet decided for 2026 S1. A progression decision " +
            "recorded now is taken without them.");
    }

    [Fact]
    public void WhatAStarAlreadyDecidedInTheWindow_IsNamedInTheAgendaCard_InThePreviewsWords()
    {
        // T215: the planner left these off because a STAR already decided their window; the card says so, as the
        // scheduling preview did, so the chair is not left wondering where PAED-001 went.
        var cut = Render(CommitteeReviewState.InProgress, Agenda(Due(2, "PAED-002")) with { DecidedInWindow = ["PAED-001", "PAED-006"] });

        var note = cut.Find("#agenda-decided-in-window");
        note.TextContent.Should().Be("Already decided in this window, so not on the agenda: PAED-001 and PAED-006.");
        note.ClassList.Should().Contain("muted");
        note.Closest("section")!.QuerySelector("h3")!.TextContent.Should().Be("Agenda");
        cut.FindAll("#agenda-line-1").Should().BeEmpty();
    }

    [Fact]
    public void AnEpaNoLongerDecided_IsNamedInItsOwnSentence_AboveTheTable_AndTheChairCanStageIt()
    {
        // T235: Start left PAED-002 off because a STAR decided its window, and that STAR was revoked while the review sat.
        // It is not added to the agenda (D46); the card names it so the chair can stage it.
        var cut = Render(
            CommitteeReviewState.InProgress,
            Agenda(Due(1, "PAED-001")) with { NoLongerDecided = [new CommitteeAgendaEpaDto(2, "PAED-002", "EPA 2")] },
            seatsAQuorum: true);

        var note = cut.Find("#agenda-no-longer-decided");
        note.ClassList.Should().Contain("alert-warning");
        note.HasAttribute("role").Should().BeFalse("it stands on the page until acted on, so it is not read out on every load");
        Text(note.QuerySelector("p")!).Should().Be(
            "PAED-002 is no longer decided in its window: the STAR that decided it has been revoked. It is not on this " +
            "agenda; stage a decision on it to decide it at this sitting.");
        note.Closest("section")!.QuerySelector("h3")!.TextContent.Should().Be("Agenda");
        cut.FindAll("#agenda-no-longer-decided ~ .table-container").Should().ContainSingle("it sits above the table");
        cut.FindAll("#agenda-line-2").Should().BeEmpty("it is not on the agenda");
        RecordButton(cut).HasAttribute("disabled").Should().BeTrue("guard: PAED-001 still blocks, and PAED-002 does not add to it");
        Text(cut.Find("#record-reason")).Should().NotContain("PAED-002");

        var stage = note.QuerySelectorAll("button").Should().ContainSingle().Subject;
        Text(stage).Should().Be("Stage PAED-002");
        stage.HasAttribute("aria-label").Should().BeFalse("its text names it, and a label without that text would break label-in-name");

        stage.Click();

        cut.Find("#pending-epa").GetAttribute("value").Should().Be("2", "Stage chooses the EPA in the staging form, as a line's Stage does");
    }

    [Fact]
    public void TwoEpasNoLongerDecided_AreNamedInOneSentence_EachWithItsStageButton()
    {
        var cut = Render(
            CommitteeReviewState.InProgress,
            Agenda(Due(1, "PAED-001")) with
            {
                NoLongerDecided = [new CommitteeAgendaEpaDto(2, "PAED-002", "EPA 2"), new CommitteeAgendaEpaDto(3, "PAED-003", "EPA 3")]
            });

        var note = cut.Find("#agenda-no-longer-decided");
        Text(note.QuerySelector("p")!).Should().Be(
            "PAED-002 and PAED-003 are no longer decided in their windows: the STARs that decided them have been revoked. " +
            "They are not on this agenda; stage a decision on each to decide it at this sitting.");
        Buttons(note).Should().Equal("Stage PAED-002", "Stage PAED-003");
    }

    [Fact]
    public void AnEpaNoLongerDecided_IsNamedToAReaderWhoIsNotTheChair_WithNoButton()
    {
        var cut = Render(
            CommitteeReviewState.InProgress,
            Agenda(Due(1, "PAED-001")) with { NoLongerDecided = [new CommitteeAgendaEpaDto(2, "PAED-002", "EPA 2")] },
            callerChairs: false);

        var note = cut.Find("#agenda-no-longer-decided");
        Text(note).Should().StartWith("PAED-002 is no longer decided in its window");
        note.QuerySelectorAll("button").Should().BeEmpty("staging is the chair's (T213)");
    }

    [Fact]
    public void AnAgendaWithNothingNoLongerDecided_SaysNothingOfIt()
    {
        var cut = Render(CommitteeReviewState.InProgress, Agenda(Due(1, "PAED-001")));

        cut.FindAll("#agenda-no-longer-decided").Should().BeEmpty();
        cut.Markup.Should().NotContain("no longer decided");
    }

    [Fact]
    public void ALineDecidedElsewhere_SaysSo_AndDoesNotHoldBackRecord()
    {
        // T235: PAED-003's window was decided at another sitting after this review planned it. It is optional now: the
        // chair may still stage it, and Record is offered with it neither staged nor deferred.
        var cut = Render(
            CommitteeReviewState.InProgress,
            Agenda(
                Staged(1, "PAED-001"),
                Line(3, "PAED-003", CommitteeAgendaLineStatus.DecidedElsewhere, CommitteeAgendaLineState.Due, window: (2026, null), closing: true)),
            seatsAQuorum: true);

        var badge = Row(cut, 3).QuerySelector(".badge")!;
        badge.TextContent.Should().Be("Decided elsewhere");
        badge.ClassList.Should().Contain("badge-completed");
        Text(Row(cut, 3)).Should().Contain(
            "Another sitting has decided it in this window, so it need not be decided here. A decision staged on it decides it again.");
        Buttons(Row(cut, 3)).Should().Equal("Stage", "Defer");
        cut.Find("#agenda-heading ~ .table-container caption").TextContent.Should().Be("2 EPAs for 2026 S1");
        RecordButton(cut).HasAttribute("disabled").Should().BeFalse();
    }

    [Theory]
    [InlineData(CommitteeReviewState.Scheduled, "")]
    [InlineData(CommitteeReviewState.Decided, " Ratifying the review records that.")]
    public void ALineDecidedElsewhere_OnAReviewNotInProgress_IsNotSaidToTakeAStagedDecision(CommitteeReviewState state, string rest)
    {
        // A decision can be staged only while the review is in progress: before Start staging is refused, and once the
        // decision is recorded the staged decisions are fixed with it (D46). So the line's note offers no staging there; on
        // a decided review, ratify settles the line (T235).
        var cut = Render(
            state,
            Agenda(
                Staged(1, "PAED-001"),
                Line(3, "PAED-003", CommitteeAgendaLineStatus.DecidedElsewhere, CommitteeAgendaLineState.Due, window: (2026, null), closing: true)));

        var detail = Text(Row(cut, 3).QuerySelector("td .muted")!);
        detail.Should().Be("Another sitting has decided it in this window, so it need not be decided here." + rest);
        detail.Should().NotContain("staged");
        Buttons(Row(cut, 3)).Should().BeEmpty();
    }

    [Fact]
    public void ALineSettledAsDecidedElsewhere_SaysWhenItWasSettled_AndOffersNothing()
    {
        var cut = Render(
            CommitteeReviewState.Decided,
            Agenda(
                Staged(1, "PAED-001"),
                Line(3, "PAED-003", CommitteeAgendaLineStatus.DecidedElsewhere, CommitteeAgendaLineState.DecidedElsewhere, window: (2026, null), closing: true)));

        Text(Row(cut, 3)).Should().Contain("Decided elsewhere")
            .And.Contain("Another sitting had decided it in this window when this review settled its agenda.");
        Buttons(Row(cut, 3)).Should().BeEmpty();
        RatifyButton(cut).HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void AnAgendaWithNothingAlreadyDecided_SaysNothingOfIt()
    {
        var cut = Render(CommitteeReviewState.InProgress, Agenda(Due(1, "PAED-001")));

        cut.FindAll("#agenda-decided-in-window").Should().BeEmpty();
        cut.Markup.Should().NotContain("Already decided");
    }

    [Fact]
    public void AReviewWithNothingOnItsAgenda_SaysWhatStagingDoes()
    {
        var cut = Render(CommitteeReviewState.InProgress, Agenda());

        Text(cut.Find("#agenda-heading").ParentElement!).Should().Contain(
            "Nothing is on this review's agenda. A decision can still be staged on any EPA of the trainee's curriculum that routes to this panel, and staging it adds it.");
        RatifyButtonOrNull(cut).Should().BeNull("the review is not decided yet");
    }

    [Fact]
    public void AScheduledReviewWithNothingOnItsAgenda_SaysStartAddsOnlyWhatIsNotAlreadyDecided()
    {
        // T215 review: beside "Already decided in this window", the empty agenda must not promise that Start adds them.
        var cut = Render(CommitteeReviewState.Scheduled, Agenda() with { DecidedInWindow = ["PAED-001", "PAED-002"] });

        var card = Text(cut.Find("#agenda-heading").ParentElement!);
        card.Should().Contain(
                "Nothing is on the agenda yet. Starting the review adds every EPA routed to this panel that is due in its " +
                "period and not already decided in its window.")
            .And.Contain("Already decided in this window, so not on the agenda: PAED-001 and PAED-002.");
    }

    [Fact]
    public void AFormativeReview_ShowsNoAgenda()
    {
        var cut = Render(CommitteeReviewState.InProgress, null, formative: true);

        cut.FindAll("#agenda-heading").Should().BeEmpty();
    }

    [Fact]
    public void Defer_WithNoReason_SaysWhyUnderTheReason_AndSendsNothing()
    {
        // T212: the box was marked invalid and nothing said why. The message's region is there before anything is
        // refused, and the box names it, so the message is read with the box once it appears.
        var cut = Render(CommitteeReviewState.InProgress, Agenda(Due(1, "PAED-001")));
        _sender.On<DeferAgendaLineCommand>(_ => Unit.Value);

        ButtonIn(Row(cut, 1), "Defer").Click();
        var reason = cut.Find("#deferral-reason");
        reason.GetAttribute("aria-describedby").Should().Be("deferral-reason-help deferral-reason-message");
        Text(cut.Find("#deferral-reason-message")).Should().BeEmpty("nothing is refused before a submit");
        InvalidFieldStyleTests.ShowsInvalid(reason).Should().BeFalse("nothing is refused before a submit");
        // Opening the form moves the focus to the reason (Defer_MovesTheFocusToTheReason).
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<Microsoft.AspNetCore.Components.ElementReference>()
            .Which.ShouldBeElementReferenceTo(cut.Find("#deferral-reason")));
        var reasonReference = (Microsoft.AspNetCore.Components.ElementReference)JSInterop.VerifyFocusAsyncInvoke().Arguments[0]!;

        reason.Closest("form")!.Submit();

        var message = cut.Find("#deferral-reason-message .validation-message");
        message.TextContent.Should().Be("Say why the committee is deferring the decision.");
        cut.Find("#deferral-reason").GetAttribute("aria-invalid").Should().Be("true");
        // T236: and the box itself shows it, not only the message under it. Blazor marks it; app.css styles the mark.
        InvalidFieldStyleTests.ShowsInvalid(cut.Find("#deferral-reason")).Should().BeTrue();
        _sender.Received.OfType<DeferAgendaLineCommand>().Should().BeEmpty();

        // The refused submit moves the focus back to the reason. Compared by the reference's id: once the box re-renders
        // with its message, bUnit's markup no longer carries the reference to compare with.
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke(calledTimes: 2)[^1].Arguments[0]
            .Should().BeOfType<Microsoft.AspNetCore.Components.ElementReference>()
            .Which.Id.Should().Be(reasonReference.Id));
    }

    [Fact]
    public void Defer_WithATooLongReason_SaysSoInTheDomainsWords_NotTheFrameworks()
    {
        // The T212 review: the message region shows whatever the model's attributes say, and [StringLength] alone says
        // "The field Reason must be a string with a maximum length of 2000."
        var cut = Render(CommitteeReviewState.InProgress, Agenda(Due(1, "PAED-001")));
        _sender.On<DeferAgendaLineCommand>(_ => Unit.Value);

        ButtonIn(Row(cut, 1), "Defer").Click();
        cut.Find("#deferral-reason").Change(new string('x', 2001));
        cut.Find("#deferral-reason").Closest("form")!.Submit();

        cut.Find("#deferral-reason-message .validation-message").TextContent
            .Should().Be("A deferral reason is at most 2000 characters.");
        _sender.Received.OfType<DeferAgendaLineCommand>().Should().BeEmpty();
    }

    [Fact]
    public void Ratify_WithNothingStaged_SaysItIssuedNoStar_NotThatItIssuedWhatWasStaged()
    {
        // The T212 review: ratify allows a review whose lines were all optional or deferred, with nothing staged. The note
        // said "ratifying the review issued what was staged as STARs" of it, beside an agenda showing none decided.
        var cut = Render(
            CommitteeReviewState.Decided,
            Agenda(
                Line(1, "PAED-001", CommitteeAgendaLineStatus.AsOpportunityAllows, CommitteeAgendaLineState.Due),
                Line(2, "PAED-002", CommitteeAgendaLineStatus.Deferred, CommitteeAgendaLineState.Deferred, closing: true, reason: "Later.")));
        _sender
            .On<RatifyCommitteeDecisionCommand>(_ => Review(CommitteeReviewState.Ratified, agenda: null))
            .On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => Array.Empty<PendingEntrustmentDecisionDto>())
            .On<GetCommitteeAgendaQuery>(_ => Agenda(
                Line(1, "PAED-001", CommitteeAgendaLineStatus.NotDecided, CommitteeAgendaLineState.NotDecided),
                Line(2, "PAED-002", CommitteeAgendaLineStatus.Deferred, CommitteeAgendaLineState.Deferred, closing: true, reason: "Later.")));

        RatifyButton(cut).Click();

        cut.WaitForState(() => cut.Markup.Contains("Decision ratified."));
        cut.Find("#no-pending-note").TextContent.Should().Be(
            "Nothing was staged when the review was ratified, so ratifying it issued no STAR.");
    }

    [Fact]
    public void ARatifiedReview_WithADecidedLine_SaysRatifyingIssuedWhatWasStaged_OnLoad()
    {
        var cut = Render(
            CommitteeReviewState.Final,
            Agenda(
                Line(1, "PAED-001", CommitteeAgendaLineStatus.Decided, CommitteeAgendaLineState.Decided),
                Line(2, "PAED-002", CommitteeAgendaLineStatus.NotDecided, CommitteeAgendaLineState.NotDecided)));

        cut.Find("#no-pending-note").TextContent.Should().Be(
            "Nothing is pending: ratifying the review issued what was staged as STARs, and the agenda names each one.");
    }

    [Fact]
    public void Ratify_WhenTheAgendaCannotBeReadAgain_CountsTheLinesStagedInTheAgendaLastRead_AsIssued()
    {
        // The page still holds the agenda read before Ratify, whose lines are Staged, not Decided. Ratifying issued each.
        var cut = Render(
            CommitteeReviewState.Decided,
            Agenda(Staged(1, "PAED-001")),
            pending: [Pending(90, 1, "PAED-001")]);
        _sender
            .On<RatifyCommitteeDecisionCommand>(_ => Review(CommitteeReviewState.Ratified, agenda: null))
            .On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => Array.Empty<PendingEntrustmentDecisionDto>())
            .On<GetCommitteeAgendaQuery>(_ => throw new InvalidOperationException("The database is unavailable."));

        RatifyButton(cut).Click();

        cut.WaitForState(() => cut.Markup.Contains("Decision ratified."));
        cut.Markup.Should().Contain("The agenda could not be read again", "guard: the agenda on the page is the one read before");
        cut.Find("#no-pending-note").TextContent.Should().Be(
            "Nothing is pending: ratifying the review issued what was staged as STARs, and the agenda names each one.");
    }

    [Fact]
    public void ARatifiedReview_WithNoAgenda_SaysOnlyThatItIsRatified()
    {
        // Not reached in production (the review query always reads the agenda), but a note with no agenda cannot say
        // whether ratifying issued anything.
        var cut = Render(CommitteeReviewState.Ratified, agenda: null);

        cut.Find("#no-pending-note").TextContent.Should().Be("Nothing is pending: the review is ratified.");
    }

    [Fact]
    public void Ratify_ReadsTheStagedDecisionsAgain_SoTheOnesItIssuedAreNoLongerListedAsPending()
    {
        // T212: ratifying issues the staged decisions as STARs and removes them, but the page read the list only when it
        // loaded, so it went on listing them as pending until a reload.
        var cut = Render(
            CommitteeReviewState.Decided,
            Agenda(Staged(1, "PAED-001"), Staged(2, "PAED-002")),
            pending: [Pending(90, 1, "PAED-001"), Pending(91, 2, "PAED-002")]);
        PendingItems(cut).Should().Equal(new[] { "PAED-001 — EPA 1", "PAED-002 — EPA 2" }, "guard: the page loads with two staged");
        _sender
            .On<RatifyCommitteeDecisionCommand>(_ => Review(CommitteeReviewState.Ratified, agenda: null))
            .On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => Array.Empty<PendingEntrustmentDecisionDto>())
            .On<GetCommitteeAgendaQuery>(_ => Agenda(
                Line(1, "PAED-001", CommitteeAgendaLineStatus.Decided, CommitteeAgendaLineState.Decided),
                Line(2, "PAED-002", CommitteeAgendaLineStatus.Decided, CommitteeAgendaLineState.Decided)));

        RatifyButton(cut).Click();

        cut.WaitForState(() => cut.Markup.Contains("Decision ratified."));
        PendingItems(cut).Should().BeEmpty();
        cut.Find("#no-pending-note").TextContent.Should().Be(
            "Nothing is pending: ratifying the review issued what was staged as STARs, and the agenda names each one.");
        _sender.Received.OfType<ListPendingEntrustmentDecisionsForReviewQuery>().Should().HaveCount(2, "once on load, once after Ratify");
    }

    [Fact]
    public void AFailedReadOfTheStagedDecisionsAfterRatify_IsAWarningInTheCard_BesideTheSuccess()
    {
        var cut = Render(
            CommitteeReviewState.Decided,
            Agenda(Staged(1, "PAED-001")),
            pending: [Pending(90, 1, "PAED-001")]);
        _sender
            .On<RatifyCommitteeDecisionCommand>(_ => Review(CommitteeReviewState.Ratified, agenda: null))
            .On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => throw new InvalidOperationException("The database is unavailable."))
            .On<GetCommitteeAgendaQuery>(_ => Agenda(Line(1, "PAED-001", CommitteeAgendaLineStatus.Decided, CommitteeAgendaLineState.Decided)));

        RatifyButton(cut).Click();

        cut.WaitForState(() => cut.Markup.Contains("Decision ratified."));
        var warning = PendingCard(cut).QuerySelector(".alert")!;
        warning.ClassList.Should().Contain("alert-warning");
        Text(warning).Should().Contain("The staged decisions could not be read again: The database is unavailable.");
        cut.FindAll(".alert-danger").Should().NotContain(alert => alert.TextContent.Contains("database is unavailable"));
        PendingItems(cut).Should().Equal(new[] { "PAED-001 — EPA 1" }, "the list last read stays in view below the warning");
    }

    [Fact]
    public void Remove_ReadsTheStagedDecisionsAgain_AndAFailedReadIsAWarning_NotTheRemovesFailure()
    {
        // T212 reads the list after Remove outside Remove's own try, as after Ratify: the decision is removed either way.
        var cut = Render(
            CommitteeReviewState.InProgress,
            Agenda(Staged(1, "PAED-001"), Staged(2, "PAED-002")),
            pending: [Pending(90, 1, "PAED-001"), Pending(91, 2, "PAED-002")]);
        _sender
            .On<RemovePendingEntrustmentDecisionCommand>(_ => Unit.Value)
            .On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => new[] { Pending(91, 2, "PAED-002") })
            .On<GetCommitteeAgendaQuery>(_ => Agenda(Due(1, "PAED-001"), Staged(2, "PAED-002")));

        RemoveButtons(cut)[0].Click();

        cut.WaitForState(() => cut.Markup.Contains("Pending entrustment decision removed."));
        _sender.Received.OfType<RemovePendingEntrustmentDecisionCommand>().Single().PendingId.Should().Be(90);
        PendingItems(cut).Should().Equal(new[] { "PAED-002 — EPA 2" });

        _sender.On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => throw new InvalidOperationException("The database is unavailable."));

        RemoveButtons(cut)[0].Click();

        cut.WaitForState(() => cut.Markup.Contains("The staged decisions could not be read again"));
        cut.Markup.Should().Contain("Pending entrustment decision removed.", "the remove succeeded; only the read after it failed");
        PendingCard(cut).QuerySelector(".alert")!.ClassList.Should().Contain("alert-warning");

        // The next read that succeeds clears the warning (the T212 review found nothing held it).
        _sender.On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => Array.Empty<PendingEntrustmentDecisionDto>());

        RemoveButtons(cut)[0].Click();

        cut.WaitForAssertion(() => PendingItems(cut).Should().BeEmpty());
        PendingCard(cut).QuerySelector(".alert").Should().BeNull("the list was read again, so the warning no longer stands");
    }

    [Fact]
    public void AWarningAboutTheStagedDecisions_DoesNotOutliveAReloadOfTheReview()
    {
        // The T212 review: LoadAsync reset the agenda's warning but not this one, so it survived a change of ReviewId.
        var cut = Render(
            CommitteeReviewState.InProgress,
            Agenda(Staged(1, "PAED-001")),
            pending: [Pending(90, 1, "PAED-001")]);
        _sender
            .On<RemovePendingEntrustmentDecisionCommand>(_ => Unit.Value)
            .On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => throw new InvalidOperationException("The database is unavailable."))
            .On<GetCommitteeAgendaQuery>(_ => Agenda(Due(1, "PAED-001")));
        RemoveButtons(cut)[0].Click();
        cut.WaitForState(() => cut.Markup.Contains("The staged decisions could not be read again"));

        _sender.On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => Array.Empty<PendingEntrustmentDecisionDto>());
        cut.SetParametersAndRender(parameters => parameters.Add(page => page.ReviewId, 31));

        cut.WaitForAssertion(() => _sender.Received.OfType<GetCommitteeReviewByIdQuery>().Should().HaveCount(2));
        cut.WaitForAssertion(() => PendingCard(cut).QuerySelector(".alert").Should().BeNull());
    }

    [Fact]
    public void BeforeRatify_AnEmptyStagedList_SaysNothingHasBeenStaged()
    {
        var cut = Render(CommitteeReviewState.InProgress, Agenda(Due(1, "PAED-001")));

        cut.Find("#no-pending-note").TextContent.Should().Be("No pending entrustment decisions have been staged for this review.");
    }

    // ---- Fixture ------------------------------------------------------------------------------------------------------

    /// <param name="seatsAQuorum">
    /// The panel's chair and one member may both sit (T165), so the panel is no reason to refuse Record; otherwise the
    /// review names no panel member.
    /// </param>
    /// <param name="pending">The staged decisions the page loads with; none unless given.</param>
    /// <param name="callerChairs">Whether the reader holds the panel's Chair seat (T213); these tests act as the chair unless told otherwise.</param>
    private IRenderedComponent<ReviewDetail> Render(
        CommitteeReviewState state,
        CommitteeAgendaDto? agenda,
        bool formative = false,
        bool seatsAQuorum = false,
        IReadOnlyList<PendingEntrustmentDecisionDto>? pending = null,
        bool callerChairs = true)
    {
        var review = Review(state, agenda, formative, seatsAQuorum) with { CallerChairs = callerChairs };

        _sender
            .On<GetCommitteeReviewByIdQuery>(_ => review)
            .On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => pending ?? Array.Empty<PendingEntrustmentDecisionDto>())
            .On<GetSamplingConcentrationWarningsQuery>(_ => null)
            .On<CountMsfCampaignsOutsideSnapshotQuery>(_ => MsfCampaignsOutsideSnapshotDto.None)
            .On<GetEpaTrajectoryForTraineeQuery>(_ => Array.Empty<EpaTrajectoryDto>())
            .On<ListStarEpaOptionsForReviewQuery>(_ => new[]
            {
                new StarEpaOptionDto(1, "PAED-001", "EPA 1", null, null),
                new StarEpaOptionDto(2, "PAED-002", "EPA 2", null, null)
            })
            .On<GetEntrustmentScalesListQuery>(_ => Array.Empty<EntrustmentScaleDto>())
            .On<GetMsfCoverageForTraineeQuery>(_ => null);

        var cut = RenderComponent<ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, 30));
        cut.WaitForState(() => cut.Markup.Contains("Committee review") && cut.Markup.Contains("Evidence snapshot"));
        return cut;
    }

    /// <summary>
    /// A review with this state and agenda, as the review query (or a command's answer) gives it. It names the trainee,
    /// so a command's answer, which the page names from the review it loaded, reads the same (T142).
    /// </summary>
    private static CommitteeReviewDetailDto Review(
        CommitteeReviewState state, CommitteeAgendaDto? agenda, bool formative = false, bool seatsAQuorum = false)
    {
        var review = new CommitteeReviewDetailDto(
            30, "trainee-1", 20, "General CCC", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), new DateOnly(2026, 7, 2),
            state, new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc), "chair-1", null, null, null, [], [],
            [
                Evidence(501, 1), Evidence(502, 1), Evidence(503, 2)
            ],
            IsFormative: formative)
        {
            AcademicYear = 2026,
            Semester = 1,
            // The page offers the chair's controls by what the query says the caller may do (T213); these tests act as the
            // chair.
            CallerChairs = true,
            CallerResolvesAppeals = true,
            Agenda = agenda,
            PanelMembers = seatsAQuorum
                ?
                [
                    new CommitteePersonDto("chair-1", DecisionPanelMemberRole.Chair) { MaySit = true },
                    new CommitteePersonDto("member-1", DecisionPanelMemberRole.Member) { MaySit = true }
                ]
                : []
        };

        return review;
    }

    private static PendingEntrustmentDecisionDto Pending(int id, int epaId, string code)
        => new(
            id, 30, epaId, code, $"EPA {epaId}", 13, "3a", new DateOnly(2026, 7, 2), null, "Consistent across the window.",
            [501, 502], new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc), "chair-1");

    private static IReadOnlyList<string> PendingItems(IRenderedComponent<ReviewDetail> cut)
        => PendingCard(cut).QuerySelectorAll("ul.list-unstyled > li strong").Select(item => item.TextContent.Trim()).ToArray();

    private static IReadOnlyList<AngleSharp.Dom.IElement> RemoveButtons(IRenderedComponent<ReviewDetail> cut)
        => PendingCard(cut).QuerySelectorAll("button").Where(button => button.TextContent.Trim() == "Remove").ToArray();

    private static AngleSharp.Dom.IElement PendingCard(IRenderedComponent<ReviewDetail> cut)
        => cut.FindAll("section.detail-card").Single(card => card.QuerySelector("h3")?.TextContent.Trim() == "Pending entrustment decisions");

    private static CommitteeAgendaDto Agenda(params CommitteeAgendaLineDto[] lines)
        => new(30, 2026, 1, "2026 S1", false, lines, []);

    private static CommitteeAgendaLineDto Due(int epaId, string code)
        => Line(epaId, code, CommitteeAgendaLineStatus.Due, CommitteeAgendaLineState.Due, closing: true, blocks: true);

    private static CommitteeAgendaLineDto Staged(int epaId, string code)
        => Line(epaId, code, CommitteeAgendaLineStatus.Staged, CommitteeAgendaLineState.Due, closing: true, staged: true);

    private static CommitteeAgendaLineDto Line(
        int epaId,
        string code,
        CommitteeAgendaLineStatus status,
        CommitteeAgendaLineState state,
        (int Year, int? Semester)? window = null,
        bool closing = false,
        bool staged = false,
        bool blocks = false,
        string? reason = null)
    {
        var (year, semester) = window ?? (2026, 1);
        return new CommitteeAgendaLineDto(
            100 + epaId, epaId, code, $"EPA {epaId}", CommitteeAgendaLineOrigin.Cadence, year, semester,
            semester is int s ? $"{year} S{s}" : $"{year}", closing, false, state, status, staged, blocks, reason, null,
            epaId == 1 ? 2 : epaId == 2 ? 1 : 0);
    }

    private static CommitteeEvidenceDto Evidence(int id, int epaId)
        => new(
            id, CommitteeEvidenceSourceType.Activity, id, null, null, $"Mini-CEX #{id}", "State: completed.",
            new DateTime(2026, 2, 10, 9, 0, 0, DateTimeKind.Utc),
            EpaId: epaId, EpaCode: $"PAED-00{epaId}", EpaTitle: $"EPA {epaId}", InstrumentName: "Mini-CEX",
            ObservedOn: new DateOnly(2026, 2, 10), ObservedOnDeclared: true, SourceState: "completed", SourceFinished: true,
            SourceStateLabel: "Completed");

    private static AngleSharp.Dom.IElement Row(IRenderedComponent<ReviewDetail> cut, int epaId) => cut.Find($"#agenda-line-{epaId}");

    private static IReadOnlyList<string> Buttons(AngleSharp.Dom.IElement row)
        => row.QuerySelectorAll("button").Select(button => button.TextContent.Trim()).ToArray();

    private static AngleSharp.Dom.IElement ButtonIn(AngleSharp.Dom.IElement row, string label)
        => row.QuerySelectorAll("button").Single(button => button.TextContent.Trim() == label);

    private static AngleSharp.Dom.IElement RecordButton(IRenderedComponent<ReviewDetail> cut)
        => cut.FindAll("button").Single(button => button.TextContent.Trim() == "Record decision");

    private static AngleSharp.Dom.IElement RatifyButton(IRenderedComponent<ReviewDetail> cut)
        => RatifyButtonOrNull(cut) ?? throw new InvalidOperationException("No Ratify button.");

    private static AngleSharp.Dom.IElement? RatifyButtonOrNull(IRenderedComponent<ReviewDetail> cut)
        => cut.FindAll("button").SingleOrDefault(button => button.TextContent.Trim() == "Ratify");

    private static string Text(AngleSharp.Dom.IElement element) => Whitespace().Replace(element.TextContent, " ").Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    /// <summary>Answers what a test registers, records every request, and refuses anything unregistered.</summary>
    private sealed class RecordingSender : IScopedSender
    {
        private readonly Dictionary<Type, Func<object, object?>> _answers = [];

        public List<object> Received { get; } = [];

        public RecordingSender On<TRequest>(Func<TRequest, object?> answer)
        {
            _answers[typeof(TRequest)] = request => answer((TRequest)request);
            return this;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => Task.FromResult((TResponse)Answer(request)!);

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            Answer(request);
            return Task.CompletedTask;
        }

        private object? Answer(object request)
        {
            Received.Add(request);

            return _answers.TryGetValue(request.GetType(), out var answer)
                ? answer(request)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
        }
    }
}
