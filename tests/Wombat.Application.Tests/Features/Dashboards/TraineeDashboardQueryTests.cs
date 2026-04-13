using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Dashboards.Trainee;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Dashboards;

public sealed class TraineeDashboardQueryTests
{
    [Fact]
    public async Task PendingTrainee_ReturnsEmptyDashboardWithFlag()
    {
        await using var db = CreateDb();
        var handler = new GetTraineeDashboardSummaryQueryHandler(db);
        var principal = CreatePrincipal("trainee-1", ["PendingTrainee"]);

        var result = await handler.Handle(
            new GetTraineeDashboardSummaryQuery(principal), CancellationToken.None);

        result.IsPendingTrainee.Should().BeTrue();
        result.CurriculumProgress.Should().BeEmpty();
        result.Inbox.Should().BeEmpty();
        result.RecentActivities.Should().BeEmpty();
        result.UpcomingDeadlines.Should().BeEmpty();
    }

    [Fact]
    public async Task Trainee_WithActivities_ReturnsCurriculumProgressAndInbox()
    {
        await using var db = CreateDb();
        SeedTraineeData(db);
        var handler = new GetTraineeDashboardSummaryQueryHandler(db);
        var principal = CreatePrincipal("trainee-1", ["Trainee"]);

        var result = await handler.Handle(
            new GetTraineeDashboardSummaryQuery(principal), CancellationToken.None);

        result.IsPendingTrainee.Should().BeFalse();
        result.CurriculumProgress.Should().HaveCount(1);
        result.CurriculumProgress[0].EpaTitle.Should().Be("EPA 1");
        result.CurriculumProgress[0].CompletedCount.Should().Be(2);
        result.CurriculumProgress[0].RequiredCount.Should().Be(5);
        result.RecentActivities.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task Trainee_WithNoProfile_ReturnsEmptyProgress()
    {
        await using var db = CreateDb();
        var handler = new GetTraineeDashboardSummaryQueryHandler(db);
        var principal = CreatePrincipal("trainee-no-profile", ["Trainee"]);

        var result = await handler.Handle(
            new GetTraineeDashboardSummaryQuery(principal), CancellationToken.None);

        result.IsPendingTrainee.Should().BeFalse();
        result.CurriculumProgress.Should().BeEmpty();
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static void SeedTraineeData(ApplicationDbContext db)
    {
        var institution = new Institution { Id = 1, Name = "Test Institution" };
        db.Institutions.Add(institution);

        var speciality = new Speciality { Id = 1, InstitutionId = 1, Name = "Test Spec" };
        db.Specialities.Add(speciality);

        var subSpec = new SubSpeciality { Id = 1, SpecialityId = 1, Name = "Test SubSpec" };
        db.SubSpecialities.Add(subSpec);

        var epa = new Epa { Id = 1, SubSpecialityId = 1, Code = "EPA1", Title = "EPA 1" };
        db.Epas.Add(epa);

        var curriculum = new Curriculum
        {
            Id = 1, SubSpecialityId = 1, Name = "Test Curriculum",
            Version = "1.0", EffectiveFrom = new DateOnly(2025, 1, 1), IsActive = true
        };
        db.Curricula.Add(curriculum);

        var item = new CurriculumItem
        {
            Id = 1, CurriculumId = 1, EpaId = 1, RequiredCount = 5,
            MinimumLevelOrder = 1, WindowMonths = 36
        };
        db.CurriculumItems.Add(item);

        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1, UserId = "trainee-1", CurriculumId = 1,
            ProgrammeStartDate = new DateOnly(2025, 1, 1),
            ExpectedCompletionDate = new DateOnly(2028, 1, 1),
            IsActive = true
        });

        db.CurriculumItemProgresses.Add(new CurriculumItemProgress
        {
            Id = 1, CurriculumItemId = 1, TraineeUserId = "trainee-1",
            CountsSoFar = 2, MinimumLevelReachedCount = 2, LastUpdated = DateTime.UtcNow
        });

        var activityType = new ActivityType
        {
            Id = 1, Key = "mini_cex", Name = "Mini-CEX",
            Scope = ActivityScope.Global
        };
        db.ActivityTypes.Add(activityType);

        db.Activities.Add(new Activity
        {
            Id = 1, ActivityTypeId = 1, SubjectUserId = "trainee-1",
            CreatedByUserId = "trainee-1", CurrentState = "draft",
            DataJson = "{}", SchemaVersion = 1,
            CreatedOn = DateTime.UtcNow.AddDays(-1), UpdatedOn = DateTime.UtcNow
        });

        db.Activities.Add(new Activity
        {
            Id = 2, ActivityTypeId = 1, SubjectUserId = "trainee-1",
            CreatedByUserId = "trainee-1", CurrentState = "requested",
            DataJson = "{}", SchemaVersion = 1,
            CreatedOn = DateTime.UtcNow.AddDays(-2), UpdatedOn = DateTime.UtcNow
        });

        db.SaveChanges();
    }

    private static ClaimsPrincipal CreatePrincipal(string userId, IReadOnlyCollection<string>? roles = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        foreach (var role in roles ?? [])
            claims.Add(new Claim(ClaimTypes.Role, role));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}
