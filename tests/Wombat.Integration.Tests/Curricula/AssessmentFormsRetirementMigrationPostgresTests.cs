using System.Globalization;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.Curricula;

/// <summary>
/// T145 on a real PostgreSQL server: the migration that retires the legacy assessment-forms feature. Its EPA links
/// restricted nothing (no activity, credit rule or picker read them); which instrument may assess an EPA is the curriculum
/// item's permitted-tools list (T122).
/// </summary>
/// <remarks>
/// <para>
/// The unit suites run on EF InMemory, which never runs a migration, so the drop is exercised nowhere else. The schema is
/// stopped at the last pre-T145 migration and given a form in the shape the retired screen wrote (a criterion and an EPA
/// link), because a drop that only ever meets empty tables proves nothing about the order it drops them in.
/// </para>
/// <para>
/// The schema helpers follow <c>SystemManagedMigrationPostgresTests</c>: each test works in a schema of its own
/// (<c>SearchPath = it_&lt;guid&gt;</c>), registered before it is created and dropped in a <c>finally</c> and again from
/// <see cref="DisposeAsync" />.
/// </para>
/// </remarks>
public sealed class AssessmentFormsRetirementMigrationPostgresTests : IAsyncLifetime
{
    private const string LastMigrationBeforeT145 = "20260924235944_T131_EntrustmentOnlyReviews";
    private const string T145MigrationSuffix = "_T145_RetireAssessmentForms";

    private static readonly string[] FormTables = ["AssessmentForms", "FormCriteria", "FormEpaLinks"];

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task Migration_DropsTheThreeFormTables_WithTheirRows_AndLeavesWhatTheyPointedAt()
    {
        try
        {
            var schema = await ArrangePreT145SchemaAsync();
            LegacyForm form;
            await using (var connection = await TestDatabase.OpenSchemaConnectionAsync(schema))
            {
                form = await InsertLinkedFormAsync(connection);
            }

            foreach (var table in FormTables)
            {
                (await ScalarAsync<long>(schema, $"""SELECT COUNT(*) FROM "{table}" """))
                    .Should().Be(1, $"guard: {table} holds the retired screen's row before T145");
            }

            await MigrateToT145Async(schema);

            foreach (var table in FormTables)
            {
                (await Catalog.TableExistsAsync(schema, table)).Should().BeFalse($"T145 drops {table}");
            }

            // The form's foreign keys pointed out of the feature, all ON DELETE RESTRICT: T145 drops its three tables and
            // touches nothing they pointed at.
            (await ScalarAsync<long>(schema, $"""SELECT COUNT(*) FROM "Epas" WHERE "Id" = {form.EpaId}""")).Should().Be(1);
            (await ScalarAsync<long>(schema, $"""SELECT COUNT(*) FROM "EntrustmentScales" WHERE "Id" = {form.ScaleId}""")).Should().Be(1);
            (await ScalarAsync<long>(schema, $"""SELECT COUNT(*) FROM "SubSpecialities" WHERE "Id" = {form.SubSpecialityId}""")).Should().Be(1);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task Down_RestoresTheThreeFormTablesEmpty()
    {
        try
        {
            var schema = await ArrangePreT145SchemaAsync();
            await using (var connection = await TestDatabase.OpenSchemaConnectionAsync(schema))
            {
                await InsertLinkedFormAsync(connection);
            }

            await MigrateToT145Async(schema);

            await using (var db = NewContext(schema))
            {
                await db.GetService<IMigrator>().MigrateAsync(LastMigrationBeforeT145);
            }

            foreach (var table in FormTables)
            {
                (await Catalog.TableExistsAsync(schema, table)).Should().BeTrue($"Down recreates {table}");
                (await ScalarAsync<long>(schema, $"""SELECT COUNT(*) FROM "{table}" """)).Should().Be(0, $"Down restores {table} empty");
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ---- the pre-T145 database ----------------------------------------------------------------------------------------

    private async Task<string> ArrangePreT145SchemaAsync()
    {
        var schema = await _schemas.CreateAsync();

        await using var db = NewContext(schema);
        await db.GetService<IMigrator>().MigrateAsync(LastMigrationBeforeT145);

        // Contain, not ContainSingle: a migration merged from a parallel branch may sort between the two.
        (await db.Database.GetPendingMigrationsAsync())
            .Should().Contain(migration => migration.EndsWith(T145MigrationSuffix, StringComparison.Ordinal));

        return schema;
    }

    private sealed record LegacyForm(int FormId, int EpaId, int ScaleId, int SubSpecialityId);

    /// <summary>What the retired screen wrote: a form on a scale, with one criterion and one EPA link.</summary>
    private static async Task<LegacyForm> InsertLinkedFormAsync(NpgsqlConnection connection)
    {
        var collegeId = await InsertAsync(connection,
            """INSERT INTO "Colleges" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('CPSA', 'CPSA', TIMESTAMPTZ '2026-06-19 00:00:00+00', TRUE) RETURNING "Id" """);
        var specialityId = await InsertAsync(connection,
            """INSERT INTO "Specialities" ("CollegeId", "Name", "IsActive") VALUES ($1, 'Paediatrics', TRUE) RETURNING "Id" """, collegeId);
        var subSpecialityId = await InsertAsync(connection,
            """INSERT INTO "SubSpecialities" ("SpecialityId", "Name", "IsActive") VALUES ($1, 'Paediatrics', TRUE) RETURNING "Id" """, specialityId);
        var epaId = await InsertAsync(connection,
            """INSERT INTO "Epas" ("SubSpecialityId", "Code", "Title", "Category", "CreatedOn", "IsActive") VALUES ($1, 'PAED-001', 'Take a history', 0, TIMESTAMPTZ '2026-01-01 00:00:00+00', TRUE) RETURNING "Id" """,
            subSpecialityId);
        var scaleId = await InsertAsync(connection,
            """INSERT INTO "EntrustmentScales" ("Name") VALUES ('O-R Scale') RETURNING "Id" """);

        var formId = await InsertAsync(connection,
            """
            INSERT INTO "AssessmentForms" ("Name", "SubSpecialityId", "ScaleId", "CanDelete", "IsActive")
            VALUES ('Legacy Mini-CEX', $1, $2, TRUE, TRUE)
            RETURNING "Id"
            """,
            subSpecialityId, scaleId);
        await InsertAsync(connection,
            """INSERT INTO "FormCriteria" ("FormId", "Order", "Prompt", "IsRequired") VALUES ($1, 1, 'History taking', TRUE) RETURNING "Id" """,
            formId);
        await InsertAsync(connection,
            """INSERT INTO "FormEpaLinks" ("FormId", "EpaId") VALUES ($1, $2) RETURNING "Id" """,
            formId, epaId);

        return new LegacyForm(formId, epaId, scaleId, subSpecialityId);
    }

    // ---- reads ---------------------------------------------------------------------------------------------------------

    // ---- schema lifecycle (as SystemManagedMigrationPostgresTests) ------------------------------------------------------

    /// <summary>
    /// Up to T145 exactly, not to the latest migration: the Down test then rolls back T145 alone, so a later migration
    /// with a broken or irreversible Down cannot fail this test for a reason that is not T145's.
    /// </summary>
    private async Task MigrateToT145Async(string schema)
    {
        await using var db = NewContext(schema);
        var t145 = db.Database.GetMigrations().Single(migration => migration.EndsWith(T145MigrationSuffix, StringComparison.Ordinal));
        await db.GetService<IMigrator>().MigrateAsync(t145);
        (await db.Database.GetAppliedMigrationsAsync()).Last().Should().Be(t145);
    }

    private ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema)).Options);

    private static async Task<int> InsertAsync(NpgsqlConnection connection, string sql, params object[] values)
    {
        await using var command = Command(connection, sql, values);
        return Convert.ToInt32(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    private async Task<T> ScalarAsync<T>(string schema, string sql)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        await using var command = new NpgsqlCommand(sql, connection);
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
}
