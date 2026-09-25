using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Infrastructure.Email;

/// <summary>
/// Development/test fallback — logs emails instead of sending them.
/// Registered when Email:SmtpHost is not configured.
/// </summary>
/// <remarks>
/// <para>
/// Reports nothing to the <see cref="IEmailDeliveryObserver" />s (T251): no mail worker runs, and nobody is mailed. So an
/// MSF link sent through it reads as still being sent for an hour, then as not delivered, which is true
/// (INFRASTRUCTURE.md § After T251).
/// </para>
/// <para>
/// Names the mail by a new reference, not its address, as every other line about a mail does (<see cref="EmailLog" />,
/// T282). It prints the subject and the text, which is what it is for: a developer follows the link in it.
/// </para>
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
            "Stub email {Reference} (tags: {Tags}) not sent: Email:SmtpHost is not configured. Subject: {Subject}{NewLine}{TextBody}",
            EmailLog.NewReference(),
            EmailLog.DescribeTags(message),
            message.Subject,
            Environment.NewLine,
            message.TextBody);

        return Task.CompletedTask;
    }
}
