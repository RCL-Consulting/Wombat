using System.Collections.Concurrent;

namespace Wombat.Infrastructure.Scheduling;

/// <summary>
/// One lock per scheduled job, shared by the scheduler (<see cref="ScheduledJobHost" />) and an administrator's "Run now"
/// (<see cref="ScheduledJobDispatcher" />), so that a job never runs twice at once. (T206 review)
/// </summary>
/// <remarks>
/// <para>
/// Until the T206 review the host kept these locks to itself, and "Run now" took none. A run started by hand while the
/// scheduled one was going read the same rows and acted on them a second time. For the MSF reminder that meant two
/// reminders to one respondent, the first carrying a link the second had already retired.
/// </para>
/// <para>
/// In-process: only <c>Wombat.Web</c> runs the scheduler in a deployment (<c>deploy.ps1</c> publishes nothing else), and
/// a job's own saves are what hold it right across processes.
/// </para>
/// </remarks>
public sealed class ScheduledJobLocks
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.Ordinal);

    /// <summary>
    /// Takes the job's lock, or returns null when a run of the job is already in progress. Disposing what it returns
    /// releases the lock.
    /// </summary>
    public IDisposable? TryEnter(string jobKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobKey);

        var semaphore = _locks.GetOrAdd(jobKey, _ => new SemaphoreSlim(1, 1));
        return semaphore.Wait(0) ? new Held(semaphore) : null;
    }

    private sealed class Held(SemaphoreSlim semaphore) : IDisposable
    {
        private int _released;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _released, 1) == 0)
            {
                semaphore.Release();
            }
        }
    }
}
