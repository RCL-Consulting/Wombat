using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Scheduling.Commands.DisableScheduledJob;
using Wombat.Application.Features.Scheduling.Commands.EnableScheduledJob;
using Wombat.Domain.Scheduling;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Scheduling;

public sealed class EnableDisableScheduledJobTests
{
    [Fact]
    public async Task DisableCommand_SetsIsEnabledToFalse()
    {
        await using var db = CreateDb();
        db.Set<ScheduledJobDefinition>().Add(new ScheduledJobDefinition
        {
            Key = "test-job",
            CronExpression = "0 * * * *",
            IsEnabled = true,
            Description = "Test"
        });
        await db.SaveChangesAsync();

        var handler = new DisableScheduledJobCommandHandler(db);
        await handler.Handle(new DisableScheduledJobCommand("test-job"), CancellationToken.None);

        var definition = await db.Set<ScheduledJobDefinition>().SingleAsync(d => d.Key == "test-job");
        definition.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task EnableCommand_SetsIsEnabledToTrue()
    {
        await using var db = CreateDb();
        db.Set<ScheduledJobDefinition>().Add(new ScheduledJobDefinition
        {
            Key = "test-job",
            CronExpression = "0 * * * *",
            IsEnabled = false,
            Description = "Test"
        });
        await db.SaveChangesAsync();

        var handler = new EnableScheduledJobCommandHandler(db);
        await handler.Handle(new EnableScheduledJobCommand("test-job"), CancellationToken.None);

        var definition = await db.Set<ScheduledJobDefinition>().SingleAsync(d => d.Key == "test-job");
        definition.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task DisableCommand_ThrowsForUnknownKey()
    {
        await using var db = CreateDb();
        var handler = new DisableScheduledJobCommandHandler(db);

        var act = () => handler.Handle(new DisableScheduledJobCommand("nope"), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task EnableCommand_ThrowsForUnknownKey()
    {
        await using var db = CreateDb();
        var handler = new EnableScheduledJobCommandHandler(db);

        var act = () => handler.Handle(new EnableScheduledJobCommand("nope"), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private static ApplicationDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
