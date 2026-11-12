using Wombat.Application.Features.Dashboards.Administrator;

namespace Wombat.Web.Tests.Dashboards;

/// <summary>The Administrator's Home as its query answers it (T400, flow 18, lane A), built by hand for the page tests.</summary>
internal static class AdministratorHomeFixtures
{
    /// <summary>2026-11-10 21:39 SAST.</summary>
    public static readonly DateTime Now = new(2026, 11, 10, 19, 39, 0, DateTimeKind.Utc);

    public static AdministratorDashboardSummaryDto Healthy(int accounts = 24, int locked = 0)
        => new(Now, null, new AdministratorAccountsDto(accounts, locked), new AdministratorMailDto(false, 0, null),
            new AdministratorJobsDto(EightSucceeded()));

    /// <summary>R3-H05: one job failed, so "Its runs" is offered beside Scheduled jobs.</summary>
    public static AdministratorDashboardSummaryDto WithAFailedJob()
        => Healthy() with
        {
            Jobs = new AdministratorJobsDto(EightSucceeded()
                .Select(job => job.Key == "entrustment-decision-expiry"
                    ? job with
                    {
                        State = AdministratorJobState.Failed,
                        LastRunStatus = "Failed",
                        LastErrorFirstLine = "Npgsql.PostgresException: 57P01 terminating connection"
                    }
                    : job)
                .ToList())
        };

    public static readonly string[] Keys =
    [
        "activity-draft-nudge", "assessor-pending-nudge", "audit-log-retention", "entrustment-decision-expiry",
        "msf-campaign-auto-close", "msf-invitation-expiry-reminder", "scheduled-job-run-retention", "weekly-coordinator-digest"
    ];

    public static List<AdministratorJobHealthDto> EightSucceeded()
        => Keys.Select(key => Job(key, AdministratorJobState.Succeeded)).ToList();

    public static AdministratorJobHealthDto Job(string key, AdministratorJobState state, DateTime? next = null)
        => new(key, "0 7 * * *", state != AdministratorJobState.Disabled,
            state == AdministratorJobState.NeverRun ? null : "Succeeded",
            state == AdministratorJobState.NeverRun ? null : new DateTime(2026, 11, 10, 5, 0, 0, DateTimeKind.Utc),
            state == AdministratorJobState.NeverRun ? null : new DateTime(2026, 11, 10, 5, 0, 2, DateTimeKind.Utc),
            null, next ?? new DateTime(2026, 11, 11, 7, 0, 0, DateTimeKind.Utc), state);
}
