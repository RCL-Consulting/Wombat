using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T189: <c>ActivityService</c>'s own refusals of a move name the move as its button does, and the state and the field by
/// their declared labels, never by key. T172 did the same for the validator's refusals. Driven over the shipped
/// <c>mini_cex_cpsa</c> and <c>clinical_audit_cpsa</c> seeds: the second has a move whose key is two words
/// (<c>sign_off</c>) and a state whose label is not its key (<c>submitted</c> is "Awaiting supervisor").
/// </summary>
/// <remarks>
/// Each activity is written straight into the store in the state the test needs, because every refusal here comes before
/// the gates and the validation a real filing would pass through.
/// </remarks>
public sealed class TransitionRefusalLabelTests
{
    private const string TraineeId = "trainee-1";
    private const string AssessorId = "assessor-1";
    private const string CoordinatorId = "coord-1";
    private const int InstitutionId = 10;
    private const int CpsaMiniCexTypeId = 300;
    private const int ClinicalAuditTypeId = 301;

    private const string MiniCexRequest = """
        {
          "epa_id": 2,
          "assessor_user_id": "assessor-1",
          "observed_on": "2026-03-10",
          "setting": "ward",
          "presenting_problem": "Fever for three days",
          "complexity": "moderate"
        }
        """;

    private const string AuditRequest = """{ "epa_id": 2, "assessor_user_id": "assessor-1", "observed_on": "2026-03-10" }""";

    [Fact]
    public async Task AMoveTheStateDoesNotOffer_NamesTheMoveAndTheState_ByTheirLabels()
    {
        var options = await SeededAsync();
        await AddActivityAsync(options, 900, CpsaMiniCexTypeId, "draft", MiniCexRequest);

        var message = await RefusedAsync(options, service => service.TransitionAsync(
            new TransitionActivityInput(900, "complete", TraineeId, Principal(TraineeId), null, null)));

        message.Should().Be("Complete is not available while the activity is Draft.");
    }

    [Fact]
    public async Task AStateWhoseLabelIsNotItsKey_IsNamedByTheLabel()
    {
        // "Submitted" would be the key made readable; the workflow calls the state "Awaiting supervisor".
        var options = await SeededAsync();
        await AddActivityAsync(options, 901, ClinicalAuditTypeId, "submitted", AuditRequest);

        var message = await RefusedAsync(options, service => service.TransitionAsync(
            new TransitionActivityInput(901, "submit", TraineeId, Principal(TraineeId), null, null)));

        message.Should().Be("Submit is not available while the activity is Awaiting supervisor.");
    }

    [Fact]
    public async Task AMoveWhoseKeyIsTwoWords_IsNamedAsItsButtonIs()
    {
        var options = await SeededAsync();
        await AddActivityAsync(options, 902, ClinicalAuditTypeId, "draft", AuditRequest);

        var message = await RefusedAsync(options, service => service.TransitionAsync(
            new TransitionActivityInput(902, "sign_off", AssessorId, Principal(AssessorId), null, null)));

        message.Should().Be("Sign Off is not available while the activity is Draft.");
        message.Should().StartWith(new ActivityActionDto("sign_off", RequiresNote: false).Label + " ",
            "the refusal names the move by the text on its button");
    }

    [Fact]
    public async Task AMoveThatNeedsANote_SaysSo_NamingTheMoveByItsLabel()
    {
        var options = await SeededAsync();
        await AddActivityAsync(options, 903, CpsaMiniCexTypeId, "requested", MiniCexRequest);

        var message = await RefusedAsync(options, service => service.TransitionAsync(
            new TransitionActivityInput(903, "decline", AssessorId, Principal(AssessorId), null, Note: "   ")));

        message.Should().Be("Decline requires a note.");
    }

    [Fact]
    public async Task APatchChangingAFieldTheActorCannotWrite_NamesTheFieldAndTheState_ByTheirLabels()
    {
        // Only a hand-made request meets this: the page never patches a field its actor cannot write.
        var options = await SeededAsync();
        await AddActivityAsync(options, 904, CpsaMiniCexTypeId, "requested", MiniCexRequest);

        var message = await RefusedAsync(options, service => service.TransitionAsync(
            new TransitionActivityInput(904, "complete", AssessorId, Principal(AssessorId), """{ "epa_id": 3 }""", null)));

        message.Should().Be("EPA: you cannot change this while the activity is Requested.");
    }

    [Fact]
    public async Task AStagedBatchAskingForAMoveTheInitialStateDoesNotOffer_NamesItByItsLabel()
    {
        // The system-written path's own lookup, from the initial state every row in a batch starts in.
        var options = await SeededAsync();

        var message = await RefusedAsync(options, service => service.StageCompletedAsync(new RecordCompletedActivitiesInput(
            "clinical_audit_cpsa", TraineeId, CoordinatorId, "sign_off", [AuditRequest], Principal(CoordinatorId))));

        message.Should().Be("Sign Off is not available while the activity is Draft.");
    }

    // ---- helpers -------------------------------------------------------------------------------------------------

    private static async Task<string> RefusedAsync(
        DbContextOptions<ApplicationDbContext> options,
        Func<ActivityService, Task> act)
    {
        await using var db = new ApplicationDbContext(options);
        var attempt = () => act(Service(db));

        var thrown = await attempt.Should().ThrowAsync<InvalidOperationException>();
        return thrown.Which.Message;
    }

    private static async Task AddActivityAsync(
        DbContextOptions<ApplicationDbContext> options,
        int id,
        int typeId,
        string state,
        string dataJson)
    {
        await using var db = new ApplicationDbContext(options);
        var utcNow = DateTime.UtcNow;
        db.Activities.Add(new Activity
        {
            Id = id,
            ActivityTypeId = typeId,
            SchemaVersion = 1,
            SubjectUserId = TraineeId,
            CreatedByUserId = TraineeId,
            InstitutionId = InstitutionId,
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
        db.ActivityTypes.Add(FromSeed(CpsaMiniCexTypeId, "mini_cex_cpsa"));
        db.ActivityTypes.Add(FromSeed(ClinicalAuditTypeId, "clinical_audit_cpsa"));
        await db.SaveChangesAsync();
        return options;
    }

    private static ActivityType FromSeed(int id, string seedKey)
    {
        string Read(string file) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", seedKey, file));

        var schemaJson = Read("schema.json");
        var workflowJson = Read("workflow.json");
        var creditRulesJson = Read("credit.json");

        var type = new ActivityType
        {
            Id = id,
            Key = seedKey,
            Name = seedKey,
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
