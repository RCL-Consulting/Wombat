---
id: T381
title: Changing a curriculum item's EPA after a programme ended rewrites the closed record
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-10-27
---

# T381 — Changing a curriculum item's EPA after a programme ended rewrites the closed record

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The same class of defect T312 closed, through one remaining catalogue edit.
**Surfaced:** 2026-10-27, T312's fix pass: "`UpdateCurriculumItem` can change an item's EPA. A frozen entry keeps the
old EpaId while the item's progress rows follow the new EPA."

## Why

T312's fix-pass agent named it as left open; not in either review. Inferred from the code, not replayed.

## Symptom

An ended record's frozen list names an item by id with the EPA it had at the end; after the item's EPA is changed, its
progress rows follow the new EPA, so the closed record's card and the rows disagree.

## Root cause

Inferred: `UpdateCurriculumItem` allows the EPA to change; T312's B1 refusal covers removal only.

## What to build

Refuse changing an item's EPA while an ended programme's list names it (as T312's B1 refuses removal), or decide otherwise.

## Verification

- [ ] An Application test: an item named by an ended list cannot have its EPA changed; one named by none can.

## Related

T312, its B1 (`RemoveCurriculumItemCommandHandler`, `ProgrammeItems.EndedRecordsNamingAsync`).

## Notes
