using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Infrastructure.Email;

/// <summary>
/// Development/test fallback — logs emails instead of sending them.
/// Registered when Email:SmtpHost is not configured.
/// </summary>
/// <remarks>
/// Reports nothing to the <see cref="IEmailDeliveryObserver" />s (T251): no mail worker runs, and nobody is mailed. So an
/// MSF link sent through it reads as still being sent for an hour, then as not delivered, which is true
/// (INFRASTRUCTURE.md § After T251).
/// </remarks>
public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Stub email to {To} — subject: {Subject} — tags: {Tags}{NewLine}{TextBody}",
            message.To,
            message.Subject,
            message.Tags is { Count: > 0 } ? string.Join(", ", message.Tags) : "(none)",
            Environment.NewLine,
            message.TextBody);

        return Task.CompletedTask;
    }
}
