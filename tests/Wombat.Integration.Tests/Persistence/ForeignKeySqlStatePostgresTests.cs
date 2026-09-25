using System.Globalization;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Wombat.Application.Common.Persistence;
using Xunit.Abstractions;

namespace Wombat.Integration.Tests.Persistence;

/// <summary>
/// Which SQLSTATE a real PostgreSQL server gives each kind of foreign-key refusal, and that
/// <see cref="PostgresErrors.IsForeignKeyViolation(string?)" /> recognises every one of them (T243).
/// </summary>
/// <remarks>
/// <para>
/// PostgreSQL 18 reports a delete refused by an <c>ON DELETE RESTRICT</c> key as <c>23001</c> (<c>restrict_violation</c>);
/// 16 and 17 report it as <c>23503</c>, as every version reports a missing referenced row and a <c>NO ACTION</c> key.
/// Measured on 2026-09-25 with this class: the dev server (16.10) and a <c>postgres:17</c> container (17.11) gave 23503
/// three times; a <c>postgres:18</c> container (18.6) gave 23001 for the RESTRICT delete. Production runs 18. So the code expected for the RESTRICT delete is chosen by the server's
/// own <c>server_version_num</c>, and the suite pins the fact on whichever server it is pointed at:
/// <c>WOMBAT_TEST_CONNECTION</c> runs it on another one (ARCHITECTURE.md § Testing).
/// </para>
/// <para>
/// Each test works in a schema of its own, dropped in a <c>finally</c> and again from <see cref="DisposeAsync" />, as the
/// other classes here do. Nothing is migrated: the tables are the smallest that carry each kind of key.
/// </para>
/// </remarks>
public sealed class ForeignKeySqlStatePostgresTests(ITestOutputHelper output) : IAsyncLifetime
{
    /// <summary>The first <c>server_version_num</c> that reports a RESTRICT refusal as <c>restrict_violation</c>.</summary>
    private const int FirstVersionWithRestrictViolation = 180000;

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public void TheHelpersCodes_AreNpgsqlsNamesForThem()
    {
        PostgresErrors.ForeignKeyViolation.Should().Be(PostgresErrorCodes.ForeignKeyViolation);
        PostgresErrors.RestrictViolation.Should().Be(PostgresErrorCodes.RestrictViolation);
    }

    [Fact]
    public async Task EachKindOfForeignKeyRefusal_GivesTheCodeItsServerVersionReports_AndTheHelperRecognisesEveryOne()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
            await ExecuteAsync(connection, """
                CREATE TABLE "Parents" ("Id" integer PRIMARY KEY);
                CREATE TABLE "RestrictChildren" (
                    "Id" integer PRIMARY KEY,
                    "ParentId" integer NOT NULL REFERENCES "Parents" ("Id") ON DELETE RESTRICT);
                CREATE TABLE "NoActionChildren" (
                    "Id" integer PRIMARY KEY,
                    "ParentId" integer NOT NULL REFERENCES "Parents" ("Id") ON DELETE NO ACTION);
                INSERT INTO "Parents" VALUES (1), (2);
                INSERT INTO "RestrictChildren" VALUES (1, 1);
                INSERT INTO "NoActionChildren" VALUES (1, 2);
                """);

            var version = await ServerVersionAsync(connection);
            var restrictDeleteState = version.Number >= FirstVersionWithRestrictViolation
                ? PostgresErrorCodes.RestrictViolation
                : PostgresErrorCodes.ForeignKeyViolation;

            var missingParent = await RefusalAsync(connection, """INSERT INTO "RestrictChildren" VALUES (2, 99)""");
            var noActionDelete = await RefusalAsync(connection, """DELETE FROM "Parents" WHERE "Id" = 2""");
            var restrictDelete = await RefusalAsync(connection, """DELETE FROM "Parents" WHERE "Id" = 1""");

            output.WriteLine($"PostgreSQL {version.Text} ({version.Number})");
            output.WriteLine($"  insert naming a missing parent: {missingParent.SqlState}");
            output.WriteLine($"  delete held by a NO ACTION key: {noActionDelete.SqlState}");
            output.WriteLine($"  delete held by a RESTRICT key:  {restrictDelete.SqlState}");

            missingParent.SqlState.Should().Be(PostgresErrorCodes.ForeignKeyViolation, "every version reports a missing row so");
            noActionDelete.SqlState.Should().Be(PostgresErrorCodes.ForeignKeyViolation, "every version reports NO ACTION so");
            restrictDelete.SqlState.Should().Be(
                restrictDeleteState,
                "PostgreSQL {0} reports a RESTRICT refusal as {1}", version.Text, restrictDeleteState);
            restrictDelete.ConstraintName.Should().Be("RestrictChildren_ParentId_fkey", "the code changed; the refusal did not");

            foreach (var refusal in new[] { missingParent, noActionDelete, restrictDelete })
            {
                PostgresErrors.IsForeignKeyViolation(refusal.SqlState).Should().BeTrue(refusal.MessageText);
                PostgresErrors.IsForeignKeyViolation(refusal).Should().BeTrue(refusal.MessageText);
                refusal.SqlState.Should().BeOneOf(PostgresErrors.ForeignKeyViolationStates);
            }

            (await ScalarAsync<long>(connection, """SELECT count(*) FROM "Parents" """))
                .Should().Be(2, "neither delete went through");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task ASaveChangesRefusedByDeleteBehaviorRestrict_IsRecognisedThroughItsDbUpdateException()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await using (var create = NewContext(schema))
            {
                var script = create.Database.GenerateCreateScript();
                script.Should().Contain("ON DELETE RESTRICT", "EF Core's DeleteBehavior.Restrict is a RESTRICT key on PostgreSQL");
                await create.Database.ExecuteSqlRawAsync(script);

                create.Parents.Add(new Parent { Id = 1 });
                create.Children.Add(new Child { Id = 1, ParentId = 1 });
                await create.SaveChangesAsync();
            }

            await using var context = NewContext(schema);
            context.Parents.Remove(await context.Parents.SingleAsync());

            var refused = await context.Invoking(async db => await db.SaveChangesAsync())
                .Should().ThrowAsync<DbUpdateException>();

            var inner = refused.Which.InnerException.Should().BeOfType<PostgresException>().Subject;
            output.WriteLine($"DbUpdateException wrapping {inner.SqlState}: {inner.MessageText}");
            PostgresErrors.IsForeignKeyViolation(refused.Which).Should().BeTrue(
                "a handler catching the refused save asks the helper, whichever code the server gave ({0})", inner.SqlState);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ---- the smallest model with a RESTRICT key -------------------------------------------------------------------------

    private sealed class Parent
    {
        public int Id { get; set; }
    }

    private sealed class Child
    {
        public int Id { get; set; }

        public int ParentId { get; set; }

        public Parent Parent { get; set; } = null!;
    }

    private sealed class RestrictContext(DbContextOptions<RestrictContext> options) : DbContext(options)
    {
        public DbSet<Parent> Parents => Set<Parent>();

        public DbSet<Child> Children => Set<Child>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent>().Property(parent => parent.Id).ValueGeneratedNever();
            modelBuilder.Entity<Child>().Property(child => child.Id).ValueGeneratedNever();
            modelBuilder.Entity<Child>()
                .HasOne(child => child.Parent)
                .WithMany()
                .HasForeignKey(child => child.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    private RestrictContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<RestrictContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema)).Options);

    // ---- SQL ------------------------------------------------------------------------------------------------------------

    private static async Task<(int Number, string Text)> ServerVersionAsync(NpgsqlConnection connection)
    {
        var number = int.Parse(await ScalarAsync<string>(connection, "SHOW server_version_num"), CultureInfo.InvariantCulture);
        var text = await ScalarAsync<string>(connection, "SHOW server_version");
        return (number, text);
    }

    private static async Task<PostgresException> RefusalAsync(NpgsqlConnection connection, string sql)
    {
        var act = async () => await ExecuteAsync(connection, sql);
        return (await act.Should().ThrowAsync<PostgresException>()).Which;
    }

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<T> ScalarAsync<T>(NpgsqlConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return (T)(await command.ExecuteScalarAsync())!;
    }
}
