using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.Scheduling;

namespace Wombat.Infrastructure.Scheduling;

public sealed class ScheduledJobDispatcher : IScheduledJobDispatcher
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScheduledJobDispatcher> _logger;

    public ScheduledJobDispatcher(IServiceScopeFactory scopeFactory, ILogger<ScheduledJobDispatcher> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task DispatchNowAsync(IScheduledJob job, string? triggeredByUserId, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var run = new ScheduledJobRun
        {
            Key = job.Key,
            StartedAt = DateTime.UtcNow,
            Status = ScheduledJobRunStatus.Running,
            TriggeredBy = triggeredByUserId
        };

        dbContext.Set<ScheduledJobRun>().Add(run);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var context = new ScheduledJobContext(DateTime.UtcNow, _logger);
            await job.ExecuteAsync(context, cancellationToken);

            run.Status = ScheduledJobRunStatus.Succeeded;
            run.FinishedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scheduled job '{JobKey}' failed.", job.Key);
            run.Status = ScheduledJobRunStatus.Failed;
            run.FinishedAt = DateTime.UtcNow;
            run.ErrorMessage = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
        }

        await dbContext.SaveChangesAsync(CancellationToken.None);
    }
}
