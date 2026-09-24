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

    public ScheduledJobDispatcher(
        IServiceScopeFactory scopeFactory,
        ScheduledJobLocks locks,
        ILogger<ScheduledJobDispatcher> logger)
    {
        _scopeFactory = scopeFactory;
        _locks = locks;
        _logger = logger;
    }

    public async Task DispatchNowAsync(IScheduledJob job, string? triggeredByUserId, CancellationToken cancellationToken)
    {
        // The scheduler's own lock for the job (T206 review): a run by hand while another is going is refused, not run
        // beside it. Refused before a run row is written, since nothing runs.
        using var held = _locks.TryEnter(job.Key)
            ?? throw new InvalidOperationException(AlreadyRunning(job.Key));

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

    /// <summary>The refusal of "Run now" while the job is already running. (T206 review)</summary>
    public static string AlreadyRunning(string jobKey)
        => $"Job '{jobKey}' is already running, so it was not started again. Wait for that run to finish, then run it " +
           "again if it is still needed.";
}
