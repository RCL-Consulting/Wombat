using Wombat.Application.Common.Interfaces;

namespace Wombat.Application.Tests.Features.Users;

/// <summary>
/// A user store for the Users handlers that records every lookup and every change asked of it, so a test can say both
/// that a refused command changed nothing and that it was refused before it looked the user up. (T061; the lookups since
/// T278)
/// </summary>
/// <remarks>
/// The real <c>UserAdministrationService</c> writes each change through <c>UserManager</c>, which saves at once, so a
/// change the store was asked for is a change made: the audit trap for these handlers is that no change is asked for
/// before the last refusal. <c>UserAdministrationSelfAndTraineePostgresTests</c> holds the same commands to it through the
/// real store and the real audit pipeline.
/// </remarks>
internal sealed class RecordingUserAdministrationService : IUserAdministrationService
{
    private readonly Dictionary<string, UserIdentityDetails> _users = new(StringComparer.Ordinal);

    public List<string> Lookups { get; } = new();
    public List<(string UserId, string Role)> AddRoleCalls { get; } = new();
    public List<(string UserId, string Role)> RemoveRoleCalls { get; } = new();
    public List<(string UserId, string Password)> ResetPasswordCalls { get; } = new();
    public List<(string UserId, bool Locked)> LockoutCalls { get; } = new();

    /// <summary>Every change asked of the store, of any kind.</summary>
    public int Changes => AddRoleCalls.Count + RemoveRoleCalls.Count + ResetPasswordCalls.Count + LockoutCalls.Count;

    public void Add(UserIdentityDetails user) => _users[user.UserId] = user;

    public Task<UserIdentityDetails?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        Lookups.Add(userId);
        return Task.FromResult(_users.TryGetValue(userId, out var user) ? user : null);
    }

    public Task<IReadOnlyList<UserIdentityDetails>> ListUsersInRoleAsync(string role, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<UserIdentityDetails>>(_users.Values.Where(user => user.Roles.Contains(role)).ToArray());

    public Task<IReadOnlyList<UserIdentityDetails>> ListAllUsersAsync(CancellationToken cancellationToken = default)
    {
        Lookups.Add("*");
        return Task.FromResult<IReadOnlyList<UserIdentityDetails>>(_users.Values.ToArray());
    }

    public Task UpdateNamesAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task UpdateScopeAsync(string userId, int institutionId, IReadOnlyCollection<int> specialityIds, IReadOnlyCollection<int> subSpecialityIds, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task PromotePendingTraineeAsync(string userId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task AddRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
    {
        AddRoleCalls.Add((userId, role));
        return Task.CompletedTask;
    }

    public Task RemoveRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
    {
        RemoveRoleCalls.Add((userId, role));
        return Task.CompletedTask;
    }

    public Task ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default)
    {
        ResetPasswordCalls.Add((userId, newPassword));
        return Task.CompletedTask;
    }

    public Task SetLockoutAsync(string userId, bool locked, CancellationToken cancellationToken = default)
    {
        LockoutCalls.Add((userId, locked));
        return Task.CompletedTask;
    }
}
