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
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.Dashboards;

public sealed class TraineeDashboardQueryTests
{
    [Fact]
    public async Task PendingTrainee_ReturnsEmptyDashboardWithFlag()
    {
        await using var db = CreateDb();
        var handler = new GetTraineeDashboardSummaryQueryHandler(db, new WorkflowEvaluator(), FakeUserDirectory.Empty);
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
        var handler = new GetTraineeDashboardSummaryQueryHandler(db, new WorkflowEvaluator(), FakeUserDirectory.Empty);
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
    public async Task AWithdrawnTrainee_WhoKeepsTheTraineeRole_GetsTheProgrammeTheyEnded_MarkedAsEnded()
    {
        // T252. Deactivation keeps the Trainee role, so a withdrawn trainee lands on this dashboard. It reads what their
        // progress page reads (TraineeQuotaProgressReader.ReadAsync), so the card can say the programme ended rather than
        // "No curriculum assigned yet".
        await using var db = CreateDb();
        SeedTraineeData(db);
        await db.SaveChangesAsync();
        db.Set<TraineeProfile>().Local.Single().Deactivate(new DateOnly(2026, 8, 20), today: new DateOnly(2026, 8, 20));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await new GetTraineeDashboardSummaryQueryHandler(db, new WorkflowEvaluator(), FakeUserDirectory.Empty).Handle(
            new GetTraineeDashboardSummaryQuery(CreatePrincipal("trainee-1", ["Trainee"]), AsOf: new DateOnly(2026, 9, 23)),
            CancellationToken.None);

        result.CurriculumTargets.Should().NotBeNull();
        result.CurriculumTargets!.Ended.Should().Be(new Wombat.Application.Features.Curricula.Quota.ProgrammeEndDto(
            Completed: false, EndedOn: new DateOnly(2026, 8, 20), Today: new DateOnly(2026, 9, 23)));
    }

    [Fact]
    public async Task Trainee_WithNoProfile_ReturnsEmptyProgress()
    {
        await using var db = CreateDb();
        var handler = new GetTraineeDashboardSummaryQueryHandler(db, new WorkflowEvaluator(), FakeUserDirectory.Empty);
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

        var result = await new GetTraineeDashboardSummaryQueryHandler(db, new WorkflowEvaluator(), FakeUserDirectory.Empty).Handle(
            new GetTraineeDashboardSummaryQuery(CreatePrincipal("trainee-1", ["Trainee"])), CancellationToken.None);

        // The inbox card is what the trainee can move (T297): she may cancel the accepted Mini-CEX and submit the draft. The
        // version-2 discussion waits on its supervisor's sign-off, so it is still due but not hers to move.
        result.Inbox.Select(item => item.ActivityId).Should().BeEquivalentTo([5, 6]);
        result.UpcomingDeadlines.Select(item => item.ActivityId).Should().BeEquivalentTo([5, 6, 7]);
    }

    /// <summary>
    /// T297, Steps 3.12 and 3.16: the card listed a declined request, which has no move left, and dropped a reflection
    /// awaiting discussion, which she may still cancel, while the inbox it opens did the opposite. It lists what the inbox
    /// lists. A declined request is shown, with its badge, on Recent activities and on My Activities. No mail announces it:
    /// <c>AssessmentDeclinedEmail</c> has no sender (T320).
    /// </summary>
    [Fact]
    public async Task ADeclinedCpsaRequest_IsNotOnTheInboxCard_AndASubmittedReflectiveExerciseIs()
    {
        await using var db = CreateDb();
        ShippedSeeds.AddType(db, 21, "mini_cex_cpsa", "Mini-CEX (Paediatrics)");
        ShippedSeeds.AddType(db, 22, "reflective_exercise_cpsa", "Reflective Exercise (Paediatrics)");
        const string namesBotha = """{ "assessor_user_id": "assessor-botha" }""";
        AddOwn(db, 1, 21, version: 1, "declined", namesBotha);
        AddOwn(db, 2, 21, version: 1, "requested", namesBotha);
        AddOwn(db, 3, 22, version: 1, "submitted", namesBotha);
        await db.SaveChangesAsync();

        var result = await new GetTraineeDashboardSummaryQueryHandler(db, new WorkflowEvaluator(), FakeUserDirectory.Empty).Handle(
            new GetTraineeDashboardSummaryQuery(CreatePrincipal("trainee-1", ["Trainee"])), CancellationToken.None);

        result.Inbox.Select(item => (item.ActivityId, item.CurrentStateLabel))
            .Should().Equal((2, "Requested"), (3, "Awaiting discussion"));
        result.RecentActivities.Select(item => item.ActivityId).Should().Contain(1, "the decline is shown with its badge there");
    }

    /// <summary>
    /// The T297 review: for a trainee who is also an assessor the card lists other trainees' requests beside her own, and
    /// a row that is someone else's says whose, as the inbox and the Assessor's card do (T250). Her own row names nobody.
    /// </summary>
    [Fact]
    public async Task ARowThatIsAnotherTraineesWork_NamesTheTrainee_AndTheCallersOwnNamesNobody()
    {
        await using var db = CreateDb();
        ShippedSeeds.AddType(db, 21, "mini_cex_cpsa", "Mini-CEX (Paediatrics)");
        AddOwn(db, 1, 21, version: 1, "requested", """{ "assessor_user_id": "assessor-botha" }""");
        AddOwn(db, 2, 21, version: 1, "requested", """{ "assessor_user_id": "trainee-1" }""", subjectUserId: "trainee-2");
        await db.SaveChangesAsync();

        var names = new FakeUserDirectory(("trainee-1", "Sipho Ndlovu"), ("trainee-2", "Nomsa Mahlangu"));
        var result = await new GetTraineeDashboardSummaryQueryHandler(db, new WorkflowEvaluator(), names).Handle(
            new GetTraineeDashboardSummaryQuery(CreatePrincipal("trainee-1", [WombatRoles.Trainee, WombatRoles.Assessor])),
            CancellationToken.None);

        result.Inbox.Select(item => (item.ActivityId, item.SubjectName)).Should().BeEquivalentTo(
            [(1, (string?)null), (2, "Nomsa Mahlangu")]);
        names.Lookups.Should().ContainSingle().Which.Should().Equal(["trainee-2"], "one lookup, for the rows that are not her own");
    }

    [Fact]
    public async Task ARecentActivity_SaysWhetherItsPinnedWorkflowIsFinished()
    {
        // The badge on Recent activities is green when finished (T266 review), by the test the inbox uses (D44): an
        // accepted teaching session is done and an accepted Mini-CEX is not; a discussed reflective exercise is done on
        // version 1 and not on version 2, where a sign-off follows.
        await using var db = CreateDb();
        SeedFinishingTypes(db);
        AddOwn(db, 1, TeachingTypeId, version: 1, "accepted", "{}");
        AddOwn(db, 2, WbaTypeId, version: 1, "accepted", "{}");
        AddOwn(db, 3, ReflectiveTypeId, version: 1, "discussed", "{}");
        AddOwn(db, 4, ReflectiveTypeId, version: 2, "discussed", "{}");
        AddOwn(db, 5, WbaTypeId, version: 1, "declined", "{}");
        await db.SaveChangesAsync();

        var result = await new GetTraineeDashboardSummaryQueryHandler(db, new WorkflowEvaluator(), FakeUserDirectory.Empty).Handle(
            new GetTraineeDashboardSummaryQuery(CreatePrincipal("trainee-1", ["Trainee"])), CancellationToken.None);

        result.RecentActivities.Select(item => (item.ActivityId, item.IsFinished))
            .Should().BeEquivalentTo([(1, true), (2, false), (3, true), (4, false), (5, false)]);
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

        var result = await new GetTraineeDashboardSummaryQueryHandler(db, new WorkflowEvaluator(), FakeUserDirectory.Empty).Handle(
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

        // Each with the version row a publish writes: a move on an activity is judged against it (T297).
        db.Set<ActivityTypeVersion>().AddRange(
            new ActivityTypeVersion
            {
                Id = 1, ActivityTypeId = ReflectiveTypeId, Version = 1, WorkflowJson = FinishingWorkflows.ReflectiveExercise
            },
            new ActivityTypeVersion
            {
                Id = 2, ActivityTypeId = ReflectiveTypeId, Version = 2,
                WorkflowJson = FinishingWorkflows.ReflectiveExerciseWithSignOff
            },
            new ActivityTypeVersion { Id = 3, ActivityTypeId = TeachingTypeId, Version = 1, WorkflowJson = FinishingWorkflows.TeachingSession },
            new ActivityTypeVersion { Id = 4, ActivityTypeId = WbaTypeId, Version = 1, WorkflowJson = FinishingWorkflows.Wba });
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
