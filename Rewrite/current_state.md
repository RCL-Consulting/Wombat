# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**T016 — Smoke test, handover, delete old Wombat source** (final task) — **Model: Opus**

T015 is complete. Next session: start `Tasks/T016-smoke-test-handover.md`.

## Critical-path reminder (post-pivot)

The plan has been restructured around a **schema-driven Activity platform** so institutions can add new activity types without code. The old per-type tasks (T007 Assessment, T008 Workflow, T009 STAR) are **superseded** — read their banners. The new critical path after the core domain is:

> T001 → T002 → T003 → T004 → T005 → T006 → **T017 → T018 → T019 → T020** → T021 → T022 → T010 → ~~T011~~ → ~~T012~~ → ~~T023~~ → ~~T024~~ → ~~T025~~ → ~~T026~~ → ~~T027~~ → ~~T013~~ → ~~T014~~ → ~~T015~~ → T016

See `PLAN.md` for the full phase/dependency graph and `CUSTOMIZATION.md` for the no-code model.

## Last session notes

### T015 — Linode deployment

**Code changes:**

- `Microsoft.Extensions.Hosting.Systemd` (v10.0.3) added to `Directory.Packages.props` and `Wombat.Web.csproj`.
- `builder.Host.UseSystemd()` added to `Program.cs` — enables `Type=notify` systemd handshake.
- `builder.Services.AddHealthChecks()` + `app.MapHealthChecks("/health").AllowAnonymous()` added to `Program.cs`.
- `--migrate` CLI flag: exits after `MigrateAsync()` without seeding or starting the server. Separate from `--seed`.

**New deploy/ folder (all committed):**

- `deploy/deploy.sh` — publish + rsync + migrate + restart + health check from dev machine.
- `deploy/wombat.service` — systemd unit (`Type=notify`, `TimeoutStartSec=60`, `ProtectSystem=strict`).
- `deploy/Caddyfile.wombat` — Caddy stanza with `flush_interval -1` (required for Blazor Server).
- `deploy/wombat-backup.sh` — nightly pg_dump with 14 daily / 4 weekly / 6 monthly retention.
- `deploy/wombat-health.sh` — cron every minute; restarts service + emails alert after 3 consecutive failures.
- `deploy/README.md` — step-by-step first-boot and ongoing deploy instructions.

Architecture tests: 19/19 green. Web build: clean.

**Live server steps (deferred to T016):**
The `deploy/README.md` "First-boot setup" checklist must be executed against a real Linode before the T016 verification checkboxes can be ticked.

## T016 prerequisites

Before starting T016, read:
- `Rewrite/Tasks/T016-smoke-test-handover.md` (create if it does not exist)
- `deploy/README.md` — run the first-boot checklist against the actual server
- `Rewrite/PLAN.md` success criteria (all 8 must be verifiable)
