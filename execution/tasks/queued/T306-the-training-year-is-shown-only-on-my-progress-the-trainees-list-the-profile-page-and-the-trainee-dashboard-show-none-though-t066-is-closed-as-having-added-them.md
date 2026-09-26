---
id: T306
title: The training year is shown only on My Progress: the trainees list, the profile page and the trainee dashboard show none, though T066 is closed as having added them
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-26
---

# T306 — The training year is shown only on My Progress: the trainees list, the profile page and the trainee dashboard show none, though T066 is closed as having added them

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. It is display only, and the year is derived correctly where it is shown. But an admin cannot see the year a start date gives while admitting or reviewing registrars, the trainee's Home does not say it, and a closed task claims all three.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-2.29a, F-2.39a).

## Symptom

- **Step 2.29.** The admit and edit form shows no training year, not even one derived from the start date as it is typed. Active profiles shows Name, Curriculum, Sub-speciality and Expected completion only (design/baseline/act-2/2.29-2-four-admitted.png).
- **Steps 2.31 and 2.39.** The trainee's Home has no training year on the Curriculum targets card or anywhere else (2.31-2-mahlangu-trainee-home.png, 2.39-1-molefe-home.png). My Progress reads 'Training year 4 — it sets the minimum level each encounter is judged against' (2.39-2-molefe-progress.png). The committee's standing panel also names it.

## Root cause

- **What T066 promised.** It is in done/ with no As built. Its Option A and Definition of done asked for a Stage column on `/admin/trainees`, a read-only stage on the admit and edit form, and 'Stage 4 — Final year' in the trainee dashboard's curriculum panel.
- **None was built:**
  - `PendingTraineesList.razor:61-65` has no year column.
  - `TraineeProfileEdit.razor` renders none.
  - `TraineeDashboard.razor:26` (Curriculum targets) shows none.
  - `GetTraineeDashboardSummaryQuery.cs:158-159` `ComputeTraineeStage` exists, but only `TraineeStageOverrideTests` calls it.
- **How it closed.** The history log (`execution/log/scenario-paediatrics-history-2026-09.md:824`) says 'since T066 My progress shows it', so T066 closed on `MyProgress.razor:156-157` alone.
- **No decision.** Nothing makes My Progress the only place: DESIGN.md:738 says only to word it 'training year N'.

## What to build

**Show the derived year, worded 'training year N' (DESIGN.md:738, D17).** The query computes it from `TraineeProfile.GetStage`; the page does not.
- **Active profiles:** a 'Training year' column.
- **The profile page:** a read-only 'Training year N (from the programme start)' line in the summary. On the admit form it is recomputed as the start date is typed.
- **The trainee dashboard:** 'Training year N' in the Curriculum targets card.
- **Ended profiles** show the year the programme ended in (T252's rule).
- **Clean-up.** Delete `ComputeTraineeStage` if nothing uses it afterwards.

If the operator instead decides My Progress is the one place, record that in T066's file as its As built, and delete the dead helper.

## Verification

- [ ] Query test: the list, profile and dashboard DTOs carry the training year at GetStage's boundaries (day one, the 365th day, a start in the future), and the year an ended profile ended in.
- [ ] bUnit: Active profiles has the column, the profile page shows the line (and on the admit form it updates as the start date changes), and the dashboard card names the year.
- [ ] Browser, runbook Step 2.29: Active profiles reads 4, 3, 2, 1 for Molefe, Dlamini, du Plessis and Mahlangu. Step 2.39: Molefe's Home reads 'Training year 4'.

## Related

T066 (closed without these), T130 (My Progress), T252 (the year of an ended programme), D17, DESIGN.md:738. Runbook Steps 2.29, 2.31 and 2.39.
