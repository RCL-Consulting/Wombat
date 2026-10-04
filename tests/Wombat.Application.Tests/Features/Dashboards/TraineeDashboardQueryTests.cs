using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Dashboards.Trainee;
using Wombat.Application.Tests.Features.EntrustmentDecisions;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Application.Features.EntrustmentDecisions;
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
        result.NeedsYou.Should().BeEmpty();
        result.RecentDecisions.Should().BeEmpty();
        result.Standing.Should().BeNull();
    }

    [Fact]
    public async Task Trainee_WithActivities_ReturnsCurriculumProgressAndTheStanding()
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
        // T355 (note 3): the standing in the same read, over the same curriculum, as on the same day.
        result.Standing.Should().NotBeNull();
        result.Standing!.AsOf.Should().Be(new DateOnly(2026, 9, 23));
        result.Standing.Epas.Select(epa => epa.EpaCode).Should().Equal("EPA1");
        // Her draft and her request in hand are no decisions: nobody else has moved them.
        result.RecentDecisions.Should().BeEmpty();
    }

    /// <summary>
    /// T355 (note 3): Home's standing is the reader in summary mode, inside the one read behind DashboardFrame: the STAR and
    /// the year's level, but no latest rating, which Home does not show and need not read.
    /// </summary>
    [Fact]
    public async Task TheStanding_IsReadInSummaryMode_WithNoRating()
    {
        await using var db = CreateDb();
        await EntrustmentStandingReaderTests.SeedRatedAsync(db);

        var result = await new GetTraineeDashboardSummaryQueryHandler(db, new WorkflowEvaluator(), FakeUserDirectory.Empty).Handle(
            new GetTraineeDashboardSummaryQuery(
                CreatePrincipal(EntrustmentStandingReaderTests.TraineeUserId, ["Trainee"]),
                AsOf: EntrustmentStandingReaderTests.YearTwo),
            CancellationToken.None);

        var epa = result.Standing!.Epas.Should().ContainSingle().Subject;
        epa.Decision.Should().NotBeNull();
        epa.YearStatus.Should().Be(EntrustmentStandingStatus.AtOrAbove);
        epa.LatestRating.Should().BeNull("summary mode reads no rating");
        result.Standing.TargetYear.Should().Be(2);
    }

    /// <summary>
    /// T355 (B1; E6): Home's Recent decisions is DecidedOnYours' first five, newest decision first, and it says whether
    /// each is finished by its pinned workflow (D44): an accepted teaching session and a discussed reflective exercise are
    /// done; a declined request is a dead end, decided and not finished.
    /// </summary>
    [Fact]
    public async Task RecentDecisions_AreTheFirstFive_NewestFirst_EachSayingWhetherItIsFinished()
    {
        await using var db = CreateDb();
        SeedFinishingTypes(db);
        AddDecided(db, 1, TeachingTypeId, version: 1, "accepted", hoursAgo: 1);
        AddDecided(db, 2, WbaTypeId, version: 1, "declined", hoursAgo: 2);
        AddDecided(db, 3, ReflectiveTypeId, version: 1, "discussed", hoursAgo: 3);
        AddDecided(db, 4, WbaTypeId, version: 1, "completed", hoursAgo: 4);
        AddDecided(db, 5, WbaTypeId, version: 1, "completed", hoursAgo: 5);
        AddDecided(db, 6, WbaTypeId, version: 1, "completed", hoursAgo: 6);
        // Discussed on version 2 leads on to a sign-off: a move is left, so it is not decided yet.
        AddDecided(db, 7, ReflectiveTypeId, version: 2, "discussed", hoursAgo: 0);
        await db.SaveChangesAsync();

        var result = await new GetTraineeDashboardSummaryQueryHandler(db, new WorkflowEvaluator(), FakeUserDirectory.Empty).Handle(
            new GetTraineeDashboardSummaryQuery(CreatePrincipal("trainee-1", ["Trainee"])), CancellationToken.None);

        GetTraineeDashboardSummaryQueryHandler.RecentDecisionsListed.Should().Be(5);
        result.RecentDecisions.Select(item => (item.Id, item.IsFinished))
            .Should().Equal((1, true), (2, false), (3, true), (4, true), (5, true));
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
    public async Task AnActivityInATerminalStateOfItsPinnedWorkflow_IsNotInNeedsYou()
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

        // Home's card is Needs you (T342, E8): her draft is hers to submit, so it is there; the accepted Mini-CEX she may
        // only cancel, so it is not (T342, B6). The version-2 discussion waits on its supervisor's sign-off, so it is still
        // due but not hers to move. The card is ListNeedsYouQuery's rows (T297's rule, restated).
        result.NeedsYou.Select(row => row.Id).Should().Equal(6);
        (await NeedsYouAsync(db, "trainee-1")).Select(row => row.Id).Should().Equal(result.NeedsYou.Select(row => row.Id));
    }

    /// <summary>
    /// T297, Steps 3.12 and 3.16: the card listed a declined request, which has no move left, and dropped a reflection
    /// awaiting discussion, which she may still cancel, while the inbox it opens did the opposite. It lists what the inbox
    /// lists. T342 (B6): a request or a reflection she may only cancel is with its assessor, so it is on neither the card
    /// nor Needs you, and a declined one has no move left. A declined request is shown, with its badge, on Recent
    /// decisions (T355, E6) and on My Activities. No mail announces it: <c>AssessmentDeclinedEmail</c> has no sender (T320).
    /// </summary>
    [Fact]
    public async Task ADeclinedCpsaRequest_AndOneSheMayOnlyCancel_AreNotOnTheCard()
    {
        await using var db = CreateDb();
        ShippedSeeds.AddType(db, 21, "mini_cex_cpsa", "Mini-CEX (Paediatrics)");
        ShippedSeeds.AddType(db, 22, "reflective_exercise_cpsa", "Reflective Exercise (Paediatrics)");
        const string namesBotha = """{ "assessor_user_id": "assessor-botha" }""";
        AddDecided(db, 1, 21, version: 1, "declined", hoursAgo: 1, namesBotha);
        AddOwn(db, 2, 21, version: 1, "requested", namesBotha);
        AddOwn(db, 3, 22, version: 1, "submitted", namesBotha);
        await db.SaveChangesAsync();

        var result = await new GetTraineeDashboardSummaryQueryHandler(db, new WorkflowEvaluator(), FakeUserDirectory.Empty).Handle(
            new GetTraineeDashboardSummaryQuery(CreatePrincipal("trainee-1", ["Trainee"])), CancellationToken.None);

        result.NeedsYou.Should().BeEmpty();
        (await NeedsYouAsync(db, "trainee-1")).Should().BeEmpty();
        result.RecentDecisions.Select(item => item.Id).Should().Equal([1], "the decline is shown with its badge there");
    }

    /// <summary>
    /// T342 (flow 03, E8): for a trainee who is also an assessor, Home's Trainee card is her own work that waits on her,
    /// Needs you, and never another trainee's request that waits on her rating: that is the Assessor's inbox. Until T342
    /// the card was the Activity inbox's first rows and listed it, naming the trainee (the T297 review).
    /// </summary>
    [Fact]
    public async Task ForATraineeWhoAlsoAssesses_TheCardIsHerOwnWork_NotTheRequestsWaitingOnHer()
    {
        await using var db = CreateDb();
        ShippedSeeds.AddType(db, 21, "mini_cex_cpsa", "Mini-CEX (Paediatrics)");
        AddOwn(db, 1, 21, version: 1, "requested", """{ "assessor_user_id": "assessor-botha" }""");
        AddOwn(db, 2, 21, version: 1, "requested", """{ "assessor_user_id": "trainee-1" }""", subjectUserId: "trainee-2");
        AddOwn(db, 3, 21, version: 1, "draft", """{ "assessor_user_id": "assessor-botha" }""");
        await db.SaveChangesAsync();

        var names = new FakeUserDirectory(("trainee-1", "Sipho Ndlovu"), ("trainee-2", "Nomsa Mahlangu"), ("assessor-botha", "Sarah Botha"));
        var principal = CreatePrincipal("trainee-1", [WombatRoles.Trainee, WombatRoles.Assessor]);
        var result = await new GetTraineeDashboardSummaryQueryHandler(db, new WorkflowEvaluator(), names).Handle(
            new GetTraineeDashboardSummaryQuery(principal), CancellationToken.None);

        result.NeedsYou.Select(row => row.Id).Should().Equal(3);
        result.NeedsYou.Single().NomineeName.Should().Be("Sarah Botha", "the row carries what My activities' row carries");
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
        AddDecided(db, 4, WbaTypeId, version: 1, "declined", hoursAgo: 1, subjectUserId: "trainee-2");
        AddDecided(db, 5, WbaTypeId, version: 1, "completed", hoursAgo: 2);
        await db.SaveChangesAsync();

        var result = await new GetTraineeDashboardSummaryQueryHandler(db, new WorkflowEvaluator(), FakeUserDirectory.Empty).Handle(
            new GetTraineeDashboardSummaryQuery(CreatePrincipal("trainee-1", ["Trainee"])), CancellationToken.None);

        result.NeedsYou.Should().BeEmpty("her own request waits on its assessor, and the others' are not hers (T342)");
        result.RecentDecisions.Select(item => item.Id).Should().Equal(5);
    }

    private static async Task<IReadOnlyList<Wombat.Application.Features.Activities.Dtos.ActivitySummaryDto>> NeedsYouAsync(
        ApplicationDbContext db, string userId)
        => await new Wombat.Application.Features.Activities.Queries.ListNeedsYou.ListNeedsYouQueryHandler(
                db, new WorkflowEvaluator(), FakeUserDirectory.Empty)
            .Handle(
                new Wombat.Application.Features.Activities.Queries.ListNeedsYou.ListNeedsYouQuery(CreatePrincipal(userId, ["Trainee"])),
                CancellationToken.None);

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

    /// <summary>
    /// One of her activities that an assessor moved last, <paramref name="hoursAgo" /> hours ago, into
    /// <paramref name="state" />: what Recent decisions reads (T355).
    /// </summary>
    private static void AddDecided(
        ApplicationDbContext db, int id, int typeId, int version, string state, int hoursAgo,
        string dataJson = "{}", string subjectUserId = "trainee-1")
    {
        var at = DateTime.UtcNow.AddHours(-hoursAgo);
        db.Activities.Add(new Activity
        {
            Id = id, ActivityTypeId = typeId, SchemaVersion = version,
            SubjectUserId = subjectUserId, CreatedByUserId = subjectUserId, CurrentState = state, DataJson = dataJson,
            CreatedOn = at.AddDays(-1), UpdatedOn = at,
            Transitions =
            [
                new ActivityTransition
                {
                    FromState = "draft", ToState = "submitted", TransitionKey = "submit", ActorUserId = subjectUserId,
                    OccurredOn = at.AddDays(-1)
                },
                new ActivityTransition
                {
                    FromState = "submitted", ToState = state, TransitionKey = "move", ActorUserId = "assessor-botha",
                    OccurredOn = at
                }
            ]
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
