---
id: T232
title: Deleting a scale that is a sub-speciality's default fails with a raw database error instead of a refusal
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T232 — Deleting a scale that is a sub-speciality's default fails with a raw database error instead of a refusal

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Nothing is deleted, but the admin sees a raw `DbUpdateException`.
**Surfaced:** 2026-09-25, the T145 review (finding 4). It predates T145.

## Symptom

`DeleteEntrustmentScaleCommandHandler` never checks `SubSpeciality.DefaultEntrustmentScaleId`, whose foreign key is ON
DELETE RESTRICT (`SubSpecialityConfiguration`). `EntrustmentScaleReferences` does not list it either. Deleting a scale that
is only some sub-speciality's default reaches the database and comes back as a raw exception.

## What to build

Add the sub-speciality default to `EntrustmentScaleReferences`, so the delete is refused before any write and names the
sub-speciality, as it does for a pinned curriculum item.

## Verification

- [x] Deleting a scale that is a sub-speciality's default is refused with a message naming it. Handler test (the audit
      trap: nothing written).

## Related

T145, T187, T174.

---

## As built — 2026-09-25 (`f319f43`)

`EntrustmentScaleReferences` includes a sub-speciality's default scale. The delete is refused before any write, naming
the sub-speciality and saying to change its default first. Handler tests (the audit trap) and a Postgres test showing
the refusal comes before the RESTRICT foreign key.

**Browser:** not run. The scale list's Delete is Administrator-only, and no session may use the admin credential.

**Filed from the review:** [T253] (P2): a delete ignores id bindings. [T254]: the delete race, and the sub-speciality
saves' wording.
