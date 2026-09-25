using System.Data.Common;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using Wombat.Application.Audit;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Commands.CreateSubSpeciality;
using Wombat.Application.Features.Institutions.Commands.UpdateSubSpeciality;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.Institutions;

/// <summary>
/// T254 on a real PostgreSQL server: a sub-speciality's create or update that the database refuses says why. Only the
/// unique index on <c>(SpecialityId, Name)</c> is reported as a duplicate name; a speciality or default scale deleted after
/// the command checked it, or the sub-speciality itself, is reported as not found; anything else is left as the database
/// gave it.
/// </summary>
/// <remarks>
/// <para>
/// Before T254 both commands reported every refused save as "A sub-speciality with the same name already exists for this
/// speciality.", a foreign key included. The races are forced by a <see cref="SaveChangesInterceptor" /> on the command's
/// own context that commits the other save through a second connection after the command's checks and just before its
/// own save reaches the server.
/// </para>
/// <para>
/// Each command runs inside the real <see cref="AuditPipelineBehavior{TRequest,TResponse}" /> and
/// <see cref="AuditWriter" /> on the request's own context, so the failure row's save would send anything the refused save
/// left tracked. The schema helpers follow <c>EntrustmentScaleDeletePostgresTests</c>.
/// </para>
/// </remarks>
public sealed class SubSpecialitySaveRacePostgresTests : IAsyncLifetime
{
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";
    private const string DuplicateName = "A sub-speciality with the same name already exists for this speciality.";

    private readonly List<string> _schemas = [];
    private string _baseConnectionString = null!;

    public Task InitializeAsync()
    {
        _baseConnectionString = ResolveBaseConnectionString();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => DropSchemasAsync();

    public enum UpdateRace
    {
        /// <summary>Another sub-speciality of Paediatrics is saved under the new name.</summary>
        NameTaken,

        /// <summary>The default scale the update sets is deleted.</summary>
        DefaultScaleDeleted,

        /// <summary>Surgery, the speciality the update moves the sub-speciality to, is deleted.</summary>
        TargetSpecialityDeleted,

        /// <summary>The sub-speciality itself is deleted, while a sibling already holds the name it is being given.</summary>
        SubSpecialityDeleted
    }

    [Theory]
    [InlineData(UpdateRace.NameTaken)]
    [InlineData(UpdateRace.DefaultScaleDeleted)]
    [InlineData(UpdateRace.TargetSpecialityDeleted)]
    [InlineData(UpdateRace.SubSpecialityDeleted)]
    public async Task AnUpdateThatLosesARace_IsRefusedInWordsForWhatHappened_AndSavesNothing(UpdateRace race)
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var world = await SeedAsync(schema);

            // Neonatology is renamed and given the ladder as its default; in one case it also moves to Surgery. In the last
            // case it is given the name Perinatology already holds, which on its own would be a duplicate name.
            var command = new UpdateSubSpecialityCommand(
                world.NeonatologyId,
                race == UpdateRace.TargetSpecialityDeleted ? world.SurgeryId : world.PaediatricsId,
                race == UpdateRace.SubSpecialityDeleted ? "Perinatology" : "Neonatal medicine",
                Description: null,
                IsActive: true,
                world.LadderId,
                Administrator());

            Func<Task> racing = race switch
            {
                UpdateRace.NameTaken => () => InsertSubSpecialityAsync(schema, world.PaediatricsId, "Neonatal medicine"),
                UpdateRace.DefaultScaleDeleted => () => DeleteAsync<EntrustmentScale>(schema, world.LadderId),
                UpdateRace.TargetSpecialityDeleted => () => DeleteAsync<Speciality>(schema, world.SurgeryId),
                _ => () => DeleteAsync<SubSpeciality>(schema, world.NeonatologyId)
            };

            var outcome = new HandlerOutcome();
            var update = () => SendThroughTheAuditPipelineAsync(
                schema, command, db => new UpdateSubSpecialityCommandHandler(db), outcome, racing);

            var thrown = (await update.Should().ThrowAsync<InvalidOperationException>()).Which;
            thrown.Should().BeSameAs(outcome.Thrown);
            thrown.Message.Should().Be(race switch
            {
                UpdateRace.NameTaken => DuplicateName,
                UpdateRace.DefaultScaleDeleted => $"Entrustment scale {world.LadderId} was not found.",
                UpdateRace.TargetSpecialityDeleted => $"Speciality {world.SurgeryId} was not found.",
                _ => $"Sub-speciality {world.NeonatologyId} was not found."
            });

            // The database's refusal is underneath, where the audit pipeline looks for it (T201).
            var refused = thrown.InnerException.Should().BeAssignableTo<DbUpdateException>().Subject;
            if (race == UpdateRace.SubSpecialityDeleted)
            {
                refused.Should().BeOfType<DbUpdateConcurrencyException>("an update of a deleted row changes nothing");
            }
            else
            {
                refused.InnerException.Should().BeOfType<PostgresException>().Which.SqlState.Should().Be(
                    race == UpdateRace.NameTaken ? PostgresErrorCodes.UniqueViolation : PostgresErrorCodes.ForeignKeyViolation);
            }

            await using var read = NewContext(schema);
            if (race != UpdateRace.SubSpecialityDeleted)
            {
                var stored = await read.SubSpecialities.AsNoTracking().SingleAsync(entity => entity.Id == world.NeonatologyId);
                stored.Name.Should().Be("Neonatology", "the refused update saved nothing, and the failure row's save sent nothing");
                stored.SpecialityId.Should().Be(world.PaediatricsId);
                stored.DefaultEntrustmentScaleId.Should().BeNull();
            }

            await ShouldHaveOneFailureRowAsync(read, nameof(UpdateSubSpecialityCommand), thrown.Message);
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    public enum CreateRace
    {
        /// <summary>Another sub-speciality of Paediatrics is saved under the same name.</summary>
        NameTaken,

        /// <summary>The speciality the sub-speciality is created in is deleted.</summary>
        SpecialityDeleted
    }

    [Theory]
    [InlineData(CreateRace.NameTaken)]
    [InlineData(CreateRace.SpecialityDeleted)]
    public async Task ACreateThatLosesARace_IsRefusedInWordsForWhatHappened_AndSavesNothing(CreateRace race)
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var world = await SeedAsync(schema);

            var specialityId = race == CreateRace.NameTaken ? world.PaediatricsId : world.SurgeryId;
            var command = new CreateSubSpecialityCommand(specialityId, "Neonatal medicine", Description: null, Administrator());

            Func<Task> racing = race == CreateRace.NameTaken
                ? () => InsertSubSpecialityAsync(schema, world.PaediatricsId, "Neonatal medicine")
                : () => DeleteAsync<Speciality>(schema, world.SurgeryId);

            var outcome = new HandlerOutcome();
            var create = () => SendThroughTheAuditPipelineAsync(
                schema, command, db => new CreateSubSpecialityCommandHandler(db), outcome, racing);

            var thrown = (await create.Should().ThrowAsync<InvalidOperationException>()).Which;
            thrown.Should().BeSameAs(outcome.Thrown);
            thrown.Message.Should().Be(race == CreateRace.NameTaken ? DuplicateName : $"Speciality {world.SurgeryId} was not found.");
            thrown.InnerException.Should().BeAssignableTo<DbUpdateException>()
                .Which.InnerException.Should().BeOfType<PostgresException>()
                .Which.SqlState.Should().Be(
                    race == CreateRace.NameTaken ? PostgresErrorCodes.UniqueViolation : PostgresErrorCodes.ForeignKeyViolation);

            await using var read = NewContext(schema);
            (await read.SubSpecialities.AsNoTracking().CountAsync(entity => entity.Name == "Neonatal medicine"))
                .Should().Be(race == CreateRace.NameTaken ? 1 : 0, "only the racing save's row, if any, was stored");
            await ShouldHaveOneFailureRowAsync(read, nameof(CreateSubSpecialityCommand), thrown.Message);
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ANameASiblingAlreadyHolds_IsStillADuplicateName(bool create)
    {
        // No race: the ordinary duplicate, which neither command checks for before saving, still gets its own words.
        try
        {
            var schema = await MigratedSchemaAsync();
            var world = await SeedAsync(schema);

            var outcome = new HandlerOutcome();
            Func<Task> save = create
                ? () => SendThroughTheAuditPipelineAsync(
                    schema,
                    new CreateSubSpecialityCommand(world.PaediatricsId, "Perinatology", null, Administrator()),
                    db => new CreateSubSpecialityCommandHandler(db),
                    outcome)
                : () => SendThroughTheAuditPipelineAsync(
                    schema,
                    new UpdateSubSpecialityCommand(world.NeonatologyId, world.PaediatricsId, "Perinatology", null, true, null, Administrator()),
                    db => new UpdateSubSpecialityCommandHandler(db),
                    outcome);

            var thrown = (await save.Should().ThrowAsync<InvalidOperationException>()).Which;
            thrown.Message.Should().Be(DuplicateName);
            thrown.InnerException.Should().BeAssignableTo<DbUpdateException>();

            await using var read = NewContext(schema);
            (await read.SubSpecialities.AsNoTracking().CountAsync(entity => entity.Name == "Perinatology")).Should().Be(1);
            (await read.SubSpecialities.AsNoTracking().SingleAsync(entity => entity.Id == world.NeonatologyId))
                .Name.Should().Be("Neonatology");
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task ARefusalNoneOfTheReadBacksExplains_IsLeftAsTheDatabaseGaveIt_NotCalledADuplicateName()
    {
        // A name longer than the column, sent past the validator (which refuses it first in a request), is refused by the
        // database for its length. No sibling holds it and every row the update names exists, so no refusal of the
        // command's own applies, and the database's is rethrown as it came.
        try
        {
            var schema = await MigratedSchemaAsync();
            var world = await SeedAsync(schema);

            var outcome = new HandlerOutcome();
            var update = () => SendThroughTheAuditPipelineAsync(
                schema,
                new UpdateSubSpecialityCommand(world.NeonatologyId, world.PaediatricsId, new string('N', 201), null, true, world.LadderId, Administrator()),
                db => new UpdateSubSpecialityCommandHandler(db),
                outcome);

            var thrown = (await update.Should().ThrowAsync<DbUpdateException>()).Which;
            thrown.Should().BeSameAs(outcome.Thrown, "the handler rethrew the database's refusal as it came");
            thrown.InnerException.Should().BeOfType<PostgresException>()
                .Which.SqlState.Should().Be(PostgresErrorCodes.StringDataRightTruncation);

            await using var read = NewContext(schema);
            var stored = await read.SubSpecialities.AsNoTracking().SingleAsync(entity => entity.Id == world.NeonatologyId);
            stored.Name.Should().Be("Neonatology");
            stored.DefaultEntrustmentScaleId.Should().BeNull();
            (await read.AuditEntries.AsNoTracking().SingleAsync(entry => entry.Action == nameof(UpdateSubSpecialityCommand)))
                .Success.Should().BeFalse();
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AReadBackThatFails_RethrowsTheDatabasesRefusal_SoTheFailureRowSavesNothing(bool create)
    {
        // Another sub-speciality of Paediatrics takes the name after the command's checks, and the unique index refuses the
        // save. The read-back then fails, as a dropped connection would, and by then the other row is gone again. Had the
        // read-back's own exception escaped, it would not carry the refusal, the audit pipeline would write its failure
        // row the ordinary way, and that save would send the still-tracked insert or rename again, which nothing now
        // refuses: it would commit under a row recording that it failed (T254 review).
        try
        {
            var schema = await MigratedSchemaAsync();
            var world = await SeedAsync(schema);

            Func<Task> race = () => InsertSubSpecialityAsync(schema, world.PaediatricsId, "Neonatal medicine");
            Func<Task> readBackFails = async () =>
            {
                await using var racing = NewContext(schema);
                (await racing.SubSpecialities.Where(entity => entity.Name == "Neonatal medicine").ExecuteDeleteAsync())
                    .Should().Be(1, "the racing row is removed again while the read-back is down");
            };

            var outcome = new HandlerOutcome();
            Func<Task> save = create
                ? () => SendThroughTheAuditPipelineAsync(
                    schema,
                    new CreateSubSpecialityCommand(world.PaediatricsId, "Neonatal medicine", null, Administrator()),
                    db => new CreateSubSpecialityCommandHandler(db),
                    outcome,
                    race,
                    readBackFails)
                : () => SendThroughTheAuditPipelineAsync(
                    schema,
                    new UpdateSubSpecialityCommand(world.NeonatologyId, world.PaediatricsId, "Neonatal medicine", null, true, null, Administrator()),
                    db => new UpdateSubSpecialityCommandHandler(db),
                    outcome,
                    race,
                    readBackFails);

            var thrown = (await save.Should().ThrowAsync<DbUpdateException>()).Which;
            thrown.Should().BeSameAs(outcome.Thrown, "the handler rethrew the database's refusal, not the read-back's failure");
            thrown.InnerException.Should().BeOfType<PostgresException>()
                .Which.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);

            await using var read = NewContext(schema);
            (await read.SubSpecialities.AsNoTracking().CountAsync(entity => entity.Name == "Neonatal medicine"))
                .Should().Be(0, "the failure row's save discarded the refused save instead of sending it again");
            (await read.SubSpecialities.AsNoTracking().SingleAsync(entity => entity.Id == world.NeonatologyId))
                .Name.Should().Be("Neonatology");
            (await read.AuditEntries.AsNoTracking()
                    .SingleAsync(entry => entry.Action == (create ? nameof(CreateSubSpecialityCommand) : nameof(UpdateSubSpecialityCommand))))
                .Success.Should().BeFalse();
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    private sealed record World(int PaediatricsId, int SurgeryId, int NeonatologyId, int LadderId);

    /// <summary>
    /// Paediatrics with Neonatology (no default scale) and Perinatology, an empty Surgery, and a ladder nothing refers to.
    /// </summary>
    private async Task<World> SeedAsync(string schema)
    {
        await using var db = NewContext(schema);

        var college = new College { Name = "T254 College", ShortCode = "T254-C" };
        var paediatrics = new Speciality { College = college, Name = "Paediatrics" };
        var surgery = new Speciality { College = college, Name = "Surgery" };
        var neonatology = new SubSpeciality { Speciality = paediatrics, Name = "Neonatology" };
        var perinatology = new SubSpeciality { Speciality = paediatrics, Name = "Perinatology" };
        var ladder = new EntrustmentScale
        {
            Name = "T254 ladder",
            Levels =
            [
                new EntrustmentLevel { Order = 1, Label = "Observe" },
                new EntrustmentLevel { Order = 2, Label = "Independent" }
            ]
        };

        db.Specialities.Add(surgery);
        db.SubSpecialities.AddRange(neonatology, perinatology);
        db.EntrustmentScales.Add(ladder);
        await db.SaveChangesAsync();

        return new World(paediatrics.Id, surgery.Id, neonatology.Id, ladder.Id);
    }

    private async Task InsertSubSpecialityAsync(string schema, int specialityId, string name)
    {
        await using var racing = NewContext(schema);
        racing.SubSpecialities.Add(new SubSpeciality { SpecialityId = specialityId, Name = name });
        await racing.SaveChangesAsync();
    }

    private async Task DeleteAsync<TEntity>(string schema, int id)
        where TEntity : class
    {
        await using var racing = NewContext(schema);
        (await racing.Set<TEntity>().Where(entity => EF.Property<int>(entity, "Id") == id).ExecuteDeleteAsync())
            .Should().Be(1, "the racing save deletes the row");
    }

    private static async Task ShouldHaveOneFailureRowAsync(ApplicationDbContext read, string action, string message)
    {
        var row = (await read.AuditEntries.AsNoTracking().Where(entry => entry.Action == action).ToListAsync())
            .Should().ContainSingle("the refused save leaves its failure row").Subject;
        row.Success.Should().BeFalse();
        row.ErrorMessage.Should().Be(message);
    }

    private async Task<SubSpecialityDto> SendThroughTheAuditPipelineAsync<TCommand>(
        string schema,
        TCommand command,
        Func<ApplicationDbContext, IRequestHandler<TCommand, SubSpecialityDto>> handlerFor,
        HandlerOutcome outcome,
        Func<Task>? race = null,
        Func<Task>? readBackFails = null)
        where TCommand : IRequest<SubSpecialityDto>
    {
        await using var db = NewContext(schema, race, readBackFails);
        var handler = handlerFor(db);

        return await new AuditPipelineBehavior<TCommand, SubSpecialityDto>(new AuditWriter(db), new FixedAuditContext())
            .Handle(
                command,
                async () =>
                {
                    SubSpecialityDto? saved = null;
                    await outcome.RecordAsync(async () => saved = await handler.Handle(command, CancellationToken.None));
                    return saved!;
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

    /// <summary>Runs the racing write once, after the command's checks and just before its first save reaches the database.</summary>
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

    /// <summary>
    /// Fails the first query the context sends after a refused save, which is the read-back's first, as a dropped
    /// connection would; the action runs just before, as the other writes that go by while it is down.
    /// </summary>
    private sealed class ReadBackFails(Func<Task> action) : DbCommandInterceptor, ISaveChangesInterceptor
    {
        private Func<Task>? _action = action;
        private int _saveRefused;

        public Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
        {
            Interlocked.Exchange(ref _saveRefused, 1);
            return Task.CompletedTask;
        }

        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            if (Volatile.Read(ref _saveRefused) == 1 && Interlocked.Exchange(ref _action, null) is { } run)
            {
                await run();
                throw new NpgsqlException("Exception while reading from stream", new IOException("The connection was reset."));
            }

            return result;
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

    private ApplicationDbContext NewContext(string schema, Func<Task>? beforeFirstSave = null, Func<Task>? readBackFails = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(SchemaConnectionString(schema));
        if (beforeFirstSave is not null)
        {
            options.AddInterceptors(new BeforeFirstSave(beforeFirstSave));
        }

        if (readBackFails is not null)
        {
            options.AddInterceptors(new ReadBackFails(readBackFails));
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
