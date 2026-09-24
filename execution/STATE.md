# State — wombat

Cap: 60 lines. `harness.py lint` enforces it; `harness.py trim` moves the overflow into `log/`.
This is a reset point, not a diary.

## Focus

Wombat is **deployed but not in service** — scenario data only, no real trainees. The live
workstream is the CPSA Paediatric EPA v11.1 catalogue; everything else is operational hardening.

## Now

- Nothing in `in_progress/`. 40 queued, 1 blocked. **One P1:** T157 — production runs the 16 September build (last
  migration T096, one user, no catalogue). Security: **T155** (P2).
- **2026-09-24 shipped T102, T105, T120, T149, T099.** A `user` field names only an eligible nominee (role + the activity's
  institution; picker = gate). A transition declares `validation` (`all`/`owned`/`draft`), so `required` flags are
  honest. Four more v11.1 instruments: CCA, RCA, chart-stimulated recall (rated), reflective exercise (unrated).
  Clinical audit and portfolio review are [T154]. The SSO link endpoint and SSO sign-in are hardened (T149).
- **Five one-line College questions** are listed in `EPA-PROGRAMME.md` § 3F: June's side, December, whether the
  per-semester figures are hard targets, late starters, and the new one: **does "clinical observed interaction"
  merge Mini-CEX with Direct observation?** None blocks anything; the last is a catalogue edit plus a migration.

## Blockers

- **T128** — off-host backup destination. Needs one operator decision (where the encrypted copy goes,
  and who holds the `age` private key). Nothing in code is waiting on it.

## Next

- **The operator, 2026-09-24: "Not deploying yet, we need to get the EPA stream completed."** T157 waits for the
  stream. The order is `EPA-PROGRAMME.md` § 4 "Order to finish the stream": the T127 group, T138, T107, T125, T137,
  T135, T160, T113 and the small ones; then the committee work (T167, T165, T166, T131); then the gated instruments.
- Operator decisions along the way: D38 (T131), D34 + portfolio shape (T154), D35 (T164), T165's quorum, T139.
- Send the College the § 3F questions (now eleven); decide T128's destination.

## Open questions

- **Dev database (2026-09-23):** T122 is applied: 15 lists on curriculum 2, 8 tool keys, 12 instruments.
  Activities 13 (D20 live) and 14–15 are T122 staging leftovers; 16 is T102's browser check (completed, credited). Snapshot `recovery/pre-t122-migration.dump`. **Production was not touched.**
- D21's trust boundary: an institution's own types are unrestricted until someone picks an instrument in the
  builder. Revisit after a release (T146 covers cross-discipline crediting).
- Task lanes were derived, not read (W-002). Five were spot-checked, none misfiled; 22 remain unchecked.

## Files to open first

- `CLAUDE.md` — conventions, footguns, and the "nothing is live" section.
- `execution/knowledge/EPA-PROGRAMME.md` — § 2 inventory, § 3 decisions (D1–D42), § 3F College asks.
- `execution/architecture/DESIGN.md` — mandatory before any Razor work.
- `execution/DASHBOARD.md` — generated; the queue at a glance.
- `deploy/verify/drift-check.sh` — run before trusting any claim about the server.

## Recent

- 2026-09-24: **T149 shipped** — SSO link hardened (cookie, lockout, rate limit); SSO refuses deactivated, admin and
  other-institution accounts. Filed T155, T156.
- 2026-09-24: **T105 + T120 shipped.** Transition validation scope; four instruments seeded, a CCA credited at 3a
  in the browser and charted as Case analysis. W-007: the compatibility preamble on every open task.
- 2026-09-24: **T102 shipped.** Browser-verified on dev (forged ids refused, stale nominee refused at submit and
  repaired, D20-style completion by a de-roled assessor credits). Design critique + 3 review rounds (a label leak caught
  in round 1). Filed T149–T153. Suites: 1980, up from 1555.
