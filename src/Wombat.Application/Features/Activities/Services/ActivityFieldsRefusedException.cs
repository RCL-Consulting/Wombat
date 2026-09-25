namespace Wombat.Application.Features.Activities.Services;

/// <summary>
/// A refused write of an activity's data that names the form fields it is about (T263): a required field left empty, a
/// value in the wrong format, an encounter date out of bounds, a nominee the directory does not list, an EPA the
/// instrument may not be filed against, a field that names the subject, a field the actor may not change.
/// </summary>
/// <remarks>
/// <para>
/// Its message is the refusal as the page shows it, each field named by its label ("Presenting problem: A value is
/// required."). <see cref="FieldKeys" /> names the same fields by their schema keys, which are the ids of the form's
/// controls, so the page that shows the refusal can mark those controls as well as name them.
/// </para>
/// <para>
/// Derived from <see cref="InvalidOperationException" />, as <c>MsfResponseRefusedException</c> is, so that nothing that
/// already catches a refusal changes: the pages, the audit pipeline, which records the message, and every test that
/// expects one. A refusal that is about no field (a move the state does not offer, a missing note) stays a plain
/// <see cref="InvalidOperationException" />.
/// </para>
/// </remarks>
public sealed class ActivityFieldsRefusedException : InvalidOperationException
{
    public ActivityFieldsRefusedException(string message, IEnumerable<string?> fieldKeys)
        : base(message)
    {
        ArgumentNullException.ThrowIfNull(fieldKeys);

        FieldKeys = fieldKeys
            .Where(key => !string.IsNullOrEmpty(key))
            .Select(key => key!)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>
    /// The schema keys of the fields the refusal is about, once each, in the order the refusal names them. Empty when the
    /// refusal names only errors that belong to no field.
    /// </summary>
    /// <remarks>
    /// Every field the refusal is about, which can be more than its message names: the tool gate names its first few
    /// refused items and counts the rest (<c>ToolPermissionGate.MaxItemsNamed</c>), and carries the field of each, so a
    /// field behind "cannot be used for 2 more EPAs" is marked as well. Those come after the ones the message names.
    /// </remarks>
    public IReadOnlyList<string> FieldKeys { get; }

    /// <summary>The fields <paramref name="exception" /> names, if it is this refusal; none for any other exception.</summary>
    public static IReadOnlyList<string> FieldKeysOf(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is ActivityFieldsRefusedException refused ? refused.FieldKeys : [];
    }
}
