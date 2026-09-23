# State — wombat

Cap: 60 lines. `harness.py lint` enforces it; `harness.py trim` moves the overflow into `log/`.
This is a reset point, not a diary.

## Focus

Wombat is **deployed but not in service** — scenario data only, no real trainees. The live
workstream is the CPSA Paediatric EPA v11.1 catalogue; everything else is operational hardening.

## Now

- Nothing in `in_progress/`. 36 queued, 1 blocked. **Three P1s remain:** T099, T102, T120.
- **T122 shipped 2026-09-23 — the EPA→tool allow-list.** Each curriculum item carries Annexure A's tool list as
  vocabulary keys (`WbaTools`, 12 instruments; D4 and D12 applied), and each activity type says which instrument it
  is (`WbaToolKey`). One predicate narrows the EPA picker and refuses at the write path, per credit directive: at
  create, on any change of target, and for an unchanged target only when the author hands it on while able to fix it. Credit
  never re-checks (D20), and a missing key or list is unrestricted (D21). A Mini-CEX can no longer be filed against
  PAED-005.
- **Five one-line College questions** are listed in `EPA-PROGRAMME.md` § 3F: June's side, December, whether the
  per-semester figures are hard targets, late starters, and the new one: **does "clinical observed interaction"
  merge Mini-CEX with Direct observation?** None blocks anything; the last is a catalogue edit plus a migration.

## Blockers

- **T128** — off-host backup destination. Needs one operator decision (where the encrypted copy goes,
  and who holds the `age` private key). Nothing in code is waiting on it.

## Next

- **T102** fix 2, then **T120** (the ten tools, after T105 per Wave 2). T120's new seeds must declare their
  `WbaToolKey` (the catalogue entry requires it); see the T122 note at the top of T120.
- **Before deploying T130 + T122 to production:** take a `pg_dump`. T130's migration empties the progress table
  (the bootstrapper refills it); T122's stamps the v11.1 lists and the seeded keys once. Rollback = restore.
- Send the College the § 3F questions; decide T128's destination.

## Open questions

- **Dev database (2026-09-23):** T122 is applied: 15 lists on curriculum 2, 8 tool keys, 12 instruments.
  Activities 13 (Mini-CEX, PAED-001, completed after a list edit — D20 live) and 14 (cancelled) are T122 staging
  leftovers. Snapshot `recovery/pre-t122-migration.dump`. **Production was not touched.**
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

- 2026-09-23: **T122 shipped.** Browser-verified on dev (refusal, repair, cancel, D20 credit after a list edit,
  admin tool lists, builder picker, three clean boots). Design critique, five adversarial review rounds (rounds 2–4
  reshaped the gate rule), 8 test agents, mutation checks. Filed T144–T148. Suites: 1555, up from 1195.
- 2026-09-23: **T130 shipped** — the annual quota: per-semester progress read against per-period targets.
