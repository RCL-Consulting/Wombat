using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Commands.CreateActivity;
using Wombat.Application.Features.Activities.Commands.TransitionActivity;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityById;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T070 end to end through ActivityService: the assessor supplies the rating on the transition that
/// completes the activity, the merge refuses to write fields the actor does not own, and creation
/// drops fields the creator does not own.
/// </summary>
public sealed class ActivityFieldPermissionTests
{
    /// <summary>
    /// THE VERIFICATION CRITERION. The trainee pre-filled level 2 against an item needing 4; the
    /// assessor patches 5 on complete. Credit must grade the assessor value, which only happens if
    /// the patch merges before CreditApplier reads the post-merge data.
    /// </summary>
    [Fact]
    public async Task Transition_AssessorPatchesAPassingLevelOnComplete_CreditsTheAssessorAdjustedValue()
    {
        await using var dbContext = CreateContext();
        var activityService = CreateService(dbContext);
        SeedRequestedActivity(dbContext, storedOverallLevel: 2);

        var completed = await activityService.TransitionAsync(new TransitionActivityInput(
            700,
            "complete",
            "assessor-1",
            CreatePrincipal("assessor-1"),
            """{ "overall_level": 5, "strengths": "Clear structured history." }""",
            "Ready for indirect supervision."));

        completed.CurrentState.Should().Be("completed");

        var progress = await dbContext.CurriculumItemProgresses.SingleAsync();
        progress.CountsSoFar.Should().Be(1);
        progress.MinimumLevelReachedCount.Should().Be(1);
    }

    [Fact]
    public async Task Transition_AssessorPatchesALevelBelowTheMinimum_CountsButDoesNotReachTheMinimum()
    {
        await using var dbContext = CreateContext();
        var activityService = CreateService(dbContext);
        SeedRequestedActivity(dbContext, storedOverallLevel: 5);

        await activityService.TransitionAsync(new TransitionActivityInput(
            700,
            "complete",
            "assessor-1",
            CreatePrincipal("assessor-1"),
            """{ "overall_level": 2, "strengths": "Needs prompting." }""",
            null));

        var progress = await dbContext.CurriculumItemProgresses.SingleAsync();
        progress.CountsSoFar.Should().Be(1);
        progress.MinimumLevelReachedCount.Should().Be(0);
    }

    /// <summary>
    /// The escalation the write gate exists to stop: a complete patch that also moves epa_id would
    /// redirect which curriculum item gets credited.
    /// </summary>
    [Fact]
    public async Task Transition_PatchThatAlsoChangesAFieldTheActorDoesNotOwn_ThrowsAndChangesNothing()
    {
        await using var dbContext = CreateContext();
        var activityService = CreateService(dbContext);
        SeedRequestedActivity(dbContext, storedOverallLevel: 2);

        var act = async () => await activityService.TransitionAsync(new TransitionActivityInput(
            700,
            "complete",
            "assessor-1",
            CreatePrincipal("assessor-1"),
            """{ "overall_level": 5, "strengths": "Clear history.", "epa_id": 5001 }""",
            null));

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*epa_id*");

        var persisted = await dbContext.Activities.SingleAsync();
        persisted.CurrentState.Should().Be("requested");
        ReadInt(persisted.DataJson, "epa_id").Should().Be(5000);
        ReadInt(persisted.DataJson, "overall_level").Should().Be(2);
        dbContext.CurriculumItemProgresses.Should().BeEmpty();
    }

    /// <summary>
    /// The DeepEquals carve-out. A full-form post-back echoes every field the actor can see,
    /// including the locked ones. Echoing a locked field unchanged must not be an error, or the
    /// write gate is a landmine for the page that builds the patch.
    /// </summary>
    [Fact]
    public async Task Transition_PatchEchoingAnUnownedFieldUnchanged_Succeeds()
    {
        await using var dbContext = CreateContext();
        var activityService = CreateService(dbContext);
        SeedRequestedActivity(dbContext, storedOverallLevel: 2);

        var completed = await activityService.TransitionAsync(new TransitionActivityInput(
            700,
            "complete",
            "assessor-1",
            CreatePrincipal("assessor-1"),
            """
            {
              "epa_id": 5000,
              "assessor_user_id": "assessor-1",
              "overall_level": 5,
              "strengths": "Clear history."
            }
            """,
            null));

        completed.CurrentState.Should().Be("completed");
        ReadInt(completed.DataJson, "overall_level").Should().Be(5);
    }

    /// <summary>
    /// Authorization runs before the merge: a patch cannot authorise its own transition. The subject
    /// is not the bound assessor, so the transition is refused with no write of any kind.
    /// </summary>
    [Fact]
    public async Task Transition_WithAValidPatchFromTheSubject_IsDeniedBeforeAnyMerge()
    {
        await using var dbContext = CreateContext();
        var activityService = CreateService(dbContext);
        SeedRequestedActivity(dbContext, storedOverallLevel: 2);

        var act = async () => await activityService.TransitionAsync(new TransitionActivityInput(
            700,
            "complete",
            "trainee-1",
            CreatePrincipal("trainee-1"),
            """{ "overall_level": 5, "strengths": "I did well." }""",
            null));

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*not allowed to perform this transition*");

        var persisted = await dbContext.Activities.SingleAsync();
        persisted.CurrentState.Should().Be("requested");
        ReadInt(persisted.DataJson, "overall_level").Should().Be(2);
    }

    [Fact]
    public async Task Transition_PersistsTheMergedSnapshotAndTheNote()
    {
        await using var dbContext = CreateContext();
        var activityService = CreateService(dbContext);
        SeedRequestedActivity(dbContext, storedOverallLevel: 2);

        await activityService.TransitionAsync(new TransitionActivityInput(
            700,
            "complete",
            "assessor-1",
            CreatePrincipal("assessor-1"),
            """{ "overall_level": 5, "strengths": "Clear history." }""",
            "Discussed with the trainee."));

        var transition = await dbContext.Set<ActivityTransition>()
            .SingleAsync(entity => entity.TransitionKey == "complete");

        ReadInt(transition.SnapshotJson, "overall_level").Should().Be(5);
        transition.SnapshotJson.Should().Contain("Clear history.");
        transition.Note.Should().Be("Discussed with the trainee.");
    }

    /// <summary>
    /// Closes "the trainee pre-fills the assessor rating": before T070 a creator could submit the
    /// whole schema, making complete satisfiable before the assessor ever saw the form.
    /// </summary>
    [Fact]
    public async Task Create_DropsTheFieldsTheCreatorDoesNotOwn()
    {
        await using var dbContext = CreateContext();
        var activityService = CreateService(dbContext);
        SeedActivityType(dbContext);
        dbContext.SaveChanges();

        IRequestHandler<CreateActivityCommand, ActivityDto> createHandler =
            new CreateActivityCommandHandler(activityService);

        var created = await createHandler.Handle(
            new CreateActivityCommand(
                600,
                "trainee-1",
                "trainee-1",
                """
                {
                  "epa_id": 5000,
                  "assessor_user_id": "assessor-1",
                  "case_summary": "Febrile infant.",
                  "overall_level": 5,
                  "strengths": "Self-assessed as excellent."
                }
                """,
                CreatePrincipal("trainee-1")),
            CancellationToken.None);

        created.DataJson.Should().Contain("epa_id");
        created.DataJson.Should().Contain("case_summary");
        created.DataJson.Should().NotContain("overall_level");
        created.DataJson.Should().NotContain("strengths");
    }

    [Fact]
    public async Task GetDetail_ReturnsTheWritableSetAndActionsForTheBoundAssessorAndNothingForAnyoneElse()
    {
        await using var dbContext = CreateContext();
        var activityService = CreateService(dbContext);
        SeedRequestedActivity(dbContext, storedOverallLevel: 2);

        IRequestHandler<GetActivityByIdQuery, ActivityDetailDto> getHandler =
            new GetActivityByIdQueryHandler(activityService);

        var assessorView = await getHandler.Handle(
            new GetActivityByIdQuery(700, CreatePrincipal("assessor-1")),
            CancellationToken.None);

        assessorView.EditableFieldKeys.Should().BeEquivalentTo(["overall_level", "strengths", "improvements", "plan"]);
        assessorView.AvailableActions.Select(action => action.TransitionKey).Should().Contain("complete");

        var strangerView = await getHandler.Handle(
            new GetActivityByIdQuery(700, CreatePrincipal("stranger-1")),
            CancellationToken.None);

        strangerView.EditableFieldKeys.Should().BeEmpty();
        strangerView.AvailableActions.Should().BeEmpty();
        strangerView.Activity.CurrentState.Should().Be("requested");
    }

    // ─── Fixtures ────────────────────────────────────────────────────────────

    /// <summary>
    /// The create-time ordering hole. A <c>field:</c> rule reads its answer out of DataJson, so if
    /// the writable set is computed against what the caller just submitted, the caller can grant
    /// themselves the rule. Here the CREATOR is not the subject (so the subject guard below does
    /// not fire) and names themself as the assessor: the assessor-owned fields must still be
    /// dropped, because at creation nobody is bound by a data field yet.
    /// </summary>
    [Fact]
    public async Task Create_CreatorNamesThemselfAsTheAssessor_StillDropsTheAssessorOwnedFields()
    {
        await using var dbContext = CreateContext();
        var activityService = CreateService(dbContext);
        SeedActivityType(dbContext);
        dbContext.SaveChanges();

        var created = await activityService.CreateDraftAsync(new CreateActivityInput(
            600,
            "trainee-1",
            "coordinator-1",
            """
            {
              "epa_id": 5000,
              "assessor_user_id": "coordinator-1",
              "case_summary": "Febrile infant.",
              "overall_level": 6,
              "strengths": "Rated by the person filing the form."
            }
            """,
            CreatePrincipal("coordinator-1")));

        created.DataJson.Should().Contain("assessor_user_id");
        created.DataJson.Should().NotContain("overall_level");
        created.DataJson.Should().NotContain("strengths");
    }

    /// <summary>
    /// The self-assessment escalation (the narrow half of T102). A subject who names themself as
    /// the assessor would match every <c>field:assessor_user_id</c> rule from the next state on —
    /// rating themself and taking their own <c>complete</c>, awarding their own credit.
    /// </summary>
    [Fact]
    public async Task Create_SubjectNamesThemselfAsTheAssessor_IsRejected()
    {
        await using var dbContext = CreateContext();
        var activityService = CreateService(dbContext);
        SeedActivityType(dbContext);
        dbContext.SaveChanges();

        var create = async () => await activityService.CreateDraftAsync(new CreateActivityInput(
            600,
            "trainee-1",
            "trainee-1",
            """
            {
              "epa_id": 5000,
              "assessor_user_id": "trainee-1",
              "case_summary": "Febrile infant."
            }
            """,
            CreatePrincipal("trainee-1")));

        await create.Should().ThrowAsync<InvalidOperationException>()
            .Where(exception => exception.Message.Contains("cannot name the person the activity is about"));

        dbContext.Activities.Should().BeEmpty();
    }

    /// <summary>
    /// The same escalation by the other route. The subject legitimately owns assessor_user_id while
    /// the request is still theirs to edit, so the merge itself will accept the patch — the guard
    /// has to run after it.
    /// </summary>
    [Fact]
    public async Task Transition_SubjectRetargetsTheAssessorFieldToThemself_IsRejected()
    {
        await using var dbContext = CreateContext();
        var activityService = CreateService(dbContext);
        SeedDraftActivity(dbContext);

        var submit = async () => await activityService.TransitionAsync(new TransitionActivityInput(
            701,
            "submit",
            "trainee-1",
            CreatePrincipal("trainee-1"),
            """{ "assessor_user_id": "trainee-1" }""",
            null));

        await submit.Should().ThrowAsync<InvalidOperationException>()
            .Where(exception => exception.Message.Contains("cannot name the person the activity is about"));

        var unchanged = await dbContext.Activities.SingleAsync();
        unchanged.CurrentState.Should().Be("draft");
        unchanged.DataJson.Should().Contain("assessor-1");
    }

    private static ApplicationDbContext CreateContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ActivityService CreateService(ApplicationDbContext dbContext)
        => new(
            dbContext,
            new SchemaValidator(),
            new WorkflowEvaluator(),
            new CreditApplier(dbContext),
            new FieldPermissionEvaluator());

    private static void SeedRequestedActivity(ApplicationDbContext dbContext, int storedOverallLevel)
    {
        SeedActivityType(dbContext);

        dbContext.Epas.Add(new Epa
        {
            Id = 5000,
            Code = "EPA-1",
            Title = "Assess an acutely unwell child"
        });

        dbContext.CurriculumItems.Add(new CurriculumItem
        {
            Id = 4000,
            CurriculumId = 3000,
            EpaId = 5000,
            RequiredCount = 3,
            MinimumLevelOrder = 4,
            WindowMonths = 12
        });

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

        var utcNow = DateTime.UtcNow;
        dbContext.Activities.Add(new Activity
        {
            Id = 700,
            ActivityTypeId = 600,
            SchemaVersion = 1,
            SubjectUserId = "trainee-1",
            CreatedByUserId = "trainee-1",
            CurrentState = "requested",
            DataJson = $$"""
                {
                  "epa_id": 5000,
                  "assessor_user_id": "assessor-1",
                  "case_summary": "Febrile infant in the emergency unit.",
                  "overall_level": {{storedOverallLevel}}
                }
                """,
            CreatedOn = utcNow,
            UpdatedOn = utcNow
        });

        dbContext.SaveChanges();
    }

    private static void SeedDraftActivity(ApplicationDbContext dbContext)
    {
        SeedActivityType(dbContext);

        var utcNow = DateTime.UtcNow;
        dbContext.Activities.Add(new Activity
        {
            Id = 701,
            ActivityTypeId = 600,
            SchemaVersion = 1,
            SubjectUserId = "trainee-1",
            CreatedByUserId = "trainee-1",
            CurrentState = "draft",
            DataJson = """
                {
                  "epa_id": 5000,
                  "assessor_user_id": "assessor-1",
                  "case_summary": "Febrile infant in the emergency unit."
                }
                """,
            CreatedOn = utcNow,
            UpdatedOn = utcNow
        });

        dbContext.SaveChanges();
    }

    private static void SeedActivityType(ApplicationDbContext dbContext)
    {
        var activityType = new ActivityType
        {
            Id = 600,
            Key = "mini_cex_cpsa",
            Name = "Mini-CEX (CPSA)",
            Scope = ActivityScope.Institution,
            ScopeId = 10,
            Version = 1,
            SchemaJson = SchemaJson,
            WorkflowJson = WorkflowJson,
            CreditRulesJson = CreditRulesJson,
            DisplayFieldsJson = """["epa_id","overall_level"]""",
            OwnerUserId = "admin-1",
            CreatedOn = DateTime.UtcNow
        };

        activityType.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = 600,
            Version = 1,
            SchemaJson = SchemaJson,
            WorkflowJson = WorkflowJson,
            CreditRulesJson = CreditRulesJson,
            DisplayFieldsJson = activityType.DisplayFieldsJson,
            PublishedByUserId = "admin-1",
            PublishedOn = DateTime.UtcNow
        });

        dbContext.ActivityTypes.Add(activityType);
    }

    private const string SchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "epa_id", "type": "epa", "label": "EPA", "required": true },
                { "key": "assessor_user_id", "type": "user", "label": "Assessor", "required": true },
                { "key": "case_summary", "type": "longtext", "label": "Case summary" }
              ]
            },
            {
              "key": "assessment",
              "title": "Entrustment",
              "editable_by": "field:assessor_user_id",
              "fields": [
                { "key": "overall_level", "type": "number", "label": "Overall level" }
              ]
            },
            {
              "key": "feedback",
              "title": "Feedback",
              "editable_by": "field:assessor_user_id",
              "fields": [
                { "key": "strengths", "type": "longtext", "label": "Strengths" },
                { "key": "improvements", "type": "longtext", "label": "Improvements" },
                { "key": "plan", "type": "longtext", "label": "Plan" }
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
            { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator" },
            {
              "key": "complete",
              "from": "requested",
              "to": "completed",
              "actor": "field:assessor_user_id",
              "requires_fields": ["overall_level", "strengths"]
            }
          ]
        }
        """;

    private const string CreditRulesJson = """
        {
          "counts_for": [
            {
              "curriculum_item_match": { "epa_field": "epa_id" },
              "amount": 1,
              "minimum_level_field": "overall_level"
            }
          ]
        }
        """;

    private static int ReadInt(string json, string propertyName)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty(propertyName).GetInt32();
    }

    private static ClaimsPrincipal CreatePrincipal(string userId)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(WombatClaimTypes.InstitutionId, "10")
            ],
            "test"));
}
