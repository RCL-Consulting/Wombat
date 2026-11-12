using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Scheduling.Queries.GetScheduledJobStatus;
using Wombat.Domain.DataRights;
using Wombat.Domain.Scheduling;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Scheduling;

/// <summary>
/// Scheduled jobs' read (T400, flow 18, lane B; R3-J01–J10): each job by key with its last run (status, start, finish,
/// the first line of a failed run's error, and who ran it, by name) and its next run by the host's own rule (K5).
/// </summary>
public sealed class GetScheduledJobStatusQueryTests
{
    // 2026-11-10 21:39 SAST, R3-J04's moment.
    private static readonly DateTime Now = new(2026, 11, 10, 19, 39, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Handle_ReturnsAllDefinitions_ByKey()
    {
        await using var db = CreateDb();
        db.Set<ScheduledJobDefinition>().AddRange(
            Definition("job-b", "0 8 * * *", enabled: false),
            Definition("job-a", "0 7 * * *"));
        await db.SaveChangesAsync();

        var result = await Handle(db);

        result.Select(job => job.Key).Should().Equal("job-a", "job-b");
        result[0].IsEnabled.Should().BeTrue();
        result[1].IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task TheLastRun_ItsStatusStartFinish_AndAFailedRunsFirstErrorLine()
    {
        await using var db = CreateDb();
        db.Set<ScheduledJobDefinition>().Add(Definition("job-a", "0 7 * * *"));
        db.Set<ScheduledJobRun>().AddRange(
            Run("job-a", Now.AddHours(-3), ScheduledJobRunStatus.Succeeded),
            Run("job-a", Now.AddHours(-1), ScheduledJobRunStatus.Failed,
                error: "Marked as failed during startup reconciliation — process likely crashed.\n   at Wombat.Something()"));
        await db.SaveChangesAsync();

        var job = (await Handle(db)).Single();

        job.LastRunAt.Should().Be(Now.AddHours(-1));
        job.LastRunFinishedAt.Should().Be(Now.AddHours(-1).AddMilliseconds(70));
        job.LastRunStatus.Should().Be("Failed");
        job.LastRunError.Should().Be("Marked as failed during startup reconciliation — process likely crashed.");
    }

    [Fact]
    public async Task ASucceededLastRun_CarriesNoError()
    {
        await using var db = CreateDb();
        db.Set<ScheduledJobDefinition>().Add(Definition("job-a", "0 7 * * *"));
        db.Set<ScheduledJobRun>().Add(Run("job-a", Now.AddHours(-1), ScheduledJobRunStatus.Succeeded, error: "left over"));
        await db.SaveChangesAsync();

        (await Handle(db)).Single().LastRunError.Should().BeNull("no \"Error: None\" on a run that did not fail (N9)");
    }

    [Fact]
    public async Task WhoRanTheLastRun_IsTheirName_AnErasedPersonsPseudonym_AndNullForTheScheduler()
    {
        await using var db = CreateDb();
        db.Set<ScheduledJobDefinition>().AddRange(
            Definition("by-hand", "0 7 * * *"), Definition("by-erased", "0 7 * * *"), Definition("by-scheduler", "0 7 * * *"));
        db.Set<ScheduledJobRun>().AddRange(
            Run("by-hand", Now.AddHours(-1), ScheduledJobRunStatus.Succeeded, by: "admin-1"),
            Run("by-erased", Now.AddHours(-1), ScheduledJobRunStatus.Succeeded, by: "admin-2"),
            Run("by-scheduler", Now.AddHours(-1), ScheduledJobRunStatus.Succeeded));
        db.Set<DataRightsErasureRecord>().Add(DataRightsErasureRecord.Create(
            Guid.NewGuid(), "admin-2", "deleted_user_0ae1d64b", Now.AddDays(-1), "[]"));
        await db.SaveChangesAsync();

        var jobs = (await Handle(db, new FakeUserDirectory(("admin-1", "Demo Administrator"), ("admin-2", "Old Name"))))
            .ToDictionary(job => job.Key);

        jobs["by-hand"].LastRunBy.Should().Be("Demo Administrator");
        jobs["by-erased"].LastRunBy.Should().Be("deleted_user_0ae1d64b");
        jobs["by-scheduler"].LastRunBy.Should().BeNull();
    }

    [Fact]
    public async Task NextRun_IsTheCronsNextOccurrenceAfterTheLastRunsStart_TheHostsRule()
    {
        // R3-J03's K5 case: msf-campaign-auto-close last ran at 21:00 SAST, so its next run is 22:00 SAST.
        await using var db = CreateDb();
        db.Set<ScheduledJobDefinition>().Add(Definition("msf-campaign-auto-close", "0 * * * *"));
        db.Set<ScheduledJobRun>().Add(Run("msf-campaign-auto-close", new DateTime(2026, 11, 10, 19, 0, 3, DateTimeKind.Utc), ScheduledJobRunStatus.Succeeded));
        await db.SaveChangesAsync();

        var job = (await Handle(db)).Single();

        job.NextRunAt.Should().Be(new DateTime(2026, 11, 10, 20, 0, 0, DateTimeKind.Utc));
        job.IsDue.Should().BeFalse();
        job.ScheduleReadable.Should().BeTrue();
    }

    [Fact]
    public async Task AJobPastItsTime_IsDue_AtTheNextCheck()
    {
        // Disabled at 20:30 SAST past its 21:00 SAST run and enabled at 21:39: due at once (K5).
        await using var db = CreateDb();
        db.Set<ScheduledJobDefinition>().Add(Definition("msf-campaign-auto-close", "0 * * * *"));
        db.Set<ScheduledJobRun>().Add(Run("msf-campaign-auto-close", new DateTime(2026, 11, 10, 18, 0, 3, DateTimeKind.Utc), ScheduledJobRunStatus.Succeeded));
        await db.SaveChangesAsync();

        var job = (await Handle(db)).Single();

        job.NextRunAt.Should().Be(new DateTime(2026, 11, 10, 19, 0, 0, DateTimeKind.Utc));
        job.IsDue.Should().BeTrue();
    }

    [Fact]
    public async Task AJobNeverRun_IsJudgedFromADayBeforeNow()
    {
        await using var db = CreateDb();
        db.Set<ScheduledJobDefinition>().Add(Definition("weekly-coordinator-digest", "0 8 * * 1"));
        await db.SaveChangesAsync();

        var job = (await Handle(db)).Single();

        job.LastRunAt.Should().BeNull();
        job.LastRunStatus.Should().BeNull();
        job.NextRunAt.Should().Be(new DateTime(2026, 11, 16, 8, 0, 0, DateTimeKind.Utc), "Monday 2026-11-16 10:00 SAST, R3-J01");
        job.IsDue.Should().BeFalse();
    }

    [Fact]
    public async Task ADisabledJob_HasNoNextRun_AndIsNotDue()
    {
        await using var db = CreateDb();
        db.Set<ScheduledJobDefinition>().Add(Definition("disabled-job", "0 * * * *", enabled: false));
        db.Set<ScheduledJobRun>().Add(Run("disabled-job", Now.AddHours(-5), ScheduledJobRunStatus.Succeeded));
        await db.SaveChangesAsync();

        var job = (await Handle(db)).Single();

        job.NextRunAt.Should().BeNull();
        job.IsDue.Should().BeFalse();
    }

    [Fact]
    public async Task ACronThatDoesNotParse_IsUnreadable_WithNoNextRun()
    {
        await using var db = CreateDb();
        db.Set<ScheduledJobDefinition>().Add(Definition("broken", "not a cron"));
        await db.SaveChangesAsync();

        var job = (await Handle(db)).Single();

        job.ScheduleReadable.Should().BeFalse();
        job.NextRunAt.Should().BeNull();
        job.IsDue.Should().BeFalse();
    }

    private static Task<IReadOnlyList<Wombat.Application.Features.Scheduling.ScheduledJobDto>> Handle(ApplicationDbContext db, FakeUserDirectory? users = null)
        => new GetScheduledJobStatusQueryHandler(db, users ?? FakeUserDirectory.Empty, new FixedClock(Now))
            .Handle(new GetScheduledJobStatusQuery(), CancellationToken.None);

    private static ScheduledJobDefinition Definition(string key, string cron, bool enabled = true)
        => new() { Key = key, CronExpression = cron, IsEnabled = enabled, Description = key };

    private static ScheduledJobRun Run(string key, DateTime startedAt, ScheduledJobRunStatus status, string? error = null, string? by = null)
        => new()
        {
            Key = key,
            StartedAt = startedAt,
            FinishedAt = startedAt.AddMilliseconds(70),
            Status = status,
            ErrorMessage = error,
            TriggeredBy = by
        };

    private static ApplicationDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private sealed class FixedClock(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow);
    }
}
