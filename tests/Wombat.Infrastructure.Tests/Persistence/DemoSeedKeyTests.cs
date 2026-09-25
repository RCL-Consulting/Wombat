using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Persistence;

/// <summary>
/// T229: <see cref="DataSeeder" /> finds the Demo Institution, the Demo College, its speciality and sub-speciality, the O-R
/// Scale, the demo EPA and the IM Core curriculum by a seed key an administrator cannot edit, so an edit between two boots
/// creates nothing.
/// </summary>
/// <remarks>
/// The fast half. EF InMemory enforces no unique index, so the pre-T229 startup failures (a <c>SingleAsync</c> on a changed
/// <c>DEMO-C</c>, a second Demo Institution refused by the unique name) and the migration's stamp are exercised only in
/// <c>DemoSeedKeyPostgresTests</c>. The keys are typed here, not read from the seeder, so a key changed in the seeder alone
/// fails.
/// </remarks>
public sealed class DemoSeedKeyTests
{
    [Fact]
    public async Task AFreshSeed_GivesEveryDemoRowItsSeedKey_EachOnTheRowOfItsName()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        await using var dbContext = database.NewContext();

        (await dbContext.Institutions.Where(entity => entity.SeedKey != null).Select(entity => new { entity.SeedKey, entity.Name, entity.ShortCode }).ToListAsync())
            .Should().ContainSingle().Which.Should().BeEquivalentTo(new { SeedKey = "demo", Name = "Demo Institution", ShortCode = "DEMO" });
        (await dbContext.Colleges.Where(entity => entity.SeedKey == "demo").Select(entity => new { entity.Name, entity.ShortCode }).ToListAsync())
            .Should().ContainSingle().Which.Should().BeEquivalentTo(new { Name = "Demo College", ShortCode = "DEMO-C" });
        (await dbContext.Specialities.Where(entity => entity.SeedKey == "demo:general-medicine").Select(entity => entity.Name).ToListAsync())
            .Should().Equal("General Medicine");
        (await dbContext.SubSpecialities.Where(entity => entity.SeedKey == "demo:general-medicine:general-internal-medicine").Select(entity => entity.Name).ToListAsync())
            .Should().Equal("General Internal Medicine");
        (await dbContext.EntrustmentScales.Where(entity => entity.SeedKey == "demo:scale:o-r").Select(entity => entity.Name).ToListAsync())
            .Should().Equal(DataSeeder.OrScaleName);
        (await dbContext.Epas.Where(entity => entity.SeedKey == "demo:general-medicine:epa:EPA-001").Select(entity => new { entity.Code, entity.OwningInstitutionId }).ToListAsync())
            .Should().ContainSingle().Which.Should().BeEquivalentTo(new { Code = "EPA-001", OwningInstitutionId = (int?)null });
        (await dbContext.Curricula.Where(entity => entity.SeedKey == "demo:general-medicine:curriculum:v2026.1").Select(entity => new { entity.Name, entity.Version }).ToListAsync())
            .Should().ContainSingle().Which.Should().BeEquivalentTo(new { Name = "IM Core Curriculum", Version = "2026.1" });

        // Every key DataSeeder writes starts "demo", which is how the catalogue's tests tell its keys from the demo's.
        (await dbContext.Colleges.Where(entity => entity.SeedKey != null).Select(entity => entity.SeedKey!).ToListAsync())
            .Should().BeEquivalentTo("demo", "cpsa");
    }

    /// <summary>
    /// Every demo row edited at once, as an administrator could: both short codes and both names, the speciality, the
    /// sub-speciality, the O-R Scale, the EPA's code and the curriculum's name and version. Before T229 the next boot threw on
    /// the College, and without that it would have created a second institution, College, scale, EPA and curriculum.
    /// </summary>
    [Fact]
    public async Task EveryDemoRowEdited_TheNextBootCreatesNothing_AndAnnouncesNothing()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        await database.EditAsync(async dbContext =>
        {
            var institution = await dbContext.Institutions.SingleAsync(entity => entity.ShortCode == "DEMO");
            institution.Name = "Groote Schuur Hospital";
            institution.ShortCode = "GSH";
            var college = await dbContext.Colleges.SingleAsync(entity => entity.ShortCode == "DEMO-C");
            college.Name = "College of Physicians of South Africa";
            college.ShortCode = "CPSA-PHYS";
            (await dbContext.Specialities.SingleAsync(entity => entity.Name == "General Medicine")).Name = "Internal Medicine";
            (await dbContext.SubSpecialities.SingleAsync(entity => entity.Name == "General Internal Medicine")).Name = "General Internal Medicine (adult)";
            (await dbContext.EntrustmentScales.SingleAsync(entity => entity.Name == DataSeeder.OrScaleName)).Name = "Observation to entrustment";
            (await dbContext.Epas.SingleAsync(entity => entity.Code == "EPA-001")).Code = "IM-001";
            var curriculum = await dbContext.Curricula.SingleAsync(entity => entity.Name == "IM Core Curriculum");
            curriculum.Name = "Internal Medicine Core";
            curriculum.Version = "2027.1";
        });
        var before = await database.CensusAsync();

        var secondBoot = new CapturingLogger<DataSeeder>();
        await database.BootAsync(secondBoot);

        (await database.CensusAsync()).Should().Be(before, "every edited row is found by its seed key");
        secondBoot.Warnings.Should().BeEmpty("nothing about an edited row differs from what the seeder checks");
    }

    /// <summary>
    /// A demo row without its key, as one an administrator edited before the T229 migration stamped the keys would be, is
    /// announced once by its key and not created again, and everything that does not need it still runs.
    /// </summary>
    [Fact]
    public async Task AKeylessCurriculum_IsAnnouncedOnce_AndNotCreatedAgain()
    {
        var database = new SeedDatabase();
        await database.BootAsync();
        await database.EditAsync(async dbContext =>
        {
            var curriculum = await dbContext.Curricula.SingleAsync(entity => entity.Name == "IM Core Curriculum");
            dbContext.Entry(curriculum).Property(entity => entity.SeedKey).CurrentValue = null;
        });
        var before = await database.CensusAsync();

        var boot = new CapturingLogger<DataSeeder>();
        await database.BootAsync(boot);

        (await database.CensusAsync()).Should().Be(before, "a missing demo row is never created again once the demo data exists");
        var warning = boot.Warnings.Should().ContainSingle().Which;
        warning.Values["DemoRow"].Should().Be("curriculum");
        warning.Values["SeedKey"].Should().Be("demo:general-medicine:curriculum:v2026.1");
    }

    /// <summary>
    /// The demo data with none of its keys, as the T229 migration leaves it when every lookup its stamp uses was edited
    /// away first but the demo's names were not. The seeder then sees a fresh database whose names are taken, and says
    /// how to recover. Its advice must work as written: every demo row needs its key, because a key on only some of them
    /// makes the seeder treat the demo data as present and announce the rest as missing at every boot.
    /// </summary>
    [Fact]
    public async Task TheDemoDataWithoutItsKeys_IsRefusedWithAWarningNamingEveryKey_AndKeyingEveryRowAsItSaysFindsTheDemoData()
    {
        var database = new SeedDatabase();
        await database.BootAsync();
        await database.EditAsync(async dbContext =>
        {
            foreach (var (_, _, find) in DemoRowsByKey)
            {
                dbContext.Entry(await find(dbContext)).Property("SeedKey").CurrentValue = null;
            }
        });
        var before = await database.CensusAsync();

        var refused = new CapturingLogger<DataSeeder>();
        await database.BootAsync(refused);

        (await database.CensusAsync()).Should().Be(before, "nothing is created beside a row the seeder did not make");
        var warning = refused.Warnings.Should().ContainSingle().Which;
        warning.Values["SeedKeys"].Should().Be(
            string.Join("; ", DemoRowsByKey.Select(row => $"{row.Row} '{row.Key}'")),
            "the warning names every demo row's key, which is what the operator has to write");

        // One key alone, as the warning said before this was fixed: the demo data now counts as present, and every other
        // row is announced as missing and created by nothing.
        await database.EditAsync(async dbContext =>
            dbContext.Entry(await DemoRowsByKey[1].Find(dbContext)).Property("SeedKey").CurrentValue = DemoRowsByKey[1].Key);
        var partlyKeyed = new CapturingLogger<DataSeeder>();
        await database.BootAsync(partlyKeyed);

        (await database.CensusAsync()).Should().Be(before);
        partlyKeyed.Warnings.Select(entry => entry.Values["SeedKey"])
            .Should().BeEquivalentTo(DemoRowsByKey.Where((_, index) => index != 1).Select(row => row.Key));

        // Every key, as the warning says now: the demo data is found whole.
        await database.EditAsync(async dbContext =>
        {
            foreach (var (_, key, find) in DemoRowsByKey)
            {
                dbContext.Entry(await find(dbContext)).Property("SeedKey").CurrentValue = key;
            }
        });
        var keyed = new CapturingLogger<DataSeeder>();
        await database.BootAsync(keyed);

        (await database.CensusAsync()).Should().Be(before);
        keyed.Warnings.Should().BeEmpty("every demo row is found by the key the warning named");
    }

    /// <summary>
    /// Each demo row as the warning names it, its key typed here rather than read from the seeder, and how this test finds
    /// the row without it.
    /// </summary>
    private static readonly (string Row, string Key, Func<ApplicationDbContext, Task<object>> Find)[] DemoRowsByKey =
    [
        ("institution", "demo", async db => await db.Institutions.SingleAsync(entity => entity.Name == "Demo Institution")),
        ("College", "demo", async db => await db.Colleges.SingleAsync(entity => entity.Name == "Demo College")),
        ("speciality", "demo:general-medicine", async db => await db.Specialities.SingleAsync(entity => entity.Name == "General Medicine")),
        ("sub-speciality", "demo:general-medicine:general-internal-medicine", async db => await db.SubSpecialities.SingleAsync(entity => entity.Name == "General Internal Medicine")),
        ("entrustment scale", "demo:scale:o-r", async db => await db.EntrustmentScales.SingleAsync(entity => entity.Name == DataSeeder.OrScaleName)),
        ("EPA EPA-001", "demo:general-medicine:epa:EPA-001", async db => await db.Epas.SingleAsync(entity => entity.Code == "EPA-001")),
        ("curriculum", "demo:general-medicine:curriculum:v2026.1", async db => await db.Curricula.SingleAsync(entity => entity.Name == "IM Core Curriculum"))
    ];

    private sealed record Census(int Institutions, int Colleges, int Specialities, int SubSpecialities, int Scales, int Levels, int Epas, int Curricula, int Items, int ActivityTypes);

    /// <summary>
    /// One in-memory database that outlives any single context, so a test can boot, edit and boot again the way a deployed
    /// server does. The same shape as <see cref="PaediatricCatalogueSeedKeyTests" />' helper.
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
        public async Task BootAsync(CapturingLogger<DataSeeder>? logger = null)
        {
            await using var dbContext = NewContext();
            await new DataSeeder(dbContext, logger).SeedAsync();
            await new PaediatricCatalogueSeeder(dbContext).SeedAsync();
        }

        public async Task EditAsync(Func<ApplicationDbContext, Task> edit)
        {
            await using var dbContext = NewContext();
            await edit(dbContext);
            await dbContext.SaveChangesAsync();
        }

        public async Task<Census> CensusAsync()
        {
            await using var dbContext = NewContext();
            return new Census(
                await dbContext.Institutions.CountAsync(),
                await dbContext.Colleges.CountAsync(),
                await dbContext.Specialities.CountAsync(),
                await dbContext.SubSpecialities.CountAsync(),
                await dbContext.EntrustmentScales.CountAsync(),
                await dbContext.EntrustmentLevels.CountAsync(),
                await dbContext.Epas.CountAsync(),
                await dbContext.Curricula.CountAsync(),
                await dbContext.CurriculumItems.CountAsync(),
                await dbContext.ActivityTypes.CountAsync());
        }
    }
}
