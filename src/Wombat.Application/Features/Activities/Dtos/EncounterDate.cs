using System.Globalization;

namespace Wombat.Application.Features.Activities.Dtos;

/// <summary>
/// The one wording of an activity's encounter date, wherever a surface shows it as one: the activity lists ([T137]),
/// the trajectory, the activity page, the portfolio PDF ([T161], D28), and the committee's evidence summary.
/// </summary>
/// <remarks>
/// <para>
/// <c>Activity.ObservedOn</c> is never null (T119). When nobody stated when the encounter happened, because the type
/// declares no date field or the field was left empty, it holds the day the activity was created (on the South African
/// calendar) and <c>ObservedOnSource</c> is <c>CreatedOn</c>. Printed bare, that date passes the audit clock off as a
/// clinical fact, so an undated one always says that no date was recorded, and which date it is instead.
/// </para>
/// <para>
/// It says "created", not "filed" (T197): the fallback is the day the form was first saved, which on a draft is not a
/// filing at all. And it never names an encounter date only to deny it; the surface's own heading or sentence already
/// says what the date is of. A surface with no such heading names it itself ("Encounter …"): a trajectory tooltip, the
/// standing panel's cell under "Latest rating", the PDF's activity line. Bare, "not recorded" there reads as though the
/// rating or the activity were.
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
    /// "not recorded (created yyyy-MM-dd)".
    /// </summary>
    /// <param name="observedOn">The stamped <c>Activity.ObservedOn</c>.</param>
    /// <param name="declared">
    /// Whether that date was stated (<c>ObservedOnSource == Declared</c>). False means it is only the day the activity
    /// was created.
    /// </param>
    public static string Label(DateOnly observedOn, bool declared)
    {
        var formatted = observedOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return declared ? formatted : $"not recorded (created {formatted})";
    }
}
