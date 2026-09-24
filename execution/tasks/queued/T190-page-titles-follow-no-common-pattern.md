---
id: T190
title: Page titles follow no common pattern
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Tabs and history read inconsistently.
**Surfaced:** 2026-09-24, the T180 browser check.

## Symptom

"Dashboard — Wombat", "Sign in - Wombat", and most pages with no suffix. Capitals are mixed ("My Activities", "My
progress"), and every activity page is titled just "Activity".

## What to build

One rule in DESIGN.md (e.g. "<page> — Wombat", sentence case), and a `PageTitle` wrapper or a test that walks every
`@page` component. An activity page's title names the activity type.

## Verification

- [ ] Every routable page's title matches the rule. bUnit or reflection test.

## Related

T180.
