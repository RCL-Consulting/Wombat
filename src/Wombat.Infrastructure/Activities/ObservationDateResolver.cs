using System.Globalization;
using System.Text.Json;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Curricula;

namespace Wombat.Infrastructure.Activities;

/// <summary>
/// The one implementation of "what date did this happen". (T119)
/// </summary>
/// <remarks>
/// Used by <see cref="ActivityService" />, which stamps on every write. It is a separate type rather
/// than a private method so that the next caller reuses it instead of writing a second one: two
/// implementations would drift, and the thing they would disagree about is which stage a completion was
/// graded against — the <see cref="ActorRuleMatcher" /> lesson restated.
/// </remarks>
internal static class ObservationDateResolver
{
    /// <summary>
    /// The encounter date an activity asserts, and where that date came from. (T119)
    /// </summary>
    /// <remarks>
    /// <para>
    /// The ONE implementation of "what date did this happen". Every reader reads the stamped column, so
    /// nothing else may answer this question — the <see cref="ActorRuleMatcher" /> lesson restated.
    /// </para>
    /// <para>
    /// Resolves from the PINNED schema, not the live one: a republish must not silently re-date an
    /// activity that is already in flight. A schema with no pointer, a pointer at a key the data does not
    /// carry, or a value that is not a date all fall back to the creation timestamp and say so through
    /// <see cref="ObservationDateSource.CreatedOn" />, rather than refusing — an activity with no stated
    /// encounter date is ordinary, not an error.
    /// </para>
    /// </remarks>
    public static (DateOnly ObservedOn, ObservationDateSource Source) Resolve(
        Activity activity,
        FormSchema schema,
        string dataJson)
    {
        // The South African date it was filed on, not the UTC one (T130). Credit buckets on this date and the
        // progress page reads "today" in South Africa. With a UTC fallback, an undated activity filed between
        // midnight and 02:00 on 1 July would land in semester 1 while the page was already showing semester 2.
        var fallback = (ProgrammeCalendar.DateOf(activity.CreatedOn), ObservationDateSource.CreatedOn);

        if (string.IsNullOrWhiteSpace(schema.ObservationDateField) || string.IsNullOrWhiteSpace(dataJson))
        {
            return fallback;
        }

        try
        {
            using var document = JsonDocument.Parse(dataJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object ||
                !document.RootElement.TryGetProperty(schema.ObservationDateField, out var value) ||
                value.ValueKind != JsonValueKind.String ||
                !DateOnly.TryParse(value.GetString(), CultureInfo.InvariantCulture, out var declared))
            {
                return fallback;
            }

            return (declared, ObservationDateSource.Declared);
        }
        catch (JsonException)
        {
            return fallback;
        }
    }

    /// <summary>Applies <see cref="Resolve" /> to the entity. Idempotent.</summary>
    public static void Stamp(Activity activity, FormSchema schema, string dataJson)
    {
        var (observedOn, source) = Resolve(activity, schema, dataJson);
        activity.ObservedOn = observedOn;
        activity.ObservedOnSource = source;
    }
}
