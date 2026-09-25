using Wombat.Application.Common.Interfaces;

namespace Wombat.Application.Features.Invitations;

/// <summary>Why an invitation could not be previewed or accepted (T285).</summary>
public enum InvitationRefusal
{
    /// <summary>No stored invitation matches the token.</summary>
    Invalid,

    /// <summary>An administrator revoked the invitation, or another was accepted for the same address.</summary>
    Revoked,

    /// <summary>The invitation has been accepted already.</summary>
    Used,

    /// <summary>The invitation's expiry date has passed.</summary>
    Expired,

    /// <summary>An account already holds the invited address.</summary>
    AccountExists,

    /// <summary>
    /// Identity will not take the invited address as a user name (a character its rules do not allow), so no account can be
    /// created for it, whatever the person enters.
    /// </summary>
    AddressNotAccepted,

    /// <summary>
    /// Identity refused the new account: a password its rules do not allow, most often. Which rules is in
    /// <see cref="InvitationRefusedException.ErrorCodes" />.
    /// </summary>
    AccountNotCreated
}

/// <summary>
/// A refusal of an invitation, meant for the person holding it: the register page shows each <see cref="Reason" />, and
/// each of Identity's <see cref="ErrorCodes" />, in a sentence of its own choosing (T285).
/// </summary>
/// <remarks>
/// Until T285 these were bare <see cref="InvalidOperationException" />s, and the register endpoint put the message into
/// the address it redirected to, where the page printed it. So the page printed whatever text a crafted link carried, and
/// the endpoint's catch put any exception's message, a database fault's included, into the address. The endpoint now
/// sends codes, which it reads off this type; any other exception is logged, and the page says registration could not be
/// completed. Derived from <see cref="InvalidOperationException" /> so that nothing that already catches a refusal
/// changes, and the audit row keeps its message.
/// </remarks>
public sealed class InvitationRefusedException : InvalidOperationException
{
    public InvitationRefusedException(InvitationRefusal reason)
        : this(reason, InvitationRefusals.Describe(reason), [])
    {
    }

    /// <param name="reason">Why.</param>
    /// <param name="message">What the audit row records. Never shown to anyone: the page chooses its own words.</param>
    /// <param name="errorCodes">Identity's error codes, for <see cref="InvitationRefusal.AccountNotCreated" />.</param>
    public InvitationRefusedException(InvitationRefusal reason, string message, IReadOnlyList<string> errorCodes)
        : base(message)
    {
        Reason = reason;
        ErrorCodes = errorCodes;
    }

    public InvitationRefusal Reason { get; }

    /// <summary>
    /// Identity's error codes when it refused the new account (<see cref="InvitationRefusal.AccountNotCreated" />):
    /// <c>PasswordTooShort</c> and the like. Empty for every other reason.
    /// </summary>
    public IReadOnlyList<string> ErrorCodes { get; }
}

/// <summary>What each refusal says, on the register page and in the audit row (T285).</summary>
public static class InvitationRefusals
{
    /// <summary>
    /// The sentence for <paramref name="reason" />. A value added later without one throws here, which a test asks of every
    /// value.
    /// </summary>
    public static string Describe(InvitationRefusal reason) => reason switch
    {
        InvitationRefusal.Invalid => "This invitation is invalid.",
        InvitationRefusal.Revoked => "This invitation has been revoked.",
        InvitationRefusal.Used => "This invitation has already been used.",
        InvitationRefusal.Expired => "This invitation has expired.",
        InvitationRefusal.AccountExists => "A user with this email address already exists.",
        InvitationRefusal.AddressNotAccepted =>
            "This email address cannot be used for an account. Ask your administrator for an invitation to another address.",
        InvitationRefusal.AccountNotCreated => "The account could not be created.",
        _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, "An invitation refusal with no sentence.")
    };

    /// <summary>
    /// The refusal for an invited address no account can be created for; null when one can. The preview and the provisioner
    /// both refuse by this, so the register page and its submit say the same (T285).
    /// </summary>
    public static InvitationRefusal? For(InvitedAddressStatus status) => status switch
    {
        InvitedAddressStatus.Available => null,
        InvitedAddressStatus.Taken => InvitationRefusal.AccountExists,
        InvitedAddressStatus.NotAccepted => InvitationRefusal.AddressNotAccepted,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "An address status with no refusal.")
    };
}
