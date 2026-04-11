namespace Wombat.Domain.Activities.Credit;

public sealed record CreditRules(
    IReadOnlyList<CreditDirective> CountsFor);
