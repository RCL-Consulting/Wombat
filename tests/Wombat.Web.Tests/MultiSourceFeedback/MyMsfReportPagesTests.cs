using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;
using static Wombat.Web.Tests.MultiSourceFeedback.MsfReportFixtures;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// Her MSF reports (T369, flow 10, lane T; boards R3-M1 to R3-M11; round 3 F3, S6, S8; T270, T280, T323, T325, T329): the
/// list, <c>/msf/my-reports</c>, and her report on a page of its own, <c>/msf/my-reports/{id}</c>.
/// </summary>
public sealed class MyMsfReportPagesTests : WombatTestContext
{
    private readonly TestAuthorizationContext _auth;
    private readonly Sender _sender = new();

    public MyMsfReportPagesTests()
    {
        _auth = this.AddTestAuthorization();
        _auth.SetAuthorized("molefe@kgk.wombat.local");
        _auth.SetRoles(WombatRoles.Trainee);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, TraineeUserId));
        Services.AddSingleton<IScopedSender>(_sender);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private FakeNavigationManager Navigation => Services.GetRequiredService<FakeNavigationManager>();

    // ─── The list ────────────────────────────────────────────────────────────

    /// <summary>M1: none released, the empty card's words, and no table or heading of its own.</summary>
    [Fact]
    public void HerList_WithNone_SaysSo()
    {
        var cut = List();

        Text(cut.Find("h1")).Should().Be("MSF reports");
        Text(cut.Find(".page-subtitle")).Should().Be("Released aggregate feedback only. Individual responses are never shown.");
        Text(cut.Find(".detail-card--empty")).Should().Be("No released reports Released multi-source feedback will appear here.");
        cut.FindAll("table").Should().BeEmpty();
        cut.FindAll(".alert").Should().BeEmpty("no report was asked for");
    }

    /// <summary>
    /// M2 (F3): Report, Released, View, and no Responses column; captioned; the report its row's header, the template over
    /// its kind; the release in SAST; View named for its row; stacked at 900px and below (T323).
    /// </summary>
    [Fact]
    public void HerList_IsReportReleasedView_WithNoCount()
    {
        _sender.Reports = [new MsfReleasedReportDto(CampaignId, "Default MSF", MsfTemplateKind.Msf, ReleasedOn)];

        var cut = List();

        var section = cut.Find("section.list-section");
        Text(cut.Find($"#{section.GetAttribute("aria-labelledby")}")).Should().Be("Your released reports");
        var table = section.QuerySelector("table")!;
        table.ClassList.Should().Contain(["clinic-table", "clinic-table--stack", "clinic-table--stack-wide"]);
        Text(table.QuerySelector("caption")!).Should().Be("Your released MSF reports, newest first");
        table.QuerySelectorAll("thead th").Select(Text).Should().Equal("Report", "Released", "Actions");
        cut.Markup.Should().NotContain("Responses", "her list carries no count (F3)");

        var row = table.QuerySelector("tbody tr")!;
        var header = row.QuerySelector("th[scope='row']")!;
        Text(header).Should().Be("Default MSF Multi-source feedback");
        Text(header.QuerySelector(".kind-line")!).Should().Be("Multi-source feedback");
        Text(row.QuerySelector("td[data-label='Released']")!).Should().Be("2026-10-14 16:34 SAST");

        var view = row.QuerySelector("a.btn")!;
        Text(view).Should().Be("View");
        view.GetAttribute("href").Should().Be(MsfLinks.MyReport(CampaignId));
        view.GetAttribute("aria-label").Should().Be("View the Default MSF report, released 2026-10-14");
    }

    /// <summary>Ten reports (S8): one row each, in the order given (newest first), every View named apart (T239).</summary>
    [Fact]
    public void HerList_OfTen_NamesEveryViewApart()
    {
        _sender.Reports = Enumerable.Range(1, 10)
            .Select(index => new MsfReleasedReportDto(index, "Default MSF", MsfTemplateKind.Msf,
                ReleasedOn.AddDays(-index * (index % 2 == 0 ? 1 : 0))))
            .ToList();

        var cut = List();

        cut.FindAll("tbody tr").Select(row => row.Id).Should().Equal(
            Enumerable.Range(1, 10).Select(index => $"msf-my-report-row-{index}"));
        cut.FindAll("tbody a.btn").Select(view => view.GetAttribute("aria-label")).Should().OnlyHaveUniqueItems();
    }

    /// <summary>
    /// M5, M11: an id that is not one of her released reports lands on her list with one info alert above it, which takes
    /// the focus as the page loads; her list stays under it.
    /// </summary>
    [Fact]
    public void HerList_AfterAnIdNotHers_SaysSoOnce_AboveTheList()
    {
        _sender.Reports = [new MsfReleasedReportDto(CampaignId, "Default MSF", MsfTemplateKind.Msf, ReleasedOn)];
        Navigation.NavigateTo($"{MsfLinks.MyReports}?unavailable=true");

        var cut = RenderComponent<MyMsfReports>();
        cut.WaitForState(() => cut.FindAll("tbody tr").Count == 1);

        var alert = cut.Find(".alert");
        cut.FindAll(".alert").Should().ContainSingle();
        alert.ClassList.Should().Contain("alert-info");
        Text(alert).Should().Be("That report could not be found. Your released reports are listed below.");
        var region = cut.Find(".action-result");
        region.HasAttribute("autofocus").Should().BeTrue("the alert takes the focus as the page loads");
        region.GetAttribute("tabindex").Should().Be("-1");
    }

    /// <summary>M7 (T329): a failed read is fixed words with Try again, never the exception's text, never the not-found alert.</summary>
    [Fact]
    public void HerList_WhenTheReadFails_SaysSoInFixedWords_AndTryAgainReadsAgain()
    {
        _sender.ListFailure = new InvalidOperationException("developer detail");

        var cut = List();

        var alert = cut.Find(".alert-danger");
        Text(alert).Should().Be(
            "Could not load your MSF reports. Nothing has changed. Try again, or come back in a few minutes. Try again");
        cut.Markup.Should().NotContain("developer detail").And.NotContain("could not be found");

        _sender.ListFailure = null;
        _sender.Reports = [new MsfReleasedReportDto(CampaignId, "Default MSF", MsfTemplateKind.Msf, ReleasedOn)];
        cut.Find(".alert-row button").Click();
        cut.WaitForState(() => cut.FindAll("tbody tr").Count == 1);
    }

    /// <summary>M6: loading, the header at once, the section's heading, a status line and skeletons; no View yet.</summary>
    [Fact]
    public void HerList_WhileLoading_ShowsItsHeadingAndSkeletons()
    {
        _sender.HoldList = new TaskCompletionSource();

        var cut = RenderComponent<MyMsfReports>();

        Text(cut.Find("h1")).Should().Be("MSF reports");
        Text(cut.Find("p[role='status']")).Should().Be("Loading your MSF reports.");
        Text(cut.Find("section.list-section h2")).Should().Be("Your released reports");
        cut.FindAll(".skeleton").Should().NotBeEmpty();
        cut.FindAll("a.btn").Should().BeEmpty();
        _sender.HoldList.SetResult();
    }

    // ─── Her report ──────────────────────────────────────────────────────────

    /// <summary>
    /// M3: h1 and tab "Default MSF, released 2026-10-14"; the subtitle the kind and the release in SAST; Narrative; What
    /// your colleagues said, the shown groups' cards for her (no count) in the enum's order, nothing of a suppressed group;
    /// On your portfolio, each recorded EPA a link to its page.
    /// </summary>
    [Fact]
    public void HerReport_ReadsAsDrawn()
    {
        _sender.Report = Released(epas: [Recorded(Paed010), Recorded(Paed012)]);

        var cut = HerReport();

        Text(cut.Find("h1")).Should().Be("Default MSF, released 2026-10-14");
        TabTitle.Of(this, cut).Should().Be("Default MSF, released 2026-10-14 · Wombat");
        Text(cut.Find(".page-subtitle")).Should().Be("Multi-source feedback · Released 2026-10-14 16:34 SAST");

        cut.FindAll(".my-report > section h2, .my-report > section > div > h2").Select(Text).Should().Equal(
            "Narrative", "What your colleagues said", "On your portfolio");
        Text(cut.Find(".narrative")).Should().Be(Narrative);
        Text(cut.Find(".groups-intro")).Should().Be(
            "Grouped by respondent role; no one is named. Ratings are averaged from 1, Well below expectations, to 5, " +
            "Well above expectations. Comments are word for word.");
        cut.FindAll(".group-card h3").Select(Text).Should().Equal("Consultant · 4.33 average rating", "Nurse · 4.67 average rating");
        cut.Markup.Should().NotContain("Peer doctor").And.NotContain("Allied health").And.NotContain("responses")
            .And.NotContain("Suppressed");

        Text(cut.Find("#my-report-portfolio + p")).Should().Be("Recorded as evidence on 2026-10-14 16:34 SAST:");
        var links = cut.FindAll(".portfolio-links a");
        links.Select(Text).Should().Equal(
            "PAED-010 — Leading and operating within a clinical team",
            "PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams");
        links.Select(link => link.GetAttribute("href")).Should().Equal("/portfolio/progress/110", "/portfolio/progress/112");
        cut.FindAll(".alert").Should().BeEmpty();
        Navigation.History.Should().BeEmpty();
    }

    /// <summary>M4: an EPA the release did not record is left out of On your portfolio.</summary>
    [Fact]
    public void HerReport_LeavesOutAnEpaNotRecorded()
    {
        _sender.Report = Released(epas: [Recorded(Paed010), Paed012]);

        var cut = HerReport();

        cut.FindAll(".portfolio-links a").Select(Text).Should().Equal("PAED-010 — Leading and operating within a clinical team");
        cut.Markup.Should().NotContain("PAED-012");
    }

    /// <summary>M4: a report that recorded nothing, or named no EPA, says so.</summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void HerReport_ThatRecordedNothing_SaysSo(bool namedAnEpa)
    {
        _sender.Report = Released(epas: namedAnEpa ? [Paed012] : []);

        var cut = HerReport();

        Text(cut.Find("#my-report-no-evidence")).Should().Be("This report records no evidence on your portfolio.");
        cut.FindAll(".portfolio-links").Should().BeEmpty();
    }

    /// <summary>M8: fifteen EPAs on her portfolio, each title whole.</summary>
    [Fact]
    public void HerReport_ListsFifteenEpas_EachTitleWhole()
    {
        const string paed015 =
            "Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research";
        _sender.Report = Released(epas: Enumerable.Range(1, 15)
            .Select(index => Recorded(Epa(100 + index, $"PAED-{index:000}", index == 15 ? paed015 : $"Title {index}")))
            .ToList());

        var cut = HerReport();

        cut.FindAll(".portfolio-links li").Should().HaveCount(15);
        Text(cut.FindAll(".portfolio-links a").Last()).Should().Be($"PAED-015 — {paed015}");
    }

    /// <summary>M9: loading, "MSF report" in the h1 and the tab, a status line, skeletons, nothing of the report.</summary>
    [Fact]
    public void HerReport_WhileLoading_IsHeadedMsfReport()
    {
        _sender.HoldReport = new TaskCompletionSource();

        var cut = RenderComponent<MyMsfReport>(parameters => parameters.Add(page => page.CampaignId, CampaignId));

        Text(cut.Find("h1")).Should().Be("MSF report");
        TabTitle.Of(this, cut).Should().Be("MSF report · Wombat");
        Text(cut.Find("p[role='status']")).Should().Be("Loading the report.");
        cut.FindAll(".skeleton").Should().NotBeEmpty();
        _sender.HoldReport.SetResult();
    }

    /// <summary>M10: a failed read is the load error with Try again, kept apart from the not-found alert: no redirect.</summary>
    [Fact]
    public void HerReport_WhenTheReadFails_IsTheLoadError_NeverTheNotFoundAlert()
    {
        _sender.ReportFailure = new InvalidOperationException("developer detail");

        var cut = RenderComponent<MyMsfReport>(parameters => parameters.Add(page => page.CampaignId, CampaignId));
        cut.WaitForState(() => cut.FindAll(".alert-danger").Count == 1);

        Text(cut.Find("h1")).Should().Be("MSF report");
        Text(cut.Find(".alert-danger")).Should().Be(
            "Could not load this report. Nothing has changed. Try again, or come back in a few minutes. Try again");
        cut.Markup.Should().NotContain("developer detail");
        Navigation.History.Should().BeEmpty("a failed read is not an id that is not hers");

        _sender.ReportFailure = null;
        _sender.Report = Released(epas: [Recorded(Paed010)]);
        cut.Find(".alert-row button").Click();
        cut.WaitForState(() => cut.FindAll(".narrative").Count == 1);
    }

    /// <summary>M11: a report not yet released, or unknown (null), sends her to her list with the one alert.</summary>
    [Theory]
    [InlineData(MsfCampaignState.UnderReview)]
    [InlineData(null)]
    public void HerReport_NotReleasedOrUnknown_SendsHerToHerList(MsfCampaignState? state)
    {
        _sender.Report = state is { } known ? Report(known) with { IsSubjectsCopy = true } : null;

        var cut = RenderComponent<MyMsfReport>(parameters => parameters.Add(page => page.CampaignId, CampaignId));
        cut.WaitForState(() => Navigation.History.Count > 0);

        var navigation = Navigation.History.Should().ContainSingle().Subject;
        navigation.Uri.Should().Be("/msf/my-reports?unavailable=true");
        navigation.Options.ReplaceHistoryEntry.Should().BeTrue();
        cut.FindAll(".group-card, .narrative").Should().BeEmpty();
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private const string Narrative =
        "Consistently strong feedback from nursing and consultant colleagues; keep documenting escalation decisions";

    private IRenderedComponent<MyMsfReports> List()
    {
        var cut = RenderComponent<MyMsfReports>();
        cut.WaitForState(() => cut.FindAll(".skeleton").Count == 0);
        return cut;
    }

    private IRenderedComponent<MyMsfReport> HerReport()
    {
        var cut = RenderComponent<MyMsfReport>(parameters => parameters.Add(page => page.CampaignId, CampaignId));
        cut.WaitForState(() => cut.FindAll(".my-report").Count == 1);
        return cut;
    }

    private static MsfCoveredEpaDto Recorded(MsfCoveredEpaDto epa) => epa with { Recorded = true };

    /// <summary>Her copy as the handler hands it to her: released, marked hers.</summary>
    private static MsfCampaignAggregateReportDto Released(IReadOnlyList<MsfCoveredEpaDto> epas)
        => Report(MsfCampaignState.Released, epas: epas) with
        {
            CoordinatorNarrative = Narrative,
            IsSubjectsCopy = true,
            EvidenceRecordedOn = ReleasedOn
        };

    private sealed class Sender : IScopedSender
    {
        public IReadOnlyList<MsfReleasedReportDto> Reports { get; set; } = [];

        public Exception? ListFailure { get; set; }

        public TaskCompletionSource? HoldList { get; set; }

        public MsfCampaignAggregateReportDto? Report { get; set; }

        public Exception? ReportFailure { get; set; }

        public TaskCompletionSource? HoldReport { get; set; }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case ListMsfCampaignsForTraineeQuery:
                    if (HoldList is not null)
                    {
                        await HoldList.Task;
                    }

                    return ListFailure is not null ? throw ListFailure : (TResponse)(object)Reports;
                case GetCampaignAggregateReportQuery:
                    if (HoldReport is not null)
                    {
                        await HoldReport.Task;
                    }

                    return ReportFailure is not null ? throw ReportFailure : (TResponse)(object)Report!;
                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
    }
}
