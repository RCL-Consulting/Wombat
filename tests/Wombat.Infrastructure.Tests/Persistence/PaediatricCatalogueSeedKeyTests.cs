using System.Globalization;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Persistence;

/// <summary>
/// T221: the catalogue seeder finds its College, speciality, sub-speciality, ladder, EPAs and curriculum by a seed key an
/// administrator cannot edit, so a rename between two boots creates nothing.
/// </summary>
/// <remarks>
/// The fast half. EF InMemory enforces no unique index, so the pre-T221 startup failure (a second College of the same
/// name) and the migration's stamp are exercised only in <c>CatalogueSeedKeyPostgresTests</c>. The keys are typed here,
/// not read from the seeder, so a key changed in the seeder alone fails.
/// </remarks>
public sealed class PaediatricCatalogueSeedKeyTests
{
    private static readonly string[] EpaCodes = Enumerable.Range(1, 15)
        .Select(number => $"PAED-{number.ToString("000", CultureInfo.InvariantCulture)}")
        .ToArray();

    [Fact]
    public async Task AFreshSeed_GivesEveryCatalogueRowItsSeedKey_AndNoOtherRowOne()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        await using var dbContext = database.NewContext();

        (await dbContext.Colleges.Where(entity => entity.SeedKey != null).Select(entity => new { entity.SeedKey, entity.ShortCode }).ToListAsync())
            .Should().ContainSingle().Which.Should().BeEquivalentTo(new { SeedKey = "cpsa", ShortCode = "CPSA" });
        (await dbContext.Specialities.Where(entity => entity.SeedKey != null).Select(entity => entity.SeedKey).ToListAsync())
            .Should().Equal("cpsa:paediatrics");
        (await dbContext.SubSpecialities.Where(entity => entity.SeedKey != null).Select(entity => entity.SeedKey).ToListAsync())
            .Should().Equal("cpsa:paediatrics:paediatrics");
        (await dbContext.EntrustmentScales.Where(entity => entity.SeedKey != null).Select(entity => entity.SeedKey).ToListAsync())
            .Should().Equal("cpsa:scale:v11.1");
        (await dbContext.Curricula.Where(entity => entity.SeedKey != null).Select(entity => entity.SeedKey).ToListAsync())
            .Should().Equal("cpsa:paediatrics:curriculum:v11.1");
        (await dbContext.Epas.Where(entity => entity.SeedKey != null).Select(entity => new { entity.Code, entity.SeedKey }).ToListAsync())
            .Should().BeEquivalentTo(EpaCodes.Select(code => new { Code = code, SeedKey = $"cpsa:paediatrics:epa:{code}" }));
    }

    /// <summary>
    /// Every catalogue row renamed at once, as an administrator could: the College's name and short code, the speciality,
    /// the sub-speciality, the ladder, an EPA's code and the curriculum's name and version. Before T221 the next boot
    /// created a second of each; now it finds every one by its key.
    /// </summary>
    [Fact]
    public async Task EveryCatalogueRowRenamed_TheNextBootCreatesNothing_AndAnnouncesNothing()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        await database.EditAsync(async dbContext =>
        {
            var college = await dbContext.Colleges.SingleAsync(entity => entity.ShortCode == "CPSA");
            college.Name = "College of Paediatricians (SA)";
            college.ShortCode = "CPAED";
            (await dbContext.Specialities.SingleAsync(entity => entity.Name == "Paediatrics")).Name = "Paediatrics and Child Health";
            (await dbContext.SubSpecialities.SingleAsync(entity => entity.Name == "Paediatrics")).Name = "General Paediatrics";
            (await dbContext.EntrustmentScales.SingleAsync(entity => entity.Name == "CPSA Paediatric Entrustment Scale v11.1")).Name = "Paediatric ladder";
            (await dbContext.Epas.SingleAsync(entity => entity.Code == "PAED-001" && entity.OwningInstitutionId == null)).Code = "PAED-01";
            var curriculum = await dbContext.Curricula.SingleAsync(entity => entity.Name == "Paediatric EPA Curriculum");
            curriculum.Name = "Paediatrics EPA Curriculum";
            curriculum.Version = "11.1-local";
        });
        var before = await database.CensusAsync();

        var secondBoot = new CapturingLogger<PaediatricCatalogueSeeder>();
        await database.BootAsync(secondBoot);

        (await database.CensusAsync()).Should().Be(before, "every renamed row is found by its seed key");
        secondBoot.Warnings.Should().BeEmpty();
    }

    private sealed record Census(int Colleges, int Specialities, int SubSpecialities, int Scales, int Epas, int Curricula, int Items);

    /// <summary>
    /// One in-memory database that outlives any single context, so a test can boot, edit and boot again the way a
    /// deployed server does. The same shape as <see cref="PaediatricCatalogueScalePinSeedTests" />' helper.
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
                await dbContext.Colleges.CountAsync(),
                await dbContext.Specialities.CountAsync(),
                await dbContext.SubSpecialities.CountAsync(),
                await dbContext.EntrustmentScales.CountAsync(),
                await dbContext.Epas.CountAsync(),
                await dbContext.Curricula.CountAsync(),
                await dbContext.CurriculumItems.CountAsync());
        }
    }
}
