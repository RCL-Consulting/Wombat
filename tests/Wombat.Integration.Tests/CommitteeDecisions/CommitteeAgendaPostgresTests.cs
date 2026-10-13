using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Wombat.Application.Common.Persistence;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

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
    private const string ThisMigration = "20260924212708_T131_ReviewAgenda";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task Migration_StampsEachExistingReviewWithTheSemesterItsWindowEndsIn_AndGivesItNoAgenda()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await using (var db = NewContext(schema))
            {
                // The migration just before this one in the assembly, found rather than named: a migration merged in
                // from another lane can fall between the two by its timestamp.
                var migrations = db.Database.GetMigrations().ToList();
                migrations.Should().Contain(ThisMigration);
                await db.GetService<IMigrator>().MigrateAsync(migrations[migrations.IndexOf(ThisMigration) - 1]);
                (await db.Database.GetPendingMigrationsAsync()).First().Should().Be(ThisMigration);
            }

            int wholeYear, firstHalf;
            await using (var connection = await TestDatabase.OpenSchemaConnectionAsync(schema))
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
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task TheNoDeferralMigration_WritesADeferredLineDueWhileItsReviewIsOpen_NotDecidedOnceClosed_AndDropsTheReason_AndAddsTheDecisionsSentence()
    {
        // T361, D53, on a schema stopped at the migration before it, with the rows it rewrites written as that schema held
        // them: a Deferred line (2) with its reason, a Due line and a NotDecided one on a ratified review; and a Deferred
        // line on a Scheduled and on an In progress review, which are still to decide (review R2).
        const string NoDeferral = "_T361_NoDeferral";
        try
        {
            var schema = await _schemas.CreateAsync();
            await using (var db = NewContext(schema))
            {
                var migrations = db.Database.GetMigrations().ToList();
                var t361 = migrations.Single(migration => migration.EndsWith(NoDeferral, StringComparison.Ordinal));
                await db.GetService<IMigrator>().MigrateAsync(migrations[migrations.IndexOf(t361) - 1]);
                (await db.Database.GetPendingMigrationsAsync()).First().Should().Be(t361);
            }

            int deferred, due, notDecided, decisionReview, deferredWhileScheduled, deferredWhileInProgress;
            await using (var connection = await TestDatabase.OpenSchemaConnectionAsync(schema))
            {
                var panelId = await InsertPanelAsync(connection, await InsertInstitutionAsync(connection, "KGK"));
                var reviewId = await InsertReviewAsync(connection, panelId, 2026, 1, state: 4);
                deferred = await InsertLineBeforeT361Async(connection, reviewId, 2, state: 2, reason: "Not observed this semester.");
                due = await InsertLineBeforeT361Async(connection, reviewId, 1, state: 1, reason: null);
                notDecided = await InsertLineBeforeT361Async(connection, reviewId, 3, state: 4, reason: null);
                decisionReview = reviewId;

                var scheduled = await InsertReviewAsync(connection, panelId, 2026, 2, state: 1, trainee: "trainee-2");
                deferredWhileScheduled = await InsertLineBeforeT361Async(connection, scheduled, 2, state: 2, reason: "Not observed yet.");
                var inProgress = await InsertReviewAsync(connection, panelId, 2026, 2, state: 2, trainee: "trainee-3");
                deferredWhileInProgress = await InsertLineBeforeT361Async(connection, inProgress, 2, state: 2, reason: "Not observed yet.");
                await ExecuteAsync(connection,
                    """INSERT INTO "CommitteeDecisions" ("ReviewId", "Category", "Rationale", "DecidedOn", "DecidedByChairUserId") VALUES ($1, 1, 'On track.', TIMESTAMPTZ '2026-07-02 09:00:00+00', 'chair-1')""",
                    reviewId);
            }

            await using (var db = NewContext(schema))
            {
                await db.Database.MigrateAsync();
                (await db.Database.GetPendingMigrationsAsync()).Should().BeEmpty();

                var states = await db.CommitteeAgendaLines.AsNoTracking().ToDictionaryAsync(line => line.Id, line => line.State);
                states.Should().Equal(new Dictionary<int, Wombat.Domain.CommitteeDecisions.CommitteeAgendaLineState>
                {
                    [deferred] = Wombat.Domain.CommitteeDecisions.CommitteeAgendaLineState.NotDecided,
                    [due] = Wombat.Domain.CommitteeDecisions.CommitteeAgendaLineState.Due,
                    [notDecided] = Wombat.Domain.CommitteeDecisions.CommitteeAgendaLineState.NotDecided,
                    [deferredWhileScheduled] = Wombat.Domain.CommitteeDecisions.CommitteeAgendaLineState.Due,
                    [deferredWhileInProgress] = Wombat.Domain.CommitteeDecisions.CommitteeAgendaLineState.Due
                });
                (await db.CommitteeDecisions.AsNoTracking().SingleAsync(decision => decision.ReviewId == decisionReview))
                    .UndecidedNote.Should().BeNull("no stored reason is guessed into the decision's sentence");
            }

            await using (var connection = await TestDatabase.OpenSchemaConnectionAsync(schema))
            {
                (await ScalarAsync<long>(connection,
                        """SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = current_schema() AND table_name = 'CommitteeAgendaLines' AND column_name = 'DeferralReason'"""))
                    .Should().Be(0);
                (await ScalarAsync<long>(connection,
                        """SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = current_schema() AND table_name = 'CommitteeDecisions' AND column_name = 'UndecidedNote' AND character_maximum_length = 4000"""))
                    .Should().Be(1);
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task OneOpenBindingReview_PerTraineePanelAndPeriod_IsHeldByTheIndex()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
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
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task AnAgendaLine_IsDecidedExactlyWhenItNamesAStar_AndCarriesNoDeferral()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
            var panelId = await InsertPanelAsync(connection, await InsertInstitutionAsync(connection, "A"));
            var reviewId = await InsertReviewAsync(connection, panelId, 2026, 1, state: 2);

            await InsertLineAsync(connection, reviewId, epaId: 1, windowSemester: 1, state: 1);
            await InsertLineAsync(connection, reviewId, epaId: 3, windowSemester: 1, state: 4);

            // T235: 5 is DecidedElsewhere. The STAR that decided the window is another sitting's, so the line names none.
            await InsertLineAsync(connection, reviewId, epaId: 4, windowSemester: null, state: 5);

            await RefusedAsync(() => InsertLineAsync(connection, reviewId, 10, 3, 1), "CK_CommitteeAgendaLines_WindowSemester");
            await RefusedAsync(() => InsertLineAsync(connection, reviewId, 11, 1, 3), "CK_CommitteeAgendaLines_DecidedNamesItsStar");

            // T361, D53: the deferral's constraint and its reason went with it.
            (await ScalarAsync<long>(connection,
                    """SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = current_schema() AND table_name = 'CommitteeAgendaLines' AND column_name = 'DeferralReason'"""))
                .Should().Be(0);
            (await ScalarAsync<long>(connection,
                    """SELECT COUNT(*) FROM pg_constraint WHERE conname = 'CK_CommitteeAgendaLines_DeferredHasAReason' AND connamespace = current_schema()::regnamespace"""))
                .Should().Be(0);

            var secondLineOnTheEpa = async () => await InsertLineAsync(connection, reviewId, 1, 1, 1);
            (await secondLineOnTheEpa.Should().ThrowAsync<PostgresException>()).Which.SqlState
                .Should().Be(PostgresErrorCodes.UniqueViolation);

            // A line leaves with its review.
            await ExecuteAsync(connection, """DELETE FROM "CommitteeReviews" WHERE "Id" = $1""", reviewId);
            (await ScalarAsync<long>(connection, """SELECT COUNT(*) FROM "CommitteeAgendaLines" """)).Should().Be(0);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task AnAgendaLine_NamesAStarOnlyWhenDecided_AndTheStarItNamesCannotBeDeleted()
    {
        // The other direction of CK_CommitteeAgendaLines_DecidedNamesItsStar: a line that is not Decided names no STAR.
        try
        {
            var schema = await MigratedSchemaAsync();
            await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
            var panelId = await InsertPanelAsync(connection, await InsertInstitutionAsync(connection, "A"));
            var reviewId = await InsertReviewAsync(connection, panelId, 2026, 1, state: 4);
            var starId = await InsertStarAsync(connection, reviewId);

            await RefusedAsync(() => InsertLineAsync(connection, reviewId, 20, 1, 1, starId), "CK_CommitteeAgendaLines_DecidedNamesItsStar");
            await RefusedAsync(() => InsertLineAsync(connection, reviewId, 22, 1, 4, starId), "CK_CommitteeAgendaLines_DecidedNamesItsStar");
            await RefusedAsync(() => InsertLineAsync(connection, reviewId, 24, null, 5, starId), "CK_CommitteeAgendaLines_DecidedNamesItsStar");
            var decided = await InsertLineAsync(connection, reviewId, 23, 1, 3, starId);

            // STARs are revoked, never deleted; the key restricts, so a Decided line never loses the STAR it names.
            var deleteTheStar = async () => await ExecuteAsync(connection, """DELETE FROM "EntrustmentDecisions" WHERE "Id" = $1""", starId);
            var refusal = (await deleteTheStar.Should().ThrowAsync<PostgresException>()).Which;
            refusal.SqlState.Should().BeOneOf(PostgresErrors.ForeignKeyViolationStates, "23001 on PostgreSQL 18 (T243)");
            refusal.ConstraintName.Should().Be("FK_CommitteeAgendaLines_EntrustmentDecisions_EntrustmentDecisi~");
            (await InsertAsync(connection,
                    """SELECT COUNT(*) FROM "CommitteeAgendaLines" WHERE "Id" = $1 AND "EntrustmentDecisionId" = $2""", decided, starId))
                .Should().Be(1);
        }
        finally
        {
            await _schemas.DropAllAsync();
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
            await using (var connection = await TestDatabase.OpenSchemaConnectionAsync(schema))
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

                // The handler's own refusal, which the page shows in its words (T364, build review R4).
                var refusal = await ours.Should().ThrowExactlyAsync<CommitteeRefusedException>();
                // The trainee by name, here the directory's id for want of one, and "scheduled" once (T364, C7).
                refusal.Which.Message.Should().StartWith(
                    $"Review #{rivalId} already puts trainee-1 before Annual review for 2026 S1 (scheduled on 2026-07-02).");
                refusal.Which.InnerException.Should().BeAssignableTo<DbUpdateException>();
            }

            await using (var read = NewContext(schema))
            {
                (await read.CommitteeReviews.Select(review => review.Id).ToListAsync()).Should().Equal(rivalId);
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ---- arrange --------------------------------------------------------------------------------------------------------

    [Fact]
    public async Task TheHeldSeat_AndCommitteeReviewsInstitution_AreReadOnPostgres()
    {
        // T364: the schedule page's Already held (GetSchedulingSeatQuery) and Committee reviews' caption (the institution's
        // name, a subquery in the list's projection) run on Postgres as on the in-memory store.
        try
        {
            var schema = await MigratedSchemaAsync();
            int institutionId, panelId;
            await using (var connection = await TestDatabase.OpenSchemaConnectionAsync(schema))
            {
                institutionId = await InsertInstitutionAsync(connection, "A");
                panelId = await InsertPanelAsync(connection, institutionId);
                await ExecuteAsync(connection,
                    """INSERT INTO "TraineeProfiles" ("UserId", "InstitutionId", "CurriculumId", "ProgrammeStartDate", "ExpectedCompletionDate", "IsActive") VALUES ('trainee-1', $1, $2, DATE '2025-01-15', DATE '2029-01-14', TRUE)""",
                    institutionId, await InsertCurriculumAsync(connection));
            }

            await using var db = NewContext(schema);
            var scheduled = await ScheduleAsync(db, panelId, Coordinator(institutionId));

            var seat = await new GetSchedulingSeatQueryHandler(db, FakeUserDirectory.TraineesOf(db)).Handle(
                new GetSchedulingSeatQuery("trainee-1", panelId, 2026, 1, Coordinator(institutionId)), CancellationToken.None);
            seat.Should().NotBeNull();
            seat!.ReviewId.Should().Be(scheduled.Id);
            seat.Sentence.Should().StartWith($"Review #{scheduled.Id} already puts trainee-1 before Annual review for 2026 S1 (scheduled on 2026-07-02).");

            var listed = await new ListReviewsForPanelQueryHandler(db, FakeUserDirectory.TraineesOf(db)).Handle(
                new ListReviewsForPanelQuery(Coordinator(institutionId)), CancellationToken.None);
            listed.Should().ContainSingle().Which.InstitutionName.Should().Be("A", "the institution's name as stored");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static Task<CommitteeReviewListItemDto> ScheduleAsync(ApplicationDbContext db, int panelId, ClaimsPrincipal principal)
        => new ScheduleCommitteeReviewCommandHandler(db, FakeUserDirectory.TraineesOf(db)).Handle(
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
        var schema = await _schemas.CreateAsync();
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
        NpgsqlConnection connection, int reviewId, int epaId, int? windowSemester, int state, int? starId = null)
        => InsertAsync(connection,
            """INSERT INTO "CommitteeAgendaLines" ("ReviewId", "CurriculumItemId", "EpaId", "EpaCode", "EpaTitle", "Origin", "WindowYear", "WindowSemester", "IsClosing", "IsPartialPeriod", "State", "EntrustmentDecisionId") VALUES ($1, $2, $2, 'PAED-' || $2::text, 'An EPA', 1, 2026, $3, TRUE, FALSE, $4, $5::integer) RETURNING "Id" """,
            reviewId,
            epaId,
            (object?)windowSemester ?? DBNull.Value,
            state,
            (object?)starId ?? DBNull.Value);

    /// <summary>A line as the schema before T361 stores one, with the deferral's reason column.</summary>
    private static Task<int> InsertLineBeforeT361Async(
        NpgsqlConnection connection, int reviewId, int epaId, int state, string? reason)
        => InsertAsync(connection,
            """INSERT INTO "CommitteeAgendaLines" ("ReviewId", "CurriculumItemId", "EpaId", "EpaCode", "EpaTitle", "Origin", "WindowYear", "WindowSemester", "IsClosing", "IsPartialPeriod", "State", "DeferralReason") VALUES ($1, $2, $2, 'PAED-' || $2::text, 'An EPA', 1, 2026, 1, TRUE, FALSE, $3, $4) RETURNING "Id" """,
            reviewId,
            epaId,
            state,
            (object?)reason ?? DBNull.Value);

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

    /// <param name="schema">The test's schema.</param>
    /// <param name="beforeFirstSave">
    /// Another request, run from start to commit just before this context's first save reaches the database: the
    /// overlap of a race, made exact (as <c>CommitteeReviewRacePostgresTests</c> does).
    /// </param>
    private ApplicationDbContext NewContext(string schema, Func<Task>? beforeFirstSave = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema));
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
}
