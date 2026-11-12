using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Wombat.Application.Common.Email;
using Wombat.Application.Features.Dashboards.Administrator;
using Wombat.Domain.Email;
using Wombat.Domain.Scheduling;
using Wombat.Infrastructure.Persistence;
using Wombat.Integration.Tests.TestSupport;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.Dashboards;

/// <summary>
/// T400 (flow 18, Q13, Q18, K4) on a real PostgreSQL server: Home's Mail line read from <c>EmailOutcomes</c> (the drops of
/// the 24 hours before Home was read, the 24-hour edge, the last drop's refusal, the last sent inside the 90 days the
/// table holds), and its jobs line's last run per job, as <see cref="GetAdministratorDashboardSummaryQueryHandler" /> reads
/// them through the migrated schema.
/// </summary>
public sealed class AdministratorHomePostgresTests : IAsyncLifetime
{
    // 2026-11-10 21:39:41 SAST.
    private static readonly DateTime Now = new(2026, 11, 10, 19, 39, 41, DateTimeKind.Utc);

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task TheMailLine_IsReadFromTheOutcomesTable_ToThe24HourEdge()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            await using (var db = NewContext(schema))
            {
                db.Set<EmailOutcome>().AddRange(
                    EmailOutcome.Dropped(Now.AddHours(-24), "exactly 24 hours ago: outside"),
                    EmailOutcome.Dropped(Now.AddHours(-24).AddMilliseconds(1), "a millisecond inside"),
                    EmailOutcome.Dropped(Now.AddHours(-2), "550 5.1.1 Mailbox unavailable"),
                    EmailOutcome.Dropped(Now.AddDays(-3), "older"),
                    EmailOutcome.Sent(Now.AddMinutes(-30)),
                    EmailOutcome.Sent(Now.AddDays(-91)));
                await db.SaveChangesAsync();
            }

            await using (var db = NewContext(schema))
            {
                var result = await HandleAsync(db, configured: true);

                result.DatabaseFailedAt.Should().BeNull();
                result.Mail.Should().Be(new AdministratorMailDto(true, 1, new AdministratorMailOutcomesDto(
                    WindowStart: Now.AddDays(-90),
                    DroppedLast24Hours: 2,
                    LastDropAt: Now.AddHours(-2),
                    LastDropRefusal: "550 5.1.1 Mailbox unavailable",
                    LastSentAt: Now.AddMinutes(-30))));
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>R3-H12: nothing sent in the 90 days the table holds, and a drop with no refusal (K3).</summary>
    [Fact]
    public async Task NothingSentInTheWindow_AndALastDropWithNoRefusal()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            await using (var db = NewContext(schema))
            {
                db.Set<EmailOutcome>().AddRange(
                    EmailOutcome.Sent(Now.AddDays(-90).AddSeconds(-1)),
                    EmailOutcome.Dropped(Now.AddHours(-1), "421 Try again later"),
                    EmailOutcome.Dropped(Now.AddMinutes(-1), refusal: null));
                await db.SaveChangesAsync();
            }

            await using (var db = NewContext(schema))
            {
                var outcomes = (await HandleAsync(db, configured: true)).Mail.Outcomes!;

                outcomes.LastSentAt.Should().BeNull();
                outcomes.WindowStart.Should().Be(Now.AddDays(-90));
                outcomes.DroppedLast24Hours.Should().Be(2);
                outcomes.LastDropAt.Should().Be(Now.AddMinutes(-1));
                outcomes.LastDropRefusal.Should().BeNull();
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>The jobs line reads each job's last run on PostgreSQL (the group-and-first the Scheduled jobs page uses).</summary>
    [Fact]
    public async Task TheJobsLine_ReadsEachJobsLastRun()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            await using (var db = NewContext(schema))
            {
                db.Set<ScheduledJobDefinition>().AddRange(
                    new ScheduledJobDefinition { Key = "entrustment-decision-expiry", CronExpression = "0 7 * * *", Description = "d" },
                    new ScheduledJobDefinition { Key = "weekly-coordinator-digest", CronExpression = "0 8 * * 1", Description = "d" });
                db.Set<ScheduledJobRun>().AddRange(
                    new ScheduledJobRun
                    {
                        Key = "entrustment-decision-expiry", StartedAt = Now.Date.AddHours(7).AddDays(-1),
                        FinishedAt = Now.Date.AddHours(7).AddDays(-1), Status = ScheduledJobRunStatus.Succeeded
                    },
                    new ScheduledJobRun
                    {
                        Key = "entrustment-decision-expiry", StartedAt = Now.Date.AddHours(7), FinishedAt = Now.Date.AddHours(7),
                        Status = ScheduledJobRunStatus.Failed, ErrorMessage = "Timeout expired.\nat Npgsql…"
                    });
                await db.SaveChangesAsync();
            }

            await using (var db = NewContext(schema))
            {
                var jobs = (await HandleAsync(db, configured: false)).Jobs!.Jobs;

                jobs.Select(job => (job.Key, job.State)).Should().Equal(
                    ("entrustment-decision-expiry", AdministratorJobState.Failed),
                    ("weekly-coordinator-digest", AdministratorJobState.NeverRun));
                jobs[0].LastErrorFirstLine.Should().Be("Timeout expired.");
                jobs[0].LastRunStartedAt.Should().Be(Now.Date.AddHours(7));
                jobs[1].NextOccurrence.Should().Be(new DateTime(2026, 11, 16, 8, 0, 0, DateTimeKind.Utc));
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static Task<AdministratorDashboardSummaryDto> HandleAsync(ApplicationDbContext db, bool configured)
        => new GetAdministratorDashboardSummaryQueryHandler(
                db, FakeUserDirectory.Empty, new FakeMail(configured, configured ? 1 : 0), new FixedClock(Now),
                NullLogger<GetAdministratorDashboardSummaryQueryHandler>.Instance)
            .Handle(new GetAdministratorDashboardSummaryQuery(new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.Role, "Administrator")], "Test"))), CancellationToken.None);

    private async Task<string> MigratedSchemaAsync()
    {
        var schema = await _schemas.CreateAsync();
        await using var db = NewContext(schema);
        await db.Database.MigrateAsync();
        return schema;
    }

    private static ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(TestDatabase.SchemaConnectionString(schema))
            .Options);

    private sealed record FakeMail(bool IsConfigured, int QueueDepth) : IEmailHealth;

    private sealed class FixedClock(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow);
    }
}
