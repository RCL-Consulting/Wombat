using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Dashboards.Administrator;
using Wombat.Web.Components.Pages.Dashboards;
using Wombat.Web.Services;
using static Wombat.Web.Tests.Dashboards.AdministratorHomeFixtures;

namespace Wombat.Web.Tests.Dashboards;

/// <summary>
/// T400 (flow 18, lane A; boards R3-H01–H12): the Administrator's Home. System health's three lines in fixed order, each
/// its name, a hidden colon, a badge and its sentences in words; no status dot and no task id; the links as footer buttons;
/// Accounts' figure and locked line; and each part Not read on its own, with no footer on a card or line that reads so.
/// </summary>
public sealed class AdministratorHomeTests : TestContext
{
    public AdministratorHomeTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("devadmin@test");
        auth.SetRoles("Administrator");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "devadmin"));
    }

    // ─── R3-H01: A.6.2 as replayed ──────────────────────────────────────────

    [Fact]
    public void H01_TheThreeLines_InFixedOrder_InWords_WithTheHiddenColon_AndNoDot()
    {
        var cut = Render(Healthy());

        var lines = cut.FindAll("ul.health-list > li.health-line").ToList();
        lines.Select(line => Text(line.QuerySelector(".health-line-name")!)).Should().Equal(
            "Database:OK", "Mail:Not configured", "Scheduled jobs:OK");
        lines.Select(line => line.QuerySelector(".health-line-name > .visually-hidden")!.TextContent).Should().OnlyContain(colon => colon == ":");
        Paragraphs(lines[0]).Should().Equal("The database answers.");
        Paragraphs(lines[1]).Should().Equal(
            "Mail is written to the log, not sent.", "To send mail, set Email__SmtpHost on the server and restart Wombat.");
        Paragraphs(lines[2]).Should().Equal("All 8 jobs last succeeded.");

        Badge(lines[0]).ClassList.Should().Contain("badge-completed");
        Badge(lines[1]).ClassList.Should().Contain("badge-accepted", "Not configured stays amber (N3)");
        cut.FindAll(".status-dot").Should().BeEmpty("S5: the dots are retired");
        cut.Markup.Should().NotContain("(T0").And.NotContain("registered users").And.NotContain("Users across institutions");
        cut.FindAll(".health-line a").Should().BeEmpty("no link in a health sentence (S1)");
        cut.FindAll("[role=status] .health-list, .health-list[aria-live]").Should().BeEmpty("standing notices are not live regions");
    }

    [Fact]
    public void H01_TheCards_AreSectionsNamedByTheirH2s_WithTheirFootersButtons()
    {
        var cut = Render(Healthy());

        var cards = cut.FindAll("section.detail-card").ToList();
        cards.Select(card => Text(card.QuerySelector("h2")!)).Should().Equal("System health", "Accounts");
        cards[0].ClassList.Should().Contain("dashboard-span-2");
        cards[1].ClassList.Should().Contain("detail-card--emphasis");
        foreach (var card in cards)
        {
            card.GetAttribute("aria-labelledby").Should().Be(card.QuerySelector("h2")!.Id);
        }

        Footer(cards[0]).Should().Equal(("Scheduled jobs", "/admin/jobs"));
        Footer(cards[1]).Should().Equal(("Users", "/admin/users"));
        cards.SelectMany(card => card.QuerySelectorAll(".dashboard-card-footer a"))
            .Should().OnlyContain(link => link.ClassList.Contains("btn") && link.ClassList.Contains("btn-sm") && link.ClassList.Contains("btn-outline"));
    }

    [Fact]
    public void H01_Accounts_IsTheFigure_TheSentence_AndNoneLocked()
    {
        var accounts = Render(Healthy()).FindAll("section.detail-card").ToList()[1];

        Text(accounts.QuerySelector(".dashboard-metric-value")!).Should().Be("24");
        Text(accounts.QuerySelector(".dashboard-metric-label")!).Should().Be("accounts");
        accounts.QuerySelectorAll(".dashboard-card-body > p").Select(Text).Should().Equal(
            "Every account Wombat holds but an erased one.", "None is locked.");
    }

    // ─── R3-H02: 1.1, a job not yet run ─────────────────────────────────────

    [Fact]
    public void H02_AJobNotYetRun_SaysItsFirstRun_InSast_AndTheBadgeIsOk()
    {
        var jobs = EightSucceeded();
        jobs[^1] = Job("weekly-coordinator-digest", AdministratorJobState.NeverRun, new DateTime(2026, 11, 16, 8, 0, 0, DateTimeKind.Utc));
        var cut = Render(Healthy(accounts: 9) with { Jobs = new AdministratorJobsDto(jobs) });

        var line = cut.FindAll("li.health-line").ToList()[2];
        Text(line.QuerySelector(".health-line-name")!).Should().Be("Scheduled jobs:OK");
        Paragraphs(line).Should().Equal(
            "7 jobs last succeeded. weekly-coordinator-digest has not run yet: its first run is 2026-11-16 10:00 SAST.");
        Text(cut.Find(".dashboard-metric-value")).Should().Be("9");
    }

    // ─── R3-H03, H12: mail sent, and nothing sent in the window ─────────────

    [Fact]
    public void H03_MailSent_SaysTheLastSent_NoneWaiting_NoneDropped()
    {
        var cut = Render(Healthy() with
        {
            Mail = new AdministratorMailDto(true, 0, new AdministratorMailOutcomesDto(Now.AddDays(-90), 0, null, null, Now.AddMinutes(-30)))
        });

        var mail = cut.FindAll("li.health-line").ToList()[1];
        Text(mail.QuerySelector(".health-line-name")!).Should().Be("Mail:OK");
        Paragraphs(mail).Should().Equal("Mail is sent. Last sent 2026-11-10 21:09 SAST; none waiting; none dropped in the last 24 hours.");
        Times(mail).Should().Equal(new[] { "2026-11-10 21:09 SAST" }, "a time in a sentence never breaks (T400's replay)");
    }

    // ─── T400's replay: a time in a health sentence never breaks ────────────

    [Fact]
    public void EveryTimeInASentence_IsOneUnbreakableUnit_InsideItsOneParagraph()
    {
        var jobs = EightSucceeded();
        jobs[^1] = Job("weekly-coordinator-digest", AdministratorJobState.NeverRun, new DateTime(2026, 11, 16, 8, 0, 0, DateTimeKind.Utc));
        var cut = Render(Healthy() with
        {
            Mail = new AdministratorMailDto(true, 0, new AdministratorMailOutcomesDto(Now.AddDays(-90), 0, null, null, null)),
            Jobs = new AdministratorJobsDto(jobs)
        });

        var lines = cut.FindAll("li.health-line").ToList();
        Times(lines[1]).Should().Equal("2026-08-12 21:39 SAST");
        Times(lines[2]).Should().Equal("2026-11-16 10:00 SAST");
        lines[2].QuerySelectorAll(".health-line-text > p").Should().ContainSingle("the time is a span inside its sentence, not a paragraph of its own");
        Paragraphs(lines[2]).Should().Equal(
            "7 jobs last succeeded. weekly-coordinator-digest has not run yet: its first run is 2026-11-16 10:00 SAST.");
    }

    [Fact]
    public void ATimeInAListedJobsSentence_AndTheDatabasesClock_AreEachOneUnit()
    {
        var jobs = EightSucceeded();
        jobs[2] = Job("audit-log-retention", AdministratorJobState.Overdue, new DateTime(2026, 11, 10, 1, 0, 0, DateTimeKind.Utc));
        jobs[3] = Job("entrustment-decision-expiry", AdministratorJobState.Failed) with { LastErrorFirstLine = "Timeout expired." };
        var cut = Render(Healthy() with { DatabaseFailedAt = Now, Jobs = new AdministratorJobsDto(jobs) });

        var lines = cut.FindAll("li.health-line").ToList();
        Times(lines[0]).Should().Equal("21:39 SAST");
        lines[2].QuerySelectorAll(".health-line-text li .nowrap").Select(Text)
            .Should().Equal("2026-11-10 07:00 SAST", "2026-11-10 03:00 SAST");
    }

    [Fact]
    public void Runs_CutsASentenceAtItsTimes_AndPutsItBackWhole()
    {
        const string sentence = "Nothing sent since 2026-08-12 21:39 SAST; none waiting. The database did not answer at 21:39 SAST.";

        var runs = AdministratorHomeWords.Runs(sentence);

        runs.Where(run => run.IsTime).Select(run => run.Text).Should().Equal("2026-08-12 21:39 SAST", "21:39 SAST");
        string.Concat(runs.Select(run => run.Text)).Should().Be(sentence);
        AdministratorHomeWords.Runs("The database answers.").Should().Equal(new[] { ("The database answers.", false) });
    }

    [Fact]
    public void H12_NothingSentInTheWindow_SaysSince_TheWindowsStart_AndIsOk()
    {
        var cut = Render(Healthy() with
        {
            Mail = new AdministratorMailDto(true, 0, new AdministratorMailOutcomesDto(Now.AddDays(-90), 0, null, null, null))
        });

        var mail = cut.FindAll("li.health-line").ToList()[1];
        Text(mail.QuerySelector(".health-line-name")!).Should().Be("Mail:OK", "no mail sent is not a state (Q13)");
        Paragraphs(mail).Should().Equal("Nothing sent since 2026-08-12 21:39 SAST; none waiting; none dropped in the last 24 hours.");
    }

    // ─── R3-H04: a drop in the 24 hours ─────────────────────────────────────

    [Fact]
    public void H04_ADrop_IsFailing_WithTheRefusalAsStored()
    {
        var cut = Render(Healthy() with
        {
            Mail = new AdministratorMailDto(true, 0, new AdministratorMailOutcomesDto(
                Now.AddDays(-90), 3, Now.AddHours(-1), "550 5.1.1 <[recipient]>: Recipient address rejected", Now.AddHours(-5)))
        });

        var mail = cut.FindAll("li.health-line").ToList()[1];
        Text(mail.QuerySelector(".health-line-name")!).Should().Be("Mail:Failing");
        Badge(mail).ClassList.Should().Contain("badge-declined");
        Paragraphs(mail).Should().Equal(
            "3 mails were dropped in the last 24 hours, the last at 2026-11-10 20:39 SAST: 550 5.1.1 <[recipient]>: Recipient address rejected. The application log names each one.");
    }

    [Fact]
    public void K3_ADropWithNoRefusal_HasNoRefusalClause_AndOneDropIsSingular()
    {
        var cut = Render(Healthy() with
        {
            Mail = new AdministratorMailDto(true, 0, new AdministratorMailOutcomesDto(Now.AddDays(-90), 1, Now.AddHours(-1), null, null))
        });

        Paragraphs(cut.FindAll("li.health-line").ToList()[1]).Should().Equal(
            "1 mail was dropped in the last 24 hours, the last at 2026-11-10 20:39 SAST. The application log names each one.");
    }

    // ─── R3-H05–H07, H09: the jobs ──────────────────────────────────────────

    [Fact]
    public void H05_AFailedJob_IsFailing_WithItsFirstErrorLine_AndItsRunsInTheFooter()
    {
        var cut = Render(WithAFailedJob() with
        {
            Jobs = new AdministratorJobsDto(EightSucceeded().Select(job => job.Key == "entrustment-decision-expiry"
                ? job with { State = AdministratorJobState.Failed, LastRunStatus = "Failed", LastErrorFirstLine = "Timeout expired." }
                : job).ToList())
        });

        var line = cut.FindAll("li.health-line").ToList()[2];
        Text(line.QuerySelector(".health-line-name")!).Should().Be("Scheduled jobs:Failing");
        Paragraphs(line).Should().Equal("entrustment-decision-expiry failed at 2026-11-10 07:00 SAST: Timeout expired.");

        var footer = cut.FindAll("section.detail-card").ToList()[0].QuerySelectorAll(".dashboard-card-footer a").ToList();
        footer.Select(link => (Text(link), link.GetAttribute("href"))).Should().Equal(
            ("Its runs: entrustment-decision-expiry, failed", "/admin/jobs/runs?job=entrustment-decision-expiry&status=Failed"),
            ("Scheduled jobs", "/admin/jobs"));
        Text(footer[0].QuerySelector(".visually-hidden")!).Should().Be(": entrustment-decision-expiry, failed");
    }

    [Fact]
    public void H06_ADisabledJob_IsDisabled_AndTheOthersLastSucceeded()
    {
        var jobs = EightSucceeded();
        jobs[4] = Job("msf-campaign-auto-close", AdministratorJobState.Disabled);
        var line = Render(Healthy() with { Jobs = new AdministratorJobsDto(jobs) }).FindAll("li.health-line").ToList()[2];

        Text(line.QuerySelector(".health-line-name")!).Should().Be("Scheduled jobs:Disabled");
        Badge(line).ClassList.Should().Contain("badge-accepted");
        Paragraphs(line).Should().Equal(
            "msf-campaign-auto-close is disabled, so the scheduler does not run it. The other 7 last succeeded.");
    }

    /// <summary>
    /// states.md at A.2.2 (the build review's S3): msf-campaign-auto-close disabled while the digest has not run yet. The
    /// other six last succeeded, and the digest says its first run.
    /// </summary>
    [Fact]
    public void H06_ADisabledJob_AndOneNotYetRun_SaysTheOtherSix_AndTheFirstRun()
    {
        var jobs = EightSucceeded();
        jobs[4] = Job("msf-campaign-auto-close", AdministratorJobState.Disabled);
        jobs[^1] = Job("weekly-coordinator-digest", AdministratorJobState.NeverRun, new DateTime(2026, 11, 16, 8, 0, 0, DateTimeKind.Utc));
        var line = Render(Healthy() with { Jobs = new AdministratorJobsDto(jobs) }).FindAll("li.health-line").ToList()[2];

        Text(line.QuerySelector(".health-line-name")!).Should().Be("Scheduled jobs:Disabled");
        Paragraphs(line).Should().Equal(
            "msf-campaign-auto-close is disabled, so the scheduler does not run it. The other 6 last succeeded. weekly-coordinator-digest has not run yet: its first run is 2026-11-16 10:00 SAST.");
    }

    /// <summary>The build review's N7: no job stored is not "All 0 jobs last succeeded.".</summary>
    [Fact]
    public void NoJobStored_IsNotConfigured_NeverAllZeroSucceeded()
    {
        var line = Render(Healthy() with { Jobs = new AdministratorJobsDto([]) }).FindAll("li.health-line").ToList()[2];

        Text(line.QuerySelector(".health-line-name")!).Should().Be("Scheduled jobs:Not configured");
        Paragraphs(line).Should().Equal("No scheduled job is stored yet: the scheduler stores them as it starts.");
    }

    [Fact]
    public void H07_AnOverdueJob_IsOverdue_WhenItWasDue()
    {
        var jobs = EightSucceeded();
        jobs[2] = Job("audit-log-retention", AdministratorJobState.Overdue, new DateTime(2026, 11, 10, 1, 0, 0, DateTimeKind.Utc));
        var line = Render(Healthy() with { Jobs = new AdministratorJobsDto(jobs) }).FindAll("li.health-line").ToList()[2];

        Text(line.QuerySelector(".health-line-name")!).Should().Be("Scheduled jobs:Overdue");
        Badge(line).ClassList.Should().Contain("badge-overdue");
        Paragraphs(line).Should().Equal(
            "audit-log-retention has not run since it was due at 2026-11-10 03:00 SAST: the scheduler did not start it. The application log says why; Run now runs it.");
    }

    [Fact]
    public void K4_AnUnreadableCron_IsFailing_InItsOwnSentence_AndARunningJobIsNeither()
    {
        var jobs = EightSucceeded();
        jobs[0] = Job("activity-draft-nudge", AdministratorJobState.Unreadable) with { Cron = "every day at noon" };
        jobs[1] = Job("assessor-pending-nudge", AdministratorJobState.Running);
        var cut = Render(Healthy() with { Jobs = new AdministratorJobsDto(jobs) });

        var line = cut.FindAll("li.health-line").ToList()[2];
        Text(line.QuerySelector(".health-line-name")!).Should().Be("Scheduled jobs:Failing");
        Paragraphs(line).Should().Equal(
            "activity-draft-nudge's schedule cannot be read (cron every day at noon), so the scheduler does not run it. The application log says why.");
        cut.FindAll(".dashboard-card-footer a").Select(Text).Should().NotContain(text => text.StartsWith("Its runs"),
            "Its runs is for a job that failed");
    }

    [Fact]
    public void H09_EveryLineFailing_TheWorstWord_AndEachJobItsOwnSentence()
    {
        var jobs = EightSucceeded();
        jobs[2] = Job("audit-log-retention", AdministratorJobState.Overdue, new DateTime(2026, 11, 10, 1, 0, 0, DateTimeKind.Utc));
        jobs[3] = Job("entrustment-decision-expiry", AdministratorJobState.Failed) with { LastErrorFirstLine = "Timeout expired." };
        jobs[4] = Job("msf-campaign-auto-close", AdministratorJobState.Disabled);
        var cut = Render(Healthy() with
        {
            Mail = new AdministratorMailDto(true, 0, new AdministratorMailOutcomesDto(Now.AddDays(-90), 2, Now.AddHours(-1), "421 busy", null)),
            Jobs = new AdministratorJobsDto(jobs)
        });

        var line = cut.FindAll("li.health-line").ToList()[2];
        Text(line.QuerySelector(".health-line-name")!).Should().Be("Scheduled jobs:Failing");
        Paragraphs(line).Should().Equal("Three jobs need you:");
        line.QuerySelectorAll(".health-line-text ul > li").Select(Text).Should().Equal(
            "entrustment-decision-expiry failed at 2026-11-10 07:00 SAST: Timeout expired.",
            "audit-log-retention has not run since it was due at 2026-11-10 03:00 SAST: the scheduler did not start it. The application log says why; Run now runs it.",
            "msf-campaign-auto-close is disabled, so the scheduler does not run it.");
        cut.FindAll("section.detail-card").ToList()[0].QuerySelectorAll(".dashboard-card-footer a").Select(Text)
            .Should().Equal("Its runs: entrustment-decision-expiry, failed", "Scheduled jobs");
    }

    // ─── R3-H08: the database down ──────────────────────────────────────────

    [Fact]
    public void H08_TheDatabaseDown_IsUnreachable_ThePartsNotRead_AndNoFooterOnWhatIsNotRead()
    {
        var cut = Render(new AdministratorDashboardSummaryDto(Now, Now, null, new AdministratorMailDto(false, 0, null), null));

        var lines = cut.FindAll("li.health-line").ToList();
        lines.Select(line => Text(line.QuerySelector(".health-line-name")!)).Should().Equal(
            "Database:Unreachable", "Mail:Not configured", "Scheduled jobs:Not read");
        Paragraphs(lines[0]).Should().Equal(
            "The database did not answer at 21:39 SAST, so Wombat can read and save nothing, and /health says Unhealthy.");
        Paragraphs(lines[2]).Should().Equal("Not read: the database did not answer.");
        Badge(lines[0]).ClassList.Should().Contain("badge-declined");
        Badge(lines[2]).ClassList.Should().Contain("badge-draft");

        var accounts = cut.FindAll("section.detail-card").ToList()[1];
        accounts.QuerySelectorAll(".dashboard-card-body > p").Select(Text).Should().Equal("Not read: the database did not answer.");
        accounts.QuerySelector(".dashboard-metric").Should().BeNull();
        cut.FindAll(".dashboard-card-footer").Should().BeEmpty("no footer on a card or line that reads Not read");
    }

    [Fact]
    public void Q18_AMailServerSet_AndTheTableNotRead_ReadsNotRead()
    {
        var cut = Render(Healthy() with { Mail = new AdministratorMailDto(true, 0, null) });

        var mail = cut.FindAll("li.health-line").ToList()[1];
        Text(mail.QuerySelector(".health-line-name")!).Should().Be("Mail:Not read");
        Paragraphs(mail).Should().Equal("Not read: the database did not answer.");
    }

    // ─── R3-H10: locked accounts ────────────────────────────────────────────

    [Theory]
    [InlineData(3, "3 are locked and cannot sign in now.")]
    [InlineData(1, "1 is locked and cannot sign in now.")]
    public void H10_TheLocked_AreSaid_WithTheirSingular(int locked, string sentence)
    {
        var accounts = Render(Healthy(locked: locked)).FindAll("section.detail-card").ToList()[1];

        accounts.QuerySelectorAll(".dashboard-card-body > p").Select(Text).Last().Should().Be(sentence);
    }

    [Fact]
    public void OneAccount_IsSingular()
        => Text(Render(Healthy(accounts: 1)).Find(".dashboard-metric-label")).Should().Be("account");

    // ─── R3-H11: loading ────────────────────────────────────────────────────

    [Fact]
    public void H11_Loading_IsTheTwoTitlesAndTheirSkeletons_AndNoFooter()
    {
        Services.AddSingleton<IScopedSender>(new Sender(null));
        var cut = RenderComponent<AdministratorDashboard>();

        cut.FindAll("section.detail-card h2").Select(Text).Should().Equal("System health", "Accounts");
        cut.FindAll(".dashboard-card-skeleton[aria-hidden=true]").Should().HaveCount(2);
        cut.FindAll(".dashboard-card-footer, .health-list").Should().BeEmpty("nothing is offered until the read is in");
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private IRenderedComponent<AdministratorDashboard> Render(AdministratorDashboardSummaryDto summary)
    {
        Services.AddSingleton<IScopedSender>(new Sender(summary));
        var cut = RenderComponent<AdministratorDashboard>();
        cut.WaitForState(() => cut.FindAll(".dashboard-card-skeleton").Count == 0);
        return cut;
    }

    private static IReadOnlyList<string> Paragraphs(IElement line)
        => line.QuerySelectorAll(".health-line-text > p").Select(Text).ToList();

    private static IReadOnlyList<string> Times(IElement line)
        => line.QuerySelectorAll(".health-line-text .nowrap").Select(Text).ToList();

    private static IElement Badge(IElement line) => line.QuerySelector(".health-line-name .badge")!;

    private static List<(string, string?)> Footer(IElement card)
        => card.QuerySelectorAll(".dashboard-card-footer a").Select(link => (Text(link), link.GetAttribute("href"))).ToList();

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    /// <summary>Answers Home's read with the summary given, or never, when it is null.</summary>
    private sealed class Sender(AdministratorDashboardSummaryDto? summary) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => summary is null ? new TaskCompletionSource<TResponse>().Task : Task.FromResult((TResponse)(object)summary);

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
