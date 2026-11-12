using Wombat.Domain.Scheduling;

namespace Wombat.Application.Scheduling;

/// <summary>
/// How a run started by "Run now" ended (T400, flow 18, K6, Q10): the run has finished when the page says what it did,
/// so the result names its status, when it started, and a failed run's error as stored. Every moment is UTC.
/// </summary>
public sealed record ScheduledJobRunOutcome(
    ScheduledJobRunStatus Status,
    DateTime StartedAt,
    DateTime? FinishedAt,
    string? ErrorMessage);
