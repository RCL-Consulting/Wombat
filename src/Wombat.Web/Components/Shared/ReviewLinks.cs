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

    /// <summary>The trainee's list of her reviews: <c>/committee/my-reviews</c> (T366, flow 09).</summary>
    public const string MyReviews = "/committee/my-reviews";

    /// <summary>
    /// The trainee's page for one review of hers: <c>/committee/my-reviews/30</c> (T366, F8). Only the review's own trainee
    /// opens it, and only once it is ratified or later; an Administrator reads a review at <see cref="Review" />.
    /// </summary>
    public static string MyReview(int reviewId) => Invariant($"{MyReviews}/{reviewId}");

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

    /// <summary>The schedule page, owned by Committee reviews (T364, flow 08): <c>/committee/reviews/new</c>.</summary>
    public const string ScheduleNew = "/committee/reviews/new";

    /// <summary>The query's key naming the page Cancel returns to: <c>return=/committee/decisions-due?…</c> (T364, C4).</summary>
    public const string ReturnKey = "return";

    /// <summary>The pages the schedule page may return to; anything else returns to Committee reviews (T364, C4).</summary>
    private static readonly string[] ScheduleReturnPages = ["/committee/decisions-due", "/committee/reviews"];

    /// <summary>
    /// The schedule page, filled: <c>/committee/reviews/new?panel=10&amp;trainee=…&amp;period=2026-1&amp;return=…</c>
    /// (T364, flow 08; round 2 C4, F1). Each part is left out when null; <paramref name="period" /> is the period's key as
    /// Decisions due writes it (<c>2026-1</c>), always the row's own period, an ended one included (F1).
    /// <paramref name="returnTo" /> is the address Cancel goes back to, with its filters.
    /// </summary>
    public static string Schedule(int? panelId, string? traineeUserId, string? period, string? returnTo)
    {
        var parts = new List<string>(4);
        if (panelId is int panel)
        {
            parts.Add(Invariant($"panel={panel}"));
        }

        if (!string.IsNullOrEmpty(traineeUserId))
        {
            parts.Add($"trainee={Uri.EscapeDataString(traineeUserId)}");
        }

        if (!string.IsNullOrEmpty(period))
        {
            parts.Add($"period={Uri.EscapeDataString(period)}");
        }

        if (!string.IsNullOrEmpty(returnTo))
        {
            parts.Add($"{ReturnKey}={Uri.EscapeDataString(returnTo)}");
        }

        return parts.Count == 0 ? ScheduleNew : $"{ScheduleNew}?{string.Join('&', parts)}";
    }

    /// <summary>
    /// Where the schedule page's Cancel goes: <paramref name="returnTo" /> when it is Decisions due or Committee reviews on
    /// this site, with or without a query; else Committee reviews. Only a path of ours is ever followed, so a crafted
    /// <c>return=</c> cannot send the reader to another site (an open redirect) or to any other page.
    /// </summary>
    public static string ScheduleReturn(string? returnTo)
    {
        if (string.IsNullOrEmpty(returnTo) || returnTo.Contains('\\') || returnTo.Contains('#') ||
            returnTo.Any(char.IsControl))
        {
            return "/committee/reviews";
        }

        var query = returnTo.IndexOf('?', StringComparison.Ordinal);
        var path = query >= 0 ? returnTo[..query] : returnTo;
        return ScheduleReturnPages.Contains(path, StringComparer.Ordinal) ? returnTo : "/committee/reviews";
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
