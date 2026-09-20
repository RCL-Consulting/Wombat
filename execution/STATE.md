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
- **The College has answered all fourteen (T129 done).** Recorded in `EPA-PROGRAMME.md` § 3A-ii.
  **T120, T121, T122 and T130 are all released.** Two things the reply did not settle, both one
  line: the scope of the "clinical observed interaction" merge, and whether June is semester 1 or 2.
- **T130 (the annual quota) and T131 (governance) were filed 2026-09-20.** They are T098
  phases 3 and 4 — planned since 2026-09-19, never filed, therefore invisible to the queue.
  T130 is the most visible gap in the product: the progress page reads "1 / 24".
- **T102**: the self-naming path closed with T070; a `user`-typed field still accepts any user id.

## Blockers

- **T128** — off-host backup destination. Needs one operator decision (where the encrypted
  copy goes, and who holds the `age` private key). Nothing in code is waiting on it.

## Next

- **T121 (MSF)** is the highest-value released task; **T130** (the quota) is now unblocked.
- Ask the two residual College questions before T120 merges any instrument.
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

- 2026-09-20: **T133 shipped** — the visual builder erased every root schema pointer on every
  operator draft save. Carried through, plus a Form-tab control and publish warnings.
  **Browser-verified**: a draft save now preserves both pointers, and the stored v4 row really
  carries T126's rating pointer — which also closes T126's own open question.
- 2026-09-20: **T134 shipped** — a v11.1 trainee's committee sampling report said there was NO
  rated evidence. One shared classifier now answers it: declared rating (T126) OR known family.
- 2026-09-20: **T126 shipped** — an activity can say which ladder it was rated against
  (`rated_level_field`); no migration needed. Found and filed T133 and T134.
