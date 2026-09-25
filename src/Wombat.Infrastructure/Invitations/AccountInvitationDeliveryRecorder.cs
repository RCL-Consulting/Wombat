using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Invitations;

namespace Wombat.Infrastructure.Invitations;

/// <summary>
/// Records onto an account invitation what became of its link's mail: sent, or dropped. The mail carries the invitation
/// and the link's hash in its <see cref="EmailMessage.DeliveryKey" /> (<see cref="Invitation.DeliveryKey" />). (T283)
/// </summary>
/// <remarks>
/// <para>
/// One statement, conditioned in the database, never a read then a write, as T251's recorder of MSF links is. It writes
/// only while the invitation still holds the link the mail carried, so a report about a link a resend has replaced changes
/// nothing: its invitee holds a link that no longer works, and the list speaks of the new one. The issue and the resend
/// both store the link before handing its mail over, so a report never arrives before its link and none is lost for it.
/// </para>
/// <para>
/// Once per link: a link already reported on is left as it is, so a failed mail is counted once towards
/// <see cref="Invitation.DeliveryFailures" />. Only a mail the mail server was offered and refused is counted; one dropped
/// before any attempt, as the app stopped, reads as not delivered and is not counted. The statement touches no column the
/// accept, the revoke or the erasure writes, and none of those saves reads the outcome, so whatever the order they all
/// land.
/// </para>
/// </remarks>
public sealed class AccountInvitationDeliveryRecorder : IEmailDeliveryObserver
{
    private readonly IApplicationDbContext _dbContext;

    public AccountInvitationDeliveryRecorder(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RecordAsync(EmailMessage message, EmailDeliveryOutcome outcome, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(outcome);

        if (!Invitation.TryReadDeliveryKey(message.DeliveryKey, out var invitationId, out var tokenHash))
        {
            return;
        }

        DateTime? sentOn = outcome.Sent ? outcome.At : null;
        DateTime? failedOn = outcome.Sent ? null : outcome.At;

        // A mail still queued as the app stopped was never offered to the mail server (no attempt): it is not delivered,
        // but nothing says its address was at fault, so it is not counted towards the address check (T283 review).
        var failure = !outcome.Sent && outcome.Attempts > 0 ? 1 : 0;

        await _dbContext.Set<Invitation>()
            .Where(invitation =>
                invitation.Id == invitationId &&
                invitation.TokenHash == tokenHash &&
                invitation.SentOn == null &&
                invitation.DeliveryFailedOn == null)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(invitation => invitation.SentOn, sentOn)
                    .SetProperty(invitation => invitation.DeliveryFailedOn, failedOn)
                    .SetProperty(invitation => invitation.DeliveryFailures, invitation => invitation.DeliveryFailures + failure),
                cancellationToken);
    }
}
