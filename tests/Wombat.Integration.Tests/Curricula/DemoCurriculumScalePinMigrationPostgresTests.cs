using System.Globalization;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Persistence.Migrations;

namespace Wombat.Integration.Tests.Curricula;

/// <summary>
/// T174 on a real PostgreSQL server: the one-off pin of the demo curriculum's own item, which replaced
/// <c>DataSeeder</c>'s boot-time pin.
/// </summary>
/// <remarks>
/// <para>
/// The unit suites run on EF InMemory, which never runs a migration, so the hand-written UPDATE in
/// <c>T174_PinDemoCurriculumItemScale</c> is exercised nowhere else. The case it exists for is production: an item
/// created before T109, left with a null <c>ScaleId</c> by T109's new column, which no boot pins any more.
/// </para>
/// <para>
/// The schema helpers follow <see cref="WbaToolAllowListPostgresTests" />: each test runs on a schema of its own
/// (<c>SearchPath = it_&lt;guid&gt;</c>, nothing else on the path), registered before it is created and dropped in a
/// <c>finally</c>, with <see cref="DisposeAsync" /> as a backstop.
/// </para>
/// </remarks>
public sealed class DemoCurriculumScalePinMigrationPostgresTests : IAsyncLifetime
{
    private const string T174Migration = "20260924140337_T174_PinDemoCurriculumItemScale";
    // T160's migration, merged after T174 but timestamped before it, sorts between T137 and T174.
    private const string LastMigrationBeforeT174 = "20260924134441_T160_FilingDaysAfterEncounter";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    /// <summary>
    /// A database in production's shape: the demo item unpinned since T109. The migration pins it to the O-R Scale and
    /// touches no other row, each decoy pinning one clause of the statement.
    /// </summary>
    [Fact]
    public async Task Migration_OnAPreT174Database_PinsOnlyTheDemoItem_ToTheOrScale()
    {
        try
        {
            var fixture = await ArrangePreT174DatabaseAsync(withOrScale: true);

            await using (var db = NewContext(fixture.Schema))
            {
                await db.Database.MigrateAsync();
                (await db.Database.GetAppliedMigrationsAsync()).Should().Contain(T174Migration);
            }

            var scales = await StoredScalesAsync(fixture.Schema);
            scales[fixture.DemoItem].Should().Be(fixture.OrScaleId, "the item DataSeeder authored on the O-R Scale is pinned to it");
            foreach (var (decoy, id) in fixture.Decoys)
            {
                scales[id].Should().BeNull($"the migration must not pin {decoy}");
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// The statement guesses nothing: with no scale named "O-R Scale" it pins nothing, although another ladder exists.
    /// </summary>
    [Fact]
    public async Task Migration_WithNoOrScale_PinsNothing_EvenWithAnotherLadderPresent()
    {
        try
        {
            var fixture = await ArrangePreT174DatabaseAsync(withOrScale: false);

            await using (var db = NewContext(fixture.Schema))
            {
                await db.Database.MigrateAsync();
            }

            (await StoredScalesAsync(fixture.Schema)).Values.Should().HaveCount(6).And.OnlyContain(scaleId => scaleId == null);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// Run again, the statement never overwrites a pin: an item an administrator has since moved to another ladder
    /// stays there. Only a null is ever filled, which is also why the migration runs once and not at every boot.
    /// </summary>
    [Fact]
    public async Task MigrationSql_RunAgainAfterTheMigration_NeverOverwritesAPin()
    {
        try
        {
            var fixture = await ArrangePreT174DatabaseAsync(withOrScale: true);

            await using (var db = NewContext(fixture.Schema))
            {
                await db.Database.MigrateAsync();
            }

            var otherLadder = await ScalarAsync<int>(fixture.Schema, """INSERT INTO "EntrustmentScales" ("Name") VALUES ('Another ladder') RETURNING "Id" """);
            await ExecuteAsync(fixture.Schema, """UPDATE "CurriculumItems" SET "ScaleId" = $1 WHERE "Id" = $2""", otherLadder, fixture.DemoItem);

            (await ExecuteAsync(fixture.Schema, T174Sql())).Should().Be(0);

            (await StoredScalesAsync(fixture.Schema))[fixture.DemoItem].Should().Be(otherLadder);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// The demo rows as a 16 September build created them (before T109 added <c>ScaleId</c>), migrated to just before
    /// T174, with a decoy for each clause of the statement.
    /// </summary>
    private async Task<PreT174Fixture> ArrangePreT174DatabaseAsync(bool withOrScale)
    {
        var schema = await _schemas.CreateAsync();

        await using (var db = NewContext(schema))
        {
            await db.GetService<IMigrator>().MigrateAsync(LastMigrationBeforeT174);

            (await db.Database.GetAppliedMigrationsAsync()).Last().Should().Be(LastMigrationBeforeT174);
            (await db.Database.GetPendingMigrationsAsync()).First().Should().Be(T174Migration);
        }

        var decoys = new Dictionary<string, int>(StringComparer.Ordinal);
        int? orScaleId = null;
        int demoItem;

        await using (var connection = await TestDatabase.OpenSchemaConnectionAsync(schema))
        {
            // Another ladder first, so that a statement which lost its scale-name clause would have a wrong scale to
            // pin to, and the tests would see it.
            await InsertScaleAsync(connection, "CPSA Paediatric Entrustment Scale v11.1", ["1", "2", "3a", "3b", "4", "5"]);
            if (withOrScale)
            {
                orScaleId = await InsertScaleAsync(
                    connection,
                    "O-R Scale",
                    ["Observe only", "Direct supervision", "Indirect supervision", "Independent", "Supervises others"]);
            }

            var demoCollege = await InsertCollegeAsync(connection, "Demo College", "DEMO-C");
            var generalMedicine = await InsertAsync(connection,
                """INSERT INTO "Specialities" ("CollegeId", "Name", "IsActive") VALUES ($1, 'General Medicine', TRUE) RETURNING "Id" """,
                demoCollege);
            var gim = await InsertSubSpecialityAsync(connection, generalMedicine, "General Internal Medicine");
            var cardiology = await InsertSubSpecialityAsync(connection, generalMedicine, "Cardiology");

            var otherCollege = await InsertCollegeAsync(connection, "Another College", "OTHER-C");
            var otherSpeciality = await InsertAsync(connection,
                """INSERT INTO "Specialities" ("CollegeId", "Name", "IsActive") VALUES ($1, 'General Medicine', TRUE) RETURNING "Id" """,
                otherCollege);
            var otherGim = await InsertSubSpecialityAsync(connection, otherSpeciality, "General Internal Medicine");

            var epa001 = await InsertEpaAsync(connection, gim, "EPA-001");
            var epa002 = await InsertEpaAsync(connection, gim, "EPA-002");
            var cardiologyEpa001 = await InsertEpaAsync(connection, cardiology, "EPA-001");
            var otherEpa001 = await InsertEpaAsync(connection, otherGim, "EPA-001");

            var imCore = await InsertCurriculumAsync(connection, gim, "IM Core Curriculum", "2026.1");
            var imCoreNext = await InsertCurriculumAsync(connection, gim, "IM Core Curriculum", "2026.2");
            var researchTrack = await InsertCurriculumAsync(connection, gim, "IM Research Track", "2026.1");
            var otherImCore = await InsertCurriculumAsync(connection, otherGim, "IM Core Curriculum", "2026.1");

            demoItem = await InsertCurriculumItemAsync(connection, imCore, epa001);

            // Pins e."Code" = 'EPA-001': an administrator's own item in the demo curriculum.
            decoys["an administrator's own item on EPA-002 in the demo curriculum"] = await InsertCurriculumItemAsync(connection, imCore, epa002);
            // Pins e."SubSpecialityId" = ss."Id": the right curriculum and code, another discipline's EPA.
            decoys["Cardiology's EPA-001 in the demo curriculum"] = await InsertCurriculumItemAsync(connection, imCore, cardiologyEpa001);
            // Pins c."Version": a clone at another version is its author's.
            decoys["EPA-001 in version 2026.2"] = await InsertCurriculumItemAsync(connection, imCoreNext, epa001);
            // Pins c."Name": another curriculum of the same discipline and version.
            decoys["EPA-001 in the IM Research Track"] = await InsertCurriculumItemAsync(connection, researchTrack, epa001);
            // Pins co."ShortCode": the same names under another College.
            decoys["another College's IM Core Curriculum"] = await InsertCurriculumItemAsync(connection, otherImCore, otherEpa001);
        }

        return new PreT174Fixture(schema, demoItem, orScaleId, decoys);
    }

    /// <summary>The migration's statement, exactly as its <c>Up</c> hands it to <c>Sql()</c>.</summary>
    private static string T174Sql()
        => new T174_PinDemoCurriculumItemScale().UpOperations.OfType<SqlOperation>().Should().ContainSingle().Which.Sql;

    private async Task<Dictionary<int, int?>> StoredScalesAsync(string schema)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        await using var command = Command(connection, """SELECT "Id", "ScaleId" FROM "CurriculumItems" """, []);
        await using var reader = await command.ExecuteReaderAsync();

        var results = new Dictionary<int, int?>();
        while (await reader.ReadAsync())
        {
            results[reader.GetInt32(0)] = reader.IsDBNull(1) ? null : reader.GetInt32(1);
        }

        return results;
    }

    private static async Task<int> InsertScaleAsync(NpgsqlConnection connection, string name, string[] rungs)
    {
        var scaleId = await InsertAsync(connection, """INSERT INTO "EntrustmentScales" ("Name") VALUES ($1) RETURNING "Id" """, name);
        for (var order = 1; order <= rungs.Length; order++)
        {
            await InsertAsync(connection,
                """INSERT INTO "EntrustmentLevels" ("ScaleId", "Order", "Label") VALUES ($1, $2, $3) RETURNING "Id" """,
                scaleId, order, rungs[order - 1]);
        }

        return scaleId;
    }

    private static Task<int> InsertCollegeAsync(NpgsqlConnection connection, string name, string shortCode)
        => InsertAsync(connection,
            """INSERT INTO "Colleges" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ($1, $2, TIMESTAMPTZ '2026-06-19 00:00:00+00', TRUE) RETURNING "Id" """,
            name, shortCode);

    private static Task<int> InsertSubSpecialityAsync(NpgsqlConnection connection, int specialityId, string name)
        => InsertAsync(connection,
            """INSERT INTO "SubSpecialities" ("SpecialityId", "Name", "IsActive") VALUES ($1, $2, TRUE) RETURNING "Id" """,
            specialityId, name);

    private static Task<int> InsertEpaAsync(NpgsqlConnection connection, int subSpecialityId, string code)
        => InsertAsync(connection,
            """INSERT INTO "Epas" ("SubSpecialityId", "OwningInstitutionId", "Code", "Title", "Category", "CreatedOn", "IsActive") VALUES ($1, NULL, $2, $2, 0, TIMESTAMPTZ '2026-01-01 00:00:00+00', TRUE) RETURNING "Id" """,
            subSpecialityId, code);

    private static Task<int> InsertCurriculumAsync(NpgsqlConnection connection, int subSpecialityId, string name, string version)
        => InsertAsync(connection,
            """INSERT INTO "Curricula" ("SubSpecialityId", "Name", "Version", "EffectiveFrom", "IsActive") VALUES ($1, $2, $3, DATE '2026-01-01', TRUE) RETURNING "Id" """,
            subSpecialityId, name, version);

    /// <summary>An unpinned item, as every item was the moment T109 added the column. 4 is the demo's minimum.</summary>
    private static Task<int> InsertCurriculumItemAsync(NpgsqlConnection connection, int curriculumId, int epaId)
        => InsertAsync(connection,
            """INSERT INTO "CurriculumItems" ("CurriculumId", "EpaId", "OwningInstitutionId", "RequiredCount", "QuotaPeriod", "MinimumLevelOrder", "WindowMonths", "ScaleId") VALUES ($1, $2, NULL, 5, 0, 4, 12, NULL) RETURNING "Id" """,
            curriculumId, epaId);

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

    private async Task<T> ScalarAsync<T>(string schema, string sql, params object[] values)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        await using var command = Command(connection, sql, values);
        var value = await command.ExecuteScalarAsync();
        return (T)Convert.ChangeType(value!, typeof(T), CultureInfo.InvariantCulture);
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

    private ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema)).Options);

    private sealed record PreT174Fixture(
        string Schema,
        int DemoItem,
        int? OrScaleId,
        IReadOnlyDictionary<string, int> Decoys);
}
