using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;
using Npgsql;

namespace Wombat.Integration.Tests.TestSupport;

/// <summary>
/// The PostgreSQL server the integration suite runs on, and every connection string a test uses to reach it (T241).
/// </summary>
/// <remarks>
/// <para>
/// Which server: <c>WOMBAT_TEST_CONNECTION</c>, else <c>Wombat.Web</c>'s user secrets
/// (<c>ConnectionStrings:DefaultConnection</c>), else a local default. Nothing else in the suite reads any of them, so
/// one variable points the whole run at another server, such as a <c>postgres:18</c> container (T243).
/// <c>TestDatabaseTests</c> fails if another type reads them.
/// </para>
/// <para>
/// No connection outlives the schema it was opened for. xUnit runs the classes in parallel, in one process, against one
/// server. When a test disposes a pooled connection, Npgsql keeps it open in the pool, idle, and on its own closes it only
/// when it prunes the pool, minutes later. Those idle connections and the tests' open ones together can pass the server's
/// <c>max_connections</c>, and the next open fails with 53300 "too many clients". So the admin connection, a few per
/// test, is unpooled: it is closed on the server when it is disposed.
/// </para>
/// <para>
/// A schema's connections are pooled, one pool for each string <see cref="SchemaConnectionString" /> hands out, and
/// dropping the schema clears its pools, which closes every idle connection in them. Every EF Core save or query opens
/// a connection. Unpooled, a run opened about 15,000, and on Windows each one closed holds a local TCP port for about two
/// minutes. Against a server on the same machine, such as a <c>postgres:18</c> container, that used up the 16,384 ports
/// Windows has, and tests failed with "Only one usage of each socket address". Pooled, a run opens about 700 (T241).
/// </para>
/// </remarks>
internal static partial class TestDatabase
{
    private const string ConnectionVariable = "WOMBAT_TEST_CONNECTION";

    /// <summary><c>Wombat.Web</c>'s <c>UserSecretsId</c>.</summary>
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";

    private const string LocalDefault = "Host=localhost;Port=5432;Database=wombat;Username=postgres;Password=postgres";

    private static readonly Lazy<string> Admin = new(
        () => new NpgsqlConnectionStringBuilder(ResolveServerConnectionString()) { Pooling = false }.ConnectionString);

    /// <summary>Every connection string handed out for each schema, so that dropping the schema can clear its pools.</summary>
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> SchemaPools = new(StringComparer.Ordinal);

    /// <summary>
    /// The values that choose the server, each with what it is, for <c>TestDatabaseTests</c> to look for in every other
    /// type. Kept here so that the test that looks for them does not contain them itself.
    /// </summary>
    internal static IReadOnlyDictionary<string, string> ServerSettings { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        [ConnectionVariable] = "the environment variable's name",
        [WombatWebUserSecretsId] = "Wombat.Web's UserSecretsId",
        [LocalDefault] = "the local default connection string"
    };

    /// <summary>
    /// The server, outside any schema: for creating and dropping schemas, and for reading <c>pg_stat_activity</c>.
    /// </summary>
    public static string AdminConnectionString => Admin.Value;

    /// <summary>
    /// The schema and nothing else on the search path, so an unqualified name can only ever resolve inside it. Pooled until
    /// the schema is dropped.
    /// </summary>
    /// <param name="applicationName">
    /// What the connection reports itself as to <c>pg_stat_activity</c>, for a test that looks for it there. Each name is
    /// a pool of its own, and dropping the schema clears it too.
    /// </param>
    public static string SchemaConnectionString(string schema, string? applicationName = null)
    {
        var connectionString = new NpgsqlConnectionStringBuilder(AdminConnectionString)
        {
            SearchPath = schema,
            ApplicationName = applicationName,
            Pooling = true
        }.ConnectionString;

        SchemaPools.GetOrAdd(schema, _ => new ConcurrentDictionary<string, byte>(StringComparer.Ordinal)).TryAdd(connectionString, 0);
        return connectionString;
    }

    public static Task<NpgsqlConnection> OpenAdminConnectionAsync() => OpenAsync(AdminConnectionString);

    public static Task<NpgsqlConnection> OpenSchemaConnectionAsync(string schema) => OpenAsync(SchemaConnectionString(schema));

    /// <summary>A name for a new test schema. <see cref="TestSchemas" /> registers it before it creates it.</summary>
    public static string NewSchemaName() => $"it_{Guid.NewGuid():N}";

    public static async Task CreateSchemaAsync(string schema)
    {
        var create = $"CREATE SCHEMA \"{TestSchemaName(schema)}\"";
        await using var connection = await OpenAdminConnectionAsync();
        await ExecuteAsync(connection, create);
    }

    /// <summary>
    /// Drops the schema and everything in it, and closes the idle connections in its pools. A schema that does not exist is
    /// not an error.
    /// </summary>
    public static async Task DropSchemaAsync(string schema)
    {
        var drop = DropStatement(schema);
        try
        {
            await using var connection = await OpenAdminConnectionAsync();
            await ExecuteAsync(connection, drop);
        }
        finally
        {
            ClearSchemaPools(schema);
        }
    }

    /// <summary>The same, on an admin connection the caller already holds.</summary>
    public static async Task DropSchemaAsync(NpgsqlConnection connection, string schema)
    {
        var drop = DropStatement(schema);
        try
        {
            await ExecuteAsync(connection, drop);
        }
        finally
        {
            ClearSchemaPools(schema);
        }
    }

    private static string DropStatement(string schema) => $"DROP SCHEMA IF EXISTS \"{TestSchemaName(schema)}\" CASCADE";

    /// <summary>
    /// Closes every idle connection in the schema's pools now. One still open is closed when it is disposed, rather than
    /// returned to the pool.
    /// </summary>
    private static void ClearSchemaPools(string schema)
    {
        if (!SchemaPools.TryRemove(schema, out var connectionStrings))
        {
            return;
        }

        foreach (var connectionString in connectionStrings.Keys)
        {
            using var key = new NpgsqlConnection(connectionString);
            NpgsqlConnection.ClearPool(key);
        }
    }

    private static async Task<NpgsqlConnection> OpenAsync(string connectionString)
    {
        var connection = new NpgsqlConnection(connectionString);
        try
        {
            await connection.OpenAsync();
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }

        return connection;
    }

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Only a name <see cref="NewSchemaName" /> could have made. The name is spliced into SQL as an identifier, and the
    /// suite never creates or drops a schema it did not name itself.
    /// </summary>
    private static string TestSchemaName(string schema)
        => TestSchemaPattern().IsMatch(schema)
            ? schema
            : throw new ArgumentException($"'{schema}' is not a test schema name (it_ and 32 hex digits).", nameof(schema));

    /// <remarks><c>\z</c>, not <c>$</c>: .NET's <c>$</c> also matches before a final newline.</remarks>
    [GeneratedRegex(@"\Ait_[0-9a-f]{32}\z", RegexOptions.CultureInvariant)]
    private static partial Regex TestSchemaPattern();

    private static string ResolveServerConnectionString()
    {
        var environmentConnectionString = Environment.GetEnvironmentVariable(ConnectionVariable);
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

        return LocalDefault;
    }
}
