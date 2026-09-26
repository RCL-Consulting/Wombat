---
id: T305
title: A completed or withdrawn trainee's profile can still be saved, re-pinning and re-dating the record that Mark complete says it archives
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-26
---

# T305 — A completed or withdrawn trainee's profile can still be saved, re-pinning and re-dating the record that Mark complete says it archives

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Data is wrongly changeable after the programme has ended. A graduate's curriculum, programme start and expected completion can be rewritten after completion, and with them their My Progress, entrustment standing and portfolio PDF. Nothing is replayed, and nothing says it happened.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-5.17a).

## Symptom

- **Step 5.17.** Prof Mbatha marks Dr Molefe complete: 'Trainee marked complete. The Trainee role has been removed and a graduation email sent.'. The summary reads Status Completed, Completed 2026-09-26. The last-day field and the end buttons are gone, but Curriculum, Programme start date, Expected completion date and **Save profile** stay enabled (design/baseline/states/trainee-profile-edit--completed.png; design/baseline/act-5/5.17-1-mbatha-marked-complete.png).
- **Step 5.27.** The same holds after Dr du Plessis's withdrawal: Status Inactive, with Save profile still offered (design/baseline/states/trainee-profile-edit--deactivated.png).
- **What the page says.** The last-day help says Mark complete 'archives the profile', and the Complete dialog says 'The profile is archived'.
- **What a save does** (from the code; the replay did not press it): it re-pins the record and re-dates it.

## Root cause

- `TraineeProfileEdit.razor:94-106`: Save profile (`:105`) renders whatever `_model.IsActive` is. Only the last-day field (`:80`) and Deactivate/Mark complete (`:96`) check it. The help at `:134-138` and the dialog at `:318` promise an archive.
- `UpdateTraineeProfile.cs:39-85` never reads `IsActive`. Its one end check is T209's start-after-end refusal (`:56-60`). It then:
  - re-pins `CurriculumId` and `AdoptionId` (`:72-73`);
  - rewrites both dates (`:74-76`);
  - rewrites the user's scope claims (`:80-85`).
- T209's `TraineeProfileRecordedEndTests.cs:23-72` saves ended profiles only to bound the start. No decision (EPA-PROGRAMME § 3, T209, T252) says an ended record stays editable.

## What to build

**An ended profile is read-only.** An ended profile is one with `IsActive` false, with `CompletedOn` or `DeactivatedOn` set.
- **The command.** `UpdateTraineeProfile` refuses it before anything is written: 'This programme ended on <date>; its record is archived and cannot be changed.'.
- **The page.** It shows curriculum, programme start and expected completion as a read-only details list, with no inputs and no Save profile, and keeps Back to trainees.
- **T209's tests.** Its two ended-profile update tests are replaced by the refusal test. The start-after-end check becomes unreachable and goes.
- **Corrections.** If the operator needs to correct an archived record (a start date found wrong after graduation), that is a separate, audited correction path that replays the trainee's credit. Record it as an open question here, not by leaving the form open.

## Verification

- [ ] Handler test: updating a completed profile and a deactivated profile is refused. After the audit pipeline's save, curriculum, adoption and both dates are unchanged (the TraineeProfileRecordedEndTests pattern).
- [ ] bUnit: TraineeProfileEdit renders no Save profile, and no editable curriculum, start or completion, for a Completed profile and for an Inactive one. An active profile is unchanged.
- [ ] Browser, runbook Steps 5.17 and 5.27: after Mark complete and after Deactivate, the profile shows its details read-only with no Save profile. The states screenshots trainee-profile-edit--completed.png and --deactivated.png are retaken.

## Related

T209 (the recorded end; its tests change), T252 (a former trainee's My Progress reads this record), T281 (a replay against an end), D49, the task for C26/C27 (land together; that task's fix otherwise opens ended profiles on superseded versions to saving). Runbook Steps 5.17 and 5.27.
