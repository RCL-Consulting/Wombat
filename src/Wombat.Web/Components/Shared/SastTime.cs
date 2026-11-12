using System.Globalization;
using Wombat.Domain.Curricula;

namespace Wombat.Web.Components.Shared;

/// <summary>
/// The one place Web turns a UTC moment into South African words (T400, flow 18, K7, Q7): "2026-11-10 21:39 SAST". South
/// Africa keeps UTC+2 all year with no daylight saving; the day is <see cref="ProgrammeCalendar.DateOf" />'s, so a time
/// and its day never disagree, and a <c>.razor</c> file never names <c>ProgrammeCalendar</c>.
/// </summary>
/// <remarks>
/// <para>
/// A moment is read as UTC whatever its <see cref="DateTime.Kind" />: the database hands timestamps back as UTC, and a
/// value built by hand is taken at its word. Never <c>ToLocalTime()</c>, which is the server's zone.
/// </para>
/// <para>
/// The audit filters' <c>datetime-local</c> inputs are typed in South African time: <see cref="FromSastInput" /> turns
/// what was typed into UTC, and <see cref="ToSastInput" /> a UTC bound back into what the input shows.
/// </para>
/// </remarks>
public static class SastTime
{
    /// <summary>South Africa's offset from UTC, all year.</summary>
    private static readonly TimeSpan Offset = TimeSpan.FromHours(2);

    /// <summary>To the minute: "2026-11-10 21:39 SAST".</summary>
    public static string Minute(DateTime utc) => Format(utc, "yyyy-MM-dd HH:mm") + " SAST";

    /// <summary>A moment with its own offset, to the minute on the South African clock: "2026-09-26 15:14 SAST".</summary>
    public static string Minute(DateTimeOffset moment) => Minute(moment.UtcDateTime);

    /// <summary>To the second: "2026-11-10 21:39:41 SAST".</summary>
    public static string Second(DateTime utc) => Format(utc, "yyyy-MM-dd HH:mm:ss") + " SAST";

    /// <summary>To the millisecond: "2026-11-10 19:10:20.376 SAST" (an audit entry's When, T4).</summary>
    public static string Millisecond(DateTime utc) => Format(utc, "yyyy-MM-dd HH:mm:ss.fff") + " SAST";

    /// <summary>The clock alone, "21:39"; the caller adds " SAST" where its sentence wants it.</summary>
    public static string Clock(DateTime utc) => Format(utc, "HH:mm");

    /// <summary>The South African day, "2026-11-10" (<see cref="ProgrammeCalendar.DateOf" />).</summary>
    public static string Day(DateTime utc)
        => ProgrammeCalendar.DateOf(AsUtc(utc)).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>The moment as stored, to the millisecond in UTC: "2026-11-10 17:01:41.861 UTC" (an audit entry's "Stored as" line, T4).</summary>
    public static string StoredUtc(DateTime utc)
        => AsUtc(utc).ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + " UTC";

    /// <summary>What a <c>datetime-local</c> input typed in South African time means, as a UTC moment.</summary>
    public static DateTime FromSastInput(DateTime sastLocal)
        => DateTime.SpecifyKind(DateTime.SpecifyKind(sastLocal, DateTimeKind.Unspecified) - Offset, DateTimeKind.Utc);

    /// <summary>A UTC moment as a <c>datetime-local</c> input shows it in South African time (kind unspecified).</summary>
    public static DateTime ToSastInput(DateTime utc)
        => DateTime.SpecifyKind(AsUtc(utc) + Offset, DateTimeKind.Unspecified);

    private static string Format(DateTime utc, string pattern)
        => (AsUtc(utc) + Offset).ToString(pattern, CultureInfo.InvariantCulture);

    private static DateTime AsUtc(DateTime utc) => DateTime.SpecifyKind(utc, DateTimeKind.Utc);
}
