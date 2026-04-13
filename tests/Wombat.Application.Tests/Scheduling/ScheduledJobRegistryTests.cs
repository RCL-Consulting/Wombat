using FluentAssertions;
using Wombat.Application.Scheduling;
using Wombat.Infrastructure.Scheduling;

namespace Wombat.Application.Tests.Scheduling;

public sealed class ScheduledJobRegistryTests
{
    [Fact]
    public void Register_AddsJobToRegistry()
    {
        var registry = new ScheduledJobRegistry();
        var job = new FakeJob("test-key");

        registry.Register(job);

        registry.Jobs.Should().ContainSingle();
        registry.GetByKey("test-key").Should().BeSameAs(job);
    }

    [Fact]
    public void Register_DuplicateKey_Throws()
    {
        var registry = new ScheduledJobRegistry();
        registry.Register(new FakeJob("dup-key"));

        var act = () => registry.Register(new FakeJob("dup-key"));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetByKey_ReturnsNullForUnknownKey()
    {
        var registry = new ScheduledJobRegistry();
        registry.GetByKey("nope").Should().BeNull();
    }

    private sealed class FakeJob(string key) : IScheduledJob
    {
        public string Key => key;
        public string CronExpression => "0 * * * *";
        public string Description => "Fake";
        public Task ExecuteAsync(ScheduledJobContext context, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
