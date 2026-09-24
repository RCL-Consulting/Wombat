---
id: T174
title: The catalogue seeder re-pins an unpinned seeded curriculum item to v11.1 at every boot, changing what its stored minima mean
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-24
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. It is the exact hazard T125 closed on the page ("a scale change carries ordinals across"), done
silently by startup code. It predates T125.
**Surfaced:** 2026-09-24, T125's correctness review (finding M1). Observed in code, not exercised.

## Symptom

`PaediatricCatalogueSeeder` (`src/Wombat.Infrastructure/Persistence/PaediatricCatalogueSeeder.cs`, the loop after
"Deliberately NOT derived from SubSpeciality.DefaultEntrustmentScaleId") sets `ScaleId` to the v11.1 scale on
**every boot** for any seeded national item whose `ScaleId` is null. It does not reset the minima and does not run
the T109 check (`CurriculumMappings.EnsureScaleCanExpressMinimaAsync`).

The failure:
1. An admin picks "Not pinned" on PAED-001, which T125's page allows. The minima reset, and the admin enters 4 and 8,
   both valid while unpinned (1–20).
2. After the next restart PAED-001 is pinned to v11.1 again. Its 4 now means 3b, and 8 is not a rung at all, so the
   item can never be credited.

## What to build

Decide first whether a seeded v11.1 item may be unpinned at all, and record the decision.
- **If it may not:** refuse "Not pinned" for a catalogue-owned item (`OwningInstitutionId is null` and seeded) on the
  write path, and stamp the pin once in a migration rather than at every boot.
- **If it may:** the seeder pins only on CREATE, as it already does for tool lists and targets. It then warns, and does
  not re-pin, where a stored item differs.

Either way no boot may change what a stored ordinal means. Recommendation: the second option, which matches how T122
and T130 treat every other catalogue value ("seeds stamp on create; a later change reaches an existing database only
through a migration"; CLAUDE.md § Editing a seed folder).

## Verification

- [ ] An unpinned seeded item stays unpinned across a restart, and its minima are unchanged. Seeder test.
- [ ] The seeder logs a warning where a stored pin differs from the catalogue.

## Related

[T125] (the page-side rule), [T109] (the server check), [T122] and [T130] (seeds stamp on create).
