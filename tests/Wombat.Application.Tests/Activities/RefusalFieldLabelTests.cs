using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T172: a validation refusal names each field by the label the form shows it under, not by its key. The refusal is
/// read on the page that shows the form (the action error on the activity's page, T127's "Saved as a draft, but not
/// submitted" notice, the create refusal on <c>/activities/new</c>), and a disabled action's reason already names
/// fields by label (T107), so the key made the two disagree. Driven through <c>ActivityService</c> over the shipped
/// <c>mini_cex_cpsa</c> seed, the form activity 20's refusal came from in the T127 browser check, and over
/// <c>qi_project</c>, whose labels repeat from one PDSA cycle to the next.
/// </summary>
public sealed class RefusalFieldLabelTests
{
    private const string TraineeId = "trainee-1";
    private const string AssessorId = "assessor-1";
    private const int InstitutionId = 10;
    private const int CpsaMiniCexTypeId = 300;
    private const int QiProjectTypeId = 301;

    [Fact]
    public async Task ARefusedSubmit_NamesThePresentingProblem_ByTheLabelTheFormShows()
    {
        var options = await SeededAsync();
        var draft = await CreateAsync(options, CpsaRequest(without: ["presenting_problem"]));

        var message = await RefusedSubmitAsync(options, draft.Id);

        message.Should().Be("Presenting problem: A value is required.");
        message.Should().NotContain("presenting_problem");
    }

    [Fact]
    public async Task ARefusalNamingSeveralFields_NamesEachByItsLabel_InTheFormsOrder_AsSentences()
    {
        // T189: each error is a sentence, so they are joined as sentences are. Before, "; " after each full stop read
        // "A value is required.; Case complexity: …".
        var options = await SeededAsync();
        var draft = await CreateAsync(options, CpsaRequest(without: ["complexity", "presenting_problem"]));

        var message = await RefusedSubmitAsync(options, draft.Id);

        message.Should().Be("Presenting problem: A value is required. Case complexity: A value is required.");
    }

    [Fact]
    public async Task ARefusalNamingAFieldWhoseLabelRepeats_SaysWhichSectionItIsIn()
    {
        // qi_project labels a field "Plan" under each of its three PDSA cycles; only cycle 1's is required. "Plan: A value
        // is required." would leave the user to guess which of the three the refusal means.
        var options = await SeededAsync();
        var draft = await CreateAsync(options, QiProjectDraft(without: "pdsa_1_plan"), QiProjectTypeId);

        var message = await RefusedSubmitAsync(options, draft.Id);

        message.Should().Be("Plan (PDSA cycle 1): A value is required.");
    }

    [Fact]
    public async Task ARefusedCreate_NamesTheFieldByItsLabel()
    {
        // The create's own check (formats only, for a draft-born type), which is what /activities/new shows.
        var options = await SeededAsync();

        var attempt = () => CreateAsync(options, CpsaRequest(without: [], observedOn: "last Tuesday"));

        (await attempt.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Be("Date observed: A valid date is required.");
    }

    [Fact]
    public async Task AFieldWithNoLabel_IsNamedByItsKey()
    {
        // The parser refuses a blank label, so the only field a refusal can meet without one is a key the pinned schema
        // does not declare. The validator reports only declared fields today; this one stands in for a key it someday
        // might, so the refusal still says which field it means rather than nothing. An error with no field at all is
        // said as it is.
        var options = await SeededAsync();
        await using var db = new ApplicationDbContext(options);
        var service = Service(db, new UnlabelledFieldValidator());

        var attempt = () => service.CreateDraftAsync(
            new CreateActivityInput(CpsaMiniCexTypeId, TraineeId, TraineeId, CpsaRequest(without: []), Principal(TraineeId)));

        (await attempt.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Be("unlabelled_note: A value is required. The data could not be read.");
    }

    [Fact]
    public async Task AnErrorThatDoesNotEndAsASentence_IsGivenAFullStop_SoTheNextDoesNotRunOnIntoIt()
    {
        // T189. Every message the shipped validator and the encounter-date gate write ends in a full stop; the joiner does
        // not depend on it.
        var options = await SeededAsync();
        await using var db = new ApplicationDbContext(options);
        var service = Service(db, new FixedErrorsValidator(
            new ActivityValidationErrorDto("presenting_problem", "Say what brought the child in", "required"),
            new ActivityValidationErrorDto(null, "  The data could not be read  ", "invalid_object"),
            new ActivityValidationErrorDto("complexity", "Is this right?", "custom")));

        var attempt = () => service.CreateDraftAsync(
            new CreateActivityInput(CpsaMiniCexTypeId, TraineeId, TraineeId, CpsaRequest(without: []), Principal(TraineeId)));

        (await attempt.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Be(
                "Presenting problem: Say what brought the child in. The data could not be read. Case complexity: Is this right?");
    }

    // ---- T263: the refusal carries the keys of the fields it names ------------------------------------------------

    [Fact]
    public async Task ARefusedSubmit_CarriesTheKeysOfTheFieldsItNames_InTheOrderItNamesThem()
    {
        // The page marks the controls whose ids are these keys, so they are the schema's keys, never the labels.
        var options = await SeededAsync();
        var draft = await CreateAsync(options, CpsaRequest(without: ["complexity", "presenting_problem"]));

        var attempt = async () =>
        {
            await using var db = new ApplicationDbContext(options);
            return await Service(db).TransitionAsync(new TransitionActivityInput(
                draft.Id, "submit", TraineeId, Principal(TraineeId), null, null));
        };

        var refusal = (await attempt.Should().ThrowAsync<ActivityFieldsRefusedException>()).Which;
        refusal.Message.Should().Be("Presenting problem: A value is required. Case complexity: A value is required.");
        refusal.FieldKeys.Should().Equal("presenting_problem", "complexity");
    }

    [Fact]
    public async Task ARefusedCreate_CarriesTheKeyOfTheFieldItNames()
    {
        var options = await SeededAsync();

        var attempt = () => CreateAsync(options, CpsaRequest(without: [], observedOn: "last Tuesday"));

        (await attempt.Should().ThrowAsync<ActivityFieldsRefusedException>())
            .Which.FieldKeys.Should().Equal("observed_on");
    }

    [Fact]
    public async Task AnErrorOfNoField_CarriesNoKey_AndAKeyNamedTwice_IsCarriedOnce()
    {
        var options = await SeededAsync();
        await using var db = new ApplicationDbContext(options);
        var service = Service(db, new FixedErrorsValidator(
            new ActivityValidationErrorDto("presenting_problem", "Say what brought the child in.", "required"),
            new ActivityValidationErrorDto(null, "The data could not be read.", "invalid_object"),
            new ActivityValidationErrorDto("presenting_problem", "Value must be at most 5 characters long.", "max_length")));

        var attempt = () => service.CreateDraftAsync(
            new CreateActivityInput(CpsaMiniCexTypeId, TraineeId, TraineeId, CpsaRequest(without: []), Principal(TraineeId)));

        (await attempt.Should().ThrowAsync<ActivityFieldsRefusedException>())
            .Which.FieldKeys.Should().Equal("presenting_problem");
    }

    // ---- helpers -------------------------------------------------------------------------------------------------

    /// <summary>A validator whose one field error names a key the schema does not declare, and whose other names none.</summary>
    private sealed class UnlabelledFieldValidator() : FixedErrorsValidator(
        new ActivityValidationErrorDto("unlabelled_note", "A value is required.", "required"),
        new ActivityValidationErrorDto(null, "The data could not be read.", "invalid_object"));

    /// <summary>A validator that reports the same errors whatever it is given.</summary>
    private class FixedErrorsValidator(params ActivityValidationErrorDto[] errors) : ISchemaValidator
    {
        public IReadOnlyList<ActivityValidationErrorDto> Validate(
            FormSchema schema,
            string dataJson,
            SchemaValidationMode mode,
            IReadOnlyCollection<string>? additionallyRequiredFieldKeys = null,
            IReadOnlySet<string>? requiredFieldScope = null)
            => errors;
    }

    private static async Task<ActivityDto> CreateAsync(
        DbContextOptions<ApplicationDbContext> options,
        string dataJson,
        int activityTypeId = CpsaMiniCexTypeId)
    {
        await using var db = new ApplicationDbContext(options);
        return await Service(db).CreateDraftAsync(
            new CreateActivityInput(activityTypeId, TraineeId, TraineeId, dataJson, Principal(TraineeId)));
    }

    private static async Task<string> RefusedSubmitAsync(DbContextOptions<ApplicationDbContext> options, int activityId)
    {
        var attempt = async () =>
        {
            await using var db = new ApplicationDbContext(options);
            return await Service(db).TransitionAsync(new TransitionActivityInput(
                activityId, "submit", TraineeId, Principal(TraineeId), null, null));
        };

        var thrown = await attempt.Should().ThrowAsync<InvalidOperationException>();
        return thrown.Which.Message;
    }

    private static string CpsaRequest(string[] without, string observedOn = "2026-03-10")
    {
        var values = new Dictionary<string, object>
        {
            ["epa_id"] = 2,
            ["assessor_user_id"] = AssessorId,
            ["observed_on"] = observedOn,
            ["setting"] = "ward",
            ["presenting_problem"] = "Fever for three days",
            ["complexity"] = "moderate"
        };

        foreach (var key in without)
        {
            values.Remove(key).Should().BeTrue("the fixture must actually drop '{0}'", key);
        }

        return System.Text.Json.JsonSerializer.Serialize(values);
    }

    /// <summary>Every field qi_project's submit requires, less <paramref name="without" />.</summary>
    private static string QiProjectDraft(string without)
    {
        var values = new[]
            {
                "project_title", "problem_statement", "team", "measures",
                "pdsa_1_plan", "pdsa_1_do", "pdsa_1_study", "pdsa_1_act",
                "outcomes", "reflection"
            }
            .ToDictionary(key => key, key => $"Some {key.Replace('_', ' ')}.");

        values.Remove(without).Should().BeTrue("the fixture must actually drop '{0}'", without);
        return System.Text.Json.JsonSerializer.Serialize(values);
    }

    private static async Task<DbContextOptions<ApplicationDbContext>> SeededAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);

        // The assessor is an eligible nominee at the trainee's institution (T102), so every refusal here is validation's.
        NomineeSeed.AddUser(db, TraineeId, InstitutionId, WombatRoles.Trainee);
        NomineeSeed.AddUser(db, AssessorId, InstitutionId, WombatRoles.Assessor);
        db.ActivityTypes.Add(FromSeed(CpsaMiniCexTypeId, "mini_cex_cpsa"));
        db.ActivityTypes.Add(FromSeed(QiProjectTypeId, "qi_project"));

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

    private static ActivityService Service(ApplicationDbContext db, ISchemaValidator? schemaValidator = null)
        => new(db, schemaValidator ?? new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator());

    private static ClaimsPrincipal Principal(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));
}
