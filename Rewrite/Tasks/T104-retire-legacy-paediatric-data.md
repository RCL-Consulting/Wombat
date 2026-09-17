# T104 — Retire the legacy FCPaed paediatric world alongside the national CPSA catalogue

**Status:** open — **do this LAST of the T098 catalogue work**
**Surfaced:** 2026-09-16 (T098 gap 8); scoped against the dev database 2026-09-17.
**Severity:** Medium, but **hazardous**: this is a live-data migration, not a code change, and it carries a
silent rating-reinterpretation hazard.

## What actually exists (dev DB, verified 2026-09-17)

Two complete paediatric worlds sit side by side, and they do **not** collide:

| | Legacy | National (T098) |
|---|---|---|
| College | 2 — `FCPaed` | 3 — `CPSA` |
| Speciality | 2 — Paediatrics | 3 — Paediatrics |
| SubSpeciality | 2 — General Paediatrics | 3 — Paediatrics |
| EPAs | 15, codes `PAED-001..015` | 15, codes `PAED-001..015` |
| Default scale | 2 — Paed General Entrustment Scale (**5 rungs**) | 3 — CPSA Paediatric Entrustment Scale v11.1 (**6 rungs**) |
| Curriculum | 2 — FCPaed(SA) Part 1, 2026.1 | 3 — Paediatric EPA Curriculum, 11.1 |
| Activity types | 11 `mini_cex_paed`, 12 `dops_paed`, 13 `procedure_log_paed`, 14 `msf_paed` | 17-20 `*_cpsa` |

**The codes do not clash.** `EpaConfiguration.cs:22-24` puts the unique index on
`(SubSpecialityId, Code)` filtered `WHERE "OwningInstitutionId" IS NULL`, and the two EPA sets live under
different sub-specialities. `ActivityType.Key` is globally unique but the four legacy keys are `*_paed`
against the new `*_cpsa`. Nothing is being silently swallowed.

**Correct the docs while you are here:** only **four** `*_paed` activity types exist (ids 11-14), not the
ten described at `current_state.md:1016` and `act3-rebuild-scratch.md:47-49`. Ids 15 and 16 do not exist.
Any retirement plan sized off those documents is 2.5x too big.

## Nothing in `src` seeds this data

`DataSeeder.cs:14-27` seeds ten *unsuffixed* keys; `PaediatricCatalogueSeeder.cs:92-100` seeds the four
`_cpsa` keys. A repo-wide grep finds `_paed` only in comments, tests and the scenario documents. The legacy
rows are **operator-built**, from the manual scenario replay (`Rewrite/scenario-paediatrics.md:300`,
`:399`). Two consequences: there is no code to delete, and **the same manual migration has to be repeated
by hand on production.**

## Why it cannot be a delete

Every relevant foreign key is `RESTRICT` (`pg_constraint.confdeltype = 'r'`):
`Activities -> ActivityTypes`, `CurriculumItems -> Epas`, `EntrustmentDecisions -> Epas`,
`PendingEntrustmentDecisions -> Epas`, `FormEpaLinks -> Epas`, `TraineeProfiles -> Curricula`,
`InstitutionCurriculumAdoptions -> Curricula`.

Live dependents on the dev DB: **10** Activities on types 11-14; **15** CurriculumItems, **4**
CurriculumItemProgress rows and **15** EntrustmentDecisions on the legacy EPAs; **5** TraineeProfiles and
**1** InstitutionCurriculumAdoption pinned to Curriculum 2, against **0** pinned to Curriculum 3.

## Deactivation alone does not hide anything

`GetCurriculumProgressForTrainee.cs:67-80` selects `CurriculumItems` by the profile's `CurriculumId` and
**never filters `Epa.IsActive`**. So flipping `IsActive` on a retired EPA leaves it rendering on
`/portfolio/progress` exactly as before. Retiring therefore needs a code change too, not just data.

Available deactivation surfaces: `DeactivateEpaCommandHandler` (`Features/Epas/GetEpas.cs:22-55`, authorised
via `CanAccessCollege`) and `ActivityType.IsActive` via `SaveActivityTypeDraftCommand.cs:92`.

## THE HAZARD — read before touching a TraineeProfile

SubSpeciality 2 defaults to the **5-rung** scale; SubSpeciality 3 to the **6-rung** ladder. Stored
entrustment values are bare ordinals with no `ScaleId` (`CurriculumItem.MinimumLevelOrder`,
`MinimumLevelByStageJson` values, activity `DataJson` values, `MsfResponseAnswer.ScaleValue`).

The two worlds are disjoint today, so the reinterpretation T098 warned about is **dormant**. It fires the
moment a `TraineeProfile` is re-pinned from Curriculum 2 to Curriculum 3: 15 `EntrustmentDecisions` and 4
progress rows carry orders that mean different rungs in each scale — a legacy "4 = Independent" silently
becomes v11.1's "4 = 3b, trainee decides".

**Plan the rating remap before re-pinning a single profile.** `EntrustmentDecision.AuthorisedLevelId` is a
real FK and is therefore safe; everything else is not.

## Shape of the work

1. Decide the fate of the 5 legacy trainees: re-pin to Curriculum 3 (needs the remap), or leave them to
   finish on Curriculum 2 and apply the national catalogue only to new intakes. **The second is much safer
   and is probably the right clinical answer** — a registrar mid-programme should not have their assessment
   history reinterpreted.
2. Filter `Epa.IsActive` in `GetCurriculumProgressForTrainee` (and audit the other progress readers for the
   same omission).
3. Deactivate the four legacy activity types and, if step 1 allows it, the 15 legacy EPAs.
4. Write the production runbook — this data does not exist in code, so nothing replays it.

## Verification

- A legacy trainee's portfolio renders exactly as before the change, or exactly as intended after an
  explicit remap — verified by querying stored orders before and after.
- Retired EPAs no longer appear on `/portfolio/progress` or in the EPA pickers.
- `/activities/new` no longer offers `mini_cex_paed` / `dops_paed` / `procedure_log_paed` / `msf_paed`.
- No FK violation and no orphan: the 10 legacy activities and 15 decisions still resolve.

## Related

T098 gap 8. Depends on T099 (scope) being settled first. Do after T070, T100, T103.
