# DashboardCard

A titled `.detail-card` for role dashboards, with an optional Lucide icon, link, stripe and column span, laid out in the shared `.dashboard-grid`.

## What the consumer provides

`<DashboardCard Title="Curriculum targets" Icon="book" Href="/portfolio/progress" Emphasis="true" Warning="false" Span="2">…</DashboardCard>`

- `Title` (required): rendered as an `h3`, after the icon at 18px.
- `Icon`: a Lucide name from the Icons group.
- `Href`: makes the whole card an `<a>` with `.detail-card--interactive`.
- `Emphasis` / `Warning`: the `secondary-color` or `warning-color` stripe.
- `Span`: 2 or 3 columns (`.dashboard-span-2`, `-3`), reset to one column at 900px and below.
- The body, in `.dashboard-card-body`. Parts to compose from: `.dashboard-metric` (a `.dashboard-metric-value`, 2rem bold `primary-color`, over a `.dashboard-metric-label`), `.progress-row` (`.progress-row-head`: label left, "n of m" right; a `.progress-bar` with its `.progress-bar-fill`, `.is-complete` in `success-color`; a `.progress-row-meta` line), `.status-dot` rows, and a `.dashboard-card-footer` for a link button.

## Rules (DESIGN.md § Dashboard layout grid; § Dashboard page)

- Every role dashboard is `.dashboard-grid` (auto-fit `minmax(min(320px, 100%), 1fr)`, gap `space-lg`) of DashboardCards; no role hand-rolls another grid.
- A dashboard adds no `h1`: Home owns "Welcome, {name}" / "Viewing as {role}".
- Every link on a dashboard opens a page that admits the dashboard's role.
- A figure is a count against a target for a named window: "1 of 3 this semester", "2 of 2 in 2026". Never a lifetime total or a mean percentage. (The trainee card's headline figures read "2 / 5 semester targets met".)
- Every `.progress-bar` has `role="progressbar"`, `aria-valuemin`, `aria-valuemax`, `aria-valuenow` and an `aria-label` that states the figure in words.
- Inline `style` is tolerated here for per-instance layout (a flex row, a bar's width) and must use tokens.

## Contrast

Text pairs pass. **Fails 3:1, kept as the source has it:** a complete bar's `success-color` fill on its `hover-bg` track, 2.73:1.

## Known gaps

A card with `Href` wraps its body in an `<a>`, so a link inside it (My authorisations' "View authorisations →") nests one anchor in another.
