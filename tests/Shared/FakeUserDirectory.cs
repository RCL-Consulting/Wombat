using Wombat.Application.Common.Interfaces;

namespace Wombat.Tests.Shared;

/// <summary>
/// A user directory that answers only the display-name lookup, and records every lookup it is asked for, so a test can
/// say both what a page is named with and how many calls it took. Linked into the test projects that build a page's
/// query handler. (T142)
/// </summary>
/// <remarks>
/// Every other member throws: a handler that only shows names must not be managing users.
/// </remarks>
internal sealed class FakeUserDirectory : IUserAdministrationService
{
    private readonly Dictionary<string, string> _names;

    public FakeUserDirectory(params (string UserId, string Name)[] people)
    {
        _names = people.ToDictionary(person => person.UserId, person => person.Name, StringComparer.Ordinal);
    }

    /// <summary>Nobody exists: every id is looked up and none is found.</summary>
    public static FakeUserDirectory Empty => new();

    /// <summary>The ids each lookup asked for, in call order.</summary>
    public List<IReadOnlyCollection<string>> Lookups { get; } = [];

    public Task<IReadOnlyDictionary<string, string>> GetDisplayNamesAsync(
        IReadOnlyCollection<string> userIds,
        CancellationToken cancellationToken = default)
    {
        Lookups.Add(userIds.ToList());

        IReadOnlyDictionary<string, string> found = userIds
            .Distinct(StringComparer.Ordinal)
            .Where(_names.ContainsKey)
            .ToDictionary(id => id, id => _names[id], StringComparer.Ordinal);

        return Task.FromResult(found);
    }

    public Task<UserIdentityDetails?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<UserIdentityDetails>> ListUsersInRoleAsync(string role, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<UserIdentityDetails>> ListAllUsersAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task UpdateNamesAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task UpdateScopeAsync(
        string userId,
        int institutionId,
        IReadOnlyCollection<int> specialityIds,
        IReadOnlyCollection<int> subSpecialityIds,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task PromotePendingTraineeAsync(string userId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task AddRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task RemoveRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task SetLockoutAsync(string userId, bool locked, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}
