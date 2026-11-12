using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Scheduling.Commands.RunScheduledJobNow;
using Wombat.Application.Scheduling;
using Wombat.Domain.Scheduling;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling;

namespace Wombat.Application.Tests.Scheduling;

/// <summary>
/// Run now (T400, flow 18, lane B; K6, T22, Q10): the run it made, answered once it has ended; a disabled job and one
/// already running refused with <see cref="ScheduledJobRefusal" /> under the job's lock, before a run row is written; a key
/// not in the registry a plain failure; and the run stored as the caller's, never "unknown".
/// </summary>
public sealed class RunScheduledJobNowTests
{
    private static readonly DateTime Now = new(2026, 11, 10, 19, 39, 58, DateTimeKind.Utc);

    [Fact]
    public async Task ARunThatSucceeds_IsAnsweredWithItsOutcome_AndStoredAsTheCallers()
    {
        using var provider = BuildProvider();
        var job = new Job("weekly-coordinator-digest");
        await StoreAsync(provider, Definition(job.Key, enabled: true));

        var outcome = await Handler(provider, job).Handle(Command(job.Key, "admin-1"), CancellationToken.None);

        outcome.Should().Be(new ScheduledJobRunOutcome(ScheduledJobRunStatus.Succeeded, Now, Now, null));
        job.Executions.Should().Be(1);
        var run = (await RunsAsync(provider)).Should().ContainSingle().Which;
        run.TriggeredBy.Should().Be("admin-1", "the caller's id, from the principal (K6)");
        run.Status.Should().Be(ScheduledJobRunStatus.Succeeded);
    }

    [Fact]
    public async Task ARunThatFails_IsAnsweredWithItsErrorAsStored()
    {
        using var provider = BuildProvider();
        var job = new Job("entrustment-decision-expiry", fails: "The mail server refused the message.");
        await StoreAsync(provider, Definition(job.Key, enabled: true));

        var outcome = await Handler(provider, job).Handle(Command(job.Key, "admin-1"), CancellationToken.None);

        outcome.Status.Should().Be(ScheduledJobRunStatus.Failed);
        outcome.ErrorMessage.Should().Be("The mail server refused the message.");
        outcome.FinishedAt.Should().Be(Now);
        (await RunsAsync(provider)).Should().ContainSingle().Which.ErrorMessage.Should().Be("The mail server refused the message.");
    }

    [Fact]
    public async Task ADisabledJob_IsRefused_InItsOwnWords_AndNothingRunsOrIsStored()
    {
        using var provider = BuildProvider();
        var job = new Job("msf-campaign-auto-close");
        await StoreAsync(provider, Definition(job.Key, enabled: false));

        var act = () => Handler(provider, job).Handle(Command(job.Key, "admin-1"), CancellationToken.None);

        (await act.Should().ThrowAsync<ScheduledJobRefusal>())
            .WithMessage("Job 'msf-campaign-auto-close' is disabled, so it was not run. Enable it, then run it.")
            .Which.JobKey.Should().Be(job.Key);
        job.Executions.Should().Be(0);
        (await RunsAsync(provider)).Should().BeEmpty("refused before a run row is written");
    }

    [Fact]
    public async Task TheDisabledCheck_IsMadeUnderTheJobsLock_SoARunGoingIsRefusedAsRunning()
    {
        // The dispatcher takes the lock first, then reads the definition (K6): while the scheduler holds the lock, a disabled
        // job and an enabled one alike are refused as already running, and nothing is read or written meanwhile.
        using var provider = BuildProvider();
        var job = new Job("audit-log-retention");
        await StoreAsync(provider, Definition(job.Key, enabled: false));
        var locks = new ScheduledJobLocks();

        using (locks.TryEnter(job.Key) ?? throw new InvalidOperationException("guard: the lock is free"))
        {
            var act = () => Handler(provider, job, locks).Handle(Command(job.Key, "admin-1"), CancellationToken.None);
            (await act.Should().ThrowAsync<ScheduledJobRefusal>()).WithMessage(ScheduledJobRefusal.AlreadyRunningSentence(job.Key));
        }

        // The lock free, the job is still disabled: refused as disabled, with no run row.
        var again = () => Handler(provider, job, locks).Handle(Command(job.Key, "admin-1"), CancellationToken.None);
        (await again.Should().ThrowAsync<ScheduledJobRefusal>()).WithMessage(ScheduledJobRefusal.DisabledSentence(job.Key));
        (await RunsAsync(provider)).Should().BeEmpty();
    }

    [Fact]
    public async Task AJobAlreadyRunning_IsRefused_InTheBuiltWords()
    {
        using var provider = BuildProvider();
        var job = new Job("audit-log-retention");
        await StoreAsync(provider, Definition(job.Key, enabled: true));
        var locks = new ScheduledJobLocks();

        using (locks.TryEnter(job.Key))
        {
            var act = () => Handler(provider, job, locks).Handle(Command(job.Key, "admin-1"), CancellationToken.None);
            (await act.Should().ThrowAsync<ScheduledJobRefusal>()).WithMessage(
                "Job 'audit-log-retention' is already running, so it was not started again. Wait for that run to finish, " +
                "then run it again if it is still needed.");
        }

        job.Executions.Should().Be(0);
        (await RunsAsync(provider)).Should().BeEmpty();
    }

    [Fact]
    public async Task AKeyNotInTheRegistry_IsAFailure_NotARefusal()
    {
        using var provider = BuildProvider();

        var act = () => Handler(provider, new Job("known")).Handle(Command("no-such-job", "admin-1"), CancellationToken.None);

        var thrown = (await act.Should().ThrowAsync<InvalidOperationException>()).Which;
        thrown.Should().NotBeOfType<ScheduledJobRefusal>("the page shows its fixed failure words, the cause logged (T23)");
        (await RunsAsync(provider)).Should().BeEmpty();
    }

    [Fact]
    public async Task APrincipalWithNoId_IsRefused_NeverRunAsUnknown()
    {
        using var provider = BuildProvider();
        var job = new Job("weekly-coordinator-digest");
        await StoreAsync(provider, Definition(job.Key, enabled: true));
        var nobody = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Administrator")], "Test"));

        var act = () => Handler(provider, job).Handle(new RunScheduledJobNowCommand(job.Key, nobody), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        job.Executions.Should().Be(0);
        (await RunsAsync(provider)).Should().BeEmpty();
    }

    /// <summary>The build review's N-3: the handler is the second lock behind the page's Administrator gate.</summary>
    [Fact]
    public async Task SomeoneOtherThanTheAdministrator_IsRefused_AndNothingRuns()
    {
        using var provider = BuildProvider();
        var job = new Job("weekly-coordinator-digest");
        await StoreAsync(provider, Definition(job.Key, enabled: true));
        var coordinator = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "coordinator-1"), new Claim(ClaimTypes.Role, "Coordinator")], "Test"));

        var act = () => Handler(provider, job).Handle(new RunScheduledJobNowCommand(job.Key, coordinator), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        job.Executions.Should().Be(0);
        (await RunsAsync(provider)).Should().BeEmpty();
    }

    [Fact]
    public async Task AJobWithNoStoredDefinitionYet_Runs()
    {
        // The host seeds the definitions as it starts; until then there is no switch to read, so nothing is disabled.
        using var provider = BuildProvider();
        var job = new Job("weekly-coordinator-digest");

        var outcome = await Handler(provider, job).Handle(Command(job.Key, "admin-1"), CancellationToken.None);

        outcome.Status.Should().Be(ScheduledJobRunStatus.Succeeded);
    }

    private static RunScheduledJobNowCommand Command(string key, string userId)
        => new(key, new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId), new Claim(ClaimTypes.Role, "Administrator")], "Test")));

    private static RunScheduledJobNowCommandHandler Handler(ServiceProvider provider, IScheduledJob job, ScheduledJobLocks? locks = null)
    {
        var registry = new ScheduledJobRegistry();
        registry.Register(job);
        var dispatcher = new ScheduledJobDispatcher(
            provider.GetRequiredService<IServiceScopeFactory>(),
            locks ?? new ScheduledJobLocks(),
            NullLogger<ScheduledJobDispatcher>.Instance,
            new FixedClock(Now));
        return new RunScheduledJobNowCommandHandler(registry, dispatcher);
    }

    private static ScheduledJobDefinition Definition(string key, bool enabled)
        => new() { Key = key, CronExpression = "0 * * * *", IsEnabled = enabled, Description = key };

    private static async Task StoreAsync(ServiceProvider provider, ScheduledJobDefinition definition)
    {
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Set<ScheduledJobDefinition>().Add(definition);
        await db.SaveChangesAsync();
    }

    private static async Task<List<ScheduledJobRun>> RunsAsync(ServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
            .Set<ScheduledJobRun>().AsNoTracking().ToListAsync();
    }

    private static ServiceProvider BuildProvider()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(dbName));
        services.AddScoped<IApplicationDbContext>(p => p.GetRequiredService<ApplicationDbContext>());
        return services.BuildServiceProvider();
    }

    private sealed class Job(string key, string? fails = null) : IScheduledJob
    {
        private int _executions;

        public int Executions => Volatile.Read(ref _executions);

        public string Key => key;
        public string CronExpression => "0 * * * *";
        public string Description => "A job for the test.";

        public Task ExecuteAsync(ScheduledJobContext context, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _executions);
            return fails is null ? Task.CompletedTask : throw new InvalidOperationException(fails);
        }
    }

    private sealed class FixedClock(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow);
    }
}
