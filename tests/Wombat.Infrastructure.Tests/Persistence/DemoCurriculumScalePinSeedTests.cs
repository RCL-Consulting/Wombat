using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Persistence;

/// <summary>
/// T174, for <see cref="DataSeeder" />: the demo curriculum's own item is pinned to the O-R Scale when the seeder
/// creates it, and on no later boot.
/// </summary>
/// <remarks>
/// <para>
/// Before T174 every boot pinned the item whenever its <c>ScaleId</c> was null, keeping its minima. An administrator
/// who chose "Not pinned" (T125) and entered 8, valid while unpinned, found it on the O-R Scale after a restart, where
/// 8 is no rung, so it could never be credited. <see cref="PaediatricCatalogueScalePinSeedTests" /> covers the same
/// contract for the v11.1 catalogue.
/// </para>
/// <para>
/// The O-R Scale is identified by its name and its five rungs typed here, never read from the seeder.
/// </para>
/// </remarks>
public sealed class DemoCurriculumScalePinSeedTests
{
    private const string CurriculumName = "IM Core Curriculum";
    private const string CurriculumVersion = "2026.1";
    private const string DemoEpaCode = "EPA-001";
    private const string OrScaleName = "O-R Scale";

    private static readonly string[] OrScaleRungs =
        ["Observe only", "Direct supervision", "Indirect supervision", "Independent", "Supervises others"];

    [Fact]
    public async Task AFreshSeed_PinsTheDemoItemToTheOrScale_AndNeitherBootWarnsAboutIt()
    {
        var database = new SeedDatabase();
        var firstBoot = new CapturingLogger<DataSeeder>();
        await database.BootAsync(firstBoot);

        var orScaleId = await database.ReadOrScaleIdAsync();
        var item = await database.LoadDemoItemAsync();
        item.ScaleId.Should().Be(orScaleId, "the seeder pins the item it creates to the ladder its minimum is written on");
        item.MinimumLevelOrder.Should().Be(4, "Independent on the O-R Scale");

        var secondBoot = new CapturingLogger<DataSeeder>();
        await database.BootAsync(secondBoot);

        firstBoot.Warnings.Should().BeEmpty();
        secondBoot.Warnings.Should().BeEmpty("the item is still on the ladder the seeder states");
        (await database.LoadDemoItemAsync()).Should().Be(item, "a boot writes nothing to an item that already exists");
    }

    /// <summary>The defect: an unpinned demo item survives a restart exactly as the administrator left it.</summary>
    [Fact]
    public async Task AnUnpinnedDemoItem_StaysUnpinnedAcrossASecondSeed_WithItsMinimaUnchanged_AndIsAnnouncedOnce()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        // T125's page resets the minima on "Not pinned", and the administrator enters 8, with 2 and 8 by stage. All are
        // valid unpinned (1 to 20); on the O-R Scale 8 is not a rung.
        const string stageMinima = """{"1":2,"2":8}""";
        await database.EditDemoItemAsync(item =>
        {
            item.ScaleId = null;
            item.MinimumLevelOrder = 8;
            item.MinimumLevelByStageJson = stageMinima;
        });
        var before = await database.LoadDemoItemAsync();

        var secondBoot = new CapturingLogger<DataSeeder>();
        await database.BootAsync(secondBoot);

        var after = await database.LoadDemoItemAsync();
        after.ScaleId.Should().BeNull("a boot never re-pins an item: that would change what its stored minima mean");
        after.MinimumLevelOrder.Should().Be(8);
        CurriculumItem.ParseStageOverrides(after.MinimumLevelByStageJson)
            .Should().BeEquivalentTo(new Dictionary<int, int> { [1] = 2, [2] = 8 });
        after.Should().Be(before);

        var orScaleId = await database.ReadOrScaleIdAsync();
        var warning = secondBoot.Warnings.Should().ContainSingle().Which;
        warning.Values["CurriculumItemId"].Should().Be(before.Id);
        warning.Values["EpaCode"].Should().Be(DemoEpaCode);
        warning.Values["CurriculumName"].Should().Be(CurriculumName);
        warning.Values["StoredScale"].Should().Be("not pinned to any scale");
        warning.Values["ExpectedScale"].Should().Be($"'{OrScaleName}' (scale {orScaleId})");
    }

    /// <summary>Warn, never write: a demo item on another ladder is named, with both ladders, and left there.</summary>
    [Fact]
    public async Task ADemoItemOnAnotherLadder_IsAnnouncedWithBothLadders_AndLeftOnIt()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        var otherLadderId = await database.AddScaleAsync("Another ladder");
        await database.EditDemoItemAsync(item => item.ScaleId = otherLadderId);

        var secondBoot = new CapturingLogger<DataSeeder>();
        await database.BootAsync(secondBoot);

        (await database.LoadDemoItemAsync()).ScaleId.Should().Be(otherLadderId, "the seeder warns and never writes");

        var orScaleId = await database.ReadOrScaleIdAsync();
        var warning = secondBoot.Warnings.Should().ContainSingle().Which;
        warning.Values["StoredScale"].Should().Be($"pinned to 'Another ladder' (scale {otherLadderId})");
        warning.Values["ExpectedScale"].Should().Be($"'{OrScaleName}' (scale {orScaleId})");
        warning.Message.Should().Contain(DemoEpaCode).And.Contain("Another ladder").And.Contain(OrScaleName);
    }

    /// <summary>
    /// The second create path: the demo curriculum exists but its item does not, so a later boot adds it. Create is
    /// create whenever it happens, so the item is pinned.
    /// </summary>
    [Fact]
    public async Task ADemoItemTheSeederRecreatesOnALaterBoot_IsPinned_AndNotAnnounced()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        await database.EditAsync(async dbContext => dbContext.CurriculumItems.Remove(await DemoItems(dbContext).SingleAsync()));

        var secondBoot = new CapturingLogger<DataSeeder>();
        await database.BootAsync(secondBoot);

        (await database.LoadDemoItemAsync()).ScaleId.Should().Be(await database.ReadOrScaleIdAsync());
        secondBoot.Warnings.Should().BeEmpty();
    }

    /// <summary>
    /// An administrator's own item in the demo curriculum was written on a ladder the seeder cannot know. It is left
    /// unpinned and is not announced: there is nothing for it to differ from.
    /// </summary>
    [Fact]
    public async Task AnAdministratorsOwnUnpinnedItemInTheDemoCurriculum_IsNeitherPinnedNorAnnounced()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        var ownItemId = 0;
        await database.EditAsync(async dbContext =>
        {
            var demoItem = await DemoItems(dbContext).SingleAsync();
            var demoEpa = await dbContext.Epas.SingleAsync(entity => entity.Id == demoItem.EpaId);
            var ownEpa = new Epa
            {
                SubSpecialityId = demoEpa.SubSpecialityId,
                Code = "EPA-002",
                Title = "An administrator's own EPA",
                CreatedOn = DateTime.UtcNow
            };
            dbContext.Epas.Add(ownEpa);
            await dbContext.SaveChangesAsync();

            var ownItem = new CurriculumItem
            {
                CurriculumId = demoItem.CurriculumId,
                EpaId = ownEpa.Id,
                RequiredCount = 2,
                MinimumLevelOrder = 12,
                WindowMonths = 12,
                ScaleId = null
            };
            dbContext.CurriculumItems.Add(ownItem);
            await dbContext.SaveChangesAsync();
            ownItemId = ownItem.Id;
        });

        var secondBoot = new CapturingLogger<DataSeeder>();
        await database.BootAsync(secondBoot);

        secondBoot.Warnings.Should().BeEmpty("the seeder speaks only for the item it authored");
        var ownItem = await database.LoadItemAsync(ownItemId);
        ownItem.ScaleId.Should().BeNull();
        ownItem.MinimumLevelOrder.Should().Be(12);
        (await database.LoadDemoItemAsync()).ScaleId.Should().Be(await database.ReadOrScaleIdAsync());
    }

    private static IQueryable<CurriculumItem> DemoItems(ApplicationDbContext dbContext)
        => from item in dbContext.CurriculumItems
           join curriculum in dbContext.Curricula on item.CurriculumId equals curriculum.Id
           join epa in dbContext.Epas on item.EpaId equals epa.Id
           where curriculum.Name == CurriculumName
                 && curriculum.Version == CurriculumVersion
                 && epa.Code == DemoEpaCode
           select item;

    private static SeededItem ToSeeded(CurriculumItem item)
        => new(item.Id, item.ScaleId, item.MinimumLevelOrder, item.MinimumLevelByStageJson);

    private sealed record SeededItem(int Id, int? ScaleId, int MinimumLevelOrder, string? MinimumLevelByStageJson);

    /// <summary>
    /// One in-memory database that outlives any single context, so a test can boot, edit and boot again the way a
    /// deployed server does.
    /// </summary>
    private sealed class SeedDatabase
    {
        private readonly InMemoryDatabaseRoot _root = new();
        private readonly string _name = Guid.NewGuid().ToString();

        private ApplicationDbContext NewContext()
            => new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(_name, _root)
                .Options);

        public async Task BootAsync(CapturingLogger<DataSeeder>? logger = null)
        {
            await using var dbContext = NewContext();
            await new DataSeeder(dbContext, logger).SeedAsync();
        }

        public async Task EditAsync(Func<ApplicationDbContext, Task> edit)
        {
            await using var dbContext = NewContext();
            await edit(dbContext);
            await dbContext.SaveChangesAsync();
        }

        public Task EditDemoItemAsync(Action<CurriculumItem> edit)
            => EditAsync(async dbContext => edit(await DemoItems(dbContext).SingleAsync()));

        public async Task<SeededItem> LoadDemoItemAsync()
        {
            await using var dbContext = NewContext();
            return ToSeeded(await DemoItems(dbContext).AsNoTracking().SingleAsync());
        }

        public async Task<SeededItem> LoadItemAsync(int id)
        {
            await using var dbContext = NewContext();
            return ToSeeded(await dbContext.CurriculumItems.AsNoTracking().SingleAsync(entity => entity.Id == id));
        }

        public async Task<int> AddScaleAsync(string name)
        {
            await using var dbContext = NewContext();
            var scale = new EntrustmentScale
            {
                Name = name,
                Levels = [new EntrustmentLevel { Order = 1, Label = "A" }, new EntrustmentLevel { Order = 2, Label = "B" }]
            };
            dbContext.EntrustmentScales.Add(scale);
            await dbContext.SaveChangesAsync();
            return scale.Id;
        }

        /// <summary>The O-R Scale, found by its name and checked against its five rungs.</summary>
        public async Task<int> ReadOrScaleIdAsync()
        {
            await using var dbContext = NewContext();
            var scale = await dbContext.EntrustmentScales
                .AsNoTracking()
                .Include(entity => entity.Levels)
                .SingleAsync(entity => entity.Name == OrScaleName);

            scale.Levels.OrderBy(level => level.Order).Select(level => level.Label).Should().Equal(OrScaleRungs);
            return scale.Id;
        }
    }
}
