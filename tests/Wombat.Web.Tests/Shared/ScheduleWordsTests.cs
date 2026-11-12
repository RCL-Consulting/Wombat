using FluentAssertions;
using Wombat.Web.Components.Pages.Admin.Jobs;

namespace Wombat.Web.Tests.Shared;

/// <summary>
/// A job's schedule in words, in South African time, with its cron under it (T400, flow 18, T2, K7; R3-J01): the eight
/// jobs' crons, the fallback for any other shape, and a UTC hour that crosses midnight there, moving a weekly job's day.
/// </summary>
public sealed class ScheduleWordsTests
{
    [Theory]
    [InlineData("0 7 * * *", "Daily at 09:00 SAST")]      // activity-draft-nudge
    [InlineData("0 9 * * *", "Daily at 11:00 SAST")]      // assessor-pending-nudge
    [InlineData("0 3 * * *", "Daily at 05:00 SAST")]      // audit-log-retention
    [InlineData("30 3 * * *", "Daily at 05:30 SAST")]     // entrustment-decision-expiry
    [InlineData("0 * * * *", "Hourly, on the hour")]      // msf-campaign-auto-close
    [InlineData("0 8 * * *", "Daily at 10:00 SAST")]      // msf-invitation-expiry-reminder
    [InlineData("0 4 * * *", "Daily at 06:00 SAST")]      // scheduled-job-run-retention
    [InlineData("0 8 * * 1", "Mondays at 10:00 SAST")]    // weekly-coordinator-digest
    public void TheEightJobsCrons_ReadAsTheBoardsDrawThem(string cron, string words)
    {
        ScheduleWords.Words(cron).Should().Be(words);
        ScheduleWords.CronLine(cron).Should().Be($"cron {cron}, UTC");
    }

    [Theory]
    [InlineData("0 22 * * *", "Daily at 00:00 SAST")]
    [InlineData("45 23 * * *", "Daily at 01:45 SAST")]
    [InlineData("0 23 * * 1", "Tuesdays at 01:00 SAST")]
    [InlineData("0 22 * * 6", "Sundays at 00:00 SAST")]
    [InlineData("0 23 * * 0", "Mondays at 01:00 SAST")]
    [InlineData("0 23 * * 7", "Mondays at 01:00 SAST")]
    [InlineData("0 21 * * 5", "Fridays at 23:00 SAST")]
    public void AUtcHourThatCrossesMidnight_MovesTheDayWithIt(string cron, string words)
        => ScheduleWords.Words(cron).Should().Be(words);

    [Theory]
    [InlineData("*/5 * * * *")]
    [InlineData("15 * * * *")]
    [InlineData("0 */2 * * *")]
    [InlineData("0 7 1 * *")]
    [InlineData("0 7 * 6 *")]
    [InlineData("0 7 * * 1-5")]
    [InlineData("0 7,19 * * *")]
    [InlineData("0 24 * * *")]
    [InlineData("not a cron")]
    [InlineData("")]
    public void AnyOtherCron_HasNoWords_ItsCronLineIsAll(string cron)
        => ScheduleWords.Words(cron).Should().BeNull();

    [Fact]
    public void TheCronLine_IsTheCronAsStored_InUtc()
        => ScheduleWords.CronLine("*/5 * * * *").Should().Be("cron */5 * * * *, UTC");
}
