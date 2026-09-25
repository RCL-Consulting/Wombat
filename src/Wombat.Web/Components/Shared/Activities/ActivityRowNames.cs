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
