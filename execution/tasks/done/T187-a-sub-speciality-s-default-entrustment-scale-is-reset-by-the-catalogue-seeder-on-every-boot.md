---
id: T187
title: A sub-speciality's default entrustment scale is reset by the catalogue seeder on every boot
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-25
---

# T187 — A sub-speciality's default entrustment scale is reset by the catalogue seeder on every boot

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. An admin's change on `SubSpecialityEdit.razor` is silently undone at the next restart.
**Surfaced:** 2026-09-24, the T174 review (finding 4, confirmed, out of scope).

## Symptom

`PaediatricCatalogueSeeder.EnsureDefaultScaleAsync` sets `SubSpeciality.DefaultEntrustmentScaleId` to v11.1 on every
boot, not only when it creates the sub-speciality.

## What to build

Stamp it on create only, and warn where it differs (the T122, T130 and T174 rule).

## Verification

- [x] A changed default survives a second seeding run. Seeder test.

## Related

T174, T125 (the add form suggests this default).

---

## As built — 2026-09-25 (`f0b973e`)

`PaediatricCatalogueSeeder` sets the Paediatrics sub-speciality's default entrustment scale only when it creates the
sub-speciality. On every later boot it warns where the stored default differs from the v11.1 scale, and never writes
it. A seeder test holds a changed default across a second seeding run. CLAUDE.md § Editing a seed folder says so.

Browser on dev (scripted Chrome, master `95b65f4`): as collegeadmin on `/admin/specialities/2/sub-specialities/2`:
- Set to "O-R Scale": after a restart the page still shows it. The boot logs exactly one warning: "Sub-speciality 2
  (Paediatrics) defaults to 'O-R Scale' (scale 1), but EPA v11.1 is written on 'CPSA Paediatric Entrustment Scale v11.1'
  (scale 2). Not changed: …"
- Set to "No default": it survives the restart (SQL null), with one "has no default scale" warning.
- Set back to v11.1: no warning, and no boot wrote to `SubSpecialities`.

**Filed from the review:** [T221] (P2): the seeder finds its rows by names an admin can edit.
