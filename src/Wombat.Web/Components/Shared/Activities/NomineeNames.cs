using Wombat.Application.Features.Activities.Services;

namespace Wombat.Web.Components.Shared.Activities;

/// <summary>
/// The name of the person a <c>user</c> field names, read from the nominee picker's own option, for "Submit to Fatima
/// Khumalo" and the lines that say whose inbox the activity goes to (T342, B2, C3). No round trip: the option is the one
/// the picker already holds.
/// </summary>
public static class NomineeNames
{
    /// <summary>The label the form gives a value that is no longer on the picker's list.</summary>
    public const string NotAvailableLabel = "Not available (choose someone else)";

    /// <summary>The ending the server gives a stored nominee who is no longer offered (T342, C4).</summary>
    public const string NotOnTheListSuffix = "(not on the current list)";

    /// <summary>
    /// The person's name, from the picker's option for <paramref name="value" />; null when nobody is named, or the one named
    /// is no longer on the list (a plain "Submit" then, as the Spec's rule says).
    /// </summary>
    /// <remarks>
    /// The directory labels a person "Fatima Khumalo (fatima@kgk)" (<c>NomineeDirectory.FormatLabel</c>): the name, then
    /// the address in brackets, which a button does not need. A label that is only an address is kept as it is.
    /// </remarks>
    public static string? NameOf(string? value, IEnumerable<ActivityCatalogueOption> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var label = options.FirstOrDefault(option => string.Equals(option.Value, value, StringComparison.Ordinal))?.Label;
        if (string.IsNullOrWhiteSpace(label) ||
            string.Equals(label, NotAvailableLabel, StringComparison.Ordinal) ||
            label.EndsWith(NotOnTheListSuffix, StringComparison.Ordinal))
        {
            return null;
        }

        return NameOfLabel(label);
    }

    /// <summary>"Fatima Khumalo" from "Fatima Khumalo (fatima@kgk)"; the label itself when it carries no address.</summary>
    public static string NameOfLabel(string label)
    {
        ArgumentNullException.ThrowIfNull(label);

        var trimmed = label.Trim();
        var open = trimmed.LastIndexOf(" (", StringComparison.Ordinal);
        return open > 0 && trimmed.EndsWith(')') && trimmed[(open + 2)..].Contains('@')
            ? trimmed[..open]
            : trimmed;
    }
}
