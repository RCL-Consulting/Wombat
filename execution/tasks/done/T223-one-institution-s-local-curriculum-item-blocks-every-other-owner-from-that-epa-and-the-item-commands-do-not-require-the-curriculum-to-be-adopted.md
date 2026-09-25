---
id: T223
title: One institution's local curriculum item blocks every other owner from that EPA, and the item commands do not require the curriculum to be adopted
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
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

- [x] Two institutions each hold a local item on the same EPA of one curriculum. Postgres test.
- [x] An institution with no adoption is refused Add and Update. Handler test.

## Related

T211, T195.

---

## As built — 2026-09-25 (`46fcd07`)

- **The migration** `T223_CurriculumItemEpaPerOwner` replaces the one-item-per-EPA index with a unique index for
  national items, a unique index per owner for local items, and an EXCLUDE constraint, so that a national and a local
  item never share an EPA.
- **Adoption.** Add and Update refuse an institution that has no adoption of the curriculum, active or superseded.
  Remove works on the institution's own items wherever they are.
- **Races.** A racing save is refused in the check's own words, which name the kind of item that holds the EPA and not
  the institution.
- **Row names.** Rows name whose item they act on.

Postgres race tests; handler tests with an institution that adopted only another curriculum.

Browser on dev (scripted Chrome, master `c6efc04`; `pg_dump` first, at `recovery/pre-g3-migration.dump`):
- The new indexes and the exclusion constraint are present, and the old index is gone.
- **The College's empty state:** "Every national EPA of Paediatrics is already on this curriculum, as a national item or
  as an institution's own item…".
- **PAED-016, created by collegeadmin.** instadmin added it as their own item, and the College's picker no longer offered
  it. Once instadmin removed it, the College added it nationally, and instadmin's row read "Set by the College".
- **Stale tabs.** In each direction the Add was refused in the check's words, naming no institution.
- **Not run:** two institutions holding one EPA, which needs a second institution admin (covered by a Postgres test),
  and the adoption refusal, which no page reaches (covered by handler tests).

**Filed from the review:** [T242] (decide what happens when the College adds an EPA an institution holds) and [T243]
(PostgreSQL 18's RESTRICT SQLSTATE).
