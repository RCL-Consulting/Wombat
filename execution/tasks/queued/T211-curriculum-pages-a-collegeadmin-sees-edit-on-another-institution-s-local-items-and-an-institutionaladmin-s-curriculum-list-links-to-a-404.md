---
id: T211
title: Curriculum pages: a CollegeAdmin sees Edit on another institution's local items, and an InstitutionalAdmin's curriculum list links to a 404
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

- [ ] A CollegeAdmin sees no Edit on another institution's local item. bUnit.
- [ ] An InstitutionalAdmin's list links only to pages they can open. Test.

## Related

T131, T125, T176, T056.
