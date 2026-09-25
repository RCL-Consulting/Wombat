using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Persistence;

/// <summary>
/// T174 and T187: the catalogue seeder puts a curriculum item, and the Paediatrics sub-speciality's default, on the v11.1
/// ladder when it creates each, and on no later boot.
/// </summary>
/// <remarks>
/// <para>
/// The pin (T109) says which ladder an item's stored minima are ordinals on. Before T174 every boot pinned any seeded
/// item whose <c>ScaleId</c> was null, keeping its minima. An administrator who chose "Not pinned" on the item editor
/// (T125) and entered 4 and 8 found the item back on v11.1 after a restart, where 4 means 3b and 8 is no rung at all.
/// </para>
/// <para>
/// The sub-speciality's default (T076) limits committee STAR level pickers to one ladder. Before T187 every boot set it
/// back to v11.1, so a change on the sub-speciality's edit page lasted only until the next restart.
/// </para>
/// <para>
/// The ladder is identified here by the name and rungs typed from the College's document, never read from the seed
/// file, because an expected value read from the file under test is a tautology.
/// </para>
/// </remarks>
public sealed class PaediatricCatalogueScalePinSeedTests
{
    private const string CurriculumName = "Paediatric EPA Curriculum";
    private const string CatalogueVersion = "11.1";
    private const string V111ScaleName = "CPSA Paediatric Entrustment Scale v11.1";

    /// <summary>EPA v11.1's six rungs, level 3 split into 3a and 3b, in rank order.</summary>
    private static readonly string[] V111Rungs = ["1", "2", "3a", "3b", "4", "5"];

    /// <summary>
    /// The create path, which is now the only path. A fresh database ends with every one of the fifteen items on the
    /// v11.1 ladder, and nothing about the pin is announced, on the first boot or the next.
    /// </summary>
    [Fact]
    public async Task AFreshSeed_PinsAllFifteenItemsToTheV111Ladder_AndWarnsAboutNoPin()
    {
        var database = new SeedDatabase();
        var firstBoot = new CapturingLogger<PaediatricCatalogueSeeder>();
        await database.BootAsync(firstBoot);

        var ladderId = await database.ReadV111LadderIdAsync();
        var items = await database.LoadCatalogueItemsAsync();

        items.Should().HaveCount(15);
        items.Select(item => item.Code).Should().OnlyHaveUniqueItems();
        items.Should().OnlyContain(item => item.ScaleId == ladderId, "the seeder pins each item to the ladder its minima are written on");

        var secondBoot = new CapturingLogger<PaediatricCatalogueSeeder>();
        await database.BootAsync(secondBoot);

        firstBoot.Warnings.Should().BeEmpty("a freshly seeded curriculum agrees with the catalogue it was seeded from");
        secondBoot.Warnings.Should().BeEmpty("every item is still on the ladder the catalogue states");
        (await database.LoadCatalogueItemsAsync()).Should().Equal(items, "a boot writes nothing to an item that already exists");
    }

    /// <summary>
    /// The defect. An unpinned seeded item, with minima that are valid only unpinned, survives a restart exactly as the
    /// administrator left it. The other fourteen are untouched too.
    /// </summary>
    [Fact]
    public async Task AnUnpinnedSeededItem_StaysUnpinnedAcrossASecondSeed_WithItsMinimaUnchanged()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        // T125's page resets the minima on "Not pinned", and the administrator enters 4 and 8. Both are valid unpinned
        // (1 to 20); on v11.1, 4 is 3b and 8 is not a rung.
        const string stageMinima = """{"1":4,"2":4,"3":8,"4":8}""";
        await database.EditItemAsync("PAED-001", item =>
        {
            item.ScaleId = null;
            item.MinimumLevelOrder = 8;
            item.MinimumLevelByStageJson = stageMinima;
        });
        var before = await database.LoadCatalogueItemsAsync();

        await database.BootAsync();

        var after = await database.LoadCatalogueItemsAsync();
        var unpinned = after.Single(item => item.Code == "PAED-001");
        unpinned.ScaleId.Should().BeNull("a boot never re-pins an item: that would change what its stored minima mean");
        unpinned.MinimumLevelOrder.Should().Be(8);
        CurriculumItem.ParseStageOverrides(unpinned.MinimumLevelByStageJson)
            .Should().BeEquivalentTo(new Dictionary<int, int> { [1] = 4, [2] = 4, [3] = 8, [4] = 8 });

        after.Should().Equal(before, "the seeder writes nothing to any existing item, pinned or not");
    }

    /// <summary>
    /// Warn, never write: a pin that differs from the catalogue is announced at every startup until someone decides
    /// which is right. An unpinned item and an item on another ladder are both differences, each named once.
    /// </summary>
    [Fact]
    public async Task ASecondSeed_WarnsOnceAboutAnUnpinnedItem_AndOnceAboutAnItemOnAnotherLadder_AndWritesNeither()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        var ladderId = await database.ReadV111LadderIdAsync();
        var otherLadderId = await database.ReadScaleIdAsync(DataSeeder.OrScaleName);
        otherLadderId.Should().NotBe(ladderId, "the premise: DataSeeder's O-R Scale is a different ladder");

        await database.EditItemAsync("PAED-001", item => item.ScaleId = null);
        await database.EditItemAsync("PAED-002", item => item.ScaleId = otherLadderId);

        var secondBoot = new CapturingLogger<PaediatricCatalogueSeeder>();
        await database.BootAsync(secondBoot);

        var pinWarnings = secondBoot.Warnings.Where(entry => entry.Values.ContainsKey("StoredScale")).ToArray();
        pinWarnings.Should().HaveCount(2);
        secondBoot.Warnings.Should().HaveCount(2, "nothing else about the curriculum was changed");

        var expectedLadder = $"'{V111ScaleName}' (scale {ladderId})";

        var unpinned = pinWarnings.Should().ContainSingle(entry => Equals(entry.Values["EpaCode"], "PAED-001")).Which;
        unpinned.Values["StoredScale"].Should().Be("not pinned to any scale");
        unpinned.Values["ExpectedScale"].Should().Be(expectedLadder);
        unpinned.Message.Should().Contain("PAED-001").And.Contain(V111ScaleName);

        var elsewhere = pinWarnings.Should().ContainSingle(entry => Equals(entry.Values["EpaCode"], "PAED-002")).Which;
        elsewhere.Values["StoredScale"].Should().Be($"pinned to '{DataSeeder.OrScaleName}' (scale {otherLadderId})");
        elsewhere.Values["ExpectedScale"].Should().Be(expectedLadder);

        var items = await database.LoadCatalogueItemsAsync();
        items.Single(item => item.Code == "PAED-001").ScaleId.Should().BeNull("the seeder warns and never writes");
        items.Single(item => item.Code == "PAED-002").ScaleId.Should().Be(otherLadderId, "the seeder warns and never writes");
        items.Where(item => item.Code is not "PAED-001" and not "PAED-002")
            .Should().HaveCount(13).And.OnlyContain(item => item.ScaleId == ladderId);
    }

    /// <summary>
    /// A curriculum item the catalogue creates on a later boot, for a database that already holds the curriculum but
    /// not that EPA's item, is still pinned: create is create whenever it happens.
    /// </summary>
    [Fact]
    public async Task AnItemTheSeederRecreatesOnALaterBoot_IsPinned()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        await database.EditAsync(async dbContext =>
        {
            var id = (await QueryCatalogueItemsAsync(dbContext)).Single(item => item.Code == "PAED-009").Id;
            dbContext.CurriculumItems.Remove(await dbContext.CurriculumItems.SingleAsync(entity => entity.Id == id));
        });
        (await database.LoadCatalogueItemsAsync()).Should().HaveCount(14);

        var secondBoot = new CapturingLogger<PaediatricCatalogueSeeder>();
        await database.BootAsync(secondBoot);

        var ladderId = await database.ReadV111LadderIdAsync();
        var items = await database.LoadCatalogueItemsAsync();
        items.Should().HaveCount(15);
        items.Single(item => item.Code == "PAED-009").ScaleId.Should().Be(ladderId);
        secondBoot.Warnings.Should().BeEmpty();
    }

    /// <summary>
    /// T187, the create path: the sub-speciality is created already defaulting to the v11.1 ladder, and no boot
    /// announces anything about it.
    /// </summary>
    [Fact]
    public async Task AFreshSeed_DefaultsTheSubSpecialityToTheV111Ladder_AndWarnsAboutNothing()
    {
        var database = new SeedDatabase();
        var firstBoot = new CapturingLogger<PaediatricCatalogueSeeder>();
        await database.BootAsync(firstBoot);

        var ladderId = await database.ReadV111LadderIdAsync();
        (await database.ReadSubSpecialityDefaultScaleIdAsync()).Should().Be(ladderId);

        var secondBoot = new CapturingLogger<PaediatricCatalogueSeeder>();
        await database.BootAsync(secondBoot);

        (await database.ReadSubSpecialityDefaultScaleIdAsync()).Should().Be(ladderId);
        firstBoot.Warnings.Should().BeEmpty("a freshly created sub-speciality defaults to the ladder the catalogue states");
        secondBoot.Warnings.Should().BeEmpty("the default is still the ladder the catalogue states");
    }

    /// <summary>
    /// T187, the defect. An administrator moves the default to another ladder; a restart keeps their choice, says once
    /// that it differs from v11.1, and touches no curriculum item.
    /// </summary>
    [Fact]
    public async Task AChangedDefaultScale_SurvivesASecondSeed_AndIsAnnouncedOnce()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        var ladderId = await database.ReadV111LadderIdAsync();
        var otherLadderId = await database.ReadScaleIdAsync(DataSeeder.OrScaleName);
        otherLadderId.Should().NotBe(ladderId, "the premise: DataSeeder's O-R Scale is a different ladder");

        await database.EditSubSpecialityAsync(subSpeciality => subSpeciality.DefaultEntrustmentScaleId = otherLadderId);
        var itemsBefore = await database.LoadCatalogueItemsAsync();

        var secondBoot = new CapturingLogger<PaediatricCatalogueSeeder>();
        await database.BootAsync(secondBoot);

        (await database.ReadSubSpecialityDefaultScaleIdAsync()).Should().Be(otherLadderId, "a boot never reverts an administrator's default");

        var warning = secondBoot.Warnings.Should().ContainSingle("nothing else was changed").Which;
        warning.Values["SubSpecialityName"].Should().Be("Paediatrics");
        warning.Values["StoredDefault"].Should().Be($"defaults to '{DataSeeder.OrScaleName}' (scale {otherLadderId})");
        warning.Values["ExpectedScale"].Should().Be($"'{V111ScaleName}' (scale {ladderId})");
        warning.Message.Should().Contain(DataSeeder.OrScaleName).And.Contain(V111ScaleName);

        (await database.LoadCatalogueItemsAsync()).Should().Equal(itemsBefore, "the default is not a pin, and moving it re-pins no item");
    }

    /// <summary>
    /// T187: "No default — offer every scale" is a choice too, and survives a restart the same way.
    /// </summary>
    [Fact]
    public async Task AClearedDefaultScale_StaysClearedAcrossASecondSeed_AndIsAnnouncedOnce()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        await database.EditSubSpecialityAsync(subSpeciality => subSpeciality.DefaultEntrustmentScaleId = null);

        var secondBoot = new CapturingLogger<PaediatricCatalogueSeeder>();
        await database.BootAsync(secondBoot);

        (await database.ReadSubSpecialityDefaultScaleIdAsync()).Should().BeNull("a boot cannot tell a cleared default from a gap, so it fills neither");

        var warning = secondBoot.Warnings.Should().ContainSingle().Which;
        warning.Values["StoredDefault"].Should().Be("has no default scale");
        warning.Values["ExpectedScale"].Should().Be($"'{V111ScaleName}' (scale {await database.ReadV111LadderIdAsync()})");
    }

    /// <summary>The sub-speciality the catalogue curriculum belongs to: the Paediatrics programme.</summary>
    private static Task<int> QueryCatalogueSubSpecialityIdAsync(ApplicationDbContext dbContext)
        => dbContext.Curricula
            .AsNoTracking()
            .Where(curriculum => curriculum.Name == CurriculumName && curriculum.Version == CatalogueVersion)
            .Select(curriculum => curriculum.SubSpecialityId)
            .SingleAsync();

    private static async Task<IReadOnlyList<SeededItem>> QueryCatalogueItemsAsync(ApplicationDbContext dbContext)
        => await (
                from item in dbContext.CurriculumItems.AsNoTracking()
                join curriculum in dbContext.Curricula on item.CurriculumId equals curriculum.Id
                join epa in dbContext.Epas on item.EpaId equals epa.Id
                where curriculum.Name == CurriculumName
                      && curriculum.Version == CatalogueVersion
                      && item.OwningInstitutionId == null
                      && epa.OwningInstitutionId == null
                orderby epa.Code
                select new SeededItem(item.Id, epa.Code, item.ScaleId, item.MinimumLevelOrder, item.MinimumLevelByStageJson))
            .ToListAsync();

    private sealed record SeededItem(int Id, string Code, int? ScaleId, int MinimumLevelOrder, string? MinimumLevelByStageJson);

    /// <summary>
    /// One in-memory database that outlives any single context, so a test can boot, edit and boot again the way a
    /// deployed server does: each startup gets a fresh scope and a fresh <see cref="ApplicationDbContext" />.
    /// </summary>
    private sealed class SeedDatabase
    {
        private readonly InMemoryDatabaseRoot _root = new();
        private readonly string _name = Guid.NewGuid().ToString();

        public ApplicationDbContext NewContext()
            => new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(_name, _root)
                .Options);

        /// <summary>The seeding half of one startup, in the order <c>Program.cs</c> runs it.</summary>
        public async Task BootAsync(ILogger<PaediatricCatalogueSeeder>? logger = null)
        {
            await using var dbContext = NewContext();
            await new DataSeeder(dbContext).SeedAsync();
            await new PaediatricCatalogueSeeder(dbContext, logger).SeedAsync();
        }

        /// <summary>An edit made between two startups, by an administrator or by hand.</summary>
        public async Task EditAsync(Func<ApplicationDbContext, Task> edit)
        {
            await using var dbContext = NewContext();
            await edit(dbContext);
            await dbContext.SaveChangesAsync();
        }

        public Task EditItemAsync(string code, Action<CurriculumItem> edit)
            => EditAsync(async dbContext =>
            {
                var seeded = (await QueryCatalogueItemsAsync(dbContext)).Single(entry => entry.Code == code);
                edit(await dbContext.CurriculumItems.SingleAsync(entity => entity.Id == seeded.Id));
            });

        public Task EditSubSpecialityAsync(Action<SubSpeciality> edit)
            => EditAsync(async dbContext =>
            {
                var id = await QueryCatalogueSubSpecialityIdAsync(dbContext);
                edit(await dbContext.SubSpecialities.SingleAsync(entity => entity.Id == id));
            });

        public async Task<int?> ReadSubSpecialityDefaultScaleIdAsync()
        {
            await using var dbContext = NewContext();
            var id = await QueryCatalogueSubSpecialityIdAsync(dbContext);
            return await dbContext.SubSpecialities
                .AsNoTracking()
                .Where(entity => entity.Id == id)
                .Select(entity => entity.DefaultEntrustmentScaleId)
                .SingleAsync();
        }

        public async Task<IReadOnlyList<SeededItem>> LoadCatalogueItemsAsync()
        {
            await using var dbContext = NewContext();
            return await QueryCatalogueItemsAsync(dbContext);
        }

        /// <summary>The v11.1 ladder, found by its name and checked against the rungs the College prints.</summary>
        public async Task<int> ReadV111LadderIdAsync()
        {
            await using var dbContext = NewContext();
            var scale = await dbContext.EntrustmentScales
                .AsNoTracking()
                .Include(entity => entity.Levels)
                .SingleAsync(entity => entity.Name == V111ScaleName);

            scale.Levels.OrderBy(level => level.Order).Select(level => level.Label).Should().Equal(V111Rungs);
            return scale.Id;
        }

        public async Task<int> ReadScaleIdAsync(string name)
        {
            await using var dbContext = NewContext();
            return await dbContext.EntrustmentScales.AsNoTracking().Where(entity => entity.Name == name).Select(entity => entity.Id).SingleAsync();
        }
    }
}
