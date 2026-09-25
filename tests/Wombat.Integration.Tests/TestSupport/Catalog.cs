using FluentAssertions;
using Npgsql;

namespace Wombat.Integration.Tests.TestSupport;

/// <summary>
/// What a test schema holds, read from the system catalogs: its tables, columns and indexes (T227, T241).
/// </summary>
/// <remarks>
/// <para>
/// Every class runs on the one server, and other tests drop their schemas while these queries run. A drop locks every
/// table and index in its schema until it commits. <c>pg_get_indexdef</c>, <c>pg_get_constraintdef</c> and
/// <c>pg_get_expr</c> wait on that lock when they render one of those objects, and then fail, with 55P03 under a lock
/// timeout or XX000 "could not open relation with OID" once the drop commits. That made the T130 migration test fail
/// intermittently (T227).
/// </para>
/// <para>
/// So every query here finds its table or index by OID: <c>to_regclass</c> looks up one quoted name on the connection's
/// search path, which is the test's schema, and takes no lock. It filters only on plain catalog columns and renders
/// only in the select list, so a rendering function runs only on the rows those filters kept. ARCHITECTURE.md
/// § Testing states the rule for any catalog query in the suite. <c>TestDatabaseTests</c> runs every helper while
/// another schema's drop is held open.
/// </para>
/// <para>
/// Each helper has two forms. One takes the test's schema and opens its own connection. The other takes a connection
/// whose search path is the schema, for a test that has set something on it first, such as a lock timeout.
/// </para>
/// </remarks>
internal static class Catalog
{
    /// <summary>Whether the schema holds a table of that name.</summary>
    public static Task<bool> TableExistsAsync(string schema, string table)
        => WithSchemaAsync(schema, connection => TableExistsAsync(connection, table));

    public static async Task<bool> TableExistsAsync(NpgsqlConnection connection, string table)
        => (bool)(await ScalarAsync(
            connection,
            "SELECT EXISTS (SELECT 1 FROM pg_class WHERE oid = to_regclass(quote_ident($1)) AND relkind IN ('r', 'p'))",
            table))!;

    /// <summary>Whether the schema's table has a column of that name.</summary>
    public static Task<bool> ColumnExistsAsync(string schema, string table, string column)
        => WithSchemaAsync(schema, connection => ColumnExistsAsync(connection, table, column));

    public static async Task<bool> ColumnExistsAsync(NpgsqlConnection connection, string table, string column)
        => (bool)(await ScalarAsync(
            connection,
            """
            SELECT EXISTS (
                SELECT 1 FROM pg_attribute
                WHERE attrelid = to_regclass(quote_ident($1)) AND attname = $2 AND attnum > 0 AND NOT attisdropped)
            """,
            table,
            column))!;

    /// <summary>
    /// A column's declared type as PostgreSQL writes it, such as <c>character varying(16)</c>, or null when the table
    /// has no such column.
    /// </summary>
    public static Task<string?> ColumnTypeAsync(string schema, string table, string column)
        => WithSchemaAsync(schema, connection => ColumnTypeAsync(connection, table, column));

    public static async Task<string?> ColumnTypeAsync(NpgsqlConnection connection, string table, string column)
        => (string?)await ScalarAsync(
            connection,
            """
            SELECT format_type(atttypid, atttypmod) FROM pg_attribute
            WHERE attrelid = to_regclass(quote_ident($1)) AND attname = $2 AND attnum > 0 AND NOT attisdropped
            """,
            table,
            column);

    /// <summary>The names of every index on the schema's table, the primary key's included, in name order.</summary>
    public static Task<List<string>> IndexNamesAsync(string schema, string table)
        => WithSchemaAsync(schema, connection => IndexNamesAsync(connection, table));

    public static async Task<List<string>> IndexNamesAsync(NpgsqlConnection connection, string table)
    {
        await using var command = Command(
            connection,
            """
            SELECT idx.relname
            FROM pg_index ix
            JOIN pg_class idx ON idx.oid = ix.indexrelid
            WHERE ix.indrelid = to_regclass(quote_ident($1))
            ORDER BY idx.relname
            """,
            table);
        await using var reader = await command.ExecuteReaderAsync();

        var names = new List<string>();
        while (await reader.ReadAsync())
        {
            names.Add(reader.GetString(0));
        }

        return names;
    }

    /// <summary>Whether the schema's index of that name is unique. Fails the test if there is no such index.</summary>
    public static Task<bool> IndexIsUniqueAsync(string schema, string index)
        => WithSchemaAsync(schema, connection => IndexIsUniqueAsync(connection, index));

    public static async Task<bool> IndexIsUniqueAsync(NpgsqlConnection connection, string index)
    {
        var unique = await ScalarAsync(
            connection,
            "SELECT indisunique FROM pg_index WHERE indexrelid = to_regclass(quote_ident($1))",
            index);

        unique.Should().NotBeNull($"guard: the index {index} exists in the schema");
        return (bool)unique!;
    }

    /// <summary>
    /// The unique indexes on the schema's table, the primary key's included, each with its definition, in name order.
    /// </summary>
    /// <remarks>
    /// T227's query. The one it replaced read <c>pg_indexes</c> and filtered on
    /// <c>indexdef LIKE 'CREATE UNIQUE INDEX%'</c>. SQL fixes no order for a WHERE clause. Under load the planner tested
    /// that predicate on the indexes of every table of the same name in every schema, and applied the schema filter last,
    /// so it rendered indexes that other tests were dropping.
    /// </remarks>
    public static Task<List<(string Name, string Definition)>> UniqueIndexesAsync(string schema, string table)
        => WithSchemaAsync(schema, connection => UniqueIndexesAsync(connection, table));

    public static async Task<List<(string Name, string Definition)>> UniqueIndexesAsync(NpgsqlConnection connection, string table)
    {
        await using var command = Command(
            connection,
            """
            SELECT idx.relname, pg_get_indexdef(idx.oid)
            FROM pg_index ix
            JOIN pg_class idx ON idx.oid = ix.indexrelid
            WHERE ix.indrelid = to_regclass(quote_ident($1)) AND ix.indisunique
            ORDER BY idx.relname
            """,
            table);
        await using var reader = await command.ExecuteReaderAsync();

        var indexes = new List<(string Name, string Definition)>();
        while (await reader.ReadAsync())
        {
            indexes.Add((reader.GetString(0), reader.GetString(1)));
        }

        return indexes;
    }

    private static async Task<T> WithSchemaAsync<T>(string schema, Func<NpgsqlConnection, Task<T>> query)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        return await query(connection);
    }

    private static async Task<object?> ScalarAsync(NpgsqlConnection connection, string sql, params string[] values)
    {
        await using var command = Command(connection, sql, values);
        var value = await command.ExecuteScalarAsync();
        return value is DBNull ? null : value;
    }

    /// <summary>Positional parameters ($1, $2, …), so no name is ever spliced into SQL text.</summary>
    private static NpgsqlCommand Command(NpgsqlConnection connection, string sql, params string[] values)
    {
        var command = new NpgsqlCommand(sql, connection);
        foreach (var value in values)
        {
            command.Parameters.Add(new NpgsqlParameter { Value = value });
        }

        return command;
    }
}
