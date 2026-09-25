namespace Wombat.Integration.Tests.TestSupport;

/// <summary>
/// The schemas one test creates, each registered before it exists, so that no failure part-way can leak one (T241).
/// </summary>
/// <remarks>
/// A test class holds one in a field. xUnit makes a new instance for each test, so it holds that test's schemas only.
/// The class calls <see cref="DropAllAsync" /> from <c>DisposeAsync</c>, and often from a <c>finally</c> as well. A
/// second call drops nothing. Schemas are created in the tests, never in <c>InitializeAsync</c>: xUnit 2 does not call
/// <c>DisposeAsync</c> after a failed <c>InitializeAsync</c>, so a schema created there could leak.
/// </remarks>
internal sealed class TestSchemas
{
    private readonly List<string> _schemas = [];
    private readonly Lock _lock = new();

    /// <summary>A new, empty schema, registered for dropping before it is created.</summary>
    public async Task<string> CreateAsync()
    {
        var schema = TestDatabase.NewSchemaName();
        lock (_lock)
        {
            _schemas.Add(schema);
        }

        await TestDatabase.CreateSchemaAsync(schema);
        return schema;
    }

    /// <summary>Drops every schema this test created, on one admin connection, and forgets each once it is dropped.</summary>
    public async Task DropAllAsync()
    {
        List<string> schemas;
        lock (_lock)
        {
            schemas = [.. _schemas];
        }

        if (schemas.Count == 0)
        {
            return;
        }

        await using var connection = await TestDatabase.OpenAdminConnectionAsync();
        foreach (var schema in schemas)
        {
            await TestDatabase.DropSchemaAsync(connection, schema);
            lock (_lock)
            {
                _schemas.Remove(schema);
            }
        }
    }
}
