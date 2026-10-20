namespace Wombat.Application.Features.Invitations;

/// <summary>Why an issue, a resend or a revoke on the invitations pages was refused (T373, flow 11, M3).</summary>
public enum InvitationIssueRefusal
{
    /// <summary>The role's scope is incomplete or names something that is not there: <see cref="InvitationRules.ValidateScope" />.</summary>
    Scope,

    /// <summary>An account already holds the address (Q12, T286's issue half).</summary>
    AccountExists,

    /// <summary>An invitation that still works already names the address, at any institution or College (Q10).</summary>
    ActiveInvitationExists,

    /// <summary>The resend's own refusals: the invitation is used, revoked, past its last day, delivered, or changed.</summary>
    CannotResend,

    /// <summary>The revoke's own refusals: the invitation is used, or not there.</summary>
    CannotRevoke
}

/// <summary>The field of Invite a person a refusal sits under (T373, Q3: a refusal beside its field, never a summary).</summary>
public enum InvitationIssueField
{
    /// <summary>No one field: the page shows the refusal in its result.</summary>
    None,
    Email,
    Role,
    College,
    Institution,
    Speciality,
    SubSpeciality
}

/// <summary>
/// A refusal of an issue, a resend or a revoke, meant for the administrator who asked (T373, flow 11, M3, S9). The page
/// maps <see cref="Reason" /> and <see cref="Field" /> to its own words under the field.
/// </summary>
/// <remarks>
/// <para>
/// Until T373 these were bare <see cref="InvalidOperationException" />s whose message the page printed, and the audit
/// pipeline wrote that message into the caller's audit row (<c>AuditPipelineBehavior</c>, <c>errorMessage: ex.Message</c>).
/// A refusal over an address another institution's account holds would have put that account's name in a row the
/// Institutional admin can search. So the message of <see cref="InvitationIssueRefusal.AccountExists" /> and
/// <see cref="InvitationIssueRefusal.ActiveInvitationExists" /> is fixed generic words in every case
/// (<see cref="AddressInUse" />), and the holder's id and name, or the other invitation's issue time and last day, are
/// carried only when that account or invitation is judged in the caller's scope (<see cref="InCallerScope" />).
/// </para>
/// <para>
/// Derived from <see cref="InvalidOperationException" />, as <c>MsfRefusedException</c> is, so nothing that catches a
/// refusal changes and the audit row keeps its message. A page shows this type's words, and treats any other
/// <see cref="InvalidOperationException" /> as an unexpected failure: a fixed sentence, logged (T329).
/// </para>
/// </remarks>
public sealed class InvitationIssueRefusedException : InvalidOperationException
{
    /// <summary>The message of an address refusal, whoever holds the address: it names nobody (M3).</summary>
    public const string AddressInUse = "The address is already in use.";

    /// <param name="reason">Why.</param>
    /// <param name="field">Which field it is about.</param>
    /// <param name="message">
    /// What the audit row records: the scope rule's sentence, the resend's or revoke's own words, or
    /// <see cref="AddressInUse" />. It never names anyone.
    /// </param>
    public InvitationIssueRefusedException(InvitationIssueRefusal reason, InvitationIssueField field, string message)
        : base(message)
    {
        Reason = reason;
        Field = field;
    }

    public InvitationIssueRefusal Reason { get; }

    public InvitationIssueField Field { get; }

    /// <summary>
    /// Whether the account or invitation that holds the address is one the caller may see: always for an Administrator;
    /// for an Institutional admin, one at their institution. False for every other reason.
    /// </summary>
    public bool InCallerScope { get; init; }

    /// <summary>The account holding the address (Q12), only <see cref="InCallerScope" />.</summary>
    public string? HolderUserId { get; init; }

    /// <summary>The name of the account holding the address (Q12), only <see cref="InCallerScope" />.</summary>
    public string? HolderName { get; init; }

    /// <summary>When the invitation that holds the address issued its current link (Q10), only <see cref="InCallerScope" />.</summary>
    public DateTime? ActiveIssuedOn { get; init; }

    /// <summary>The last day of the invitation that holds the address (Q10), only <see cref="InCallerScope" />.</summary>
    public DateOnly? ActiveExpiresOn { get; init; }

    /// <summary>
    /// The invitation that holds the address, outside the caller's scope, is a College's rather than another institution's
    /// (Q10's out-of-scope words: "from a College"). It names neither.
    /// </summary>
    public bool HeldByCollege { get; init; }

    /// <summary>A scope refusal, under its field (N02, N10, N11).</summary>
    internal static InvitationIssueRefusedException ForScope(InvitationRules.ScopeRefusal refusal)
        => new(InvitationIssueRefusal.Scope, refusal.Field, refusal.Message);
}

/// <summary>Which exceptions from the invitations pages' commands and queries are refusals, whose words a page shows (T373, S9).</summary>
public static class InvitationIssueRefusals
{
    /// <summary>
    /// The validator's (<see cref="FluentValidation.ValidationException" />), a scope refusal
    /// (<see cref="UnauthorizedAccessException" />, in the handler's words), or the invitation's own
    /// (<see cref="InvitationIssueRefusedException" />). Anything else is an unexpected failure: a fixed sentence, logged.
    /// </summary>
    public static bool IsRefusal(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is FluentValidation.ValidationException or UnauthorizedAccessException
            or InvitationIssueRefusedException;
    }
}
