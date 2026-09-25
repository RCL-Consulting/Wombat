using System.Security.Claims;
using FluentAssertions;
using Wombat.Application.Common.Security;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;

namespace Wombat.Application.Tests.Activities;

public sealed class WorkflowEvaluatorTests
{
    private readonly WorkflowEvaluator _evaluator = new();

    [Fact]
    public void Evaluate_CoversSubjectCreatorRoleScopeAnyAndAllRules()
    {
        var activity = CreateActivity(ActivityScope.Speciality, 120, institutionId: 7, specialityId: 120);

        _evaluator.Evaluate(CreateWorkflow("subject"), activity, "act", CreatePrincipal("subject-1"))
            .Allowed.Should().BeTrue();

        _evaluator.Evaluate(CreateWorkflow("creator"), activity, "act", CreatePrincipal("someone-else"))
            .Allowed.Should().BeFalse();

        _evaluator.Evaluate(CreateWorkflow("role:Assessor"), activity, "act", CreatePrincipal("u-1", roles: [WombatRoles.Assessor]))
            .Allowed.Should().BeTrue();

        // Both claims: a Speciality is College-owned and therefore national, so T101 conjoins the
        // institution — the speciality claim alone is not a match.
        _evaluator.Evaluate(CreateWorkflow("scope:speciality"), activity, "act", CreatePrincipal("u-1", institutionId: 7, specialityIds: [120]))
            .Allowed.Should().BeTrue();

        _evaluator.Evaluate(CreateWorkflow("subject+role:Assessor"), activity, "act", CreatePrincipal("subject-1", roles: [WombatRoles.Assessor]))
            .Allowed.Should().BeTrue();

        _evaluator.Evaluate(CreateWorkflow("creator|role:Assessor"), activity, "act", CreatePrincipal("u-1", roles: [WombatRoles.Assessor]))
            .Allowed.Should().BeTrue();
    }

    /// <summary>
    /// T101. A <c>scope:</c> rule asks where the SUBJECT trains, not which programme published the
    /// tool. Before T101 it compared against <c>ActivityType.ScopeId</c>, so the admin of the tool's
    /// speciality — who has no relationship to the trainee — matched, and the admin who actually
    /// oversees the trainee did not. The dev database contains exactly this shape.
    /// </summary>
    [Fact]
    public void Evaluate_ScopeRulesFollowTheSubjectNotTheActivityType()
    {
        // A tool published by speciality 7, filed about a trainee who trains in speciality 2 at
        // institution 5.
        var activity = CreateActivity(ActivityScope.Speciality, 7, institutionId: 5, specialityId: 2);
        var workflow = CreateWorkflow("role:SpecialityAdmin+scope:speciality");

        var overseesTheTrainee = CreatePrincipal("admin-2", institutionId: 5, specialityIds: [2], roles: [WombatRoles.SpecialityAdmin]);
        var ownsTheToolOnly = CreatePrincipal("admin-7", institutionId: 5, specialityIds: [7], roles: [WombatRoles.SpecialityAdmin]);

        // Right speciality, wrong hospital: a national speciality id is not on its own a grant.
        var rightSpecialityElsewhere = CreatePrincipal("admin-2b", institutionId: 6, specialityIds: [2], roles: [WombatRoles.SpecialityAdmin]);

        _evaluator.Evaluate(workflow, activity, "act", overseesTheTrainee).Allowed.Should().BeTrue();
        _evaluator.Evaluate(workflow, activity, "act", ownsTheToolOnly).Allowed.Should().BeFalse();
        _evaluator.Evaluate(workflow, activity, "act", rightSpecialityElsewhere).Allowed.Should().BeFalse();
    }

    /// <summary>
    /// Fail closed: an activity with no stamp satisfies no <c>scope:</c> rule, rather than matching
    /// everyone or falling back to the activity type.
    /// </summary>
    [Fact]
    public void Evaluate_AnUnstampedActivitySatisfiesNoScopeRule()
    {
        var activity = CreateActivity(ActivityScope.Speciality, 7);
        var workflow = CreateWorkflow("role:SpecialityAdmin+scope:speciality");

        _evaluator.Evaluate(workflow, activity, "act",
            CreatePrincipal("admin-7", institutionId: 5, specialityIds: [7], roles: [WombatRoles.SpecialityAdmin]))
            .Allowed.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_IsDeterministicAcrossRandomizedInputs()
    {
        var random = new Random(1234);

        for (var index = 0; index < 1000; index++)
        {
            var scope = (ActivityScope)random.Next(0, 4);
            var scopeId = random.Next(1, 5);
            var activity = CreateActivity(scope, scopeId);
            var workflow = CreateWorkflow(index % 2 == 0 ? "role:Assessor|subject" : "scope:institution+role:Coordinator");
            var principal = CreatePrincipal(
                random.Next(0, 2) == 0 ? "subject-1" : "other",
                institutionId: random.Next(1, 5),
                specialityIds: [random.Next(1, 5)],
                subSpecialityIds: [random.Next(1, 5)],
                roles: random.Next(0, 2) == 0 ? [WombatRoles.Assessor] : [WombatRoles.Coordinator]);

            var first = _evaluator.Evaluate(workflow, activity, "act", principal);
            var second = _evaluator.Evaluate(workflow, activity, "act", principal);

            first.Should().BeEquivalentTo(second);
        }
    }

    [Fact]
    public void Evaluate_FieldActorRuleMatchesUserIdStoredInActivityData()
    {
        var activity = CreateActivity(ActivityScope.Speciality, 120);
        activity.DataJson = """{ "assessor_user_id": "assessor-1" }""";

        _evaluator.Evaluate(CreateWorkflow("field:assessor_user_id"), activity, "act", CreatePrincipal("assessor-1"))
            .Allowed.Should().BeTrue();

        _evaluator.Evaluate(CreateWorkflow("field:assessor_user_id"), activity, "act", CreatePrincipal("someone-else"))
            .Allowed.Should().BeFalse();
    }

    /// <summary>
    /// T189. The refusal of a move the state does not offer names the move as its button does and the state by its label,
    /// the wording <c>ActivityService</c>'s own lookups share. A state the workflow does not declare keeps its key.
    /// </summary>
    [Fact]
    public void Evaluate_AMoveTheStateDoesNotOffer_IsRefused_NamingTheMoveAndTheStateByTheirLabels()
    {
        var principal = CreatePrincipal("subject-1");
        var activity = CreateActivity(ActivityScope.Global, 0);

        var refused = _evaluator.Evaluate(CreateWorkflow("subject"), activity, "sign_off", principal);
        refused.Allowed.Should().BeFalse();
        refused.Reason.Should().Be("Sign Off is not available while the activity is Draft.");

        activity.CurrentState = "archived";
        _evaluator.Evaluate(CreateWorkflow("subject"), activity, "act", principal)
            .Reason.Should().Be("Act is not available while the activity is archived.");
    }

    private static Workflow CreateWorkflow(string actorRule)
        => new(
            1,
            "draft",
            [new WorkflowState("draft", "Draft", false, null), new WorkflowState("done", "Done", true, null)],
            [new WorkflowTransition("act", ["draft"], "done", ActorRuleParser.Parse(actorRule), false, [], TransitionValidation.All)]);

    /// <summary>
    /// <paramref name="typeScope" />/<paramref name="typeScopeId" /> are the ACTIVITY TYPE's scope —
    /// who may offer the tool. The stamps are the ACTIVITY's own scope — where the subject trains.
    /// T101 made <c>scope:</c> rules read the stamps, so the two are separate parameters here on
    /// purpose; see <see cref="Evaluate_ScopeRulesFollowTheSubjectNotTheActivityType" />.
    /// </summary>
    private static Activity CreateActivity(
        ActivityScope typeScope,
        int typeScopeId,
        int? institutionId = null,
        int? specialityId = null,
        int? subSpecialityId = null)
        => new()
        {
            SubjectUserId = "subject-1",
            CreatedByUserId = "creator-1",
            CurrentState = "draft",
            InstitutionId = institutionId,
            SpecialityId = specialityId,
            SubSpecialityId = subSpecialityId,
            ActivityType = new ActivityType
            {
                Scope = typeScope,
                ScopeId = typeScopeId
            }
        };

    private static ClaimsPrincipal CreatePrincipal(
        string userId,
        int? institutionId = null,
        IReadOnlyCollection<int>? specialityIds = null,
        IReadOnlyCollection<int>? subSpecialityIds = null,
        IReadOnlyCollection<string>? roles = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId)
        };

        if (institutionId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, institutionId.Value.ToString()));
        }

        foreach (var specialityId in specialityIds ?? [])
        {
            claims.Add(new Claim(WombatClaimTypes.SpecialityId, specialityId.ToString()));
        }

        foreach (var subSpecialityId in subSpecialityIds ?? [])
        {
            claims.Add(new Claim(WombatClaimTypes.SubSpecialityId, subSpecialityId.ToString()));
        }

        foreach (var role in roles ?? [])
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}
