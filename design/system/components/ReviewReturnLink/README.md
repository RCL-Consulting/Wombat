# ReviewReturnLink

The committee review's way back (`Components/Shared/ReviewLinks.cs`; drawn by `Pages/Activities/ActivityView.razor`): a snapshot line on the review opens flow 03's activity page with the review in its address, and the activity page offers "Back to Lerato Molefe · 2026 S2", which lands on that snapshot line. One class builds and reads both addresses, so the two pages cannot drift apart. Flow 07 (T361, 2026-10-12, 44de180b, with 59bb72fd; round 2 review 9, round 3 item 9) made it. Until then the activity page offered only its own list's way back, and the committee member found the review again by the menu.

## The addresses (`ReviewLinks`)

- **Out**: a snapshot line's item links to `/activities/{activityId}?from=review-{reviewId}&at=snap-{snapshotItemId}` (`ActivityFromReview`).
- **Back**: `/committee/reviews/{reviewId}#snap-{snapshotItemId}` (`SnapshotLine`), each snapshot line on the review carrying `id="snap-{snapshotItemId}"` (`SnapshotLineId`). A line that is not the review's lands on the review's top (`Review`).
- **Read back** (`TryReadReturn`): "review-" and a positive integer in plain digits, nothing else (no sign, space or "+"); the line likewise, null when absent or malformed, so a forged line reads as no line.
- **Landing** (59bb72fd): the review draws its lines only once it is read, after the navigation, so the browser never scrolls to the fragment; the page reads it (`SnapshotLineIn`) and takes the focus at that line, clear of the bars (`scroll-margin-top`).

## On the activity page

```html
<div class="activity-page activity-page--from-review">
  <a class="return-link" href="/committee/reviews/30#snap-12"><svg class="icon">arrow-left</svg>Back to Lerato Molefe · 2026 S2</a>
  <!-- PageHeader, then the page as flow 03 and 04 built it -->
</div>
```

- **`a.return-link`** above the header: inline-flex, the 16px `arrow-left` icon 4px before the words, weight 600, 8px under it, at least 24px tall.
- **The words**: "Back to " the trainee and the period, as the review's h1 names it ("Back to the committee review · 2026 S2" where the review names no trainee).
- **It replaces the page's own way back**: the way on's back button reads the same (WayOn), and below 641px `.activity-page--from-review` hides the trail (`nav:has(> .breadcrumbs)`), so the return link is the one back link, a 44px row (Breadcrumbs). From 641px the trail stays, by the owner table.
- **Only for a reader the review reads for**: the page asks `GetCommitteeReviewByIdQuery` as the reader, before it draws the loaded page. A forged, unknown or unreadable id gets nothing, the same nothing (T194), and the page is as it was; a read that fails for any other reason is logged and leaves the page's own link. It is never a load error: it says nothing about the activity.

## Rules (DESIGN.md § Record page with a workflow, "Opened from a committee review's snapshot")

- One back link, never two.
- The address carries the way back; nothing is remembered in the browser.
- It names nobody to a reader who may not read the review.

## Contrast

`link-color` 6.36:1 on the page's background (6.70:1 on a card).

## Known gaps

- Only the review's snapshot links carry it; an activity reached from the review's evidence reader is read in place (EvidenceReader) and needs none, but one reached from the trajectory table or the standing panel's rating links goes back by the activity page's own list.
