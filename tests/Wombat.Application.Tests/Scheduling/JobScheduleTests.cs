using FluentAssertions;
using Wombat.Application.Scheduling;

namespace Wombat.Application.Tests.Scheduling;

/// <summary>
/// T400 (flow 18, K4, K5, Q3; build-contracts § 0): <see cref="JobSchedule" />, the scheduler's rule that Home and the
/// Scheduled jobs page judge by. The next run is the cron's next occurrence after the last run's start (after a day ago
/// for a job never run); due when it is not after now; overdue an hour past it, never while the last run is running.
/// </summary>
public sealed class JobScheduleTests
{
    // 2026-11-10 is a Tuesday. 05:00 UTC is 07:00 SAST.
    private static readonly DateTime LastRun = Utc(2026, 11, 10, 5, 0);

    /// <summary>The eight registered jobs' crons, each read as the host reads it (UTC), from a last run at 05:00 UTC.</summary>
    [Theory]
    [InlineData("0 7 * * *", "2026-11-10 07:00")]   // activity-draft-nudge
    [InlineData("0 9 * * *", "2026-11-10 09:00")]   // assessor-pending-nudge
    [InlineData("0 3 * * *", "2026-11-11 03:00")]   // audit-log-retention
    [InlineData("30 3 * * *", "2026-11-11 03:30")]  // entrustment-decision-expiry
    [InlineData("0 * * * *", "2026-11-10 06:00")]   // msf-campaign-auto-close
    [InlineData("0 8 * * *", "2026-11-10 08:00")]   // msf-invitation-expiry-reminder
    [InlineData("0 4 * * *", "2026-11-11 04:00")]   // scheduled-job-run-retention
    [InlineData("0 8 * * 1", "2026-11-16 08:00")]   // weekly-coordinator-digest: the next Monday
    public void EachJobsCron_RunsNextAtItsNextOccurrenceAfterTheLastRun_AndIsDueFromThen(string cron, string next)
    {
        var expected = DateTime.SpecifyKind(DateTime.Parse(next, System.Globalization.CultureInfo.InvariantCulture), DateTimeKind.Utc);
        JobSchedule.TryParse(cron, out var expression).Should().BeTrue();

        JobSchedule.NextAfter(expression!, LastRun, LastRun.AddMinutes(1)).Should().Be(expected);
        JobSchedule.IsDue(expression!, LastRun, expected.AddTicks(-1)).Should().BeFalse("not before its time");
        JobSchedule.IsDue(expression!, LastRun, expected).Should().BeTrue("due at its time, as the host's <= has it");
    }

    [Fact]
    public void AJobNeverRun_CountsFromADayBeforeNow_SoItsLastMissedOccurrenceIsDue()
    {
        var cron = Parse("0 4 * * *");
        var now = Utc(2026, 11, 10, 5, 0);

        JobSchedule.NextAfter(cron, null, now).Should().Be(Utc(2026, 11, 10, 4, 0));
        JobSchedule.IsDue(cron, null, now).Should().BeTrue();
        JobSchedule.NextAfter(cron, null, Utc(2026, 11, 10, 3, 59)).Should().Be(Utc(2026, 11, 9, 4, 0),
            "a day before 03:59 is 03:59 yesterday, and yesterday's 04:00 follows it: a daily job never run is always due");

        // A weekly job never run is due only when its time fell in the last day: at Tuesday 07:59, Monday 08:00 is 23:59
        // ago; at Tuesday 08:00 it is a whole day ago, so the job waits for the next Monday.
        var weekly = Parse("0 8 * * 1");
        JobSchedule.IsDue(weekly, null, Utc(2026, 11, 10, 7, 59)).Should().BeTrue();
        JobSchedule.NextAfter(weekly, null, Utc(2026, 11, 10, 8, 0)).Should().Be(Utc(2026, 11, 16, 8, 0));
        JobSchedule.IsDue(weekly, null, Utc(2026, 11, 10, 8, 0)).Should().BeFalse();
    }

    /// <summary>The build review's N-2: a moment read back with no kind is the UTC it was stored as, never a throw.</summary>
    [Fact]
    public void AMomentWithNoKind_IsReadAsUtc()
    {
        var cron = Parse("0 * * * *");
        var lastRun = DateTime.SpecifyKind(Utc(2026, 11, 10, 19, 0), DateTimeKind.Unspecified);
        var now = DateTime.SpecifyKind(Utc(2026, 11, 10, 19, 30), DateTimeKind.Unspecified);

        JobSchedule.NextAfter(cron, lastRun, now).Should().Be(Utc(2026, 11, 10, 20, 0));
        JobSchedule.NextAfter(cron, null, now).Should().Be(Utc(2026, 11, 9, 20, 0));
    }

    [Fact]
    public void AHourlyJobLastRunAt2100Sast_RunsNextAt2200Sast()
    {
        // R3-J03's K5 case: 19:00 UTC is 21:00 SAST; the next hour is 22:00 SAST, 20:00 UTC, and not before.
        var cron = Parse("0 * * * *");
        var lastRun = Utc(2026, 11, 10, 19, 0, 2);

        JobSchedule.NextAfter(cron, lastRun, Utc(2026, 11, 10, 19, 30)).Should().Be(Utc(2026, 11, 10, 20, 0));
        JobSchedule.IsDue(cron, lastRun, Utc(2026, 11, 10, 19, 59, 59)).Should().BeFalse();
        JobSchedule.IsDue(cron, lastRun, Utc(2026, 11, 10, 20, 0)).Should().BeTrue();
    }

    [Fact]
    public void AJobDisabledPastItsTime_IsDueAtOnce_WhenEnabled()
    {
        // K5: its next occurrence after its last run has passed, so the host's next tick (within 30 seconds) runs it; the
        // page says "At the next check, within 30 seconds", never the next occurrence after now.
        var cron = Parse("0 4 * * *");
        var lastRun = Utc(2026, 11, 7, 4, 0, 1);
        var now = Utc(2026, 11, 10, 12, 0);

        JobSchedule.NextAfter(cron, lastRun, now).Should().Be(Utc(2026, 11, 8, 4, 0));
        JobSchedule.IsDue(cron, lastRun, now).Should().BeTrue();
    }

    [Fact]
    public void ARunningJob_IsNeverOverdue()
    {
        // K4: a running run holds the job's lock, so the host cannot start it again; it is late, not missed.
        var cron = Parse("0 * * * *");
        var lastRun = Utc(2026, 11, 10, 5, 0);
        var now = Utc(2026, 11, 10, 9, 0);

        JobSchedule.IsOverdue(cron, lastRun, lastRunRunning: true, now).Should().BeFalse();
        JobSchedule.IsOverdue(cron, lastRun, lastRunRunning: false, now).Should().BeTrue();
    }

    [Fact]
    public void AJob_IsOverdueOnlyPastOneHourOfGrace()
    {
        var cron = Parse("0 4 * * *");
        var lastRun = Utc(2026, 11, 9, 4, 0, 1);
        var next = Utc(2026, 11, 10, 4, 0);

        JobSchedule.OverdueGrace.Should().Be(TimeSpan.FromHours(1));
        JobSchedule.IsOverdue(cron, lastRun, false, next).Should().BeFalse("due, not overdue");
        JobSchedule.IsOverdue(cron, lastRun, false, next + JobSchedule.OverdueGrace).Should().BeFalse("one hour exactly is within the grace");
        JobSchedule.IsOverdue(cron, lastRun, false, next + JobSchedule.OverdueGrace + TimeSpan.FromTicks(1)).Should().BeTrue();
    }

    [Theory]
    [InlineData("not a cron")]
    [InlineData("61 * * * *")]
    [InlineData("0 0 7 * * *")]
    [InlineData("")]
    [InlineData("   ")]
    public void ACronThatDoesNotParse_IsRefusedWithoutAThrow(string cron)
    {
        JobSchedule.TryParse(cron, out var expression).Should().BeFalse();
        expression.Should().BeNull();
    }

    private static Cronos.CronExpression Parse(string cron)
    {
        JobSchedule.TryParse(cron, out var expression).Should().BeTrue();
        return expression!;
    }

    private static DateTime Utc(int year, int month, int day, int hour, int minute, int second = 0)
        => new(year, month, day, hour, minute, second, DateTimeKind.Utc);
}
