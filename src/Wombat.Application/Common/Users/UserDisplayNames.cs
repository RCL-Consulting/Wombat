using Wombat.Application.Common.Interfaces;

namespace Wombat.Application.Common.Users;

/// <summary>
/// The names of the people one page shows, looked up in one call. (T130, T142)
/// </summary>
/// <remarks>
/// <para>
/// A page that lists people is given their names by the query that serves it, as a field on its DTO: never resolved
/// in Razor, never one lookup per row, and never inside a mapper that other paths share. The query collects every id
/// its rows name, asks <see cref="IUserAdministrationService.GetDisplayNamesAsync" /> once, and fills each row from
/// <see cref="NameOf" />.
/// </para>
/// <para>
/// The id is shown only where no name can be: the user no longer exists, or exists with no name on record. A blank
/// cell would read as "nobody", which is worse than an id.
/// </para>
/// </remarks>
public sealed class UserDisplayNames
{
    private static readonly UserDisplayNames Empty = new(new Dictionary<string, string>(StringComparer.Ordinal));

    private readonly IReadOnlyDictionary<string, string> _names;

    private UserDisplayNames(IReadOnlyDictionary<string, string> names)
    {
        _names = names;
    }

    /// <summary>
    /// One lookup for every distinct id named, or none at all when no id is.
    /// </summary>
    public static async Task<UserDisplayNames> ResolveAsync(
        IUserAdministrationService users,
        IEnumerable<string?> userIds,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(userIds);

        var ids = userIds
            .Where(id => !string.IsNullOrEmpty(id))
            .Select(id => id!)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return ids.Count == 0
            ? Empty
            : new UserDisplayNames(await users.GetDisplayNamesAsync(ids, cancellationToken));
    }

    /// <summary>
    /// "First Last" for this user, or their id when there is no user by that id or the user has no name.
    /// </summary>
    public string NameOf(string userId)
        => _names.TryGetValue(userId, out var name) && !string.IsNullOrWhiteSpace(name) ? name : userId;
}
