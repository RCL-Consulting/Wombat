namespace Wombat.Domain.Activities.Workflow;

public abstract record ActorRule;

public sealed record SubjectUserActorRule() : ActorRule;

public sealed record CreatorUserActorRule() : ActorRule;

public sealed record NamedRoleActorRule(string Role) : ActorRule;

public sealed record ScopeMatchActorRule(string Scope) : ActorRule;

public sealed record FieldUserActorRule(string Field) : ActorRule;

public sealed record CombinedActorRule(
    ActorRuleCombinationKind CombinationKind,
    IReadOnlyList<ActorRule> Rules) : ActorRule;

public enum ActorRuleCombinationKind
{
    All = 0,
    Any = 1
}
