using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Email;
using Wombat.Infrastructure.Email;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Email;

/// <summary>
/// The mail worker records what became of every mail in <c>EmailOutcomes</c>, for Home's Mail line (T400, flow 18, Q18,
/// K3): sent; dropped after its last attempt with the server's refusal, redacted; dropped with no refusal when abandoned
/// as the host stops or after a failed attempt while it waited to retry. The mail in flight records nothing, as it reports
/// nothing (T251 review). A recording that fails is logged and never stops the next mail.
/// </summary>
public sealed class EmailWorkerOutcomeTests
{
    private static readonly DateTime Now = new(2026, 11, 10, 19, 39, 0, DateTimeKind.Utc);

    [Fact]
    public async Task AMailSent_IsRecordedSent_AskingForNoReport()
    {
        var (worker, queue, outcomes, _) = Build(new ScriptedSmtp(failures: 1));

        // No delivery key: never reported to an observer (ReportAsync returns early), and recorded all the same.
        await queue.Writer.WriteAsync(new QueuedEmail(Mail("nurse-1@example.test")));
        queue.Writer.Complete();
        await worker.ExecutePublicAsync(FiveSeconds());

        (await outcomes()).Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { OccurredAt = Now, Kind = EmailOutcomeKind.Sent, Refusal = (string?)null });
    }

    [Fact]
    public async Task AMailTheServerNeverTakes_IsRecordedDropped_WithItsRefusalRedacted()
    {
        var smtp = new ScriptedSmtp(failures: int.MaxValue,
            refusal: "550 5.1.1 <nurse-1@example.test>: Recipient address rejected");
        var (worker, queue, outcomes, _) = Build(smtp);

        await queue.Writer.WriteAsync(new QueuedEmail(Mail("nurse-1@example.test")));
        queue.Writer.Complete();
        await worker.ExecutePublicAsync(FiveSeconds());

        var dropped = (await outcomes()).Should().ContainSingle().Which;
        dropped.Kind.Should().Be(EmailOutcomeKind.Dropped);
        dropped.OccurredAt.Should().Be(Now);
        dropped.Refusal.Should().Be("550 5.1.1 <[recipient]>: Recipient address rejected");
        dropped.Refusal.Should().NotContain("nurse-1", "no address is ever stored (Q18)");
    }

    [Fact]
    public async Task EachMail_IsRecordedOnce_InTheOrderTheWorkerFinishedWithThem()
    {
        var (worker, queue, outcomes, _) = Build(new ScriptedSmtp(failures: 0, failFor: "nurse-2@example.test"));

        await queue.Writer.WriteAsync(new QueuedEmail(Mail("nurse-1@example.test")));
        await queue.Writer.WriteAsync(new QueuedEmail(Mail("nurse-2@example.test")));
        await queue.Writer.WriteAsync(new QueuedEmail(Mail("nurse-3@example.test")));
        queue.Writer.Complete();
        await worker.ExecutePublicAsync(FiveSeconds());

        (await outcomes()).Select(outcome => outcome.Kind).Should().Equal(
            EmailOutcomeKind.Sent, EmailOutcomeKind.Dropped, EmailOutcomeKind.Sent);
    }

    /// <summary>K3: every mail still queued as the host stops is recorded dropped, with no refusal; the one in flight not.</summary>
    [Fact]
    public async Task WhenTheHostStops_EachQueuedMailIsRecordedDroppedWithNoRefusal_AndTheMailInFlightRecordsNothing()
    {
        var smtp = new ScriptedSmtp(failures: 0, hangs: true);
        var (worker, queue, outcomes, _) = Build(smtp);
        using var stopping = new CancellationTokenSource();

        await queue.Writer.WriteAsync(new QueuedEmail(Mail("nurse-1@example.test")));
        await queue.Writer.WriteAsync(new QueuedEmail(Mail("nurse-2@example.test")));
        await queue.Writer.WriteAsync(new QueuedEmail(Mail("nurse-3@example.test")));

        var running = worker.ExecutePublicAsync(stopping.Token);
        await smtp.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await stopping.CancelAsync();
        await running.WaitAsync(TimeSpan.FromSeconds(5));

        (await outcomes()).Should().HaveCount(2).And.OnlyContain(outcome =>
            outcome.Kind == EmailOutcomeKind.Dropped && outcome.Refusal == null && outcome.OccurredAt == Now);
    }

    /// <summary>K3: a stop while the worker waits to retry: its attempt failed, so it is dropped, with no refusal.</summary>
    [Fact]
    public async Task WhenTheHostStopsBetweenAttempts_TheMailIsRecordedDroppedWithNoRefusal()
    {
        using var stopping = new CancellationTokenSource();
        var (worker, queue, outcomes, _) = Build(new ScriptedSmtp(failures: int.MaxValue), retryDelay: _ =>
        {
            stopping.Cancel();
            return TimeSpan.FromMinutes(5);
        });

        await queue.Writer.WriteAsync(new QueuedEmail(Mail("nurse-1@example.test")));
        await worker.ExecutePublicAsync(stopping.Token).WaitAsync(TimeSpan.FromSeconds(5));

        (await outcomes()).Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { Kind = EmailOutcomeKind.Dropped, Refusal = (string?)null });
    }

    [Fact]
    public async Task ARecordingThatFails_IsLogged_AndTheNextMailIsSent()
    {
        var smtp = new ScriptedSmtp(failures: 0);
        var logger = new ListLogger();
        var database = new Mock<IApplicationDbContext>();
        database.Setup(db => db.Set<EmailOutcome>()).Throws(new InvalidOperationException("The database did not answer."));

        var services = new ServiceCollection();
        services.AddSingleton<ISmtpSender>(smtp);
        services.AddSingleton(database.Object);
        services.AddScoped<EmailOutcomeRecorder>();
        var provider = services.BuildServiceProvider();
        var queue = new EmailQueue();
        var worker = new EmailWorker(queue, provider.GetRequiredService<IServiceScopeFactory>(), logger, _ => TimeSpan.Zero,
            new FixedClock(Now));

        await queue.Writer.WriteAsync(new QueuedEmail(Mail("nurse-1@example.test")));
        await queue.Writer.WriteAsync(new QueuedEmail(Mail("nurse-2@example.test")));
        queue.Writer.Complete();
        await worker.ExecutePublicAsync(FiveSeconds());

        smtp.Sent.Should().Equal("nurse-1@example.test", "nurse-2@example.test");
        logger.Errors.Should().HaveCount(2).And.OnlyContain(line => line.Contains("could not be recorded for Home"));
        logger.Errors.Should().NotContain(line => line.Contains("nurse-"), "the log names a mail by its reference, never its address");
    }

    /// <summary>A host with no recorder registered (no mail server) records nothing, and says nothing of it.</summary>
    [Fact]
    public async Task WithNoRecorderRegistered_NothingIsRecorded_AndNothingIsLogged()
    {
        var smtp = new ScriptedSmtp(failures: 0);
        var logger = new ListLogger();
        var services = new ServiceCollection();
        services.AddSingleton<ISmtpSender>(smtp);
        var provider = services.BuildServiceProvider();
        var queue = new EmailQueue();
        var worker = new EmailWorker(queue, provider.GetRequiredService<IServiceScopeFactory>(), logger, _ => TimeSpan.Zero,
            new FixedClock(Now));

        await queue.Writer.WriteAsync(new QueuedEmail(Mail("nurse-1@example.test")));
        queue.Writer.Complete();
        await worker.ExecutePublicAsync(FiveSeconds());

        smtp.Sent.Should().ContainSingle();
        logger.Errors.Should().BeEmpty();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static EmailMessage Mail(string to) => new(to, "Digest", "<p/>", "T", Tags: ["digest"]);

    private static CancellationToken FiveSeconds() => new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;

    private static (EmailWorker Worker, EmailQueue Queue, Func<Task<List<EmailOutcome>>> Outcomes, ListLogger Logger) Build(
        ISmtpSender smtp, Func<int, TimeSpan>? retryDelay = null)
    {
        var databaseName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddSingleton(smtp);
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(databaseName));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<EmailOutcomeRecorder>();
        var provider = services.BuildServiceProvider();
        var queue = new EmailQueue();
        var logger = new ListLogger();

        var worker = new EmailWorker(queue, provider.GetRequiredService<IServiceScopeFactory>(), logger,
            retryDelay ?? (_ => TimeSpan.Zero), new FixedClock(Now));

        async Task<List<EmailOutcome>> Read()
        {
            await using var scope = provider.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Set<EmailOutcome>()
                .AsNoTracking().OrderBy(outcome => outcome.Id).ToListAsync();
        }

        return (worker, queue, Read, logger);
    }

    /// <summary>Fails the first <c>failures</c> attempts (or every attempt to <c>failFor</c>); or, hanging, waits for the stop.</summary>
    private sealed class ScriptedSmtp(int failures, bool hangs = false, string? failFor = null, string? refusal = null)
        : ISmtpSender
    {
        public int Attempts { get; private set; }

        public List<string> Sent { get; } = [];

        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
        {
            Attempts++;
            Entered.TrySetResult();

            if (hangs)
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }

            if (Attempts <= failures || message.To == failFor)
            {
                throw new InvalidOperationException(refusal ?? "The mail server could not be reached.");
            }

            Sent.Add(message.To);
        }
    }

    private sealed class ListLogger : ILogger<EmailWorker>
    {
        public List<string> Errors { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (logLevel >= LogLevel.Error)
            {
                Errors.Add($"{formatter(state, exception)} {exception}");
            }
        }
    }

    private sealed class FixedClock(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow);
    }
}
