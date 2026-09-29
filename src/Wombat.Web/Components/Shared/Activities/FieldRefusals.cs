namespace Wombat.Web.Components.Shared.Activities;

/// <summary>
/// One line of a refusal summary (T342, C8, T263): the refusal of one field, or a sentence about no field.
/// </summary>
/// <param name="FieldKey">The schema key of the field the line is about, or null for a sentence about no field.</param>
/// <param name="Text">The line as the summary shows it: "Date observed: The date cannot be after today (2026-09-29)."</param>
/// <param name="FieldMessage">
/// The same refusal as the field shows it under its control, without its label: "The date cannot be after today
/// (2026-09-29).". Null when the refusal's words do not name the field (the tool gate's "and 2 more"), so the field is
/// marked but carries no message of its own.
/// </param>
public sealed record RefusalLine(string? FieldKey, string Text, string? FieldMessage);

/// <summary>
/// A refusal of an activity form's data, split into one line per field (T342, C8).
/// </summary>
/// <remarks>
/// <para>
/// The server words a field refusal as one message, each field by its label ("Presenting problem: A value is required.
/// Assessor: A value is required."), and names the same fields by their keys beside it
/// (<c>ActivityFieldsRefusedException.FieldKeys</c>, in the order the message names them). The summary links a line per
/// field to its input, and the field shows its own part under the control, so the message is cut where each field's
/// label begins. The label is the form's own, so the cut is where the server put it; a label the message does not
/// contain gives the field a line of its label alone.
/// </para>
/// <para>
/// Words before the first label, and every message that names no field, are one line with no field.
/// </para>
/// </remarks>
public static class FieldRefusals
{
    /// <param name="message">The refusal as the server words it.</param>
    /// <param name="fieldKeys">The fields it names, by key, in the order it names them.</param>
    /// <param name="labelOf">A field's label by its key, or null for a key the form does not render.</param>
    public static IReadOnlyList<RefusalLine> Split(
        string? message,
        IReadOnlyCollection<string>? fieldKeys,
        Func<string, string?> labelOf)
    {
        ArgumentNullException.ThrowIfNull(labelOf);

        var text = message?.Trim() ?? string.Empty;
        var keys = fieldKeys ?? [];
        var starts = new List<(int Index, string Key, string Label)>();
        var unnamed = new List<(string Key, string Label)>();
        var from = 0;

        foreach (var key in keys)
        {
            var label = labelOf(key);
            if (string.IsNullOrWhiteSpace(label))
            {
                continue;
            }

            // Searched from the last field found, in the message's own order, so a label that recurs inside an earlier
            // field's words ("EPA" in "the EPA is closed") is not taken for the next field's start.
            var index = IndexOfLabel(text, label, from);
            if (index < 0)
            {
                index = IndexOfLabel(text, label, 0);
            }

            if (index < 0 || starts.Any(start => start.Index == index))
            {
                unnamed.Add((key, label));
                continue;
            }

            starts.Add((index, key, label));
            from = index + label.Length;
        }

        starts.Sort((left, right) => left.Index.CompareTo(right.Index));

        var lines = new List<RefusalLine>();
        var lead = (starts.Count == 0 ? text : text[..starts[0].Index]).Trim();
        if (lead.Length > 0)
        {
            lines.Add(new RefusalLine(null, lead, null));
        }

        for (var i = 0; i < starts.Count; i++)
        {
            var (index, key, label) = starts[i];
            var end = i + 1 < starts.Count ? starts[i + 1].Index : text.Length;
            var line = text[index..end].Trim();
            var fieldMessage = line[(label.Length + 1)..].Trim();
            lines.Add(new RefusalLine(key, line, fieldMessage.Length == 0 ? null : fieldMessage));
        }

        lines.AddRange(unnamed.Select(field => new RefusalLine(field.Key, field.Label, null)));
        return lines;
    }

    // "<label>:" at a word's start: at the message's start, or after a space.
    private static int IndexOfLabel(string text, string label, int from)
    {
        var needle = label + ":";
        var index = from;
        while (index <= text.Length - needle.Length)
        {
            index = text.IndexOf(needle, index, StringComparison.Ordinal);
            if (index < 0)
            {
                return -1;
            }

            if (index == 0 || char.IsWhiteSpace(text[index - 1]))
            {
                return index;
            }

            index++;
        }

        return -1;
    }
}
