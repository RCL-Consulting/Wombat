using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Programme.Commands.SendActivityReminder;
using Wombat.Application.Features.Programme.Waiting;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling;
using Wombat.Tests.Shared;

namespace Wombat.Infrastructure.Tests.Scheduling;

/// <summary>
/// T358 (flow 06; review 2; E1): the recipient port a staff reminder reads, over the periodic reminders' projection. It
/// reports each account as the digests read it, deactivation by <see cref="UserDeactivation" /> and nothing of the
/// opt-out; the rule a reminder applies (<see cref="ReminderRecipientRules" />) refuses a missing account, a deactivated
/// one and a missing address, in that order, and never an opt-out of digest emails.
/// </summary>
public sealed class ReminderRecipientsTests
{
    [Fact]
    public async Task EachAccount_IsReportedAsTheDigestsReadIt_AndAnIdNamingNoAccountIsAbsent()
    {
        await using var db = CreateDb();
        var zulu = NomineeSeed.AddUser(db, "zulu", 10);
        zulu.FirstName = "Thandi";
        zulu.LastName = "Zulu";
        NomineeSeed.AddUser(db, "khumalo", 10, UserDeactivation.IndefiniteLockoutEnd);
        NomineeSeed.AddUser(db, "locked-out", 10, DateTimeOffset.UtcNow.AddMinutes(15));
        NomineeSeed.AddUser(db, "opted-out", 10).OptOutOfDigestEmails = true;
        NomineeSeed.AddUser(db, "no-address", 10).Email = null;
        await db.SaveChangesAsync();

        var loaded = await new ReminderRecipients(db).LoadAsync(
            ["zulu", "khumalo", "locked-out", "opted-out", "no-address", "erased", "zulu"], CancellationToken.None);

        loaded.Keys.Should().BeEquivalentTo(["zulu", "khumalo", "locked-out", "opted-out", "no-address"]);
        loaded["zulu"].Should().Be(new Application.Common.Interfaces.ReminderRecipientDto(
            "zulu", "zulu@test.local", "Thandi", "Zulu", IsDeactivated: false));

        var refusals = new[] { "zulu", "khumalo", "locked-out", "opted-out", "no-address", "erased" }
            .Select(id => (id, ReminderRecipientRules.RefusalFor(loaded.GetValueOrDefault(id))))
            .ToList();
        refusals.Should().Equal(
            ("zulu", (ReminderOutcome?)null),
            ("khumalo", ReminderOutcome.Deactivated),
            ("locked-out", null),
            ("opted-out", null),
            ("no-address", ReminderOutcome.NoEmail),
            ("erased", ReminderOutcome.NoAccount));
    }

    [Fact]
    public async Task NoIds_ReadsNothing()
    {
        await using var db = CreateDb();

        (await new ReminderRecipients(db).LoadAsync([], CancellationToken.None)).Should().BeEmpty();
    }

    /// <summary>An erased account is deactivated and has no address: deactivated is asked first, so it is counted once.</summary>
    [Fact]
    public void ADeactivatedAccountWithNoAddress_IsRefusedAsDeactivated()
        => ReminderRecipientRules.RefusalFor(new Application.Common.Interfaces.ReminderRecipientDto("erased", null, "", "", true))
            .Should().Be(ReminderOutcome.Deactivated);

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
