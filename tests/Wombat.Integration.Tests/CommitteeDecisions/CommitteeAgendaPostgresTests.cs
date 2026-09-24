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
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.CommitteeDecisions;

/// <summary>
/// T131 slice 4 on a real PostgreSQL server: a review sits for an academic period, one open binding review per trainee,
/// panel and period, and its agenda lines hold their states by the table's own checks.
/// </summary>
/// <remarks>
/// <para>
/// EF InMemory, which the handler tests run on, enforces no check constraint, no filtered unique index and no migration
/// SQL, so they are shown here. The ratify that closes an agenda in the same save as it issues the STAR a Decided line
/// names is shown on Postgres by <c>CommitteeEvidenceSnapshotPostgresTests</c>.
/// </para>
/// <para>
/// The schema helpers follow <c>DecisionPanelBodyPostgresTests</c>: each test runs on a schema of its own, dropped in a
/// <c>finally</c>, with <see cref="DisposeAsync" /> as a backstop.
/// </para>
/// </remarks>
public sealed class CommitteeAgendaPostgresTests : IAsyncLifetime
{
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";

    private const string ThisMigration = "20260924212708_T131_ReviewAgenda";
    private const string LastMigrationBeforeThis = "20260924210830_T131_PanelBodies";

    private readonly List<string> _schemas = [];
    private string _baseConnectionString = null!;

    public Task InitializeAsync()
    {
        _baseConnectionString = ResolveBaseConnectionString();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => DropSchemasAsync();

    [Fact]
    public async Task Migration_StampsEachExistingReviewWithTheSemesterItsWindowEndsIn_AndGivesItNoAgenda()
    {
        try
        {
            var schema = await CreateSchemaAsync();
            await using (var db = NewContext(schema))
            {
                await db.GetService<IMigrator>().MigrateAsync(LastMigrationBeforeThis);
                (await db.Database.GetPendingMigrationsAsync()).First().Should().Be(ThisMigration);
            }

            int wholeYear, firstHalf;
            await using (var connection = await OpenAsync(schema))
            {
                var panelId = await InsertPanelAsync(connection, await InsertInstitutionAsync(connection, "KGK"));
                wholeYear = await InsertAsync(connection,
                    """INSERT INTO "CommitteeReviews" ("TraineeUserId", "PanelId", "ReviewPeriodFrom", "ReviewPeriodTo", "ScheduledOn", "IsFormative", "ReviewType", "State") VALUES ('trainee-1', $1, DATE '2026-01-01', DATE '2026-12-31', DATE '2026-09-24', FALSE, 1, 2) RETURNING "Id" """,
                    panelId);
                firstHalf = await InsertAsync(connection,
                    """INSERT INTO "CommitteeReviews" ("TraineeUserId", "PanelId", "ReviewPeriodFrom", "ReviewPeriodTo", "ScheduledOn", "IsFormative", "ReviewType", "State") VALUES ('trainee-1', $1, DATE '2026-01-01', DATE '2026-06-30', DATE '2026-07-02', FALSE, 1, 4) RETURNING "Id" """,
                    panelId);
            }

            await using (var db = NewContext(schema))
            {
                await db.Database.MigrateAsync();
                var stamped = await db.CommitteeReviews.AsNoTracking()
                    .ToDictionaryAsync(review => review.Id, review => (review.AcademicYear, review.Semester));
                stamped[wholeYear].Should().Be((2026, 2));
                stamped[firstHalf].Should().Be((2026, 1));
                (await db.CommitteeAgendaLines.CountAsync()).Should().Be(0, "a review started before agendas existed has none");
            }
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task OneOpenBindingReview_PerTraineePanelAndPeriod_IsHeldByTheIndex()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            await using var connection = await OpenAsync(schema);
            var panelId = await InsertPanelAsync(connection, await InsertInstitutionAsync(connection, "A"));

            await InsertReviewAsync(connection, panelId, 2026, 1, state: 2);

            var second = async () => await InsertReviewAsync(connection, panelId, 2026, 1, state: 1);
            (await second.Should().ThrowAsync<PostgresException>()).Which.ConstraintName
                .Should().Be("IX_CommitteeReviews_OneOpenBindingReviewPerPeriod");

            // A formative sitting, a ratified one, another period and another trainee are each a different review.
            await InsertReviewAsync(connection, panelId, 2026, 1, state: 1, formative: true);
            await InsertReviewAsync(connection, panelId, 2026, 1, state: 4);
            await InsertReviewAsync(connection, panelId, 2026, 2, state: 1);
            await InsertReviewAsync(connection, panelId, 2026, 1, state: 1, trainee: "trainee-2");

            var noSuchSemester = async () => await InsertReviewAsync(connection, panelId, 2026, 3, state: 1, trainee: "trainee-3");
            (await noSuchSemester.Should().ThrowAsync<PostgresException>()).Which.ConstraintName
                .Should().Be("CK_CommitteeReviews_Semester");

            // A year AcademicPeriod cannot represent with its neighbours would take the review's page down (Period throws).
            foreach (var year in new[] { 0, 1, 9999 })
            {
                var noSuchYear = async () => await InsertReviewAsync(connection, panelId, year, 1, state: 1, trainee: $"trainee-y{year}");
                (await noSuchYear.Should().ThrowAsync<PostgresException>()).Which.ConstraintName
                    .Should().Be("CK_CommitteeReviews_AcademicYear");
            }

            await InsertReviewAsync(connection, panelId, 2, 1, state: 1, trainee: "trainee-first-year");
            await InsertReviewAsync(connection, panelId, 9998, 2, state: 1, trainee: "trainee-last-year");
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task AnAgendaLine_IsDecidedExactlyWhenItNamesAStar_AndDeferredExactlyWhenItHasAReason()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            await using var connection = await OpenAsync(schema);
            var panelId = await InsertPanelAsync(connection, await InsertInstitutionAsync(connection, "A"));
            var reviewId = await InsertReviewAsync(connection, panelId, 2026, 1, state: 2);

            await InsertLineAsync(connection, reviewId, epaId: 1, windowSemester: 1, state: 1, reason: null);
            await InsertLineAsync(connection, reviewId, epaId: 2, windowSemester: null, state: 2, reason: "Rotation moved.");
            await InsertLineAsync(connection, reviewId, epaId: 3, windowSemester: 1, state: 4, reason: null);

            await RefusedAsync(() => InsertLineAsync(connection, reviewId, 10, 3, 1, null), "CK_CommitteeAgendaLines_WindowSemester");
            await RefusedAsync(() => InsertLineAsync(connection, reviewId, 11, 1, 3, null), "CK_CommitteeAgendaLines_DecidedNamesItsStar");
            await RefusedAsync(() => InsertLineAsync(connection, reviewId, 12, 1, 2, null), "CK_CommitteeAgendaLines_DeferredHasAReason");
            await RefusedAsync(() => InsertLineAsync(connection, reviewId, 13, 1, 1, "A reason on a due line."), "CK_CommitteeAgendaLines_DeferredHasAReason");

            var secondLineOnTheEpa = async () => await InsertLineAsync(connection, reviewId, 1, 1, 1, null);
            (await secondLineOnTheEpa.Should().ThrowAsync<PostgresException>()).Which.SqlState
                .Should().Be(PostgresErrorCodes.UniqueViolation);

            // A line leaves with its review.
            await ExecuteAsync(connection, """DELETE FROM "CommitteeReviews" WHERE "Id" = $1""", reviewId);
            (await ScalarAsync<long>(connection, """SELECT COUNT(*) FROM "CommitteeAgendaLines" """)).Should().Be(0);
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task AnAgendaLine_NamesAStarOnlyWhenDecided_AndTheStarItNamesCannotBeDeleted()
    {
        // The other direction of CK_CommitteeAgendaLines_DecidedNamesItsStar: a line that is not Decided names no STAR.
        try
        {
            var schema = await MigratedSchemaAsync();
            await using var connection = await OpenAsync(schema);
            var panelId = await InsertPanelAsync(connection, await InsertInstitutionAsync(connection, "A"));
            var reviewId = await InsertReviewAsync(connection, panelId, 2026, 1, state: 4);
            var starId = await InsertStarAsync(connection, reviewId);

            await RefusedAsync(() => InsertLineAsync(connection, reviewId, 20, 1, 1, null, starId), "CK_CommitteeAgendaLines_DecidedNamesItsStar");
            await RefusedAsync(() => InsertLineAsync(connection, reviewId, 21, 1, 2, "Later.", starId), "CK_CommitteeAgendaLines_DecidedNamesItsStar");
            await RefusedAsync(() => InsertLineAsync(connection, reviewId, 22, 1, 4, null, starId), "CK_CommitteeAgendaLines_DecidedNamesItsStar");
            var decided = await InsertLineAsync(connection, reviewId, 23, 1, 3, null, starId);

            // STARs are revoked, never deleted; the key restricts, so a Decided line never loses the STAR it names.
            var deleteTheStar = async () => await ExecuteAsync(connection, """DELETE FROM "EntrustmentDecisions" WHERE "Id" = $1""", starId);
            var refusal = (await deleteTheStar.Should().ThrowAsync<PostgresException>()).Which;
            refusal.SqlState.Should().Be(PostgresErrorCodes.ForeignKeyViolation);
            refusal.ConstraintName.Should().Be("FK_CommitteeAgendaLines_EntrustmentDecisions_EntrustmentDecisi~");
            (await InsertAsync(connection,
                    """SELECT COUNT(*) FROM "CommitteeAgendaLines" WHERE "Id" = $1 AND "EntrustmentDecisionId" = $2""", decided, starId))
                .Should().Be(1);
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task TwoCoordinatorsSchedulingTheSameReviewAtOnce_TheSecondIsToldWhichReview_AndNothingOfItIsWritten()
    {
        // Both requests pass the handler's check before either saves; the index refuses the second save. The page prints
        // the refusal, so it must name the review in the way, with the database's refusal kept underneath (T201).
        try
        {
            var schema = await MigratedSchemaAsync();
            int institutionId, panelId;
            await using (var connection = await OpenAsync(schema))
            {
                institutionId = await InsertInstitutionAsync(connection, "A");
                panelId = await InsertPanelAsync(connection, institutionId);
                await ExecuteAsync(connection,
                    """INSERT INTO "TraineeProfiles" ("UserId", "InstitutionId", "CurriculumId", "ProgrammeStartDate", "ExpectedCompletionDate", "IsActive") VALUES ('trainee-1', $1, $2, DATE '2025-01-15', DATE '2029-01-14', TRUE)""",
                    institutionId, await InsertCurriculumAsync(connection));
            }

            int rivalId = 0;
            await using (var db = NewContext(schema, beforeFirstSave: async () =>
                         {
                             await using var other = NewContext(schema);
                             rivalId = (await ScheduleAsync(other, panelId, Coordinator(institutionId))).Id;
                         }))
            {
                var ours = () => ScheduleAsync(db, panelId, Coordinator(institutionId));

                var refusal = await ours.Should().ThrowExactlyAsync<InvalidOperationException>();
                refusal.Which.Message.Should().StartWith($"Review #{rivalId} already puts this trainee before Annual review for 2026 S1");
                refusal.Which.InnerException.Should().BeAssignableTo<DbUpdateException>();
            }

            await using (var read = NewContext(schema))
            {
                (await read.CommitteeReviews.Select(review => review.Id).ToListAsync()).Should().Equal(rivalId);
            }
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    // ---- arrange --------------------------------------------------------------------------------------------------------

    private static Task<CommitteeReviewListItemDto> ScheduleAsync(ApplicationDbContext db, int panelId, ClaimsPrincipal principal)
        => new ScheduleCommitteeReviewCommandHandler(db).Handle(
            new ScheduleCommitteeReviewCommand(
                "trainee-1", panelId, 2026, 1, new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), new DateOnly(2026, 7, 2), principal),
            CancellationToken.None);

    private static ClaimsPrincipal Coordinator(int institutionId)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, $"coordinator-{institutionId}"),
                new Claim(ClaimTypes.Role, WombatRoles.Coordinator),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
            ],
            "test"));

    private static async Task RefusedAsync(Func<Task<int>> insert, string constraint)
    {
        var act = async () => await insert();
        (await act.Should().ThrowAsync<PostgresException>()).Which.ConstraintName.Should().Be(constraint);
    }

    private async Task<string> MigratedSchemaAsync()
    {
        var schema = await CreateSchemaAsync();
        await using var db = NewContext(schema);
        await db.Database.MigrateAsync();
        (await db.Database.GetAppliedMigrationsAsync()).Should().Contain(ThisMigration);
        return schema;
    }

    private static Task<int> InsertInstitutionAsync(NpgsqlConnection connection, string shortCode)
        => InsertAsync(connection,
            """INSERT INTO "Institutions" ("Name", "ShortCode", "IsActive", "CreatedOn") VALUES ($1, $1, TRUE, TIMESTAMPTZ '2026-06-19 00:00:00+00') RETURNING "Id" """,
            shortCode);

    private static Task<int> InsertPanelAsync(NpgsqlConnection connection, int institutionId)
        => InsertAsync(connection,
            """INSERT INTO "DecisionPanels" ("Name", "Scope", "InstitutionId", "CreatedOn") VALUES ('Annual review', 1, $1, TIMESTAMPTZ '2026-06-19 00:00:00+00') RETURNING "Id" """,
            institutionId);

    private static async Task<int> InsertSubSpecialityAsync(NpgsqlConnection connection)
    {
        var collegeId = await InsertAsync(connection,
            """INSERT INTO "Colleges" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('CPSA', 'CPSA', TIMESTAMPTZ '2026-06-19 00:00:00+00', TRUE) RETURNING "Id" """);
        var specialityId = await InsertAsync(connection,
            """INSERT INTO "Specialities" ("CollegeId", "Name", "IsActive") VALUES ($1, 'Paediatrics', TRUE) RETURNING "Id" """,
            collegeId);
        return await InsertAsync(connection,
            """INSERT INTO "SubSpecialities" ("SpecialityId", "Name", "IsActive") VALUES ($1, 'General Paediatrics', TRUE) RETURNING "Id" """,
            specialityId);
    }

    /// <summary>A STAR on a new EPA, issued by the review: a real row for a line to name.</summary>
    private static async Task<int> InsertStarAsync(NpgsqlConnection connection, int reviewId)
    {
        var subSpecialityId = await InsertSubSpecialityAsync(connection);
        var epaId = await InsertAsync(connection,
            """INSERT INTO "Epas" ("SubSpecialityId", "Code", "Title", "Category", "IsActive", "CreatedOn") VALUES ($1, 'PAED-001', 'An EPA', 1, TRUE, TIMESTAMPTZ '2026-06-19 00:00:00+00') RETURNING "Id" """,
            subSpecialityId);
        var scaleId = await InsertAsync(connection,
            """INSERT INTO "EntrustmentScales" ("Name") VALUES ('A ladder') RETURNING "Id" """);
        var levelId = await InsertAsync(connection,
            """INSERT INTO "EntrustmentLevels" ("ScaleId", "Order", "Label") VALUES ($1, 3, '3a') RETURNING "Id" """,
            scaleId);
        return await InsertAsync(connection,
            """INSERT INTO "EntrustmentDecisions" ("TraineeUserId", "EpaId", "AuthorisedLevelId", "IssuedOn", "IssuedByCommitteeReviewId", "IssuedByChairUserId", "Rationale", "Status") VALUES ('trainee-1', $1, $2, DATE '2026-07-02', $3, 'chair-1', 'Consistent.', 1) RETURNING "Id" """,
            epaId, levelId, reviewId);
    }

    private static async Task<int> InsertCurriculumAsync(NpgsqlConnection connection)
    {
        var subSpecialityId = await InsertSubSpecialityAsync(connection);
        return await InsertAsync(connection,
            """INSERT INTO "Curricula" ("SubSpecialityId", "Name", "Version", "EffectiveFrom", "IsActive") VALUES ($1, 'General Paediatrics', '11.1', DATE '2026-01-01', TRUE) RETURNING "Id" """,
            subSpecialityId);
    }

    private static Task<int> InsertReviewAsync(
        NpgsqlConnection connection, int panelId, int year, int semester, int state, bool formative = false, string trainee = "trainee-1")
        => InsertAsync(connection,
            """INSERT INTO "CommitteeReviews" ("TraineeUserId", "PanelId", "AcademicYear", "Semester", "ReviewPeriodFrom", "ReviewPeriodTo", "ScheduledOn", "IsFormative", "ReviewType", "State") VALUES ($1, $2, $3, $4, DATE '2026-01-01', DATE '2026-06-30', DATE '2026-07-02', $5, 1, $6) RETURNING "Id" """,
            trainee, panelId, year, semester, formative, state);

    private static Task<int> InsertLineAsync(
        NpgsqlConnection connection, int reviewId, int epaId, int? windowSemester, int state, string? reason, int? starId = null)
        => InsertAsync(connection,
            """INSERT INTO "CommitteeAgendaLines" ("ReviewId", "CurriculumItemId", "EpaId", "EpaCode", "EpaTitle", "Origin", "WindowYear", "WindowSemester", "IsClosing", "IsPartialPeriod", "State", "DeferralReason", "EntrustmentDecisionId") VALUES ($1, $2, $2, 'PAED-' || $2::text, 'An EPA', 1, 2026, $3, TRUE, FALSE, $4, $5, $6) RETURNING "Id" """,
            reviewId,
            epaId,
            (object?)windowSemester ?? DBNull.Value,
            state,
            (object?)reason ?? DBNull.Value,
            (object?)starId ?? DBNull.Value);

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
    /// Positional parameters ($1, $2, …), so no value is ever spliced into SQL text. A null is typed by its position in
    /// the agenda line insert: the window's semester and the STAR are integers, the reason text.
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
                    NpgsqlDbType = index == 4 ? NpgsqlTypes.NpgsqlDbType.Varchar : NpgsqlTypes.NpgsqlDbType.Integer
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
