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
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.Epas;

/// <summary>
/// T254 on a real PostgreSQL server: a scale delete whose checks passed, but which lost the race to a write that took
/// the scale or one of its levels before its save, is refused in the words its checks would have used, with the
/// database's refusal underneath.
/// </summary>
/// <remarks>
/// <para>
/// The race is forced, not waited for: a <see cref="SaveChangesInterceptor" /> on the command's own context commits the
/// racing write through a second connection after every check has run and just before the delete's save reaches the
/// server. Each ON DELETE RESTRICT reference to a scale or a level is one case, and each would have been refused before
/// the save had it been there when the command checked (<c>EntrustmentScaleAdminHandlerTests</c>,
/// <c>EntrustmentScaleDeletePostgresTests</c>).
/// </para>
/// <para>
/// The command runs inside the real <see cref="AuditPipelineBehavior{TRequest,TResponse}" /> and <see cref="AuditWriter" />
/// on the request's own context, so the failure row's save would send anything the refused delete left tracked. The
/// schema helpers follow <c>EntrustmentScaleDeletePostgresTests</c>.
/// </para>
/// </remarks>
public sealed class EntrustmentScaleDeleteRacePostgresTests : IAsyncLifetime
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

    public enum Reference
    {
        SubSpecialityDefault,
        CurriculumItemPin,
        ScoredProgress,
        IssuedDecision,
        PendingDecision
    }

    [Theory]
    [InlineData(Reference.SubSpecialityDefault)]
    [InlineData(Reference.CurriculumItemPin)]
    [InlineData(Reference.ScoredProgress)]
    [InlineData(Reference.IssuedDecision)]
    [InlineData(Reference.PendingDecision)]
    public async Task AReferenceTakenAfterTheChecks_IsRefusedInTheChecksWords_AndDeletesNothing(Reference reference)
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var world = await SeedAsync(schema);

            var outcome = new HandlerOutcome();
            var delete = () => DeleteThroughTheAuditPipelineAsync(schema, world.ScaleId, outcome, () => TakeAsync(schema, world, reference));

            var thrown = (await delete.Should().ThrowAsync<InvalidOperationException>()).Which;
            thrown.Should().BeSameAs(outcome.Thrown);
            thrown.Message.Should().Be(reference switch
            {
                Reference.SubSpecialityDefault =>
                    "This entrustment scale is the default scale of the sub-speciality \"Neonatology\" (Paediatrics), so " +
                    "it cannot be deleted. Change that sub-speciality's default entrustment scale to another scale, or to " +
                    "no default, first.",
                Reference.CurriculumItemPin =>
                    "This entrustment scale is pinned to one or more curriculum items and cannot be deleted.",
                Reference.ScoredProgress =>
                    "Trainee progress has been scored against this entrustment scale and it cannot be deleted.",
                _ => "Levels of this entrustment scale are referenced by entrustment decisions and cannot be deleted."
            });

            // It was the foreign key, not the command's checks, that stopped the delete: the checks passed, and the
            // database's refusal is carried underneath, where the audit pipeline looks for it (T201).
            var refused = thrown.InnerException.Should().BeAssignableTo<DbUpdateException>().Subject;
            refused.InnerException.Should().BeOfType<PostgresException>()
                .Which.SqlState.Should().BeOneOf(PostgresErrorCodes.ForeignKeyViolation, PostgresErrorCodes.RestrictViolation);

            await using var read = NewContext(schema);
            var scale = await read.EntrustmentScales.AsNoTracking().Include(entity => entity.Levels)
                .SingleAsync(entity => entity.Id == world.ScaleId);
            scale.Levels.Should().HaveCount(2, "the refused delete removed nothing, and the failure row's save sent nothing");

            var row = (await read.AuditEntries.AsNoTracking()
                    .Where(entry => entry.Action == nameof(DeleteEntrustmentScaleCommand))
                    .ToListAsync())
                .Should().ContainSingle().Subject;
            row.Success.Should().BeFalse();
            row.ErrorMessage.Should().Be(thrown.Message);
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task AScaleAnotherDeleteRemovedFirst_IsReportedNotFound()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var world = await SeedAsync(schema);

            var outcome = new HandlerOutcome();
            var delete = () => DeleteThroughTheAuditPipelineAsync(schema, world.ScaleId, outcome, async () =>
            {
                await using var racing = NewContext(schema);
                await racing.EntrustmentScales.Where(entity => entity.Id == world.ScaleId).ExecuteDeleteAsync();
            });

            var thrown = (await delete.Should().ThrowAsync<InvalidOperationException>()).Which;
            thrown.Message.Should().Be($"Entrustment scale {world.ScaleId} was not found.");
            thrown.InnerException.Should().BeOfType<DbUpdateConcurrencyException>(
                "the delete's rows were already gone, which EF reports as a concurrency conflict");

            await using var read = NewContext(schema);
            (await read.AuditEntries.AsNoTracking().SingleAsync(entry => entry.Action == nameof(DeleteEntrustmentScaleCommand)))
                .ErrorMessage.Should().Be(thrown.Message);
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task ALevelAnotherSaveRemovedFirst_IsReportedAsAChange_AndASecondAttemptDeletes()
    {
        // Another save removed the scale's top level after this delete loaded it, as UpdateEntrustmentScale does. Nothing
        // refers to the scale, so no check applies; the level's removal changed nothing, and EF refused the whole save as a
        // concurrency conflict. The administrator is told the scale changed, not EF's row counts (T254 review).
        try
        {
            var schema = await MigratedSchemaAsync();
            var world = await SeedAsync(schema);

            var outcome = new HandlerOutcome();
            var delete = () => DeleteThroughTheAuditPipelineAsync(schema, world.ScaleId, outcome, async () =>
            {
                await using var racing = NewContext(schema);
                await racing.EntrustmentLevels.Where(level => level.ScaleId == world.ScaleId && level.Order == 2).ExecuteDeleteAsync();
            });

            var thrown = (await delete.Should().ThrowAsync<InvalidOperationException>()).Which;
            thrown.Should().BeSameAs(outcome.Thrown);
            thrown.Message.Should().Be(
                "This entrustment scale was changed by another save while it was being deleted, so it has not been deleted. " +
                "Try again.");
            thrown.InnerException.Should().BeOfType<DbUpdateConcurrencyException>(
                "the refusal stays underneath, where the audit pipeline looks for it (T201)");

            await using (var read = NewContext(schema))
            {
                (await read.EntrustmentLevels.AsNoTracking().Where(level => level.ScaleId == world.ScaleId).Select(level => level.Order).ToListAsync())
                    .Should().Equal(new[] { 1 }, "the refused delete removed nothing; only the racing save's removal stands");
                (await read.AuditEntries.AsNoTracking().SingleAsync(entry => entry.Action == nameof(DeleteEntrustmentScaleCommand)))
                    .ErrorMessage.Should().Be(thrown.Message);
            }

            // What the message promises: the second attempt reads the levels afresh and deletes the scale.
            await DeleteThroughTheAuditPipelineAsync(schema, world.ScaleId, new HandlerOutcome(), () => Task.CompletedTask);

            await using var after = NewContext(schema);
            (await after.EntrustmentScales.AsNoTracking().AnyAsync(entity => entity.Id == world.ScaleId)).Should().BeFalse();
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task ARefusalNoCheckExplains_IsLeftAsTheDatabaseGaveIt()
    {
        // A reference the command's checks do not know about, standing in for one a later table adds without a check: a
        // row that holds the scale's lower level ON DELETE RESTRICT, taken after the checks. No check applies and the
        // refusal is not a concurrency conflict, so the delete's refusal is the database's own.
        try
        {
            var schema = await MigratedSchemaAsync();
            var world = await SeedAsync(schema);

            var outcome = new HandlerOutcome();
            var delete = () => DeleteThroughTheAuditPipelineAsync(schema, world.ScaleId, outcome, async () =>
            {
                await using var racing = NewContext(schema);
                await racing.Database.ExecuteSqlRawAsync(
                    "CREATE TABLE \"T254UnknownReferences\" (\"LevelId\" integer NOT NULL " +
                    "REFERENCES \"EntrustmentLevels\" (\"Id\") ON DELETE RESTRICT)");
                await racing.Database.ExecuteSqlAsync(
                    $"INSERT INTO \"T254UnknownReferences\" (\"LevelId\") VALUES ({world.LowerLevelId})");
            });

            var thrown = (await delete.Should().ThrowAsync<DbUpdateException>()).Which;
            thrown.Should().BeSameAs(outcome.Thrown, "the handler rethrew the database's refusal as it came");
            thrown.Should().NotBeOfType<DbUpdateConcurrencyException>();
            thrown.InnerException.Should().BeOfType<PostgresException>()
                .Which.SqlState.Should().BeOneOf(PostgresErrorCodes.ForeignKeyViolation, PostgresErrorCodes.RestrictViolation);

            await using var read = NewContext(schema);
            (await read.EntrustmentLevels.AsNoTracking().CountAsync(level => level.ScaleId == world.ScaleId))
                .Should().Be(2, "the refused delete removed nothing");
            (await read.AuditEntries.AsNoTracking().SingleAsync(entry => entry.Action == nameof(DeleteEntrustmentScaleCommand)))
                .Success.Should().BeFalse();
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task ALevelAnotherSaveAddedAfterTheLoad_IsAskedAboutToo()
    {
        // Another save added a third level after this delete loaded the scale's two, and a decision was staged on it. The
        // delete removes only the levels it loaded; the scale's own removal cascades to the third, and the decision's
        // RESTRICT refuses that. The read-back asks about the levels stored now, so the refusal is the decisions check's,
        // which a check against the loaded levels would miss.
        try
        {
            var schema = await MigratedSchemaAsync();
            var world = await SeedAsync(schema);

            var outcome = new HandlerOutcome();
            var delete = () => DeleteThroughTheAuditPipelineAsync(schema, world.ScaleId, outcome, async () =>
            {
                await using var racing = NewContext(schema);
                var added = new EntrustmentLevel { ScaleId = world.ScaleId, Order = 3, Label = "Supervise others" };
                racing.EntrustmentLevels.Add(added);
                await racing.SaveChangesAsync();

                racing.PendingEntrustmentDecisions.Add(PendingEntrustmentDecision.Stage(
                    world.ReviewId, world.EpaId, added.Id, new DateOnly(2026, 7, 1), expiresOn: null,
                    "Consistent across the period.", [1], "chair-1", DateTime.UtcNow));
                await racing.SaveChangesAsync();
            });

            var thrown = (await delete.Should().ThrowAsync<InvalidOperationException>()).Which;
            thrown.Should().BeSameAs(outcome.Thrown);
            thrown.Message.Should().Be("Levels of this entrustment scale are referenced by entrustment decisions and cannot be deleted.");
            thrown.InnerException.Should().BeAssignableTo<DbUpdateException>()
                .Which.InnerException.Should().BeOfType<PostgresException>()
                .Which.SqlState.Should().BeOneOf(PostgresErrorCodes.ForeignKeyViolation, PostgresErrorCodes.RestrictViolation);

            await using var read = NewContext(schema);
            (await read.EntrustmentLevels.AsNoTracking().CountAsync(level => level.ScaleId == world.ScaleId))
                .Should().Be(3, "the refused delete removed nothing");
            (await read.AuditEntries.AsNoTracking().SingleAsync(entry => entry.Action == nameof(DeleteEntrustmentScaleCommand)))
                .ErrorMessage.Should().Be(thrown.Message);
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task AReadBackThatFails_RethrowsTheDatabasesRefusal_SoTheFailureRowDeletesNothing()
    {
        // A sub-speciality takes the scale as its default after the checks, and the foreign key refuses the delete. The
        // read-back then fails, as a dropped connection would, and by then the default has been changed back. Had the
        // read-back's own exception escaped, it would not carry the refusal, the audit pipeline would write its failure
        // row the ordinary way, and that save would send the still-tracked removal again, which nothing now refuses: the
        // scale would be deleted under a row recording that it was not (T254 review).
        try
        {
            var schema = await MigratedSchemaAsync();
            var world = await SeedAsync(schema);

            var outcome = new HandlerOutcome();
            var delete = () => DeleteThroughTheAuditPipelineAsync(
                schema,
                world.ScaleId,
                outcome,
                () => TakeAsync(schema, world, Reference.SubSpecialityDefault),
                readBackFails: async () =>
                {
                    await using var racing = NewContext(schema);
                    await racing.SubSpecialities.Where(entity => entity.Id == world.SubSpecialityId)
                        .ExecuteUpdateAsync(set => set.SetProperty(entity => entity.DefaultEntrustmentScaleId, (int?)null));
                });

            var thrown = (await delete.Should().ThrowAsync<DbUpdateException>()).Which;
            thrown.Should().BeSameAs(outcome.Thrown, "the handler rethrew the database's refusal, not the read-back's failure");
            thrown.InnerException.Should().BeOfType<PostgresException>()
                .Which.SqlState.Should().BeOneOf(PostgresErrorCodes.ForeignKeyViolation, PostgresErrorCodes.RestrictViolation);

            await using var read = NewContext(schema);
            (await read.EntrustmentLevels.AsNoTracking().CountAsync(level => level.ScaleId == world.ScaleId))
                .Should().Be(2, "the failure row's save discarded the refused removal instead of sending it again");
            (await read.AuditEntries.AsNoTracking().SingleAsync(entry => entry.Action == nameof(DeleteEntrustmentScaleCommand)))
                .Success.Should().BeFalse();
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    private sealed record World(
        int ScaleId, int LowerLevelId, int SubSpecialityId, int EpaId, int CurriculumItemId, int ProgressId, int ReviewId);

    /// <summary>
    /// A two-level scale nothing refers to, and one of each row that could come to refer to it: a sub-speciality with no
    /// default, an unpinned curriculum item, a progress row scored on no scale, and a committee review with no decision.
    /// </summary>
    private async Task<World> SeedAsync(string schema)
    {
        await using var db = NewContext(schema);

        var scale = new EntrustmentScale
        {
            Name = "T254 ladder",
            Levels =
            [
                new EntrustmentLevel { Order = 1, Label = "Observe" },
                new EntrustmentLevel { Order = 2, Label = "Independent" }
            ]
        };
        var college = new College { Name = "T254 College", ShortCode = "T254-C" };
        var speciality = new Speciality { College = college, Name = "Paediatrics" };
        var subSpeciality = new SubSpeciality { Speciality = speciality, Name = "Neonatology" };
        var epa = new Epa { SubSpeciality = subSpeciality, Code = "T254-001", Title = "Resuscitate a newborn" };
        var curriculum = new Curriculum
        {
            SubSpeciality = subSpeciality,
            Name = "T254 curriculum",
            Version = "1",
            EffectiveFrom = new DateOnly(2026, 1, 1)
        };
        var item = new CurriculumItem
        {
            Curriculum = curriculum,
            Epa = epa,
            RequiredCount = 1,
            MinimumLevelOrder = 1,
            WindowMonths = 12
        };
        var institution = new Institution { Name = "T254 Hospital", ShortCode = "T254-H" };

        db.EntrustmentScales.Add(scale);
        db.CurriculumItems.Add(item);
        db.Institutions.Add(institution);
        await db.SaveChangesAsync();

        var panel = new DecisionPanel
        {
            Name = "T254 CCC",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = institution.Id,
            CreatedOn = DateTime.UtcNow,
            Members = [new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair }]
        };
        db.DecisionPanels.Add(panel);
        await db.SaveChangesAsync();

        var progress = new CurriculumItemProgress
        {
            CurriculumItemId = item.Id,
            TraineeUserId = "trainee-1",
            AcademicYear = 2026,
            Semester = 1
        };
        var review = new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 1,
            PanelId = panel.Id,
            TraineeUserId = "trainee-1",
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 6, 30),
            ScheduledOn = new DateOnly(2026, 7, 1)
        };
        db.CurriculumItemProgresses.Add(progress);
        db.CommitteeReviews.Add(review);
        await db.SaveChangesAsync();

        var lowerLevelId = scale.Levels.Single(level => level.Order == 1).Id;
        return new World(scale.Id, lowerLevelId, subSpeciality.Id, epa.Id, item.Id, progress.Id, review.Id);
    }

    /// <summary>The racing write: another administrator's save that makes something refer to the scale or its lower level.</summary>
    private async Task TakeAsync(string schema, World world, Reference reference)
    {
        await using var racing = NewContext(schema);

        switch (reference)
        {
            case Reference.SubSpecialityDefault:
                await racing.SubSpecialities.Where(entity => entity.Id == world.SubSpecialityId)
                    .ExecuteUpdateAsync(set => set.SetProperty(entity => entity.DefaultEntrustmentScaleId, world.ScaleId));
                break;

            case Reference.CurriculumItemPin:
                await racing.CurriculumItems.Where(entity => entity.Id == world.CurriculumItemId)
                    .ExecuteUpdateAsync(set => set.SetProperty(entity => entity.ScaleId, world.ScaleId));
                break;

            case Reference.ScoredProgress:
                await racing.CurriculumItemProgresses.Where(entity => entity.Id == world.ProgressId)
                    .ExecuteUpdateAsync(set => set.SetProperty(entity => entity.MinimumLevelScaleId, world.ScaleId));
                break;

            case Reference.IssuedDecision:
                racing.EntrustmentDecisions.Add(EntrustmentDecision.Issue(
                    "trainee-1", world.EpaId, world.LowerLevelId, new DateOnly(2026, 7, 1), expiresOn: null,
                    world.ReviewId, "chair-1", "Consistent across the period.", StarEvidence.One()));
                await racing.SaveChangesAsync();
                break;

            case Reference.PendingDecision:
                racing.PendingEntrustmentDecisions.Add(PendingEntrustmentDecision.Stage(
                    world.ReviewId, world.EpaId, world.LowerLevelId, new DateOnly(2026, 7, 1), expiresOn: null,
                    "Consistent across the period.", [1], "chair-1", DateTime.UtcNow));
                await racing.SaveChangesAsync();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(reference), reference, null);
        }
    }

    private async Task DeleteThroughTheAuditPipelineAsync(
        string schema, int scaleId, HandlerOutcome outcome, Func<Task> race, Func<Task>? readBackFails = null)
    {
        await using var db = NewContext(schema, race, readBackFails);
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
