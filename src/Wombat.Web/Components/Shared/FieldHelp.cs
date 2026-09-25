namespace Wombat.Web.Components.Shared;

/// <summary>
/// The ids that tie a field's help text to its input (T193). <see cref="FormField" /> renders its help text with
/// <see cref="Id" />; the input in its slot names it with <see cref="DescribedBy" />.
/// </summary>
/// <remarks>
/// <para>
/// FormField cannot put <c>aria-describedby</c> on its input itself: the input is the caller's markup, which a component
/// renders but cannot change. So every caller that gives a field help text puts <see cref="DescribedBy" /> on the input,
/// passing the same help text, and a help text that comes and goes takes its reference with it. Without the link a
/// screen reader never reads the help with the field. <c>FormFieldHelpTextLinkTests</c> holds every caller to it.
/// </para>
/// <para>
/// A class of its own, not static members of FormField: every page imports <c>Wombat.Domain.Activities.Schema</c> as well,
/// whose <c>FormField</c> is the schema's field, so the name is ambiguous in any page's C#.
/// </para>
/// </remarks>
public static class FieldHelp
{
    /// <summary>The id of the help text of the field whose input is <paramref name="inputId" />.</summary>
    /// <remarks>
    /// Built with <see cref="IdPart" />: an activity form's input id is the builder's field key, which is free text, and
    /// <c>aria-describedby</c> reads a key with a space in it as two ids that name nothing.
    /// </remarks>
    public static string Id(string inputId) => $"{IdPart(inputId)}-help";

    /// <summary>
    /// <paramref name="value" /> with every character that is not safe in both an id and the space-separated id list
    /// <c>aria-describedby</c> reads replaced by <c>-</c>: only ASCII letters, digits, <c>-</c> and <c>_</c> are kept.
    /// Use it for any id built from free text that something names in an id list (T193), as the workflow actions do for
    /// a transition key.
    /// </summary>
    public static string IdPart(string value)
        => string.Concat(value.Select(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_' ? character : '-'));

    /// <summary>
    /// The input's <c>aria-describedby</c>: the help text's id when there is help text, then the id of each other region
    /// that describes the input (a warning, a refusal), in that order. Null when nothing describes it, so the attribute
    /// is left off rather than naming an element that is not there.
    /// </summary>
    public static string? DescribedBy(string inputId, string? helpText, params string?[] regionIds)
    {
        var ids = new List<string>(regionIds.Length + 1);
        if (!string.IsNullOrWhiteSpace(helpText))
        {
            ids.Add(Id(inputId));
        }

        foreach (var regionId in regionIds)
        {
            if (!string.IsNullOrWhiteSpace(regionId) && !ids.Contains(regionId, StringComparer.Ordinal))
            {
                ids.Add(regionId);
            }
        }

        return ids.Count == 0 ? null : string.Join(' ', ids);
    }
}
