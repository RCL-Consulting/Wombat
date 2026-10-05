using Wombat.Domain.Activities;

namespace Wombat.Domain.Tests.Activities;

/// <summary>
/// T358 (flow 06): a reminder's day is the South African day of its moment, read once where it is recorded, so the
/// same-day block and the "Reminded 2026-10-04" line cannot disagree with when it was sent.
/// </summary>
public sealed class ActivityReminderTests
{
    [Theory]
    [InlineData(2026, 10, 4, 6, 0, 4)]
    [InlineData(2026, 10, 4, 21, 59, 4)]
    [InlineData(2026, 10, 4, 22, 0, 5)]
    public void Record_ReadsTheDayOnTheSouthAfricanCalendar(int year, int month, int day, int hour, int minute, int dayInOctober)
    {
        var sentOn = new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc);

        var reminder = ActivityReminder.Record(42, "smit", "zulu", sentOn);

        Assert.Equal(42, reminder.ActivityId);
        Assert.Equal("smit", reminder.SentByUserId);
        Assert.Equal("zulu", reminder.AssessorUserId);
        Assert.Equal(sentOn, reminder.SentOn);
        Assert.Equal(new DateOnly(2026, 10, dayInOctober), reminder.SentOnDay);
    }

    [Fact]
    public void Record_RefusesAMomentThatIsNotUtc_AndAnUnnamedSenderOrAssessor()
    {
        var local = new DateTime(2026, 10, 4, 8, 0, 0, DateTimeKind.Unspecified);
        var utc = DateTime.SpecifyKind(local, DateTimeKind.Utc);

        Assert.Throws<ArgumentException>(() => ActivityReminder.Record(1, "smit", "zulu", local));
        Assert.Throws<ArgumentException>(() => ActivityReminder.Record(1, " ", "zulu", utc));
        Assert.Throws<ArgumentException>(() => ActivityReminder.Record(1, "smit", "", utc));
    }
}
