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
/// <para>
/// Every line it writes about a mail names the mail by its reference and its tags, never by an address, and an exception
/// about a mail is logged with the mail's addresses taken out of its text (<see cref="EmailLog" />, T282). The line that
/// says a mail was sent is written here too, on the attempt that sent it.
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
            await foreach (var mail in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                // The reader goes on handing out what is already queued after a stop, without asking the token.
                if (stoppingToken.IsCancellationRequested)
                {
                    await AbandonAsync(mail);
                    break;
                }

                var outcome = await SendWithRetryAsync(mail, stoppingToken);
                if (outcome is not null)
                {
                    await ReportAsync(mail, outcome);
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
    private async Task AbandonAsync(QueuedEmail mail)
    {
        _logger.LogWarning(
            "Email {Reference} (tags: {Tags}) abandoned unsent: the app is shutting down.",
            mail.Reference, EmailLog.DescribeTags(mail.Message));
        await ReportAsync(mail, EmailDeliveryOutcome.Dropped(0, UtcNow));
    }

    /// <summary>
    /// Sends one mail, retrying, and answers what became of it: sent, or dropped; or null when the host stopped with the
    /// mail in flight, whose fate is not known. (T251)
    /// </summary>
    private async Task<EmailDeliveryOutcome?> SendWithRetryAsync(QueuedEmail mail, CancellationToken stoppingToken)
    {
        var message = mail.Message;
        for (var attempt = 1; ; attempt++)
        {
            Exception? failed = null;
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISmtpSender>();
                await sender.SendAsync(message, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // App is shutting down — don't retry. Cut off mid-send, the mail may have reached the mail server, so its
                // fate is not known and it is not reported (T251 review): a link reported dropped is let go when it is
                // replaced, and this one may be in its respondent's hands.
                _logger.LogWarning(
                    "Email {Reference} (tags: {Tags}) cut off by the app's shutdown; whether it was sent is not known.",
                    mail.Reference, EmailLog.DescribeTags(message));
                return null;
            }
            catch (Exception ex)
            {
                failed = ex;
            }

            // Each outcome is logged outside the send's try: a logger that threw in there would count a sent mail as a
            // failed attempt, and send it again.
            if (failed is null)
            {
                _logger.LogInformation(
                    "Email {Reference} (tags: {Tags}) sent on attempt {Attempt}.",
                    mail.Reference, EmailLog.DescribeTags(message), attempt);
                return EmailDeliveryOutcome.Delivered(attempt, UtcNow);
            }

            // The server's reply, which MailKit makes the message, usually quotes the address it refused.
            var failure = EmailLog.Redact(failed, message);
            if (attempt == MaxRetries)
            {
                _logger.LogError(failure,
                    "Email {Reference} (tags: {Tags}) failed after {MaxRetries} attempts. Message dropped.",
                    mail.Reference, EmailLog.DescribeTags(message), MaxRetries);
                return EmailDeliveryOutcome.Dropped(attempt, UtcNow);
            }

            var delay = _retryDelay(attempt);
            _logger.LogWarning(failure,
                "Email {Reference} (tags: {Tags}) failed (attempt {Attempt}/{MaxRetries}). Retrying in {Delay}s.",
                mail.Reference, EmailLog.DescribeTags(message), attempt, MaxRetries, delay.TotalSeconds);

            try
            {
                await Task.Delay(delay, _timeProvider, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Until T251 a stop during the wait between attempts ended the worker with this mail neither sent nor
                // logged. Its last attempt failed, so it is reported dropped, as after the last retry.
                _logger.LogWarning(
                    "Email {Reference} (tags: {Tags}) abandoned after attempt {Attempt}: the app is shutting down.",
                    mail.Reference, EmailLog.DescribeTags(message), attempt);
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
    private async Task ReportAsync(QueuedEmail mail, EmailDeliveryOutcome outcome)
    {
        var message = mail.Message;
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
            _logger.LogError(EmailLog.Redact(ex, message),
                "The outcome of email {Reference} (tags: {Tags}, sent: {Sent}) could not be recorded.",
                mail.Reference, EmailLog.DescribeTags(message), outcome.Sent);
        }
    }

    private DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;
}
