using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Wombat.Application.Features.Activities.Commands.RebuildCurriculumProgress;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Dashboards.Trainee;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.Curricula;

/// <summary>
/// T130 on a real PostgreSQL server: what the unit suites cannot see.
/// </summary>
/// <remarks>
/// <para>
/// Every progress test in Application and Infrastructure runs on EF InMemory, which enforces neither the new
/// four-column unique index nor the CHECK constraints, has no xmin concurrency token, and evaluates any .NET
/// expression in a predicate that Npgsql would refuse to translate. It also never runs a migration. So the
/// migration, the constraints, the concurrency guards and the readers' SQL are only ever exercised here.
/// </para>
/// <para>
/// Each test builds <see cref="ApplicationDbContext" /> directly on a schema of its own (<c>SearchPath = it_&lt;guid&gt;</c>,
/// nothing else on the path), so no statement can reach <c>public</c>. Every schema a test creates is registered
/// BEFORE it is created and dropped in <see cref="DisposeAsync" />. Nothing that can fail runs in
/// <see cref="InitializeAsync" />, because xUnit 2 does not call DisposeAsync after a failed InitializeAsync, and
/// the schema would leak.
/// </para>
/// <para>
/// Every date is fixed, and <see cref="AsOf" /> pins "today" to 2026-09-23, which is semester 2 of 2026.
/// </para>
/// </remarks>
public sealed class AcademicPeriodQuotaPostgresTests : IAsyncLifetime
{
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";

    private const string T130Migration = "20260923082939_T130_AcademicPeriodQuota";
    private const string LastMigrationBeforeT130 = "20260921071752_T121_MsfEvidenceRecordedPerEpa";
    private const string NaturalKeyIndex = "UX_CurriculumItemProgresses_Item_Trainee_Period";
    private const string PaediatricCurriculumName = "Paediatric EPA Curriculum";

    private const string TraineeUserId = "trainee-pg";
    private const string LateStarterUserId = "trainee-late";

    private static readonly DateOnly AsOf = new(2026, 9, 23);

    /// <summary>
    /// Annexure B of CPSA EPA v11.1, typed out by hand: a per-semester target for the ten EPAs that publish a
    /// per-semester figure, one per academic year for the five that do not. Deliberately not read from the seed
    /// JSON or from the migration's own table, because an oracle read from the thing under test agrees with it
    /// by construction.
    /// </summary>
    private static readonly (string Code, QuotaPeriod Period, int Target)[] AnnexureB =
    [
        ("PAED-001", QuotaPeriod.Semester, 3),
        ("PAED-002", QuotaPeriod.Semester, 3),
        ("PAED-003", QuotaPeriod.Semester, 3),
        ("PAED-004", QuotaPeriod.Semester, 3),
        ("PAED-005", QuotaPeriod.Semester, 3),
        ("PAED-006", QuotaPeriod.Semester, 2),
        ("PAED-007", QuotaPeriod.Semester, 1),
        ("PAED-008", QuotaPeriod.AcademicYear, 1),
        ("PAED-009", QuotaPeriod.AcademicYear, 1),
        ("PAED-010", QuotaPeriod.Semester, 3),
        ("PAED-011", QuotaPeriod.AcademicYear, 1),
        ("PAED-012", QuotaPeriod.Semester, 3),
        ("PAED-013", QuotaPeriod.AcademicYear, 1),
        ("PAED-014", QuotaPeriod.AcademicYear, 1),
        ("PAED-015", QuotaPeriod.Semester, 1),
    ];

    private readonly List<string> _schemas = [];
    private string _baseConnectionString = null!;

    public Task InitializeAsync()
    {
        _baseConnectionString = ResolveBaseConnectionString();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_schemas.Count == 0)
        {
            return;
        }

        await using var connection = new NpgsqlConnection(_baseConnectionString);
        await connection.OpenAsync();

        foreach (var schema in _schemas)
        {
            // Belt and braces: this class only ever drops a schema it named itself.
            if (!schema.StartsWith("it_", StringComparison.Ordinal))
            {
                continue;
            }

            await using var drop = connection.CreateCommand();
            drop.CommandText = $"DROP SCHEMA IF EXISTS \"{schema}\" CASCADE";
            await drop.ExecuteNonQueryAsync();
        }
    }

    [Fact]
    public async Task MigrateAsync_OnAFreshDatabase_AppliesT130WithItsColumnsIndexAndChecks()
    {
        // T130's Verification item "MigrateAsync applies it on a fresh database". A hand-edited migration
        // without its Designer file is skipped by MigrateAsync with no error, and a snapshot that drifted from
        // the model makes the next `dotnet ef migrations add` write the difference into somebody else's
        // migration. Both are caught here and nowhere else.
        var schema = await CreateSchemaAsync();

        await using (var db = NewContext(schema))
        {
            await db.Database.MigrateAsync();

            var applied = (await db.Database.GetAppliedMigrationsAsync()).ToList();
            applied.Should().Contain(T130Migration);
            applied.Should().Equal(db.Database.GetMigrations(), "every migration in the assembly is applied, in order");
            (await db.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
            db.Database.HasPendingModelChanges().Should().BeFalse(
                "the model snapshot must match the model, or the next generated migration inherits T130's leftovers");
        }

        var columns = await QueryAsync(
            schema,
            """
            SELECT table_name, column_name, data_type, is_nullable, column_default
            FROM information_schema.columns
            WHERE table_schema = $1
              AND ((table_name = 'CurriculumItemProgresses' AND column_name IN ('AcademicYear', 'Semester', 'LastObservedOn'))
                OR (table_name = 'CurriculumItems' AND column_name = 'QuotaPeriod'))
            """,
            reader => (
                Table: reader.GetString(0),
                Column: reader.GetString(1),
                Type: reader.GetString(2),
                Nullable: reader.GetString(3),
                Default: reader.IsDBNull(4) ? null : reader.GetString(4)),
            schema);

        // AcademicYear and Semester have NO default (HAND-EDIT 3): a writer that forgot its semester must fail,
        // not quietly become (0, 0). QuotaPeriod keeps EF's backfill default of 0, which is AcademicYear.
        columns.OrderBy(column => column.Table, StringComparer.Ordinal).ThenBy(column => column.Column, StringComparer.Ordinal)
            .Should().Equal(
                ("CurriculumItemProgresses", "AcademicYear", "integer", "NO", (string?)null),
                ("CurriculumItemProgresses", "LastObservedOn", "date", "YES", (string?)null),
                ("CurriculumItemProgresses", "Semester", "integer", "NO", (string?)null),
                ("CurriculumItems", "QuotaPeriod", "integer", "NO", "0"));

        var uniqueIndexes = await QueryAsync(
            schema,
            """
            SELECT indexname, indexdef
            FROM pg_indexes
            WHERE schemaname = $1 AND tablename = 'CurriculumItemProgresses' AND indexdef LIKE 'CREATE UNIQUE INDEX%'
            ORDER BY indexname
            """,
            reader => (Name: reader.GetString(0), Definition: reader.GetString(1)),
            schema);

        // Only the primary key and the period key. The old unique index on (item, trainee) would refuse the
        // second semester's row for the same item, so its absence matters as much as the new one's presence.
        uniqueIndexes.Select(index => index.Name).Should().BeEquivalentTo("PK_CurriculumItemProgresses", NaturalKeyIndex);
        uniqueIndexes.Single(index => index.Name == NaturalKeyIndex).Definition.Should().EndWith(
            "USING btree (\"CurriculumItemId\", \"TraineeUserId\", \"AcademicYear\", \"Semester\")");

        var checks = await QueryAsync(
            schema,
            """
            SELECT con.conname
            FROM pg_constraint con
            JOIN pg_class rel ON rel.oid = con.conrelid
            JOIN pg_namespace ns ON ns.oid = rel.relnamespace
            WHERE ns.nspname = $1 AND con.contype = 'c'
              AND rel.relname IN ('CurriculumItemProgresses', 'CurriculumItems')
            """,
            reader => reader.GetString(0),
            schema);

        checks.Should().BeEquivalentTo(
            "CK_CurriculumItemProgresses_AcademicYear",
            "CK_CurriculumItemProgresses_Semester",
            "CK_CurriculumItems_QuotaPeriod",
            // Not T130's: T131 slice 2 checks the decision cadence the same way (DecisionCadenceMigrationPostgresTests).
            "CK_CurriculumItems_DecisionCadence");
    }

    [Fact]
    public async Task Seeders_OnPostgres_GiveEachPaediatricEpaItsAnnexureBWindowAndTarget()
    {
        // The seeder path on the real provider: migrate a fresh schema, run both seeders the way startup does,
        // and read the fifteen items back. Before T130 these were the annual figure times four ("1 / 24").
        var schema = await SeededSchemaAsync();

        await using var db = NewContext(schema);
        var items = await db.CurriculumItems
            .AsNoTracking()
            .Where(item => item.Curriculum.Name == PaediatricCurriculumName && item.OwningInstitutionId == null)
            .Select(item => new { item.Epa.Code, item.QuotaPeriod, item.RequiredCount })
            .ToListAsync();

        items.OrderBy(item => item.Code, StringComparer.Ordinal)
            .Select(item => (item.Code, item.QuotaPeriod, item.RequiredCount))
            .Should().Equal(AnnexureB);

        // Cross-checks on the oracle itself, from Annexure B's own prose: 25 of the 55 encounters fall in each
        // semester, and the five one-per-annum EPAs make up the rest.
        AnnexureB.Where(row => row.Period == QuotaPeriod.Semester).Sum(row => row.Target).Should().Be(25);
        AnnexureB.Sum(row => row.Period == QuotaPeriod.Semester ? 2 * row.Target : row.Target).Should().Be(55);

        // The migration's hand-written UPDATE writes QuotaPeriod as a bare integer (1 = Semester). The seeder
        // writes it through EF. The two only agree while the enum is stored as its integer value.
        var stored = await ScalarAsync<int>(
            schema,
            """
            SELECT ci."QuotaPeriod"
            FROM "CurriculumItems" ci
            JOIN "Epas" e ON e."Id" = ci."EpaId"
            JOIN "Curricula" c ON c."Id" = ci."CurriculumId"
            WHERE e."Code" = 'PAED-001' AND e."OwningInstitutionId" IS NULL AND c."Name" = $1
            """,
            PaediatricCurriculumName);
        stored.Should().Be(1);
    }

    [Fact]
    public async Task Postgres_RefusesAProgressRowOutsideTheTwoSemesters_AndASecondRowForTheSameBucket()
    {
        // The constraints InMemory ignores. Each refusal is asserted by SQLSTATE and constraint name, so a
        // row refused for some other reason (a foreign key, say) cannot pass for the one under test.
        var schema = await SeededSchemaAsync();
        var itemId = await ScalarAsync<int>(schema, """SELECT MIN("Id") FROM "CurriculumItems" """);

        var semesterThree = await FluentActions.Awaiting(() => InsertProgressRowAsync(schema, itemId, "trainee-a", 2026, semester: 3))
            .Should().ThrowAsync<PostgresException>();
        semesterThree.Which.SqlState.Should().Be("23514");
        semesterThree.Which.ConstraintName.Should().Be("CK_CurriculumItemProgresses_Semester");

        var yearZero = await FluentActions.Awaiting(() => InsertProgressRowAsync(schema, itemId, "trainee-a", 0, semester: 1))
            .Should().ThrowAsync<PostgresException>();
        yearZero.Which.SqlState.Should().Be("23514");
        yearZero.Which.ConstraintName.Should().Be("CK_CurriculumItemProgresses_AcademicYear");

        // No column default: a writer that leaves the semester out is refused, not filed under semester 0.
        var noSemester = await FluentActions.Awaiting(() => ExecuteAsync(
                schema,
                """
                INSERT INTO "CurriculumItemProgresses"
                    ("CurriculumItemId", "TraineeUserId", "AcademicYear", "CountsSoFar", "MinimumLevelReachedCount",
                     "ScaleMismatchCount", "UnverifiedLevelCount", "LastUpdated", "CreditedActivityKeysJson")
                VALUES ($1, 'trainee-a', 2026, 1, 0, 0, 0, TIMESTAMPTZ '2026-09-23 08:00:00+00', '[]')
                """,
                itemId))
            .Should().ThrowAsync<PostgresException>();
        noSemester.Which.SqlState.Should().Be("23502");
        noSemester.Which.ColumnName.Should().Be("Semester");

        await InsertProgressRowAsync(schema, itemId, "trainee-a", 2026, semester: 1);

        var duplicate = await FluentActions.Awaiting(() => InsertProgressRowAsync(schema, itemId, "trainee-a", 2026, semester: 1))
            .Should().ThrowAsync<PostgresException>();
        duplicate.Which.SqlState.Should().Be("23505");
        duplicate.Which.ConstraintName.Should().Be(NaturalKeyIndex);

        // The same item and trainee in the other semester, and in the same semester of another year, are
        // different buckets. Both would have been refused by the pre-T130 index on (item, trainee).
        await InsertProgressRowAsync(schema, itemId, "trainee-a", 2026, semester: 2);
        await InsertProgressRowAsync(schema, itemId, "trainee-a", 2027, semester: 1);

        (await ScalarAsync<long>(
                schema,
                """SELECT COUNT(*) FROM "CurriculumItemProgresses" WHERE "CurriculumItemId" = $1 AND "TraineeUserId" = 'trainee-a'""",
                itemId))
            .Should().Be(3);
    }

    [Fact]
    public async Task Postgres_RefusesAQuotaPeriodThatIsNeitherAcademicYearNorSemester()
    {
        // QuotaWindow reads any value other than Semester as an academic year, so a stray integer would not
        // crash a reader. It would silently mis-score the item, which is why the write is refused at the source.
        var schema = await SeededSchemaAsync();
        var itemId = await ScalarAsync<int>(schema, """SELECT MIN("Id") FROM "CurriculumItems" """);

        var refused = await FluentActions.Awaiting(() => ExecuteAsync(
                schema, """UPDATE "CurriculumItems" SET "QuotaPeriod" = 5 WHERE "Id" = $1""", itemId))
            .Should().ThrowAsync<PostgresException>();
        refused.Which.SqlState.Should().Be("23514");
        refused.Which.ConstraintName.Should().Be("CK_CurriculumItems_QuotaPeriod");

        (await ExecuteAsync(schema, """UPDATE "CurriculumItems" SET "QuotaPeriod" = 1 WHERE "Id" = $1""", itemId))
            .Should().Be(1);
    }

    [Fact]
    public async Task Migration_OnAPopulatedPreT130Database_EmptiesProgressAndCorrectsOnlyTheUneditedSeededTargets()
    {
        // The migration's two data edits (the DELETE and the one-off Annexure B UPDATE) touch nothing on a fresh
        // database, because the seeders run after migrations. The only databases they ever act on are existing
        // ones, dev and production, at startup, where a failure stops the app. So they are rehearsed here on a
        // schema stopped at the last pre-T130 migration and filled in the old shape by raw SQL.
        var schema = await CreateSchemaAsync();

        await using (var db = NewContext(schema))
        {
            await db.GetService<IMigrator>().MigrateAsync(LastMigrationBeforeT130);

            (await db.Database.GetAppliedMigrationsAsync()).Last().Should().Be(LastMigrationBeforeT130);
            (await db.Database.GetPendingMigrationsAsync()).First().Should().Be(T130Migration);
        }

        (await ScalarAsync<long>(
                schema,
                """
                SELECT COUNT(*) FROM information_schema.columns
                WHERE table_schema = $1 AND table_name = 'CurriculumItemProgresses' AND column_name = 'Semester'
                """,
                schema))
            .Should().Be(0, "guard: the schema must really be in the pre-T130 shape");

        int paed001, paed008, paed002Edited, localItem, otherCurriculumItem;
        await using (var connection = await OpenAsync(schema))
        {
            var hospitalId = await InsertAsync(connection,
                """INSERT INTO "Institutions" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('Legacy Hospital', 'LEGACY', TIMESTAMPTZ '2026-01-01 00:00:00+00', TRUE) RETURNING "Id" """);
            var collegeId = await InsertAsync(connection,
                """INSERT INTO "Colleges" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('College of Paediatricians of South Africa', 'CPSA', TIMESTAMPTZ '2026-01-01 00:00:00+00', TRUE) RETURNING "Id" """);
            var specialityId = await InsertAsync(connection,
                """INSERT INTO "Specialities" ("CollegeId", "Name", "IsActive") VALUES ($1, 'Paediatrics', TRUE) RETURNING "Id" """,
                collegeId);
            var subSpecialityId = await InsertAsync(connection,
                """INSERT INTO "SubSpecialities" ("SpecialityId", "Name", "IsActive") VALUES ($1, 'Paediatrics', TRUE) RETURNING "Id" """,
                specialityId);

            var epa001 = await InsertEpaAsync(connection, subSpecialityId, "PAED-001", owningInstitutionId: null);
            var epa002 = await InsertEpaAsync(connection, subSpecialityId, "PAED-002", owningInstitutionId: null);
            var epa008 = await InsertEpaAsync(connection, subSpecialityId, "PAED-008", owningInstitutionId: null);
            // An institution may reuse a national code for a local EPA: national codes are unique only among
            // national EPAs. The correction must not follow the code onto it.
            var localEpa001 = await InsertEpaAsync(connection, subSpecialityId, "PAED-001", owningInstitutionId: hospitalId);

            var curriculumId = await InsertCurriculumAsync(connection, subSpecialityId, PaediatricCurriculumName, "11.1");
            var otherCurriculumId = await InsertCurriculumAsync(connection, subSpecialityId, "Paediatric Research Track", "1");

            // The old seeder's lifetime totals: per annum x 4 programme years.
            paed001 = await InsertCurriculumItemAsync(connection, curriculumId, epa001, requiredCount: 24, owningInstitutionId: null);
            paed008 = await InsertCurriculumItemAsync(connection, curriculumId, epa008, requiredCount: 4, owningInstitutionId: null);
            // An administrator changed this one (PAED-002 was seeded at 24). An edit is never overwritten.
            paed002Edited = await InsertCurriculumItemAsync(connection, curriculumId, epa002, requiredCount: 7, owningInstitutionId: null);
            // Decoys at exactly the old seeded value: a local item on a local EPA that shares the code, and a
            // national item in another curriculum of the same discipline.
            localItem = await InsertCurriculumItemAsync(connection, curriculumId, localEpa001, requiredCount: 24, owningInstitutionId: hospitalId);
            otherCurriculumItem = await InsertCurriculumItemAsync(connection, otherCurriculumId, epa001, requiredCount: 24, owningInstitutionId: null);

            // Lifetime tallies in the pre-T130 shape: no AcademicYear, no Semester.
            foreach (var trainee in new[] { "trainee-legacy-1", "trainee-legacy-2" })
            {
                await ExecuteAsync(connection,
                    """
                    INSERT INTO "CurriculumItemProgresses"
                        ("CurriculumItemId", "TraineeUserId", "CountsSoFar", "MinimumLevelReachedCount", "ScaleMismatchCount",
                         "UnverifiedLevelCount", "LastUpdated", "CreditedActivityKeysJson")
                    VALUES ($1, $2, 5, 3, 0, 5, TIMESTAMPTZ '2026-09-01 08:00:00+00', '["11:complete", "12:complete"]')
                    """,
                    paed001, trainee);
            }
        }

        (await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "CurriculumItemProgresses" """))
            .Should().Be(2, "guard: the legacy tallies must be there for the migration to delete");

        await using (var db = NewContext(schema))
        {
            await db.Database.MigrateAsync();
            (await db.Database.GetAppliedMigrationsAsync()).Should().Contain(T130Migration);
            (await db.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
        }

        // A lifetime tally cannot be split into semesters, so the rows go and a rebuild regenerates them.
        (await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "CurriculumItemProgresses" """)).Should().Be(0);

        await using (var db = NewContext(schema))
        {
            var items = await db.CurriculumItems
                .AsNoTracking()
                .ToDictionaryAsync(item => item.Id, item => (item.QuotaPeriod, item.RequiredCount));

            items[paed001].Should().Be((QuotaPeriod.Semester, 3), "PAED-001 is three per semester in Annexure B");
            items[paed008].Should().Be((QuotaPeriod.AcademicYear, 1), "PAED-008 is one per annum in Annexure B");
            items[paed002Edited].Should().Be((QuotaPeriod.AcademicYear, 7), "an administrator's edit is kept");
            items[localItem].Should().Be((QuotaPeriod.AcademicYear, 24), "an institution-local item is not the College's to correct");
            items[otherCurriculumItem].Should().Be((QuotaPeriod.AcademicYear, 24), "only the Paediatric EPA Curriculum is corrected");
        }
    }

    [Fact]
    public async Task CreditApplier_OnPostgres_WritesOneRowPerSemester_AndARebuildReproducesThemInPlace()
    {
        // The production shape: every completion arrives in its own DbContext and saves, so the applier finds
        // the previous completion's row through its DATABASE half, not through Local, and every insert meets
        // the real unique index. Three completions on PAED-001: two in semester 1, filed in the opposite order
        // to their encounters, and one in semester 2.
        var fixture = await ArrangePaediatricTraineeAsync();

        int a, c, b;
        await using (var db = NewContext(fixture.Schema))
        {
            a = await AddCompletedActivityAsync(db, fixture, TraineeUserId, fixture.Paed001.EpaId, score: 4,
                observedOn: new DateOnly(2026, 3, 10), filedOn: new DateTime(2026, 3, 12, 9, 0, 0, DateTimeKind.Utc));
            c = await AddCompletedActivityAsync(db, fixture, TraineeUserId, fixture.Paed001.EpaId, score: 3,
                observedOn: new DateOnly(2026, 2, 2), filedOn: new DateTime(2026, 4, 1, 9, 0, 0, DateTimeKind.Utc));
            b = await AddCompletedActivityAsync(db, fixture, TraineeUserId, fixture.Paed001.EpaId, score: 2,
                observedOn: new DateOnly(2026, 8, 4), filedOn: new DateTime(2026, 8, 5, 9, 0, 0, DateTimeKind.Utc));
        }

        foreach (var activityId in new[] { a, c, b })
        {
            (await CreditInFreshContextAsync(fixture.Schema, activityId)).UpdatedRows.Should().ContainSingle();
        }

        // Re-applying a credited activity is a no-op. On Postgres the stored key set comes back as Postgres
        // renders jsonb, not as the applier wrote it, and the dedupe still has to recognise it.
        (await CreditInFreshContextAsync(fixture.Schema, a)).UpdatedRows.Should().BeEmpty();

        IReadOnlyList<ProgressSnapshot> incremental;
        IReadOnlyList<int> incrementalIds;
        await using (var db = NewContext(fixture.Schema))
        {
            incremental = await SnapshotsAsync(db);
            incrementalIds = await RowIdsAsync(db);
        }

        // The absolute oracle. Semester 1 holds A and C: both at or above the year-1 minimum (order 3), both
        // compared without scale proof because the test schema's "score" field names no scale_key.
        // LastActivityId is C (filed last), LastObservedOn is A's date (the latest encounter), and the two differ
        // on purpose.
        incremental.Should().Equal(
            new ProgressSnapshot(fixture.Paed001.ItemId, TraineeUserId, 2026, 1, CountsSoFar: 2, MinimumLevelReachedCount: 2,
                ScaleMismatchCount: 0, UnverifiedLevelCount: 2, LastActivityId: c,
                LastObservedOn: new DateOnly(2026, 3, 10), CreditedActivityKeys: Keys(a, c)),
            new ProgressSnapshot(fixture.Paed001.ItemId, TraineeUserId, 2026, 2, CountsSoFar: 1, MinimumLevelReachedCount: 0,
                ScaleMismatchCount: 0, UnverifiedLevelCount: 1, LastActivityId: b,
                LastObservedOn: new DateOnly(2026, 8, 4), CreditedActivityKeys: Keys(b)));

        await using (var db = NewContext(fixture.Schema))
        {
            var result = await new RebuildCurriculumProgressCommandHandler(db, new CreditApplier(db))
                .Handle(new RebuildCurriculumProgressCommand(Administrator()), CancellationToken.None);

            result.Should().Be(new RebuildCurriculumProgressResult(
                ActivitiesReplayed: 3, CreditApplications: 3, ProgressRowsWritten: 2, ProgressRowsRemoved: 0, TransitionsStamped: 3));
        }

        await using (var db = NewContext(fixture.Schema))
        {
            // Compared canonical-to-canonical: SnapshotsAsync parses CreditedActivityKeysJson and re-serialises
            // it, because Postgres discards the submitted jsonb text and renders its own.
            (await SnapshotsAsync(db)).Should().Equal(incremental);

            // Zeroed and refilled in place, never deleted and re-inserted, so the rows keep their identity.
            (await RowIdsAsync(db)).Should().Equal(incrementalIds);

            var completions = await db.ActivityTransitions
                .AsNoTracking()
                .Where(transition => transition.TransitionKey == "complete")
                .Select(transition => new { transition.CreditedItemCount, transition.CreditScaleMismatchCount })
                .ToListAsync();

            completions.Should().HaveCount(3).And.OnlyContain(stamp => stamp.CreditedItemCount == 1 && stamp.CreditScaleMismatchCount == 0);
        }
    }

    [Fact]
    public async Task ProgressReaders_TranslateOnPostgres_AndReadTheCurrentWindow()
    {
        // A translation smoke test with literal expectations. InMemory evaluates any .NET call in a predicate;
        // Npgsql refuses what it cannot translate. So each reader runs once against real SQL, and its figures
        // are checked, not only its lack of an exception.
        //
        // trainee-pg started on 1 January 2026, so every target applies on 2026-09-23. PAED-001 (three per
        // semester) has one encounter in each semester; PAED-008 (one per year) has one in semester 1, which
        // counts toward the 2026 academic year. trainee-late started on 15 August 2026, part-way through
        // semester 2, so D14 exempts every target for the rest of 2026 (D42: later than the semester's first month,
        // and on or after 1 July for a yearly target).
        var fixture = await ArrangePaediatricTraineeAsync();

        await using (var db = NewContext(fixture.Schema))
        {
            db.TraineeProfiles.Add(new TraineeProfile
            {
                UserId = LateStarterUserId,
                InstitutionId = fixture.InstitutionId,
                CurriculumId = fixture.CurriculumId,
                ProgrammeStartDate = new DateOnly(2026, 8, 15),
                ExpectedCompletionDate = new DateOnly(2030, 8, 14),
                IsActive = true
            });
            await db.SaveChangesAsync();

            var ids = new[]
            {
                await AddCompletedActivityAsync(db, fixture, TraineeUserId, fixture.Paed001.EpaId, score: 4,
                    observedOn: new DateOnly(2026, 3, 10), filedOn: new DateTime(2026, 3, 12, 9, 0, 0, DateTimeKind.Utc)),
                await AddCompletedActivityAsync(db, fixture, TraineeUserId, fixture.Paed008.EpaId, score: 4,
                    observedOn: new DateOnly(2026, 3, 10), filedOn: new DateTime(2026, 3, 13, 9, 0, 0, DateTimeKind.Utc)),
                await AddCompletedActivityAsync(db, fixture, TraineeUserId, fixture.Paed001.EpaId, score: 4,
                    observedOn: new DateOnly(2026, 8, 4), filedOn: new DateTime(2026, 8, 5, 9, 0, 0, DateTimeKind.Utc)),
            };

            foreach (var id in ids)
            {
                (await CreditInFreshContextAsync(fixture.Schema, id)).UpdatedRows.Should().ContainSingle();
            }
        }

        await using (var db = NewContext(fixture.Schema))
        {
            (await db.CurriculumItemProgresses.CountAsync()).Should().Be(3, "guard: PAED-001 in two semesters, PAED-008 in one");

            var summary = await new GetCurriculumProgressForTraineeQueryHandler(db)
                .Handle(new GetCurriculumProgressForTraineeQuery(TraineeUserId, TraineePrincipal(TraineeUserId), AsOf), CancellationToken.None);

            summary.Should().NotBeNull();
            summary!.CurrentSemesterName.Should().Be("Semester 2, 2026");
            summary.CurrentSemesterMonths.Should().Be("July to November");
            summary.CurrentSemesterNominalEnd.Should().Be(new DateOnly(2026, 11, 30));
            summary.IsAfterTeachingYear.Should().BeFalse();
            summary.TraineeStage.Should().Be(1);
            summary.ProgrammeNotStarted.Should().BeFalse();
            summary.SemesterTargetsStart.Should().BeNull();
            summary.YearTargetsStart.Should().BeNull();
            (summary.SemesterTargetsMet, summary.SemesterTargetsApplying, summary.YearTargetsMet, summary.YearTargetsApplying)
                .Should().Be((0, 10, 1, 5));

            summary.Items.Select(item => (item.EpaCode, item.QuotaPeriod, item.Target)).Should().Equal(AnnexureB);
            summary.Items.Where(item => item.EpaCode is not ("PAED-001" or "PAED-008"))
                .Should().OnlyContain(item => item.Current.Count == 0 && item.Current.Applies);

            var paed001 = summary.Items.Single(item => item.EpaCode == "PAED-001");
            paed001.Current.Name.Should().Be("Semester 2, 2026");
            paed001.Current.Start.Should().Be(new DateOnly(2026, 7, 1));
            paed001.Current.NominalEnd.Should().Be(new DateOnly(2026, 11, 30));
            paed001.Current.Status.Should().Be(QuotaWindowStatus.Counting);
            (paed001.Current.Count, paed001.Current.IsMet, paed001.Current.Shortfall, paed001.Current.PercentOfTarget)
                .Should().Be((1, false, 2, 33));
            paed001.Current.LastObservedOn.Should().Be(new DateOnly(2026, 8, 4));
            paed001.Previous.Should().NotBeNull();
            paed001.Previous!.Name.Should().Be("Semester 1, 2026");
            (paed001.Previous.Count, paed001.Previous.IsMet).Should().Be((1, false));
            paed001.EffectiveMinimumLevelLabel.Should().Be("3a", "the year-1 minimum is order 3, which the CPSA ladder prints as 3a");

            var paed008 = summary.Items.Single(item => item.EpaCode == "PAED-008");
            paed008.Current.Name.Should().Be("2026 academic year");
            paed008.Current.Months.Should().Be("January to November");
            (paed008.Current.Count, paed008.Current.IsMet, paed008.Current.Shortfall, paed008.Current.PercentOfTarget)
                .Should().Be((1, true, 0, 100));
            paed008.Current.MinimumLevelReachedCount.Should().Be(1);
            paed008.Current.LastObservedOn.Should().Be(new DateOnly(2026, 3, 10));
            paed008.Previous.Should().BeNull("the 2025 academic year is before the programme started");
            paed008.EffectiveMinimumLevelLabel.Should().Be("2");

            var late = await new GetCurriculumProgressForTraineeQueryHandler(db)
                .Handle(new GetCurriculumProgressForTraineeQuery(LateStarterUserId, TraineePrincipal(LateStarterUserId), AsOf), CancellationToken.None);

            late.Should().NotBeNull();
            (late!.SemesterTargetsApplying, late.YearTargetsApplying).Should().Be((0, 0));
            late.SemesterTargetsStart.Should().Be(new QuotaStartDto("semester 1, 2027", new DateOnly(2027, 1, 1)));
            late.YearTargetsStart.Should().Be(new QuotaStartDto("the 2027 academic year", new DateOnly(2027, 1, 1)));
            late.Items.Should().HaveCount(15).And.OnlyContain(item => item.Current.IsExempt && item.Current.Count == 0);

            // The dashboard card reads the same model, plus the activity lists, which are SQL of their own.
            var dashboard = await new GetTraineeDashboardSummaryQueryHandler(db)
                .Handle(new GetTraineeDashboardSummaryQuery(TraineePrincipal(TraineeUserId), AsOf), CancellationToken.None);

            dashboard.CurriculumTargets.Should().NotBeNull();
            (dashboard.CurriculumTargets!.SemesterTargetsMet, dashboard.CurriculumTargets.SemesterTargetsApplying,
                    dashboard.CurriculumTargets.YearTargetsMet, dashboard.CurriculumTargets.YearTargetsApplying)
                .Should().Be((0, 10, 1, 5));
            dashboard.RecentActivities.Should().HaveCount(3);
            dashboard.Inbox.Should().BeEmpty();

            var profiles = await db.TraineeProfiles.AsNoTracking().ToListAsync();
            profiles.Select(profile => profile.UserId).Should().BeEquivalentTo(TraineeUserId, LateStarterUserId);

            var coverage = await CurriculumCoverageReader.ReadAsync(db, profiles, AsOf, CancellationToken.None);

            coverage.CurrentSemesterName.Should().Be("Semester 2, 2026");
            coverage.ExemptTraineeCount.Should().Be(1);
            coverage.Trainees.Should().Equal(new TraineeTargetCoverage(TraineeUserId, 0, 10, 1, 5));
            coverage.Epas.Select(epa => (epa.EpaCode, epa.QuotaPeriod, epa.Target)).Should().Equal(AnnexureB);
            coverage.Epas.Should().OnlyContain(epa => epa.TraineesApplying == 1 && epa.TraineesExempt == 1);
            coverage.Epas.Where(epa => epa.TraineesMet == 1).Select(epa => epa.EpaCode).Should().Equal("PAED-008");
        }
    }

    [Fact]
    public async Task TwoCompletionsCreditingOneStoredRowAtOnce_OnPostgres_RefuseTheSecondSaveRatherThanLoseACredit()
    {
        // A progress row is a running total. Two live completions that both read it and both add one would
        // otherwise save the same number, and one credit would vanish while its transition still claimed it. T130
        // maps Postgres's xmin as a concurrency token (and removed the AddColumn EF generated for it), so the
        // second save must fail instead. Only Postgres has an xmin, so only this suite can see it work.
        var fixture = await ArrangePaediatricTraineeAsync();

        int a, b, c;
        await using (var db = NewContext(fixture.Schema))
        {
            a = await AddCompletedActivityAsync(db, fixture, TraineeUserId, fixture.Paed001.EpaId, score: 4,
                observedOn: new DateOnly(2026, 8, 3), filedOn: new DateTime(2026, 8, 3, 9, 0, 0, DateTimeKind.Utc));
            b = await AddCompletedActivityAsync(db, fixture, TraineeUserId, fixture.Paed001.EpaId, score: 4,
                observedOn: new DateOnly(2026, 8, 10), filedOn: new DateTime(2026, 8, 10, 9, 0, 0, DateTimeKind.Utc));
            c = await AddCompletedActivityAsync(db, fixture, TraineeUserId, fixture.Paed001.EpaId, score: 4,
                observedOn: new DateOnly(2026, 8, 11), filedOn: new DateTime(2026, 8, 11, 9, 0, 0, DateTimeKind.Utc));
        }

        (await CreditInFreshContextAsync(fixture.Schema, a)).UpdatedRows.Should().ContainSingle();

        await using (var first = NewContext(fixture.Schema))
        await using (var second = NewContext(fixture.Schema))
        {
            var firstApplier = new CreditApplier(first);
            var secondApplier = new CreditApplier(second);
            var activityB = await first.Activities.Include(entity => entity.Transitions).SingleAsync(entity => entity.Id == b);
            var activityC = await second.Activities.Include(entity => entity.Transitions).SingleAsync(entity => entity.Id == c);

            // Both plans read the same stored semester-2 row before either writes.
            var firstPlan = await firstApplier.PlanAsync(CreditSubject.Of(activityB), PinnedType());
            var secondPlan = await secondApplier.PlanAsync(CreditSubject.Of(activityC), PinnedType());

            firstApplier.Apply(firstPlan, activityB).UpdatedRows.Should().ContainSingle();
            await first.SaveChangesAsync();

            secondApplier.Apply(secondPlan, activityC).UpdatedRows.Should().ContainSingle();
            await FluentActions.Awaiting(() => second.SaveChangesAsync()).Should().ThrowAsync<DbUpdateConcurrencyException>();
        }

        await using (var verify = NewContext(fixture.Schema))
        {
            // B's credit survives. Without the token the second save would have written CountsSoFar 2 with the
            // keys [A, C], and B would be gone.
            var row = (await SnapshotsAsync(verify)).Should().ContainSingle().Subject;
            (row.AcademicYear, row.Semester, row.CountsSoFar, row.CreditedActivityKeys).Should().Be((2026, 2, 2, Keys(a, b)));
        }
    }

    [Fact]
    public async Task TwoFirstCompletionsInANewSemesterAtOnce_OnPostgres_TheSecondInsertIsRefusedByTheNaturalKey()
    {
        // Semester-grain rows mean the first credit of every (item, trainee, semester) is an INSERT, so two first
        // completions racing each other each create "the" row. The unique index is what turns the loser into a
        // loud failure instead of a second bucket for the same semester. InMemory would accept both.
        var fixture = await ArrangePaediatricTraineeAsync();

        int a, b;
        await using (var db = NewContext(fixture.Schema))
        {
            a = await AddCompletedActivityAsync(db, fixture, TraineeUserId, fixture.Paed001.EpaId, score: 4,
                observedOn: new DateOnly(2026, 2, 3), filedOn: new DateTime(2026, 2, 3, 9, 0, 0, DateTimeKind.Utc));
            b = await AddCompletedActivityAsync(db, fixture, TraineeUserId, fixture.Paed001.EpaId, score: 4,
                observedOn: new DateOnly(2026, 2, 4), filedOn: new DateTime(2026, 2, 4, 9, 0, 0, DateTimeKind.Utc));
        }

        await using (var first = NewContext(fixture.Schema))
        await using (var second = NewContext(fixture.Schema))
        {
            var firstApplier = new CreditApplier(first);
            var secondApplier = new CreditApplier(second);
            var activityA = await first.Activities.Include(entity => entity.Transitions).SingleAsync(entity => entity.Id == a);
            var activityB = await second.Activities.Include(entity => entity.Transitions).SingleAsync(entity => entity.Id == b);

            var firstPlan = await firstApplier.PlanAsync(CreditSubject.Of(activityA), PinnedType());
            var secondPlan = await secondApplier.PlanAsync(CreditSubject.Of(activityB), PinnedType());
            firstPlan.ExistingRows.Should().BeEmpty("guard: neither completion can see a semester-1 row yet");
            secondPlan.ExistingRows.Should().BeEmpty();

            firstApplier.Apply(firstPlan, activityA);
            await first.SaveChangesAsync();

            secondApplier.Apply(secondPlan, activityB);
            var refused = await FluentActions.Awaiting(() => second.SaveChangesAsync()).Should().ThrowAsync<DbUpdateException>();
            var postgres = refused.Which.InnerException.Should().BeOfType<PostgresException>().Subject;
            postgres.SqlState.Should().Be("23505");
            postgres.ConstraintName.Should().Be(NaturalKeyIndex);
        }

        await using (var verify = NewContext(fixture.Schema))
        {
            var row = (await SnapshotsAsync(verify)).Should().ContainSingle().Subject;
            (row.AcademicYear, row.Semester, row.CountsSoFar, row.CreditedActivityKeys).Should().Be((2026, 1, 1, Keys(a)));
        }
    }

    private async Task<CreditFixture> ArrangePaediatricTraineeAsync()
    {
        var schema = await SeededSchemaAsync();

        await using var db = NewContext(schema);

        var institutionId = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
        var curriculumId = await db.Curricula.Where(entity => entity.Name == PaediatricCurriculumName).Select(entity => entity.Id).SingleAsync();
        var items = await db.CurriculumItems
            .Where(item => item.CurriculumId == curriculumId && item.OwningInstitutionId == null)
            .Select(item => new { item.Id, item.EpaId, item.Epa.Code })
            .ToListAsync();

        var paed001 = items.Single(item => item.Code == "PAED-001");
        var paed008 = items.Single(item => item.Code == "PAED-008");

        // Starts on the boundary, so no D14 exemption applies to any window of 2026.
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
            Key = "wba_t130_postgres",
            Name = "WBA under test (T130)",
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

        return new CreditFixture(
            schema,
            institutionId,
            curriculumId,
            activityType.Id,
            new CatalogueItem(paed001.Id, paed001.EpaId),
            new CatalogueItem(paed008.Id, paed008.EpaId));
    }

    private static async Task<int> AddCompletedActivityAsync(
        ApplicationDbContext db,
        CreditFixture fixture,
        string subjectUserId,
        int epaId,
        int score,
        DateOnly observedOn,
        DateTime filedOn)
    {
        var activity = new Activity
        {
            ActivityTypeId = fixture.ActivityTypeId,
            SchemaVersion = 1,
            SubjectUserId = subjectUserId,
            CreatedByUserId = subjectUserId,
            CurrentState = "completed",
            DataJson = $$"""{ "epa_id": {{epaId}}, "score": {{score}} }""",
            CreatedOn = filedOn.AddHours(-1),
            UpdatedOn = filedOn,
            ObservedOn = observedOn,
            ObservedOnSource = ObservationDateSource.Declared,
            InstitutionId = fixture.InstitutionId
        };

        activity.Transitions.Add(new ActivityTransition
        {
            FromState = "draft",
            ToState = "submitted",
            TransitionKey = "submit",
            ActorUserId = subjectUserId,
            OccurredOn = filedOn.AddMinutes(-5)
        });

        activity.Transitions.Add(new ActivityTransition
        {
            FromState = "submitted",
            ToState = "completed",
            TransitionKey = "complete",
            ActorUserId = subjectUserId,
            OccurredOn = filedOn
        });

        db.Activities.Add(activity);
        await db.SaveChangesAsync();
        return activity.Id;
    }

    /// <summary>One live completion's credit, as ActivityService does it: its own DbContext, then a save.</summary>
    private async Task<CreditApplicationResult> CreditInFreshContextAsync(string schema, int activityId)
    {
        await using var db = NewContext(schema);
        var activity = await db.Activities.Include(entity => entity.Transitions).SingleAsync(entity => entity.Id == activityId);

        var result = await new CreditApplier(db).ApplyAsync(activity, PinnedType(), CancellationToken.None);

        await db.SaveChangesAsync();
        return result;
    }

    /// <summary>The pinned version's rules and schema, as the live path and the rebuild pass them.</summary>
    private static ActivityType PinnedType() => new() { CreditRulesJson = CreditsTheEpa, SchemaJson = SchemaJson };

    private static async Task<IReadOnlyList<ProgressSnapshot>> SnapshotsAsync(ApplicationDbContext db)
    {
        var rows = await db.CurriculumItemProgresses
            .AsNoTracking()
            .OrderBy(row => row.CurriculumItemId)
            .ThenBy(row => row.TraineeUserId)
            .ThenBy(row => row.AcademicYear)
            .ThenBy(row => row.Semester)
            .ToListAsync();

        // Postgres keeps a key per (item, trainee, year, semester) unique now, but the count is asserted here as
        // well, so a failure names the duplicate rather than surfacing as a 23505 somewhere else.
        rows.GroupBy(row => (row.CurriculumItemId, row.TraineeUserId, row.AcademicYear, row.Semester))
            .Should().OnlyContain(group => group.Count() == 1);

        return rows
            .Select(row => new ProgressSnapshot(
                row.CurriculumItemId,
                row.TraineeUserId,
                row.AcademicYear,
                row.Semester,
                row.CountsSoFar,
                row.MinimumLevelReachedCount,
                row.ScaleMismatchCount,
                row.UnverifiedLevelCount,
                row.LastActivityId,
                row.LastObservedOn,
                Canonical(row.CreditedActivityKeysJson)))
            .ToList();
    }

    private static async Task<IReadOnlyList<int>> RowIdsAsync(ApplicationDbContext db)
        => await db.CurriculumItemProgresses
            .AsNoTracking()
            .OrderBy(row => row.AcademicYear)
            .ThenBy(row => row.Semester)
            .Select(row => row.Id)
            .ToListAsync();

    /// <summary>A stored key set parsed and re-serialised, so Postgres's own jsonb rendering compares equal to ours.</summary>
    private static string Canonical(string json) => JsonSerializer.Serialize(JsonSerializer.Deserialize<string[]>(json) ?? []);

    private static string Keys(params int[] activityIds)
        => JsonSerializer.Serialize(activityIds
            .Select(id => $"{id.ToString(CultureInfo.InvariantCulture)}:complete")
            .OrderBy(key => key, StringComparer.Ordinal));

    private static ClaimsPrincipal Administrator()
        => new(new ClaimsIdentity([new Claim(ClaimTypes.Role, WombatRoles.Administrator)], "IntegrationTest"));

    private static ClaimsPrincipal TraineePrincipal(string userId)
        => new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId), new Claim(ClaimTypes.Role, WombatRoles.Trainee)],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    private async Task InsertProgressRowAsync(string schema, int itemId, string traineeUserId, int academicYear, int semester)
        => await ExecuteAsync(
            schema,
            """
            INSERT INTO "CurriculumItemProgresses"
                ("CurriculumItemId", "TraineeUserId", "AcademicYear", "Semester", "CountsSoFar", "MinimumLevelReachedCount",
                 "ScaleMismatchCount", "UnverifiedLevelCount", "LastUpdated", "CreditedActivityKeysJson")
            VALUES ($1, $2, $3, $4, 1, 0, 0, 0, TIMESTAMPTZ '2026-09-23 08:00:00+00', '[]')
            """,
            itemId, traineeUserId, academicYear, semester);

    private static Task<int> InsertEpaAsync(NpgsqlConnection connection, int subSpecialityId, string code, int? owningInstitutionId)
        => owningInstitutionId is null
            ? InsertAsync(connection,
                """INSERT INTO "Epas" ("SubSpecialityId", "OwningInstitutionId", "Code", "Title", "Category", "CreatedOn", "IsActive") VALUES ($1, NULL, $2, $2, 0, TIMESTAMPTZ '2026-01-01 00:00:00+00', TRUE) RETURNING "Id" """,
                subSpecialityId, code)
            : InsertAsync(connection,
                """INSERT INTO "Epas" ("SubSpecialityId", "OwningInstitutionId", "Code", "Title", "Category", "CreatedOn", "IsActive") VALUES ($1, $3, $2, $2, 0, TIMESTAMPTZ '2026-01-01 00:00:00+00', TRUE) RETURNING "Id" """,
                subSpecialityId, code, owningInstitutionId.Value);

    private static Task<int> InsertCurriculumAsync(NpgsqlConnection connection, int subSpecialityId, string name, string version)
        => InsertAsync(connection,
            """INSERT INTO "Curricula" ("SubSpecialityId", "Name", "Version", "EffectiveFrom", "IsActive") VALUES ($1, $2, $3, DATE '2026-01-01', TRUE) RETURNING "Id" """,
            subSpecialityId, name, version);

    private static Task<int> InsertCurriculumItemAsync(NpgsqlConnection connection, int curriculumId, int epaId, int requiredCount, int? owningInstitutionId)
        => owningInstitutionId is null
            ? InsertAsync(connection,
                """INSERT INTO "CurriculumItems" ("CurriculumId", "EpaId", "OwningInstitutionId", "RequiredCount", "MinimumLevelOrder", "WindowMonths") VALUES ($1, $2, NULL, $3, 6, 12) RETURNING "Id" """,
                curriculumId, epaId, requiredCount)
            : InsertAsync(connection,
                """INSERT INTO "CurriculumItems" ("CurriculumId", "EpaId", "OwningInstitutionId", "RequiredCount", "MinimumLevelOrder", "WindowMonths") VALUES ($1, $2, $4, $3, 6, 12) RETURNING "Id" """,
                curriculumId, epaId, requiredCount, owningInstitutionId.Value);

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

    private async Task<int> ExecuteAsync(string schema, string sql, params object[] values)
    {
        await using var connection = await OpenAsync(schema);
        return await ExecuteAsync(connection, sql, values);
    }

    private async Task<T> ScalarAsync<T>(string schema, string sql, params object[] values)
    {
        await using var connection = await OpenAsync(schema);
        await using var command = Command(connection, sql, values);
        var value = await command.ExecuteScalarAsync();
        return (T)Convert.ChangeType(value!, typeof(T), CultureInfo.InvariantCulture);
    }

    private async Task<List<T>> QueryAsync<T>(string schema, string sql, Func<NpgsqlDataReader, T> map, params object[] values)
    {
        await using var connection = await OpenAsync(schema);
        await using var command = Command(connection, sql, values);
        await using var reader = await command.ExecuteReaderAsync();

        var results = new List<T>();
        while (await reader.ReadAsync())
        {
            results.Add(map(reader));
        }

        return results;
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

    /// <summary>A fresh schema, migrated and seeded the way startup does it: DataSeeder, then the paediatric catalogue.</summary>
    private async Task<string> SeededSchemaAsync()
    {
        var schema = await CreateSchemaAsync();

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

    private ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(SchemaConnectionString(schema))
            .Options);

    /// <summary>
    /// The schema and nothing else on the search path, so an unqualified name can only ever resolve inside it.
    /// </summary>
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

    private sealed record CatalogueItem(int ItemId, int EpaId);

    private sealed record CreditFixture(
        string Schema,
        int InstitutionId,
        int CurriculumId,
        int ActivityTypeId,
        CatalogueItem Paed001,
        CatalogueItem Paed008);

    /// <summary>Everything a replay writes to a row except the audit clock (LastUpdated), which it cannot reproduce.</summary>
    private sealed record ProgressSnapshot(
        int CurriculumItemId,
        string TraineeUserId,
        int AcademicYear,
        int Semester,
        int CountsSoFar,
        int MinimumLevelReachedCount,
        int ScaleMismatchCount,
        int UnverifiedLevelCount,
        int? LastActivityId,
        DateOnly? LastObservedOn,
        string CreditedActivityKeys);

    // Copied from RebuildCurriculumProgressTests: credit the EPA the activity names, gated on "score", which
    // names no scale_key, so every comparison is Unpinned.
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
