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
/// T307 (D51) on a real PostgreSQL server: the migration that rewrites every stored Upheld appeal (1) as Dismissed (2),
/// what the engine had done with it, and then refuses 1 with <c>CK_CommitteeAppeals_Outcome</c>.
/// </summary>
/// <remarks>
/// The unit suites run on EF InMemory, which runs no migration and enforces no check, and on a fresh database the rewrite
/// touches nothing. So it is rehearsed on a schema stopped at the last migration before it. That schema has every table and
/// column the current model has (T307 adds a check and changes no column), so its rows are written through the domain,
/// and the one Upheld outcome, which the domain no longer writes, by raw SQL. The schema helpers follow
/// <c>SystemManagedMigrationPostgresTests</c>.
/// </remarks>
public sealed class AppealOutcomeMigrationPostgresTests : IAsyncLifetime
{
    private const string LastMigrationBeforeT307 = "20260925172953_T281_ProgressWithoutEncountersAfterTheEnd";

    private static readonly DateTime Sitting = new(2026, 9, 1, 9, 0, 0, DateTimeKind.Utc);

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task Migration_RewritesUpheldAsDismissed_LeavesEveryOtherAppeal_AndRefusesUpheldFromThenOn()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await using (var db = NewContext(schema))
            {
                await db.GetService<IMigrator>().MigrateAsync(LastMigrationBeforeT307);
                (await db.Database.GetPendingMigrationsAsync())
                    .Should().ContainSingle(migration => migration.EndsWith("_T307_AppealOutcomesDismissedOrRemitted", StringComparison.Ordinal));
            }

            int upheld, dismissed, remitted, open;
            await using (var db = NewContext(schema))
            {
                var institution = new Institution { Name = "Kgosi Kgari", ShortCode = "KGK", IsActive = true, CreatedOn = Sitting };
                db.Institutions.Add(institution);
                await db.SaveChangesAsync();

                var members = new[]
                {
                    new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair },
                    new DecisionPanelMember { UserId = "member-1", Role = DecisionPanelMemberRole.Member },
                    new DecisionPanelMember { UserId = "external-1", Role = DecisionPanelMemberRole.External }
                };
                var panel = new DecisionPanel
                {
                    Name = "Paed Annual Review Panel",
                    Scope = DecisionPanelScope.Institution,
                    InstitutionId = institution.Id,
                    CreatedOn = Sitting,
                    Members = members
                };
                db.DecisionPanels.Add(panel);

                var reviews = Enumerable.Range(1, 4).Select(trainee => AppealedReview(panel, members, $"trainee-{trainee}")).ToArray();
                reviews[0].ResolveAppeal(CommitteeAppealOutcome.Dismissed, "chair-1", Sitting.AddDays(30)); // made Upheld below
                reviews[1].ResolveAppeal(CommitteeAppealOutcome.Dismissed, "chair-1", Sitting.AddDays(30));
                reviews[2].ResolveAppeal(
                    CommitteeAppealOutcome.Remitted, "external-1", Sitting.AddDays(30), CommitteeDecisionCategory.SatisfactoryWithObservations,
                    "Progress is adequate.", "Two observed Mini-CEX before the next review.", [members[0], members[2]]);
                db.CommitteeReviews.AddRange(reviews);
                await db.SaveChangesAsync();

                var appealIds = reviews.Select(review => review.Appeals.Single().Id).ToArray();
                (upheld, dismissed, remitted, open) = (appealIds[0], appealIds[1], appealIds[2], appealIds[3]);
            }

            (await ExecuteAsync(schema, """UPDATE "CommitteeAppeals" SET "Outcome" = 1 WHERE "Id" = $1""", upheld))
                .Should().Be(1, "guard: before T307 an Upheld outcome could be stored");

            await using (var db = NewContext(schema))
            {
                await db.Database.MigrateAsync();
                (await db.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
            }

            await using (var db = NewContext(schema))
            {
                var outcomes = await db.CommitteeAppeals.AsNoTracking().ToDictionaryAsync(appeal => appeal.Id, appeal => appeal.Outcome);
                outcomes.Should().Equal(new Dictionary<int, CommitteeAppealOutcome?>
                {
                    [upheld] = CommitteeAppealOutcome.Dismissed,
                    [dismissed] = CommitteeAppealOutcome.Dismissed,
                    [remitted] = CommitteeAppealOutcome.Remitted,
                    [open] = null
                });

                // The rewrite changes the name, not what happened: the Upheld appeal's review keeps its one decision.
                var reviewOfUpheld = await db.CommitteeReviews.AsNoTracking().Include(review => review.Decisions)
                    .SingleAsync(review => review.Appeals.Any(appeal => appeal.Id == upheld));
                reviewOfUpheld.Decisions.Should().ContainSingle()
                    .Which.Category.Should().Be(CommitteeDecisionCategory.InadequateProgressAdditionalTraining);
            }

            foreach (var value in new[] { 0, 1, 4 })
            {
                var refused = () => ExecuteAsync(schema, """UPDATE "CommitteeAppeals" SET "Outcome" = $1 WHERE "Id" = $2""", value, dismissed);
                (await refused.Should().ThrowAsync<PostgresException>($"outcome {value}"))
                    .Which.ConstraintName.Should().Be("CK_CommitteeAppeals_Outcome");
            }

            (await ExecuteAsync(schema, """UPDATE "CommitteeAppeals" SET "Outcome" = 3 WHERE "Id" = $1""", dismissed))
                .Should().Be(1, "Remitted is an outcome");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static CommitteeReview AppealedReview(DecisionPanel panel, DecisionPanelMember[] members, string traineeUserId)
    {
        var review = new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 2,
            Panel = panel,
            TraineeUserId = traineeUserId,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 12, 31),
            ScheduledOn = new DateOnly(2026, 9, 1)
        };
        review.Start([], "chair-1", Sitting);
        review.RecordDecision(
            CommitteeDecisionCategory.InadequateProgressAdditionalTraining, "Not enough observed evidence.", null, "chair-1",
            Sitting, [members[0], members[1]], [], []);
        review.Ratify("chair-1", Sitting);
        review.LodgeAppeal("The single DOPS reflects the start of the year.", traineeUserId, Sitting.AddDays(1));
        return review;
    }

    private static ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema)).Options);

    /// <summary>Positional parameters ($1, $2, …), so no value is ever spliced into SQL text.</summary>
    private static async Task<int> ExecuteAsync(string schema, string sql, params object[] values)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        await using var command = new NpgsqlCommand(sql, connection);
        foreach (var value in values)
        {
            command.Parameters.Add(new NpgsqlParameter { Value = value });
        }

        return Convert.ToInt32(await command.ExecuteNonQueryAsync(), CultureInfo.InvariantCulture);
    }
}
