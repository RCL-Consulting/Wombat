using Wombat.Application.Common.Email;

namespace Wombat.Infrastructure.Email;

/// <summary>
/// Internal interface satisfied by <see cref="MailKitEmailSender"/>.
/// Exists so <see cref="EmailWorker"/> can be unit-tested without MailKit.
/// </summary>
public interface ISmtpSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}
