using System.Globalization;
using Wombat.Application.Features.Scheduling;
using Wombat.Application.Scheduling;
using Wombat.Domain.Scheduling;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Components.Pages.Admin.Jobs;

/// <summary>
/// The words of Scheduled jobs and Run history (T400, flow 18, lane B; R3-0-spec-2 § 9): their subtitles, loading and
/// load errors, the jobs' results, refusals and failures, the runs' count line and empty states, every time in South
/// African time (<see cref="SastTime" />), and a run's status by its word, never an enum's name.
/// </summary>
public static class JobWords
{
    // ---- Scheduled jobs ----

    /// <summary>T15.</summary>
    public const string JobsSubtitle =
        "The eight jobs Wombat runs on a schedule. Run one now, disable or enable one, or read every run in Run history.";

    public const string JobsLoading = "Loading the scheduled jobs.";

    public const string JobsLoadFailed =
        "Could not load the scheduled jobs. Nothing has changed. Try again, or come back in a few minutes.";

    public const string JobsCaption = "Scheduled jobs";

    public const string RunHistory = "Run history";

    public const string NotRunYet = "Not run yet";

    public const string NoneWhileDisabled = "None while disabled";

    /// <summary>A job already due: the scheduler's next check, every 30 seconds, runs it (K5).</summary>
    public const string AtTheNextCheck = "At the next check, within 30 seconds";

    /// <summary>Next run of a job whose stored cron does not parse, which the scheduler never runs (K4). On no board.</summary>
    public const string ScheduleUnreadable = "None: the schedule cannot be read";

    /// <summary>Why Run now does nothing on a disabled job (R3-J02).</summary>
    public const string EnableToRun = "Enable it to run it.";

    public const string RunNow = "Run now";

    /// <summary>Run now's words while its run goes (T16, R3-J05).</summary>
    public const string Running = "Running…";

    public const string Disable = "Disable";

    public const string Enable = "Enable";

    /// <summary>The result of Disable (T5, R3-J02).</summary>
    public static string Disabled(string key) => $"Disabled {key}. The scheduler will not run it until you enable it.";

    /// <summary>
    /// The result of Enable, read from the job as read again (T5, K5, R3-J03): its next run, or, for a job due at once,
    /// the next check.
    /// </summary>
    public static string Enabled(string key, ScheduledJobDto? job)
        => job switch
        {
            { IsDue: true } => $"Enabled {key}. It runs at the next check, within 30 seconds.",
            { NextRunAt: { } next } => $"Enabled {key}. Its next run is {SastTime.Minute(next)}.",
            _ => $"Enabled {key}."
        };

    /// <summary>
    /// The result of Run now, once the run has ended (T5, Q10, R3-J04, J07): "Ran ‹key›: Succeeded at ‹time› SAST.", or
    /// "Ran ‹key›: Failed at ‹time› SAST: ‹the error as stored›."
    /// </summary>
    public static string Ran(string key, ScheduledJobRunOutcome outcome)
    {
        var at = SastTime.Minute(outcome.FinishedAt ?? outcome.StartedAt);
        var lead = $"Ran {key}: {Status(outcome.Status.ToString())} at {at}";
        return Failed(outcome) && !string.IsNullOrWhiteSpace(outcome.ErrorMessage)
            ? $"{lead}: {Sentence(outcome.ErrorMessage.Trim())}"
            : $"{lead}.";
    }

    /// <summary>Whether Run now's run failed, so its result is a danger alert with "Its runs" (R3-J07).</summary>
    public static bool Failed(ScheduledJobRunOutcome outcome) => outcome.Status == ScheduledJobRunStatus.Failed;

    /// <summary>"Its runs", with its visually hidden ": ‹key›, failed" (R3-J07, Home's H05).</summary>
    public const string ItsRuns = "Its runs";

    public static string ItsRunsHidden(string key) => $": {key}, failed";

    /// <summary>Run history opened on the job's failed runs (T18).</summary>
    public static string FailedRunsOf(string key) => JobLinks.RunsOf(key, nameof(ScheduledJobRunStatus.Failed));

    /// <summary>The failure's first sentence, bolded (T23, R3-J10): "‹key› could not be disabled."</summary>
    public static string CouldNotDisable(string key) => $"{key} could not be disabled.";

    public static string CouldNotEnable(string key) => $"{key} could not be enabled.";

    public static string CouldNotStart(string key) => $"{key} could not be started.";

    /// <summary>What follows a failure's first sentence (T23).</summary>
    public const string NothingHasChanged = "Nothing has changed. Try again, or come back in a few minutes.";

    // ---- Run history ----

    /// <summary>T15, kept (K18).</summary>
    public const string RunsSubtitle = "Every run of the eight jobs, newest first, kept for 90 days.";

    public const string RunsLoading = "Loading the run history.";

    public const string RunsLoadFailed =
        "Could not load the run history. Nothing has changed. Try again, or come back in a few minutes.";

    public const string FilterName = "Filter the runs";

    public const string JobLabel = "Job";

    public const string AllJobs = "All jobs";

    public const string StatusLabel = "Status";

    public const string AllStatuses = "All";

    public const string FromLabel = "From (a South African day)";

    public const string ToLabel = "To (a South African day)";

    public const string Show = "Show";

    public const string ClearFilters = "Clear filters";

    public const string ClearTheFilters = "Clear the filters";

    public const string RunsHeading = "Runs";

    public const string RunsCaption = "Runs, newest first";

    /// <summary>The statuses offered by Status, in the boards' order (R3-R01), each its own word.</summary>
    public static readonly IReadOnlyList<string> Statuses =
    [
        nameof(ScheduledJobRunStatus.Running),
        nameof(ScheduledJobRunStatus.Succeeded),
        nameof(ScheduledJobRunStatus.Failed)
    ];

    /// <summary>"21 runs.", "1 run.", or "No runs match these filters." (T17, K19).</summary>
    public static string CountLine(int total)
        => total switch
        {
            0 => "No runs match these filters.",
            1 => "1 run.",
            _ => $"{Number(total)} runs."
        };

    public const string NoMatchTitle = "No runs match";

    /// <summary>"The filters are kept above. Clear them to see all 21." (T17); its singular is on no board.</summary>
    public static string NoMatchBody(int allRuns)
        => allRuns == 1
            ? "The filters are kept above. Clear them to see the one run."
            : $"The filters are kept above. Clear them to see all {Number(allRuns)}.";

    public const string NoneTitle = "No job has run yet";

    public const string NoneBody = "The scheduler runs each job at its time; Scheduled jobs shows when each runs next.";

    /// <summary>Ran by: the person's name, or "Scheduler" for the scheduler's own runs (T1).</summary>
    public static string RanBy(string? name) => string.IsNullOrWhiteSpace(name) ? "Scheduler" : name;

    /// <summary>
    /// How long a run took, "7 ms" as the boards draw it; a run of a second or more in seconds, of a minute or more in
    /// minutes and seconds; "—" while it runs (R3-R07).
    /// </summary>
    public static string Duration(DateTime startedAt, DateTime? finishedAt)
    {
        if (finishedAt is not { } finished)
        {
            return "—";
        }

        var duration = finished - startedAt;
        if (duration < TimeSpan.Zero)
        {
            duration = TimeSpan.Zero;
        }

        if (duration.TotalSeconds < 1)
        {
            return $"{Math.Round(duration.TotalMilliseconds).ToString("0", CultureInfo.InvariantCulture)} ms";
        }

        if (duration.TotalMinutes < 1)
        {
            return $"{duration.TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture)} s";
        }

        return $"{(int)duration.TotalMinutes} min {duration.Seconds} s";
    }

    // ---- shared ----

    /// <summary>A run's status in its word (R3-J01, R3-R01), never an enum's name read through.</summary>
    public static string Status(string? status)
        => status switch
        {
            nameof(ScheduledJobRunStatus.Running) => "Running",
            nameof(ScheduledJobRunStatus.Succeeded) => "Succeeded",
            nameof(ScheduledJobRunStatus.Failed) => "Failed",
            _ => "Unknown"
        };

    /// <summary>The South African day of a moment, its time's first half: "2026-11-10".</summary>
    public static string Day(DateTime utc) => SastTime.Day(utc);

    /// <summary>The time to the minute with its zone, the second half: "21:39 SAST".</summary>
    public static string ClockToMinute(DateTime utc) => SastTime.Minute(utc).Split(' ', 2)[1];

    /// <summary>The time to the second with its zone: "21:41:44 SAST".</summary>
    public static string ClockToSecond(DateTime utc) => SastTime.Second(utc).Split(' ', 2)[1];

    private static string Number(int value) => value.ToString("N0", CultureInfo.InvariantCulture);

    // An error that ends a sentence keeps its own stop; else the result's sentence gets one.
    private static string Sentence(string text)
        => text.EndsWith('.') || text.EndsWith('!') || text.EndsWith('?') ? text : text + ".";
}
