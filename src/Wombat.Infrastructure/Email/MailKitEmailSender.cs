using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Wombat.Application.Common.Email;

namespace Wombat.Infrastructure.Email;

public sealed class MailKitEmailSender : ISmtpSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<MailKitEmailSender> _logger;

    public MailKitEmailSender(IOptions<EmailSettings> settings, ILogger<MailKitEmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        var mimeMessage = BuildMimeMessage(message);

        using var client = new SmtpClient();
        client.Timeout = _settings.TimeoutSeconds * 1000;

        var socketOptions = _settings.UseSsl
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTlsWhenAvailable;

        await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, socketOptions, cancellationToken);

        if (!string.IsNullOrWhiteSpace(_settings.SmtpUser))
        {
            await client.AuthenticateAsync(_settings.SmtpUser, _settings.SmtpPassword, cancellationToken);
        }

        await client.SendAsync(mimeMessage, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);

        _logger.LogInformation(
            "Email sent to {To} — subject: {Subject} — tags: {Tags}",
            message.To,
            message.Subject,
            message.Tags is { Count: > 0 } ? string.Join(", ", message.Tags) : "(none)");
    }

    private MimeMessage BuildMimeMessage(EmailMessage message)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        mime.To.Add(MailboxAddress.Parse(message.To));

        if (!string.IsNullOrWhiteSpace(message.Cc))
        {
            mime.Cc.Add(MailboxAddress.Parse(message.Cc));
        }

        mime.Subject = message.Subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = message.HtmlBody,
            TextBody = message.TextBody
        };

        mime.Body = bodyBuilder.ToMessageBody();
        return mime;
    }
}
