using System.Globalization;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.CommitteeDecisions;

/// <summary>
/// T182 on a real PostgreSQL server: every decision panel carries the institution that runs it.
/// </summary>
/// <remarks>
/// <para>
/// A panel reviews only trainees at its own institution, so a panel without one could review nobody. Until T182 only an
/// InstitutionalAdmin's panels were stamped. The migration makes the column required and a foreign key; it does not
/// guess an institution for an unstamped panel, it stops and names it.
/// </para>
/// <para>
/// The unit suites run on EF InMemory, which never runs a migration or enforces a constraint. The schema helpers follow
/// <c>DemoCurriculumScalePinMigrationPostgresTests</c>: each test runs on a schema of its own, dropped in a
/// <c>finally</c>, with <see cref="DisposeAsync" /> as a backstop.
/// </para>
/// </remarks>
public sealed class DecisionPanelInstitutionMigrationPostgresTests : IAsyncLifetime
{
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";

    private const string T182Migration = "20260924145621_T182_DecisionPanelInstitutionRequired";
    private const string LastMigrationBeforeT182 = "20260924140337_T174_PinDemoCurriculumItemScale";

    private readonly List<string> _schemas = [];
    private string _baseConnectionString = null!;

    public Task InitializeAsync()
    {
        _baseConnectionString = ResolveBaseConnectionString();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => DropSchemasAsync();

    [Fact]
    public async Task Migration_WithEveryPanelStamped_MakesTheInstitutionRequired_AndAForeignKey()
    {
        try
        {
            var schema = await ArrangePreT182DatabaseAsync();
            int institutionId;
            await using (var connection = await OpenAsync(schema))
            {
                institutionId = await InsertInstitutionAsync(connection, "KGK");
                await InsertPanelAsync(connection, institutionId);
            }

            await using (var db = NewContext(schema))
            {
                await db.Database.MigrateAsync();
                (await db.Database.GetAppliedMigrationsAsync()).Should().Contain(T182Migration);
            }

            await using (var connection = await OpenAsync(schema))
            {
                var unstamped = async () => await InsertPanelAsync(connection, null);
                (await unstamped.Should().ThrowAsync<PostgresException>()).Which.SqlState
                    .Should().Be(PostgresErrorCodes.NotNullViolation);

                var nowhere = async () => await InsertPanelAsync(connection, institutionId + 1000);
                (await nowhere.Should().ThrowAsync<PostgresException>()).Which.SqlState
                    .Should().Be(PostgresErrorCodes.ForeignKeyViolation);

                var deleteTheInstitution = async () => await ExecuteAsync(
                    connection, """DELETE FROM "Institutions" WHERE "Id" = $1""", institutionId);
                (await deleteTheInstitution.Should().ThrowAsync<PostgresException>()).Which.SqlState
                    .Should().Be(PostgresErrorCodes.ForeignKeyViolation, "a panel's institution cannot vanish under it");
            }
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task Migration_WithAnUnstampedPanel_StopsAndNamesIt_RatherThanGuessOrDefault()
    {
        // The scaffold would have set every unstamped panel to institution 0, which then fails the foreign key with an
        // error that names neither the panel nor the fix.
        try
        {
            var schema = await ArrangePreT182DatabaseAsync();
            int unstampedPanel;
            await using (var connection = await OpenAsync(schema))
            {
                var institutionId = await InsertInstitutionAsync(connection, "KGK");
                await InsertPanelAsync(connection, institutionId);
                unstampedPanel = await InsertPanelAsync(connection, null);
            }

            await using (var db = NewContext(schema))
            {
                var migrate = () => db.Database.MigrateAsync();

                var refusal = (await migrate.Should().ThrowAsync<PostgresException>()).Which;
                refusal.MessageText.Should().Contain("T182").And.Contain(unstampedPanel.ToString(CultureInfo.InvariantCulture));
            }

            await using (var db = NewContext(schema))
            {
                (await db.Database.GetAppliedMigrationsAsync()).Should().NotContain(T182Migration);
            }

            await using (var connection = await OpenAsync(schema))
            {
                (await ScalarAsync<long>(connection, """SELECT COUNT(*) FROM "DecisionPanels" WHERE "InstitutionId" IS NULL"""))
                    .Should().Be(1, "nothing was stamped by guesswork");
            }
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    private async Task<string> ArrangePreT182DatabaseAsync()
    {
        var schema = await CreateSchemaAsync();

        await using var db = NewContext(schema);
        await db.GetService<IMigrator>().MigrateAsync(LastMigrationBeforeT182);

        (await db.Database.GetAppliedMigrationsAsync()).Last().Should().Be(LastMigrationBeforeT182);
        (await db.Database.GetPendingMigrationsAsync()).First().Should().Be(T182Migration);
        return schema;
    }

    private static Task<int> InsertInstitutionAsync(NpgsqlConnection connection, string shortCode)
        => InsertAsync(connection,
            """INSERT INTO "Institutions" ("Name", "ShortCode", "IsActive", "CreatedOn") VALUES ($1, $1, TRUE, TIMESTAMPTZ '2026-06-19 00:00:00+00') RETURNING "Id" """,
            shortCode);

    private static Task<int> InsertPanelAsync(NpgsqlConnection connection, int? institutionId)
        => InsertAsync(connection,
            """INSERT INTO "DecisionPanels" ("Name", "Scope", "InstitutionId", "SpecialityId", "CreatedOn") VALUES ('Annual review', 1, $1, NULL, TIMESTAMPTZ '2026-06-19 00:00:00+00') RETURNING "Id" """,
            (object?)institutionId ?? DBNull.Value);

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

    private static async Task<T> ScalarAsync<T>(NpgsqlConnection connection, string sql)
    {
        await using var command = Command(connection, sql, []);
        var value = await command.ExecuteScalarAsync();
        return (T)Convert.ChangeType(value!, typeof(T), CultureInfo.InvariantCulture);
    }

    /// <summary>Positional parameters ($1, $2, …), so no value is ever spliced into SQL text.</summary>
    private static NpgsqlCommand Command(NpgsqlConnection connection, string sql, object[] values)
    {
        var command = new NpgsqlCommand(sql, connection);
        foreach (var value in values)
        {
            command.Parameters.Add(value is DBNull
                ? new NpgsqlParameter { Value = value, NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Integer }
                : new NpgsqlParameter { Value = value });
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

    private ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(SchemaConnectionString(schema)).Options);

    /// <summary>The schema and nothing else on the search path, so an unqualified name can only ever resolve inside it.</summary>
    private string SchemaConnectionString(string schema)
        => new NpgsqlConnectionStringBuilder(_baseConnectionString)
        {
            SearchPath = schema,
            Pooling = false
        }.ConnectionString;

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
