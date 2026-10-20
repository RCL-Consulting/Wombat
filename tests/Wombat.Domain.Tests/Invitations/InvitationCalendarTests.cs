using Wombat.Domain.Invitations;

namespace Wombat.Domain.Tests.Invitations;

/// <summary>
/// T373 (flow 11, round 3 F7 and S4; build-contracts § 0): a link's last day is a day on the South African calendar, and
/// an address is compared as one spelling. Until T373 the last day was the UTC date's, so a link issued at 01:00 SAST had 13
/// of its reader's days, and the boundary fell at 02:00 SAST.
/// </summary>
public sealed class InvitationCalendarTests
{
    [Fact]
    public void ALinkIssuedAt2230Utc_CountsItsFourteenDaysFromTheSouthAfricanDay_WhichIsAlreadyTomorrow()
    {
        // 2026-10-19 22:30 UTC is 2026-10-20 00:30 SAST.
        var issued = new DateTime(2026, 10, 19, 22, 30, 0, DateTimeKind.Utc);

        Assert.Equal(new DateOnly(2026, 11, 3), Invitation.LinkExpiresOn(issued));
    }

    [Fact]
    public void ALinkIssuedInTheSouthAfricanMorning_WorksUntilFourteenDaysLater()
    {
        // 2026-10-19 07:22 UTC is 09:22 SAST, the cast's Trainee invitations: last day 2026-11-02.
        var issued = new DateTime(2026, 10, 19, 7, 22, 0, DateTimeKind.Utc);

        Assert.Equal(new DateOnly(2026, 11, 2), Invitation.LinkExpiresOn(issued));
    }

    [Theory]
    [InlineData(21, 59, false)] // 23:59 SAST on the last day: it still works
    [InlineData(22, 0, true)]   // 00:00 SAST the next day: it has passed
    public void ALinkPassesItsLastDay_AtSouthAfricanMidnight(int hour, int minute, bool passed)
    {
        var lastDay = new DateOnly(2026, 11, 2);
        var at = new DateTime(2026, 11, 2, hour, minute, 0, DateTimeKind.Utc);

        Assert.Equal(passed, Invitation.HasPassedLastDay(lastDay, at));
    }

    [Fact]
    public void ALinkHasNotPassedItsLastDay_OnTheDayBefore()
    {
        Assert.False(Invitation.HasPassedLastDay(new DateOnly(2026, 11, 2), new DateTime(2026, 10, 31, 23, 0, 0, DateTimeKind.Utc)));
    }

    [Theory]
    [InlineData("molefe@kgk.wombat.local", "molefe@kgk.wombat.local")]
    [InlineData("  Molefe@KGK.Wombat.Local ", "molefe@kgk.wombat.local")]
    [InlineData("NDLVOU@kgk.wombat.local", "ndlvou@kgk.wombat.local")]
    public void AnAddress_IsComparedTrimmedAndLowerCased(string typed, string normalised)
    {
        Assert.Equal(normalised, Invitation.NormaliseAddress(typed));
    }
}
