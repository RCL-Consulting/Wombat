using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.Activities;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Epas;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.CommitteeDecisions;

public sealed class SamplingConcentrationWarningsTests
{
    [Fact]
    public async Task NoRatedEvidence_ReturnsEmptyReport()
    {
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var report = await handler.Handle(new GetSamplingConcentrationWarningsQuery(reviewId), CancellationToken.None);

        report.AnyWarning.Should().BeFalse();
        report.PerEpa.Should().BeEmpty();
        report.TotalRatedActivities.Should().Be(0);
    }

    [Fact]
    public async Task OneAssessorOverHalf_EmitsWarning()
    {
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");
        var cbd = await SeedActivityTypeAsync(dbContext, "cbd");

        // 3 ratings on EPA 7: assessor-A twice, assessor-B once -> one assessor at 2/3 > 50%
        AddActivity(dbContext, miniCex, subject: "trainee-1", assessor: "assessor-a", epaId: 7, createdOn: new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, miniCex, subject: "trainee-1", assessor: "assessor-a", epaId: 7, createdOn: new DateTime(2026, 2, 5, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, cbd, subject: "trainee-1", assessor: "assessor-b", epaId: 7, createdOn: new DateTime(2026, 2, 10, 10, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var report = await handler.Handle(new GetSamplingConcentrationWarningsQuery(reviewId), CancellationToken.None);

        report.AnyWarning.Should().BeTrue();
        var warning = report.PerEpa.Should().ContainSingle(entry => entry.EpaId == 7).Subject;
        warning.OneAssessorOverHalf.Should().BeTrue();
        warning.DominantAssessorUserId.Should().Be("assessor-a");
        warning.DominantAssessorCount.Should().Be(2);
        warning.RatingCount.Should().Be(3);
        warning.DistinctAssessorCount.Should().Be(2);
        warning.DistinctSourceCount.Should().Be(2);
    }

    [Fact]
    public async Task SingleSource_EmitsWarning()
    {
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");
        var dops = await SeedActivityTypeAsync(dbContext, "dops");

        // Both activity types map to DirectObservation -> only one source covered
        AddActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, dops, "trainee-1", "assessor-b", 7, new DateTime(2026, 2, 5, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, miniCex, "trainee-1", "assessor-c", 7, new DateTime(2026, 2, 10, 10, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var report = await handler.Handle(new GetSamplingConcentrationWarningsQuery(reviewId), CancellationToken.None);

        var warning = report.PerEpa.Should().ContainSingle(entry => entry.EpaId == 7).Subject;
        warning.SingleSource.Should().BeTrue();
        warning.DistinctSourceCount.Should().Be(1);
        warning.FewerThanThreeAssessors.Should().BeFalse();
        warning.OneAssessorOverHalf.Should().BeFalse();
    }

    [Fact]
    public async Task FewerThanThreeAssessors_EmitsWarning()
    {
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");
        var cbd = await SeedActivityTypeAsync(dbContext, "cbd");

        // Two assessors across two sources -> triggers only fewer-than-three
        AddActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, cbd, "trainee-1", "assessor-b", 7, new DateTime(2026, 2, 5, 10, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var report = await handler.Handle(new GetSamplingConcentrationWarningsQuery(reviewId), CancellationToken.None);

        var warning = report.PerEpa.Should().ContainSingle(entry => entry.EpaId == 7).Subject;
        warning.FewerThanThreeAssessors.Should().BeTrue();
        warning.OneAssessorOverHalf.Should().BeFalse();
        warning.SingleSource.Should().BeFalse();
        warning.DistinctAssessorCount.Should().Be(2);
    }

    [Fact]
    public async Task WellSampledEvidence_NoWarnings()
    {
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");
        var cbd = await SeedActivityTypeAsync(dbContext, "cbd");
        var acat = await SeedActivityTypeAsync(dbContext, "acat");

        // 4 ratings, 4 distinct assessors, 2 sources -> no assessor > 50%, multi-source, >=3 assessors
        AddActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, cbd, "trainee-1", "assessor-b", 7, new DateTime(2026, 2, 5, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, acat, "trainee-1", "assessor-c", 7, new DateTime(2026, 2, 10, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, miniCex, "trainee-1", "assessor-d", 7, new DateTime(2026, 2, 15, 10, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var report = await handler.Handle(new GetSamplingConcentrationWarningsQuery(reviewId), CancellationToken.None);

        report.AnyWarning.Should().BeFalse();
        report.PerEpa.Should().BeEmpty();
        report.TotalRatedActivities.Should().Be(4);
        report.DistinctAssessorCount.Should().Be(4);
    }

    [Fact]
    public async Task ExcludesActivitiesOutsideReviewPeriod()
    {
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        // Activity outside window should be ignored
        AddActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, new DateTime(2025, 12, 1, 10, 0, 0, DateTimeKind.Utc));
        AddActivity(dbContext, miniCex, "trainee-1", "assessor-b", 7, new DateTime(2026, 4, 15, 10, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var report = await handler.Handle(new GetSamplingConcentrationWarningsQuery(reviewId), CancellationToken.None);

        report.TotalRatedActivities.Should().Be(0);
        report.AnyWarning.Should().BeFalse();
    }

    [Fact]
    public async Task IgnoresUnratedActivityTypes()
    {
        await using var dbContext = CreateDbContext();
        var reviewId = await SeedReviewAsync(dbContext);
        var reflectiveNote = await SeedActivityTypeAsync(dbContext, "reflective_note");

        AddActivity(dbContext, reflectiveNote, "trainee-1", "assessor-a", 7, new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetSamplingConcentrationWarningsQueryHandler(dbContext);
        var report = await handler.Handle(new GetSamplingConcentrationWarningsQuery(reviewId), CancellationToken.None);

        report.TotalRatedActivities.Should().Be(0);
        report.AnyWarning.Should().BeFalse();
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task<int> SeedReviewAsync(ApplicationDbContext dbContext)
    {
        var epa = new Epa { Id = 7, SubSpecialityId = 1, Code = "EPA-07", Title = "Emergency triage", IsActive = true };
        dbContext.Epas.Add(epa);

        var review = new CommitteeReview
        {
            Id = 42,
            PanelId = 1,
            TraineeUserId = "trainee-1",
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 3, 31),
            ScheduledOn = new DateOnly(2026, 4, 1)
        };
        dbContext.CommitteeReviews.Add(review);
        await dbContext.SaveChangesAsync();
        return review.Id;
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

    private static void AddActivity(
        ApplicationDbContext dbContext,
        ActivityType activityType,
        string subject,
        string assessor,
        int epaId,
        DateTime createdOn)
    {
        var dataJson = $"{{\"epa_id\": {epaId}, \"assessor_user_id\": \"{assessor}\"}}";
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
