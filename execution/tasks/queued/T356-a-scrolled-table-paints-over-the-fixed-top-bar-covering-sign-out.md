---
id: T356
title: A scrolled table paints over the fixed top bar, covering Sign out
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-10-04
---

# T356 — A scrolled table paints over the fixed top bar, covering Sign out

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. The shell's top bar is the one place Sign out and the account link live; a table scrolled under
it covers them, on every page with a table (DataTable, the clinic tables). Nothing is lost, but the bar stops working
until the user scrolls back.
**Surfaced:** 2026-10-04, the T355 replay (step 7), re-checking Step 4.15 on the committee review page
(`design/baseline/act-4/4.15-3-…png`).

## Symptom

On a page with a table at 1280, scroll so the table passes under the fixed top bar: the table's container paints over
the bar and its Sign out button.

## Root cause

Inferred, to confirm: `.table-container` is `position: relative` (flow 01, `b347e11c`), which makes it a positioned
element painted above a fixed bar that has no `z-index` above it (`MainLayout.razor.css`, the fixed/sticky bar rules).

## What to build

Give the fixed top bar (and the phone bar) a stacking order above page content, as a token or one documented `z-index`
in the layout stylesheet, or drop the container's positioning if nothing needs it. Check every positioned element in
the page body (the chart's scroll region, the skeletons, dialogs keep their own order above the bar).

## Verification

- [ ] A bUnit or stylesheet test pins the bar's stacking order; a browser check at 1280 and 390 on a page with a long
  table shows Sign out on top while scrolled.

## Related

T355 (found by its replay), flow 01 (T335, the shell).
