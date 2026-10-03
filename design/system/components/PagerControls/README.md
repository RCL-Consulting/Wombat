# PagerControls

The one pager: a "Showing 1–20 of 137" summary, Previous and Next, and a "Per page" select, under every list that can grow.

## What the consumer provides

`<PagerControls Page="_page" PageSize="_pageSize" TotalCount="_total" Label="Decided by you, pages" OnPageChanged="…" OnPageSizeChanged="…" />`. Page sizes are fixed at 10, 20, 50 and 100. It renders nothing when `TotalCount` is 0.

- `Label` (flow 04, T350 build review, D2): given, the pager is a navigation landmark by that name (`nav.pager[aria-label]`, the Activity inbox's "Decided by you, pages"); without it, a plain `div.pager`.

## Markup

`.pager` (flex, wrapping, gap 1rem) holds `.pager-info` (`muted-text`, 0.9rem: "Showing 1–20 of 137", an en dash), `.pager-actions` (two `.btn .btn-outline .btn-sm`, 28px) and `.pager-page-size` (a `.pager-page-size-label`, 0.85rem, and a `.form-select .form-select-sm .pager-page-size-select`, 28px).

**At either end** Previous or Next is `aria-disabled="true"`, never `disabled` (flow 04, C10 f): it keeps its place and its focus, looks unavailable (`.pager .btn[aria-disabled="true"]`: the not-allowed cursor, and the dim every `aria-disabled` button has), and a press there sends nothing. A natively disabled Next pressed onto the last page dropped the focus to the page body. The page that holds the pager moves the focus to its list's heading once the new page is drawn.

**Below 641px** Previous and Next are 44px.

## Rules (DESIGN.md § Pager)

Use it on every list that can grow; never hand-roll paging.

## Known gaps

The select has no accessible name: "Per page:" is a bare `span`, not a `<label for>`, and the select carries no `aria-label` (T280).
