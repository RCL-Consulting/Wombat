using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T107 (D33 part 1): an action the actor is allowed to take but cannot complete from the page is listed disabled, with
/// the fields it needs, instead of offered as a button that can only be refused. Driven through
/// <c>ActivityService.GetDetailAsync</c> over EF in-memory, as the other ActivityService tests are.
/// </summary>
/// <remarks>
/// The predicate: the transition's own validation (<c>validation</c> scope plus <c>requires_fields</c>) against the
/// STORED data under the PINNED version, minus the fields this actor may write in the current state. The motivating
/// case is an activity pinned to a version whose assessor fields its assessor could not write (T103 pins; nothing
/// re-pins).
/// </remarks>
public sealed class UnavailableActionTests
{
    private const string TraineeId = "trainee-1";
    private const string AssessorId = "assessor-1";

    private const int StrandableTypeId = 400;
    private const int CountersignedTypeId = 401;
    private const int HidableTypeId = 403;

    private const string RequestOnly = """{ "assessor_user_id": "assessor-1", "case_summary": "Wheeze on the ward." }""";

    [Fact]
    public async Task OnAnActivityPinnedToTheOldVersion_CompleteIsListedDisabled_NamingTheFieldsTheAssessorCannotWrite()
    {
        var options = await SeededAsync();
        await AddActivityAsync(options, 900, StrandableTypeId, schemaVersion: 1, RequestOnly);

        var complete = Action(await DetailAsync(options, 900, AssessorId), "complete");

        complete.IsAvailable.Should().BeFalse();
        complete.UnavailableReason.Should().Be(
            "Needs Overall level and Strengths, which you cannot fill in here. " +
            "This activity was filed on version 1 of the form; the current version is 2.");
    }

    [Fact]
    public async Task AFieldHiddenByItsCondition_IsNotNamed_AndIsOnceItsConditionHolds()
    {
        // The validator's own visibility decides what is required: `escalation_detail` is required, unwritable here, and
        // shown only when `escalated` is "yes".
        var options = await SeededAsync();
        await AddActivityAsync(options, 900, StrandableTypeId, schemaVersion: 1, RequestOnly);
        await AddActivityAsync(options, 901, StrandableTypeId, schemaVersion: 1,
            """{ "assessor_user_id": "assessor-1", "case_summary": "Wheeze on the ward.", "escalated": "yes" }""");

        Action(await DetailAsync(options, 900, AssessorId), "complete")
            .UnavailableReason.Should().NotContain("Escalation detail");
        Action(await DetailAsync(options, 901, AssessorId), "complete")
            .UnavailableReason.Should().StartWith("Needs Overall level, Strengths and Escalation detail, which you cannot");
    }

    [Fact]
    public async Task AFlaggedFieldTheActorsOwnMoveCanHide_IsNotNamed_BecauseTheServerAcceptsThatMove()
    {
        // The review section (by its own show_if) and the countersignature (by the field's) are required, shown by the
        // stored data and writable only by a Coordinator. But the conditions read fields the trainee writes, so the
        // trainee's submit can hide them, and the server, validating the merged data, then accepts it. Marking submit
        // unavailable here would take away a move that works, and leave the page read-only.
        var options = await SeededAsync();
        await AddActivityAsync(options, 910, HidableTypeId, schemaVersion: 1,
            """{ "case_summary": "Wheeze on the ward.", "needs_review": "yes", "needs_countersign": "no" }""", state: "draft");
        await AddActivityAsync(options, 911, HidableTypeId, schemaVersion: 1,
            """{ "case_summary": "Wheeze on the ward.", "needs_review": "no", "needs_countersign": "yes" }""", state: "draft");

        Action(await DetailAsync(options, 910, TraineeId), "submit").UnavailableReason.Should().BeNull();
        Action(await DetailAsync(options, 911, TraineeId), "submit").UnavailableReason.Should().BeNull();

        // Not vacuous: against the stored data the validator does flag them, and the move that hides them is accepted.
        var unpatched = () => TransitionAsync(options, 910, "submit", TraineeId);
        (await unpatched.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Contain("review_note");
        (await TransitionAsync(options, 910, "submit", TraineeId, dataPatchJson: """{ "needs_review": "no" }"""))
            .CurrentState.Should().Be("submitted");
        (await TransitionAsync(options, 911, "submit", TraineeId, dataPatchJson: """{ "needs_countersign": "no" }"""))
            .CurrentState.Should().Be("submitted");
    }

    [Fact]
    public async Task TheSameTypesCurrentVersion_OffersComplete_ThoughTheRatingsAreStillEmpty()
    {
        // The flagged fields are the assessor's to fill in with the move, so they do not make it unavailable.
        var options = await SeededAsync();
        await AddActivityAsync(options, 902, StrandableTypeId, schemaVersion: 2, RequestOnly);

        var detail = await DetailAsync(options, 902, AssessorId);

        detail.EditableFieldKeys.Should().Contain(["overall_level", "strengths"]);
        Action(detail, "complete").UnavailableReason.Should().BeNull();
    }

    [Fact]
    public async Task OnlyTheFlaggedFieldsTheActorCannotWrite_AreNamed_WithNoVersionSentenceOnTheCurrentVersion()
    {
        var options = await SeededAsync();
        await AddActivityAsync(options, 903, CountersignedTypeId, schemaVersion: 1, RequestOnly);

        Action(await DetailAsync(options, 903, AssessorId), "complete")
            .UnavailableReason.Should().Be("Needs Countersignature, which you cannot fill in here.");
    }

    [Fact]
    public async Task OnTheStrandedActivity_DraftValidatedDeclineAndCancel_StayAvailable()
    {
        var options = await SeededAsync();
        await AddActivityAsync(options, 900, StrandableTypeId, schemaVersion: 1, RequestOnly);

        var assessorView = await DetailAsync(options, 900, AssessorId);
        assessorView.AvailableActions.Select(action => action.TransitionKey).Should().BeEquivalentTo(["complete", "decline"]);
        Action(assessorView, "decline").IsAvailable.Should().BeTrue();

        var traineeView = await DetailAsync(options, 900, TraineeId);
        traineeView.AvailableActions.Select(action => action.TransitionKey).Should().Equal("cancel");
        Action(traineeView, "cancel").IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task TheServerStillRefusesTheMove_AndTheStrandedActivityCanBeDeclined()
    {
        var options = await SeededAsync();
        await AddActivityAsync(options, 900, StrandableTypeId, schemaVersion: 1, RequestOnly);

        var complete = () => TransitionAsync(options, 900, "complete", AssessorId);
        (await complete.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Contain("overall_level");

        (await TransitionAsync(options, 900, "decline", AssessorId, note: "Filed on a form I cannot complete."))
            .CurrentState.Should().Be("declined");
    }

    [Fact]
    public async Task ANewCpsaMiniCex_FromTheShippedSeed_OffersEveryActionAtEveryStep()
    {
        // T107's third Verification item: an activity pinned to the current version is unaffected. A half-filled draft
        // still offers submit (what is missing is the trainee's to fill in with it), and the assessor's complete is
        // offered before any rating exists, because every rating is theirs to write.
        var options = await SeededAsync();
        await using (var db = new ApplicationDbContext(options))
        {
            NomineeSeed.AddUser(db, TraineeId, 10, WombatRoles.Trainee);
            NomineeSeed.AddUser(db, AssessorId, 10, WombatRoles.Assessor);
            db.ActivityTypes.Add(FromSeed(402, "mini_cex_cpsa"));
            await db.SaveChangesAsync();
        }

        ActivityDto draft;
        await using (var db = new ApplicationDbContext(options))
        {
            draft = await Service(db).CreateDraftAsync(new CreateActivityInput(
                402, TraineeId, TraineeId, """{ "presenting_problem": "Wheeze" }""", Principal(TraineeId)));
        }

        (await DetailAsync(options, draft.Id, TraineeId)).AvailableActions.Should().OnlyContain(action => action.IsAvailable);

        await using (var db = new ApplicationDbContext(options))
        {
            await Service(db).TransitionAsync(new TransitionActivityInput(
                draft.Id,
                "submit",
                TraineeId,
                Principal(TraineeId),
                """
                {
                  "epa_id": 2, "assessor_user_id": "assessor-1", "observed_on": "2026-03-10",
                  "setting": "ward", "complexity": "moderate"
                }
                """,
                null));
        }

        var assessorView = await DetailAsync(options, draft.Id, AssessorId);
        assessorView.AvailableActions.Select(action => action.TransitionKey).Should().Contain("complete");
        assessorView.AvailableActions.Should().OnlyContain(action => action.IsAvailable);
        (await DetailAsync(options, draft.Id, TraineeId)).AvailableActions.Should().OnlyContain(action => action.IsAvailable);
    }

    // ---- helpers -------------------------------------------------------------------------------------------------

    private static ActivityType FromSeed(int id, string seedKey)
    {
        string Read(string file) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", seedKey, file));
        var type = new ActivityType
        {
            Id = id,
            Key = seedKey,
            Name = seedKey,
            Scope = ActivityScope.Global,
            Version = 1,
            SchemaJson = Read("schema.json"),
            WorkflowJson = Read("workflow.json"),
            CreditRulesJson = Read("credit.json"),
            DisplayFieldsJson = "[]",
            OwnerUserId = "system",
            CreatedOn = DateTime.UtcNow
        };

        type.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = id,
            Version = 1,
            SchemaJson = type.SchemaJson,
            WorkflowJson = type.WorkflowJson,
            CreditRulesJson = type.CreditRulesJson,
            DisplayFieldsJson = "[]",
            PublishedByUserId = "system",
            PublishedOn = DateTime.UtcNow
        });

        return type;
    }

    private static ActivityActionDto Action(ActivityDetailDto detail, string transitionKey)
        => detail.AvailableActions.Should().ContainSingle(action => action.TransitionKey == transitionKey).Which;

    private static async Task<ActivityDetailDto> DetailAsync(DbContextOptions<ApplicationDbContext> options, int activityId, string userId)
    {
        await using var db = new ApplicationDbContext(options);
        var detail = await Service(db).GetDetailAsync(activityId, Principal(userId));
        detail.Should().NotBeNull();
        return detail!;
    }

    private static async Task<ActivityDto> TransitionAsync(
        DbContextOptions<ApplicationDbContext> options,
        int activityId,
        string transitionKey,
        string actorUserId,
        string? note = null,
        string? dataPatchJson = null)
    {
        await using var db = new ApplicationDbContext(options);
        return await Service(db).TransitionAsync(new TransitionActivityInput(
            activityId, transitionKey, actorUserId, Principal(actorUserId), dataPatchJson, note));
    }

    private static async Task AddActivityAsync(
        DbContextOptions<ApplicationDbContext> options,
        int id,
        int typeId,
        int schemaVersion,
        string dataJson,
        string state = "requested")
    {
        await using var db = new ApplicationDbContext(options);
        var utcNow = DateTime.UtcNow;
        db.Activities.Add(new Activity
        {
            Id = id,
            ActivityTypeId = typeId,
            SchemaVersion = schemaVersion,
            SubjectUserId = TraineeId,
            CreatedByUserId = TraineeId,
            CurrentState = state,
            DataJson = dataJson,
            CreatedOn = utcNow,
            UpdatedOn = utcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task<DbContextOptions<ApplicationDbContext>> SeededAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);

        // Version 1 left the assessment section on the subject|creator default, so in `requested` (writable only by the
        // named assessor) nobody can write the ratings. Version 2 gives them to the assessor. The type is on version 2.
        db.ActivityTypes.Add(Type(
            StrandableTypeId,
            "strandable_under_test",
            [StrandableSchema(assessmentEditableBy: null), StrandableSchema(assessmentEditableBy: "field:assessor_user_id")]));

        // One version, current: the assessor owns the ratings but not the countersignature, which `all` still requires.
        db.ActivityTypes.Add(Type(
            CountersignedTypeId,
            "countersigned_under_test",
            [CountersignedSchema]));

        // One version: required Coordinator-only fields whose conditions read fields the trainee writes.
        db.ActivityTypes.Add(Type(HidableTypeId, "hidable_under_test", [HidableSchema], HidableWorkflowJson));

        await db.SaveChangesAsync();
        return options;
    }

    private const string HidableSchema = """
        {
          "version": 1,
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "case_summary", "type": "longtext", "label": "Case summary", "required": true },
                { "key": "needs_review", "type": "choice", "label": "Needs review", "options": ["yes", "no"] },
                { "key": "needs_countersign", "type": "choice", "label": "Needs countersign", "options": ["yes", "no"] }
              ]
            },
            {
              "key": "review",
              "title": "Review",
              "editable_by": "role:Coordinator",
              "show_if": { "field": "needs_review", "operator": "equals", "value": "yes" },
              "fields": [
                { "key": "review_note", "type": "longtext", "label": "Review note", "required": true }
              ]
            },
            {
              "key": "countersign",
              "title": "Countersign",
              "editable_by": "role:Coordinator",
              "fields": [
                {
                  "key": "countersignature",
                  "type": "text",
                  "label": "Countersignature",
                  "required": true,
                  "show_if": { "field": "needs_countersign", "operator": "equals", "value": "yes" }
                }
              ]
            }
          ]
        }
        """;

    private const string HidableWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "submitted", "label": "Submitted" }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject", "validation": "all" }
          ]
        }
        """;

    private static string StrandableSchema(string? assessmentEditableBy)
        => $$"""
            {
              "version": 1,
              "sections": [
                {
                  "key": "request",
                  "title": "Request",
                  "fields": [
                    { "key": "assessor_user_id", "type": "user", "label": "Assessor", "required": true },
                    { "key": "case_summary", "type": "longtext", "label": "Case summary", "required": true },
                    { "key": "escalated", "type": "choice", "label": "Escalated", "options": ["yes", "no"] }
                  ]
                },
                {
                  "key": "assessment",
                  "title": "Assessment",
                  {{(assessmentEditableBy is null ? string.Empty : $"\"editable_by\": \"{assessmentEditableBy}\",")}}
                  "fields": [
                    { "key": "overall_level", "type": "number", "label": "Overall level", "required": true },
                    { "key": "strengths", "type": "longtext", "label": "Strengths", "required": true },
                    {
                      "key": "escalation_detail",
                      "type": "longtext",
                      "label": "Escalation detail",
                      "required": true,
                      "show_if": { "field": "escalated", "operator": "equals", "value": "yes" }
                    }
                  ]
                }
              ]
            }
            """;

    private const string CountersignedSchema = """
        {
          "version": 1,
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "assessor_user_id", "type": "user", "label": "Assessor", "required": true },
                { "key": "case_summary", "type": "longtext", "label": "Case summary", "required": true }
              ]
            },
            {
              "key": "assessment",
              "title": "Assessment",
              "editable_by": "field:assessor_user_id",
              "fields": [
                { "key": "overall_level", "type": "number", "label": "Overall level", "required": true },
                { "key": "strengths", "type": "longtext", "label": "Strengths", "required": true }
              ]
            },
            {
              "key": "countersign",
              "title": "Countersign",
              "fields": [
                { "key": "countersignature", "type": "text", "label": "Countersignature", "required": true }
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
            { "key": "completed", "label": "Completed", "terminal": true },
            { "key": "declined", "label": "Declined" },
            { "key": "cancelled", "label": "Cancelled" }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject", "validation": "owned" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id", "validation": "all" },
            {
              "key": "decline",
              "from": "requested",
              "to": "declined",
              "actor": "field:assessor_user_id",
              "requires_note": true,
              "validation": "draft"
            },
            { "key": "cancel", "from": "requested", "to": "cancelled", "actor": "subject", "validation": "draft" }
          ]
        }
        """;

    private const string CreditRulesJson = """{ "counts_for": [] }""";

    /// <summary>A type on its last version, with every version before it still published for pinned activities.</summary>
    private static ActivityType Type(
        int id,
        string key,
        IReadOnlyList<string> schemaJsonByVersion,
        string workflowJson = WorkflowJson)
    {
        var type = new ActivityType
        {
            Id = id,
            Key = key,
            Name = key,
            Scope = ActivityScope.Global,
            Version = schemaJsonByVersion.Count,
            SchemaJson = schemaJsonByVersion[^1],
            WorkflowJson = workflowJson,
            CreditRulesJson = CreditRulesJson,
            DisplayFieldsJson = "[]",
            OwnerUserId = "system",
            CreatedOn = DateTime.UtcNow
        };

        for (var index = 0; index < schemaJsonByVersion.Count; index++)
        {
            type.Versions.Add(new ActivityTypeVersion
            {
                ActivityTypeId = id,
                Version = index + 1,
                SchemaJson = schemaJsonByVersion[index],
                WorkflowJson = workflowJson,
                CreditRulesJson = CreditRulesJson,
                DisplayFieldsJson = "[]",
                PublishedByUserId = "system",
                PublishedOn = DateTime.UtcNow
            });
        }

        return type;
    }

    private static ActivityService Service(ApplicationDbContext db)
        => new(db, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator());

    private static ClaimsPrincipal Principal(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));
}
