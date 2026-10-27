using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Scheduling;

namespace Wombat.Infrastructure.Tests.Scheduling;

/// <summary>
/// T379 (flow 13, T313 with T287's verify item; Q16): the host schedules eight jobs, and none of them deletes the export
/// records Verify reads. Until T379 <c>portfolio-export-cleanup</c> deleted every export older than 90 days, so a genuine
/// portfolio read "No matching export found" on its 91st day. The job is gone, and the T379 migration removes its stored
/// definition and runs; an export row lives as long as its trainee's record (erasure pseudonymises it, nothing deletes it).
/// </summary>
public sealed class ScheduledJobCompositionTests
{
    [Fact]
    public void TheHost_SchedulesEightJobs_AndNoneRetiresAnExport()
    {
        using var provider = Host();

        var keys = provider.GetRequiredService<IScheduledJobRegistry>().Jobs.Select(job => job.Key).ToList();

        keys.Should().BeEquivalentTo(
            "activity-draft-nudge",
            "assessor-pending-nudge",
            "msf-campaign-auto-close",
            "msf-invitation-expiry-reminder",
            "weekly-coordinator-digest",
            "audit-log-retention",
            "scheduled-job-run-retention",
            "entrustment-decision-expiry");
        keys.Should().NotContain("portfolio-export-cleanup", "Verify answers for a file as long as its record is kept (T313)");
        typeof(DependencyInjection).Assembly.GetTypes().Should().NotContain(
            type => type.Name == "PortfolioExportCleanupJob", "the job is removed, not only unregistered");
    }

    /// <summary>The host's composition. No connection is opened: nothing here queries.</summary>
    private static ServiceProvider Host()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=t379_never_opened"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);
        return services.BuildServiceProvider();
    }
}
