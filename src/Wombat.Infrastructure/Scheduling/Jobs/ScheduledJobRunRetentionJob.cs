using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.Scheduling;

namespace Wombat.Infrastructure.Scheduling.Jobs;

public sealed class ScheduledJobRunRetentionJob : IScheduledJob
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ScheduledJobRunRetentionJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public string Key => "scheduled-job-run-retention";
    public string CronExpression => "0 4 * * *";
    public string Description => "Deletes ScheduledJobRun records older than 90 days (daily at 04:00 UTC).";

    public async Task ExecuteAsync(ScheduledJobContext context, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var cutoff = context.UtcNow.AddDays(-90);

        var oldRuns = dbContext.Set<ScheduledJobRun>()
            .Where(r => r.StartedAt < cutoff);

        var count = oldRuns.Count();
        if (count == 0)
        {
            context.Logger.LogInformation("ScheduledJobRunRetentionJob: no old runs to delete.");
            return;
        }

        dbContext.Set<ScheduledJobRun>().RemoveRange(oldRuns);
        await dbContext.SaveChangesAsync(cancellationToken);

        context.Logger.LogInformation("ScheduledJobRunRetentionJob: deleted {Count} old job run records.", count);
    }
}
