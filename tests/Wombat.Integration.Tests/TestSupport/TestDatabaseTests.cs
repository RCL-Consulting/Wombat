using System.Reflection;
using System.Reflection.Emit;
using FluentAssertions;
using Npgsql;

namespace Wombat.Integration.Tests.TestSupport;

/// <summary>
/// T241: the suite reaches its server only through <see cref="TestDatabase" />, no connection it hands out outlives its
/// use (an admin connection) or its schema (a schema connection), and every <see cref="Catalog" /> helper keeps T227's
/// rule.
/// </summary>
/// <remarks>
/// <para>
/// Before T241 each of 45 classes carried its own copy of the server lookup and opened its schema-admin connections
/// pooled. A new class starts as a copy of an old one, so the scan below keeps the copy from coming back.
/// </para>
/// <para>
/// The scan reads each method's IL, not the source. It also sees the state machines of async methods and the closures
/// of lambdas, which the compiler makes nested types, and a <c>const string</c>, which is compiled into its caller as the
/// literal itself.
/// </para>
/// </remarks>
public sealed class TestDatabaseTests : IAsyncLifetime
{
    private const string LoadsASetting = "loads the server setting";
    private const string DeclaresASetting = "declares the server setting";
    private const string SetsPooling = "sets NpgsqlConnectionStringBuilder.Pooling";
    private const string BuildsAConnectionString = "builds an NpgsqlConnectionStringBuilder";

    /// <summary>
    /// How long a closed connection may take to leave <c>pg_stat_activity</c>. Npgsql would keep an idle pooled one for
    /// minutes.
    /// </summary>
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(10);

    private static readonly Dictionary<short, OpCode> OpCodesByValue = typeof(OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Select(field => (OpCode)field.GetValue(null)!)
        .ToDictionary(code => code.Value);

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public void OnlyTestDatabase_ChoosesTheServer_OrBuildsAConnectionString()
    {
        var offenders = Findings()
            .Where(finding => finding.TopLevelType != typeof(TestDatabase))
            .Select(finding => $"{finding.Where} {finding.What}")
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

        // Every offender is named in the reason, because the assertion's own message shows only the first.
        offenders.Should().BeEmpty(
            "the server is chosen in one place, so one variable points the whole suite at another server, and every " +
            "connection string comes from TestDatabase, whose schema connections are closed when their schema is dropped " +
            "(T241). Use TestDatabase.SchemaConnectionString, OpenSchemaConnectionAsync or OpenAdminConnectionAsync, and " +
            "TestSchemas. Found: {0}",
            string.Join("; ", offenders));
    }

    [Theory]
    [InlineData(LoadsASetting, 3)]
    [InlineData(DeclaresASetting, 3)]
    [InlineData(SetsPooling, 1)]
    [InlineData(BuildsAConnectionString, 1)]
    public void TheScan_StillSeesTestDatabaseDoEachOfThem(string what, int times)
    {
        // The guard for the test above: if a part of the scan found nothing anywhere, that test would pass for the wrong
        // reason. TestDatabase declares and loads all three settings, and builds both connection strings, setting pooling
        // on each.
        Findings()
            .Where(finding => finding.TopLevelType == typeof(TestDatabase) && finding.What.StartsWith(what, StringComparison.Ordinal))
            .Select(finding => finding.What)
            .Distinct(StringComparer.Ordinal)
            .Should().HaveCount(times);
    }

    [Fact]
    public async Task AnAdminConnection_IsClosedOnTheServer_WhenItIsDisposed()
    {
        var backend = await BackendOfAsync(await TestDatabase.OpenAdminConnectionAsync());

        (await StillConnectedAsync(backend)).Should().BeFalse(
            "an admin connection is unpooled, so disposing it closes it on the server; a pooled one stays open, idle, " +
            "and many of them across a parallel run can pass max_connections (53300)");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("t241-named")]
    public async Task ASchemaConnection_IsReusedWhileItsSchemaExists_AndClosedOnTheServer_WhenTheSchemaIsDropped(string? applicationName)
    {
        var schema = await _schemas.CreateAsync();

        var first = await BackendOfAsync(await OpenAsync(TestDatabase.SchemaConnectionString(schema, applicationName)));
        var second = await BackendOfAsync(await OpenAsync(TestDatabase.SchemaConnectionString(schema, applicationName)));

        second.Should().Be(
            first,
            "a schema's connections are pooled, so the next open reuses the server process the last one left idle; an " +
            "unpooled run closes thousands, and each holds a local port for two minutes");

        await _schemas.DropAllAsync();

        (await StillConnectedAsync(first)).Should().BeFalse(
            "dropping the schema clears every pool it was handed out under, so no idle connection outlives it");
    }

    [Fact]
    public async Task ASchemaConnection_StillOpenWhenTheSchemaIsDropped_IsClosedOnTheServer_WhenItIsDisposed()
    {
        var schema = await _schemas.CreateAsync();
        var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        Backend backend;
        await using (connection)
        {
            backend = await BackendOfAsync(connection, disposeIt: false);

            await _schemas.DropAllAsync();
        }

        (await StillConnectedAsync(backend)).Should().BeFalse(
            "a connection that was in use when its pool was cleared is closed when it is disposed, not returned to the pool");
    }

    [Fact]
    public async Task TestSchemas_DropsEverySchemaItCreated_AndForgetsEachOnceItIsDropped()
    {
        var schemas = new TestSchemas();
        var first = await schemas.CreateAsync();
        var second = await schemas.CreateAsync();
        try
        {
            (await SchemasExistingAsync(first, second)).Should().Equal([first, second], "guard: both were created");

            await schemas.DropAllAsync();

            (await SchemasExistingAsync(first, second)).Should().BeEmpty();

            // A schema of the same name made again, not through the instance: a second drop that had not forgotten the
            // first one would drop it now. DROP SCHEMA IF EXISTS on a schema that is gone would pass either way.
            await TestDatabase.CreateSchemaAsync(first);
            await schemas.DropAllAsync();

            (await SchemasExistingAsync(first, second)).Should().Equal(
                [first], "the first drop forgot every schema it dropped, so the second has nothing to drop");
        }
        finally
        {
            // Not through the instance under test, which may be what failed.
            await TestDatabase.DropSchemaAsync(first);
            await TestDatabase.DropSchemaAsync(second);
        }
    }

    [Theory]
    [InlineData("wombat_not_a_test_schema")]
    [InlineData("it_not_32_hex_digits")]
    [InlineData("it_0123456789abcdef0123456789abcdef\" CASCADE; --")]
    [InlineData("it_0123456789abcdef0123456789abcdef\n")] // a well-formed name and a newline, which $ would let through
    [InlineData("it_0123456789ABCDEF0123456789ABCDEF")] // Guid.ToString("N") is lower case
    public async Task DroppingASchemaTheSuiteCouldNotHaveNamed_IsRefused_BeforeAnySqlIsSent(string schema)
    {
        // The name is spliced into DROP SCHEMA as an identifier, so only a name NewSchemaName could have made is dropped.
        var drop = () => TestDatabase.DropSchemaAsync(schema);

        await drop.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task EveryCatalogHelper_WhileAnotherSchemaIsBeingDropped_ReadsOnlyItsOwnSchema_AndWaitsOnNothing()
    {
        // T227's moment, for every helper: another test's DisposeAsync has dropped its schema, whose table, columns and
        // index names match these, and has not committed. The drop holds locks on all of them.
        var own = await _schemas.CreateAsync();
        var other = await _schemas.CreateAsync();
        await CreateProbeTableAsync(own, "Own");
        await CreateProbeTableAsync(other, "Other");

        await using var dropper = await TestDatabase.OpenAdminConnectionAsync();
        await using var drop = await dropper.BeginTransactionAsync();
        await ExecuteAsync(dropper, $"DROP SCHEMA \"{other}\" CASCADE", drop);

        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(own);
        // A read that touches the other schema's objects now waits on the drop, and the lock timeout turns the wait into
        // a failure. join_collapse_limit = 1 keeps a view's joins in their written order, which for pg_indexes applies
        // the schema filter last: the order under which T227's old query failed.
        await ExecuteAsync(connection, "SET lock_timeout = '3s'");
        await ExecuteAsync(connection, "SET join_collapse_limit = 1");

        (await Catalog.TableExistsAsync(connection, "Probe")).Should().BeTrue();
        (await Catalog.TableExistsAsync(connection, "Missing")).Should().BeFalse();
        (await Catalog.ColumnExistsAsync(connection, "Probe", "Name")).Should().BeTrue();
        (await Catalog.ColumnExistsAsync(connection, "Probe", "Missing")).Should().BeFalse();
        (await Catalog.ColumnTypeAsync(connection, "Probe", "Name")).Should().Be("character varying(16)");
        (await Catalog.ColumnTypeAsync(connection, "Probe", "Missing")).Should().BeNull();
        (await Catalog.IndexNamesAsync(connection, "Probe")).Should().Equal("IX_Own_Label", "PK_Own", "UX_Own_Name");
        (await Catalog.IndexIsUniqueAsync(connection, "UX_Own_Name")).Should().BeTrue();
        (await Catalog.IndexIsUniqueAsync(connection, "IX_Own_Label")).Should().BeFalse();
        (await Catalog.UniqueIndexesAsync(connection, "Probe")).Should().Equal(
            ("PK_Own", $"CREATE UNIQUE INDEX \"PK_Own\" ON {own}.\"Probe\" USING btree (\"Id\")"),
            ("UX_Own_Name", $"CREATE UNIQUE INDEX \"UX_Own_Name\" ON {own}.\"Probe\" USING btree (\"Name\")"));

        // Nothing is committed: the other schema stays, and DisposeAsync drops it with this one.
        await drop.RollbackAsync();
    }

    // ─── The connections ────────────────────────────────────────────────────

    private sealed record Backend(int Pid, DateTime Started);

    /// <summary>Which server process the connection is. Then disposes the connection, unless told not to.</summary>
    private static async Task<Backend> BackendOfAsync(NpgsqlConnection connection, bool disposeIt = true)
    {
        try
        {
            await using var command = new NpgsqlCommand(
                "SELECT pid, backend_start FROM pg_stat_activity WHERE pid = pg_backend_pid()", connection);
            await using var reader = await command.ExecuteReaderAsync();
            (await reader.ReadAsync()).Should().BeTrue("guard: the server lists its own session");
            return new Backend(reader.GetInt32(0), reader.GetDateTime(1));
        }
        finally
        {
            if (disposeIt)
            {
                await connection.DisposeAsync();
            }
        }
    }

    private static async Task<NpgsqlConnection> OpenAsync(string connectionString)
    {
        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        return connection;
    }

    /// <summary>Whether that server process is still connected after <see cref="Patience" />. A closed one leaves at once.</summary>
    private static async Task<bool> StillConnectedAsync(Backend backend)
    {
        await using var watcher = await TestDatabase.OpenAdminConnectionAsync();
        var deadline = DateTime.UtcNow + Patience;
        while (true)
        {
            await using var command = new NpgsqlCommand(
                "SELECT count(*) FROM pg_stat_activity WHERE pid = $1 AND backend_start = $2", watcher);
            command.Parameters.Add(new NpgsqlParameter { Value = backend.Pid });
            command.Parameters.Add(new NpgsqlParameter { Value = backend.Started });
            if ((long)(await command.ExecuteScalarAsync())! == 0)
            {
                return false;
            }

            if (DateTime.UtcNow > deadline)
            {
                return true;
            }

            await Task.Delay(50);
        }
    }

    private static async Task<List<string>> SchemasExistingAsync(params string[] schemas)
    {
        await using var connection = await TestDatabase.OpenAdminConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT nspname::text FROM pg_namespace WHERE nspname = ANY($1) ORDER BY array_position($1, nspname::text)", connection);
        command.Parameters.Add(new NpgsqlParameter { Value = schemas });
        await using var reader = await command.ExecuteReaderAsync();

        var existing = new List<string>();
        while (await reader.ReadAsync())
        {
            existing.Add(reader.GetString(0));
        }

        return existing;
    }

    /// <summary>A table with a primary key, a unique index and a plain one, each named for its schema.</summary>
    private static async Task CreateProbeTableAsync(string schema, string owner)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        await ExecuteAsync(connection, $"""
            CREATE TABLE "Probe" ("Id" integer CONSTRAINT "PK_{owner}" PRIMARY KEY, "Name" varchar(16) NOT NULL, "Label" text);
            CREATE UNIQUE INDEX "UX_{owner}_Name" ON "Probe" ("Name");
            CREATE INDEX "IX_{owner}_Label" ON "Probe" ("Label");
            """);
    }

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql, NpgsqlTransaction? transaction = null)
    {
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        await command.ExecuteNonQueryAsync();
    }

    // ─── The scan ───────────────────────────────────────────────────────────

    private sealed record Finding(Type TopLevelType, string Where, string What);

    /// <summary>
    /// Everywhere in this assembly that names one of the values that choose the server, builds a connection string, or
    /// turns pooling on or off.
    /// </summary>
    private static IEnumerable<Finding> Findings()
    {
        var settings = TestDatabase.ServerSettings;

        foreach (var type in typeof(TestDatabase).Assembly.GetTypes())
        {
            var topLevel = type;
            while (topLevel.DeclaringType is not null)
            {
                topLevel = topLevel.DeclaringType;
            }

            foreach (var field in type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (field is { IsLiteral: true } && field.GetRawConstantValue() is string value && settings.TryGetValue(value, out var setting))
                {
                    yield return new Finding(topLevel, $"{type.FullName}.{field.Name}", $"{DeclaresASetting}: {setting}");
                }
            }

            const BindingFlags members = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var methods = type.GetMethods(members).Cast<MethodBase>().Concat(type.GetConstructors(members));
            foreach (var method in methods)
            {
                foreach (var (code, operand) in Instructions(method))
                {
                    if (code == OpCodes.Ldstr && settings.TryGetValue(method.Module.ResolveString(operand), out var setting))
                    {
                        yield return new Finding(topLevel, $"{type.FullName}.{method.Name}", $"{LoadsASetting}: {setting}");
                    }

                    if ((code == OpCodes.Call || code == OpCodes.Callvirt)
                        && ResolveMethod(method, operand) is { Name: "set_Pooling" } called
                        && called.DeclaringType == typeof(NpgsqlConnectionStringBuilder))
                    {
                        yield return new Finding(topLevel, $"{type.FullName}.{method.Name}", SetsPooling);
                    }

                    // A string built from one TestDatabase handed out is a pool TestDatabase does not know of, so dropping
                    // the schema would leave its idle connections open.
                    if (code == OpCodes.Newobj
                        && ResolveMethod(method, operand) is ConstructorInfo constructor
                        && constructor.DeclaringType == typeof(NpgsqlConnectionStringBuilder))
                    {
                        yield return new Finding(topLevel, $"{type.FullName}.{method.Name}", BuildsAConnectionString);
                    }
                }
            }
        }
    }

    private static MethodBase? ResolveMethod(MethodBase caller, int token)
    {
        var typeArguments = caller.DeclaringType is { IsGenericType: true } declaring ? declaring.GetGenericArguments() : null;
        var methodArguments = caller.IsGenericMethod ? caller.GetGenericArguments() : null;
        return caller.Module.ResolveMethod(token, typeArguments, methodArguments);
    }

    /// <summary>Each instruction of the method's body, with its operand when that is a metadata token or a number.</summary>
    private static IEnumerable<(OpCode Code, int Operand)> Instructions(MethodBase method)
    {
        var il = method.GetMethodBody()?.GetILAsByteArray();
        if (il is null)
        {
            yield break;
        }

        var position = 0;
        while (position < il.Length)
        {
            short value = il[position++];
            if (value == 0xFE)
            {
                value = unchecked((short)(0xFE00 | il[position++]));
            }

            var code = OpCodesByValue[value];
            var size = code.OperandType switch
            {
                OperandType.InlineNone => 0,
                OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
                OperandType.InlineVar => 2,
                OperandType.InlineI8 or OperandType.InlineR => 8,
                OperandType.InlineSwitch => 4 + (4 * BitConverter.ToInt32(il, position)),
                _ => 4
            };

            yield return (code, size == 4 ? BitConverter.ToInt32(il, position) : 0);
            position += size;
        }
    }
}
