using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Scheduling;
using Wombat.Application.Features.Scheduling.Queries.GetScheduledJobRuns;
using Wombat.Domain.DataRights;
using Wombat.Domain.Scheduling;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Scheduling;

/// <summary>
/// Run history's read (T400, flow 18, lane B; T1, K7, M3): a page and a total in place of the 200 it cut at, the job by
/// its whole key, From and To as South African days, Ran by by name, and the jobs' keys for the Job select.
/// </summary>
public sealed class GetScheduledJobRunsQueryTests
{
    [Fact]
    public async Task Handle_ReturnsRunsOrderedByStartedAtDescending()
    {
        await using var db = CreateDb();
        db.Set<ScheduledJobRun>().AddRange(
            Run("job-a", new DateTime(2026, 4, 10, 7, 0, 0, DateTimeKind.Utc), ScheduledJobRunStatus.Succeeded),
            Run("job-a", new DateTime(2026, 4, 11, 7, 0, 0, DateTimeKind.Utc), ScheduledJobRunStatus.Failed, error: "Something broke"));
        await db.SaveChangesAsync();

        var result = await Handle(db, new GetScheduledJobRunsQuery());

        result.Items.Should().HaveCount(2);
        result.Items[0].StartedAt.Should().BeAfter(result.Items[1].StartedAt);
        result.Items[0].ErrorMessage.Should().Be("Something broke");
    }

    [Fact]
    public async Task TheJob_IsMatchedByItsWholeKey_NeverByPart()
    {
        await using var db = CreateDb();
        db.Set<ScheduledJobRun>().AddRange(
            Run("weekly-coordinator-digest", DateTime.UtcNow, ScheduledJobRunStatus.Succeeded),
            Run("job-b", DateTime.UtcNow, ScheduledJobRunStatus.Succeeded));
        await db.SaveChangesAsync();

        (await Handle(db, new GetScheduledJobRunsQuery(Key: "weekly-coordinator-digest"))).Items
            .Should().ContainSingle().Which.Key.Should().Be("weekly-coordinator-digest");
        (await Handle(db, new GetScheduledJobRunsQuery(Key: "digest"))).Items
            .Should().BeEmpty("the select sends a whole key; \"digest\" is no job's (T1)");
    }

    [Fact]
    public async Task Handle_FiltersOnStatus_AndIgnoresAStatusThatIsNoneOfThem()
    {
        await using var db = CreateDb();
        db.Set<ScheduledJobRun>().AddRange(
            Run("job-a", DateTime.UtcNow, ScheduledJobRunStatus.Succeeded),
            Run("job-a", DateTime.UtcNow, ScheduledJobRunStatus.Failed, error: "err"));
        await db.SaveChangesAsync();

        (await Handle(db, new GetScheduledJobRunsQuery(Status: "Failed"))).Items
            .Should().ContainSingle().Which.Status.Should().Be("Failed");
        (await Handle(db, new GetScheduledJobRunsQuery(Status: "2"))).TotalCount.Should().Be(2, "a number is no status's name");
        (await Handle(db, new GetScheduledJobRunsQuery(Status: "Skipped"))).TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task FromAndTo_AreSouthAfricanDays_SoTheirEdgesAre22hUtc()
    {
        // 2026-11-10 in South Africa is [2026-11-09 22:00 UTC, 2026-11-10 22:00 UTC) (K7).
        await using var db = CreateDb();
        db.Set<ScheduledJobRun>().AddRange(
            Run("before", new DateTime(2026, 11, 9, 21, 59, 59, DateTimeKind.Utc), ScheduledJobRunStatus.Succeeded),
            Run("first", new DateTime(2026, 11, 9, 22, 0, 0, DateTimeKind.Utc), ScheduledJobRunStatus.Succeeded),
            Run("last", new DateTime(2026, 11, 10, 21, 59, 59, DateTimeKind.Utc), ScheduledJobRunStatus.Succeeded),
            Run("after", new DateTime(2026, 11, 10, 22, 0, 0, DateTimeKind.Utc), ScheduledJobRunStatus.Succeeded));
        await db.SaveChangesAsync();

        var day = new DateOnly(2026, 11, 10);
        (await Handle(db, new GetScheduledJobRunsQuery(From: day, To: day))).Items.Select(run => run.Key)
            .Should().Equal("last", "first");
        (await Handle(db, new GetScheduledJobRunsQuery(From: day))).Items.Select(run => run.Key)
            .Should().Equal("after", "last", "first");
        (await Handle(db, new GetScheduledJobRunsQuery(To: day))).Items.Select(run => run.Key)
            .Should().Equal("last", "first", "before");
    }

    /// <summary>The build review's N-1: the calendar's first and last days, typed into the address, bound nothing.</summary>
    [Fact]
    public async Task FromAndTo_AtTheCalendarsEdges_BoundNothing_AndDoNotThrow()
    {
        await using var db = CreateDb();
        db.Set<ScheduledJobRun>().Add(Run("job-a", new DateTime(2026, 11, 10, 7, 0, 0, DateTimeKind.Utc), ScheduledJobRunStatus.Succeeded));
        await db.SaveChangesAsync();

        (await Handle(db, new GetScheduledJobRunsQuery(From: DateOnly.MinValue, To: DateOnly.MaxValue))).TotalCount.Should().Be(1);
        (await Handle(db, new GetScheduledJobRunsQuery(From: new DateOnly(1, 1, 2), To: new DateOnly(9999, 12, 30)))).TotalCount
            .Should().Be(1);
    }

    [Fact]
    public async Task APage_HoldsItsRuns_WithTheMatchingTotal_AndEveryRunHeld()
    {
        await using var db = CreateDb();
        var start = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);
        for (var hour = 0; hour < 120; hour++)
        {
            db.Set<ScheduledJobRun>().Add(Run("msf-campaign-auto-close", start.AddHours(hour), ScheduledJobRunStatus.Succeeded));
        }

        db.Set<ScheduledJobRun>().Add(Run("audit-log-retention", start, ScheduledJobRunStatus.Failed, error: "x"));
        await db.SaveChangesAsync();

        var first = await Handle(db, new GetScheduledJobRunsQuery(Key: "msf-campaign-auto-close"));
        first.Page.Should().Be(1);
        first.PageSize.Should().Be(50, "fifty a page (R3-R01)");
        first.Items.Should().HaveCount(50);
        first.TotalCount.Should().Be(120, "the total, not the 200 it once cut at (R3-R06)");
        first.AllRunsCount.Should().Be(121, "every run held, for \"Clear them to see all …\"");
        first.Items[0].StartedAt.Should().Be(start.AddHours(119));

        var second = await Handle(db, new GetScheduledJobRunsQuery(Key: "msf-campaign-auto-close", Page: 2));
        second.Items[0].StartedAt.Should().Be(start.AddHours(69));

        var third = await Handle(db, new GetScheduledJobRunsQuery(Key: "msf-campaign-auto-close", Page: 3));
        third.Items.Should().HaveCount(20);

        (await Handle(db, new GetScheduledJobRunsQuery(Key: "msf-campaign-auto-close", Page: 9))).Page
            .Should().Be(3, "a page past the last reads the last");
        (await Handle(db, new GetScheduledJobRunsQuery(PageSize: 7))).PageSize.Should().Be(50, "only the pager's sizes");
        (await Handle(db, new GetScheduledJobRunsQuery(PageSize: 100))).Items.Should().HaveCount(100);
    }

    [Fact]
    public async Task RanBy_IsTheLiveName_AnErasedAdministratorsPseudonym_AndNullForTheScheduler()
    {
        await using var db = CreateDb();
        var at = new DateTime(2026, 11, 10, 19, 41, 0, DateTimeKind.Utc);
        db.Set<ScheduledJobRun>().AddRange(
            Run("by-hand", at, ScheduledJobRunStatus.Succeeded, by: "admin-1"),
            Run("by-erased", at.AddMinutes(-1), ScheduledJobRunStatus.Succeeded, by: "admin-2"),
            Run("by-scheduler", at.AddMinutes(-2), ScheduledJobRunStatus.Succeeded));
        db.Set<DataRightsErasureRecord>().Add(DataRightsErasureRecord.Create(
            Guid.NewGuid(), "admin-2", "deleted_user_0ae1d64b", at.AddDays(-1), "[]"));
        await db.SaveChangesAsync();

        var users = new FakeUserDirectory(("admin-1", "Demo Administrator"), ("admin-2", "Old Name"));
        var runs = (await Handle(db, new GetScheduledJobRunsQuery(), users)).Items;

        runs.Select(run => run.RanBy).Should().Equal("Demo Administrator", "deleted_user_0ae1d64b", null);
    }

    [Fact]
    public async Task TheJobKeys_AreTheStoredJobsByKey()
    {
        await using var db = CreateDb();
        db.Set<ScheduledJobDefinition>().AddRange(
            new ScheduledJobDefinition { Key = "weekly-coordinator-digest", CronExpression = "0 8 * * 1", Description = "d" },
            new ScheduledJobDefinition { Key = "activity-draft-nudge", CronExpression = "0 7 * * *", Description = "d" });
        await db.SaveChangesAsync();

        var result = await Handle(db, new GetScheduledJobRunsQuery());

        result.JobKeys.Should().Equal("activity-draft-nudge", "weekly-coordinator-digest");
        result.Items.Should().BeEmpty();
        result.AllRunsCount.Should().Be(0);
    }

    private static Task<ScheduledJobRunsPageDto> Handle(ApplicationDbContext db, GetScheduledJobRunsQuery query, FakeUserDirectory? users = null)
        => new GetScheduledJobRunsQueryHandler(db, users ?? FakeUserDirectory.Empty).Handle(query, CancellationToken.None);

    private static ScheduledJobRun Run(string key, DateTime startedAt, ScheduledJobRunStatus status, string? error = null, string? by = null)
        => new()
        {
            Key = key,
            StartedAt = startedAt,
            FinishedAt = startedAt.AddMilliseconds(20),
            Status = status,
            ErrorMessage = error,
            TriggeredBy = by
        };

    private static ApplicationDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
