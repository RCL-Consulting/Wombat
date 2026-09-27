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
- `IsEmpty`: renders `div.detail-card.detail-card--empty` with a `.state-panel-title` (`EmptyTitle`, default "Nothing here yet"), a `.state-panel-copy` (`EmptyBody`, default "There is no data to show yet.") and `EmptyActions` in a `.form-actions` row.
- The content, rendered otherwise.

`<Skeleton Width="100%" Height="1rem" Count="1" />` renders `div.skeleton`s: a `hover-bg` → `border-color` → `hover-bg` gradient, `radius-4`, pulsing over 1.2s.

## Rules (DESIGN.md § Alerts, validation, empty states; § Skeleton loaders)

- Every list page handles all three states explicitly. No "Loading…" text anywhere (`PageShapeSmokeTests` scans for it).
- Skeletons match the shape of what loads, so the page does not shift when data lands.
- An empty state names what is missing and what would change it, in the page's words.

## Contrast

The empty card's copy passes (`muted-text`, 5.09:1). The error Alert's text fails AA at 3.57:1 (T322). The skeleton animation has no `prefers-reduced-motion` rule.
