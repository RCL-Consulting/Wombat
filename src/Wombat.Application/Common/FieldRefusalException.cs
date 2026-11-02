namespace Wombat.Application.Common;

/// <summary>
/// A command's refusal that belongs to one field of the form that sent it (T392, flow 15, F4, S6): an EPA's code that is a
/// real duplicate, a sub-speciality the institution has not adopted, an item's EPA another item holds, a curriculum a
/// registrar cannot be moved into. The page puts <see cref="Exception.Message" /> under the control <see cref="Field" />
/// names, first in its <c>aria-describedby</c>, and keeps what was typed; a refusal with no field stays in the page's alert.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Field" /> is the command's property name (<c>nameof(CreateEpaCommand.Code)</c>), the same key a FluentValidation
/// failure carries as its <c>PropertyName</c>, so a page maps both kinds to its fields one way (<c>FieldRefusal.Of</c> in Web).
/// The message is the sentence the page shows, written for the person who pressed; it names no person and no record out
/// of the caller's scope, since the audit pipeline writes it into the caller's audit row. It may name a national catalogue
/// entry the caller's institution has not adopted (a discipline, N-5): the catalogue's names are national and not secret.
/// </para>
/// <para>
/// Derived from <see cref="InvalidOperationException" />, as <c>ActivityFieldsRefusedException</c> and
/// <c>DataRightsRefusedException</c> are, so that nothing that already catches a refusal changes: the audit pipeline, which
/// discards the refused save and records the message (T201), and every test that expects an
/// <see cref="InvalidOperationException" />. Thrown where the commands throw today, before anything is staged, or wrapping a
/// database refusal (<paramref name="inner" />), so a refused save is still discarded.
/// </para>
/// </remarks>
public sealed class FieldRefusalException : InvalidOperationException
{
    public FieldRefusalException(string field, string message, Exception? inner = null)
        : base(message, inner)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        Field = field;
    }

    /// <summary>The refused command's property the refusal is about, by its C# name (<c>"SubSpecialityId"</c>, <c>"Code"</c>).</summary>
    public string Field { get; }
}
