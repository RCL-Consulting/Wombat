using System.Globalization;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.Activities;

/// <summary>
/// T162 on a real PostgreSQL server: the migration that flags the seeded <c>msf_cpsa</c> as system-managed, deletes the
/// hand-made drafts of it that nobody could ever finish, and drops the dead <c>ActivityPermissionRules</c> table.
/// </summary>
/// <remarks>
/// <para>
/// The migration's SQL runs nowhere else: the unit suites use EF InMemory, which never runs a migration, and on a fresh
/// database the UPDATE and the DELETE touch nothing, because the seeders run afterwards. So they are rehearsed here on a
/// schema stopped at the last pre-T162 migration and filled in its shape by raw SQL.
/// </para>
/// <para>
/// The schema helpers follow <c>EvidenceEpaMigrationPostgresTests</c>: each test works in a schema of its own
/// (<c>SearchPath = it_&lt;guid&gt;</c>), registered before it is created and dropped in a <c>finally</c> and again from
/// <see cref="DisposeAsync" />.
/// </para>
/// </remarks>
public sealed class SystemManagedMigrationPostgresTests : IAsyncLifetime
{
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";
    private const string LastMigrationBeforeT162 = "20260924153716_T167_CommitteeEvidenceNamesEachLine";
    private const string SeedOwnerUserId = "seed-system";
    private const string TraineeId = "trainee-1";
    private const string CoordinatorId = "coordinator-1";

    private readonly List<string> _schemas = [];
    private string _baseConnectionString = null!;

    public Task InitializeAsync()
    {
        _baseConnectionString = ResolveBaseConnectionString();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => DropSchemasAsync();

    [Fact]
    public async Task Migration_FlagsTheSeededMsfType_AndNothingElse()
    {
        try
        {
            var schema = await ArrangePreT162SchemaAsync();
            await using (var connection = await OpenAsync(schema))
            {
                await InsertSeedTypeAsync(connection, "msf_cpsa", SeedOwnerUserId);
                await InsertSeedTypeAsync(connection, "mini_cex_cpsa", SeedOwnerUserId);
            }

            await MigrateToLatestAsync(schema);

            await using var db = NewContext(schema);
            var flags = await db.ActivityTypes.AsNoTracking().ToDictionaryAsync(type => type.Key, type => type.SystemManaged);
            flags.Should().Equal(new Dictionary<string, bool> { ["msf_cpsa"] = true, ["mini_cex_cpsa"] = false });
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task Migration_DeletesOnlyTheHandMadeMsfDrafts_WithTheirHistory()
    {
        try
        {
            var schema = await ArrangePreT162SchemaAsync();
            var activities = new Dictionary<string, int>(StringComparer.Ordinal);
            await using (var connection = await OpenAsync(schema))
            {
                var msf = await InsertSeedTypeAsync(connection, "msf_cpsa", SeedOwnerUserId);
                var miniCex = await InsertSeedTypeAsync(connection, "mini_cex_cpsa", SeedOwnerUserId);

                // Dev activity 23's shape: a trainee filed an msf_cpsa draft from /activities/new, and every field was
                // dropped at creation.
                activities["the trainee's own msf draft"] = await InsertActivityAsync(connection, msf, "draft", creator: TraineeId);

                // What the release writes: recorded, by the releasing coordinator, about the trainee.
                activities["a released msf record"] = await InsertActivityAsync(connection, msf, "recorded", creator: CoordinatorId);

                // Neither of these is a shape any path writes; each pins one half of the predicate.
                activities["an msf draft someone else created"] = await InsertActivityAsync(connection, msf, "draft", creator: CoordinatorId);
                activities["a self-created msf row that was recorded"] = await InsertActivityAsync(connection, msf, "recorded", creator: TraineeId);

                // Another type's draft is not the migration's business.
                activities["the trainee's own mini-cex draft"] = await InsertActivityAsync(connection, miniCex, "draft", creator: TraineeId);

                foreach (var id in activities.Values)
                {
                    await InsertCreateTransitionAsync(connection, id);
                }
            }

            await MigrateToLatestAsync(schema);

            await using var db = NewContext(schema);
            var remaining = await db.Activities.AsNoTracking().Select(activity => activity.Id).ToListAsync();
            remaining.Should().BeEquivalentTo(new[]
            {
                activities["a released msf record"],
                activities["an msf draft someone else created"],
                activities["a self-created msf row that was recorded"],
                activities["the trainee's own mini-cex draft"]
            });

            var transitions = await db.ActivityTransitions.AsNoTracking().Select(transition => transition.ActivityId).ToListAsync();
            transitions.Should().BeEquivalentTo(remaining, "the deleted draft's history goes with it, and nobody else's does");
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task Migration_LeavesATypeAnOperatorBuiltUnderTheKey_AndItsDrafts_Alone()
    {
        // T122's rule: a type an operator built under a colliding key is theirs, and the migration asserts nothing about it.
        try
        {
            var schema = await ArrangePreT162SchemaAsync();
            int draft;
            await using (var connection = await OpenAsync(schema))
            {
                var operatorMsf = await InsertSeedTypeAsync(connection, "msf_cpsa", "operator-1");
                draft = await InsertActivityAsync(connection, operatorMsf, "draft", creator: TraineeId);
            }

            await MigrateToLatestAsync(schema);

            await using var db = NewContext(schema);
            (await db.ActivityTypes.AsNoTracking().SingleAsync()).SystemManaged.Should().BeFalse();
            (await db.Activities.AsNoTracking().Select(activity => activity.Id).ToListAsync()).Should().Equal(draft);
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task Migration_DropsThePermissionRuleTable_AndDownRestoresItEmpty()
    {
        try
        {
            var schema = await ArrangePreT162SchemaAsync();
            (await TableExistsAsync(schema, "ActivityPermissionRules")).Should().BeTrue("guard: the table exists before T162");

            await MigrateToLatestAsync(schema);

            (await TableExistsAsync(schema, "ActivityPermissionRules")).Should().BeFalse();
            (await ColumnExistsAsync(schema, "ActivityTypes", "SystemManaged")).Should().BeTrue();

            await using (var db = NewContext(schema))
            {
                await db.GetService<IMigrator>().MigrateAsync(LastMigrationBeforeT162);
            }

            (await TableExistsAsync(schema, "ActivityPermissionRules")).Should().BeTrue();
            (await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "ActivityPermissionRules" """)).Should().Be(0);
            (await ColumnExistsAsync(schema, "ActivityTypes", "SystemManaged")).Should().BeFalse();
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    // ---- the pre-T162 database ----------------------------------------------------------------------------------------

    private async Task<string> ArrangePreT162SchemaAsync()
    {
        var schema = await CreateSchemaAsync();

        await using var db = NewContext(schema);
        await db.GetService<IMigrator>().MigrateAsync(LastMigrationBeforeT162);
        (await db.Database.GetPendingMigrationsAsync())
            .Should().ContainSingle(migration => migration.EndsWith("_T162_SystemManagedActivityTypes", StringComparison.Ordinal));

        return schema;
    }

    /// <summary>A type with its seed folder's canonical payloads, published at version 1, owned by <paramref name="ownerUserId" />.</summary>
    private static async Task<int> InsertSeedTypeAsync(NpgsqlConnection connection, string key, string ownerUserId)
    {
        var entry = ActivityTypeSeedCatalogue.Entries.Single(candidate => candidate.Key == key);
        var payload = await ActivityTypeSeedCatalogue.ReadCanonicalAsync(entry, CancellationToken.None);

        var typeId = await InsertAsync(connection,
            """
            INSERT INTO "ActivityTypes"
                ("Key", "Name", "Scope", "Version", "SchemaJson", "WorkflowJson", "CreditRulesJson", "DisplayFieldsJson",
                 "OwnerUserId", "CreatedOn", "IsActive", "WbaToolKey")
            VALUES ($1, $2, 0, 1, $3::jsonb, $4::jsonb, $5::jsonb, $6::jsonb, $7, TIMESTAMPTZ '2026-06-19 00:00:00+00', TRUE, $8)
            RETURNING "Id"
            """,
            key, entry.Name, payload.SchemaJson, payload.WorkflowJson, payload.CreditRulesJson, payload.DisplayFieldsJson,
            ownerUserId, (object?)entry.WbaToolKey ?? DBNull.Value);

        await ExecuteAsync(connection,
            """
            INSERT INTO "ActivityTypeVersions"
                ("ActivityTypeId", "Version", "SchemaJson", "WorkflowJson", "CreditRulesJson", "DisplayFieldsJson", "PublishedByUserId", "PublishedOn")
            VALUES ($1, 1, $2::jsonb, $3::jsonb, $4::jsonb, $5::jsonb, $6, TIMESTAMPTZ '2026-06-19 00:00:00+00')
            """,
            typeId, payload.SchemaJson, payload.WorkflowJson, payload.CreditRulesJson, payload.DisplayFieldsJson, ownerUserId);

        return typeId;
    }

    private static Task<int> InsertActivityAsync(NpgsqlConnection connection, int activityTypeId, string state, string creator)
        => InsertAsync(connection,
            """
            INSERT INTO "Activities"
                ("ActivityTypeId", "SchemaVersion", "SubjectUserId", "CreatedByUserId", "CurrentState", "DataJson",
                 "CreatedOn", "UpdatedOn", "ObservedOn", "ObservedOnSource")
            VALUES ($1, 1, $2, $3, $4, '{}'::jsonb,
                    TIMESTAMPTZ '2026-09-20 08:00:00+00', TIMESTAMPTZ '2026-09-20 08:00:00+00', DATE '2026-09-20', 0)
            RETURNING "Id"
            """,
            activityTypeId, TraineeId, creator, state);

    private static Task<int> InsertCreateTransitionAsync(NpgsqlConnection connection, int activityId)
        => ExecuteAsync(connection,
            """
            INSERT INTO "ActivityTransitions"
                ("ActivityId", "FromState", "ToState", "TransitionKey", "ActorUserId", "OccurredOn", "SnapshotJson")
            VALUES ($1, 'draft', 'draft', 'create', $2, TIMESTAMPTZ '2026-09-20 08:00:00+00', '{}'::jsonb)
            """,
            activityId, TraineeId);

    // ---- reads ---------------------------------------------------------------------------------------------------------

    private Task<bool> TableExistsAsync(string schema, string table)
        => ScalarAsync<bool>(schema, $"""
            SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = current_schema() AND table_name = '{table}')
            """);

    private Task<bool> ColumnExistsAsync(string schema, string table, string column)
        => ScalarAsync<bool>(schema, $"""
            SELECT EXISTS (
                SELECT 1 FROM information_schema.columns
                WHERE table_schema = current_schema() AND table_name = '{table}' AND column_name = '{column}')
            """);

    // ---- schema lifecycle (as EvidenceEpaMigrationPostgresTests) --------------------------------------------------------

    private async Task MigrateToLatestAsync(string schema)
    {
        await using var db = NewContext(schema);
        await db.Database.MigrateAsync();
        (await db.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
    }

    private ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(SchemaConnectionString(schema)).Options);

    private string SchemaConnectionString(string schema)
        => new NpgsqlConnectionStringBuilder(_baseConnectionString) { SearchPath = schema, Pooling = false }.ConnectionString;

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

    private async Task<T> ScalarAsync<T>(string schema, string sql)
    {
        await using var connection = await OpenAsync(schema);
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

    private async Task<NpgsqlConnection> OpenAsync(string schema)
    {
        var connection = new NpgsqlConnection(SchemaConnectionString(schema));
        await connection.OpenAsync();
        return connection;
    }

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

    /// <summary>The same resolution order as <c>WbaToolAllowListPostgresTests</c>.</summary>
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
}
