using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;

namespace Wombat.Tests.Shared;

/// <summary>
/// A user directory that answers the display-name lookup and the role listing, and records every name lookup it is asked
/// for, so a test can say both what a page is named with and how many calls it took. Linked into the test projects that
/// build a page's query handler. (T142)
/// </summary>
/// <remarks>
/// Every member that manages users throws: a handler that only shows names, or reads who holds a role, must not be
/// managing users. The role listing answers the users added with <see cref="With" />; a committee's panel members are
/// read through it (T165, <c>PanelSeat</c>), and so is whether a trainee still holds Trainee (T238).
/// </remarks>
internal sealed class FakeUserDirectory : IUserAdministrationService
{
    private readonly Dictionary<string, string> _names;
    private readonly List<UserIdentityDetails> _users = [];

    public FakeUserDirectory(params (string UserId, string Name)[] people)
    {
        _names = people.ToDictionary(person => person.UserId, person => person.Name, StringComparer.Ordinal);
    }

    /// <summary>Nobody exists: every id is looked up and none is found.</summary>
    public static FakeUserDirectory Empty => new();

    /// <summary>
    /// Active CommitteeMember holders at <paramref name="institutionId" />: the people a panel there may seat (T165).
    /// </summary>
    public static FakeUserDirectory CommitteeMembersAt(int institutionId, params string[] userIds)
        => new FakeUserDirectory().WithCommitteeMembers(institutionId, userIds);

    /// <summary>Adds active CommitteeMember holders at <paramref name="institutionId" />.</summary>
    public FakeUserDirectory WithCommitteeMembers(int institutionId, params string[] userIds)
    {
        foreach (var userId in userIds)
        {
            With(new UserIdentityDetails(
                userId, $"{userId}@test", userId, string.Empty, institutionId, [], [], ["CommitteeMember"]));
        }

        return this;
    }

    /// <summary>
    /// Accounts that hold Trainee, one per id: whom a fixture's trainee profiles belong to. An active profile on one of
    /// these is a current trainee (<c>TraineeScopeResolver.ResolveCurrentAsync</c>, T238); a profile whose id is not here
    /// is one that outlived its trainee, as an erased trainee's pseudonym is.
    /// </summary>
    public static FakeUserDirectory Trainees(params string[] userIds)
        => new FakeUserDirectory().WithTrainees(userIds);

    /// <summary>
    /// An account that holds Trainee for every trainee profile in <paramref name="db" />, as it stands when this is
    /// called: a fixture in which every profile belongs to a trainee, so each active one is a current trainee (T238). A
    /// test about a profile that outlived its trainee builds its directory with <see cref="Trainees" /> instead.
    /// </summary>
    public static FakeUserDirectory TraineesOf(IApplicationDbContext db)
        => new FakeUserDirectory().WithTraineesOf(db);

    /// <summary>Adds an account that holds Trainee for every trainee profile in <paramref name="db" /> (T238).</summary>
    public FakeUserDirectory WithTraineesOf(IApplicationDbContext db)
        => WithTrainees(db.Set<TraineeProfile>().Select(profile => profile.UserId).Distinct().ToArray());

    /// <summary>Adds accounts that hold Trainee, one per id (T238).</summary>
    public FakeUserDirectory WithTrainees(params string[] userIds)
    {
        foreach (var userId in userIds)
        {
            With(new UserIdentityDetails(userId, $"{userId}@test", userId, string.Empty, null, [], [], ["Trainee"]));
        }

        return this;
    }

    /// <summary>Adds a user whom the role listing answers for each role they hold.</summary>
    public FakeUserDirectory With(UserIdentityDetails user)
    {
        _users.Add(user);
        return this;
    }

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

    /// <summary>
    /// A user added with <see cref="With" />, with every role they were added with; null for anyone else, as the real
    /// store answers an id that names no account (an erased trainee's pseudonym among them). (T238)
    /// </summary>
    public Task<UserIdentityDetails?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var records = _users.Where(user => string.Equals(user.UserId, userId, StringComparison.Ordinal)).ToArray();

        return Task.FromResult(records.Length == 0
            ? null
            : records[0] with { Roles = records.SelectMany(user => user.Roles).Distinct(StringComparer.Ordinal).ToArray() });
    }

    public Task<IReadOnlyList<UserIdentityDetails>> ListUsersInRoleAsync(string role, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<UserIdentityDetails>>(_users
            .Where(user => user.Roles.Contains(role, StringComparer.OrdinalIgnoreCase))
            .Select(user => user with { Roles = [role] })
            .ToArray());

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
