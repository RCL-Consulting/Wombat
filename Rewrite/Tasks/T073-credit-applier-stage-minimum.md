# T073 — CreditApplier ignored per-stage minimum-level overrides

**Status:** ✅ SHIPPED 2026-05-30 (Opus).
**Severity:** High — credit "level reached" gate disagreed with the stage-aware minimum the UI shows.
**Surface:** `src/Wombat.Infrastructure/Activities/CreditApplier.cs`

## Symptom (found heading into Act 3 Phase 3.D)

`CreditApplier.MeetsMinimumLevel` compared the submitted level against the curriculum item's **flat**
`MinimumLevelOrder`, ignoring `MinimumLevelByStageJson`. But the trainee dashboard and
`/portfolio/progress` display the **stage-effective** minimum via `CurriculumItem.GetMinimumLevelForStage`.
So for PAED-011 (flat `MinimumLevelOrder=4`, stage map `{"1":2,"2":3,"3":4,"4":4}`), a year-2 trainee
(du Plessis, stage 2 → min 3) completing at level 3 was shown against a "Minimum level 3 (year 2)" bar
yet the completion silently did **not** count toward `MinimumLevelReachedCount` (engine gated at 4).
The engine and the UI disagreed.

## Decision + fix

Chosen (with user): the credit engine should gate on the **stage-aware** minimum, matching the display
and the scenario's Step 3.9 expectation.

- Added `TraineeProfile.GetStage(DateOnly today)` in Domain as the single source of truth for the
  1-based programme stage. `GetTraineeDashboardSummaryQueryHandler.ComputeTraineeStage` now delegates
  to it.
- `CreditApplier.ApplyAsync` resolves the subject's active `TraineeProfile` and computes their stage;
  `MeetsMinimumLevel` now gates on `curriculumItem.GetMinimumLevelForStage(traineeStage)` instead of
  the flat `MinimumLevelOrder`. No profile / pre-programme → stage null → falls back to the flat order
  (unchanged behaviour). Composes with T071 (volume always counts; level-reached only when met).

## Verification
- Unit: `CreditApplierTests.ApplyAsync_GatesOnStageMinimum_NotFlatTargetLevel` (stage-2 level-3
  completion against a flat-4 / stage-2-min-3 item → `MinimumLevelReachedCount=1`). Application 250.
- Live: Act 3 Phase 3.D play-through (procedure-log entries by du Plessis against PAED-011).

## Related
- [[T071]] — volume-vs-level credit semantics.
