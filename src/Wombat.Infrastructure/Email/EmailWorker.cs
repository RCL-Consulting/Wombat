using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email;

namespace Wombat.Infrastructure.Email;

/// <summary>
/// Delivers what <see cref="QueuedEmailSender" /> queued, retrying each mail up to <see cref="MaxRetries" /> times, and
/// reports each outcome to the <see cref="IEmailDeliveryObserver" />s of a mail that asks for it (T251).
/// </summary>
/// <remarks>
/// <para>
/// A mail is reported sent on the attempt that succeeded, or dropped once the last attempt has failed. Until T251 a
/// dropped mail was only logged, so a campaign opened while the mail server was down told its coordinator that every
/// respondent had been emailed, and none of them could ever answer.
/// </para>
/// <para>
/// The queue lives in this process, so what it holds when the host stops is never sent. Each such mail is reported
/// dropped as the host stops, so that its link reads as not delivered at once rather than after
/// <c>MsfInvitation.DeliveryReportDeadline</c>, and so is a mail whose last attempt failed while the worker waited to
/// retry it. The one in flight is not reported (T251 review): cut off mid-send, it may have reached the mail server, and
/// a link reported dropped is let go when a new one replaces it (<c>MsfInvitation.ReplaceLink</c>), which would retire a
/// link its respondent may be answering through. Unreported, it counts as not delivered once the deadline passes and is
/// kept as the previous link when resent. A host that crashes reports nothing, and the deadline covers that too.
/// </para>
/// </remarks>
public sealed class EmailWorker : BackgroundService
{
    internal const int MaxRetries = 3;

    private readonly EmailQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailWorker> _logger;
    private readonly Func<int, TimeSpan> _retryDelay;
    private readonly TimeProvider _timeProvider;

    public EmailWorker(
        EmailQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<EmailWorker> logger,
        TimeProvider timeProvider)
        : this(queue, scopeFactory, logger, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)), timeProvider)
    {
    }

    // Constructor used by unit tests to inject a zero-delay strategy.
    internal EmailWorker(
        EmailQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<EmailWorker> logger,
        Func<int, TimeSpan> retryDelay,
        TimeProvider? timeProvider = null)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _retryDelay = retryDelay;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    // Exposed for unit tests — production code uses the BackgroundService host lifecycle.
    internal Task ExecutePublicAsync(CancellationToken ct) => ExecuteAsync(ct);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var message in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                // The reader goes on handing out what is already queued after a stop, without asking the token.
                if (stoppingToken.IsCancellationRequested)
                {
                    await AbandonAsync(message);
                    break;
                }

                var outcome = await SendWithRetryAsync(message, stoppingToken);
                if (outcome is not null)
                {
                    await ReportAsync(message, outcome);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // The host is stopping; what is still queued is reported below.
        }

        // Nothing still queued will be sent: the queue goes with the process. Each mail is reported dropped, so that a
        // link it carried can be sent again at once (T251). Bounded by the host's shutdown timeout, like any stop.
        while (_queue.Reader.TryRead(out var abandoned))
        {
            await AbandonAsync(abandoned);
        }
    }

    /// <summary>A mail still queued as the host stops: never attempted, and reported dropped. (T251)</summary>
    private async Task AbandonAsync(EmailMessage message)
    {
        _logger.LogWarning(
            "Email (subject: {Subject}, tags: {Tags}) abandoned unsent: the app is shutting down.",
            message.Subject, DescribeTags(message));
        await ReportAsync(message, EmailDeliveryOutcome.Dropped(0, UtcNow));
    }

    /// <summary>
    /// Sends one mail, retrying, and answers what became of it: sent, or dropped; or null when the host stopped with the
    /// mail in flight, whose fate is not known. (T251)
    /// </summary>
    private async Task<EmailDeliveryOutcome?> SendWithRetryAsync(EmailMessage message, CancellationToken stoppingToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            TimeSpan delay;
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISmtpSender>();
                await sender.SendAsync(message, stoppingToken);
                return EmailDeliveryOutcome.Delivered(attempt, UtcNow);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // App is shutting down — don't retry. Cut off mid-send, the mail may have reached the mail server, so its
                // fate is not known and it is not reported (T251 review): a link reported dropped is let go when it is
                // replaced, and this one may be in its respondent's hands.
                _logger.LogWarning(
                    "Email to {To} (subject: {Subject}) cut off by the app's shutdown; whether it was sent is not known.",
                    message.To, message.Subject);
                return null;
            }
            catch (Exception ex)
            {
                if (attempt == MaxRetries)
                {
                    _logger.LogError(ex,
                        "Email to {To} (subject: {Subject}, tags: {Tags}) failed after {MaxRetries} attempts. Message dropped.",
                        message.To, message.Subject, DescribeTags(message), MaxRetries);
                    return EmailDeliveryOutcome.Dropped(attempt, UtcNow);
                }

                delay = _retryDelay(attempt);
                _logger.LogWarning(ex,
                    "Email to {To} failed (attempt {Attempt}/{MaxRetries}). Retrying in {Delay}s.",
                    message.To, attempt, MaxRetries, delay.TotalSeconds);
            }

            try
            {
                await Task.Delay(delay, _timeProvider, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Until T251 a stop during the wait between attempts ended the worker with this mail neither sent nor
                // logged. Its last attempt failed, so it is reported dropped, as after the last retry.
                _logger.LogWarning(
                    "Email to {To} (subject: {Subject}) abandoned — app shutdown.",
                    message.To, message.Subject);
                return EmailDeliveryOutcome.Dropped(attempt, UtcNow);
            }
        }
    }

    /// <summary>
    /// Hands a mail's outcome to every observer, when the mail asks for it (<see cref="EmailMessage.DeliveryKey" />).
    /// Never throws: a record that fails is logged, and the next mail is sent. (T251)
    /// </summary>
    /// <remarks>
    /// Not cancellable: the outcome is already known, and it is reported as the host stops too. The log names neither the
    /// recipient nor the key, which together would say whose invitation it was (<c>MsfInvitation.DeliveryKey</c>).
    /// </remarks>
    private async Task ReportAsync(EmailMessage message, EmailDeliveryOutcome outcome)
    {
        if (message.DeliveryKey is null)
        {
            return;
        }

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            foreach (var observer in scope.ServiceProvider.GetServices<IEmailDeliveryObserver>())
            {
                await observer.RecordAsync(message, outcome, CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "The outcome of an email (subject: {Subject}, tags: {Tags}, sent: {Sent}) could not be recorded.",
                message.Subject, DescribeTags(message), outcome.Sent);
        }
    }

    private DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;

    private static string DescribeTags(EmailMessage message)
        => message.Tags is { Count: > 0 } ? string.Join(", ", message.Tags) : "(none)";
}
