# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-09-20 (Opus) — rationalisation, a broken production backup, and this workspace

### Done

- **Retired what had outlived its name.** The two vendored reference trees (44 MB — verified
  every file had a live source first), three stale plan documents, 39 dev-DB snapshots plus
  their 451 MB of PostgreSQL template databases, and a 648 MB SDK cache. `Rewrite/` became
  `Programme/`, and `CLAUDE.md` stopped claiming the rewrite was "in progress" — it finished
  in June.
- **Wrote `Programme/HANDOVER.md`**, the T016 deliverable that had been missing for three
  months while production ran.
- **Fixed a production backup that had been lying for 94 days.** `/usr/local/bin/wombat-backup.sh`
  was the 2026-06-17 version: database dump only, no `wombat.env`, no DataProtection keys, no
  encryption, no off-host — and it exited 0, so cron reported success nightly. T097's rewrite
  never reached the box because **the cron scripts had no deployment path**. Both halves fixed.
- **Added `deploy/verify/`** — six read-only checks. The drift check found `/opt/wombat/app`
  100% world-writable (231 files) and a README step that told you to create a file nothing reads.
- **Bootstrapped this workspace.** 115 task files into lanes with v2 frontmatter; hooks in
  `.claude/settings.json`.

### Next

1. **T128** — pick the off-host backup destination. Everything else is downstream of it.
2. Then the queued P1s: **T099**, **T102**, **T121**.

### Fixed upstream in the harness, same day

Bootstrapping this queue exposed two defects in `C:\dev\rcl_execution`, both fixed there with
tests (suite 92 → 104, `99be1b4` and `b3658b3`):

- **The queue preview and DASHBOARD sorted by filename, then truncated at 12.** Six P3 builder
  follow-ups filled the window and pushed three P1s out of sight. Both now sort by priority,
  and the preview says `Showing 12 of 26`.
- **`context --plain`, `lint` and `status` died on a non-ASCII task title.** Latent — the hook
  path escapes non-ASCII via `json.dumps`, so nothing exercised it until the reordering pulled
  T122 (`the EPA→tool mapping`) into the window. The ordering fix did not cause it; it removed
  what was hiding it.

### Traps

- **The nightly backup now exits 1 on purpose** and will mail you until T128 lands. That is
  the control working, not a fault.
- **Task lanes are a first pass.** They were derived from `PLAN.md`, `practical-plan.md` and
  commit subjects because the `**Status:**` lines were unreliable — 26 said open, 7 of those
  had shipped. Spot-check before trusting the queue. `harness.py task done <id>` keeps it
  honest from here.
- **`sha256sum` the deployed file against the repo before believing any doc about the server.**
  `wombat.service`, `Caddyfile.wombat` and `appsettings.Production.json` are still
  install-once-by-hand and can drift the same way `wombat-backup.sh` did.
- Two path styles live in this repo — `Programme/` and `C:\...\Programme\`. A forward-slash
  sweep misses the backslash ones.
- The solution build and the per-project tools resolve **different** Release output trees —
  Any CPU versus x64. Never pass `--no-build`; see CLAUDE.md for the measured table.

### Verification status

- `dotnet build Wombat.sln -c Release` — **0 warnings, 0 errors** (2026-09-20).
- Test suites **not re-run** this session; no application code changed. Last known: 875 green
  (2026-09-19), Integration suite Docker-gated and not run.
- `deploy/verify/drift-check.sh` — **exit 0, no drift** (2026-09-20, after remediation).
- `deploy/verify/restore-rehearsal.sh` — 444 TOC entries, clean restore, 10 roles / 1 user /
  31 migrations.
- Production: hardened backup installed (sha `95845015c6e9`), `/health` 200, service active.
- 27 commits pushed; the origin remote and local master are in step.
