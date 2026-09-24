using System.Security.Claims;
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
/// T108, second half: a completed activity that credited nothing must say so.
/// </summary>
/// <remarks>
/// <para>
/// Before this, <c>CreditApplier</c> returning zero rows on a terminal transition was
/// indistinguishable from success at every surface in the product. The result was discarded, no
/// domain event was raised, and <c>CreditApplier</c> has no logger — so there was not even a line to
/// grep. That is how a registrar reaches the end of a year believing fifty-five encounters were
/// logged.
/// </para>
/// <para>
/// The signal is a three-valued stamp on the transition that caused it: <c>null</c> = never
/// evaluated, <c>0</c> = evaluated and matched nothing, <c>N</c> = credited N items. The
/// <c>counts_for</c> gate is checked BEFORE the credit call, which is what stops it crying wolf on
/// the six seeded activity types that credit nothing by design.
/// </para>
/// </remarks>
public sealed class CreditOutcomeSignalTests
{
    private const int CreditedEpaId = 5000;
    private const int UncreditableEpaId = 6000;

    private const string CreditsAnEpa = """
        {
          "counts_for": [
            { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1 }
          ]
        }
        """;

    private const string CreditsNothingByDesign = """{ "counts_for": [] }""";

    [Fact]
    public async Task ACompletionThatMatchesNoCurriculumItem_IsStampedZero()
    {
        // The T108 scenario exactly: the registrar picked an EPA from a catalogue their institution
        // has not adopted. The activity completes, the data is stored, and nothing counts.
        await using var db = CreateDb();
        Seed(db, CreditsAnEpa);

        var activity = await CompleteAsync(db, UncreditableEpaId);

        db.CurriculumItemProgresses.Should().BeEmpty("this is the bug, not a test artefact");
        Completion(activity).CreditedItemCount.Should().Be(0);
    }

    [Fact]
    public async Task ACompletionThatCredits_IsStampedWithTheCount_AndIsNotFlagged()
    {
        await using var db = CreateDb();
        Seed(db, CreditsAnEpa);

        var activity = await CompleteAsync(db, CreditedEpaId);

        (await db.CurriculumItemProgresses.SingleAsync()).CountsSoFar.Should().Be(1);
        Completion(activity).CreditedItemCount.Should().Be(1);
        activity.Transitions.Should().NotContain(transition => transition.CreditedItemCount == 0);
    }

    [Fact]
    public async Task AnActivityTypeThatCreditsNothingByDesign_IsNeverStamped()
    {
        // A reflective note declares an epa field AND an empty counts_for: it tags an EPA and credits
        // nothing, by design. Stamping it zero would flag six of the fourteen seeded activity types
        // as broken on every single completion.
        await using var db = CreateDb();
        Seed(db, CreditsNothingByDesign);

        var activity = await CompleteAsync(db, UncreditableEpaId);

        Completion(activity).CreditedItemCount.Should().BeNull(
            "credit was never evaluated, so there is nothing to report");
        activity.Transitions.Should().NotContain(transition => transition.CreditedItemCount == 0);
    }

    [Fact]
    public async Task ANonTerminalTransition_IsNeverStamped()
    {
        await using var db = CreateDb();
        Seed(db, CreditsAnEpa);

        var activity = await CompleteAsync(db, CreditedEpaId);

        var submission = activity.Transitions.Single(transition => transition.TransitionKey == "submit");
        submission.CreditedItemCount.Should().BeNull();
    }

    [Fact]
    public async Task ASubjectWithNoTraineeProfile_IsStampedZeroRatherThanSilentlyPassing()
    {
        // CreditApplier returns nothing at all without a profile. A PendingTrainee can reach
        // /activities/new, so this is reachable rather than hypothetical — and "nothing counted" is
        // an honest thing to tell them either way.
        await using var db = CreateDb();
        Seed(db, CreditsAnEpa, seedTraineeProfile: false);

        var activity = await CompleteAsync(db, CreditedEpaId);

        Completion(activity).CreditedItemCount.Should().Be(0);
    }

    private static ActivityTransition Completion(Activity activity)
        => activity.Transitions.Single(transition => transition.TransitionKey == "complete");

    /// <summary>
    /// T158. The EPA was active when the activity was filed and submitted, and deactivated before the assessor
    /// completed it. Credit is judged at the moment of credit, so it counts towards nothing, and the stamp says so:
    /// the T108 warning is how the activity explains it. Reactivating the EPA brings credit back for the next one.
    /// </summary>
    [Fact]
    public async Task ACompletionAfterItsEpaWasDeactivated_CreditsNothing_AndIsStampedZero()
    {
        await using var db = CreateDb();
        Seed(db, CreditsAnEpa);

        var activity = await CompleteAsync(db, CreditedEpaId, beforeCompletion: async () =>
        {
            (await db.Epas.SingleAsync(epa => epa.Id == CreditedEpaId)).IsActive = false;
            await db.SaveChangesAsync();
        });

        db.CurriculumItemProgresses.Should().BeEmpty("the EPA was inactive at the moment of credit");
        Completion(activity).CreditedItemCount.Should().Be(0);

        (await db.Epas.SingleAsync(epa => epa.Id == CreditedEpaId)).IsActive = true;
        await db.SaveChangesAsync();

        var afterReactivation = await CompleteAsync(db, CreditedEpaId);

        Completion(afterReactivation).CreditedItemCount.Should().Be(1);
        (await db.CurriculumItemProgresses.SingleAsync()).CountsSoFar.Should().Be(1,
            "the completion made while the EPA was inactive is not credited retroactively; only a rebuild replays it");
    }

    /// <summary>Creates a draft, submits it, and completes it — the whole live path.</summary>
    private static async Task<Activity> CompleteAsync(ApplicationDbContext db, int epaId, Func<Task>? beforeCompletion = null)
    {
        var service = new ActivityService(
            db,
            new SchemaValidator(),
            new WorkflowEvaluator(),
            new CreditApplier(db),
            new FieldPermissionEvaluator());

        var principal = Principal("trainee-1");

        var draft = await service.CreateDraftAsync(
            new CreateActivityInput(100, "trainee-1", "trainee-1", $$"""{ "epa_id": {{epaId}}, "score": 4 }""", principal),
            CancellationToken.None);

        await service.TransitionAsync(
            new TransitionActivityInput(draft.Id, "submit", "trainee-1", principal, null, null),
            CancellationToken.None);

        if (beforeCompletion is not null)
        {
            await beforeCompletion();
        }

        await service.TransitionAsync(
            new TransitionActivityInput(draft.Id, "complete", "trainee-1", principal, null, null),
            CancellationToken.None);

        return await db.Activities
            .Include(activity => activity.Transitions)
            .SingleAsync(activity => activity.Id == draft.Id);
    }

    private static void Seed(ApplicationDbContext db, string creditRulesJson, bool seedTraineeProfile = true)
    {
        db.Epas.Add(new Epa { Id = CreditedEpaId, SubSpecialityId = 1, Code = "EPA-1", Title = "Take a history" });
        db.Epas.Add(new Epa { Id = UncreditableEpaId, SubSpecialityId = 1, Code = "EPA-X", Title = "From an unadopted catalogue" });

        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 4000,
            CurriculumId = 3000,
            EpaId = CreditedEpaId,
            RequiredCount = 1,
            MinimumLevelOrder = 3,
            WindowMonths = 12
        });

        if (seedTraineeProfile)
        {
            db.Set<TraineeProfile>().Add(new TraineeProfile
            {
                Id = 1,
                UserId = "trainee-1",
                InstitutionId = 10,
                CurriculumId = 3000,
                ProgrammeStartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-1),
                ExpectedCompletionDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(2),
                IsActive = true
            });
        }

        const string schemaJson = """
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

        var activityType = new ActivityType
        {
            Id = 100,
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
            CreatedOn = DateTime.UtcNow
        };

        activityType.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = 100,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditRulesJson,
            DisplayFieldsJson = activityType.DisplayFieldsJson,
            PublishedByUserId = "admin-1",
            PublishedOn = DateTime.UtcNow
        });

        db.ActivityTypes.Add(activityType);
        db.SaveChanges();
    }

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ClaimsPrincipal Principal(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));
}
