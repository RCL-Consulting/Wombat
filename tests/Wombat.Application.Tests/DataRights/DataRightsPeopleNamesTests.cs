using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.DataRights.Queries;
using Wombat.Domain.DataRights;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.DataRights;

/// <summary>
/// T400 (flow 18, M3, Q15; build-contracts § 0): <c>DataRightsPeople.NamesOfAsync</c>, the one reader run history's Ran by
/// and the audit's Person and Who name people by: the live "First Last", an erased person's pseudonym (by the id they had
/// or by the pseudonym itself), and the id itself for an account with no name.
/// </summary>
public sealed class DataRightsPeopleNamesTests
{
    [Fact]
    public async Task ALivingPerson_IsTheirLiveName()
    {
        await using var db = CreateDb();
        var users = new FakeUserDirectory(("admin-1", "Demo Administrator"));

        var names = await DataRightsPeople.NamesOfAsync(db, users, ["admin-1"], CancellationToken.None);

        names.Should().Equal(new Dictionary<string, string> { ["admin-1"] = "Demo Administrator" });
    }

    [Fact]
    public async Task AnErasedPerson_ByTheIdTheyHad_IsTheirPseudonym_AndTheDirectoryIsNotAskedForThem()
    {
        await using var db = CreateDb();
        AddErasure(db, "sipho", "deleted_user_0ae1d64b");
        await db.SaveChangesAsync();
        // The directory still knows the old id by its stored name; an erased person is never asked about.
        var users = new FakeUserDirectory(("sipho", "Sipho Ndlovu"));

        var names = await DataRightsPeople.NamesOfAsync(db, users, ["sipho"], CancellationToken.None);

        names["sipho"].Should().Be("deleted_user_0ae1d64b");
        users.Lookups.Should().BeEmpty();
    }

    [Fact]
    public async Task AnErasedPerson_ByTheirPseudonym_IsThePseudonym()
    {
        await using var db = CreateDb();
        AddErasure(db, "sipho", "deleted_user_0ae1d64b");
        await db.SaveChangesAsync();

        var names = await DataRightsPeople.NamesOfAsync(db, FakeUserDirectory.Empty, ["deleted_user_0ae1d64b"], CancellationToken.None);

        names["deleted_user_0ae1d64b"].Should().Be("deleted_user_0ae1d64b");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AnAccountWithNoName_IsItsId(string blank)
    {
        await using var db = CreateDb();
        var users = new FakeUserDirectory(("nameless", blank));

        var names = await DataRightsPeople.NamesOfAsync(db, users, ["nameless", "unknown"], CancellationToken.None);

        names["nameless"].Should().Be("nameless");
        names["unknown"].Should().Be("unknown", "an account the directory does not know is named by its id");
    }

    [Fact]
    public async Task AnIdAskedTwice_IsNamedOnce_InOneDirectoryRead()
    {
        await using var db = CreateDb();
        AddErasure(db, "sipho", "deleted_user_0ae1d64b");
        await db.SaveChangesAsync();
        var users = new FakeUserDirectory(("admin-1", "Demo Administrator"), ("coord-1", "Thandi Mokoena"));

        var names = await DataRightsPeople.NamesOfAsync(
            db, users, ["admin-1", "coord-1", "admin-1", "sipho"], CancellationToken.None);

        names.Should().HaveCount(3);
        names["admin-1"].Should().Be("Demo Administrator");
        names["coord-1"].Should().Be("Thandi Mokoena");
        names["sipho"].Should().Be("deleted_user_0ae1d64b");
        users.Lookups.Should().ContainSingle().Which.Should().BeEquivalentTo(["admin-1", "coord-1"]);
    }

    [Fact]
    public async Task NoIds_AreNoNames()
    {
        await using var db = CreateDb();
        var users = new FakeUserDirectory();

        var names = await DataRightsPeople.NamesOfAsync(db, users, [], CancellationToken.None);

        names.Should().BeEmpty();
        users.Lookups.Should().BeEmpty();
    }

    private static void AddErasure(ApplicationDbContext db, string userId, string pseudonym)
        => db.Set<DataRightsErasureRecord>().Add(
            DataRightsErasureRecord.Create(Guid.NewGuid(), userId, pseudonym, new DateTime(2026, 10, 27, 22, 30, 0, DateTimeKind.Utc), "[]"));

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
}
