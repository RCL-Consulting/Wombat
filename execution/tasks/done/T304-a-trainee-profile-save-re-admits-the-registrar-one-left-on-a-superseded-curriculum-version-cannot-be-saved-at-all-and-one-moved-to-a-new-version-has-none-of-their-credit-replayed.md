---
id: T304
title: A trainee profile save re-admits the registrar: one left on a superseded curriculum version cannot be saved at all, and one moved to a new version has none of their credit replayed
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-26
started: 2026-10-04
completed: 2026-10-04
---

# T304 — A trainee profile save re-admits the registrar: one left on a superseded curriculum version cannot be saved at all, and one moved to a new version has none of their credit replayed

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** High. Two things go wrong once an institution re-adopts. First, no profile of a registrar left on the old version can be saved by anyone, an Administrator included, not even to change a date. That contradicts the pinning the design promises. Second, a registrar moved to the new version reads zero progress to themself and their committee until a global Administrator runs the rebuild, and the admin who moved them is not told.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-6.34a, F-6.37a).

## Symptom

**6.34, Dr Dlamini (on 11.1, after KGK re-adopted 11.2 at 6.32).**
- Prof Mbatha moves her expected completion from 2028-01-14 to 2028-07-14, leaves the curriculum as 11.1, and presses Save profile.
- It is refused: 'Trainees must be admitted into the curriculum version this institution has adopted.' (design/baseline/act-6/6.34-1-profile-save-refused.png; design/baseline/states/trainee-profile-edit--curriculum-refused.png).
- The audit row is `UpdateTraineeProfileCommand` FAILED, and SQL shows profile 3 unchanged (curriculum 2, adoption 1).
- Every 11.1 registrar at KGK is in the same position.

**6.36–6.37, Dr Ndlovu (moved to 11.2).**
- The move reads only 'Trainee profile saved.'.
- His My Progress then reads 0 on every card, PAED-002 included (it read 1 of 3 at 6.35), and nothing on the page says his earlier evidence is not yet counted (design/baseline/act-6/6.37-1-ndlovu-on-11.2-before-rebuild.png; design/baseline/states/my-progress--before-rebuild.png).
- Only devadmin's global rebuild at 6.38 restores it ('Stale tallies removed 1').

**The pickers (from the code).**
- The profile page offers every version the admin can open, so a registrar on 11.2 is offered 11.1, and a move back is refused.
- After a re-adoption, the admit form offers 11.1 too, and admission into it is refused.
- For an Administrator, the admit form offers every curriculum in the catalogue.

## Root cause

**The unchanged save is judged as an admission.**
- `UpdateTraineeProfile.cs:69-73` re-resolves `profile.AdoptionId` on every save through `TraineeAdoptionResolver.ResolveAdoptionIdAsync`.
- That throws whenever the institution's active adoption for the discipline is another curriculum (`TraineeAdoptionResolver.cs:22-39`), even when the profile's curriculum is unchanged.
- T091 built that as the hard gate *at admission*. `CurriculumAdminScope.cs:20-25` says re-adopting 'deactivates the old adoption, but its trainees stay pinned to the old version', and D24 left mid-programme registrars on their version.

**The move is not replayed.**
- `UpdateTraineeProfile.cs:72-78` moves `CurriculumId` and `AdoptionId` and saves with no replay.
- Tallies are rows per curriculum item, so his old ones stay on 11.1's items while `TraineeQuotaProgress` reads 11.2's.
- `CurriculumProgressReplay.RunAsync` runs only from `ProgrammeEndCredit.cs:108` and the rebuild (`RebuildCurriculumProgressCommand.cs:129`). The rebuild is Administrator-only (`:120-125`; `CurriculumProgressRebuild.razor:2`), though its `TraineeUserId` (`:78-86`) is documented for 'after one trainee's ... curriculum or programme start have been corrected'.
- A start change has the same gap: `CreditTargetResolver.cs:87` judges each completion at `profile.GetStage(observedOn)`.

**The pickers.** `TraineeProfileEdit.razor` fills its curriculum picker from `GetCurriculaListQuery`, which lists every version the caller can open, not what the save accepts.

## What to build

**The profile's pin is kept unless the curriculum changes.**
- In `UpdateTraineeProfile`, an unchanged `CurriculumId` keeps `profile.AdoptionId` as stored, superseded or not.
- Only a change of curriculum is judged by `TraineeAdoptionResolver`, and a move is allowed only into the institution's active adoption for that discipline.
- Every check runs before any write (the audit trap).

**A move, or a change of programme start, replays that trainee's credit in the same save.**
- Call `CurriculumProgressReplay.RunAsync` for the trainee, as `ProgrammeEndCredit` does.
- Take the trainee credit lock exclusively before the profile is read, as `ITraineeCreditLock.HoldForEndAsync` does, or with a sibling hold named for a move. No completion then credits against the old pin while the move saves.
- If the replay throws, the profile is put back.
- The result says what moved, for example 'Trainee profile saved. 1 completion was counted again against 11.2.'.

**The pickers offer exactly what the command accepts** (DESIGN.md:427).
- On an existing profile: its pinned version, plus the institution's active adoption for that discipline.
- At admission: the active adoption's version only.

**The runbook.** Its Expects at 6.34 (the save succeeds and Dlamini stays on 11.1, adoption 1), 6.37 (his cards count PAED-002 at once) and 6.38 (nothing stale left for him) change with this work, and are rewritten in the same change.

**Ordering.** Land this with, or after, the task on ended profiles (C28). Once an unchanged curriculum stops being re-judged, an ended profile on a superseded version also becomes saveable.

## Verification

- [x] Handler test: after a re-adoption, saving a profile pinned to the superseded version, with only the expected completion changed, succeeds. AdoptionId is still the superseded adoption. — `TraineeProfileMoveTests.AfterAReAdoption_SavingAProfilePinnedToTheSupersededVersion_WithOnlyTheExpectedCompletionChanged_Succeeds` (`95d4e950`).
- [x] Handler test: a move from 11.2 back to 11.1 (inactive adoption) is refused before any write. The audit pipeline's save afterwards commits nothing. — `AMoveBackToTheSupersededVersion_IsRefusedBeforeAnyWrite`, `AMoveIntoAnotherDisciplinesAdoptedVersion_IsRefusedBeforeAnyWrite`.
- [x] Handler test: a move to 11.2 re-credits the trainee's completions on 11.2's items, and removes the 11.1 tallies, in one save. A replay that throws leaves the profile on 11.1. — `AMoveTo112_ReCreditsTheTraineesCompletionsOn112sItems_AndRemoves111sTallies_InOneSave`; `WhenTheMovesReplayFails_TheProfileStaysOn111_AndTheAuditSaveCommitsNothing`; `AMoveToAVersionWithoutTheCompletionsEpa_…` (`a27983fd`).
- [x] Handler test: a programme-start change replays, and a completion's minimum is re-judged at the new training year. — `AProgrammeStartChange_Replays_AndJudgesACompletionsMinimumAtTheNewTrainingYear`.
- [x] Integration test in the style of ProgrammeEndWritePathTests: the move takes the trainee hold, and a completion racing it credits against the moved profile. — `ProgrammeEndCreditRacePostgresTests.ACompletion_WaitsForAMoveInFlight_AndCreditsAgainstTheMovedProfile` and `AMove_WaitsForACompletionInFlight_…`; Architecture `Whatever_moves_a_stored_profile_holds_it_for_the_move`.
- [x] bUnit: the profile page's picker offers the pinned version and the active adoption's. The admit form offers the active adoption's alone. — `TheProfilesPicker_ListsWhatTheChoicesQueryAnswers_WithThePinnedVersionChosen`, `TheAdmitForm_ListsWhatTheAdmissionChoicesQueryAnswers`; the rule in `TraineeCurriculumChoicesTests`.
- [x] Browser, runbook Step 6.34: Save profile reads 'Trainee profile saved.', and reopened it shows 2028-07-14 on 11.1. Step 6.37: Ndlovu's PAED-002 reads 1 of 3 before any rebuild. Step 6.38: 'Stale tallies removed 0'. — the T304 replay on `wombat_scenario_t304` (2026-10-04): 6.34 'Trainee profile saved.', reopened 2028-07-14 on 11.1, SQL adoption 1; 6.36 '…1 completion was checked against 11.2, and 1 counts towards it.'; 6.37 PAED-002 1 of 3 at once; 6.38 'Stale tallies removed 0'. Suites: Domain 810, Application 3,609, Infrastructure 1,011, Architecture 52, Web 3,051, Integration 483.

## Related

T091 (the adoption gate at admission), T211 and T223 (CurriculumAdminScope pinning), D24, T281 (ProgrammeEndCredit: the one-trainee replay and the credit lock this reuses), T130 (per-item tallies and the bootstrapper, which does not refill a non-empty table), T171, C28's task on ended profiles, and the C24 note on T289 (UpdateTraineeProfile.cs:80-85 rewrites scope). Runbook Steps 6.32–6.38.
