using System.Security.Claims;
using System.Text.RegularExpressions;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// T131 slice 6: the decisions-due page lists, per trainee, which EPAs are due in a period and where each decision stands,
/// with a summary per EPA, filters for period, status and EPA, links to the review a status names, and a Schedule link
/// that fills in the scheduling form. Loading, error and empty are each shown.
/// </summary>
public sealed partial class DecisionsDuePageTests : WombatTestContext
{
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

        Services.AddSingleton<IScopedSender>(_sender);
        _sender.Due = query => Task.FromResult<EntrustmentDecisionsDueDto?>(Result(query.AcademicYear, query.Semester, SampleRows()));
    }

    // ---- The three states -------------------------------------------------------------------------------------------

    [Fact]
    public void WhileTheQueryRuns_ThePageShowsSkeletons()
    {
        var pending = new TaskCompletionSource<EntrustmentDecisionsDueDto?>();
        _sender.Due = _ => pending.Task;

        var cut = RenderComponent<DecisionsDue>();

        cut.FindAll(".skeleton").Should().NotBeEmpty();
        cut.FindAll("#due-items").Should().BeEmpty();

        pending.SetResult(Result(_current.AcademicYear, _current.Semester, SampleRows()));
        cut.WaitForState(() => cut.FindAll("#due-items").Count == 1, AsyncWorkTimeout);
        cut.FindAll(".skeleton").Should().BeEmpty();
    }

    [Fact]
    public void AFailedQuery_IsShownAsAnError()
    {
        _sender.Due = _ => throw new InvalidOperationException("The database is unreachable.");

        var cut = RenderComponent<DecisionsDue>();

        Text(cut.Find(".alert-danger")).Should().Be("The database is unreachable.");
        cut.FindAll("#due-items").Should().BeEmpty();
    }

    [Fact]
    public void NothingDue_ShowsTheEmptyState()
    {
        _sender.Due = query => Task.FromResult<EntrustmentDecisionsDueDto?>(Result(query.AcademicYear, query.Semester, []));

        var cut = RenderComponent<DecisionsDue>();

        var empty = Text(cut.Find(".detail-card--empty"));
        empty.Should().Contain($"Nothing due for {PeriodLabel}")
            .And.Contain($"No trainee you oversee at Kgosi Kgari owes an entrustment decision in {PeriodLabel}.");
        cut.FindAll("#due-summary").Should().BeEmpty();
    }

    // ---- What it asks, and what it shows ----------------------------------------------------------------------------

    [Fact]
    public void ItAsksForTheCurrentPeriod_WithNoInstitution_ForAnyoneButAnAdministrator()
    {
        RenderComponent<DecisionsDue>();

        var query = _sender.Received.OfType<GetEntrustmentDecisionsDueQuery>().Should().ContainSingle().Subject;
        (query.AcademicYear, query.Semester, query.InstitutionId).Should().Be((_current.AcademicYear, _current.Semester, (int?)null));
        _sender.Received.OfType<GetInstitutionsListQuery>().Should().BeEmpty();
    }

    [Fact]
    public void ByDefault_OnlyWhatIsNotYetDecidedIsListed_EachWithItsBadgeAndWhatItMeans()
    {
        var cut = RenderComponent<DecisionsDue>();

        Rows(cut).Should().Equal(
            ("Ada Trainee", "PAED-001 — Acute admission", "Missed"),
            ("Ada Trainee", "PAED-003 — Ward round", "Revoked: re-decide"),
            ("Ada Trainee", "PAED-004 — Newborn", "Scheduled"),
            ("Lerato Late", "PAED-001 — Acute admission", "Not scheduled"));

        var missed = cut.Find("#due-item-trainee-a-1");
        missed.QuerySelector(".badge")!.ClassList.Should().Contain("badge-declined");
        Text(missed.QuerySelector(".muted")!).Should().Be(
            $"{PeriodLabel} has ended with nothing decided, deferred or on an open review's agenda.");
        cut.Find("#due-item-trainee-a-3 .badge").ClassList.Should().Contain("badge-declined");
        Text(cut.Find("#due-item-trainee-a-3 .muted")).Should().Be(
            $"STAR #7, issued for {_current.AcademicYear}, was revoked. Schedule a review to decide it again.");
        cut.Find("#due-item-trainee-a-4 .badge").ClassList.Should().Contain("badge-submitted");
        cut.Find("#due-item-trainee-late-1 .badge").ClassList.Should().Contain("badge-draft");
        Text(cut.Find("#due-intro")).Should().Contain($"in {PeriodLabel} for the 2 trainees you oversee at Kgosi Kgari");
    }

    [Fact]
    public void AnOutstandingRow_LinksToScheduleTheTraineeBeforeThePanelTheEpaRoutesTo_ForThePeriod()
    {
        var cut = RenderComponent<DecisionsDue>();

        var schedule = cut.Find("#due-item-trainee-late-1 a[href^='/committee/reviews?']");
        schedule.TextContent.Trim().Should().Be("Schedule");
        schedule.GetAttribute("href").Should().Be(
            $"/committee/reviews?panel={Panel}&trainee=trainee-late&period={_current.AcademicYear}-{_current.Semester}");
        schedule.GetAttribute("aria-label").Should().Be("Schedule a review of Lerato Late for PAED-001 before General CCC");
    }

    [Fact]
    public void AnAnnualRow_SchedulesTheYearsLastSitting()
    {
        var cut = RenderComponent<DecisionsDue>();

        cut.Find("#due-item-trainee-a-3 a[href^='/committee/reviews?']").GetAttribute("href").Should().Be(
            $"/committee/reviews?panel={Panel}&trainee=trainee-a&period={_current.AcademicYear}-2");
    }

    [Fact]
    public void WhereAnOpenReviewHoldsTheSeat_TheRowPointsToIt_InsteadOfOfferingSchedule()
    {
        _sender.Due = query => Task.FromResult<EntrustmentDecisionsDueDto?>(Result(query.AcademicYear, query.Semester,
        [
            Row("trainee-a", "Ada Trainee", 1, "PAED-001", "Acute admission", EntrustmentDecisionDueStatus.Revoked,
                reviewId: 3, mayOpen: true, starId: 7, holdingReviewId: 44, mayOpenHolding: true),
            Row("trainee-a", "Ada Trainee", 2, "PAED-002", "Handover", EntrustmentDecisionDueStatus.NotScheduled,
                holdingReviewId: 44, mayOpenHolding: false),
            Row("trainee-a", "Ada Trainee", 5, "PAED-005", "Neonatal", EntrustmentDecisionDueStatus.Deferred,
                reviewId: 44, mayOpen: true, holdingReviewId: 44, mayOpenHolding: true)
        ]));

        var cut = RenderComponent<DecisionsDue>();

        var revoked = cut.Find("#due-item-trainee-a-1");
        revoked.QuerySelectorAll("a").Select(link => (Text(link), link.GetAttribute("href"))).Should().Equal(
            ("Open review", "/committee/reviews/3"), ("Open review #44", "/committee/reviews/44"));
        revoked.QuerySelectorAll("a")[1].GetAttribute("aria-label").Should().Be(
            "Open review #44, open for 2026 S2, to decide PAED-001 for Ada Trainee");
        Text(revoked.QuerySelector(".muted")!).Should().Be(
            "STAR #7, issued for 2026 S2, was revoked. Review #44 is open for 2026 S2 before General CCC: decide it there, " +
            "or ratify that review before scheduling another.");

        var unreadable = cut.Find("#due-item-trainee-a-2");
        unreadable.QuerySelectorAll("a").Should().BeEmpty("the caller cannot open review 44, and Schedule would be refused");
        Text(unreadable.QuerySelector(".muted")!).Should().Contain("Review #44 is open for 2026 S2 before General CCC");

        var deferredThere = cut.Find("#due-item-trainee-a-5");
        deferredThere.QuerySelectorAll("a").Select(link => Text(link)).Should().Equal("Open review");
        Text(deferredThere.QuerySelector(".muted")!).Should().Be(
            "Deferred at review #44, which is still open: its chair can reinstate it there.");
    }

    [Fact]
    public void TheCountOfRowsShown_IsAStatusRegion_ThatFollowsTheFilters()
    {
        var cut = RenderComponent<DecisionsDue>();

        var count = cut.Find("#due-count");
        count.GetAttribute("role").Should().Be("status");
        Text(count).Should().Be($"4 of 5 decisions due in {PeriodLabel} shown.");

        cut.Find("#due-status").Change("all");
        Text(cut.Find("#due-count")).Should().Be($"5 of 5 decisions due in {PeriodLabel} shown.");
    }

    [Fact]
    public void TheSummarysScrollContainer_CanTakeTheKeyboardFocus()
    {
        var cut = RenderComponent<DecisionsDue>();

        var scroll = cut.Find("#due-summary-scroll");
        (scroll.GetAttribute("tabindex"), scroll.GetAttribute("role"), scroll.GetAttribute("aria-labelledby"))
            .Should().Be(("0", "region", "due-summary-heading"));
    }

    [Fact]
    public void AScheduledRow_LinksToItsOpenReview_AndOffersNoSchedule()
    {
        var cut = RenderComponent<DecisionsDue>();

        var row = cut.Find("#due-item-trainee-a-4");
        row.QuerySelectorAll("a").Select(link => (link.TextContent.Trim(), link.GetAttribute("href")))
            .Should().Equal(("Open review", "/committee/reviews/42"));
        Text(row.QuerySelector(".muted")!).Should().Be("On the agenda of review #42, which is still open.");
    }

    [Fact]
    public void AReviewTheCallerCannotOpen_IsNamedButNotLinked()
    {
        var cut = RenderComponent<DecisionsDue>();

        var row = cut.Find("#due-item-trainee-a-3");
        row.QuerySelectorAll("a").Select(link => link.TextContent.Trim()).Should().Equal("Schedule");
        Text(row).Should().Contain("STAR #7");
    }

    [Fact]
    public void WhereNoPanelTheCallerCanScheduleBeforeDecidesIt_ThePageSaysSo()
    {
        var cut = RenderComponent<DecisionsDue>();

        var row = cut.Find("#due-item-trainee-a-1");
        row.QuerySelectorAll("a").Should().BeEmpty();
        Text(row).Should().Contain("No panel you can schedule before decides it.");
    }

    [Fact]
    public void EveryStatus_ShowsTheDecidedRowsToo_AndAStatusShowsOnlyItself()
    {
        var cut = RenderComponent<DecisionsDue>();

        cut.Find("#due-status").Change("all");
        Rows(cut).Should().HaveCount(5).And.Contain(("Ada Trainee", "PAED-002 — Handover", "Decided"));
        Text(cut.Find("#due-item-trainee-a-2 .muted")).Should().Be("STAR #5, issued at review #3.");
        cut.Find("#due-item-trainee-a-2 a").GetAttribute("href").Should().Be("/committee/reviews/3");

        cut.Find("#due-status").Change(nameof(EntrustmentDecisionDueStatus.Missed));
        Rows(cut).Should().Equal(("Ada Trainee", "PAED-001 — Acute admission", "Missed"));

        cut.Find("#due-status").Change(nameof(EntrustmentDecisionDueStatus.Deferred));
        Text(cut.Find("#due-no-match")).Should().Contain("No decision matches the filters");
    }

    [Fact]
    public void AnEpa_NarrowsTheListToIt()
    {
        var cut = RenderComponent<DecisionsDue>();

        cut.FindAll("#due-epa option").Select(option => option.TextContent.Trim()).Should().Equal(
            "Every EPA", "PAED-001 — Acute admission", "PAED-002 — Handover", "PAED-003 — Ward round", "PAED-004 — Newborn");

        cut.Find("#due-epa").Change("1");

        Rows(cut).Select(row => row.Trainee).Should().Equal("Ada Trainee", "Lerato Late");
    }

    [Fact]
    public void TheSummary_CountsEachEpa_WhateverTheFilters()
    {
        var cut = RenderComponent<DecisionsDue>();
        cut.Find("#due-status").Change(nameof(EntrustmentDecisionDueStatus.Scheduled));

        SummaryCells(cut, 1).Should().Equal("PAED-001 — Acute admission", "2", "0", "0", "0", "1", "1", "0");
        SummaryCells(cut, 2).Should().Equal("PAED-002 — Handover", "1", "1", "0", "0", "0", "0", "0");
        SummaryCells(cut, 3).Should().Equal("PAED-003 — Ward round", "1", "0", "0", "0", "1", "0", "0");
        SummaryCells(cut, 4).Should().Equal("PAED-004 — Newborn", "1", "0", "1", "0", "0", "0", "0");
    }

    [Fact]
    public void ChoosingAnotherPeriod_AsksForIt()
    {
        var cut = RenderComponent<DecisionsDue>();
        var earliest = CommitteeReviewPeriods.Around(QuotaCalendar.Today())[0];

        cut.Find("#due-period").Change(earliest.Key);

        var query = _sender.Received.OfType<GetEntrustmentDecisionsDueQuery>().Last();
        (query.AcademicYear, query.Semester).Should().Be((earliest.AcademicYear, earliest.Semester));
        Text(cut.Find("#due-intro")).Should().Contain($"in {earliest.AcademicYear} S{earliest.Semester}");
    }

    [Fact]
    public void ALongList_IsPaged()
    {
        var rows = Enumerable.Range(1, 25)
            .Select(index => Row($"trainee-{index:00}", $"Trainee {index:00}", 1, "PAED-001", "Acute admission",
                EntrustmentDecisionDueStatus.NotScheduled))
            .ToArray();
        _sender.Due = query => Task.FromResult<EntrustmentDecisionsDueDto?>(Result(query.AcademicYear, query.Semester, rows));

        var cut = RenderComponent<DecisionsDue>();

        cut.FindAll("#due-items tbody tr").Should().HaveCount(20);
        Text(cut.Find(".pager-info")).Should().Be("Showing 1-20 of 25");
        cut.FindAll(".pager-actions button").Single(button => button.TextContent.Trim() == "Next").Click();
        cut.FindAll("#due-items tbody tr").Should().HaveCount(5);
    }

    [Fact]
    public void AnAdministrator_ChoosesTheInstitutionFirst()
    {
        _auth.SetRoles(WombatRoles.Administrator);
        _sender.Institutions = [new InstitutionDto(1, "Kgosi Kgari", "KGK", null, true, DateTime.UtcNow)];

        var cut = RenderComponent<DecisionsDue>();

        cut.Find("#due-choose-institution").TextContent.Should().Contain("Choose an institution");
        _sender.Received.OfType<GetEntrustmentDecisionsDueQuery>().Should().BeEmpty();

        cut.Find("#due-institution").Change("1");

        _sender.Received.OfType<GetEntrustmentDecisionsDueQuery>().Should().ContainSingle()
            .Which.InstitutionId.Should().Be(1);
        cut.FindAll("#due-items").Should().ContainSingle();
    }

    // ---- Fixture ----------------------------------------------------------------------------------------------------

    private string PeriodLabel => $"{_current.AcademicYear} S{_current.Semester}";

    private string PeriodKey => $"{_current.AcademicYear}-{_current.Semester}";

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
                reviewId: 3, mayOpen: false, starId: 7, periodKey: $"{year}-2", periodLabel: $"{year} S2"),
            Row("trainee-a", "Ada Trainee", 4, "PAED-004", "Newborn", EntrustmentDecisionDueStatus.Scheduled, window,
                reviewId: 42, mayOpen: true, schedulePanelId: NeonatalPanel, schedulePanelName: "Neonatal CCC",
                periodKey: PeriodKey, periodLabel: PeriodLabel),
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
        bool mayOpenHolding = false)
        => new(traineeId, name, epaId, code, title, window, status, reviewId, mayOpen, starId, schedulePanelId, schedulePanelName,
            periodKey, periodLabel, holdingReviewId, mayOpenHolding);

    private static EntrustmentDecisionsDueDto Result(int year, int semester, IReadOnlyList<EntrustmentDecisionDueDto> rows)
        => new(1, "Kgosi Kgari", year, semester, $"{year} S{semester}", rows);

    private static IReadOnlyList<(string Trainee, string Epa, string Status)> Rows(IRenderedComponent<DecisionsDue> cut)
        => cut.FindAll("#due-items tbody tr")
            .Select(row => (
                Text(row.QuerySelector("th")!),
                Text(row.QuerySelectorAll("td")[0]),
                Text(row.QuerySelector(".badge")!)))
            .ToArray();

    private static IReadOnlyList<string> SummaryCells(IRenderedComponent<DecisionsDue> cut, int epaId)
        => cut.Find($"#due-summary-{epaId}").Children.Select(Text).ToArray();

    private static string Text(AngleSharp.Dom.IElement element) => Whitespace().Replace(element.TextContent, " ").Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    /// <summary>Answers the page's two queries, records every request, and refuses anything else.</summary>
    private sealed class DueSender : IScopedSender
    {
        public List<object> Received { get; } = [];

        public Func<GetEntrustmentDecisionsDueQuery, Task<EntrustmentDecisionsDueDto?>> Due { get; set; } =
            _ => Task.FromResult<EntrustmentDecisionsDueDto?>(null);

        public IReadOnlyList<InstitutionDto> Institutions { get; set; } = [];

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Received.Add(request);

            return request switch
            {
                GetEntrustmentDecisionsDueQuery query => (TResponse)(object)(await Due(query))!,
                GetInstitutionsListQuery => (TResponse)(object)Institutions,
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
