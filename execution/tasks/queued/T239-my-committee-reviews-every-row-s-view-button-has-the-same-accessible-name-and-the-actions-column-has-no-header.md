---
id: T239
title: My committee reviews: every row's View button has the same accessible name, and the actions column has no header
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T239 — My committee reviews: every row's View button has the same accessible name, and the actions column has no header

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. A screen-reader user hears "View, View, View".
**Surfaced:** 2026-09-25, the T212 review (finding 7). It predates T212.

## What to build

Give each View an `aria-label` naming the review ("View the 2026 S2 review"), and the actions column a `.visually-hidden`
"Actions" header, as DESIGN.md asks for row actions (T198, T222). Then sweep the other lists for the same pattern and
fix them, or list them.

## Verification

- [ ] bUnit: each View has a unique accessible name, and the header has text.

## Related

T212, T198, T222.
