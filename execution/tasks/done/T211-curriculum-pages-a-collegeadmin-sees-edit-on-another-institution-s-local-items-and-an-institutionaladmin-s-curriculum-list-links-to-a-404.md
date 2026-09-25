---
id: T211
title: Curriculum pages: a CollegeAdmin sees Edit on another institution's local items, and an InstitutionalAdmin's curriculum list links to a 404
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-25
---

# T211 — Curriculum pages: a CollegeAdmin sees Edit on another institution's local items, and an InstitutionalAdmin's curriculum list links to a 404

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Nothing leaks, since the handlers refuse. The pages offer actions and links that fail.
**Surfaced:** 2026-09-24, the T131 slice 2 review (found while checking scope).

## Symptom

- `GetCurriculumByIdQuery` lists every item, including institution-local ones, so a CollegeAdmin sees Edit and Remove
  on another institution's local items. The handlers then refuse the save, because `CanAccessInstitution` is false for
  a CollegeAdmin.
- `GetCurriculaListQuery` shows an InstitutionalAdmin the curricula their institution has adopted. The detail read then
  returns null for them (only an Administrator or that college's CollegeAdmin passes `CanAccessCollege`), so the list's
  link leads to not-found.

## What to build

Offer each admin only what they can act on (picker = gate, T056): hide Edit and Remove on items the caller cannot write,
and either let an InstitutionalAdmin read an adopted curriculum (their local items editable, national items read-only),
or leave it off their list.

## Verification

- [x] Browser (moved from T195): an InstitutionalAdmin edits a curriculum item. The EPA picker offers PAED-001 to 015
      for a national item, and the institution's own local EPAs for a local item.

- [x] A CollegeAdmin sees no Edit on another institution's local item. bUnit.
- [x] An InstitutionalAdmin's list links only to pages they can open. Test.

## Related

T131, T125, T176, T056.

---

## As built — 2026-09-25 (`13387fe`)

`CurriculumAdminScope` decides who may open a curriculum and edit each item. An InstitutionalAdmin opens a curriculum
their institution adopted, or one that holds their institution's own items. There, national items are read-only ("Set
by the College") and the institution's own items are editable. A new item's owner is decided per curriculum
(`CurriculumItemEpas.OwnerOfNewItem`). The list offers Edit and Create only to those the target page admits, and each row
carries the command's own `CanEdit`. Handler, bUnit and Postgres tests.

Browser on dev (scripted Chrome, master `95b65f4`):
- **instadmin (DEMO).** They adopted curriculum 2 and created local EPA `DEMO-LOC-01` through the UI.
  - `/admin/curricula` lists one row with only "Items".
  - All 15 national rows read "Set by the College", with no Edit or Remove.
  - A local item on DEMO-LOC-01 was added. **Its edit picker offered DEMO-LOC-01 plus PAED-001 to PAED-015**; Save and
    Remove worked. This is the check moved from T195: an InstitutionalAdmin has no national item to edit.
- **collegeadmin.** Create and Edit open the curriculum page, and all 15 national rows have Edit and Remove. **Editing
  PAED-003, the picker was exactly PAED-001 to PAED-015**, with DEMO-LOC-01 not offered. The edit was cancelled.
- **Not run:** the Administrator's view (the admin credential), which is covered by handler tests.

**Filed from the review:** [T222] (pickers offer EPAs already on the curriculum; whose item; Remove unconfirmed) and
[T223] (one owner's item blocks others; commands do not require adoption). The nav's Curriculum link for speciality
admins is added to [T178].
