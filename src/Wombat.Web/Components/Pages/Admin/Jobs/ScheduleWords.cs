using System.Globalization;

namespace Wombat.Web.Components.Pages.Admin.Jobs;

/// <summary>
/// A job's schedule in words, in South African time, with its cron under it (T400, flow 18, T2, K7; R3-J01): "Daily at
/// 09:00 SAST" over "cron 0 7 * * *, UTC". The jobs' three shapes have words (daily at a time, hourly on the hour, weekly
/// on a day at a time); any other cron is its cron line alone.
/// </summary>
/// <remarks>
/// South Africa keeps UTC+2 all year, so a UTC hour of 22 or 23 is the next day's 00 or 01 there, and a weekly job's day
/// moves with it: "0 23 * * 1" is "Tuesdays at 01:00 SAST".
/// </remarks>
public static class ScheduleWords
{
    private const int MinutesADay = 24 * 60;

    // South Africa's offset from UTC, all year (SastTime's, in minutes).
    private const int SouthAfricanOffsetMinutes = 120;

    private static readonly string[] Weekdays =
        ["Sundays", "Mondays", "Tuesdays", "Wednesdays", "Thursdays", "Fridays", "Saturdays"];

    /// <summary>The schedule in words, or null where the cron is none of the three shapes (its cron line is then all).</summary>
    public static string? Words(string? cron)
    {
        var fields = Fields(cron);
        if (fields.Length != 5 || fields[2] != "*" || fields[3] != "*")
        {
            return null;
        }

        if (fields[1] == "*")
        {
            return fields[0] == "0" && fields[4] == "*" ? "Hourly, on the hour" : null;
        }

        if (Number(fields[0], 59) is not { } minute || Number(fields[1], 23) is not { } hour)
        {
            return null;
        }

        var sast = (hour * 60) + minute + SouthAfricanOffsetMinutes;
        var nextDay = sast >= MinutesADay ? 1 : 0;
        sast %= MinutesADay;
        var time = $"{sast / 60:00}:{sast % 60:00} SAST";

        if (fields[4] == "*")
        {
            return $"Daily at {time}";
        }

        // Cron counts Sunday as 0 or 7.
        return Number(fields[4], 7) is { } weekday ? $"{Weekdays[(weekday + nextDay) % 7]} at {time}" : null;
    }

    /// <summary>"cron 0 7 * * *, UTC": the cron as stored, read in UTC, as the scheduler reads it.</summary>
    public static string CronLine(string? cron) => $"cron {string.Join(' ', Fields(cron))}, UTC";

    private static string[] Fields(string? cron) => (cron ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);

    // A plain number from 0 to max: no sign, no list, no range, no step.
    private static int? Number(string field, int max)
        => int.TryParse(field, NumberStyles.None, CultureInfo.InvariantCulture, out var value) && value <= max ? value : null;
}
