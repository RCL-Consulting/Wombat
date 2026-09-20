# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-09-20b (Opus) — workspace audit, EPA-first re-rating, the College RFI

No application code changed. Register work, one document written, one upstream fix.

### Done

- **Verified the harness end to end.** `lint --strict` clean; SessionStart emits valid hook
  JSON; Stop `check-handoff` resolves; no duplicate ids; all task files' `status:` matches
  their lane; tree clean and level with `origin/master`.
- **Moved T128 to `blocked/`.** STATE, HANDOFF and its own file all called it blocked on an
  operator decision while it sat in `queued/`, so the bundle offered it as an ordinary P1.
- **Retitled T102.** Was *"a trainee can name themselves as their own assessor"* — that path
  closed with T070 (`ThrowIfActorFieldNamesSubject`, `c33c14b`), which the file said only in a
  Progress section at the foot. Now names what is actually open: a `user`-typed field accepts
  any user id, unchecked for role or scope. Filename keeps the old slug; `EPA-PROGRAMME.md:571`
  cites the path.
- **Re-rated the queue EPA-first** on your call. T120 and T110 → P1. **All eight P1s are now
  EPA work or gate it.** I did *not* demote T113/T117/T127 as I first proposed — raising the
  EPA work achieves the same ordering without rating a live data-exposure defect as low.
- **Filed the three tasks the programme had planned but never filed:**
  **T129** the College RFI, **T130** the annual quota (T098 phase 3), **T131** governance
  (T098 phase 4). § 2 of `EPA-PROGRAMME.md` says of phase 3 in as many words: *"has no task
  file of its own. Write one before starting."* Nobody had.
- **Wrote `knowledge/college-rfi-v11-1.md`** — the fourteen open decisions (D1, D4, D6–D16,
  D37) translated for clinicians. Every question carries a proposed default so the cheapest
  valid reply is "defaults confirmed"; four are marked as having none: **D1, D12, D13, D16**.
- **Corrected four stale paragraphs.** T119 shipped but the § 2 inventory still listed it
  **READY** and Wave 1 still told you to build it; T120's prerequisites still warned that
  `CreditApplier.ResolveObservationDate` uses `CreatedOn` — that method no longer exists.

### Next

1. **T129 — send the RFI.** Operator work, not an agent's. Fill in addressee and reply-by
   date, read it once as the College will read it, send. Everything in Waves 3 and 4 is
   downstream of the reply.
2. **T128** — still blocked on you: destination + `age` key holder.
3. Then T120 or T121. T121 (MSF) is the highest-value missing tool by the programme's own
   reckoning — required on every EPA, can credit none.

### Traps

- **The RFI makes commitments on your behalf.** Each proposed default is what Wombat will do
  if nobody objects. Read them as promises before sending.
- **Two internal docs disagreed on a number that was heading into a letter.** § 3's D12 said
  "Direct observation (7 EPAs)", T120's table said eight. The source (`annexure-a.json`) says
  **eight**. D12 corrected. Re-derive from the annexure, not from prose, before quoting the
  College anything.
- **Both hooks hard-code `C:\dev\rcl_execution\bin\harness.py`.** Invisible from inside this
  repo; if that checkout moves, the hooks go with it. It is on `main`, not `master`.
- **`sha256sum` the deployed file before believing any doc about the server** — `drift-check.sh` does it.
- The solution build and the per-project tools resolve **different** Release output trees,
  Any CPU versus x64. Never pass `--no-build`.

### Spot-check of W-002 (lanes were derived, not read)

Four of 26 sampled against the code, **none misfiled**: T113 open (both queries still take a
bare `TraineeUserId`), T110 open, T102 partially shipped, T099 accurate. 22 unchecked. Since
then T119 was also confirmed genuinely done, from the code rather than from its lane.

### Upstream, in `C:\dev\rcl_execution`

**rcl-harness 2.4.3** — `fix(lint): a git ref is not a dead route`. `TICKED_REF` claims any
backticked token carrying a slash and calls it a path, so this handoff failed the lint for
saying the tree was level with `origin/master`. `stale_refs` now skips `refs/`, `origin/` and
`upstream/`; the guard is anchored, so a real path merely containing a remote name is still
caught. A remote named anything else still trips — reword, or add it to `GIT_REF`.
`TestGitRefsAreNotDeadRoutes`, 4 cases, three verified to fail with the guard removed.
Suite **123 → 127**. Pushed.

### Verification status

- `harness.py lint --strict` — **clean**. Harness suite **127 green**.
- **No build or test run this session**; no application code changed. Last known 875 green
  (2026-09-19); Integration suite is Docker-gated and was not run.
- Previous session's evidence stands: `dotnet build Wombat.sln -c Release` 0/0,
  `drift-check.sh` exit 0, `restore-rehearsal.sh` clean.
