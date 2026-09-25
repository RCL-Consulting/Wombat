using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Wombat.Application.Features.Activities.Commands.RebuildCurriculumProgress;
using Wombat.Application.Features.Activities.Queries.ListActivitiesBySubject;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.Curricula;

/// <summary>
/// T196 on a real PostgreSQL server: the migration and its check, and the time-aware credit rule as Npgsql translates it.
/// </summary>
/// <remarks>
/// <para>
/// The unit suites run on EF InMemory, which enforces no CHECK constraint, never runs a migration, and compares
/// <see cref="DateTime" /> values in .NET. Here the in-force predicate (<c>CurriculumItemsInForce.InForceAt</c>) is
/// translated to SQL and compared against a <c>timestamp with time zone</c> column, and the pause's start comes back
/// from the server at microsecond precision.
/// </para>
/// <para>
/// Each test works on a schema of its own (<c>SearchPath = it_&lt;guid&gt;</c>), registered before it is created and
/// dropped in <see cref="DisposeAsync" />, as <c>AcademicPeriodQuotaPostgresTests</c> does.
/// </para>
/// </remarks>
public sealed class EpaActivePeriodPostgresTests : IAsyncLifetime
{
    private const string T196Migration = "20260925031827_T196_EpaDeactivatedOn";
    private const string PaediatricCurriculumName = "Paediatric EPA Curriculum";
    private const string TraineeUserId = "trainee-t196";

    private static readonly DateTime DeactivatedAt = new(2026, 5, 1, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ReactivatedAt = new(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc);

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task Migration_BackfillsAnEpaAlreadyInactive_AndTheCheckKeepsTheFlagAndThePauseInStep()
    {
        // The backfill touches nothing on a fresh database, because the seeders run after migrations. It is rehearsed
        // here on a schema stopped just before T196 and filled in the old shape by raw SQL.
        var schema = await _schemas.CreateAsync();

        await using (var db = NewContext(schema))
        {
            var migrations = db.Database.GetMigrations().ToList();
            var before = migrations[migrations.IndexOf(T196Migration) - 1];
            await db.GetService<IMigrator>().MigrateAsync(before);
            (await db.Database.GetPendingMigrationsAsync()).Should().Contain(T196Migration);
        }

        int activeEpaId, inactiveEpaId;
        await using (var connection = await TestDatabase.OpenSchemaConnectionAsync(schema))
        {
            var collegeId = await InsertAsync(connection,
                """INSERT INTO "Colleges" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('College', 'COL', TIMESTAMPTZ '2026-01-01 00:00:00+00', TRUE) RETURNING "Id" """);
            var specialityId = await InsertAsync(connection,
                """INSERT INTO "Specialities" ("CollegeId", "Name", "IsActive") VALUES ($1, 'Paediatrics', TRUE) RETURNING "Id" """,
                collegeId);
            var subSpecialityId = await InsertAsync(connection,
                """INSERT INTO "SubSpecialities" ("SpecialityId", "Name", "IsActive") VALUES ($1, 'Paediatrics', TRUE) RETURNING "Id" """,
                specialityId);

            activeEpaId = await InsertAsync(connection,
                """INSERT INTO "Epas" ("SubSpecialityId", "Code", "Title", "Category", "CreatedOn", "IsActive") VALUES ($1, 'PAED-001', 'One', 0, TIMESTAMPTZ '2026-01-02 00:00:00+00', TRUE) RETURNING "Id" """,
                subSpecialityId);
            inactiveEpaId = await InsertAsync(connection,
                """INSERT INTO "Epas" ("SubSpecialityId", "Code", "Title", "Category", "CreatedOn", "IsActive") VALUES ($1, 'PAED-002', 'Two', 0, TIMESTAMPTZ '2026-01-03 00:00:00+00', FALSE) RETURNING "Id" """,
                subSpecialityId);
        }

        await using (var db = NewContext(schema))
        {
            await db.Database.MigrateAsync();
            (await db.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
            db.Database.HasPendingModelChanges().Should().BeFalse("the snapshot must match the model");

            var epas = await db.Epas.AsNoTracking()
                .Where(epa => epa.Id == activeEpaId || epa.Id == inactiveEpaId)
                .ToDictionaryAsync(epa => epa.Id);

            epas[activeEpaId].DeactivatedOn.Should().BeNull();
            epas[inactiveEpaId].DeactivatedOn.Should().Be(new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc),
                "an EPA already inactive has no recorded moment, so its pause is taken to have begun when it was created");
        }

        var flagWithoutMoment = await FluentActions.Awaiting(() => ExecuteAsync(
                schema, """UPDATE "Epas" SET "IsActive" = FALSE WHERE "Id" = $1""", activeEpaId))
            .Should().ThrowAsync<PostgresException>();
        flagWithoutMoment.Which.SqlState.Should().Be("23514");
        flagWithoutMoment.Which.ConstraintName.Should().Be("CK_Epas_DeactivatedOn");

        var momentWithoutFlag = await FluentActions.Awaiting(() => ExecuteAsync(
                schema, """UPDATE "Epas" SET "IsActive" = TRUE WHERE "Id" = $1""", inactiveEpaId))
            .Should().ThrowAsync<PostgresException>();
        momentWithoutFlag.Which.SqlState.Should().Be("23514");
        momentWithoutFlag.Which.ConstraintName.Should().Be("CK_Epas_DeactivatedOn");
    }

    [Fact]
    public async Task LiveCredit_ARebuildWhileInactive_AndAReactivation_AgreeOnPostgres()
    {
        var fixture = await ArrangeAsync();

        // In force: credited live.
        var a = await AddCompletedActivityAsync(fixture, observedOn: new DateOnly(2026, 3, 10), completedAt: new DateTime(2026, 3, 12, 9, 0, 0, DateTimeKind.Utc));
        (await CreditLiveAsync(fixture.Schema, a)).Should().Be(1);

        await using (var db = NewContext(fixture.Schema))
        {
            await new DeactivateEpaCommandHandler(db, new EpaCreditLock(db), new FixedClock(DeactivatedAt))
                .Handle(new DeactivateEpaCommand(fixture.EpaId, Administrator()), CancellationToken.None);
        }

        // Paused: the encounter was observed before the deactivation, but the completion came after it.
        var b = await AddCompletedActivityAsync(fixture, observedOn: new DateOnly(2026, 4, 20), completedAt: new DateTime(2026, 8, 5, 9, 0, 0, DateTimeKind.Utc));
        var c = await AddCompletedActivityAsync(fixture, observedOn: new DateOnly(2026, 8, 6), completedAt: new DateTime(2026, 8, 7, 9, 0, 0, DateTimeKind.Utc));
        (await CreditLiveAsync(fixture.Schema, b)).Should().Be(0);
        (await CreditLiveAsync(fixture.Schema, c)).Should().Be(0);

        // A rebuild while the EPA is inactive keeps A's credit and credits neither paused completion. Before T196 it
        // removed A's row.
        await using (var db = NewContext(fixture.Schema))
        {
            var result = await Rebuild(db);
            (result.CreditApplications, result.ProgressRowsRemoved).Should().Be((1, 0));
        }

        (await KeysAsync(fixture.Schema)).Should().BeEquivalentTo([$"{a}:complete"]);

        UpdateEpaResult reactivated;
        await using (var db = NewContext(fixture.Schema))
        {
            var epa = await db.Epas.AsNoTracking().SingleAsync(entity => entity.Id == fixture.EpaId);
            epa.DeactivatedOn.Should().Be(DeactivatedAt);

            reactivated = await new UpdateEpaCommandHandler(db, new CreditApplier(db), new EpaCreditLock(db), new TraineeCreditLock(db), new FixedClock(ReactivatedAt)).Handle(
                new UpdateEpaCommand(epa.Id, epa.SubSpecialityId, epa.Code, epa.Title, epa.Description, epa.RequiredKnowledgeSkills, epa.Category, IsActive: true, Administrator()),
                CancellationToken.None);
        }

        reactivated.CompletionsCredited.Should().Be(2);
        (await KeysAsync(fixture.Schema)).Should().BeEquivalentTo([$"{a}:complete", $"{b}:complete", $"{c}:complete"]);

        var afterReactivation = await SnapshotAsync(fixture.Schema);
        afterReactivation.Stamps.Should().OnlyContain(stamp => stamp.CreditedItemCount == 1);

        // And an Administrator rebuild after it changes nothing: the reactivation wrote what a rebuild writes, in place.
        await using (var db = NewContext(fixture.Schema))
        {
            (await Rebuild(db)).ProgressRowsRemoved.Should().Be(0);
        }

        (await SnapshotAsync(fixture.Schema)).Should().BeEquivalentTo(afterReactivation);
    }

    /// <summary>
    /// T231. The trainee's activity list marks an EPA that is not in force now, through the LEFT JOIN as Npgsql
    /// translates it, and stops marking it once it is reactivated: the flag is the EPA's standing now, not a stamp.
    /// </summary>
    [Fact]
    public async Task TheActivityList_SaysAnEpaIsNotInForce_WhileItIsDeactivated()
    {
        var fixture = await ArrangeAsync();

        var filed = await AddCompletedActivityAsync(fixture, observedOn: new DateOnly(2026, 3, 10), completedAt: new DateTime(2026, 3, 12, 9, 0, 0, DateTimeKind.Utc));
        (await ExecuteAsync(fixture.Schema, """UPDATE "Activities" SET "EpaId" = $1 WHERE "Id" = $2""", fixture.EpaId, filed)).Should().Be(1);
        // Left unstamped: an activity about no EPA.
        var aboutNoEpa = await AddCompletedActivityAsync(fixture, observedOn: new DateOnly(2026, 3, 11), completedAt: new DateTime(2026, 3, 13, 9, 0, 0, DateTimeKind.Utc));

        (await InForceByActivityAsync(fixture.Schema)).Should().BeEquivalentTo(
            new Dictionary<int, bool?> { [filed] = true, [aboutNoEpa] = null });

        await using (var db = NewContext(fixture.Schema))
        {
            await new DeactivateEpaCommandHandler(db, new EpaCreditLock(db), new FixedClock(DeactivatedAt))
                .Handle(new DeactivateEpaCommand(fixture.EpaId, Administrator()), CancellationToken.None);
        }

        (await InForceByActivityAsync(fixture.Schema)).Should().BeEquivalentTo(
            new Dictionary<int, bool?> { [filed] = false, [aboutNoEpa] = null });

        await using (var db = NewContext(fixture.Schema))
        {
            var epa = await db.Epas.AsNoTracking().SingleAsync(entity => entity.Id == fixture.EpaId);
            await new UpdateEpaCommandHandler(db, new CreditApplier(db), new EpaCreditLock(db), new TraineeCreditLock(db), new FixedClock(ReactivatedAt)).Handle(
                new UpdateEpaCommand(epa.Id, epa.SubSpecialityId, epa.Code, epa.Title, epa.Description, epa.RequiredKnowledgeSkills, epa.Category, IsActive: true, Administrator()),
                CancellationToken.None);
        }

        (await InForceByActivityAsync(fixture.Schema)).Should().BeEquivalentTo(
            new Dictionary<int, bool?> { [filed] = true, [aboutNoEpa] = null });
    }

    [Fact]
    public async Task AReactivationThatLosesARaceForAProgressRow_SaysProgressChanged_NotThatTheCodeIsTaken()
    {
        // T196 review. A reactivation opens a progress row for a paused completion's semester. If another save opens the
        // same (item, trainee, semester) row after this one read the table, the natural-key index refuses the insert.
        // Every refusal used to be reported as a duplicate EPA code.
        //
        // The other save was a second reactivation of the same EPA until T230, which holds the EPA for the whole of a
        // reactivation: a second one now waits for the first and finds the EPA active (EpaCreditRacePostgresTests). A
        // rebuild takes no hold, and here it opens the row for a completion from before the pause that nothing had
        // credited yet (a curriculum edit since, say), in the semester the paused completion falls in.
        var fixture = await ArrangeAsync();

        var a = await AddCompletedActivityAsync(fixture, observedOn: new DateOnly(2026, 3, 10), completedAt: new DateTime(2026, 3, 12, 9, 0, 0, DateTimeKind.Utc));

        await using (var db = NewContext(fixture.Schema))
        {
            await new DeactivateEpaCommandHandler(db, new EpaCreditLock(db), new FixedClock(DeactivatedAt))
                .Handle(new DeactivateEpaCommand(fixture.EpaId, Administrator()), CancellationToken.None);
        }

        // Semester 1 as well, with no row yet, so the reactivation plans to open one.
        var b = await AddCompletedActivityAsync(fixture, observedOn: new DateOnly(2026, 4, 20), completedAt: new DateTime(2026, 8, 5, 9, 0, 0, DateTimeKind.Utc));
        (await CreditLiveAsync(fixture.Schema, b)).Should().Be(0);
        (await KeysAsync(fixture.Schema)).Should().BeEmpty();

        UpdateEpaCommand reactivate;
        await using (var db = NewContext(fixture.Schema))
        {
            var epa = await db.Epas.AsNoTracking().SingleAsync(entity => entity.Id == fixture.EpaId);
            reactivate = new UpdateEpaCommand(epa.Id, epa.SubSpecialityId, epa.Code, epa.Title, epa.Description, epa.RequiredKnowledgeSkills, epa.Category, IsActive: true, Administrator());
        }

        InvalidOperationException refusal;
        await using (var loser = NewContext(fixture.Schema, beforeFirstSave: async () =>
        {
            // Reads the EPA as it was committed, inactive: A is in force at its moment, B is not.
            await using var winner = NewContext(fixture.Schema);
            (await Rebuild(winner)).CreditApplications.Should().Be(1);
        }))
        {
            var handler = new UpdateEpaCommandHandler(loser, new CreditApplier(loser), new EpaCreditLock(loser), new TraineeCreditLock(loser), new FixedClock(ReactivatedAt));
            refusal = (await FluentActions.Awaiting(() => handler.Handle(reactivate, CancellationToken.None))
                .Should().ThrowAsync<InvalidOperationException>()).Which;
        }

        refusal.Message.Should().Be("Curriculum progress changed while this EPA was being reactivated, so nothing was saved. Save again.");
        refusal.InnerException.Should().BeOfType<DbUpdateException>()
            .Which.InnerException.Should().BeOfType<PostgresException>()
            .Which.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation, "the natural-key index refused the second row");

        // The rebuild's credit stands, and the reactivation wrote nothing: the EPA is still inactive.
        (await KeysAsync(fixture.Schema)).Should().BeEquivalentTo([$"{a}:complete"]);
        await using (var db = NewContext(fixture.Schema))
        {
            (await db.Epas.AsNoTracking().SingleAsync(entity => entity.Id == fixture.EpaId)).IsActive.Should().BeFalse();
        }

        // Saving again, as the message asks, credits B into the row the rebuild opened.
        await using (var db = NewContext(fixture.Schema))
        {
            (await new UpdateEpaCommandHandler(db, new CreditApplier(db), new EpaCreditLock(db), new TraineeCreditLock(db), new FixedClock(ReactivatedAt))
                .Handle(reactivate, CancellationToken.None)).CompletionsCredited.Should().Be(1);
        }

        (await KeysAsync(fixture.Schema)).Should().BeEquivalentTo([$"{a}:complete", $"{b}:complete"]);
        (await SnapshotAsync(fixture.Schema)).Stamps.Should().OnlyContain(stamp => stamp.CreditedItemCount == 1);
    }

    [Fact]
    public async Task ACodeAnotherEpaHolds_IsStillReportedAsADuplicateCode()
    {
        var fixture = await ArrangeAsync();

        await using var db = NewContext(fixture.Schema);
        var epa = await db.Epas.AsNoTracking().SingleAsync(entity => entity.Id == fixture.EpaId);
        var taken = await db.Epas.AsNoTracking()
            .Where(other => other.Id != epa.Id && other.SubSpecialityId == epa.SubSpecialityId && other.OwningInstitutionId == null)
            .Select(other => other.Code)
            .FirstAsync();

        var handler = new UpdateEpaCommandHandler(db, new CreditApplier(db), new EpaCreditLock(db), new TraineeCreditLock(db), new FixedClock(ReactivatedAt));
        var refusal = (await FluentActions.Awaiting(() => handler.Handle(
                new UpdateEpaCommand(epa.Id, epa.SubSpecialityId, taken, epa.Title, epa.Description, epa.RequiredKnowledgeSkills, epa.Category, epa.IsActive, Administrator()),
                CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()).Which;

        refusal.Message.Should().Be("An EPA with the same code already exists for this sub-speciality.");
        refusal.InnerException.Should().BeOfType<DbUpdateException>()
            .Which.InnerException.Should().BeOfType<PostgresException>()
            .Which.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private sealed record Fixture(string Schema, int InstitutionId, int ActivityTypeId, int EpaId);

    /// <summary>A seeded schema, a PAED-001 trainee from 2026, and a type crediting the EPA its data names.</summary>
    private async Task<Fixture> ArrangeAsync()
    {
        var schema = await SeededSchemaAsync();
        await using var db = NewContext(schema);

        var institutionId = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
        var curriculumId = await db.Curricula.Where(entity => entity.Name == PaediatricCurriculumName).Select(entity => entity.Id).SingleAsync();
        var epaId = await db.CurriculumItems
            .Where(item => item.CurriculumId == curriculumId && item.OwningInstitutionId == null && item.Epa.Code == "PAED-001")
            .Select(item => item.EpaId)
            .SingleAsync();

        db.TraineeProfiles.Add(new TraineeProfile
        {
            UserId = TraineeUserId,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            ProgrammeStartDate = new DateOnly(2026, 1, 1),
            ExpectedCompletionDate = new DateOnly(2029, 12, 31),
            IsActive = true
        });

        var publishedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var activityType = new ActivityType
        {
            Key = "wba_t196_postgres",
            Name = "WBA under test (T196)",
            Scope = ActivityScope.Institution,
            ScopeId = institutionId,
            Version = 1,
            SchemaJson = SchemaJson,
            WorkflowJson = WorkflowJson,
            CreditRulesJson = CreditsTheEpa,
            DisplayFieldsJson = """["epa_id","score"]""",
            OwnerUserId = "admin-1",
            CreatedOn = publishedOn,
            IsActive = true
        };

        activityType.Versions.Add(new ActivityTypeVersion
        {
            Version = 1,
            SchemaJson = SchemaJson,
            WorkflowJson = WorkflowJson,
            CreditRulesJson = CreditsTheEpa,
            DisplayFieldsJson = activityType.DisplayFieldsJson,
            PublishedByUserId = "admin-1",
            PublishedOn = publishedOn
        });

        db.ActivityTypes.Add(activityType);
        await db.SaveChangesAsync();

        return new Fixture(schema, institutionId, activityType.Id, epaId);
    }

    private async Task<int> AddCompletedActivityAsync(Fixture fixture, DateOnly observedOn, DateTime completedAt)
    {
        await using var db = NewContext(fixture.Schema);

        var activity = new Activity
        {
            ActivityTypeId = fixture.ActivityTypeId,
            SchemaVersion = 1,
            SubjectUserId = TraineeUserId,
            CreatedByUserId = TraineeUserId,
            CurrentState = "completed",
            DataJson = $$"""{ "epa_id": {{fixture.EpaId}}, "score": 4 }""",
            CreatedOn = completedAt.AddHours(-1),
            UpdatedOn = completedAt,
            ObservedOn = observedOn,
            ObservedOnSource = ObservationDateSource.Declared,
            InstitutionId = fixture.InstitutionId
        };

        activity.Transitions.Add(new ActivityTransition
        {
            FromState = "draft",
            ToState = "submitted",
            TransitionKey = "submit",
            ActorUserId = TraineeUserId,
            OccurredOn = completedAt.AddMinutes(-5)
        });

        activity.Transitions.Add(new ActivityTransition
        {
            FromState = "submitted",
            ToState = "completed",
            TransitionKey = "complete",
            ActorUserId = TraineeUserId,
            OccurredOn = completedAt
        });

        db.Activities.Add(activity);
        await db.SaveChangesAsync();
        return activity.Id;
    }

    /// <summary>
    /// One completion's credit as the live path gives it: its own context, judged at the completion's moment, stamped,
    /// saved. Returns the stamp.
    /// </summary>
    private async Task<int> CreditLiveAsync(string schema, int activityId)
    {
        await using var db = NewContext(schema);
        var activity = await db.Activities.Include(entity => entity.Transitions).SingleAsync(entity => entity.Id == activityId);

        var credited = await new CreditApplier(db).ApplyAsync(
            activity, new ActivityType { CreditRulesJson = CreditsTheEpa, SchemaJson = SchemaJson }, CancellationToken.None);

        var completion = activity.Transitions.Single(transition => transition.TransitionKey == "complete");
        completion.CreditedItemCount = credited.UpdatedRows.Count;
        completion.CreditScaleMismatchCount = credited.ScaleMismatchCount;
        await db.SaveChangesAsync();

        return credited.UpdatedRows.Count;
    }

    /// <summary>The trainee's own list, as the trainee reads it: each row's EPA-in-force flag by activity.</summary>
    private async Task<IReadOnlyDictionary<int, bool?>> InForceByActivityAsync(string schema)
    {
        await using var db = NewContext(schema);
        var rows = await new ListActivitiesBySubjectQueryHandler(db).Handle(
            new ListActivitiesBySubjectQuery(
                TraineeUserId,
                new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, TraineeUserId)], "IntegrationTest"))),
            CancellationToken.None);

        return rows.ToDictionary(row => row.Id, row => row.EpaInForce);
    }

    private static Task<RebuildCurriculumProgressResult> Rebuild(ApplicationDbContext db)
        => new RebuildCurriculumProgressCommandHandler(db, new CreditApplier(db))
            .Handle(new RebuildCurriculumProgressCommand(Administrator()), CancellationToken.None);

    private async Task<IReadOnlyList<string>> KeysAsync(string schema)
    {
        await using var db = NewContext(schema);
        return (await db.CurriculumItemProgresses.AsNoTracking().Select(row => row.CreditedActivityKeysJson).ToListAsync())
            .SelectMany(json => JsonSerializer.Deserialize<string[]>(json)!)
            .ToList();
    }

    private sealed record Row(
        int Id, int CurriculumItemId, string TraineeUserId, int AcademicYear, int Semester, int CountsSoFar,
        int MinimumLevelReachedCount, int UnverifiedLevelCount, int? LastActivityId, DateOnly? LastObservedOn, string Keys);

    private sealed record Stamp(int ActivityId, int? CreditedItemCount, int? CreditScaleMismatchCount);

    private sealed record Snapshot(IReadOnlyList<Row> Rows, IReadOnlyList<Stamp> Stamps);

    /// <summary>
    /// Every row and every completion's stamp. The key set is parsed and re-serialised, because Postgres discards the
    /// submitted jsonb text and renders its own; the row id is kept, because a rebuild refills rows in place.
    /// </summary>
    private async Task<Snapshot> SnapshotAsync(string schema)
    {
        await using var db = NewContext(schema);

        var rows = (await db.CurriculumItemProgresses.AsNoTracking().ToListAsync())
            .Select(row => new Row(
                row.Id, row.CurriculumItemId, row.TraineeUserId, row.AcademicYear, row.Semester, row.CountsSoFar,
                row.MinimumLevelReachedCount, row.UnverifiedLevelCount, row.LastActivityId, row.LastObservedOn,
                JsonSerializer.Serialize(JsonSerializer.Deserialize<string[]>(row.CreditedActivityKeysJson)!.Order(StringComparer.Ordinal))))
            .OrderBy(row => row.Id)
            .ToList();

        var stamps = await db.ActivityTransitions.AsNoTracking()
            .Where(transition => transition.TransitionKey == "complete")
            .OrderBy(transition => transition.ActivityId)
            .Select(transition => new Stamp(transition.ActivityId, transition.CreditedItemCount, transition.CreditScaleMismatchCount))
            .ToListAsync();

        return new Snapshot(rows, stamps);
    }

    private static ClaimsPrincipal Administrator()
        => new(new ClaimsIdentity([new Claim(ClaimTypes.Role, WombatRoles.Administrator)], "IntegrationTest"));

    private sealed class FixedClock(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }

    // ─── Postgres plumbing, as in AcademicPeriodQuotaPostgresTests ────────────

    private static async Task<int> InsertAsync(NpgsqlConnection connection, string sql, params object[] values)
    {
        await using var command = Command(connection, sql, values);
        return Convert.ToInt32(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    private async Task<int> ExecuteAsync(string schema, string sql, params object[] values)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        await using var command = Command(connection, sql, values);
        return await command.ExecuteNonQueryAsync();
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

    /// <summary>A fresh schema, migrated and seeded the way startup does it: DataSeeder, then the paediatric catalogue.</summary>
    private async Task<string> SeededSchemaAsync()
    {
        var schema = await _schemas.CreateAsync();

        await using (var db = NewContext(schema))
        {
            await db.Database.MigrateAsync();
        }

        await using (var db = NewContext(schema))
        {
            await new DataSeeder(db).SeedAsync();
        }

        await using (var db = NewContext(schema))
        {
            await new PaediatricCatalogueSeeder(db).SeedAsync();
        }

        return schema;
    }

    private ApplicationDbContext NewContext(string schema, Func<Task>? beforeFirstSave = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(TestDatabase.SchemaConnectionString(schema));

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

    private const string CreditsTheEpa = """
        {
          "counts_for": [
            { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1, "minimum_level_field": "score" }
          ]
        }
        """;

    private const string SchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "details",
              "title": "Details",
              "fields": [
                { "key": "epa_id", "type": "epa", "label": "EPA", "required": true },
                { "key": "score", "type": "number", "label": "Score", "required": true }
              ]
            }
          ]
        }
        """;

    private const string WorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "submitted", "label": "Submitted" },
            { "key": "completed", "label": "Completed", "terminal": true }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject" },
            { "key": "complete", "from": "submitted", "to": "completed", "actor": "subject" }
          ]
        }
        """;
}
