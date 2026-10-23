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

    /// <summary>Each scope written (T376, M2): the person, the institution, the specialities and sub-specialities.</summary>
    public List<(string UserId, int InstitutionId, int[] SpecialityIds, int[] SubSpecialityIds)> UpdateScopeCalls { get; } = new();

    public List<(string UserId, string Provider)> RemoveSignInCalls { get; } = new();

    /// <summary>How many times the work was run in one transaction (M2).</summary>
    public int Transactions { get; private set; }

    /// <summary>Thrown by the scope's write, to stand for a fault after the role (M2's rollback; the Postgres test holds the real one).</summary>
    public Exception? UpdateScopeFault { get; set; }

    /// <summary>
    /// Run when a transaction opens, before its work: stands for another admin's write that committed between the
    /// handler's first read and its own transaction (T376 build review, B2).
    /// </summary>
    public Action<RecordingUserAdministrationService>? OnTransactionStart { get; set; }

    /// <summary>How each account signs in (F3, F7), by id; an account not named has a password and no institutional sign-in.</summary>
    public Dictionary<string, AccountSignInMethods> SignInMethods { get; } = new(StringComparer.Ordinal);

    /// <summary>What a removal of an institutional sign-in answers (F7).</summary>
    public InstitutionalSignInRemoval RemovalAnswer { get; set; } = InstitutionalSignInRemoval.Removed;

    /// <summary>Every change asked of the store, of any kind.</summary>
    public int Changes => AddRoleCalls.Count + RemoveRoleCalls.Count + ResetPasswordCalls.Count + LockoutCalls.Count
                          + UpdateScopeCalls.Count + RemoveSignInCalls.Count;

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
    {
        if (UpdateScopeFault is not null)
        {
            throw UpdateScopeFault;
        }

        UpdateScopeCalls.Add((userId, institutionId, specialityIds.ToArray(), subSpecialityIds.ToArray()));
        return Task.CompletedTask;
    }

    public Task<T> InOneTransactionAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken cancellationToken = default)
    {
        Transactions++;
        OnTransactionStart?.Invoke(this);
        return work(cancellationToken);
    }

    public Task<AccountSignInMethods?> GetSignInMethodsAsync(string userId, CancellationToken cancellationToken = default)
        => Task.FromResult<AccountSignInMethods?>(
            SignInMethods.TryGetValue(userId, out var methods) ? methods : _users.ContainsKey(userId) ? new AccountSignInMethods(true, []) : null);

    public Task<InstitutionalSignInRemoval> RemoveInstitutionalSignInAsync(string userId, string provider, CancellationToken cancellationToken = default)
    {
        if (RemovalAnswer == InstitutionalSignInRemoval.Removed)
        {
            RemoveSignInCalls.Add((userId, provider));
        }

        return Task.FromResult(RemovalAnswer);
    }

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
