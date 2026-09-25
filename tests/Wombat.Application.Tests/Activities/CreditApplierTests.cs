using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Commands.RebuildCurriculumProgress;
using Wombat.Application.Features.Activities.Services;
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

    /// <summary>
    /// T158. Credit follows the picker: an item whose EPA is deactivated is not in force, so a completion while it is
    /// inactive credits nothing, by every way a directive can name its target. Reactivating brings credit back.
    /// </summary>
    [Theory]
    [InlineData("""{ "epa_field": "epa_id" }""")]
    [InlineData("""{ "curriculum_item_field": "item_id" }""")]
    [InlineData("""{ "curriculum_item_id": 4000 }""")]
    public async Task ApplyAsync_WhenTheItemsEpaIsDeactivated_CreditsNothing_AndCreditsOnceItIsReactivated(string match)
    {
        await using var dbContext = CreateDbContext();
        SeedCurriculum(dbContext);
        (await dbContext.Epas.SingleAsync(epa => epa.Id == 5000)).IsActive = false;
        await dbContext.SaveChangesAsync();

        var activityType = new ActivityType
        {
            CreditRulesJson = $$"""{ "counts_for": [ { "curriculum_item_match": {{match}}, "amount": 1 } ] }"""
        };
        const string data = """{ "epa_id": 5000, "item_id": 4000, "score": 4 }""";
        var applier = new CreditApplier(dbContext);

        var whileInactive = await applier.ApplyAsync(CreateCompletedActivity(data, activityId: 100), activityType, CancellationToken.None);
        await dbContext.SaveChangesAsync();

        whileInactive.UpdatedRows.Should().BeEmpty("the EPA is deactivated, so its item takes no new credit");
        dbContext.CurriculumItemProgresses.Should().BeEmpty();

        (await dbContext.Epas.SingleAsync(epa => epa.Id == 5000)).IsActive = true;
        await dbContext.SaveChangesAsync();

        var onceActive = await applier.ApplyAsync(CreateCompletedActivity(data, activityId: 101), activityType, CancellationToken.None);
        await dbContext.SaveChangesAsync();

        onceActive.UpdatedRows.Should().ContainSingle();
        (await dbContext.CurriculumItemProgresses.SingleAsync()).CountsSoFar.Should().Be(1);
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

    /// <param name="programmeStart">
    /// Null keeps the original "a year ago" start. T130 tests pass a FIXED date, because the semester a credit
    /// lands in is asserted literally and must not depend on the day the suite runs.
    /// </param>
    private static void SeedCurriculum(ApplicationDbContext dbContext, DateOnly? programmeStart = null)
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
            ProgrammeStartDate = programmeStart ?? DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-1),
            ExpectedCompletionDate = programmeStart?.AddYears(4) ?? DateOnly.FromDateTime(DateTime.UtcNow).AddYears(2),
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

    // ─── T130: semester buckets ──────────────────────────────────────────────
    //
    // Every date below is fixed. A row is keyed by the semester containing Activity.ObservedOn, so an expectation
    // relative to DateTime.UtcNow would land in one semester on some run dates and in two on others. The expected
    // keys are literals (an absolute oracle), so a bucket written as (0, 0), or computed from the wrong clock, fails
    // here rather than agreeing with itself on both sides of a comparison.

    [Fact]
    public async Task ApplyAsync_CreditsTheSemesterContainingObservedOn_NotTheFilingDateOrTheTransitionTime()
    {
        // The encounter is on the last day of semester 1. The activity was filed and completed the next morning,
        // in semester 2. CreditApplier reads three clocks (ObservedOn for the bucket, the newest transition for the
        // dedupe key, UtcNow for LastUpdated), and only the first may choose the row. The programme start is
        // deliberately not on a boundary: credit must not care where the trainee's own year begins.
        await using var dbContext = CreateDbContext();
        SeedCurriculum(dbContext, programmeStart: new DateOnly(2025, 4, 14));

        var activity = CreateCompletedActivity(
            """{ "epa_id": 5000, "score": 4 }""",
            observedOn: new DateOnly(2026, 6, 30),
            createdOn: new DateTime(2026, 7, 1, 7, 30, 0, DateTimeKind.Utc),
            completedAt: new DateTime(2026, 7, 1, 8, 0, 0, DateTimeKind.Utc));

        var result = await new CreditApplier(dbContext).ApplyAsync(activity, CreateActivityType(), CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var credited = result.UpdatedRows.Should().ContainSingle().Subject;
        (credited.AcademicYear, credited.Semester).Should().Be((2026, 1));

        var rows = await StoredRowsAsync(dbContext);
        var row = rows.Should().ContainSingle().Subject;
        (row.AcademicYear, row.Semester).Should().Be((2026, 1), "30 June is semester 1 (D40); the filing and completion on 1 July are not the encounter");
        row.CountsSoFar.Should().Be(1);
        row.LastObservedOn.Should().Be(new DateOnly(2026, 6, 30));
        rows.Should().NotContain(stored => stored.AcademicYear == 2026 && stored.Semester == 2);
    }

    [Fact]
    public async Task ApplyAsync_TwoCompletionsInOneSemesterEachInAFreshContext_ShareOneRow()
    {
        // Production's shape. Every completion arrives in its own scoped DbContext, so the second one finds the
        // semester's row in the DATABASE, not in Local. Every earlier multi-credit test reused one context, so only
        // the Local half of the lookup was ever exercised. A lookup that missed the stored row would add a second
        // (2026, 1) row: InMemory accepts it, Postgres refuses it with a 23505 inside the completion's save.
        //
        // The later encounter is credited FIRST, so LastObservedOn has to be a maximum, not the last date written,
        // while LastActivityId is the completion that most recently moved the row.
        var options = NewDatabase();

        await using (var seed = new ApplicationDbContext(options))
        {
            SeedCurriculum(seed, programmeStart: new DateOnly(2025, 4, 14));
        }

        await using (var first = new ApplicationDbContext(options))
        {
            await new CreditApplier(first).ApplyAsync(
                CompletedActivityObservedOn(new DateOnly(2026, 5, 20), activityId: 101), CreateActivityType(), CancellationToken.None);
            await first.SaveChangesAsync();
        }

        await using (var second = new ApplicationDbContext(options))
        {
            var result = await new CreditApplier(second).ApplyAsync(
                CompletedActivityObservedOn(new DateOnly(2026, 3, 10), activityId: 100), CreateActivityType(), CancellationToken.None);
            await second.SaveChangesAsync();

            result.UpdatedRows.Should().ContainSingle();
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            var row = (await StoredRowsAsync(verify)).Should().ContainSingle().Subject;
            (row.AcademicYear, row.Semester).Should().Be((2026, 1));
            row.CountsSoFar.Should().Be(2);
            row.MinimumLevelReachedCount.Should().Be(2);
            row.LastObservedOn.Should().Be(new DateOnly(2026, 5, 20), "the later encounter, although it was credited first");
            row.LastActivityId.Should().Be(100);
            row.CreditedActivityKeysJson.Should().Be("""["100:complete","101:complete"]""");
        }
    }

    [Fact]
    public async Task ApplyAsync_InOneContextAfterASave_FindsTheRowOnceAlthoughItIsBothTrackedAndStored()
    {
        // The other half of the lookup. After a save the row is tracked (Local) AND returned by the database query,
        // and identity resolution makes those the same instance. A plain concatenation of the two would hand Apply
        // the row twice; a single-match lookup would then throw, and a first-match one could still double-count.
        await using var dbContext = CreateDbContext();
        SeedCurriculum(dbContext, programmeStart: new DateOnly(2025, 4, 14));
        var applier = new CreditApplier(dbContext);

        await applier.ApplyAsync(CompletedActivityObservedOn(new DateOnly(2026, 3, 10), activityId: 100), CreateActivityType(), CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var second = CompletedActivityObservedOn(new DateOnly(2026, 4, 2), activityId: 101);
        var plan = await applier.PlanAsync(CreditSubject.Of(second), CreateActivityType(), CancellationToken.None);
        plan.ExistingRows.Should().ContainSingle("the tracked row and the stored row are one entity, not two");

        var result = await applier.ApplyAsync(second, CreateActivityType(), CancellationToken.None);
        result.UpdatedRows.Should().ContainSingle();

        dbContext.CurriculumItemProgresses.Local
            .GroupBy(CurriculumItemProgressKey.Of)
            .Should().OnlyContain(group => group.Count() == 1, "one tracked row per (item, trainee, year, semester) before the save");

        await dbContext.SaveChangesAsync();

        var row = (await StoredRowsAsync(dbContext)).Should().ContainSingle().Subject;
        (row.AcademicYear, row.Semester).Should().Be((2026, 1));
        row.CountsSoFar.Should().Be(2);
        row.LastActivityId.Should().Be(101);
        row.LastObservedOn.Should().Be(new DateOnly(2026, 4, 2));
    }

    [Fact]
    public async Task ApplyAsync_CompletionsInTwoSemesters_WriteTwoRowsEachWithItsOwnTally()
    {
        // Two semesters of one item, credited in ONE context with ONE save at the end: the rebuild's shape, where
        // the semester-1 row an earlier completion added is still only in Local when the next one needs it. The
        // completion order interleaves the buckets (S1, S2, S1), so a LastActivityId that leaked across buckets
        // would show on the semester-2 row. The two semester-1 completions differ in level, so the minimum count is
        // per bucket too.
        await using var dbContext = CreateDbContext();
        SeedCurriculum(dbContext, programmeStart: new DateOnly(2025, 4, 14));
        var applier = new CreditApplier(dbContext);

        await applier.ApplyAsync(CompletedActivityObservedOn(new DateOnly(2026, 2, 11), activityId: 100, score: 2), CreateActivityType(), CancellationToken.None);
        await applier.ApplyAsync(CompletedActivityObservedOn(new DateOnly(2026, 7, 1), activityId: 101, score: 4), CreateActivityType(), CancellationToken.None);
        await applier.ApplyAsync(CompletedActivityObservedOn(new DateOnly(2026, 6, 30), activityId: 102, score: 4), CreateActivityType(), CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var rows = await StoredRowsAsync(dbContext);
        rows.Should().HaveCount(2);

        var semester1 = rows[0];
        (semester1.CurriculumItemId, semester1.TraineeUserId, semester1.AcademicYear, semester1.Semester)
            .Should().Be((4000, "trainee-1", 2026, 1));
        semester1.CountsSoFar.Should().Be(2);
        semester1.MinimumLevelReachedCount.Should().Be(1, "activity 100 was below the minimum of 3");
        semester1.LastActivityId.Should().Be(102);
        semester1.LastObservedOn.Should().Be(new DateOnly(2026, 6, 30));
        semester1.CreditedActivityKeysJson.Should().Be("""["100:complete","102:complete"]""");

        var semester2 = rows[1];
        (semester2.CurriculumItemId, semester2.TraineeUserId, semester2.AcademicYear, semester2.Semester)
            .Should().Be((4000, "trainee-1", 2026, 2));
        semester2.CountsSoFar.Should().Be(1);
        semester2.MinimumLevelReachedCount.Should().Be(1);
        semester2.LastActivityId.Should().Be(101, "activity 102 was credited later, but into semester 1");
        semester2.LastObservedOn.Should().Be(new DateOnly(2026, 7, 1));
        semester2.CreditedActivityKeysJson.Should().Be("""["101:complete"]""");
    }

    // ─── T219: whether the last encounter's date was stated ─────────────────────
    //
    // A reader shows LastObservedOn as the last encounter, and an undated one must say it is only the day a form was
    // created. The row keeps no link to the completion the date came from, so it records the flag beside the date.

    [Fact]
    public async Task ApplyAsync_RecordsWhetherTheLastEncounterDateWasStated_AndOnlyALaterEncounterReplacesIt()
    {
        await using var dbContext = CreateDbContext();
        SeedCurriculum(dbContext, programmeStart: new DateOnly(2025, 4, 14));
        var applier = new CreditApplier(dbContext);

        async Task<(DateOnly? LastObservedOn, bool Declared)> CreditAsync(DateOnly observedOn, int activityId, ObservationDateSource source)
        {
            await applier.ApplyAsync(CompletedActivityObservedOn(observedOn, activityId, source: source), CreateActivityType(), CancellationToken.None);
            await dbContext.SaveChangesAsync();
            var row = (await StoredRowsAsync(dbContext)).Should().ContainSingle("every encounter here is in semester 2 of 2026").Subject;
            return (row.LastObservedOn, row.LastObservedOnDeclared);
        }

        (await CreditAsync(new DateOnly(2026, 8, 4), 100, ObservationDateSource.Declared))
            .Should().Be(((DateOnly?)new DateOnly(2026, 8, 4), true));

        (await CreditAsync(new DateOnly(2026, 8, 20), 101, ObservationDateSource.CreatedOn))
            .Should().Be(((DateOnly?)new DateOnly(2026, 8, 20), false), "the latest date is only the day a form was created");

        (await CreditAsync(new DateOnly(2026, 8, 10), 102, ObservationDateSource.Declared))
            .Should().Be(((DateOnly?)new DateOnly(2026, 8, 20), false), "an earlier encounter, although stated, is not the last one");

        (await CreditAsync(new DateOnly(2026, 9, 1), 103, ObservationDateSource.Declared))
            .Should().Be(((DateOnly?)new DateOnly(2026, 9, 1), true));

        (await CreditAsync(new DateOnly(2026, 9, 5), 104, ObservationDateSource.CreatedOn))
            .Should().Be(((DateOnly?)new DateOnly(2026, 9, 5), false));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ApplyAsync_TwoEncountersOnTheLastDate_OneStated_IsStated_WhicheverIsCreditedFirst(bool statedFirst)
    {
        // An encounter is known to have happened on that date, so the date is stated. The answer must not depend on the
        // order the two were credited in, or a rebuild (which replays in filing order) could disagree with the live
        // path. Each completion arrives in its own context, as in production.
        var options = NewDatabase();

        await using (var seed = new ApplicationDbContext(options))
        {
            SeedCurriculum(seed, programmeStart: new DateOnly(2025, 4, 14));
        }

        var sameDay = new DateOnly(2026, 8, 20);
        var order = statedFirst
            ? new[] { (Id: 100, Source: ObservationDateSource.Declared), (Id: 101, Source: ObservationDateSource.CreatedOn) }
            : new[] { (Id: 100, Source: ObservationDateSource.CreatedOn), (Id: 101, Source: ObservationDateSource.Declared) };

        foreach (var (id, source) in order)
        {
            await using var db = new ApplicationDbContext(options);
            await new CreditApplier(db).ApplyAsync(CompletedActivityObservedOn(sameDay, id, source: source), CreateActivityType(), CancellationToken.None);
            await db.SaveChangesAsync();
        }

        await using var verify = new ApplicationDbContext(options);
        var row = (await StoredRowsAsync(verify)).Should().ContainSingle().Subject;
        row.CountsSoFar.Should().Be(2, "guard: both were credited");
        (row.LastObservedOn, row.LastObservedOnDeclared).Should().Be(((DateOnly?)sameDay, true));
    }

    [Fact]
    public async Task PlanAsync_CarriesWhetherTheSubjectsDateWasStated()
    {
        // The live path plans BEFORE the transition stamps the activity, so the plan takes the flag from the subject it
        // is given, not from the entity; a replay's subject reads it off the entity's ObservedOnSource.
        await using var dbContext = CreateDbContext();
        SeedCurriculum(dbContext, programmeStart: new DateOnly(2025, 4, 14));
        var applier = new CreditApplier(dbContext);

        var undated = CompletedActivityObservedOn(new DateOnly(2026, 8, 20), source: ObservationDateSource.CreatedOn);
        CreditSubject.Of(undated).ObservedOnDeclared.Should().BeFalse();
        (await applier.PlanAsync(CreditSubject.Of(undated), CreateActivityType(), CancellationToken.None))
            .ObservedOnDeclared.Should().BeFalse();

        var dated = CompletedActivityObservedOn(new DateOnly(2026, 8, 20));
        CreditSubject.Of(dated).ObservedOnDeclared.Should().BeTrue();
        (await applier.PlanAsync(CreditSubject.Of(dated), CreateActivityType(), CancellationToken.None))
            .ObservedOnDeclared.Should().BeTrue();
    }

    [Fact]
    public async Task ApplyAsync_CreditsAnEncounterBeforeTheProgrammeStartAndOneInAnExemptPeriod()
    {
        // D14 is applied when progress is READ, never when credit is written (D41). The trainee started on
        // 20 September 2026, so an encounter on 10 March precedes the programme and one on 5 October falls in a
        // period D14 exempts. Both happened, so both are credited into their own semester. Suppressing either would
        // make UpdatedRows empty, ActivityService would stamp CreditedItemCount = 0, and T108's "this credited
        // nothing" banner would blame the curriculum for a perfectly good encounter.
        var programmeStart = new DateOnly(2026, 9, 20);
        var beforeStart = new DateOnly(2026, 3, 10);
        var inExemptPeriod = new DateOnly(2026, 10, 5);

        // Guards: the fixture really is in the states the test is named for, judged by the one implementation of D14.
        QuotaWindow.For(QuotaPeriod.AcademicYear, beforeStart, programmeStart).Status.Should().Be(QuotaWindowStatus.NotStarted);
        QuotaWindow.For(QuotaPeriod.AcademicYear, inExemptPeriod, programmeStart).Status.Should().Be(QuotaWindowStatus.ExemptPartialPeriod);
        QuotaWindow.For(QuotaPeriod.Semester, inExemptPeriod, programmeStart).Status.Should().Be(QuotaWindowStatus.ExemptPartialPeriod);

        await using var dbContext = CreateDbContext();
        SeedCurriculum(dbContext, programmeStart: programmeStart);
        var applier = new CreditApplier(dbContext);

        var preStart = await applier.ApplyAsync(CompletedActivityObservedOn(beforeStart, activityId: 100), CreateActivityType(), CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var exempt = await applier.ApplyAsync(CompletedActivityObservedOn(inExemptPeriod, activityId: 101), CreateActivityType(), CancellationToken.None);
        await dbContext.SaveChangesAsync();

        preStart.UpdatedRows.Should().ContainSingle("the transition would be stamped CreditedItemCount = 1, so no banner");
        exempt.UpdatedRows.Should().ContainSingle();

        var rows = await StoredRowsAsync(dbContext);
        rows.Select(row => (row.AcademicYear, row.Semester, row.CountsSoFar, row.LastActivityId))
            .Should().Equal((2026, 1, 1, (int?)100), (2026, 2, 1, (int?)101));
    }

    [Fact]
    public async Task ApplyAsync_AfterTheEncounterIsReDated_DoesNotCountItTwice_AndARebuildMovesIt()
    {
        // The cross-bucket dedupe, pinned as documented behaviour. Activity 100 was credited into semester 1
        // (30 June). Its encounter date then moved to 1 August, and credit ran again through the same transition
        // key. The key is already in the semester-1 row, so nothing is added anywhere: the activity is not counted
        // in two buckets, and it stays in the OLD one. Only a rebuild recomputes the bucket from ObservedOn, which
        // moves the credit and removes the row it no longer reproduces. Every step runs in a fresh context, as the
        // live path does.
        var options = NewDatabase();

        await using (var seed = new ApplicationDbContext(options))
        {
            SeedCurriculum(seed, programmeStart: new DateOnly(2025, 4, 14));
            SeedCreditingActivityType(seed);
            AddStoredCompletedActivity(seed, activityId: 100, observedOn: new DateOnly(2026, 6, 30),
                completedAt: new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc));
            await seed.SaveChangesAsync();
        }

        await using (var live = new ApplicationDbContext(options))
        {
            var activity = await live.Activities.Include(entity => entity.Transitions).SingleAsync(entity => entity.Id == 100);
            (await new CreditApplier(live).ApplyAsync(activity, CreateActivityType(), CancellationToken.None))
                .UpdatedRows.Should().ContainSingle();
            await live.SaveChangesAsync();
        }

        await using (var reDated = new ApplicationDbContext(options))
        {
            var activity = await reDated.Activities.Include(entity => entity.Transitions).SingleAsync(entity => entity.Id == 100);
            activity.ObservedOn = new DateOnly(2026, 8, 1);

            var result = await new CreditApplier(reDated).ApplyAsync(activity, CreateActivityType(), CancellationToken.None);
            await reDated.SaveChangesAsync();

            result.UpdatedRows.Should().BeEmpty("100:complete is already counted, in semester 1");
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            var rows = await StoredRowsAsync(verify);
            rows.Sum(row => row.CountsSoFar).Should().Be(1, "one activity, counted once across every bucket");

            var row = rows.Should().ContainSingle("a re-apply that dedupes must create no empty row").Subject;
            (row.AcademicYear, row.Semester).Should().Be((2026, 1), "the credit stays in the old bucket until a rebuild");
            row.LastObservedOn.Should().Be(new DateOnly(2026, 6, 30));
        }

        await using (var rebuild = new ApplicationDbContext(options))
        {
            var result = await new RebuildCurriculumProgressCommandHandler(rebuild, new CreditApplier(rebuild))
                .Handle(new RebuildCurriculumProgressCommand(Administrator()), CancellationToken.None);

            result.ActivitiesReplayed.Should().Be(1);
            result.ProgressRowsWritten.Should().Be(1);
            result.ProgressRowsRemoved.Should().Be(1, "the semester-1 row is no longer reproduced");
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            var row = (await StoredRowsAsync(verify)).Should().ContainSingle().Subject;
            (row.AcademicYear, row.Semester).Should().Be((2026, 2));
            row.CountsSoFar.Should().Be(1);
            row.LastActivityId.Should().Be(100);
            row.LastObservedOn.Should().Be(new DateOnly(2026, 8, 1));
            row.CreditedActivityKeysJson.Should().Be("""["100:complete"]""");
        }
    }

    [Fact]
    public async Task PlanAsync_MutatesNothing_AndCarriesTheTraineesRowsOfEverySemesterOfTheItem()
    {
        // The split that closes the audit trap. The audit pipeline's catch saves the request's DbContext, so every
        // read (and every await) has to finish before the first mutation. PlanAsync must therefore leave the change
        // tracker exactly as it found it, even with one of the trainee's rows already tracked. And its rows must
        // span EVERY semester of the candidate item, not only the one the encounter falls in: the dedupe looks
        // across buckets, and here the encounter's own bucket (2026, 2) has no row yet.
        var options = NewDatabase();

        await using (var seed = new ApplicationDbContext(options))
        {
            SeedCurriculum(seed, programmeStart: new DateOnly(2025, 4, 14));
            seed.Epas.Add(new Epa { Id = 5009, Code = "EPA-9", Title = "Not this EPA" });
            seed.CurriculumItems.Add(new CurriculumItem { Id = 4009, CurriculumId = 3000, EpaId = 5009, RequiredCount = 1, MinimumLevelOrder = 3, WindowMonths = 12 });
            seed.CurriculumItemProgresses.AddRange(
                StoredRow(2025, 2, 1, """["90:complete"]"""),
                StoredRow(2026, 1, 2, """["91:complete","92:complete"]"""),
                StoredRow(2026, 2, 1, """["93:complete"]""", traineeUserId: "trainee-2"),
                StoredRow(2026, 2, 1, """["94:complete"]""", curriculumItemId: 4009));
            await seed.SaveChangesAsync();
        }

        await using var dbContext = new ApplicationDbContext(options);
        var tracked = await dbContext.CurriculumItemProgresses.SingleAsync(row =>
            row.TraineeUserId == "trainee-1" && row.CurriculumItemId == 4000 && row.AcademicYear == 2026 && row.Semester == 1);

        var plan = await new CreditApplier(dbContext).PlanAsync(
            new CreditSubject("trainee-1", new DateOnly(2026, 8, 3), ObservedOnDeclared: true, """{ "epa_id": 5000, "score": 4 }"""),
            CreateActivityType(),
            CancellationToken.None);

        dbContext.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Should().BeEmpty("a plan that failed half-way must leave nothing for the audit pipeline's save to commit");
        (await dbContext.SaveChangesAsync()).Should().Be(0);

        plan.TraineeUserId.Should().Be("trainee-1");
        plan.ObservedOn.Should().Be(new DateOnly(2026, 8, 3));
        plan.Credits.Should().ContainSingle().Which.CurriculumItemId.Should().Be(4000);

        plan.ExistingRows.Select(row => (row.CurriculumItemId, row.TraineeUserId, row.AcademicYear, row.Semester))
            .Should().BeEquivalentTo(new[] { (4000, "trainee-1", 2025, 2), (4000, "trainee-1", 2026, 1) },
                "every semester of the candidate item, and neither another trainee's row nor another item's");
        plan.ExistingRows.Should().Contain(row => ReferenceEquals(row, tracked),
            "the rows are the TRACKED instances Apply will increment, not detached copies");
    }

    [Fact]
    public async Task PlanAsync_LeavesOutARowAlreadyMarkedForDeletion()
    {
        // A row marked Deleted is on its way out: an increment written onto it would be thrown away at save, so the
        // plan must not offer it to Apply as the semester's row.
        var options = NewDatabase();

        await using (var seed = new ApplicationDbContext(options))
        {
            SeedCurriculum(seed, programmeStart: new DateOnly(2025, 4, 14));
            seed.CurriculumItemProgresses.AddRange(
                StoredRow(2026, 1, 1, """["90:complete"]"""),
                StoredRow(2026, 2, 1, """["91:complete"]"""));
            await seed.SaveChangesAsync();
        }

        await using var dbContext = new ApplicationDbContext(options);
        var doomed = await dbContext.CurriculumItemProgresses.SingleAsync(row => row.AcademicYear == 2026 && row.Semester == 1);
        dbContext.CurriculumItemProgresses.Remove(doomed);

        var plan = await new CreditApplier(dbContext).PlanAsync(
            CreditSubject.Of(CompletedActivityObservedOn(new DateOnly(2026, 3, 10))), CreateActivityType(), CancellationToken.None);

        plan.ExistingRows.Select(row => (row.AcademicYear, row.Semester)).Should().Equal((2026, 2));
    }

    [Fact]
    public async Task Apply_WithAPlanThatCreditsNothing_ReturnsEmptyAndTouchesNothing()
    {
        // Every "nothing to credit" route (no counts_for, no trainee profile, no matching item) ends in a plan with
        // no credits, and Apply must return the shared Empty result without adding a row.
        await using var dbContext = CreateDbContext();
        SeedCurriculum(dbContext, programmeStart: new DateOnly(2025, 4, 14));
        var applier = new CreditApplier(dbContext);
        var activity = CompletedActivityObservedOn(new DateOnly(2026, 3, 10));

        applier.Apply(CreditPlan.Nothing, activity).Should().BeSameAs(CreditApplicationResult.Empty);

        var creditsNothing = new ActivityType { CreditRulesJson = """{ "counts_for": [] }""" };
        (await applier.PlanAsync(CreditSubject.Of(activity), creditsNothing, CancellationToken.None))
            .Should().BeSameAs(CreditPlan.Nothing);

        var unmatched = await applier.PlanAsync(
            new CreditSubject("trainee-1", new DateOnly(2026, 3, 10), ObservedOnDeclared: true, """{ "epa_id": 9999, "score": 4 }"""),
            CreateActivityType(),
            CancellationToken.None);
        unmatched.Credits.Should().BeEmpty();
        applier.Apply(unmatched, activity).Should().BeSameAs(CreditApplicationResult.Empty);

        dbContext.ChangeTracker.Entries()
            .Where(entry => entry.State != EntityState.Unchanged)
            .Should().BeEmpty();
        (await dbContext.SaveChangesAsync()).Should().Be(0);
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
    private static Activity CreateCompletedActivity(
        string dataJson,
        int activityId = 100,
        DateOnly? observedOn = null,
        DateTime? createdOn = null,
        DateTime? completedAt = null,
        ObservationDateSource source = ObservationDateSource.Declared)
        => new()
        {
            Id = activityId,
            SubjectUserId = "trainee-1",
            CurrentState = "completed",
            DataJson = dataJson,
            ObservedOn = observedOn ?? DateOnly.FromDateTime(DateTime.UtcNow),
            ObservedOnSource = source,
            CreatedOn = createdOn ?? default,
            Transitions =
            [
                new ActivityTransition
                {
                    TransitionKey = "complete",
                    OccurredOn = completedAt ?? DateTime.UtcNow
                }
            ]
        };

    /// <summary>
    /// A completion whose encounter, filing and completion all happen on one FIXED day (T130). Every clock the
    /// applier could read is pinned, so a period assertion cannot pass or fail by the day the suite runs.
    /// </summary>
    /// <remarks>
    /// With <paramref name="source" /> <c>CreatedOn</c> it is an undated completion (T219): its <c>ObservedOn</c> is the
    /// day its form was created, which is the day this fixture creates it on.
    /// </remarks>
    private static Activity CompletedActivityObservedOn(
        DateOnly observedOn, int activityId = 100, int score = 4, ObservationDateSource source = ObservationDateSource.Declared)
        => CreateCompletedActivity(
            $$"""{ "epa_id": 5000, "score": {{score}} }""",
            activityId,
            observedOn,
            createdOn: observedOn.ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc),
            completedAt: observedOn.ToDateTime(new TimeOnly(10, 0), DateTimeKind.Utc),
            source: source);

    private static DbContextOptions<ApplicationDbContext> NewDatabase()
        => new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    /// <summary>
    /// Every stored progress row, after checking there is one per (item, trainee, year, semester). The in-memory
    /// provider enforces no unique index, so a duplicate bucket that Postgres would refuse with a 23505 is only
    /// caught if the test counts it.
    /// </summary>
    private static async Task<List<CurriculumItemProgress>> StoredRowsAsync(ApplicationDbContext dbContext)
    {
        var rows = await dbContext.CurriculumItemProgresses.AsNoTracking().ToListAsync();

        rows.GroupBy(CurriculumItemProgressKey.Of)
            .Should().OnlyContain(group => group.Count() == 1, "one row per (item, trainee, year, semester)");

        return rows
            .OrderBy(row => row.CurriculumItemId)
            .ThenBy(row => row.TraineeUserId, StringComparer.Ordinal)
            .ThenBy(row => row.AcademicYear)
            .ThenBy(row => row.Semester)
            .ToList();
    }

    private static CurriculumItemProgress StoredRow(int academicYear, int semester, int countsSoFar, string creditedKeysJson, string traineeUserId = "trainee-1", int curriculumItemId = 4000)
        => new()
        {
            CurriculumItemId = curriculumItemId,
            TraineeUserId = traineeUserId,
            AcademicYear = academicYear,
            Semester = semester,
            CountsSoFar = countsSoFar,
            MinimumLevelReachedCount = countsSoFar,
            LastUpdated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CreditedActivityKeysJson = creditedKeysJson
        };

    private const int CreditingTypeId = 700;

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

    /// <summary>
    /// A published, crediting activity type with a pinned version 1, carrying the SAME credit rules as
    /// <see cref="CreateActivityType" />. The rebuild replays from the pinned version, so a test that credits
    /// incrementally and then rebuilds needs the two to agree.
    /// </summary>
    private static void SeedCreditingActivityType(ApplicationDbContext dbContext)
    {
        var publishedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var creditRulesJson = CreateActivityType().CreditRulesJson!;

        var activityType = new ActivityType
        {
            Id = CreditingTypeId,
            Key = "wba_under_test",
            Name = "WBA under test",
            Scope = ActivityScope.Institution,
            ScopeId = 10,
            Version = 1,
            SchemaJson = SchemaJson,
            WorkflowJson = WorkflowJson,
            CreditRulesJson = creditRulesJson,
            DisplayFieldsJson = """["epa_id","score"]""",
            OwnerUserId = "admin-1",
            CreatedOn = publishedOn
        };

        activityType.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = CreditingTypeId,
            Version = 1,
            SchemaJson = SchemaJson,
            WorkflowJson = WorkflowJson,
            CreditRulesJson = creditRulesJson,
            DisplayFieldsJson = activityType.DisplayFieldsJson,
            PublishedByUserId = "admin-1",
            PublishedOn = publishedOn
        });

        dbContext.ActivityTypes.Add(activityType);
    }

    /// <summary>A completed activity persisted with its pinned type, so the rebuild can find and replay it.</summary>
    private static void AddStoredCompletedActivity(ApplicationDbContext dbContext, int activityId, DateOnly observedOn, DateTime completedAt)
    {
        var activity = new Activity
        {
            Id = activityId,
            ActivityTypeId = CreditingTypeId,
            SchemaVersion = 1,
            SubjectUserId = "trainee-1",
            CreatedByUserId = "trainee-1",
            CurrentState = "completed",
            DataJson = """{ "epa_id": 5000, "score": 4 }""",
            CreatedOn = completedAt.AddHours(-1),
            UpdatedOn = completedAt,
            ObservedOn = observedOn
        };

        activity.Transitions.Add(new ActivityTransition
        {
            ActivityId = activityId,
            FromState = "submitted",
            ToState = "completed",
            TransitionKey = "complete",
            ActorUserId = "trainee-1",
            OccurredOn = completedAt
        });

        dbContext.Activities.Add(activity);
    }

    private static ClaimsPrincipal Administrator()
        => new(new ClaimsIdentity([new Claim(ClaimTypes.Role, WombatRoles.Administrator)], "test"));
}
