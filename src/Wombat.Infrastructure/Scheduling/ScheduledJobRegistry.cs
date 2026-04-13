using Wombat.Application.Scheduling;

namespace Wombat.Infrastructure.Scheduling;

public sealed class ScheduledJobRegistry : IScheduledJobRegistry
{
    private readonly List<IScheduledJob> _jobs = [];

    public IReadOnlyList<IScheduledJob> Jobs => _jobs;

    public void Register(IScheduledJob job)
    {
        if (_jobs.Any(j => string.Equals(j.Key, job.Key, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException($"A job with key '{job.Key}' is already registered.");
        }

        _jobs.Add(job);
    }

    public IScheduledJob? GetByKey(string key)
        => _jobs.FirstOrDefault(j => string.Equals(j.Key, key, StringComparison.Ordinal));
}
