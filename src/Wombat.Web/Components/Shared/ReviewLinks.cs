using System.Globalization;

namespace Wombat.Web.Components.Shared;

/// <summary>
/// The addresses between a committee review and flow 03's activity page (T361, flow 07; round 2 review 9, round 3 item 9).
/// The review's snapshot opens an activity with <see cref="ActivityFromReview" />, and the activity page reads it back with
/// <see cref="TryReadReturn" /> and offers "Back to Lerato Molefe · 2026 S2", which goes to <see cref="SnapshotLine" />.
/// One place builds and reads them, so the two pages cannot drift apart.
/// </summary>
public static class ReviewLinks
{
    /// <summary>The query's key naming the review the activity was opened from: <c>from=review-30</c>.</summary>
    public const string FromKey = "from";

    /// <summary>The query's key naming the snapshot line it was opened from: <c>at=snap-12</c>.</summary>
    public const string AtKey = "at";

    private const string ReviewPrefix = "review-";
    private const string SnapshotPrefix = "snap-";

    /// <summary>The review's page: <c>/committee/reviews/30</c>.</summary>
    public static string Review(int reviewId) => Invariant($"/committee/reviews/{reviewId}");

    /// <summary>
    /// The review's page at one snapshot line: <c>/committee/reviews/30#snap-12</c>. The review page gives each line
    /// <c>id="snap-{snapshotItemId}"</c> (<see cref="SnapshotLineId" />).
    /// </summary>
    public static string SnapshotLine(int reviewId, int snapshotItemId) => $"{Review(reviewId)}#{SnapshotLineId(snapshotItemId)}";

    /// <summary>The id of a snapshot line on the review page, the fragment <see cref="SnapshotLine" /> lands on.</summary>
    public static string SnapshotLineId(int snapshotItemId) => Invariant($"{SnapshotPrefix}{snapshotItemId}");

    /// <summary>
    /// The snapshot line an address of the review page is opened at, <c>snap-12</c> from <c>…/reviews/30#snap-12</c>, or
    /// null when the address has no fragment of that form. The page draws its lines only once the review is read, after the
    /// navigation, so the browser never scrolls to the fragment itself: the page takes the focus there (4.17, T361 replay).
    /// </summary>
    public static string? SnapshotLineIn(string uri)
    {
        var hash = uri.IndexOf('#', StringComparison.Ordinal);
        return hash >= 0 && TryReadId(uri[(hash + 1)..], SnapshotPrefix, out var itemId) ? SnapshotLineId(itemId) : null;
    }

    /// <summary>
    /// Flow 03's activity page, opened from a review's snapshot line:
    /// <c>/activities/12?from=review-30&amp;at=snap-12</c>.
    /// </summary>
    public static string ActivityFromReview(int activityId, int reviewId, int snapshotItemId)
        => Invariant($"/activities/{activityId}?{FromKey}={ReviewPrefix}{reviewId}&{AtKey}={SnapshotPrefix}{snapshotItemId}");

    /// <summary>
    /// Reads <c>?from=review-{id}&amp;at=snap-{itemId}</c> back. True with the review's id when <paramref name="from" /> is
    /// "review-" and a positive integer in plain digits, nothing else; the line's id likewise from <paramref name="at" />,
    /// null when it is absent or not of that form, so a forged line reads as no line. Nothing here says the reader may see
    /// the review: the page asks for it, and shows the link only when it is read.
    /// </summary>
    public static bool TryReadReturn(string? from, string? at, out int reviewId, out int? snapshotItemId)
    {
        snapshotItemId = TryReadId(at, SnapshotPrefix, out var itemId) ? itemId : null;
        if (TryReadId(from, ReviewPrefix, out reviewId))
        {
            return true;
        }

        snapshotItemId = null;
        return false;
    }

    // Digits only: int.Parse alone takes a sign, white space and a leading "+", none of which an address of ours carries.
    private static bool TryReadId(string? value, string prefix, out int id)
    {
        id = 0;
        if (value is null || !value.StartsWith(prefix, StringComparison.Ordinal))
        {
            return false;
        }

        var digits = value.AsSpan(prefix.Length);
        if (digits.Length is 0 or > 9)
        {
            return false;
        }

        foreach (var character in digits)
        {
            if (character is < '0' or > '9')
            {
                return false;
            }
        }

        id = int.Parse(digits, NumberStyles.None, CultureInfo.InvariantCulture);
        return id > 0;
    }

    private static string Invariant(FormattableString value) => value.ToString(CultureInfo.InvariantCulture);
}
