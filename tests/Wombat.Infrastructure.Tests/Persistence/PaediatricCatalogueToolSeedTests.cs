using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Persistence.Migrations;

namespace Wombat.Infrastructure.Tests.Persistence;

/// <summary>
/// T122: each EPA of the paediatric curriculum may be credited only by the instruments Annexure A names for it.
/// </summary>
/// <remarks>
/// <para>
/// Annexure A's tools column reaches a database by four routes, and each can drift from the others on its own: the
/// verbatim cell the catalogue carries as <c>annexureTools</c>, the instrument keys it carries as <c>wbaTools</c> (the
/// cell with the College's aliases applied, which is what the seeder reads), the curriculum items the seeder builds
/// from those keys, and the frozen copy inside the T122 migration that stamps databases which already hold the
/// curriculum. These tests hold all four against tables typed into this file from the College's document. Nothing
/// expected is read from the file under test, because an expected value read from the file under test is a tautology.
/// </para>
/// <para>
/// Two neighbours share the vocabulary: every seeded activity type states which instrument it is
/// (<c>WbaToolKey</c>), and the seeder inserts the College's twelve instruments as <see cref="WbaTool" /> rows. A seeded
/// type with the wrong key, or none, is refused or unrestricted on every EPA with a list, so the keys are pinned here
/// too. The seeders never write a list or a key into a row that already exists; they warn instead, and those warnings
/// are pinned as the behaviour they are.
/// </para>
/// </remarks>
public sealed class PaediatricCatalogueToolSeedTests
{
    private const string CurriculumName = "Paediatric EPA Curriculum";
    private const string CatalogueVersion = "11.1";

    /// <summary>
    /// Annexure A of CPSA EPA v11.1, the "tools" cell of each EPA's row, in the order the Annexure prints it. The
    /// "(n)" count suffix the Annexure appends to some names is stripped; nothing else is changed. "Case note review"
    /// (PAED-006) and "Directly observed clinical examination" (PAED-007) are the Annexure's own words: the College's
    /// rulings that they are CCA and Mini-CEX (D4, D12) belong to the vocabulary, not to this transcription.
    /// </summary>
    private static readonly (string Code, string[] Tools)[] AnnexureATools =
    [
        ("PAED-001", ["DOPS", "Mini-CEX", "CCA", "CBD", "MSF", "RCA", "Clinical audit", "Reflective exercise"]),
        ("PAED-002", ["Mini-CEX", "CBD", "DOPS", "Direct observation", "CCA", "Chart-stimulated recall", "MSF", "RCA", "Clinical audit"]),
        ("PAED-003", ["Mini-CEX", "CCA", "CBD", "MSF", "RCA", "DOPS", "Clinical audit", "Reflective exercise"]),
        ("PAED-004", ["Mini-CEX", "CBD", "DOPS", "CCA", "Direct observation", "MSF"]),
        ("PAED-005", ["CBD", "DOPS", "MSF"]),
        ("PAED-006", ["Direct observation", "Mini-CEX", "DOPS", "Case note review", "MSF"]),
        ("PAED-007", ["Mini-CEX", "DOPS", "Directly observed clinical examination", "MSF"]),
        ("PAED-008", ["Mini-CEX", "DOPS", "CBD", "MSF", "Reflective exercise"]),
        ("PAED-009", ["Direct observation", "CBD", "MSF"]),
        ("PAED-010", ["MSF", "Direct observation"]),
        ("PAED-011", ["MSF", "CBD", "Direct observation"]),
        ("PAED-012", ["Mini-CEX", "MSF", "CBD", "Chart-stimulated recall"]),
        ("PAED-013", ["MSF", "Direct observation", "CBD", "Mini-CEX"]),
        ("PAED-014", ["MSF", "Reflective exercise", "CBD"]),
        ("PAED-015", ["Direct observation", "CBD", "Learner feedback", "MSF", "Portfolio and logbook review"]),
    ];

    /// <summary>
    /// Instruments the College added to an EPA beyond its Annexure A cell. One so far: on 2026-09-20 the College ruled
    /// that EPA 7 does not exclude general Direct observation (D12). Typed here rather than read from the catalogue's
    /// <c>wbaToolsNote</c>, which is prose for a reader.
    /// </summary>
    private static readonly (string Code, string ToolKey)[] CollegeAmendments =
    [
        ("PAED-007", "direct_observation"),
    ];

    /// <summary>
    /// What each seeded curriculum item must permit: the cell above with the aliases resolved and the amendment
    /// applied, as instrument keys in ordinal order. Written out literally rather than derived, so this table cannot
    /// share a mistake with the resolver the catalogue checks use.
    /// </summary>
    private static readonly (string Code, string[] Keys)[] ExpectedAllowLists =
    [
        ("PAED-001", ["cbd", "cca", "clinical_audit", "dops", "mini_cex", "msf", "rca", "reflective_exercise"]),
        ("PAED-002", ["cbd", "cca", "chart_stimulated_recall", "clinical_audit", "direct_observation", "dops", "mini_cex", "msf", "rca"]),
        ("PAED-003", ["cbd", "cca", "clinical_audit", "dops", "mini_cex", "msf", "rca", "reflective_exercise"]),
        ("PAED-004", ["cbd", "cca", "direct_observation", "dops", "mini_cex", "msf"]),
        ("PAED-005", ["cbd", "dops", "msf"]),
        ("PAED-006", ["cca", "direct_observation", "dops", "mini_cex", "msf"]),
        ("PAED-007", ["direct_observation", "dops", "mini_cex", "msf"]),
        ("PAED-008", ["cbd", "dops", "mini_cex", "msf", "reflective_exercise"]),
        ("PAED-009", ["cbd", "direct_observation", "msf"]),
        ("PAED-010", ["direct_observation", "msf"]),
        ("PAED-011", ["cbd", "direct_observation", "msf"]),
        ("PAED-012", ["cbd", "chart_stimulated_recall", "mini_cex", "msf"]),
        ("PAED-013", ["cbd", "direct_observation", "mini_cex", "msf"]),
        ("PAED-014", ["cbd", "msf", "reflective_exercise"]),
        ("PAED-015", ["cbd", "direct_observation", "learner_feedback", "msf", "portfolio_review"]),
    ];

    /// <summary>The College's twelve instruments, as keys, in ordinal order.</summary>
    private static readonly string[] VocabularyKeys =
    [
        "cbd",
        "cca",
        "chart_stimulated_recall",
        "clinical_audit",
        "direct_observation",
        "dops",
        "learner_feedback",
        "mini_cex",
        "msf",
        "portfolio_review",
        "rca",
        "reflective_exercise",
    ];

    /// <summary>
    /// The three instruments Annexure A names but page 8 of v11.1 does not define. Their rows carry a null
    /// description, and anything else would be a definition the College never published.
    /// </summary>
    private static readonly string[] UndefinedOnPage8 = ["chart_stimulated_recall", "learner_feedback", "portfolio_review"];

    /// <summary>
    /// Which instrument every seeded activity type is. The nine CPSA seeds, the generic Mini-CEX, DOPS and CBD (the same
    /// instruments: unkeyed, they would be unrestricted on every EPA with a list), and null for the seven generic types
    /// that are not College-named instruments (D21). Every seed entry is listed, so a new seed forces a decision here.
    /// </summary>
    private static readonly (string TypeKey, string? WbaToolKey)[] SeededTypeToolKeys =
    [
        ("mini_cex_cpsa", "mini_cex"),
        ("dops_cpsa", "dops"),
        ("cbd_cpsa", "cbd"),
        ("direct_observation_cpsa", "direct_observation"),
        ("msf_cpsa", "msf"),
        ("cca_cpsa", "cca"),
        ("rca_cpsa", "rca"),
        ("chart_stimulated_recall_cpsa", "chart_stimulated_recall"),
        ("reflective_exercise_cpsa", "reflective_exercise"),
        ("mini_cex", "mini_cex"),
        ("dops", "dops"),
        ("cbd", "cbd"),
        ("acat", null),
        ("reflective_note", null),
        ("procedure_log", null),
        ("research_output", null),
        ("teaching_session", null),
        ("qi_project", null),
        ("journal_club", null),
    ];

    /// <summary>
    /// The T122 migration's <c>PermittedTools</c> as it shipped, typed out. PERMANENT: a shipped migration never
    /// changes, so this snapshot never follows the catalogue. A later change to a tool list is a new migration, and
    /// this table stays as it is. It happens to equal <see cref="ExpectedAllowLists" /> today; that is deliberately
    /// not asserted, because the day the College changes a list the two must be allowed to part.
    /// </summary>
    private static readonly (string Code, string ToolsJson)[] T122PermittedToolsSnapshot =
    [
        ("PAED-001", """["cbd","cca","clinical_audit","dops","mini_cex","msf","rca","reflective_exercise"]"""),
        ("PAED-002", """["cbd","cca","chart_stimulated_recall","clinical_audit","direct_observation","dops","mini_cex","msf","rca"]"""),
        ("PAED-003", """["cbd","cca","clinical_audit","dops","mini_cex","msf","rca","reflective_exercise"]"""),
        ("PAED-004", """["cbd","cca","direct_observation","dops","mini_cex","msf"]"""),
        ("PAED-005", """["cbd","dops","msf"]"""),
        ("PAED-006", """["cca","direct_observation","dops","mini_cex","msf"]"""),
        ("PAED-007", """["direct_observation","dops","mini_cex","msf"]"""),
        ("PAED-008", """["cbd","dops","mini_cex","msf","reflective_exercise"]"""),
        ("PAED-009", """["cbd","direct_observation","msf"]"""),
        ("PAED-010", """["direct_observation","msf"]"""),
        ("PAED-011", """["cbd","direct_observation","msf"]"""),
        ("PAED-012", """["cbd","chart_stimulated_recall","mini_cex","msf"]"""),
        ("PAED-013", """["cbd","direct_observation","mini_cex","msf"]"""),
        ("PAED-014", """["cbd","msf","reflective_exercise"]"""),
        ("PAED-015", """["cbd","direct_observation","learner_feedback","msf","portfolio_review"]"""),
    ];

    /// <summary>The T122 migration's <c>SeededToolKeys</c> as it shipped. Permanent, for the same reason.</summary>
    private static readonly (string TypeKey, string WbaToolKey)[] T122SeededToolKeysSnapshot =
    [
        ("mini_cex_cpsa", "mini_cex"),
        ("dops_cpsa", "dops"),
        ("cbd_cpsa", "cbd"),
        ("direct_observation_cpsa", "direct_observation"),
        ("msf_cpsa", "msf"),
        ("mini_cex", "mini_cex"),
        ("dops", "dops"),
        ("cbd", "cbd"),
    ];

    /// <summary>
    /// Root keys of the catalogue that <c>PaediatricCatalogueSeeder.CatalogueSeed</c> deliberately does not read. An
    /// entry here is a decision, and <see cref="FindUnaccountedKeys" /> fails on one that has gone stale either way.
    /// </summary>
    private static readonly string[] DeliberatelyUnreadRootKeys =
    [
        // The citation of the College's document the catalogue was transcribed from. Provenance for a reader; the
        // product stores the version it seeds as `catalogueVersion`.
        "source",
    ];

    /// <summary>
    /// Keys every vocabulary entry may carry that <c>PaediatricCatalogueSeeder.WbaToolSeed</c> deliberately does not
    /// read (T122).
    /// </summary>
    private static readonly string[] DeliberatelyUnreadVocabularyEntryKeys =
    [
        // The verbatim Annexure A names the key stands for, aliases included ("Case note review" under cca, D4). The
        // audit trail this file resolves each EPA's `annexureTools` through; the product speaks keys only.
        "annexureNames",

        // Prose recording the decision behind an alias, or why an instrument has no page-8 definition.
        "note",
    ];

    // ---------------------------------------------------------------------------------------------------------------
    // The catalogue file
    // ---------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// The verbatim cell is what a reader of the Annexure checks the catalogue against. If it drifted from the
    /// Annexure, every derived list below would be checked against the wrong source and still pass.
    /// </summary>
    [Fact]
    public void TheCatalogueFile_CarriesAnnexureAsToolsCellVerbatim_ForEveryEpa()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(CataloguePath));
        var cells = ReadAnnexureCells(document.RootElement);

        cells.Select(cell => cell.Code).Should().Equal(AnnexureATools.Select(row => row.Code));

        using var scope = new AssertionScope();
        foreach (var (code, tools) in AnnexureATools)
        {
            cells.Single(cell => cell.Code == code).Tools
                .Should().Equal(tools, "{0}'s annexureTools is Annexure A's cell, verbatim and in the Annexure's order", code);
        }
    }

    /// <summary>
    /// The link between the Annexure and what the seeder reads. <c>wbaTools</c> must be exactly the verbatim cell
    /// resolved through the vocabulary's <c>annexureNames</c>, plus the College's amendments, in ordinal order. An
    /// instrument added to <c>wbaTools</c> with no Annexure name behind it, or one dropped, fails here by EPA.
    /// </summary>
    [Fact]
    public async Task EveryEpasWbaTools_IsItsAnnexureCellResolvedThroughTheVocabulary_PlusTheCollegesAmendments()
    {
        var catalogue = await PaediatricCatalogueSeeder.ReadCatalogueForTestsAsync();
        using var document = JsonDocument.Parse(File.ReadAllText(CataloguePath));
        var resolver = ResolverFor(document.RootElement);
        var cells = ReadAnnexureCells(document.RootElement);

        cells.Should().HaveCount(15);

        using var scope = new AssertionScope();
        foreach (var (code, tools) in cells)
        {
            var resolved = resolver.Resolve(code, tools);
            var amendments = CollegeAmendments.Where(row => row.Code == code).Select(row => row.ToolKey).ToArray();

            // The premise, checked rather than assumed: an amendment the cell already yields would make this test
            // unable to tell an amended list from an unamended one.
            if (amendments.Length > 0)
            {
                resolved.Should().NotContain(amendments, "{0}'s amendment must add something its Annexure A cell does not already give", code);
            }

            var expected = resolved
                .Concat(amendments)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();

            catalogue.Epas.Single(epa => epa.Code == code).WbaTools
                .Should().Equal(expected, "{0}'s wbaTools must be its Annexure A cell resolved through the vocabulary, plus the College's amendments, in ordinal order", code);
        }

        resolver.Problems.Should().BeEmpty(
            "every Annexure A name must resolve to exactly one vocabulary key. Problems: {0}",
            string.Join("; ", resolver.Problems));
    }

    /// <summary>The resolver is only worth having if a name the vocabulary does not list fails it.</summary>
    [Fact]
    public void TheAnnexureNameResolver_FailsOnANameWithNoVocabularyEntry()
    {
        var resolver = new AnnexureNameResolver([("cbd", ["CBD"]), ("msf", ["MSF"])]);

        var resolved = resolver.Resolve("PAED-900", ["CBD", "Case review", "MSF"]);

        resolved.Should().Equal("cbd", "msf");
        resolver.Problems.Should().ContainSingle().Which.Should().Contain("PAED-900").And.Contain("'Case review'");
    }

    /// <summary>
    /// A name listed under two keys is ambiguous: which instrument an EPA permits would then depend on the order the
    /// vocabulary happens to be written in.
    /// </summary>
    [Fact]
    public void TheAnnexureNameResolver_FailsOnANameListedUnderTwoKeys()
    {
        var resolver = new AnnexureNameResolver(
        [
            ("cca", ["CCA", "Case note review"]),
            ("cbd", ["CBD", "Case note review"]),
        ]);

        resolver.Problems.Should().ContainSingle()
            .Which.Should().Contain("'Case note review'").And.Contain("'cca'").And.Contain("'cbd'");
    }

    /// <summary>
    /// A per-EPA key the vocabulary does not define is a list naming an instrument that has no row, no name to put in
    /// a refusal message, and no activity type that could ever carry it.
    /// </summary>
    [Fact]
    public async Task EveryPerEpaToolKey_IsAVocabularyKey()
    {
        var catalogue = await PaediatricCatalogueSeeder.ReadCatalogueForTestsAsync();
        var vocabulary = catalogue.WbaToolVocabulary.Select(tool => tool.Key).ToHashSet(StringComparer.Ordinal);

        using var scope = new AssertionScope();
        foreach (var epa in catalogue.Epas)
        {
            epa.WbaTools.Should().OnlyContain(key => vocabulary.Contains(key), "every instrument {0} permits must be in the vocabulary", epa.Code);
        }
    }

    /// <summary>
    /// The seeder normalises a vocabulary key before it stores it, while the per-EPA lists and the activity-type keys
    /// are compared against the stored form. A key the normaliser would change is one the file and the database
    /// spell differently.
    /// </summary>
    [Fact]
    public async Task EveryVocabularyKey_IsAlreadyInTheFormTheNormaliserWrites()
    {
        var catalogue = await PaediatricCatalogueSeeder.ReadCatalogueForTestsAsync();

        catalogue.WbaToolVocabulary.Should().OnlyContain(tool => WbaTool.NormalizeKey(tool.Key) == tool.Key);
    }

    /// <summary>
    /// Twelve instruments, no more. Learner feedback stays distinct from MSF (the College has not declared it an
    /// alias), and the two aliases are not instruments of their own (D4, D12).
    /// </summary>
    [Fact]
    public async Task TheVocabulary_IsExactlyTheCollegesTwelveInstruments()
    {
        var catalogue = await PaediatricCatalogueSeeder.ReadCatalogueForTestsAsync();

        catalogue.WbaToolVocabulary
            .Select(tool => tool.Key)
            .Order(StringComparer.Ordinal)
            .Should()
            .Equal(VocabularyKeys);
    }

    /// <summary>
    /// The College's two rulings, named. Each alias appears in exactly one EPA's cell, so a wrong resolution would
    /// quietly change exactly one EPA's list.
    /// </summary>
    [Theory]
    [InlineData("Case note review", "cca")]
    [InlineData("Directly observed clinical examination", "mini_cex")]
    public void EachOfTheCollegesAliases_ResolvesToTheInstrumentTheCollegeConfirmed(string annexureName, string expectedKey)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(CataloguePath));
        var resolver = ResolverFor(document.RootElement);

        resolver.Resolve("alias", [annexureName]).Should().Equal(expectedKey);
        resolver.Problems.Should().BeEmpty();
    }

    /// <summary>
    /// The no-loss guard for the catalogue's root (T122). A root key nothing reads is data the file promises and the
    /// product ignores, which is how the vocabulary would vanish if <c>CatalogueSeed</c> stopped binding it.
    /// </summary>
    [Fact]
    public void EveryRootKeyOfTheCatalogue_IsReadByTheSeederOrDeliberatelyUnread()
    {
        var declared = DeclaredJsonNames(typeof(PaediatricCatalogueSeeder.CatalogueSeed));
        declared.Should().Contain(
            new[] { "catalogueVersion", "scale", "wbaToolVocabulary", "epas" },
            "the guard below is vacuous unless the reflection actually sees CatalogueSeed's JSON names");

        using var document = JsonDocument.Parse(File.ReadAllText(CataloguePath));

        var problems = FindUnaccountedKeys(
            [document.RootElement], RootLabel, declared, DeliberatelyUnreadRootKeys, "CatalogueSeed", "catalogue root");

        problems.Should().BeEmpty(
            "every root key must be read by CatalogueSeed or allow-listed with a reason. Problems: {0}",
            string.Join("; ", problems));
    }

    /// <summary>
    /// The no-loss guard for each vocabulary entry (T122). The seeder reads the key, the name and the definition; the
    /// aliases and the notes are the audit trail, and are allow-listed with a reason.
    /// </summary>
    [Fact]
    public void EveryKeyOnEveryVocabularyEntry_IsReadByTheSeederOrDeliberatelyUnread()
    {
        var declared = DeclaredJsonNames(typeof(PaediatricCatalogueSeeder.WbaToolSeed));
        declared.Should().Contain(
            new[] { "key", "name", "description" },
            "the guard below is vacuous unless the reflection actually sees WbaToolSeed's JSON names");

        using var document = JsonDocument.Parse(File.ReadAllText(CataloguePath));
        var entries = document.RootElement.GetProperty("wbaToolVocabulary").EnumerateArray().ToArray();
        entries.Should().HaveCount(12);

        var problems = FindUnaccountedKeys(
            entries, VocabularyEntryLabel, declared, DeliberatelyUnreadVocabularyEntryKeys, "WbaToolSeed", "vocabulary entry");

        problems.Should().BeEmpty(
            "every vocabulary-entry key must be read by WbaToolSeed or allow-listed with a reason. Problems: {0}",
            string.Join("; ", problems));
    }

    /// <summary>The vocabulary guard is only worth having if it fails when a key goes unread.</summary>
    [Fact]
    public void VocabularyEntryGuard_DetectsAKeyNothingReads()
    {
        using var document = JsonDocument.Parse(SyntheticVocabularyEntry(extraKey: "aliasOf"));

        var problems = FindUnaccountedKeys(
            [document.RootElement],
            VocabularyEntryLabel,
            DeclaredJsonNames(typeof(PaediatricCatalogueSeeder.WbaToolSeed)),
            DeliberatelyUnreadVocabularyEntryKeys,
            "WbaToolSeed",
            "vocabulary entry");

        problems.Should().ContainSingle().Which.Should().Contain("zz_tool.aliasOf");
    }

    /// <summary>A stale entry in one direction: a key the seeder has started to read, left on the allow-list.</summary>
    [Fact]
    public void VocabularyEntryGuard_DetectsAnAllowListEntryTheSeederNowReads()
    {
        using var document = JsonDocument.Parse(SyntheticVocabularyEntry());

        var problems = FindUnaccountedKeys(
            [document.RootElement],
            VocabularyEntryLabel,
            DeclaredJsonNames(typeof(PaediatricCatalogueSeeder.WbaToolSeed)),
            [.. DeliberatelyUnreadVocabularyEntryKeys, "description"],
            "WbaToolSeed",
            "vocabulary entry");

        problems.Should().ContainSingle().Which.Should().Contain("'description'").And.Contain("WbaToolSeed now reads it");
    }

    /// <summary>
    /// A stale entry in the other direction: a key no entry carries any more. Left in place, it would quietly excuse
    /// the key if it ever came back with a different meaning.
    /// </summary>
    [Fact]
    public void VocabularyEntryGuard_DetectsAnAllowListEntryTheCatalogueNoLongerCarries()
    {
        using var document = JsonDocument.Parse(SyntheticVocabularyEntry());

        var problems = FindUnaccountedKeys(
            [document.RootElement],
            VocabularyEntryLabel,
            DeclaredJsonNames(typeof(PaediatricCatalogueSeeder.WbaToolSeed)),
            [.. DeliberatelyUnreadVocabularyEntryKeys, "retiredOn"],
            "WbaToolSeed",
            "vocabulary entry");

        problems.Should().ContainSingle().Which.Should().Contain("'retiredOn'").And.Contain("no vocabulary entry in the catalogue carries it");
    }

    /// <summary>The root guard fails on a root key nothing reads, and on stale allow-list entries both ways.</summary>
    [Fact]
    public void RootGuard_DetectsAKeyNothingReads_AndStaleAllowListEntries()
    {
        var declared = DeclaredJsonNames(typeof(PaediatricCatalogueSeeder.CatalogueSeed));

        using (var withExtra = JsonDocument.Parse(SyntheticRoot(extraKey: "annexureC")))
        {
            FindUnaccountedKeys([withExtra.RootElement], RootLabel, declared, DeliberatelyUnreadRootKeys, "CatalogueSeed", "catalogue root")
                .Should().ContainSingle().Which.Should().Contain("(root).annexureC");
        }

        using var plain = JsonDocument.Parse(SyntheticRoot());

        FindUnaccountedKeys([plain.RootElement], RootLabel, declared, [.. DeliberatelyUnreadRootKeys, "epas"], "CatalogueSeed", "catalogue root")
            .Should().ContainSingle().Which.Should().Contain("'epas'").And.Contain("CatalogueSeed now reads it");

        FindUnaccountedKeys([plain.RootElement], RootLabel, declared, [.. DeliberatelyUnreadRootKeys, "annexureB"], "CatalogueSeed", "catalogue root")
            .Should().ContainSingle().Which.Should().Contain("'annexureB'").And.Contain("no catalogue root in the catalogue carries it");
    }

    // ---------------------------------------------------------------------------------------------------------------
    // What a boot seeds
    // ---------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// The end of the chain: what a fresh database actually holds. Compared through
    /// <see cref="CurriculumItem.ParsePermittedTools" />, never as strings, because the column is <c>jsonb</c> and a
    /// raw comparison would pass on InMemory and fail on Postgres.
    /// </summary>
    [Fact]
    public async Task EverySeededItem_PermitsExactlyAnnexureAsInstruments_ForAllFifteenEpas()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        await using var dbContext = database.NewContext();
        var items = await LoadCatalogueItemsAsync(dbContext);

        // The in-memory provider enforces no unique index, so a second item for one EPA would slip past a lookup by
        // code. Count them instead.
        items.GroupBy(item => item.Code).Should().OnlyContain(group => group.Count() == 1);
        items.Select(item => item.Code).Order(StringComparer.Ordinal).Should().Equal(ExpectedAllowLists.Select(row => row.Code));

        using var scope = new AssertionScope();
        foreach (var (code, keys) in ExpectedAllowLists)
        {
            CurriculumItem.ParsePermittedTools(items.Single(item => item.Code == code).PermittedToolsJson)
                .Should().Equal(keys, "{0} is credited only by the instruments Annexure A names for it", code);
        }
    }

    /// <summary>
    /// The vocabulary rows are what a refusal message and the admin editor name, so they must be the College's names,
    /// with page 8's definition where there is one and nothing invented where there is not.
    /// </summary>
    [Fact]
    public async Task TheVocabularyRows_AreTheTwelveInstruments_NamedAndDefinedAsTheCatalogueSays()
    {
        var catalogue = await PaediatricCatalogueSeeder.ReadCatalogueForTestsAsync();
        var database = new SeedDatabase();
        await database.BootAsync();

        await using var dbContext = database.NewContext();
        var tools = await dbContext.WbaTools.AsNoTracking().ToListAsync();

        tools.Select(tool => tool.Key).Order(StringComparer.Ordinal).Should().Equal(VocabularyKeys);

        using var scope = new AssertionScope();
        foreach (var seed in catalogue.WbaToolVocabulary)
        {
            var tool = tools.Single(row => row.Key == seed.Key);
            tool.Name.Should().Be(seed.Name, "{0}'s row carries the catalogue's name", seed.Key);
            tool.Description.Should().Be(seed.Description, "{0}'s row carries the catalogue's definition", seed.Key);
        }

        tools.Where(tool => UndefinedOnPage8.Contains(tool.Key))
            .Should().HaveCount(3).And.OnlyContain(tool => tool.Description == null, "page 8 defines none of these three");
        tools.Where(tool => !UndefinedOnPage8.Contains(tool.Key))
            .Should().HaveCount(9).And.OnlyContain(tool => !string.IsNullOrWhiteSpace(tool.Description), "page 8 defines the other nine");
    }

    /// <summary>
    /// Every boot runs the vocabulary reconcile. On a database that already agrees it must be a no-op: no new rows, and
    /// no row replaced under a new id, since nothing references a tool by id but a replacement would show churn.
    /// Nor does an untouched database draw a warning from either seeder.
    /// </summary>
    [Fact]
    public async Task ASecondBoot_ChangesNoVocabularyRow_AndWarnsAboutNothing()
    {
        var database = new SeedDatabase();
        await database.BootAsync();
        var before = await database.ReadToolRowsAsync();

        var catalogueLog = new CapturingLogger<PaediatricCatalogueSeeder>();
        var dataLog = new CapturingLogger<DataSeeder>();
        await database.BootAsync(catalogueLog, dataLog);

        var after = await database.ReadToolRowsAsync();
        after.Should().HaveCount(12).And.Equal(before);
        catalogueLog.Warnings.Should().BeEmpty();
        dataLog.Warnings.Should().BeEmpty();
    }

    /// <summary>
    /// The reconcile is the one pass in this seeder that writes to an existing row, and it is safe only because
    /// nothing else writes WbaTools. A corrected display name must reach an existing database, in place.
    /// </summary>
    [Fact]
    public async Task ASecondBoot_RestoresAVocabularyRowWhoseNameOrDefinitionWasChanged_InPlace()
    {
        var database = new SeedDatabase();
        await database.BootAsync();
        var before = await database.ReadToolRowsAsync();

        await database.EditAsync(async dbContext =>
        {
            var cbd = await dbContext.WbaTools.SingleAsync(tool => tool.Key == "cbd");
            cbd.Name = "Case discussion";
            cbd.Description = "Edited behind the seeder's back.";

            var learnerFeedback = await dbContext.WbaTools.SingleAsync(tool => tool.Key == "learner_feedback");
            learnerFeedback.Description = "An invented definition.";
        });

        await database.BootAsync();

        var after = await database.ReadToolRowsAsync();
        after.Should().Equal(before, "the catalogue is the only author a WbaTool row has, so the boot restores it in place");
    }

    /// <summary>
    /// Which instrument each seeded type is, after a fresh boot. Both seeders stamp it on create, and a type with the
    /// wrong key is refused, or unrestricted, on every EPA that has a list.
    /// </summary>
    [Fact]
    public async Task EverySeededActivityType_CarriesTheInstrumentKeyItsSeedIsDeclaredToBe()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        await using var dbContext = database.NewContext();
        var stored = await dbContext.ActivityTypes
            .AsNoTracking()
            .Select(type => new { type.Key, type.WbaToolKey })
            .ToListAsync();

        stored.Select(type => (TypeKey: type.Key, type.WbaToolKey))
            .OrderBy(type => type.TypeKey, StringComparer.Ordinal)
            .Should()
            .Equal(SeededTypeToolKeys.OrderBy(type => type.TypeKey, StringComparer.Ordinal));
    }

    /// <summary>
    /// The hand-typed table above must name every seed entry, so a new seed cannot arrive without someone stating
    /// here which instrument it is. Pins the table's coverage, not the values, which the boot test pins.
    /// </summary>
    [Fact]
    public void TheSeededTypeTable_NamesEverySeedEntry_AndAgreesWithIt()
    {
        ActivityTypeSeedCatalogue.Entries
            .Select(entry => (TypeKey: entry.Key, entry.WbaToolKey))
            .OrderBy(entry => entry.TypeKey, StringComparer.Ordinal)
            .Should()
            .Equal(SeededTypeToolKeys.OrderBy(type => type.TypeKey, StringComparer.Ordinal));
    }

    /// <summary>
    /// A CPSA seed with no key would, under D21, credit every EPA of the curriculum, and one naming a key the
    /// vocabulary does not define would be refused on every EPA. Either is a College instrument with no instrument.
    /// </summary>
    [Fact]
    public async Task EveryCpsaSeedEntry_NamesAnInstrumentTheVocabularyDefines()
    {
        var catalogue = await PaediatricCatalogueSeeder.ReadCatalogueForTestsAsync();
        var vocabulary = catalogue.WbaToolVocabulary.Select(tool => tool.Key).ToHashSet(StringComparer.Ordinal);

        var entries = ActivityTypeSeedCatalogue.For(ActivityTypeSeedSource.PaediatricCollege).ToArray();
        entries.Should().HaveCount(9);

        using var scope = new AssertionScope();
        foreach (var entry in entries)
        {
            entry.WbaToolKey.Should().NotBeNull("{0} is a College-named instrument", entry.Key);
            vocabulary.Should().Contain(entry.WbaToolKey!, "{0}'s instrument key must be one the vocabulary defines", entry.Key);
        }
    }

    /// <summary>
    /// The generic Mini-CEX, DOPS and CBD are keyed because they ARE those instruments: unkeyed, anyone who can file
    /// them could credit any EPA of a curriculum with lists. The other seven generics are not College-named
    /// instruments and stay unrestricted (D21). A key on one of them reaches existing databases only by a new
    /// migration, so a change here is a decision, not a tidy-up.
    /// </summary>
    [Fact]
    public async Task TheGenericSeedEntries_KeyExactlyMiniCexDopsAndCbd_AndNothingElse()
    {
        var catalogue = await PaediatricCatalogueSeeder.ReadCatalogueForTestsAsync();
        var vocabulary = catalogue.WbaToolVocabulary.Select(tool => tool.Key).ToHashSet(StringComparer.Ordinal);
        var generic = ActivityTypeSeedCatalogue.For(ActivityTypeSeedSource.Generic).ToArray();

        generic.Where(entry => entry.WbaToolKey is not null)
            .Select(entry => (entry.Key, entry.WbaToolKey))
            .OrderBy(entry => entry.Key, StringComparer.Ordinal)
            .Should()
            .Equal(("cbd", "cbd"), ("dops", "dops"), ("mini_cex", "mini_cex"));

        generic.Where(entry => entry.WbaToolKey is not null)
            .Should().OnlyContain(entry => vocabulary.Contains(entry.WbaToolKey!));

        generic.Where(entry => entry.WbaToolKey is null)
            .Select(entry => entry.Key)
            .Order(StringComparer.Ordinal)
            .Should()
            .Equal("acat", "journal_club", "procedure_log", "qi_project", "reflective_note", "research_output", "teaching_session");
    }

    // ---------------------------------------------------------------------------------------------------------------
    // Warn, never write
    // ---------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Null is "any instrument", and null is also what an administrator saves when they clear a list or withdraw an
    /// instrument on purpose. A boot-time backfill cannot tell those apart and would silently re-impose a restriction
    /// someone withdrew, so both seeders warn and write nothing. The warning is the behaviour; a missing one lets the
    /// difference go unnoticed for ever.
    /// </summary>
    [Fact]
    public async Task Reseeding_WarnsOnceAboutAClearedListAndOnceAboutAWithdrawnInstrument_AndWritesNeither()
    {
        var database = new SeedDatabase();
        var firstCatalogueLog = new CapturingLogger<PaediatricCatalogueSeeder>();
        var firstDataLog = new CapturingLogger<DataSeeder>();
        await database.BootAsync(firstCatalogueLog, firstDataLog);

        firstCatalogueLog.Warnings.Should().BeEmpty("a freshly seeded database agrees with the catalogue it was seeded from");
        firstDataLog.Warnings.Should().BeEmpty();

        await database.SetPermittedToolsAsync("PAED-005", null);
        await database.SetWbaToolKeyAsync("mini_cex_cpsa", null);

        var catalogueLog = new CapturingLogger<PaediatricCatalogueSeeder>();
        var dataLog = new CapturingLogger<DataSeeder>();
        await database.BootAsync(catalogueLog, dataLog);

        catalogueLog.Warnings.Should().HaveCount(2);

        var listWarning = catalogueLog.Warnings.Should().ContainSingle(entry => entry.Values.ContainsKey("EpaCode")).Which;
        listWarning.Values["EpaCode"].Should().Be("PAED-005");
        listWarning.Values["StoredTools"].Should().Be("any instrument");
        listWarning.Values["ExpectedTools"].Should().Be("cbd, dops, msf");
        listWarning.Message.Should().Contain("PAED-005");

        var keyWarning = catalogueLog.Warnings.Should().ContainSingle(entry => entry.Values.ContainsKey("StoredWbaToolKey")).Which;
        keyWarning.Values["Key"].Should().Be("mini_cex_cpsa");
        keyWarning.Values["StoredWbaToolKey"].Should().Be("(none)");
        keyWarning.Values["ExpectedWbaToolKey"].Should().Be("mini_cex");
        keyWarning.Message.Should().Contain("mini_cex_cpsa");

        dataLog.Warnings.Should().BeEmpty("mini_cex_cpsa is the catalogue seeder's type, not DataSeeder's");

        await using var dbContext = database.NewContext();
        var items = await LoadCatalogueItemsAsync(dbContext);
        items.Should().HaveCount(15, "a reseed adds nothing to a curriculum that already holds every EPA");
        items.Single(item => item.Code == "PAED-005").PermittedToolsJson.Should().BeNull("the seeder warns and never writes");

        using (new AssertionScope())
        {
            foreach (var (code, keys) in ExpectedAllowLists.Where(row => row.Code != "PAED-005"))
            {
                CurriculumItem.ParsePermittedTools(items.Single(item => item.Code == code).PermittedToolsJson)
                    .Should().Equal(keys, "{0} was not edited and must still permit Annexure A's instruments", code);
            }
        }

        var miniCex = await dbContext.ActivityTypes.AsNoTracking().SingleAsync(type => type.Key == "mini_cex_cpsa");
        miniCex.WbaToolKey.Should().BeNull("the seeder stamps an instrument on create only");
    }

    /// <summary>
    /// The comparison is canonical to canonical. On Postgres the stored <c>jsonb</c> never comes back byte-equal to
    /// what was written, so a raw comparison would warn about every item on every boot, and a warning that always
    /// fires is one nobody reads. Order, case, padding and duplicates are not differences.
    /// </summary>
    [Theory]
    [InlineData("""["msf","cbd","dops"]""")]
    [InlineData("""["msf", "cbd", "dops"]""")]
    [InlineData("""[" MSF ","cbd","DOPS","dops"]""")]
    public async Task Reseeding_DoesNotWarnAboutAListThatIsTheSameSetStoredDifferently(string storedJson)
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        await database.SetPermittedToolsAsync("PAED-005", storedJson);

        var catalogueLog = new CapturingLogger<PaediatricCatalogueSeeder>();
        await database.BootAsync(catalogueLog);

        catalogueLog.Warnings.Should().BeEmpty("{0} is the same set as Annexure A's CBD, DOPS and MSF", storedJson);

        await using var dbContext = database.NewContext();
        var item = (await LoadCatalogueItemsAsync(dbContext)).Single(entry => entry.Code == "PAED-005");

        // Not a stored-versus-serialiser comparison: InMemory keeps the bytes it was given, and this checks only that
        // the seeder did not write the row back in its own form.
        item.PermittedToolsJson.Should().Be(storedJson, "the seeder never writes an existing item, even to tidy it");
        CurriculumItem.ParsePermittedTools(item.PermittedToolsJson).Should().Equal("cbd", "dops", "msf");
    }

    /// <summary>The instrument-key comparison normalises too: casing or padding is not a different instrument.</summary>
    [Fact]
    public async Task Reseeding_DoesNotWarnAboutAnInstrumentKeyThatDiffersOnlyInCaseOrPadding()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        await database.SetWbaToolKeyAsync("dops_cpsa", " DOPS ");

        var catalogueLog = new CapturingLogger<PaediatricCatalogueSeeder>();
        await database.BootAsync(catalogueLog);

        catalogueLog.Warnings.Should().BeEmpty();
    }

    /// <summary>
    /// A type an operator has taken over (another owner) is theirs to key as they choose. Reporting it at every
    /// startup would be noise about a decision somebody made on purpose.
    /// </summary>
    [Fact]
    public async Task Reseeding_DoesNotReportTheInstrumentKeyOfASeedTypeAnOperatorHasTakenOver()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        await database.EditAsync(async dbContext =>
        {
            var type = await dbContext.ActivityTypes.SingleAsync(entity => entity.Key == "mini_cex_cpsa");
            type.OwnerUserId = "operator-1";
            type.WbaToolKey = null;
        });

        var catalogueLog = new CapturingLogger<PaediatricCatalogueSeeder>();
        await database.BootAsync(catalogueLog);

        catalogueLog.Warnings.Should().BeEmpty();
    }

    /// <summary>
    /// DataSeeder's half of the same contract, for the three generic instruments it seeds. It gained an optional
    /// logger for this in T122; without one the warning would go nowhere.
    /// </summary>
    [Fact]
    public async Task DataSeeder_WarnsOnceAboutAGenericInstrumentWhoseKeyWasWithdrawn_AndWritesNothing()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        await database.SetWbaToolKeyAsync("mini_cex", null);

        var catalogueLog = new CapturingLogger<PaediatricCatalogueSeeder>();
        var dataLog = new CapturingLogger<DataSeeder>();
        await database.BootAsync(catalogueLog, dataLog);

        var warning = dataLog.Warnings.Should().ContainSingle().Subject;
        warning.Values["Key"].Should().Be("mini_cex");
        warning.Values["StoredWbaToolKey"].Should().Be("(none)");
        warning.Values["ExpectedWbaToolKey"].Should().Be("mini_cex");
        warning.Message.Should().Contain("mini_cex");

        catalogueLog.Warnings.Should().BeEmpty("the generic Mini-CEX is DataSeeder's type, not the catalogue seeder's");

        await using var dbContext = database.NewContext();
        var type = await dbContext.ActivityTypes.AsNoTracking().SingleAsync(entity => entity.Key == "mini_cex");
        type.WbaToolKey.Should().BeNull("DataSeeder stamps an instrument on create only");
    }

    /// <summary>
    /// National core is not a synonym for seeded. A CollegeAdmin may add their own national item to this curriculum
    /// for an EPA the catalogue does not declare; the catalogue has no list for it, so neither the seeder nor its
    /// warning may invent one. It stays unrestricted (D21) until its author decides otherwise.
    /// </summary>
    [Fact]
    public async Task ACollegeAdminsOwnNationalItem_OnAnEpaTheCatalogueDoesNotDeclare_StaysUnrestrictedAcrossBoots()
    {
        var database = new SeedDatabase();
        await database.BootAsync();

        var itemId = 0;
        await database.EditAsync(async dbContext =>
        {
            var curriculum = await dbContext.Curricula
                .SingleAsync(entity => entity.Name == CurriculumName && entity.Version == CatalogueVersion);

            var epa = new Epa
            {
                SubSpecialityId = curriculum.SubSpecialityId,
                OwningInstitutionId = null,
                Code = "PAED-016",
                Title = "A CollegeAdmin's own national EPA",
                CreatedOn = DateTime.UtcNow
            };
            dbContext.Epas.Add(epa);
            await dbContext.SaveChangesAsync();

            var item = new CurriculumItem
            {
                CurriculumId = curriculum.Id,
                EpaId = epa.Id,
                OwningInstitutionId = null,
                RequiredCount = 1,
                QuotaPeriod = QuotaPeriod.AcademicYear,
                MinimumLevelOrder = 3,
                WindowMonths = 12,
                PermittedToolsJson = null
            };
            dbContext.CurriculumItems.Add(item);
            await dbContext.SaveChangesAsync();
            itemId = item.Id;
        });

        var secondBoot = new CapturingLogger<PaediatricCatalogueSeeder>();
        await database.BootAsync(secondBoot);
        var thirdBoot = new CapturingLogger<PaediatricCatalogueSeeder>();
        await database.BootAsync(thirdBoot);

        secondBoot.Warnings.Should().BeEmpty("the catalogue declares nothing for PAED-016, so there is nothing to differ from");
        thirdBoot.Warnings.Should().BeEmpty();

        await using var dbContext = database.NewContext();
        var stored = await dbContext.CurriculumItems.AsNoTracking().SingleAsync(entity => entity.Id == itemId);
        stored.PermittedToolsJson.Should().BeNull("an item the catalogue does not declare is never given a list");
    }

    // ---------------------------------------------------------------------------------------------------------------
    // The T122 migration
    // ---------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// The migration's frozen copy is held to the snapshot it shipped with, permanently. Not to the live catalogue:
    /// a later change to a list is a new migration, and this one must go on stamping exactly what it stamped.
    /// </summary>
    [Fact]
    public void TheMigrationsFrozenToolLists_AreTheT122Snapshot()
    {
        T122_WbaToolAllowLists.PermittedTools.Should().Equal(T122PermittedToolsSnapshot);
    }

    /// <summary>
    /// The frozen literals are what the migration writes into a <c>jsonb</c> column, and what a later list read
    /// compares canonically. Each must be exactly the stored form the normaliser produces, so a migrated database and
    /// a freshly seeded one hold the same thing.
    /// </summary>
    [Fact]
    public void EveryFrozenToolList_IsExactlyWhatTheNormaliserWrites()
    {
        using var scope = new AssertionScope();
        foreach (var (code, toolsJson) in T122_WbaToolAllowLists.PermittedTools)
        {
            var keys = JsonSerializer.Deserialize<string[]>(toolsJson);
            keys.Should().NotBeNullOrEmpty("{0}'s frozen list must be a non-empty array", code);

            CurriculumItem.NormalizePermittedToolsJson(keys).Should().Be(toolsJson, "{0}'s frozen literal must be canonical", code);
        }
    }

    /// <summary>
    /// The frozen instrument keys are held to their snapshot, and must not assert anything the seed catalogue
    /// contradicts. A subset, not an equality: a later seed may carry a key this migration never stamped.
    /// </summary>
    [Fact]
    public void TheMigrationsFrozenInstrumentKeys_AreTheT122Snapshot_AndASubsetOfTheSeedCatalogue()
    {
        T122_WbaToolAllowLists.SeededToolKeys.Should().Equal(T122SeededToolKeysSnapshot);

        var entries = ActivityTypeSeedCatalogue.Entries.ToDictionary(entry => entry.Key, StringComparer.Ordinal);

        using var scope = new AssertionScope();
        foreach (var (typeKey, wbaToolKey) in T122_WbaToolAllowLists.SeededToolKeys)
        {
            entries.Should().ContainKey(typeKey, "the migration stamps only seeded types");
            if (entries.TryGetValue(typeKey, out var entry))
            {
                entry.WbaToolKey.Should().Be(wbaToolKey, "the migration and the seed catalogue must agree which instrument {0} is", typeKey);
            }
        }
    }

    /// <summary>
    /// The migration's ownership guard keys off a literal. If it ever stopped matching the seed actor, the stamp would
    /// touch no row and every existing database would keep its seeded instruments unkeyed, with nobody told.
    /// </summary>
    [Fact]
    public void TheMigrationsSeedOwner_IsTheSeedActor()
    {
        T122_WbaToolAllowLists.SeedOwnerUserId.Should().Be(ActivityTypeSeedCatalogue.SeedActorUserId);
    }

    /// <summary>
    /// The tool-list stamp must reach the catalogue's own national items at v11.1 and nothing else: not a clone at
    /// another version, not an institution's local item, not a CollegeAdmin's EPA, and never a list somebody has
    /// already set.
    /// </summary>
    [Fact]
    public void TheToolListBackfill_IsScopedToTheCataloguesOwnNationalItems_AndOnlyWhereStillNull()
    {
        var sql = T122_WbaToolAllowLists.BuildPermittedToolsBackfill();

        sql.Should().Contain("ss.\"Name\" = 'Paediatrics'");
        sql.Should().Contain($"c.\"Name\" = '{CurriculumName}'");
        sql.Should().Contain($"c.\"Version\" = '{CatalogueVersion}'");
        sql.Should().Contain("ci.\"OwningInstitutionId\" IS NULL", "an institution's local item is never the College's to restrict");
        sql.Should().Contain("e.\"OwningInstitutionId\" IS NULL", "an institution's local EPA is never the College's to restrict");
        sql.Should().Contain("ci.\"PermittedToolsJson\" IS NULL", "a re-run must never overwrite a list somebody set");
        sql.Should().Contain("::jsonb");

        foreach (var (code, toolsJson) in T122PermittedToolsSnapshot)
        {
            sql.Should().Contain($"('{code}', '{toolsJson}')");
        }

        Regex.Matches(sql, @"\('PAED-\d{3}'").Should().HaveCount(15, "each EPA is stamped exactly once");
    }

    /// <summary>
    /// The instrument stamp must touch only types the seeder still owns, and only where no key is set: a type an
    /// operator built under a colliding key is theirs, and is not asserted to be anything.
    /// </summary>
    [Fact]
    public void TheInstrumentKeyBackfill_IsScopedToSeedOwnedTypes_AndOnlyWhereStillNull()
    {
        var sql = T122_WbaToolAllowLists.BuildSeededToolKeysBackfill();

        sql.Should().Contain("t.\"OwnerUserId\" = 'seed-system'");
        sql.Should().Contain("t.\"WbaToolKey\" IS NULL");

        foreach (var (typeKey, wbaToolKey) in T122SeededToolKeysSnapshot)
        {
            sql.Should().Contain($"('{typeKey}', '{wbaToolKey}')");
        }

        Regex.Matches(sql, @"\('[a-z_]+', '[a-z_]+'\)").Should().HaveCount(8, "each seeded instrument type is stamped exactly once");
    }

    // ---------------------------------------------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------------------------------------------

    private static string CataloguePath
        => Path.Combine(AppContext.BaseDirectory, "Persistence", "Seeds", "paediatric-epa-v11.1.json");

    private static string RootLabel(JsonElement element, int index) => "(root)";

    private static string VocabularyEntryLabel(JsonElement element, int index)
        => element.TryGetProperty("key", out var key) && key.ValueKind == JsonValueKind.String
            ? key.GetString()!
            : $"wbaToolVocabulary[{index}]";

    /// <summary>Each EPA's verbatim Annexure A cell, as the catalogue carries it, in file order.</summary>
    private static IReadOnlyList<(string Code, string[] Tools)> ReadAnnexureCells(JsonElement root)
        => root.GetProperty("epas")
            .EnumerateArray()
            .Select(epa => (
                Code: epa.GetProperty("code").GetString()!,
                Tools: epa.GetProperty("annexureTools").EnumerateArray().Select(tool => tool.GetString()!).ToArray()))
            .ToList();

    /// <summary>
    /// A resolver over the catalogue's own vocabulary, read raw: <c>annexureNames</c> is deliberately not bound by
    /// the seeder, so only the raw JSON has it.
    /// </summary>
    private static AnnexureNameResolver ResolverFor(JsonElement root)
        => new(root.GetProperty("wbaToolVocabulary")
            .EnumerateArray()
            .Select(entry => (
                Key: entry.GetProperty("key").GetString()!,
                Names: entry.TryGetProperty("annexureNames", out var names) && names.ValueKind == JsonValueKind.Array
                    ? names.EnumerateArray().Select(name => name.GetString()!).ToArray()
                    : Array.Empty<string>()))
            .ToList());

    /// <summary>
    /// Every problem a set of catalogue objects has with the seed record that binds them: a key read by nothing and not
    /// allow-listed, or an allow-list entry gone stale because the record now reads it or no object carries it.
    /// </summary>
    /// <remarks>
    /// The same contract as <c>PaediatricCatalogueQuotaSeedTests.FindUnaccountedKeys</c>, generalised over which
    /// objects and which record. Declared names compare case-insensitively because that is how the seeder binds them
    /// (<c>PropertyNameCaseInsensitive = true</c>): a key the deserializer would read counts as read.
    /// </remarks>
    private static IReadOnlyList<string> FindUnaccountedKeys(
        IReadOnlyList<JsonElement> objects,
        Func<JsonElement, int, string> label,
        IReadOnlyCollection<string> declaredKeys,
        IReadOnlyCollection<string> allowList,
        string reader,
        string carrier)
    {
        var declared = new HashSet<string>(declaredKeys, StringComparer.OrdinalIgnoreCase);
        var allowed = new HashSet<string>(allowList, StringComparer.Ordinal);
        var carried = new HashSet<string>(StringComparer.Ordinal);
        var problems = new List<string>();

        for (var index = 0; index < objects.Count; index++)
        {
            var element = objects[index];
            foreach (var property in element.EnumerateObject())
            {
                carried.Add(property.Name);

                if (!declared.Contains(property.Name) && !allowed.Contains(property.Name))
                {
                    problems.Add($"{label(element, index)}.{property.Name}: carried by the catalogue, read by nothing, and not allow-listed");
                }
            }
        }

        foreach (var name in allowed)
        {
            if (declared.Contains(name))
            {
                problems.Add($"'{name}' is allow-listed, but {reader} now reads it: take it off the allow-list");
            }

            if (!carried.Contains(name))
            {
                problems.Add($"'{name}' is allow-listed, but no {carrier} in the catalogue carries it: take it off the allow-list");
            }
        }

        return problems;
    }

    /// <summary>The JSON names a seed record binds, read off its <see cref="JsonPropertyNameAttribute" />s.</summary>
    private static IReadOnlyCollection<string> DeclaredJsonNames(Type seedRecord)
        => seedRecord
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name)
            .ToArray();

    /// <summary>
    /// One vocabulary entry carrying every key the seeder reads and every allow-listed key, so the only problem the
    /// guard can report is the one a test adds.
    /// </summary>
    private static string SyntheticVocabularyEntry(string? extraKey = null)
        => SyntheticObject(
            DeclaredJsonNames(typeof(PaediatricCatalogueSeeder.WbaToolSeed)).Concat(DeliberatelyUnreadVocabularyEntryKeys),
            extraKey,
            ("key", "zz_tool"));

    /// <summary>A catalogue root carrying every key the seeder reads and every allow-listed key.</summary>
    private static string SyntheticRoot(string? extraKey = null)
        => SyntheticObject(
            DeclaredJsonNames(typeof(PaediatricCatalogueSeeder.CatalogueSeed)).Concat(DeliberatelyUnreadRootKeys),
            extraKey);

    private static string SyntheticObject(
        IEnumerable<string> keys,
        string? extraKey,
        params (string Key, object Value)[] overrides)
    {
        var entry = new Dictionary<string, object?>(StringComparer.Ordinal);

        foreach (var key in keys)
        {
            entry[key] = 0;
        }

        foreach (var (key, value) in overrides)
        {
            entry[key] = value;
        }

        if (extraKey is not null)
        {
            entry[extraKey] = 2;
        }

        return JsonSerializer.Serialize(entry);
    }

    private static async Task<IReadOnlyList<SeededItem>> LoadCatalogueItemsAsync(ApplicationDbContext dbContext)
        => await (
                from item in dbContext.CurriculumItems.AsNoTracking()
                join curriculum in dbContext.Curricula on item.CurriculumId equals curriculum.Id
                join epa in dbContext.Epas on item.EpaId equals epa.Id
                where curriculum.Name == CurriculumName
                      && curriculum.Version == CatalogueVersion
                      && item.OwningInstitutionId == null
                      && epa.OwningInstitutionId == null
                select new SeededItem(item.Id, epa.Code, item.PermittedToolsJson))
            .ToListAsync();

    private sealed record SeededItem(int Id, string Code, string? PermittedToolsJson);

    private sealed record ToolRow(int Id, string Key, string Name, string? Description);

    /// <summary>
    /// Resolves Annexure A's verbatim names to vocabulary keys through each entry's <c>annexureNames</c>, collecting
    /// every problem rather than stopping at the first: a name no entry lists, and a name listed under two keys.
    /// </summary>
    private sealed class AnnexureNameResolver
    {
        private readonly Dictionary<string, string> _keyByName = new(StringComparer.Ordinal);

        public AnnexureNameResolver(IReadOnlyList<(string Key, string[] Names)> vocabulary)
        {
            foreach (var (key, names) in vocabulary)
            {
                foreach (var name in names)
                {
                    if (_keyByName.TryGetValue(name, out var other))
                    {
                        if (!string.Equals(other, key, StringComparison.Ordinal))
                        {
                            Problems.Add($"'{name}' is listed under both '{other}' and '{key}'");
                        }

                        continue;
                    }

                    _keyByName[name] = key;
                }
            }
        }

        public List<string> Problems { get; } = [];

        /// <summary>The keys the names stand for, in the names' order. Exact, case-sensitive matching: names are verbatim.</summary>
        public IReadOnlyList<string> Resolve(string code, IEnumerable<string> names)
        {
            var keys = new List<string>();

            foreach (var name in names)
            {
                if (_keyByName.TryGetValue(name, out var key))
                {
                    keys.Add(key);
                }
                else
                {
                    Problems.Add($"{code}: '{name}' has no annexureNames entry in the vocabulary");
                }
            }

            return keys;
        }
    }

    /// <summary>
    /// One in-memory database that outlives any single context, so a test can boot, edit and boot again the way a
    /// deployed server does: each startup gets a fresh scope and a fresh <see cref="ApplicationDbContext" />. Copied
    /// from <c>PaediatricCatalogueQuotaSeedTests</c>, with DataSeeder's logger added.
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
        public async Task BootAsync(
            ILogger<PaediatricCatalogueSeeder>? catalogueLogger = null,
            ILogger<DataSeeder>? dataLogger = null)
        {
            await using var dbContext = NewContext();
            await new DataSeeder(dbContext, dataLogger).SeedAsync();
            await new PaediatricCatalogueSeeder(dbContext, catalogueLogger).SeedAsync();
        }

        /// <summary>An edit made between two startups, by an administrator or by hand.</summary>
        public async Task EditAsync(Func<ApplicationDbContext, Task> edit)
        {
            await using var dbContext = NewContext();
            await edit(dbContext);
            await dbContext.SaveChangesAsync();
        }

        /// <summary>Stores a raw value in one seeded item's list, bypassing the normaliser the way direct SQL would.</summary>
        public Task SetPermittedToolsAsync(string code, string? rawJson)
            => EditAsync(async dbContext =>
            {
                var seeded = (await LoadCatalogueItemsAsync(dbContext)).Single(entry => entry.Code == code);
                var item = await dbContext.CurriculumItems.SingleAsync(entity => entity.Id == seeded.Id);
                item.PermittedToolsJson = rawJson;
            });

        public Task SetWbaToolKeyAsync(string typeKey, string? wbaToolKey)
            => EditAsync(async dbContext =>
            {
                var type = await dbContext.ActivityTypes.SingleAsync(entity => entity.Key == typeKey);
                type.WbaToolKey = wbaToolKey;
            });

        public async Task<IReadOnlyList<ToolRow>> ReadToolRowsAsync()
        {
            await using var dbContext = NewContext();
            return await dbContext.WbaTools
                .AsNoTracking()
                .OrderBy(tool => tool.Id)
                .Select(tool => new ToolRow(tool.Id, tool.Key, tool.Name, tool.Description))
                .ToListAsync();
        }
    }
}
