using System.Globalization;
using System.Security.Claims;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;

namespace Wombat.Web.Components.Pages.Admin.ActivityTypes;

/// <summary>
/// The activity-type list's words (T398, flow 17, lane B; boards R2-A01–A12, R3-A09-nocreate, R3-A12, R3-A13,
/// R3-C08-list): the subtitle by role (Q12, T11, N12), the search and its count line (Q7, T326), the empty catalogue and
/// the no-match state each in its own words, the loading line and the load failure (T329). The words both activity-type
/// pages print (an owner, a scope, a version, a draft) are <see cref="ActivityTypeWords" />'s. Words no board draws are
/// marked NEW.
/// </summary>
public static class ActivityTypesListWords
{
    private const string Lead = "The forms registrars file and assessors complete.";

    /// <summary>An institution's subtitle (Q12, R2-A01).</summary>
    public const string InstitutionSubtitle = Lead + " You can change the types you set; the College sets its instruments.";

    /// <summary>The Administrator's subtitle (R2-A08): every type is theirs to change but the two the releases write (Q9).</summary>
    public const string AdministratorSubtitle = Lead + " You can change every type but the two that Wombat's releases write.";

    /// <summary>
    /// The subtitle by role, never by count (Q12, T11): the Administrator's, a College admin's (their College by name, or
    /// N12's when it holds no speciality), else an institution's. Null for a College admin until the list has said which
    /// College is theirs; the lead alone for one whose account names no College (NEW: no board draws that account).
    /// </summary>
    public static string? Subtitle(ClaimsPrincipal user, ActivityTypeAdminListDto? list)
    {
        if (user.IsAdministrator())
        {
            return AdministratorSubtitle;
        }

        if (!user.IsCollegeAdmin())
        {
            return InstitutionSubtitle;
        }

        if (list is null)
        {
            return null;
        }

        return list.CallerCollegeName is not { } college
            ? Lead
            : list.CallerCollegeHasSpeciality
                ? $"{Lead} You can change the types {college} sets; the others you can read."
                : $"{Lead} {college} has no speciality yet, so there are no types for you to change.";
    }

    public const string NewActivityType = "New activity type";

    public const string SearchName = "Search activity types";

    public const string SearchLabel = "Search by name or key";

    public const string Show = "Show";

    public const string Clear = "Clear";

    public const string ClearTheSearch = "Clear the search";

    /// <summary>The table's caption, which does not repeat the heading (R2-A01, R2-A02).</summary>
    public static string Caption(string? applied)
        => applied is null ? "Activity types you can read or change" : $"Activity types matching “{applied}”";

    /// <summary>
    /// The count line above the table (R2-A01, R2-A02, R2-A03): "23 activity types.", "12 of 22 activity types match
    /// “cpsa”."; each with its singular ("1 activity type.", and NEW "1 of 1 activity type matches “x”.").
    /// </summary>
    public static string CountLine(string? applied, int matching, int total)
    {
        if (applied is null)
        {
            return total == 1 ? "1 activity type." : string.Create(CultureInfo.InvariantCulture, $"{total} activity types.");
        }

        return total == 1
            ? string.Create(CultureInfo.InvariantCulture, $"{matching} of 1 activity type matches “{applied}”.")
            : string.Create(CultureInfo.InvariantCulture, $"{matching} of {total} activity types match “{applied}”.");
    }

    /// <summary>The no-match state's title (R2-A04, T326), apart from the empty catalogue's.</summary>
    public static string NoMatchTitle(string applied) => $"No activity types match \"{applied}\".";

    /// <summary>
    /// The no-match state's body (R2-A04): "The search is kept above. Clear it to see all 23."; with one type, NEW "Clear it
    /// to see the one activity type.".
    /// </summary>
    public static string NoMatchBody(int total)
        => total == 1
            ? "The search is kept above. Clear it to see the one activity type."
            : string.Create(CultureInfo.InvariantCulture, $"The search is kept above. Clear it to see all {total}.");

    /// <summary>The empty catalogue (R2-A09-create, R2-A09-nocreate, R3-A09-nocreate), by whether the caller may create.</summary>
    public const string EmptyTitle = "No activity types";

    public const string EmptyBodyMayCreate = "Create the first activity type to start the builder.";

    public const string EmptyBodyMayNotCreate = "No activity type is open to you yet.";

    public const string Loading = "Loading the activity types.";

    /// <summary>The load failure (R2-A11, T329): fixed words, the exception's text never shown.</summary>
    public const string LoadFailed = "Could not load the activity types. Nothing has changed. Try again, or come back in a few minutes.";

    public const string Edit = "Edit";

    public const string View = "View";

    /// <summary>The Status cell (R2-A01, R3-A12).</summary>
    public static string Status(bool isActive) => isActive ? "Active" : "Inactive";

    /// <summary>The Draft cell (R2-A05): "Saved 2026-11-05", the South African day of the save, or "None".</summary>
    public static string Draft(ActivityTypeAdminListItemDto item)
        => ActivityTypeWords.Draft(item.HasDraft ? item.DraftUpdatedOn : null);

    /// <summary>
    /// The rows in the list's order (Q11, N13): the caller's own (<c>CanWrite</c>) first, each part by name, compared
    /// ordinally ignoring case ("DOPS" after "Direct Observation"); the Administrator's by name alone (R2-A08), since every
    /// type but the releases' is theirs.
    /// </summary>
    public static IReadOnlyList<ActivityTypeAdminListItemDto> Ordered(IEnumerable<ActivityTypeAdminListItemDto> items, bool byNameAlone)
    {
        return items
            .OrderBy(item => byNameAlone || item.CanWrite ? 0 : 1)
            .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// Whose the row's type is: the query's owner, or, where a row carries none, one read from its scope alone (the
    /// institution by the target's name; a College unnamed, "Its College").
    /// </summary>
    public static ActivityTypeOwnerDto Owner(ActivityTypeAdminListItemDto item)
        => item.Owner ?? item.Scope switch
        {
            ActivityScope.Speciality or ActivityScope.SubSpeciality
                => new ActivityTypeOwnerDto(ActivityTypeOwnerKind.College, null),
            ActivityScope.Institution => new ActivityTypeOwnerDto(ActivityTypeOwnerKind.Institution, item.ScopeTargetName),
            _ => new ActivityTypeOwnerDto(ActivityTypeOwnerKind.Platform, null)
        };

    /// <summary>Whether a row answers the applied search: its name or key, ignoring case (Q7).</summary>
    public static bool Matches(ActivityTypeAdminListItemDto item, string applied)
        => item.Name.Contains(applied, StringComparison.OrdinalIgnoreCase)
           || item.Key.Contains(applied, StringComparison.OrdinalIgnoreCase);
}
