namespace Wombat.Domain.Activities.Schema;

public sealed record FormField(
    string Key,
    FieldType Type,
    string Label,
    string? HelpText,
    bool Required,
    IReadOnlyList<string> Options,
    string? ScaleKey,
    FieldValidation? Validation,
    VisibilityCondition? ShowIf);
