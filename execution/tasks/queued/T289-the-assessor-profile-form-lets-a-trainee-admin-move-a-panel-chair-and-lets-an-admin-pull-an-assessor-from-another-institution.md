---
id: T289
title: The assessor-profile form lets a trainee-admin move a panel chair, and lets an admin pull an Assessor from another institution
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-25
---

# T289 — The assessor-profile form lets a trainee-admin move a panel chair, and lets an admin pull an Assessor from another institution

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium.
**Surfaced:** 2026-09-25, the T278 review.

## Symptom

- `CreateOrUpdateAssessorProfile` never asks the trainee-first rule. A Trainee who is also an Administrator can move their
  panel's chair to another institution through `UpdateScopeAsync`, and so stall their own review.
- The new-profile branch never checks the user's current institution. A forged form value lets an admin at A pull an
  Assessor from B, and the move drops that user's external logins.

## What to build

Refuse anyone who holds Trainee first. Require the user to be at the caller's institution before creating or moving a
profile, with an Administrator bound by the same rule for moves between institutions, or naming both institutions
explicitly. Handler tests and the audit trap.

## Verification

- [ ] Both paths are refused. Tests.

## Related

T278, T256, T237.

Note, 2026-09-25 (the T159 replay, step 2.4): the `Assessor user` picker never narrows. A user who already has a profile is
still offered, and the page stays on `/admin/assessors/edit` after a save. `ListAssessorUsersQuery` has no exclusion, and
`CreateOrUpdateAssessorProfile` looks a profile up by user, so creating one for someone already profiled would overwrite
theirs (inferred, not tried). T064 is marked done; this is its gap.

## Notes

- **T295 replay, 2026-09-26 (C23).** Note, 2026-09-26 (the T295 replay, Step 2.14): the T159 note is confirmed on a fresh database.
- **What the replay saw.** Each of the five saves read 'Assessor profile saved.' and stayed at `/admin/assessors/edit`. Every new form's Assessor user still offered all five KGK Assessors, profiled or not (`design/baseline/act-2/2.14-2-profile-new-form.png`, `2.14-3-zulu-saved.png`, `2.14-4-khumalo-saved.png`, `design/baseline/states/assessor-profile-edit--saved.png`).
- **The code.**
  - `AssessorProfileEdit.razor` `SaveAsync` (`:183-217`) sets the status and `_isExisting`, and never navigates.
  - `ListAssessorUsers.cs:28-43` filters by role and institution only.
  - `CreateOrUpdateAssessorProfile.cs:96-111` finds the profile by `UserId` and updates it. So a 'new' profile for someone already profiled overwrites theirs (inferred, not tried).
- **What to build.** T064's two parts. After the first save, move to `?id=` for the saved profile. A new form's picker leaves out everyone already profiled, and the page opens an existing profile rather than overwriting it from a 'new' form.

This does not widen the task beyond the T159 note.
- **T295 replay, 2026-09-26 (C24).** Note, 2026-09-26 (the T295 triage; the reviewer's suspect at Step 2.14, verified in code, not exercised): the assessor-profile save owns the user's **whole** scope, not just the institution.
- **What the save does.** `UserAdministrationService.UpdateScopeAsync` (`:195-260`) runs `SyncScopes` (`:239`, `:249`; body `:382-406`), which removes every speciality and sub-speciality scope not in the set passed. `CreateOrUpdateAssessorProfile.cs:125-130` passes only the profile's speciality and sub-speciality, and none for 'All'.
- **Who loses.** Saving the assessor profile of someone who is also a SpecialityAdmin, SubSpecialityAdmin or CommitteeMember strips the scope their invitation gave that role, and with it what their pages and panels read from those claims. The changed stamp also signs them out (T279).
- **Other callers.** `AdmitTrainee.cs:98-103` and `UpdateTraineeProfile.cs:80-85` do the same with the curriculum's discipline.
- **The dependency the other way.** The CommitteeMember dashboard counts trainees by the member's sub-speciality claims, which only an assessor-profile save writes (Zulu, Naidoo and Botha at Steps 2.37 and 3.52). Its fix and this one have to land together.

This widens the task. Decide which record owns an assessor's discipline: the profile, read from the profile, or the claims. Then have the profile save add or remove only the scope it contributed, or stop writing claims at all, and never remove a scope another role carries. Handler test: saving the profile of a SpecialityAdmin who is also an Assessor, with Speciality 'All', leaves the SpecialityAdmin scope.
