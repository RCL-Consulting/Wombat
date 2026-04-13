namespace Wombat.Domain.Scheduling;

public sealed class ScheduledJobDefinition
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public string Description { get; set; } = string.Empty;
}
