using System.Data.Common;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using Wombat.Application.Audit;
using Wombat.Application.Features.Epas.Commands.DeleteEntrustmentScale;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.Epas;

/// <summary>
/// T232 on a real PostgreSQL server: deleting an entrustment scale that is a sub-speciality's default is refused by the
/// handler, in a sentence naming the sub-speciality, before anything is sent that the ON DELETE RESTRICT foreign key on
/// <c>SubSpecialities.DefaultEntrustmentScaleId</c> would refuse.
/// </summary>
/// <remarks>
/// <para>
/// Only a real server enforces that foreign key: EF InMemory, which the handler tests run on, would delete the scale. So
/// this test shows both halves on the same rows: the handler refuses first and sends no DELETE, and the same removal
/// sent without asking is refused by the server with the raw exception the administrator used to see.
/// </para>
/// <para>
/// The command runs inside the real <see cref="AuditPipelineBehavior{TRequest,TResponse}" />, writing through the real
/// <see cref="AuditWriter" /> on the request's own context, as a request would, so the failure row's save would commit
/// anything the handler had tracked before it threw. The schema helpers follow <c>AuditOnRefusedSavePostgresTests</c>:
/// a schema of its own, registered before it is created and dropped in a <c>finally</c>, with
/// <see cref="DisposeAsync" /> as a backstop.
/// </para>
/// </remarks>
public sealed class EntrustmentScaleDeletePostgresTests : IAsyncLifetime
{
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";
    private const string ForeignKey = "FK_SubSpecialities_EntrustmentScales_DefaultEntrustmentScaleId";

    private readonly List<string> _schemas = [];
    private string _baseConnectionString = null!;

    public Task InitializeAsync()
    {
        _baseConnectionString = ResolveBaseConnectionString();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => DropSchemasAsync();

    [Fact]
    public async Task DeletingASubSpecialitysDefaultScale_IsRefusedByTheHandler_BeforeTheRestrictForeignKeyCouldRefuseIt()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var world = await SeedAsync(schema);

            var commands = new CommandLog();
            var outcome = new HandlerOutcome();
            var delete = () => DeleteThroughTheAuditPipelineAsync(schema, world.ScaleId, commands, outcome);

            var thrown = (await delete.Should().ThrowAsync<InvalidOperationException>()).Which;
            thrown.Should().BeSameAs(outcome.Thrown, "the caller sees the handler's refusal, not a database error");
            thrown.Message.Should().Be(
                "This entrustment scale is the default scale of the sub-speciality \"Neonatology\" (Paediatrics), so it " +
                "cannot be deleted. Change that sub-speciality's default entrustment scale to another scale, or to no " +
                "default, first.");
            thrown.InnerException.Should().BeNull("the handler refused before it saved, so no DbUpdateException lies inside");

            commands.Texts.Should().NotBeEmpty("the handler's reads were logged");
            commands.Texts.Should().NotContain(
                text => text.Contains("DELETE", StringComparison.OrdinalIgnoreCase),
                "nothing that removes a row reached the server, so the foreign key never had anything to refuse");

            await using (var read = NewContext(schema))
            {
                var row = (await read.AuditEntries
                        .AsNoTracking()
                        .Where(entry => entry.Action == nameof(DeleteEntrustmentScaleCommand))
                        .ToListAsync())
                    .Should().ContainSingle("the refused delete leaves its failure row").Subject;
                row.Success.Should().BeFalse();
                row.ErrorMessage.Should().Be(thrown.Message);

                var scale = await read.EntrustmentScales
                    .AsNoTracking()
                    .Include(entity => entity.Levels)
                    .SingleAsync(entity => entity.Id == world.ScaleId);
                scale.Levels.Should().HaveCount(2, "the failure row's save committed no removal the handler had tracked");

                (await read.SubSpecialities.AsNoTracking().SingleAsync(entity => entity.Id == world.SubSpecialityId))
                    .DefaultEntrustmentScaleId.Should().Be(world.ScaleId);
            }

            // The refusal the handler got ahead of: the same removal, sent without asking, meets the foreign key.
            await using (var bypass = NewContext(schema))
            {
                var scale = await bypass.EntrustmentScales
                    .Include(entity => entity.Levels)
                    .SingleAsync(entity => entity.Id == world.ScaleId);
                bypass.EntrustmentLevels.RemoveRange(scale.Levels);
                bypass.EntrustmentScales.Remove(scale);

                var save = () => bypass.SaveChangesAsync();
                var refused = (await save.Should().ThrowAsync<DbUpdateException>()).Which;
                var postgres = refused.InnerException.Should().BeOfType<PostgresException>().Subject;
                postgres.SqlState.Should().Be(PostgresErrorCodes.ForeignKeyViolation);
                postgres.ConstraintName.Should().Be(ForeignKey);
            }

            await using (var read = NewContext(schema))
            {
                (await read.EntrustmentScales.AnyAsync(entity => entity.Id == world.ScaleId))
                    .Should().BeTrue("the server refused the whole removal");
            }
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    private sealed record World(int ScaleId, int SubSpecialityId);

    private async Task<World> SeedAsync(string schema)
    {
        await using var db = NewContext(schema);

        var scale = new EntrustmentScale
        {
            Name = "T232 ladder",
            Levels =
            [
                new EntrustmentLevel { Order = 1, Label = "Observe" },
                new EntrustmentLevel { Order = 2, Label = "Independent" }
            ]
        };
        var college = new College { Name = "T232 College", ShortCode = "T232-C" };
        var speciality = new Speciality { College = college, Name = "Paediatrics" };
        var subSpeciality = new SubSpeciality
        {
            Speciality = speciality,
            Name = "Neonatology",
            DefaultEntrustmentScale = scale
        };

        db.SubSpecialities.Add(subSpeciality);
        await db.SaveChangesAsync();

        return new World(scale.Id, subSpeciality.Id);
    }

    private async Task DeleteThroughTheAuditPipelineAsync(
        string schema, int scaleId, CommandLog commands, HandlerOutcome outcome)
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

    /// <summary>The exception the handler itself threw, captured inside the pipeline, before the audit write runs.</summary>
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

    /// <summary>Every command the request's context sends, readers and non-queries alike.</summary>
    private sealed class CommandLog : DbCommandInterceptor
    {
        public List<string> Texts { get; } = [];

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            Texts.Add(command.CommandText);
            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            Texts.Add(command.CommandText);
            return ValueTask.FromResult(result);
        }

        public override InterceptionResult<int> NonQueryExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        {
            Texts.Add(command.CommandText);
            return result;
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Texts.Add(command.CommandText);
            return ValueTask.FromResult(result);
        }
    }

    // ─── Schema helpers (as AuditOnRefusedSavePostgresTests) ────────────────

    private async Task<string> MigratedSchemaAsync()
    {
        var schema = await CreateSchemaAsync();

        await using var db = NewContext(schema);
        await db.Database.MigrateAsync();

        return schema;
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

    /// <summary>Drops every schema this test created. Called from the test's finally and again from DisposeAsync.</summary>
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

    private ApplicationDbContext NewContext(string schema, IInterceptor? interceptor = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(SchemaConnectionString(schema));
        if (interceptor is not null)
        {
            options.AddInterceptors(interceptor);
        }

        return new ApplicationDbContext(options.Options);
    }

    /// <summary>The schema and nothing else on the search path, so an unqualified name can only ever resolve inside it.</summary>
    private string SchemaConnectionString(string schema)
        => new NpgsqlConnectionStringBuilder(_baseConnectionString)
        {
            SearchPath = schema,
            Pooling = false
        }.ConnectionString;

    /// <summary>The same resolution order as <c>MsfRespondEndpointFlowTests</c>.</summary>
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
