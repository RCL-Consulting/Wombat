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
/// T130: the paediatric curriculum's targets are Annexure B's per-window quotas, not a lifetime multiple.
/// </summary>
/// <remarks>
/// <para>
/// Three things carry Annexure B into a database, and each can drift from the other two on its own: the
/// catalogue JSON the seeder reads, the frozen copy inside the T130 migration (which corrects databases that
/// already hold the curriculum, because the seeder skips existing items), and the seeder's own mapping from
/// a catalogue row to a <see cref="CurriculumItem" />. These tests hold all three against
/// <see cref="AnnexureB" />, a table typed into this file from the College's document. It is never read from
/// the seed file, because an expected value read from the file under test is a tautology.
/// </para>
/// <para>
/// Before these tests the seeder had never run under test at all. Its only mention was a doc comment.
/// </para>
/// </remarks>
public sealed class PaediatricCatalogueQuotaSeedTests
{
    private const string CurriculumName = "Paediatric EPA Curriculum";

    /// <summary>
    /// Annexure B of CPSA EPA v11.1, "what version 11.1 asks for, per registrar per year", transcribed from
    /// <c>execution/tasks/done/T098-data/annexure-b.json</c>. <c>PerSemester</c> is null where the Annexure
    /// prints an em-dash: the five EPAs at one per annum, "scheduled throughout the year as opportunities
    /// arise". The last two columns are the curriculum item the seeder must produce, written out literally
    /// rather than derived, so this table cannot share a mistake with <c>PaediatricCatalogueSeeder.QuotaFor</c>.
    /// </summary>
    private static readonly AnnexureBRow[] AnnexureB =
    [
        new("PAED-001", PerYear: 6, PerSemester: 3, QuotaPeriod.Semester, Target: 3),
        new("PAED-002", PerYear: 6, PerSemester: 3, QuotaPeriod.Semester, Target: 3),
        new("PAED-003", PerYear: 6, PerSemester: 3, QuotaPeriod.Semester, Target: 3),
        new("PAED-004", PerYear: 6, PerSemester: 3, QuotaPeriod.Semester, Target: 3),
        new("PAED-005", PerYear: 6, PerSemester: 3, QuotaPeriod.Semester, Target: 3),
        new("PAED-006", PerYear: 4, PerSemester: 2, QuotaPeriod.Semester, Target: 2),
        new("PAED-007", PerYear: 2, PerSemester: 1, QuotaPeriod.Semester, Target: 1),
        new("PAED-008", PerYear: 1, PerSemester: null, QuotaPeriod.AcademicYear, Target: 1),
        new("PAED-009", PerYear: 1, PerSemester: null, QuotaPeriod.AcademicYear, Target: 1),
        new("PAED-010", PerYear: 6, PerSemester: 3, QuotaPeriod.Semester, Target: 3),
        new("PAED-011", PerYear: 1, PerSemester: null, QuotaPeriod.AcademicYear, Target: 1),
        new("PAED-012", PerYear: 6, PerSemester: 3, QuotaPeriod.Semester, Target: 3),
        new("PAED-013", PerYear: 1, PerSemester: null, QuotaPeriod.AcademicYear, Target: 1),
        new("PAED-014", PerYear: 1, PerSemester: null, QuotaPeriod.AcademicYear, Target: 1),
        new("PAED-015", PerYear: 2, PerSemester: 1, QuotaPeriod.Semester, Target: 1),
    ];

    /// <summary>
    /// Keys every catalogue EPA carries that <c>PaediatricCatalogueSeeder.EpaSeed</c> deliberately does not read.
    /// An entry here is a decision, not an oversight, and <see cref="FindUnaccountedKeys" /> fails on an entry
    /// that has gone stale in either direction.
    /// </summary>
    private static readonly string[] DeliberatelyUnreadEpaKeys =
    [
        // The source's "expiry period if not practised", which Annexure B relabels as the entrustment-DECISION
        // cadence. It is not the quota window (D39): it reads "annually" for EPAs 3, 6 and 7, which Annexure B
        // observes three, two and one per semester. Phase 4 (T131) is where the decision cadence is used.
        "currency",

        // The WBA instruments Annexure A lists per EPA. Mapping an EPA to the tools that may credit it is T122.
        "wbaTools",

        // The EPA's ordinal in Annexure A (1..15). `code` (PAED-001..015) already carries it.
        "number",

        // The domain's abbreviation (AEC, CN, CDP, LPS, ECP). The seeder stores the full name from `domain`.
        "domainCode",

        // Annexure A's prose ("Six per annum"). `observationsPerYear` is the same figure as a number, so nothing
        // ever has to parse it.
        "frequencyText",
    ];

    [Fact]
    public async Task EverySeededItem_TargetsWhatAnnexureBPublishes_ForAllFifteenEpas()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        await using var dbContext = database.NewContext();
        var items = await LoadCatalogueItemsAsync(dbContext);

        // The in-memory provider enforces no unique index, so a second item for one EPA would slip past a
        // lookup by code. Count them instead.
        items.GroupBy(item => item.Code).Should().OnlyContain(group => group.Count() == 1);

        items
            .OrderBy(item => item.Code, StringComparer.Ordinal)
            .Select(item => (item.Code, item.QuotaPeriod, item.RequiredCount))
            .Should()
            .Equal(AnnexureB.Select(row => (row.Code, row.Period, row.Target)));
    }

    /// <summary>
    /// D39, named. The task file first proposed seeding the window from <c>currency</c>, and these three are
    /// exactly the EPAs that reading gets wrong: their currency says "annually", yet Annexure B observes them
    /// every semester, and its own total of 25 per semester only adds up if they count per semester.
    /// </summary>
    [Theory]
    [InlineData("PAED-003", 3)]
    [InlineData("PAED-006", 2)]
    [InlineData("PAED-007", 1)]
    public async Task AnEpaWhoseCurrencySaysAnnually_IsStillTargetedPerSemester(string code, int perSemester)
    {
        // The premise, checked rather than assumed. If a catalogue edit rewords this EPA's currency, the test
        // would stop discriminating between the two sources and pass for the wrong reason.
        var currency = ReadRawCatalogueEpa(code).GetProperty("currency").GetString();
        currency.Should().Contain("annually").And.NotContain("semester");

        var database = new SeedDatabase();
        await database.BootAsync();

        await using var dbContext = database.NewContext();
        var item = (await LoadCatalogueItemsAsync(dbContext)).Should().ContainSingle(entry => entry.Code == code).Subject;

        item.QuotaPeriod.Should().Be(QuotaPeriod.Semester, "{0}'s currency is its decision cadence, not its observation window (D39)", code);
        item.RequiredCount.Should().Be(perSemester);
    }

    /// <summary>
    /// Annexure B's own totals: 25 observations a semester, 55 a year (25 + 25 + the five at one per annum).
    /// Both only hold if every item carries the right window AND the right target.
    /// </summary>
    [Fact]
    public async Task SemesterTargetsSumTo25_AndTheAnnualisedTargetsSumTo55()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        await using var dbContext = database.NewContext();
        var items = await LoadCatalogueItemsAsync(dbContext);

        items.Count(item => item.QuotaPeriod == QuotaPeriod.Semester).Should().Be(10);
        items.Count(item => item.QuotaPeriod == QuotaPeriod.AcademicYear).Should().Be(5);

        items.Where(item => item.QuotaPeriod == QuotaPeriod.Semester)
            .Sum(item => item.RequiredCount)
            .Should().Be(25);

        items.Sum(item => item.QuotaPeriod == QuotaPeriod.Semester ? 2 * item.RequiredCount : item.RequiredCount)
            .Should().Be(55);
    }

    /// <summary>
    /// The defect T130 exists for: the old seeder stored per-annum x 4 programme years, so the progress page
    /// read "1 / 24" against a number the College never published.
    /// </summary>
    [Fact]
    public async Task NoSeededTarget_IsTheOldLifetimeMultiple()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        await using var dbContext = database.NewContext();
        var items = await LoadCatalogueItemsAsync(dbContext);
        var perYear = AnnexureB.ToDictionary(row => row.Code, row => row.PerYear, StringComparer.Ordinal);

        items.Should().HaveCount(15);
        foreach (var item in items)
        {
            item.RequiredCount.Should().NotBe(
                perYear[item.Code] * 4,
                "{0} must target its per-window quota, not per-annum x 4 programme years", item.Code);
        }
    }

    /// <summary>
    /// The cross-check that makes the seed file's numbers trustworthy: the file agrees with the Annexure, not
    /// merely with itself. A misspelled <c>observationsPerSemester</c> key would deserialize to null here and
    /// silently turn every EPA into a once-a-year target.
    /// </summary>
    [Fact]
    public async Task TheCatalogueFile_CarriesAnnexureBsFiguresForEveryEpa()
    {
        var catalogue = await PaediatricCatalogueSeeder.ReadCatalogueForTestsAsync();

        catalogue.Epas
            .Select(epa => (epa.Code, epa.ObservationsPerYear, epa.ObservationsPerSemester))
            .OrderBy(row => row.Code, StringComparer.Ordinal)
            .Should()
            .Equal(AnnexureB.Select(row => (row.Code, row.PerYear, row.PerSemester)));
    }

    /// <summary>
    /// The migration cannot read the seed file (a shipped migration must never change), so it carries a
    /// frozen copy. Its doc comment promises this test holds the copy to the file.
    /// </summary>
    [Fact]
    public async Task TheMigrationsFrozenAnnexureB_EqualsTheCatalogueFile_AndTheAnnexure()
    {
        var catalogue = await PaediatricCatalogueSeeder.ReadCatalogueForTestsAsync();
        var frozen = T130_AcademicPeriodQuota.AnnexureB
            .OrderBy(row => row.Code, StringComparer.Ordinal)
            .ToArray();

        frozen.Should().HaveCount(15);
        frozen.Should().Equal(
            catalogue.Epas
                .Select(epa => (epa.Code, epa.ObservationsPerYear, epa.ObservationsPerSemester))
                .OrderBy(row => row.Code, StringComparer.Ordinal));
        frozen.Should().Equal(AnnexureB.Select(row => (row.Code, row.PerYear, row.PerSemester)));
    }

    /// <summary>
    /// The one-off correction for databases that already hold the curriculum. It must name every EPA with the
    /// right window and target, and it must touch only rows still at the value the old seeder wrote, so an
    /// administrator's deliberate edit survives the deploy (and the seeder's warning then names it).
    /// </summary>
    [Fact]
    public void TheMigrationsCorrection_CoversEveryEpa_AndOnlyRowsStillAtTheOldLifetimeValue()
    {
        var sql = T130_AcademicPeriodQuota.BuildAnnexureBCorrection();

        foreach (var row in AnnexureB)
        {
            // (code, per_year, quota_period, required_count). The window is cast from the enum so the SQL's
            // integers are checked against the values EF actually stores for QuotaPeriod.
            sql.Should().Contain($"('{row.Code}', {row.PerYear}, {(int)row.Period}, {row.Target})");
        }

        Regex.Matches(sql, @"\('PAED-\d{3}'").Should().HaveCount(15, "each EPA is corrected exactly once");
        sql.Should().Contain("\"RequiredCount\" = v.per_year * 4");
        sql.Should().Contain("ci.\"OwningInstitutionId\" IS NULL", "an institution's local item is never the College's to correct");
        sql.Should().Contain($"c.\"Name\" = '{CurriculumName}'");
    }

    /// <summary>
    /// The no-loss guard for the catalogue (T130). Before T130 two per-EPA keys, <c>currency</c> and
    /// <c>wbaTools</c>, sat in the JSON and nothing read them, and that is how the quota window came to have no
    /// source. Every key on every EPA must now be either read by <c>EpaSeed</c> or allow-listed with a reason.
    /// </summary>
    [Fact]
    public void EveryKeyOnEveryCatalogueEpa_IsReadByTheSeederOrDeliberatelyUnread()
    {
        var declared = DeclaredEpaSeedKeys();
        declared.Should().Contain(
            new[] { "code", "observationsPerYear", "observationsPerSemester" },
            "the guard below is vacuous unless the reflection actually sees EpaSeed's JSON names");

        using var document = JsonDocument.Parse(File.ReadAllText(CataloguePath));
        var epas = document.RootElement.GetProperty("epas").EnumerateArray().ToArray();
        epas.Should().HaveCount(15);

        var problems = FindUnaccountedKeys(epas, declared, DeliberatelyUnreadEpaKeys);

        problems.Should().BeEmpty(
            "every catalogue key must be read by EpaSeed or allow-listed with a reason. Problems: {0}",
            string.Join("; ", problems));
    }

    /// <summary>The guard is only worth having if it fails when a key goes unread.</summary>
    [Fact]
    public void UnreadKeyGuard_DetectsAKeyNothingReads()
    {
        using var document = JsonDocument.Parse(SyntheticCatalogueEpa(extraKey: "observationsPerQuarter"));

        var problems = FindUnaccountedKeys([document.RootElement], DeclaredEpaSeedKeys(), DeliberatelyUnreadEpaKeys);

        problems.Should().ContainSingle().Which.Should().Contain("PAED-900.observationsPerQuarter");
    }

    /// <summary>
    /// A stale entry in one direction: a key the seeder has started to read, left on the allow-list. The entry
    /// would then document a decision that no longer holds.
    /// </summary>
    [Fact]
    public void UnreadKeyGuard_DetectsAnAllowListEntryTheSeederNowReads()
    {
        using var document = JsonDocument.Parse(SyntheticCatalogueEpa());

        var problems = FindUnaccountedKeys(
            [document.RootElement],
            DeclaredEpaSeedKeys(),
            [.. DeliberatelyUnreadEpaKeys, "observationsPerSemester"]);

        problems.Should().ContainSingle().Which.Should().Contain("'observationsPerSemester'").And.Contain("EpaSeed now reads it");
    }

    /// <summary>
    /// A stale entry in the other direction: a key the catalogue no longer carries. Left in place, it would
    /// quietly excuse the key if it ever came back with a different meaning.
    /// </summary>
    [Fact]
    public void UnreadKeyGuard_DetectsAnAllowListEntryTheCatalogueNoLongerCarries()
    {
        using var document = JsonDocument.Parse(SyntheticCatalogueEpa());

        var problems = FindUnaccountedKeys(
            [document.RootElement],
            DeclaredEpaSeedKeys(),
            [.. DeliberatelyUnreadEpaKeys, "annexureCReference"]);

        problems.Should().ContainSingle().Which.Should().Contain("'annexureCReference'").And.Contain("no EPA in the catalogue carries it");
    }

    /// <summary>
    /// The seeder never overwrites an existing item, because an administrator may have set a target on purpose.
    /// What T130 adds is that the difference is announced at every startup instead of being silent. 24 is the
    /// value the pre-T130 seeder wrote for PAED-001, and the one the migration's correction would have missed
    /// on an item an administrator had since touched.
    /// </summary>
    [Fact]
    public async Task Reseeding_WarnsOnceAboutATargetThatDiffersFromTheCatalogue_AndLeavesTheItemAlone()
    {
        var database = new SeedDatabase();
        var firstBoot = new CapturingLogger<PaediatricCatalogueSeeder>();
        await database.BootAsync(firstBoot);

        firstBoot.Warnings.Should().BeEmpty("a freshly seeded curriculum agrees with the catalogue it was seeded from");

        await database.SetTargetAsync("PAED-001", QuotaPeriod.Semester, requiredCount: 24);

        var secondBoot = new CapturingLogger<PaediatricCatalogueSeeder>();
        await database.BootAsync(secondBoot);

        var warning = secondBoot.Warnings.Should().ContainSingle().Subject;
        warning.Message.Should().Contain("PAED-001");
        warning.Values["EpaCode"].Should().Be("PAED-001");
        warning.Values["RequiredCount"].Should().Be(24);
        warning.Values["ExpectedCount"].Should().Be(3);

        await using var dbContext = database.NewContext();
        var items = await LoadCatalogueItemsAsync(dbContext);
        items.Should().HaveCount(15, "a reseed adds nothing to a curriculum that already holds every EPA");

        var changed = items.Single(item => item.Code == "PAED-001");
        changed.RequiredCount.Should().Be(24, "the seeder warns and never writes");
        changed.QuotaPeriod.Should().Be(QuotaPeriod.Semester);

        items.Where(item => item.Code != "PAED-001")
            .OrderBy(item => item.Code, StringComparer.Ordinal)
            .Select(item => (item.Code, item.QuotaPeriod, item.RequiredCount))
            .Should()
            .Equal(AnnexureB.Where(row => row.Code != "PAED-001").Select(row => (row.Code, row.Period, row.Target)));
    }

    /// <summary>
    /// The window is half of the quota. An item flipped to per-semester with the right count still targets
    /// twice what the College publishes, so it has to be announced too.
    /// </summary>
    [Fact]
    public async Task Reseeding_WarnsWhenOnlyTheWindowDiffersFromTheCatalogue()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        await database.SetTargetAsync("PAED-008", QuotaPeriod.Semester, requiredCount: 1);

        var secondBoot = new CapturingLogger<PaediatricCatalogueSeeder>();
        await database.BootAsync(secondBoot);

        var warning = secondBoot.Warnings.Should().ContainSingle().Subject;
        warning.Values["EpaCode"].Should().Be("PAED-008");
        warning.Values["QuotaPeriod"].Should().Be(QuotaPeriod.Semester);
        warning.Values["ExpectedPeriod"].Should().Be(QuotaPeriod.AcademicYear);

        await using var dbContext = database.NewContext();
        var item = (await LoadCatalogueItemsAsync(dbContext)).Single(entry => entry.Code == "PAED-008");
        item.QuotaPeriod.Should().Be(QuotaPeriod.Semester, "the seeder warns and never writes");
    }

    /// <summary>
    /// Every problem the catalogue has with <c>EpaSeed</c>: a key read by nothing and not allow-listed, or an
    /// allow-list entry that has gone stale because the seeder now reads it or the catalogue no longer carries it.
    /// </summary>
    /// <remarks>
    /// Declared names compare case-insensitively because that is how the seeder binds them
    /// (<c>PropertyNameCaseInsensitive = true</c>): a key the deserializer would read counts as read.
    /// </remarks>
    private static IReadOnlyList<string> FindUnaccountedKeys(
        IReadOnlyList<JsonElement> epas,
        IReadOnlyCollection<string> declaredKeys,
        IReadOnlyCollection<string> allowList)
    {
        var declared = new HashSet<string>(declaredKeys, StringComparer.OrdinalIgnoreCase);
        var allowed = new HashSet<string>(allowList, StringComparer.Ordinal);
        var carried = new HashSet<string>(StringComparer.Ordinal);
        var problems = new List<string>();

        for (var index = 0; index < epas.Count; index++)
        {
            var epa = epas[index];
            var label = epa.TryGetProperty("code", out var code) && code.ValueKind == JsonValueKind.String
                ? code.GetString()
                : $"epas[{index}]";

            foreach (var property in epa.EnumerateObject())
            {
                carried.Add(property.Name);

                if (!declared.Contains(property.Name) && !allowed.Contains(property.Name))
                {
                    problems.Add($"{label}.{property.Name}: carried by the catalogue, read by nothing, and not allow-listed");
                }
            }
        }

        foreach (var name in allowed)
        {
            if (declared.Contains(name))
            {
                problems.Add($"'{name}' is allow-listed, but EpaSeed now reads it: take it off the allow-list");
            }

            if (!carried.Contains(name))
            {
                problems.Add($"'{name}' is allow-listed, but no EPA in the catalogue carries it: take it off the allow-list");
            }
        }

        return problems;
    }

    /// <summary>The JSON names <c>EpaSeed</c> binds, read off its <see cref="JsonPropertyNameAttribute" />s.</summary>
    private static IReadOnlyCollection<string> DeclaredEpaSeedKeys()
        => typeof(PaediatricCatalogueSeeder.EpaSeed)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name)
            .ToArray();

    /// <summary>
    /// One EPA object carrying every key the seeder reads and every allow-listed key, so the only problem the
    /// guard can report is the one a test adds.
    /// </summary>
    private static string SyntheticCatalogueEpa(string? extraKey = null)
    {
        var epa = new Dictionary<string, object?>(StringComparer.Ordinal);

        foreach (var key in DeclaredEpaSeedKeys().Concat(DeliberatelyUnreadEpaKeys))
        {
            epa[key] = 0;
        }

        epa["code"] = "PAED-900";

        if (extraKey is not null)
        {
            epa[extraKey] = 2;
        }

        return JsonSerializer.Serialize(epa);
    }

    private static string CataloguePath
        => Path.Combine(AppContext.BaseDirectory, "Persistence", "Seeds", "paediatric-epa-v11.1.json");

    private static JsonElement ReadRawCatalogueEpa(string code)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(CataloguePath));

        return document.RootElement
            .GetProperty("epas")
            .EnumerateArray()
            .Single(epa => epa.GetProperty("code").GetString() == code)
            .Clone();
    }

    private static async Task<IReadOnlyList<SeededItem>> LoadCatalogueItemsAsync(ApplicationDbContext dbContext)
        => await (
                from item in dbContext.CurriculumItems
                join curriculum in dbContext.Curricula on item.CurriculumId equals curriculum.Id
                join epa in dbContext.Epas on item.EpaId equals epa.Id
                where curriculum.Name == CurriculumName && item.OwningInstitutionId == null
                select new SeededItem(item.Id, epa.Code, item.QuotaPeriod, item.RequiredCount))
            .ToListAsync();

    private sealed record AnnexureBRow(string Code, int PerYear, int? PerSemester, QuotaPeriod Period, int Target);

    private sealed record SeededItem(int Id, string Code, QuotaPeriod QuotaPeriod, int RequiredCount);

    /// <summary>
    /// One in-memory database that outlives any single context, so a test can boot, edit and boot again the
    /// way a deployed server does: each startup gets a fresh scope and a fresh <see cref="ApplicationDbContext" />.
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

        /// <summary>An administrator's edit to one seeded item, made between two startups.</summary>
        public async Task SetTargetAsync(string code, QuotaPeriod quotaPeriod, int requiredCount)
        {
            await using var dbContext = NewContext();
            var seeded = (await LoadCatalogueItemsAsync(dbContext)).Single(entry => entry.Code == code);
            var item = await dbContext.CurriculumItems.SingleAsync(entity => entity.Id == seeded.Id);

            item.QuotaPeriod = quotaPeriod;
            item.RequiredCount = requiredCount;
            await dbContext.SaveChangesAsync();
        }
    }
}
