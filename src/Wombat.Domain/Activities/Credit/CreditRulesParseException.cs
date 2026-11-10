namespace Wombat.Domain.Activities.Credit;

/// <summary>
/// Credit rules the parser, or the check across them and the schema (<see cref="EvidenceEpa" />), refuses.
/// </summary>
/// <remarks>
/// A refusal that says where it is (T398, flow 17, Q6, J2): <see cref="Places" /> numbers the directive of each problem.
/// One thrown with its message alone has one place, the Credit tab's, with the message as its sentence: the summary line
/// only. <see cref="Exception.Message" /> is unchanged by the places.
/// </remarks>
public sealed class CreditRulesParseException : Exception, IActivityTypeRefusal
{
    public CreditRulesParseException(string message)
        : base(message)
    {
        Places = [new RefusalPlace(RefusalTab.Credit, RefusalKind.Other, message)];
    }

    /// <param name="message">The refusal's message, as before T398: every problem's sentence.</param>
    /// <param name="places">Where each problem is; never empty.</param>
    public CreditRulesParseException(string message, IReadOnlyList<RefusalPlace> places)
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
    internal static CreditRulesParseException At(RefusalPlace place) => new(place.Sentence, [place]);

    /// <summary>This refusal at <paramref name="place" />, when its thrower named none (T398, Q6); else as it is.</summary>
    internal CreditRulesParseException PlacedAt(Func<RefusalPlace, RefusalPlace> place)
        => HasOwnPlace ? this : At(place(Places[0]));
}
