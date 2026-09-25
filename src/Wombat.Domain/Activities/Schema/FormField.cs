using Wombat.Domain.Activities.Workflow;

namespace Wombat.Domain.Activities.Schema;

/// <param name="Options">
/// The answers the field offers, each a stored value and the label shown for it (T191). Validation compares the value
/// only; read what a stored value should read as through <see cref="LabelFor" />, never by printing the value.
/// </param>
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
    IReadOnlyList<FieldOption> Options,
    string? CatalogueKey,
    string? ScaleKey,
    string? NomineeRole,
    FieldValidation? Validation,
    VisibilityCondition? ShowIf,
    ActorRule? EditableBy)
{
    /// <summary>True when <paramref name="value" /> is the stored value of one of <see cref="Options" />.</summary>
    public bool Offers(string value)
        => Options.Any(option => string.Equals(option.Value, value, StringComparison.Ordinal));

    /// <summary>
    /// The words to show for a stored <paramref name="value" />: its option's label, or the value itself when no option
    /// declares it (a field with no options, or a value from a version that offered it and this one does not).
    /// </summary>
    public string LabelFor(string value)
        => Options.FirstOrDefault(option => string.Equals(option.Value, value, StringComparison.Ordinal))?.Label ?? value;
}
