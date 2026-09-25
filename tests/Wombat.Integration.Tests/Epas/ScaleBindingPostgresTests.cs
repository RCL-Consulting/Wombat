using System.Data.Common;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql;
using Wombat.Application.Audit;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.Epas.Commands.DeleteEntrustmentScale;
using Wombat.Application.Features.Epas.Commands.UpdateEntrustmentScale;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Persistence.Migrations;

namespace Wombat.Integration.Tests.Epas;

/// <summary>
/// T253 on a real PostgreSQL server: one binding rule for a schema's <c>scale_key</c>, read the same way by the resolver
/// and by the scale's delete.
/// </summary>
/// <remarks>
/// <para>
/// The resolver's one query matches a key against a scale's id, its nullable seed key and its name at once, which EF
/// InMemory evaluates in memory and Npgsql translates to SQL. The seeded corpus is booted here as a startup boots it, the
/// v11.1 ladder renamed through the command the scale editor sends, and every seeded scale field resolved on the server.
/// </para>
/// <para>
/// The delete runs inside the real <see cref="AuditPipelineBehavior{TRequest,TResponse}" />, writing through the real
/// <see cref="AuditWriter" /> on the request's own context, so the failure row's save would commit anything the handler
/// had removed before it threw. Schema helpers as <c>EntrustmentScaleDeletePostgresTests</c>.
/// </para>
/// </remarks>
public sealed class ScaleBindingPostgresTests : IAsyncLifetime
{
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";

    private readonly List<string> _schemas = [];
    private string _baseConnectionString = null!;

    public Task InitializeAsync()
    {
        _baseConnectionString = ResolveBaseConnectionString();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => DropSchemasAsync();

    [Fact]
    public async Task DeletingAScaleAPublishedFormBindsById_IsRefusedNamingTheType_AndTheFailureRowCommitsNothing()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            int scaleId;
            await using (var db = NewContext(schema))
            {
                var scale = new EntrustmentScale
                {
                    Name = "T253 ladder",
                    Levels = [new EntrustmentLevel { Order = 1, Label = "Observe" }, new EntrustmentLevel { Order = 2, Label = "Independent" }]
                };
                db.EntrustmentScales.Add(scale);
                await db.SaveChangesAsync();
                scaleId = scale.Id;

                // As the activity-type builder publishes it: the field's scale_key is the scale's id.
                var type = new ActivityType
                {
                    Key = "t253_local_mini_cex",
                    Name = "T253 Local Mini-CEX",
                    Scope = ActivityScope.Global,
                    OwnerUserId = "admin-1",
                    CreatedOn = DateTime.UtcNow,
                    IsActive = true
                };
                type.SaveDraft(RatedSchema(scaleId.ToString(CultureInfo.InvariantCulture)), Workflow, "{\"counts_for\":[]}", "[]", "admin-1");
                type.PublishDraft("admin-1");
                db.ActivityTypes.Add(type);
                await db.SaveChangesAsync();
            }

            var commands = new CommandLog();
            var outcome = new HandlerOutcome();
            var delete = () => DeleteThroughTheAuditPipelineAsync(schema, scaleId, commands, outcome);

            var thrown = (await delete.Should().ThrowAsync<InvalidOperationException>()).Which;
            thrown.Should().BeSameAs(outcome.Thrown);
            thrown.Message.Should().Be(
                "The form of the activity type \"T253 Local Mini-CEX\" (t253_local_mini_cex, version 1) uses this " +
                "entrustment scale, so it cannot be deleted. A published version never changes, and activities stay on " +
                "the version they were filed on, so no later version can take that back. Leave the scale in place instead.");
            commands.Texts.Should().NotContain(
                text => text.Contains("DELETE", StringComparison.OrdinalIgnoreCase),
                "the handler refused before it removed anything");

            await using var read = NewContext(schema);
            var row = (await read.AuditEntries.AsNoTracking()
                    .Where(entry => entry.Action == nameof(DeleteEntrustmentScaleCommand))
                    .ToListAsync())
                .Should().ContainSingle().Subject;
            row.Success.Should().BeFalse();
            row.ErrorMessage.Should().Be(thrown.Message);
            (await read.EntrustmentScales.AsNoTracking().Include(entity => entity.Levels).SingleAsync(entity => entity.Id == scaleId))
                .Levels.Should().HaveCount(2, "the failure row's save committed no removal");
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task TheSeededCorpus_AfterItsLaddersAreRenamed_ResolvesEveryScaleFieldOnTheServer()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            await using (var boot = NewContext(schema))
            {
                await new DataSeeder(boot).SeedAsync();
                await new PaediatricCatalogueSeeder(boot).SeedAsync();
            }

            var renamed = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var (seedKey, newName) in new[] { ("cpsa:scale:v11.1", "Paediatric ladder"), ("demo:scale:o-r", "Observation to entrustment") })
            {
                await using var db = NewContext(schema);
                var scale = await db.EntrustmentScales.AsNoTracking().Include(entity => entity.Levels)
                    .SingleAsync(entity => entity.SeedKey == seedKey);
                var saved = await new UpdateEntrustmentScaleCommandHandler(db).Handle(
                    new UpdateEntrustmentScaleCommand(
                        scale.Id,
                        newName,
                        scale.Description,
                        scale.Levels.OrderBy(level => level.Order)
                            .Select(level => new EntrustmentLevelUpdate(level.Id, level.Order, level.Label, level.Description))
                            .ToList(),
                        Administrator()),
                    CancellationToken.None);
                saved.RenameWarning.Should().BeNull("no seeded schema binds a ladder by its name");
                renamed[ScaleBinding.ForSeedKey(seedKey)] = scale.Id;
            }

            await using var read = NewContext(schema);
            var scaleKeys = (await read.ActivityTypeVersions.AsNoTracking().Select(version => version.SchemaJson).ToListAsync())
                .SelectMany(json => FormSchemaParser.Parse(json).Sections.SelectMany(section => section.Fields))
                .Where(field => !string.IsNullOrWhiteSpace(field.ScaleKey))
                .Select(field => field.ScaleKey!)
                .Distinct(StringComparer.Ordinal)
                .ToList();
            scaleKeys.Should().BeEquivalentTo(renamed.Keys, "every seeded scale field binds one of the two ladders by seed key");

            var resolved = await EntrustmentScaleBindings.ResolveAsync(read, scaleKeys.Append("CPSA Paediatric Entrustment Scale v11.1"));
            resolved.Should().BeEquivalentTo(renamed, "each seed key binds its renamed ladder, and the old name binds nothing");

            var rungs = await EntrustmentRungLabels.LoadForScaleKeysAsync(read, scaleKeys);
            rungs.FormatByScaleKey("seed:cpsa:scale:v11.1", 5).Should().Be("4", "order 5 is the College's rung 4");
            rungs.FormatByScaleKey("seed:demo:scale:o-r", 4).Should().Be("Independent");
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    /// <summary>
    /// T253 review, finding 1, through the real audit pipeline: a save refused after its rename used to commit the rename
    /// anyway, because the failure row is saved through the request's own context and the name was assigned before the
    /// level checks. Refused here by the third of them, a level id the scale does not hold.
    /// </summary>
    [Fact]
    public async Task ARefusedScaleSave_CommitsNeitherItsRenameNorItsLevelEdits_ThroughTheFailureRow()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            int scaleId;
            int keptLevelId;
            int foreignLevelId;
            await using (var db = NewContext(schema))
            {
                var scale = LadderOf("T253 ladder");
                scale.Description = "Before.";
                var other = LadderOf("Another ladder");
                db.EntrustmentScales.AddRange(scale, other);
                await db.SaveChangesAsync();
                scaleId = scale.Id;
                keptLevelId = scale.Levels.Single(level => level.Order == 1).Id;
                foreignLevelId = other.Levels.Single(level => level.Order == 2).Id;
            }

            var commands = new CommandLog();
            var outcome = new HandlerOutcome();
            var update = () => UpdateThroughTheAuditPipelineAsync(
                schema,
                new UpdateEntrustmentScaleCommand(
                    scaleId,
                    "T253 ladder renamed",
                    "After.",
                    [
                        new EntrustmentLevelUpdate(keptLevelId, 1, "Low (relabelled)", null),
                        new EntrustmentLevelUpdate(foreignLevelId, 2, "Borrowed", null)
                    ],
                    Administrator()),
                commands,
                outcome);

            var thrown = (await update.Should().ThrowAsync<InvalidOperationException>()).Which;
            thrown.Should().BeSameAs(outcome.Thrown);
            thrown.Message.Should().Be($"Level {foreignLevelId} was not found on scale {scaleId}.");
            commands.Texts.Should().NotContain(
                text => text.Contains("UPDATE \"EntrustmentScales\"", StringComparison.Ordinal)
                        || text.Contains("UPDATE \"EntrustmentLevels\"", StringComparison.Ordinal)
                        || text.Contains("DELETE FROM \"EntrustmentLevels\"", StringComparison.Ordinal),
                "the handler refused before it assigned anything, so the failure row's save sent no scale or level write");

            await using var read = NewContext(schema);
            (await read.AuditEntries.AsNoTracking()
                    .Where(entry => entry.Action == nameof(UpdateEntrustmentScaleCommand))
                    .ToListAsync())
                .Should().ContainSingle().Which.Success.Should().BeFalse();
            var stored = await read.EntrustmentScales.AsNoTracking().Include(entity => entity.Levels).SingleAsync(entity => entity.Id == scaleId);
            stored.Name.Should().Be("T253 ladder");
            stored.Description.Should().Be("Before.");
            stored.Levels.OrderBy(level => level.Order).Select(level => level.Label).Should().Equal("Low", "High");
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    /// <summary>
    /// T253 review, finding 3: the migration rewrites a <c>scale_key</c> that names a scale to the same scale's seed key, or
    /// its id when it has none, in every published version, each type's current copy and its draft. Every field binds the
    /// scale it bound before, a key that is not a name (or names no scale) is left alone, and a second run changes nothing.
    /// </summary>
    [Fact]
    public async Task TheMigration_RewritesEveryNameBinding_ToTheSameScalesSeedKeyOrId_AndNothingElse()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            int localId;
            int otherId;
            await using (var db = NewContext(schema))
            {
                var ladder = LadderOf("CPSA Paediatric Entrustment Scale v11.1", "cpsa:scale:v11.1");
                var localLadder = LadderOf("Local \"quoted\" ladder");
                var otherLadder = LadderOf("Other ladder");
                // A name T253 reads as an id; the validators refuse it now, but a scale could have been given it before.
                var digits = LadderOf("42");
                db.EntrustmentScales.AddRange(ladder, localLadder, otherLadder, digits);
                await db.SaveChangesAsync();
                localId = localLadder.Id;
                otherId = otherLadder.Id;

                var firstVersion = SchemaWith(
                    ("cpsa_by_name", "CPSA Paediatric Entrustment Scale v11.1", null),
                    ("local_by_name", "Local \"quoted\" ladder", null),
                    ("ghost_by_name", "Ghost ladder", "Bound as \"scale_key\": \"Other ladder\" before T253."),
                    ("digits", "42", null),
                    ("other_by_id", otherId.ToString(CultureInfo.InvariantCulture), null),
                    ("cpsa_by_seed_key", "seed:cpsa:scale:v11.1", null));
                var secondVersion = SchemaWith(("cpsa_by_name", "CPSA Paediatric Entrustment Scale v11.1", null));
                var draft = SchemaWith(("local_by_name", "Local \"quoted\" ladder", null), ("other_by_name", "Other ladder", null));

                var type = new ActivityType
                {
                    Key = "t253_old_mini_cex",
                    Name = "T253 old Mini-CEX",
                    Scope = ActivityScope.Global,
                    OwnerUserId = "admin-1",
                    CreatedOn = DateTime.UtcNow,
                    IsActive = true,
                    Version = 2,
                    SchemaJson = secondVersion,
                    WorkflowJson = Workflow,
                    CreditRulesJson = NoCredit,
                    DisplayFieldsJson = "[]",
                    StagingSchemaJson = draft,
                    StagingWorkflowJson = Workflow,
                    StagingCreditRulesJson = NoCredit,
                    StagingDisplayFieldsJson = "[]"
                };
                type.Versions.Add(VersionOf(1, firstVersion));
                type.Versions.Add(VersionOf(2, secondVersion));
                db.ActivityTypes.Add(type);
                await db.SaveChangesAsync();
            }

            var before = await BindingsAsync(schema);
            before["draft/other_by_name"].Key.Should().Be("Other ladder", "guard: the draft binds by name");

            await ExecuteAsync(schema, NameBindingRewriteSql());
            var after = await BindingsAsync(schema);

            var local = localId.ToString(CultureInfo.InvariantCulture);
            var other = otherId.ToString(CultureInfo.InvariantCulture);
            after.ToDictionary(entry => entry.Key, entry => entry.Value.Key).Should().BeEquivalentTo(new Dictionary<string, string>
            {
                ["version 1/cpsa_by_name"] = "seed:cpsa:scale:v11.1",
                ["version 1/local_by_name"] = local,
                ["version 1/ghost_by_name"] = "Ghost ladder",
                ["version 1/digits"] = "42",
                ["version 1/other_by_id"] = other,
                ["version 1/cpsa_by_seed_key"] = "seed:cpsa:scale:v11.1",
                ["version 2/cpsa_by_name"] = "seed:cpsa:scale:v11.1",
                ["current/cpsa_by_name"] = "seed:cpsa:scale:v11.1",
                ["draft/local_by_name"] = local,
                ["draft/other_by_name"] = other
            });
            after.ToDictionary(entry => entry.Key, entry => entry.Value.ScaleId).Should().BeEquivalentTo(
                before.ToDictionary(entry => entry.Key, entry => entry.Value.ScaleId),
                "every field binds the scale it bound before, and a key that bound nothing still binds nothing");
            after["version 1/ghost_by_name"].HelpText.Should().Be(
                "Bound as \"scale_key\": \"Other ladder\" before T253.", "a string that quotes a binding is not a binding");

            var stored = await SchemaColumnsAsync(schema);
            await ExecuteAsync(schema, NameBindingRewriteSql());
            (await SchemaColumnsAsync(schema)).Should().Equal(stored, "a second run finds nothing to rewrite");
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    /// <summary>
    /// The migration on the seeded corpus: a database booted before T253, whose seeded versions bind the two ladders by
    /// name, is left holding exactly what a fresh boot writes. So the seed refresher finds no difference to republish for,
    /// and no stored version binds a ladder by a name an administrator can change.
    /// </summary>
    [Fact]
    public async Task ADatabaseBootedBeforeT253_IsLeftHoldingExactlyWhatAFreshBootWrites()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            await using (var boot = NewContext(schema))
            {
                await new DataSeeder(boot).SeedAsync();
                await new PaediatricCatalogueSeeder(boot).SeedAsync();
            }

            var fresh = await SchemaColumnsAsync(schema);

            // Every seeded binding as the seeds wrote it before T253: by the ladder's name.
            var aged = await ExecuteAsync(
                schema,
                """
                UPDATE "ActivityTypeVersions" SET "SchemaJson" = replace(replace("SchemaJson"::text,
                    '"scale_key": "seed:cpsa:scale:v11.1"', '"scale_key": "CPSA Paediatric Entrustment Scale v11.1"'),
                    '"scale_key": "seed:demo:scale:o-r"', '"scale_key": "O-R Scale"')::jsonb;
                UPDATE "ActivityTypes" SET "SchemaJson" = replace(replace("SchemaJson"::text,
                    '"scale_key": "seed:cpsa:scale:v11.1"', '"scale_key": "CPSA Paediatric Entrustment Scale v11.1"'),
                    '"scale_key": "seed:demo:scale:o-r"', '"scale_key": "O-R Scale"')::jsonb
                    WHERE "SchemaJson" IS NOT NULL;
                """);
            aged.Should().BePositive("guard: the seeded types were reached");
            var agedColumns = await SchemaColumnsAsync(schema);
            agedColumns.Should().NotEqual(fresh, "guard: the aging rewrote something");
            agedColumns.Should().NotContain(
                column => column.Contains("\"scale_key\": \"seed:", StringComparison.Ordinal),
                "guard: no seeded binding is by seed key any more");

            await ExecuteAsync(schema, NameBindingRewriteSql());

            (await SchemaColumnsAsync(schema)).Should().Equal(fresh);
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    private const string Workflow =
        """{"version":1,"initial_state":"logged","states":[{"key":"logged","label":"Logged"}],"transitions":[]}""";

    private const string NoCredit = """{"counts_for":[]}""";

    /// <summary>The migration's statement, exactly as its <c>Up</c> hands it to <c>Sql()</c>.</summary>
    private static string NameBindingRewriteSql()
        => new T253_ScaleKeysBindByIdOrSeedKey().UpOperations.OfType<SqlOperation>().Should().ContainSingle().Which.Sql;

    private static EntrustmentScale LadderOf(string name, string? seedKey = null)
        => new()
        {
            SeedKey = seedKey,
            Name = name,
            Levels = [new EntrustmentLevel { Order = 1, Label = "Low" }, new EntrustmentLevel { Order = 2, Label = "High" }]
        };

    private static ActivityTypeVersion VersionOf(int version, string schemaJson)
        => new()
        {
            Version = version,
            SchemaJson = schemaJson,
            WorkflowJson = Workflow,
            CreditRulesJson = NoCredit,
            DisplayFieldsJson = "[]",
            PublishedByUserId = "admin-1",
            PublishedOn = DateTime.UtcNow
        };

    /// <summary>A one-section schema, a scale field per entry, serialised so a quote in a key or a help text is escaped.</summary>
    private static string SchemaWith(params (string Key, string ScaleKey, string? HelpText)[] fields)
        => JsonSerializer.Serialize(new
        {
            version = 1,
            sections = new[]
            {
                new
                {
                    key = "assessment",
                    title = "Assessment",
                    fields = fields
                        .Select(field => field.HelpText is null
                            ? (object)new { key = field.Key, type = "scale", label = field.Key, scale_key = field.ScaleKey }
                            : new { key = field.Key, type = "scale", label = field.Key, help_text = field.HelpText, scale_key = field.ScaleKey })
                        .ToArray()
                }
            }
        });

    private sealed record Binding(string Key, int? ScaleId, string? HelpText);

    /// <summary>
    /// Every scale field's binding in the three schema columns, keyed "version N/field", "current/field" or "draft/field",
    /// with the scale the shared resolver reads it as.
    /// </summary>
    private async Task<Dictionary<string, Binding>> BindingsAsync(string schema)
    {
        await using var db = NewContext(schema);
        var versions = await db.ActivityTypeVersions.AsNoTracking()
            .Select(version => new { version.Version, version.SchemaJson })
            .ToListAsync();
        var types = await db.ActivityTypes.AsNoTracking()
            .Select(type => new { type.SchemaJson, type.StagingSchemaJson })
            .ToListAsync();

        var columns = versions
            .Select(version => (Where: $"version {version.Version}", Json: version.SchemaJson))
            .Concat(types.Where(type => type.SchemaJson is not null).Select(type => (Where: "current", Json: type.SchemaJson!)))
            .Concat(types.Where(type => type.StagingSchemaJson is not null).Select(type => (Where: "draft", Json: type.StagingSchemaJson!)))
            .ToList();

        var fields = columns
            .SelectMany(column => FormSchemaParser.Parse(column.Json).Sections
                .SelectMany(section => section.Fields)
                .Where(field => field.ScaleKey is not null)
                .Select(field => (Where: $"{column.Where}/{field.Key}", Field: field)))
            .ToList();

        var resolved = await EntrustmentScaleBindings.ResolveAsync(db, fields.Select(entry => entry.Field.ScaleKey));
        return fields.ToDictionary(
            entry => entry.Where,
            entry => new Binding(
                entry.Field.ScaleKey!,
                resolved.TryGetValue(entry.Field.ScaleKey!, out var scaleId) ? scaleId : null,
                entry.Field.HelpText));
    }

    /// <summary>The three schema columns of every row, as the server renders them, in a stable order.</summary>
    private async Task<List<string>> SchemaColumnsAsync(string schema)
    {
        await using var connection = new NpgsqlConnection(SchemaConnectionString(schema));
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT 'version ' || "ActivityTypeId" || '/' || "Version" || ': ' || "SchemaJson"::text FROM "ActivityTypeVersions"
            UNION ALL
            SELECT 'current ' || "Id" || ': ' || COALESCE("SchemaJson"::text, '') FROM "ActivityTypes"
            UNION ALL
            SELECT 'draft ' || "Id" || ': ' || COALESCE("StagingSchemaJson"::text, '') FROM "ActivityTypes"
            ORDER BY 1
            """;

        var rows = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            rows.Add(reader.GetString(0));
        }

        return rows;
    }

    private async Task<int> ExecuteAsync(string schema, string sql)
    {
        await using var connection = new NpgsqlConnection(SchemaConnectionString(schema));
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return await command.ExecuteNonQueryAsync();
    }

    private async Task UpdateThroughTheAuditPipelineAsync(
        string schema, UpdateEntrustmentScaleCommand command, CommandLog commands, HandlerOutcome outcome)
    {
        await using var db = NewContext(schema, commands);
        var handler = new UpdateEntrustmentScaleCommandHandler(db);

        await new AuditPipelineBehavior<UpdateEntrustmentScaleCommand, UpdateEntrustmentScaleResult>(new AuditWriter(db), new FixedAuditContext())
            .Handle(
                command,
                async () =>
                {
                    UpdateEntrustmentScaleResult result = null!;
                    await outcome.RecordAsync(async () => result = await handler.Handle(command, CancellationToken.None));
                    return result;
                },
                CancellationToken.None);
    }

    private static string RatedSchema(string scaleKey)
        => $$"""
            {"version":1,"rated_level_field":"overall_level","sections":[{"key":"assessment","title":"Assessment","fields":[
              {"key":"overall_level","type":"scale","label":"Overall","scale_key":"{{scaleKey}}"}
            ]}]}
            """;

    private async Task DeleteThroughTheAuditPipelineAsync(string schema, int scaleId, CommandLog commands, HandlerOutcome outcome)
    {
        await using var db = NewContext(schema, commands);
        var command = new DeleteEntrustmentScaleCommand(scaleId, Administrator());
        var handler = new DeleteEntrustmentScaleCommandHandler(db);

        await new AuditPipelineBehavior<DeleteEntrustmentScaleCommand, Unit>(new AuditWriter(db), new FixedAuditContext())
            .Handle(
                command,
                async () =>
                {
                    await outcome.RecordAsync(() => handler.Handle(command, CancellationToken.None));
                    return Unit.Value;
                },
                CancellationToken.None);
    }

    private static ClaimsPrincipal Administrator()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "admin-1"),
                new Claim(ClaimTypes.Role, WombatRoles.Administrator)
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    private sealed class HandlerOutcome
    {
        public Exception? Thrown { get; private set; }

        public async Task RecordAsync(Func<Task> handle)
        {
            try
            {
                await handle();
            }
            catch (Exception exception)
            {
                Thrown = exception;
                throw;
            }
        }
    }

    private sealed class FixedAuditContext : IAuditContextProvider
    {
        public string? UserId => "admin-1";
        public string? UserDisplay => "Admin";
        public string? IpAddress => "10.0.0.0/24";
        public string? UserAgent => "Test/1.0";
        public int? InstitutionId => null;
        public void DeclareInstitution(int institutionId) { }
        public void DeclareActor(string userId, string display) { }
    }

    private sealed class CommandLog : DbCommandInterceptor
    {
        public List<string> Texts { get; } = [];

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            Texts.Add(command.CommandText);
            return ValueTask.FromResult(result);
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Texts.Add(command.CommandText);
            return ValueTask.FromResult(result);
        }
    }

    // ─── Schema helpers (as EntrustmentScaleDeletePostgresTests) ────────────

    private async Task<string> MigratedSchemaAsync()
    {
        var schema = await CreateSchemaAsync();

        await using var db = NewContext(schema);
        await db.Database.MigrateAsync();

        return schema;
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
            if (schema.StartsWith("it_", StringComparison.Ordinal))
            {
                await using var drop = connection.CreateCommand();
                drop.CommandText = $"DROP SCHEMA IF EXISTS \"{schema}\" CASCADE";
                await drop.ExecuteNonQueryAsync();
            }

            _schemas.Remove(schema);
        }
    }

    private ApplicationDbContext NewContext(string schema, IInterceptor? interceptor = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(SchemaConnectionString(schema));
        if (interceptor is not null)
        {
            options.AddInterceptors(interceptor);
        }

        return new ApplicationDbContext(options.Options);
    }

    private string SchemaConnectionString(string schema)
        => new NpgsqlConnectionStringBuilder(_baseConnectionString)
        {
            SearchPath = schema,
            Pooling = false
        }.ConnectionString;

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
