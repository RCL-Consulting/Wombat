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

using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using Azure;
using Azure.Communication.Email;

namespace Wombat.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<EmailSender> logger;

        private EmailSettings emailSettings;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            this.configuration = configuration;
            this.logger = logger;

            emailSettings = new EmailSettings();
            configuration.GetSection("EmailSettings").Bind(emailSettings);
        }

        public async Task SendEmailAsync(string recipient, string subject, string htmlContent)
        {
            try
            {
                if (emailSettings is null)
                {
                    await SendViaAzure(recipient, subject, htmlContent);
                    return;
                }

                if (emailSettings.UseSMTP)
                {
                    SendViaSmtp(recipient, subject, htmlContent);
                }
                else
                {
                    await SendViaAzure(recipient, subject, htmlContent);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending email");
            }
        }

        private void SendViaSmtp(string recipient, string subject, string htmlContent)
        {
            try
            {
                var message = new MailMessage
                {
                    From = new MailAddress(emailSettings.Email),
                    Subject = subject,
                    Body = htmlContent,
                    IsBodyHtml = true
                };

                message.To.Add(new MailAddress(recipient));

                using (var client = new SmtpClient(emailSettings.Host, emailSettings.Port))
                {
                    if (!string.IsNullOrEmpty(emailSettings.Password))
                        client.Credentials = new NetworkCredential(emailSettings.Email, emailSettings.Password);

                    client.Send(message);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending email via SMTP");
            }
        }

        private async Task SendViaAzure(string recipient, string subject, string htmlContent)
        {
            string connectionString = Environment.GetEnvironmentVariable("COMMUNICATION_SERVICES_CONNECTION_STRING");
            EmailClient emailClient = new EmailClient(connectionString);

            var sender = "donotreply@98e0c7bc-2212-48f8-8427-5aa4f7ced394.azurecomm.net";

            try
            {
                Console.WriteLine("Sending email...");
                EmailSendOperation emailSendOperation = await emailClient.SendAsync(
                    Azure.WaitUntil.Completed,
                    sender,
                    recipient,
                    subject,
                    htmlContent);
                EmailSendResult statusMonitor = emailSendOperation.Value;

                Console.WriteLine($"Email Sent. Status = {emailSendOperation.Value.Status}");

                /// Get the OperationId so that it can be used for tracking the message for troubleshooting
                string operationId = emailSendOperation.Id;
                Console.WriteLine($"Email operation id = {operationId}");
            }
            catch (RequestFailedException ex)
            {
                /// OperationID is contained in the exception message and can be used for troubleshooting purposes
                Console.WriteLine($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
                logger.LogError(ex, "Error sending email via Azure");
            }
        }

        public class EmailSettings
        {
            public bool UseSMTP { get; set; }
            public string Host { get; set; }
            public int Port { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public bool EnableSSL { get; set; }
        }
    }
}
