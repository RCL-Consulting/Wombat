# ActivityHistory

The activity's history (`Components/Shared/Activities/ActivityHistory.razor`): every move, oldest first, as a real table; below 641px the same moves folded into a native `<details>`. The last card on the activity page. Flow 03 (T342, 2026-09-29, 725237ee; A7, B13, T325) redesigned it; until then it was a list of lines, newest first, with times in UTC.

## What the consumer provides

`<ActivityHistory Transitions="@model.Activity.Transitions" />`: the activity's `ActivityTransitionDto`s, which it orders oldest first (`ActivityPageModel.Ordered`: by time, then id).

## Markup

`section.detail-card` named by its `h2.activity-card-title` "History", then:

- **The table:** `div.table-container.history-table-container > table.clinic-table.history-table`, named by the heading, with the columns **Move**, **From → to**, **By**, **When**, **Credit**.
  - Move is the move's label as its button had it ("Create", "Submit", "Complete", "Record discussion"; sentence case since flow 04); From → to its states by their labels. **The create row reads "— → Draft"**: it runs from nothing.
  - By is the mover's name, or "—".
  - When is South African time with its zone, "2026-09-30 07:58 SAST" (`ActivityMoments.When`); a late filing adds "Filed 20 days after the encounter" under it (`span.history-late`, muted 0.85rem, on its own line).
  - Credit is `CreditOutcome.Label`: "1 item", "None", "—".
  - **A note is its own row** under its move: one `td.history-note[colspan=5]`, "Note: …" ("Note:" muted), keeping its lines; the move's row above drops its rule (`tr.has-note`), so the two read as one. A long reason never squeezes the columns.
- **The fold:** `details.history-details`, hidden from 641px. Its `summary`, 44px, weight 600, is "All 3 moves" ("All 1 move") led by a 16px `chevron-right` (`.history-details-marker`) that turns 90deg, down, while open, over `motion-fast`: the flex summary drops the browser's own triangle, so it draws its own. Inside, `ol.history-list` of `li.history-row` blocks, 8px apart (a hairline, `radius-md`, 12px padding, its lines 4px apart): the move and its states in bold ("Submit · Draft → Requested"), then "Anele Dlamini · 2026-09-30 07:58 SAST", any lateness and "Credit —" muted (`.history-meta`), and "Note: …".
- `p.text-sm.text-muted.history-zone`: "Times are South African time."

With no moves: "No moves have been recorded yet." (`p.text-muted`).

**Below 641px, scoped to `.activity-page`,** the table container hides and the fold shows; nowhere else.

## Rules (DESIGN.md § Page-level patterns, "Record page with a workflow")

- A move and a state are named by their labels from the pinned workflow (T220), never their keys.
- Every time carries its zone; a line under the table says whose time it is.
- A note is kept whole, on its own row.

## Contrast

`text-color` on the surface, 12.63:1, the head on `header-bg` 11.36:1; the late line, "Note:" and the meta `muted-text` 5.09:1.
