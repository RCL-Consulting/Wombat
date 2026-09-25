namespace Wombat.Application.Common.Email;

/// <summary>
/// Records what became of a mail that carries a <see cref="EmailMessage.DeliveryKey" />: the mail worker calls every
/// observer once per such mail, when the mail server has accepted it or the worker has given up on it. (T251)
/// </summary>
/// <remarks>
/// <para>
/// A send is a hand-off to an in-process queue (<c>QueuedEmailSender</c>) that never fails for want of a mail server, so
/// the request that sends a mail cannot say whether it went. Until T251 nothing could: a mail the worker gave up on was
/// logged and dropped. An observer is how the product learns it after the request.
/// </para>
/// <para>
/// Called on a scope of its own, after the request that sent the mail has ended, and possibly before that request's own
/// save has committed. Each observer reads its own keys and ignores the rest. A throw is logged by the worker and goes no
/// further: a failed record never stops the mail that follows.
/// </para>
/// </remarks>
public interface IEmailDeliveryObserver
{
    Task RecordAsync(EmailMessage message, EmailDeliveryOutcome outcome, CancellationToken cancellationToken);
}

/// <summary>What became of one mail: sent, on which attempt, or dropped after how many. (T251)</summary>
/// <param name="Sent">True when the mail server accepted it; false when the worker gave up on it.</param>
/// <param name="Attempts">
/// The attempts made: the one that succeeded, or every one made before giving up; none for a mail still queued when the
/// host stopped.
/// </param>
/// <param name="At">When the outcome was known (UTC).</param>
public sealed record EmailDeliveryOutcome(bool Sent, int Attempts, DateTime At)
{
    public static EmailDeliveryOutcome Delivered(int attempts, DateTime at) => new(true, attempts, at);

    public static EmailDeliveryOutcome Dropped(int attempts, DateTime at) => new(false, attempts, at);
}
