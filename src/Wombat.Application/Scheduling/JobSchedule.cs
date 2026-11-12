using Cronos;

namespace Wombat.Application.Scheduling;

/// <summary>
/// When a scheduled job runs next, whether it is due, and whether it is overdue (T400, flow 18, K4, K5): the one rule the
/// scheduler (<c>ScheduledJobHost.IsDue</c>), Home's jobs line and the Scheduled jobs page all judge by, so the page never
/// names a time the host would not keep.
/// </summary>
/// <remarks>
/// <para>
/// The host's rule: a job's next run is the cron's next occurrence after its last run's start, or, for a job never run,
/// after one day before now. A job is due when that occurrence is not after now; the host's tick (every 30 seconds)
/// then runs it. So a job disabled past its time, then enabled, is due at once: its next occurrence after its last run
/// has already passed (K5).
/// </para>
/// <para>
/// Every moment is UTC (<see cref="DateTimeKind.Utc" />), as Cronos requires and as the run records hold them.
/// </para>
/// </remarks>
public static class JobSchedule
{
    /// <summary>
    /// How long past its next occurrence a job that has not run reads as overdue on Home (Q3): an hour, so a tick that is
    /// late by its 30 seconds, or a run still being written, is never reported.
    /// </summary>
    public static readonly TimeSpan OverdueGrace = TimeSpan.FromHours(1);

    /// <summary>The cron as the host reads it (five fields, UTC); false, with no throw and no log, when it does not parse.</summary>
    public static bool TryParse(string cron, out CronExpression? expression)
    {
        expression = null;
        if (string.IsNullOrWhiteSpace(cron))
        {
            return false;
        }

        if (CronExpression.TryParse(cron, out var parsed))
        {
            expression = parsed;
            return true;
        }

        return false;
    }

    /// <summary>
    /// The occurrence the host runs the job at next: the cron's next occurrence after <paramref name="lastRunStartUtc" />,
    /// or after <paramref name="nowUtc" /> less one day for a job never run (<c>ScheduledJobHost</c>'s rule, unchanged).
    /// Null when the cron has no later occurrence. A moment read back with no kind (<see cref="DateTimeKind.Unspecified" />)
    /// is taken as the UTC it is stored as, here, so every caller has the one guard Cronos needs (the build review's N-2).
    /// </summary>
    public static DateTime? NextAfter(CronExpression cron, DateTime? lastRunStartUtc, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(cron);
        var baseTime = AsUtc(lastRunStartUtc ?? nowUtc.AddDays(-1));
        return cron.GetNextOccurrence(baseTime, inclusive: false);
    }

    private static DateTime AsUtc(DateTime moment)
        => moment.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(moment, DateTimeKind.Utc) : moment;

    /// <summary>Whether the host's next tick runs the job: its next occurrence (<see cref="NextAfter" />) is not after now.</summary>
    public static bool IsDue(CronExpression cron, DateTime? lastRunStartUtc, DateTime nowUtc)
        => NextAfter(cron, lastRunStartUtc, nowUtc) is { } next && next <= nowUtc;

    /// <summary>
    /// Whether the job has missed its time by more than <see cref="OverdueGrace" />. Never while its last run is still
    /// running: that run holds the job's lock, so the host cannot start it again until it ends (K4).
    /// </summary>
    public static bool IsOverdue(CronExpression cron, DateTime? lastRunStartUtc, bool lastRunRunning, DateTime nowUtc)
    {
        if (lastRunRunning)
        {
            return false;
        }

        return NextAfter(cron, lastRunStartUtc, nowUtc) is { } next && next + OverdueGrace < nowUtc;
    }
}
