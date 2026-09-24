using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.CommitteeDecisions;

/// <summary>
/// T131 slice 3 on a real PostgreSQL server: a decision panel may sit as one of the College's decision bodies.
/// </summary>
/// <remarks>
/// <para>
/// The migration adds <c>DecisionPanels.DecisionBodyKey</c>, a restricting foreign key, and a unique index on
/// (institution, speciality, body) that is partial (only panels that carry a body) and NULLS NOT DISTINCT, so a null
/// speciality counts as one value. EF InMemory, which the handler tests run on, enforces none of that, so it is shown
/// here: one institution-wide neonatal panel per institution, one per speciality, one each at A and B, and as many
/// general panels as an institution likes.
/// </para>
/// <para>
/// The schema helpers follow <c>DecisionPanelInstitutionMigrationPostgresTests</c>: each test runs on a schema of its
/// own, dropped in a <c>finally</c>, with <see cref="DisposeAsync" /> as a backstop.
/// </para>
/// </remarks>
public sealed class DecisionPanelBodyPostgresTests : IAsyncLifetime
{
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";

    private const string ThisMigration = "20260924210830_T131_PanelBodies";
    private const string LastMigrationBeforeThis = "20260924195753_T164_LearnerFeedbackKind";

    private readonly List<string> _schemas = [];
    private string _baseConnectionString = null!;

    public Task InitializeAsync()
    {
        _baseConnectionString = ResolveBaseConnectionString();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => DropSchemasAsync();

    [Fact]
    public async Task Migration_LeavesEveryExistingPanelAGeneralPanel()
    {
        try
        {
            var schema = await CreateSchemaAsync();
            await using (var db = NewContext(schema))
            {
                await db.GetService<IMigrator>().MigrateAsync(LastMigrationBeforeThis);
                (await db.Database.GetPendingMigrationsAsync()).First().Should().Be(ThisMigration);
            }

            int panelId;
            await using (var connection = await OpenAsync(schema))
            {
                var institutionId = await InsertInstitutionAsync(connection, "KGK");
                panelId = await InsertAsync(connection,
                    """INSERT INTO "DecisionPanels" ("Name", "Scope", "InstitutionId", "SpecialityId", "CreatedOn") VALUES ('Annual review', 1, $1, NULL, TIMESTAMPTZ '2026-06-19 00:00:00+00') RETURNING "Id" """,
                    institutionId);
            }

            await using (var db = NewContext(schema))
            {
                await db.Database.MigrateAsync();
                (await db.Database.GetAppliedMigrationsAsync()).Should().Contain(ThisMigration);
                (await db.DecisionPanels.SingleAsync(panel => panel.Id == panelId)).DecisionBodyKey.Should().BeNull();
            }
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task TheIndex_AllowsOneInstitutionWidePanelAndOnePerSpecialityPerBody_AtEachInstitution()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            await using var connection = await OpenAsync(schema);
            var a = await InsertInstitutionAsync(connection, "A");
            var b = await InsertInstitutionAsync(connection, "B");
            var paediatrics = await InsertSpecialityAsync(connection);

            await InsertPanelAsync(connection, a, null, "neonatal");
            await InsertPanelAsync(connection, b, null, "neonatal");
            await InsertPanelAsync(connection, a, paediatrics, "neonatal");
            await InsertPanelAsync(connection, b, paediatrics, "neonatal");

            // General panels are left out of the index, so an institution keeps as many as it likes.
            await InsertPanelAsync(connection, a, null, null);
            await InsertPanelAsync(connection, a, null, null);
            await InsertPanelAsync(connection, a, paediatrics, null);
            await InsertPanelAsync(connection, a, paediatrics, null);

            var secondInstitutionWide = async () => await InsertPanelAsync(connection, a, null, "neonatal");
            (await secondInstitutionWide.Should().ThrowAsync<PostgresException>()).Which.SqlState
                .Should().Be(PostgresErrorCodes.UniqueViolation, "a null speciality is one value: NULLS NOT DISTINCT");

            var secondForTheSpeciality = async () => await InsertPanelAsync(connection, a, paediatrics, "neonatal");
            (await secondForTheSpeciality.Should().ThrowAsync<PostgresException>()).Which.SqlState
                .Should().Be(PostgresErrorCodes.UniqueViolation);

            (await ScalarAsync<string>(connection,
                    """SELECT indexdef FROM pg_indexes WHERE schemaname = current_schema() AND indexname = 'IX_DecisionPanels_InstitutionId_SpecialityId_DecisionBodyKey'"""))
                .Should().Contain("UNIQUE").And.Contain("NULLS NOT DISTINCT").And.Contain("WHERE");
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task TheForeignKey_RefusesAnUnknownBody_AndABodyInUseCannotBeDeleted()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            await using var connection = await OpenAsync(schema);
            var a = await InsertInstitutionAsync(connection, "A");
            await InsertPanelAsync(connection, a, null, "neonatal");

            var unknown = async () => await InsertPanelAsync(connection, a, null, "cardiac");
            (await unknown.Should().ThrowAsync<PostgresException>()).Which.SqlState
                .Should().Be(PostgresErrorCodes.ForeignKeyViolation);

            var deleteTheBody = async () => await ExecuteAsync(connection, """DELETE FROM "DecisionBodies" WHERE "Key" = 'neonatal'""");
            (await deleteTheBody.Should().ThrowAsync<PostgresException>()).Which.SqlState
                .Should().Be(PostgresErrorCodes.ForeignKeyViolation, "a body a panel sits as cannot vanish under it");
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task TheHandler_GivesAAndBEachANeonatalPanel_AndNamesThePanelInTheWayOfASecond()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            int a, b, panelA, panelB, secondAtA;
            await using (var connection = await OpenAsync(schema))
            {
                a = await InsertInstitutionAsync(connection, "A");
                b = await InsertInstitutionAsync(connection, "B");
                panelA = await InsertPanelAsync(connection, a, null, null);
                panelB = await InsertPanelAsync(connection, b, null, null);
                secondAtA = await InsertPanelAsync(connection, a, null, null);
            }

            await using (var db = NewContext(schema))
            {
                await SetBodyAsync(db, panelA, "neonatal", InstitutionalAdmin(a));
                await SetBodyAsync(db, panelB, "neonatal", InstitutionalAdmin(b));
            }

            await using (var db = NewContext(schema))
            {
                var second = () => SetBodyAsync(db, secondAtA, "neonatal", InstitutionalAdmin(a));
                (await second.Should().ThrowAsync<InvalidOperationException>()).Which.Message
                    .Should().Contain("already sits as the Neonatal team Clinical Competency Committee for the whole institution");

                // The audit pipeline saves the request's context from its catch: nothing was staged to commit.
                await db.SaveChangesAsync();
            }

            await using (var db = NewContext(schema))
            {
                (await db.DecisionPanels.AsNoTracking().OrderBy(panel => panel.Id)
                        .Select(panel => panel.DecisionBodyKey).ToListAsync())
                    .Should().Equal("neonatal", "neonatal", null);
            }
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task TwoAdminsTaggingTwoPanelsAtOnce_TheSecondIsToldWhichPanelHoldsTheBody_AndNothingOfItIsWritten()
    {
        // Both requests pass the handler's check before either saves; the index refuses the second save. The page prints
        // the refusal, so it must be the sentence naming the panel in the way, not EF's "An error occurred while saving
        // the entity changes". The database's exception stays underneath, which is what tells the audit pipeline to
        // write its failure row without re-sending the refused change (T201).
        try
        {
            var schema = await MigratedSchemaAsync();
            int a, ours, rival;
            await using (var connection = await OpenAsync(schema))
            {
                a = await InsertInstitutionAsync(connection, "A");
                ours = await InsertPanelAsync(connection, a, null, null, "Our CCC");
                rival = await InsertPanelAsync(connection, a, null, null, "Rival CCC");
            }

            await using (var db = NewContext(schema, beforeFirstSave: async () =>
                         {
                             await using var other = NewContext(schema);
                             await SetBodyAsync(other, rival, "neonatal", InstitutionalAdmin(a));
                         }))
            {
                var ourSet = () => SetBodyAsync(db, ours, "neonatal", InstitutionalAdmin(a));

                var refusal = await ourSet.Should().ThrowExactlyAsync<InvalidOperationException>();
                refusal.Which.Message.Should().StartWith(
                    "Rival CCC already sits as the Neonatal team Clinical Competency Committee for the whole institution.");
                refusal.Which.InnerException.Should().BeAssignableTo<DbUpdateException>();
            }

            (await StoredBodiesAsync(schema)).Should().BeEquivalentTo(
                new Dictionary<int, string?> { [ours] = null, [rival] = "neonatal" });
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task APanelCreatedAsABodyWhileAnotherIsTagged_IsRefusedNamingThatPanel_AndIsNotCreated()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            int a, rival;
            await using (var connection = await OpenAsync(schema))
            {
                a = await InsertInstitutionAsync(connection, "A");
                rival = await InsertPanelAsync(connection, a, null, null, "Rival CCC");
            }

            await using (var db = NewContext(schema, beforeFirstSave: async () =>
                         {
                             await using var other = NewContext(schema);
                             await SetBodyAsync(other, rival, "neonatal", InstitutionalAdmin(a));
                         }))
            {
                // The chair plus one, both committee members at A: a panel T165 would seat (PanelSeat).
                var create = () => new CreateDecisionPanelCommandHandler(db, FakeUserDirectory.CommitteeMembersAt(a, "chair", "member")).Handle(
                    new CreateDecisionPanelCommand(
                        "Our CCC",
                        DecisionPanelScope.Institution,
                        a,
                        SpecialityId: null,
                        [
                            new DecisionPanelMemberInput("chair", DecisionPanelMemberRole.Chair),
                            new DecisionPanelMemberInput("member", DecisionPanelMemberRole.Member)
                        ],
                        InstitutionalAdmin(a),
                        "neonatal"),
                    CancellationToken.None);

                var refusal = await create.Should().ThrowExactlyAsync<InvalidOperationException>();
                refusal.Which.Message.Should().StartWith(
                    "Rival CCC already sits as the Neonatal team Clinical Competency Committee for the whole institution.");
                refusal.Which.InnerException.Should().BeAssignableTo<DbUpdateException>();
            }

            (await StoredBodiesAsync(schema)).Should().BeEquivalentTo(new Dictionary<int, string?> { [rival] = "neonatal" });
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task APanelsScopeAndSpeciality_MustAgree()
    {
        // Routing reads the scope and the body index reads the speciality. An institution-wide panel carrying a
        // speciality would take that speciality's body slot while routing treated it as the institution's, leaving room
        // for a second institution-wide neonatal panel beside it.
        try
        {
            var schema = await MigratedSchemaAsync();
            await using var connection = await OpenAsync(schema);
            var a = await InsertInstitutionAsync(connection, "A");
            var paediatrics = await InsertSpecialityAsync(connection);

            var institutionWideWithASpeciality = async () => await InsertScopedPanelAsync(connection, a, scope: 1, paediatrics);
            (await institutionWideWithASpeciality.Should().ThrowAsync<PostgresException>()).Which.ConstraintName
                .Should().Be("CK_DecisionPanels_ScopeMatchesSpeciality");

            var specialityWithout = async () => await InsertScopedPanelAsync(connection, a, scope: 2, specialityId: null);
            (await specialityWithout.Should().ThrowAsync<PostgresException>()).Which.ConstraintName
                .Should().Be("CK_DecisionPanels_ScopeMatchesSpeciality");

            var unknownScope = async () => await InsertScopedPanelAsync(connection, a, scope: 3, paediatrics);
            (await unknownScope.Should().ThrowAsync<PostgresException>()).Which.SqlState
                .Should().Be(PostgresErrorCodes.CheckViolation);

            await InsertScopedPanelAsync(connection, a, scope: 1, specialityId: null);
            await InsertScopedPanelAsync(connection, a, scope: 2, paediatrics);
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    // ---- arrange --------------------------------------------------------------------------------------------------------

    private async Task<string> MigratedSchemaAsync()
    {
        var schema = await CreateSchemaAsync();
        await using var db = NewContext(schema);
        await db.Database.MigrateAsync();
        (await db.Database.GetAppliedMigrationsAsync()).Should().Contain(ThisMigration);
        return schema;
    }

    private static async Task<DecisionPanelDetailDto> SetBodyAsync(
        ApplicationDbContext db, int panelId, string? bodyKey, ClaimsPrincipal principal)
        => await new SetDecisionPanelBodyCommandHandler(db).Handle(
            new SetDecisionPanelBodyCommand(panelId, bodyKey, principal), CancellationToken.None);

    private static ClaimsPrincipal InstitutionalAdmin(int institutionId)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, $"inst-admin-{institutionId}"),
                new Claim(ClaimTypes.Role, WombatRoles.InstitutionalAdmin),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
            ],
            "test"));

    private static Task<int> InsertInstitutionAsync(NpgsqlConnection connection, string shortCode)
        => InsertAsync(connection,
            """INSERT INTO "Institutions" ("Name", "ShortCode", "IsActive", "CreatedOn") VALUES ($1, $1, TRUE, TIMESTAMPTZ '2026-06-19 00:00:00+00') RETURNING "Id" """,
            shortCode);

    private static async Task<int> InsertSpecialityAsync(NpgsqlConnection connection)
    {
        var collegeId = await InsertAsync(connection,
            """INSERT INTO "Colleges" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('CPSA', 'CPSA', TIMESTAMPTZ '2026-06-19 00:00:00+00', TRUE) RETURNING "Id" """);
        return await InsertAsync(connection,
            """INSERT INTO "Specialities" ("CollegeId", "Name", "IsActive") VALUES ($1, 'Paediatrics', TRUE) RETURNING "Id" """,
            collegeId);
    }

    private static Task<int> InsertPanelAsync(
        NpgsqlConnection connection, int institutionId, int? specialityId, string? bodyKey, string name = "Panel")
        => InsertAsync(connection,
            """INSERT INTO "DecisionPanels" ("Name", "Scope", "InstitutionId", "SpecialityId", "DecisionBodyKey", "CreatedOn") VALUES ($4, CASE WHEN $2::integer IS NULL THEN 1 ELSE 2 END, $1, $2, $3, TIMESTAMPTZ '2026-06-19 00:00:00+00') RETURNING "Id" """,
            institutionId,
            (object?)specialityId ?? DBNull.Value,
            (object?)bodyKey ?? DBNull.Value,
            name);

    /// <summary>A general panel with its scope given outright, so a scope and a speciality that disagree can be tried.</summary>
    private static Task<int> InsertScopedPanelAsync(NpgsqlConnection connection, int institutionId, int scope, int? specialityId)
        => InsertAsync(connection,
            """INSERT INTO "DecisionPanels" ("Name", "InstitutionId", "SpecialityId", "Scope", "CreatedOn") VALUES ('Panel', $1, $2, $3, TIMESTAMPTZ '2026-06-19 00:00:00+00') RETURNING "Id" """,
            institutionId,
            (object?)specialityId ?? DBNull.Value,
            scope);

    private async Task<Dictionary<int, string?>> StoredBodiesAsync(string schema)
    {
        await using var db = NewContext(schema);
        return await db.DecisionPanels.AsNoTracking().ToDictionaryAsync(panel => panel.Id, panel => panel.DecisionBodyKey);
    }

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

    /// <summary>
    /// Positional parameters ($1, $2, …), so no value is ever spliced into SQL text. A null is typed by its position: the
    /// speciality is an integer, the body key text.
    /// </summary>
    private static NpgsqlCommand Command(NpgsqlConnection connection, string sql, object[] values)
    {
        var command = new NpgsqlCommand(sql, connection);
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index];
            command.Parameters.Add(value is DBNull
                ? new NpgsqlParameter
                {
                    Value = value,
                    NpgsqlDbType = index == 2 ? NpgsqlTypes.NpgsqlDbType.Varchar : NpgsqlTypes.NpgsqlDbType.Integer
                }
                : new NpgsqlParameter { Value = value });
        }

        return command;
    }

    // ---- schemas --------------------------------------------------------------------------------------------------------

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

    /// <param name="schema">The test's schema.</param>
    /// <param name="beforeFirstSave">
    /// Another request, run from start to commit just before this context's first save reaches the database: the
    /// overlap of a race, made exact (as <c>CommitteeReviewRacePostgresTests</c> does).
    /// </param>
    private ApplicationDbContext NewContext(string schema, Func<Task>? beforeFirstSave = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(SchemaConnectionString(schema));
        if (beforeFirstSave is not null)
        {
            options.AddInterceptors(new BeforeFirstSave(beforeFirstSave));
        }

        return new ApplicationDbContext(options.Options);
    }

    /// <summary>Runs the other request once, just before this context's first save goes to the database.</summary>
    private sealed class BeforeFirstSave(Func<Task> action) : SaveChangesInterceptor
    {
        private Func<Task>? _action = action;

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (Interlocked.Exchange(ref _action, null) is { } run)
            {
                await run();
            }

            return result;
        }
    }

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
