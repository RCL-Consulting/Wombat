# State — wombat

Cap: 60 lines. `harness.py lint` enforces it; `harness.py trim` moves the overflow into `log/`.
This is a reset point, not a diary.

## Focus

Wombat is **deployed but not in service** — scenario data only, no real trainees. The live
workstream is the CPSA Paediatric EPA v11.1 catalogue; everything else is operational hardening.

## Now

- Nothing in `in_progress/`. 32 queued, 1 blocked. **Four P1s remain:** T099, T102, T120, T122.
- **T130 shipped 2026-09-23 — the annual quota.** Progress is stored per semester, targets are per period,
  and the College's D14 exemption is applied when reading. One read model sits behind the progress page,
  the trainee dashboard and the three staff dashboards. "1 / 24" is gone; PAED-001 reads "2 of 3 this semester".
- **Four one-line College questions** are listed in `EPA-PROGRAMME.md` § 3F: June's side, December,
  whether Annexure B's per-semester figures are hard targets, and late starters. **None blocks anything:**
  storage is per semester, so each answer is a read-model change, one constant, or a rebuild.

## Blockers

- **T128** — off-host backup destination. Needs one operator decision (where the encrypted copy goes,
  and who holds the `age` private key). Nothing in code is waiting on it.

## Next

- **T122** (the EPA→tool allow-list) — **Model: Opus**: one predicate, two callers (picker and submit,
  D20), a seed change to `wbaTools` keys, and the permissive fallback of D21. `EpaSeed` is `internal` now.
- **T102** fix 2, then **T120** (the ten tools, after T105 per Wave 2).
- **Before deploying T130 to production:** take a `pg_dump`. The migration empties
  `CurriculumItemProgresses`; `CurriculumProgressBootstrapper` refills it at the first boot. `Down()` does
  not restore lifetime tallies, so a rollback means restoring the dump.
- Send the College the § 3F questions; decide T128's destination.

## Open questions

- **Dev database (2026-09-23):** T130 is applied. The dev trainee is on the paediatric curriculum with
  start 2026-01-01 and has activities 7–12 (Mini-CEX, PAED-001/006/011) completed by committee@ and
  assessor@. `assessor@wombat.local` exists now; DevUserSeeder seeds it with Paediatrics scopes.
  Snapshots `pre-t130-staging` and `pre-t130-migration` are in `recovery/`. **Production was not touched.**
- D42's month tolerance is invented and named as such; the College may overrule it.
- Task lanes were derived, not read (W-002). Five were spot-checked, none misfiled; 22 remain unchecked.

## Files to open first

- `CLAUDE.md` — conventions, footguns, and the "nothing is live" section.
- `execution/knowledge/EPA-PROGRAMME.md` — § 2 inventory, § 3 decisions (D1–D42), § 3F College asks.
- `execution/architecture/DESIGN.md` — mandatory before any Razor work.
- `execution/DASHBOARD.md` — generated; the queue at a glance.
- `deploy/verify/drift-check.sh` — run before trusting any claim about the server.

## Recent

- 2026-09-23: **T130 shipped.** Browser-verified on dev: the migration ran against real old-grain rows,
  the startup rebuild refilled them, a live completion credited, and a manual rebuild reproduced the
  tallies byte for byte. A design critique and a 90-agent adversarial review ran first. Filed T139–T143.
  Suites: 1195, up from 1040.
- 2026-09-21: **T121 shipped** — a released MSF campaign writes one terminal `msf_cpsa` activity per
  covered EPA. It credits nothing (D8).
