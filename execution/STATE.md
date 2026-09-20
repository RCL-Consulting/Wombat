# State — wombat

Cap: 60 lines. `harness.py lint` enforces it; `harness.py trim` moves the overflow into `log/`.
This is a reset point, not a diary.

## Focus

Wombat is **deployed but not in service** — scenario data only, no real trainees. The live
workstream is the CPSA Paediatric EPA v11.1 catalogue; everything else is operational hardening.

## Now

- Nothing in `in_progress/`. 26 queued. **P1: T102, T121, T122, T128** — plus **T099**, which
  says "dev done 2026-09-17" but was never deployed or verified, so it stays open.
- **T102 is the sharpest of them:** a trainee can name themselves as their own assessor and
  self-award entrustment credit. T128 is blocked on you; the other three are ready to pick up.
- The last session was a rationalisation pass: reference trees, stale plan docs, deploy
  artifacts and 39 dev-DB snapshots retired; a production backup found broken and fixed.
- This execution workspace was bootstrapped then too. Task lanes were derived from
  `PLAN.md`, `practical-plan.md` and git — **spot-check them**; 7 tasks said "open" while
  having shipped, and the 6 `T019-b…g` follow-ups were sitting in the wrong place.

## Blockers

- **T128** — off-host backup destination. Needs one operator decision (where the encrypted
  copy goes, and who holds the `age` private key). Nothing in code is waiting on it.

## Next

- Decide T128's destination, then `apply` it and rehearse a restore **from the retrieved copy**.

## Open questions

- `.github/workflows/WombatWeb20240504122946.yml` is a 2024 Azure workflow from the *old*
  Wombat. Never examined. Probably dead.
- Task lanes were derived, not read (W-002). Worth a spot-check against what you know shipped.

## Files to open first

- `CLAUDE.md` — conventions, footguns, and the "nothing is live" section.
- `execution/knowledge/HANDOVER.md` — running the live service; its first-month list is the ops backlog.
- `execution/architecture/DESIGN.md` — mandatory before any Razor work.
- `execution/DASHBOARD.md` — generated; the queue at a glance.
- `deploy/verify/drift-check.sh` — run before trusting any claim about the server.

## Recent

- 2026-09-20: all project docs now live under `execution/`; `Rewrite/` and `Programme/` gone.
  Session-start load 301,455 → ~10,000 bytes. See W-001..W-005 in `DECISIONS.md`.
- 2026-09-20: T097's backup script had never been deployed — 94 nights of a dump-only backup
  reporting success, because the cron scripts had no deploy path. Fixed; **T128** filed.
- 2026-09-20: `deploy/verify/` added — six read-only checks. The drift check found
  `/opt/wombat/app` 100% world-writable; fixed on the box and in both deploy scripts.
- 2026-09-20: 1.1 GB of dead weight retired — reference trees, DB snapshots, an SDK cache.
- 2026-09-19: Wave 1 shipped — T100, T111, T123, T124; every rung a clinician reads is labelled.
