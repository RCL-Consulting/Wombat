using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.MultiSourceFeedback;

/// <summary>
/// The T207 migration clears every respondent hash already stored, and leaves the rest of each invitation. (T207)
/// </summary>
/// <remarks>
/// <para>
/// Until T207 anonymising an invitation replaced the address with an unsalted SHA-256 of it, upper-cased, in
/// <c>MsfInvitations.RespondentEmailHash</c>. Nothing read it; anyone holding the invited addresses could hash each one
/// and find its row, and so the answers given on it. The column is dropped, not re-keyed: nothing needs to recognise a
/// respondent once the campaign has closed.
/// </para>
/// <para>
/// The migration is rehearsed on a schema stopped just before it and holding a hash in the shape the old code wrote.
/// That nothing the app does today leaves a trace (the close command, the auto-close job and the withdraw command, each
/// through MediatR and so through the audit pipeline) is
/// <c>MsfRespondEndpointFlowTests.ClosingByHand_AutoClosing_AndWithdrawing_ThroughTheApp_LeaveNoTraceOfARespondentsAddress</c>,
/// on the Api host. The schema helpers follow <c>SystemManagedMigrationPostgresTests</c>.
/// </para>
/// </remarks>
public sealed class MsfRespondentHashPostgresTests : IAsyncLifetime
{
    private const string T207MigrationSuffix = "_T207_DropMsfRespondentEmailHash";

    private const string ClosedByHand = "nurse.closed-by-hand@example.test";
    private const string Withdrawn = "nurse.withdrawn@example.TEST";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task Migration_DropsTheRespondentHash_WithEveryHashAlreadyStored_AndLeavesTheRestOfTheInvitation()
    {
        try
        {
            var (schema, _) = await ArrangePreT207SchemaAsync();
            var oldHash = OldUnsaltedHash(ClosedByHand);

            int anonymised, stillOpen;
            await using (var connection = await TestDatabase.OpenSchemaConnectionAsync(schema))
            {
                var template = await InsertAsync(connection,
                    """
                    INSERT INTO "MsfTemplates" ("Name", "IsActive", "AllowPatientResponses", "Kind")
                    VALUES ('T207 MSF', TRUE, FALSE, 0) RETURNING "Id"
                    """);

                var closed = await InsertCampaignAsync(connection, template, MsfCampaignState.UnderReview);
                var open = await InsertCampaignAsync(connection, template, MsfCampaignState.Open);

                // What the old anonymising left: the address gone, its unsalted hash in its place.
                anonymised = await InsertInvitationAsync(connection, closed, email: null, emailHash: oldHash, anonymisedOn: true);

                // An invitation on a campaign still open: its address is operational data, and the migration leaves it.
                stillOpen = await InsertInvitationAsync(connection, open, email: Withdrawn, emailHash: null, anonymisedOn: false);
            }

            (await TracesAsync(schema, ClosedByHand)).Should().NotBeEmpty("guard: the old hash is stored before T207");
            var hashedVersion = await RowVersionAsync(schema, anonymised);
            var unhashedVersion = await RowVersionAsync(schema, stillOpen);

            // To T207 and no further: the row versions below are T207's own writes. A later migration may rewrite the
            // whole table, as T228's stored key column does, and that is no evidence about this one.
            await MigrateToT207Async(schema);

            (await Catalog.ColumnExistsAsync(schema, "MsfInvitations", "RespondentEmailHash")).Should().BeFalse();
            (await TracesAsync(schema, ClosedByHand)).Should().BeEmpty("no column the app can read holds the hash");

            // DROP COLUMN only hides a column: each value stays in its row on disk until the row is next written, and an
            // anonymised invitation is never written again. A row written anew has a new xmin, so the row that held a
            // hash must have one, and the row that held none must not: the migration rewrote exactly the hashed rows,
            // and the drop itself rewrote nothing.
            (await RowVersionAsync(schema, anonymised)).Should().NotBe(
                hashedVersion, "the row that held a hash is rewritten without it before the drop, so no live row keeps one");
            (await RowVersionAsync(schema, stillOpen)).Should().Be(
                unhashedVersion, "guard: a row with no hash is not rewritten, so the rewrite above is the migration's own");

            // Read through today's model, which needs today's schema.
            await MigrateToLatestAsync(schema);
            (await TracesAsync(schema, ClosedByHand)).Should().BeEmpty("no later migration brings the hash back");

            await using var read = NewContext(schema);
            var invitations = await read.MsfInvitations.AsNoTracking().ToDictionaryAsync(invitation => invitation.Id);
            invitations.Keys.Should().BeEquivalentTo([anonymised, stillOpen], "no invitation is deleted");
            invitations[anonymised].RespondentEmail.Should().BeNull();
            invitations[anonymised].AnonymizedOn.Should().NotBeNull();
            invitations[anonymised].RespondentCategory.Should().Be(MsfRespondentCategory.Nurse);
            invitations[stillOpen].RespondentEmail.Should().Be(Withdrawn);
            invitations[stillOpen].AnonymizedOn.Should().BeNull();
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task Migration_Down_RestoresTheColumnEmpty_AndNoHashComesBack()
    {
        try
        {
            var (schema, predecessor) = await ArrangePreT207SchemaAsync();
            await using (var connection = await TestDatabase.OpenSchemaConnectionAsync(schema))
            {
                var template = await InsertAsync(connection,
                    """
                    INSERT INTO "MsfTemplates" ("Name", "IsActive", "AllowPatientResponses", "Kind")
                    VALUES ('T207 MSF', TRUE, FALSE, 0) RETURNING "Id"
                    """);
                var closed = await InsertCampaignAsync(connection, template, MsfCampaignState.UnderReview);
                await InsertInvitationAsync(
                    connection, closed, email: null, emailHash: OldUnsaltedHash(ClosedByHand), anonymisedOn: true);
            }

            await MigrateToLatestAsync(schema);
            (await Catalog.ColumnExistsAsync(schema, "MsfInvitations", "RespondentEmailHash")).Should().BeFalse("guard: T207 applied");

            await using (var db = NewContext(schema))
            {
                await db.GetService<IMigrator>().MigrateAsync(predecessor);
            }

            (await Catalog.ColumnExistsAsync(schema, "MsfInvitations", "RespondentEmailHash")).Should().BeTrue();
            (await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "MsfInvitations" WHERE "RespondentEmailHash" IS NOT NULL"""))
                .Should().Be(0, "a dropped hash is not recoverable, and Down invents none");
            (await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "MsfInvitations" """)).Should().Be(1);
            (await TracesAsync(schema, ClosedByHand)).Should().BeEmpty();
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ---- the search ----------------------------------------------------------------------------------------------------

    private Task<List<string>> TracesAsync(string schema, string address)
        => RespondentTraceSearch.TracesAsync(TestDatabase.SchemaConnectionString(schema), address);

    /// <summary>The transaction that wrote the row's live version: it changes whenever the row is written anew.</summary>
    private async Task<string> RowVersionAsync(string schema, int invitationId)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        await using var command = Command(
            connection, """SELECT xmin::text FROM "MsfInvitations" WHERE "Id" = $1""", [invitationId]);
        return (string)(await command.ExecuteScalarAsync())!;
    }

    /// <summary>What <c>MsfInvitation.Anonymize</c> stored before T207.</summary>
    private static string OldUnsaltedHash(string address)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(address.Trim().ToUpperInvariant())));

    // ---- the pre-T207 database, by raw SQL in its own shape ---------------------------------------------------------

    /// <summary>
    /// A schema stopped at the migration just before T207, found from the assembly rather than named, so a migration
    /// another change adds in between does not strand this test.
    /// </summary>
    private async Task<(string Schema, string Predecessor)> ArrangePreT207SchemaAsync()
    {
        var schema = await _schemas.CreateAsync();

        await using var db = NewContext(schema);
        var migrations = db.Database.GetMigrations().ToList();
        var t207 = migrations.FindIndex(migration => migration.EndsWith(T207MigrationSuffix, StringComparison.Ordinal));
        t207.Should().BePositive("guard: the T207 migration is in the assembly, after at least one other");
        var predecessor = migrations[t207 - 1];

        await db.GetService<IMigrator>().MigrateAsync(predecessor);
        (await db.Database.GetPendingMigrationsAsync())
            .Should().ContainSingle(migration => migration.EndsWith(T207MigrationSuffix, StringComparison.Ordinal));

        return (schema, predecessor);
    }

    private static Task<int> InsertCampaignAsync(NpgsqlConnection connection, int templateId, MsfCampaignState state)
        => InsertAsync(connection,
            """
            INSERT INTO "MsfCampaigns"
                ("SubjectUserId", "TemplateId", "CreatedByUserId", "CreatedOn", "OpensOn", "ClosesOn", "State",
                 "MinimumResponses", "MinimumRespondentCategories", "MinimumCategoryResponses")
            VALUES ('trainee-1', $1, 'coordinator-1', TIMESTAMPTZ '2026-09-01 08:00:00+00', DATE '2026-09-01', DATE '2026-09-15', $2,
                    0, 0, 0)
            RETURNING "Id"
            """,
            templateId, (int)state);

    private static Task<int> InsertInvitationAsync(
        NpgsqlConnection connection, int campaignId, string? email, string? emailHash, bool anonymisedOn)
        => InsertAsync(connection,
            """
            INSERT INTO "MsfInvitations"
                ("CampaignId", "RespondentEmail", "RespondentEmailHash", "RespondentCategory", "TokenHash", "IssuedOn",
                 "ExpiresOn", "AnonymizedOn")
            VALUES ($1, $2, $3, $4, $5, TIMESTAMPTZ '2026-09-01 08:00:00+00', DATE '2026-09-22',
                    CASE WHEN $6 THEN TIMESTAMPTZ '2026-09-16 08:00:00+00' END)
            RETURNING "Id"
            """,
            campaignId, (object?)email ?? DBNull.Value, (object?)emailHash ?? DBNull.Value, (int)MsfRespondentCategory.Nurse,
            Guid.NewGuid().ToString("N"), anonymisedOn);

    // ---- reads -------------------------------------------------------------------------------------------------------

    // ---- schema lifecycle (as SystemManagedMigrationPostgresTests) ----------------------------------------------------

    /// <summary>Applies the T207 migration and nothing after it, found from the assembly as its predecessor is.</summary>
    private async Task MigrateToT207Async(string schema)
    {
        await using var db = NewContext(schema);
        var t207 = db.Database.GetMigrations()
            .Single(migration => migration.EndsWith(T207MigrationSuffix, StringComparison.Ordinal));
        await db.GetService<IMigrator>().MigrateAsync(t207);
        (await db.Database.GetAppliedMigrationsAsync()).Last().Should().Be(t207);
    }

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
