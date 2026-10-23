using System.Globalization;
using Wombat.Application.Features.Users.Queries.ListUsers;
using Wombat.Domain.Identity;

namespace Wombat.Web.Components.Pages.Admin.Users;

/// <summary>
/// Users' words (T376, flow 12, boards R3-U01–U14; Spec 3's messages), in one place so the page and its tests say the same.
/// "They" for a person, no pronoun stored; straight apostrophes; the search quoted as the boards quote it.
/// </summary>
public static class UsersListWords
{
    /// <summary>The subtitle before the list has loaded, and when it fails (S9: the institution's name is not yet known).</summary>
    public const string SubtitleWithoutName =
        "Everyone with an account at your institution: the roles each holds, their scope, and whether they can sign in.";

    /// <summary>The Administrator's subtitle (U14).</summary>
    public const string AdministratorSubtitle =
        "Every account in Wombat: the institution, roles and scope of each, and whether they can sign in.";

    /// <summary>The subtitle once the list has loaded (U01).</summary>
    public static string Subtitle(string? institutionName)
        => string.IsNullOrWhiteSpace(institutionName)
            ? SubtitleWithoutName
            : $"Everyone with an account at {institutionName}: the roles each holds, their scope, and whether they can sign in.";

    /// <summary>The table's caption (U01; U14's for the Administrator).</summary>
    public static string Caption(bool isAdministrator, string? institutionName)
        => isAdministrator
            ? "Every institution, by last name: each person's institution, roles, scope and status."
            : $"At {(string.IsNullOrWhiteSpace(institutionName) ? "your institution" : institutionName)}, by last name: each person's roles, one scope for all of them, and whether they can sign in.";

    public const string FilterName = "Filter people";
    public const string SearchLabel = "Name or email";
    public const string SearchHelp = "A name in either order, part of one, or an address.";
    public const string RoleLabel = "Role";
    public const string AnyRole = "Any role";
    public const string NoRole = "No role";
    public const string StatusLabel = "Status";
    public const string AnyStatus = "Any status";
    public const string Active = "Active";
    public const string LockedOut = "Locked out";
    public const string Show = "Show";
    public const string Clear = "Clear";
    public const string ListTitle = "People";
    public const string PagerName = "Pages of people";
    public const string PageSizeLabel = "Rows per page";
    public const string Loading = "Loading the users.";

    /// <summary>A failed read (U12; T329): fixed words, the failure itself the log's.</summary>
    public const string LoadFailed = "Could not load the users. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>
    /// What a person with no role and an ended programme is (S2, D54): under No role in their Roles cell, and under the
    /// Role filter when No role is chosen (U09).
    /// </summary>
    public const string EndedProgrammeNote = "A completed or withdrawn registrar keeps an account and its records.";

    /// <summary>
    /// The Role filter's choices, in U01's order: Administrator and College admin first for the Administrator alone (S6
    /// (b), U14), then the institution's roles with Pending trainee before Trainee, then No role.
    /// </summary>
    public static IReadOnlyList<(string Value, string Label)> RoleOptions(bool isAdministrator)
    {
        var roles = new List<string>();
        if (isAdministrator)
        {
            roles.AddRange(UsersFilterValues.AdministratorOnlyRoles);
        }

        roles.AddRange([
            WombatRoles.InstitutionalAdmin,
            WombatRoles.SpecialityAdmin,
            WombatRoles.SubSpecialityAdmin,
            WombatRoles.Coordinator,
            WombatRoles.CommitteeMember,
            WombatRoles.Assessor,
            WombatRoles.PendingTrainee,
            WombatRoles.Trainee
        ]);

        return roles
            .Select(role => (role, WombatRoleLabels.For(role)))
            .Append((UsersFilterValues.NoRole, NoRole))
            .ToArray();
    }

    /// <summary>The Status filter's choices (Q6; K25: Locked out is the administrative lock).</summary>
    public static IReadOnlyList<(string Value, string Label)> StatusOptions { get; } =
    [
        (UsersFilterValues.Active, Active),
        (UsersFilterValues.Locked, LockedOut)
    ];

    /// <summary>
    /// The heading's count (U01, U02, U09): the number of people when nothing is filtered ("10"), else how many of them
    /// matched and what was asked ("1 of 10 match “zulu”", "2 of 14 hold no role").
    /// </summary>
    public static string HeadingCount(UsersFilter filter, int matchCount, int totalCount)
        => filter.IsFiltered
            ? $"{Count(matchCount)} of {Count(totalCount)} {Predicate(filter, singular: totalCount == 1)}"
            : Count(totalCount);

    /// <summary>
    /// The hidden live count after a filter (S21): "1 of 10 people match “zulu”.", "2 of 14 people hold no role."; with no
    /// match, "No one matches “zzz”." (U03). Null when nothing is filtered.
    /// </summary>
    public static string? LiveCount(UsersFilter filter, int matchCount, int totalCount)
    {
        if (!filter.IsFiltered)
        {
            return null;
        }

        if (matchCount == 0)
        {
            return $"No one {Predicate(filter, singular: true)}.";
        }

        var people = totalCount == 1 ? "person" : "people";
        return $"{Count(matchCount)} of {Count(totalCount)} {people} {Predicate(filter, singular: totalCount == 1)}.";
    }

    /// <summary>No match's title (U03): "No one matches “zzz”", or, when more than a search was asked, the filters'.</summary>
    public static string NoMatchTitle(UsersFilter filter)
        => filter.OnlySearch ? $"No one matches {Quoted(filter.Search!)}" : "No one matches these filters";

    /// <summary>
    /// No match's copy (U03): "Nobody at Kgosi Kgari Teaching Hospital has “zzz” in their name or address. Press Clear to see
    /// all 10 people."
    /// </summary>
    public static string NoMatchBody(UsersFilter filter, bool isAdministrator, string? institutionName, int totalCount)
    {
        var where = isAdministrator
            ? "in Wombat"
            : $"at {(string.IsNullOrWhiteSpace(institutionName) ? "your institution" : institutionName)}";
        var asked = filter.OnlySearch
            ? $"Nobody {where} has {Quoted(filter.Search!)} in their name or address."
            : $"Nobody {where} matches these filters.";
        var all = totalCount == 1 ? "the 1 person" : $"all {Count(totalCount)} people";
        return $"{asked} Press Clear to see {all}.";
    }

    /// <summary>What a scope with no person in it reads (no board: nobody holds an account here yet).</summary>
    public const string EmptyTitle = "No accounts yet";

    public const string EmptyBody = "Nobody has an account here yet. Use Invite a person to add the first.";

    private static string Predicate(UsersFilter filter, bool singular)
    {
        var parts = new List<string>();
        if (filter.Search is { } search)
        {
            parts.Add($"{(singular ? "matches" : "match")} {Quoted(search)}");
        }

        if (filter.NoRole)
        {
            parts.Add($"{(singular ? "holds" : "hold")} no role");
        }
        else if (filter.Role is { } role)
        {
            parts.Add($"{(singular ? "holds" : "hold")} {WombatRoleLabels.For(role)}");
        }

        if (filter.Status is { } status)
        {
            var word = status == UsersFilterValues.Locked ? "locked out" : "active";
            parts.Add($"{(singular ? "is" : "are")} {word}");
        }

        return parts.Count switch
        {
            1 => parts[0],
            2 => $"{parts[0]} and {parts[1]}",
            _ => $"{string.Join(", ", parts.Take(parts.Count - 1))} and {parts[^1]}"
        };
    }

    private static string Quoted(string search) => $"“{search}”";

    private static string Count(int count) => count.ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// What Users was asked for, read from its address (T376, flow 12, S6): each value as the page offers it, anything else
/// (a role not offered to the caller, an unknown status, a page below 1, a size the pager does not offer) read as not asked.
/// </summary>
public sealed record UsersFilter(string? Search, string? Role, bool NoRole, string? Status, int Page, int PageSize)
{
    /// <summary>Something narrows the list (a page or size alone does not).</summary>
    public bool IsFiltered => Search is not null || Role is not null || NoRole || Status is not null;

    /// <summary>Only a name or address was asked (U03's words).</summary>
    public bool OnlySearch => Search is not null && Role is null && !NoRole && Status is null;

    /// <summary>The Role filter's value: a role's key, <see cref="UsersFilterValues.NoRole" />, or null.</summary>
    public string? RoleValue => NoRole ? UsersFilterValues.NoRole : Role;

    /// <summary>Reads the address's values for a caller who is, or is not, the Administrator.</summary>
    public static UsersFilter From(string? q, string? role, string? status, int? page, int? size, bool isAdministrator)
    {
        var search = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
        var offered = UsersListWords.RoleOptions(isAdministrator).Select(option => option.Value).ToHashSet(StringComparer.Ordinal);
        var roleValue = string.IsNullOrWhiteSpace(role) || !offered.Contains(role.Trim()) ? null : role.Trim();
        var noRole = string.Equals(roleValue, UsersFilterValues.NoRole, StringComparison.Ordinal);
        var statusValue = status?.Trim() is UsersFilterValues.Active or UsersFilterValues.Locked ? status.Trim() : null;
        return new UsersFilter(
            search,
            noRole ? null : roleValue,
            noRole,
            statusValue,
            page is > 0 ? page.Value : 1,
            size is { } chosen && UsersFilterValues.PageSizes.Contains(chosen) ? chosen : UsersFilterValues.DefaultPageSize);
    }
}
