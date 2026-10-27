namespace Wombat.Domain.Curricula;

/// <summary>
/// The calendar date of an instant in South Africa, where the national programme runs (T130).
/// </summary>
/// <remarks>
/// <para>
/// A clinician types an encounter date as a South African calendar date. Everything else that turns an instant
/// into a date has to agree with that, or the two disagree for the first two hours of every day. At a semester
/// boundary that means a different bucket. There are two such places: "today" for the progress pages, and the
/// encounter date of an activity nobody dated, which falls back to when it was filed.
/// </para>
/// <para>
/// South Africa keeps UTC+2 all year, with no daylight saving, so a fixed offset is exact and needs no time-zone
/// database, which the Linux host and the Windows development machines would otherwise have to agree on.
/// </para>
/// </remarks>
public static class ProgrammeCalendar
{
    private static readonly TimeSpan SouthAfricanOffset = TimeSpan.FromHours(2);

    /// <summary>The South African calendar date of a UTC instant.</summary>
    public static DateOnly DateOf(DateTime utc) => DateOnly.FromDateTime(utc + SouthAfricanOffset);

    /// <summary>
    /// The UTC instant a South African day ends: the first instant of the next day there, so every instant whose
    /// <see cref="DateOf" /> is <paramref name="day" /> is before it (T312).
    /// </summary>
    public static DateTime EndOf(DateOnly day)
        => DateTime.SpecifyKind(day.AddDays(1).ToDateTime(TimeOnly.MinValue) - SouthAfricanOffset, DateTimeKind.Utc);
}
