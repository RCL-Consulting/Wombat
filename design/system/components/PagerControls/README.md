# PagerControls

The one pager: a "Showing 1–20 of 137" summary, Previous and Next, and a page-size select named by its own label ("Per page:", or the list's own words), under every list that can grow.

## What the consumer provides

`<PagerControls Page="_page" PageSize="_pageSize" TotalCount="_total" Label="Decided by you, pages" OnPageChanged="…" OnPageSizeChanged="…" />`. Page sizes are fixed at 10, 20, 50 and 100. It renders nothing when `TotalCount` is 0.

- `Label` (flow 04, T350 build review, D2): given, the pager is a navigation landmark by that name (`nav.pager[aria-label]`, the Activity inbox's "Decided by you, pages", Decisions due's "Decisions, pages"); without it, a plain `div.pager`.
- `PageSizeLabel` (flow 08, T364, C9; T280): the page-size select's label, its accessible name, "Per page:" by default; a page gives it its list's words, "Decisions per page" on Decisions due.

## Markup

`.pager` (flex, wrapping, gap 1rem) holds `.pager-info` (`muted-text`, 0.9rem: "Showing 1–20 of 137", an en dash), `.pager-actions` (two `.btn .btn-outline .btn-sm`, 28px) and `.pager-page-size` (a `label.pager-page-size-label`, 0.85rem `muted-text`, its `for` the select's id, one per instance (`pager-size-{guid}`) so two pagers never share it; and a `.form-select .form-select-sm .pager-page-size-select`, 28px). Until flow 08 the label was a bare `span`, so the select had no name (T280).

**At either end** Previous or Next is `aria-disabled="true"`, never `disabled` (flow 04, C10 f): it keeps its place and its focus, looks unavailable (`.pager .btn[aria-disabled="true"]`: the not-allowed cursor, and the dim every `aria-disabled` button has), and a press there sends nothing. A natively disabled Next pressed onto the last page dropped the focus to the page body. The page that holds the pager moves the focus to its list's heading once the new page is drawn; since flow 08 that heading says which page it is on where there is more than one, "Decisions, page 2 of 4" (`span.list-section-page`; DecisionsDue, C9, T264). A change of page size leaves the focus on the select.

**Below 641px** Previous and Next are 44px.

## Rules (DESIGN.md § Pager)

Use it on every list that can grow; never hand-roll paging.

**Drawn only when the list runs past one page** (flow 12, T376, F9, round-3-check Left 2): Users draws it only while `MatchCount > PageSize`, Assessors while `TotalCount > PageSize`, so a list of fourteen at 20 a page has none; `PagerControls` itself is unchanged. A page turn focuses the list's heading; a change of rows per page keeps the focus on its select ("Rows per page"), unless the answer fails or no longer draws the pager (build review P6). On Users the page and the size are in the address (`page=`, `size=`), so a person's way back returns to the same page.

## Contrast

The summary and the label (`muted-text`) 5.09:1 on the surface, 4.83:1 on the page; Previous and Next (`secondary-color`) 4.86:1; the select's `input-border` edge 3.80:1.

## Known gaps

None. T280's pager half closed with flow 08 (T364): the select is named by its label on every list.
