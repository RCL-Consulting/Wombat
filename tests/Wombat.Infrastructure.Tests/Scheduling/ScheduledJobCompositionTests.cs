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

    /// <summary>
    /// T400 (flow 18, T15, Q18; R3-0-spec-2 § 9): each job says what it does, word for word as Scheduled jobs shows it, with
    /// no time of day and no "UTC" (the Schedule column says when, in South African time) and no property's name
    /// ("ExpiresOn", "ScheduledJobRun"). The host brings a stored description up to the job's at every start.
    /// </summary>
    [Fact]
    public void EachJobsDescription_IsTheBoardsWords_WithNoTimeOfDay()
    {
        using var provider = Host();

        var descriptions = provider.GetRequiredService<IScheduledJobRegistry>().Jobs
            .ToDictionary(job => job.Key, job => job.Description);

        descriptions.Should().Equal(new Dictionary<string, string>
        {
            ["activity-draft-nudge"] = "Reminds trainees about draft activities older than 14 days.",
            ["assessor-pending-nudge"] = "Nudges assessors about activities waiting for their assessment for more than 5 days.",
            ["audit-log-retention"] = "Archives audit log entries older than 2 years into the archive table.",
            ["entrustment-decision-expiry"] = "Expires entrustment decisions past their expiry date and sends 30-day expiry reminders.",
            ["msf-campaign-auto-close"] = "Closes open MSF campaigns whose window has ended and anonymises their respondents.",
            ["msf-invitation-expiry-reminder"] =
                "Reminds MSF respondents who have not responded, once, two days before their last day to respond.",
            ["scheduled-job-run-retention"] = "Deletes job run records and mail outcomes older than 90 days.",
            ["weekly-coordinator-digest"] = "Sends weekly digest emails to Coordinators."
        });
        descriptions.Values.Should().NotContain(description => description.Contains("UTC") || description.Contains("daily at"));
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
