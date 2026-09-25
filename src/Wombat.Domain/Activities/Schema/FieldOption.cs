namespace Wombat.Domain.Activities.Schema;

/// <summary>
/// One answer a <c>choice</c>, <c>multichoice</c> or <c>scale</c> field offers (T191): the value stored on the activity,
/// and the words the form and the portfolio show for it.
/// </summary>
/// <remarks>
/// Written in a schema as a bare string, whose value is its label (<c>"1"</c>), or as
/// <c>{ "value": "admission_notes", "label": "Admission notes" }</c>. <see cref="FormSchemaParser.Serialize" /> writes
/// the bare string whenever the two agree, so a schema that labels nothing canonicalises exactly as it did before labels
/// existed and the seed refresher sees no difference in it. Only the value is ever stored, compared or validated: a
/// label can be reworded without touching an activity.
/// </remarks>
public sealed record FieldOption(string Value, string Label)
{
    /// <summary>
    /// What separates an option's value from its label on a line of the builder's Options box: <c>picu | PICU</c>.
    /// </summary>
    /// <remarks>
    /// <see cref="FormSchemaParser" /> refuses it in a value, so the first bar on a line always ends the value and a
    /// label may hold bars of its own. That, and one line per option, is what makes the box an exact round trip of every
    /// schema that parses.
    /// </remarks>
    public const char LabelSeparator = '|';

    /// <summary>An option whose label is its value: the bare-string form.</summary>
    public static FieldOption Unlabelled(string value) => new(value, value);

    /// <summary>True when the option shows words of its own, which is when it serialises as an object.</summary>
    public bool IsLabelled => !string.Equals(Value, Label, StringComparison.Ordinal);
}
