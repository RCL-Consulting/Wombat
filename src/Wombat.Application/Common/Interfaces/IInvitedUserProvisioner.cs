namespace Wombat.Application.Common.Interfaces;

public sealed record ProvisionedInvitationUser(string UserId, string AssignedRole);

/// <summary>Whether an account can be created for an invited address (T285).</summary>
public enum InvitedAddressStatus
{
    /// <summary>No account holds the address, and Identity accepts it as a user name.</summary>
    Available,

    /// <summary>An account holds the address, as its email or as its user name.</summary>
    Taken,

    /// <summary>Identity refuses the address as a user name: it holds a character Identity's rules do not allow.</summary>
    NotAccepted
}

public interface IInvitedUserProvisioner
{
    /// <summary>
    /// Whether an account can be created for <paramref name="email" />, judged as <see cref="ProvisionAsync" /> judges it
    /// before creating one. The invitation's preview asks the same, so the register page offers no form for an invitation
    /// that no password could complete, and the page and the submit agree (T285).
    /// </summary>
    Task<InvitedAddressStatus> GetAddressStatusAsync(string email, CancellationToken cancellationToken = default);

    Task<ProvisionedInvitationUser> ProvisionAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string targetRole,
        int? institutionId,
        int? collegeId,
        int? specialityId,
        int? subSpecialityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs <paramref name="work" /> in one database transaction on the context the account is created in, and commits
    /// it only when the work returns (T373, flow 11, F10). The accept provisions the account (the user, the role, the
    /// scopes) and marks the invitation used inside it, so a fault between the two leaves no account and an unused
    /// invitation, and the register page's "Nothing was saved." holds. On a fault the transaction is rolled back and
    /// what the work left tracked is discarded, so the failed command's audit row, saved through the same context, commits
    /// nothing the rollback undid (<c>AuditPipelineBehavior</c>). Inside a transaction already open, the work joins it.
    /// </summary>
    Task<T> InOneTransactionAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken cancellationToken = default);
}
