---
id: T385
title: A former trainee's My activities breaks a date after its hyphen at 901 and 1000 px
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-10-28
---

# T385 — A former trainee's My activities breaks a date after its hyphen at 901 and 1000 px

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. A wrap inside a date on a read-only list.
**Surfaced:** 2026-10-28, T379 (flow 13).

## Why

T379's replay, A.7.4: "on a former trainee's My activities at 901 and 1000 px, a date inside the activity link breaks after its hyphen (2026-10-18 at 901, 2026-10-23 at 1000). Nothing scrolls sideways at any width."

## Symptom

As above.

## Root cause

Inferred: the activity cell wraps at the date's hyphens; no `white-space: nowrap` on the date inside the link.

## What to build

Keep the date whole (a nowrap span) in My activities' activity cell, as other lists keep dates.

## Verification

- [ ] A browser check at 901, 1000 and 1100 px: no date splits.

## Related

T379, T325.

## Notes
- **2026-10-28, the states pass:** the same split at 390 px on Export portfolio's From help ("…started on 2023-01-" /
  "15.", `states/export-portfolio--staff-narrow.png`). In that capture the word "until" in "Exporting the same dates again
  gives the same file, until the record changes." is blue, as a link; at 1280 it is grey, and the markup
  (`ExportPortfolioForm.razor:73`, `.export-note`) has no link or colour: needs confirmation in a real phone browser.
