namespace Wombat.Application.Scheduling;

/// <summary>
/// "Run now" refused, in words for the person who pressed it (T400, flow 18, K6, T22): the job is already running, or it
/// is disabled. Thrown by the dispatcher under the job's lock, before a run row is written, so nothing ran and nothing
/// was stored; the jobs page shows the sentence in a warning, and anything else that fails is its fixed failure words.
/// </summary>
public sealed class ScheduledJobRefusal : InvalidOperationException
{
    private ScheduledJobRefusal(string jobKey, string message)
        : base(message)
    {
        JobKey = jobKey;
    }

    /// <summary>The job the refusal is about.</summary>
    public string JobKey { get; }

    /// <summary>A run of the job is going (the scheduler's, or another person's "Run now"). (T206 review)</summary>
    public static ScheduledJobRefusal AlreadyRunning(string jobKey) => new(jobKey, AlreadyRunningSentence(jobKey));

    /// <summary>The job is disabled: a page read before someone disabled it offered Run now (T22, R3-J09).</summary>
    public static ScheduledJobRefusal Disabled(string jobKey) => new(jobKey, DisabledSentence(jobKey));

    /// <summary>"Job '‹key›' is already running, so it was not started again. …" (built, moved from the dispatcher word for word).</summary>
    public static string AlreadyRunningSentence(string jobKey)
        => $"Job '{jobKey}' is already running, so it was not started again. Wait for that run to finish, then run it " +
           "again if it is still needed.";

    /// <summary>"Job '‹key›' is disabled, so it was not run. Enable it, then run it." (T22)</summary>
    public static string DisabledSentence(string jobKey)
        => $"Job '{jobKey}' is disabled, so it was not run. Enable it, then run it.";
}
