using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Wombat.Domain.Curricula;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Persistence.Migrations;

namespace Wombat.Infrastructure.Tests.Persistence;

/// <summary>
/// T131 slice 2: each EPA of the paediatric curriculum carries Annexure B's entrustment-decision cadence, the body that
/// decides it (the neonatal CCC for EPAs 4 and 5), and whether it is decided as opportunity allows (8, 9 and 13).
/// </summary>
/// <remarks>
/// <para>
/// The decision cells reach a database by three routes, and each can drift on its own: the catalogue JSON the seeder
/// reads, the seeder's mapping from a catalogue row to a <see cref="CurriculumItem" />, and the frozen copy inside the T131
/// migration that stamps databases which already hold the curriculum. These tests hold all three against
/// <see cref="AnnexureB" />, typed into this file from the College's document
/// (<c>execution/tasks/done/T098-data/annexure-b.json</c>, its <c>entrustment_decision</c> column and its note on the
/// neonatal CCC; Annexure A's status column for the three opportunistic EPAs). Nothing expected is read from the file
/// under test.
/// </para>
/// </remarks>
public sealed class PaediatricCatalogueDecisionSeedTests
{
    private const string CurriculumName = "Paediatric EPA Curriculum";
    private const string CatalogueVersion = "11.1";
    private const string NeonatalName = "Neonatal team Clinical Competency Committee";

    /// <summary>
    /// Annexure B's "Each semester" / "Annually", its note that "for EPAs 4 and 5 the decision is taken by the neonatal team
    /// Clinical Competency Committee", and Annexure A's "as rotation allows" (8), "opportunity with adolescents" (9) and
    /// "as opportunity allows" (13). Written out literally, so it cannot share a mistake with the seeder's mapping.
    /// </summary>
    private static readonly DecisionRow[] AnnexureB =
    [
        new("PAED-001", QuotaPeriod.Semester, null, false),
        new("PAED-002", QuotaPeriod.Semester, null, false),
        new("PAED-003", QuotaPeriod.AcademicYear, null, false),
        new("PAED-004", QuotaPeriod.Semester, "neonatal", false),
        new("PAED-005", QuotaPeriod.Semester, "neonatal", false),
        new("PAED-006", QuotaPeriod.AcademicYear, null, false),
        new("PAED-007", QuotaPeriod.AcademicYear, null, false),
        new("PAED-008", QuotaPeriod.AcademicYear, null, true),
        new("PAED-009", QuotaPeriod.AcademicYear, null, true),
        new("PAED-010", QuotaPeriod.Semester, null, false),
        new("PAED-011", QuotaPeriod.AcademicYear, null, false),
        new("PAED-012", QuotaPeriod.Semester, null, false),
        new("PAED-013", QuotaPeriod.AcademicYear, null, true),
        new("PAED-014", QuotaPeriod.AcademicYear, null, false),
        new("PAED-015", QuotaPeriod.AcademicYear, null, false),
    ];

    /// <summary>The catalogue file's spelling of each cadence, as the Annexure's two words.</summary>
    private static string? FileSpelling(QuotaPeriod? cadence) => cadence switch
    {
        QuotaPeriod.Semester => "semester",
        QuotaPeriod.AcademicYear => "annual",
        _ => null
    };

    // ---------------------------------------------------------------------------------------------------------------
    // The catalogue file
    // ---------------------------------------------------------------------------------------------------------------

    [Fact]
    public async Task TheCatalogueFile_CarriesAnnexureBsDecisionCells_ForEveryEpa()
    {
        var catalogue = await PaediatricCatalogueSeeder.ReadCatalogueForTestsAsync();

        catalogue.Epas
            .OrderBy(epa => epa.Code, StringComparer.Ordinal)
            .Select(epa => (epa.Code, epa.DecisionCadence, epa.DecisionBody, epa.DecisionOpportunistic))
            .Should()
            .Equal(AnnexureB.Select(row => (row.Code, FileSpelling(row.Cadence), row.BodyKey, row.Opportunistic)));
    }

    /// <summary>
    /// "All 15 items have an explicit cadence": the key is written on every EPA, never left to deserialise to null, so an
    /// EPA that lost its line in an edit of the file would fail here rather than become silently never due.
    /// </summary>
    [Fact]
    public void EveryCatalogueEpa_WritesAllThreeDecisionKeysExplicitly()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(CataloguePath));
        var epas = document.RootElement.GetProperty("epas").EnumerateArray().ToArray();

        epas.Should().HaveCount(15);
        foreach (var epa in epas)
        {
            var code = epa.GetProperty("code").GetString();
            epa.TryGetProperty("decisionCadence", out var cadence).Should().BeTrue("{0} states its cadence", code);
            cadence.ValueKind.Should().Be(JsonValueKind.String, "{0} has a published cadence", code);
            epa.TryGetProperty("decisionBody", out _).Should().BeTrue("{0} states its body, null for the general panel", code);
            epa.TryGetProperty("decisionOpportunistic", out _).Should().BeTrue("{0} states whether it is opportunistic", code);
        }
    }

    [Fact]
    public async Task TheDecisionBodyVocabulary_IsTheNeonatalCccAlone()
    {
        var catalogue = await PaediatricCatalogueSeeder.ReadCatalogueForTestsAsync();

        catalogue.DecisionBodyVocabulary.Select(body => (body.Key, body.Name))
            .Should().Equal(("neonatal", NeonatalName));
        catalogue.DecisionBodyVocabulary.Should().OnlyContain(body => DecisionBody.NormalizeKey(body.Key) == body.Key,
            "a key the normaliser would change is one the file and the database spell differently");
    }

    /// <summary>
    /// The College's key must not be parsed out of <c>currency</c>, and the two must not be confused: EPA 15's currency
    /// says "observed each semester", yet Annexure B decides it annually. The premise is checked, not assumed.
    /// </summary>
    [Fact]
    public async Task Epa15_IsDecidedAnnually_ThoughItsCurrencySaysSemester()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(CataloguePath));
        var currency = document.RootElement.GetProperty("epas").EnumerateArray()
            .Single(epa => epa.GetProperty("code").GetString() == "PAED-015")
            .GetProperty("currency").GetString();
        currency.Should().Contain("semester");

        var database = new SeedDatabase();
        await database.BootAsync();

        (await database.LoadCatalogueItemsAsync()).Single(item => item.Code == "PAED-015")
            .Cadence.Should().Be(QuotaPeriod.AcademicYear);
    }

    /// <summary>The no-loss guard for each decision-body entry: the seeder reads the key and the name; the note is provenance.</summary>
    [Fact]
    public void EveryKeyOnEveryDecisionBodyEntry_IsReadByTheSeederOrDeliberatelyUnread()
    {
        var declared = typeof(PaediatricCatalogueSeeder.DecisionBodySeed)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        declared.Should().BeEquivalentTo(new[] { "key", "name" }, "guard: the reflection sees DecisionBodySeed's JSON names");

        string[] deliberatelyUnread =
        [
            // The Annexure's own sentence, quoted, so a reader can check the entry against the College's document.
            "note",
        ];

        using var document = JsonDocument.Parse(File.ReadAllText(CataloguePath));
        var carried = document.RootElement.GetProperty("decisionBodyVocabulary").EnumerateArray()
            .SelectMany(entry => entry.EnumerateObject().Select(property => property.Name))
            .ToHashSet(StringComparer.Ordinal);

        carried.Where(name => !declared.Contains(name) && !deliberatelyUnread.Contains(name))
            .Should().BeEmpty("every key must be read by DecisionBodySeed or allow-listed with a reason");
        deliberatelyUnread.Should().OnlyContain(name => carried.Contains(name) && !declared.Contains(name),
            "an allow-list entry must still be carried and still be unread");
    }

    // ---------------------------------------------------------------------------------------------------------------
    // The seeder's mapping, and its refusals
    // ---------------------------------------------------------------------------------------------------------------

    [Fact]
    public void DecisionFor_RefusesACadenceItDoesNotKnow()
    {
        var act = () => PaediatricCatalogueSeeder.DecisionFor(Seed(cadence: "quarterly"), Vocabulary);

        act.Should().Throw<InvalidOperationException>().WithMessage("*PAED-900*'quarterly'*");
    }

    [Fact]
    public void DecisionFor_RefusesABodyTheVocabularyDoesNotList()
    {
        var act = () => PaediatricCatalogueSeeder.DecisionFor(Seed(cadence: "semester", body: "paediatric_icu"), Vocabulary);

        act.Should().Throw<InvalidOperationException>().WithMessage("*PAED-900*'paediatric_icu'*");
    }

    [Fact]
    public void DecisionFor_RefusesAnOpportunisticEpaWithNoCadence()
    {
        var act = () => PaediatricCatalogueSeeder.DecisionFor(Seed(cadence: null, opportunistic: true), Vocabulary);

        act.Should().Throw<InvalidOperationException>().WithMessage("*PAED-900*opportunity*no decision cadence*");
    }

    /// <summary>The control for the three refusals: the same seed with valid cells maps, and a body key is normalised.</summary>
    [Theory]
    [InlineData("semester", " Neonatal ", false, QuotaPeriod.Semester, "neonatal")]
    [InlineData("annual", null, true, QuotaPeriod.AcademicYear, null)]
    [InlineData(null, null, false, null, null)]
    public void DecisionFor_MapsEachValidCell(string? cadence, string? body, bool opportunistic, QuotaPeriod? expectedCadence, string? expectedBody)
    {
        PaediatricCatalogueSeeder.DecisionFor(Seed(cadence, body, opportunistic), Vocabulary)
            .Should().Be((expectedCadence, expectedBody, opportunistic));
    }

    // ---------------------------------------------------------------------------------------------------------------
    // What a boot seeds
    // ---------------------------------------------------------------------------------------------------------------

    [Fact]
    public async Task EverySeededItem_CarriesAnnexureBsDecision_ForAllFifteenEpas()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        var items = await database.LoadCatalogueItemsAsync();

        items.GroupBy(item => item.Code).Should().OnlyContain(group => group.Count() == 1);
        items.OrderBy(item => item.Code, StringComparer.Ordinal)
            .Select(item => new DecisionRow(item.Code, item.Cadence, item.BodyKey, item.Opportunistic))
            .Should().Equal(AnnexureB);
    }

    /// <summary>Annexure B's own arithmetic: six decided each semester and nine annually, 6 × 2 + 9 = 21 a year.</summary>
    [Fact]
    public async Task SixSemesterAndNineAnnualDecisions_Make21AYear()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        var items = await database.LoadCatalogueItemsAsync();

        items.Should().HaveCount(15).And.OnlyContain(item => item.Cadence != null, "every v11.1 EPA has a published cadence");
        var semester = items.Count(item => item.Cadence == QuotaPeriod.Semester);
        var annual = items.Count(item => item.Cadence == QuotaPeriod.AcademicYear);

        semester.Should().Be(6);
        annual.Should().Be(9);
        (semester * 2 + annual).Should().Be(21, "Annexure B's entrustment decisions per year");
    }

    [Fact]
    public async Task ADecisionBody_IsOnEpas4And5Only_AndItIsTheNeonatalCcc()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        var items = await database.LoadCatalogueItemsAsync();

        items.Where(item => item.BodyKey != null).Select(item => (item.Code, item.BodyKey))
            .OrderBy(item => item.Code, StringComparer.Ordinal)
            .Should().Equal(("PAED-004", "neonatal"), ("PAED-005", "neonatal"));
        (await database.ReadBodiesAsync()).Should().Equal(("neonatal", NeonatalName));
    }

    [Fact]
    public async Task ASecondBoot_ChangesNoDecision_AndWarnsAboutNothing()
    {
        var database = new SeedDatabase();
        var firstBoot = new CapturingLogger<PaediatricCatalogueSeeder>();
        await database.BootAsync(firstBoot);
        var before = await database.LoadCatalogueItemsAsync();

        var secondBoot = new CapturingLogger<PaediatricCatalogueSeeder>();
        await database.BootAsync(secondBoot);

        firstBoot.Warnings.Should().BeEmpty("a freshly seeded curriculum agrees with the catalogue it was seeded from");
        secondBoot.Warnings.Should().BeEmpty();
        (await database.LoadCatalogueItemsAsync()).Should().Equal(before);
        (await database.ReadBodiesAsync()).Should().ContainSingle();
    }

    /// <summary>
    /// The seeder stamps on create and never after: an item whose decision differs is announced at every startup and left
    /// as it is, because null is also what an administrator saves on purpose (T174's contract).
    /// </summary>
    [Fact]
    public async Task Reseeding_WarnsAboutEachDecisionThatDiffers_AndWritesNone()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        await database.EditItemAsync("PAED-004", item => item.DecisionBodyKey = null);
        await database.EditItemAsync("PAED-003", item => item.DecisionCadence = QuotaPeriod.Semester);
        await database.EditItemAsync("PAED-013", item => item.DecisionIsOpportunistic = false);

        var boot = new CapturingLogger<PaediatricCatalogueSeeder>();
        await database.BootAsync(boot);

        boot.Warnings.Select(warning => warning.Values["EpaCode"]).Should().BeEquivalentTo(new[] { "PAED-003", "PAED-004", "PAED-013" });
        var paed004 = boot.Warnings.Single(warning => Equals(warning.Values["EpaCode"], "PAED-004"));
        paed004.Values["DecisionBody"].Should().Be("the general panel");
        paed004.Values["ExpectedBody"].Should().Be("neonatal");

        var items = await database.LoadCatalogueItemsAsync();
        items.Single(item => item.Code == "PAED-004").BodyKey.Should().BeNull("the seeder warns and never writes");
        items.Single(item => item.Code == "PAED-003").Cadence.Should().Be(QuotaPeriod.Semester);
        items.Single(item => item.Code == "PAED-013").Opportunistic.Should().BeFalse();
    }

    /// <summary>The vocabulary is the one pass that reconciles: nothing else writes a body, so a changed name comes back.</summary>
    [Fact]
    public async Task ABoot_RestoresADecisionBodysName_InPlace()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        await database.EditAsync(async dbContext =>
            (await dbContext.DecisionBodies.SingleAsync(body => body.Key == "neonatal")).Name = "Edited behind the seeder's back");

        await database.BootAsync();

        (await database.ReadBodiesAsync()).Should().Equal(("neonatal", NeonatalName));
    }

    /// <summary>
    /// T131's verification: a cloned curriculum version keeps all three cells, for all fifteen items, through the store.
    /// </summary>
    [Fact]
    public async Task ACloneOfTheSeededCurriculum_KeepsEveryItemsDecision()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        await database.EditAsync(async dbContext =>
        {
            var seeded = await dbContext.Curricula.Include(curriculum => curriculum.Items)
                .SingleAsync(curriculum => curriculum.Name == CurriculumName && curriculum.Version == CatalogueVersion);
            dbContext.Curricula.Add(seeded.CloneAsNewVersion("11.2", new DateOnly(2027, 1, 1), null));
        });

        (await database.LoadCatalogueItemsAsync("11.2"))
            .OrderBy(item => item.Code, StringComparer.Ordinal)
            .Select(item => new DecisionRow(item.Code, item.Cadence, item.BodyKey, item.Opportunistic))
            .Should().Equal(AnnexureB);
    }

    // ---------------------------------------------------------------------------------------------------------------
    // The migration's frozen copy
    // ---------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// The migration cannot read the seed file, so it carries a frozen copy. Held here to the Annexure and to the file. A
    /// later change to the catalogue is a new migration, and this test is then re-pointed at a snapshot of what shipped.
    /// </summary>
    [Fact]
    public async Task TheMigrationsFrozenCopy_EqualsTheAnnexure_AndTheCatalogueFile()
    {
        var catalogue = await PaediatricCatalogueSeeder.ReadCatalogueForTestsAsync();

        var frozen = T131_DecisionCadenceAndBodies.AnnexureBDecisions
            .OrderBy(row => row.Code, StringComparer.Ordinal)
            .Select(row => new DecisionRow(row.Code, row.Cadence is int value ? (QuotaPeriod)value : null, row.BodyKey, row.Opportunistic))
            .ToArray();

        frozen.Should().HaveCount(15).And.Equal(AnnexureB);
        frozen.Should().Equal(catalogue.Epas
            .OrderBy(epa => epa.Code, StringComparer.Ordinal)
            .Select(epa =>
            {
                var (cadence, bodyKey, opportunistic) = PaediatricCatalogueSeeder.DecisionFor(epa, catalogue.DecisionBodyVocabulary);
                return new DecisionRow(epa.Code, cadence, bodyKey, opportunistic);
            }));

        T131_DecisionCadenceAndBodies.DecisionBodies
            .Should().Equal(catalogue.DecisionBodyVocabulary.Select(body => (body.Key, body.Name)));
    }

    /// <summary>The literals stand in for the enum a shipped migration may not read. They must be the integers EF stores.</summary>
    [Fact]
    public void TheMigrationsCadenceLiterals_AreTheIntegersEfStoresForQuotaPeriod()
    {
        T131_DecisionCadenceAndBodies.EachSemester.Should().Be((int)QuotaPeriod.Semester);
        T131_DecisionCadenceAndBodies.EachAcademicYear.Should().Be((int)QuotaPeriod.AcademicYear);
    }

    [Fact]
    public void TheMigrationsStamp_NamesEveryEpaOnce_AndTouchesOnlyTheCataloguesUntouchedNationalItems()
    {
        var sql = T131_DecisionCadenceAndBodies.BuildDecisionStamp();

        foreach (var row in AnnexureB)
        {
            var body = row.BodyKey is null ? "NULL" : $"'{row.BodyKey}'";
            sql.Should().Contain($"('{row.Code}', {(int)row.Cadence!.Value}, {body}, {(row.Opportunistic ? "TRUE" : "FALSE")})");
        }

        Regex.Matches(sql, @"\('PAED-\d{3}'").Should().HaveCount(15, "each EPA is stamped exactly once");
        sql.Should().Contain("ci.\"OwningInstitutionId\" IS NULL", "an institution's local item is never the College's to stamp");
        sql.Should().Contain("e.\"OwningInstitutionId\" IS NULL");
        sql.Should().Contain($"c.\"Name\" = '{CurriculumName}'");
        sql.Should().Contain($"c.\"Version\" = '{CatalogueVersion}'", "a clone at another version is its author's to decide");
        sql.Should().Contain("ci.\"DecisionCadence\" IS NULL")
            .And.Contain("ci.\"DecisionBodyKey\" IS NULL")
            .And.Contain("ci.\"DecisionIsOpportunistic\" = FALSE");
    }

    [Fact]
    public void TheMigrationsInsert_AddsEveryFrozenBody_AndIsANoOpWhenRunAgain()
    {
        var sql = T131_DecisionCadenceAndBodies.BuildDecisionBodiesInsert();

        sql.Should().Contain($"('neonatal', '{NeonatalName}')");
        sql.Should().Contain("ON CONFLICT (\"Key\") DO NOTHING");
    }

    // ---------------------------------------------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------------------------------------------

    private static readonly IReadOnlyList<PaediatricCatalogueSeeder.DecisionBodySeed> Vocabulary =
        [new("neonatal", NeonatalName)];

    private static PaediatricCatalogueSeeder.EpaSeed Seed(string? cadence, string? body = null, bool opportunistic = false)
        => new(
            "PAED-900", "Synthetic", "Domain", "Description", [], 1, null,
            new Dictionary<string, int>(), 3, [], cadence, body, opportunistic);

    private static string CataloguePath
        => Path.Combine(AppContext.BaseDirectory, "Persistence", "Seeds", "paediatric-epa-v11.1.json");

    private sealed record DecisionRow(string Code, QuotaPeriod? Cadence, string? BodyKey, bool Opportunistic);

    private sealed record SeededItem(int Id, string Code, QuotaPeriod? Cadence, string? BodyKey, bool Opportunistic);

    /// <summary>
    /// One in-memory database that outlives any single context, so a test can boot, edit and boot again the way a deployed
    /// server does. Copied from <c>PaediatricCatalogueQuotaSeedTests</c>.
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

        public Task EditItemAsync(string code, Action<CurriculumItem> edit)
            => EditAsync(async dbContext =>
            {
                var item = await dbContext.CurriculumItems
                    .SingleAsync(entity => entity.Curriculum.Name == CurriculumName
                                           && entity.Curriculum.Version == CatalogueVersion
                                           && entity.OwningInstitutionId == null
                                           && entity.Epa.Code == code);
                edit(item);
            });

        public async Task<IReadOnlyList<SeededItem>> LoadCatalogueItemsAsync(string version = CatalogueVersion)
        {
            await using var dbContext = NewContext();
            return await (
                    from item in dbContext.CurriculumItems
                    join curriculum in dbContext.Curricula on item.CurriculumId equals curriculum.Id
                    join epa in dbContext.Epas on item.EpaId equals epa.Id
                    where curriculum.Name == CurriculumName && curriculum.Version == version && item.OwningInstitutionId == null
                    orderby item.Id
                    select new SeededItem(item.Id, epa.Code, item.DecisionCadence, item.DecisionBodyKey, item.DecisionIsOpportunistic))
                .ToListAsync();
        }

        public async Task<IReadOnlyList<(string Key, string Name)>> ReadBodiesAsync()
        {
            await using var dbContext = NewContext();
            return (await dbContext.DecisionBodies.AsNoTracking().OrderBy(body => body.Key).ToListAsync())
                .Select(body => (body.Key, body.Name))
                .ToList();
        }
    }
}
