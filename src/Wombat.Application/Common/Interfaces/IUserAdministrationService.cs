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
