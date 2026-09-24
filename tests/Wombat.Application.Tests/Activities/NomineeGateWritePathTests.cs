using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T102 on every path that writes an activity's data: the create, a transition's patch (a submit, a self-transition
/// save, a move into a dead end), and the staged, system-written batch. <c>UpdateDraftAsync</c> was the fourth and was
/// deleted; <c>Wombat.Architecture.Tests.ActivityWritePathTests</c> keeps it deleted.
/// </summary>
/// <remarks>
/// <para>
/// The gate's truth table (each eligibility condition) and the hand-on clause have their own tests. These prove that
/// each write path actually reaches the gate, that it reaches it before the first mutation, and that the older,
/// narrower self-nomination guard (fix 1) still speaks first where it applies, because its message says exactly
/// what is wrong.
/// </para>
/// <para>
/// Every refusal is checked against the audit trap, as <see cref="ToolPermissionGateTests" /> does: the change tracker
/// holds nothing Added, Modified or Deleted, and the audit pipeline's save of the same context writes nothing. For
/// <see cref="ActivityService.StageCompletedAsync" /> that is the whole contract: the caller still has its own
/// mutation to make, so a refusal that left one row added would be committed with the caller's audit row.
/// </para>
/// </remarks>
public sealed class NomineeGateWritePathTests
{
    private const string TraineeId = "trainee-1";
    private const string AssessorId = "assessor-1";
    private const string SecondAssessorId = "assessor-2";
    private const string OtherInstitutionAssessorId = "assessor-elsewhere";
    private const string NonAssessorId = "registrar-2";
    private const string DeactivatedAssessorId = "assessor-locked";
    private const string CoordinatorId = "coord-1";

    private const int InstitutionId = 10;
    private const int OtherInstitutionId = 20;

    private const int RequestTypeId = 200;
    private const int RecordTypeId = 201;
    private const int BoundRecordTypeId = 202;

    private const string RecordTypeKey = "reviewed_record_under_test";
    private const string BoundRecordTypeKey = "bound_reviewed_record_under_test";

    /// <summary>The fragment every nominee-gate refusal carries.</summary>
    private const string GateRefusal = "cannot be named here";

    /// <summary>Fix 1's message, after the field label. It must win wherever it applies.</summary>
    private const string SelfNominationRefusal =
        "this decides who may act on the activity, so it cannot name the person the activity is about.";

    // ---- create ---------------------------------------------------------------------------------------------------

    [Fact]
    public async Task Create_NamingAnEligibleAssessor_IsFiled()
    {
        // The control: a gate that refused everyone would pass every refusal below.
        var options = NewDatabase();
        await SeedAsync(options);

        var created = await CreateAsync(options, AssessorId);

        created.CurrentState.Should().Be("draft");
        ReadString(created.DataJson, "assessor_user_id").Should().Be(AssessorId);
    }

    [Fact]
    public async Task Create_NamingSomeoneWhoIsNotAnAssessor_IsRefused_AndNothingIsPersisted()
    {
        var options = NewDatabase();
        await SeedAsync(options);

        var message = await ShouldBeRefusedAsync(options, service => service.CreateDraftAsync(
            new CreateActivityInput(RequestTypeId, TraineeId, TraineeId, RequestData(NonAssessorId), Principal(TraineeId))));

        message.Should().StartWith("Assessor: ").And.Contain(GateRefusal);
        await AssertNoActivitiesAsync(options);
    }

    [Fact]
    public async Task Create_NamingTheSubject_GetsTheSelfNominationMessage_EvenWhenTheSubjectHoldsTheAssessorRole()
    {
        // Fix 1 runs inside BuildDraftActivity, before either gate. Giving the trainee the Assessor role at their own
        // institution leaves "is not the subject" as the only condition the directory would fail them on, so the two
        // guards would both refuse; the one that speaks must be the one whose message says exactly what is wrong.
        var options = NewDatabase();
        await SeedAsync(options);
        await GiveRoleAsync(options, TraineeId, WombatRoles.Assessor);

        var message = await ShouldBeRefusedAsync(options, service => service.CreateDraftAsync(
            new CreateActivityInput(RequestTypeId, TraineeId, TraineeId, RequestData(TraineeId), Principal(TraineeId))));

        message.Should().Be($"Assessor: {SelfNominationRefusal}");
        message.Should().NotContain(GateRefusal);
        await AssertNoActivitiesAsync(options);
    }

    [Fact]
    public async Task Create_ByACoordinatorNamingTheSubject_GetsTheSelfNominationMessage()
    {
        // The creator is not the subject here, so this is not "a trainee naming themselves": it is anyone naming the
        // person the activity is about. Fix 1 compares with the subject, not with the caller.
        var options = NewDatabase();
        await SeedAsync(options);

        var message = await ShouldBeRefusedAsync(options, service => service.CreateDraftAsync(
            new CreateActivityInput(RequestTypeId, TraineeId, CoordinatorId, RequestData(TraineeId), CoordinatorPrincipal())));

        message.Should().Be($"Assessor: {SelfNominationRefusal}");
        await AssertNoActivitiesAsync(options);
    }

    // ---- transition -----------------------------------------------------------------------------------------------

    [Fact]
    public async Task Submit_WithAPatchNamingTheSubject_GetsTheSelfNominationMessage_AndTheDraftIsUnchanged()
    {
        // Fix 1 runs right after the merge, so it still speaks before the T122 gate and the nominee gate on a patch.
        var options = NewDatabase();
        await SeedAsync(options);
        await GiveRoleAsync(options, TraineeId, WombatRoles.Assessor);
        var draft = await CreateAsync(options, AssessorId);

        var message = await ShouldBeRefusedAsync(options, service => service.TransitionAsync(new TransitionActivityInput(
            draft.Id, "submit", TraineeId, Principal(TraineeId), $$"""{ "assessor_user_id": "{{TraineeId}}" }""", null)));

        message.Should().Be($"Assessor: {SelfNominationRefusal}");

        var stored = await StoredAsync(options, draft.Id);
        stored.CurrentState.Should().Be("draft");
        ReadString(stored.DataJson, "assessor_user_id").Should().Be(AssessorId);
        stored.Transitions.Select(transition => transition.TransitionKey).Should().Equal("create");
    }

    [Fact]
    public async Task Save_ASelfTransitionNamingTheSubject_GetsTheSelfNominationMessage()
    {
        // A self-transition is the shape any post-creation save must take now that UpdateDraftAsync is gone (T106, T127).
        var options = NewDatabase();
        await SeedAsync(options);
        var draft = await CreateAsync(options, AssessorId);

        var message = await ShouldBeRefusedAsync(options, service => service.TransitionAsync(new TransitionActivityInput(
            draft.Id, "save", TraineeId, Principal(TraineeId), $$"""{ "assessor_user_id": "{{TraineeId}}" }""", null)));

        message.Should().Be($"Assessor: {SelfNominationRefusal}");
        ReadString((await StoredAsync(options, draft.Id)).DataJson, "assessor_user_id").Should().Be(AssessorId);
    }

    [Fact]
    public async Task Save_ASelfTransitionThatRenamesTheAssessorToSomeoneIneligible_IsRefused_AndTheDraftKeepsItsAssessor()
    {
        // A save moves nothing forward, but a changed nominee is judged on every move: the value grants read access,
        // an inbox row and nudges in every state, the draft included.
        var options = NewDatabase();
        await SeedAsync(options);
        var draft = await CreateAsync(options, AssessorId);

        var message = await ShouldBeRefusedAsync(options, service => service.TransitionAsync(new TransitionActivityInput(
            draft.Id, "save", TraineeId, Principal(TraineeId), $$"""{ "assessor_user_id": "{{NonAssessorId}}" }""", null)));

        message.Should().StartWith("Assessor: ").And.Contain(GateRefusal);

        var stored = await StoredAsync(options, draft.Id);
        ReadString(stored.DataJson, "assessor_user_id").Should().Be(AssessorId);
        stored.Transitions.Select(transition => transition.TransitionKey).Should().Equal("create");
    }

    [Fact]
    public async Task Save_ASelfTransitionThatRenamesTheAssessorToAnotherEligibleAssessor_IsSaved()
    {
        // The control for the refusal above: a re-pick from the list is an ordinary save.
        var options = NewDatabase();
        await SeedAsync(options);
        var draft = await CreateAsync(options, AssessorId);

        var saved = await TransitionAsync(options, draft.Id, "save", TraineeId, $$"""{ "assessor_user_id": "{{SecondAssessorId}}" }""");

        saved.CurrentState.Should().Be("draft");
        ReadString(saved.DataJson, "assessor_user_id").Should().Be(SecondAssessorId);
    }

    [Fact]
    public async Task Cancel_WithAPatchThatNamesSomeoneIneligible_IsRefused_EvenThoughCancelledIsADeadEnd()
    {
        // Unlike the EPA→tool gate, a changed nominee is judged on a move into a dead end too: a cancelled record still
        // shows up for whoever it names, so a cancel must not be a way to plant a name.
        var options = NewDatabase();
        await SeedAsync(options);
        var draft = await CreateAsync(options, AssessorId);

        var message = await ShouldBeRefusedAsync(options, service => service.TransitionAsync(new TransitionActivityInput(
            draft.Id, "cancel", TraineeId, Principal(TraineeId), $$"""{ "assessor_user_id": "{{OtherInstitutionAssessorId}}" }""", null)));

        message.Should().Contain(GateRefusal);
        (await StoredAsync(options, draft.Id)).CurrentState.Should().Be("draft");
    }

    [Fact]
    public async Task Cancel_OfADraftWhoseStoredAssessorHasSinceBeenDeactivated_IsNotJudged()
    {
        // The mirror of the test above: an UNCHANGED nominee is judged only at the author's hand-on, and a move into a
        // dead end hands nothing on. Refusing it would leave the trainee with a draft they could neither submit nor drop.
        //
        // This also pins the first conjunct of the shared UnchangedFieldsHandedOn, "the move can still reach a terminal
        // state". A dead end is writable by nobody, so the author does lose write access here, which on its own would
        // count as a hand-on. The EPA→tool gate checks reachability before it ever asks the predicate, so only the
        // nominee gate depends on the conjunct being inside it.
        var options = NewDatabase();
        await SeedAsync(options);
        var draft = await CreateAsync(options, AssessorId);
        await DeactivateAsync(options, AssessorId);
        await AssertNowIneligibleAsync(options, AssessorId);

        var cancelled = await TransitionAsync(options, draft.Id, "cancel", TraineeId);

        cancelled.CurrentState.Should().Be("cancelled");
    }

    // ---- staged, system-written ------------------------------------------------------------------------------------

    [Fact]
    public async Task StageCompleted_NamingAnEligibleAssessor_StagesEveryRow_AndTheyAreRecordedWithTheirReviewer()
    {
        // The control for the staged path: a Coordinator-only terminal transition on a non-crediting type, like msf_cpsa,
        // plus a user field. Every refusal below would pass against a path that refused for some other reason.
        var options = NewDatabase();
        await SeedAsync(options);

        await using (var db = new ApplicationDbContext(options))
        {
            var staged = await Service(db).StageCompletedAsync(StageInput(RecordTypeKey, AssessorId, SecondAssessorId));

            staged.Should().Be(2);
            db.ChangeTracker.Entries<Activity>().Should().HaveCount(2)
                .And.OnlyContain(entry => entry.State == EntityState.Added, "staged, not saved: the caller saves");
            await db.SaveChangesAsync();
        }

        await using var verify = new ApplicationDbContext(options);
        var activities = await verify.Activities.AsNoTracking().OrderBy(activity => activity.Id).ToListAsync();
        activities.Should().HaveCount(2).And.OnlyContain(activity => activity.CurrentState == "recorded");
        activities.Select(activity => ReadString(activity.DataJson, "reviewer_user_id"))
            .Should().Equal(AssessorId, SecondAssessorId);
        activities.Should().OnlyContain(activity => activity.InstitutionId == InstitutionId);
    }

    [Fact]
    public async Task StageCompleted_NamingSomeoneWhoIsNotAnAssessor_IsRefusedBeforeAnythingIsStaged()
    {
        var options = NewDatabase();
        await SeedAsync(options);

        var message = await ShouldBeRefusedAsync(options, service => service.StageCompletedAsync(
            StageInput(RecordTypeKey, NonAssessorId)));

        // Led by the label, and named: the person belongs to the trainee's own institution.
        message.Should().StartWith($"Reviewing consultant: First {NonAssessorId} {GateRefusal}");
        await AssertNoActivitiesAsync(options);
    }

    [Fact]
    public async Task StageCompleted_ABatchWhoseLastRowNamesAnotherInstitutionsAssessor_StagesNothing_AndDoesNotNameThem()
    {
        // The first row is fine and was fully built before the second was judged. Nothing may be in the context when the
        // second throws: AddRange comes after the loop, which is what makes the batch all-or-nothing. The refused person
        // belongs to another institution, so the message must not say who they are.
        var options = NewDatabase();
        await SeedAsync(options);

        var message = await ShouldBeRefusedAsync(options, service => service.StageCompletedAsync(
            StageInput(RecordTypeKey, AssessorId, OtherInstitutionAssessorId)));

        message.Should().StartWith($"Reviewing consultant: that person {GateRefusal}");
        message.Should().NotContain(OtherInstitutionAssessorId);
        await AssertNoActivitiesAsync(options);
    }

    [Fact]
    public async Task StageCompleted_ADeactivatedAssessor_IsRefused()
    {
        var options = NewDatabase();
        await SeedAsync(options);

        var message = await ShouldBeRefusedAsync(options, service => service.StageCompletedAsync(
            StageInput(RecordTypeKey, DeactivatedAssessorId)));

        message.Should().Contain(GateRefusal);
        await AssertNoActivitiesAsync(options);
    }

    [Fact]
    public async Task StageCompleted_IsJudgedAgainstTheSubjectsInstitution_NotTheCallers()
    {
        // The coordinator releasing the record sits at another institution. The nominee must be at the TRAINEE's, which
        // is what the record is stamped with and what every scope: rule on it reads.
        var options = NewDatabase();
        await SeedAsync(options);
        var elsewhere = CoordinatorPrincipal(OtherInstitutionId);

        (await ShouldBeRefusedAsync(options, service => service.StageCompletedAsync(
                StageInput(RecordTypeKey, OtherInstitutionAssessorId) with { Principal = elsewhere })))
            .Should().Contain(GateRefusal);

        await using var db = new ApplicationDbContext(options);
        (await Service(db).StageCompletedAsync(StageInput(RecordTypeKey, AssessorId) with { Principal = elsewhere }))
            .Should().Be(1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task StageCompleted_ABlankReviewer_IsNotJudged(string? reviewer)
    {
        // Nobody is being named, so there is nobody to judge. Whether the field may be blank is the schema's question,
        // and this schema says it may.
        var options = NewDatabase();
        await SeedAsync(options);

        await using var db = new ApplicationDbContext(options);
        var staged = await Service(db).StageCompletedAsync(StageInput(RecordTypeKey, reviewer));

        staged.Should().Be(1);
    }

    [Fact]
    public async Task StageCompleted_NamingTheSubjectInAUserFieldNoRuleNames_IsRefusedByTheNomineeGate()
    {
        // Fix 1 guards only fields a `field:` rule reads. A user field no rule names still lands a person on the record,
        // so it is a nominee field too, and "is not the subject" is one of the gate's own conditions.
        var options = NewDatabase();
        await SeedAsync(options);
        await GiveRoleAsync(options, TraineeId, WombatRoles.Assessor);

        var message = await ShouldBeRefusedAsync(options, service => service.StageCompletedAsync(
            StageInput(RecordTypeKey, TraineeId)));

        message.Should().Contain(GateRefusal);
        message.Should().NotContain(SelfNominationRefusal);
        await AssertNoActivitiesAsync(options);
    }

    [Fact]
    public async Task StageCompleted_NamingTheSubjectInAFieldARuleNames_GetsTheSelfNominationMessage()
    {
        // The staged path builds each row with BuildDraftActivity, so fix 1 runs there too, before the nominee gate.
        var options = NewDatabase();
        await SeedAsync(options);
        await GiveRoleAsync(options, TraineeId, WombatRoles.Assessor);

        var message = await ShouldBeRefusedAsync(options, service => service.StageCompletedAsync(
            StageInput(BoundRecordTypeKey, TraineeId)));

        message.Should().Be($"Reviewing consultant: {SelfNominationRefusal}");
        await AssertNoActivitiesAsync(options);
    }

    [Fact]
    public async Task TheShippedMsfType_DeclaresNoNomineeField_SoItsReleaseNeverConsultsTheDirectory()
    {
        // StageCompletedAsync's gate is a no-op for msf_cpsa today, which is why MsfEvidenceFanOutTests seed no
        // Identity users and still release. If the seed ever gains a user field or a field: rule, the release starts
        // being judged, and those tests must then seed the nominee eligible rather than lose an assertion.
        var seedDirectory = Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", "msf_cpsa");
        var schema = FormSchemaParser.Parse(await File.ReadAllTextAsync(Path.Combine(seedDirectory, "schema.json")));
        var workflow = WorkflowParser.Parse(await File.ReadAllTextAsync(Path.Combine(seedDirectory, "workflow.json")));

        ActorFieldRules.RequiredRolesByNomineeField(schema, workflow).Should().BeEmpty();
    }

    // ---- helpers --------------------------------------------------------------------------------------------------

    /// <summary>
    /// Runs one refused request on its own context, asserts it left nothing for the audit save to commit, performs
    /// that save, and returns the refusal message.
    /// </summary>
    private static async Task<string> ShouldBeRefusedAsync(
        DbContextOptions<ApplicationDbContext> options,
        Func<ActivityService, Task> act)
    {
        await using var db = new ApplicationDbContext(options);
        var service = Service(db);

        var attempt = async () => await act(service);
        var thrown = await attempt.Should().ThrowAsync<InvalidOperationException>();

        db.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(entry => $"{entry.Metadata.ClrType.Name}: {entry.State}")
            .Should().BeEmpty("the audit pipeline's catch saves this context, so anything dirty here would be committed");

        // What the audit pipeline's catch does next: save the same context.
        (await db.SaveChangesAsync()).Should().Be(0);

        return thrown.Which.Message;
    }

    /// <summary>
    /// The guard for a test that proves an unchanged nominee is NOT re-judged: naming the same person afresh must be
    /// refused now, or the test would pass because the deactivation did not take.
    /// </summary>
    private static async Task AssertNowIneligibleAsync(DbContextOptions<ApplicationDbContext> options, string userId)
    {
        var message = await ShouldBeRefusedAsync(options, service => service.CreateDraftAsync(
            new CreateActivityInput(RequestTypeId, TraineeId, TraineeId, RequestData(userId), Principal(TraineeId))));

        message.Should().Contain(GateRefusal, "guard: this person must really be ineligible now");
    }

    private static async Task AssertNoActivitiesAsync(DbContextOptions<ApplicationDbContext> options)
    {
        await using var verify = new ApplicationDbContext(options);
        (await verify.Activities.CountAsync()).Should().Be(0, "a refused write must leave no activity behind");
        (await verify.ActivityTransitions.CountAsync()).Should().Be(0);
    }

    private static async Task<ActivityDto> CreateAsync(DbContextOptions<ApplicationDbContext> options, string assessorUserId)
    {
        await using var db = new ApplicationDbContext(options);
        return await Service(db).CreateDraftAsync(
            new CreateActivityInput(RequestTypeId, TraineeId, TraineeId, RequestData(assessorUserId), Principal(TraineeId)));
    }

    private static async Task<ActivityDto> TransitionAsync(
        DbContextOptions<ApplicationDbContext> options,
        int activityId,
        string transitionKey,
        string actorUserId,
        string? patch = null)
    {
        await using var db = new ApplicationDbContext(options);
        return await Service(db).TransitionAsync(
            new TransitionActivityInput(activityId, transitionKey, actorUserId, Principal(actorUserId), patch, null));
    }

    private static async Task<Activity> StoredAsync(DbContextOptions<ApplicationDbContext> options, int activityId)
    {
        await using var db = new ApplicationDbContext(options);
        return await db.Activities
            .AsNoTracking()
            .Include(entity => entity.Transitions)
            .SingleAsync(entity => entity.Id == activityId);
    }

    /// <summary>What an administrator's grant does, in its own request.</summary>
    private static async Task GiveRoleAsync(DbContextOptions<ApplicationDbContext> options, string userId, string role)
    {
        await using var db = new ApplicationDbContext(options);
        NomineeSeed.AddRole(db, userId, role);
        await db.SaveChangesAsync();
    }

    /// <summary>What an administrator's lock does, in its own request.</summary>
    private static async Task DeactivateAsync(DbContextOptions<ApplicationDbContext> options, string userId)
    {
        await using var db = new ApplicationDbContext(options);
        var user = await db.Users.SingleAsync(entity => entity.Id == userId);
        user.LockoutEnd = UserDeactivation.IndefiniteLockoutEnd;
        await db.SaveChangesAsync();
    }

    private static RecordCompletedActivitiesInput StageInput(string typeKey, params string?[] reviewers)
        => new(
            typeKey,
            TraineeId,
            CoordinatorId,
            "record",
            reviewers.Select(RecordData).ToArray(),
            CoordinatorPrincipal());

    private static ActivityService Service(ApplicationDbContext db)
        => new(db, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator());

    private static string RequestData(string assessorUserId)
        => $$"""
            {
              "assessor_user_id": "{{assessorUserId}}",
              "observed_on": "2026-03-10",
              "presenting_problem": "Fever for three days"
            }
            """;

    /// <summary>A system-written record. A null reviewer leaves the key out; any other value is written as given.</summary>
    private static string RecordData(string? reviewerUserId)
        => reviewerUserId is null
            ? """{ "observed_on": "2026-06-01", "summary": "Released after review." }"""
            : $$"""{ "observed_on": "2026-06-01", "reviewer_user_id": "{{reviewerUserId}}", "summary": "Released after review." }""";

    private static string? ReadString(string dataJson, string key)
    {
        using var document = JsonDocument.Parse(dataJson);
        return document.RootElement.TryGetProperty(key, out var value) ? value.GetString() : null;
    }

    private static DbContextOptions<ApplicationDbContext> NewDatabase()
        => new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static ClaimsPrincipal Principal(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));

    /// <summary>
    /// The releasing coordinator, with the role claim type set so <c>IsInRole</c> matches <c>role:Coordinator</c>.
    /// </summary>
    private static ClaimsPrincipal CoordinatorPrincipal(int institutionId = InstitutionId)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, CoordinatorId),
                new Claim(ClaimTypes.Role, WombatRoles.Coordinator),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString())
            ],
            "test",
            ClaimTypes.Name,
            ClaimTypes.Role));

    // ---- fixture --------------------------------------------------------------------------------------------------

    /// <summary>A CPSA-shaped request, without the EPA, plus a `save` self-transition in the draft.</summary>
    private const string RequestSchemaJson = """
        {
          "version": 1,
          "observation_date_field": "observed_on",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "assessor_user_id", "type": "user", "label": "Assessor", "required": true },
                { "key": "observed_on", "type": "date", "label": "Date observed", "required": true },
                { "key": "presenting_problem", "type": "text", "label": "Presenting problem" }
              ]
            },
            {
              "key": "assessment",
              "title": "Entrustment",
              "editable_by": "field:assessor_user_id",
              "fields": [
                { "key": "overall_level", "type": "number", "label": "Supervision required for this encounter" }
              ]
            }
          ]
        }
        """;

    private const string RequestWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true },
            { "key": "cancelled", "label": "Cancelled" }
          ],
          "transitions": [
            { "key": "save", "from": "draft", "to": "draft", "actor": "subject|creator" },
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id", "requires_fields": ["overall_level"] },
            { "key": "cancel", "from": ["draft", "requested"], "to": "cancelled", "actor": "subject|creator" }
          ]
        }
        """;

    /// <summary>
    /// msf_cpsa's shape: every section written by the coordinator, one Coordinator-only move straight to a terminal
    /// state, no credit. Plus a user field, which msf_cpsa does not have, and which no actor rule names.
    /// </summary>
    private const string RecordSchemaJson = """
        {
          "version": 1,
          "observation_date_field": "observed_on",
          "sections": [
            {
              "key": "evidence",
              "title": "Evidence",
              "editable_by": "role:Coordinator|role:Administrator",
              "fields": [
                { "key": "observed_on", "type": "date", "label": "Feedback window closed", "required": true },
                { "key": "reviewer_user_id", "type": "user", "label": "Reviewing consultant" },
                { "key": "summary", "type": "longtext", "label": "Reviewer's summary" }
              ]
            }
          ]
        }
        """;

    private const string RecordWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "recorded", "label": "Recorded", "terminal": true }
          ],
          "transitions": [
            { "key": "record", "from": "draft", "to": "recorded", "actor": "role:Coordinator|role:Administrator" }
          ]
        }
        """;

    /// <summary>The same record, with the reviewer also allowed to record it: a `field:` rule now names the field.</summary>
    private static readonly string BoundRecordWorkflowJson = RecordWorkflowJson.Replace(
        "\"actor\": \"role:Coordinator|role:Administrator\"",
        "\"actor\": \"role:Coordinator|role:Administrator|field:reviewer_user_id\"",
        StringComparison.Ordinal);

    private const string CreditsNothing = """{ "counts_for": [] }""";

    private static async Task SeedAsync(DbContextOptions<ApplicationDbContext> options)
    {
        await using var db = new ApplicationDbContext(options);

        NomineeSeed.AddUser(db, TraineeId, InstitutionId, WombatRoles.Trainee);
        NomineeSeed.AddUser(db, AssessorId, InstitutionId, WombatRoles.Assessor);
        NomineeSeed.AddUser(db, SecondAssessorId, InstitutionId, WombatRoles.Assessor);
        NomineeSeed.AddUser(db, OtherInstitutionAssessorId, OtherInstitutionId, WombatRoles.Assessor);
        NomineeSeed.AddUser(db, NonAssessorId, InstitutionId, WombatRoles.Trainee);
        NomineeSeed.AddUser(db, DeactivatedAssessorId, InstitutionId, UserDeactivation.IndefiniteLockoutEnd, WombatRoles.Assessor);
        NomineeSeed.AddUser(db, CoordinatorId, InstitutionId, WombatRoles.Coordinator);

        // Where the trainee trains: the create stamps this institution, and it is the one every nominee is judged at.
        db.TraineeProfiles.Add(new TraineeProfile
        {
            Id = 1,
            UserId = TraineeId,
            InstitutionId = InstitutionId,
            CurriculumId = 3000,
            ProgrammeStartDate = new DateOnly(2025, 4, 14),
            ExpectedCompletionDate = new DateOnly(2029, 4, 13),
            IsActive = true
        });

        db.ActivityTypes.AddRange(
            Type(RequestTypeId, "request_under_test", RequestSchemaJson, RequestWorkflowJson),
            Type(RecordTypeId, RecordTypeKey, RecordSchemaJson, RecordWorkflowJson),
            Type(BoundRecordTypeId, BoundRecordTypeKey, RecordSchemaJson, BoundRecordWorkflowJson));

        await db.SaveChangesAsync();
    }

    private static ActivityType Type(int id, string key, string schemaJson, string workflowJson)
    {
        var publishedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var activityType = new ActivityType
        {
            Id = id,
            Key = key,
            Name = key,
            Scope = ActivityScope.Institution,
            ScopeId = InstitutionId,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = CreditsNothing,
            DisplayFieldsJson = "[]",
            OwnerUserId = "admin-1",
            CreatedOn = publishedOn
        };

        activityType.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = id,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = CreditsNothing,
            DisplayFieldsJson = "[]",
            PublishedByUserId = "admin-1",
            PublishedOn = publishedOn
        });

        return activityType;
    }
}
