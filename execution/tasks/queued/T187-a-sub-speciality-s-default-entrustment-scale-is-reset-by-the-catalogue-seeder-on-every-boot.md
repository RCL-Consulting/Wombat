---
id: T187
title: A sub-speciality's default entrustment scale is reset by the catalogue seeder on every boot
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

**Severity:** Low. An admin's change on `SubSpecialityEdit.razor` is silently undone at the next restart.
**Surfaced:** 2026-09-24, the T174 review (finding 4, confirmed, out of scope).

## Symptom

`PaediatricCatalogueSeeder.EnsureDefaultScaleAsync` sets `SubSpeciality.DefaultEntrustmentScaleId` to v11.1 on every
boot, not only when it creates the sub-speciality.

## What to build

Stamp it on create only, and warn where it differs (the T122, T130 and T174 rule).

## Verification

- [ ] A changed default survives a second seeding run. Seeder test.

## Related

T174, T125 (the add form suggests this default).
