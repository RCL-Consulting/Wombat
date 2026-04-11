using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Infrastructure.Email;

public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Stub email queued to {ToEmail}. Subject: {Subject}. Body:{NewLine}{Body}",
            toEmail,
            subject,
            Environment.NewLine,
            body);

        return Task.CompletedTask;
    }
}
