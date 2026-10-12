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
using Wombat.Web.Components.Pages.CommitteeDecisions.Review;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// The review page's agenda (T131 slice 4): each line with its window, a state badge, the evidence about it and its
/// action; Record and Ratify held back by no line (T361, D53); what another panel still owes, read-only, and a warning on
/// the progression decision while it does (O8). Since T361 (round 3) the agenda is the page's spine: a staged decision is
/// shown in its own line with its Remove… (items 17, 30), a line's Stage opens it in place (C2), and what the agenda says
/// about what was issued sits above its table (the Pending card is gone).
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
        JSInterop.Mode = JSRuntimeMode.Loose; // the dialogs open and close, and results take the focus (T361)
    }

    [Fact]
    public void TheAgenda_ListsEachLine_WithItsWindow_ItsStateBadge_AndTheEvidenceAboutIt()
    {
        var cut = Render(CommitteeReviewState.InProgress, Agenda(
            Due(1, "PAED-001"),
            Staged(2, "PAED-002"),
            Line(3, "PAED-003", CommitteeAgendaLineStatus.DueByYearEnd, CommitteeAgendaLineState.Due, window: (2026, null)),
            Line(4, "PAED-004", CommitteeAgendaLineStatus.AsOpportunityAllows, CommitteeAgendaLineState.Due)));

        AgendaRows(cut).Should().HaveCount(4);
        Row(cut, 1).QuerySelector(".badge")!.ClassList.Should().Contain("badge-draft");
        // T361, D53: a closing line is due at this sitting, and nothing more is asked of it. Round 3: the code over its title.
        Text(Row(cut, 1)).Should().Contain("PAED-001 EPA 1").And.Contain("2026 S1").And.Contain("Due at this sitting")
            .And.Contain("2 items").And.NotContain("defer");
        Row(cut, 2).QuerySelector(".badge")!.ClassList.Should().Contain("badge-submitted");
        Text(Row(cut, 3)).Should().Contain("2026").And.Contain("Due by year end");
        Row(cut, 4).QuerySelector(".badge")!.ClassList.Should().Contain("badge-draft");

        cut.Find("#agenda caption").TextContent.Should().Be("4 EPAs for 2026 S1 · 1 staged");

        // The agenda sits above the decision until the decision is recorded (round 3, § Detail page).
        var sections = cut.FindAll(".review-page > section").Select(section => section.Id).ToList();
        sections.IndexOf("agenda").Should().BeLessThan(sections.IndexOf("decision"));
    }

    [Fact]
    public void EachLine_OffersWhatItsStateAllows()
    {
        // T361 (round 3 items 17, 30): a staged line is removed in its own line, by Remove….
        var cut = Render(CommitteeReviewState.InProgress, Agenda(
            Due(1, "PAED-001"),
            Staged(2, "PAED-002")),
            pending: [Pending(91, 2, "PAED-002")]);

        Buttons(Row(cut, 1)).Should().Equal("Stage");
        Buttons(Row(cut, 2)).Should().Equal("Remove…");
    }

    [Fact]
    public void Stage_OpensTheLineInPlace_WithItsEpasForm()
    {
        // T361 (C2; round 3, 1280 stage frame 1): Stage opens the line, its form in the row under it, the focus on its level.
        var cut = Render(CommitteeReviewState.InProgress, Agenda(Due(1, "PAED-001"), Due(2, "PAED-002")));

        ButtonIn(Row(cut, 2), "Stage").Click();

        cut.Find("#agenda-form-2 h3").TextContent.Should().Be("Stage a decision on PAED-002");
        cut.FindAll("#agenda-form-1").Should().BeEmpty();
        JSInterop.Invocations.Where(call => call.Identifier == "wombat.focusById")
            .Select(call => call.Arguments[0]).Should().Contain("stage-2-level");
    }

    [Theory]
    [InlineData(CommitteeReviewState.Decided)]
    [InlineData(CommitteeReviewState.Ratified)]
    public void AnAgendaWhoseLinesOfferNothing_HasNoActionColumn(CommitteeReviewState state)
    {
        // T226: once the decision is recorded the lines offer nothing, so the chair's Action column was a column of blank
        // cells (reviews 4, 6 and 7 on dev).
        var cut = Render(state, Agenda(
            Line(1, "PAED-001", CommitteeAgendaLineStatus.Decided, CommitteeAgendaLineState.Decided),
            Staged(2, "PAED-002"),
            Line(3, "PAED-003", CommitteeAgendaLineStatus.DueByYearEnd, CommitteeAgendaLineState.Due, window: (2026, null)),
            Line(4, "PAED-004", CommitteeAgendaLineStatus.NotDecided, CommitteeAgendaLineState.Due, closing: true, readsNotDecided: true)));

        AgendaHeaders(cut).Should().Equal("EPA", "Window", "State", "Evidence");
        AgendaRows(cut).Should().HaveCount(4).And.OnlyContain(row => row.Children.Length == 4, "no line has a blank Action cell");
    }

    [Fact]
    public void AWithdrawnReview_SaysWhenAndWhy_ReadsWhatItHeldDueAsNotDecided_AndOffersNothing()
    {
        // T258: an erasure withdraws a review still open. Its page is the record of it: when and why it was withdrawn, and
        // the lines it never decided. Nothing more is decided at it, so no action is offered, the chair included. Since
        // T361 the when and why is the sitting bar's sentence.
        var cut = Render(
            CommitteeReviewState.Withdrawn,
            Agenda(
                Line(1, "PAED-001", CommitteeAgendaLineStatus.NotDecided, CommitteeAgendaLineState.Due, closing: true)),
            withdrawn: (new DateOnly(2026, 9, 25), CommitteeReview.WithdrawnTraineeErased));

        Text(cut.Find("#sitting-sentence")).Should().Be("Withdrawn on 2026-09-25. " + CommitteeReview.WithdrawnTraineeErased);
        Text(Row(cut, 1)).Should().Contain("Not decided at this sitting").And.Contain("The review was withdrawn before deciding it.");
        AgendaHeaders(cut).Should().Equal("EPA", "Window", "State", "Evidence");
        PageButtons(cut).Should().NotContain(["Start review…", "Close review…", "Record decision…", "Ratify…", "Stage", "Remove…"]);
    }

    [Fact]
    public void AnOpenReview_ShowsNoWithdrawal()
    {
        var cut = Render(CommitteeReviewState.InProgress, Agenda(Due(1, "PAED-001")));

        cut.Markup.Should().NotContain("Withdrawn on");
    }

    [Fact]
    public void AnAgendaInProgress_WhoseEveryLineIsStaged_OffersRemoveOnEach()
    {
        // T361 (round 3 items 17, 30): each staged line keeps its Remove…, so the Action column stays.
        var cut = Render(
            CommitteeReviewState.InProgress,
            Agenda(Staged(1, "PAED-001"), Staged(2, "PAED-002")),
            pending: [Pending(90, 1, "PAED-001"), Pending(91, 2, "PAED-002")]);

        AgendaHeaders(cut).Should().Equal("EPA", "Window", "State", "Evidence", "Action");
        Buttons(Row(cut, 1)).Should().Equal("Remove…");
        Buttons(Row(cut, 2)).Should().Equal("Remove…");
    }

    [Fact]
    public void TheActionColumn_StaysWhileAnyLineOffersSomething_AndALineThatOffersNothingSaysWhy()
    {
        var cut = Render(CommitteeReviewState.InProgress, Agenda(
            Due(1, "PAED-001"),
            Line(3, "PAED-003", CommitteeAgendaLineStatus.DecidedElsewhere, CommitteeAgendaLineState.DecidedElsewhere, window: (2026, null))));

        AgendaHeaders(cut).Should().Equal("EPA", "Window", "State", "Evidence", "Action");
        Buttons(Row(cut, 1)).Should().Equal("Stage");
        ActionReason(Row(cut, 1)).Should().BeNull("a line that offers something needs no reason");

        // DESIGN.md § Table system (T211): a row with no action is not left blank; its actions cell says why, muted.
        Row(cut, 3).Children.Should().HaveCount(5);
        Buttons(Row(cut, 3)).Should().BeEmpty();
        ActionReason(Row(cut, 3)).Should().Be("Nothing to do at this sitting");

        // The buttons are a cluster inside the cell: a <td> that is itself a flex box is no longer a table cell, and its
        // border no longer meets its row's.
        Row(cut, 1).QuerySelectorAll("td > .actions-cell > button").Should().ContainSingle();
    }

    [Fact]
    public void InProgress_AStagedLine_SaysItsDecisionInItsOwnLine()
    {
        // T361 (round 3 item 30): the staged decision is said in its line, in the line's words, beside its Remove….
        var cut = Render(
            CommitteeReviewState.InProgress,
            Agenda(Due(1, "PAED-001"), Staged(2, "PAED-002")),
            pending: [Pending(91, 2, "PAED-002")]);

        Text(Row(cut, 2)).Should().Contain(
            "Staged Level 3a · issued 2026-07-02 · no expiry. Rests on 2 items of the snapshot: Mini-CEX #501; Mini-CEX #502.");
        Buttons(Row(cut, 2)).Should().Equal("Remove…");
    }

    [Fact]
    public void Record_IsOffered_WithAClosingLineNothingIsStagedOn()
    {
        // T361, D53: a closing line no longer holds Record back; left unstaged, it is not decided at this sitting.
        var cut = Render(CommitteeReviewState.InProgress, Agenda(Due(1, "PAED-001"), Staged(2, "PAED-002")), seatsAQuorum: true);
        cut.Find("#decision-present-1").Change(true);

        RecordButton(cut).HasAttribute("aria-disabled").Should().BeFalse();
        cut.FindAll("#record-reason").Should().BeEmpty();
    }

    [Fact]
    public void Record_IsOffered_WithEveryClosingLineStaged()
    {
        var cut = Render(CommitteeReviewState.InProgress, Agenda(
            Staged(1, "PAED-001"),
            Line(3, "PAED-003", CommitteeAgendaLineStatus.DueByYearEnd, CommitteeAgendaLineState.Due, window: (2026, null))),
            seatsAQuorum: true);
        cut.Find("#decision-present-1").Change(true);

        RecordButton(cut).HasAttribute("aria-disabled").Should().BeFalse();
        RecordButton(cut).HasAttribute("disabled").Should().BeFalse();
        cut.FindAll("#record-reason").Should().BeEmpty();
    }

    [Fact]
    public void Ratify_IsNotHeldBackByALineNothingIsStagedOn()
    {
        // T361, D53: ratify writes such a line Not decided.
        var cut = Render(CommitteeReviewState.Decided, Agenda(Due(1, "PAED-001"), Due(5, "PAED-005"), Staged(2, "PAED-002")));

        RatifyButton(cut).HasAttribute("aria-disabled").Should().BeFalse();
        cut.FindAll("#ratify-reason").Should().BeEmpty();
    }

    [Fact]
    public void Ratify_IsAvailable_WithEveryClosingLineStaged()
    {
        var cut = Render(CommitteeReviewState.Decided, Agenda(
            Staged(1, "PAED-001"),
            Line(3, "PAED-003", CommitteeAgendaLineStatus.DueByYearEnd, CommitteeAgendaLineState.Due, window: (2026, null))));

        RatifyButton(cut).HasAttribute("aria-disabled").Should().BeFalse();
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
        note.Closest("section")!.QuerySelector("h2")!.TextContent.Should().Be("Agenda");
        cut.FindAll("#agenda-line-1").Should().BeEmpty();
    }

    [Fact]
    public void AnEpaNoLongerDecided_IsNamedInItsOwnSentence_BeforeTheCommitteeDecides_AndTheChairCanStageIt()
    {
        // T235: Start left PAED-002 off because a STAR decided its window, and that STAR was revoked while the review sat.
        // It is not added to the agenda (D46); the page names it so the chair can stage it. Since T361 (round 3) it is the
        // notes card's ("Before the committee decides"), and its Stage chooses the EPA in the agenda's foot form.
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
        note.Closest("section")!.Id.Should().Be("notes");
        cut.FindAll("#agenda-line-2").Should().BeEmpty("it is not on the agenda");
        // T361, D53: no line holds Record back, PAED-001 unstaged included, and PAED-002 adds no reason.
        cut.Find("#decision-present-1").Change(true);
        RecordButton(cut).HasAttribute("aria-disabled").Should().BeFalse();
        cut.FindAll("#record-reason").Should().BeEmpty();

        var stage = cut.Find("#agenda-no-longer-decided").QuerySelectorAll("button").Should().ContainSingle().Subject;
        Text(stage).Should().Be("Stage PAED-002");
        stage.HasAttribute("aria-label").Should().BeFalse("its text names it, and a label without that text would break label-in-name");

        stage.Click();

        cut.Find("#stage-0-epa").GetAttribute("value").Should().Be("2", "Stage chooses the EPA in the foot form, the chair's line");
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
        // chair may still stage it, and Record is offered with it unstaged.
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
        Buttons(Row(cut, 3)).Should().Equal("Stage");
        cut.Find("#agenda caption").TextContent.Should().Be("2 EPAs for 2026 S1 · 1 staged");
        cut.Find("#decision-present-1").Change(true);
        RecordButton(cut).HasAttribute("aria-disabled").Should().BeFalse();
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

        var detail = Text(Row(cut, 3).QuerySelector(".agenda-line-detail")!);
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
        RatifyButton(cut).HasAttribute("aria-disabled").Should().BeFalse();
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

        Text(cut.Find("#agenda")).Should().Contain(
            "Nothing is on this review's agenda. A decision can still be staged on any EPA of the trainee's curriculum that routes to this panel, and staging it adds it.");
        cut.FindAll("#sitting-ratify").Should().BeEmpty("the review is not decided yet");
    }

    [Fact]
    public void AScheduledReviewWithNothingOnItsAgenda_SaysStartAddsOnlyWhatIsNotAlreadyDecided()
    {
        // T215 review: beside "Already decided in this window", the empty agenda must not promise that Start adds them.
        var cut = Render(CommitteeReviewState.Scheduled, Agenda() with { DecidedInWindow = ["PAED-001", "PAED-002"] });

        var card = Text(cut.Find("#agenda"));
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
    public void Ratify_WithNothingStaged_SaysItIssuedNoStar_NotThatItIssuedWhatWasStaged()
    {
        // The T212 review: ratify allows a review with nothing staged (T361, D53: every line is then not decided). The note
        // said "ratifying the review issued what was staged as STARs" of it, beside an agenda showing none decided.
        var cut = Render(
            CommitteeReviewState.Decided,
            Agenda(
                Line(1, "PAED-001", CommitteeAgendaLineStatus.AsOpportunityAllows, CommitteeAgendaLineState.Due),
                Line(2, "PAED-002", CommitteeAgendaLineStatus.NotDecided, CommitteeAgendaLineState.Due, closing: true, readsNotDecided: true)));
        var ratified = Review(CommitteeReviewState.Ratified, Agenda(
            Line(1, "PAED-001", CommitteeAgendaLineStatus.NotDecided, CommitteeAgendaLineState.NotDecided),
            Line(2, "PAED-002", CommitteeAgendaLineStatus.NotDecided, CommitteeAgendaLineState.NotDecided, closing: true, readsNotDecided: true)));
        _sender
            .On<RatifyCommitteeDecisionCommand>(_ => ratified)
            .On<GetCommitteeReviewByIdQuery>(_ => ratified);

        Ratify(cut, "Ratify the review");

        cut.WaitForState(() => cut.Markup.Contains("Decision ratified."));
        cut.Find("#agenda-intro").TextContent.Should().Be(
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

        cut.Find("#agenda-intro").TextContent.Should().Be(
            "Nothing is pending: ratifying the review issued what was staged as STARs, and the agenda names each one.");
    }

    [Fact]
    public void Ratify_WhenTheReviewCannotBeReadAgain_CountsTheLinesStagedInTheAgendaLastRead_AsIssued()
    {
        // The page still holds the agenda read before Ratify, whose lines are Staged, not Decided. Ratifying issued each.
        // Since T361 the page reads the review again after every action (T260); here neither it nor the agenda can be read.
        var cut = Render(
            CommitteeReviewState.Decided,
            Agenda(Staged(1, "PAED-001")),
            pending: [Pending(90, 1, "PAED-001")]);
        _sender
            .On<RatifyCommitteeDecisionCommand>(_ => Review(CommitteeReviewState.Ratified, agenda: null))
            .On<GetCommitteeReviewByIdQuery>(_ => throw new InvalidOperationException("The database is unavailable."))
            .On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => Array.Empty<PendingEntrustmentDecisionDto>())
            .On<GetCommitteeAgendaQuery>(_ => throw new InvalidOperationException("The database is unavailable."));

        Ratify(cut, "Ratify and issue 1 STAR");

        cut.WaitForState(() => cut.Markup.Contains("Decision ratified."));
        cut.Markup.Should().Contain(ReviewWords.AgendaNotRead, "guard: the agenda on the page is the one read before");
        cut.Markup.Should().NotContain("The database is unavailable.", "the failure's text is the log's (T329)");
        cut.Find("#agenda-intro").TextContent.Should().Be(
            "Nothing is pending: ratifying the review issued what was staged as STARs, and the agenda names each one.");
    }

    [Fact]
    public void ARatifiedReview_WithNoAgenda_ClaimsNothingAboutWhatWasIssued()
    {
        // Not reached in production (the review query always reads the agenda), but with no agenda the page cannot say
        // whether ratifying issued anything, so it says nothing of it (T361: the Pending card's fallback went with it).
        var cut = Render(CommitteeReviewState.Ratified, agenda: null);

        cut.FindAll("#agenda-intro").Should().BeEmpty();
        cut.Markup.Should().NotContain("issued what was staged").And.NotContain("issued no STAR");
    }

    [Fact]
    public void Ratify_ReadsTheStagedDecisionsAgain_SoTheOnesItIssuedAreNoLongerShownAsStaged()
    {
        // T212: ratifying issues the staged decisions as STARs and removes them, but the page read the list only when it
        // loaded, so it went on listing them as pending until a reload.
        var cut = Render(
            CommitteeReviewState.Decided,
            Agenda(Staged(1, "PAED-001"), Staged(2, "PAED-002")),
            pending: [Pending(90, 1, "PAED-001"), Pending(91, 2, "PAED-002")]);
        StagedLines(cut).Should().Equal(new[] { 1, 2 }, "guard: the page loads with two staged");
        var ratified = Review(CommitteeReviewState.Ratified, Agenda(
            Line(1, "PAED-001", CommitteeAgendaLineStatus.Decided, CommitteeAgendaLineState.Decided),
            Line(2, "PAED-002", CommitteeAgendaLineStatus.Decided, CommitteeAgendaLineState.Decided)));
        _sender
            .On<RatifyCommitteeDecisionCommand>(_ => ratified)
            .On<GetCommitteeReviewByIdQuery>(_ => ratified)
            .On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => Array.Empty<PendingEntrustmentDecisionDto>());

        Ratify(cut, "Ratify and issue 2 STARs");

        cut.WaitForState(() => cut.Markup.Contains("Decision ratified."));
        StagedLines(cut).Should().BeEmpty();
        cut.Find("#agenda-intro").TextContent.Should().Be(
            "Nothing is pending: ratifying the review issued what was staged as STARs, and the agenda names each one.");
        _sender.Received.OfType<ListPendingEntrustmentDecisionsForReviewQuery>().Should().HaveCount(2, "once on load, once after Ratify");
    }

    [Fact]
    public void AFailedReadOfTheStagedDecisionsAfterRatify_IsAWarningInTheAgenda_BesideTheSuccess()
    {
        var cut = Render(
            CommitteeReviewState.Decided,
            Agenda(Staged(1, "PAED-001")),
            pending: [Pending(90, 1, "PAED-001")]);
        var ratified = Review(CommitteeReviewState.Ratified, Agenda(Line(1, "PAED-001", CommitteeAgendaLineStatus.Decided, CommitteeAgendaLineState.Decided)));
        _sender
            .On<RatifyCommitteeDecisionCommand>(_ => ratified)
            .On<GetCommitteeReviewByIdQuery>(_ => ratified)
            .On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => throw new InvalidOperationException("The database is unavailable."));

        Ratify(cut, "Ratify and issue 1 STAR");

        cut.WaitForState(() => cut.Markup.Contains("Decision ratified."));
        var warning = cut.Find("#agenda .alert");
        warning.ClassList.Should().Contain("alert-warning");
        Text(warning).Should().Be(ReviewWords.StagedNotRead);
        cut.FindAll(".alert-danger").Should().BeEmpty("the action succeeded");
        cut.Markup.Should().NotContain("The database is unavailable.", "the failure's text is the log's (T329)");
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
            .On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => new[] { Pending(91, 2, "PAED-002") });

        Remove(cut, 1);

        cut.WaitForState(() => cut.Markup.Contains("Pending entrustment decision removed."));
        _sender.Received.OfType<RemovePendingEntrustmentDecisionCommand>().Single().PendingId.Should().Be(90);
        StagedLines(cut).Should().Equal(2);

        _sender.On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => throw new InvalidOperationException("The database is unavailable."));

        Remove(cut, 2);

        cut.WaitForState(() => cut.Markup.Contains(ReviewWords.StagedNotRead));
        cut.Markup.Should().Contain("Pending entrustment decision removed.", "the remove succeeded; only the read after it failed");
        cut.Find("#agenda .alert").ClassList.Should().Contain("alert-warning");

        // The next read that succeeds clears the warning (the T212 review found nothing held it).
        _sender.On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => Array.Empty<PendingEntrustmentDecisionDto>());

        Remove(cut, 2);

        cut.WaitForAssertion(() => StagedLines(cut).Should().BeEmpty());
        cut.FindAll("#agenda .alert-warning").Should().BeEmpty("the list was read again, so the warning no longer stands");
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
            .On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => throw new InvalidOperationException("The database is unavailable."));
        Remove(cut, 1);
        cut.WaitForState(() => cut.Markup.Contains(ReviewWords.StagedNotRead));

        _sender.On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => Array.Empty<PendingEntrustmentDecisionDto>());
        var reads = _sender.Received.OfType<GetCommitteeReviewByIdQuery>().Count();
        cut.SetParametersAndRender(parameters => parameters.Add(page => page.ReviewId, 31));

        cut.WaitForAssertion(() => _sender.Received.OfType<GetCommitteeReviewByIdQuery>().Should().HaveCount(reads + 1));
        cut.WaitForAssertion(() => cut.FindAll("#agenda .alert-warning").Should().BeEmpty());
    }

    [Fact]
    public void BeforeRatify_NothingStaged_TheCaptionSaysNoneStaged()
    {
        // T361 (round 3): the caption counts what is staged; the Pending card that said so is gone.
        var cut = Render(CommitteeReviewState.InProgress, Agenda(Due(1, "PAED-001")));

        cut.Find("#agenda caption").TextContent.Should().Be("1 EPA for 2026 S1 · none staged");
        cut.FindAll("#agenda-intro").Should().BeEmpty();
    }

    [Fact]
    public void AWithdrawnReview_SaysNothingIsPending_NotThatNothingWasStaged()
    {
        // T258 review: withdrawing removed whatever was staged, so "none have been staged" could be false.
        var cut = Render(
            CommitteeReviewState.Withdrawn,
            Agenda(Line(1, "PAED-001", CommitteeAgendaLineStatus.NotDecided, CommitteeAgendaLineState.Due, closing: true)),
            withdrawn: (new DateOnly(2026, 9, 25), CommitteeReview.WithdrawnTraineeErased));

        cut.Find("#agenda-intro").TextContent.Should().Be(
            "Nothing is pending: the review was withdrawn, so it issues no STAR, and anything staged at it was removed.");
    }

    // ---- Fixture ------------------------------------------------------------------------------------------------------

    /// <param name="seatsAQuorum">
    /// The panel's chair and one member may both sit (T165), so the panel is no reason to refuse Record once the member is
    /// ticked; otherwise the review names no panel member.
    /// </param>
    /// <param name="pending">The staged decisions the page loads with; none unless given.</param>
    /// <param name="callerChairs">Whether the reader holds the panel's Chair seat (T213); these tests act as the chair unless told otherwise.</param>
    private IRenderedComponent<ReviewDetail> Render(
        CommitteeReviewState state,
        CommitteeAgendaDto? agenda,
        bool formative = false,
        bool seatsAQuorum = false,
        IReadOnlyList<PendingEntrustmentDecisionDto>? pending = null,
        bool callerChairs = true,
        (DateOnly On, string Reason)? withdrawn = null)
    {
        var review = Review(state, agenda, formative, seatsAQuorum) with
        {
            CallerChairs = callerChairs,
            WithdrawnOn = withdrawn?.On,
            WithdrawalReason = withdrawn?.Reason
        };

        _sender
            .On<GetCommitteeReviewByIdQuery>(_ => review)
            .On<GetCommitteeAgendaQuery>(_ => review.Agenda)
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
            .On<GetEntrustmentStandingForTraineeQuery>(_ => null)
            .On<GetMsfCoverageForTraineeQuery>(_ => null)
            .On<GetStartPreviewQuery>(_ => null);

        var cut = RenderComponent<ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, 30));
        cut.WaitForState(() => cut.Markup.Contains("Evidence snapshot"));
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
            state, new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc), "chair-1", null, null, null,
            state is CommitteeReviewState.Decided or CommitteeReviewState.Ratified or CommitteeReviewState.Final && !formative
                ? [new CommitteeDecisionDto(41, CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null,
                    new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc), "chair-1", null)]
                : [],
            [],
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

    /// <summary>The EPAs whose agenda lines say a staged decision (T361, round 3 item 30).</summary>
    private static IReadOnlyList<int> StagedLines(IRenderedComponent<ReviewDetail> cut)
        => cut.FindAll("#agenda tbody tr.agenda-line")
            .Where(row => row.TextContent.Contains("Rests on", StringComparison.Ordinal))
            .Select(row => int.Parse(row.Id!.Replace("agenda-line-", string.Empty, StringComparison.Ordinal), System.Globalization.CultureInfo.InvariantCulture))
            .ToArray();

    /// <summary>Remove… on the line, then the dialog's Remove the decision (T361, round 3 items 17, 22).</summary>
    private static void Remove(IRenderedComponent<ReviewDetail> cut, int epaId)
    {
        cut.Find($"#agenda-remove-{epaId}").Click();
        cut.FindAll("dialog button").Single(button => button.TextContent.Trim() == "Remove the decision").Click();
    }

    /// <summary>Ratify… in the sitting bar, then the dialog's confirm (T361, round 3 item 22).</summary>
    private static void Ratify(IRenderedComponent<ReviewDetail> cut, string confirm)
    {
        RatifyButton(cut).Click();
        cut.FindAll("dialog button").Single(button => button.TextContent.Trim() == confirm).Click();
    }

    private static CommitteeAgendaDto Agenda(params CommitteeAgendaLineDto[] lines)
        => new(30, 2026, 1, "2026 S1", false, lines, []);

    private static CommitteeAgendaLineDto Due(int epaId, string code)
        => Line(epaId, code, CommitteeAgendaLineStatus.Due, CommitteeAgendaLineState.Due, closing: true);

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
        bool readsNotDecided = false)
    {
        var (year, semester) = window ?? (2026, 1);
        return new CommitteeAgendaLineDto(
            100 + epaId, epaId, code, $"EPA {epaId}", CommitteeAgendaLineOrigin.Cadence, year, semester,
            semester is int s ? $"{year} S{s}" : $"{year}", closing, false, state, status, staged, readsNotDecided, null,
            epaId == 1 ? 2 : epaId == 2 ? 1 : 0);
    }

    private static CommitteeEvidenceDto Evidence(int id, int epaId)
        => new(
            id, CommitteeEvidenceSourceType.Activity, id, null, null, $"Mini-CEX #{id}", "State: completed.",
            new DateTime(2026, 2, 10, 9, 0, 0, DateTimeKind.Utc),
            EpaId: epaId, EpaCode: $"PAED-00{epaId}", EpaTitle: $"EPA {epaId}", InstrumentName: "Mini-CEX",
            ObservedOn: new DateOnly(2026, 2, 10), ObservedOnDeclared: true, SourceState: "completed", SourceFinished: true,
            SourceStateLabel: "Completed");

    private static IReadOnlyList<string> AgendaHeaders(IRenderedComponent<ReviewDetail> cut)
        => cut.FindAll("#agenda thead th").Select(header => header.TextContent.Trim()).ToArray();

    private static IReadOnlyList<AngleSharp.Dom.IElement> AgendaRows(IRenderedComponent<ReviewDetail> cut)
        => cut.FindAll("#agenda tbody tr.agenda-line").ToArray();

    private static AngleSharp.Dom.IElement Row(IRenderedComponent<ReviewDetail> cut, int epaId) => cut.Find($"#agenda-line-{epaId}");

    private static IReadOnlyList<string> Buttons(AngleSharp.Dom.IElement row)
        => row.QuerySelectorAll("button").Select(button => button.TextContent.Trim()).ToArray();

    // The page's own buttons: the dialogs' are always in the page (T361, round 3 item 22).
    private static IReadOnlyList<string> PageButtons(IRenderedComponent<ReviewDetail> cut)
        => cut.FindAll("button").Where(button => button.Closest("dialog") is null).Select(button => button.TextContent.Trim()).ToArray();

    /// <summary>The muted reason in a line's Action cell (the row's fifth cell), or null where it has none.</summary>
    private static string? ActionReason(AngleSharp.Dom.IElement row)
        => row.Children.Length < 5 ? null : row.Children[4].QuerySelector(".actions-cell > span.muted")?.TextContent.Trim();

    private static AngleSharp.Dom.IElement ButtonIn(AngleSharp.Dom.IElement row, string label)
        => row.QuerySelectorAll("button").Single(button => button.TextContent.Trim() == label);

    private static AngleSharp.Dom.IElement RecordButton(IRenderedComponent<ReviewDetail> cut) => cut.Find("#record-decision");

    private static AngleSharp.Dom.IElement RatifyButton(IRenderedComponent<ReviewDetail> cut) => cut.Find("#sitting-ratify");

    private static string Text(AngleSharp.Dom.IElement element) => Whitespace().Replace(element.TextContent, " ").Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    /// <summary>Answers what a test registers (the last registration wins), records every request, refuses the rest.</summary>
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
