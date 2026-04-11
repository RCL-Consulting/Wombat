namespace Wombat.Domain.Activities.Schema;

public sealed record FormSection(
    string Key,
    string Title,
    VisibilityCondition? ShowIf,
    IReadOnlyList<FormField> Fields);
