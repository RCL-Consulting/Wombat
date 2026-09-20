# State — wombat

Cap: 60 lines. `harness.py lint` enforces it; `harness.py trim` moves the overflow into `log/`.
This is a reset point, not a diary.

## Focus

Wombat is **deployed but not in service** — scenario data only, no real trainees. The live
workstream is the CPSA Paediatric EPA v11.1 catalogue; everything else is operational hardening.

## Now

- Nothing in `in_progress/`. 28 queued, 1 blocked. **All eight P1s are EPA work or gate it:**
  T099, T102, T110, T120, T121, T122, T129, T130. Re-rated 2026-09-20 on the operator's call
  that the EPA work finishes first.
- **T129 is the critical path and it is yours, not an agent's.** Nine EPA tasks are marked
  NEEDS A DECISION and wait on fourteen questions the College has never been asked. The message
  is written — `knowledge/college-rfi-v11-1.md`. Fill in the addressee and send it.
- **T130 (the annual quota) and T131 (governance) were filed 2026-09-20.** They are T098
  phases 3 and 4 — planned since 2026-09-19, never filed, therefore invisible to the queue.
  T130 is the most visible gap in the product: the progress page reads "1 / 24".
- **T102**: the self-naming path closed with T070; a `user`-typed field still accepts any user id.

## Blockers

- **T128** — off-host backup destination. Needs one operator decision (where the encrypted
  copy goes, and who holds the `age` private key). Nothing in code is waiting on it.

## Next

- Send T129. Everything in Waves 3 and 4 is downstream of the reply.
- Then T120 or T121 — both are College-gated; T121 (MSF) is the highest-value missing tool.
- Decide T128's destination, then `apply` it and rehearse a restore **from the retrieved copy**.

## Open questions

- `.github/workflows/WombatWeb20240504122946.yml` — a 2024 Azure workflow from the *old* Wombat, never examined. Probably dead.
- Who the RFI actually goes to. The programme says "the College / CPSA content owner"
  throughout and never names a person.
- Task lanes were derived, not read (W-002). Four spot-checked 2026-09-20 (T099, T102, T110,
  T113) — none misfiled. 22 unchecked.

## Files to open first

- `CLAUDE.md` — conventions, footguns, and the "nothing is live" section.
- `execution/knowledge/EPA-PROGRAMME.md` — § 2 the inventory, § 3 the decisions, § 4 the waves.
- `execution/architecture/DESIGN.md` — mandatory before any Razor work.
- `execution/DASHBOARD.md` — generated; the queue at a glance.
- `deploy/verify/drift-check.sh` — run before trusting any claim about the server.

## Recent

- 2026-09-20: **T126 shipped** — an activity can now say which entrustment ladder it was rated
  against (`rated_level_field`). No migration: the repo forbids an FK on Activity's snapshot columns,
  and nothing filters on a rating. Found and filed **T133** (the builder erases root schema pointers)
  and **T134** (committee sampling reports zero rated evidence for every v11.1 trainee). Suites 924.
- 2026-09-20: **T132 shipped** — every MSF expiry reminder mailed the token *hash* in a relative URL; it now re-issues.
- 2026-09-20: queue re-rated EPA-first; T129/T130/T131 filed; four stale paragraphs corrected.
- 2026-09-20: all project docs now live under `execution/`; `Rewrite/` and `Programme/` gone.
  See W-001..W-005 in `DECISIONS.md`. T097's backup had never been deployed; **T128** filed.
