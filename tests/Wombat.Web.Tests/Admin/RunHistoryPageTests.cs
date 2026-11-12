using System.Reflection;
using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Scheduling;
using Wombat.Application.Features.Scheduling.Queries.GetScheduledJobRuns;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Jobs;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// Run history (T400, flow 18, lane B; R3-R01–R09, R2-R03, R08, R10, R11): the Job select, Status and the South African
/// days, Show and Clear filters, the address read and written, the count line and its focus, the pager and its focus,
/// the error whole, Ran by by name, the two empty states, and the loading and load error.
/// </summary>
public sealed class RunHistoryPageTests : TestContext
{
    private static readonly string[] Keys =
    [
        "activity-draft-nudge", "assessor-pending-nudge", "audit-log-retention", "entrustment-decision-expiry",
        "msf-campaign-auto-close", "msf-invitation-expiry-reminder", "scheduled-job-run-retention", "weekly-coordinator-digest"
    ];

    // 2026-11-10 21:41:44 SAST.
    private static readonly DateTime Started = new(2026, 11, 10, 19, 41, 44, DateTimeKind.Utc);

    private readonly FakeSender _sender = new();

    public RunHistoryPageTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("devadmin@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<IScopedSender>(_sender);
    }

    [Fact]
    public void TheList_ItsFilters_ItsCountLine_AndItsRows_AsR3R01DrawsThem()
    {
        _sender.Answer = _ => Page(
        [
            Run(1, "scheduled-job-run-retention", Started, 7, "Succeeded", ranBy: "Demo Administrator"),
            Run(2, "msf-campaign-auto-close", Started.AddMinutes(-41), 29, "Succeeded")
        ], total: 21, all: 21);

        var cut = Render();

        cut.Find("div.people-page.job-runs-page").Should().NotBeNull();
        Text(cut.Find(".page-subtitle")).Should().Be("Every run of the eight jobs, newest first, kept for 90 days.");
        cut.FindAll(".header-container a").Should().BeEmpty("no Back to jobs (T1)");
        cut.Markup.Should().NotContain("Back to jobs").And.NotContain("Job run history").And.NotContain("Triggered by");

        var form = cut.Find("form.search-container.people-filter");
        form.GetAttribute("role").Should().Be("search");
        form.GetAttribute("aria-label").Should().Be("Filter the runs");
        form.QuerySelectorAll("label").Select(Text).Should().Equal(
            "Job", "Status", "From (a South African day)", "To (a South African day)");
        cut.FindAll("#runs-job option").Select(Text).Should().Equal(new[] { "All jobs" }.Concat(Keys));
        cut.FindAll("#runs-status option").Select(Text).Should().Equal("All", "Running", "Succeeded", "Failed");
        cut.Find("#runs-from").GetAttribute("type").Should().Be("date");
        form.QuerySelectorAll(".filter-actions button").Select(Text).Should().Equal(new[] { "Show" }, "Clear filters only once a filter is applied");

        var heading = cut.Find("h2#runs-heading");
        heading.ClassList.Should().Contain("list-section-title");
        heading.GetAttribute("tabindex").Should().Be("-1");
        Text(heading).Should().Be("Runs");
        var count = cut.Find("p#runs-count");
        Text(count).Should().Be("21 runs.");
        count.GetAttribute("tabindex").Should().Be("-1");
        count.HasAttribute("role").Should().BeFalse("the count line is no live region (N2)");

        cut.Find("table").ClassList.Should().Contain("clinic-table--stack-wide");
        Text(cut.Find("caption")).Should().Be("Runs, newest first");
        cut.FindAll("thead th").Select(Text).Should().Equal("Job", "Started", "Duration", "Status", "Ran by");
        var first = cut.FindAll("tbody tr").ToList()[0];
        Text(first.QuerySelector("th[scope=row]")!).Should().Be("scheduled-job-run-retention");
        first.QuerySelectorAll("td[data-label=Started] .nowrap").Select(Text).Should().Equal(
            new[] { "2026-11-10 21:41:44 SAST" }, "a time never breaks: the day and the hour are one unit (T400's replay)");
        Text(first.QuerySelector("td[data-label=Duration]")!).Should().Be("7 ms");
        Text(first.QuerySelector("td[data-label=Status] .badge")!).Should().Be("Succeeded");
        Text(first.QuerySelector("td[data-label='Ran by']")!).Should().Be("Demo Administrator");
        Text(cut.FindAll("tbody tr").ToList()[1].QuerySelector("td[data-label='Ran by']")!).Should().Be("Scheduler");
        cut.FindAll("tbody td").Should().OnlyContain(cell => cell.HasAttribute("data-label"));

        Text(cut.Find(".pager .pager-info")).Should().Be("Showing 1–21 of 21", "the pager counts the total");
        cut.Markup.Should().NotContain("scheduler<").And.NotContain("UTC");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void ARunningRun_HasADash_AndAFailedRun_ItsErrorWhole_UnderItsBadge()
    {
        var error = string.Join(' ', Enumerable.Repeat("The mail server refused the message with a long reason.", 30));
        _sender.Answer = _ => Page(
        [
            Run(1, "audit-log-retention", Started, null, "Running"),
            Run(2, "entrustment-decision-expiry", Started.AddMinutes(-1), 29, "Failed", error: error, ranBy: "Demo Administrator")
        ]);

        var cut = Render();

        var running = cut.FindAll("tbody tr").ToList()[0];
        Text(running.QuerySelector("td[data-label=Duration]")!).Should().Be("—");
        Text(running.QuerySelector("td[data-label=Status]")!).Should().Be("Running", "once, the badge (N9)");
        running.QuerySelector("td[data-label=Status] .badge")!.ClassList.Should().Contain("badge-accepted");

        var failed = cut.FindAll("tbody tr").ToList()[1];
        failed.QuerySelector("td[data-label=Status] .text-sm")!.TextContent.Should().Be(error, "whole, never cut at 80 (T1)");
        failed.QuerySelector("[title]").Should().BeNull("no tooltip");
        cut.Markup.Should().NotContain("Error: None");
    }

    [Fact]
    public void TheAddress_SetsTheFilters_AndIsWhatIsAsked()
    {
        _sender.Answer = _ => Page([Run(1, "entrustment-decision-expiry", Started, 29, "Failed", error: "x")]);

        var cut = Render(JobLinks.RunsOf("entrustment-decision-expiry", "Failed") + "&from=2026-11-01&to=2026-11-10&page=2");

        var asked = _sender.Queries.Last();
        asked.Should().Be(new GetScheduledJobRunsQuery(
            "entrustment-decision-expiry", "Failed", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 10), 2, 50));
        cut.Find("#runs-job").GetAttribute("value").Should().Be("entrustment-decision-expiry");
        cut.Find("#runs-status").GetAttribute("value").Should().Be("Failed");
        cut.Find("#runs-from").GetAttribute("value").Should().Be("2026-11-01");
        cut.Find("#runs-to").GetAttribute("value").Should().Be("2026-11-10");
        cut.FindAll(".filter-actions button").Select(Text).Should().Equal("Show", "Clear filters");
    }

    /// <summary>
    /// T400's replay: on a full load and through Home's "Its runs", the list was the address's job's while the select showed
    /// activity-draft-nudge. Before the read the options are "All jobs" and the address's job, which the browser selects;
    /// the read puts the stored keys before it. bUnit re-renders the markup whole, so it shows the job selected either way:
    /// what decides the browser is whether the diff keeps that option element. Unkeyed, it rewrote the second option's
    /// value to the first stored key, and the browser kept that element selected. This holds the option to its key.
    /// </summary>
    [Fact]
    public void TheAddressesJob_IsTheSameOption_AfterTheStoredKeysArrive_SoTheSelectStillShowsIt()
    {
        var pending = new TaskCompletionSource<ScheduledJobRunsPageDto>();
        _sender.Pending = pending.Task;
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo(JobLinks.RunsOf("entrustment-decision-expiry", "Failed"));
        var cut = RenderComponent<ScheduledJobRunsList>();

        var before = JobOptions(cut);
        before.Select(option => option.Value).Should().Equal("", "entrustment-decision-expiry");

        cut.InvokeAsync(() => pending.SetResult(Page([Run(1, "entrustment-decision-expiry", Started, 29, "Failed", error: "x")])));
        cut.WaitForState(() => cut.FindAll("p.loading-line").Count == 0);

        var after = JobOptions(cut);
        after.Select(option => option.Value).Should().Equal(new[] { "" }.Concat(Keys));
        cut.Find("#runs-job").GetAttribute("value").Should().Be("entrustment-decision-expiry");

        // The element the browser selected before the read is, after it, the one carrying the same key and value.
        var chosen = before.Single(option => option.Value == "entrustment-decision-expiry");
        chosen.Key.Should().Be("entrustment-decision-expiry", "an unkeyed option is matched by its place, and its value rewritten");
        after.Where(option => Equals(option.Key, chosen.Key)).Select(option => option.Value)
            .Should().Equal(new[] { "entrustment-decision-expiry" }, "the diff keeps the chosen element, still holding its job");
        after.Skip(1).Should().OnlyContain(option => Equals(option.Key, option.Value), "every job's option is keyed by its job");
    }

    [Fact]
    public void AnAddressItCannotRead_AsksForEveryRun()
    {
        _sender.Answer = _ => Page([Run(1, "a", Started, 1, "Succeeded")]);

        Render("/admin/jobs/runs?status=2&from=10-11-2026&page=-3&size=7");

        _sender.Queries.Last().Should().Be(new GetScheduledJobRunsQuery(null, null, null, null, 1, 50));
    }

    [Fact]
    public void Show_GoesToThePagesOwnAddress_WithWhatWasChosen_AndNothingIsAskedBeforeIt()
    {
        _sender.Answer = _ => Page([Run(1, "a", Started, 1, "Succeeded")]);
        var cut = Render();

        cut.Find("#runs-job").Change("weekly-coordinator-digest");
        cut.Find("#runs-status").Change("Succeeded");
        cut.Find("#runs-from").Change("2026-11-10");
        _sender.Queries.Should().HaveCount(1, "a field's change sends nothing");

        cut.Find("form.people-filter").Submit();

        new Uri(Services.GetRequiredService<FakeNavigationManager>().Uri).PathAndQuery
            .Should().Be("/admin/jobs/runs?job=weekly-coordinator-digest&status=Succeeded&from=2026-11-10");
        cut.WaitForAssertion(() => _sender.Queries.Last().Should().Be(
            new GetScheduledJobRunsQuery("weekly-coordinator-digest", "Succeeded", new DateOnly(2026, 11, 10), null, 1, 50)));
        cut.WaitForAssertion(() => FocusedId().Should().Be("runs-count", "Show moves the focus to the count line (R3-R02)"));
    }

    [Fact]
    public void Show_OnTheAddressAlreadyShown_ReadsAgainInPlace_AndFocusesTheCountLine()
    {
        _sender.Answer = _ => Page([Run(1, "a", Started, 1, "Succeeded")], total: 1, all: 1);
        var cut = Render();

        cut.Find("form.people-filter").Submit();

        cut.WaitForAssertion(() => _sender.Queries.Should().HaveCount(2));
        Text(cut.Find("p#runs-count")).Should().Be("1 run.");
        cut.WaitForAssertion(() => FocusedId().Should().Be("runs-count"));
    }

    [Fact]
    public void ClearFilters_GoesToTheBareAddress_AndFocusesJob()
    {
        _sender.Answer = _ => Page([Run(1, "a", Started, 1, "Succeeded")]);
        var cut = Render(JobLinks.RunsOf("weekly-coordinator-digest", null));

        cut.FindAll(".filter-actions button").Single(button => Text(button) == "Clear filters").Click();

        new Uri(Services.GetRequiredService<FakeNavigationManager>().Uri).PathAndQuery.Should().Be("/admin/jobs/runs");
        cut.WaitForAssertion(() => FocusedId().Should().Be("runs-job"));
    }

    [Fact]
    public void NoMatch_SaysSo_InTheCountLineAndACard_AndClearTheFilters_ClearsThem()
    {
        _sender.Answer = query => query.Status is null ? Page([Run(1, "a", Started, 1, "Succeeded")], total: 21, all: 21) : Page([], total: 0, all: 21);
        var cut = Render(JobLinks.RunsOf(null, "Failed"));

        Text(cut.Find("p#runs-count")).Should().Be("No runs match these filters.");
        var card = cut.Find(".detail-card--empty");
        Text(card.QuerySelector(".state-panel-title")!).Should().Be("No runs match");
        Text(card.QuerySelector(".state-panel-copy")!).Should().Be("The filters are kept above. Clear them to see all 21.");
        cut.FindAll("table").Should().BeEmpty();
        cut.FindAll(".pager").Should().BeEmpty();

        card.QuerySelector("button.btn-primary")!.Click();

        new Uri(Services.GetRequiredService<FakeNavigationManager>().Uri).PathAndQuery.Should().Be("/admin/jobs/runs");
        cut.WaitForAssertion(() => FocusedId().Should().Be("runs-job", "Clear the filters moves the focus to Job (R2-R03)"));
    }

    [Fact]
    public void NoRunsAtAll_IsNoJobHasRunYet_WithNoCountLine_NoTable_AndNoPager()
    {
        _sender.Answer = _ => Page([], total: 0, all: 0);
        var cut = Render();

        Text(cut.Find(".detail-card--empty .state-panel-title")).Should().Be("No job has run yet");
        Text(cut.Find(".detail-card--empty .state-panel-copy")).Should().Be(
            "The scheduler runs each job at its time; Scheduled jobs shows when each runs next.");
        cut.FindAll("#runs-count").Should().BeEmpty();
        cut.FindAll("table").Should().BeEmpty();
        cut.FindAll(".pager").Should().BeEmpty();
    }

    [Fact]
    public void ThePager_IsFiftyAPage_AndATurn_GoesToItsAddress_AndFocusesRuns()
    {
        var rows = Enumerable.Range(1, 50).Select(n => Run(n, "msf-campaign-auto-close", Started.AddHours(-n), 30, "Succeeded")).ToList();
        _sender.Answer = query => Page(rows, total: 2800, all: 2800, page: query.Page);
        var cut = Render();

        Text(cut.Find(".pager .pager-info")).Should().Be("Showing 1–50 of 2800");
        cut.Find(".pager-page-size-select").GetAttribute("value").Should().Be("50");
        Text(cut.Find("p#runs-count")).Should().Be("2,800 runs.");

        cut.FindAll(".pager-actions button").Single(button => Text(button) == "Next").Click();

        new Uri(Services.GetRequiredService<FakeNavigationManager>().Uri).PathAndQuery.Should().Be("/admin/jobs/runs?page=2");
        cut.WaitForAssertion(() => _sender.Queries.Last().Page.Should().Be(2));
        cut.WaitForAssertion(() => FocusedId().Should().Be("runs-heading", "a page turn focuses \"Runs\" (N1)"));
        Text(cut.Find(".pager .pager-info")).Should().Be("Showing 51–100 of 2800");
    }

    [Fact]
    public void OneRun_IsTheSingular()
    {
        _sender.Answer = _ => Page([Run(1, "a", Started, 1, "Succeeded")], total: 1, all: 5);

        Text(Render().Find("p#runs-count")).Should().Be("1 run.");
    }

    [Fact]
    public void WhileTheRunsLoad_TheFiltersStay_ShowWaits_AndTheLoadingLineSaysWhat()
    {
        var pending = new TaskCompletionSource<ScheduledJobRunsPageDto>();
        _sender.Answer = _ => throw new InvalidOperationException("not used");
        _sender.Pending = pending.Task;
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo(JobLinks.Runs);
        var cut = RenderComponent<ScheduledJobRunsList>();

        Text(cut.Find("p.loading-line")).Should().Be("Loading the run history.");
        cut.Find("p.loading-line").GetAttribute("role").Should().Be("status");
        cut.Find("form.people-filter").Should().NotBeNull("the filters stay drawn (R2-R10)");
        var show = cut.Find(".filter-actions button[type=submit]");
        show.GetAttribute("aria-disabled").Should().Be("true");
        Text(cut.Find("h2#runs-heading")).Should().Be("Runs");

        cut.Find("form.people-filter").Submit();
        _sender.Queries.Should().HaveCount(1, "Show waits for the first read");
        new Uri(Services.GetRequiredService<FakeNavigationManager>().Uri).PathAndQuery.Should().Be("/admin/jobs/runs");
    }

    [Fact]
    public void AFailedRead_IsTheHouseSentence_UnderTheFilters_WithTryAgain()
    {
        var calls = 0;
        _sender.Answer = _ => ++calls == 1
            ? throw new InvalidOperationException("Npgsql: connection refused at 10.0.0.1")
            : Page([Run(1, "a", Started, 1, "Succeeded")]);
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo(JobLinks.Runs);
        var cut = RenderComponent<ScheduledJobRunsList>();

        var alert = cut.WaitForElement(".alert-danger");
        Text(alert.QuerySelector(".alert-row-text")!).Should().Be(
            "Could not load the run history. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("Npgsql");
        cut.Find("form.people-filter").Should().NotBeNull("the filters stay (R2-R11)");
        cut.FindAll(".detail-card--empty").Should().BeEmpty("nothing under the alert");

        cut.FindAll("button").Single(button => Text(button) == "Try again").Click();

        cut.WaitForState(() => cut.FindAll("tbody tr").Count == 1);
        JSInterop.Invocations.Should().Contain(call => call.Identifier == PageFocus.FocusHeadingIdentifier);
    }

    [Fact]
    public void RunHistoryAddress_LeavesOutWhatIsNotAsked()
    {
        RunHistoryAddress.None.Address().Should().Be("/admin/jobs/runs");
        (RunHistoryAddress.None with { Job = "a job", Status = "Failed", FromDay = new DateOnly(2026, 11, 1), Page = 3, PageSize = 100 })
            .Address().Should().Be("/admin/jobs/runs?job=a%20job&status=Failed&from=2026-11-01&page=3&size=100");
    }

    // ---- helpers ----

    private IRenderedComponent<ScheduledJobRunsList> Render(string address = JobLinks.Runs)
    {
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo(address);
        var cut = RenderComponent<ScheduledJobRunsList>();
        cut.WaitForState(() => cut.FindAll("p.loading-line").Count == 0);
        return cut;
    }

    /// <summary>
    /// The Job select's options as the renderer holds them, each its <c>@key</c> and value: what the browser's diff matches
    /// elements by, which the markup does not show.
    /// </summary>
    /// <remarks>The render tree's types are the framework's own (BL0006), read here on purpose, as MainLayoutTests does.</remarks>
#pragma warning disable BL0006
    private List<(object? Key, string Value)> JobOptions(IRenderedComponent<ScheduledJobRunsList> cut)
    {
        var method = typeof(Microsoft.AspNetCore.Components.RenderTree.Renderer)
            .GetMethod("GetCurrentRenderTreeFrames", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
        var frames = (ArrayRange<RenderTreeFrame>)method.Invoke(Renderer, [cut.ComponentId])!;
        var options = new List<(object? Key, string Value)>();
        var inSelect = false;
        var selectEnd = 0;
        for (var i = 0; i < frames.Count; i++)
        {
            var frame = frames.Array[i];
            if (frame.FrameType != RenderTreeFrameType.Element)
            {
                continue;
            }

            if (frame.ElementName == "select" && !inSelect && HasAttribute(frames, i, "id", "runs-job"))
            {
                inSelect = true;
                selectEnd = i + frame.ElementSubtreeLength;
            }
            else if (inSelect && i < selectEnd && frame.ElementName == "option")
            {
                options.Add((frame.ElementKey, AttributeOf(frames, i, "value") ?? string.Empty));
            }
        }

        return options;
    }

    private static bool HasAttribute(ArrayRange<RenderTreeFrame> frames, int element, string name, string value)
        => AttributeOf(frames, element, name) == value;

    private static string? AttributeOf(ArrayRange<RenderTreeFrame> frames, int element, string name)
    {
        for (var i = element + 1; i < frames.Count && frames.Array[i].FrameType == RenderTreeFrameType.Attribute; i++)
        {
            if (frames.Array[i].AttributeName == name)
            {
                // An empty value is compiled as a bare attribute: true.
                return frames.Array[i].AttributeValue is true ? string.Empty : frames.Array[i].AttributeValue?.ToString();
            }
        }

        return null;
    }
#pragma warning restore BL0006

    private string? FocusedId()
        => JSInterop.Invocations.LastOrDefault(call => call.Identifier == PageFocus.FocusByIdIdentifier).Arguments?.FirstOrDefault() as string;

    private static string Text(IElement element)
        => string.Join(' ', element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static ScheduledJobRunsPageDto Page(
        IReadOnlyList<ScheduledJobRunDto> runs, int? total = null, int? all = null, int page = 1)
        => new(runs, page, 50, total ?? runs.Count, all ?? total ?? runs.Count, Keys);

    private static ScheduledJobRunDto Run(int id, string key, DateTime started, int? milliseconds, string status, string? error = null, string? ranBy = null)
        => new(id, key, started, milliseconds is { } ms ? started.AddMilliseconds(ms) : null, status, error, ranBy);

    private sealed class FakeSender : IScopedSender
    {
        public Func<GetScheduledJobRunsQuery, ScheduledJobRunsPageDto> Answer { get; set; } = _ => throw new InvalidOperationException();

        public Task<ScheduledJobRunsPageDto>? Pending { get; set; }

        public List<GetScheduledJobRunsQuery> Queries { get; } = [];

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is not GetScheduledJobRunsQuery query)
            {
                throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }

            Queries.Add(query);
            if (Pending is not null)
            {
                return (TResponse)(object)await Pending;
            }

            await Task.Yield();
            return (TResponse)(object)Answer(query);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
