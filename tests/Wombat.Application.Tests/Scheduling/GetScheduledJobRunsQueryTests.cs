using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Scheduling.Queries.GetScheduledJobRuns;
using Wombat.Domain.Scheduling;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Scheduling;

public sealed class GetScheduledJobRunsQueryTests
{
    [Fact]
    public async Task Handle_ReturnsRunsOrderedByStartedAtDescending()
    {
        await using var db = CreateDb();
        db.Set<ScheduledJobRun>().AddRange(
            new ScheduledJobRun
            {
                Key = "job-a",
                StartedAt = new DateTime(2026, 4, 10, 7, 0, 0, DateTimeKind.Utc),
                Status = ScheduledJobRunStatus.Succeeded
            },
            new ScheduledJobRun
            {
                Key = "job-a",
                StartedAt = new DateTime(2026, 4, 11, 7, 0, 0, DateTimeKind.Utc),
                Status = ScheduledJobRunStatus.Failed,
                ErrorMessage = "Something broke"
            });
        await db.SaveChangesAsync();

        var handler = new GetScheduledJobRunsQueryHandler(db);
        var result = await handler.Handle(new GetScheduledJobRunsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].StartedAt.Should().BeAfter(result[1].StartedAt);
    }

    [Fact]
    public async Task Handle_FiltersOnKey()
    {
        await using var db = CreateDb();
        db.Set<ScheduledJobRun>().AddRange(
            new ScheduledJobRun { Key = "job-a", StartedAt = DateTime.UtcNow, Status = ScheduledJobRunStatus.Succeeded },
            new ScheduledJobRun { Key = "job-b", StartedAt = DateTime.UtcNow, Status = ScheduledJobRunStatus.Succeeded });
        await db.SaveChangesAsync();

        var handler = new GetScheduledJobRunsQueryHandler(db);
        var result = await handler.Handle(new GetScheduledJobRunsQuery(Key: "job-a"), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Key.Should().Be("job-a");
    }

    [Fact]
    public async Task Handle_FiltersOnStatus()
    {
        await using var db = CreateDb();
        db.Set<ScheduledJobRun>().AddRange(
            new ScheduledJobRun { Key = "job-a", StartedAt = DateTime.UtcNow, Status = ScheduledJobRunStatus.Succeeded },
            new ScheduledJobRun { Key = "job-a", StartedAt = DateTime.UtcNow, Status = ScheduledJobRunStatus.Failed, ErrorMessage = "err" });
        await db.SaveChangesAsync();

        var handler = new GetScheduledJobRunsQueryHandler(db);
        var result = await handler.Handle(new GetScheduledJobRunsQuery(Status: "Failed"), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Status.Should().Be("Failed");
    }

    [Fact]
    public async Task Handle_FiltersOnDateRange()
    {
        await using var db = CreateDb();
        db.Set<ScheduledJobRun>().AddRange(
            new ScheduledJobRun { Key = "job-a", StartedAt = new DateTime(2026, 4, 5, 7, 0, 0, DateTimeKind.Utc), Status = ScheduledJobRunStatus.Succeeded },
            new ScheduledJobRun { Key = "job-a", StartedAt = new DateTime(2026, 4, 12, 7, 0, 0, DateTimeKind.Utc), Status = ScheduledJobRunStatus.Succeeded });
        await db.SaveChangesAsync();

        var handler = new GetScheduledJobRunsQueryHandler(db);
        var result = await handler.Handle(
            new GetScheduledJobRunsQuery(From: new DateOnly(2026, 4, 10), To: new DateOnly(2026, 4, 15)),
            CancellationToken.None);

        result.Should().ContainSingle();
    }

    private static ApplicationDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
