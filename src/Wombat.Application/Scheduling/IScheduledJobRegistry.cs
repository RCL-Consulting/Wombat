namespace Wombat.Application.Scheduling;

public interface IScheduledJobRegistry
{
    IReadOnlyList<IScheduledJob> Jobs { get; }
    IScheduledJob? GetByKey(string key);
}
