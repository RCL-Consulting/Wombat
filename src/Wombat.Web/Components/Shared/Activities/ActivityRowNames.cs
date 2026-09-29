using System.Globalization;
using Wombat.Application.Features.Activities.Dtos;

namespace Wombat.Web.Components.Shared.Activities;

/// <summary>
/// An activity as a list row's Open or View names it (T239), in the list's own words and order: its type, whose it is
/// where the list is more than one person's, its EPA's code, and its encounter date as the Encounter date column says it
/// (<see cref="EncounterDate.Label" />): "Mini-CEX for Thandi Nkosi, PAED-003, encounter date 2026-09-01".
/// </summary>
/// <remarks>
/// Two rows can show the same in every one of those columns (two Mini-CEX on one EPA on one day). A name two rows share
/// adds each one's state, as the State column says it; in the inbox, then when each was last updated, to the minute, as
/// its Updated column says it. The subject's own list has no Updated column, so it does not name one. Rows still alike
/// are numbered in list order (<see cref="RowNames.Distinct{TItem, TKey}" />): two drafts saved in the same minute read
/// the same in every column.
/// </remarks>
public static class ActivityRowNames
{
    /// <summary>Each activity's name on one list, by activity id; no two are the same.</summary>
    /// <param name="activities">The list's rows.</param>
    /// <param name="withSubject">Whether the list is an inbox, many people's: it names whose activity each is, and when it was updated.</param>
    public static IReadOnlyDictionary<int, string> For(IEnumerable<ActivitySummaryDto> activities, bool withSubject)
    {
        List<Func<ActivitySummaryDto, string>> tieBreakers = [activity => $", {activity.CurrentStateLabel}"];
        if (withSubject)
        {
            tieBreakers.Add(activity =>
                $", updated {activity.UpdatedOn.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)}");
        }

        return RowNames.Distinct(activities, activity => activity.Id, activity => Base(activity, withSubject), [.. tieBreakers]);
    }

    /// <summary>
    /// Each activity link's accessible name on My activities and in Needs you (T342, flow 03; T280, A16), by activity id:
    /// the words the link shows, its name and, after a visually hidden ", ", its second line ("Mini-CEX (Paediatrics) ·
    /// PAED-003 · 2026-09-25, to David Naidoo"; <see cref="ActivityListWords" />). The name already carries the nominee
    /// where two share the rest (E7), so a name two links still share is rare: two drafts saved with neither EPA nor date.
    /// Those add their state, then are numbered in list order (<see cref="RowNames.Distinct{TItem, TKey}" />), and only
    /// those need an <c>aria-label</c>: every other link's own words are its name. One activity listed twice on a page
    /// (in Needs you and in All activities) is one id, one name.
    /// </summary>
    public static IReadOnlyDictionary<int, string> Links(IEnumerable<ActivitySummaryDto> activities)
        => RowNames.Distinct(
            activities.DistinctBy(activity => activity.Id),
            activity => activity.Id,
            LinkWords,
            activity => $", {activity.CurrentStateLabel}");

    /// <summary>What a link reads when nothing else on its list shares it: its name, then ", " and its second line.</summary>
    public static string LinkWords(ActivitySummaryDto activity)
    {
        ArgumentNullException.ThrowIfNull(activity);
        var name = ActivityListWords.NameOf(activity);
        return ActivityListWords.NomineeLine(activity) is { } line ? $"{name}, {line}" : name;
    }

    private static string Base(ActivitySummaryDto activity, bool withSubject)
    {
        var parts = new List<string>
        {
            withSubject && !string.IsNullOrWhiteSpace(activity.SubjectName)
                ? $"{activity.ActivityTypeName} for {activity.SubjectName}"
                : activity.ActivityTypeName
        };

        if (!string.IsNullOrWhiteSpace(activity.EpaCode))
        {
            parts.Add(activity.EpaCode);
        }

        parts.Add($"encounter date {EncounterDate.Label(activity.ObservedOn, activity.ObservedOnDeclared)}");
        return string.Join(", ", parts);
    }
}
