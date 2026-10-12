using Bunit;
using FluentAssertions;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Web.Tests.Design;

namespace Wombat.Web.Tests.CommitteeDecisions.ReviewPage;

/// <summary>
/// T361 (flow 07, round 3): the agenda, the page's spine. 1280 stage frames 1–13, 16; 1280 start frame 6; 1280 more
/// frame 1; round 3 items 13, 16, 17, 20, 24, 25, 27, 30; E1, E9; C2. No deferral anywhere (D53).
/// </summary>
public sealed class ReviewPageAgendaTests : ReviewPageTestBase
{
    // ─── The lines (24, 30; A.7.13) ──────────────────────────────────────────────

    [Fact]
    public void Scheduled_TheAgendaListsEachLine_CodeOverTitle_WindowStateEvidence_AndNoActionColumn()
    {
        var cut = Render(Review(CommitteeReviewState.Scheduled));

        Text(cut.Find("#agenda h2")).Should().Be("Agenda");
        Text(cut.Find("#agenda caption")).Should().Be("4 EPAs for 2026 S2");
        cut.FindAll("#agenda thead th").Select(Text).Should().Equal("EPA", "Window", "State", "Evidence");
        var line = cut.Find("#agenda-line-1");
        Text(line.QuerySelector(".agenda-epa-code")!).Should().Be("PAED-001");
        Text(line.QuerySelector(".agenda-epa-title")!).Should().Be("Providing paediatric emergency care to children");
        Text(line.QuerySelectorAll("td")[0]).Should().Be("2026 S2");
        Text(line.QuerySelector(".badge")!).Should().Be("Due at this sitting");
        Text(cut.Find("#agenda-line-8").QuerySelectorAll("td")[0]).Should().Be("2026");
        Text(cut.Find("#agenda-line-8 .badge")).Should().Be("As opportunity allows");
        Text(line.QuerySelectorAll("td")[2]).Should().Be("3 items");
    }

    [Fact]
    public void TheAgendaTable_CarriesItsRolesExplicitly_SoAStackedLineKeepsThem()
    {
        // Round 3 item 20: a container query stacks the lines below 760px.
        var cut = Render(Review());

        cut.Find("#agenda table").GetAttribute("role").Should().Be("table");
        cut.Find("#agenda-line-1").GetAttribute("role").Should().Be("row");
        cut.Find("#agenda-line-1 th").GetAttribute("role").Should().Be("rowheader");
        cut.FindAll("#agenda-line-1 td").Should().OnlyContain(cell => cell.GetAttribute("role") == "cell");
        cut.FindAll("#agenda thead th").Should().OnlyContain(header => header.GetAttribute("role") == "columnheader");
        cut.Find("#agenda table").ParentElement!.ClassList.Should().Contain("agenda-table-container");
    }

    [Fact]
    public void AStackedLinesCells_AreLabelledByTheirColumnsHeaders()
    {
        // Review D3: below 760px the header row is hidden and each cell prints its data-label, so each label is the
        // header of the column the cell is in.
        var cut = Render(Review(CommitteeReviewState.InProgress));

        var headers = cut.FindAll("#agenda thead th").Select(header => Text(header)).ToList();
        var cells = cut.Find("#agenda-line-1").Children;
        cells.Should().HaveCount(headers.Count);
        for (var index = 0; index < cells.Length; index++)
        {
            if (cells[index].GetAttribute("data-label") is { } label)
            {
                label.Should().Be(headers[index]);
            }
        }

        cut.FindAll("#agenda-line-1 td[data-label]").Select(cell => cell.GetAttribute("data-label"))
            .Should().Equal("Window", "State", "Evidence");

        var css = Stylesheet.AppCss();
        var before = css.RulesFor(".agenda-table td[data-label]::before", "@container agenda (width < 760px)")
            .Should().ContainSingle().Which;
        before.Declarations.Where(declaration => declaration.Property == "content").Select(declaration => declaration.Value)
            .Should().Equal("attr(data-label) \": \"", "attr(data-label) \": \" / \"\"");
        css.Cascaded(".agenda-table td[data-label]::before").Should().BeEmpty("a wide agenda keeps its header row");
    }

    [Fact]
    public void InProgress_TheChairsLinesOfferStage_NamedByTheirEpa_AndTheCaptionCountsTheStaged()
    {
        var cut = Render(Review());

        Text(cut.Find("#agenda caption")).Should().Be("4 EPAs for 2026 S2 · none staged");
        var stage = cut.Find("#agenda-stage-1");
        Text(stage).Should().Be("Stage");
        stage.GetAttribute("aria-label").Should().Be("Stage a decision on PAED-001");
        stage.ClassList.Should().BeEquivalentTo("btn", "btn-sm", "btn-outline");
        stage.GetAttribute("aria-expanded").Should().Be("false");
        Buttons(cut).Should().NotContain(b => b.Contains("Defer") || b.Contains("Reinstate"), "there is no deferral (D53)");
    }

    [Fact]
    public void AMember_ReadsFourColumns_AndNoStage()
    {
        SignInAs(Naidoo);
        var cut = Render(Review(callerChairs: false));

        cut.FindAll("#agenda thead th").Select(Text).Should().Equal("EPA", "Window", "State", "Evidence");
        cut.FindAll("#agenda-stage-1").Should().BeEmpty();
        cut.FindAll("#agenda-foot").Should().BeEmpty("the foot form is the chair's");
    }

    // ─── Opening a line (C2; 4.17) ───────────────────────────────────────────────

    [Fact]
    public void Stage_OpensTheLineInPlace_WithItsFormUnderIt_AndTheFocusOnAuthorisedLevel()
    {
        var cut = Render(Review());

        cut.Find("#agenda-stage-1").Click();

        cut.Find("#agenda-line-1").ClassList.Should().Contain("is-editing");
        var row = cut.Find("#agenda-form-1");
        row.ClassList.Should().Contain("is-editing").And.Contain("agenda-form-row");
        Text(row.QuerySelector("h3")!).Should().Be("Stage a decision on PAED-001");
        cut.Find("#agenda-stage-1").GetAttribute("aria-expanded").Should().Be("true");
        FocusedIds().Should().Contain("stage-1-level");
        Text(cut.Find("#stage-1-level-help")).Should().Be(
            "Rungs of CPSA Paediatric Entrustment Scale v11.1, the ladder this EPA is assessed on.");
        cut.FindAll("#stage-1-level option").Select(Text).Should().Equal("Choose a rung…", "1", "3b", "4", "5");
        cut.Find("#stage-1-issued").GetAttribute("value").Should().Be("2026-10-05", "today on the South African calendar");
    }

    [Fact]
    public void TheOpenLine_ListsItsEpasEvidenceFirst_ByInstrumentAtoZ_WithReadOnEach_AndTheOthersBehindAButton()
    {
        // Round 3 item 30; 1280 stage frame 1.
        var cut = Render(Review());
        cut.Find("#agenda-stage-1").Click();

        var pick = cut.Find("#agenda-form-1 .snapshot-pick");
        pick.QuerySelectorAll(".snapshot-pick-line label").Take(3).Select(label => Text(label)).Should().Equal(
            "Case-Based Discussion (Paediatrics) #13 · David Naidoo Rated 5 · 2026-09-24 · Completed",
            "DOPS (Paediatrics) #14 · Mohammed Patel Rated 4 · 2026-09-26 · Completed",
            "Mini-CEX (Paediatrics) #12 · Thandi Zulu Rated 5 · 2026-09-23 · Completed");
        cut.Find("#stage-1-read-12").GetAttribute("aria-label").Should().Be("Read Mini-CEX (Paediatrics) #12");

        var others = cut.Find("#stage-1-others");
        Text(others).Should().Be("Show the other 3 snapshot lines");
        others.GetAttribute("aria-expanded").Should().Be("false");
        cut.FindAll("#stage-1-evidence-21").Should().BeEmpty();

        others.Click();
        cut.Find("#stage-1-others").GetAttribute("aria-expanded").Should().Be("true");
        cut.FindAll("#stage-1-evidence-21").Should().ContainSingle();
        cut.FindAll("#stage-1-evidence-1").Should().ContainSingle("the campaign report may ground a decision too");
    }

    [Fact]
    public void StagePendingDecision_IsUnavailableUntilATick_NeverNativelyDisabled_AndAPressMovesTheFocusToItsReason()
    {
        // Round 3 item 16.
        var cut = Render(Review());
        cut.Find("#agenda-stage-1").Click();

        var stage = cut.Find("#stage-1-submit");
        stage.HasAttribute("disabled").Should().BeFalse();
        stage.GetAttribute("aria-disabled").Should().Be("true");
        stage.ClassList.Should().Contain("is-unavailable");
        stage.GetAttribute("aria-describedby").Should().Be("stage-1-reason");
        Text(cut.Find("#stage-1-reason")).Should().Be("Name at least one item of the evidence snapshot.");

        stage.Click();
        FocusedIds().Should().Contain("stage-1-reason");
        Sender.Sent<StagePendingEntrustmentDecisionCommand>().Should().BeEmpty();

        cut.Find("#stage-1-evidence-12").Change(true);
        cut.Find("#stage-1-submit").HasAttribute("aria-disabled").Should().BeFalse();
        cut.Find("#stage-1-submit").ClassList.Should().NotContain("is-unavailable");
        cut.FindAll("#stage-1-reason").Should().BeEmpty();
    }

    [Fact]
    public void OneTick_SaysTheHint_NeverRefused_AndTwoSayNothing()
    {
        var cut = Render(Review());
        cut.Find("#agenda-stage-1").Click();

        cut.Find("#stage-1-evidence-12").Change(true);
        Text(cut.Find("#stage-1-hints")).Should().Be(
            "Only one item is named. The College's rule is that an entrustment decision is never taken from a single form.");
        cut.Find("#stage-1-hints").HasAttribute("role").Should().BeFalse("hints are not live (round 3)");

        cut.Find("#stage-1-evidence-13").Change(true);
        Text(cut.Find("#stage-1-hints")).Should().BeEmpty();
    }

    [Fact]
    public void Staging_SendsTheLinesEpaAndTheTicksInOrder_ClosesTheLine_AndTheResultUnderItTakesTheFocus()
    {
        var cut = Render(Review());
        Sender.On<StagePendingEntrustmentDecisionCommand>(_ => Pending(1, "5", [12, 13]));
        cut.Find("#agenda-stage-1").Click();
        cut.Find("#stage-1-level").Change("36");
        cut.Find("#stage-1-rationale").Change("Leads resuscitation without prompting.");
        cut.Find("#stage-1-evidence-12").Change(true);
        cut.Find("#stage-1-evidence-13").Change(true);

        Sender.Now<ListPendingEntrustmentDecisionsForReviewQuery>(_ => new[] { Pending(1, "5", [12, 13]) });
        Sender.Now<GetCommitteeReviewByIdQuery>(_ => Review(agenda: Agenda(
        [
            Line(1, "Providing paediatric emergency care to children", CommitteeAgendaLineStatus.Staged, staged: true, evidence: 3),
            .. Lines().Skip(1)
        ])));
        cut.Find("#stage-1-submit").Click();

        var sent = Sender.Sent<StagePendingEntrustmentDecisionCommand>().Should().ContainSingle().Which;
        (sent.EpaId, sent.AuthorisedLevelId, sent.IssuedOn, sent.Rationale).Should().Be(
            (1, 36, new DateOnly(2026, 10, 5), "Leads resuscitation without prompting."));
        sent.EvidenceItemIds.Should().Equal(12, 13);

        cut.WaitForAssertion(() => cut.FindAll("#agenda-form-1").Should().BeEmpty());
        var result = cut.Find("#agenda-line-1 + tr.agenda-result-row .action-result");
        Text(result).Should().Be("Pending entrustment decision staged.");
        ElementFocusCalls().Should().BeGreaterThan(0);
        Text(cut.Find("#agenda-line-1 .badge")).Should().Be("Staged");
        Text(cut.Find("#agenda caption")).Should().Be("4 EPAs for 2026 S2 · 1 staged");
    }

    [Fact]
    public void AStagedLine_SaysItsDecisionInItem30sWords_WithRemoveAsASmallOutlineButton()
    {
        var agenda = Agenda(
        [
            Line(1, "Providing paediatric emergency care to children", CommitteeAgendaLineStatus.Staged, staged: true, evidence: 3),
            Line(10, "Leading and operating within a clinical team", CommitteeAgendaLineStatus.Staged, staged: true, evidence: 2)
        ]);
        var cut = Render(Review(agenda: agenda),
            [Pending(1, "5", [12, 13]), Pending(10, "4", [21, 22], expires: new DateOnly(2026, 10, 25))]);

        Text(cut.Find("#agenda-line-1 td[data-label=State]")).Should().Be(
            "Staged Level 5 · issued 2026-10-05 · no expiry. " +
            "Rests on 2 items of the snapshot: Mini-CEX (Paediatrics) #12; Case-Based Discussion (Paediatrics) #13.");
        Text(cut.Find("#agenda-line-10 td[data-label=State]")).Should().Be(
            "Staged Level 4 · issued 2026-10-05 · expires 2026-10-25. " +
            "Rests on 2 items of the snapshot: Direct Observation #21; MSF record #22.");
        var remove = cut.Find("#agenda-remove-1");
        Text(remove).Should().Be("Remove…");
        remove.ClassList.Should().BeEquivalentTo("btn", "btn-sm", "btn-outline");
        remove.GetAttribute("aria-label").Should().Be("Remove the staged decision on PAED-001");
    }

    [Fact]
    public void AStagedLineOnOneItemAboutAnotherEpa_SaysBothHints()
    {
        // 1280 more frame 1: the pre-graduation lines rest on Default MSF #1 alone.
        var agenda = Agenda([Line(2, "Managing common paediatric presentations", CommitteeAgendaLineStatus.Staged, staged: true)]);
        var cut = Render(Review(agenda: agenda), [Pending(2, "5", [12])]);

        Text(cut.Find("#agenda-line-2 td[data-label=State]")).Should().EndWith(
            "Rests on 1 item of the snapshot: Mini-CEX (Paediatrics) #12. Only one item is named. The College's rule is " +
            "that an entrustment decision is never taken from a single form. None of the named items is about PAED-002.");
    }

    [Fact]
    public void AChairsLine_SaysAddedByTheChair()
    {
        var agenda = Agenda(
        [
            Line(10, "Leading and operating within a clinical team", CommitteeAgendaLineStatus.Staged, staged: true,
                origin: CommitteeAgendaLineOrigin.Chair)
        ]);
        var cut = Render(Review(agenda: agenda), [Pending(10, "5", [21, 22])]);

        Text(cut.Find("#agenda-line-10 td[data-label=State]")).Should().StartWith("Staged Added by the chair. Level 5 · issued 2026-10-05");
    }

    [Fact]
    public void AStagedDecisionOnAnEpaNoLongerInUse_IsMarkedInItsLine()
    {
        // Round 3 item 13, T273.
        var agenda = Agenda([Line(2, "Managing common paediatric presentations", CommitteeAgendaLineStatus.Staged, staged: true)]);
        var cut = Render(Review(agenda: agenda), [Pending(2, "5", [12]) with { EpaInForce = false }]);

        Text(cut.Find("#agenda-line-2 .agenda-epa-code")).Should().Be("PAED-002 (no longer in use)");
    }

    // ─── One open line at a time; a draft kept (E9) ──────────────────────────────

    [Fact]
    public void OpeningAnotherLine_ClosesTheFirst_KeepingItsDraft_WhichItsStageReopensAsTyped()
    {
        var cut = Render(Review());
        cut.Find("#agenda-stage-1").Click();
        cut.Find("#stage-1-rationale").Change("Leads resuscitation.");
        cut.Find("#stage-1-evidence-12").Change(true);

        cut.Find("#agenda-stage-10").Click();

        cut.FindAll("#agenda-form-1").Should().BeEmpty();
        cut.FindAll("#agenda-form-10").Should().ContainSingle();
        Text(cut.Find("#agenda-line-1 .agenda-draft-kept")).Should().Be("Draft kept");
        cut.Find("#agenda-stage-1").GetAttribute("aria-label").Should().Be("Stage a decision on PAED-001 (draft kept)");

        cut.Find("#agenda-stage-1").Click();

        cut.Find("#stage-1-rationale").GetAttribute("value").Should().Be("Leads resuscitation.");
        cut.Find("#stage-1-evidence-12").HasAttribute("checked").Should().BeTrue();
        Text(cut.Find("#agenda-line-10 .agenda-draft-kept")).Should().Be("Draft kept");
    }

    [Fact]
    public void Cancel_DiscardsTheDraft_ClosesTheLine_AndReturnsTheFocusToItsStage()
    {
        var cut = Render(Review());
        cut.Find("#agenda-stage-1").Click();
        cut.Find("#stage-1-rationale").Change("Leads resuscitation.");

        cut.FindAll("#agenda-form-1 button").Single(button => Text(button) == "Cancel").Click();

        cut.FindAll("#agenda-form-1").Should().BeEmpty();
        cut.FindAll("#agenda-line-1 .agenda-draft-kept").Should().BeEmpty();
        FocusedIds().Should().EndWith("agenda-stage-1");

        cut.Find("#agenda-stage-1").Click();
        cut.Find("#stage-1-rationale").GetAttribute("value").Should().BeNullOrEmpty();
    }

    // ─── Refusals (4.18, 4.20; T324) ─────────────────────────────────────────────

    [Fact]
    public void AnExpiryBeforeTheIssue_IsRefusedAtTheFormsTop_TheFieldMarked_EverythingTypedKept_NothingSent()
    {
        var cut = Render(Review());
        cut.Find("#agenda-stage-10").Click();
        cut.Find("#stage-10-level").Change("35");
        cut.Find("#stage-10-expires").Change("2026-10-01");
        cut.Find("#stage-10-rationale").Change("Leads the ward round safely.");
        cut.Find("#stage-10-evidence-21").Change(true);

        cut.Find("#stage-10-submit").Click();

        Text(cut.Find("#agenda-form-10 .action-result")).Should().Be("Not staged. An expiry date must be after the issue date.");
        cut.Find("#agenda-form-10 .action-result .alert").GetAttribute("role").Should().Be("alert");
        cut.Find("#stage-10-expires").GetAttribute("aria-invalid").Should().Be("true");
        Text(cut.Find("#stage-10-expires-message")).Should().Be("An expiry date must be after the issue date.");
        cut.Find("#stage-10-rationale").GetAttribute("value").Should().Be("Leads the ward round safely.");
        Sender.Sent<StagePendingEntrustmentDecisionCommand>().Should().BeEmpty();
        Text(cut.Find("#agenda-line-10 .badge")).Should().Be("Due at this sitting");
    }

    [Fact]
    public void ASecondDecisionOnAnEpa_FromTheFootForm_IsRefusedAtItsTop_InTheServersWords()
    {
        var cut = Render(Review());
        const string refusal = "An entrustment decision on PAED-001 is already staged at this review. Remove it to stage another.";
        Sender.On<StagePendingEntrustmentDecisionCommand>(_ => throw new InvalidOperationException(refusal));

        Text(cut.Find("#agenda-foot h3")).Should().Be("Stage a decision on an EPA not on this agenda");
        cut.Find("#stage-0-epa").Change("1");
        cut.Find("#stage-0-level").Change("35");
        cut.Find("#stage-0-rationale").Change("Typed.");
        cut.Find("#stage-0-evidence-14").Change(true);
        cut.Find("#stage-0-submit").Click();

        cut.WaitForAssertion(() => Text(cut.Find("#agenda-foot .action-result")).Should().Be($"Not staged. {refusal}"));
        cut.Find("#stage-0-rationale").GetAttribute("value").Should().Be("Typed.");
    }

    // ─── Remove (17, 22; 4.19) ───────────────────────────────────────────────────

    [Fact]
    public void Remove_OpensADialog_NamingTheEpaAndWhatIsDiscarded_KeepItSafe()
    {
        var agenda = Agenda([Line(1, "Providing paediatric emergency care to children", CommitteeAgendaLineStatus.Staged, staged: true, evidence: 3)]);
        var cut = Render(Review(agenda: agenda), [Pending(1, "5", [12, 13])]);

        cut.Find("#agenda-remove-1").Click();

        var dialog = Dialog(cut, "Remove the staged decision on PAED-001?");
        Text(dialog.QuerySelector("p")!).Should().Be(
            "Its level 5, its rationale and the 2 snapshot items it rests on are discarded. PAED-001 is due at this sitting again.");
        var buttons = dialog.QuerySelectorAll("button").Select(button => Text(button)).ToArray();
        buttons.Should().Equal("Keep it", "Remove the decision");
        dialog.QuerySelectorAll("button")[1].ClassList.Should().Contain("btn-danger");
        JSInterop.Invocations.Should().Contain(invocation => invocation.Identifier == "wombatDialog.showModal");
    }

    [Fact]
    public void Removing_PutsTheResultInTheLinesPlace_AndTheLineIsDueAgain()
    {
        var agenda = Agenda([Line(1, "Providing paediatric emergency care to children", CommitteeAgendaLineStatus.Staged, staged: true, evidence: 3)]);
        var cut = Render(Review(agenda: agenda), [Pending(1, "5", [12, 13])]);
        Sender.On<RemovePendingEntrustmentDecisionCommand>(_ => MediatR.Unit.Value);

        cut.Find("#agenda-remove-1").Click();
        Sender.Now<GetCommitteeReviewByIdQuery>(_ => Review(agenda: Agenda([Line(1, "Providing paediatric emergency care to children", evidence: 3)])));
        Sender.Now<ListPendingEntrustmentDecisionsForReviewQuery>(_ => Array.Empty<PendingEntrustmentDecisionDto>());
        DialogButton(cut, "Remove the staged decision on PAED-001?", "Remove the decision").Click();

        cut.WaitForAssertion(() => Text(cut.Find("#agenda-line-1 + tr.agenda-result-row")).Should().Be("Pending entrustment decision removed."));
        Text(cut.Find("#agenda-line-1 .badge")).Should().Be("Due at this sitting");
        Sender.Sent<RemovePendingEntrustmentDecisionCommand>().Should().ContainSingle().Which.PendingId.Should().Be(901);
    }

    // ─── After Record, and once ratified (E1, D53) ───────────────────────────────

    [Fact]
    public void Decided_EveryUnstagedLine_ReadsNotDecidedAtThisSitting_WithTheDecisionsSentence()
    {
        var agenda = Agenda(
        [
            Line(1, "Providing paediatric emergency care to children", CommitteeAgendaLineStatus.Staged, staged: true, evidence: 3),
            Line(2, "Managing common paediatric presentations", CommitteeAgendaLineStatus.NotDecided, readsNotDecided: true),
            Line(8, "Evaluating and managing neurodevelopmental and behavioural presentations in children",
                CommitteeAgendaLineStatus.NotDecided, annual: true, readsNotDecided: true)
        ]);
        var cut = Render(Review(CommitteeReviewState.Decided, agenda: agenda), [Pending(1, "5", [12, 13])]);

        Text(cut.Find("#agenda-intro")).Should().Be("Fixed when the committee's decision was recorded: ratifying issues exactly these.");
        foreach (var epa in new[] { 2, 8 })
        {
            Text(cut.Find($"#agenda-line-{epa} td[data-label=State]")).Should().Be(
                "Not decided at this sitting Not at a decision point this year.");
        }

        Page(cut).Replace("Outcome Deferred", string.Empty, StringComparison.Ordinal)
            .Should().NotContainEquivalentOf("defer", "there is no deferral (D53)");
    }

    [Fact]
    public void Ratified_TheIssuedLinesNameTheirStar_AndTheAgendaSaysWhatRatifyingIssued()
    {
        var agenda = Agenda(
        [
            Line(1, "Providing paediatric emergency care to children", CommitteeAgendaLineStatus.Decided, CommitteeAgendaLineState.Decided, starId: 1),
            Line(2, "Managing common paediatric presentations", CommitteeAgendaLineStatus.NotDecided, CommitteeAgendaLineState.NotDecided, readsNotDecided: true)
        ]);
        var cut = Render(Review(CommitteeReviewState.Ratified, callerChairs: false, agenda: agenda));

        Text(cut.Find("#agenda-intro")).Should().Be(
            "Nothing is pending: ratifying the review issued what was staged as STARs, and the agenda names each one.");
        Text(cut.Find("#agenda-line-1 td[data-label=State]")).Should().Be("Decided STAR #1.");
    }

    [Fact]
    public void Ratified_WithNothingStaged_SaysItIssuedNoStar()
    {
        // du Plessis (4.30).
        var agenda = Agenda([Line(2, "Managing common paediatric presentations", CommitteeAgendaLineStatus.NotDecided, CommitteeAgendaLineState.NotDecided, readsNotDecided: true)]);
        var cut = Render(Review(CommitteeReviewState.Ratified, callerChairs: false, agenda: agenda));

        Text(cut.Find("#agenda-intro")).Should().Be("Nothing was staged when the review was ratified, so ratifying it issued no STAR.");
        Text(cut.Find(".sitting-bar-count")).Should().Be("Satisfactory Progress · no STAR issued");
    }

    [Fact]
    public void WhatAStarAlreadyDecided_IsNamedUnderTheCaption()
    {
        // 1280 more frame 1 (25).
        var cut = Render(Review() with { Agenda = Agenda() with { DecidedInWindow = ["PAED-001", "PAED-012"] } });

        Text(cut.Find("#agenda-decided-in-window")).Should().Be("Already decided in this window, so not on the agenda: PAED-001 and PAED-012.");
    }

    // ─── The standing in the open line (11, 26) and the chart pair (C2) ──────────

    [Fact]
    public void TheOpenLinesStanding_IsOneLine_EachPartApart()
    {
        var cut = RenderWithStanding(standingFails: false);
        cut.Find("#agenda-stage-1").Click();

        Text(cut.Find("#stage-1-standing")).Should().Be(
            "Year 4 target 5 · no STAR yet · 3 ratings in the review window · MSF: None released yet");
        Text(cut.Find("#stage-1-chart-jump-1")).Should().Be("Rating chart for PAED-001");
    }

    [Fact]
    public void APartOfTheStandingThatFailed_IsReplacedOnce_AndTheRestStands()
    {
        var cut = RenderWithStanding(standingFails: true);
        cut.Find("#agenda-stage-1").Click();

        Text(cut.Find("#stage-1-standing")).Should().Be(
            "Standing could not be loaded. · 3 ratings in the review window · MSF: None released yet");
        Page(cut).Should().NotContain("Boom");
    }

    [Fact]
    public void TheChartJump_MovesTheFocusToTheChart_WhichSaysBackToTheLine_WhileItIsOpen()
    {
        var cut = RenderWithStanding(standingFails: false);
        cut.Find("#agenda-stage-1").Click();

        cut.Find("#stage-1-chart-jump-1").Click();
        FocusedIds().Should().EndWith("trajectory-1-h");

        var back = cut.Find("#trajectory-1 .chart-back");
        Text(back).Should().Be("Back to PAED-001's line");
        back.Click();
        FocusedIds().Should().EndWith("stage-1-chart-jump-1");

        cut.FindAll("#agenda-form-1 button").Single(button => Text(button) == "Cancel").Click();
        cut.FindAll(".chart-back").Should().BeEmpty("only while the line is open");
    }

    [Fact]
    public void TheFootFormAndAnOpenLine_OnOneEpa_EachHaveTheirOwnChartJump_AndBackFindsTheOpenLines()
    {
        // Review D4: the foot form chose PAED-001 while PAED-001's line is open; each draws "Rating chart for PAED-001".
        var cut = RenderWithStanding(standingFails: false);
        cut.Find("#agenda-stage-1").Click();
        cut.Find("#stage-0-epa").Change("1");

        var jumps = cut.FindAll(".chart-jump").Select(jump => jump.Id).ToList();
        jumps.Should().BeEquivalentTo(["stage-1-chart-jump-1", "stage-0-chart-jump-1"]);
        jumps.Should().OnlyHaveUniqueItems();

        var back = cut.Find("#trajectory-1 .chart-back");
        back.GetAttribute("href").Should().Be("#stage-1-chart-jump-1");
        back.Click();
        FocusedIds().Should().EndWith("stage-1-chart-jump-1");
    }

    [Fact]
    public void AFailedMsfRead_SaysSoInTheMsfCardsOwnWords_AndTheOpenLinesStandingSaysTheStandingFailedOnce()
    {
        Sender.On<GetMsfCoverageForTraineeQuery>(_ => throw new InvalidOperationException("Boom"));
        var cut = Render(Review());
        cut.Find("#agenda-stage-1").Click();

        Text(cut.Find("#msf-coverage-heading").ParentElement!).Should()
            .Contain("could not be loaded: the read failed. The rest of the review stands.")
            .And.NotContain("Standing could not be loaded").And.NotContain("Boom");
        Text(cut.Find("#stage-1-standing")).Should().Be("0 ratings in the review window · Standing could not be loaded.");
    }

    [Fact]
    public void WithTheAgendaUnread_TheFootFormStillStands()
    {
        var cut = Render(Review() with { Agenda = null });

        cut.FindAll("#agenda-foot").Should().ContainSingle();
        cut.FindAll("#stage-0-epa").Should().ContainSingle();
    }

    [Fact]
    public void OnADecidedReview_RemovingADecisionThatNoLongerFits_DoesNotSayTheLineIsDueAgain()
    {
        var agenda = Agenda([Line(1, "Providing paediatric emergency care to children", CommitteeAgendaLineStatus.Staged, staged: true, evidence: 3)]);
        var cut = Render(Review(CommitteeReviewState.Decided, agenda: agenda), [Pending(1, "5", [12, 13]) with { NoLongerFits = "Not on the curriculum." }]);

        cut.Find("#agenda-remove-1").Click();

        Text(Dialog(cut, "Remove the staged decision on PAED-001?").QuerySelector("p")!).Should().Be(
            "Its level 5, its rationale and the 2 snapshot items it rests on are discarded.");
    }

    private Bunit.IRenderedComponent<Wombat.Web.Components.Pages.CommitteeDecisions.ReviewDetail> RenderWithStanding(bool standingFails)
    {
        if (standingFails)
        {
            Sender.On<GetEntrustmentStandingForTraineeQuery>(_ => throw new InvalidOperationException("Boom"));
        }
        else
        {
            Sender.On<GetEntrustmentStandingForTraineeQuery>(_ => new EntrustmentStandingDto(
                new DateOnly(2026, 10, 5), new DateOnly(2023, 1, 15), 4, false,
                [
                    new EpaStandingDto(501, 1, "PAED-001", "Providing paediatric emergency care to children", "CPSA", false,
                        6, "5", false, 6, "5", null, EntrustmentStandingStatus.NoDecision, EntrustmentStandingStatus.NoDecision, null)
                ],
                new ExitRuleReadinessDto(15, 0, [], [])));
        }

        Sender.On<GetMsfCoverageForTraineeQuery>(_ => new MsfCoverageDto(new DateOnly(2026, 12, 31),
        [
            new MsfCoveragePeriodDto(2026, 2, "Semester 2, 2026", "July to November", new DateOnly(2026, 7, 1),
                new DateOnly(2026, 12, 31), new DateOnly(2026, 11, 30), false, 0)
        ],
        [
            new MsfEpaCoverageDto(501, 1, "PAED-001", "Providing paediatric emergency care to children", false,
                [new MsfEpaPeriodCoverageDto(2026, 2, [])])
        ]));
        Sender.On<GetEpaTrajectoryForTraineeQuery>(_ => new[]
        {
            new EpaTrajectoryDto(1, "PAED-001", "Providing paediatric emergency care to children", true, 3, "CPSA", [],
            [
                Point(12, new DateOnly(2026, 9, 23)), Point(13, new DateOnly(2026, 9, 24)), Point(14, new DateOnly(2026, 9, 26))
            ])
        });

        return Render(Review());
    }

    private static TrajectoryPointDto Point(int activityId, DateOnly observedOn)
        => new(activityId, observedOn, true, 6, "5", "Mini-CEX (Paediatrics)", "zulu") { AssessorName = "Thandi Zulu" };
}
