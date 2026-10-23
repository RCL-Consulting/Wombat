using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;

namespace Wombat.Web.Components.Shared;

/// <summary>
/// The people pages' addresses (T376, flow 12, build-contracts § 0), so no page writes one by hand: Users and its filter
/// (lane U), a person's page and its training section (lanes P and T), Assessors and its old edit route (lane T), her
/// institution's record (lanes U and I) and SSO mappings (lane I). The menu's items keep their own literals, which a test
/// holds equal to these.
/// </summary>
public static class PeopleLinks
{
    /// <summary>Users: <c>/admin/users</c>.</summary>
    public const string Users = "/admin/users";

    /// <summary>Assessors: <c>/admin/assessors</c>.</summary>
    public const string Assessors = "/admin/assessors";

    /// <summary>
    /// The old assessor form's route (Q2, S10): it now redirects to the person's training section, and stays only so an old
    /// link or bookmark lands somewhere.
    /// </summary>
    public const string LegacyAssessorEdit = "/admin/assessors/edit";

    /// <summary>SSO mappings: <c>/admin/sso/group-mappings</c>.</summary>
    public const string SsoMappings = "/admin/sso/group-mappings";

    /// <summary>The user page's <c>section</c> value that opens Assessor training (Q1, S10).</summary>
    public const string TrainingSection = "training";

    /// <summary>Users' filter: the name or address typed (S6).</summary>
    public const string QueryParameter = "q";

    /// <summary>Users' filter: a role's key, or <see cref="NoRole" /> (S6).</summary>
    public const string RoleParameter = "role";

    /// <summary>Users' filter: the account's status (S6; its values are lane U's).</summary>
    public const string StatusParameter = "status";

    /// <summary>Users' page number, from 1 (F9, Q22).</summary>
    public const string PageParameter = "page";

    /// <summary>Users' page size (PagerControls' 10, 20, 50 or 100; 20 by default, Q22).</summary>
    public const string SizeParameter = "size";

    /// <summary>The user page's way back to the Users page it was opened from (S6).</summary>
    public const string BackParameter = "back";

    /// <summary>The Role filter's "No role" value (Q6, S2).</summary>
    public const string NoRole = "none";

    private static readonly HashSet<string> UsersParameters = new(StringComparer.Ordinal)
    {
        QueryParameter, RoleParameter, StatusParameter, PageParameter, SizeParameter
    };

    /// <summary>
    /// Users, filtered (S6): <c>/admin/users?role=Assessor</c>. Home's People card links each role here (Q7); a page of the
    /// list links each person with it as their way back. Each value is escaped; an empty one is left out.
    /// </summary>
    public static string UsersFiltered(string? q = null, string? role = null, string? status = null, int? page = null, int? size = null)
    {
        var query = new Dictionary<string, string?>(StringComparer.Ordinal);
        Add(query, QueryParameter, q);
        Add(query, RoleParameter, role);
        Add(query, StatusParameter, status);
        Add(query, PageParameter, page?.ToString(CultureInfo.InvariantCulture));
        Add(query, SizeParameter, size?.ToString(CultureInfo.InvariantCulture));
        return query.Count == 0 ? Users : QueryHelpers.AddQueryString(Users, query);
    }

    /// <summary>
    /// A person's page: <c>/admin/users/…</c>, with <c>?section=training</c> to open Assessor training (S10) and
    /// <c>back=</c> the Users page it was opened from (S6), each only when given.
    /// </summary>
    public static string User(string userId, string? section = null, string? back = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        var path = $"{Users}/{Uri.EscapeDataString(userId)}";
        var query = new Dictionary<string, string?>(StringComparer.Ordinal);
        Add(query, "section", section);
        Add(query, BackParameter, back);
        return query.Count == 0 ? path : QueryHelpers.AddQueryString(path, query);
    }

    /// <summary>A person's Assessor training, opened (Q1, Q2, S10): <c>/admin/users/…?section=training</c>.</summary>
    public static string UserTraining(string userId) => User(userId, TrainingSection);

    /// <summary>An institution's record: <c>/admin/institutions/1</c> (Home's Your institution card, Q3).</summary>
    public static string Institution(int institutionId)
        => string.Create(CultureInfo.InvariantCulture, $"/admin/institutions/{institutionId}");

    /// <summary>
    /// The Users address a person's page may lead back to (S6, an allow-listed return as flow 11's sign-out): the Users
    /// path, with only Users' own filter parameters, rebuilt from their parsed values, so a crafted <c>back</c> can never send
    /// anyone to another page or host. Anything else, a missing value included, is plain <see cref="Users" />.
    /// </summary>
    public static string SafeUsersReturn(string? back)
    {
        if (string.IsNullOrWhiteSpace(back))
        {
            return Users;
        }

        var queryStart = back.IndexOf('?', StringComparison.Ordinal);
        var path = queryStart < 0 ? back : back[..queryStart];
        if (!string.Equals(path, Users, StringComparison.Ordinal))
        {
            return Users;
        }

        if (queryStart < 0)
        {
            return Users;
        }

        var parsed = QueryHelpers.ParseQuery(back[queryStart..]);
        if (parsed.Keys.Any(key => !UsersParameters.Contains(key)))
        {
            return Users;
        }

        int? Number(string key)
            => parsed.TryGetValue(key, out var value)
                && int.TryParse(value.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out var number)
                && number > 0
                    ? number
                    : null;

        string? Text(string key) => parsed.TryGetValue(key, out var value) ? value.ToString() : null;

        return UsersFiltered(Text(QueryParameter), Text(RoleParameter), Text(StatusParameter), Number(PageParameter), Number(SizeParameter));
    }

    private static void Add(Dictionary<string, string?> query, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            query[key] = value.Trim();
        }
    }
}
