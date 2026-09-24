using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T120's evidence run, repeated for each new instrument over its shipped seed files: a trainee files it against an EPA
/// whose College list names it, an assessor rates it at 3a, and it credits on the CPSA ladder. Plus the reflective
/// exercise, which is unrated evidence and credits nothing (D6, D7).
/// </summary>
public sealed class RemainingWbaToolsTests
{
    private const string TraineeId = "trainee-1";
    private const string AssessorId = "assessor-1";
    private const int InstitutionId = 10;

    private const int CpsaScaleId = 901;
    private const string CpsaScaleName = "CPSA Paediatric Entrustment Scale v11.1";

    private const int CurriculumId = 3000;
    private const int PermittingEpaId = 5000;
    private const int PermittingItemId = 4000;
    private const int ForbiddingEpaId = 5001;
    private const int ForbiddingItemId = 4001;

    // Order 3 on the CPSA ladder is rung "3a"; the curriculum's minimum is the same rung.
    private const int RungThreeA = 3;

    private static readonly DateOnly ProgrammeStart = new(2026, 1, 12);

    public static TheoryData<string, string, string> RatedTools => new()
    {
        { "cca_cpsa", "cca", """{ "case_reference": "Bed 4, 12 March", "documents_reviewed": ["admission_notes", "discharge_summary"], "setting": "ward", "reasoning_discussed": "Fluid plan in bronchiolitis." }""" },
        { "rca_cpsa", "rca", """{ "case_reference": "Clinic list, 3 March", "selection_method": "chosen_at_random_by_assessor", "setting": "outpatient_clinic" }""" },
        { "chart_stimulated_recall_cpsa", "chart_stimulated_recall", """{ "record_reference": "Ward round, 5 March", "setting": "ward", "decisions_probed": "Why oral antibiotics were stopped on day two." }""" },
    };

    [Theory]
    [MemberData(nameof(RatedTools))]
    public async Task ARatedTool_FiledAgainstAPermittingEpa_AndRatedAt3a_Credits(string seedKey, string toolKey, string requestFieldsJson)
    {
        var options = await SeededAsync(seedKey, toolKey);

        var draft = await CreateAsync(options, WithRequest(requestFieldsJson, PermittingEpaId));
        await TransitionAsync(options, draft.Id, "submit", TraineeId);
        var completed = await TransitionAsync(options, draft.Id, "complete", AssessorId, patch: $$"""
            { "overall_level": {{RungThreeA}}, "strengths": "Clear.", "improvements": "Earlier escalation.", "plan": "Repeat." }
            """);

        completed.CurrentState.Should().Be("completed");
        var record = completed.Transitions.Single(transition => transition.TransitionKey == "complete");
        record.CreditedItemCount.Should().Be(1);
        record.CreditScaleMismatchCount.Should().Be(0, "the seed binds the CPSA ladder by its exact name");

        await using var db = new ApplicationDbContext(options);
        var progress = await db.CurriculumItemProgresses.SingleAsync();
        progress.CurriculumItemId.Should().Be(PermittingItemId);
        progress.CountsSoFar.Should().Be(1);
        progress.MinimumLevelReachedCount.Should().Be(1, "3a meets a 3a minimum on the same ladder");
    }

    [Theory]
    [MemberData(nameof(RatedTools))]
    public async Task ARatedTool_FiledAgainstAnEpaWhoseListDoesNotNameIt_IsRefused(string seedKey, string toolKey, string requestFieldsJson)
    {
        // The T122 gate applies from the first line: the seed's catalogue entry declares which instrument it is.
        var options = await SeededAsync(seedKey, toolKey);

        var attempt = () => CreateAsync(options, WithRequest(requestFieldsJson, ForbiddingEpaId));

        (await attempt.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Contain("cannot be used as evidence");
    }

    [Fact]
    public async Task AReflectiveExercise_IsDiscussedByTheNamedMentor_AndCreditsNothing()
    {
        var options = await SeededAsync("reflective_exercise_cpsa", "reflective_exercise");

        var draft = await CreateAsync(options, ReflectionData());
        await TransitionAsync(options, draft.Id, "submit", TraineeId);
        var discussed = await TransitionAsync(options, draft.Id, "record_discussion", AssessorId, patch: """
            { "discussion_notes": "Agreed a debrief checklist for future resuscitations." }
            """);

        discussed.CurrentState.Should().Be("discussed");
        discussed.Transitions.Single(transition => transition.TransitionKey == "record_discussion")
            .CreditedItemCount.Should().BeNull("an empty counts_for declares no credit, so none is evaluated (D7)");

        await using var db = new ApplicationDbContext(options);
        (await db.CurriculumItemProgresses.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task AReflectiveExercise_TheMentorCanReturnItForRevision_WithANote_AndTheTraineeCanResubmit()
    {
        var options = await SeededAsync("reflective_exercise_cpsa", "reflective_exercise");
        var draft = await CreateAsync(options, ReflectionData());
        await TransitionAsync(options, draft.Id, "submit", TraineeId);

        var withoutNote = () => TransitionAsync(options, draft.Id, "return", AssessorId);
        await withoutNote.Should().ThrowAsync<InvalidOperationException>();

        (await TransitionAsync(options, draft.Id, "return", AssessorId, note: "Say more about what you would change."))
            .CurrentState.Should().Be("draft");
        (await TransitionAsync(options, draft.Id, "submit", TraineeId))
            .CurrentState.Should().Be("submitted");
    }

    [Fact]
    public async Task AReflectiveExercise_TheMentorCannotRecordTheDiscussion_WithoutWritingIt()
    {
        var options = await SeededAsync("reflective_exercise_cpsa", "reflective_exercise");
        var draft = await CreateAsync(options, ReflectionData());
        await TransitionAsync(options, draft.Id, "submit", TraineeId);

        var attempt = () => TransitionAsync(options, draft.Id, "record_discussion", AssessorId);

        (await attempt.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Contain("discussion_notes");
    }

    // ---- helpers -------------------------------------------------------------------------------------------------

    private static string WithRequest(string requestFieldsJson, int epaId)
    {
        var fields = System.Text.Json.Nodes.JsonNode.Parse(requestFieldsJson)!.AsObject();
        fields["epa_id"] = epaId;
        fields["assessor_user_id"] = AssessorId;
        fields["observed_on"] = "2026-03-10";
        return fields.ToJsonString();
    }

    private static string ReflectionData() => $$"""
        {
          "epa_id": {{PermittingEpaId}},
          "assessor_user_id": "{{AssessorId}}",
          "observed_on": "2026-03-10",
          "prompt": "critical_incident",
          "what_happened": "A delayed escalation during a resuscitation.",
          "analysis": "Roles were clear; the escalation call came late.",
          "learning": "Name the escalation trigger at the start.",
          "action_plan": "Use the debrief checklist."
        }
        """;

    private static async Task<ActivityDto> CreateAsync(DbContextOptions<ApplicationDbContext> options, string dataJson)
    {
        await using var db = new ApplicationDbContext(options);
        return await Service(db).CreateDraftAsync(new CreateActivityInput(1, TraineeId, TraineeId, dataJson, Principal(TraineeId)));
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

    private static async Task<DbContextOptions<ApplicationDbContext>> SeededAsync(string seedKey, string toolKey)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);

        NomineeSeed.AddUser(db, TraineeId, InstitutionId, WombatRoles.Trainee);
        NomineeSeed.AddUser(db, AssessorId, InstitutionId, WombatRoles.Assessor);

        db.EntrustmentScales.Add(new EntrustmentScale { Id = CpsaScaleId, Name = CpsaScaleName });
        db.Epas.AddRange(
            new Epa { Id = PermittingEpaId, Code = "PAED-001", Title = "Permits this tool" },
            new Epa { Id = ForbiddingEpaId, Code = "PAED-010", Title = "Does not permit this tool" });
        db.CurriculumItems.AddRange(
            new CurriculumItem
            {
                Id = PermittingItemId,
                CurriculumId = CurriculumId,
                EpaId = PermittingEpaId,
                RequiredCount = 3,
                MinimumLevelOrder = RungThreeA,
                WindowMonths = 12,
                ScaleId = CpsaScaleId,
                PermittedToolsJson = $$"""["cbd","{{toolKey}}"]"""
            },
            new CurriculumItem
            {
                Id = ForbiddingItemId,
                CurriculumId = CurriculumId,
                EpaId = ForbiddingEpaId,
                RequiredCount = 3,
                MinimumLevelOrder = RungThreeA,
                WindowMonths = 12,
                ScaleId = CpsaScaleId,
                PermittedToolsJson = """["direct_observation","msf"]"""
            });
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = TraineeId,
            InstitutionId = InstitutionId,
            CurriculumId = CurriculumId,
            ProgrammeStartDate = ProgrammeStart,
            ExpectedCompletionDate = ProgrammeStart.AddYears(4),
            IsActive = true
        });

        string Read(string file) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", seedKey, file));
        var schemaJson = Read("schema.json");
        var workflowJson = Read("workflow.json");
        var creditJson = Read("credit.json");

        var type = new ActivityType
        {
            Id = 1,
            Key = seedKey,
            Name = seedKey,
            Scope = ActivityScope.Global,
            Version = 1,
            WbaToolKey = toolKey,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditJson,
            DisplayFieldsJson = "[]",
            OwnerUserId = "system",
            CreatedOn = DateTime.UtcNow
        };
        type.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = 1,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditJson,
            DisplayFieldsJson = "[]",
            PublishedByUserId = "system",
            PublishedOn = DateTime.UtcNow
        });
        db.ActivityTypes.Add(type);

        await db.SaveChangesAsync();
        return options;
    }

    private static ActivityService Service(ApplicationDbContext db)
        => new(db, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator());

    private static ClaimsPrincipal Principal(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));
}
