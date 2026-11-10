namespace Wombat.Domain.Activities.Schema;

/// <summary>
/// A form schema the parser, or a check across the schema and the workflow (<see cref="ActorFieldRules" />), refuses.
/// </summary>
/// <remarks>
/// A refusal that says where it is (T398, flow 17, Q6, J2): <see cref="Places" /> names the section and fields of each
/// problem, so the builder can word it at its row. One thrown with its message alone has one place, the Form tab's, with
/// the message as its sentence: the summary line only. <see cref="Exception.Message" /> is unchanged by the places.
/// </remarks>
public sealed class SchemaParseException : Exception, IActivityTypeRefusal
{
    public SchemaParseException(string message)
        : base(message)
    {
        Places = [new RefusalPlace(RefusalTab.Form, RefusalKind.Other, message)];
    }

    /// <param name="message">The refusal's message, as before T398: every problem's sentence.</param>
    /// <param name="places">Where each problem is; never empty.</param>
    public SchemaParseException(string message, IReadOnlyList<RefusalPlace> places)
        : base(message)
    {
        ArgumentNullException.ThrowIfNull(places);
        if (places.Count == 0)
        {
            throw new ArgumentException("A placed refusal names at least one place.", nameof(places));
        }

        Places = places;
        HasOwnPlace = true;
    }

    public IReadOnlyList<RefusalPlace> Places { get; }

    /// <summary>Whether the thrower named a place; false for the fallback the message alone gives.</summary>
    internal bool HasOwnPlace { get; }

    /// <summary>One problem at one place, its sentence the message.</summary>
    internal static SchemaParseException At(RefusalPlace place) => new(place.Sentence, [place]);

    /// <summary>
    /// This refusal at <paramref name="place" />, when its thrower named none: an enclosing section or field knows where a
    /// helper's refusal is (T398, Q6). A refusal that already has its place is kept as it is.
    /// </summary>
    internal SchemaParseException PlacedAt(Func<RefusalPlace, RefusalPlace> place)
        => HasOwnPlace ? this : At(place(Places[0]));
}
