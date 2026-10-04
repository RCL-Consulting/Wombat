# StatePanel

The wrapper every list and dashboard uses to render its three non-content states explicitly: loading as skeletons, an error as a danger Alert, and empty as a dashed card with a title, a line of copy and an optional action.

## What the consumer provides

```razor
<StatePanel IsLoading="_loading" LoadError="@_error" OnRetry="LoadAsync" FocusAfterRetry="FocusContentAsync"
            IsEmpty="@(_page is { TotalCount: 0 } && _needsYou.Count == 0)" EmptyTitle="No activities yet"
            EmptyBody="Log an activity to ask an assessor to rate an encounter, or to log a teaching session.">
  <EmptyActions><a class="btn btn-outline" href="/activities/new">Log an activity</a></EmptyActions>
  <ChildContent>…the content…</ChildContent>
</StatePanel>
```

- `IsLoading`: renders `SkeletonCount` (4) `Skeleton`s of `SkeletonHeight` (3rem), or `LoadingContent` (flow 03, T342), the page's own skeleton in the shape of what loads: the activity page's is a status card (three skeleton lines in `.activity-status--others`) over a details grid of two cards, all `aria-hidden`. A page whose skeleton says nothing to a screen reader keeps a visually hidden `role="status"` line on the page, always there and filled while it loads ("Loading the activity.", "Loading what you can file.", "Loading the Activity inbox.": flow 04, whose skeleton is each section's heading over its rows; "Loading My progress.", "Loading this EPA.": flow 05).
- `LoadError`: renders `<Alert Kind="danger">` with the message.
- `OnRetry` (T339, flow 02, B12): set, the failure offers **Try again** beside its words, as Home's does: the Alert's words and an outline `.btn-sm` with `refresh-cw` in an `.alert-row`, inside an `ActionResult`. Pressed, the page reads again (the skeletons while it runs, so the button is gone); once the answer is drawn the focus moves: a failure again, to the alert drawn again, so it is read again; a success, to `FocusAfterRetry`, the page's own place for its content (My account's Account heading), which a page may leave empty when it has a result to say that takes the focus itself; an empty answer focuses the empty state, the region that replaces the error. Unset, the failure is its words alone, as before. `FocusFailureAfterRender()` (flow 04, A3) is for a page whose own later read fails, a page turn: the failure replaces the content and the pressed button with it, so once drawn it takes the focus, as a failed Try again's does. Below 641px Try again is 44px.
- `IsEmpty`: renders `div.detail-card.detail-card--empty` (`tabindex="-1"`, so a Try again answered by the empty state can focus it; flow 04, T350 build review, D4) with a `.state-panel-title` (`EmptyTitle`, default "Nothing here yet"), a `.state-panel-copy` (`EmptyBody`, default "There is no data to show yet.") and `EmptyActions` in a `.form-actions` row.
- The content, rendered otherwise.

**A section's own failure** (flow 05, C8): a part of a page that reads on its own draws its own StatePanel, so its failure replaces only that part and the rest of the page stands. My progress's standing panel does (EntrustmentStandingPanel: "Could not load your standing. Nothing has changed. Try again, or come back in a few minutes." with Try again under the section's heading, answering into it), and the index beside it says "Not loaded" in each verdict cell rather than a claim. A read too small for a panel fails as one line in place with no button, `p.section-error` (My progress's MSF: "Multi-source feedback: Could not load MSF coverage. Your counts above are not affected.").

`<Skeleton Width="100%" Height="1rem" Count="1" />` renders `div.skeleton`s: a `hover-bg` → `border-color` → `hover-bg` gradient, `radius-sm`, pulsing over 1.2s; under `prefers-reduced-motion: reduce` a still `header-bg` block the same size.

## Rules (DESIGN.md § Alerts, validation, empty states; § Skeleton loaders)

- Every list page handles all three states explicitly. No "Loading…" text anywhere (`PageShapeSmokeTests` scans for it).
- Skeletons match the shape of what loads, so the page does not shift when data lands.
- An empty state names what is missing and what would change it, in the page's words.

## Contrast

Every pair passes: the empty card's copy (`muted-text`, 5.09:1); the error Alert's words 11.81:1 on `danger-bg`, its edge and icon 5.56:1; Try again, `secondary-color` on the surface fill of an outline button, 4.86:1.

## Known gaps

- **Most load errors still break the rule that a load failure says nothing changed and offers the read again.** Six pages follow it: Home's DashboardFrame ("**Could not load your Home.** Nothing has changed. Try again, or come back in a few minutes."); since flow 02, My account ("Could not load your account. …", with `OnRetry`); and since flow 03, Log an activity ("Could not load what you can file. Nothing has changed. Try again, or come back in a few minutes."), an activity ("Could not load this activity. …"), My activities ("Could not load your activities. …") and the Activity inbox ("Could not load the Activity inbox. …", redrawn by flow 04, its Try again's answer focusing "Waiting for you"); and since flow 05, My progress ("Could not load your progress. …", answering into This period's heading), an EPA's page ("Could not load this EPA. …", answering into the h1) and the standing panel (above); each with Try again and the exception sent to the log. The committee page's standing panel, which offers no Try again, says "Could not load the trainee's standing. Nothing has changed. Reload the page, or come back in a few minutes." Every other `LoadError` is printed as the page gives it, with no Try again, and the pages give it the exception's own text: `exception.Message` on the curriculum items editor, the scales, an MSF report and My authorisations (which flow 05 left as it was); `RefusalText.Of(exception)` on others (Users, a user, Decisions due, My committee reviews, the review schedule). Flows 06 to 18 should give theirs fixed words and `OnRetry`.
- `.state-panel-title` is a `<div>` at 1.1rem/600 with the body's line height (`panel-title`), not a heading.
