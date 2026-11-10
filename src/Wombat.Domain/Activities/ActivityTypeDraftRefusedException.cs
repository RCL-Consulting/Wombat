namespace Wombat.Domain.Activities;

/// <summary>
/// A draft refused on more than one tab at once (T398, flow 17, P2: "every tab at once", the operator's 2026-11-10):
/// every place the three payloads and the two cross-checks found, so one save names every problem rather than the first
/// payload's. Its message is the places' sentences joined by a space, each as its own refusal's message says it.
/// </summary>
/// <remarks>
/// An <see cref="InvalidOperationException" />, as the cross-checks' own refusals are, so a caller that reads a refusal's
/// message as before still reads one. The builder reads <see cref="Places" />.
/// </remarks>
public sealed class ActivityTypeDraftRefusedException : InvalidOperationException, IActivityTypeRefusal
{
    public ActivityTypeDraftRefusedException(IReadOnlyList<RefusalPlace> places)
        : base(MessageOf(places))
    {
        Places = places;
    }

    public IReadOnlyList<RefusalPlace> Places { get; }

    private static string MessageOf(IReadOnlyList<RefusalPlace> places)
    {
        ArgumentNullException.ThrowIfNull(places);
        if (places.Count == 0)
        {
            throw new ArgumentException("A refused draft names at least one place.", nameof(places));
        }

        return string.Join(" ", places.Select(place => place.Sentence));
    }
}
