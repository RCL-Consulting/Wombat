# T016 — Smoke test, handover, delete old Wombat source

## Goal

Final verification that the rewrite is complete, coherent, and ship-ready. Confirm
all PLAN.md success criteria are met at the code level, clean up reference folders,
and leave the repository in a state where a fresh clone → `deploy/README.md` gets
a working system.

## Verification checklist

### Build & test

- [x] `dotnet build Wombat.sln -c Release` — zero errors, zero warnings
- [x] `dotnet test` — 191 tests green (17 Domain + 122 Application + 19 Architecture + 33 Web)

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

## Model recommendation

**Opus** — full-system understanding needed to verify every workflow end-to-end.

## Deliverables

- All verification boxes checked
- `current_state.md` updated to "rewrite complete"
- `PLAN.md` T016 checkbox ticked
- Commit: `T016: smoke test and handover verification`
