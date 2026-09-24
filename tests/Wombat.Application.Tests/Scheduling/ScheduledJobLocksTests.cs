using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.Scheduling;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling;

namespace Wombat.Application.Tests.Scheduling;

/// <summary>
/// A job never runs twice at once: the scheduler and an administrator's "Run now" take one lock per job
/// (<see cref="ScheduledJobLocks" />). And a stored job's description follows the job's own. (T206 review)
/// </summary>
/// <remarks>
/// Until the T206 review the scheduler kept its locks to itself and "Run now" took none, so a run by hand during the
/// scheduled one read the same rows and acted on them again: for the MSF reminder, a second reminder to each respondent,
/// whose link retired the first one's.
/// </remarks>
public sealed class ScheduledJobLocksTests
{
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(5);

    [Fact]
    public async Task RunNow_WhileTheSchedulerIsRunningTheJob_IsRefused_AndTheJobRunsOnce()
    {
        using var provider = BuildProvider();
        var job = new BlockingJob("blocking-job");
        var locks = new ScheduledJobLocks();
        var host = Host(provider, locks, job);

        await host.SeedDefinitionsAsync(CancellationToken.None);
        await host.TickAsync(CancellationToken.None);
        await job.Started.Task.WaitAsync(Patience);

        try
        {
            // Bounded: a run that was not refused would wait on the job, which holds every run open until released.
            var runNow = () => Dispatcher(provider, locks)
                .DispatchNowAsync(job, "admin-1", CancellationToken.None)
                .WaitAsync(Patience);
            (await runNow.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage(ScheduledJobDispatcher.AlreadyRunning(job.Key));
        }
        finally
        {
            job.Release();
        }

        await WaitUntilLockIsFreeAsync(locks, job.Key);

        job.Executions.Should().Be(1);
        var runs = await RunsAsync(provider);
        runs.Should().ContainSingle("the refused run by hand wrote no run row")
            .Which.TriggeredBy.Should().BeNull("the one run is the scheduler's");
    }

    [Fact]
    public async Task ASecondRunNow_WhileTheFirstIsGoing_IsRefused_AndOnceItHasFinished_TheJobCanBeRunAgain()
    {
        using var provider = BuildProvider();
        var job = new BlockingJob("blocking-job");
        var locks = new ScheduledJobLocks();
        var dispatcher = Dispatcher(provider, locks);

        var first = dispatcher.DispatchNowAsync(job, "admin-1", CancellationToken.None);
        await job.Started.Task.WaitAsync(Patience);

        try
        {
            // Bounded, as above.
            var second = () => dispatcher.DispatchNowAsync(job, "admin-2", CancellationToken.None).WaitAsync(Patience);
            (await second.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage(ScheduledJobDispatcher.AlreadyRunning(job.Key));
        }
        finally
        {
            job.Release();
        }

        await first.WaitAsync(Patience);

        await dispatcher.DispatchNowAsync(job, "admin-2", CancellationToken.None);

        job.Executions.Should().Be(2);
        (await RunsAsync(provider)).Select(run => run.TriggeredBy).Should().Equal("admin-1", "admin-2");
    }

    [Fact]
    public async Task TheCatchUpOfAMissedRun_WhileARunByHandIsGoing_DoesNotStartTheJobAgain()
    {
        using var provider = BuildProvider();
        var job = new BlockingJob("blocking-job");
        job.Release();
        var locks = new ScheduledJobLocks();
        var host = Host(provider, locks, job);

        // The last run started two minutes ago, so the next, a minute ago, was missed and is inside the catch-up window.
        await host.SeedDefinitionsAsync(CancellationToken.None);
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Set<ScheduledJobRun>().Add(new ScheduledJobRun
            {
                Key = job.Key,
                StartedAt = DateTime.UtcNow.AddMinutes(-2),
                FinishedAt = DateTime.UtcNow.AddMinutes(-2),
                Status = ScheduledJobRunStatus.Succeeded
            });
            await db.SaveChangesAsync();
        }

        using (locks.TryEnter(job.Key) ?? throw new InvalidOperationException("guard: the lock is free"))
        {
            // What "Run now" holds while it runs the job.
            await host.CatchUpMissedRunsAsync(CancellationToken.None);
        }

        job.Executions.Should().Be(0);
        (await RunsAsync(provider)).Should().ContainSingle("the catch-up wrote no run row");

        await host.CatchUpMissedRunsAsync(CancellationToken.None);
        job.Executions.Should().Be(1, "guard: with the lock free, the missed run is caught up");
    }

    [Fact]
    public async Task AStoredJobsDescription_IsBroughtUpToDate_AndItsScheduleAndSwitchAreLeftAsAnAdministratorSetThem()
    {
        using var provider = BuildProvider();
        var job = new BlockingJob("blocking-job");

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Set<ScheduledJobDefinition>().Add(new ScheduledJobDefinition
            {
                Key = job.Key,
                CronExpression = "0 3 * * *",
                IsEnabled = false,
                Description = "What the job did when it was first deployed."
            });
            await db.SaveChangesAsync();
        }

        await Host(provider, new ScheduledJobLocks(), job).SeedDefinitionsAsync(CancellationToken.None);

        using var read = provider.CreateScope();
        var stored = await read.ServiceProvider.GetRequiredService<ApplicationDbContext>()
            .Set<ScheduledJobDefinition>().AsNoTracking().SingleAsync();
        stored.Description.Should().Be(job.Description);
        stored.CronExpression.Should().Be("0 3 * * *");
        stored.IsEnabled.Should().BeFalse();
    }

    private static ScheduledJobHost Host(ServiceProvider provider, ScheduledJobLocks locks, IScheduledJob job)
    {
        var registry = new ScheduledJobRegistry();
        registry.Register(job);
        return new ScheduledJobHost(
            registry,
            provider.GetRequiredService<IServiceScopeFactory>(),
            locks,
            NullLogger<ScheduledJobHost>.Instance);
    }

    private static ScheduledJobDispatcher Dispatcher(ServiceProvider provider, ScheduledJobLocks locks)
        => new(provider.GetRequiredService<IServiceScopeFactory>(), locks, NullLogger<ScheduledJobDispatcher>.Instance);

    private static async Task<List<ScheduledJobRun>> RunsAsync(ServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
            .Set<ScheduledJobRun>().AsNoTracking().OrderBy(run => run.StartedAt).ToListAsync();
    }

    /// <summary>The scheduler's run releases the lock once it has stored how the run ended.</summary>
    private static async Task WaitUntilLockIsFreeAsync(ScheduledJobLocks locks, string key)
    {
        var deadline = DateTime.UtcNow + Patience;
        while (DateTime.UtcNow < deadline)
        {
            using var held = locks.TryEnter(key);
            if (held is not null)
            {
                return;
            }

            await Task.Delay(20);
        }

        throw new TimeoutException($"Job '{key}' still held its lock after {Patience}.");
    }

    private static ServiceProvider BuildProvider()
    {
        // Named once, outside the lambda, so every scope reads the same database.
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(dbName));
        services.AddScoped<IApplicationDbContext>(p => p.GetRequiredService<ApplicationDbContext>());
        return services.BuildServiceProvider();
    }

    /// <summary>A job due every minute that, until released, holds each run open once it has started.</summary>
    private sealed class BlockingJob(string key) : IScheduledJob
    {
        private readonly TaskCompletionSource _released = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _executions;

        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int Executions => Volatile.Read(ref _executions);

        public string Key => key;
        public string CronExpression => "* * * * *";
        public string Description => "Blocks each run until the test releases it.";

        public async Task ExecuteAsync(ScheduledJobContext context, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _executions);
            Started.TrySetResult();
            await _released.Task;
        }

        public void Release() => _released.TrySetResult();
    }
}
