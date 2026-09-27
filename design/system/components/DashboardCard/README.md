# DashboardCard

A titled `.detail-card` for Home's role dashboards, with an optional Lucide icon, count badge, link, stripe and column span, laid out in the shared three-track `.dashboard-grid` inside a DashboardFrame that draws the loading and the load-error states.

## What the consumer provides

```razor
<DashboardFrame IsLoading="IsLoading" Failed="Failed" OnRetry="LoadAsync">
  <DashboardCard Title="Waiting for your rating" Icon="inbox" Emphasis="true" Span="2" IsLoading="IsLoading"
                 Count="@(Summary?.PendingRequestCount)" Warning="@(Summary?.AwaitingReview.Any(a => a.IsOverdue) == true)">
    @if (Summary is { } summary) { … }
  </DashboardCard>
</DashboardFrame>
```

- `Title` (required): an `h2.dashboard-card-title` under Home's h1, with the icon at 18px before it.
- `Icon`: a Lucide name from the Icons group.
- `Count`: a number shown after the title as a `badge-submitted` ("Waiting for your rating 2"); not while loading.
- `Href`: the whole card becomes an `<a>` with `.detail-card--interactive` (it lifts 2px on hover); not while loading.
- `Emphasis` / `Warning`: the 4px `secondary-color` or `warning-color` stripe; with both, the warning's shows.
- `Span`: 2 or 3 (`.dashboard-span-2`, `-3`).
- `IsLoading`: the title over a `.dashboard-card-skeleton` (three skeleton lines, `aria-hidden`), and not the content.
- The body, in `.dashboard-card-body`, composed from: `.list-row` (a row of a list: its label left, a badge or a date right), `.dashboard-metric` (a `.dashboard-metric-value`, 2rem/700 `primary-color`, over a `.dashboard-metric-label`), `.dashboard-metric-row` (figures side by side), `.progress-row` (`.progress-row-head`: label left, "n of m" right; a `.progress-bar` and its `.progress-bar-fill`, `.is-complete` in `success-color`; a `.progress-row-meta` line), `.status-dot` rows, and a `.dashboard-card-footer` for a link button ("Open inbox →").

## DashboardFrame (Components/Shared/DashboardFrame.razor)

Every dashboard that reads draws its cards inside one, in an `ActionResult`:

- **Loading:** the grid, `aria-busy="true"`, and every card's title at once over its skeleton. Nothing is offered until the read returns.
- **Failed:** one `danger` Alert with fixed words and an `.alert-row`: "**Could not load your Home.** Nothing has changed. Try again, or come back in a few minutes.", and Try again (`.btn-outline .btn-sm`, refresh-cw), which reads again. **No cards**: a card drawn empty would say there is nothing, and a skeleton that it is still coming. The failure's own text goes to the log. Once Try again answers, the region takes the focus.
- **Loaded:** the grid and its cards.

## The grid (DESIGN.md § Dashboard layout grid)

- `.dashboard-grid`: three `minmax(0, 1fr)` tracks from 1100px, two from 901px, one at 900px and below, gap `space-lg`, `align-items: start`. At 1280px the content column is 974px, so three tracks of about 309px. A card spanning three takes the whole row at every width; spans reset to one track at 900px and below.
- A card's own content never widens its track (`min-width: 0`): a wide table scrolls inside it.

## Rules (DESIGN.md § Dashboard page)

- Home's header is the page's only one: "Home", "{acting role} · Semester N, YYYY", and one action for the Trainee ("Log an activity") and the Institutional admin ("Invite a person"). Dashboards add no header.
- **Home with no role** (a graduate, or an account whose last role was removed): no subtitle, no header action and no dashboard, but one `Span="3"` `Emphasis` card in a `.dashboard-grid`. A graduate holding the `trainee_record` claim gets "Your training record" (`book`): "You hold no role at the moment. Your training record is kept, read-only: your progress in each period, and your portfolio to export.", with "Open My progress →" (`/portfolio/progress`, `.btn-sm .btn-outline`). Anyone else gets "No role assigned" (`info`): "Your account holds no role at the moment, so there is nothing to show you here. An administrator can give you one.", with no link. It is the graduate's landing page, which flow 13 designs.
- Every link on a dashboard opens a page that admits the dashboard's role.
- A figure is a count against a target for a named window: "1 of 3 this semester", "1 of 2 in 2026". Never a lifetime total or a mean percentage. (The trainee's headline figures read "2 / 5" "semester targets met".)
- Every `.progress-bar` has `role="progressbar"`, `aria-valuemin`, `aria-valuemax`, `aria-valuenow` and an `aria-label` that states the figure in words.
- Inline `style` only for a per-instance value (a bar's width); a row is `.list-row`, a row of buttons `.actions-cell`, figures side by side `.dashboard-metric-row`.
- The cards themselves belong to later flows (S20); flow 01 dropped only the duplicates (the Assessor's three ways to one inbox, the Administrator's Maintenance card, the Trainee's Actions card).

## Contrast

Text pairs pass. A progress fill on its `hover-bg` track: `secondary-color` 4.61:1, a complete bar's `success-color` 5.58:1 (2.73:1 until T335).

## Known gaps

- A card with `Href` wraps its body in an `<a>`, so a link inside it (the Trainee's "Open inbox →", "View authorisations →") nests one anchor in another.
- A linked card's `<a>` carries an inline `style="text-decoration:none;color:inherit;"` (DashboardCard.razor), which the previews copy. It breaks the rule above: a class such as `.detail-card--interactive` should carry it. Do not take the inline style as the pattern.
- The Administrator's System health rows for the email queue and the nightly job are stubs that always show `warn`, labelled with task ids "(T012)" and "(T024)" (T327).
