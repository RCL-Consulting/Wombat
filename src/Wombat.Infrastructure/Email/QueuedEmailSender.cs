using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Infrastructure.Email;

/// <summary>
/// The IEmailSender implementation injected into Application code.
/// Writes the message to the in-process channel and returns immediately;
/// actual SMTP delivery is handled by <see cref="EmailWorker"/>.
/// </summary>
/// <remarks>
/// Each mail is queued under a new reference, which this line and every line the worker writes about it print in place of
/// its address (<see cref="EmailLog" />, T282).
/// </remarks>
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
        var mail = new QueuedEmail(message);

        _logger.LogDebug(
            "Email {Reference} (tags: {Tags}) queued.",
            mail.Reference,
            EmailLog.DescribeTags(message));

        return _queue.Writer.WriteAsync(mail, cancellationToken);
    }

    Task IEmailSender.SendAsync(EmailMessage message, CancellationToken cancellationToken)
        => SendAsync(message, cancellationToken).AsTask();
}
