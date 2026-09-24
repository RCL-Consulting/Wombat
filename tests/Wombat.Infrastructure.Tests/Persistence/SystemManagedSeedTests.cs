using System.Security.Claims;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Queries.ListActivityTypes;
using Wombat.Domain.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Persistence.Migrations;

namespace Wombat.Infrastructure.Tests.Persistence;

/// <summary>
/// Which seeded activity types only the system writes (T162): declared on the catalogue entry, stamped by the seeders on
/// create, stamped on existing databases by the T162 migration, and announced, never repaired, when the two differ.
/// </summary>
/// <remarks>
/// The same shape as <c>WbaToolKey</c> (T122), and for the same reason: a flag the seeders re-imposed on every boot would
/// be a second author of a column the catalogue already owns. Unlike the instrument key, nothing in the product changes
/// this flag after create, so a difference can only be a catalogue change without its migration, or a hand edit.
/// </remarks>
public sealed class SystemManagedSeedTests
{
    [Fact]
    public void TheCatalogue_MarksExactlyTheTwoFeedbackRecordsSystemManaged()
    {
        // msf_cpsa's rows are written by a released campaign (T121), and learner_feedback_cpsa's by a released
        // learner-feedback campaign (T164); every other seed is filed by a person. A new system-written seed changes this
        // list on purpose. The seeders stamp a NEW type on create, so learner_feedback_cpsa needed no migration; flipping
        // a type existing databases already hold does, and it is a new one: T162's is frozen.
        ActivityTypeSeedCatalogue.Entries
            .Where(entry => entry.SystemManaged)
            .Select(entry => entry.Key)
            .Should().Equal("msf_cpsa", "learner_feedback_cpsa");
    }

    /// <summary>
    /// The migration's frozen list is held to the snapshot it shipped with, permanently, and must assert nothing the
    /// seed catalogue contradicts. A subset, not an equality (T122's rule): a later seed may be system-managed without
    /// this migration ever having stamped it, and that must not make anyone edit a shipped migration.
    /// </summary>
    [Fact]
    public void TheT162MigrationsFrozenKeys_AreItsSnapshot_AndEachIsSystemManagedInTheCatalogue()
    {
        T162_SystemManagedActivityTypes.SystemManagedTypeKeys.Should().Equal(T162SystemManagedTypeKeysSnapshot);

        var entries = ActivityTypeSeedCatalogue.Entries.ToDictionary(entry => entry.Key, StringComparer.Ordinal);

        using var scope = new AssertionScope();
        foreach (var typeKey in T162_SystemManagedActivityTypes.SystemManagedTypeKeys)
        {
            entries.Should().ContainKey(typeKey, "the migration flags only seeded types");
            if (entries.TryGetValue(typeKey, out var entry))
            {
                entry.SystemManaged.Should().BeTrue("the migration and the seed catalogue must agree that {0} is system-written", typeKey);
            }
        }
    }

    /// <summary>
    /// The migration's ownership guard keys off a literal. If it stopped matching the seed actor, the UPDATE would flag
    /// no row and every existing database would go on offering msf_cpsa, with nobody told.
    /// </summary>
    [Fact]
    public void TheT162MigrationsSeedOwner_IsTheSeedActor()
        => T162_SystemManagedActivityTypes.SeedOwnerUserId.Should().Be(ActivityTypeSeedCatalogue.SeedActorUserId);

    /// <summary>
    /// End to end over the real seeds: after a boot, the picker offers someone in the paediatric speciality every
    /// person-filed CPSA type, and not msf_cpsa. No subject, so only the claims filter and the flag decide.
    /// </summary>
    [Fact]
    public async Task AfterASeededBoot_ThePicker_OffersEveryPersonFiledCpsaType_AndNotMsfCpsa()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        await using var dbContext = database.NewContext();
        var msf = await dbContext.ActivityTypes.AsNoTracking().SingleAsync(type => type.Key == "msf_cpsa");
        msf.Scope.Should().Be(ActivityScope.Speciality, "guard: msf_cpsa is in the paediatric speciality's scope");
        msf.IsActive.Should().BeTrue("guard: only the flag keeps it off the picker");

        var paediatricTrainee = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(WombatClaimTypes.SpecialityId, msf.ScopeId!.Value.ToString())],
            "test"));

        var offered = await new ListActivityTypesQueryHandler(dbContext).Handle(
            new ListActivityTypesQuery(paediatricTrainee),
            CancellationToken.None);

        var personFiledCpsaKeys = ActivityTypeSeedCatalogue.For(ActivityTypeSeedSource.PaediatricCollege)
            .Where(entry => !entry.SystemManaged)
            .Select(entry => entry.Key)
            .ToList();
        personFiledCpsaKeys.Should().NotBeEmpty("guard");

        var offeredKeys = offered.Select(type => type.Key).ToList();
        offeredKeys.Should().Contain(personFiledCpsaKeys);
        offeredKeys.Should().NotContain("msf_cpsa");
    }

    [Fact]
    public async Task AFreshBoot_StampsEachSeededTypeWithItsEntrysFlag()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        await using var dbContext = database.NewContext();
        var stored = await dbContext.ActivityTypes
            .AsNoTracking()
            .ToDictionaryAsync(type => type.Key, type => type.SystemManaged, StringComparer.Ordinal);

        stored.Should().HaveCount(ActivityTypeSeedCatalogue.Entries.Count, "guard: both seeders ran");
        foreach (var entry in ActivityTypeSeedCatalogue.Entries)
        {
            stored[entry.Key].Should().Be(entry.SystemManaged, "{0} is stamped from its entry on create", entry.Key);
        }

        stored["msf_cpsa"].Should().BeTrue();
    }

    [Fact]
    public async Task ASecondBoot_AfterTheFlagWasCleared_WarnsOnce_AndWritesNothing()
    {
        var database = new SeedDatabase();
        var firstCatalogueLog = new CapturingLogger<PaediatricCatalogueSeeder>();
        var firstDataLog = new CapturingLogger<DataSeeder>();
        await database.BootAsync(firstCatalogueLog, firstDataLog);

        firstCatalogueLog.Warnings.Should().BeEmpty("a freshly seeded database agrees with the catalogue it was seeded from");
        firstDataLog.Warnings.Should().BeEmpty();

        await database.SetSystemManagedAsync("msf_cpsa", false);
        await database.SetSystemManagedAsync("mini_cex", true);

        var catalogueLog = new CapturingLogger<PaediatricCatalogueSeeder>();
        var dataLog = new CapturingLogger<DataSeeder>();
        await database.BootAsync(catalogueLog, dataLog);

        var catalogueWarning = catalogueLog.Warnings.Should().ContainSingle().Which;
        catalogueWarning.Values["Key"].Should().Be("msf_cpsa");
        catalogueWarning.Values["StoredSystemManaged"].Should().Be(false);
        catalogueWarning.Values["ExpectedSystemManaged"].Should().Be(true);

        // Each seeder reports its own types: mini_cex is DataSeeder's.
        var dataWarning = dataLog.Warnings.Should().ContainSingle().Which;
        dataWarning.Values["Key"].Should().Be("mini_cex");
        dataWarning.Values["StoredSystemManaged"].Should().Be(true);

        await using var dbContext = database.NewContext();
        (await dbContext.ActivityTypes.AsNoTracking().SingleAsync(type => type.Key == "msf_cpsa"))
            .SystemManaged.Should().BeFalse("the seeders stamp the flag on create only");
        (await dbContext.ActivityTypes.AsNoTracking().SingleAsync(type => type.Key == "mini_cex"))
            .SystemManaged.Should().BeTrue();
    }

    [Fact]
    public async Task ATypeAnOperatorOwns_IsNotReported()
    {
        // An operator who built a type under a seed's key owns it; the catalogue asserts nothing about it (T122's rule).
        var database = new SeedDatabase();
        await database.BootAsync();
        await database.EditAsync(async dbContext =>
        {
            var type = await dbContext.ActivityTypes.SingleAsync(entity => entity.Key == "msf_cpsa");
            type.OwnerUserId = "operator-1";
            type.SystemManaged = false;
        });

        var catalogueLog = new CapturingLogger<PaediatricCatalogueSeeder>();
        await database.BootAsync(catalogueLog);

        catalogueLog.Warnings.Should().BeEmpty();
    }

    /// <summary>What the T162 migration flagged when it shipped. Never edit: a later change is a new migration.</summary>
    private static readonly string[] T162SystemManagedTypeKeysSnapshot = ["msf_cpsa"];

    /// <summary>One InMemory store, booted the way <c>Program.cs</c> seeds, and edited between boots.</summary>
    private sealed class SeedDatabase
    {
        private readonly InMemoryDatabaseRoot _root = new();
        private readonly string _name = Guid.NewGuid().ToString();

        public ApplicationDbContext NewContext()
            => new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(_name, _root)
                .Options);

        public async Task BootAsync(
            Microsoft.Extensions.Logging.ILogger<PaediatricCatalogueSeeder>? catalogueLogger = null,
            Microsoft.Extensions.Logging.ILogger<DataSeeder>? dataLogger = null)
        {
            await using var dbContext = NewContext();
            await new DataSeeder(dbContext, dataLogger).SeedAsync();
            await new PaediatricCatalogueSeeder(dbContext, catalogueLogger).SeedAsync();
        }

        public async Task EditAsync(Func<ApplicationDbContext, Task> edit)
        {
            await using var dbContext = NewContext();
            await edit(dbContext);
            await dbContext.SaveChangesAsync();
        }

        public Task SetSystemManagedAsync(string typeKey, bool systemManaged)
            => EditAsync(async dbContext =>
            {
                var type = await dbContext.ActivityTypes.SingleAsync(entity => entity.Key == typeKey);
                type.SystemManaged = systemManaged;
            });
    }
}
