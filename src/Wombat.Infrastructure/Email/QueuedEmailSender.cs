using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Infrastructure.Email;

/// <summary>
/// The IEmailSender implementation injected into Application code.
/// Writes the message to the in-process channel and returns immediately;
/// actual SMTP delivery is handled by <see cref="EmailWorker"/>.
/// </summary>
public sealed class QueuedEmailSender : IEmailSender
{
    private readonly EmailQueue _queue;
    private readonly ILogger<QueuedEmailSender> _logger;

    public QueuedEmailSender(EmailQueue queue, ILogger<QueuedEmailSender> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    public ValueTask SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Enqueuing email to {To} — subject: {Subject} — tags: {Tags}",
            message.To,
            message.Subject,
            message.Tags is { Count: > 0 } ? string.Join(", ", message.Tags) : "(none)");

        return _queue.Writer.WriteAsync(message, cancellationToken);
    }

    Task IEmailSender.SendAsync(EmailMessage message, CancellationToken cancellationToken)
        => SendAsync(message, cancellationToken).AsTask();
}
