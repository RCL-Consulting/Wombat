using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Activities;

public sealed class CreditApplierTests
{
    [Fact]
    public async Task ApplyAsync_IncrementsMatchingProgressRow()
    {
        await using var dbContext = CreateDbContext();
        SeedCurriculum(dbContext);

        var activity = CreateCompletedActivity("""{ "epa_id": 5000, "score": 4 }""");
        var applier = new CreditApplier(dbContext);

        var updated = await applier.ApplyAsync(activity, CreateActivityType(), CancellationToken.None);
        await dbContext.SaveChangesAsync();

        updated.Should().ContainSingle();
        var progress = await dbContext.CurriculumItemProgresses.SingleAsync();
        progress.CountsSoFar.Should().Be(1);
        progress.MinimumLevelReachedCount.Should().Be(1);
        progress.LastActivityId.Should().Be(activity.Id);
    }

    [Fact]
    public async Task ApplyAsync_WhenNoCurriculumItemMatches_DoesNothing()
    {
        await using var dbContext = CreateDbContext();
        SeedCurriculum(dbContext);

        var activity = CreateCompletedActivity("""{ "epa_id": 9999, "score": 4 }""");
        var applier = new CreditApplier(dbContext);

        var updated = await applier.ApplyAsync(activity, CreateActivityType(), CancellationToken.None);
        await dbContext.SaveChangesAsync();

        updated.Should().BeEmpty();
        dbContext.CurriculumItemProgresses.Should().BeEmpty();
    }

    [Fact]
    public async Task ApplyAsync_WhenMinimumLevelIsBelowRequired_CountsVolumeButNotLevelReached()
    {
        // T071: a below-required-level completion still counts as volume evidence (CountsSoFar),
        // but does not contribute to MinimumLevelReachedCount.
        await using var dbContext = CreateDbContext();
        SeedCurriculum(dbContext);

        var activity = CreateCompletedActivity("""{ "epa_id": 5000, "score": 2 }""");
        var applier = new CreditApplier(dbContext);

        var updated = await applier.ApplyAsync(activity, CreateActivityType(), CancellationToken.None);
        await dbContext.SaveChangesAsync();

        updated.Should().ContainSingle();
        var progress = await dbContext.CurriculumItemProgresses.SingleAsync();
        progress.CountsSoFar.Should().Be(1);
        progress.MinimumLevelReachedCount.Should().Be(0);
        progress.LastActivityId.Should().Be(activity.Id);
    }

    [Fact]
    public async Task ApplyAsync_AccumulatesVolumeAndLevelReachedSeparatelyAcrossActivities()
    {
        // T071: two completions on the same curriculum item — one below the required level, one at
        // it — should leave CountsSoFar=2 and MinimumLevelReachedCount=1.
        await using var dbContext = CreateDbContext();
        SeedCurriculum(dbContext);

        var applier = new CreditApplier(dbContext);

        var belowLevel = CreateCompletedActivity("""{ "epa_id": 5000, "score": 2 }""", activityId: 100);
        await applier.ApplyAsync(belowLevel, CreateActivityType(), CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var atLevel = CreateCompletedActivity("""{ "epa_id": 5000, "score": 4 }""", activityId: 101);
        await applier.ApplyAsync(atLevel, CreateActivityType(), CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var progress = await dbContext.CurriculumItemProgresses.SingleAsync();
        progress.CountsSoFar.Should().Be(2);
        progress.MinimumLevelReachedCount.Should().Be(1);
        progress.LastActivityId.Should().Be(atLevel.Id);
    }

    [Fact]
    public async Task ApplyAsync_IsIdempotentForTheSameActivity()
    {
        await using var dbContext = CreateDbContext();
        SeedCurriculum(dbContext);

        var activity = CreateCompletedActivity("""{ "epa_id": 5000, "score": 4 }""");
        var applier = new CreditApplier(dbContext);

        await applier.ApplyAsync(activity, CreateActivityType(), CancellationToken.None);
        await applier.ApplyAsync(activity, CreateActivityType(), CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var progress = await dbContext.CurriculumItemProgresses.SingleAsync();
        progress.CountsSoFar.Should().Be(1);
        progress.MinimumLevelReachedCount.Should().Be(1);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static void SeedCurriculum(ApplicationDbContext dbContext)
    {
        dbContext.Epas.Add(new Epa
        {
            Id = 5000,
            Code = "EPA-1",
            Title = "Take a history"
        });

        dbContext.CurriculumItems.Add(new CurriculumItem
        {
            Id = 4000,
            CurriculumId = 3000,
            EpaId = 5000,
            RequiredCount = 3,
            MinimumLevelOrder = 3,
            WindowMonths = 12
        });

        dbContext.SaveChanges();
    }

    private static ActivityType CreateActivityType()
        => new()
        {
            CreditRulesJson = """
                {
                  "counts_for": [
                    {
                      "curriculum_item_match": { "epa_field": "epa_id" },
                      "amount": 1,
                      "minimum_level_field": "score"
                    }
                  ]
                }
                """
        };

    private static Activity CreateCompletedActivity(string dataJson, int activityId = 100)
        => new()
        {
            Id = activityId,
            SubjectUserId = "trainee-1",
            CurrentState = "completed",
            DataJson = dataJson,
            Transitions =
            [
                new ActivityTransition
                {
                    TransitionKey = "complete",
                    OccurredOn = DateTime.UtcNow
                }
            ]
        };
}
