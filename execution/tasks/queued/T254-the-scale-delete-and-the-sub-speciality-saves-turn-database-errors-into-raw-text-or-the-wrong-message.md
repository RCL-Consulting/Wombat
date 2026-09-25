---
id: T254
title: The scale delete and the sub-speciality saves turn database errors into raw text or the wrong message
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T254 — The scale delete and the sub-speciality saves turn database errors into raw text or the wrong message

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Nothing is written wrongly. The admin just sees the wrong words.
**Surfaced:** 2026-09-25, the T232 review.

## Symptom

- **The scale delete.** `DeleteEntrustmentScaleCommandHandler` runs its reference checks as reads, then saves. A
  sub-speciality default or item pin set to the scale in between surfaces as a raw `DbUpdateException`.
- **The sub-speciality saves.** `UpdateSubSpecialityCommandHandler` and `CreateSubSpecialityCommandHandler` turn every
  `DbUpdateException` into "A sub-speciality with the same name already exists for this speciality.", including a
  foreign-key failure.

## What to build

- In the scale delete, catch `DbUpdateException`, re-run the checks on a fresh read, and throw the named refusal if one
  now applies; otherwise rethrow.
- In the sub-speciality commands, map only the unique violation on `(SpecialityId, Name)` to the duplicate-name
  message. Use T243's helper for SQLSTATEs.

## Verification

- [ ] Each case gets its own message. Postgres tests with an interceptor forcing the race.

## Related

T232, T243, T223.
