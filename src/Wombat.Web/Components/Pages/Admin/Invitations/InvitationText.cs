using System.Globalization;
using Wombat.Application.Features.Invitations;
using Wombat.Domain.Invitations;

namespace Wombat.Web.Components.Pages.Admin.Invitations;

/// <summary>What the invitations list says about each invitation's mail, and what an issue or a resend did. (T283)</summary>
public static class InvitationText
{
    /// <summary>Whether the invitation's current link was not delivered, so its row offers Resend.</summary>
    public static bool OffersResend(ActiveInvitationDto invitation)
    {
        ArgumentNullException.ThrowIfNull(invitation);
        return invitation.Delivery == InvitationDelivery.NotDelivered;
    }

    /// <summary>Whether the invitation's current link is still being sent: its cell is muted, as nothing is wrong yet.</summary>
    public static bool IsBeingSent(ActiveInvitationDto invitation)
    {
        ArgumentNullException.ThrowIfNull(invitation);
        return invitation.Delivery == InvitationDelivery.BeingSent;
    }

    /// <summary>The Delivery cell's first words: what became of the current link's mail.</summary>
    public static string Delivery(InvitationDelivery delivery) => delivery switch
    {
        InvitationDelivery.Sent => "Sent",
        InvitationDelivery.BeingSent => "Being sent",
        _ => "Not delivered."
    };

    /// <summary>
    /// What follows "Not delivered." once enough of the invitation's mails have failed that the address may be what is
    /// wrong (<see cref="ActiveInvitationDto.SuggestCheckingAddress" />); null before then.
    /// </summary>
    /// <remarks>
    /// A mail server that always refuses an address (a mistyped domain) leaves it not delivered however often it is resent,
    /// and the product cannot tell that from a mail server that was down. Only an administrator can: by reading the
    /// address. A wrong address is not corrected in place, since the invitation names who may register.
    /// </remarks>
    public static string? CheckAddress(ActiveInvitationDto invitation)
    {
        ArgumentNullException.ThrowIfNull(invitation);
        if (!invitation.SuggestCheckingAddress)
        {
            return null;
        }

        var failures = invitation.DeliveryFailures.ToString(CultureInfo.InvariantCulture);
        return $"Its email has failed {failures} times, so the mail server may be refusing this address. " +
               "Check the address; if it is wrong, revoke this invitation and issue a new one to the right address.";
    }

    /// <summary>
    /// What a row not delivered says last, after "Not delivered." and any <see cref="CheckAddress" />: that Resend
    /// retires the current link. The link alert tells the administrator who issued it that the link may be shared another
    /// way; this is where anyone about to press Resend on that row (the row's Resend names this text) learns that it
    /// would stop a shared link working. (T283 review)
    /// </summary>
    public const string ResendRetiresTheLink =
        "Resend emails a new link in place of the current one, which then stops working.";

    /// <summary>The result of an issue: the link is shown below it, and its email is on its way.</summary>
    public static string Issued(string email)
        => $"Invitation issued for {email}. Its email is being sent. Copy the link below — it is shown only once.";

    /// <summary>The result of a resend: a new link, emailed, and the old one retired.</summary>
    public static string Resent(string? email)
        => (email is null ? "A new invitation link is being emailed." : $"A new invitation link is being emailed to {email}.") +
           " The link it replaces no longer works. Copy the new link below — it is shown only once.";

    /// <summary>
    /// What the link's alert says about its email: the Delivery column says whether it arrived, and the link can be
    /// shared another way meanwhile. Until T283 the page said only that email delivery was "configured separately".
    /// </summary>
    public const string LinkIsBeingEmailed =
        "It is also being emailed to the invitee, and the Delivery column below says whether that arrived. If it was not " +
        "delivered, share this link another way, or press Resend, which emails a new link in place of this one.";
}
