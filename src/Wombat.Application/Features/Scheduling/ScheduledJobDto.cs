namespace Wombat.Application.Features.Scheduling;

/// <summary>
/// One scheduled job as Scheduled jobs shows it (T400, flow 18; R3-J01–J10). Every moment is UTC; the page says it in
/// South African time.
/// </summary>
/// <param name="LastRunAt">When the last run started; null for a job never run ("Not run yet").</param>
/// <param name="LastRunStatus">The last run's status by name (Running, Succeeded, Failed); null for a job never run.</param>
/// <param name="LastRunFinishedAt">When the last run ended; null while it runs, or for a job never run.</param>
/// <param name="LastRunError">The first line of a failed last run's error as stored (R3-J08); null otherwise.</param>
/// <param name="LastRunBy">Who ran the last run by hand, by name (an erased person by their pseudonym); null for the scheduler.</param>
/// <param name="NextRunAt">
/// When the scheduler runs the job next, by the host's rule (<c>JobSchedule.NextAfter</c>, K5); null while disabled, or
/// when the schedule cannot be read.
/// </param>
/// <param name="IsDue">
/// The next run is not after now, so the scheduler's next check, within 30 seconds, runs it (K5): "At the next check,
/// within 30 seconds". False while disabled.
/// </param>
/// <param name="ScheduleReadable">Whether the stored cron parses; one that does not is never run by the scheduler (K4).</param>
public sealed record ScheduledJobDto(
    int Id,
    string Key,
    string CronExpression,
    bool IsEnabled,
    string Description,
    DateTime? LastRunAt,
    string? LastRunStatus,
    DateTime? LastRunFinishedAt,
    string? LastRunError,
    string? LastRunBy,
    DateTime? NextRunAt,
    bool IsDue,
    bool ScheduleReadable = true);
