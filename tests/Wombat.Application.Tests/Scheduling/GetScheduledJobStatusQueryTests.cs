using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Scheduling.Queries.GetScheduledJobStatus;
using Wombat.Domain.Scheduling;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Scheduling;

public sealed class GetScheduledJobStatusQueryTests
{
    [Fact]
    public async Task Handle_ReturnsAllDefinitions()
    {
        await using var db = CreateDb();
        db.Set<ScheduledJobDefinition>().Add(new ScheduledJobDefinition
        {
            Key = "job-a",
            CronExpression = "0 7 * * *",
            IsEnabled = true,
            Description = "Job A"
        });
        db.Set<ScheduledJobDefinition>().Add(new ScheduledJobDefinition
        {
            Key = "job-b",
            CronExpression = "0 8 * * *",
            IsEnabled = false,
            Description = "Job B"
        });
        await db.SaveChangesAsync();

        var handler = new GetScheduledJobStatusQueryHandler(db);
        var result = await handler.Handle(new GetScheduledJobStatusQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().Contain(j => j.Key == "job-a" && j.IsEnabled);
        result.Should().Contain(j => j.Key == "job-b" && !j.IsEnabled);
    }

    [Fact]
    public async Task Handle_IncludesLastRunInfo()
    {
        await using var db = CreateDb();
        db.Set<ScheduledJobDefinition>().Add(new ScheduledJobDefinition
        {
            Key = "job-a",
            CronExpression = "0 7 * * *",
            IsEnabled = true,
            Description = "Job A"
        });
        db.Set<ScheduledJobRun>().Add(new ScheduledJobRun
        {
            Key = "job-a",
            StartedAt = new DateTime(2026, 4, 10, 7, 0, 0, DateTimeKind.Utc),
            FinishedAt = new DateTime(2026, 4, 10, 7, 0, 5, DateTimeKind.Utc),
            Status = ScheduledJobRunStatus.Succeeded
        });
        await db.SaveChangesAsync();

        var handler = new GetScheduledJobStatusQueryHandler(db);
        var result = await handler.Handle(new GetScheduledJobStatusQuery(), CancellationToken.None);

        var job = result.Single();
        job.LastRunAt.Should().NotBeNull();
        job.LastRunStatus.Should().Be("Succeeded");
    }

    [Fact]
    public async Task Handle_DisabledJobHasNullNextRun()
    {
        await using var db = CreateDb();
        db.Set<ScheduledJobDefinition>().Add(new ScheduledJobDefinition
        {
            Key = "disabled-job",
            CronExpression = "0 7 * * *",
            IsEnabled = false,
            Description = "Disabled"
        });
        await db.SaveChangesAsync();

        var handler = new GetScheduledJobStatusQueryHandler(db);
        var result = await handler.Handle(new GetScheduledJobStatusQuery(), CancellationToken.None);

        result.Single().NextRunAt.Should().BeNull();
    }

    private static ApplicationDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
