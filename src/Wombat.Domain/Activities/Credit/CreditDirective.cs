namespace Wombat.Domain.Activities.Credit;

public sealed record CreditDirective(
    CurriculumItemMatchRule CurriculumItemMatchRule,
    int Amount,
    string? MinimumLevelField,
    string? MinimumLevelFixed);
