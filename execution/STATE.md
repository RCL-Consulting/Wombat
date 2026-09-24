# State — wombat

Cap: 60 lines. `harness.py lint` enforces it; `harness.py trim` moves the overflow into `log/`.
This is a reset point, not a diary.

## Focus

Wombat is **deployed but not in service** — scenario data only, no real trainees. The live
workstream is the CPSA Paediatric EPA v11.1 catalogue; everything else is operational hardening.

## Now

- Nothing in `in_progress/`. 40 queued, 1 blocked. **Three P1s remain:** T099, T120, and the new **T149** (SSO
  link endpoint: an unthrottled password oracle that binds any external identity; SSO ignores lockout).
- **T102 shipped 2026-09-24 — a `user` field names only an eligible nominee.** An active holder of the field's `role`
  (default Assessor) at the activity's stamped institution, never the subject, no Administrator bypass. A changed
  value is judged on every write; an unchanged one only at the author's hand-on (shared with T122). The picker is
  the same query, and labels only STORED values. `UpdateDraftAsync` is deleted.
- **Five one-line College questions** are listed in `EPA-PROGRAMME.md` § 3F: June's side, December, whether the
  per-semester figures are hard targets, late starters, and the new one: **does "clinical observed interaction"
  merge Mini-CEX with Direct observation?** None blocks anything; the last is a catalogue edit plus a migration.

## Blockers

- **T128** — off-host backup destination. Needs one operator decision (where the encrypted copy goes,
  and who holds the `age` private key). Nothing in code is waiting on it.

## Next

- **T120** (the ten tools, after T105 per Wave 2); new seeds declare their `WbaToolKey` and may declare a `role` on a
  user field (see the T122 and T102 notes at the top of T120). **T149** (security) alongside.
- **Before deploying T130 + T122 + T102 to production:** take a `pg_dump`. T130's migration empties the progress table
  (the bootstrapper refills it); T122's stamps the v11.1 lists and the seeded keys once. Rollback = restore.
- Send the College the § 3F questions; decide T128's destination.

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

- 2026-09-24: **T102 shipped.** Browser-verified on dev (forged ids refused, stale nominee refused at submit and
  repaired, D20-style completion by a de-roled assessor credits). Design critique + 3 review rounds (a label leak caught
  in round 1). Filed T149–T153. Suites: 1980, up from 1555.
- 2026-09-23: **T122 shipped.** Browser-verified on dev (refusal, repair, cancel, D20 credit after a list edit,
  admin tool lists, builder picker, three clean boots). Design critique, five adversarial review rounds (rounds 2–4
  reshaped the gate rule), 8 test agents, mutation checks. Filed T144–T148. Suites: 1555, up from 1195.
