using System.Data.Common;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Logging;
using Npgsql;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Persistence.Migrations;

namespace Wombat.Integration.Tests.Curricula;

/// <summary>
/// T122 on a real PostgreSQL server: the migration's one-off stamping of existing databases, the seeders'
/// warn-and-never-write contract, and the <c>jsonb</c> allow-list column.
/// </summary>
/// <remarks>
/// <para>
/// None of this is visible to the unit suites. They run on EF InMemory, which never runs a migration, so the two
/// hand-written UPDATEs in <c>T122_WbaToolAllowLists</c> are exercised nowhere else. InMemory also stores a string as
/// it was written, whereas Postgres discards the submitted <c>jsonb</c> text and renders its own. A seeder warning
/// that compared strings would therefore stay silent in every unit test and fire on every boot of dev and production.
/// </para>
/// <para>
/// The schema helpers are copied from <see cref="AcademicPeriodQuotaPostgresTests" />. Each test builds
/// <see cref="ApplicationDbContext" /> directly on a schema of its own (<c>SearchPath = it_&lt;guid&gt;</c>, with
/// nothing else on the path), so no statement can reach <c>public</c>. Every schema is registered BEFORE it is
/// created. Each test drops its schemas in a <c>finally</c>, and <see cref="DisposeAsync" /> repeats the drop as a
/// backstop. Nothing that can fail runs in <see cref="InitializeAsync" />.
/// </para>
/// <para>
/// The oracles are typed out by hand from Annexure A of CPSA EPA v11.1: the allow-lists, the vocabulary and the
/// seeded instrument keys. They are deliberately not read from the catalogue file or from the migration's frozen
/// arrays, because an oracle read from the thing under test agrees with it by construction.
/// </para>
/// </remarks>
public sealed class WbaToolAllowListPostgresTests : IAsyncLifetime
{
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";

    private const string T122Migration = "20260923105040_T122_WbaToolAllowLists";
    private const string LastMigrationBeforeT122 = "20260923082939_T130_AcademicPeriodQuota";
    private const string PaediatricCurriculumName = "Paediatric EPA Curriculum";
    private const string CatalogueVersion = "11.1";

    /// <summary>A literal, like the migration's: the owner guard must match what the seeders really wrote.</summary>
    private const string SeedOwnerUserId = "seed-system";

    private const string OperatorUserId = "admin-legacy";

    /// <summary>The seeded key an operator's own type took before the seeder could create it.</summary>
    private const string OperatorCollisionKey = "mini_cex_cpsa";

    /// <summary>
    /// Annexure A's tools column for EPA v11.1 as tool keys, sorted ordinally. The College's rulings are applied:
    /// "Case note review" is CCA (D4), "Directly observed clinical examination" is Mini-CEX (D12), and EPA 7 gains
    /// Direct observation (D12). Mini-CEX is absent from PAED-005, 009, 010, 011, 014 and 015, which is the gap T122
    /// closes.
    /// </summary>
    private static readonly (string Code, string[] Tools)[] AnnexureA =
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

    /// <summary>
    /// Annexure B's targets, copied from <see cref="AcademicPeriodQuotaPostgresTests" />. The populated pre-T122
    /// database carries them because T130 had already corrected every existing database by then. Any other value
    /// would make the seeder's T130 target warning fire, and a boot that warned about targets could hide a
    /// tool-list warning.
    /// </summary>
    private static readonly (string Code, QuotaPeriod Period, int Target)[] AnnexureB =
    [
        ("PAED-001", QuotaPeriod.Semester, 3),
        ("PAED-002", QuotaPeriod.Semester, 3),
        ("PAED-003", QuotaPeriod.Semester, 3),
        ("PAED-004", QuotaPeriod.Semester, 3),
        ("PAED-005", QuotaPeriod.Semester, 3),
        ("PAED-006", QuotaPeriod.Semester, 2),
        ("PAED-007", QuotaPeriod.Semester, 1),
        ("PAED-008", QuotaPeriod.AcademicYear, 1),
        ("PAED-009", QuotaPeriod.AcademicYear, 1),
        ("PAED-010", QuotaPeriod.Semester, 3),
        ("PAED-011", QuotaPeriod.AcademicYear, 1),
        ("PAED-012", QuotaPeriod.Semester, 3),
        ("PAED-013", QuotaPeriod.AcademicYear, 1),
        ("PAED-014", QuotaPeriod.AcademicYear, 1),
        ("PAED-015", QuotaPeriod.Semester, 1),
    ];

    /// <summary>The College's twelve instruments, as (key, Annexure A display name), ordinal by key.</summary>
    private static readonly (string Key, string Name)[] Vocabulary =
    [
        ("cbd", "CBD"),
        ("cca", "CCA"),
        ("chart_stimulated_recall", "Chart-stimulated recall"),
        ("clinical_audit", "Clinical audit"),
        ("direct_observation", "Direct observation"),
        ("dops", "DOPS"),
        ("learner_feedback", "Learner feedback"),
        ("mini_cex", "Mini-CEX"),
        ("msf", "MSF"),
        ("portfolio_review", "Portfolio and logbook review"),
        ("rca", "RCA"),
        ("reflective_exercise", "Reflective exercise"),
    ];

    /// <summary>
    /// The seeded types that ARE College instruments, ordinal by type key: the eleven CPSA seeds and the generic
    /// Mini-CEX, DOPS and CBD. ACAT and the other generics are not instruments the College names, so D21 leaves them
    /// unkeyed.
    /// </summary>
    private static readonly (string TypeKey, string ToolKey)[] SeededToolKeys =
    [
        ("cbd", "cbd"),
        ("cbd_cpsa", "cbd"),
        ("cca_cpsa", "cca"),
        ("chart_stimulated_recall_cpsa", "chart_stimulated_recall"),
        ("clinical_audit_cpsa", "clinical_audit"),
        ("direct_observation_cpsa", "direct_observation"),
        ("dops", "dops"),
        ("dops_cpsa", "dops"),
        ("mini_cex", "mini_cex"),
        ("mini_cex_cpsa", "mini_cex"),
        ("msf_cpsa", "msf"),
        ("portfolio_review_cpsa", "portfolio_review"),
        ("rca_cpsa", "rca"),
        ("reflective_exercise_cpsa", "reflective_exercise"),
    ];

    /// <summary>
    /// The instruments seeded after T122 shipped: T120's four and T154's two. A seed added after T122 is keyed where it
    /// is new, by the seeder on create, with no migration: on an upgraded database they are the only types a boot
    /// creates.
    /// </summary>
    private static readonly (string TypeKey, string ToolKey)[] TypesAddedAfterT122 =
    [
        ("cca_cpsa", "cca"),
        ("chart_stimulated_recall_cpsa", "chart_stimulated_recall"),
        ("rca_cpsa", "rca"),
        ("reflective_exercise_cpsa", "reflective_exercise"),
        ("clinical_audit_cpsa", "clinical_audit"),
        ("portfolio_review_cpsa", "portfolio_review"),
    ];

    /// <summary>Every activity type the two seeders had created when T122 shipped.</summary>
    private static readonly string[] TypesSeededBeforeT122 =
    [
        "mini_cex", "dops", "cbd", "acat", "reflective_note", "procedure_log", "research_output", "teaching_session",
        "qi_project", "journal_club", "mini_cex_cpsa", "dops_cpsa", "cbd_cpsa", "direct_observation_cpsa", "msf_cpsa",
    ];

    private readonly List<string> _schemas = [];
    private string _baseConnectionString = null!;

    public Task InitializeAsync()
    {
        _baseConnectionString = ResolveBaseConnectionString();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => DropSchemasAsync();

    [Fact]
    public async Task MigrateAsync_OnAFreshDatabase_AddsBothColumnsAndAnEmptyVocabularyTable_WithNoForeignKeyIntoIt()
    {
        // MigrateAsync skips a hand-edited migration that has no Designer file, and reports no error. Two of this
        // migration's choices are load-bearing, so the shape is checked column by column:
        //  - PermittedToolsJson is jsonb, so every reader must compare canonical to canonical;
        //  - nothing has a foreign key into WbaTools. The migration stamps ActivityTypes.WbaToolKey BEFORE the
        //    seeder inserts the vocabulary, so a foreign key would make that stamp fail on every existing database
        //    and stop the app at startup.
        try
        {
            var schema = await CreateSchemaAsync();

            await using (var db = NewContext(schema))
            {
                await db.Database.MigrateAsync();

                (await db.Database.GetAppliedMigrationsAsync()).Should().Contain(T122Migration);
                (await db.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
            }

            var columns = await QueryAsync(
                schema,
                """
                SELECT table_name, column_name, data_type, character_maximum_length, is_nullable
                FROM information_schema.columns
                WHERE table_schema = $1
                  AND ((table_name = 'CurriculumItems' AND column_name = 'PermittedToolsJson')
                    OR (table_name = 'ActivityTypes' AND column_name = 'WbaToolKey')
                    OR table_name = 'WbaTools')
                """,
                reader => (
                    Table: reader.GetString(0),
                    Column: reader.GetString(1),
                    Type: reader.GetString(2),
                    MaxLength: reader.IsDBNull(3) ? (int?)null : Convert.ToInt32(reader.GetValue(3), CultureInfo.InvariantCulture),
                    Nullable: reader.GetString(4)),
                schema);

            columns.OrderBy(column => column.Table, StringComparer.Ordinal).ThenBy(column => column.Column, StringComparer.Ordinal)
                .Should().Equal(
                    ("ActivityTypes", "WbaToolKey", "character varying", (int?)64, "YES"),
                    ("CurriculumItems", "PermittedToolsJson", "jsonb", (int?)null, "YES"),
                    ("WbaTools", "Description", "character varying", (int?)2000, "YES"),
                    ("WbaTools", "Id", "integer", (int?)null, "NO"),
                    ("WbaTools", "Key", "character varying", (int?)64, "NO"),
                    ("WbaTools", "Name", "character varying", (int?)200, "NO"));

            var keyIndex = await QueryAsync(
                schema,
                """SELECT indexdef FROM pg_indexes WHERE schemaname = $1 AND tablename = 'WbaTools' AND indexname = 'IX_WbaTools_Key'""",
                reader => reader.GetString(0),
                schema);

            keyIndex.Should().ContainSingle().Which.Should().StartWith("CREATE UNIQUE INDEX").And.EndWith("USING btree (\"Key\")");

            (await ScalarAsync<long>(
                    schema,
                    """
                    SELECT COUNT(*)
                    FROM pg_constraint con
                    JOIN pg_class rel ON rel.oid = con.conrelid
                    JOIN pg_namespace ns ON ns.oid = rel.relnamespace
                    LEFT JOIN pg_class ref ON ref.oid = con.confrelid
                    WHERE ns.nspname = $1 AND con.contype = 'f' AND (rel.relname = 'WbaTools' OR ref.relname = 'WbaTools')
                    """,
                    schema))
                .Should().Be(0, "the vocabulary arrives after MigrateAsync, so a foreign key into it would refuse the migration's own stamping");

            (await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "WbaTools" """))
                .Should().Be(0, "PaediatricCatalogueSeeder inserts the vocabulary at the next boot; the migration never does");
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task FreshDatabase_MigratedThenSeeded_CarriesAnnexureAListsTheVocabularyAndTheEightToolKeys_AndASecondBootIsSilent()
    {
        // A new deployment's path, with the seeders run the way startup runs them. On a fresh database the migration
        // stamps nothing, because there are no rows yet, so every value here was written by the seeders on create.
        // The second boot matters as much as the first. Seeders run on every startup, and a boot that rewrote a
        // list, or warned about a list it had just written, would do so for ever on dev and production.
        try
        {
            var schema = await CreateSchemaAsync();

            await using (var db = NewContext(schema))
            {
                await db.Database.MigrateAsync();
            }

            var firstBoot = await BootAsync(schema);

            firstBoot.Writes.Should().NotBeEmpty(
                "guard: the write counter must see the first boot's inserts, or a silent second boot proves nothing");
            firstBoot.CatalogueSeederWarnings.Select(entry => entry.Message).Should().BeEmpty(
                "a freshly seeded catalogue agrees with the file it was seeded from");
            firstBoot.DataSeederWarnings.Select(entry => entry.Message).Should().BeEmpty();

            (await CatalogueListsAsync(schema)).OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => (pair.Key, pair.Value))
                .Should().Equal(ExpectedLists());

            await using (var db = NewContext(schema))
            {
                var items = await db.CurriculumItems
                    .AsNoTracking()
                    .Select(item => new { item.Id, CurriculumName = item.Curriculum.Name, item.Curriculum.Version, item.PermittedToolsJson })
                    .ToListAsync();

                var others = items.Where(item => item.CurriculumName != PaediatricCurriculumName || item.Version != CatalogueVersion).ToList();
                others.Should().NotBeEmpty("guard: DataSeeder's demo curriculum item is the control");
                others.Should().OnlyContain(item => item.PermittedToolsJson == null, "no tool list is declared for the demo curriculum");

                // The create path is the only path that pins (T174), so a fresh database must come out of it pinned.
                var ladderId = await db.EntrustmentScales.AsNoTracking()
                    .Where(scale => scale.Name == "CPSA Paediatric Entrustment Scale v11.1")
                    .Select(scale => scale.Id)
                    .SingleAsync();
                (await db.CurriculumItems.AsNoTracking()
                        .Where(item => item.Curriculum.Name == PaediatricCurriculumName
                                       && item.Curriculum.Version == CatalogueVersion
                                       && item.OwningInstitutionId == null)
                        .Select(item => item.ScaleId)
                        .ToListAsync())
                    .Should().HaveCount(15).And.OnlyContain(scaleId => scaleId == ladderId, "the seeder pins each item it creates to the v11.1 ladder");

                // And DataSeeder's demo item to the O-R Scale, by the same rule.
                var orScaleId = await db.EntrustmentScales.AsNoTracking()
                    .Where(scale => scale.Name == DataSeeder.OrScaleName)
                    .Select(scale => scale.Id)
                    .SingleAsync();
                (await db.CurriculumItems.AsNoTracking()
                        .Where(item => item.Curriculum.Name == "IM Core Curriculum" && item.Epa.Code == "EPA-001")
                        .Select(item => item.ScaleId)
                        .SingleAsync())
                    .Should().Be(orScaleId, "DataSeeder pins the demo item it creates to the O-R Scale");

                var tools = await db.WbaTools.AsNoTracking().Select(tool => new { tool.Key, tool.Name, tool.Description }).ToListAsync();

                // Ordered here, not in SQL: the database collation does not sort "chart_stimulated_recall" ordinally.
                tools.OrderBy(tool => tool.Key, StringComparer.Ordinal).Select(tool => (tool.Key, tool.Name)).Should().Equal(Vocabulary);
                tools.Where(tool => tool.Description is null).Select(tool => tool.Key).Should().BeEquivalentTo(
                    new[] { "chart_stimulated_recall", "learner_feedback", "portfolio_review" },
                    "page 8 of EPA v11.1 defines every instrument but these three");

                var types = await db.ActivityTypes.AsNoTracking().Select(type => new { type.Key, type.WbaToolKey, type.OwnerUserId }).ToListAsync();

                types.Select(type => type.Key).Should().BeEquivalentTo(
                    TypesSeededBeforeT122.Concat(TypesAddedAfterT122.Select(added => added.TypeKey)),
                    "the ten generic seeds and the eleven CPSA seeds");
                types.Should().OnlyContain(type => type.OwnerUserId == SeedOwnerUserId);
                types.Where(type => type.WbaToolKey != null)
                    .OrderBy(type => type.Key, StringComparer.Ordinal)
                    .Select(type => (type.Key, type.WbaToolKey!))
                    .Should().Equal(SeededToolKeys);

                // No foreign key can check either reference (see the first test), so check that the seeders only
                // ever name an instrument the vocabulary holds.
                var vocabulary = tools.Select(tool => tool.Key).ToHashSet(StringComparer.Ordinal);
                items.SelectMany(item => CurriculumItem.ParsePermittedTools(item.PermittedToolsJson))
                    .Should().OnlyContain(key => vocabulary.Contains(key), "every allow-listed key is a vocabulary row");
                types.Where(type => type.WbaToolKey != null)
                    .Should().OnlyContain(type => vocabulary.Contains(type.WbaToolKey!), "every type's instrument is a vocabulary row");
            }

            var secondBoot = await BootAsync(schema);

            secondBoot.Writes.Should().BeEmpty("a second boot finds everything in place and writes nothing");
            secondBoot.CatalogueSeederWarnings.Select(entry => entry.Message).Should().BeEmpty(
                "the stored lists are Postgres's rendering of what the seeder wrote, and still equal the catalogue canonically");
            secondBoot.DataSeederWarnings.Select(entry => entry.Message).Should().BeEmpty();
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task Migration_OnAPopulatedPreT122Database_StampsOnlyTheCatalogueItemsAndSeedOwnedTypes_AndTheNextBootIsSilent()
    {
        // The migration's two UPDATEs touch nothing on a fresh database, because the seeders run after migrations.
        // The only databases they ever act on are existing ones, dev and production, at startup. So they are
        // rehearsed here on a schema stopped at the last pre-T122 migration and filled in its shape by raw SQL.
        //
        // Every decoy pins one scoping clause of the UPDATE. Each is something the seeders would never have written,
        // and stamping it would restrict instruments on an EPA the College's list does not describe. The operator's
        // type pins the owner guard: a type an operator built under a seed key is theirs.
        try
        {
            var fixture = await ArrangePopulatedPreT122DatabaseAsync();

            await using (var db = NewContext(fixture.Schema))
            {
                await db.Database.MigrateAsync();

                (await db.Database.GetAppliedMigrationsAsync()).Should().Contain(T122Migration);
                (await db.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
            }

            var listsAfterMigration = await StoredListsAsync(fixture.Schema);

            listsAfterMigration.Where(pair => pair.Value != null).Select(pair => pair.Key)
                .Should().BeEquivalentTo(fixture.CatalogueItems.Values, "exactly the fifteen national items of the v11.1 catalogue are stamped");
            fixture.CatalogueItems.OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => (pair.Key, listsAfterMigration[pair.Value]))
                .Should().Equal(ExpectedLists());

            foreach (var (decoy, itemId) in fixture.DecoyItems)
            {
                listsAfterMigration[itemId].Should().BeNull("{0} is not an item of the College's v11.1 catalogue", decoy);
            }

            var typesAfterMigration = await StoredTypesAsync(fixture.Schema);

            typesAfterMigration.Where(type => type.WbaToolKey != null)
                .OrderBy(type => type.Key, StringComparer.Ordinal)
                .Select(type => (type.Key, type.WbaToolKey!))
                .Should().Equal(
                    // The migration stamps only what existed before T120: the types seeded after T122 do not exist yet.
                    SeededToolKeys.Where(pair => pair.TypeKey != OperatorCollisionKey &&
                                                 !TypesAddedAfterT122.Any(added => added.TypeKey == pair.TypeKey)),
                    "every seed-owned instrument type is stamped, and nothing else is");
            typesAfterMigration.Single(type => type.Key == OperatorCollisionKey)
                .Should().Be(new TypeRow(OperatorCollisionKey, null, OperatorUserId), "an operator's type under a seed key is not asserted to be anything");
            typesAfterMigration.Single(type => type.Key == "acat").WbaToolKey
                .Should().BeNull("ACAT is not an instrument the College names (D21)");
            typesAfterMigration.Single(type => type.Key == "dops_paed").WbaToolKey
                .Should().BeNull("a type is matched by its exact key, never by resemblance");

            (await ScalarAsync<long>(fixture.Schema, """SELECT COUNT(*) FROM "WbaTools" """))
                .Should().Be(0, "the vocabulary is the seeder's to insert, at the next boot");

            var boot = await BootAsync(fixture.Schema);

            boot.CatalogueSeederWarnings.Select(entry => entry.Message).Should().BeEmpty(
                "the migration's literals equal the catalogue canonically, and neither decoys nor an operator's type are seed-owned");
            boot.DataSeederWarnings.Select(entry => entry.Message).Should().BeEmpty(
                "the generic Mini-CEX, DOPS and CBD were stamped by the migration, as their seed entries say");

            var listsAfterBoot = await StoredListsAsync(fixture.Schema);
            foreach (var (itemId, list) in listsAfterMigration)
            {
                listsAfterBoot[itemId].Should().Be(list, "the seeders never write a list onto an item that already exists (item {0})", itemId);
            }

            listsAfterBoot.Where(pair => !listsAfterMigration.ContainsKey(pair.Key))
                .Should().OnlyContain(pair => pair.Value == null, "the only item a boot adds here is the demo curriculum's");

            // Every type the migration saw is left exactly as the migration stamped it. The only rows the boot adds are
            // the instruments seeded after T122, each keyed on create.
            (await StoredTypesAsync(fixture.Schema)).Should().BeEquivalentTo(
                typesAfterMigration.Concat(TypesAddedAfterT122.Select(added => new TypeRow(added.TypeKey, added.ToolKey, SeedOwnerUserId))),
                "no existing type is re-stamped, and the only types created are the ones seeded after T122");
            (await ScalarAsync<long>(fixture.Schema, """SELECT COUNT(*) FROM "WbaTools" """)).Should().Be(12);

            var secondBoot = await BootAsync(fixture.Schema);

            secondBoot.Writes.Should().BeEmpty("the second boot after the upgrade is a no-op");
            secondBoot.CatalogueSeederWarnings.Select(entry => entry.Message).Should().BeEmpty();
            secondBoot.DataSeederWarnings.Select(entry => entry.Message).Should().BeEmpty();
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task MigrationBackfillSql_RunAgainAfterTheMigration_FillsOnlyWhatIsStillNull_AndNeverOverwritesAValueAlreadySet()
    {
        // Both columns are created by the same migration, so on its one real run the IS NULL guards always hold, and
        // a pre-T122 fixture cannot test them. They matter only if the SQL is ever run again: replayed on a restored
        // dump, or copied into a later migration. So the migration's own statements are re-run here against a
        // post-migration schema. One list and one key are set to something else first, as an administrator would.
        // Each has a control that was cleared to null and must be filled again, so a guard that matched nothing at
        // all could not pass for one that works.
        try
        {
            var schema = await SeededSchemaAsync();
            var (permittedToolsSql, seededToolKeysSql) = T122BackfillSql();

            (await SetListAsync(schema, "PAED-005", """["cbd"]""")).Should().Be(1, "guard: an administrator narrowed PAED-005 to CBD");
            (await SetListAsync(schema, "PAED-010", null)).Should().Be(1, "guard: the control list is cleared");
            (await SetToolKeyAsync(schema, "mini_cex_cpsa", "direct_observation")).Should().Be(1, "guard: an administrator re-filed the type");
            (await SetToolKeyAsync(schema, "dops_cpsa", null)).Should().Be(1, "guard: the control key is cleared");
            (await ExecuteAsync(
                    schema,
                    """UPDATE "ActivityTypes" SET "OwnerUserId" = $1, "WbaToolKey" = NULL WHERE "Key" = 'cbd_cpsa'""",
                    OperatorUserId))
                .Should().Be(1, "guard: an operator took cbd_cpsa over and cleared its instrument");

            (await ExecuteAsync(schema, permittedToolsSql)).Should().Be(1, "only PAED-010's list is null");
            (await ExecuteAsync(schema, seededToolKeysSql)).Should().Be(1, "only dops_cpsa is seed-owned and unkeyed");

            var lists = await CatalogueListsAsync(schema);
            lists["PAED-005"].Should().Be("cbd", "a list that is already set is never overwritten");
            lists["PAED-010"].Should().Be("direct_observation,msf", "control: a null list is still stamped, so the guard is what spared PAED-005");
            lists.Where(pair => pair.Key is not ("PAED-005" or "PAED-010"))
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => (pair.Key, pair.Value))
                .Should().Equal(ExpectedLists().Where(row => row.Code is not ("PAED-005" or "PAED-010")));

            var types = (await StoredTypesAsync(schema)).ToDictionary(type => type.Key, StringComparer.Ordinal);
            types["mini_cex_cpsa"].WbaToolKey.Should().Be("direct_observation", "a key that is already set is never overwritten");
            types["dops_cpsa"].WbaToolKey.Should().Be("dops", "control: a null seed-owned key is still stamped");
            types["cbd_cpsa"].WbaToolKey.Should().BeNull("the owner guard holds on a re-run too");
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task PermittedToolsJson_WrittenThroughEf_ComesBackAsPostgresRendersIt_AndParsesToTheSameKeys()
    {
        // The jsonb trap, on this column (CLAUDE.md). What EF writes is not what EF reads back, so a raw comparison
        // anywhere (a seeder warning, an "unchanged" check in an editor, a test) would see a difference on every
        // row. ParsePermittedTools must see none.
        try
        {
            var schema = await SeededSchemaAsync();

            var written = CurriculumItem.NormalizePermittedToolsJson(["msf", " DOPS", "cbd", "msf"]);
            written.Should().Be("""["cbd","dops","msf"]""", "guard: this is the exact text EF sends");

            int itemId, clearedId;
            await using (var db = NewContext(schema))
            {
                var item = await CatalogueItems(db, "PAED-001").SingleAsync();
                var cleared = await CatalogueItems(db, "PAED-002").SingleAsync();

                item.PermittedToolsJson = written;
                cleared.PermittedToolsJson = CurriculumItem.NormalizePermittedToolsJson([" ", ""]);
                await db.SaveChangesAsync();

                itemId = item.Id;
                clearedId = cleared.Id;
            }

            await using (var db = NewContext(schema))
            {
                var readBack = await db.CurriculumItems.AsNoTracking()
                    .Where(item => item.Id == itemId)
                    .Select(item => item.PermittedToolsJson)
                    .SingleAsync();

                readBack.Should().NotBeNull();
                readBack.Should().NotBe(written, "Postgres keeps a parsed tree and renders its own text, so the bytes never survive");
                CurriculumItem.ParsePermittedTools(readBack).Should().Equal(new[] { "cbd", "dops", "msf" },"canonical to canonical, nothing is lost");
            }

            (await ScalarAsync<bool>(schema, """SELECT "PermittedToolsJson" IS NULL FROM "CurriculumItems" WHERE "Id" = $1""", clearedId))
                .Should().BeTrue("an emptied list is stored as SQL NULL, which means unrestricted; never as [] or as a jsonb null");

            // EF's snapshot of a loaded row is Postgres's rendering, and it compares equal to itself, so a tracked item
            // saved unchanged writes nothing.
            var writes = new WriteCounter();
            await using (var db = NewContext(schema, writes))
            {
                _ = await db.CurriculumItems.SingleAsync(item => item.Id == itemId);
                await db.SaveChangesAsync();
            }

            writes.Writes.Should().BeEmpty();
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task CatalogueSeeder_OnPostgres_WarnsOnlyWhereAListOrKeyReallyDiffers_AndWritesNothing()
    {
        // The seeder compares canonical to canonical, and on Postgres that is tested twice over. The stored text is
        // always Postgres's rendering, and direct SQL can store any spelling. A list that differs only in case,
        // padding, order or duplicates IS the catalogue's list. It must stay silent, or every boot warns and a real
        // warning drowns in the noise. A real difference is announced once and left alone: an administrator may have
        // made it on purpose.
        try
        {
            var schema = await SeededSchemaAsync();

            (await SetListAsync(schema, "PAED-010", """[" MSF", "Direct_Observation ", "msf"]""")).Should().Be(1);
            (await SetListAsync(schema, "PAED-005", """["msf", "cbd"]""")).Should().Be(1);
            (await SetToolKeyAsync(schema, "mini_cex_cpsa", " MINI_CEX ")).Should().Be(1);
            (await SetToolKeyAsync(schema, "dops_cpsa", "cbd")).Should().Be(1);

            var boot = await BootAsync(schema);

            boot.Writes.Should().BeEmpty("the seeders warn and never write");
            boot.DataSeederWarnings.Select(entry => entry.Message).Should().BeEmpty();
            boot.CatalogueSeederWarnings.Should().HaveCount(2, "one real list difference and one real key difference; the respellings are silent");

            var listWarning = boot.CatalogueSeederWarnings.Should().ContainSingle(entry => entry.Values.ContainsKey("EpaCode")).Subject;
            listWarning.Values["EpaCode"].Should().Be("PAED-005");
            listWarning.Values["StoredTools"].Should().Be("cbd, msf");
            listWarning.Values["ExpectedTools"].Should().Be("cbd, dops, msf");

            var keyWarning = boot.CatalogueSeederWarnings.Should().ContainSingle(entry => entry.Values.ContainsKey("StoredWbaToolKey")).Subject;
            keyWarning.Values["Key"].Should().Be("dops_cpsa");
            keyWarning.Values["StoredWbaToolKey"].Should().Be("cbd");
            keyWarning.Values["ExpectedWbaToolKey"].Should().Be("dops");

            var lists = await CatalogueListsAsync(schema);
            lists["PAED-005"].Should().Be("cbd,msf", "the warning leaves the item alone");
            lists["PAED-010"].Should().Be("direct_observation,msf", "the respelling reads as the catalogue's list");

            var types = (await StoredTypesAsync(schema)).ToDictionary(type => type.Key, StringComparer.Ordinal);
            types["dops_cpsa"].WbaToolKey.Should().Be("cbd", "the warning leaves the type alone");
            types["mini_cex_cpsa"].WbaToolKey.Should().Be(" MINI_CEX ", "a respelling is not a difference, so nothing rewrites it either");
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    /// <summary>
    /// A database as T122 found dev: stopped at T130 and filled by raw SQL in that shape. The demo tree
    /// <see cref="DataSeeder" /> expects is there, the CPSA discipline with its v11.1 curriculum at the Annexure B
    /// targets and pinned to the v11.1 ladder, every seeded activity type with its real published payloads, and the
    /// decoys.
    /// </summary>
    private async Task<PreT122Fixture> ArrangePopulatedPreT122DatabaseAsync()
    {
        var schema = await CreateSchemaAsync();

        await using (var db = NewContext(schema))
        {
            await db.GetService<IMigrator>().MigrateAsync(LastMigrationBeforeT122);

            (await db.Database.GetAppliedMigrationsAsync()).Last().Should().Be(LastMigrationBeforeT122);
            (await db.Database.GetPendingMigrationsAsync()).First().Should().Be(T122Migration);
        }

        (await ScalarAsync<long>(
                schema,
                """
                SELECT COUNT(*) FROM information_schema.columns
                WHERE table_schema = $1
                  AND ((table_name = 'CurriculumItems' AND column_name = 'PermittedToolsJson')
                    OR (table_name = 'ActivityTypes' AND column_name = 'WbaToolKey'))
                """,
                schema))
            .Should().Be(0, "guard: the schema must really be in the pre-T122 shape");

        var catalogueItems = new Dictionary<string, int>(StringComparer.Ordinal);
        var decoys = new Dictionary<string, int>(StringComparer.Ordinal);

        await using (var connection = await OpenAsync(schema))
        {
            var demoCollegeId = await InsertAsync(connection,
                """INSERT INTO "Colleges" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('Demo College', 'DEMO-C', TIMESTAMPTZ '2026-06-19 00:00:00+00', TRUE) RETURNING "Id" """);
            var demoSpecialityId = await InsertAsync(connection,
                """INSERT INTO "Specialities" ("CollegeId", "Name", "IsActive") VALUES ($1, 'General Medicine', TRUE) RETURNING "Id" """,
                demoCollegeId);
            await InsertAsync(connection,
                """INSERT INTO "SubSpecialities" ("SpecialityId", "Name", "IsActive") VALUES ($1, 'General Internal Medicine', TRUE) RETURNING "Id" """,
                demoSpecialityId);
            await InsertAsync(connection,
                """INSERT INTO "Institutions" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('Demo Institution', 'DEMO', TIMESTAMPTZ '2026-06-19 00:00:00+00', TRUE) RETURNING "Id" """);

            var hospitalId = await InsertAsync(connection,
                """INSERT INTO "Institutions" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('Legacy Hospital', 'LEGACY', TIMESTAMPTZ '2026-06-19 00:00:00+00', TRUE) RETURNING "Id" """);
            var cpsaId = await InsertAsync(connection,
                """INSERT INTO "Colleges" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('College of Paediatricians of South Africa', 'CPSA', TIMESTAMPTZ '2026-06-19 00:00:00+00', TRUE) RETURNING "Id" """);
            var paediatricsId = await InsertAsync(connection,
                """INSERT INTO "Specialities" ("CollegeId", "Name", "IsActive") VALUES ($1, 'Paediatrics', TRUE) RETURNING "Id" """,
                cpsaId);
            var paediatricsSubId = await InsertAsync(connection,
                """INSERT INTO "SubSpecialities" ("SpecialityId", "Name", "IsActive") VALUES ($1, 'Paediatrics', TRUE) RETURNING "Id" """,
                paediatricsId);
            var neonatologySubId = await InsertAsync(connection,
                """INSERT INTO "SubSpecialities" ("SpecialityId", "Name", "IsActive") VALUES ($1, 'Neonatology', TRUE) RETURNING "Id" """,
                paediatricsId);

            var epaIds = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var (code, _) in AnnexureA)
            {
                epaIds[code] = await InsertEpaAsync(connection, paediatricsSubId, code, owningInstitutionId: null);
            }

            // National codes are unique only among national EPAs of one discipline, so all three of these can exist.
            var localPaed005 = await InsertEpaAsync(connection, paediatricsSubId, "PAED-005", owningInstitutionId: hospitalId);
            var localPaed006 = await InsertEpaAsync(connection, paediatricsSubId, "PAED-006", owningInstitutionId: hospitalId);
            var paed016 = await InsertEpaAsync(connection, paediatricsSubId, "PAED-016", owningInstitutionId: null);
            var neonatalPaed001 = await InsertEpaAsync(connection, neonatologySubId, "PAED-001", owningInstitutionId: null);

            var catalogueCurriculum = await InsertCurriculumAsync(connection, paediatricsSubId, PaediatricCurriculumName, CatalogueVersion);
            var nextVersion = await InsertCurriculumAsync(connection, paediatricsSubId, PaediatricCurriculumName, "11.2");
            var researchTrack = await InsertCurriculumAsync(connection, paediatricsSubId, "Paediatric Research Track", CatalogueVersion);
            var neonatalCurriculum = await InsertCurriculumAsync(connection, neonatologySubId, PaediatricCurriculumName, CatalogueVersion);

            foreach (var (code, period, target) in AnnexureB)
            {
                catalogueItems[code] = await InsertCurriculumItemAsync(connection, catalogueCurriculum, epaIds[code], period, target, owningInstitutionId: null);
            }

            // The v11.1 ladder, with the fifteen items pinned to it, as the seeder created them (T109). Since T174 no boot
            // pins an existing item, so without this the fixture would be a database whose items an administrator had
            // unpinned, and the seeder would rightly announce all fifteen. The decoys stay unpinned: none is the
            // catalogue's item, so the pin check does not reach them.
            var ladderId = await InsertAsync(connection,
                """INSERT INTO "EntrustmentScales" ("Name") VALUES ('CPSA Paediatric Entrustment Scale v11.1') RETURNING "Id" """);
            string[] rungs = ["1", "2", "3a", "3b", "4", "5"];
            for (var order = 1; order <= rungs.Length; order++)
            {
                await ExecuteAsync(connection,
                    """INSERT INTO "EntrustmentLevels" ("ScaleId", "Order", "Label") VALUES ($1, $2, $3)""",
                    ladderId, order, rungs[order - 1]);
            }

            (await ExecuteAsync(connection,
                    """UPDATE "CurriculumItems" SET "ScaleId" = $1 WHERE "Id" = ANY($2)""",
                    ladderId, catalogueItems.Values.ToArray()))
                .Should().Be(15, "guard: exactly the fifteen catalogue items are pinned");

            // Pins the two "OwningInstitutionId" IS NULL clauses together: an institution's local EPA may reuse a
            // national code. The item-side clause cannot be pinned alone here, because every catalogue EPA already
            // has its national item in this curriculum and (CurriculumId, EpaId) is unique.
            decoys["an institution-local item on a local EPA reusing PAED-005"] =
                await InsertCurriculumItemAsync(connection, catalogueCurriculum, localPaed005, QuotaPeriod.Semester, 3, owningInstitutionId: hospitalId);
            // Pins e."OwningInstitutionId" IS NULL on its own: a national item on a local EPA. T091 forbids it, so only
            // direct SQL could produce it, but the code alone must not carry the College's list onto a local EPA.
            decoys["a national item on a local EPA reusing PAED-006"] =
                await InsertCurriculumItemAsync(connection, catalogueCurriculum, localPaed006, QuotaPeriod.Semester, 2, owningInstitutionId: null);
            // Pins e."Code" = v.code: a CollegeAdmin's own national item on an EPA the catalogue does not declare.
            decoys["a national item on PAED-016, which the catalogue does not declare"] =
                await InsertCurriculumItemAsync(connection, catalogueCurriculum, paed016, QuotaPeriod.AcademicYear, 1, owningInstitutionId: null);
            // Pins e."SubSpecialityId" = ss."Id": the right curriculum, and a catalogue code on another discipline's EPA.
            decoys["a national item in the catalogue curriculum on Neonatology's PAED-001"] =
                await InsertCurriculumItemAsync(connection, catalogueCurriculum, neonatalPaed001, QuotaPeriod.Semester, 3, owningInstitutionId: null);
            // Pins c."Version" = '11.1': a clone at another version is its author's to restrict.
            decoys["PAED-001 in version 11.2 of the curriculum"] =
                await InsertCurriculumItemAsync(connection, nextVersion, epaIds["PAED-001"], QuotaPeriod.Semester, 3, owningInstitutionId: null);
            decoys["PAED-005 in version 11.2 of the curriculum"] =
                await InsertCurriculumItemAsync(connection, nextVersion, epaIds["PAED-005"], QuotaPeriod.Semester, 3, owningInstitutionId: null);
            // Pins c."Name": another curriculum of the same discipline and version.
            decoys["PAED-001 in the Paediatric Research Track"] =
                await InsertCurriculumItemAsync(connection, researchTrack, epaIds["PAED-001"], QuotaPeriod.Semester, 3, owningInstitutionId: null);
            // Pins ss."Name" = 'Paediatrics': another discipline's curriculum of the same name and version.
            decoys["PAED-001 in Neonatology's own Paediatric EPA Curriculum 11.1"] =
                await InsertCurriculumItemAsync(connection, neonatalCurriculum, neonatalPaed001, QuotaPeriod.Semester, 3, owningInstitutionId: null);

            foreach (var key in TypesSeededBeforeT122)
            {
                var entry = ActivityTypeSeedCatalogue.Entries.Single(candidate => candidate.Key == key);
                var payload = await ActivityTypeSeedCatalogue.ReadCanonicalAsync(entry, CancellationToken.None);

                if (key == OperatorCollisionKey)
                {
                    // Built in the builder before the seeder got to it, so the seeder skipped the key for good.
                    await InsertActivityTypeAsync(connection, key, "Mini-CEX (Legacy Hospital)", ActivityScope.Institution, hospitalId, OperatorUserId, payload);
                    continue;
                }

                var scopeId = entry.Source == ActivityTypeSeedSource.Generic ? demoSpecialityId : paediatricsId;
                await InsertActivityTypeAsync(connection, key, entry.Name, entry.Scope, scopeId, SeedOwnerUserId, payload);
            }

            // A scenario leftover of the kind that once swallowed two CPSA seeds.
            var dopsPayload = await ActivityTypeSeedCatalogue.ReadCanonicalAsync(
                ActivityTypeSeedCatalogue.Entries.Single(candidate => candidate.Key == "dops_cpsa"), CancellationToken.None);
            await InsertActivityTypeAsync(connection, "dops_paed", "DOPS (Paed)", ActivityScope.Institution, hospitalId, OperatorUserId, dopsPayload);
        }

        return new PreT122Fixture(schema, catalogueItems, decoys);
    }

    /// <summary>
    /// One startup's seeding, the way <c>Program.cs</c> runs it: <see cref="DataSeeder" />, then the paediatric
    /// catalogue, each in its own context. Every write statement and every warning is captured.
    /// </summary>
    private async Task<BootLog> BootAsync(string schema)
    {
        var writes = new WriteCounter();
        var dataSeederLog = new CapturingLogger<DataSeeder>();
        var catalogueSeederLog = new CapturingLogger<PaediatricCatalogueSeeder>();

        await using (var db = NewContext(schema, writes))
        {
            await new DataSeeder(db, dataSeederLog).SeedAsync();
        }

        await using (var db = NewContext(schema, writes))
        {
            await new PaediatricCatalogueSeeder(db, catalogueSeederLog).SeedAsync();
        }

        return new BootLog(writes.Writes, dataSeederLog.Warnings.ToList(), catalogueSeederLog.Warnings.ToList());
    }

    /// <summary>
    /// The migration's two hand-written statements, exactly as its <c>Up</c> hands them to <c>Sql()</c>. They are the
    /// output of <c>BuildPermittedToolsBackfill()</c> and <c>BuildSeededToolKeysBackfill()</c>, read through the
    /// public <see cref="Migration.UpOperations" /> because those builders are internal to Infrastructure.
    /// </summary>
    private static (string PermittedTools, string SeededToolKeys) T122BackfillSql()
    {
        var statements = new T122_WbaToolAllowLists().UpOperations.OfType<SqlOperation>().Select(operation => operation.Sql).ToList();
        statements.Should().HaveCount(2, "guard: T122 carries exactly two hand-written statements");

        return (
            statements.Single(sql => sql.Contains("UPDATE \"CurriculumItems\"", StringComparison.Ordinal)),
            statements.Single(sql => sql.Contains("UPDATE \"ActivityTypes\"", StringComparison.Ordinal)));
    }

    private static IEnumerable<(string Code, string? Tools)> ExpectedLists()
        => AnnexureA.Select(row => (row.Code, (string?)string.Join(",", row.Tools)));

    /// <summary>A stored list as its canonical keys joined by commas, or null for no list at all.</summary>
    private static string? Canonical(string? json)
        => json is null ? null : string.Join(",", CurriculumItem.ParsePermittedTools(json));

    /// <summary>The v11.1 catalogue's national item for a national EPA code, tracked.</summary>
    private static IQueryable<CurriculumItem> CatalogueItems(ApplicationDbContext db, string code)
        => db.CurriculumItems.Where(item =>
            item.Epa.Code == code
            && item.Epa.OwningInstitutionId == null
            && item.OwningInstitutionId == null
            && item.Curriculum.Name == PaediatricCurriculumName
            && item.Curriculum.Version == CatalogueVersion);

    /// <summary>The v11.1 catalogue's national items, by EPA code, as canonical lists. For a schema with no decoys.</summary>
    private async Task<Dictionary<string, string?>> CatalogueListsAsync(string schema)
    {
        await using var db = NewContext(schema);
        var items = await db.CurriculumItems
            .AsNoTracking()
            .Where(item => item.Curriculum.Name == PaediatricCurriculumName
                           && item.Curriculum.Version == CatalogueVersion
                           && item.OwningInstitutionId == null
                           && item.Epa.OwningInstitutionId == null)
            .Select(item => new { item.Epa.Code, item.PermittedToolsJson })
            .ToListAsync();

        items.Should().HaveCount(15, "guard: the catalogue's fifteen items, each once");
        return items.ToDictionary(item => item.Code, item => Canonical(item.PermittedToolsJson), StringComparer.Ordinal);
    }

    /// <summary>Every curriculum item's list, canonical, by item id.</summary>
    private async Task<Dictionary<int, string?>> StoredListsAsync(string schema)
    {
        await using var db = NewContext(schema);
        var items = await db.CurriculumItems.AsNoTracking().Select(item => new { item.Id, item.PermittedToolsJson }).ToListAsync();
        return items.ToDictionary(item => item.Id, item => Canonical(item.PermittedToolsJson));
    }

    private async Task<List<TypeRow>> StoredTypesAsync(string schema)
    {
        await using var db = NewContext(schema);
        var types = await db.ActivityTypes.AsNoTracking().Select(type => new { type.Key, type.WbaToolKey, type.OwnerUserId }).ToListAsync();
        return types.Select(type => new TypeRow(type.Key, type.WbaToolKey, type.OwnerUserId)).ToList();
    }

    /// <summary>Sets one catalogue item's list by direct SQL, which is the only way to store an unnormalised value.</summary>
    private Task<int> SetListAsync(string schema, string code, string? json)
    {
        const string Scope =
            """
            FROM "Epas" AS e, "Curricula" AS c
            WHERE ci."EpaId" = e."Id"
              AND ci."CurriculumId" = c."Id"
              AND c."Name" = 'Paediatric EPA Curriculum'
              AND c."Version" = '11.1'
              AND ci."OwningInstitutionId" IS NULL
              AND e."OwningInstitutionId" IS NULL
              AND e."Code" = $1
            """;

        return json is null
            ? ExecuteAsync(schema, $"""UPDATE "CurriculumItems" AS ci SET "PermittedToolsJson" = NULL {Scope}""", code)
            : ExecuteAsync(schema, $"""UPDATE "CurriculumItems" AS ci SET "PermittedToolsJson" = $2::jsonb {Scope}""", code, json);
    }

    private Task<int> SetToolKeyAsync(string schema, string typeKey, string? toolKey)
        => toolKey is null
            ? ExecuteAsync(schema, """UPDATE "ActivityTypes" SET "WbaToolKey" = NULL WHERE "Key" = $1""", typeKey)
            : ExecuteAsync(schema, """UPDATE "ActivityTypes" SET "WbaToolKey" = $2 WHERE "Key" = $1""", typeKey, toolKey);

    private static async Task InsertActivityTypeAsync(
        NpgsqlConnection connection,
        string key,
        string name,
        ActivityScope scope,
        int scopeId,
        string ownerUserId,
        ActivityTypeSeedPayload payload)
    {
        var typeId = await InsertAsync(connection,
            """
            INSERT INTO "ActivityTypes"
                ("Key", "Name", "Scope", "ScopeId", "Version", "SchemaJson", "WorkflowJson", "CreditRulesJson", "DisplayFieldsJson",
                 "OwnerUserId", "CreatedOn", "IsActive")
            VALUES ($1, $2, $3, $4, 1, $5::jsonb, $6::jsonb, $7::jsonb, $8::jsonb, $9, TIMESTAMPTZ '2026-06-19 00:00:00+00', TRUE)
            RETURNING "Id"
            """,
            key, name, (int)scope, scopeId, payload.SchemaJson, payload.WorkflowJson, payload.CreditRulesJson, payload.DisplayFieldsJson, ownerUserId);

        await ExecuteAsync(connection,
            """
            INSERT INTO "ActivityTypeVersions"
                ("ActivityTypeId", "Version", "SchemaJson", "WorkflowJson", "CreditRulesJson", "DisplayFieldsJson", "PublishedByUserId", "PublishedOn")
            VALUES ($1, 1, $2::jsonb, $3::jsonb, $4::jsonb, $5::jsonb, $6, TIMESTAMPTZ '2026-06-19 00:00:00+00')
            """,
            typeId, payload.SchemaJson, payload.WorkflowJson, payload.CreditRulesJson, payload.DisplayFieldsJson, ownerUserId);
    }

    private static Task<int> InsertEpaAsync(NpgsqlConnection connection, int subSpecialityId, string code, int? owningInstitutionId)
        => owningInstitutionId is null
            ? InsertAsync(connection,
                """INSERT INTO "Epas" ("SubSpecialityId", "OwningInstitutionId", "Code", "Title", "Category", "CreatedOn", "IsActive") VALUES ($1, NULL, $2, $2, 0, TIMESTAMPTZ '2026-01-01 00:00:00+00', TRUE) RETURNING "Id" """,
                subSpecialityId, code)
            : InsertAsync(connection,
                """INSERT INTO "Epas" ("SubSpecialityId", "OwningInstitutionId", "Code", "Title", "Category", "CreatedOn", "IsActive") VALUES ($1, $3, $2, $2, 0, TIMESTAMPTZ '2026-01-01 00:00:00+00', TRUE) RETURNING "Id" """,
                subSpecialityId, code, owningInstitutionId.Value);

    private static Task<int> InsertCurriculumAsync(NpgsqlConnection connection, int subSpecialityId, string name, string version)
        => InsertAsync(connection,
            """INSERT INTO "Curricula" ("SubSpecialityId", "Name", "Version", "EffectiveFrom", "IsActive") VALUES ($1, $2, $3, DATE '2026-01-01', TRUE) RETURNING "Id" """,
            subSpecialityId, name, version);

    private static Task<int> InsertCurriculumItemAsync(
        NpgsqlConnection connection,
        int curriculumId,
        int epaId,
        QuotaPeriod quotaPeriod,
        int requiredCount,
        int? owningInstitutionId)
        => owningInstitutionId is null
            ? InsertAsync(connection,
                """INSERT INTO "CurriculumItems" ("CurriculumId", "EpaId", "OwningInstitutionId", "RequiredCount", "QuotaPeriod", "MinimumLevelOrder", "WindowMonths") VALUES ($1, $2, NULL, $3, $4, 6, 12) RETURNING "Id" """,
                curriculumId, epaId, requiredCount, (int)quotaPeriod)
            : InsertAsync(connection,
                """INSERT INTO "CurriculumItems" ("CurriculumId", "EpaId", "OwningInstitutionId", "RequiredCount", "QuotaPeriod", "MinimumLevelOrder", "WindowMonths") VALUES ($1, $2, $5, $3, $4, 6, 12) RETURNING "Id" """,
                curriculumId, epaId, requiredCount, (int)quotaPeriod, owningInstitutionId.Value);

    private static async Task<int> InsertAsync(NpgsqlConnection connection, string sql, params object[] values)
    {
        await using var command = Command(connection, sql, values);
        return Convert.ToInt32(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    private static async Task<int> ExecuteAsync(NpgsqlConnection connection, string sql, params object[] values)
    {
        await using var command = Command(connection, sql, values);
        return await command.ExecuteNonQueryAsync();
    }

    private async Task<int> ExecuteAsync(string schema, string sql, params object[] values)
    {
        await using var connection = await OpenAsync(schema);
        return await ExecuteAsync(connection, sql, values);
    }

    private async Task<T> ScalarAsync<T>(string schema, string sql, params object[] values)
    {
        await using var connection = await OpenAsync(schema);
        await using var command = Command(connection, sql, values);
        var value = await command.ExecuteScalarAsync();
        return (T)Convert.ChangeType(value!, typeof(T), CultureInfo.InvariantCulture);
    }

    private async Task<List<T>> QueryAsync<T>(string schema, string sql, Func<NpgsqlDataReader, T> map, params object[] values)
    {
        await using var connection = await OpenAsync(schema);
        await using var command = Command(connection, sql, values);
        await using var reader = await command.ExecuteReaderAsync();

        var results = new List<T>();
        while (await reader.ReadAsync())
        {
            results.Add(map(reader));
        }

        return results;
    }

    /// <summary>Positional parameters ($1, $2, …), so no value is ever spliced into SQL text.</summary>
    private static NpgsqlCommand Command(NpgsqlConnection connection, string sql, object[] values)
    {
        var command = new NpgsqlCommand(sql, connection);
        foreach (var value in values)
        {
            command.Parameters.Add(new NpgsqlParameter { Value = value });
        }

        return command;
    }

    private async Task<NpgsqlConnection> OpenAsync(string schema)
    {
        var connection = new NpgsqlConnection(SchemaConnectionString(schema));
        await connection.OpenAsync();
        return connection;
    }

    /// <summary>A new, empty schema, registered for dropping before it exists so that no failure can leak it.</summary>
    private async Task<string> CreateSchemaAsync()
    {
        var schema = $"it_{Guid.NewGuid():N}";
        _schemas.Add(schema);

        await using var connection = new NpgsqlConnection(_baseConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE SCHEMA \"{schema}\"";
        await command.ExecuteNonQueryAsync();

        return schema;
    }

    /// <summary>Drops every schema this test created. Called from each test's finally and again from DisposeAsync.</summary>
    private async Task DropSchemasAsync()
    {
        if (_schemas.Count == 0)
        {
            return;
        }

        await using var connection = new NpgsqlConnection(_baseConnectionString);
        await connection.OpenAsync();

        foreach (var schema in _schemas.ToList())
        {
            // Belt and braces: this class only ever drops a schema it named itself.
            if (schema.StartsWith("it_", StringComparison.Ordinal))
            {
                await using var drop = connection.CreateCommand();
                drop.CommandText = $"DROP SCHEMA IF EXISTS \"{schema}\" CASCADE";
                await drop.ExecuteNonQueryAsync();
            }

            _schemas.Remove(schema);
        }
    }

    /// <summary>A fresh schema, migrated and seeded the way startup does it: DataSeeder, then the paediatric catalogue.</summary>
    private async Task<string> SeededSchemaAsync()
    {
        var schema = await CreateSchemaAsync();

        await using (var db = NewContext(schema))
        {
            await db.Database.MigrateAsync();
        }

        await BootAsync(schema);
        return schema;
    }

    private ApplicationDbContext NewContext(string schema, params IInterceptor[] interceptors)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(SchemaConnectionString(schema));
        if (interceptors.Length > 0)
        {
            options.AddInterceptors(interceptors);
        }

        return new ApplicationDbContext(options.Options);
    }

    /// <summary>
    /// The schema and nothing else on the search path, so an unqualified name can only ever resolve inside it.
    /// </summary>
    private string SchemaConnectionString(string schema)
        => new NpgsqlConnectionStringBuilder(_baseConnectionString)
        {
            SearchPath = schema,
            Pooling = false
        }.ConnectionString;

    /// <summary>The same resolution order as <c>MsfRespondEndpointFlowTests</c>.</summary>
    private static string ResolveBaseConnectionString()
    {
        var environmentConnectionString = Environment.GetEnvironmentVariable("WOMBAT_TEST_CONNECTION");
        if (!string.IsNullOrWhiteSpace(environmentConnectionString))
        {
            return environmentConnectionString;
        }

        var secretsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Microsoft",
            "UserSecrets",
            WombatWebUserSecretsId,
            "secrets.json");

        if (File.Exists(secretsPath))
        {
            using var document = JsonDocument.Parse(File.ReadAllText(secretsPath));
            if (document.RootElement.TryGetProperty("ConnectionStrings:DefaultConnection", out var property)
                && property.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(property.GetString()))
            {
                return property.GetString()!;
            }
        }

        return "Host=localhost;Port=5432;Database=wombat;Username=postgres;Password=postgres";
    }

    private sealed record PreT122Fixture(
        string Schema,
        IReadOnlyDictionary<string, int> CatalogueItems,
        IReadOnlyDictionary<string, int> DecoyItems);

    private sealed record TypeRow(string Key, string? WbaToolKey, string OwnerUserId);

    private sealed record BootLog(
        IReadOnlyList<string> Writes,
        IReadOnlyList<CapturedLogEntry> DataSeederWarnings,
        IReadOnlyList<CapturedLogEntry> CatalogueSeederWarnings);

    /// <summary>
    /// Records every INSERT, UPDATE, DELETE or MERGE statement EF sends, so "a boot writes nothing" is asserted on
    /// what reached the server rather than inferred from row counts.
    /// </summary>
    /// <remarks>
    /// Multiline, because EF batches several statements into one command. A write counter that missed writes would
    /// make every "writes nothing" assertion pass vacuously, so the fresh-database test also asserts that the first
    /// boot IS seen.
    /// </remarks>
    private sealed class WriteCounter : DbCommandInterceptor
    {
        private static readonly Regex WriteStatement = new(
            @"^\s*(INSERT|UPDATE|DELETE|MERGE)\b",
            RegexOptions.Multiline | RegexOptions.CultureInvariant);

        public List<string> Writes { get; } = [];

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            Inspect(command);
            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            Inspect(command);
            return ValueTask.FromResult(result);
        }

        public override InterceptionResult<int> NonQueryExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        {
            Inspect(command);
            return result;
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Inspect(command);
            return ValueTask.FromResult(result);
        }

        public override InterceptionResult<object> ScalarExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
        {
            Inspect(command);
            return result;
        }

        public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
        {
            Inspect(command);
            return ValueTask.FromResult(result);
        }

        private void Inspect(DbCommand command)
        {
            if (WriteStatement.IsMatch(command.CommandText))
            {
                Writes.Add(command.CommandText);
            }
        }
    }

    /// <summary>
    /// Keeps every log entry with its structured values, so a test asserts on <c>{EpaCode}</c> itself rather than on
    /// a substring of the rendered message. A copy of Wombat.Infrastructure.Tests' <c>CapturingLogger</c>, which is
    /// internal to that assembly. For the seeders' drift warnings the log line IS the behaviour, and a
    /// <c>NullLogger</c> would let it vanish without a failing test.
    /// </summary>
    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<CapturedLogEntry> Entries { get; } = [];

        public IEnumerable<CapturedLogEntry> Warnings => Entries.Where(entry => entry.Level == LogLevel.Warning);

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var values = state is IReadOnlyList<KeyValuePair<string, object?>> pairs
                ? pairs.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal)
                : new Dictionary<string, object?>(StringComparer.Ordinal);

            Entries.Add(new CapturedLogEntry(logLevel, formatter(state, exception), values));
        }
    }

    private sealed record CapturedLogEntry(LogLevel Level, string Message, IReadOnlyDictionary<string, object?> Values);
}
