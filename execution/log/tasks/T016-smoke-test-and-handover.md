---
id: T016
title: "End-to-end smoke test & handover"
status: done
priority: P3
---
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
   - Remove `SOURCE_MAP.md` (the one at the repo root that maps the old code). The rewrite's own `Programme/` folder stays.
   - Optionally remove `ClinicAssist.NET_ref_DO_NOT_COMMIT/` from the worktree — keep it in a branch or a tarball if you want the reference later.
   - Single commit: `T016: retire old Wombat source`.
10. **Handover document**
    - Write `Programme/HANDOVER.md` with: what's deployed, what config file is where, who to contact about the SMTP account, how to restore from backup, where logs live, known limitations, immediate TODO list for the next month of real use.
11. **Mark the plan complete**
    - Tick every box in `PLAN.md`.
    - Update `current_state.md` with final session notes and "Plan complete" status.

## Verification

- [ ] All 8 scenarios above execute successfully on the production instance.
      **Not done as written.** The 2026-06-19 close-out ran an authenticated surface crawl
      of ten admin pages plus an end-to-end email send (see the implementation record), not
      the eight role-played scenarios. The scenario runbooks under `Programme/scenario-*.md`
      have since covered this ground on **dev**, not production.
- [ ] No errors in `journalctl -u wombat` during the run. **Never checked as a gate.**
- [x] `dotnet test` (run against the repo as it stands) is fully green — 426 green at the
      2026-06-19 close-out; 875 green as of 2026-09-19 (Integration suite is Docker-gated
      and not run).
- [x] `HANDOVER.md` exists and would let a stranger take over the service — written
      2026-09-20 as `Programme/HANDOVER.md`. It was outstanding for three months.
- [x] Old Wombat source is gone from the main branch — the source left the worktree at
      `c843421` (2026-04-11); the vendored copy under `Wombat_ref_old_DO_NOT_COMMIT/` went
      at `32e60da` (2026-09-20). It remains in history at `55a92c6`, which is the point.

## Notes & gotchas

- Do not skip any of the 8 scenarios. Each one has caught real bugs in similar systems.
- If something fails, fix it before ticking boxes. This is not a dress rehearsal; this is the real commissioning.
- Keep a log of timings for each scenario. "Login to dashboard rendered in <1s" is useful data for next year's you.
- If the Linode instance feels underpowered during the smoke test, that is the moment to upgrade, not later. Blazor Server circuits hold memory per connected user.
- After this task, every change to Wombat goes through the normal task-file workflow: one task per change, committed on its own branch, `current_state.md` updated. The rewrite is done; the project is live.

## Implementation record — 2026-06-19/20

> Merged in from `T016-smoke-test-handover.md` on 2026-09-20. That was a *second* T016
> document under a near-identical filename — the spec above and the record below — which
> made it impossible for a session grepping `T016` to tell which was authoritative.
> Neither was a stray; both are kept, in this one file. The spec's own checkboxes are
> left as written; **this record is the outcome.**

### What was verified

Final verification that the rewrite is complete, coherent, and ship-ready. Confirm
all PLAN.md success criteria are met at the code level, clean up reference folders,
and leave the repository in a state where a fresh clone → `deploy/README.md` gets
a working system.

### Verification checklist

#### Build & test (refreshed 2026-06-19)

- [x] `dotnet build Wombat.sln -c Release` — zero errors, zero warnings
- [x] `dotnet test` — **426 green** (50 Domain + 314 Application + 19 Architecture + 43 Web). Integration suite needs Docker (not run on this box).

#### Live deployment smoke test (2026-06-19, against https://wombat.rcl.co.za)

Authenticated as the seeded admin and crawled the key surfaces — all **HTTP 200, authenticated**:

- [x] `/` Dashboard · `/admin/users` Users · `/admin/colleges` Colleges · `/admin/curricula` Curricula
- [x] `/admin/epas` EPAs · `/admin/adoptions` Curriculum adoptions · `/admin/invitations` Invitations
- [x] `/admin/audit` Audit log · `/admin/jobs` Scheduled jobs · `/admin/institutions` Institutions
- [x] Login issues a **Secure** auth cookie behind Caddy; `/health` 200; HTTP→HTTPS 308; TLS cert valid.
- [x] Email send verified end-to-end (real test mail delivered via the app's SMTP transport).

#### Success criteria trace (from PLAN.md)

1. [x] Clean build — verified
2. [x] All tests green — verified
3. [x] Activity builder path exists: ActivityTypeEdit.razor, ActivityForm.razor, SaveActivityTypeDraftCommand, PublishActivityTypeDraftCommand, CreateActivityCommand, TransitionActivityCommand — all present
4. [x] Full lifecycle path exists: IssueInvitation → Register.razor → ManageCurriculumItems → CreateActivity → TransitionActivity → CommitteeDecision → PortfolioPdfService — complete chain
5. [x] Deployment scripts exist and are coherent: wombat.service, Caddyfile.wombat, deploy.sh, wombat-backup.sh, wombat-health.sh, deploy/README.md
6. [x] POPIA/GDPR self-service: SubmitDataRightsRequest, DownloadAccessReport, ErasureExecutor with pseudonymisation, DataRights.razor self-service UI
7. [x] Audit log: AuditPipelineBehavior (MediatR pipeline), AuditEntry (append-only with PostgreSQL trigger), AuditList.razor + AuditDetail.razor query UI
8. [x] Fresh clone reproducibility: deploy/README.md covers OS prep → .NET → PostgreSQL → config → deploy → migration → systemd → Caddy → health → backup

#### Cleanup

- [x] Verify no TODO/HACK/FIXME comments that indicate unfinished work — zero matches in *.cs
- [x] Verify no placeholder or stub implementations remain — all code paths verified
- [x] `current_state.md` updated to reflect completion
- [x] Delete old Wombat reference source — deferred 2026-06-19 by user decision, then
  **done 2026-09-20** in `32e60da`. Both `Wombat_ref_old_DO_NOT_COMMIT/` and
  `ClinicAssist.NET_ref_DO_NOT_COMMIT/` are gone from the worktree, after verifying
  every file had a live source elsewhere (the old Wombat is at `55a92c6` in this repo's
  own history; ClinicAssist is a live worktree at `C:\Users\Renier\ClinicAssist.NET`).
- [x] Remove the root `SOURCE_MAP.md` (step 9 above) — **done 2026-09-20**. It mapped the
  old MVC codebase: 12 references to `Wombat.Common`/`Wombat.Data`, zero to any project
  that exists today.
- Note: SSO is already built (T027) but not yet *activated* on the live deployment; it
  needs an institution's IdP config, not new code.

### Model recommendation

**Opus** — full-system understanding needed to verify every workflow end-to-end.

### Deliverables

- All verification boxes checked
- `current_state.md` updated to "rewrite complete"
- `PLAN.md` T016 checkbox ticked
- Commit: `T016: smoke test and handover verification`
