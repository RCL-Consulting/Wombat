using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Interfaces;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling.Jobs;

namespace Wombat.Application.Tests.Scheduling;

/// <summary>
/// The run-record retention job (T400, flow 18, lane B; Q18, K18): it deletes job runs and mail outcomes more than 90
/// days old, so Run history's "kept for 90 days." and Home's mail line ("since ‹time›", the window's start) hold, and its
/// description says both. The deletes are set-based (<c>ExecuteDeleteAsync</c>, the build review's N-7), which the in-memory
/// provider cannot run, so what they delete is pinned on PostgreSQL (<c>JobRunsPostgresTests</c>).
/// </summary>
public sealed class ScheduledJobRunRetentionTests
{
    [Fact]
    public void ItsDescription_SaysBoth_WithNoTimeOfDay()
    {
        new ScheduledJobRunRetentionJob(BuildProvider().GetRequiredService<IServiceScopeFactory>()).Description
            .Should().Be("Deletes job run records and mail outcomes older than 90 days.");
    }

    private static ServiceProvider BuildProvider()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(dbName));
        services.AddScoped<IApplicationDbContext>(p => p.GetRequiredService<ApplicationDbContext>());
        return services.BuildServiceProvider();
    }
}
