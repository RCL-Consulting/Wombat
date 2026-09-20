# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-09-20 (Opus) — rationalisation, a broken production backup, this workspace

No application code changed. Everything below is repo hygiene, operations, or tooling.

### Done

- **Retired what had outlived its use.** Two vendored reference trees (44 MB), three stale
  plan documents, 39 dev-DB snapshots *and* their 451 MB of PostgreSQL template databases,
  a 648 MB SDK cache. Each verified redundant before deletion — the old Wombat is at
  `55a92c6` in this repo's own history, ClinicAssist is a live worktree elsewhere.
- **Wrote `knowledge/HANDOVER.md`**, the T016 deliverable missing for three months while
  production ran.
- **Fixed a production backup that had been lying for 94 nights.** The deployed
  `wombat-backup.sh` was the June version: database dump only, no `wombat.env`, no
  DataProtection keys, no encryption, no off-host — and it exited 0. T097's rewrite never
  reached the box because **the cron scripts had no deployment path**. Both halves fixed.
- **Added `deploy/verify/`** — six read-only checks. The drift check immediately found
  `/opt/wombat/app` 100% world-writable and a README step telling you to create a file
  nothing reads.
- **Adopted the rcl-harness workspace.** Session-start load **301,455 → ~10,000 bytes**.
  All project docs now live under `execution/` (`architecture/`, `knowledge/`, `log/`);
  `Rewrite/` and `Programme/` are gone. See W-001..W-005 in `DECISIONS.md`.

### Next

1. **T128** — pick the off-host backup destination. Everything else is downstream of it,
   and the nightly job mails you until it lands.
2. Then the P1s: **T102** (a trainee can self-award entrustment credit — the sharpest),
   **T121**, **T122**. **T099** is P1 but says "dev done"; confirm what remains.

### Traps

- **The nightly backup now exits 1 on purpose.** That is the control working, not a fault.
- **Task lanes were derived, not read** (W-002). 26 files said open; 7 had shipped. Spot-check
  before trusting the queue. `harness.py task done <id>` keeps it honest from here.
- **`sha256sum` the deployed file against the repo before believing any doc about the server.**
  `wombat.service`, the Caddyfile and `appsettings.Production.json` are still
  install-once-by-hand. `deploy/verify/drift-check.sh` does this for you.
- **Two path styles live here**, forward-slash and Windows backslash. A sweep for one misses
  the other — that bit twice today.
- The solution build and the per-project tools resolve **different** Release output trees,
  Any CPU versus x64. Never pass `--no-build`.
- Paths in `log/` and in completed task files still read `Rewrite/` or `Programme/`. They were
  correct when written and are deliberately left alone.

### Verification status

- `dotnet build Wombat.sln -c Release` — **0 warnings, 0 errors**.
- `harness.py lint --strict` — **clean**.
- **Test suites not re-run this session**; no application code changed. Last known 875 green
  (2026-09-19); Integration suite is Docker-gated and was not run.
- `deploy/verify/drift-check.sh` — **exit 0, no drift**, after remediation.
- `deploy/verify/restore-rehearsal.sh` — 444 TOC entries, clean restore, 10 roles / 1 user /
  31 migrations. Production `/health` 200, service active.
- Both repos pushed and clean — Wombat and rcl_execution are each level with their origin.

### Upstream, in `C:\dev\rcl_execution`

Adopting the harness exposed **seven** defects in it, all fixed there with tests —
suite **92 → 123**, versions 2.3.0 through 2.4.2, pushed. Queue ordering, UTF-8 output,
id style, an unsatisfiable `--strict` warning, an unbounded `done/`, a template field
filled by trailing whitespace, and globs reported as dead routes. Each verified to fail
with its fix disabled.
