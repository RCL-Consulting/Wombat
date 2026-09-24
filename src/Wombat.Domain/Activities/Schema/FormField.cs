using Wombat.Domain.Activities.Workflow;

namespace Wombat.Domain.Activities.Schema;

/// <param name="NomineeRole">
/// The role a <c>user</c> field's nominee must hold, serialised as <c>role</c> (T102). Null on every other field
/// type, and on a <c>user</c> field that does not say, which means <see cref="Identity.WombatRoles.Assessor" /> —
/// read it through <see cref="ActorFieldRules.RequiredRolesByNomineeField" />, never directly.
/// </param>
public sealed record FormField(
    string Key,
    FieldType Type,
    string Label,
    string? HelpText,
    bool Required,
    IReadOnlyList<string> Options,
    string? CatalogueKey,
    string? ScaleKey,
    string? NomineeRole,
    FieldValidation? Validation,
    VisibilityCondition? ShowIf,
    ActorRule? EditableBy);
