using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Scheduling.Queries.GetScheduledJobRuns;
using Wombat.Application.Scheduling;
using Wombat.Domain.Email;
using Wombat.Domain.Scheduling;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling.Jobs;
using Wombat.Integration.Tests.TestSupport;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.Scheduling;

/// <summary>
/// T400 (flow 18, lane B; T1, K7, Q18) on a real PostgreSQL server: Run history's page and total, its South African day
/// range at 22:00 UTC on <c>timestamp with time zone</c>, and the retention job's delete of mail outcomes older than 90 days
/// beside the runs.
/// </summary>
/// <remarks>Isolated as <c>EmailOutcomePostgresTests</c> is: a migrated schema of its own, dropped in a finally and on dispose.</remarks>
public sealed class JobRunsPostgresTests : IAsyncLifetime
{
    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task RunHistory_PagesWithATotal_AndReadsFromAndToAsSouthAfricanDays()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var start = new DateTime(2026, 11, 9, 0, 0, 0, DateTimeKind.Utc);
            await using (var db = NewContext(schema))
            {
                // 72 hourly runs from 2026-11-09 00:00 UTC, the last at 2026-11-11 23:00 UTC.
                for (var hour = 0; hour < 72; hour++)
                {
                    db.Set<ScheduledJobRun>().Add(new ScheduledJobRun
                    {
                        Key = "msf-campaign-auto-close",
                        StartedAt = start.AddHours(hour),
                        FinishedAt = start.AddHours(hour).AddMilliseconds(30),
                        Status = ScheduledJobRunStatus.Succeeded
                    });
                }

                db.Set<ScheduledJobRun>().Add(new ScheduledJobRun
                {
                    Key = "weekly-coordinator-digest",
                    StartedAt = start.AddHours(10),
                    Status = ScheduledJobRunStatus.Succeeded,
                    TriggeredBy = "admin-1"
                });
                await db.SaveChangesAsync();
            }

            await using (var db = NewContext(schema))
            {
                var handler = new GetScheduledJobRunsQueryHandler(db, new FakeUserDirectory(("admin-1", "Demo Administrator")));

                var first = await handler.Handle(new GetScheduledJobRunsQuery(Key: "msf-campaign-auto-close"), CancellationToken.None);
                first.TotalCount.Should().Be(72);
                first.AllRunsCount.Should().Be(73);
                first.Items.Should().HaveCount(50);
                first.Items[0].StartedAt.Should().Be(start.AddHours(71));
                first.Items[0].StartedAt.Kind.Should().Be(DateTimeKind.Utc);

                var second = await handler.Handle(new GetScheduledJobRunsQuery(Key: "msf-campaign-auto-close", Page: 2), CancellationToken.None);
                second.Items.Should().HaveCount(22);
                second.Items[^1].StartedAt.Should().Be(start);

                // 2026-11-10 in South Africa: [2026-11-09 22:00 UTC, 2026-11-10 22:00 UTC), 24 hourly runs.
                var day = new DateOnly(2026, 11, 10);
                var tenth = await handler.Handle(
                    new GetScheduledJobRunsQuery(Key: "msf-campaign-auto-close", From: day, To: day), CancellationToken.None);
                tenth.TotalCount.Should().Be(24);
                tenth.Items[^1].StartedAt.Should().Be(new DateTime(2026, 11, 9, 22, 0, 0, DateTimeKind.Utc), "00:00 SAST is in");
                tenth.Items[0].StartedAt.Should().Be(new DateTime(2026, 11, 10, 21, 0, 0, DateTimeKind.Utc), "22:00 UTC is the 11th's");

                var byHand = await handler.Handle(new GetScheduledJobRunsQuery(Key: "weekly-coordinator-digest"), CancellationToken.None);
                byHand.Items.Should().ContainSingle().Which.RanBy.Should().Be("Demo Administrator");
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task TheRetentionJob_DeletesRunsAndMailOutcomesOlderThan90Days_AndKeepsTheRest()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var now = new DateTime(2026, 11, 11, 2, 0, 0, DateTimeKind.Utc);
            var cutoff = now.AddDays(-90);
            await using (var db = NewContext(schema))
            {
                db.Set<ScheduledJobRun>().AddRange(
                    new ScheduledJobRun { Key = "old", StartedAt = cutoff.AddMinutes(-1), Status = ScheduledJobRunStatus.Succeeded },
                    new ScheduledJobRun { Key = "edge", StartedAt = cutoff, Status = ScheduledJobRunStatus.Succeeded },
                    new ScheduledJobRun { Key = "new", StartedAt = cutoff.AddMinutes(1), Status = ScheduledJobRunStatus.Succeeded });
                db.Set<EmailOutcome>().AddRange(
                    EmailOutcome.Sent(cutoff.AddDays(-10)),
                    EmailOutcome.Dropped(cutoff.AddMinutes(-1), "550 refused"),
                    EmailOutcome.Sent(cutoff),
                    EmailOutcome.Sent(cutoff.AddMinutes(1)),
                    EmailOutcome.Dropped(now.AddHours(-1), null));
                await db.SaveChangesAsync();
            }

            var logged = await RunRetentionAsync(schema, now);
            logged.Should().Equal(
                "ScheduledJobRunRetentionJob: deleted 1 old job run records and 2 old mail outcomes.");

            await using (var db = NewContext(schema))
            {
                (await db.Set<ScheduledJobRun>().OrderBy(run => run.StartedAt).Select(run => run.Key).ToListAsync())
                    .Should().Equal("edge", "new");
                (await db.Set<EmailOutcome>().OrderBy(outcome => outcome.OccurredAt).Select(outcome => outcome.OccurredAt).ToListAsync())
                    .Should().Equal(cutoff, cutoff.AddMinutes(1), now.AddHours(-1));
            }

            // A second run finds nothing old: the sentence step A.2.9 expects in the log.
            (await RunRetentionAsync(schema, now)).Should().Equal(
                "ScheduledJobRunRetentionJob: no old runs or mail outcomes to delete.");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>Runs the retention job once against <paramref name="schema" />; what it logged, each line formatted.</summary>
    private static async Task<IReadOnlyList<string>> RunRetentionAsync(string schema, DateTime now)
    {
        var logger = new LinesLogger();
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(TestDatabase.SchemaConnectionString(schema)));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        await using var provider = services.BuildServiceProvider();
        await new ScheduledJobRunRetentionJob(provider.GetRequiredService<IServiceScopeFactory>())
            .ExecuteAsync(new ScheduledJobContext(now, logger), CancellationToken.None);
        return logger.Lines;
    }

    private sealed class LinesLogger : Microsoft.Extensions.Logging.ILogger
    {
        public List<string> Lines { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;

        public void Log<TState>(
            Microsoft.Extensions.Logging.LogLevel logLevel,
            Microsoft.Extensions.Logging.EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
            => Lines.Add(formatter(state, exception));
    }

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
}
