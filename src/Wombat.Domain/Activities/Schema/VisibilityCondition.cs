namespace Wombat.Domain.Activities.Schema;

public sealed record VisibilityCondition(
    string Field,
    string Operator,
    string? Value);
