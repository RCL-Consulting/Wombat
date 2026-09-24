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
/// T105: a transition declares how much of the form it insists on (<c>validation</c>: <c>all</c>, <c>owned</c> or
/// <c>draft</c>), so the schema can say honestly which fields are mandatory. Driven through <c>ActivityService</c> over
/// the shipped seed files, because the seeds are what trainees and assessors actually file.
/// </summary>
/// <remarks>
/// Before T105 every transition validated the whole schema as if submitting: a half-filled draft could not be cancelled,
/// an assessor could not decline without ratings, and the legacy types' <c>accept</c> was unsatisfiable. The CPSA seeds
/// hid the assessor's fields from the validator to cope. These tests are T105's Verification section, one each.
/// </remarks>
public sealed class TransitionValidationTests
{
    private const string TraineeId = "trainee-1";
    private const string AssessorId = "assessor-1";
    private const int InstitutionId = 10;

    private const int CpsaMiniCexTypeId = 300;
    private const int LegacyMiniCexTypeId = 301;
    private const int DisposalWithRequiredReasonTypeId = 302;
    private const int LogTypeId = 303;

    // A refusal names each field by the label the form shows it under (T172), so these are the seeds' labels, not keys.
    private static readonly string[] CpsaAssessorFieldLabels =
        ["Supervision required for this encounter", "What was done well", "Areas for development", "Agreed plan"];

    private static readonly string[] LegacyAssessorFieldLabels =
        ["Overall performance", "Strengths", "Areas for improvement", "Agreed plan"];

    // ---- the CPSA Mini-CEX (draft-born) --------------------------------------------------------------------------

    [Fact]
    public async Task ACpsaDraftWithOneFieldFilled_CanBeCancelled()
    {
        var options = await SeededAsync();
        var draft = await CreateAsync(options, CpsaMiniCexTypeId, """{ "presenting_problem": "Wheeze" }""");

        var cancelled = await TransitionAsync(options, draft.Id, "cancel", TraineeId);

        cancelled.CurrentState.Should().Be("cancelled");
    }

    [Fact]
    public async Task ACpsaSubmit_AsksForTheTraineesRequiredFields_AndNotForTheAssessors()
    {
        // `owned`: the trainee must fill what is theirs before handing on. The four assessor fields are required too,
        // but only the assessor can write them, so they are not the trainee's to fill yet.
        var options = await SeededAsync();
        var draft = await CreateAsync(options, CpsaMiniCexTypeId, CpsaRequest(without: "setting"));

        var message = await RefusedAsync(options, draft.Id, "submit", TraineeId);

        message.Should().Be("Clinical setting: A value is required.");
        foreach (var assessorField in CpsaAssessorFieldLabels)
        {
            message.Should().NotContain(assessorField);
        }
    }

    [Fact]
    public async Task ACpsaSubmit_WithTheRequestComplete_Passes_ThoughNoRatingExistsYet()
    {
        var options = await SeededAsync();
        var draft = await CreateAsync(options, CpsaMiniCexTypeId, CpsaRequest());

        var requested = await TransitionAsync(options, draft.Id, "submit", TraineeId);

        requested.CurrentState.Should().Be("requested");
    }

    [Fact]
    public async Task AnAssessor_CanDeclineWithANote_AndNoRatings()
    {
        var options = await SeededAsync();
        var requested = await SubmittedCpsaAsync(options);

        var declined = await TransitionAsync(options, requested.Id, "decline", AssessorId, note: "I did not observe this encounter.");

        declined.CurrentState.Should().Be("declined");
    }

    [Fact]
    public async Task ACpsaComplete_WithoutTheRatings_IsRefused_NamingEveryRequiredAssessorField()
    {
        var options = await SeededAsync();
        var requested = await SubmittedCpsaAsync(options);

        var message = await RefusedAsync(options, requested.Id, "complete", AssessorId);

        foreach (var assessorField in CpsaAssessorFieldLabels)
        {
            message.Should().Contain($"{assessorField}: A value is required.");
        }
    }

    [Fact]
    public async Task ACpsaComplete_StillEnforcesEveryRequiredRequestField()
    {
        // `all` at completion: everything the form asks for must be there, the trainee's fields included. Reached here by
        // a request that lost a field after its submit, which no page can do, so it is written straight to the store.
        var options = await SeededAsync();
        var requested = await SubmittedCpsaAsync(options);
        await using (var db = new ApplicationDbContext(options))
        {
            var stored = await db.Activities.SingleAsync(activity => activity.Id == requested.Id);
            stored.DataJson = CpsaRequest(without: "complexity");
            await db.SaveChangesAsync();
        }

        var message = await RefusedAsync(options, requested.Id, "complete", AssessorId, patch: Ratings());

        message.Should().Be("Case complexity: A value is required.");
    }

    [Fact]
    public async Task ACpsaComplete_WithTheRatings_Completes()
    {
        var options = await SeededAsync();
        var requested = await SubmittedCpsaAsync(options);

        var completed = await TransitionAsync(options, requested.Id, "complete", AssessorId, patch: Ratings());

        completed.CurrentState.Should().Be("completed");
    }

    // ---- the legacy Mini-CEX (requested-born) --------------------------------------------------------------------

    [Fact]
    public async Task TheLegacyMiniCex_IsFiledByItsCreate_SoACreateMissingAFieldTheTraineeOwes_IsRefused()
    {
        // Born in `requested`, the create is the submission: the trainee's one move left is `cancel`, a withdrawal, so no
        // later move of theirs would ask for what they owe. A request naming no assessor could then be accepted, repaired
        // or completed by nobody. The create asks, as a submit declaring `owned` would, and stores nothing when refused.
        var options = await SeededAsync();

        var missingAssessor = await RefusedCreateAsync(options, LegacyMiniCexTypeId, LegacyRequest(without: "assessor_user_id"));
        var missingComplexity = await RefusedCreateAsync(options, LegacyMiniCexTypeId, LegacyRequest(without: "complexity"));

        missingAssessor.Should().Be("Assessor user id: A value is required.");
        missingComplexity.Should().Be("Case complexity: A value is required.");
        foreach (var assessorField in LegacyAssessorFieldLabels)
        {
            missingComplexity.Should().NotContain(assessorField, "the ratings are the assessor's to give, after the create");
        }

        await using var db = new ApplicationDbContext(options);
        (await db.Activities.CountAsync()).Should().Be(0, "a refused create stores nothing");
    }

    [Fact]
    public async Task TheLegacyMiniCex_WithTheRequestComplete_IsFiledInRequested_ThoughNoRatingExistsYet()
    {
        var options = await SeededAsync();

        var request = await CreateAsync(options, LegacyMiniCexTypeId, LegacyRequest());

        request.CurrentState.Should().Be("requested");
    }

    [Fact]
    public async Task ATypeBornTerminal_IsFiledByItsCreate_SoItsRequiredFieldsAreAskedFor()
    {
        // procedure_log's shape: the create is the whole record, and nothing after it could ask.
        var options = await SeededAsync();

        var message = await RefusedCreateAsync(options, LogTypeId, """{ "notes": "Uneventful." }""");
        var logged = await CreateAsync(options, LogTypeId, """{ "title": "Lumbar puncture" }""");

        message.Should().Be("Title: A value is required.");
        logged.CurrentState.Should().Be("logged");
    }

    [Fact]
    public async Task TheLegacyMiniCex_AssessorCanAcceptAPartlyFilledRequest_WithNoRatings()
    {
        // `accept` validates `draft`. A partly filled request can no longer be filed (above), so it is written straight
        // to the store, as a request that lost a field after it was filed would be.
        var options = await SeededAsync();
        var request = await PartlyFilledLegacyRequestAsync(options);

        var accepted = await TransitionAsync(options, request.Id, "accept", AssessorId);

        accepted.CurrentState.Should().Be("accepted");
    }

    [Fact]
    public async Task TheLegacyMiniCex_AssessorCanDecline_AndTheTraineeCanCancel()
    {
        var options = await SeededAsync();
        var toDecline = await PartlyFilledLegacyRequestAsync(options);
        var toCancel = await PartlyFilledLegacyRequestAsync(options);

        (await TransitionAsync(options, toDecline.Id, "decline", AssessorId, note: "Not my patient."))
            .CurrentState.Should().Be("declined");
        (await TransitionAsync(options, toCancel.Id, "cancel", TraineeId))
            .CurrentState.Should().Be("cancelled");
    }

    [Fact]
    public async Task TheLegacyMiniCex_CompletionAsksForTheRatingsTheSchemaNowRequires()
    {
        var options = await SeededAsync();
        var request = await CreateAsync(options, LegacyMiniCexTypeId, LegacyRequest());
        await TransitionAsync(options, request.Id, "accept", AssessorId);

        var message = await RefusedAsync(options, request.Id, "complete", AssessorId);
        foreach (var field in LegacyAssessorFieldLabels)
        {
            message.Should().Contain($"{field}: A value is required.");
        }

        var completed = await TransitionAsync(options, request.Id, "complete", AssessorId, patch: """
            { "overall": 4, "strengths": "Calm.", "improvements": "Summarise sooner.", "plan": "Repeat next month." }
            """);
        completed.CurrentState.Should().Be("completed");
    }

    // ---- requires_fields under every value -----------------------------------------------------------------------

    [Fact]
    public async Task RequiresFields_StillApply_OnADraftValidatedMove()
    {
        // `draft` relaxes the schema's own `required` flags, never the transition's explicit extra demands.
        var options = await SeededAsync();
        var draft = await CreateAsync(options, DisposalWithRequiredReasonTypeId, """{ "title": "Half done" }""");

        var message = await RefusedAsync(options, draft.Id, "withdraw", TraineeId);
        message.Should().Be("Reason: A value is required.",
            "the schema's own required Summary does not count on a draft-validated move");

        var withdrawn = await TransitionAsync(options, draft.Id, "withdraw", TraineeId, patch: """{ "reason": "Duplicate." }""");
        withdrawn.CurrentState.Should().Be("withdrawn");
    }

    // ---- helpers -------------------------------------------------------------------------------------------------

    private static async Task<ActivityDto> SubmittedCpsaAsync(DbContextOptions<ApplicationDbContext> options)
    {
        var draft = await CreateAsync(options, CpsaMiniCexTypeId, CpsaRequest());
        return await TransitionAsync(options, draft.Id, "submit", TraineeId);
    }

    private static async Task<ActivityDto> CreateAsync(DbContextOptions<ApplicationDbContext> options, int typeId, string dataJson)
    {
        await using var db = new ApplicationDbContext(options);
        return await Service(db).CreateDraftAsync(new CreateActivityInput(typeId, TraineeId, TraineeId, dataJson, Principal(TraineeId)));
    }

    private static async Task<string> RefusedCreateAsync(DbContextOptions<ApplicationDbContext> options, int typeId, string dataJson)
    {
        var attempt = () => CreateAsync(options, typeId, dataJson);
        var thrown = await attempt.Should().ThrowAsync<InvalidOperationException>();
        return thrown.Which.Message;
    }

    private static async Task<ActivityDto> PartlyFilledLegacyRequestAsync(DbContextOptions<ApplicationDbContext> options)
    {
        var request = await CreateAsync(options, LegacyMiniCexTypeId, LegacyRequest());
        await using var db = new ApplicationDbContext(options);
        var stored = await db.Activities.SingleAsync(activity => activity.Id == request.Id);
        stored.DataJson = LegacyRequest(without: "complexity");
        await db.SaveChangesAsync();
        return request;
    }

    private static async Task<ActivityDto> TransitionAsync(
        DbContextOptions<ApplicationDbContext> options,
        int activityId,
        string transitionKey,
        string actorUserId,
        string? patch = null,
        string? note = null)
    {
        await using var db = new ApplicationDbContext(options);
        return await Service(db).TransitionAsync(new TransitionActivityInput(
            activityId, transitionKey, actorUserId, Principal(actorUserId), patch, note));
    }

    private static async Task<string> RefusedAsync(
        DbContextOptions<ApplicationDbContext> options,
        int activityId,
        string transitionKey,
        string actorUserId,
        string? patch = null)
    {
        var attempt = () => TransitionAsync(options, activityId, transitionKey, actorUserId, patch);
        var thrown = await attempt.Should().ThrowAsync<InvalidOperationException>();
        return thrown.Which.Message;
    }

    private static string CpsaRequest(string? without = null)
        => Json(new Dictionary<string, object>
        {
            ["epa_id"] = 2,
            ["assessor_user_id"] = AssessorId,
            ["observed_on"] = "2026-03-10",
            ["setting"] = "ward",
            ["presenting_problem"] = "Fever for three days",
            ["complexity"] = "moderate"
        }, without);

    private static string LegacyRequest(string? without = null)
        => Json(new Dictionary<string, object>
        {
            ["epa_id"] = 2,
            ["assessor_user_id"] = AssessorId,
            ["observed_on"] = "2026-03-10",
            ["setting"] = "ward",
            ["presenting_complaint"] = "Fever for three days",
            ["complexity"] = "moderate"
        }, without);

    private static string Ratings()
        => """{ "overall_level": 3, "strengths": "Thorough history.", "improvements": "Examine earlier.", "plan": "Repeat on the ward." }""";

    private static string Json(Dictionary<string, object> values, string? without)
    {
        if (without is not null)
        {
            values.Remove(without).Should().BeTrue("the fixture must actually drop '{0}'", without);
        }

        return System.Text.Json.JsonSerializer.Serialize(values);
    }

    private static async Task<DbContextOptions<ApplicationDbContext>> SeededAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);

        // The trainee's Identity row is where the create stamps their institution; the assessor is an eligible nominee
        // there (T102), so every refusal below comes from validation.
        NomineeSeed.AddUser(db, TraineeId, InstitutionId, WombatRoles.Trainee);
        NomineeSeed.AddUser(db, AssessorId, InstitutionId, WombatRoles.Assessor);

        db.ActivityTypes.Add(FromSeed(CpsaMiniCexTypeId, "mini_cex_cpsa"));
        db.ActivityTypes.Add(FromSeed(LegacyMiniCexTypeId, "mini_cex"));
        db.ActivityTypes.Add(Type(
            DisposalWithRequiredReasonTypeId,
            "withdrawable_under_test",
            """
            {
              "version": 1,
              "sections": [
                {
                  "key": "main",
                  "title": "Main",
                  "fields": [
                    { "key": "title", "type": "text", "label": "Title", "required": true },
                    { "key": "summary", "type": "longtext", "label": "Summary", "required": true },
                    { "key": "reason", "type": "text", "label": "Reason" }
                  ]
                }
              ]
            }
            """,
            """
            {
              "version": 1,
              "initial_state": "draft",
              "states": [
                { "key": "draft", "label": "Draft" },
                { "key": "done", "label": "Done", "terminal": true },
                { "key": "withdrawn", "label": "Withdrawn" }
              ],
              "transitions": [
                { "key": "finish", "from": "draft", "to": "done", "actor": "subject", "validation": "all" },
                { "key": "withdraw", "from": "draft", "to": "withdrawn", "actor": "subject", "requires_fields": ["reason"], "validation": "draft" }
              ]
            }
            """,
            """{ "counts_for": [] }"""));
        db.ActivityTypes.Add(Type(
            LogTypeId,
            "log_under_test",
            """
            {
              "version": 1,
              "sections": [
                {
                  "key": "main",
                  "title": "Main",
                  "fields": [
                    { "key": "title", "type": "text", "label": "Title", "required": true },
                    { "key": "notes", "type": "longtext", "label": "Notes" }
                  ]
                }
              ]
            }
            """,
            """
            {
              "version": 1,
              "initial_state": "logged",
              "states": [
                { "key": "logged", "label": "Logged", "terminal": true }
              ],
              "transitions": []
            }
            """,
            """{ "counts_for": [] }"""));

        await db.SaveChangesAsync();
        return options;
    }

    private static ActivityType FromSeed(int id, string seedKey)
    {
        string Read(string file) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", seedKey, file));
        return Type(id, seedKey, Read("schema.json"), Read("workflow.json"), Read("credit.json"));
    }

    private static ActivityType Type(int id, string key, string schemaJson, string workflowJson, string creditRulesJson)
    {
        var type = new ActivityType
        {
            Id = id,
            Key = key,
            Name = key,
            Scope = ActivityScope.Global,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditRulesJson,
            DisplayFieldsJson = "[]",
            OwnerUserId = "system",
            CreatedOn = DateTime.UtcNow
        };

        type.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = id,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditRulesJson,
            DisplayFieldsJson = "[]",
            PublishedByUserId = "system",
            PublishedOn = DateTime.UtcNow
        });

        return type;
    }

    private static ActivityService Service(ApplicationDbContext db)
        => new(db, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator());

    private static ClaimsPrincipal Principal(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));
}
