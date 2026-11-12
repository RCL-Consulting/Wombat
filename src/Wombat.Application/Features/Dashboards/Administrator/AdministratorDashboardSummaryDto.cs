namespace Wombat.Application.Features.Dashboards.Administrator;

/// <summary>
/// The Administrator's Home (T400, flow 18, K11, T9): System health's three lines and the Accounts card, each part read
/// on its own so a database that does not answer leaves the others. A part that could not be read is null.
/// </summary>
/// <param name="ReadAt">The moment Home was read, UTC (the handler's <c>TimeProvider</c>).</param>
/// <param name="DatabaseFailedAt">
/// When the database did not answer Home's probe, UTC; null when it answered (the Database line reads OK).
/// </param>
/// <param name="Accounts">The accounts card; null when it could not be read.</param>
/// <param name="Mail">The Mail line: always present, since whether a mail server is set is configuration.</param>
/// <param name="Jobs">The Scheduled jobs line; null when it could not be read.</param>
public sealed record AdministratorDashboardSummaryDto(
    DateTime ReadAt,
    DateTime? DatabaseFailedAt,
    AdministratorAccountsDto? Accounts,
    AdministratorMailDto Mail,
    AdministratorJobsDto? Jobs);

/// <summary>Every account but an erased one, and how many of them cannot sign in now (K2, Q4, T14).</summary>
public sealed record AdministratorAccountsDto(int Accounts, int Locked);

/// <summary>The Mail line (Q2, Q13, Q18).</summary>
/// <param name="IsConfigured">A mail server is set (<c>Email:SmtpHost</c>); false: mail is written to the log.</param>
/// <param name="QueueDepth">The mails waiting in this host's queue now.</param>
/// <param name="Outcomes">
/// What the outcomes table says; null when mail is not configured (it is not read) or when the table could not be read.
/// </param>
public sealed record AdministratorMailDto(bool IsConfigured, int QueueDepth, AdministratorMailOutcomesDto? Outcomes);

/// <summary>The mail outcomes Home reads (Q18): the drops of the last 24 hours, the last drop and the last mail sent.</summary>
/// <param name="WindowStart">The oldest moment the table holds by its retention, now less 90 days, UTC: "Nothing sent since …".</param>
/// <param name="DroppedLast24Hours">The mails dropped in the 24 hours before Home was read.</param>
/// <param name="LastDropAt">The last of those drops, UTC; null when none was dropped in the 24 hours.</param>
/// <param name="LastDropRefusal">That drop's refusal as stored (redacted); null when it had none (K3).</param>
/// <param name="LastSentAt">The last mail sent since <paramref name="WindowStart" />, UTC; null when none was.</param>
public sealed record AdministratorMailOutcomesDto(
    DateTime WindowStart,
    int DroppedLast24Hours,
    DateTime? LastDropAt,
    string? LastDropRefusal,
    DateTime? LastSentAt);

/// <summary>The Scheduled jobs line (Q3, K4): every job, by key.</summary>
public sealed record AdministratorJobsDto(IReadOnlyList<AdministratorJobHealthDto> Jobs);

/// <summary>One job as Home judges it (Q3, K4, the host's rule through <c>JobSchedule</c>).</summary>
/// <param name="Key">The job's key.</param>
/// <param name="Cron">Its stored cron expression, as stored (UTC).</param>
/// <param name="IsEnabled">The scheduler runs it.</param>
/// <param name="LastRunStatus">The last run's status ("Running", "Succeeded", "Failed"); null when it has never run.</param>
/// <param name="LastRunStartedAt">The last run's start, UTC.</param>
/// <param name="LastRunFinishedAt">The last run's finish, UTC; null while it runs.</param>
/// <param name="LastErrorFirstLine">The first line of the last run's error, when it failed with one.</param>
/// <param name="NextOccurrence">
/// The host's next occurrence for it (<c>JobSchedule.NextAfter</c>: after the last run's start, or after a day ago for a
/// job never run), UTC; null when the cron cannot be read.
/// </param>
/// <param name="State">What the line says of it.</param>
public sealed record AdministratorJobHealthDto(
    string Key,
    string Cron,
    bool IsEnabled,
    string? LastRunStatus,
    DateTime? LastRunStartedAt,
    DateTime? LastRunFinishedAt,
    string? LastErrorFirstLine,
    DateTime? NextOccurrence,
    AdministratorJobState State);

/// <summary>What Home's jobs line says of one job (Q3, K4). The badge takes the worst: Failing, then Overdue, then Disabled.</summary>
public enum AdministratorJobState
{
    /// <summary>Its last run succeeded, and it is not overdue.</summary>
    Succeeded,

    /// <summary>
    /// Its last run is still running: neither failing nor overdue, since it holds the job's lock and the scheduler waits
    /// on it (K4).
    /// </summary>
    Running,

    /// <summary>Enabled and never run, its first run not yet an hour past: "has not run yet: its first run is …".</summary>
    NeverRun,

    /// <summary>Its last run failed (badge Failing).</summary>
    Failed,

    /// <summary>Its stored cron does not parse, so the scheduler never runs it (badge Failing; K4).</summary>
    Unreadable,

    /// <summary>Enabled, and more than an hour past the time the scheduler should have started it (Q3, the host's rule).</summary>
    Overdue,

    /// <summary>Disabled: the scheduler does not run it.</summary>
    Disabled
}
