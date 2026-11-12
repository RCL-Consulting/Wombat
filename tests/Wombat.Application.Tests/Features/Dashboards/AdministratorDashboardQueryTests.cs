using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Dashboards.Administrator;
using Wombat.Domain.Email;
using Wombat.Domain.Institutions;
using Wombat.Domain.Scheduling;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Dashboards;

/// <summary>
/// The Administrator's Home (T400, flow 18): each part read on its own (K11), the accounts by the directory's one count
/// (K2), the mail from the outcomes table (Q18), the jobs by the host's rule (K4). Rewritten whole for T400: until then the
/// summary was a database flag and a count of distinct ids across every role's listing.
/// </summary>
public sealed class AdministratorDashboardQueryTests
{
    // 2026-11-10 21:39 SAST.
    private static readonly DateTime Now = new(2026, 11, 10, 19, 39, 0, DateTimeKind.Utc);

    [Fact]
    public async Task EveryPartAnswers_TheAccountsAreTheDirectorysCount_AndNoRoleListingIsRead()
    {
        await using var db = CreateDb();
        var users = Directory(new AccountCount(24, 0));

        var result = await Handle(db, users.Object);

        result.ReadAt.Should().Be(Now);
        result.DatabaseFailedAt.Should().BeNull();
        result.Accounts.Should().Be(new AdministratorAccountsDto(24, 0));
        result.Jobs.Should().NotBeNull();
        users.Verify(s => s.CountAccountsAsync(Now, It.IsAny<CancellationToken>()), Times.Once);
        users.Verify(s => s.ListUsersInRoleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never,
            "the accounts are counted in one query, never by looping over the roles (K2)");
    }

    [Fact]
    public async Task TheLockedAreTheDirectorysToo()
    {
        await using var db = CreateDb();

        var result = await Handle(db, Directory(new AccountCount(24, 3)).Object);

        result.Accounts.Should().Be(new AdministratorAccountsDto(24, 3));
    }

    [Fact]
    public async Task AnAccountsReadThatFails_IsNotRead_AndTheOtherPartsStillAnswer()
    {
        await using var db = CreateDb();
        var users = new Mock<IUserAdministrationService>();
        users.Setup(s => s.CountAccountsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("The directory is down."));

        var result = await Handle(db, users.Object, Mail(configured: true));

        result.Accounts.Should().BeNull();
        result.DatabaseFailedAt.Should().BeNull();
        result.Jobs.Should().NotBeNull();
        result.Mail.Outcomes.Should().NotBeNull();
    }

    /// <summary>
    /// T9, R3-H08: the database does not answer. The Database line names the moment; the accounts, the jobs and the mail
    /// table are each not read; the Mail line with no mail server is configuration, and still answers.
    /// </summary>
    [Fact]
    public async Task TheDatabaseDown_NamesTheMoment_AndEveryPartThatNeedsItIsNotRead()
    {
        var db = CreateDb();
        await db.DisposeAsync();
        var users = new Mock<IUserAdministrationService>();
        users.Setup(s => s.CountAccountsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("The database did not answer."));

        var notConfigured = await Handle(db, users.Object, Mail(configured: false));

        notConfigured.DatabaseFailedAt.Should().Be(Now);
        notConfigured.Accounts.Should().BeNull();
        notConfigured.Jobs.Should().BeNull();
        notConfigured.Mail.Should().Be(new AdministratorMailDto(false, 0, null));

        var configured = await Handle(db, users.Object, Mail(configured: true, depth: 2));

        configured.Mail.IsConfigured.Should().BeTrue();
        configured.Mail.QueueDepth.Should().Be(2);
        configured.Mail.Outcomes.Should().BeNull("a mail server set and the database down reads Not read (Q18)");

        // The build review's S1: with the probe failed, the parts that need the database are not even tried, so a database
        // that does not answer costs Home one wait, not four.
        users.Verify(s => s.CountAccountsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>The build review's N-3: the handler is the second lock behind the page's Administrator gate.</summary>
    [Fact]
    public async Task SomeoneOtherThanTheAdministrator_IsRefused_AndNothingIsRead()
    {
        await using var db = CreateDb();
        var users = Directory(new AccountCount(24, 0));
        var handler = new GetAdministratorDashboardSummaryQueryHandler(
            db, users.Object, Mail(configured: true), new FixedClock(Now),
            NullLogger<GetAdministratorDashboardSummaryQueryHandler>.Instance);
        var institutionalAdmin = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "ia-1"), new Claim(ClaimTypes.Role, "InstitutionalAdmin")], "test"));

        var act = () => handler.Handle(new GetAdministratorDashboardSummaryQuery(institutionalAdmin), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        users.Verify(s => s.CountAccountsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WithNoMailServer_TheOutcomesAreNotRead()
    {
        await using var db = CreateDb();
        db.Set<EmailOutcome>().Add(EmailOutcome.Dropped(Now.AddHours(-1), "refused"));
        await db.SaveChangesAsync();

        var result = await Handle(db, Directory(new AccountCount(1, 0)).Object, Mail(configured: false));

        result.Mail.Should().Be(new AdministratorMailDto(false, 0, null));
    }

    /// <summary>Q13, Q18: the drops of the 24 hours before Home was read, the last of them with its refusal, the last sent.</summary>
    [Fact]
    public async Task TheMail_CountsTheDropsOfTheLast24Hours_AndNamesTheLastDropAndTheLastSent()
    {
        await using var db = CreateDb();
        db.Set<EmailOutcome>().AddRange(
            EmailOutcome.Dropped(Now.AddHours(-24), "exactly 24 hours ago: outside"),
            EmailOutcome.Dropped(Now.AddHours(-30), "older"),
            EmailOutcome.Dropped(Now.AddHours(-23).AddMinutes(-59), "550 mailbox unavailable"),
            EmailOutcome.Dropped(Now.AddHours(-2), "421 the server is busy"),
            EmailOutcome.Sent(Now.AddHours(-5)),
            EmailOutcome.Sent(Now.AddHours(-3)));
        await db.SaveChangesAsync();

        var result = await Handle(db, Directory(new AccountCount(1, 0)).Object, Mail(configured: true));

        result.Mail.Outcomes.Should().Be(new AdministratorMailOutcomesDto(
            WindowStart: Now.AddDays(-90),
            DroppedLast24Hours: 2,
            LastDropAt: Now.AddHours(-2),
            LastDropRefusal: "421 the server is busy",
            LastSentAt: Now.AddHours(-3)));
    }

    /// <summary>K3: a mail abandoned as the host stopped, or after a failed attempt while it waited, has no refusal.</summary>
    [Fact]
    public async Task ALastDropWithNoRefusal_ReadsNone()
    {
        await using var db = CreateDb();
        db.Set<EmailOutcome>().AddRange(
            EmailOutcome.Dropped(Now.AddHours(-3), "550 mailbox unavailable"),
            EmailOutcome.Dropped(Now.AddMinutes(-10), refusal: null));
        await db.SaveChangesAsync();

        var result = await Handle(db, Directory(new AccountCount(1, 0)).Object, Mail(configured: true));

        result.Mail.Outcomes!.DroppedLast24Hours.Should().Be(2);
        result.Mail.Outcomes.LastDropAt.Should().Be(Now.AddMinutes(-10));
        result.Mail.Outcomes.LastDropRefusal.Should().BeNull();
    }

    /// <summary>R3-H12: a mail server set and nothing sent in the 90 days the table holds: "Nothing sent since …".</summary>
    [Fact]
    public async Task NothingSentSinceTheWindowsStart_ReadsNoLastSent_AndNoDrop()
    {
        await using var db = CreateDb();
        db.Set<EmailOutcome>().Add(EmailOutcome.Sent(Now.AddDays(-91)));
        await db.SaveChangesAsync();

        var result = await Handle(db, Directory(new AccountCount(1, 0)).Object, Mail(configured: true, depth: 0));

        result.Mail.Outcomes.Should().Be(new AdministratorMailOutcomesDto(Now.AddDays(-90), 0, null, null, null));
    }

    /// <summary>Q3, K4: each job as the host judges it.</summary>
    [Fact]
    public async Task TheJobs_AreJudgedByTheHostsRule()
    {
        await using var db = CreateDb();
        db.Set<ScheduledJobDefinition>().AddRange(
            Job("a-succeeded", "0 7 * * *"),
            Job("b-failed", "0 7 * * *"),
            Job("c-running", "0 * * * *"),
            Job("d-unreadable", "every day at noon"),
            Job("e-disabled", "0 7 * * *", enabled: false),
            Job("f-overdue", "0 7 * * *"),
            Job("g-never-run", "0 8 * * 1"),
            Job("h-due-within-the-hour", "0 19 * * *"));
        db.Set<ScheduledJobRun>().AddRange(
            Run("a-succeeded", Now.Date.AddHours(7), ScheduledJobRunStatus.Succeeded),
            Run("b-failed", Now.Date.AddHours(7).AddDays(-1), ScheduledJobRunStatus.Succeeded),
            Run("b-failed", Now.Date.AddHours(7), ScheduledJobRunStatus.Failed,
                "Npgsql.PostgresException: 57P01 terminating connection\n   at Npgsql.Internal…"),
            // Started hours ago and still running: it holds its lock, so it is neither failing nor overdue (K4).
            Run("c-running", Now.AddHours(-5), ScheduledJobRunStatus.Running),
            Run("e-disabled", Now.AddDays(-3), ScheduledJobRunStatus.Succeeded),
            // Last ran yesterday at 07:00; today's 07:00 is 12 hours past.
            Run("f-overdue", Now.Date.AddHours(7).AddDays(-1), ScheduledJobRunStatus.Succeeded),
            // Last ran yesterday at 19:00; today's 19:00 is 39 minutes past: due, not overdue.
            Run("h-due-within-the-hour", Now.Date.AddHours(19).AddDays(-1), ScheduledJobRunStatus.Succeeded));
        await db.SaveChangesAsync();

        var result = await Handle(db, Directory(new AccountCount(1, 0)).Object);

        var jobs = result.Jobs!.Jobs.ToDictionary(job => job.Key);
        jobs.Keys.Should().Equal("a-succeeded", "b-failed", "c-running", "d-unreadable", "e-disabled", "f-overdue",
            "g-never-run", "h-due-within-the-hour");

        jobs["a-succeeded"].State.Should().Be(AdministratorJobState.Succeeded);

        jobs["b-failed"].State.Should().Be(AdministratorJobState.Failed);
        jobs["b-failed"].LastErrorFirstLine.Should().Be("Npgsql.PostgresException: 57P01 terminating connection");
        jobs["b-failed"].LastRunStatus.Should().Be("Failed");

        jobs["c-running"].State.Should().Be(AdministratorJobState.Running);

        jobs["d-unreadable"].State.Should().Be(AdministratorJobState.Unreadable);
        jobs["d-unreadable"].NextOccurrence.Should().BeNull();
        jobs["d-unreadable"].Cron.Should().Be("every day at noon");

        jobs["e-disabled"].State.Should().Be(AdministratorJobState.Disabled);
        jobs["e-disabled"].IsEnabled.Should().BeFalse();

        jobs["f-overdue"].State.Should().Be(AdministratorJobState.Overdue);
        jobs["f-overdue"].NextOccurrence.Should().Be(Now.Date.AddHours(7), "the time it was due");

        // Never run: the next Monday at 08:00 UTC after a day ago, 2026-11-16 08:00 UTC (10:00 SAST, R3-H02).
        jobs["g-never-run"].State.Should().Be(AdministratorJobState.NeverRun);
        jobs["g-never-run"].NextOccurrence.Should().Be(new DateTime(2026, 11, 16, 8, 0, 0, DateTimeKind.Utc));
        jobs["g-never-run"].LastRunStatus.Should().BeNull();

        jobs["h-due-within-the-hour"].State.Should().Be(AdministratorJobState.Succeeded,
            "an hour of grace before a job reads overdue (Q3)");
    }

    [Fact]
    public async Task AFailedRunWithNoError_HasNoFirstLine()
    {
        await using var db = CreateDb();
        db.Set<ScheduledJobDefinition>().Add(Job("b-failed", "0 7 * * *"));
        db.Set<ScheduledJobRun>().Add(Run("b-failed", Now.Date.AddHours(7), ScheduledJobRunStatus.Failed, "  "));
        await db.SaveChangesAsync();

        var result = await Handle(db, Directory(new AccountCount(1, 0)).Object);

        result.Jobs!.Jobs.Single().LastErrorFirstLine.Should().BeNull();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static Task<AdministratorDashboardSummaryDto> Handle(
        ApplicationDbContext db, IUserAdministrationService users, IEmailHealth? mail = null)
    {
        var handler = new GetAdministratorDashboardSummaryQueryHandler(
            db, users, mail ?? Mail(configured: false), new FixedClock(Now),
            NullLogger<GetAdministratorDashboardSummaryQueryHandler>.Instance);
        return handler.Handle(new GetAdministratorDashboardSummaryQuery(Administrator()), CancellationToken.None);
    }

    private static Mock<IUserAdministrationService> Directory(AccountCount count)
    {
        var users = new Mock<IUserAdministrationService>();
        users.Setup(s => s.CountAccountsAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(count);
        return users;
    }

    private static IEmailHealth Mail(bool configured, int depth = 0) => new FakeMail(configured, depth);

    private static ScheduledJobDefinition Job(string key, string cron, bool enabled = true)
        => new() { Key = key, CronExpression = cron, IsEnabled = enabled, Description = key };

    private static ScheduledJobRun Run(string key, DateTime startedAt, ScheduledJobRunStatus status, string? error = null)
        => new()
        {
            Key = key,
            StartedAt = startedAt,
            FinishedAt = status == ScheduledJobRunStatus.Running ? null : startedAt.AddSeconds(2),
            Status = status,
            ErrorMessage = error,
            TriggeredBy = "scheduler"
        };

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new ApplicationDbContext(options);
        db.Institutions.Add(new Institution { Id = 1, Name = "Test" });
        db.SaveChanges();
        return db;
    }

    private static ClaimsPrincipal Administrator()
        => new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "admin-1"), new Claim(ClaimTypes.Role, "Administrator")], "test"));

    private sealed record FakeMail(bool IsConfigured, int QueueDepth) : IEmailHealth;

    private sealed class FixedClock(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow);
    }
}
