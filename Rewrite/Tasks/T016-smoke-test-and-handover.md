# T016 — End-to-end smoke test & handover

**Phase:** 4 — Quality & ship
**Depends on:** T011, T013, T015
**Blocks:** nothing (this is the final task)

## Goal

Run through every core flow on the deployed instance as a human, catch anything the automated tests missed, delete the old Wombat source, and mark the rewrite complete.

## What to do

1. **Fresh Administrator bootstrap**
   - On the production instance, log in as the seeded admin.
   - Create a real `Institution`, `Speciality`, `SubSpeciality`.
   - Create an `EntrustmentScale` (or use the default).
   - Create an `Epa`, an `AssessmentForm` linked to it, and a `Curriculum` with one `CurriculumItem`.
2. **Invitation flow**
   - Issue an invitation for a test email address.
   - Receive the email, click the link, register.
   - Confirm the new user lands as `PendingTrainee` with a "welcome" screen.
   - Log back in as admin, admit the new user to the curriculum.
   - The user is now a `Trainee`.
3. **Assessor onboarding**
   - Issue an invitation for another test email, role `Assessor`.
   - Receive the email, register.
   - Log in as the new assessor, land on the assessor dashboard.
4. **Assessment happy path**
   - As the trainee, create a `NewAssessmentRequest` targeting the assessor and the EPA.
   - The assessor receives an email (check MailHog / real inbox).
   - As the assessor, accept the request, schedule it, then complete the form.
   - As the trainee, see the completed assessment on the dashboard.
5. **Assessment edge cases**
   - Trainee creates a request, then cancels it before the assessor responds.
   - Assessor declines a request with a reason; trainee sees the reason.
   - Assessor accepts, then cancels before completion; trainee sees it cancelled.
6. **STAR reflection flow**
   - As the trainee, create a draft STAR reflection against the EPA.
   - Fill in all four sections, submit.
   - As the admin (SpecialityAdmin), decline with feedback.
   - As the trainee, edit and resubmit.
   - As the admin, approve.
   - Trainee sees the approved reflection with feedback visible.
7. **Role gating**
   - Try to access an admin page as a trainee — gets `/AccessDenied`.
   - Try to modify a different trainee's assessment as an assessor — gets an error (handler authorisation kicks in).
   - Sign out and try to access any page — redirected to `/Account/Login`.
8. **Operations**
   - Restart the service: `systemctl restart wombat`. Confirm it comes back up.
   - Trigger the backup manually: `/usr/local/bin/wombat-backup.sh`. Confirm output.
   - Inspect `journalctl -u wombat` for any errors or warnings from the session.
9. **Delete the old Wombat**
   - Remove the old `Wombat.Common`, `Wombat.Data`, `Wombat.Application`, `Wombat.Web` folders from the repo if they weren't removed in T001.
   - Remove `SOURCE_MAP.md` (the one at the repo root that maps the old code). The rewrite's own `Rewrite/` folder stays.
   - Optionally remove `ClinicAssist.NET_ref_DO_NOT_COMMIT/` from the worktree — keep it in a branch or a tarball if you want the reference later.
   - Single commit: `T016: retire old Wombat source`.
10. **Handover document**
    - Write `Rewrite/HANDOVER.md` with: what's deployed, what config file is where, who to contact about the SMTP account, how to restore from backup, where logs live, known limitations, immediate TODO list for the next month of real use.
11. **Mark the plan complete**
    - Tick every box in `PLAN.md`.
    - Update `current_state.md` with final session notes and "Plan complete" status.

## Verification

- [ ] All 8 scenarios above execute successfully on the production instance.
- [ ] No errors in `journalctl -u wombat` during the run.
- [ ] `dotnet test` (run against the repo as it stands) is fully green.
- [ ] `HANDOVER.md` exists and would let a stranger take over the service.
- [ ] Old Wombat source is gone from the main branch.

## Notes & gotchas

- Do not skip any of the 8 scenarios. Each one has caught real bugs in similar systems.
- If something fails, fix it before ticking boxes. This is not a dress rehearsal; this is the real commissioning.
- Keep a log of timings for each scenario. "Login to dashboard rendered in <1s" is useful data for next year's you.
- If the Linode instance feels underpowered during the smoke test, that is the moment to upgrade, not later. Blazor Server circuits hold memory per connected user.
- After this task, every change to Wombat goes through the normal task-file workflow: one task per change, committed on its own branch, `current_state.md` updated. The rewrite is done; the project is live.
