---
id: T223
title: One institution's local curriculum item blocks every other owner from that EPA, and the item commands do not require the curriculum to be adopted
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T223 — One institution's local curriculum item blocks every other owner from that EPA, and the item commands do not require the curriculum to be adopted

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. It needs two owners wanting the same EPA on one shared curriculum, or a crafted command.
**Surfaced:** 2026-09-25, the T211 implementation and review.

## Symptom

- The unique index of one item per EPA per curriculum covers every owner. One institution's local item on an EPA
  therefore blocks the College and every other adopting institution from an item on that EPA.
- `AddCurriculumItem`, `UpdateCurriculumItem` and `RemoveCurriculumItem` do not require an InstitutionalAdmin's
  institution to have adopted the curriculum. With T211's rule that a curriculum holding the institution's own items
  opens for it, an institution that saves an item there by calling the command directly can then open that
  curriculum. It sees only national items, which are not secret, but the command should refuse.

## What to build

- Make the index per owner (curriculum, EPA, owning institution; national items with a null owner kept unique by a
  partial index). This needs a migration.
- Add and Update refuse an InstitutionalAdmin whose institution has no adoption of the curriculum, active or
  superseded. Remove keeps working on the institution's own items wherever they are (T211's moved-off case).

## Verification

- [ ] Two institutions each hold a local item on the same EPA of one curriculum. Postgres test.
- [ ] An institution with no adoption is refused Add and Update. Handler test.

## Related

T211, T195.
