using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Infrastructure.Email;

/// <summary>
/// Development/test fallback — logs emails instead of sending them.
/// Registered when Email:SmtpHost is not configured.
/// </summary>
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
