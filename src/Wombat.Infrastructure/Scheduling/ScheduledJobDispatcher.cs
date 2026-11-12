using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.Scheduling;

namespace Wombat.Infrastructure.Scheduling;

public sealed class ScheduledJobDispatcher : IScheduledJobDispatcher
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ScheduledJobLocks _locks;
    private readonly ILogger<ScheduledJobDispatcher> _logger;
    private readonly TimeProvider _timeProvider;

    public ScheduledJobDispatcher(
        IServiceScopeFactory scopeFactory,
        ScheduledJobLocks locks,
        ILogger<ScheduledJobDispatcher> logger,
        TimeProvider? timeProvider = null)
    {
        _scopeFactory = scopeFactory;
        _locks = locks;
        _logger = logger;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<ScheduledJobRunOutcome> DispatchNowAsync(
        IScheduledJob job, string? triggeredByUserId, CancellationToken cancellationToken)
    {
        // The scheduler's own lock for the job (T206 review): a run by hand while another is going is refused, not run
        // beside it. Refused before a run row is written, since nothing runs.
        using var held = _locks.TryEnter(job.Key)
            ?? throw ScheduledJobRefusal.AlreadyRunning(job.Key);

        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        // A disabled job is refused here, under the lock the scheduler takes too (T400, K6, T22): a page read before
        // someone disabled the job offered Run now, and a Disable between this check and the run is then honoured, since
        // the scheduler's tick reads the enabled set before taking its own lock. No stored definition (the host has not
        // seeded it yet) is not a disabled one.
        var enabled = await dbContext.Set<ScheduledJobDefinition>()
            .AsNoTracking()
            .Where(definition => definition.Key == job.Key)
            .Select(definition => (bool?)definition.IsEnabled)
            .SingleOrDefaultAsync(cancellationToken);
        if (enabled == false)
        {
            throw ScheduledJobRefusal.Disabled(job.Key);
        }

        var run = new ScheduledJobRun
        {
            Key = job.Key,
            StartedAt = UtcNow(),
            Status = ScheduledJobRunStatus.Running,
            TriggeredBy = triggeredByUserId
        };

        dbContext.Set<ScheduledJobRun>().Add(run);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var context = new ScheduledJobContext(UtcNow(), _logger);
            await job.ExecuteAsync(context, cancellationToken);

            run.Status = ScheduledJobRunStatus.Succeeded;
            run.FinishedAt = UtcNow();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scheduled job '{JobKey}' failed.", job.Key);
            run.Status = ScheduledJobRunStatus.Failed;
            run.FinishedAt = UtcNow();
            run.ErrorMessage = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
        }

        await dbContext.SaveChangesAsync(CancellationToken.None);

        // The run has ended: the page says how (T400, Q10).
        return new ScheduledJobRunOutcome(run.Status, run.StartedAt, run.FinishedAt, run.ErrorMessage);
    }

    private DateTime UtcNow() => _timeProvider.GetUtcNow().UtcDateTime;
}
