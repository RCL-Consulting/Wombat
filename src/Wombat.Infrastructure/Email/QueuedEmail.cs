using Wombat.Application.Common.Email;

namespace Wombat.Infrastructure.Email;

/// <summary>
/// A mail in the <see cref="EmailQueue" />, with the reference every log line about it prints in place of its address
/// (T282).
/// </summary>
/// <param name="Reference">
/// Drawn when the mail is queued (<see cref="EmailLog.NewReference" />), so the line that queued it and every line the
/// worker writes about sending it carry the same one.
/// </param>
public sealed record QueuedEmail(EmailMessage Message, string Reference)
{
    /// <summary><paramref name="message" />, under a new reference.</summary>
    public QueuedEmail(EmailMessage message)
        : this(message, EmailLog.NewReference())
    {
    }
}
