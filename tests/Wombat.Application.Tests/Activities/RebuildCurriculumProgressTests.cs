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

/// <summary>
/// T119: the rebuild is the only thing in the product that can move an already-stored tally, so wiring
/// the encounter date is what finally makes it necessary to run — and it was not safe to run.
/// </summary>
/// <remarks>
/// <para>
/// The defect these tests exist for: the handler deleted every <c>CurriculumItemProgress</c> row and
/// SAVED, then replayed and saved separately. Anything going wrong in between — a failure, a
/// cancellation, a deadlock — left every trainee in the system on zero progress with no way back. The
/// repair is to zero the rows in place, replay into them, and commit once.
/// </para>
/// <para>
/// The counters are also the second half of T106 item 12: a rebuild that re-credits an activity must
/// refresh the transition's <c>CreditedItemCount</c>, or T108's "this credited nothing" banner survives
/// the exact remediation it recommends.
/// </para>
/// </remarks>
public sealed class RebuildCurriculumProgressTests
{
    private const int CreditingTypeId = 100;
    private const int NonCreditingTypeId = 101;
    private const int CreditedEpaId = 5000;
    private const int UncreditedEpaId = 6000;
    private const int NationalItemId = 4000;
    private const int OrphanItemId = 4001;
    private const int CurriculumId = 3000;

    [Fact]
    public async Task Rebuild_ThatThrowsPartWay_LeavesThePriorRowsIntact()
    {
        // The whole reason this handler was unsafe. trainee-2 already has a stored tally; trainee-1 has
        // none yet, so the replay must ADD a row for them before it reaches the failure. Both halves of
        // the rollback are therefore exercised: the zeroed row has to come back, and the added row has
        // to disappear.
        var options = NewDatabase();

        await using (var seed = new ApplicationDbContext(options))
        {
            Seed(seed);
            AddCompletedActivity(seed, activityId: 200, subjectUserId: "trainee-1", score: 4, daysAgo: 100);
            AddCompletedActivity(seed, activityId: 201, subjectUserId: "trainee-2", score: 4, daysAgo: 50);

            seed.CurriculumItemProgresses.Add(new CurriculumItemProgress
            {
                Id = 900,
                CurriculumItemId = NationalItemId,
                TraineeUserId = "trainee-2",
                CountsSoFar = 7,
                MinimumLevelReachedCount = 5,
                LastActivityId = 201,
                LastUpdated = DateTime.UtcNow.AddDays(-1),
                CreditedActivityKeysJson = """["201:complete"]"""
            });

            await seed.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            // Activity 201 (trainee-2) is replayed second, because the replay runs in filing order.
            var handler = new RebuildCurriculumProgressCommandHandler(
                db, new ThrowsOnCall(new CreditApplier(db), failOnCall: 2));

            var rebuild = async () => await handler.Handle(
                new RebuildCurriculumProgressCommand(Administrator()), CancellationToken.None);

            await rebuild.Should().ThrowAsync<InvalidOperationException>().WithMessage("*induced*");

            // The context must also be left clean: it is scoped to the request, and a half-finished
            // rebuild that left zeroed rows sitting in the change tracker would simply move the defect
            // to whoever calls SaveChanges next.
            await db.SaveChangesAsync();
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            var rows = await verify.CurriculumItemProgresses.ToListAsync();

            rows.Should().ContainSingle("the failed rebuild must not have added trainee-1's row");

            var trainee2 = rows.Single();
            trainee2.TraineeUserId.Should().Be("trainee-2");
            trainee2.CountsSoFar.Should().Be(7);
            trainee2.MinimumLevelReachedCount.Should().Be(5);
            trainee2.CreditedActivityKeysJson.Should().Be("""["201:complete"]""");
        }
    }

    [Fact]
    public async Task Rebuild_StampsCreditedItemCountOnTheTransitionThatCreditedIt()
    {
        // T106 item 12: the activity was completed while the curriculum was wrong, so T108 stamped a
        // zero and ActivityView has been telling the reader to fix the curriculum and rebuild ever
        // since. The rebuild is that remediation; if it does not refresh the stamp, the banner stays on
        // for ever.
        var options = NewDatabase();

        await using (var seed = new ApplicationDbContext(options))
        {
            Seed(seed);
            var activity = AddCompletedActivity(seed, activityId: 200, subjectUserId: "trainee-1", score: 4, daysAgo: 100);
            Completion(activity).CreditedItemCount = 0;
            Completion(activity).CreditScaleMismatchCount = 0;
            await seed.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            var result = await Rebuild(db);

            result.TransitionsStamped.Should().Be(1);
            result.ProgressRowsWritten.Should().Be(1);
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            var transitions = await verify.ActivityTransitions.Where(t => t.ActivityId == 200).ToListAsync();

            transitions.Single(t => t.TransitionKey == "complete").CreditedItemCount.Should().Be(1);
            transitions.Single(t => t.TransitionKey == "complete").CreditScaleMismatchCount.Should().Be(0);

            transitions.Single(t => t.TransitionKey == "submit").CreditedItemCount.Should().BeNull(
                "a non-terminal move never had credit evaluated for it");
        }
    }

    [Fact]
    public async Task Rebuild_LeavesTypesThatCreditNothingByDesignUnstamped()
    {
        // The counts_for gate is checked before the applier is called, exactly as the live path checks
        // it. Reading a zero out of the result instead would flag every reflective note, journal club,
        // procedure log, QI project, research output and teaching session as having credited nothing.
        var options = NewDatabase();

        await using (var seed = new ApplicationDbContext(options))
        {
            Seed(seed);
            AddCompletedActivity(seed, activityId: 210, subjectUserId: "trainee-1", score: 4, daysAgo: 100,
                activityTypeId: NonCreditingTypeId);
            await seed.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            var result = await Rebuild(db);
            result.ActivitiesReplayed.Should().Be(0);
            result.TransitionsStamped.Should().Be(0);
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            (await verify.ActivityTransitions.SingleAsync(t => t.ActivityId == 210 && t.TransitionKey == "complete"))
                .CreditedItemCount.Should().BeNull();
        }
    }

    [Fact]
    public async Task Rebuild_ReproducesTheTalliesTheIncrementalPathProduced()
    {
        // Same activities, same pinned version, same applier: a rebuild is a replay, so it must land on
        // the same numbers the live completions did — including LastActivityId, which is why the replay
        // runs in filing order rather than whatever order the rows come back in.
        var options = NewDatabase();
        Snapshot incremental;

        await using (var db = new ApplicationDbContext(options))
        {
            Seed(db);
            var first = AddCompletedActivity(db, activityId: 200, subjectUserId: "trainee-1", score: 2, daysAgo: 100);
            var second = AddCompletedActivity(db, activityId: 201, subjectUserId: "trainee-1", score: 4, daysAgo: 50);
            await db.SaveChangesAsync();

            // The live path: one credit application per completion, each committed on its own — which is
            // exactly what ActivityService.TransitionAsync does at the end of a terminal move.
            var applier = new CreditApplier(db);
            await applier.ApplyAsync(first, PinnedType(CreditsTheEpa), CancellationToken.None);
            await db.SaveChangesAsync();
            await applier.ApplyAsync(second, PinnedType(CreditsTheEpa), CancellationToken.None);
            await db.SaveChangesAsync();

            incremental = Snapshot.Of(await db.CurriculumItemProgresses.SingleAsync());
            incremental.CountsSoFar.Should().Be(2, "guard: the baseline itself must be the two-completion tally");
            incremental.MinimumLevelReachedCount.Should().Be(1, "guard: only the level-4 completion meets the minimum");
        }

        await using (var db = new ApplicationDbContext(options))
        {
            await Rebuild(db);
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            Snapshot.Of(await verify.CurriculumItemProgresses.SingleAsync()).Should().Be(incremental);
        }
    }

    [Fact]
    public async Task Rebuild_ScopedToOneTrainee_LeavesEveryOtherTraineeUntouched()
    {
        // T119 wants a rebuild confined to the rows whose dates actually moved. trainee-2's row is
        // deliberately wrong; a scoped rebuild must not notice, and a global one must fix it.
        var options = NewDatabase();

        await using (var seed = new ApplicationDbContext(options))
        {
            Seed(seed);
            AddCompletedActivity(seed, activityId: 200, subjectUserId: "trainee-1", score: 4, daysAgo: 100);
            AddCompletedActivity(seed, activityId: 201, subjectUserId: "trainee-2", score: 4, daysAgo: 50);

            seed.CurriculumItemProgresses.Add(new CurriculumItemProgress
            {
                Id = 900,
                CurriculumItemId = NationalItemId,
                TraineeUserId = "trainee-2",
                CountsSoFar = 99,
                MinimumLevelReachedCount = 99,
                LastUpdated = DateTime.UtcNow.AddDays(-1),
                CreditedActivityKeysJson = """["201:complete"]"""
            });

            await seed.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            var result = await Rebuild(db, traineeUserId: "trainee-1");
            result.ActivitiesReplayed.Should().Be(1, "trainee-2's activity must not even be read");
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            (await verify.CurriculumItemProgresses.SingleAsync(row => row.TraineeUserId == "trainee-1"))
                .CountsSoFar.Should().Be(1);
            (await verify.CurriculumItemProgresses.SingleAsync(row => row.TraineeUserId == "trainee-2"))
                .CountsSoFar.Should().Be(99, "a scoped rebuild must leave every other trainee alone");
        }

        await using (var db = new ApplicationDbContext(options))
        {
            await Rebuild(db);
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            (await verify.CurriculumItemProgresses.SingleAsync(row => row.TraineeUserId == "trainee-2"))
                .CountsSoFar.Should().Be(1, "a global rebuild re-scores everybody from the activities");
        }
    }

    [Fact]
    public async Task Rebuild_RemovesAProgressRowNothingReproduces()
    {
        // A rebuild is a rebuild, not a top-up: a row no completion accounts for any more — the activity
        // was cancelled, or re-dated out of the trainee's programme — must go, or the tally is additive
        // for ever.
        var options = NewDatabase();

        await using (var seed = new ApplicationDbContext(options))
        {
            Seed(seed);
            AddCompletedActivity(seed, activityId: 200, subjectUserId: "trainee-1", score: 4, daysAgo: 100);

            seed.CurriculumItemProgresses.Add(new CurriculumItemProgress
            {
                Id = 901,
                CurriculumItemId = OrphanItemId,
                TraineeUserId = "trainee-1",
                CountsSoFar = 3,
                LastUpdated = DateTime.UtcNow.AddDays(-1),
                CreditedActivityKeysJson = """["999:complete"]"""
            });

            await seed.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            var result = await Rebuild(db);
            result.ProgressRowsRemoved.Should().Be(1);
        }

        await using (var verify = new ApplicationDbContext(options))
        {
            var rows = await verify.CurriculumItemProgresses.ToListAsync();
            rows.Should().ContainSingle();
            rows.Single().CurriculumItemId.Should().Be(NationalItemId);
        }
    }

    [Fact]
    public async Task Rebuild_RefusesACallerWhoIsNotAnAdministrator()
    {
        // The command has no entry point yet. When it gets one, the gate is already here rather than
        // only on the page — a rebuild re-scores every trainee in every institution.
        var options = NewDatabase();

        await using var db = new ApplicationDbContext(options);
        Seed(db);
        AddCompletedActivity(db, activityId: 200, subjectUserId: "trainee-1", score: 4, daysAgo: 100);
        await db.SaveChangesAsync();

        var handler = new RebuildCurriculumProgressCommandHandler(db, new CreditApplier(db));

        var rebuild = async () => await handler.Handle(
            new RebuildCurriculumProgressCommand(Principal(WombatRoles.InstitutionalAdmin)), CancellationToken.None);

        await rebuild.Should().ThrowAsync<UnauthorizedAccessException>();
        db.CurriculumItemProgresses.Should().BeEmpty();
    }

    // ─── Harness ─────────────────────────────────────────────────────────────

    private static async Task<RebuildCurriculumProgressResult> Rebuild(
        ApplicationDbContext db, string? traineeUserId = null)
    {
        var handler = new RebuildCurriculumProgressCommandHandler(db, new CreditApplier(db));
        return await handler.Handle(
            new RebuildCurriculumProgressCommand(Administrator(), traineeUserId), CancellationToken.None);
    }

    /// <summary>Everything a replay is supposed to reproduce, compared as one value.</summary>
    private sealed record Snapshot(
        int CurriculumItemId,
        string TraineeUserId,
        int CountsSoFar,
        int MinimumLevelReachedCount,
        int ScaleMismatchCount,
        int UnverifiedLevelCount,
        int? LastActivityId,
        string CreditedActivityKeysJson)
    {
        public static Snapshot Of(CurriculumItemProgress row)
            => new(
                row.CurriculumItemId,
                row.TraineeUserId,
                row.CountsSoFar,
                row.MinimumLevelReachedCount,
                row.ScaleMismatchCount,
                row.UnverifiedLevelCount,
                row.LastActivityId,
                row.CreditedActivityKeysJson);
    }

    /// <summary>Fails the Nth credit application, leaving the earlier ones already written in memory.</summary>
    private sealed class ThrowsOnCall : ICreditApplier
    {
        private readonly ICreditApplier _inner;
        private readonly int _failOnCall;
        private int _calls;

        public ThrowsOnCall(ICreditApplier inner, int failOnCall)
        {
            _inner = inner;
            _failOnCall = failOnCall;
        }

        public async Task<CreditApplicationResult> ApplyAsync(
            Activity completedActivity, ActivityType activityType, CancellationToken cancellationToken = default)
        {
            _calls++;
            if (_calls == _failOnCall)
            {
                throw new InvalidOperationException("induced failure mid-replay");
            }

            return await _inner.ApplyAsync(completedActivity, activityType, cancellationToken);
        }
    }

    private static ActivityTransition Completion(Activity activity)
        => activity.Transitions.Single(transition => transition.TransitionKey == "complete");

    private static DbContextOptions<ApplicationDbContext> NewDatabase()
        => new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static ClaimsPrincipal Administrator() => Principal(WombatRoles.Administrator);

    private static ClaimsPrincipal Principal(string role)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], "test"));

    private static ActivityType PinnedType(string creditRulesJson)
        => new() { CreditRulesJson = creditRulesJson, SchemaJson = SchemaJson };

    private static Activity AddCompletedActivity(
        ApplicationDbContext db,
        int activityId,
        string subjectUserId,
        int score,
        int daysAgo,
        int activityTypeId = CreditingTypeId)
    {
        var filedOn = DateTime.UtcNow.AddDays(-daysAgo);

        var activity = new Activity
        {
            Id = activityId,
            ActivityTypeId = activityTypeId,
            SchemaVersion = 1,
            SubjectUserId = subjectUserId,
            CreatedByUserId = subjectUserId,
            CurrentState = "completed",
            DataJson = $$"""{ "epa_id": {{CreditedEpaId}}, "score": {{score}} }""",
            CreatedOn = filedOn,
            UpdatedOn = filedOn,
            // T119: the encounter date, which is what the stage minimum is selected by. Set alongside
            // CreatedOn rather than left at default, because default(DateOnly) is 0001-01-01 and falls
            // before every programme start.
            ObservedOn = DateOnly.FromDateTime(filedOn)
        };

        activity.Transitions.Add(new ActivityTransition
        {
            ActivityId = activityId,
            FromState = "draft",
            ToState = "submitted",
            TransitionKey = "submit",
            ActorUserId = subjectUserId,
            OccurredOn = filedOn.AddMinutes(-5)
        });

        activity.Transitions.Add(new ActivityTransition
        {
            ActivityId = activityId,
            FromState = "submitted",
            ToState = "completed",
            TransitionKey = "complete",
            ActorUserId = subjectUserId,
            OccurredOn = filedOn
        });

        db.Activities.Add(activity);
        return activity;
    }

    private static void Seed(ApplicationDbContext db)
    {
        db.Epas.Add(new Epa { Id = CreditedEpaId, SubSpecialityId = 1, Code = "EPA-1", Title = "Take a history" });
        db.Epas.Add(new Epa { Id = UncreditedEpaId, SubSpecialityId = 1, Code = "EPA-X", Title = "Nothing files against this" });

        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = NationalItemId,
            CurriculumId = CurriculumId,
            EpaId = CreditedEpaId,
            RequiredCount = 3,
            MinimumLevelOrder = 3,
            WindowMonths = 12
        });

        // Exists so an orphaned progress row has something to point at; no activity ever credits it.
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = OrphanItemId,
            CurriculumId = CurriculumId,
            EpaId = UncreditedEpaId,
            RequiredCount = 1,
            MinimumLevelOrder = 3,
            WindowMonths = 12
        });

        foreach (var (id, userId) in new[] { (1, "trainee-1"), (2, "trainee-2") })
        {
            db.Set<TraineeProfile>().Add(new TraineeProfile
            {
                Id = id,
                UserId = userId,
                InstitutionId = 10,
                CurriculumId = CurriculumId,
                ProgrammeStartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-400),
                ExpectedCompletionDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(2),
                IsActive = true
            });
        }

        db.ActivityTypes.Add(ActivityTypeWith(CreditingTypeId, "wba_under_test", CreditsTheEpa));
        db.ActivityTypes.Add(ActivityTypeWith(NonCreditingTypeId, "reflective_note_under_test", CreditsNothingByDesign));

        db.SaveChanges();
    }

    private static ActivityType ActivityTypeWith(int id, string key, string creditRulesJson)
    {
        var activityType = new ActivityType
        {
            Id = id,
            Key = key,
            Name = key,
            Scope = ActivityScope.Institution,
            ScopeId = 10,
            Version = 1,
            SchemaJson = SchemaJson,
            WorkflowJson = WorkflowJson,
            CreditRulesJson = creditRulesJson,
            DisplayFieldsJson = """["epa_id","score"]""",
            OwnerUserId = "admin-1",
            CreatedOn = DateTime.UtcNow
        };

        activityType.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = id,
            Version = 1,
            SchemaJson = SchemaJson,
            WorkflowJson = WorkflowJson,
            CreditRulesJson = creditRulesJson,
            DisplayFieldsJson = activityType.DisplayFieldsJson,
            PublishedByUserId = "admin-1",
            PublishedOn = DateTime.UtcNow
        });

        return activityType;
    }

    private const string CreditsTheEpa = """
        {
          "counts_for": [
            { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1, "minimum_level_field": "score" }
          ]
        }
        """;

    private const string CreditsNothingByDesign = """{ "counts_for": [] }""";

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
}
