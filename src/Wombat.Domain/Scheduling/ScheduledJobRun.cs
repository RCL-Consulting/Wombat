namespace Wombat.Domain.Scheduling;

public sealed class ScheduledJobRun
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public ScheduledJobRunStatus Status { get; set; } = ScheduledJobRunStatus.Running;
    public string? ErrorMessage { get; set; }
    public string? TriggeredBy { get; set; }
}
