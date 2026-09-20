# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-09-20b (Opus) — the College's answers, T133, T134, T126, T132

Four application tasks shipped, fourteen product decisions closed, one upstream harness fix.

### Done

- **[T133] — the visual builder erased every root schema pointer on every operator draft save.**
  A live T119 regression, and after T134 it un-declared a committee statistic. Three parts, and the
  first two are **not separable**: carry the pointer through, AND drop one whose field was deleted
  (the parser refuses an orphan and `SaveDraft` runs the parser, so carrying it alone would brick
  the type through the only door there is), AND a Form-tab control + publish warnings so neither is
  silent. `BuilderSchemaModel` had **no test of any kind** — `InternalsVisibleTo` added.
- **[T134] — a v11.1 trainee's committee sampling report said there was NO rated evidence.**
  The gate matched EXACT keys over four legacy tools, so the whole seeded CPSA set fell outside it.
  One shared `RatedActivityTypes` now answers it: **declared rating (T126) OR known family**. The
  disjunction is the point — a pointer-only gate leaves the `*_paed` types reading zero and forces
  rewriting the fixture, the only evidence legacy behaviour is unchanged. **Fixture untouched.**
  Two duplicate maps retired; `WbaSourceCategory` deleted with both members grep proved dead.
- **The College answered all fourteen decisions — [T129] closed.** Transcribed into
  `EPA-PROGRAMME.md` § 3A-ii. **[T120], [T121], [T122] and [T130] are all released.** Six needed a
  second pass — D6 "all rated" would have had trainees rating themselves; D12 answered a different
  pairing; D13 blank; D8 "not sure"; D7 unanswered; D15 ambiguous. All put back, and answered.
- **Renumbered [T121]'s local `D1`–`D4`** to the College's **D9, D8, D10, D11**; they collided.
- **[T126] — an activity can now say which ladder it was rated against.** `rated_level_field`
  on `FormSchema`; eight seeds declare it. D30's off-ladder rule works. **The composite FK the task
  specified was NOT built** — snapshot columns carry no FK, nothing filters on a rating.
- **[T132]** — MSF expiry reminders mailed the token *hash*; now re-issued. Filed [T130]/[T131];
  re-rated the queue EPA-first; [T128] to `blocked/`; retitled [T102].

### Filed, not fixed

- **[T135] P2** — drafts count as rated evidence (no `CurrentState` predicate), and a readable row
  that fails `TryParseRating` is in neither the numerator nor the withheld count while
  `EvidenceComplete` reads true. T134 widened who that can reach.

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
3. **[T135]** — the sampling denominator/numerator disagreement, found in T134's design pass.
4. **[T128]** — still blocked on you: destination + `age` key holder.

### Traps

- **`counts_for` is permanent per pinned version.** `Activity.SchemaVersion` is assigned once, there
  is no re-pin path, and a rebuild replays against the pinned version. "Ship `[]` now and switch
  later" was never available — which is why D8 had to be right first time. It is: MSF gets `[]`.
- **T126's off-ladder rule is INERT on every activity now on dev.** Browser-checked: the chart
  renders the CPSA axis correctly and marks nothing, because every filed activity is pinned to a
  **pre-T126 schema version** and the resolver reads the pinned one by design. Correct, and it
  means **looking at the product today shows no difference** — file a fresh activity first.
  In-flight activities stay pinned; `OffLadder = false` means "not knowable", never "on the ladder".
- **`lint` tells you to run `trim` when STATE/HANDOFF overflow, and `trim` reports "moved 0".** The
  advice is unactionable; both files were cut by hand. Worth fixing upstream next time you are there.
- **The family arm in `RatedActivityTypes` is INTERIM**, retiring with [T122]'s `WbaToolKey`.
  Tests pin it, so removing it is a decision rather than a discovery.
- Hooks hard-code `C:\dev\rcl_execution\bin\harness.py` (`main`). Never pass `--no-build`.

### Verification status

- `dotnet build Wombat.sln -c Release` — **0 warnings, 0 errors**.
- Suites green, no `--no-build`: Domain **78**, Application **544**, Infrastructure **207**,
  Architecture **23**, Web **111** — **963 total**, up from 875 at session start. Integration is
  Docker-gated and was not run.
- T126, T132, T133 and T134 each **verified to fail against the unfixed code** before being called
  done. **T133 and T134 also browser-verified** on dev; T126's chart renders but cannot yet fire.
- `harness.py lint --strict` clean; harness 2.4.3 pushed upstream, suite 127. Not re-verified:
  `drift-check.sh`, `restore-rehearsal.sh` — unchanged since 2026-09-20a.
