using System.Globalization;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Logging;
using Npgsql;
using Wombat.Application.Common.Persistence;
using Wombat.Domain.Curricula;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Persistence.Migrations;

namespace Wombat.Integration.Tests.Curricula;

/// <summary>
/// T131 slice 2 on a real PostgreSQL server: the migration that gives each curriculum item Annexure B's decision cadence,
/// its deciding body and its opportunistic flag, and stamps the catalogue's fifteen items on a database that already holds
/// them; and the seeder that writes the same values on create.
/// </summary>
/// <remarks>
/// <para>
/// The unit suites run on EF InMemory, which never runs a migration and enforces no foreign key or check. The migration's
/// two hand-written statements, the restricting foreign key into <c>DecisionBodies</c> and the check on the cadence are
/// exercised only here. The stamp touches nothing on a fresh database, because the seeders run after migrations, so it is
/// rehearsed on a schema stopped at the last migration before it and filled in its shape by raw SQL.
/// </para>
/// <para>
/// The oracle is typed from Annexure B, not read from the catalogue file or from the migration's frozen copy. The schema
/// helpers follow <c>WbaToolAllowListPostgresTests</c>: each test works in a schema of its own, registered before it is
/// created and dropped in a <c>finally</c> and again from <see cref="DisposeAsync" />.
/// </para>
/// </remarks>
public sealed class DecisionCadenceMigrationPostgresTests : IAsyncLifetime
{
    private const string LastMigrationBeforeThis = "20260924173137_T131_EvidenceProvenance";
    private const string CurriculumName = "Paediatric EPA Curriculum";
    private const string CatalogueVersion = "11.1";
    private const string NeonatalName = "Neonatal team Clinical Competency Committee";

    /// <summary>Annexure B's decision cells: (code, cadence, body, opportunistic).</summary>
    private static readonly (string Code, QuotaPeriod? Cadence, string? Body, bool Opportunistic)[] AnnexureB =
    [
        ("PAED-001", QuotaPeriod.Semester, null, false),
        ("PAED-002", QuotaPeriod.Semester, null, false),
        ("PAED-003", QuotaPeriod.AcademicYear, null, false),
        ("PAED-004", QuotaPeriod.Semester, "neonatal", false),
        ("PAED-005", QuotaPeriod.Semester, "neonatal", false),
        ("PAED-006", QuotaPeriod.AcademicYear, null, false),
        ("PAED-007", QuotaPeriod.AcademicYear, null, false),
        ("PAED-008", QuotaPeriod.AcademicYear, null, true),
        ("PAED-009", QuotaPeriod.AcademicYear, null, true),
        ("PAED-010", QuotaPeriod.Semester, null, false),
        ("PAED-011", QuotaPeriod.AcademicYear, null, false),
        ("PAED-012", QuotaPeriod.Semester, null, false),
        ("PAED-013", QuotaPeriod.AcademicYear, null, true),
        ("PAED-014", QuotaPeriod.AcademicYear, null, false),
        ("PAED-015", QuotaPeriod.AcademicYear, null, false),
    ];

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task MigrateAsync_OnAFreshDatabase_AddsTheColumnsTheBodiesTable_AndTheConstraintsThatGuardThem()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await MigrateToLatestAsync(schema);

            var columns = await QueryAsync(
                schema,
                """
                SELECT column_name, data_type, character_maximum_length, is_nullable, column_default
                FROM information_schema.columns
                WHERE table_schema = current_schema() AND table_name = 'CurriculumItems' AND column_name LIKE 'Decision%'
                ORDER BY column_name
                """,
                reader => (
                    Column: reader.GetString(0),
                    Type: reader.GetString(1),
                    MaxLength: reader.IsDBNull(2) ? (int?)null : Convert.ToInt32(reader.GetValue(2), CultureInfo.InvariantCulture),
                    Nullable: reader.GetString(3),
                    Default: reader.IsDBNull(4) ? null : reader.GetString(4)));

            columns.Should().Equal(
                ("DecisionBodyKey", "character varying", (int?)32, "YES", (string?)null),
                ("DecisionCadence", "integer", (int?)null, "YES", (string?)null),
                ("DecisionIsOpportunistic", "boolean", (int?)null, "NO", (string?)"false"));

            (await ScalarAsync<string>(
                    schema,
                    """
                    SELECT con.confdeltype::text
                    FROM pg_constraint con
                    JOIN pg_class rel ON rel.oid = con.conrelid
                    JOIN pg_class ref ON ref.oid = con.confrelid
                    JOIN pg_namespace ns ON ns.oid = rel.relnamespace
                    WHERE ns.nspname = current_schema() AND con.contype = 'f'
                      AND rel.relname = 'CurriculumItems' AND ref.relname = 'DecisionBodies'
                    """))
                .Should().Be("r", "a body in use cannot be deleted from under the items that name it (restrict)");

            (await QueryAsync(schema, """SELECT "Key", "Name" FROM "DecisionBodies" """, reader => (reader.GetString(0), reader.GetString(1))))
                .Should().Equal(new[] { ("neonatal", NeonatalName) }, "the migration inserts the body its stamp names, before the seeder runs");

            // The two constraints, each on an otherwise valid item.
            var itemId = await InsertBareCatalogueItemAsync(schema);

            var badCadence = () => ExecuteAsync(schema, """UPDATE "CurriculumItems" SET "DecisionCadence" = 5 WHERE "Id" = $1""", itemId);
            (await badCadence.Should().ThrowAsync<PostgresException>()).Which.SqlState.Should().Be(PostgresErrorCodes.CheckViolation);

            var unknownBody = () => ExecuteAsync(schema, """UPDATE "CurriculumItems" SET "DecisionBodyKey" = 'paediatric_icu' WHERE "Id" = $1""", itemId);
            (await unknownBody.Should().ThrowAsync<PostgresException>()).Which.SqlState.Should().BeOneOf(PostgresErrors.ForeignKeyViolationStates);

            var deleteInUse = async () =>
            {
                await ExecuteAsync(schema, """UPDATE "CurriculumItems" SET "DecisionBodyKey" = 'neonatal' WHERE "Id" = $1""", itemId);
                await ExecuteAsync(schema, """DELETE FROM "DecisionBodies" WHERE "Key" = 'neonatal'""");
            };
            (await deleteInUse.Should().ThrowAsync<PostgresException>()).Which.SqlState.Should().BeOneOf(
                PostgresErrors.ForeignKeyViolationStates, "a RESTRICT key: 23001 on PostgreSQL 18 (T243)");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task AFreshDatabase_MigratedThenSeeded_CarriesAnnexureB_AndASecondBootChangesNothing()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await MigrateToLatestAsync(schema);

            var firstBoot = await BootAsync(schema);
            firstBoot.Where(IsDecisionWarning).Should().BeEmpty();

            var afterFirst = await CatalogueDecisionsAsync(schema);
            afterFirst.OrderBy(row => row.Code, StringComparer.Ordinal).Should().Equal(AnnexureB);
            var perYear = afterFirst.Count(row => row.Cadence == QuotaPeriod.Semester) * 2
                          + afterFirst.Count(row => row.Cadence == QuotaPeriod.AcademicYear);
            perYear.Should().Be(21, "Annexure B: six EPAs decided each semester and nine annually");

            var secondBoot = await BootAsync(schema);
            secondBoot.Should().BeEmpty("a second boot finds every item as the catalogue says, and warns about nothing");
            (await CatalogueDecisionsAsync(schema)).Should().Equal(afterFirst);
            (await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "DecisionBodies" """)).Should().Be(1);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task Migration_OnAPopulatedDatabase_StampsExactlyTheFifteenCatalogueItems_AndTheNextBootAgrees()
    {
        try
        {
            var fixture = await ArrangePopulatedDatabaseAsync();

            await MigrateToLatestAsync(fixture.Schema);

            var stamped = await DecisionsByIdAsync(fixture.Schema);
            fixture.CatalogueItems.OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => (pair.Key, stamped[pair.Value].Cadence, stamped[pair.Value].Body, stamped[pair.Value].Opportunistic))
                .Should().Equal(AnnexureB);

            foreach (var (decoy, itemId) in fixture.Decoys)
            {
                stamped[itemId].Should().Be(((QuotaPeriod?)null, (string?)null, false), "{0} is not an item of the College's v11.1 catalogue", decoy);
            }

            var boot = await BootAsync(fixture.Schema);
            boot.Where(IsDecisionWarning).Select(entry => entry.Message).Should().BeEmpty(
                "the migration's literals equal what the seeder would have written");
            (await DecisionsByIdAsync(fixture.Schema)).Where(pair => stamped.ContainsKey(pair.Key))
                .Should().BeEquivalentTo(stamped, "the seeder never writes a decision onto an item that already exists");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task TheStamp_RunAgainAfterAnEdit_OverwritesNothingAlreadySet()
    {
        // Every column is created by the same migration, so on its one real run the guard always holds. It matters only if
        // the SQL is ever replayed, on a restored dump for instance, after an administrator has changed an item.
        try
        {
            var fixture = await ArrangePopulatedDatabaseAsync();
            await MigrateToLatestAsync(fixture.Schema);

            var paed004 = fixture.CatalogueItems["PAED-004"];
            await ExecuteAsync(fixture.Schema, """UPDATE "CurriculumItems" SET "DecisionBodyKey" = NULL WHERE "Id" = $1""", paed004);

            var stamp = new T131_DecisionCadenceAndBodies().UpOperations.OfType<SqlOperation>()
                .Select(operation => operation.Sql)
                .Single(sql => sql.Contains("UPDATE \"CurriculumItems\"", StringComparison.Ordinal));
            (await ExecuteAsync(fixture.Schema, stamp)).Should().Be(0);

            (await DecisionsByIdAsync(fixture.Schema))[paed004].Should().Be(((QuotaPeriod?)QuotaPeriod.Semester, (string?)null, false));
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task Down_RemovesTheColumnsAndTheBodiesTable()
    {
        try
        {
            var fixture = await ArrangePopulatedDatabaseAsync();
            await MigrateToLatestAsync(fixture.Schema);

            await using (var db = NewContext(fixture.Schema))
            {
                await db.GetService<IMigrator>().MigrateAsync(LastMigrationBeforeThis);
            }

            (await ScalarAsync<long>(
                    fixture.Schema,
                    """
                    SELECT COUNT(*) FROM information_schema.columns
                    WHERE table_schema = current_schema() AND table_name = 'CurriculumItems' AND column_name LIKE 'Decision%'
                    """))
                .Should().Be(0);
            (await ScalarAsync<bool>(
                    fixture.Schema,
                    """SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = current_schema() AND table_name = 'DecisionBodies')"""))
                .Should().BeFalse();
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ---- the populated database, before this migration ----------------------------------------------------------------

    private sealed record Fixture(string Schema, IReadOnlyDictionary<string, int> CatalogueItems, IReadOnlyDictionary<string, int> Decoys);

    private async Task<Fixture> ArrangePopulatedDatabaseAsync()
    {
        var schema = await _schemas.CreateAsync();

        await using (var db = NewContext(schema))
        {
            await db.GetService<IMigrator>().MigrateAsync(LastMigrationBeforeThis);
            (await db.Database.GetPendingMigrationsAsync())
                .Should().ContainSingle(migration => migration.EndsWith("_T131_DecisionCadenceAndBodies", StringComparison.Ordinal));
        }

        var catalogueItems = new Dictionary<string, int>(StringComparer.Ordinal);
        var decoys = new Dictionary<string, int>(StringComparer.Ordinal);

        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);

        var hospitalId = await InsertAsync(connection,
            """INSERT INTO "Institutions" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('Legacy Hospital', 'LEGACY', TIMESTAMPTZ '2026-06-19 00:00:00+00', TRUE) RETURNING "Id" """);
        var cpsaId = await InsertAsync(connection,
            """INSERT INTO "Colleges" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('College of Paediatricians of South Africa', 'CPSA', TIMESTAMPTZ '2026-06-19 00:00:00+00', TRUE) RETURNING "Id" """);
        var paediatricsId = await InsertAsync(connection,
            """INSERT INTO "Specialities" ("CollegeId", "Name", "IsActive") VALUES ($1, 'Paediatrics', TRUE) RETURNING "Id" """, cpsaId);
        var paediatricsSubId = await InsertAsync(connection,
            """INSERT INTO "SubSpecialities" ("SpecialityId", "Name", "IsActive") VALUES ($1, 'Paediatrics', TRUE) RETURNING "Id" """, paediatricsId);
        var neonatologySubId = await InsertAsync(connection,
            """INSERT INTO "SubSpecialities" ("SpecialityId", "Name", "IsActive") VALUES ($1, 'Neonatology', TRUE) RETURNING "Id" """, paediatricsId);

        var epaIds = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var (code, _, _, _) in AnnexureB)
        {
            epaIds[code] = await InsertEpaAsync(connection, paediatricsSubId, code, owningInstitutionId: null);
        }

        var localPaed004 = await InsertEpaAsync(connection, paediatricsSubId, "PAED-004", owningInstitutionId: hospitalId);
        var paed016 = await InsertEpaAsync(connection, paediatricsSubId, "PAED-016", owningInstitutionId: null);
        var neonatalPaed004 = await InsertEpaAsync(connection, neonatologySubId, "PAED-004", owningInstitutionId: null);

        var catalogue = await InsertCurriculumAsync(connection, paediatricsSubId, CurriculumName, CatalogueVersion);
        var nextVersion = await InsertCurriculumAsync(connection, paediatricsSubId, CurriculumName, "11.2");
        var researchTrack = await InsertCurriculumAsync(connection, paediatricsSubId, "Paediatric Research Track", CatalogueVersion);
        var neonatalCurriculum = await InsertCurriculumAsync(connection, neonatologySubId, CurriculumName, CatalogueVersion);

        foreach (var (code, _, _, _) in AnnexureB)
        {
            catalogueItems[code] = await InsertCurriculumItemAsync(connection, catalogue, epaIds[code], owningInstitutionId: null);
        }

        // Each decoy pins one clause of the stamp's scope, as T122's fixture does.
        decoys["an institution-local item on a local EPA reusing PAED-004"] =
            await InsertCurriculumItemAsync(connection, catalogue, localPaed004, owningInstitutionId: hospitalId);
        decoys["a national item on PAED-016, which the catalogue does not declare"] =
            await InsertCurriculumItemAsync(connection, catalogue, paed016, owningInstitutionId: null);
        decoys["a national item in the catalogue curriculum on Neonatology's PAED-004"] =
            await InsertCurriculumItemAsync(connection, catalogue, neonatalPaed004, owningInstitutionId: null);
        decoys["PAED-004 in version 11.2 of the curriculum"] =
            await InsertCurriculumItemAsync(connection, nextVersion, epaIds["PAED-004"], owningInstitutionId: null);
        decoys["PAED-004 in the Paediatric Research Track"] =
            await InsertCurriculumItemAsync(connection, researchTrack, epaIds["PAED-004"], owningInstitutionId: null);
        decoys["PAED-004 in Neonatology's own Paediatric EPA Curriculum 11.1"] =
            await InsertCurriculumItemAsync(connection, neonatalCurriculum, neonatalPaed004, owningInstitutionId: null);

        return new Fixture(schema, catalogueItems, decoys);
    }

    /// <summary>One valid national item on a fresh, migrated schema, to try the constraints against.</summary>
    private async Task<int> InsertBareCatalogueItemAsync(string schema)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        var collegeId = await InsertAsync(connection,
            """INSERT INTO "Colleges" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('CPSA', 'CPSA', TIMESTAMPTZ '2026-06-19 00:00:00+00', TRUE) RETURNING "Id" """);
        var specialityId = await InsertAsync(connection,
            """INSERT INTO "Specialities" ("CollegeId", "Name", "IsActive") VALUES ($1, 'Paediatrics', TRUE) RETURNING "Id" """, collegeId);
        var subSpecialityId = await InsertAsync(connection,
            """INSERT INTO "SubSpecialities" ("SpecialityId", "Name", "IsActive") VALUES ($1, 'Paediatrics', TRUE) RETURNING "Id" """, specialityId);
        var epaId = await InsertEpaAsync(connection, subSpecialityId, "PAED-001", owningInstitutionId: null);
        var curriculumId = await InsertCurriculumAsync(connection, subSpecialityId, CurriculumName, CatalogueVersion);
        return await InsertCurriculumItemAsync(connection, curriculumId, epaId, owningInstitutionId: null);
    }

    private static Task<int> InsertEpaAsync(NpgsqlConnection connection, int subSpecialityId, string code, int? owningInstitutionId)
        => InsertAsync(connection,
            """INSERT INTO "Epas" ("SubSpecialityId", "OwningInstitutionId", "Code", "Title", "Category", "CreatedOn", "IsActive") VALUES ($1, $3, $2, $2, 0, TIMESTAMPTZ '2026-01-01 00:00:00+00', TRUE) RETURNING "Id" """,
            subSpecialityId, code, (object?)owningInstitutionId ?? DBNull.Value);

    private static Task<int> InsertCurriculumAsync(NpgsqlConnection connection, int subSpecialityId, string name, string version)
        => InsertAsync(connection,
            """INSERT INTO "Curricula" ("SubSpecialityId", "Name", "Version", "EffectiveFrom", "IsActive") VALUES ($1, $2, $3, DATE '2026-01-01', TRUE) RETURNING "Id" """,
            subSpecialityId, name, version);

    private static Task<int> InsertCurriculumItemAsync(NpgsqlConnection connection, int curriculumId, int epaId, int? owningInstitutionId)
        => InsertAsync(connection,
            """INSERT INTO "CurriculumItems" ("CurriculumId", "EpaId", "OwningInstitutionId", "RequiredCount", "QuotaPeriod", "MinimumLevelOrder", "WindowMonths") VALUES ($1, $2, $3, 3, 1, 6, 12) RETURNING "Id" """,
            curriculumId, epaId, (object?)owningInstitutionId ?? DBNull.Value);

    // ---- reads and boots ------------------------------------------------------------------------------------------------

    private async Task<List<(string Code, QuotaPeriod? Cadence, string? Body, bool Opportunistic)>> CatalogueDecisionsAsync(string schema)
    {
        await using var db = NewContext(schema);
        var rows = await db.CurriculumItems.AsNoTracking()
            .Where(item => item.Curriculum.Name == CurriculumName && item.Curriculum.Version == CatalogueVersion && item.OwningInstitutionId == null)
            .Select(item => new { item.Epa.Code, item.DecisionCadence, item.DecisionBodyKey, item.DecisionIsOpportunistic })
            .ToListAsync();

        return rows.OrderBy(row => row.Code, StringComparer.Ordinal)
            .Select(row => (row.Code, row.DecisionCadence, row.DecisionBodyKey, row.DecisionIsOpportunistic))
            .ToList();
    }

    private async Task<Dictionary<int, (QuotaPeriod? Cadence, string? Body, bool Opportunistic)>> DecisionsByIdAsync(string schema)
    {
        await using var db = NewContext(schema);
        return await db.CurriculumItems.AsNoTracking()
            .ToDictionaryAsync(item => item.Id, item => (item.DecisionCadence, item.DecisionBodyKey, item.DecisionIsOpportunistic));
    }

    /// <summary>One startup's seeding, as <c>Program.cs</c> runs it. Returns the catalogue seeder's warnings.</summary>
    private async Task<List<CapturedWarning>> BootAsync(string schema)
    {
        var log = new WarningLog();

        await using (var db = NewContext(schema))
        {
            await new DataSeeder(db).SeedAsync();
        }

        await using (var db = NewContext(schema))
        {
            await new PaediatricCatalogueSeeder(db, log).SeedAsync();
        }

        return log.Warnings;
    }

    private static bool IsDecisionWarning(CapturedWarning warning) => warning.Values.ContainsKey("ExpectedCadence");

    private sealed record CapturedWarning(string Message, IReadOnlyDictionary<string, object?> Values);

    private sealed class WarningLog : ILogger<PaediatricCatalogueSeeder>
    {
        public List<CapturedWarning> Warnings { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel != LogLevel.Warning)
            {
                return;
            }

            var values = state is IReadOnlyList<KeyValuePair<string, object?>> pairs
                ? pairs.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal)
                : new Dictionary<string, object?>(StringComparer.Ordinal);
            Warnings.Add(new CapturedWarning(formatter(state, exception), values));
        }
    }

    // ---- schema lifecycle (as WbaToolAllowListPostgresTests) ------------------------------------------------------------

    private async Task MigrateToLatestAsync(string schema)
    {
        await using var db = NewContext(schema);
        await db.Database.MigrateAsync();
        (await db.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
    }

    private ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema)).Options);

    private static async Task<int> InsertAsync(NpgsqlConnection connection, string sql, params object[] values)
    {
        await using var command = Command(connection, sql, values);
        return Convert.ToInt32(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    private async Task<int> ExecuteAsync(string schema, string sql, params object[] values)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        await using var command = Command(connection, sql, values);
        return await command.ExecuteNonQueryAsync();
    }

    private async Task<T> ScalarAsync<T>(string schema, string sql)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        await using var command = new NpgsqlCommand(sql, connection);
        var value = await command.ExecuteScalarAsync();
        return (T)Convert.ChangeType(value!, typeof(T), CultureInfo.InvariantCulture);
    }

    private async Task<List<T>> QueryAsync<T>(string schema, string sql, Func<NpgsqlDataReader, T> map)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        await using var command = new NpgsqlCommand(sql, connection);
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
}
