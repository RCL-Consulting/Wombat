# PagerControls

The one pager: a "Showing 1-20 of 137" summary, Previous and Next, and a "Per page" select, under every list that can grow.

## What the consumer provides

`<PagerControls Page="_page" PageSize="_pageSize" TotalCount="_total" OnPageChanged="…" OnPageSizeChanged="…" />`. Page sizes are fixed at 10, 20, 50 and 100. It renders nothing when `TotalCount` is 0.

## Markup

`.pager` (flex, wrapping, gap 1rem) holds `.pager-info` (`muted-text`, 0.9rem), `.pager-actions` (two `.btn .btn-outline .btn-sm`, disabled at either end) and `.pager-page-size` (a `.pager-page-size-label` and a `.form-select .form-select-sm .pager-page-size-select`).

## Rules (DESIGN.md § Pager)

Use it on every list that can grow; never hand-roll paging.

## Known gaps

The select has no programmatic label: "Per page:" is a `span`, not a `<label for>`.
