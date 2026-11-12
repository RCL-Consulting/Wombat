using System.Globalization;
using Wombat.Application.Features.Dashboards.Administrator;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Components.Pages.Dashboards;

/// <summary>
/// The words of the Administrator's Home (T400, flow 18, lane A; boards R3-H01–H12; R3-0-spec-2 § 8 and § 9): System
/// health's three lines, each a name, a badge and its sentences, and the Accounts card. Every time is
/// <see cref="SastTime" />'s.
/// </summary>
/// <remarks>
/// A state is a word on a badge, never a colour alone (T327, S5: the status dots are retired). The badge's tint is
/// <see cref="BadgeFor" />'s (R3-0-spec-3 § 11): OK green; Failing and Unreachable red; Not configured and Disabled amber
/// (N3); Overdue its own; Not read grey.
/// </remarks>
public static class AdministratorHomeWords
{
    public const string HealthTitle = "System health";

    public const string AccountsTitle = "Accounts";

    public const string DatabaseName = "Database";

    public const string MailName = "Mail";

    public const string JobsName = "Scheduled jobs";

    public const string Ok = "OK";

    public const string Failing = "Failing";

    public const string Unreachable = "Unreachable";

    public const string NotConfigured = "Not configured";

    public const string Disabled = "Disabled";

    public const string Overdue = "Overdue";

    public const string NotRead = "Not read";

    /// <summary>A part the database did not answer for (T9, R3-H08).</summary>
    public const string NotReadSentence = "Not read: the database did not answer.";

    public const string DatabaseAnswers = "The database answers.";

    /// <summary>Q2.</summary>
    public const string MailNotConfigured = "Mail is written to the log, not sent.";

    public const string MailNotConfiguredHelp = "To send mail, set Email__SmtpHost on the server and restart Wombat.";

    /// <summary>The Accounts card's sentence (T14).</summary>
    public const string AccountsSentence = "Every account Wombat holds but an erased one.";

    public const string NoneLocked = "None is locked.";

    /// <summary>The jobs line when no job is stored (the build review's N7; NEW, build-contracts § 5).</summary>
    public const string NoJobsStored = "No scheduled job is stored yet: the scheduler stores them as it starts.";

    /// <summary>The footer buttons (S1; T13, T14).</summary>
    public const string ScheduledJobsLink = "Scheduled jobs";

    public const string ItsRunsLink = "Its runs";

    public const string UsersLink = "Users";

    /// <summary>One line of System health: its name, its badge's word and class, and what it says.</summary>
    /// <param name="Paragraphs">Its sentences, each a paragraph, in order.</param>
    /// <param name="Items">A list under the paragraphs: the jobs that need the Administrator, a sentence each (R3-H09).</param>
    public sealed record HealthLine(
        string Name,
        string Badge,
        string BadgeClass,
        IReadOnlyList<string> Paragraphs,
        IReadOnlyList<string> Items);

    // ─── Database ───────────────────────────────────────────────────────────

    public static HealthLine Database(AdministratorDashboardSummaryDto summary)
        => summary.DatabaseFailedAt is { } failedAt
            ? new HealthLine(DatabaseName, Unreachable, BadgeFor.State(BadgeState.Declined),
                [$"The database did not answer at {SastTime.Clock(failedAt)} SAST, so Wombat can read and save nothing, and /health says Unhealthy."], [])
            : new HealthLine(DatabaseName, Ok, BadgeFor.State(BadgeState.Completed), [DatabaseAnswers], []);

    // ─── Mail (Q2, Q13, Q18) ────────────────────────────────────────────────

    public static HealthLine Mail(AdministratorMailDto mail)
    {
        if (!mail.IsConfigured)
        {
            return new HealthLine(MailName, NotConfigured, BadgeFor.State(BadgeState.Accepted),
                [MailNotConfigured, MailNotConfiguredHelp], []);
        }

        if (mail.Outcomes is not { } outcomes)
        {
            return NotReadLine(MailName);
        }

        if (outcomes.DroppedLast24Hours > 0 && outcomes.LastDropAt is { } lastDrop)
        {
            // Q13: a drop in the 24 hours is Failing. K3: a drop with no refusal (abandoned as the host stopped, or after a
            // failed attempt while it waited) reads without the refusal clause.
            var dropped = outcomes.DroppedLast24Hours == 1
                ? "1 mail was dropped in the last 24 hours"
                : Invariant($"{outcomes.DroppedLast24Hours} mails were dropped in the last 24 hours");
            var refusal = string.IsNullOrWhiteSpace(outcomes.LastDropRefusal) ? string.Empty : $": {Unstopped(outcomes.LastDropRefusal)}";
            return new HealthLine(MailName, Failing, BadgeFor.State(BadgeState.Declined),
                [$"{dropped}, the last at {SastTime.Minute(lastDrop)}{refusal}. The application log names each one."], []);
        }

        var waiting = mail.QueueDepth == 0 ? "none waiting" : Invariant($"{mail.QueueDepth} waiting");
        var sentence = outcomes.LastSentAt is { } lastSent
            ? $"Mail is sent. Last sent {SastTime.Minute(lastSent)}; {waiting}; none dropped in the last 24 hours."
            : $"Nothing sent since {SastTime.Minute(outcomes.WindowStart)}; {waiting}; none dropped in the last 24 hours.";
        return new HealthLine(MailName, Ok, BadgeFor.State(BadgeState.Completed), [sentence], []);
    }

    // ─── Scheduled jobs (Q3, K4) ────────────────────────────────────────────

    public static HealthLine Jobs(AdministratorJobsDto? jobs)
    {
        if (jobs is null)
        {
            return NotReadLine(JobsName);
        }

        var all = jobs.Jobs;
        if (all.Count == 0)
        {
            // No definition stored: the host stores the jobs as it starts, so only a host that never started its
            // scheduler reaches this. "All 0 jobs last succeeded." would claim what is not so (the build review's N7).
            return new HealthLine(JobsName, NotConfigured, BadgeFor.State(BadgeState.Accepted), [NoJobsStored], []);
        }

        var needing = all.Where(job => NeedsYou(job.State))
            .OrderBy(job => Severity(job.State))
            .ThenBy(job => job.Key, StringComparer.Ordinal)
            .ToList();
        var neverRun = all.Where(job => job.State == AdministratorJobState.NeverRun).ToList();
        var fine = all.Count(job => job.State is AdministratorJobState.Succeeded or AdministratorJobState.Running);

        if (needing.Count == 0)
        {
            var paragraphs = new List<string>();
            if (neverRun.Count == 0)
            {
                paragraphs.Add(Invariant($"All {all.Count} {JobWord(all.Count)} last succeeded."));
            }
            else
            {
                var lead = Invariant($"{fine} {JobWord(fine)} last succeeded.");
                paragraphs.Add(string.Join(' ', [lead, .. neverRun.Select(NotRunYet)]));
            }

            return new HealthLine(JobsName, Ok, BadgeFor.State(BadgeState.Completed), paragraphs, []);
        }

        var (badge, badgeClass) = Worst(needing);
        if (needing.Count == 1)
        {
            var job = needing[0];
            var sentence = Sentence(job);
            if (job.State == AdministratorJobState.Disabled)
            {
                // R3-H06: the one job the scheduler does not run, and the rest.
                sentence += Invariant($" The other {fine} last succeeded.");
            }

            return new HealthLine(JobsName, badge, badgeClass, [string.Join(' ', [sentence, .. neverRun.Select(NotRunYet)])], []);
        }

        // R3-H09: each job its own sentence under the count.
        return new HealthLine(JobsName, badge, badgeClass, [$"{Count(needing.Count)} jobs need you:"],
            needing.Select(Sentence).ToList());
    }

    /// <summary>
    /// The one job whose runs "Its runs" opens (R3-H05, H09): the job that failed, when exactly one did; null otherwise,
    /// and then the footer offers Scheduled jobs alone.
    /// </summary>
    public static string? FailedJobKey(AdministratorJobsDto? jobs)
    {
        var failed = jobs?.Jobs.Where(job => job.State == AdministratorJobState.Failed).ToList();
        return failed is { Count: 1 } ? failed[0].Key : null;
    }

    /// <summary>"Its runs"' hidden words: ": entrustment-decision-expiry, failed" (R3-H05).</summary>
    public static string ItsRunsHidden(string key) => $": {key}, failed";

    // ─── Accounts (Q4, Q14, T14) ────────────────────────────────────────────

    public static string AccountsLabel(int accounts) => accounts == 1 ? "account" : "accounts";

    public static string Locked(int locked) => locked switch
    {
        0 => NoneLocked,
        1 => "1 is locked and cannot sign in now.",
        _ => Invariant($"{locked} are locked and cannot sign in now.")
    };

    // ─── Helpers ────────────────────────────────────────────────────────────

    private static HealthLine NotReadLine(string name)
        => new(name, NotRead, BadgeFor.State(BadgeState.Draft), [NotReadSentence], []);

    private static bool NeedsYou(AdministratorJobState state) => state is AdministratorJobState.Failed
        or AdministratorJobState.Unreadable or AdministratorJobState.Overdue or AdministratorJobState.Disabled;

    /// <summary>The worst first: Failing, then Overdue, then Disabled (R3-H09).</summary>
    private static int Severity(AdministratorJobState state) => state switch
    {
        AdministratorJobState.Failed => 0,
        AdministratorJobState.Unreadable => 1,
        AdministratorJobState.Overdue => 2,
        _ => 3
    };

    private static (string Badge, string Class) Worst(IReadOnlyList<AdministratorJobHealthDto> needing)
        => Severity(needing[0].State) switch
        {
            <= 1 => (Failing, BadgeFor.State(BadgeState.Declined)),
            2 => (Overdue, BadgeFor.Overdue),
            _ => (Disabled, BadgeFor.State(BadgeState.Accepted))
        };

    private static string Sentence(AdministratorJobHealthDto job) => job.State switch
    {
        AdministratorJobState.Failed => job.LastErrorFirstLine is { } error
            ? $"{job.Key} failed at {SastTime.Minute(FailedAt(job))}: {Unstopped(error)}."
            : $"{job.Key} failed at {SastTime.Minute(FailedAt(job))}.",
        // K4, NEW (build-contracts § 5 row 2).
        AdministratorJobState.Unreadable =>
            $"{job.Key}'s schedule cannot be read (cron {job.Cron}), so the scheduler does not run it. The application log says why.",
        AdministratorJobState.Overdue => job.NextOccurrence is { } due
            ? $"{job.Key} has not run since it was due at {SastTime.Minute(due)}: the scheduler did not start it. The application log says why; Run now runs it."
            : $"{job.Key} has not run since it was due: the scheduler did not start it. The application log says why; Run now runs it.",
        _ => $"{job.Key} is disabled, so the scheduler does not run it."
    };

    private static string NotRunYet(AdministratorJobHealthDto job) => job.NextOccurrence is { } first
        ? $"{job.Key} has not run yet: its first run is {SastTime.Minute(first)}."
        : $"{job.Key} has not run yet.";

    private static DateTime FailedAt(AdministratorJobHealthDto job) => job.LastRunFinishedAt ?? job.LastRunStartedAt ?? default;

    private static string JobWord(int count) => count == 1 ? "job" : "jobs";

    /// <summary>"Three jobs need you:" (R3-H09): the count in words to ten.</summary>
    private static string Count(int count) => count switch
    {
        2 => "Two",
        3 => "Three",
        4 => "Four",
        5 => "Five",
        6 => "Six",
        7 => "Seven",
        8 => "Eight",
        9 => "Nine",
        10 => "Ten",
        _ => count.ToString(CultureInfo.InvariantCulture)
    };

    /// <summary>A stored text whose own full stop the sentence's would double.</summary>
    private static string Unstopped(string text)
    {
        var trimmed = text.Trim();
        return trimmed.EndsWith('.') ? trimmed[..^1] : trimmed;
    }

    private static string Invariant(FormattableString text) => FormattableString.Invariant(text);
}
