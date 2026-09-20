# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-09-20b (Opus) — workspace audit, EPA-first re-rating, the College RFI, T132

Register work, the College RFI, one upstream harness fix — and **T132 shipped**, the only
application code this session.

### Done

- **Shipped [T132] — every MSF expiry reminder was a dead link.**
  `MsfInvitationExpiryReminderJob.cs:57` mailed `invitation.TokenHash` — the one-way SHA-256 —
  inside a **relative** URL. Unusable *and* unclickable, and the job logged success either way.
  MSF is required by every EPA and is the one instrument that cannot credit; the sole mechanism
  for chasing a non-responder had never worked.
  The plaintext is unrecoverable by design, so the job now **re-issues**: fresh token, new hash,
  absolute URL from `Wombat:MsfRespondUrl`, saved. `RequireMsfRespondUrl` is one implementation
  shared with `OpenMsfCampaign`, which had its own copy.
  **The cost is recorded, not hidden:** re-issuing retires the link mailed at open, the email
  says so, and a test pins it. 5 new tests, 4 verified to fail against the old line.
- **Verified the harness end to end** — lint, both hooks, lane/status consistency, no duplicate
  ids. Moved **T128** to `blocked/` (it was called blocked everywhere but the register).
- **Retitled T102** to the defect that is still open: a `user`-typed field accepts any user id,
  unchecked for role or scope. The self-naming half closed with T070 (`c33c14b`).
- **Re-rated the queue EPA-first.** T120 and T110 → P1; all eight P1s are now EPA work or gate
  it. T113/T117/T127 were *not* demoted — raising the EPA work gives the same ordering without
  rating a data-exposure defect as low.
- **Filed T129 (College RFI), T130 (annual quota, T098 phase 3), T131 (governance, phase 4).**
  All three were planned in `EPA-PROGRAMME.md` and never filed, so the queue could not show them.
- **Wrote `knowledge/college-rfi-v11-1.md`** — D1, D4, D6–D16, D37 translated for clinicians,
  each with a proposed default. Four have none: **D1, D12, D13, D16**.
- **Corrected four stale paragraphs** — T119 shipped but was still listed READY and Wave 1 still
  told you to build it; T120 warned about a method that no longer exists.

### Next

1. **T129 — send the RFI.** Yours, not an agent's. Addressee and reply-by date are blank.
   Everything in Waves 3 and 4 is downstream of the reply.
2. **T128** — still blocked on you: destination + `age` key holder.
3. More College-independent EPA work: **T126** (READY, M) and **T125** (READY, S) need nobody.
   T120 and T121 are gated on the reply.

### Traps

- **The RFI makes commitments on your behalf.** Each proposed default is what Wombat will do if
  nobody objects. Read them as promises before sending.
- **T132 changes respondent behaviour.** Someone who kept the original invitation email and
  clicks it after a reminder gets an invalid token. Intended, and stated in the email.
- **Two internal docs disagreed on a number heading into a letter** — D12 said "Direct
  observation (7 EPAs)", the source says **eight**. Re-derive from `annexure-a.json`, not prose.
- **Both hooks hard-code `C:\dev\rcl_execution\bin\harness.py`.** It is on `main`, not `master`.
- The solution build and the per-project tools resolve **different** Release output trees,
  Any CPU versus x64. Never pass `--no-build`.

### Spot-check of W-002 (lanes were derived, not read)

Five sampled against the code, **none misfiled**: T113, T110, T102, T099, and T119 (confirmed
genuinely done from the code, not from its lane). 22 unchecked.

### Upstream, in `C:\dev\rcl_execution`

**rcl-harness 2.4.3** — `fix(lint): a git ref is not a dead route`. `TICKED_REF` claims any
backticked token with a slash and calls it a path, so this handoff failed the lint for saying
the tree was level with `origin/master`. `stale_refs` now skips `refs/`, `origin/` and
`upstream/`, anchored so a real path containing a remote name is still caught. A remote named
anything else still trips. Suite **123 → 127**. Pushed.

### Verification status

- `dotnet build Wombat.sln -c Release` — **0 warnings, 0 errors**.
- Suites green, no `--no-build`: Application **508**, Infrastructure **177**, Domain **69**,
  Web **103**, Architecture **23** — **880 total**, up from 875. Integration is Docker-gated
  and was not run.
- `harness.py lint --strict` clean. Harness suite **127 green**.
- Not re-verified this session: `drift-check.sh`, `restore-rehearsal.sh` — unchanged since
  2026-09-20a, both clean then.
