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

        var updated = (await applier.ApplyAsync(activity, CreateActivityType(), CancellationToken.None)).UpdatedRows;
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

        var updated = (await applier.ApplyAsync(activity, CreateActivityType(), CancellationToken.None)).UpdatedRows;
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

        var updated = (await applier.ApplyAsync(activity, CreateActivityType(), CancellationToken.None)).UpdatedRows;
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

    [Fact]
    public async Task ApplyAsync_ResolvesStageFromObservationDate_NotToday()
    {
        // Regression (T098): the stage was resolved from DateTime.UtcNow, so replaying an old
        // encounter judged it against the trainee's CURRENT year. Here a year-1 encounter (level 2,
        // meeting the year-1 minimum of 2) is credited for a trainee who is now in year 3 — under
        // the old behaviour it would have been judged against the year-3 minimum of 4 and failed.
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
        var programmeStart = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-800); // trainee is now ~year 3
        dbContext.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = "trainee-1",
            CurriculumId = 3000,
            ProgrammeStartDate = programmeStart,
            ExpectedCompletionDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1),
            IsActive = true
        });
        dbContext.SaveChanges();

        var activity = CreateCompletedActivity("""{ "epa_id": 5000, "score": 2 }""");
        activity.CreatedOn = programmeStart.AddDays(30).ToDateTime(TimeOnly.MinValue);
        // T119: the encounter date is what selects the stage minimum now, not the filing timestamp.
        activity.ObservedOn = programmeStart.AddDays(30); // observed in year 1
        var applier = new CreditApplier(dbContext);

        await applier.ApplyAsync(activity, CreateActivityType(), CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var progress = await dbContext.CurriculumItemProgresses.SingleAsync();
        progress.CountsSoFar.Should().Be(1);
        progress.MinimumLevelReachedCount.Should().Be(1); // year-1 minimum (2) met at the time observed
    }

    [Fact]
    public async Task ApplyAsync_CreditsGraduatedTrainee()
    {
        // Regression (T098): the trainee lookup filtered on IsActive, which TraineeProfile.Complete()
        // clears on graduation. Harmless for live submissions, but RebuildCurriculumProgress deletes
        // every progress row before replaying — so alumni came back with nothing, unrecoverably.
        await using var dbContext = CreateDbContext();
        dbContext.Epas.Add(new Epa { Id = 5000, Code = "EPA-1", Title = "IV access" });
        dbContext.CurriculumItems.Add(new CurriculumItem
        {
            Id = 4000,
            CurriculumId = 3000,
            EpaId = 5000,
            RequiredCount = 30,
            MinimumLevelOrder = 2,
            WindowMonths = 36
        });
        dbContext.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = "trainee-1",
            CurriculumId = 3000,
            ProgrammeStartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1500),
            ExpectedCompletionDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30),
            IsActive = false // graduated
        });
        dbContext.SaveChanges();

        var activity = CreateCompletedActivity("""{ "epa_id": 5000, "score": 3 }""");
        activity.CreatedOn = DateTime.UtcNow.AddDays(-400);
        activity.ObservedOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-400)); // T119
        var applier = new CreditApplier(dbContext);

        var updated = (await applier.ApplyAsync(activity, CreateActivityType(), CancellationToken.None)).UpdatedRows;
        await dbContext.SaveChangesAsync();

        updated.Should().ContainSingle();
        var progress = await dbContext.CurriculumItemProgresses.SingleAsync();
        progress.CountsSoFar.Should().Be(1);
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

        var updated = (await applier.ApplyAsync(activity, CreateActivityType(), CancellationToken.None)).UpdatedRows;
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

    /// <summary>
    /// A completed activity observed TODAY unless a test says otherwise.
    /// </summary>
    /// <remarks>
    /// T119: <c>ObservedOn</c> must be set here, not left to default. Production always arrives through
    /// <c>ActivityService</c>, which stamps it; a fixture that builds the entity directly gets
    /// <c>default(DateOnly)</c> = 0001-01-01, which precedes every <c>ProgrammeStartDate</c>, so
    /// <c>GetStage</c> returns null and the gate quietly falls back to the flat <c>MinimumLevelOrder</c>
    /// instead of the stage minimum. That is a silent wrong answer, not a failure — which is exactly how
    /// it was found.
    /// </remarks>
    private static Activity CreateCompletedActivity(string dataJson, int activityId = 100)
        => new()
        {
            Id = activityId,
            SubjectUserId = "trainee-1",
            CurrentState = "completed",
            DataJson = dataJson,
            ObservedOn = DateOnly.FromDateTime(DateTime.UtcNow),
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
