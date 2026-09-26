---
id: T303
title: Add role offers Trainee, so an admin can make a pending registrar or a graduate a Trainee with no profile, bypassing admission
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-26
---

# T303 — Add role offers Trainee, so an admin can make a pending registrar or a graduate a Trainee with no profile, bypassing admission

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. It is a path around admission. An account that holds Trainee has no TraineeProfile and no adoption pin, and a pending registrar keeps PendingTrainee beside it. Admitting that registrar afterwards fails part-way. For a graduate, it undoes Mark complete's removal of the role.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-2.28a).

## Symptom

- **Step 2.28.** Dr Molefe's user page, as Prof Mbatha sees it, marks PendingTrainee 'System-managed', but Add role offers InstitutionalAdmin, SpecialityAdmin, SubSpecialityAdmin, Coordinator, CommitteeMember, Assessor and **Trainee** (design/baseline/act-2/2.28-3-molefe-user.png).
- **Step 5.17.** After Mark complete, the same page reads 'This user has no roles.' and Add role again offers Trainee.
- **What pressing it does** (from the code; the replay did not press it):
  - The account gains Trainee, keeps PendingTrainee, and stays under Pending admission on /admin/trainees, with no profile and no curriculum.
  - A later 'Admit to curriculum' saves the profile and the scope and removes PendingTrainee. It then fails with Identity's 'User already in role 'Trainee'.', so the admin sees a refusal for an admission that was committed.
  - For a graduate, the role comes back against a completed profile, reopening the trainee pages and filing.
- **The mirror.** Remove role lets an admin take Trainee from someone with an active profile, which leaves a running programme whose trainee cannot open their own pages.

## Root cause

- `UserAdministrationRules.cs:15-24`: `AssignableRoles` includes `WombatRoles.Trainee` (`:23`). T061 excluded Administrator and PendingTrainee as system-managed (`:11-13`), but not Trainee.
- `UserDetail.razor:256-257` offers every assignable role the user does not hold.
- `AddRoleToUserCommand` checks only the caller and the scope, then `UserAdministrationService.AddRoleAsync` (`:287-304`) adds the role and nothing else.
- Only `AdmitTrainee.cs:84-105` creates the profile, pins `AdoptionId` and calls `PromotePendingTraineeAsync` (`UserAdministrationService.cs:262-285`). That removes PendingTrainee (saved) and then calls `AddToRoleAsync(Trainee)` (`:278`), which fails if the role is already held. By then `AdmitTrainee.cs:96` and `:98-103` have already saved.
- `ListPendingTrainees.cs:22` lists by the PendingTrainee role.
- `RemoveRoleFromUserCommand.cs` (the handler) removes Trainee with no look at the profile.

## What to build

Trainee is system-managed, like PendingTrainee.
- **How the role changes.** It is granted only by admission (`AdmitTrainee`) and removed only by `CompleteTraineeProfile`. A withdrawal's role is whatever T290 decides.
- **The commands.** Take Trainee out of `AssignableRoles`. Add role and Remove role both refuse it before any write, with a message pointing to Trainees → 'Admit to curriculum'.
- **The user page.** It marks Trainee 'System-managed', as it does PendingTrainee, with no Remove and no Add.
- **Out of scope, as a question.** A graduate admitted into a second programme needs an admission path, not Add role. Record that question on T290 or here.

## Verification

- [ ] Handler tests: AddRoleToUser and RemoveRoleFromUser refuse Trainee to an Administrator and to an InstitutionalAdmin. After the audit pipeline's save, the user's roles are unchanged.
- [ ] bUnit: UserDetail never offers Trainee under Add role, and shows a held Trainee as 'System-managed' with no Remove.
- [ ] The existing admission tests still pass: PendingTrainee becomes Trainee, with the profile and the adoption pin.
- [ ] Browser, runbook Step 2.28: Molefe's Add role lists InstitutionalAdmin to Assessor and not Trainee. Step 5.17: after Mark complete, her page offers no Trainee.

## Related

T061 (the Users surface; system-managed roles), T278 (trainee-first rule on the Users surface), T290 (whether a withdrawn trainee keeps the role), T091 (admission's adoption pin). Runbook Steps 2.28 and 5.17.
