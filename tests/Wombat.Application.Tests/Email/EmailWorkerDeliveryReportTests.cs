using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Wombat.Application.Common.Email;
using Wombat.Infrastructure.Email;

namespace Wombat.Application.Tests.Email;

/// <summary>
/// The mail worker reports what became of each mail that asks for it: sent, on which attempt, or dropped. (T251)
/// </summary>
/// <remarks>
/// Until T251 a mail the worker gave up on was logged and dropped, and nothing else learned of it: a campaign opened while
/// the mail server was down told its coordinator that every respondent had been emailed. The queue lives in the web
/// process, so what it holds when the host stops is never sent either, and is reported dropped as the host stops. The mail
/// in flight at that moment is not reported: it may have reached the mail server (T251 review).
/// </remarks>
public sealed class EmailWorkerDeliveryReportTests
{
    private static readonly DateTime Now = new(2029, 3, 7, 9, 30, 0, DateTimeKind.Utc);

    [Fact]
    public async Task AMailSentOnItsSecondAttempt_IsReportedSent_AfterTwoAttempts()
    {
        var smtp = new ScriptedSmtp(failures: 1);
        var observer = new RecordingObserver();
        var (worker, queue) = Build(smtp, observer);

        await queue.Writer.WriteAsync(new QueuedEmail(Keyed("nurse-1@example.test", "msf-link:1:first")));
        queue.Writer.Complete();
        await worker.ExecutePublicAsync(FiveSeconds());

        observer.Reports.Should().ContainSingle()
            .Which.Should().Be(("msf-link:1:first", new EmailDeliveryOutcome(true, 2, Now)));
    }

    [Fact]
    public async Task AMailTheServerNeverTakes_IsReportedDropped_AfterEveryAttempt()
    {
        var smtp = new ScriptedSmtp(failures: int.MaxValue);
        var observer = new RecordingObserver();
        var (worker, queue) = Build(smtp, observer);

        await queue.Writer.WriteAsync(new QueuedEmail(Keyed("nurse-1@example.test", "msf-link:1:first")));
        queue.Writer.Complete();
        await worker.ExecutePublicAsync(FiveSeconds());

        smtp.Attempts.Should().Be(EmailWorker.MaxRetries);
        observer.Reports.Should().ContainSingle()
            .Which.Should().Be(("msf-link:1:first", new EmailDeliveryOutcome(false, EmailWorker.MaxRetries, Now)));
    }

    [Fact]
    public async Task AMailThatAsksForNoReport_IsNotReported()
    {
        var observer = new RecordingObserver();
        var (worker, queue) = Build(new ScriptedSmtp(failures: int.MaxValue), observer);

        await queue.Writer.WriteAsync(new QueuedEmail(new EmailMessage("a@b.test", "Digest", "<p/>", "T", Tags: ["digest"])));
        await queue.Writer.WriteAsync(new QueuedEmail(new EmailMessage("c@d.test", "Digest", "<p/>", "T", Tags: ["digest"])));
        queue.Writer.Complete();
        await worker.ExecutePublicAsync(FiveSeconds());

        observer.Reports.Should().BeEmpty("only a mail that carries a delivery key asks what became of it");
    }

    /// <summary>
    /// Every mail still queued is reported dropped as the host stops: none will be sent, and a link each carried can then
    /// be sent again at once rather than an hour later. The mail in flight is not reported (T251 review): cut off mid-send,
    /// it may have reached the mail server, and a link reported dropped is let go when it is replaced, which would retire a
    /// link its respondent may be answering through. Unreported, it is kept as the previous link when it is resent.
    /// </summary>
    [Fact]
    public async Task WhenTheHostStops_EveryMailStillQueuedIsReportedDropped_AndTheMailInFlightIsNotReported()
    {
        var smtp = new ScriptedSmtp(failures: 0, hangs: true);
        var observer = new RecordingObserver();
        var (worker, queue) = Build(smtp, observer);
        using var stopping = new CancellationTokenSource();

        await queue.Writer.WriteAsync(new QueuedEmail(Keyed("nurse-1@example.test", "msf-link:1:first")));
        await queue.Writer.WriteAsync(new QueuedEmail(Keyed("nurse-2@example.test", "msf-link:2:second")));
        await queue.Writer.WriteAsync(new QueuedEmail(Keyed("nurse-3@example.test", "msf-link:3:third")));

        var running = worker.ExecutePublicAsync(stopping.Token);
        await smtp.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await stopping.CancelAsync();
        await running.WaitAsync(TimeSpan.FromSeconds(5));

        observer.Reports.Should().Equal(
            ("msf-link:2:second", new EmailDeliveryOutcome(false, 0, Now)),
            ("msf-link:3:third", new EmailDeliveryOutcome(false, 0, Now)));
        smtp.Attempts.Should().Be(1, "nothing is sent once the host is stopping");
    }

    /// <summary>
    /// A stop while the worker waits to retry: the attempt made has failed, as surely as the last retry's, so the mail is
    /// reported dropped.
    /// </summary>
    [Fact]
    public async Task WhenTheHostStopsBetweenAttempts_TheMailIsReportedDropped_NotLostSilently()
    {
        var smtp = new ScriptedSmtp(failures: int.MaxValue);
        var observer = new RecordingObserver();
        using var stopping = new CancellationTokenSource();
        var (worker, queue) = Build(smtp, observer, retryDelay: _ =>
        {
            // The host stops while the worker waits to retry.
            stopping.Cancel();
            return TimeSpan.FromMinutes(5);
        });

        await queue.Writer.WriteAsync(new QueuedEmail(Keyed("nurse-1@example.test", "msf-link:1:first")));
        await worker.ExecutePublicAsync(stopping.Token).WaitAsync(TimeSpan.FromSeconds(5));

        observer.Reports.Should().ContainSingle()
            .Which.Should().Be(("msf-link:1:first", new EmailDeliveryOutcome(false, 1, Now)));
    }

    [Fact]
    public async Task AReportThatFails_StopsNothing_AndTheNextMailIsSentAndReported()
    {
        var smtp = new ScriptedSmtp(failures: 0);
        var observer = new RecordingObserver { ThrowFor = "msf-link:1:first" };
        var (worker, queue) = Build(smtp, observer);

        await queue.Writer.WriteAsync(new QueuedEmail(Keyed("nurse-1@example.test", "msf-link:1:first")));
        await queue.Writer.WriteAsync(new QueuedEmail(Keyed("nurse-2@example.test", "msf-link:2:second")));
        queue.Writer.Complete();
        await worker.ExecutePublicAsync(FiveSeconds());

        smtp.Sent.Should().Equal("nurse-1@example.test", "nurse-2@example.test");
        observer.Reports.Should().Equal(
            ("msf-link:1:first", new EmailDeliveryOutcome(true, 1, Now)),
            ("msf-link:2:second", new EmailDeliveryOutcome(true, 1, Now)));
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static EmailMessage Keyed(string to, string key)
        => new(to, "Feedback request", "<p/>", "T", Tags: ["msf-invite", "campaign:1"], DeliveryKey: key);

    private static CancellationToken FiveSeconds() => new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;

    private static (EmailWorker Worker, EmailQueue Queue) Build(
        ISmtpSender smtp, IEmailDeliveryObserver observer, Func<int, TimeSpan>? retryDelay = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton(smtp);
        services.AddSingleton(observer);
        var provider = services.BuildServiceProvider();
        var queue = new EmailQueue();

        var worker = new EmailWorker(
            queue,
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<EmailWorker>.Instance,
            retryDelay ?? (_ => TimeSpan.Zero),
            new FixedClock(Now));

        return (worker, queue);
    }

    /// <summary>Fails the first <c>failures</c> attempts; or, hanging, waits until the host stops.</summary>
    private sealed class ScriptedSmtp(int failures, bool hangs = false) : ISmtpSender
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

            if (Attempts <= failures)
            {
                throw new InvalidOperationException("The mail server could not be reached.");
            }

            Sent.Add(message.To);
        }
    }

    private sealed class RecordingObserver : IEmailDeliveryObserver
    {
        public List<(string? Key, EmailDeliveryOutcome Outcome)> Reports { get; } = [];

        public string? ThrowFor { get; init; }

        public Task RecordAsync(EmailMessage message, EmailDeliveryOutcome outcome, CancellationToken cancellationToken)
        {
            Reports.Add((message.DeliveryKey, outcome));
            return message.DeliveryKey == ThrowFor
                ? Task.FromException(new InvalidOperationException("The database is down."))
                : Task.CompletedTask;
        }
    }

    private sealed class FixedClock(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow);
    }
}
