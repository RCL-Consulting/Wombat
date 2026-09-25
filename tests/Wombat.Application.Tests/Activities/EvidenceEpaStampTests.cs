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
/// T137: <c>Activity.EpaId</c> is stamped from the pinned schema's <c>evidence_epa_field</c> at create and on every
/// transition, over the shipped <c>mini_cex_cpsa</c> seed. The system-written path (an MSF release) is covered in
/// <c>MsfEvidenceFanOutTests</c>.
/// </summary>
/// <remarks>
/// Before T137 the column existed and nothing wrote it. Each assertion below is on the stored row, read back through a
/// fresh context, because the stamp that matters is the one a list query will join on.
/// </remarks>
public sealed class EvidenceEpaStampTests
{
    private const string TraineeId = "trainee-1";
    private const string AssessorId = "assessor-1";
    private const int InstitutionId = 10;

    private const int CpsaScaleId = 901;
    private const string CpsaScaleName = "CPSA Paediatric Entrustment Scale v11.1";
    private const string CpsaScaleSeedKey = "cpsa:scale:v11.1";

    private const int CurriculumId = 3000;
    private const int CurriculumEpaId = 5000;
    private const int CurriculumItemId = 4000;

    /// <summary>An EPA that exists but is on no curriculum item: unrestricted by the tool gate (D21), credits nothing.</summary>
    private const int UncataloguedEpaId = 5001;

    /// <summary>An id no EPA has.</summary>
    private const int UnknownEpaId = 9999;

    private const int RungThreeA = 3;

    private static readonly DateOnly ProgrammeStart = new(2026, 1, 12);

    [Fact]
    public async Task Create_StampsTheEpaTheFormNames()
    {
        var options = await SeededAsync();

        var draft = await CreateAsync(options, Request(CurriculumEpaId));

        draft.EpaId.Should().Be(CurriculumEpaId);
        (await StoredEpaIdAsync(options, draft.Id)).Should().Be(CurriculumEpaId);
    }

    /// <summary>
    /// The form stores the picker's value as a string. The stamp reads it the way credit does, so "5000" and 5000 are
    /// one EPA.
    /// </summary>
    [Fact]
    public async Task Create_StampsAnEpaIdTheFormStoredAsAString()
    {
        var options = await SeededAsync();

        var draft = await CreateAsync(options, Request($"\"{CurriculumEpaId}\""));

        (await StoredEpaIdAsync(options, draft.Id)).Should().Be(CurriculumEpaId);
    }

    [Fact]
    public async Task Create_WithAnIdThatNamesNoEpa_StampsNothing()
    {
        var options = await SeededAsync();

        var draft = await CreateAsync(options, Request(UnknownEpaId));

        (await StoredEpaIdAsync(options, draft.Id)).Should().BeNull("a stamp a join cannot resolve is worse than none");
    }

    [Fact]
    public async Task Create_WithNoEpaYet_StampsNothing()
    {
        var options = await SeededAsync();

        var draft = await CreateAsync(options, """{ "presenting_problem": "Wheeze" }""");

        (await StoredEpaIdAsync(options, draft.Id)).Should().BeNull();
    }

    /// <summary>
    /// A trainee who corrects the EPA in the submit's own patch moves the stamp with it: the transition stamps from the
    /// MERGED data, not from what was stored before the move.
    /// </summary>
    [Fact]
    public async Task Transition_RestampsFromTheDataTheMoveWrites()
    {
        var options = await SeededAsync();
        var draft = await CreateAsync(options, Request(CurriculumEpaId));

        var submitted = await TransitionAsync(options, draft.Id, "submit", TraineeId, patch: $$"""{ "epa_id": "{{UncataloguedEpaId}}" }""");

        submitted.CurrentState.Should().Be("requested");
        (await StoredEpaIdAsync(options, draft.Id)).Should().Be(UncataloguedEpaId);
    }

    [Fact]
    public async Task Transition_ToAnIdThatNamesNoEpa_ClearsTheStamp()
    {
        var options = await SeededAsync();
        var draft = await CreateAsync(options, Request(CurriculumEpaId));

        await TransitionAsync(options, draft.Id, "submit", TraineeId, patch: $$"""{ "epa_id": {{UnknownEpaId}} }""");

        (await StoredEpaIdAsync(options, draft.Id)).Should().BeNull();
    }

    /// <summary>
    /// The property the pointer and <c>EvidenceEpa.EnsureCreditAgrees</c> exist for: the EPA the row is stamped with is
    /// the EPA of the curriculum item its completion credited.
    /// </summary>
    [Fact]
    public async Task Completion_StampsTheEpaOfTheItemItCredited()
    {
        var options = await SeededAsync();
        var draft = await CreateAsync(options, Request(CurriculumEpaId));
        await TransitionAsync(options, draft.Id, "submit", TraineeId);

        var completed = await TransitionAsync(options, draft.Id, "complete", AssessorId, patch: $$"""
            { "overall_level": {{RungThreeA}}, "strengths": "Clear.", "improvements": "Earlier escalation.", "plan": "Repeat." }
            """);

        completed.Transitions.Single(transition => transition.TransitionKey == "complete").CreditedItemCount.Should().Be(1);

        await using var db = new ApplicationDbContext(options);
        var credited = await db.CurriculumItemProgresses.SingleAsync();
        var creditedEpaId = (await db.CurriculumItems.SingleAsync(item => item.Id == credited.CurriculumItemId)).EpaId;
        (await StoredEpaIdAsync(options, draft.Id)).Should().Be(creditedEpaId);
    }

    /// <summary>
    /// Pinned, not live, and no guessing from a key name: a version that declares no pointer stamps nothing, even when
    /// its data carries an <c>epa_id</c>. A version is only publishable that way if it credits by no EPA field.
    /// </summary>
    [Fact]
    public async Task APinnedVersionWithoutThePointer_StampsNothing()
    {
        var options = await SeededAsync(withoutPointer: true);

        var draft = await CreateAsync(options, Request(CurriculumEpaId));

        (await StoredEpaIdAsync(options, draft.Id)).Should().BeNull();
    }

    // ---- helpers -------------------------------------------------------------------------------------------------

    private static string Request(int epaId) => Request(epaId.ToString(System.Globalization.CultureInfo.InvariantCulture));

    private static string Request(string epaIdJson) => $$"""
        {
          "epa_id": {{epaIdJson}},
          "assessor_user_id": "{{AssessorId}}",
          "observed_on": "2026-03-10",
          "setting": "ward",
          "presenting_problem": "Bronchiolitis",
          "complexity": "moderate"
        }
        """;

    private static async Task<int?> StoredEpaIdAsync(DbContextOptions<ApplicationDbContext> options, int activityId)
    {
        await using var db = new ApplicationDbContext(options);
        return (await db.Activities.AsNoTracking().SingleAsync(activity => activity.Id == activityId)).EpaId;
    }

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
        string? patch = null)
    {
        await using var db = new ApplicationDbContext(options);
        return await Service(db).TransitionAsync(new TransitionActivityInput(
            activityId, transitionKey, actorUserId, Principal(actorUserId), patch, null));
    }

    private static async Task<DbContextOptions<ApplicationDbContext>> SeededAsync(bool withoutPointer = false)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);

        NomineeSeed.AddUser(db, TraineeId, InstitutionId, WombatRoles.Trainee);
        NomineeSeed.AddUser(db, AssessorId, InstitutionId, WombatRoles.Assessor);

        db.EntrustmentScales.Add(new EntrustmentScale { Id = CpsaScaleId, Name = CpsaScaleName, SeedKey = CpsaScaleSeedKey });
        db.Epas.AddRange(
            new Epa { Id = CurriculumEpaId, Code = "PAED-001", Title = "On the curriculum" },
            new Epa { Id = UncataloguedEpaId, Code = "PAED-099", Title = "On no curriculum item" });
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = CurriculumItemId,
            CurriculumId = CurriculumId,
            EpaId = CurriculumEpaId,
            RequiredCount = 3,
            MinimumLevelOrder = RungThreeA,
            WindowMonths = 12,
            ScaleId = CpsaScaleId,
            PermittedToolsJson = """["mini_cex"]"""
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

        string Read(string file) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", "mini_cex_cpsa", file));
        var schemaJson = Read("schema.json");
        var workflowJson = Read("workflow.json");
        var creditJson = Read("credit.json");

        if (withoutPointer)
        {
            // A version as it would stand before T137, or a builder type that never declared the field. It credits by
            // no EPA field, because a version that did could not be published without the pointer.
            schemaJson = schemaJson.Replace("\"evidence_epa_field\": \"epa_id\",", string.Empty, StringComparison.Ordinal);
            creditJson = """{ "counts_for": [] }""";
        }

        var type = new ActivityType
        {
            Id = 1,
            Key = "mini_cex_cpsa",
            Name = "Mini-CEX (CPSA)",
            Scope = ActivityScope.Global,
            Version = 1,
            WbaToolKey = "mini_cex",
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
