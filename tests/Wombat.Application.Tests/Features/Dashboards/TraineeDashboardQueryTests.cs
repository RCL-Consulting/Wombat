using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Dashboards.Trainee;
using Wombat.Application.Tests.TestHelpers;
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
        result.CurriculumTargets.Should().BeNull();
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
            new GetTraineeDashboardSummaryQuery(principal, AsOf: new DateOnly(2026, 9, 23)), CancellationToken.None);

        result.IsPendingTrainee.Should().BeFalse();
        // The same read model as the progress page (T130): the item's 2026 academic-year window, the target per window.
        var item = result.CurriculumTargets!.Items.Should().ContainSingle().Subject;
        item.EpaTitle.Should().Be("EPA 1");
        item.Current.Name.Should().Be("2026 academic year");
        item.Current.Count.Should().Be(2);
        item.Target.Should().Be(5);
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
        result.CurriculumTargets.Should().BeNull();
    }

    [Fact]
    public async Task AnActivityInATerminalStateOfItsPinnedWorkflow_IsNeitherInTheInboxNorDue()
    {
        // T203: finished is a terminal state of the activity's PINNED workflow, not the literal "completed". A
        // discussed reflective exercise and an accepted teaching session are done; a Mini-CEX in "accepted" is not.
        await using var db = CreateDb();
        SeedFinishingTypes(db);
        var due = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(3).ToString("yyyy-MM-dd");
        var withDueDate = $$"""{ "due_date": "{{due}}" }""";

        AddOwn(db, 1, ReflectiveTypeId, version: 1, "discussed", withDueDate);
        AddOwn(db, 2, TeachingTypeId, version: 1, "accepted", withDueDate);
        AddOwn(db, 3, WbaTypeId, version: 1, "completed", withDueDate);
        AddOwn(db, 4, WbaTypeId, version: 1, "cancelled", withDueDate);
        AddOwn(db, 5, WbaTypeId, version: 1, "accepted", withDueDate);
        AddOwn(db, 6, ReflectiveTypeId, version: 1, "draft", withDueDate);
        // Pinned to version 2, where "discussed" leads on to a sign-off: not finished, so still due.
        AddOwn(db, 7, ReflectiveTypeId, version: 2, "discussed", withDueDate);
        await db.SaveChangesAsync();

        var result = await new GetTraineeDashboardSummaryQueryHandler(db).Handle(
            new GetTraineeDashboardSummaryQuery(CreatePrincipal("trainee-1", ["Trainee"])), CancellationToken.None);

        result.Inbox.Select(item => item.ActivityId).Should().BeEquivalentTo([5, 6]);
        result.UpcomingDeadlines.Select(item => item.ActivityId).Should().BeEquivalentTo([5, 6, 7]);
    }

    [Fact]
    public async Task AnotherTraineesActivities_AreInNoneOfTheLists()
    {
        // The architecture test's exemption for this handler rests on its rows being the caller's own
        // (ActivityReadBoundaryTests: "Confined to SubjectUserId == the signed-in user"). Every list is pinned here: another
        // trainee's open, due activities would otherwise read as the caller's inbox, deadlines and recent work.
        await using var db = CreateDb();
        SeedFinishingTypes(db);
        var due = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(3).ToString("yyyy-MM-dd");
        var withDueDate = $$"""{ "due_date": "{{due}}" }""";

        AddOwn(db, 1, WbaTypeId, version: 1, "requested", withDueDate);
        AddOwn(db, 2, WbaTypeId, version: 1, "requested", withDueDate, subjectUserId: "trainee-2");
        AddOwn(db, 3, ReflectiveTypeId, version: 1, "draft", withDueDate, subjectUserId: "trainee-2");
        AddOwn(db, 4, WbaTypeId, version: 1, "declined", withDueDate, subjectUserId: "trainee-2");
        await db.SaveChangesAsync();

        var result = await new GetTraineeDashboardSummaryQueryHandler(db).Handle(
            new GetTraineeDashboardSummaryQuery(CreatePrincipal("trainee-1", ["Trainee"])), CancellationToken.None);

        result.Inbox.Select(item => item.ActivityId).Should().Equal(1);
        result.UpcomingDeadlines.Select(item => item.ActivityId).Should().Equal(1);
        result.RecentActivities.Select(item => item.ActivityId).Should().Equal(1);
    }

    private const int ReflectiveTypeId = 11;
    private const int TeachingTypeId = 12;
    private const int WbaTypeId = 13;

    private static void SeedFinishingTypes(ApplicationDbContext db)
    {
        db.ActivityTypes.AddRange(
            new ActivityType
            {
                Id = ReflectiveTypeId, Key = "reflective_exercise", Name = "Reflective exercise",
                Scope = ActivityScope.Global, Version = 2,
                WorkflowJson = FinishingWorkflows.ReflectiveExerciseWithSignOff
            },
            new ActivityType
            {
                Id = TeachingTypeId, Key = "teaching_session", Name = "Teaching session",
                Scope = ActivityScope.Global, Version = 1, WorkflowJson = FinishingWorkflows.TeachingSession
            },
            new ActivityType
            {
                Id = WbaTypeId, Key = "mini_cex", Name = "Mini-CEX",
                Scope = ActivityScope.Global, Version = 1, WorkflowJson = FinishingWorkflows.Wba
            });

        db.Set<ActivityTypeVersion>().AddRange(
            new ActivityTypeVersion
            {
                Id = 1, ActivityTypeId = ReflectiveTypeId, Version = 1, WorkflowJson = FinishingWorkflows.ReflectiveExercise
            },
            new ActivityTypeVersion
            {
                Id = 2, ActivityTypeId = ReflectiveTypeId, Version = 2,
                WorkflowJson = FinishingWorkflows.ReflectiveExerciseWithSignOff
            });
    }

    private static void AddOwn(
        ApplicationDbContext db, int id, int typeId, int version, string state, string dataJson,
        string subjectUserId = "trainee-1")
    {
        var now = DateTime.UtcNow;
        db.Activities.Add(new Activity
        {
            Id = id, ActivityTypeId = typeId, SchemaVersion = version,
            SubjectUserId = subjectUserId, CreatedByUserId = subjectUserId, CurrentState = state, DataJson = dataJson,
            CreatedOn = now.AddDays(-1), UpdatedOn = now.AddMinutes(-id)
        });
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

        var speciality = new Speciality { Id = 1, CollegeId = 1, Name = "Test Spec" };
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
            AcademicYear = 2026, Semester = 2,
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
