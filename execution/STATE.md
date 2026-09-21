# State — wombat

Cap: 60 lines. `harness.py lint` enforces it; `harness.py trim` moves the overflow into `log/`.
This is a reset point, not a diary.

## Focus

Wombat is **deployed but not in service** — scenario data only, no real trainees. The live
workstream is the CPSA Paediatric EPA v11.1 catalogue; everything else is operational hardening.

## Now

- Nothing in `in_progress/`. 28 queued, 1 blocked. **Five P1s remain, all EPA work or gating it:**
  T099, T102, T120, T122, T130.
- **T121 shipped 2026-09-21.** MSF now leaves per-EPA evidence on the trainee's record. It credits
  nothing, by College decision D8 — the value is the evidence link, not a count.
- **T130 (the annual quota) is the most visible gap left** and is unblocked. The progress page still
  reads "1 / 24" against a lifetime total the College never published.
- Two College questions are still open and both are one line: the scope of the "clinical observed
  interaction" merge, and whether June is semester 1 or 2.

## Blockers

- **T128** — off-host backup destination. Needs one operator decision (where the encrypted copy goes,
  and who holds the `age` private key). Nothing in code is waiting on it.

## Next

- **T130** (the quota), then **T120** (the ten remaining v11.1 tools) and **T122** (the EPA→tool
  allow-list, which retires `RatedActivityTypes`' family map).
- Ask the two residual College questions before T120 merges any instrument.
- Decide T128's destination, then `apply` it and rehearse a restore **from the retrieved copy**.

## Open questions

- **The dev database was emptied and rebuilt 2026-09-20** (W-006), then T121 changed it again on
  2026-09-21: the dev trainee moved to the **paediatric** curriculum, and dev now holds MSF campaign 1
  and `msf_cpsa` activities 1–3. 15 seeded types, all v1. **Production was deliberately not touched.**
- **T126's off-ladder rule still cannot fire on dev's activities** — the three MSF rows credit nothing,
  so the path it is measured on is never entered. File a WBA to see it.
- Who the RFI actually goes to: the programme never names a person.
- Task lanes were derived, not read (W-002). Five spot-checked; none misfiled. 22 unchecked.

## Files to open first

- `CLAUDE.md` — conventions, footguns, and the "nothing is live" section.
- `execution/knowledge/EPA-PROGRAMME.md` — § 2 the inventory, § 3 the decisions, § 4 the waves.
- `execution/architecture/DESIGN.md` — mandatory before any Razor work.
- `execution/DASHBOARD.md` — generated; the queue at a glance.
- `deploy/verify/drift-check.sh` — run before trusting any claim about the server.

## Recent

- 2026-09-21: **T121 shipped** — a released MSF campaign writes one terminal `msf_cpsa` activity per
  covered EPA. **Browser-verified three times.** An adversarial diff review found six real defects,
  all fixed; a pre-existing portfolio-export crash was fixed too. Filed T137, T138. Suites 1040.
- 2026-09-20: **T133 shipped** — the visual builder erased every root schema pointer on every operator
  draft save. **Browser-verified.**
- 2026-09-20: **dev DB emptied and three compatibility hedges removed** (W-006). T134's gate is now the
  declared pointer alone; the trajectory's KNOWN LIMITATION is closed; T126 is no longer inert.
