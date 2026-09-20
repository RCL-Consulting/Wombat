# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-09-20b (Opus) — EPA-first re-rating, the College RFI, T132 and T126

Register work, the College RFI, one upstream harness fix, and two application tasks shipped.

### Done

- **[T126] — an activity can now say which ladder it was rated against.** `rated_level_field` on
  `FormSchema`, following T119's `observation_date_field` exactly: allow-listed, parsed, **validated
  at parse time** (must name an existing `scale` field), serialised at a fixed position. Eight seeds
  declare it, six abstain. A read-time resolver reads it off the activity's **pinned**
  `ActivityTypeVersion`, and D30's real off-ladder rule now works: a five-rung "4" on the six-rung
  CPSA axis used to draw as rung "3b" and look entirely normal.
  **The composite FK the task specified was NOT built.** `ActivityConfiguration` records that
  Activity's snapshot columns deliberately carry no FK, there is no `HasAlternateKey` anywhere to
  point one at, and nothing filters or sorts on a rating — T119's column existed because `ObservedOn`
  is a real SQL predicate. No migration was needed at all. The task text is amended.
- **[T132] — every MSF expiry reminder was a dead link.** The job mailed `invitation.TokenHash` in a
  relative URL, and logged success. It now re-issues; the email says the new link replaces the old.
- **Re-rated the queue EPA-first**, filed **T129** (College RFI), **T130** (annual quota, T098 phase
  3) and **T131** (governance, phase 4) — all three were planned in `EPA-PROGRAMME.md` and never
  filed, so the queue could not show them.
- **Wrote `knowledge/college-rfi-v11-1.md`** — D1, D4, D6–D16, D37 for clinicians, each with a
  proposed default. Four have none: **D1, D12, D13, D16**.
- **Verified the harness end to end**; moved **T128** to `blocked/`; retitled **T102**.

### Found while doing T126, filed, not fixed

- **[T133] P1 — the visual builder erases every root schema pointer.** `BuilderSchemaModel.ToJson`
  calls the two-argument `new FormSchema(...)` and `Parse` never reads the pointers, so any operator
  draft save silently drops `observation_date_field` (a live T119 regression) and now
  `rated_level_field` too. **Carry-through alone is not enough** — once preserved, deleting the
  pointed-at field makes the type unsaveable with no UI to clear the pointer. Both halves ship
  together. Read the task before starting.
- **[T134] P1 — committee sampling reports zero rated evidence for every v11.1 trainee.**
  `GetSamplingConcentrationWarnings` matches rated types by **exact key** over
  `mini_cex`/`dops`/`cbd`/`acat`, and uses it as the SQL filter. Every `*_cpsa` key falls outside.
  Wrong on screen today. It also contradicts T126's "nothing is wrong on screen today", which is
  corrected in that task.

### Next

1. **T129 — send the RFI.** Yours, not an agent's. Addressee and reply-by date are blank.
2. **T134** then **T133** — both P1, both found this session, neither gated on the College.
3. **T128** — still blocked on you: destination + `age` key holder.

### Traps

- **T126 had no browser check.** The chart change is a server-computed boolean feeding the existing
  `is-off-scale` class at two sites — `MyProgress.razor` and the committee page `ReviewDetail.razor`.
  No new markup. It has not been seen rendered.
- **Seeded types republish at next boot** (eight schema files changed), which strands in-flight
  activities on their old version by design. Those resolve no ladder and are left alone rather than
  mis-marked — `OffLadder = false` means "no disagreement established", never "on the ladder".
- **The RFI makes commitments on your behalf.** Each default is what Wombat will do if nobody objects.
- **Three definitions of "which types are rated" still disagree** — the trajectory's family-prefix
  list, T134's exact-key list, and the credit rules. T126 added the authoritative answer and
  deliberately retired none of them; T122's `WbaToolKey` is the structural fix.
- **Both hooks hard-code `C:\dev\rcl_execution\bin\harness.py`.** It is on `main`, not `master`.
- The solution build and the per-project tools resolve **different** Release output trees,
  Any CPU versus x64. Never pass `--no-build`.

### Verification status

- `dotnet build Wombat.sln -c Release` — **0 warnings, 0 errors**.
- Suites green, no `--no-build`: Domain **78**, Application **513**, Infrastructure **207**,
  Architecture **23**, Web **103** — **924 total**, up from 875 at session start. Integration is
  Docker-gated and was not run.
- Both T126 and T132 were **verified to fail against the unfixed code** before being called done.
- T119's two parse refusals, untested since it shipped, are now tested.
- `harness.py lint --strict` clean. Harness suite **127 green** (2.4.3 pushed upstream).
- Not re-verified: `drift-check.sh`, `restore-rehearsal.sh` — unchanged since 2026-09-20a.
