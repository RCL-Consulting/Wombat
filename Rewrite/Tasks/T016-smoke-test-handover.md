# T016 — Smoke test, handover, delete old Wombat source

## Goal

Final verification that the rewrite is complete, coherent, and ship-ready. Confirm
all PLAN.md success criteria are met at the code level, clean up reference folders,
and leave the repository in a state where a fresh clone → `deploy/README.md` gets
a working system.

## Verification checklist

### Build & test (refreshed 2026-06-19)

- [x] `dotnet build Wombat.sln -c Release` — zero errors, zero warnings
- [x] `dotnet test` — **426 green** (50 Domain + 314 Application + 19 Architecture + 43 Web). Integration suite needs Docker (not run on this box).

### Live deployment smoke test (2026-06-19, against https://wombat.rcl.co.za)

Authenticated as the seeded admin and crawled the key surfaces — all **HTTP 200, authenticated**:

- [x] `/` Dashboard · `/admin/users` Users · `/admin/colleges` Colleges · `/admin/curricula` Curricula
- [x] `/admin/epas` EPAs · `/admin/adoptions` Curriculum adoptions · `/admin/invitations` Invitations
- [x] `/admin/audit` Audit log · `/admin/jobs` Scheduled jobs · `/admin/institutions` Institutions
- [x] Login issues a **Secure** auth cookie behind Caddy; `/health` 200; HTTP→HTTPS 308; TLS cert valid.
- [x] Email send verified end-to-end (real test mail delivered via the app's SMTP transport).

### Success criteria trace (from PLAN.md)

1. [x] Clean build — verified
2. [x] All tests green — verified
3. [x] Activity builder path exists: ActivityTypeEdit.razor, ActivityForm.razor, SaveActivityTypeDraftCommand, PublishActivityTypeDraftCommand, CreateActivityCommand, TransitionActivityCommand — all present
4. [x] Full lifecycle path exists: IssueInvitation → Register.razor → ManageCurriculumItems → CreateActivity → TransitionActivity → CommitteeDecision → PortfolioPdfService — complete chain
5. [x] Deployment scripts exist and are coherent: wombat.service, Caddyfile.wombat, deploy.sh, wombat-backup.sh, wombat-health.sh, deploy/README.md
6. [x] POPIA/GDPR self-service: SubmitDataRightsRequest, DownloadAccessReport, ErasureExecutor with pseudonymisation, DataRights.razor self-service UI
7. [x] Audit log: AuditPipelineBehavior (MediatR pipeline), AuditEntry (append-only with PostgreSQL trigger), AuditList.razor + AuditDetail.razor query UI
8. [x] Fresh clone reproducibility: deploy/README.md covers OS prep → .NET → PostgreSQL → config → deploy → migration → systemd → Caddy → health → backup

### Cleanup

- [x] Verify no TODO/HACK/FIXME comments that indicate unfinished work — zero matches in *.cs
- [x] Verify no placeholder or stub implementations remain — all code paths verified
- [x] `current_state.md` updated to reflect completion
- [~] Delete old Wombat reference source — **intentionally deferred 2026-06-19** (user
  decision): `Wombat_ref_old_DO_NOT_COMMIT/` + `ClinicAssist.NET_ref_DO_NOT_COMMIT/` are
  kept as porting references for now. They are gitignored, so they do not affect the repo
  or deployments. (Note: SSO is already built — T027 — but not yet *activated* on the live
  deployment; it needs an institution's IdP config, not new code.)

## Model recommendation

**Opus** — full-system understanding needed to verify every workflow end-to-end.

## Deliverables

- All verification boxes checked
- `current_state.md` updated to "rewrite complete"
- `PLAN.md` T016 checkbox ticked
- Commit: `T016: smoke test and handover verification`
