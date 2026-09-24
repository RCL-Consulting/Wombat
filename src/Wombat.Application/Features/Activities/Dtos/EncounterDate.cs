using System.Globalization;

namespace Wombat.Application.Features.Activities.Dtos;

/// <summary>
/// The one wording of an activity's encounter date, wherever a surface shows it as one: the activity lists ([T137]),
/// and the trajectory, the activity page and the portfolio PDF ([T161], D28).
/// </summary>
/// <remarks>
/// <para>
/// <c>Activity.ObservedOn</c> is never null (T119). When nobody stated when the encounter happened, because the type
/// declares no date field or the field was left empty, it holds the filing day and <c>ObservedOnSource</c> is
/// <c>CreatedOn</c>. Printed bare, that date passes the audit clock off as a clinical fact, so an undated one always
/// carries the same qualifier.
/// </para>
/// <para>
/// The qualifier is text, never only a colour, a title attribute or an icon: a keyboard, touch or screen-reader user
/// never sees a title, and a printed portfolio has neither colour nor hover.
/// </para>
/// </remarks>
public static class EncounterDate
{
    /// <summary>
    /// <paramref name="observedOn" /> as an encounter date: the ISO date alone when a clinician stated it, otherwise
    /// the date followed by "(filed; no encounter date)".
    /// </summary>
    /// <param name="observedOn">The stamped <c>Activity.ObservedOn</c>.</param>
    /// <param name="declared">
    /// Whether that date was stated (<c>ObservedOnSource == Declared</c>). False means it is only the filing day.
    /// </param>
    public static string Label(DateOnly observedOn, bool declared)
    {
        var formatted = observedOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return declared ? formatted : $"{formatted} (filed; no encounter date)";
    }
}
