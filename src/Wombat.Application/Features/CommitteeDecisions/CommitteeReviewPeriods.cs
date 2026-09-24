using System.Globalization;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// A period a review can be scheduled for, with the evidence window the scheduling page fills in when it is chosen.
/// </summary>
/// <param name="Key">"2026-1": the select's value.</param>
/// <param name="Label">"2026 S1 · 1 Jan to 30 Jun 2026".</param>
/// <param name="WindowFrom">
/// The first day of the period's academic year, the earliest day of any decision window a sitting for the period closes:
/// a semester-2 sitting decides the annual EPAs, whose window is the whole year (Decision 5), so their evidence starts
/// in semester 1. For a semester-1 period it is the period's own first day.
/// </param>
/// <param name="WindowTo">The period's last counted day (<see cref="AcademicPeriod.End" />): semester 2 holds the year's last month (D13).</param>
public sealed record CommitteeReviewPeriodOptionDto(
    string Key,
    int AcademicYear,
    int Semester,
    string Label,
    DateOnly WindowFrom,
    DateOnly WindowTo);

/// <summary>
/// The periods the scheduling page offers, read from <see cref="AcademicPeriod" /> and nothing else: the committee has
/// no calendar of its own (T131, Decision 4, and the brief's "no second calendar").
/// </summary>
/// <remarks>
/// The evidence window each fills runs from the start of the period's academic year to the period's end. A window of
/// only the semester would leave half the year out of the snapshot, the evidence counts and the staging picker for the
/// six annual EPAs that close at a semester-2 sitting (T131 slice 4 review). A semester EPA decided at that sitting sees
/// semester 1's evidence too, in the one snapshot; the scheduler can still narrow the window.
/// </remarks>
public static class CommitteeReviewPeriods
{
    /// <summary>How many semesters back the select reaches: a sitting is often held after its period ends.</summary>
    public const int SemestersBack = 3;

    /// <summary>How many semesters ahead the select reaches.</summary>
    public const int SemestersAhead = 2;

    /// <summary>
    /// The period holding <paramref name="today" /> and the semesters around it, oldest first. A day in the year's last
    /// month is in semester 2 (<see cref="AcademicPeriod.Containing" />, D13), so a review scheduled then defaults to that
    /// year's semester 2.
    /// </summary>
    public static IReadOnlyList<CommitteeReviewPeriodOptionDto> Around(DateOnly today)
    {
        var current = AcademicPeriod.Containing(today);

        var earlier = new List<AcademicPeriod>();
        var before = current.Previous();
        while (before is { } previous && earlier.Count < SemestersBack)
        {
            earlier.Add(previous);
            before = previous.Previous();
        }

        var later = new List<AcademicPeriod>();
        var after = current.Next();
        while (after is { } next && later.Count < SemestersAhead)
        {
            later.Add(next);
            after = next.Next();
        }

        return earlier
            .AsEnumerable()
            .Reverse()
            .Append(current)
            .Concat(later)
            .Select(ToOption)
            .ToArray();
    }

    /// <summary>The period holding <paramref name="today" />: the scheduling page's default.</summary>
    public static CommitteeReviewPeriodOptionDto Current(DateOnly today) => ToOption(AcademicPeriod.Containing(today));

    private static CommitteeReviewPeriodOptionDto ToOption(AcademicPeriod period)
        => new(
            string.Create(CultureInfo.InvariantCulture, $"{period.Year}-{period.Semester}"),
            period.Year,
            period.Semester,
            string.Create(
                CultureInfo.InvariantCulture,
                $"{period} · {period.Start:d MMM} to {period.End:d MMM yyyy}"),
            AcademicPeriod.SemestersOf(period.Year)[0].Start,
            period.End);
}
