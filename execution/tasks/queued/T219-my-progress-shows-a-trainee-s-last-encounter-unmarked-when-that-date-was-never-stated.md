---
id: T219
title: My progress shows a trainee's last encounter unmarked when that date was never stated
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T219 — My progress shows a trainee's last encounter unmarked when that date was never stated

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. It is one surface T197's marking does not reach.
**Surfaced:** 2026-09-25, split from T197 (its second Verification item).

## What to build

`CurriculumItemProgress.LastObservedOn` records no "was it stated" flag and no link to the activity (`CreditApplier`).
Add the flag (a migration and a progress rebuild), and mark "Last encounter" on My progress with T197's wording ("not
recorded (created …)").

## Verification

- [ ] An undated last encounter is marked on My progress. bUnit and browser.

## Related

T197, T161, D28.
