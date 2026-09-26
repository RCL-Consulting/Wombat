using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Activities;

/// <summary>
/// <see cref="ActivityWaiting" />, the one reading of "waiting" the inbox, the dashboards' waiting cards and the assessor
/// nudge share (T297), over the workflows the programme really runs.
/// </summary>
public sealed class ActivityWaitingTests
{
    /// <summary>
    /// Where each shipped workflow awaits a reviewer: a non-terminal state with a move for someone other than the author.
    /// Every rated CPSA instrument waits in <c>requested</c>, the supervised write-ups in <c>submitted</c>, and the legacy
    /// Demo instruments in <c>requested</c> and <c>accepted</c>. A draft waits on its author, a declined or cancelled
    /// request on nobody. The MSF rows are recorded by their release as they are written, so their <c>draft</c> is never
    /// seen.
    /// </summary>
    [Fact]
    public void EachShippedWorkflow_AwaitsAReviewer_WhereSomeoneOtherThanTheAuthorMovesNext()
    {
        var expected = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["acat"] = ["requested", "accepted"],
            ["cbd"] = ["requested", "accepted"],
            ["dops"] = ["requested", "accepted"],
            ["mini_cex"] = ["requested", "accepted"],
            ["cbd_cpsa"] = ["requested"],
            ["cca_cpsa"] = ["requested"],
            ["chart_stimulated_recall_cpsa"] = ["requested"],
            ["direct_observation_cpsa"] = ["requested"],
            ["dops_cpsa"] = ["requested"],
            ["mini_cex_cpsa"] = ["requested"],
            ["rca_cpsa"] = ["requested"],
            ["clinical_audit_cpsa"] = ["submitted"],
            ["portfolio_review_cpsa"] = ["submitted"],
            ["reflective_exercise_cpsa"] = ["submitted"],
            ["qi_project"] = ["submitted"],
            ["reflective_note"] = ["submitted"],
            ["research_output"] = ["submitted"],
            ["teaching_session"] = ["submitted"],
            ["learner_feedback_cpsa"] = ["draft"],
            ["msf_cpsa"] = ["draft"],
            ["journal_club"] = [],
            ["procedure_log"] = []
        };

        var actual = ShippedSeeds.Keys().ToDictionary(
            key => key,
            key =>
            {
                var workflow = WorkflowParser.Parse(ShippedSeeds.Workflow(key));
                return workflow.States.Select(state => state.Key).Where(state => ActivityWaiting.AwaitsReviewer(workflow, state)).ToArray();
            },
            StringComparer.Ordinal);

        actual.Should().BeEquivalentTo(expected, options => options.WithoutStrictOrdering());
    }

    /// <summary>
    /// The nudge's own need (a nominee in a field) is only ever narrower: every state in which a move is a field
    /// nominee's awaits a reviewer, so every request the nudge mails about is one the Coordinator's card can list.
    /// </summary>
    [Fact]
    public void EveryStateWhoseMoveIsAFieldNominees_AwaitsAReviewer()
    {
        foreach (var key in ShippedSeeds.Keys())
        {
            var workflow = WorkflowParser.Parse(ShippedSeeds.Workflow(key));
            foreach (var state in workflow.States.Where(state => !state.Terminal))
            {
                var aNomineeMoves = workflow.Transitions.Any(transition =>
                    transition.From.Contains(state.Key, StringComparer.Ordinal) && NamesAField(transition.Actor));
                if (aNomineeMoves)
                {
                    ActivityWaiting.AwaitsReviewer(workflow, state.Key).Should().BeTrue($"{key}'s {state.Key} waits on a nominee");
                }
            }
        }
    }

    [Fact]
    public void NoWorkflow_AndAStateTheWorkflowDoesNotDeclare_AwaitNobody()
    {
        ActivityWaiting.AwaitsReviewer(null, "requested").Should().BeFalse();
        ActivityWaiting.AwaitsReviewer(WorkflowParser.Parse(ShippedSeeds.Workflow("mini_cex_cpsa")), "in_review").Should().BeFalse();
    }

    /// <summary>
    /// A move on an activity is judged against its pinned version's row (<c>ActivityService</c> refuses a move on a pin
    /// without one), so a pin with no version row, or one whose workflow no longer parses, is actionable by nobody, though
    /// the type's own columns carry a workflow that would allow the move.
    /// </summary>
    [Fact]
    public async Task APinWithNoPublishedVersion_OrAnUnparseableOne_IsActionableByNobody()
    {
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        ShippedSeeds.AddType(db, 1, "mini_cex_cpsa", "Mini-CEX (Paediatrics)");
        db.ActivityTypes.Add(new ActivityType
        {
            Id = 2, Key = "unpublished", Name = "Unpublished", Scope = ActivityScope.Global, Version = 1,
            WorkflowJson = ShippedSeeds.Workflow("mini_cex_cpsa")
        });
        db.Set<ActivityTypeVersion>().Add(new ActivityTypeVersion
        {
            ActivityTypeId = 1, Version = 2, WorkflowJson = "{ not a workflow"
        });
        const string namesHim = """{ "assessor_user_id": "assessor-1" }""";
        db.Activities.AddRange(
            Requested(1, typeId: 1, version: 1, namesHim),
            Requested(2, typeId: 2, version: 1, namesHim),
            Requested(3, typeId: 1, version: 2, namesHim));
        await db.SaveChangesAsync();

        var rows = await ActivityWaiting.LoadActionableAsync(
            db.Activities, db, new WorkflowEvaluator(),
            new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "assessor-1")], "test")));

        rows.Select(row => row.Activity.Id).Should().Equal(1);
    }

    /// <summary>
    /// The T297 review: the actionable read narrows in SQL to the rows the caller could reach (<c>NarrowToCaller</c>)
    /// before the act gate runs, and that narrowing must never drop a row the gate would keep. Every shipped workflow is
    /// filed in every state, six ways (own, another's naming the caller, naming someone else, at another institution,
    /// raised by a coordinator, unstamped), and for each caller the read equals the act gate run on every row in the
    /// database, the way the inbox read them all before T297.
    /// </summary>
    /// <remarks>
    /// One caller's id is one the JSON writer escapes (<c>+</c> and <c>'</c> are written <c>+</c> and
    /// <c>'</c>), so a text search of the data for it finds nothing: the narrowing must reach every row for that id.
    /// </remarks>
    [Fact]
    public async Task TheNarrowing_KeepsEveryRowTheActGateAdmits_ForEveryShippedWorkflowAndCaller()
    {
        await using var db = NewContext();
        const string escapedId = "o'brien+1";
        var id = 0;
        var typeId = 0;
        foreach (var key in ShippedSeeds.Keys())
        {
            ShippedSeeds.AddType(db, ++typeId, key);
            var workflow = WorkflowParser.Parse(ShippedSeeds.Workflow(key));
            var fields = new HashSet<string>(StringComparer.Ordinal);
            foreach (var transition in workflow.Transitions)
            {
                ActorFieldRules.CollectFieldNames(transition.Actor, fields);
            }

            foreach (var state in workflow.States.Select(state => state.Key))
            {
                db.Activities.AddRange(
                    Filed(++id, typeId, state, subject: "trainee-1", creator: "trainee-1", Naming(fields, "assessor-1"), institution: 10),
                    Filed(++id, typeId, state, subject: "trainee-2", creator: "trainee-2", Naming(fields, "assessor-1"), institution: 10),
                    Filed(++id, typeId, state, subject: "trainee-2", creator: "trainee-2", Naming(fields, "assessor-2"), institution: 10),
                    Filed(++id, typeId, state, subject: "trainee-3", creator: "trainee-3", Naming(fields, "assessor-1"), institution: 20),
                    Filed(++id, typeId, state, subject: "trainee-3", creator: "coordinator-1", Naming(fields, escapedId), institution: 20),
                    Filed(++id, typeId, state, subject: "trainee-2", creator: "trainee-2", "{}", institution: null));
            }
        }

        await db.SaveChangesAsync();

        var callers = new Dictionary<string, ClaimsPrincipal>
        {
            ["trainee"] = TestPrincipals.InRoles([WombatRoles.Trainee], "trainee-1", 10),
            ["assessor"] = TestPrincipals.InRoles([WombatRoles.Assessor], "assessor-1", 10),
            ["assessor who is also a trainee"] = TestPrincipals.InRoles([WombatRoles.Assessor, WombatRoles.Trainee], "trainee-2", 10),
            ["assessor whose id the JSON escapes"] = TestPrincipals.InRoles([WombatRoles.Assessor], escapedId, 10),
            ["coordinator"] = TestPrincipals.InRoles([WombatRoles.Coordinator], "coordinator-1", 20),
            ["speciality admin"] = TestPrincipals.InRoles([WombatRoles.SpecialityAdmin], "speciality-admin", 10, 5, 6),
            ["administrator"] = TestPrincipals.InRoles([WombatRoles.Administrator], "admin", null),
            ["coordinator with no id"] = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.Role, WombatRoles.Coordinator), new Claim(WombatClaimTypes.InstitutionId, "20")], "test"))
        };

        var everyRow = await db.Activities.AsNoTracking().Include(activity => activity.ActivityType).ToListAsync();
        var published = await db.Set<ActivityTypeVersion>().AsNoTracking().ToListAsync();
        var evaluator = new WorkflowEvaluator();

        foreach (var (who, principal) in callers)
        {
            var admitted = everyRow
                .Where(activity => published.SingleOrDefault(version =>
                        version.ActivityTypeId == activity.ActivityTypeId && version.Version == activity.SchemaVersion) is { } pin &&
                    ActivityWaiting.IsActionableBy(WorkflowParser.Parse(pin.WorkflowJson), activity, principal, evaluator))
                .Select(activity => activity.Id)
                .ToList();

            var read = await ActivityWaiting.LoadActionableAsync(db.Activities, db, evaluator, principal);

            read.Select(row => row.Activity.Id).Should().BeEquivalentTo(admitted, $"the {who}'s read is the act gate's, row for row");
        }

        // Not vacuous: the callers between them are admitted on each of the gate's arms.
        (await ActivityWaiting.LoadActionableAsync(db.Activities, db, evaluator, callers["assessor whose id the JSON escapes"]))
            .Should().NotBeEmpty("a request naming him waits on him, although his id is escaped in the data");
    }

    /// <summary>
    /// And the narrowing narrows: a request naming another assessor, another trainee's draft, and a programme's work at
    /// another institution are left in the database, while the rows each caller could move are read.
    /// </summary>
    [Fact]
    public async Task TheNarrowing_LeavesInTheDatabaseTheRowsTheCallerCannotReach()
    {
        await using var db = NewContext();
        ShippedSeeds.AddType(db, 1, "mini_cex_cpsa");
        ShippedSeeds.AddType(db, 2, "teaching_session");
        db.Activities.AddRange(
            Filed(1, 1, "requested", subject: "trainee-1", creator: "trainee-1", """{ "assessor_user_id": "assessor-1" }""", institution: 10),
            Filed(2, 1, "requested", subject: "trainee-2", creator: "trainee-2", """{ "assessor_user_id": "assessor-2" }""", institution: 10),
            Filed(3, 1, "draft", subject: "trainee-2", creator: "trainee-2", """{ "assessor_user_id": "assessor-1" }""", institution: 10),
            Filed(4, 1, "draft", subject: "trainee-1", creator: "trainee-1", "{}", institution: 10),
            Filed(5, 2, "submitted", subject: "trainee-1", creator: "trainee-1", "{}", institution: 10, speciality: 5),
            Filed(6, 2, "submitted", subject: "trainee-3", creator: "trainee-3", "{}", institution: 20, speciality: 5));
        await db.SaveChangesAsync();

        var movable = await ActivityWaiting.LoadMovableAsync(db);
        async Task<IEnumerable<int>> Reached(ClaimsPrincipal principal)
            => (await movable.NarrowToCaller(db.Activities, principal).ToListAsync()).Select(activity => activity.Id);

        (await Reached(TestPrincipals.InRoles([WombatRoles.Assessor], "assessor-1", 10))).Should().BeEquivalentTo(
            [1], "the request naming him: not one naming another assessor, nor a draft, whose moves are its author's");
        (await Reached(TestPrincipals.InRoles([WombatRoles.Trainee], "trainee-1", 10))).Should().BeEquivalentTo(
            [1, 4], "her own rows with a move of hers left: not her teaching session, which only its SpecialityAdmin moves");
        (await Reached(TestPrincipals.InRoles([WombatRoles.SpecialityAdmin], "speciality-admin", 10, 5, 6))).Should().BeEquivalentTo(
            [5], "her institution's teaching session, not another institution's");
        (await Reached(TestPrincipals.InRoles([WombatRoles.Coordinator], "coordinator-1", 20))).Should().BeEmpty(
            "no move on either type is a coordinator's");
    }

    [Theory]
    [InlineData("subject", CallerReach.Subject)]
    [InlineData("creator", CallerReach.Creator)]
    [InlineData("field:assessor_user_id", CallerReach.NamedInData)]
    [InlineData("role:Assessor", CallerReach.AnyRow)]
    [InlineData("role:Coordinator", CallerReach.None)]
    [InlineData("scope:global", CallerReach.AnyRow)]
    [InlineData("scope:institution", CallerReach.CallersInstitution)]
    [InlineData("scope:speciality", CallerReach.CallersInstitution)]
    [InlineData("subject|field:assessor_user_id", CallerReach.Subject | CallerReach.NamedInData)]
    [InlineData("role:Coordinator|role:Assessor", CallerReach.AnyRow)]
    [InlineData("role:Assessor+scope:speciality", CallerReach.CallersInstitution)]
    [InlineData("role:Coordinator+scope:speciality", CallerReach.None)]
    [InlineData("scope:global+subject", CallerReach.Subject)]
    public void AnActorRule_ReachesTheRowsItsArmsCouldAdmitTheCallerOn(string actor, CallerReach expected)
    {
        var assessor = TestPrincipals.InRoles([WombatRoles.Assessor], "assessor-1", 10);

        ActivityWaiting.ReachOf(ActorRuleParser.Parse(actor), assessor).Should().Be(expected);
    }

    [Fact]
    public void AScopeArm_ReachesNothing_ForACallerWithNoInstitution()
    {
        ActivityWaiting.ReachOf(ActorRuleParser.Parse("scope:institution"), TestPrincipals.InRoles([WombatRoles.Assessor], "a", null))
            .Should().Be(CallerReach.None);
    }

    private static string Naming(IReadOnlySet<string> fields, string userId)
        => JsonSerializer.Serialize(fields.ToDictionary(field => field, _ => userId, StringComparer.Ordinal));

    private static Activity Filed(
        int id, int typeId, string state, string subject, string creator, string dataJson, int? institution, int? speciality = 5) => new()
    {
        Id = id, ActivityTypeId = typeId, SchemaVersion = 1,
        SubjectUserId = subject, CreatedByUserId = creator, CurrentState = state, DataJson = dataJson,
        InstitutionId = institution, SpecialityId = institution is null ? null : speciality,
        SubSpecialityId = institution is null ? null : 6,
        CreatedOn = DateTime.UtcNow.AddDays(-1), UpdatedOn = DateTime.UtcNow.AddMinutes(-id)
    };

    private static ApplicationDbContext NewContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static Activity Requested(int id, int typeId, int version, string dataJson) => new()
    {
        Id = id, ActivityTypeId = typeId, SchemaVersion = version,
        SubjectUserId = "trainee-1", CreatedByUserId = "trainee-1", CurrentState = "requested", DataJson = dataJson,
        CreatedOn = DateTime.UtcNow.AddDays(-1), UpdatedOn = DateTime.UtcNow.AddMinutes(-id)
    };

    private static bool NamesAField(ActorRule rule) => rule switch
    {
        FieldUserActorRule => true,
        CombinedActorRule combined => combined.Rules.Any(NamesAField),
        _ => false
    };
}
