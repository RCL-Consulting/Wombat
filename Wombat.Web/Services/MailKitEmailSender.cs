using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Wombat.Web.Services
{
    public class MailKitEmailSender : IEmailSender
    {
        private readonly EmailSettings _s;
        private readonly ILogger<MailKitEmailSender> _log;
        public MailKitEmailSender(IOptions<EmailSettings> s, ILogger<MailKitEmailSender> log)
        { _s = s.Value; _log = log; }

        public async Task SendEmailAsync(string recipient, string subject, string html)
        {
            var msg = new MimeMessage();
            msg.From.Add(MailboxAddress.Parse(_s.Email));
            msg.To.Add(MailboxAddress.Parse(recipient));
            msg.Subject = subject;
            msg.Body = new BodyBuilder { HtmlBody = html }.ToMessageBody();

            using var client = new SmtpClient();
            client.Timeout = 10000; // 10s so the request won't “hang”

            // Decide TLS mode from settings/port
            SecureSocketOptions tls =
                _s.Port == 465 ? SecureSocketOptions.SslOnConnect :
                (_s.Port == 587 && _s.EnableSSL) ? SecureSocketOptions.StartTls :
                SecureSocketOptions.None; // e.g., Papercut on 25

            try
            {
                // 465 implicit TLS works cleanly on your host
                await client.ConnectAsync(_s.Host, _s.Port, tls, CancellationToken.None);
                if (!string.IsNullOrEmpty(_s.Password))
                    await client.AuthenticateAsync(_s.Email, _s.Password, CancellationToken.None);
                await client.SendAsync(msg);
                await client.DisconnectAsync(true);
                _log.LogInformation("Email sent to {Recipient}", recipient);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "MailKit send failed to {Recipient} via {Host}:{Port}", recipient, _s.Host, _s.Port);
                throw; // keep bubbling during setup so you see the error
            }
        }
    }
}
