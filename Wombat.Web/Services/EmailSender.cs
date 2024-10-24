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
        private readonly string smtpServer;
        private readonly int port;
        private readonly string fromEmailAddress;

        public EmailSender( string smtpServer, int port, string fromEmailAddress)
        {
            this.smtpServer=smtpServer;
            this.port=port;
            this.fromEmailAddress=fromEmailAddress;
        }

        public async Task SendEmailAsync(string recipient, string subject, string htmlContent)
        {
            //var message = new MailMessage
            //{
            //    From = new MailAddress(fromEmailAddress),
            //    Subject = subject,
            //    Body = htmlContent,
            //    IsBodyHtml = true
            //};

            //message.To.Add(new MailAddress(recipient));

            //using (var client = new SmtpClient(smtpServer, port))
            //{
            //    client.Send(message);
            //}

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
            }

        }
    }
}
