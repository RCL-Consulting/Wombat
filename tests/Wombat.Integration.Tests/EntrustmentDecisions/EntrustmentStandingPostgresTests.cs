using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.Activities;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.EntrustmentDecisions;

/// <summary>
/// T166 on a real PostgreSQL server, over the seeded CPSA v11.1 catalogue: the standing query translates, reads
/// Annexure A's year targets from the seeded per-stage maps, and counts the exit rule as the College states it, "Level 5
/// in 9 EPAs and Level 4 in the remaining 6".
/// </summary>
/// <remarks>
/// <para>
/// The unit suite runs on EF InMemory, which never translates a query. This asserts the one inference T166 rests on
/// against the catalogue the seeder actually writes: that "every EPA at or above its own <c>MinimumLevelOrder</c>" and
/// the College's rule are the same statement, because the catalogue pins nine EPAs at rung 5 and six at rung 4.
/// </para>
/// <para>
/// The schema helpers follow <c>EntrustmentDecisionAdminScopePostgresTests</c>: a schema of its own per test, registered
/// before it is created and dropped in a <c>finally</c>, with <see cref="DisposeAsync" /> as a backstop.
/// </para>
/// </remarks>
public sealed class EntrustmentStandingPostgresTests : IAsyncLifetime
{
    private const string TraineeUserId = "trainee-t166";
    private const string CpsaScaleName = "CPSA Paediatric Entrustment Scale v11.1";

    /// <summary>424 days after a 1 January 2025 start: training year 2.</summary>
    private static readonly DateOnly YearTwo = new(2026, 3, 1);

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task TheSeededCatalogue_OnPostgres_CountsTheExitRuleAsLevel5InNineAndLevel4InSix()
    {
        try
        {
            var schema = await SeededSchemaAsync();

            await using (var db = NewContext(schema))
            {
                await SeedTraineeAsync(db);
            }

            EntrustmentStandingDto? standing;
            await using (var db = NewContext(schema))
            {
                standing = await new GetEntrustmentStandingForTraineeQueryHandler(db).Handle(
                    new GetEntrustmentStandingForTraineeQuery(TraineeUserId, Trainee(), YearTwo),
                    CancellationToken.None);
            }

            standing.Should().NotBeNull();
            standing!.TargetYear.Should().Be(2);
            standing.Epas.Should().HaveCount(15);
            standing.Epas.Select(epa => epa.ScaleName).Distinct().Should().Equal(CpsaScaleName);

            standing.Exit.Groups.Select(group => (group.LevelLabel, group.AtLevel, group.EpaCount))
                .Should().Equal([("5", 9, 9), ("4", 5, 6)], "the catalogue pins nine EPAs at rung 5 and six at rung 4");
            standing.Exit.AtExitLevel.Should().Be(14);
            standing.Exit.NotYetEpaCodes.Should().Equal("PAED-008");

            var paed001 = standing.Epas.Single(epa => epa.EpaCode == "PAED-001");
            paed001.YearTargetLabel.Should().Be("3b", "Annexure A's year-2 level for PAED-001");
            paed001.ExitLevelLabel.Should().Be("5");
            paed001.YearStatus.Should().Be(EntrustmentStandingStatus.AtOrAbove);

            var rating = paed001.LatestRating;
            rating.Should().NotBeNull("a completed Mini-CEX rated by its named assessor is on record");
            rating!.RatingLabel.Should().Be("3b");
            rating.OtherLadderName.Should().BeNull("mini_cex_cpsa rates on the ladder PAED-001 is pinned to");
            rating.AgainstYearTarget.Should().Be(EntrustmentStandingStatus.AtOrAbove);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// A trainee in training year 2 on the seeded v11.1 curriculum at the demo institution, holding every EPA's exit
    /// level by STAR decision except PAED-008 (rung 4), which they hold at 3b; and one completed Mini-CEX on PAED-001,
    /// rated 3b by its named assessor.
    /// </summary>
    private static async Task SeedTraineeAsync(ApplicationDbContext db)
    {
        var institutionId = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();

        var items = await db.CurriculumItems
            .Where(item => item.Epa.Code.StartsWith("PAED-"))
            .Select(item => new { item.CurriculumId, item.EpaId, item.Epa.Code, item.MinimumLevelOrder })
            .ToListAsync();
        var curriculumId = items.Select(item => item.CurriculumId).Distinct().Single();

        var levelByOrder = await db.EntrustmentLevels
            .Where(level => level.Scale.Name == CpsaScaleName)
            .ToDictionaryAsync(level => level.Order, level => level.Id);
        var rung3b = levelByOrder[4];

        db.TraineeProfiles.Add(new TraineeProfile
        {
            UserId = TraineeUserId,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            ProgrammeStartDate = new DateOnly(2025, 1, 1),
            ExpectedCompletionDate = new DateOnly(2029, 1, 1),
            IsActive = true
        });

        var panel = new DecisionPanel
        {
            Name = "T166 CCC",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = institutionId,
            CreatedOn = DateTime.UtcNow,
            Members = [new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair }]
        };
        db.DecisionPanels.Add(panel);
        await db.SaveChangesAsync();

        var review = new CommitteeReview
        {
            AcademicYear = 2025,
            Semester = 2,
            PanelId = panel.Id,
            TraineeUserId = TraineeUserId,
            ReviewPeriodFrom = new DateOnly(2025, 1, 1),
            ReviewPeriodTo = new DateOnly(2025, 12, 31),
            ScheduledOn = new DateOnly(2026, 1, 15)
        };
        db.CommitteeReviews.Add(review);
        db.Entry(review).Property(entity => entity.State).CurrentValue = CommitteeReviewState.Ratified;
        await db.SaveChangesAsync();

        foreach (var item in items)
        {
            var levelId = item.Code == "PAED-008" ? rung3b : levelByOrder[item.MinimumLevelOrder];
            db.EntrustmentDecisions.Add(EntrustmentDecision.Issue(
                TraineeUserId, item.EpaId, levelId, new DateOnly(2026, 1, 15), expiresOn: null,
                review.Id, "chair-1", "Consistent across the period.", StarEvidence.One()));
        }

        var miniCex = await db.ActivityTypes.Where(type => type.Key == "mini_cex_cpsa").Select(type => new { type.Id, type.Version }).SingleAsync();
        var paed001 = items.Single(item => item.Code == "PAED-001").EpaId;
        db.Activities.Add(new Activity
        {
            ActivityTypeId = miniCex.Id,
            SchemaVersion = miniCex.Version,
            SubjectUserId = TraineeUserId,
            CreatedByUserId = TraineeUserId,
            CurrentState = "completed",
            DataJson = $$"""{ "epa_id": {{paed001}}, "assessor_user_id": "assessor-1", "overall_level": 4 }""",
            EpaId = paed001,
            CreatedOn = new DateTime(2026, 2, 20, 9, 0, 0, DateTimeKind.Utc),
            UpdatedOn = new DateTime(2026, 2, 20, 10, 0, 0, DateTimeKind.Utc),
            ObservedOn = new DateOnly(2026, 2, 20),
            ObservedOnSource = ObservationDateSource.Declared,
            InstitutionId = institutionId
        });

        await db.SaveChangesAsync();
    }

    private static ClaimsPrincipal Trainee()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, TraineeUserId),
                new Claim(ClaimTypes.Role, WombatRoles.Trainee)
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

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

    private ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema)).Options);
}
