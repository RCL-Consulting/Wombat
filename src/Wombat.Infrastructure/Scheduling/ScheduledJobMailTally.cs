using System.Collections.Concurrent;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email;
using Wombat.Application.Scheduling;

namespace Wombat.Infrastructure.Scheduling;

/// <summary>
/// Counts what became of the mail a scheduled job's run handed over (the two nudges and the weekly coordinator digest),
/// and logs one line per run once the mail worker has reported on every mail: how many were sent and how many were not
/// delivered. (T283)
/// </summary>
/// <remarks>
/// <para>
/// A job's own line is written as its run ends, when its mail has only been handed to the queue (T251), so it can say
/// whom it wrote to but not whether anyone received it. Until T283 a nudge or digest the worker gave up on was dropped
/// with a line of its own and nothing else. These are periodic reminders, sent again on the next run, so a count of the
/// failures in the log is all they need: nothing is recorded per recipient, and no page shows it. The line goes to the
/// logger the job's own line went to (<see cref="ScheduledJobContext.Logger" />), and names the job and when its run
/// started, so the two read together.
/// </para>
/// <para>
/// In memory: the queue is in the process too, so what it holds is lost with it. A run whose mail is not all reported
/// (the host has no mail worker, or stopped with a mail in flight) is logged with how many were not reported before the
/// same job next ran, or with how many were never reported as the host stops (<see cref="Dispose" />). So at most one
/// run a job is held, and a host that never reports holds one per job. The first says "before the job ran again", not
/// "never": a "Run now" while the last run's mail is still being retried lets that run go with its mail still on its way,
/// and a report that comes after is counted nowhere (T283 review).
/// </para>
/// </remarks>
public sealed class ScheduledJobMailTally : IEmailDeliveryObserver, IDisposable
{
    private const string DeliveryKeyPrefix = "job-mail:";

    /// <summary>The line once every mail the run handed over has been reported on.</summary>
    private const string ReportedTemplate =
        "{JobName} mail of the run started {RunStartedAt}: sent {SentCount}, not delivered {NotDeliveredCount}.";

    /// <summary>
    /// The line for a run some of whose mail was not reported when the same job ran again: none may come, or a mail may
    /// still be being retried. (T283 review)
    /// </summary>
    private const string SupersededTemplate =
        "{JobName} mail of the run started {RunStartedAt}: sent {SentCount}, not delivered {NotDeliveredCount}, not " +
        "reported before the job ran again {UnreportedCount}.";

    /// <summary>The line for a run some of whose mail was never reported: the host stopped.</summary>
    private const string UnreportedTemplate =
        "{JobName} mail of the run started {RunStartedAt}: sent {SentCount}, not delivered {NotDeliveredCount}, never " +
        "reported {UnreportedCount}.";

    private readonly ConcurrentDictionary<string, Run> _runs = new(StringComparer.Ordinal);

    /// <summary>
    /// Opens the count for a run of <paramref name="jobName" />. An earlier run of the same job still waiting on reports is
    /// logged first, with how many had not come.
    /// </summary>
    public ScheduledJobMailRun Start(string jobName, ScheduledJobContext context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobName);
        ArgumentNullException.ThrowIfNull(context);

        foreach (var earlier in _runs.Values.Where(run => run.JobName == jobName).ToList())
        {
            Abandon(earlier, jobRanAgain: true);
        }

        var started = new Run(Guid.NewGuid().ToString("N"), jobName, context.UtcNow, context.Logger);
        _runs[started.Id] = started;
        return new ScheduledJobMailRun(this, started);
    }

    public Task RecordAsync(EmailMessage message, EmailDeliveryOutcome outcome, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(outcome);

        if (message.DeliveryKey is not { } key ||
            !key.StartsWith(DeliveryKeyPrefix, StringComparison.Ordinal) ||
            !_runs.TryGetValue(key[DeliveryKeyPrefix.Length..], out var run))
        {
            return Task.CompletedTask;
        }

        if (outcome.Sent)
        {
            Interlocked.Increment(ref run.Sent);
        }
        else
        {
            Interlocked.Increment(ref run.NotDelivered);
        }

        TryComplete(run);
        return Task.CompletedTask;
    }

    /// <summary>Logs every run still waiting on reports, with how many never came: the host is stopping.</summary>
    public void Dispose()
    {
        foreach (var run in _runs.Values.ToList())
        {
            Abandon(run, jobRanAgain: false);
        }
    }

    internal static string DeliveryKeyOf(Run run) => DeliveryKeyPrefix + run.Id;

    internal void HandedOver(Run run) => Interlocked.Increment(ref run.HandedOver);

    /// <remarks>
    /// The close is a full fence, as each report's count is (<c>Interlocked.Increment</c>), so whichever of the last report
    /// and the close comes second sees the other. With a plain volatile write here, the close's read of the counts could
    /// pass its own write, and a report on another thread miss it: neither would log the line, and the run would be
    /// logged later as unreported. (T283 review)
    /// </remarks>
    internal void Close(Run run)
    {
        Interlocked.Exchange(ref run.Closed, 1);
        TryComplete(run);
    }

    /// <summary>
    /// Logs the run's line once it has handed over all its mail and every mail has been reported on. A run that handed
    /// over none logs nothing: the job's own line says so. Whichever of the last report and the close comes second logs
    /// it, once.
    /// </summary>
    private void TryComplete(Run run)
    {
        if (Volatile.Read(ref run.Closed) == 0)
        {
            return;
        }

        var handedOver = Volatile.Read(ref run.HandedOver);
        var sent = Volatile.Read(ref run.Sent);
        var notDelivered = Volatile.Read(ref run.NotDelivered);
        if (sent + notDelivered < handedOver || !_runs.TryRemove(new KeyValuePair<string, Run>(run.Id, run)))
        {
            return;
        }

        if (handedOver == 0)
        {
            return;
        }

        var startedAt = run.StartedAt.ToString("u", CultureInfo.InvariantCulture);
        if (notDelivered > 0)
        {
            run.Logger.LogWarning(ReportedTemplate, run.JobName, startedAt, sent, notDelivered);
        }
        else
        {
            run.Logger.LogInformation(ReportedTemplate, run.JobName, startedAt, sent, notDelivered);
        }
    }

    /// <summary>
    /// Lets a run go that is still waiting on reports, and logs how many had not come: "before the job ran again" when
    /// the same job started another run, "never" as the host stops. Two calls, each with its own constant template, so a
    /// reader (and <c>NoAddressInLogsTests</c>) can see what each logs.
    /// </summary>
    private void Abandon(Run run, bool jobRanAgain)
    {
        if (!_runs.TryRemove(new KeyValuePair<string, Run>(run.Id, run)))
        {
            return;
        }

        var handedOver = Volatile.Read(ref run.HandedOver);
        var sent = Volatile.Read(ref run.Sent);
        var notDelivered = Volatile.Read(ref run.NotDelivered);
        if (handedOver == 0)
        {
            return;
        }

        try
        {
            var startedAt = run.StartedAt.ToString("u", CultureInfo.InvariantCulture);
            var unreported = Math.Max(0, handedOver - sent - notDelivered);
            if (jobRanAgain)
            {
                run.Logger.LogWarning(SupersededTemplate, run.JobName, startedAt, sent, notDelivered, unreported);
            }
            else
            {
                run.Logger.LogWarning(UnreportedTemplate, run.JobName, startedAt, sent, notDelivered, unreported);
            }
        }
        catch (ObjectDisposedException)
        {
            // The host is stopping and its logging has gone first; there is nowhere left to say it.
        }
    }

    /// <summary>One run's count. Fields, so that the worker's reports and the job's hand-offs count without a lock.</summary>
    internal sealed class Run(string id, string jobName, DateTime startedAt, ILogger logger)
    {
        public string Id { get; } = id;
        public string JobName { get; } = jobName;
        public DateTime StartedAt { get; } = startedAt;
        public ILogger Logger { get; } = logger;

        public int HandedOver;
        public int Sent;
        public int NotDelivered;

        /// <summary>1 once the run has handed over all it will; an int, so that the close can be a full fence.</summary>
        public int Closed;
    }
}

/// <summary>
/// A scheduled job's run, as <see cref="ScheduledJobMailTally" /> counts its mail: each mail is keyed before it is handed
/// over, counted once it has been, and the run is closed once the job has handed over all it will. (T283)
/// </summary>
public sealed class ScheduledJobMailRun
{
    private readonly ScheduledJobMailTally _tally;
    private readonly ScheduledJobMailTally.Run _run;

    internal ScheduledJobMailRun(ScheduledJobMailTally tally, ScheduledJobMailTally.Run run)
    {
        _tally = tally;
        _run = run;
    }

    /// <summary>
    /// <paramref name="message" />, carrying this run's key, which the mail worker hands back with its outcome. Never a
    /// tag, and never logged.
    /// </summary>
    public EmailMessage Keyed(EmailMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return message with { DeliveryKey = ScheduledJobMailTally.DeliveryKeyOf(_run) };
    }

    /// <summary>Counts a mail the queue has taken. Called after the hand-off, so one that failed is not waited for.</summary>
    public void HandedOver() => _tally.HandedOver(_run);

    /// <summary>
    /// The run has handed over all it will: its line is logged once the last of its mail has been reported on. Called in a
    /// <c>finally</c>, so a run that failed part-way still counts what it sent.
    /// </summary>
    public void Close() => _tally.Close(_run);
}
