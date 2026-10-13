using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// Decisions due (T131 slice 6; T364, flow 08, lane D; boards R2-3): per trainee, which EPAs are due in a period and where
/// each decision stands. The filters are applied with Show and carried in the address (F2); By EPA's counts sit in a fold;
/// the Decisions heading says the page and takes the focus after Show and a page turn (C9, T264); with one trainee
/// filtered the trainee line says what holds the period (C2); each row carries one action (C3), its Schedule filled with
/// the row's own period (F1). Loading, a failure, nothing due and no match are each shown.
/// </summary>
public sealed partial class DecisionsDuePageTests : WombatTestContext
{
    /// <summary>What <see cref="ElementReference" />.FocusAsync calls.</summary>
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    private const string Route = "/committee/decisions-due";
    private const int Panel = 10;
    private const int NeonatalPanel = 11;

    private readonly DueSender _sender = new();
    private readonly CommitteeReviewPeriodOptionDto _current = CommitteeReviewPeriods.Current(QuotaCalendar.Today());
    private readonly TestAuthorizationContext _auth;

    public DecisionsDuePageTests()
    {
        _auth = this.AddTestAuthorization();
        _auth.SetAuthorized("coordinator@test");
        _auth.SetRoles(WombatRoles.Coordinator);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "coordinator-a"));
        JSInterop.Mode = JSRuntimeMode.Loose;

        Services.AddSingleton<IScopedSender>(_sender);
        _sender.Due = query => Task.FromResult<EntrustmentDecisionsDueDto?>(Result(query.AcademicYear, query.Semester, SampleRows()));
    }

    // ---- The states (C11) ---------------------------------------------------------------------------------------------

    [Fact]
    public void WhileTheQueryRuns_TheHeaderAndTheFormStand_WithSkeletons_AndNoScheduleOrOpenReview()
    {
        var pending = new TaskCompletionSource<EntrustmentDecisionsDueDto?>();
        _sender.Due = _ => pending.Task;

        var cut = Render();

        Text(cut.Find("h1")).Should().Be("Decisions due");
        Text(cut.Find(".page-subtitle")).Should().Be(
            "What the committee must decide, per registrar and per EPA, and where each decision stands.");
        cut.FindAll("form.search-container").Should().ContainSingle();
        Text(cut.Find("p.visually-hidden[role=status]")).Should().Be("Loading Decisions due.");
        cut.FindAll("h2").Select(Text).Should().Equal("By EPA", "Decisions");
        cut.FindAll(".skeleton").Should().NotBeEmpty();
        cut.FindAll("a.btn").Should().BeEmpty("no Schedule or Open review until the read is in (T329)");

        pending.SetResult(Result(_current.AcademicYear, _current.Semester, SampleRows()));
        cut.WaitForState(() => cut.FindAll("#due-items").Count == 1, AsyncWorkTimeout);
        cut.FindAll(".skeleton").Should().BeEmpty();
    }

    [Fact]
    public void AnUnexpectedFailure_IsFixedWords_WithTryAgain_WhoseAnswersHeadingTakesTheFocus()
    {
        var fail = true;
        _sender.Due = query => fail
            ? Task.FromException<EntrustmentDecisionsDueDto?>(new TimeoutException("a secret connection string"))
            : Task.FromResult<EntrustmentDecisionsDueDto?>(Result(query.AcademicYear, query.Semester, SampleRows()));

        var cut = Render();

        var alert = cut.Find(".action-result .alert-danger");
        Text(alert.QuerySelector(".alert-row-text")!).Should().Be(
            "Could not load Decisions due. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("secret");
        cut.FindAll("#due-items").Should().BeEmpty();

        fail = false;
        alert.QuerySelector("button")!.Click();

        cut.WaitForAssertion(() => cut.FindAll("#due-items").Should().ContainSingle());
        cut.WaitForAssertion(() => FocusedHeading(cut));
    }

    [Fact]
    public void AHandlersOwnRefusal_StillShowsInItsWords()
    {
        // S4: "never the exception's text" is for unexpected failures; a handler's own sentence still shows.
        _sender.Due = _ => Task.FromException<EntrustmentDecisionsDueDto?>(
            new ValidationException([new ValidationFailure("Semester", "A semester is 1 or 2.")]));

        var cut = Render();

        Text(cut.Find(".alert-danger .alert-row-text")).Should().Be("A semester is 1 or 2.");
    }

    [Fact]
    public void AFrameworksInvalidOperation_IsFixedWords_ButTheHandlersRefusalIsItsOwn()
    {
        // T364, build review R4 (T338): the framework throws InvalidOperationException too; only the handler's own refusal
        // (CommitteeRefusedException) is said in its words.
        Exception failure = new InvalidOperationException("Sequence contains no elements");
        _sender.Due = _ => Task.FromException<EntrustmentDecisionsDueDto?>(failure);

        var cut = Render();

        Text(cut.Find(".alert-danger .alert-row-text")).Should().Be(DecisionsDueText.LoadFailed);
        cut.Markup.Should().NotContain("Sequence contains");

        failure = new CommitteeRefusedException("Choose the institution whose decisions due to show.");
        cut.Find(".alert-danger button").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".alert-danger .alert-row-text"))
            .Should().Be("Choose the institution whose decisions due to show."));
    }

    [Fact]
    public void NothingDue_ShowsTheEmptyState_AndTheFormStands()
    {
        _sender.Due = query => Task.FromResult<EntrustmentDecisionsDueDto?>(Result(query.AcademicYear, query.Semester, []));

        var cut = Render();

        var empty = Text(cut.Find(".detail-card--empty"));
        empty.Should().Contain($"Nothing due for {PeriodLabel}")
            .And.Contain($"No trainee you oversee at Kgosi Kgari owes an entrustment decision in {PeriodLabel}.");
        cut.FindAll("#due-summary").Should().BeEmpty();
        cut.FindAll("form.search-container").Should().ContainSingle("another period can be read");
    }

    // ---- What it asks, and the filters (F2, F6) ------------------------------------------------------------------------

    [Fact]
    public void ItAsksForTheCurrentPeriod_WithNoInstitution_ForAnyoneButAnAdministrator()
    {
        Render();

        var query = _sender.Received.OfType<GetEntrustmentDecisionsDueQuery>().Should().ContainSingle().Subject;
        (query.AcademicYear, query.Semester, query.InstitutionId).Should().Be((_current.AcademicYear, _current.Semester, (int?)null));
        _sender.Received.OfType<GetInstitutionsListQuery>().Should().BeEmpty();
    }

    [Fact]
    public void ThePeriodsHelp_SaysWhySemester2sWindowRunsToDecember_AndTheFiltersAreLabelled()
    {
        var cut = Render();

        var period = cut.Find("#due-period");
        period.GetAttribute("aria-describedby").Should().Be("due-period-help");
        Text(cut.Find("#due-period-help")).Should().Be(
            "The College's semester 2 ends in November; its window runs to 31 December, so December's work counts.");
        cut.Find("form.search-container").GetAttribute("aria-label").Should().Be("Filter Decisions due");
        cut.FindAll("#due-trainee option").Select(Text).Should().Equal("Every trainee", "Ada Trainee", "Lerato Late");
        cut.FindAll("#due-status option").Take(3).Select(Text).Should().Equal(
            "Outstanding: not yet decided", "Every status", "Missed");
        Text(cut.Find(".filter-actions button[type=submit]")).Should().Be("Show");
        cut.FindAll(".filter-actions a").Should().BeEmpty("Clear filters only once a filter is set");
    }

    [Fact]
    public void SettingTheFields_ChangesNothing_UntilShow_WhichPutsThemInTheAddress_AndTheDecisionsHeadingTakesTheFocus()
    {
        var cut = Render();
        var navigation = Services.GetRequiredService<NavigationManager>();

        cut.Find("#due-status").Change("all");
        Text(cut.Find("#due-count")).Should().Be($"4 of 5 decisions due in {PeriodLabel} shown.", "nothing acts on change");

        cut.Find("form.search-container").Submit();

        navigation.Uri.Should().EndWith($"{Route}?period={PeriodKey}&status=all");
        cut.WaitForAssertion(() => Text(cut.Find("#due-count")).Should().Be($"5 of 5 decisions due in {PeriodLabel} shown: every status."));
        cut.WaitForAssertion(() => FocusedHeading(cut));
        _sender.Received.OfType<GetEntrustmentDecisionsDueQuery>().Should().ContainSingle("the period did not change: the answer is narrowed, not read again");
        Text(cut.Find(".filter-actions a")).Should().Be("Clear filters");
        cut.Find(".filter-actions a").GetAttribute("href").Should().Be($"{Route}?period={PeriodKey}");
    }

    [Fact]
    public void AnotherPeriodFromTheAddress_IsRead()
    {
        var earliest = CommitteeReviewPeriods.Around(QuotaCalendar.Today())[0];

        var cut = Render($"{Route}?period={earliest.Key}");

        var query = _sender.Received.OfType<GetEntrustmentDecisionsDueQuery>().Should().ContainSingle().Subject;
        (query.AcademicYear, query.Semester).Should().Be((earliest.AcademicYear, earliest.Semester));
        Text(cut.Find("#due-intro")).Should().Contain($"in {earliest.AcademicYear} S{earliest.Semester}");
        cut.Find("#due-period").GetAttribute("value").Should().Be(earliest.Key);
    }

    [Fact]
    public void ARowsStatusAndEpa_FromTheAddress_NarrowTheList_AndTheCountAndCaptionSaySo()
    {
        var cut = Render($"{Route}?status=NotScheduled&epa=1");

        Rows(cut).Should().Equal(("PAED-001 — Acute admission", "Lerato Late", "Not scheduled"));
        Text(cut.Find("#due-count")).Should().Be($"1 of 5 decisions due in {PeriodLabel} shown: Not scheduled, PAED-001.");
        Text(cut.Find("#due-items caption")).Should().Be($"Decisions due in {PeriodLabel}: Not scheduled, PAED-001");
        SummaryCells(cut, 4).Should().Equal("PAED-004 — Newborn", "1", "0", "1", "0", "0", "0", "0");
    }

    [Fact]
    public void NoMatch_SaysWhatWasAsked_WithClearFilters()
    {
        var cut = Render($"{Route}?status=NotDecided");

        Text(cut.Find("#due-count")).Should().Be($"0 of 5 decisions due in {PeriodLabel} shown.");
        var card = cut.Find("#due-no-match");
        Text(card.QuerySelector(".state-panel-title")!).Should().Be("No decision matches these filters.");
        Text(card.QuerySelector(".state-panel-copy")!).Should().Be("Not decided only, every trainee, every EPA.");
        card.QuerySelector("a")!.GetAttribute("href").Should().Be($"{Route}?period={PeriodKey}");
        cut.FindAll("#due-items").Should().BeEmpty();
    }

    // ---- The rows (C3, F1, T259, S7) -----------------------------------------------------------------------------------

    [Fact]
    public void ByDefault_WhatIsNotYetDecidedIsListed_BySurnameAsRead_TheEpaTheRowHeader_EachWithItsBadgeAndSentence()
    {
        var cut = Render();

        cut.FindAll("#due-items thead th").Select(Text).Should().Equal("EPA", "Trainee", "Window", "Status", "Actions");
        Rows(cut).Should().Equal(
            ("PAED-001 — Acute admission", "Ada Trainee", "Missed"),
            ("PAED-003 — Ward round", "Ada Trainee", "Revoked: re-decide"),
            ("PAED-004 — Newborn", "Ada Trainee", "Scheduled"),
            ("PAED-001 — Acute admission", "Lerato Late", "Not scheduled"));
        Text(cut.Find("#due-items caption")).Should().Be($"Decisions due in {PeriodLabel}, by trainee then EPA");
        Text(cut.Find("#due-count")).Should().Be($"4 of 5 decisions due in {PeriodLabel} shown.");
        cut.Find("#due-count").GetAttribute("role").Should().Be("status");

        var missed = cut.Find("#due-item-trainee-a-1");
        missed.QuerySelector(".badge")!.ClassList.Should().Contain("badge-declined");
        Text(missed.QuerySelector(".status-cell p")!).Should().Be(
            $"{PeriodLabel} has ended with nothing decided or on an open review's agenda.");
        cut.Find("#due-item-trainee-a-4 .badge").ClassList.Should().Contain("badge-draft", "Scheduled is grey on both lists (S7)");
        cut.Find("#due-item-trainee-late-1 .badge").ClassList.Should().Contain("badge-draft");
        Text(cut.Find("#due-intro")).Should().Contain($"in {PeriodLabel} for the 2 trainees you oversee at Kgosi Kgari");
    }

    [Fact]
    public void EachRow_CarriesOneAction()
    {
        var cut = Render($"{Route}?status=all");

        Actions(cut, "trainee-a-2").Should().Equal("Decided: nothing to schedule");
        Actions(cut, "trainee-a-4").Should().Equal("Open review #42");
        Actions(cut, "trainee-a-1").Should().Equal("No panel you can schedule before decides it.");
        Actions(cut, "trainee-a-3").Should().Equal("Schedule");
        Actions(cut, "trainee-late-1").Should().Equal("Schedule");
    }

    [Fact]
    public void AScheduledRow_OpensItsReview_AndItsSentenceNamesThePanel()
    {
        var cut = Render();

        var row = cut.Find("#due-item-trainee-a-4");
        var open = row.QuerySelector(".actions-cell a")!;
        (Text(open), open.GetAttribute("href"), open.GetAttribute("aria-label")).Should().Be(
            ("Open review #42", "/committee/reviews/42", "Open review #42 for Ada Trainee, PAED-004"));
        Text(row.QuerySelector(".status-cell p")!).Should().Be(
            "On the agenda of review #42 before Neonatal CCC, which is still open.");
        row.QuerySelector(".status-cell p a")!.GetAttribute("href").Should().Be("/committee/reviews/42");
    }

    [Fact]
    public void AReviewTheCallerMayOpen_IsLinkedInTheSentence_AndOneTheyMayNot_IsNamedOnly()
    {
        var cut = Render($"{Route}?status=all");

        var decided = cut.Find("#due-item-trainee-a-2 .status-cell p");
        Text(decided).Should().Be("STAR #5, issued at review #3.");
        decided.QuerySelector("a")!.GetAttribute("href").Should().Be("/committee/reviews/3");

        var revoked = cut.Find("#due-item-trainee-a-3 .status-cell p");
        Text(revoked).Should().Be($"STAR #7, issued for {_current.AcademicYear}, was revoked. Schedule a review to decide it again.");
        revoked.QuerySelectorAll("a").Should().BeEmpty();
    }

    [Fact]
    public void ARowsSchedule_FillsTheSchedulePage_WithTheRowsOwnPeriod_AndThisPageToComeBackTo()
    {
        var cut = Render($"{Route}?status=all");

        var schedule = cut.Find("#due-item-trainee-late-1 .actions-cell a");
        schedule.GetAttribute("href").Should().Be(ReviewLinks.Schedule(
            Panel, "trainee-late", PeriodKey, $"{Route}?period={PeriodKey}&status=all"));
        schedule.GetAttribute("aria-label").Should().Be("Schedule a review of Lerato Late for PAED-001 before General CCC");
    }

    [Fact]
    public void InAnEndedPeriod_EveryRowsSchedule_OpensThatPeriod_AndSaysSo()
    {
        // F1: an S2 sitting never decides what S1 missed; the form opens on the row's own ended period.
        _sender.Due = query => Task.FromResult<EntrustmentDecisionsDueDto?>(Result(query.AcademicYear, query.Semester,
        [
            Row("trainee-a", "Ada Trainee", 1, "PAED-001", "Acute admission", EntrustmentDecisionDueStatus.Missed,
                window: "2026 S1", periodKey: "2026-1", periodLabel: "2026 S1")
        ], hasEnded: true));

        var cut = Render();

        var schedule = cut.Find("#due-item-trainee-a-1 .actions-cell a");
        schedule.GetAttribute("href").Should().StartWith("/committee/reviews/new?panel=10&trainee=trainee-a&period=2026-1&return=");
        schedule.GetAttribute("aria-label").Should().Be(
            "Schedule a review of Ada Trainee for PAED-001 before General CCC, for 2026 S1");
    }

    [Fact]
    public void WhereAnOpenReviewHoldsTheSeat_TheRowOpensIt_AndItsSentenceNamesThePanelItSitsBefore()
    {
        _sender.Due = query => Task.FromResult<EntrustmentDecisionsDueDto?>(Result(query.AcademicYear, query.Semester,
        [
            Row("trainee-a", "Ada Trainee", 1, "PAED-001", "Acute admission", EntrustmentDecisionDueStatus.Revoked,
                reviewId: 3, mayOpen: true, starId: 7, holdingReviewId: 44, mayOpenHolding: true, holdingPanel: "Second CCC"),
            Row("trainee-a", "Ada Trainee", 2, "PAED-002", "Handover", EntrustmentDecisionDueStatus.NotScheduled,
                holdingReviewId: 44, mayOpenHolding: false, holdingPanel: "Second CCC"),
            Row("trainee-a", "Ada Trainee", 5, "PAED-005", "Neonatal", EntrustmentDecisionDueStatus.NotDecided,
                reviewId: 41, mayOpen: true, holdingReviewId: 44, mayOpenHolding: true, holdingPanel: "Second CCC")
        ]));

        var cut = Render();

        var revoked = cut.Find("#due-item-trainee-a-1");
        Actions(cut, "trainee-a-1").Should().Equal("Open review #44");
        revoked.QuerySelector(".actions-cell a")!.GetAttribute("aria-label").Should().Be("Open review #44 for Ada Trainee, PAED-001");
        // T259: the panel the holding review sits before, not the one routing names.
        Text(revoked.QuerySelector(".status-cell p")!).Should().Be(
            "STAR #7, issued for 2026 S2, was revoked. Review #44 is open for 2026 S2 before Second CCC: decide it there, " +
            "or ratify that review before scheduling another.");

        Actions(cut, "trainee-a-2").Should().Equal("Review #44 holds it: nothing to schedule");
        cut.FindAll("#due-item-trainee-a-2 a").Should().BeEmpty("the caller cannot open review 44, and Schedule would be refused");

        // T361, D53: a ratified review left it not decided; nothing to reinstate, and the open review holds the seat.
        var notDecided = cut.Find("#due-item-trainee-a-5");
        Actions(cut, "trainee-a-5").Should().Equal("Open review #44");
        Text(notDecided.QuerySelector(".status-cell p")!).Should().StartWith("Not decided at review #41. A later sitting for ")
            .And.NotContain("reinstate");
        notDecided.QuerySelector(".status-cell p a")!.GetAttribute("href").Should().Be("/committee/reviews/41");
    }

    // ---- The trainee line (Q2, C2) -------------------------------------------------------------------------------------

    [Fact]
    public void OneTraineeFiltered_ListsTheirRowsByEpa_WithNoTraineeColumn_AndTheLineSaysNothingHoldsThePeriod()
    {
        var cut = Render($"{Route}?trainee=trainee-late&status=all");

        cut.Find("#due-trainee").GetAttribute("value").Should().Be("trainee-late");
        cut.FindAll("#due-items thead th").Select(Text).Should().Equal("EPA", "Window", "Status", "Actions");
        Text(cut.Find("#due-items caption")).Should().Be($"Lerato Late's decisions due in {PeriodLabel}, every status, by EPA");
        Text(cut.Find("#due-count")).Should().Be($"1 of 5 decisions due in {PeriodLabel} shown: Lerato Late, every status.");

        var line = cut.Find(".trainee-line");
        Text(line.QuerySelector("p")!).Should().Be($"No open review holds {PeriodLabel}.");
        var link = line.QuerySelector("a")!;
        (Text(link), link.GetAttribute("aria-label")).Should().Be(
            ("Schedule a review", $"Schedule a review of Lerato Late for {PeriodLabel} before General CCC"));
        link.GetAttribute("href").Should().Be(ReviewLinks.Schedule(
            Panel, "trainee-late", PeriodKey, $"{Route}?period={PeriodKey}&trainee=trainee-late&status=all"));
    }

    [Fact]
    public void OneTraineeFiltered_WhoseSeatIsHeld_TheLineNamesTheReview_AndASecondPanelsLineNamesItsPanel()
    {
        var cut = Render($"{Route}?trainee=trainee-a");

        var lines = cut.FindAll(".trainee-line").ToList();
        lines.Should().HaveCount(3);
        Text(lines[0].QuerySelector("p")!).Should().Be(
            $"Review #42 (annual progression) holds {PeriodLabel} before Paed Annual Review Panel, scheduled on 2026-10-12.");
        var open = lines[0].QuerySelector("a")!;
        (Text(open), open.GetAttribute("href"), open.GetAttribute("aria-label")).Should().Be(
            ("Open review #42", "/committee/reviews/42", "Open review #42 of Ada Trainee"));

        Text(lines[1].QuerySelector("p")!).Should().Be($"No open review before Neonatal CCC holds {PeriodLabel}.");
        var schedule = lines[1].QuerySelector("a")!;
        (Text(schedule), schedule.GetAttribute("aria-label")).Should().Be(
            ("Schedule a review before Neonatal CCC", $"Schedule a review before Neonatal CCC of Ada Trainee for {PeriodLabel}"));
        schedule.GetAttribute("href").Should().StartWith($"/committee/reviews/new?panel={NeonatalPanel}&trainee=trainee-a&period={PeriodKey}");

        // A formative check-in takes no seat (S7: its link's name keeps its visible words).
        Text(lines[2].QuerySelector("p")!).Should().Be("Formative check-in #6 is open: it takes no seat.");
        var checkIn = lines[2].QuerySelector("a")!;
        (Text(checkIn), checkIn.GetAttribute("aria-label")).Should().Be(("Open check-in #6", "Open check-in #6 of Ada Trainee"));
    }

    [Fact]
    public void ATraineeTheAnswerDoesNotHold_IsNoFilter()
    {
        var cut = Render($"{Route}?trainee=someone-else");

        cut.FindAll(".trainee-line").Should().BeEmpty();
        Rows(cut).Should().HaveCount(4);
        cut.Find("#due-trainee").GetAttribute("value").Should().BeEmpty();
    }

    // ---- By EPA (M3, M8) and the pager (C9, T264, T280) ------------------------------------------------------------------

    [Fact]
    public void ByEpa_IsAFold_OfCountsTheFiltersDoNotNarrow_InAScrollableRegion()
    {
        var cut = Render($"{Route}?status=Scheduled");

        var summary = cut.Find("#due-summary-fold summary");
        Text(summary).Should().StartWith($"Counts for the 4 EPAs due in {PeriodLabel}");
        summary.QuerySelectorAll(".section-fold-show > span").Select(Text).Should().Equal("Show", "Hide");
        cut.Find("#due-summary-fold").HasAttribute("open").Should().BeFalse();
        var scroll = cut.Find("#due-summary-scroll");
        (scroll.GetAttribute("tabindex"), scroll.GetAttribute("role"), scroll.GetAttribute("aria-label"))
            .Should().Be(("0", "region", $"By EPA, {PeriodLabel}, scrollable"));
        cut.Find("#due-summary").ClassList.Should().Contain("clinic-table--stack-wide");

        SummaryCells(cut, 1).Should().Equal("PAED-001 — Acute admission", "2", "0", "0", "0", "1", "1", "0");
        SummaryCells(cut, 2).Should().Equal("PAED-002 — Handover", "1", "1", "0", "0", "0", "0", "0");
        SummaryCells(cut, 3).Should().Equal("PAED-003 — Ward round", "1", "0", "0", "0", "1", "0", "0");
        SummaryCells(cut, 4).Should().Equal("PAED-004 — Newborn", "1", "0", "1", "0", "0", "0", "0");
    }

    [Fact]
    public void ALongList_IsPaged_TheHeadingSaysThePage_AndTakesTheFocusAfterATurn()
    {
        var rows = Enumerable.Range(1, 25)
            .Select(index => Row($"trainee-{index:00}", $"Trainee {index:00}", 1, "PAED-001", "Acute admission",
                EntrustmentDecisionDueStatus.NotScheduled))
            .ToArray();
        _sender.Due = query => Task.FromResult<EntrustmentDecisionsDueDto?>(Result(query.AcademicYear, query.Semester, rows));

        var cut = Render();

        cut.FindAll("#due-items tbody tr").Should().HaveCount(20);
        Text(cut.Find("#due-list-heading")).Should().Be("Decisions, page 1 of 2");
        Text(cut.Find(".pager-info")).Should().Be("Showing 1–20 of 25");
        Text(cut.Find(".pager-page-size label")).Should().Be("Decisions per page");
        cut.Find("nav.pager").GetAttribute("aria-label").Should().Be("Decisions, pages");

        cut.FindAll(".pager-actions button").Single(button => Text(button) == "Next").Click();

        cut.FindAll("#due-items tbody tr").Should().HaveCount(5);
        Text(cut.Find("#due-list-heading")).Should().Be("Decisions, page 2 of 2");
        cut.WaitForAssertion(() => FocusedHeading(cut));
    }

    [Fact]
    public void AListOfOnePage_HeadingIsDecisions()
    {
        var cut = Render();

        Text(cut.Find("#due-list-heading")).Should().Be("Decisions");
        cut.Find("#due-list-heading").GetAttribute("tabindex").Should().Be("-1");
    }

    // ---- An Administrator (T338) ---------------------------------------------------------------------------------------

    [Fact]
    public void AnAdministrator_ChoosesTheInstitutionFirst_ThenShow()
    {
        _auth.SetRoles(WombatRoles.Administrator);
        _sender.Institutions = [new InstitutionDto(1, "Kgosi Kgari", "KGK", null, true, DateTime.UtcNow)];

        var cut = Render();

        Text(cut.Find("#due-intro")).Should().Be(
            "An administrator belongs to no institution, so the decisions due are shown for the one chosen above.");
        Text(cut.Find("#due-choose-institution")).Should().Be(
            "Choose an institution Choose an institution above, then Show, to read what its committees must decide.");
        _sender.Received.OfType<GetEntrustmentDecisionsDueQuery>().Should().BeEmpty();

        cut.Find("#due-institution").Change("1");
        _sender.Received.OfType<GetEntrustmentDecisionsDueQuery>().Should().BeEmpty("nothing acts on change");
        cut.Find("form.search-container").Submit();

        Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith($"{Route}?institution=1&period={PeriodKey}");
        cut.WaitForAssertion(() => _sender.Received.OfType<GetEntrustmentDecisionsDueQuery>().Should().ContainSingle()
            .Which.InstitutionId.Should().Be(1));
        cut.WaitForAssertion(() => cut.FindAll("#due-items").Should().ContainSingle());
    }

    [Fact]
    public void AnAdministratorsInstitutionList_ThatFails_SaysSo_WithTryAgain()
    {
        _auth.SetRoles(WombatRoles.Administrator);
        var fail = true;
        _sender.InstitutionsAnswer = () => fail
            ? throw new TimeoutException("a secret")
            : [new InstitutionDto(1, "Kgosi Kgari", "KGK", null, true, DateTime.UtcNow)];

        var cut = Render();

        var alert = cut.Find(".action-result .alert-danger");
        Text(alert.QuerySelector(".alert-row-text")!).Should().Be(
            "Could not load the institutions to choose from. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("secret");

        fail = false;
        alert.QuerySelector("button")!.Click();

        cut.WaitForAssertion(() => cut.FindAll("#due-institution option").Select(Text).Should().Equal("Select institution", "Kgosi Kgari"));
        cut.FindAll(".alert-danger").Should().BeEmpty();
    }

    // ---- Fixture ----------------------------------------------------------------------------------------------------

    private string PeriodLabel => $"{_current.AcademicYear} S{_current.Semester}";

    private string PeriodKey => $"{_current.AcademicYear}-{_current.Semester}";

    private IRenderedComponent<DecisionsDue> Render(string uri = Route)
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo(uri);
        return RenderComponent<DecisionsDue>();
    }

    private IReadOnlyList<EntrustmentDecisionDueDto> SampleRows()
    {
        var window = PeriodLabel;
        var year = $"{_current.AcademicYear}";
        return
        [
            Row("trainee-a", "Ada Trainee", 1, "PAED-001", "Acute admission", EntrustmentDecisionDueStatus.Missed, window,
                schedulePanelId: null, schedulePanelName: null, periodKey: PeriodKey, periodLabel: PeriodLabel),
            Row("trainee-a", "Ada Trainee", 2, "PAED-002", "Handover", EntrustmentDecisionDueStatus.Decided, window,
                reviewId: 3, mayOpen: true, starId: 5, periodKey: PeriodKey, periodLabel: PeriodLabel),
            Row("trainee-a", "Ada Trainee", 3, "PAED-003", "Ward round", EntrustmentDecisionDueStatus.Revoked, year,
                reviewId: 3, mayOpen: false, starId: 7, periodKey: PeriodKey, periodLabel: PeriodLabel),
            Row("trainee-a", "Ada Trainee", 4, "PAED-004", "Newborn", EntrustmentDecisionDueStatus.Scheduled, window,
                reviewId: 42, mayOpen: true, schedulePanelId: NeonatalPanel, schedulePanelName: "Neonatal CCC",
                periodKey: PeriodKey, periodLabel: PeriodLabel, reviewPanel: "Neonatal CCC"),
            Row("trainee-late", "Lerato Late", 1, "PAED-001", "Acute admission", EntrustmentDecisionDueStatus.NotScheduled, window,
                periodKey: PeriodKey, periodLabel: PeriodLabel)
        ];
    }

    private static EntrustmentDecisionDueDto Row(
        string traineeId,
        string name,
        int epaId,
        string code,
        string title,
        EntrustmentDecisionDueStatus status,
        string window = "2026 S2",
        int? reviewId = null,
        bool mayOpen = false,
        int? starId = null,
        int? schedulePanelId = Panel,
        string? schedulePanelName = "General CCC",
        string periodKey = "2026-2",
        string periodLabel = "2026 S2",
        int? holdingReviewId = null,
        bool mayOpenHolding = false,
        string? reviewPanel = null,
        string? holdingPanel = null)
        => new(traineeId, name, epaId, code, title, window, status, reviewId, mayOpen, starId, schedulePanelId, schedulePanelName,
            periodKey, periodLabel, holdingReviewId, mayOpenHolding, reviewPanel, holdingPanel);

    /// <summary>
    /// The answer: Ada's general seat held by review #42 before Paed Annual Review Panel, her neonatal seat free, and
    /// check-in #6 open; Lerato's one seat free.
    /// </summary>
    private static EntrustmentDecisionsDueDto Result(
        int year, int semester, IReadOnlyList<EntrustmentDecisionDueDto> rows, bool hasEnded = false)
        => new(1, "Kgosi Kgari", year, semester, $"{year} S{semester}", rows,
        [
            new DecisionsDueTraineeDto(
                "trainee-a",
                "Ada Trainee",
                [
                    new DecisionsDueSeatLineDto(
                        Panel, "General CCC", 42, "Annual progression", "Paed Annual Review Panel", new DateOnly(2026, 10, 12), true),
                    new DecisionsDueSeatLineDto(NeonatalPanel, "Neonatal CCC", null, null, null, null, false)
                ],
                [new DecisionsDueCheckInDto(6, true)]),
            new DecisionsDueTraineeDto(
                "trainee-late",
                "Lerato Late",
                [new DecisionsDueSeatLineDto(Panel, "General CCC", null, null, null, null, false)],
                [])
        ],
        hasEnded);

    private static IReadOnlyList<(string Epa, string Trainee, string Status)> Rows(IRenderedComponent<DecisionsDue> cut)
        => cut.FindAll("#due-items tbody tr")
            .Select(row => (
                Text(row.QuerySelector("th")!),
                Text(row.QuerySelector("td[data-label=Trainee]")!),
                Text(row.QuerySelector(".badge")!)))
            .ToArray();

    private static IReadOnlyList<string> Actions(IRenderedComponent<DecisionsDue> cut, string rowKey)
        => cut.Find($"#due-item-{rowKey} .actions-cell").Children.Select(Text).ToArray();

    private static IReadOnlyList<string> SummaryCells(IRenderedComponent<DecisionsDue> cut, int epaId)
        => cut.Find($"#due-summary-{epaId}").Children.Select(Text).ToArray();

    private void FocusedHeading(IRenderedComponent<DecisionsDue> cut)
        => JSInterop.Invocations.Where(invocation => invocation.Identifier == FocusIdentifier)
            .Should().ContainSingle()
            .Which.Arguments[0].Should().BeOfType<ElementReference>()
            .Which.Id.Should().Be(cut.Instance.DecisionsHeading.Id);

    private static string Text(IElement element) => Whitespace().Replace(element.TextContent, " ").Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    /// <summary>Answers the page's two queries, records every request, and refuses anything else.</summary>
    private sealed class DueSender : IScopedSender
    {
        public List<object> Received { get; } = [];

        public Func<GetEntrustmentDecisionsDueQuery, Task<EntrustmentDecisionsDueDto?>> Due { get; set; } =
            _ => Task.FromResult<EntrustmentDecisionsDueDto?>(null);

        public IReadOnlyList<InstitutionDto> Institutions { get; set; } = [];

        public Func<IReadOnlyList<InstitutionDto>>? InstitutionsAnswer { get; set; }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Received.Add(request);

            return request switch
            {
                GetEntrustmentDecisionsDueQuery query => (TResponse)(object)(await Due(query))!,
                GetInstitutionsListQuery => (TResponse)(object)(InstitutionsAnswer?.Invoke() ?? Institutions),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
