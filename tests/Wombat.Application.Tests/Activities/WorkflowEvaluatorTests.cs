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
        var activity = CreateActivity(ActivityScope.Speciality, 120);

        _evaluator.Evaluate(CreateWorkflow("subject"), activity, "act", CreatePrincipal("subject-1"))
            .Allowed.Should().BeTrue();

        _evaluator.Evaluate(CreateWorkflow("creator"), activity, "act", CreatePrincipal("someone-else"))
            .Allowed.Should().BeFalse();

        _evaluator.Evaluate(CreateWorkflow("role:Assessor"), activity, "act", CreatePrincipal("u-1", roles: [WombatRoles.Assessor]))
            .Allowed.Should().BeTrue();

        _evaluator.Evaluate(CreateWorkflow("scope:speciality"), activity, "act", CreatePrincipal("u-1", specialityIds: [120]))
            .Allowed.Should().BeTrue();

        _evaluator.Evaluate(CreateWorkflow("subject+role:Assessor"), activity, "act", CreatePrincipal("subject-1", roles: [WombatRoles.Assessor]))
            .Allowed.Should().BeTrue();

        _evaluator.Evaluate(CreateWorkflow("creator|role:Assessor"), activity, "act", CreatePrincipal("u-1", roles: [WombatRoles.Assessor]))
            .Allowed.Should().BeTrue();
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

    private static Workflow CreateWorkflow(string actorRule)
        => new(
            1,
            "draft",
            [new WorkflowState("draft", "Draft", false), new WorkflowState("done", "Done", true)],
            [new WorkflowTransition("act", ["draft"], "done", ActorRuleParser.Parse(actorRule), false, [])]);

    private static Activity CreateActivity(ActivityScope scope, int scopeId)
        => new()
        {
            SubjectUserId = "subject-1",
            CreatedByUserId = "creator-1",
            CurrentState = "draft",
            ActivityType = new ActivityType
            {
                Scope = scope,
                ScopeId = scopeId
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
