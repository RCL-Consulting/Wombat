using System.Globalization;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.CommitteeDecisions;

/// <summary>
/// T131 slice 5 on a real PostgreSQL server: a committee decision's category became optional, so an entrustment-only
/// review's decision is stored with none, and every category recorded before keeps its value.
/// </summary>
/// <remarks>
/// EF InMemory, which the handler tests run on, ignores a column's nullability as the migration writes it. The schema
/// helpers follow <c>CommitteeAgendaPostgresTests</c>: each test runs on a schema of its own, dropped in a <c>finally</c>,
/// with <see cref="DisposeAsync" /> as a backstop.
/// </remarks>
public sealed class EntrustmentOnlyReviewPostgresTests : IAsyncLifetime
{
    private const string ThisMigrationSuffix = "_T131_EntrustmentOnlyReviews";

    /// <summary>PostgreSQL's not_null_violation.</summary>
    private const string NotNullViolation = "23502";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task Migration_KeepsEveryRecordedCategory_AndLetsADecisionRecordNone()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await using (var db = NewContext(schema))
            {
                // The migration just before this one in the assembly, both found rather than named: a migration merged in
                // from another lane can fall between the two by its timestamp, and this one can be regenerated after it.
                var migrations = db.Database.GetMigrations().ToList();
                var thisMigration = migrations.FindIndex(migration => migration.EndsWith(ThisMigrationSuffix, StringComparison.Ordinal));
                thisMigration.Should().BePositive("guard: the T131 slice 5 migration is in the assembly, after at least one other");
                await db.GetService<IMigrator>().MigrateAsync(migrations[thisMigration - 1]);
                (await db.Database.GetPendingMigrationsAsync())
                    .Should().ContainSingle(migration => migration.EndsWith(ThisMigrationSuffix, StringComparison.Ordinal));
            }

            int reviewId, recorded;
            await using (var connection = await TestDatabase.OpenSchemaConnectionAsync(schema))
            {
                var institutionId = await InsertAsync(connection,
                    """INSERT INTO "Institutions" ("Name", "ShortCode", "IsActive", "CreatedOn") VALUES ('KGK', 'KGK', TRUE, TIMESTAMPTZ '2026-06-19 00:00:00+00') RETURNING "Id" """);
                var panelId = await InsertAsync(connection,
                    """INSERT INTO "DecisionPanels" ("Name", "Scope", "InstitutionId", "CreatedOn") VALUES ('Annual review', 1, $1, TIMESTAMPTZ '2026-06-19 00:00:00+00') RETURNING "Id" """,
                    institutionId);
                reviewId = await InsertAsync(connection,
                    """INSERT INTO "CommitteeReviews" ("TraineeUserId", "PanelId", "AcademicYear", "Semester", "ReviewPeriodFrom", "ReviewPeriodTo", "ScheduledOn", "IsFormative", "ReviewType", "State") VALUES ('trainee-1', $1, 2026, 1, DATE '2026-01-01', DATE '2026-06-30', DATE '2026-07-02', FALSE, 1, 3) RETURNING "Id" """,
                    panelId);
                recorded = await InsertDecisionAsync(connection, reviewId, (int)CommitteeDecisionCategory.InadequateProgressRepeat);

                // Before the migration the column refuses a decision with no category.
                var refused = () => InsertDecisionAsync(connection, reviewId, category: null);
                (await refused.Should().ThrowAsync<PostgresException>()).Which.SqlState.Should().Be(NotNullViolation);
            }

            await using (var db = NewContext(schema))
            {
                await db.Database.MigrateAsync();
                (await db.Database.GetAppliedMigrationsAsync())
                    .Should().ContainSingle(migration => migration.EndsWith(ThisMigrationSuffix, StringComparison.Ordinal));
            }

            await using (var connection = await TestDatabase.OpenSchemaConnectionAsync(schema))
            {
                var none = await InsertDecisionAsync(connection, reviewId, category: null);
                await using var read = NewContext(schema);
                var categories = await read.CommitteeDecisions.AsNoTracking()
                    .ToDictionaryAsync(decision => decision.Id, decision => decision.Category);
                categories[recorded].Should().Be(CommitteeDecisionCategory.InadequateProgressRepeat);
                categories[none].Should().BeNull();
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task AnEntrustmentOnlyReview_IsRecordedAndRatifiedWithNoCategory_AndReadsBackSo()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            int reviewId;
            await using (var db = NewContext(schema))
            {
                await db.Database.MigrateAsync();

                var institution = new Institution { Name = "KGK", ShortCode = "KGK", IsActive = true, CreatedOn = DateTime.UtcNow };
                db.Institutions.Add(institution);
                await db.SaveChangesAsync();

                var members = new List<DecisionPanelMember>
                {
                    new() { UserId = "chair-n", Role = DecisionPanelMemberRole.Chair },
                    new() { UserId = "member-n", Role = DecisionPanelMemberRole.Member }
                };
                var panel = new DecisionPanel
                {
                    Name = "Neonatal CCC",
                    Scope = DecisionPanelScope.Institution,
                    InstitutionId = institution.Id,
                    CreatedOn = DateTime.UtcNow,
                    Members = members
                };
                var sitting = new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc);
                var review = new CommitteeReview
                {
                    TraineeUserId = "trainee-1",
                    Panel = panel,
                    AcademicYear = 2026,
                    Semester = 1,
                    ReviewPeriodFrom = new DateOnly(2026, 1, 1),
                    ReviewPeriodTo = new DateOnly(2026, 6, 30),
                    ScheduledOn = new DateOnly(2026, 7, 2),
                    ReviewType = CommitteeReviewType.EntrustmentOnly
                };
                review.Start([], "chair-n", sitting);
                review.RecordDecision(null, "PAED-004 entrusted at 3a.", null, "chair-n", sitting, members, [], []);
                review.Ratify("chair-n", sitting);
                db.CommitteeReviews.Add(review);
                await db.SaveChangesAsync();
                reviewId = review.Id;
            }

            await using (var read = NewContext(schema))
            {
                var stored = await read.CommitteeReviews.AsNoTracking()
                    .Include(review => review.Decisions)
                    .SingleAsync(review => review.Id == reviewId);
                stored.ReviewType.Should().Be(CommitteeReviewType.EntrustmentOnly);
                stored.State.Should().Be(CommitteeReviewState.Ratified);
                stored.Decisions.Should().ContainSingle().Which.Category.Should().BeNull();
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ---- arrange --------------------------------------------------------------------------------------------------------

    private static async Task<int> InsertDecisionAsync(NpgsqlConnection connection, int reviewId, int? category)
    {
        await using var command = new NpgsqlCommand(
            """INSERT INTO "CommitteeDecisions" ("ReviewId", "Category", "Rationale", "DecidedOn", "DecidedByChairUserId") VALUES ($1, $2, 'Decided.', TIMESTAMPTZ '2026-07-02 09:00:00+00', 'chair-1') RETURNING "Id" """,
            connection);
        command.Parameters.Add(new NpgsqlParameter { Value = reviewId });
        command.Parameters.Add(new NpgsqlParameter
        {
            Value = (object?)category ?? DBNull.Value,
            NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Integer
        });
        return Convert.ToInt32(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    /// <summary>Positional parameters ($1, $2, …), so no value is ever spliced into SQL text.</summary>
    private static async Task<int> InsertAsync(NpgsqlConnection connection, string sql, params object[] values)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        foreach (var value in values)
        {
            command.Parameters.Add(new NpgsqlParameter { Value = value });
        }

        return Convert.ToInt32(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    // ---- schemas --------------------------------------------------------------------------------------------------------

    private ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema)).Options);
}
