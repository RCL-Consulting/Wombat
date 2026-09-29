namespace Wombat.Web.Components.Shared.Activities;

/// <summary>
/// The ids an activity form gives each field's parts (T342, C8), built from the field's schema key the way
/// <see cref="FieldHelp" /> builds a help text's: a key is free text, and an id list reads a key with a space in it as two
/// ids that name nothing (T193).
/// </summary>
/// <remarks>
/// The input's id is the key with <c>-in</c> after it, so a refusal summary's link (<c>href="#observed_on-in"</c>) lands
/// on the control itself, not on its wrapper, and a key can never collide with an id the page gives something else (the
/// field <c>refusal</c> beside the summary). A multi-choice field's fieldset carries the id, since it has no single
/// control.
/// </remarks>
public static class ActivityFieldIds
{
    /// <summary>The control's id: <c>observed_on-in</c>.</summary>
    public static string Input(string key) => $"{FieldHelp.IdPart(key)}-in";

    /// <summary>The id of the field's own refusal message under it: <c>observed_on-msg</c>.</summary>
    public static string Message(string key) => $"{FieldHelp.IdPart(key)}-msg";

    /// <summary>The id of the live region under the encounter date that holds its late-filing and programme notices.</summary>
    public static string FilingNotice(string key) => $"{FieldHelp.IdPart(key)}-filing-notice";

    /// <summary>The id of a section's heading, which the section is named by.</summary>
    public static string Section(string key) => $"sec-{FieldHelp.IdPart(key)}";
}
