# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-09-20b (Opus) — the College's answers, T126, T132, EPA-first re-rating

Two application tasks shipped, fourteen product decisions closed, one upstream harness fix.

### Done

- **The College answered all fourteen decisions — [T129] closed.** Transcribed into
  `EPA-PROGRAMME.md` § 3A-ii, each with the option chosen and its consequence.
  **[T120], [T121], [T122] and [T130] are all released.** Six needed a second pass: D6 came back
  "all rated" (not the proposed default, and it would have had trainees rating themselves), D12
  answered a different pairing than the one asked, D13 was blank, D8 "not sure", D7 unanswered,
  D15 ambiguous. All six were put back with the reason, and answered.
- **Renumbered [T121]'s local `D1`–`D4`** to the College's **D9, D8, D10, D11** — they collided
  head-on with the product register, the exact trap `DECISIONS.md` splits its prefixes to avoid.
- **[T126] — an activity can now say which ladder it was rated against.** `rated_level_field` on
  `FormSchema`, following T119's `observation_date_field` exactly: allow-listed, parsed, validated
  at parse time, serialised at a fixed position. Eight seeds declare it, six abstain. A read-time
  resolver reads it off the activity's **pinned** version, and D30's off-ladder rule now works.
  **The composite FK the task specified was NOT built** — `ActivityConfiguration` records that
  Activity's snapshot columns deliberately carry no FK, and nothing filters or sorts on a rating.
  No migration at all. The task text is amended.
- **[T132] — every MSF expiry reminder was a dead link.** It mailed `invitation.TokenHash` in a
  relative URL and logged success. It now re-issues; the email says the new link replaces the old.
- Filed [T130]/[T131]; re-rated the queue EPA-first; [T128] to `blocked/`; retitled [T102].

### Found while doing T126, filed, not fixed

- **[T133] P1 — the visual builder erases every root schema pointer.** `BuilderSchemaModel.ToJson`
  calls the two-argument `new FormSchema(...)` and `Parse` never reads them, so any operator draft
  save silently drops `observation_date_field` (a live T119 regression) and now `rated_level_field`.
  **Carry-through alone is not enough** — once preserved, deleting the pointed-at field makes the
  type unsaveable with no UI to clear the pointer. Both halves ship together; read the task first.
- **[T134] P1 — committee sampling reports zero rated evidence for every v11.1 trainee.**
  `GetSamplingConcentrationWarnings` matches rated types by **exact key** over
  `mini_cex`/`dops`/`cbd`/`acat` and uses it as the SQL filter. Every `*_cpsa` key falls outside.

### Next

1. **Ask the two residual College questions.** Neither blocks starting; both block finishing.
   - **How far does the "clinical observed interaction" merge go?** Mini-CEX + *Directly observed
     clinical examination* changes no EPA's permitted set (EPA 7 already names both). Mini-CEX +
     **Direct observation** changes eight, and makes **EPA 10, "Leading and operating within a
     clinical team" (published list: "MSF, Direct observation (2)"), creditable by a Mini-CEX** —
     the defect [T122] exists to prevent. The reply's own example named *handover*, which is page
     8's definition of Direct observation, not of Mini-CEX.
   - **Is June semester 1 or semester 2?** D13 gave "boundary in June"; recorded Jan–Jun / Jul–Nov.
2. **[T121] (MSF)** — all four blocking decisions answered; the highest-value released task.
   **[T130]** (the quota) is unblocked too.
3. **[T134]** then **[T133]** — both P1, both found this session, neither gated on the College.
4. **[T128]** — still blocked on you: destination + `age` key holder.

### Traps

- **`counts_for` is permanent per pinned version.** `Activity.SchemaVersion` is assigned once, there
  is no re-pin path, and a rebuild replays against the pinned version. "Ship `[]` now and switch
  later" was never available — which is why D8 had to be right first time. It is: MSF gets `[]`.
- **T126 had no browser check.** The chart change is a server-computed boolean feeding the existing
  `is-off-scale` class at `MyProgress.razor` and the committee page `ReviewDetail.razor`.
- **Seeded types republish at next boot** (eight schema files changed), stranding in-flight
  activities on their old version by design. Those resolve no ladder and are left alone —
  `OffLadder = false` means "no disagreement established", never "on the ladder".
- **Three definitions of "which types are rated" still disagree** — the trajectory's family-prefix
  list, T134's exact-key list, and the credit rules. T126 added the authoritative answer and
  retired none of them; T122's `WbaToolKey` is the structural fix.
- **Both hooks hard-code `C:\dev\rcl_execution\bin\harness.py`** (on `main`, not `master`). The solution
  build and the per-project tools resolve different Release trees — never pass `--no-build`.

### Verification status

- `dotnet build Wombat.sln -c Release` — **0 warnings, 0 errors**.
- Suites green, no `--no-build`: Domain **78**, Application **513**, Infrastructure **207**,
  Architecture **23**, Web **103** — **924 total**, up from 875 at session start. Integration is
  Docker-gated and was not run. No code changed after that run — the College work was documents only.
- Both T126 and T132 were **verified to fail against the unfixed code** before being called done.
- `harness.py lint --strict` clean. Harness suite **127 green** (2.4.3 pushed upstream).
- Not re-verified: `drift-check.sh`, `restore-rehearsal.sh` — unchanged since 2026-09-20a.
