using Wombat.Application.Features.Activities.Dtos;

namespace Wombat.Web.Components.Shared.Activities;

/// <summary>
/// An activity as a list row's Open or View names it (T239), in the list's own words and order: its type, whose it is
/// where the list is more than one person's, its EPA's code, and its encounter date as the Encounter date column says it
/// (<see cref="EncounterDate.Label" />): "Mini-CEX for Thandi Nkosi, PAED-003, encounter date 2026-09-01".
/// </summary>
/// <remarks>
/// Two rows can show the same in every one of those columns (two Mini-CEX on one EPA on one day). A name two rows share
/// adds each one's state, as the State column says it; in the inbox, then when each was last updated, to the minute, in
/// South African time with its zone (T350, note 8). The subject's own list has no Updated column, so it does not name one.
/// Rows still alike
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
            // SAST, never the server's zone (T350, note 8): "2026-09-30 08:12 SAST".
            tieBreakers.Add(activity => $", updated {ActivityMoments.When(activity.UpdatedOn)}");
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

    /// <summary>
    /// Each link's accessible name on a waiting list (<c>WaitingList</c>: the Assessor's Home, the Activity inbox and the
    /// way on), by activity id (T350, note 9; C11): the words the link shows, its name and ", from &lt;registrar&gt;"
    /// (<see cref="WaitingLinkWords" />). Two that read the same (two requests from one registrar, on one EPA and date) add
    /// their state, then ", waiting since 2026-09-30 08:12 SAST", then are numbered in list order, "(1 of 2)"; only those
    /// need an <c>aria-label</c>. The tie-breakers are on the link, where the list's words are, not on an Open button.
    /// </summary>
    public static IReadOnlyDictionary<int, string> Waiting(IEnumerable<ActivitySummaryDto> activities)
        => RowNames.Distinct(
            activities.DistinctBy(activity => activity.Id),
            activity => activity.Id,
            WaitingLinkWords,
            activity => $", {activity.CurrentStateLabel}",
            activity => $", waiting since {ActivityMoments.When(activity.UpdatedOn)}");

    /// <summary>
    /// What a waiting row's link reads when nothing else on its list shares it: its name, then ", from &lt;registrar&gt;"
    /// (<see cref="ActivityListWords.FromLine" />), "Mini-CEX (Paediatrics) · PAED-001 · 2026-09-20, from Anele Dlamini".
    /// </summary>
    public static string WaitingLinkWords(ActivitySummaryDto activity)
    {
        ArgumentNullException.ThrowIfNull(activity);
        var name = ActivityListWords.NameOf(activity);
        return ActivityListWords.FromLine(activity) is { } line ? $"{name}, {line}" : name;
    }

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
