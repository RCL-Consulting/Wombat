using Wombat.Application.Common.Email;

namespace Wombat.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
