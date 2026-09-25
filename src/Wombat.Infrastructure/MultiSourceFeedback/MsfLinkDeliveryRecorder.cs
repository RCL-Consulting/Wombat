using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Infrastructure.MultiSourceFeedback;

/// <summary>
/// Records onto the invitation what became of an MSF link's mail: sent, or dropped. The mail carries the invitation and
/// the link in its <see cref="EmailMessage.DeliveryKey" /> (<see cref="MsfInvitation.DeliveryKey" />). (T251)
/// </summary>
/// <remarks>
/// <para>
/// One statement, conditioned in the database, never a read then a write. The worker reports on its own scope, and the
/// report can arrive at any moment of the request that sent the mail: before the open or the reminder has stored the
/// link (a fast mail server answers first), or while a close is anonymising the campaign. So it writes by the invitation's
/// id, naming the link the outcome is about (<see cref="MsfInvitation.DeliveryLinkSelector" />), and touches no column
/// that issuing a link writes; whatever the order of the two, both land, and the outcome counts once its link is the
/// current one. It writes no campaign row, so the open's save, checked against the campaign's xmin, is never refused
/// because a mail was reported.
/// </para>
/// <para>
/// Nothing is written onto an invitation whose address has been erased: a closed or withdrawn campaign's invitations are
/// anonymised, and anonymising clears the outcome (<see cref="MsfInvitation.Anonymize" />). The outcome is not nothing
/// about the respondent: the worker logs the address it sent to at the same instant, so a time left on an erased row
/// would tie that row, and its answers, to the address. A close that read an invitation just before this statement and
/// saves just after therefore writes the three columns whatever it read (<c>ApplicationDbContext</c>, T251 review), and
/// the row's lock orders the two: a report that committed first is overwritten, and one that waited on the close finds
/// the address erased and writes nothing.
/// </para>
/// <para>
/// The last report wins, whichever link it is about. The worker sends one mail at a time in the order they were handed
/// over, and an invitation's links are handed over in the order they are issued, so the last report is usually about the
/// newest link. A report about a link not stored yet cannot be told from one about a link that never will be (the open,
/// the reminder and the resend all store theirs after the report can land), so none is refused for it. So whenever two
/// requests hand over a link for one invitation at once, and the one refused at its save handed its mail over last, its
/// report lands last and the link stored reads as unreported, and an hour on as not delivered: two opens that race
/// (T184), two resends (two tabs, or two coordinators), or a resend and the reminder job. The cost is a second mail,
/// never a lost one.
/// </para>
/// </remarks>
public sealed class MsfLinkDeliveryRecorder : IEmailDeliveryObserver
{
    private readonly IApplicationDbContext _dbContext;

    public MsfLinkDeliveryRecorder(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RecordAsync(EmailMessage message, EmailDeliveryOutcome outcome, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(outcome);

        if (!MsfInvitation.TryReadDeliveryKey(message.DeliveryKey, out var invitationId, out var linkSelector))
        {
            return;
        }

        DateTime? sentOn = outcome.Sent ? outcome.At : null;
        DateTime? failedOn = outcome.Sent ? null : outcome.At;

        await _dbContext.Set<MsfInvitation>()
            .Where(invitation =>
                invitation.Id == invitationId &&
                invitation.RespondentEmail != null &&
                invitation.AnonymizedOn == null)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(invitation => invitation.DeliveryLinkSelector, linkSelector)
                    .SetProperty(invitation => invitation.SentOn, sentOn)
                    .SetProperty(invitation => invitation.DeliveryFailedOn, failedOn),
                cancellationToken);
    }
}
