namespace Wombat.Domain.Activities.Credit;

public sealed record CurriculumItemMatchRule(
    string? EpaField,
    int? CurriculumItemId,
    string? CurriculumItemField);
