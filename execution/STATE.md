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

- Do `Programme/`'s doctrine files (DOMAIN, ARCHITECTURE, DESIGN, CUSTOMIZATION, INFRASTRUCTURE,
  HANDOVER, scenario runbooks) move under `execution/knowledge/` + `architecture/`, or stay?
  Stage 1 left them where they are to avoid re-churning 180 cross-references in one day.
- `.github/workflows/WombatWeb20240504122946.yml` is a 2024 Azure workflow from the *old*
  Wombat. Never examined. Probably dead.

## Files to open first

- `CLAUDE.md` — architecture, conventions, footguns, and the "nothing is live" section.
- `Programme/HANDOVER.md` — running the live service; its first-month list is the ops backlog.
- `execution/DASHBOARD.md` — generated; the queue at a glance.
- `deploy/verify/drift-check.sh` — run before trusting any claim about the server.

## Recent

- 2026-09-20: harness bootstrapped; 115 task files moved into lanes with v2 frontmatter.
- 2026-09-20: `Rewrite/` → `Programme/`; CLAUDE.md stopped calling the rewrite "in progress".
- 2026-09-20: T097's backup script had never been deployed — 94 nights of a dump-only backup
  reporting success. Fixed, and the cron scripts now have a deploy path. **T128** filed.
- 2026-09-20: `deploy/verify/` added — restore rehearsal, audit trigger, DataProtection keys,
  smoke test, auth-cookie, drift check. First drift run found `/opt/wombat/app` 100% world-writable.
- 2026-09-19: Wave 1 shipped — T100, T111, T123, T124; every rung a clinician reads is labelled.
