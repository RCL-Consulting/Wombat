---
id: T271
title: A scale delete and a publish binding that scale can race, and the builder shows a seed-key binding as "Select…"
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T271 — A scale delete and a publish binding that scale can race, and the builder shows a seed-key binding as "Select…"

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Only an Administrator deletes a scale, so the race is rare. The builder shows the wrong choice
whenever a seed type is opened.
**Surfaced:** 2026-09-25, the T253 review (findings 6 and 7).

## Symptom

1. **A delete and a publish can race.** `DeleteEntrustmentScaleCommandHandler` checks, then saves, without a lock. The
   publish check is built the same way, and no database constraint covers `scale_key`. A delete and a publish that bind
   the same scale can each pass their check.
2. **The builder hides seed-key and name bindings.** Its scale dropdown lists only scale ids
   (`GetEntrustmentScaleOptionsAsync`), so a field bound by seed key or name shows "Select…". Saving without touching
   the field keeps the stored key.

## What to build

1. A shared lock, as T230's `IEpaCreditLock`: FOR SHARE on the scale row at publish, FOR UPDATE at delete. Postgres race
   tests.
2. The builder's option list includes, and labels, whichever scale the stored key resolves to (the one T253 resolver),
   as the nominee picker does for stored values.

## Verification

- [ ] The race ends with one refusal. Postgres test.
- [ ] A seed type's scale field shows its scale in the builder. bUnit.

## Related

T253, T254, T230.
