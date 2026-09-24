using System.Data.Common;
using System.Globalization;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.MultiSourceFeedback;

/// <summary>
/// T163 on a real PostgreSQL server: a respondent's link is looked up by one read of one row, by the selector's unique
/// index, and the migration leaves every invitation stored before it findable by no link.
/// </summary>
/// <remarks>
/// <para>
/// Until T163 <see cref="MsfCampaignRules.GetActiveInvitationByTokenAsync" /> loaded every invitation there was, with its
/// campaign and questionnaire, and hashed the token against each, on a public page, for every load and every submit.
/// The unit tests run on the in-memory provider, which has no query plan; this runs the lookup's own statement, captured
/// as EF sent it, through <c>EXPLAIN</c> with the parameters it was sent with.
/// </para>
/// <para>
/// Isolated as <c>MsfRespondentHashPostgresTests</c> is: a schema of its own (<c>it_&lt;guid&gt;</c>), registered before it
/// is created and dropped in a finally and again on dispose.
/// </para>
/// </remarks>
public sealed class MsfLinkSelectorPostgresTests : IAsyncLifetime
{
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";
    private const string T163MigrationSuffix = "_T163_MsfInvitationTokenSelector";
    private const string SelectorIndex = "IX_MsfInvitations_TokenSelector";
    private const string OldHashIndex = "IX_MsfInvitations_TokenHash";

    /// <summary>Invitations other than the one the link names: enough that reading them all is not what a planner picks.</summary>
    private const int OtherInvitations = 2000;

    /// <summary>Invitations with no link issued, as a draft's invitees and every invitation stored before T163 are.</summary>
    private const int InvitationsWithNoLink = 500;

    private readonly InvitationTokenService _tokens = new();
    private readonly List<string> _schemas = [];
    private string _baseConnectionString = null!;

    public Task InitializeAsync()
    {
        _baseConnectionString = ResolveBaseConnectionString();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => DropSchemasAsync();

    [Fact]
    public async Task ALink_IsLookedUpInOneStatement_ThatReadsOneRowByTheSelectorsIndex()
    {
        try
        {
            var schema = await CreateSchemaAsync();
            await MigrateToLatestAsync(schema);

            var link = _tokens.GenerateSelectorToken();
            int named;
            await using (var arrange = NewContext(schema))
            {
                var campaign = OpenCampaign();
                var invitation = Invitation(campaign, "named@example.test", link.Selector, link.Hash);
                arrange.MsfInvitations.Add(invitation);
                await arrange.SaveChangesAsync();
                named = invitation.Id;

                await using var connection = await OpenAsync(schema);
                await ExecuteAsync(connection,
                    """
                    INSERT INTO "MsfInvitations"
                        ("CampaignId", "RespondentEmail", "RespondentCategory", "TokenSelector", "TokenHash", "IssuedOn", "ExpiresOn")
                    SELECT $1, 'respondent-' || g || '@example.test', $2,
                           CASE WHEN g <= $3 THEN lpad(g::text, 16, '0') END, md5(g::text), now(), CURRENT_DATE + 10
                    FROM generate_series(1, $3 + $4) AS g
                    """,
                    campaign.Id, (int)MsfRespondentCategory.Nurse, OtherInvitations, InvitationsWithNoLink);
                await ExecuteAsync(connection, """ANALYZE "MsfInvitations" """);
            }

            (await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "MsfInvitations" WHERE "TokenSelector" IS NULL"""))
                .Should().Be(InvitationsWithNoLink, "guard: the unique index holds any number of invitations with no link");

            var commands = new CommandLog();
            await using var db = NewContext(schema, commands);

            var found = await MsfCampaignRules.GetActiveInvitationByTokenAsync(db, link.Token, _tokens, CancellationToken.None);

            found.Id.Should().Be(named);
            found.Campaign.Template.Questions.Should().HaveCount(2, "the questionnaire comes in the same statement");

            var statement = commands.Statements.Should().ContainSingle("the invitation, its campaign and questionnaire are one read").Subject;
            statement.Text.Should().Contain("\"TokenSelector\" =", "the row is chosen on the server, by its selector");

            var plan = await ExplainAsync(schema, statement);
            plan.Should().Contain($"Index Scan using \"{SelectorIndex}\" on \"MsfInvitations\"", plan)
                .And.NotContain("Seq Scan on \"MsfInvitations\"", plan);
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task OnPostgres_AValidLinkOpens_AndItsSelectorWithAnotherSecretIsRefused()
    {
        try
        {
            var schema = await CreateSchemaAsync();
            await MigrateToLatestAsync(schema);

            var link = _tokens.GenerateSelectorToken();
            await using (var arrange = NewContext(schema))
            {
                arrange.MsfInvitations.Add(Invitation(OpenCampaign(), "named@example.test", link.Selector, link.Hash));
                await arrange.SaveChangesAsync();
            }

            await using var db = NewContext(schema);
            (await MsfCampaignRules.GetActiveInvitationByTokenAsync(db, link.Token, _tokens, CancellationToken.None))
                .TokenSelector.Should().Be(link.Selector);

            var forged = link.Selector + _tokens.GenerateSelectorToken().Token[InvitationTokenService.SelectorLength..];
            var open = () => MsfCampaignRules.GetActiveInvitationByTokenAsync(db, forged, _tokens, CancellationToken.None);
            (await open.Should().ThrowAsync<MsfResponseRefusedException>()).Which.Reason.Should().Be(MsfResponseRefusal.LinkNotRecognised);
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    /// <summary>
    /// The lookup reads the one row a selector names (<c>SingleOrDefaultAsync</c>), so the migration's index must be
    /// unique: two rows sharing a selector would turn a respondent's page into a server error. The server refuses the
    /// second, by that index and no other, while any number of rows hold no selector. The column is sized to the
    /// selector. (T163 review: the model snapshot said unique, and nothing checked that the migration did.)
    /// </summary>
    [Fact]
    public async Task TheSelectorsIndex_RefusesASecondRowWithASelectorAlreadyHeld_AndHoldsAnyNumberWithNone()
    {
        try
        {
            var schema = await CreateSchemaAsync();
            await MigrateToLatestAsync(schema);

            int campaignId;
            await using (var arrange = NewContext(schema))
            {
                var campaign = OpenCampaign();
                arrange.MsfCampaigns.Add(campaign);
                await arrange.SaveChangesAsync();
                campaignId = campaign.Id;
            }

            (await ScalarAsync<string>(schema,
                    """
                    SELECT data_type || '(' || character_maximum_length || ')' FROM information_schema.columns
                    WHERE table_schema = current_schema() AND table_name = 'MsfInvitations' AND column_name = 'TokenSelector'
                    """))
                .Should().Be($"character varying({InvitationTokenService.SelectorLength})");
            (await IndexIsUniqueAsync(schema, SelectorIndex)).Should().BeTrue();

            const string withSelector =
                """
                INSERT INTO "MsfInvitations"
                    ("CampaignId", "RespondentEmail", "RespondentCategory", "TokenSelector", "TokenHash", "IssuedOn", "ExpiresOn")
                VALUES ($1, $2, $3, $4, $5, now(), CURRENT_DATE + 10)
                """;
            const string withNoSelector =
                """
                INSERT INTO "MsfInvitations"
                    ("CampaignId", "RespondentEmail", "RespondentCategory", "TokenHash", "IssuedOn", "ExpiresOn")
                VALUES ($1, $2, $3, $4, now(), CURRENT_DATE + 10)
                """;

            var link = _tokens.GenerateSelectorToken();
            var nurse = (int)MsfRespondentCategory.Nurse;
            await using var connection = await OpenAsync(schema);
            await ExecuteAsync(connection, withSelector, campaignId, "first@example.test", nurse, link.Selector, link.Hash);
            await ExecuteAsync(connection, withNoSelector, campaignId, "draft-1@example.test", nurse, _tokens.GenerateSelectorToken().Hash);
            await ExecuteAsync(connection, withNoSelector, campaignId, "draft-2@example.test", nurse, _tokens.GenerateSelectorToken().Hash);

            // Everything but the selector differs, so only the selector's index can refuse it.
            var second = () => ExecuteAsync(
                connection, withSelector, campaignId, "second@example.test", nurse, link.Selector, _tokens.GenerateSelectorToken().Hash);
            var refused = (await second.Should().ThrowAsync<PostgresException>()).Which;
            refused.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);
            refused.ConstraintName.Should().Be(SelectorIndex);

            (await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "MsfInvitations" WHERE "TokenSelector" IS NULL"""))
                .Should().Be(2);
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    /// <summary>
    /// Every link mailed before T163 stops working (W-007): the migration gives no invitation already stored a selector,
    /// so no link finds it. The rest of the invitation is left, and <c>Down</c> puts the hash's unique index back.
    /// </summary>
    [Fact]
    public async Task Migration_LeavesEveryInvitationStoredBeforeIt_FoundByNoLink_AndDownRestoresTheHashIndex()
    {
        try
        {
            var schema = await CreateSchemaAsync();
            string predecessor;
            await using (var db = NewContext(schema))
            {
                var migrations = db.Database.GetMigrations().ToList();
                var t163 = migrations.FindIndex(migration => migration.EndsWith(T163MigrationSuffix, StringComparison.Ordinal));
                t163.Should().BePositive("guard: the T163 migration is in the assembly, after at least one other");
                predecessor = migrations[t163 - 1];
                await db.GetService<IMigrator>().MigrateAsync(predecessor);
            }

            // A link as the product mailed it before T163: a 43-character token, only its hash stored.
            var oldToken = _tokens.GenerateToken();
            int invitationId;
            await using (var connection = await OpenAsync(schema))
            {
                var template = await InsertAsync(connection,
                    """
                    INSERT INTO "MsfTemplates" ("Name", "IsActive", "AllowPatientResponses", "Kind")
                    VALUES ('T163 MSF', TRUE, FALSE, 0) RETURNING "Id"
                    """);
                var campaign = await InsertAsync(connection,
                    """
                    INSERT INTO "MsfCampaigns"
                        ("SubjectUserId", "TemplateId", "CreatedByUserId", "CreatedOn", "OpensOn", "ClosesOn", "State",
                         "MinimumResponses", "MinimumRespondentCategories", "MinimumCategoryResponses")
                    VALUES ('trainee-1', $1, 'coordinator-1', now(), CURRENT_DATE - 1, CURRENT_DATE + 3, $2, 0, 0, 0)
                    RETURNING "Id"
                    """,
                    template, (int)MsfCampaignState.Open);
                invitationId = await InsertAsync(connection,
                    """
                    INSERT INTO "MsfInvitations"
                        ("CampaignId", "RespondentEmail", "RespondentCategory", "TokenHash", "IssuedOn", "ExpiresOn")
                    VALUES ($1, 'nurse@example.test', $2, $3, now(), CURRENT_DATE + 10)
                    RETURNING "Id"
                    """,
                    campaign, (int)MsfRespondentCategory.Nurse, _tokens.HashToken(oldToken));
            }

            (await IndexesAsync(schema)).Should().Contain(OldHashIndex, "guard: the pre-T163 schema");

            await MigrateToLatestAsync(schema);

            (await IndexesAsync(schema)).Should().Contain(SelectorIndex).And.NotContain(OldHashIndex);
            (await IndexIsUniqueAsync(schema, SelectorIndex)).Should().BeTrue();
            await using (var db = NewContext(schema))
            {
                var stored = await db.MsfInvitations.AsNoTracking().SingleAsync();
                stored.Id.Should().Be(invitationId, "no invitation is deleted");
                stored.TokenSelector.Should().BeNull();
                stored.TokenHash.Should().Be(_tokens.HashToken(oldToken));
                stored.RespondentEmail.Should().Be("nurse@example.test");

                var oldLink = () => MsfCampaignRules.GetActiveInvitationByTokenAsync(db, oldToken, _tokens, CancellationToken.None);
                (await oldLink.Should().ThrowAsync<MsfResponseRefusedException>())
                    .Which.Reason.Should().Be(MsfResponseRefusal.LinkNotRecognised);

                await db.GetService<IMigrator>().MigrateAsync(predecessor);
            }

            (await IndexesAsync(schema)).Should().Contain(OldHashIndex).And.NotContain(SelectorIndex);
            (await IndexIsUniqueAsync(schema, OldHashIndex)).Should().BeTrue("Down puts the pre-T163 index back as it was");
            (await ScalarAsync<bool>(schema,
                    """
                    SELECT EXISTS (SELECT 1 FROM information_schema.columns
                                   WHERE table_schema = current_schema() AND table_name = 'MsfInvitations'
                                     AND column_name = 'TokenSelector')
                    """))
                .Should().BeFalse();
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    // ---- the data ----------------------------------------------------------------------------------------------------

    private static MsfCampaign OpenCampaign()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new MsfCampaign
        {
            SubjectUserId = "trainee-1",
            CreatedByUserId = "coordinator-1",
            CreatedOn = DateTime.UtcNow,
            OpensOn = today.AddDays(-1),
            ClosesOn = today.AddDays(3),
            State = MsfCampaignState.Open,
            Template = new MsfTemplate
            {
                Name = "Annual MSF",
                IsActive = true,
                Questions =
                [
                    new MsfQuestion { Order = 1, Prompt = "Communication", Type = MsfQuestionType.LongText, Required = true },
                    new MsfQuestion { Order = 2, Prompt = "Anything else", Type = MsfQuestionType.LongText, Required = false }
                ]
            }
        };
    }

    private static MsfInvitation Invitation(MsfCampaign campaign, string email, string selector, string hash)
        => new()
        {
            Campaign = campaign,
            RespondentEmail = email,
            RespondentCategory = MsfRespondentCategory.Nurse,
            TokenSelector = selector,
            TokenHash = hash,
            IssuedOn = DateTime.UtcNow,
            ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10)
        };

    // ---- the plan ----------------------------------------------------------------------------------------------------

    /// <summary>The server's plan for a statement EF sent, with the parameter values it was sent with.</summary>
    private async Task<string> ExplainAsync(string schema, CapturedStatement statement)
    {
        await using var connection = await OpenAsync(schema);
        await using var command = new NpgsqlCommand("EXPLAIN " + statement.Text, connection);
        command.Parameters.AddRange(statement.Parameters.ToArray());

        var lines = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lines.Add(reader.GetString(0));
        }

        return string.Join('\n', lines);
    }

    private async Task<List<string>> IndexesAsync(string schema)
    {
        await using var connection = await OpenAsync(schema);
        await using var command = new NpgsqlCommand(
            """SELECT indexname FROM pg_indexes WHERE schemaname = current_schema() AND tablename = 'MsfInvitations'""",
            connection);

        var names = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            names.Add(reader.GetString(0));
        }

        return names;
    }

    /// <summary>Whether the server holds the named index of <c>MsfInvitations</c>, in this schema, as unique.</summary>
    private async Task<bool> IndexIsUniqueAsync(string schema, string indexName)
    {
        await using var connection = await OpenAsync(schema);
        await using var command = Command(connection,
            """
            SELECT i.indisunique
            FROM pg_index AS i
            JOIN pg_class AS c ON c.oid = i.indexrelid
            WHERE c.relname = $1 AND c.relnamespace = current_schema()::regnamespace
            """,
            [indexName]);

        var value = await command.ExecuteScalarAsync();
        value.Should().NotBeNull($"guard: the index {indexName} exists in the schema");
        return (bool)value!;
    }

    // ---- schema lifecycle (as MsfRespondentHashPostgresTests) ---------------------------------------------------------

    private async Task MigrateToLatestAsync(string schema)
    {
        await using var db = NewContext(schema);
        await db.Database.MigrateAsync();
        (await db.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
    }

    private ApplicationDbContext NewContext(string schema, CommandLog? commands = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(SchemaConnectionString(schema));
        if (commands is not null)
        {
            options.AddInterceptors(commands);
        }

        return new ApplicationDbContext(options.Options);
    }

    /// <summary>The schema and nothing else on the search path, so an unqualified name can only ever resolve inside it.</summary>
    private string SchemaConnectionString(string schema)
        => new NpgsqlConnectionStringBuilder(_baseConnectionString) { SearchPath = schema, Pooling = false }.ConnectionString;

    private static async Task<int> InsertAsync(NpgsqlConnection connection, string sql, params object[] values)
    {
        await using var command = Command(connection, sql, values);
        return Convert.ToInt32(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql, params object[] values)
    {
        await using var command = Command(connection, sql, values);
        await command.ExecuteNonQueryAsync();
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

    private sealed record CapturedStatement(string Text, IReadOnlyList<NpgsqlParameter> Parameters);

    /// <summary>Every query that reached the server, with a copy of the parameters it was sent with.</summary>
    private sealed class CommandLog : DbCommandInterceptor
    {
        public List<CapturedStatement> Statements { get; } = [];

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            Capture(command);
            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            Capture(command);
            return ValueTask.FromResult(result);
        }

        public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
        {
            Capture(command);
            return ValueTask.FromResult(result);
        }

        private void Capture(DbCommand command)
            => Statements.Add(new CapturedStatement(
                command.CommandText,
                command.Parameters.Cast<NpgsqlParameter>().Select(parameter => parameter.Clone()).ToList()));
    }
}
