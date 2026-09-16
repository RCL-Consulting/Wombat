using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Domain.Activities;
using Wombat.Domain.Epas;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Activities;

public sealed class GetEpaTrajectoryForTraineeTests
{
    [Fact]
    public async Task ReturnsPointsOrderedByDate()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 2, 10, 9, 0, 0, DateTimeKind.Utc));
        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-b", 7, 4, new DateTime(2026, 1, 5, 9, 0, 0, DateTimeKind.Utc));
        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 5, new DateTime(2026, 3, 1, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(new GetEpaTrajectoryForTraineeQuery("trainee-1"), CancellationToken.None);

        var trajectory = result.Should().ContainSingle().Subject;
        trajectory.EpaId.Should().Be(7);
        trajectory.Points.Select(p => p.Rating).Should().Equal(4, 3, 5);
        trajectory.Points.Select(p => p.ObservedOn).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GroupsByEpaAndSortsByEpaCode()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        dbContext.Epas.Add(new Epa { Id = 3, SubSpecialityId = 1, Code = "EPA-03", Title = "Ward round", IsActive = true });
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc));
        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-b", 3, 4, new DateTime(2026, 2, 5, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(new GetEpaTrajectoryForTraineeQuery("trainee-1"), CancellationToken.None);

        result.Select(dto => dto.EpaCode).Should().Equal("EPA-03", "EPA-07");
    }

    [Fact]
    public async Task IgnoresUnratedActivityTypes()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var reflectiveNote = await SeedActivityTypeAsync(dbContext, "reflective_note");

        AddRatedActivity(dbContext, reflectiveNote, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 2, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(new GetEpaTrajectoryForTraineeQuery("trainee-1"), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task IgnoresActivitiesWithoutOverallRating()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        // No overall field -> skipped
        dbContext.Activities.Add(new Activity
        {
            ActivityTypeId = miniCex.Id,
            ActivityType = miniCex,
            SchemaVersion = miniCex.Version,
            SubjectUserId = "trainee-1",
            CreatedByUserId = "assessor-a",
            CurrentState = "requested",
            DataJson = "{\"epa_id\": 7, \"assessor_user_id\": \"assessor-a\"}",
            CreatedOn = new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc),
            UpdatedOn = new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc)
        });
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(new GetEpaTrajectoryForTraineeQuery("trainee-1"), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AppliesDateRangeFilter()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 2, new DateTime(2025, 12, 1, 9, 0, 0, DateTimeKind.Utc));
        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-b", 7, 3, new DateTime(2026, 2, 5, 9, 0, 0, DateTimeKind.Utc));
        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-c", 7, 4, new DateTime(2026, 5, 5, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(
            new GetEpaTrajectoryForTraineeQuery(
                "trainee-1",
                From: new DateOnly(2026, 1, 1),
                To: new DateOnly(2026, 3, 31)),
            CancellationToken.None);

        var trajectory = result.Should().ContainSingle().Subject;
        trajectory.Points.Should().ContainSingle();
        trajectory.Points[0].Rating.Should().Be(3);
    }

    [Fact]
    public async Task ScopesByTrainee()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc));
        AddRatedActivity(dbContext, miniCex, "trainee-2", "assessor-a", 7, 5, new DateTime(2026, 2, 5, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(new GetEpaTrajectoryForTraineeQuery("trainee-1"), CancellationToken.None);

        var trajectory = result.Should().ContainSingle().Subject;
        trajectory.Points.Should().ContainSingle();
        trajectory.Points[0].Rating.Should().Be(3);
    }

    [Fact]
    public async Task MapsSourceByActivityKey()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var dops = await SeedActivityTypeAsync(dbContext, "dops");
        var cbd = await SeedActivityTypeAsync(dbContext, "cbd");

        AddRatedActivity(dbContext, dops, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc));
        AddRatedActivity(dbContext, cbd, "trainee-1", "assessor-b", 7, 4, new DateTime(2026, 2, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(new GetEpaTrajectoryForTraineeQuery("trainee-1"), CancellationToken.None);

        var trajectory = result.Should().ContainSingle().Subject;
        trajectory.Points.Select(p => p.Source).Should().Equal("Direct observation", "Conversation");
    }

    [Fact]
    public async Task MapsSchemaDrivenKeyVariant()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        // Schema-driven types built via the visual builder carry institution-specific
        // key suffixes (e.g. "mini_cex_paed"); they must still map to the assessment family.
        var miniCexPaed = await SeedActivityTypeAsync(dbContext, "mini_cex_paed");

        AddRatedActivity(dbContext, miniCexPaed, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(new GetEpaTrajectoryForTraineeQuery("trainee-1"), CancellationToken.None);

        var trajectory = result.Should().ContainSingle().Subject;
        trajectory.Points.Should().ContainSingle();
        trajectory.Points[0].Source.Should().Be("Direct observation");
    }

    [Fact]
    public async Task ReadsOverallLevelFieldWhenOverallAbsent()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var miniCexPaed = await SeedActivityTypeAsync(dbContext, "mini_cex_paed");

        // Visual-builder schema stores the overall rating as "overall_level", not "overall".
        dbContext.Activities.Add(new Activity
        {
            ActivityTypeId = miniCexPaed.Id,
            ActivityType = miniCexPaed,
            SchemaVersion = miniCexPaed.Version,
            SubjectUserId = "trainee-1",
            CreatedByUserId = "assessor-a",
            CurrentState = "completed",
            DataJson = "{\"epa_id\": \"7\", \"assessor_user_id\": \"assessor-a\", \"overall_level\": \"4\"}",
            CreatedOn = new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc),
            UpdatedOn = new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc)
        });
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(new GetEpaTrajectoryForTraineeQuery("trainee-1"), CancellationToken.None);

        var trajectory = result.Should().ContainSingle().Subject;
        trajectory.Points.Should().ContainSingle();
        trajectory.Points[0].Rating.Should().Be(4);
    }

    [Fact]
    public async Task IncludesRatingsAboveFive()
    {
        // Regression (T098): the rating guard was hard-coded to 1..5, so an observation recorded
        // on a scale with more rungs was dropped silently — no error, no log, no gap in the chart.
        // The paediatric v11.1 ladder has six rungs (1, 2, 3a, 3b, 4, 5), so rung 6 must survive.
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 6, new DateTime(2026, 2, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(new GetEpaTrajectoryForTraineeQuery("trainee-1"), CancellationToken.None);

        var trajectory = result.Should().ContainSingle().Subject;
        trajectory.Points.Should().ContainSingle();
        trajectory.Points[0].Rating.Should().Be(6);
    }

    [Fact]
    public async Task IgnoresImplausibleRatings()
    {
        // The 1..5 clamp was removed, but a value that cannot be a rung at all (a mis-mapped
        // percentage, a year) must still be rejected rather than drawn.
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 2026, new DateTime(2026, 2, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(new GetEpaTrajectoryForTraineeQuery("trainee-1"), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task MapsVersionElevenAssessmentFamilies()
    {
        // T098: v11.1 names CCA, RCA, chart-stimulated recall and case note review as WBA tools.
        // They must chart as assessment evidence once those activity types exist.
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var cca = await SeedActivityTypeAsync(dbContext, "cca");
        var rca = await SeedActivityTypeAsync(dbContext, "rca");
        var csr = await SeedActivityTypeAsync(dbContext, "chart_stimulated_recall");

        AddRatedActivity(dbContext, cca, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc));
        AddRatedActivity(dbContext, rca, "trainee-1", "assessor-b", 7, 4, new DateTime(2026, 2, 5, 9, 0, 0, DateTimeKind.Utc));
        AddRatedActivity(dbContext, csr, "trainee-1", "assessor-c", 7, 5, new DateTime(2026, 2, 9, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(new GetEpaTrajectoryForTraineeQuery("trainee-1"), CancellationToken.None);

        var trajectory = result.Should().ContainSingle().Subject;
        trajectory.Points.Select(p => p.Source).Should().Equal("Case analysis", "Case analysis", "Conversation");
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task SeedCoreAsync(ApplicationDbContext dbContext)
    {
        dbContext.Epas.Add(new Epa { Id = 7, SubSpecialityId = 1, Code = "EPA-07", Title = "Emergency triage", IsActive = true });
        await dbContext.SaveChangesAsync();
    }

    private static async Task<ActivityType> SeedActivityTypeAsync(ApplicationDbContext dbContext, string key)
    {
        var activityType = new ActivityType
        {
            Key = key,
            Name = key,
            Version = 1,
            IsActive = true,
            OwnerUserId = "admin-1",
            CreatedOn = DateTime.UtcNow
        };
        dbContext.ActivityTypes.Add(activityType);
        await dbContext.SaveChangesAsync();
        return activityType;
    }

    private static void AddRatedActivity(
        ApplicationDbContext dbContext,
        ActivityType activityType,
        string subject,
        string assessor,
        int epaId,
        int overall,
        DateTime createdOn)
    {
        var dataJson = $"{{\"epa_id\": {epaId}, \"assessor_user_id\": \"{assessor}\", \"overall\": \"{overall}\"}}";
        dbContext.Activities.Add(new Activity
        {
            ActivityTypeId = activityType.Id,
            ActivityType = activityType,
            SchemaVersion = activityType.Version,
            SubjectUserId = subject,
            CreatedByUserId = assessor,
            CurrentState = "completed",
            DataJson = dataJson,
            CreatedOn = createdOn,
            UpdatedOn = createdOn
        });
    }
}
