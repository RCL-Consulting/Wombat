# StatePanel

The wrapper every list and dashboard uses to render its three non-content states explicitly: loading as skeletons, an error as a danger Alert, and empty as a dashed card with a title, a line of copy and an optional action.

## What the consumer provides

```razor
<StatePanel IsLoading="_loading" LoadError="@_error" IsEmpty="@(_items.Count == 0)"
            EmptyTitle="No activities yet" EmptyBody="Create the first activity from the activity catalogue.">
  <EmptyActions><a class="btn btn-primary" href="/activities/new">New activity</a></EmptyActions>
  …the content…
</StatePanel>
```

- `IsLoading`: renders `SkeletonCount` (4) `Skeleton`s of `SkeletonHeight` (3rem).
- `LoadError`: renders `<Alert Kind="danger">` with the message.
- `OnRetry` (T339, flow 02, B12): set, the failure offers **Try again** beside its words, as Home's does: the Alert's words and an outline `.btn-sm` with `refresh-cw` in an `.alert-row`, inside an `ActionResult`. Pressed, the page reads again (the skeletons while it runs, so the button is gone); once the answer is drawn the focus moves: a failure again, to the alert drawn again, so it is read again; a success, to `FocusAfterRetry`, the page's own place for its content (My account's Account heading), which a page may leave empty when it has a result to say that takes the focus itself. Unset, the failure is its words alone, as before.
- `IsEmpty`: renders `div.detail-card.detail-card--empty` with a `.state-panel-title` (`EmptyTitle`, default "Nothing here yet"), a `.state-panel-copy` (`EmptyBody`, default "There is no data to show yet.") and `EmptyActions` in a `.form-actions` row.
- The content, rendered otherwise.

`<Skeleton Width="100%" Height="1rem" Count="1" />` renders `div.skeleton`s: a `hover-bg` → `border-color` → `hover-bg` gradient, `radius-sm`, pulsing over 1.2s; under `prefers-reduced-motion: reduce` a still `header-bg` block the same size.

## Rules (DESIGN.md § Alerts, validation, empty states; § Skeleton loaders)

- Every list page handles all three states explicitly. No "Loading…" text anywhere (`PageShapeSmokeTests` scans for it).
- Skeletons match the shape of what loads, so the page does not shift when data lands.
- An empty state names what is missing and what would change it, in the page's words.

## Contrast

Every pair passes: the empty card's copy (`muted-text`, 5.09:1); the error Alert's words 11.81:1 on `danger-bg`, its edge and icon 5.56:1; Try again, `secondary-color` on the surface fill of an outline button, 4.86:1.

## Known gaps

- **Most load errors still break the rule that a load failure says nothing changed and offers the read again.** Two pages follow it: Home's DashboardFrame ("**Could not load your Home.** Nothing has changed. Try again, or come back in a few minutes.") and, since flow 02, My account ("Could not load your account. Nothing has changed. Try again, or come back in a few minutes.", with `OnRetry`). Every other `LoadError` is printed as the page gives it, with no Try again, and the pages give it the exception's own text: `exception.Message` on the activity inbox, My activities, an activity, the curriculum items editor, the scales, an MSF report and My authorisations; `RefusalText.Of(exception)` on others (Users, a user, Decisions due, My committee reviews, the review schedule). Flows 03 to 18 should give theirs fixed words and `OnRetry`.
- `.state-panel-title` is a `<div>` at 1.1rem/600 with the body's line height (`panel-title`), not a heading.
