namespace Wombat.Application.Scheduling;

public interface IScheduledJobDispatcher
{
    /// <summary>
    /// Runs the job now and answers how the run ended (T400, K6). Refuses with <see cref="ScheduledJobRefusal" />, before
    /// any run row is written, a job already running or one whose stored definition is disabled, both judged under the
    /// job's lock.
    /// </summary>
    Task<ScheduledJobRunOutcome> DispatchNowAsync(IScheduledJob job, string? triggeredByUserId, CancellationToken cancellationToken);
}
