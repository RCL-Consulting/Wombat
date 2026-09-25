namespace Wombat.Application.Common.Interfaces;

public interface IUserAdministrationService
{
    Task<UserIdentityDetails?> GetByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserIdentityDetails>> ListUsersInRoleAsync(string role, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserIdentityDetails>> ListAllUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// "First Last" for exactly the users asked about, whatever roles they hold. A user who does not exist is
    /// left out. The default implementation reads every user; the real service overrides it with one query by id.
    /// </summary>
    async Task<IReadOnlyDictionary<string, string>> GetDisplayNamesAsync(
        IReadOnlyCollection<string> userIds,
        CancellationToken cancellationToken = default)
    {
        var wanted = userIds.ToHashSet(StringComparer.Ordinal);
        return (await ListAllUsersAsync(cancellationToken))
            .Where(user => wanted.Contains(user.UserId))
            .ToDictionary(user => user.UserId, user => $"{user.FirstName} {user.LastName}".Trim(), StringComparer.Ordinal);
    }

    /// <summary>
    /// Which of exactly the users asked about hold <paramref name="role" />. A user who does not exist, or does not hold
    /// it, is left out. The default implementation reads the role's whole listing; the real service overrides it with one
    /// query by id, so asking about a panel's few candidates does not load every holder of the role. (T237)
    /// </summary>
    async Task<IReadOnlySet<string>> WhichHoldRoleAsync(
        IReadOnlyCollection<string> userIds,
        string role,
        CancellationToken cancellationToken = default)
    {
        var wanted = userIds.ToHashSet(StringComparer.Ordinal);
        return (await ListUsersInRoleAsync(role, cancellationToken))
            .Select(user => user.UserId)
            .Where(wanted.Contains)
            .ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>
    /// Which of exactly the users asked about hold <paramref name="role" /> on an account that is not deactivated
    /// (<see cref="UserIdentityDetails.IsDeactivated" />: an administrator's lock or an erasure, never a brute-force
    /// lockout that lifts itself after minutes). A user who does not exist, does not hold the role, or is deactivated is
    /// left out. The account half of a current trainee (<c>TraineeScopeResolver</c>, T238, T268). The default
    /// implementation reads the role's whole listing; the real service overrides it with one query by id.
    /// </summary>
    async Task<IReadOnlySet<string>> WhichActivelyHoldRoleAsync(
        IReadOnlyCollection<string> userIds,
        string role,
        CancellationToken cancellationToken = default)
    {
        var wanted = userIds.ToHashSet(StringComparer.Ordinal);
        return (await ListUsersInRoleAsync(role, cancellationToken))
            .Where(user => !user.IsDeactivated)
            .Select(user => user.UserId)
            .Where(wanted.Contains)
            .ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>
    /// The name and email of exactly the users asked about, whatever roles they hold: what a picker labels a person by.
    /// A user who does not exist is left out. The default implementation reads every user; the real service overrides it
    /// with one query by id, so a picker of an institution's trainees reads only them. (T248)
    /// </summary>
    async Task<IReadOnlyDictionary<string, UserContact>> GetContactsAsync(
        IReadOnlyCollection<string> userIds,
        CancellationToken cancellationToken = default)
    {
        var wanted = userIds.ToHashSet(StringComparer.Ordinal);
        return (await ListAllUsersAsync(cancellationToken))
            .Where(user => wanted.Contains(user.UserId))
            .ToDictionary(
                user => user.UserId,
                user => new UserContact(user.UserId, user.FirstName, user.LastName, user.Email),
                StringComparer.Ordinal);
    }

    Task UpdateNamesAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken = default);
    Task UpdateScopeAsync(
        string userId,
        int institutionId,
        IReadOnlyCollection<int> specialityIds,
        IReadOnlyCollection<int> subSpecialityIds,
        CancellationToken cancellationToken = default);
    Task PromotePendingTraineeAsync(string userId, CancellationToken cancellationToken = default);
    Task AddRoleAsync(string userId, string role, CancellationToken cancellationToken = default);
    Task RemoveRoleAsync(string userId, string role, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default);
    Task SetLockoutAsync(string userId, bool locked, CancellationToken cancellationToken = default);
}

/// <param name="IsLockedOut">Locked out right now, for any reason: an administrator's lock, an erasure, or a brute-force
/// lockout that lifts itself after minutes.</param>
/// <param name="IsDeactivated">
/// Locked indefinitely, as an administrator's lock and an erasure leave an account, and not merely locked out for a few
/// minutes by failed passwords (<c>UserDeactivation</c>, T102). A deactivated account can sit on no committee panel
/// (T165).
/// </param>
public sealed record UserIdentityDetails(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    int? InstitutionId,
    IReadOnlyCollection<int> SpecialityIds,
    IReadOnlyCollection<int> SubSpecialityIds,
    IReadOnlyCollection<string> Roles,
    bool IsLockedOut = false,
    bool IsDeactivated = false);

/// <summary>What a picker labels a person by: their name and email, and nothing about their roles or scope. (T248)</summary>
public sealed record UserContact(string UserId, string FirstName, string LastName, string Email);
