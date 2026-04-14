# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**Rewrite complete.** All 27 tasks (T001–T027) are done. The PLAN.md success criteria have been verified.

## What was verified (T016)

- `dotnet build Wombat.sln -c Release` — zero errors, zero warnings
- `dotnet test` — 191 tests green (17 Domain + 122 Application + 19 Architecture + 33 Web)
- All 8 PLAN.md success criteria traced through the codebase — see `Tasks/T016-smoke-test-handover.md`
- Zero TODO/HACK/FIXME markers in source

## What remains (operational, not code)

- Execute `deploy/README.md` first-boot checklist against a real Linode server
- Configure DNS, TLS certificate (Caddy auto-provisions via ACME)
- Set production secrets in `/opt/wombat/config/wombat.env`
- Run `--seed` to provision the admin user and seeded activity types
- Revoke UPDATE/DELETE on AuditEntries table after first migration

## Post-launch follow-ups (optional, non-blocking)

- T019-b through T019-g — Activity builder enhancements (drag-and-drop, nested sections, visual workflow/credit editors, multi-condition visibility, templates)

## Last verified commit

T016 commit (this session).
