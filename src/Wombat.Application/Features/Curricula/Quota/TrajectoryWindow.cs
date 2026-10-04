using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Curricula.Quota;

/// <summary>
/// The span a trainee's rating chart is drawn over on their EPA page: one academic year (T355, Q5; R4).
/// </summary>
/// <remarks>
/// The College's academic year runs January to November, and December's encounters count into it (D40), so the year a
/// day belongs to is its calendar year: 1 January to 31 December, the two semesters of <see cref="AcademicPeriod" />
/// back to back. The EPA page passes today's, or the ended programme's last day's; the committee page passes its
/// review's own window instead (T355, decision D2).
/// </remarks>
public static class TrajectoryWindow
{
    /// <summary>1 January to 31 December of the academic year containing <paramref name="day" />.</summary>
    public static (DateOnly From, DateOnly To) AcademicYearOf(DateOnly day)
        => (new AcademicPeriod(day.Year, 1).Start, new AcademicPeriod(day.Year, 2).End);
}
