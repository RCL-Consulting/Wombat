using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T130: on the live path, every read credit needs happens BEFORE the transition mutates the activity.
/// </summary>
/// <remarks>
/// <para>
/// The audit pipeline's catch saves the request's DbContext (the HANDOFF trap: <c>AuditWriter</c> shares it and
/// calls <c>SaveChangesAsync</c>). So an exception thrown after <c>ApplyTransition</c> COMMITS the half-finished
/// move: a terminal activity with no credit and a <c>CreditedItemCount</c> of null, which reads as "never
/// evaluated", and only a rebuild could repair it. Before T130 the applier awaited the database between
/// increments, after the transition had already been applied.
/// </para>
/// <para>
/// <c>ActivityService</c> now plans credit (every read) first and applies it synchronously after the transition.
/// These tests drive the real service. One fails the plan and checks that nothing is left in the change tracker for
/// the audit save to write. The other checks the price of planning early: the plan must be built from the encounter
/// date the transition is ABOUT to write, not the one the entity still carries.
/// </para>
/// </remarks>
public sealed class CreditPlannedBeforeTransitionTests
{
    private const int ActivityTypeId = 100;
    private const int CreditedEpaId = 5000;

    [Fact]
    public async Task AFailureWhilePlanningCredit_LeavesTheActivityUntransitioned_WithNothingForTheAuditSaveToCommit()
    {
        // The completion also re-dates the encounter from 10 March to 1 August as it moves. If the observed-date
        // stamp, the data merge or the transition ran before the plan, the failure would leave that change sitting
        // in the tracker, and the audit pipeline's save would commit it.
        var options = NewDatabase();
        await using var db = new ApplicationDbContext(options);
        Seed(db);

        var planner = new FailingPlanner(new CreditApplier(db));
        var service = Service(db, planner);
        var principal = Principal("trainee-1");

        var draft = await service.CreateDraftAsync(
            new CreateActivityInput(ActivityTypeId, "trainee-1", "trainee-1", DataObservedOn("2026-03-10"), principal),
            CancellationToken.None);

        await service.TransitionAsync(
            new TransitionActivityInput(draft.Id, "submit", "trainee-1", principal, null, null),
            CancellationToken.None);

        var complete = async () => await service.TransitionAsync(
            new TransitionActivityInput(draft.Id, "complete", "trainee-1", principal, """{ "observed_on": "2026-08-01" }""", null),
            CancellationToken.None);

        await complete.Should().ThrowAsync<InvalidOperationException>().WithMessage("*induced*");
        planner.PlanCalls.Should().Be(1, "guard: the failure comes from the credit plan, not from an earlier gate");
        planner.ApplyCalls.Should().Be(0);

        var activity = db.Activities.Local.Single(entity => entity.Id == draft.Id);
        activity.CurrentState.Should().Be("submitted");
        activity.ObservedOn.Should().Be(new DateOnly(2026, 3, 10), "the observed-date stamp runs after the plan");
        ObservedOnIn(activity.DataJson).Should().Be("2026-03-10", "the patch is only written by ApplyTransition");
        activity.Transitions.Select(transition => transition.TransitionKey)
            .Should().BeEquivalentTo(new[] { "create", "submit" });

        db.ChangeTracker.Entries<ActivityTransition>()
            .Should().NotContain(entry => entry.State == EntityState.Added, "no completion transition may be tracked");
        db.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Should().BeEmpty("the audit pipeline's catch saves this context, so anything dirty here would be committed");

        // What the audit pipeline's catch does next: save the same context.
        (await db.SaveChangesAsync()).Should().Be(0);

        await using var verify = new ApplicationDbContext(options);
        var stored = await verify.Activities.Include(entity => entity.Transitions).SingleAsync(entity => entity.Id == draft.Id);
        stored.CurrentState.Should().Be("submitted");
        stored.ObservedOn.Should().Be(new DateOnly(2026, 3, 10));
        stored.Transitions.Should().HaveCount(2);
        stored.Transitions.Should().OnlyContain(transition => transition.CreditedItemCount == null);
        (await verify.CurriculumItemProgresses.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task AWorkingPlan_CreditsTheSemesterOfTheDateTheCompletionWrites()
    {
        // The control for the test above, and the risk of planning early. The plan is built BEFORE the stamp, so it
        // must be built from the merged data the transition is about to write. The entity still says 10 March
        // (semester 1); the completion says 1 August (semester 2). Credit must land in semester 2, and the transition
        // must be stamped with the one item it credited.
        var options = NewDatabase();
        await using var db = new ApplicationDbContext(options);
        Seed(db);

        var service = Service(db, new CreditApplier(db));
        var principal = Principal("trainee-1");

        var draft = await service.CreateDraftAsync(
            new CreateActivityInput(ActivityTypeId, "trainee-1", "trainee-1", DataObservedOn("2026-03-10"), principal),
            CancellationToken.None);

        await service.TransitionAsync(
            new TransitionActivityInput(draft.Id, "submit", "trainee-1", principal, null, null),
            CancellationToken.None);

        await service.TransitionAsync(
            new TransitionActivityInput(draft.Id, "complete", "trainee-1", principal, """{ "observed_on": "2026-08-01" }""", null),
            CancellationToken.None);

        await using var verify = new ApplicationDbContext(options);
        var stored = await verify.Activities.Include(entity => entity.Transitions).SingleAsync(entity => entity.Id == draft.Id);
        stored.CurrentState.Should().Be("completed");
        stored.ObservedOn.Should().Be(new DateOnly(2026, 8, 1), "guard: the patch was accepted, so the test above re-dated for real");
        stored.Transitions.Single(transition => transition.TransitionKey == "complete").CreditedItemCount.Should().Be(1);

        var rows = await verify.CurriculumItemProgresses.AsNoTracking().ToListAsync();
        var row = rows.Should().ContainSingle().Subject;
        (row.AcademicYear, row.Semester).Should().Be((2026, 2), "the encounter the completion recorded is 1 August");
        row.CountsSoFar.Should().Be(1);
        row.LastObservedOn.Should().Be(new DateOnly(2026, 8, 1));
        row.CreditedActivityKeysJson.Should().Be($$"""["{{draft.Id}}:complete"]""");
    }

    private static ActivityService Service(ApplicationDbContext db, ICreditApplier creditApplier)
        => new(db, new SchemaValidator(), new WorkflowEvaluator(), creditApplier, new FieldPermissionEvaluator());

    private static string DataObservedOn(string isoDate)
        => $$"""{ "epa_id": {{CreditedEpaId}}, "score": 4, "observed_on": "{{isoDate}}" }""";

    private static string? ObservedOnIn(string dataJson)
    {
        using var document = JsonDocument.Parse(dataJson);
        return document.RootElement.TryGetProperty("observed_on", out var value) ? value.GetString() : null;
    }

    /// <summary>Fails every plan, as a dropped connection or a cancellation part-way through the reads would.</summary>
    private sealed class FailingPlanner : ICreditApplier
    {
        private readonly ICreditApplier _inner;

        public FailingPlanner(ICreditApplier inner)
        {
            _inner = inner;
        }

        public int PlanCalls { get; private set; }

        public int ApplyCalls { get; private set; }

        public async Task<CreditPlan> PlanAsync(
            CreditSubject subject, ActivityType activityType, CancellationToken cancellationToken = default)
        {
            PlanCalls++;
            await Task.Yield();
            throw new InvalidOperationException("induced failure while planning credit");
        }

        public CreditApplicationResult Apply(CreditPlan plan, Activity completedActivity)
        {
            ApplyCalls++;
            return _inner.Apply(plan, completedActivity);
        }

        public Task<CreditApplicationResult> ApplyAsync(
            Activity completedActivity, ActivityType activityType, CancellationToken cancellationToken = default)
        {
            ApplyCalls++;
            return _inner.ApplyAsync(completedActivity, activityType, cancellationToken);
        }
    }

    private static void Seed(ApplicationDbContext db)
    {
        db.Epas.Add(new Epa { Id = CreditedEpaId, SubSpecialityId = 1, Code = "EPA-1", Title = "Take a history" });

        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 4000,
            CurriculumId = 3000,
            EpaId = CreditedEpaId,
            RequiredCount = 3,
            QuotaPeriod = QuotaPeriod.Semester,
            MinimumLevelOrder = 3,
            WindowMonths = 12
        });

        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = "trainee-1",
            InstitutionId = 10,
            CurriculumId = 3000,
            ProgrammeStartDate = new DateOnly(2025, 4, 14),
            ExpectedCompletionDate = new DateOnly(2029, 4, 13),
            IsActive = true
        });

        // The observation date comes from the form (T119), so the completion can move it.
        const string schemaJson = """
            {
              "version": 1,
              "observation_date_field": "observed_on",
              "sections": [
                {
                  "key": "details",
                  "title": "Details",
                  "fields": [
                    { "key": "epa_id", "type": "epa", "label": "EPA", "required": true },
                    { "key": "score", "type": "number", "label": "Score", "required": true },
                    { "key": "observed_on", "type": "date", "label": "Date observed", "required": true }
                  ]
                }
              ]
            }
            """;

        const string workflowJson = """
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

        const string creditRulesJson = """
            {
              "counts_for": [
                { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1, "minimum_level_field": "score" }
              ]
            }
            """;

        var publishedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var activityType = new ActivityType
        {
            Id = ActivityTypeId,
            Key = "wba_under_test",
            Name = "WBA under test",
            Scope = ActivityScope.Institution,
            ScopeId = 10,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditRulesJson,
            DisplayFieldsJson = """["epa_id","score"]""",
            OwnerUserId = "admin-1",
            CreatedOn = publishedOn
        };

        activityType.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = ActivityTypeId,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditRulesJson,
            DisplayFieldsJson = activityType.DisplayFieldsJson,
            PublishedByUserId = "admin-1",
            PublishedOn = publishedOn
        });

        db.ActivityTypes.Add(activityType);
        db.SaveChanges();
    }

    private static DbContextOptions<ApplicationDbContext> NewDatabase()
        => new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static ClaimsPrincipal Principal(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));
}
