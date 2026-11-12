using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Scheduling;
using Wombat.Application.Features.Scheduling.Commands.DisableScheduledJob;
using Wombat.Application.Features.Scheduling.Commands.EnableScheduledJob;
using Wombat.Application.Features.Scheduling.Commands.RunScheduledJobNow;
using Wombat.Application.Features.Scheduling.Queries.GetScheduledJobStatus;
using Wombat.Application.Scheduling;
using Wombat.Domain.Identity;
using Wombat.Domain.Scheduling;
using Wombat.Web.Components.Pages.Admin.Jobs;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// Scheduled jobs (T400, flow 18, lane B; R3-J01–J10, R2-J11, J12): the columns in words and South African time, Run now
/// aria-disabled with its reason on a disabled job, "Running…", each action's result in its words above the list and
/// focused, the refusal and the failure, never an exception's text, and the loading and load error.
/// </summary>
public sealed class ScheduledJobsPageTests : TestContext
{
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    // 2026-11-10 17:00 SAST.
    private static readonly DateTime Five = new(2026, 11, 10, 15, 0, 0, DateTimeKind.Utc);

    public ScheduledJobsPageTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("devadmin@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void TheColumns_SayEachJob_ItsScheduleInWords_ItsLastAndNextRun_InSouthAfricanTime()
    {
        var cut = Render(new FakeSender(
            Job("activity-draft-nudge", "0 7 * * *", "Reminds trainees about draft activities older than 14 days.",
                lastRun: Five, next: new DateTime(2026, 11, 11, 7, 0, 0, DateTimeKind.Utc)),
            Job("weekly-coordinator-digest", "0 8 * * 1", "Sends weekly digest emails to Coordinators.",
                next: new DateTime(2026, 11, 16, 8, 0, 0, DateTimeKind.Utc))));

        cut.Find("div.people-page.jobs-page").Should().NotBeNull();
        Text(cut.Find(".page-subtitle")).Should().Be(
            "The eight jobs Wombat runs on a schedule. Run one now, disable or enable one, or read every run in Run history.");
        var header = cut.Find(".header-container a.btn");
        Text(header).Should().Be("Run history");
        header.GetAttribute("href").Should().Be("/admin/jobs/runs");

        cut.Find("table").ClassList.Should().Contain(["clinic-table", "clinic-table--stack", "clinic-table--stack-wide"]);
        Text(cut.Find("caption")).Should().Be("Scheduled jobs");
        cut.FindAll("thead th").Select(Text).Should().Equal("Job", "Schedule", "Last run", "Next run", "Actions");

        var nudge = RowOf(cut, "activity-draft-nudge");
        var rowHeader = nudge.QuerySelector("th[scope=row] .person-cell")!;
        Text(rowHeader.QuerySelector("strong")!).Should().Be("activity-draft-nudge");
        Text(rowHeader.QuerySelector("span")!).Should().Be("Reminds trainees about draft activities older than 14 days.");
        Text(nudge.QuerySelector("td[data-label=Schedule]")!).Should().Be("Daily at 09:00 SASTcron 0 7 * * *, UTC");
        Text(nudge.QuerySelector("td[data-label=Schedule] code")!).Should().Be("cron 0 7 * * *, UTC");
        nudge.QuerySelectorAll("td[data-label='Last run'] .nowrap").Select(Text).Should().Equal("2026-11-10 17:00 SAST");
        Text(nudge.QuerySelector("td[data-label='Last run'] .badge")!).Should().Be("Succeeded");
        nudge.QuerySelectorAll("td[data-label='Next run'] .nowrap").Select(Text).Should().Equal("2026-11-11 09:00 SAST");

        var digest = RowOf(cut, "weekly-coordinator-digest");
        Text(digest.QuerySelector("td[data-label='Last run']")!).Should().Be("Not run yet");
        Text(digest.QuerySelector("td[data-label='Next run']")!).Should().Be("2026-11-16 10:00 SAST");
        Text(digest.QuerySelector("td[data-label=Schedule]")!).Should().StartWith("Mondays at 10:00 SAST");

        // The actions: Run now, then Disable, each named for its row (T239).
        nudge.QuerySelectorAll("td .actions-cell button").Select(button => button.GetAttribute("aria-label"))
            .Should().Equal("Run now: activity-draft-nudge", "Disable activity-draft-nudge");
        cut.FindAll("tbody td").Where(cell => cell.QuerySelector(".actions-cell") is null)
            .Should().OnlyContain(cell => cell.HasAttribute("data-label"));

        cut.Markup.Should().NotContain("Enabled").And.NotContain("dispatched").And.NotContain("(T0")
            .And.NotContain("ExpiresOn").And.NotContain("ScheduledJobRun");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void AnyOtherCron_IsItsCronLineAlone()
    {
        var cut = Render(new FakeSender(Job("odd-job", "*/5 * * * *", "Odd.")));

        var schedule = RowOf(cut, "odd-job").QuerySelector("td[data-label=Schedule]")!;
        Text(schedule).Should().Be("cron */5 * * * *, UTC");
        schedule.QuerySelector(".person-cell").Should().BeNull("no words line (K7)");
    }

    [Fact]
    public void AFailedLastRun_ShowsItsErrorUnderItsBadge()
    {
        var cut = Render(new FakeSender(Job("entrustment-decision-expiry", "30 3 * * *", "d", lastRun: Five, status: "Failed",
            error: "Marked as failed during startup reconciliation — process likely crashed.")));

        var cell = RowOf(cut, "entrustment-decision-expiry").QuerySelector("td[data-label='Last run']")!;
        Text(cell.QuerySelector(".badge")!).Should().Be("Failed");
        cell.QuerySelector(".badge")!.ClassList.Should().Contain("badge-declined");
        Text(cell.QuerySelector(".text-sm")!).Should().Be("Marked as failed during startup reconciliation — process likely crashed.");
    }

    [Fact]
    public void AJobAlreadyDue_RunsAtTheNextCheck()
    {
        var cut = Render(new FakeSender(Job("msf-campaign-auto-close", "0 * * * *", "d", lastRun: Five, due: true,
            next: Five.AddHours(1))));

        Text(RowOf(cut, "msf-campaign-auto-close").QuerySelector("td[data-label='Next run']")!)
            .Should().Be("At the next check, within 30 seconds");
    }

    [Fact]
    public void ADisabledJob_HasNoNextRun_AndItsRunNow_IsAriaDisabled_WithItsReason_AndAPressSendsNothing()
    {
        var sender = new FakeSender(Job("msf-campaign-auto-close", "0 * * * *", "d", lastRun: Five, enabled: false));
        var cut = Render(sender);
        var row = RowOf(cut, "msf-campaign-auto-close");

        Text(row.QuerySelector("td[data-label='Next run']")!).Should().Be("None while disabled");
        var runNow = Button(cut, "Run now: msf-campaign-auto-close");
        runNow.HasAttribute("disabled").Should().BeFalse("it stays in the tab order (R3-J02)");
        runNow.GetAttribute("aria-disabled").Should().Be("true");
        var why = cut.Find($"#{runNow.GetAttribute("aria-describedby")}");
        Text(why).Should().Be("Enable it to run it.");
        why.TagName.Should().Be("SPAN");
        Text(Button(cut, "Enable msf-campaign-auto-close")).Should().Be("Enable");

        runNow.Click();

        sender.Sent.OfType<RunScheduledJobNowCommand>().Should().BeEmpty("a press on a disabled job's Run now sends nothing");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void Disable_SaysWhatItDid_AboveTheList_AndTheResultTakesTheFocus()
    {
        var sender = new FakeSender(Job("msf-campaign-auto-close", "0 * * * *", "d", lastRun: Five));
        var cut = Render(sender);
        sender.Jobs = [Job("msf-campaign-auto-close", "0 * * * *", "d", lastRun: Five, enabled: false)];

        Button(cut, "Disable msf-campaign-auto-close").Click();

        var alert = cut.WaitForElement(".action-result .alert-success");
        Text(alert).Should().Be("Disabled msf-campaign-auto-close. The scheduler will not run it until you enable it.");
        alert.GetAttribute("role").Should().Be("status");
        sender.Sent.OfType<DisableScheduledJobCommand>().Should().ContainSingle().Which.Key.Should().Be("msf-campaign-auto-close");
        ResultIsAboveTheList(cut);
        cut.WaitForAssertion(() => ResultTookTheFocus(cut));
        Text(RowOf(cut, "msf-campaign-auto-close").QuerySelector("td[data-label='Next run']")!).Should().Be("None while disabled",
            "the page reads again after the action");
    }

    [Fact]
    public void Enable_SaysItsNextRun_InSouthAfricanTime()
    {
        var sender = new FakeSender(Job("msf-campaign-auto-close", "0 * * * *", "d", lastRun: Five, enabled: false));
        var cut = Render(sender);
        sender.Jobs = [Job("msf-campaign-auto-close", "0 * * * *", "d", lastRun: Five, next: new DateTime(2026, 11, 10, 20, 0, 0, DateTimeKind.Utc))];

        Button(cut, "Enable msf-campaign-auto-close").Click();

        Text(cut.WaitForElement(".action-result .alert-success"))
            .Should().Be("Enabled msf-campaign-auto-close. Its next run is 2026-11-10 22:00 SAST.");
        cut.WaitForAssertion(() => ResultTookTheFocus(cut));
    }

    [Fact]
    public void Enable_OfAJobAlreadyDue_SaysItRunsAtTheNextCheck()
    {
        var sender = new FakeSender(Job("msf-campaign-auto-close", "0 * * * *", "d", lastRun: Five, enabled: false));
        var cut = Render(sender);
        sender.Jobs = [Job("msf-campaign-auto-close", "0 * * * *", "d", lastRun: Five, next: Five.AddHours(1), due: true)];

        Button(cut, "Enable msf-campaign-auto-close").Click();

        Text(cut.WaitForElement(".action-result .alert-success"))
            .Should().Be("Enabled msf-campaign-auto-close. It runs at the next check, within 30 seconds.");
    }

    [Fact]
    public void RunNow_SaysHowTheRunEnded_AsTheCaller_AndTheResultTakesTheFocus()
    {
        var ran = new DateTime(2026, 11, 10, 19, 39, 58, DateTimeKind.Utc);
        var sender = new FakeSender(Job("weekly-coordinator-digest", "0 8 * * 1", "d"))
        {
            Ran = new ScheduledJobRunOutcome(ScheduledJobRunStatus.Succeeded, ran, ran.AddMilliseconds(25), null)
        };
        var cut = Render(sender);

        Button(cut, "Run now: weekly-coordinator-digest").Click();

        Text(cut.WaitForElement(".action-result .alert-success"))
            .Should().Be("Ran weekly-coordinator-digest: Succeeded at 2026-11-10 21:39 SAST.");
        var command = sender.Sent.OfType<RunScheduledJobNowCommand>().Should().ContainSingle().Which;
        command.Key.Should().Be("weekly-coordinator-digest");
        command.Principal.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be("admin-1", "the caller, never \"unknown\" (K6)");
        cut.WaitForAssertion(() => ResultTookTheFocus(cut));
        ResultIsAboveTheList(cut);
    }

    [Fact]
    public void RunNow_ThatFailed_SaysSo_WithTheErrorAsStored_AndItsRuns()
    {
        var ran = new DateTime(2026, 11, 10, 19, 41, 23, DateTimeKind.Utc);
        var sender = new FakeSender(Job("entrustment-decision-expiry", "30 3 * * *", "d"))
        {
            Ran = new ScheduledJobRunOutcome(ScheduledJobRunStatus.Failed, ran, ran.AddMilliseconds(29), "The mail server refused the message.")
        };
        var cut = Render(sender);

        Button(cut, "Run now: entrustment-decision-expiry").Click();

        var alert = cut.WaitForElement(".action-result .alert-danger");
        alert.GetAttribute("role").Should().Be("alert");
        Text(alert).Should().Be(
            "Ran entrustment-decision-expiry: Failed at 2026-11-10 21:41 SAST: The mail server refused the message. " +
            "Its runs: entrustment-decision-expiry, failed");
        var link = alert.QuerySelector("a")!;
        link.GetAttribute("href").Should().Be("/admin/jobs/runs?job=entrustment-decision-expiry&status=Failed");
        Text(link.QuerySelector(".visually-hidden")!).Should().Be(": entrustment-decision-expiry, failed");
    }

    [Fact]
    public void WhileARunGoes_ItsButtonSaysRunning_AndTheOtherRowActionsWait()
    {
        var sender = new FakeSender(
            Job("assessor-pending-nudge", "0 9 * * *", "d", lastRun: Five),
            Job("audit-log-retention", "0 3 * * *", "d", lastRun: Five))
        {
            Hold = new TaskCompletionSource()
        };
        var cut = Render(sender);

        Button(cut, "Run now: assessor-pending-nudge").Click();

        cut.WaitForAssertion(() => Text(Button(cut, "Running…: assessor-pending-nudge")).Should().Be("Running…"));
        var running = Button(cut, "Running…: assessor-pending-nudge");
        running.GetAttribute("aria-disabled").Should().Be("true");
        running.HasAttribute("disabled").Should().BeFalse("it has the focus (T234)");
        Button(cut, "Disable assessor-pending-nudge").HasAttribute("disabled").Should().BeTrue();
        Button(cut, "Run now: audit-log-retention").HasAttribute("disabled").Should().BeTrue();

        sender.Hold.SetResult();
        cut.WaitForAssertion(() => Text(Button(cut, "Run now: assessor-pending-nudge")).Should().Be("Run now"));
    }

    [Fact]
    public void ARefusal_IsAWarningInItsOwnWords_TakesTheFocus_AndThePageReadsAgain()
    {
        var sender = new FakeSender(Job("msf-campaign-auto-close", "0 * * * *", "d", lastRun: Five))
        {
            RunFails = ScheduledJobRefusal.Disabled("msf-campaign-auto-close")
        };
        var cut = Render(sender);
        sender.Jobs = [Job("msf-campaign-auto-close", "0 * * * *", "d", lastRun: Five, enabled: false)];

        Button(cut, "Run now: msf-campaign-auto-close").Click();

        var alert = cut.WaitForElement(".action-result .alert-warning");
        alert.GetAttribute("role").Should().Be("alert");
        Text(alert).Should().Be("Job 'msf-campaign-auto-close' is disabled, so it was not run. Enable it, then run it.");
        cut.WaitForAssertion(() => ResultTookTheFocus(cut));
        Text(RowOf(cut, "msf-campaign-auto-close").QuerySelector("td[data-label='Next run']")!)
            .Should().Be("None while disabled", "the row now reads disabled (R3-J09)");
    }

    [Fact]
    public void AFailedRunNow_IsFixedWords_NeverTheExceptions_AndTakesTheFocus()
    {
        var sender = new FakeSender(Job("audit-log-retention", "0 3 * * *", "d", lastRun: Five))
        {
            RunFails = new InvalidOperationException("Npgsql: connection refused at 10.0.0.1")
        };
        var cut = Render(sender);

        Button(cut, "Run now: audit-log-retention").Click();

        var alert = cut.WaitForElement(".action-result .alert-danger");
        Text(alert.QuerySelector("strong")!).Should().Be("audit-log-retention could not be started.");
        Text(alert).Should().Be(
            "audit-log-retention could not be started. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("Npgsql").And.NotContain("connection refused");
        cut.WaitForAssertion(() => ResultTookTheFocus(cut));
    }

    [Fact]
    public void AFailedDisable_IsFixedWords_AndTheRowIsAsItWas()
    {
        var sender = new FakeSender(Job("msf-campaign-auto-close", "0 * * * *", "d", lastRun: Five))
        {
            ToggleFails = new InvalidOperationException("Scheduled job 'msf-campaign-auto-close' not found.")
        };
        var cut = Render(sender);

        Button(cut, "Disable msf-campaign-auto-close").Click();

        var alert = cut.WaitForElement(".action-result .alert-danger");
        Text(alert).Should().Be(
            "msf-campaign-auto-close could not be disabled. Nothing has changed. Try again, or come back in a few minutes.");
        Text(alert.QuerySelector("strong")!).Should().Be("msf-campaign-auto-close could not be disabled.");
        cut.Markup.Should().NotContain("not found");
        Text(Button(cut, "Disable msf-campaign-auto-close")).Should().Be("Disable");
        cut.WaitForAssertion(() => ResultTookTheFocus(cut));
    }

    [Fact]
    public void WhileTheJobsLoad_TheLoadingLine_AndRunHistoryStaysInTheHeader()
    {
        var pending = new TaskCompletionSource<IReadOnlyList<ScheduledJobDto>>();
        Services.AddSingleton<IScopedSender>(new FakeSender { Read = () => pending.Task });
        var cut = RenderComponent<ScheduledJobsList>();

        var line = cut.Find("p.loading-line");
        line.GetAttribute("role").Should().Be("status");
        Text(line).Should().Be("Loading the scheduled jobs.");
        Text(cut.Find(".header-container a.btn")).Should().Be("Run history", "it needs no data (R2-J11)");
        cut.FindAll("table").Should().BeEmpty();
    }

    [Fact]
    public void AFailedRead_IsTheHouseSentence_WithTryAgain_WhichFocusesTheHeadingOnceItReads()
    {
        var calls = 0;
        var sender = new FakeSender(Job("audit-log-retention", "0 3 * * *", "d"));
        sender.Read = () => ++calls == 1
            ? Task.FromException<IReadOnlyList<ScheduledJobDto>>(new InvalidOperationException("Npgsql: connection refused"))
            : Task.FromResult<IReadOnlyList<ScheduledJobDto>>(sender.Jobs);
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<ScheduledJobsList>();

        var alert = cut.WaitForElement(".alert-danger");
        Text(alert.QuerySelector(".alert-row-text")!).Should().Be(
            "Could not load the scheduled jobs. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("Npgsql");

        cut.FindAll("button").Single(button => Text(button) == "Try again").Click();

        cut.WaitForState(() => cut.FindAll("tbody tr").Count == 1);
        JSInterop.Invocations.Should().Contain(call => call.Identifier == PageFocus.FocusHeadingIdentifier);
    }

    // ---- helpers ----

    private IRenderedComponent<ScheduledJobsList> Render(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<ScheduledJobsList>();
        cut.WaitForState(() => cut.FindAll("p.loading-line").Count == 0);
        return cut;
    }

    private void ResultTookTheFocus(IRenderedComponent<ScheduledJobsList> cut)
    {
        var region = cut.FindComponents<ActionResult>().First();
        JSInterop.Invocations.Where(call => call.Identifier == FocusIdentifier)
            .Should().Contain(call => call.Arguments[0] is ElementReference
                                      && ((ElementReference)call.Arguments[0]!).Id == region.Instance.Element.Id);
    }

    private static void ResultIsAboveTheList(IRenderedComponent<ScheduledJobsList> cut)
    {
        var markup = cut.Markup;
        markup.IndexOf("action-result", StringComparison.Ordinal)
            .Should().BeLessThan(markup.IndexOf("table-container", StringComparison.Ordinal), "the result is above the list (T5)");
    }

    private static IElement Button(IRenderedFragment cut, string name)
        => cut.FindAll("button").Single(button => button.GetAttribute("aria-label") == name);

    private static IElement RowOf(IRenderedFragment cut, string key)
        => cut.FindAll("tbody tr").Single(row => Text(row.QuerySelector("th[scope=row] strong")!) == key);

    private static string Text(IElement element) => string.Join(' ', element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static ScheduledJobDto Job(
        string key, string cron, string description, DateTime? lastRun = null, string status = "Succeeded", string? error = null,
        bool enabled = true, DateTime? next = null, bool due = false)
        => new(1, key, cron, enabled, description, lastRun, lastRun is null ? null : status, lastRun?.AddMilliseconds(30),
            error, null, enabled ? next : null, enabled && due);

    private sealed class FakeSender : IScopedSender
    {
        public FakeSender(params ScheduledJobDto[] jobs)
        {
            Jobs = jobs;
            Read = () => Task.FromResult<IReadOnlyList<ScheduledJobDto>>(Jobs);
        }

        public IReadOnlyList<ScheduledJobDto> Jobs { get; set; }

        public Func<Task<IReadOnlyList<ScheduledJobDto>>> Read { get; set; }

        public List<object> Sent { get; } = [];

        public ScheduledJobRunOutcome Ran { get; init; } =
            new(ScheduledJobRunStatus.Succeeded, Five, Five.AddMilliseconds(20), null);

        public Exception? RunFails { get; init; }

        public Exception? ToggleFails { get; init; }

        public TaskCompletionSource? Hold { get; init; }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Sent.Add(request);
            switch (request)
            {
                case GetScheduledJobStatusQuery:
                    return (TResponse)(object)await Read();
                case RunScheduledJobNowCommand:
                    if (Hold is not null)
                    {
                        await Hold.Task;
                    }

                    return RunFails is not null ? throw RunFails : (TResponse)(object)Ran;
                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            Sent.Add(request);
            if (request is not (EnableScheduledJobCommand or DisableScheduledJobCommand))
            {
                throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }

            return ToggleFails is not null ? Task.FromException(ToggleFails) : Task.CompletedTask;
        }
    }
}
