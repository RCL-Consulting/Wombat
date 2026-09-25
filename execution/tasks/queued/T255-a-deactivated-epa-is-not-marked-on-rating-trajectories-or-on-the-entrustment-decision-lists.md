---
id: T255
title: A deactivated EPA is not marked on rating trajectories or on the entrustment decision lists
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T255 — A deactivated EPA is not marked on rating trajectories or on the entrustment decision lists

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low.
**Surfaced:** 2026-09-25, the T231 review (finding 4).

## Symptom

T231 marks an EPA that is no longer in force "(no longer in use)" on My activities and the Inbox. These still print a
bare "Code — Title":
- the trajectory card headings on My progress and on the committee review page (`GetEpaTrajectoryForTraineeQuery`
  carries no in-force flag);
- `Admin/EntrustmentDecisions/Index.razor`, from `ListEntrustmentDecisionsForAdmin`;
- `Portfolio/MyAuthorisations.razor`, from `GetActiveDecisionsForTrainee`.

The entrustment standing panel is not affected: it reads `.InForce()`.

## What to build

Carry the in-force flag (D48, `Epa.InForceAt`) on those DTOs and mark them through `EpaOptionLabel`, the same rule T231
uses.

## Verification

- [ ] Each surface marks a deactivated EPA. bUnit.

## Related

T231, T196, D48.
