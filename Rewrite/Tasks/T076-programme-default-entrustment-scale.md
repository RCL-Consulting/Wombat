# T076 — Programme default entrustment scale (F-4D-1)

**Status:** ✅ Shipped 2026-06-01 (Opus). Resolves F-4D-1 from the Act 4 play-through.

## Problem (F-4D-1)

The committee STAR staging form's **Authorised level** picker listed every level from **every**
entrustment scale (two near-duplicate 1–5 sets — "4. Independent" from the O-R Scale vs
"4. Unsupervised" from the Paed scale) and did **not** narrow to the relevant scale. A committee
member could silently stage a STAR against the wrong scale.

Root cause: scales are **global** (no scope), and an EPA has **no** direct scale link — the only
indirect path (`Epa → FormEpaLink → AssessmentForm.ScaleId`) is empty in the schema-driven world
(0 AssessmentForms / 0 FormEpaLinks). So "the EPA's scale" was undefined.

## Fix (Option A — add a programme-level default scale)

Give the **sub-speciality** a `DefaultEntrustmentScaleId`. When set, committee STARs for that
programme's trainees are constrained to that scale; unset falls back to offering every scale.

- **Domain:** `SubSpeciality.DefaultEntrustmentScaleId` (`int?`) + `DefaultEntrustmentScale` nav.
- **EF / migration:** optional FK to `EntrustmentScales`, `OnDelete(Restrict)` (a scale in use as a
  programme default can't be deleted). Migration `20260601161846_ProgrammeDefaultEntrustmentScale`
  (dotnet-ef scaffolded: column + index + FK).
- **Admin UX:** `SubSpecialityEdit` gains a "Default entrustment scale" picker (options from
  `GetEntrustmentScalesListQuery`); `SubSpecialityDto` + `UpdateSubSpecialityCommand` + both list
  projections carry the field. The Update handler rejects an unknown scale id.
- **STAR picker filtering:** new `GetProgramScaleIdForReviewQuery` resolves the trainee's programme
  scale (review → `TraineeProfile` → `Curriculum` → `SubSpeciality.DefaultEntrustmentScaleId`).
  `ReviewDetail` filters the level `<select>` to that scale when resolved, else all scales.
- **Server-side guard (defense in depth):** `StagePendingEntrustmentDecision` resolves the same
  programme scale and rejects a level whose `ScaleId` differs ("must belong to the programme's
  entrustment scale") — so a crafted request can't bypass the UI filter.

## Tests

`ProgramDefaultScaleTests` (+7): resolver returns scale/null; stage rejects other-scale level,
allows matching, allows any when unset; Update persists the scale and rejects an unknown id.
Application 258 → 265. Domain 45, Infrastructure 8, Architecture 19, Web 42 all green;
Integration not run (Docker).

## Live verification

Restored `act4-A-scheduled`, applied the migration (startup), set `General Paediatrics`
(sub-speciality 2) → Paed General Entrustment Scale (2). As chair Zulu, the STAR level picker on a
started review showed **only** the 5 Paed levels (O-R Scale + the duplicate "4. Independent" gone).
As Mbatha, the `SubSpecialityEdit` picker showed "Paed General Entrustment Scale" selected
(nullable binding round-trips).

## Seeding / snapshots

- Scenario data (Paediatrics) lives in DB snapshots, not the `DataSeeder` (which seeds only a generic
  demo sub-speciality with no default — null fallback is fine), so no seeder change.
- New baseline **`act4-complete-t076`** = `act4-complete` + this migration + Paed default scale set.
  Act 5 should restore this. The original `act4-complete` is kept as the pure play-through record.
