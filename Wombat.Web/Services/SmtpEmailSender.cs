/*Copyright (C) 2024 RCL Consulting
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>
 */

using Azure;
using Azure.Communication.Email;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using Wombat.Web.Services;

namespace Wombat.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<EmailSettings> settings, ILogger<SmtpEmailSender> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string recipient, string subject, string htmlMessage)
        {
            if (!_settings.UseSMTP)
            {
                _logger.LogWarning("SMTP disabled in settings; email to {Recipient} skipped.", recipient);
                return;
            }

            using var msg = new MailMessage
            {
                From = new MailAddress(_settings.Email),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            msg.To.Add(recipient);

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                // IMPORTANT for cPanel
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_settings.Email, _settings.Password),
                EnableSsl = _settings.EnableSSL,               // 465 (implicit TLS) or 587 (STARTTLS)
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 20000
            };

            try
            {
                await client.SendMailAsync(msg);
                _logger.LogInformation("Email sent to {Recipient}", recipient);
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP send failed to {Recipient} via {Host}:{Port}", recipient, _settings.Host, _settings.Port);
                throw; // bubble up during setup so you see a clear 500 + log
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email send failed (non-SMTP) to {Recipient}", recipient);
                throw;
            }
        }
    }
}
