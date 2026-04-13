using System.Collections.Concurrent;
using Cronos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.Scheduling;

namespace Wombat.Infrastructure.Scheduling;

public sealed class ScheduledJobHost : BackgroundService
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan GraceWindow = TimeSpan.FromMinutes(30);

    private readonly IScheduledJobRegistry _registry;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScheduledJobHost> _logger;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public ScheduledJobHost(
        IScheduledJobRegistry registry,
        IServiceScopeFactory scopeFactory,
        ILogger<ScheduledJobHost> logger)
    {
        _registry = registry;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ScheduledJobHost starting. {Count} jobs registered.", _registry.Jobs.Count);

        await SeedDefinitionsAsync(stoppingToken);
        await ReconcileStaleRunsAsync(stoppingToken);
        await CatchUpMissedRunsAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await TickAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "ScheduledJobHost tick error.");
            }

            await Task.Delay(TickInterval, stoppingToken);
        }
    }

    private async Task TickAsync(CancellationToken stoppingToken)
    {
        var utcNow = DateTime.UtcNow;

        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var definitions = await dbContext.Set<ScheduledJobDefinition>()
            .Where(d => d.IsEnabled)
            .ToListAsync(stoppingToken);

        var enabledKeys = definitions.Select(d => d.Key).ToHashSet(StringComparer.Ordinal);

        foreach (var job in _registry.Jobs)
        {
            if (!enabledKeys.Contains(job.Key))
                continue;

            var definition = definitions.First(d => d.Key == job.Key);
            if (!IsDue(definition, utcNow, dbContext, stoppingToken, out _))
                continue;

            var semaphore = _locks.GetOrAdd(job.Key, _ => new SemaphoreSlim(1, 1));
            if (!semaphore.Wait(0))
            {
                _logger.LogDebug("Job '{JobKey}' is still running, skipping.", job.Key);
                continue;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    await DispatchJobAsync(job, null, stoppingToken);
                }
                finally
                {
                    semaphore.Release();
                }
            }, stoppingToken);
        }
    }

    private bool IsDue(ScheduledJobDefinition definition, DateTime utcNow, IApplicationDbContext dbContext, CancellationToken ct, out CronExpression? cronExpression)
    {
        cronExpression = null;
        try
        {
            cronExpression = CronExpression.Parse(definition.CronExpression);
        }
        catch
        {
            _logger.LogWarning("Invalid cron expression for job '{Key}': {Cron}", definition.Key, definition.CronExpression);
            return false;
        }

        var lastRun = dbContext.Set<ScheduledJobRun>()
            .Where(r => r.Key == definition.Key)
            .OrderByDescending(r => r.StartedAt)
            .Select(r => (DateTime?)r.StartedAt)
            .FirstOrDefault();

        var baseTime = lastRun ?? utcNow.AddDays(-1);
        var nextOccurrence = cronExpression.GetNextOccurrence(baseTime, inclusive: false);

        return nextOccurrence.HasValue && nextOccurrence.Value <= utcNow;
    }

    private async Task DispatchJobAsync(IScheduledJob job, string? triggeredBy, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var run = new ScheduledJobRun
        {
            Key = job.Key,
            StartedAt = DateTime.UtcNow,
            Status = ScheduledJobRunStatus.Running,
            TriggeredBy = triggeredBy
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
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Scheduled job '{JobKey}' failed.", job.Key);
            run.Status = ScheduledJobRunStatus.Failed;
            run.FinishedAt = DateTime.UtcNow;
            run.ErrorMessage = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
        }

        await dbContext.SaveChangesAsync(CancellationToken.None);
    }

    private async Task SeedDefinitionsAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var existing = await dbContext.Set<ScheduledJobDefinition>()
            .Select(d => d.Key)
            .ToListAsync(cancellationToken);

        var existingKeys = existing.ToHashSet(StringComparer.Ordinal);

        foreach (var job in _registry.Jobs)
        {
            if (existingKeys.Contains(job.Key))
                continue;

            dbContext.Set<ScheduledJobDefinition>().Add(new ScheduledJobDefinition
            {
                Key = job.Key,
                CronExpression = job.CronExpression,
                IsEnabled = true,
                Description = job.Description
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ReconcileStaleRunsAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var staleRuns = await dbContext.Set<ScheduledJobRun>()
            .Where(r => r.Status == ScheduledJobRunStatus.Running)
            .ToListAsync(cancellationToken);

        foreach (var run in staleRuns)
        {
            _logger.LogWarning("Reconciling stale run for job '{Key}' (started {StartedAt}).", run.Key, run.StartedAt);
            run.Status = ScheduledJobRunStatus.Failed;
            run.FinishedAt = DateTime.UtcNow;
            run.ErrorMessage = "Marked as failed during startup reconciliation — process likely crashed.";
        }

        if (staleRuns.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task CatchUpMissedRunsAsync(CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;

        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var definitions = await dbContext.Set<ScheduledJobDefinition>()
            .Where(d => d.IsEnabled)
            .ToListAsync(cancellationToken);

        foreach (var definition in definitions)
        {
            var job = _registry.GetByKey(definition.Key);
            if (job is null)
                continue;

            CronExpression cron;
            try
            {
                cron = CronExpression.Parse(definition.CronExpression);
            }
            catch
            {
                continue;
            }

            var lastRun = await dbContext.Set<ScheduledJobRun>()
                .Where(r => r.Key == definition.Key)
                .OrderByDescending(r => r.StartedAt)
                .Select(r => (DateTime?)r.StartedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastRun is null)
                continue;

            var nextAfterLastRun = cron.GetNextOccurrence(lastRun.Value, inclusive: false);
            if (nextAfterLastRun.HasValue && nextAfterLastRun.Value <= utcNow && (utcNow - nextAfterLastRun.Value) <= GraceWindow)
            {
                _logger.LogInformation("Catching up missed run for job '{Key}' (was due {DueAt}).", definition.Key, nextAfterLastRun.Value);
                await DispatchJobAsync(job, null, cancellationToken);
            }
        }
    }
}
