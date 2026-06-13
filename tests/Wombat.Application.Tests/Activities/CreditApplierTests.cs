using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
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

    [Fact]
    public async Task ApplyAsync_GatesOnStageMinimum_NotFlatTargetLevel()
    {
        // The curriculum item's flat target is level 4, but stage 2 only requires level 3. A year-2
        // trainee completing at level 3 should count toward MinimumLevelReachedCount (it meets the
        // stage minimum the dashboard shows), even though it is below the flat target.
        await using var dbContext = CreateDbContext();
        dbContext.Epas.Add(new Epa { Id = 5000, Code = "EPA-1", Title = "IV access" });
        dbContext.CurriculumItems.Add(new CurriculumItem
        {
            Id = 4000,
            CurriculumId = 3000,
            EpaId = 5000,
            RequiredCount = 30,
            MinimumLevelOrder = 4,
            MinimumLevelByStageJson = """{"1":2,"2":3,"3":4,"4":4}""",
            WindowMonths = 36
        });
        dbContext.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = "trainee-1",
            CurriculumId = 3000,
            ProgrammeStartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-400), // ~1.1y -> stage 2
            ExpectedCompletionDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(2),
            IsActive = true
        });
        dbContext.SaveChanges();

        var activity = CreateCompletedActivity("""{ "epa_id": 5000, "score": 3 }""");
        var applier = new CreditApplier(dbContext);

        await applier.ApplyAsync(activity, CreateActivityType(), CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var progress = await dbContext.CurriculumItemProgresses.SingleAsync();
        progress.CountsSoFar.Should().Be(1);
        progress.MinimumLevelReachedCount.Should().Be(1); // would be 0 if gated on the flat target (4)
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

        // Credit accrues only against the trainee's own (active) profile, which pins the adopted
        // national curriculum version and the trainee's institution (T091 phase 4).
        dbContext.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = "trainee-1",
            InstitutionId = 10,
            CurriculumId = 3000,
            ProgrammeStartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-1),
            ExpectedCompletionDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(2),
            IsActive = true
        });

        dbContext.SaveChanges();
    }

    [Fact]
    public async Task ApplyAsync_ScopesToAdoptedVersionAndOwnInstitutionLocalExtras()
    {
        // The trainee follows curriculum version 3000 at institution 10. Matching by EPA must credit only
        // that version's national core item plus institution 10's local extra — never another institution's
        // local extra, nor an item in a different curriculum version that shares the same EPA. (T091 phase 4.)
        await using var dbContext = CreateDbContext();
        dbContext.Epas.Add(new Epa { Id = 5000, Code = "EPA-1", Title = "Take a history" });

        dbContext.CurriculumItems.AddRange(
            new CurriculumItem { Id = 4000, CurriculumId = 3000, EpaId = 5000, OwningInstitutionId = null, RequiredCount = 3, MinimumLevelOrder = 3, WindowMonths = 12 },
            new CurriculumItem { Id = 4001, CurriculumId = 3000, EpaId = 5000, OwningInstitutionId = 10, RequiredCount = 3, MinimumLevelOrder = 3, WindowMonths = 12 },
            new CurriculumItem { Id = 4002, CurriculumId = 3000, EpaId = 5000, OwningInstitutionId = 99, RequiredCount = 3, MinimumLevelOrder = 3, WindowMonths = 12 },
            new CurriculumItem { Id = 4003, CurriculumId = 3001, EpaId = 5000, OwningInstitutionId = null, RequiredCount = 3, MinimumLevelOrder = 3, WindowMonths = 12 });

        dbContext.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = "trainee-1",
            InstitutionId = 10,
            CurriculumId = 3000,
            ProgrammeStartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-1),
            ExpectedCompletionDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(2),
            IsActive = true
        });
        dbContext.SaveChanges();

        var activity = CreateCompletedActivity("""{ "epa_id": 5000, "score": 4 }""");
        var applier = new CreditApplier(dbContext);

        await applier.ApplyAsync(activity, CreateActivityType(), CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var creditedItemIds = await dbContext.CurriculumItemProgresses
            .Select(progress => progress.CurriculumItemId)
            .ToListAsync();
        creditedItemIds.Should().BeEquivalentTo(new[] { 4000, 4001 });
    }

    [Fact]
    public async Task ApplyAsync_WhenNoActiveTraineeProfile_DoesNothing()
    {
        // Without an active trainee profile there is no adopted curriculum to credit against.
        await using var dbContext = CreateDbContext();
        dbContext.Epas.Add(new Epa { Id = 5000, Code = "EPA-1", Title = "Take a history" });
        dbContext.CurriculumItems.Add(new CurriculumItem { Id = 4000, CurriculumId = 3000, EpaId = 5000, RequiredCount = 3, MinimumLevelOrder = 3, WindowMonths = 12 });
        dbContext.SaveChanges();

        var activity = CreateCompletedActivity("""{ "epa_id": 5000, "score": 4 }""");
        var applier = new CreditApplier(dbContext);

        var updated = await applier.ApplyAsync(activity, CreateActivityType(), CancellationToken.None);
        await dbContext.SaveChangesAsync();

        updated.Should().BeEmpty();
        dbContext.CurriculumItemProgresses.Should().BeEmpty();
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
