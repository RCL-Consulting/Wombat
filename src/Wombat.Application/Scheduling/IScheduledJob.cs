namespace Wombat.Application.Scheduling;

public interface IScheduledJob
{
    string Key { get; }
    string CronExpression { get; }
    string Description { get; }
    Task ExecuteAsync(ScheduledJobContext context, CancellationToken cancellationToken);
}
