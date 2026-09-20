# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-09-20b (Opus) — the College's answers, T134, T126, T132

Three application tasks shipped, fourteen product decisions closed, one upstream harness fix.

### Done

- **[T134] — a v11.1 trainee's committee sampling report said there was NO rated evidence.**
  The gate matched EXACT keys over `mini_cex`/`dops`/`cbd`/`acat`, so the whole seeded CPSA tool set
  fell outside it. One shared `RatedActivityTypes` classifier now answers it for the whole repo:
  **declared rating (T126) OR known family**. The disjunction is the point: a pointer-only gate
  leaves the operator-built `*_paed` types reading zero, and forces rewriting the test fixture —
  which is the only evidence legacy behaviour is unchanged. **The fixture is untouched.** Two
  duplicate maps retired; `WbaSourceCategory` deleted with both members grep proved dead.
- **The College answered all fourteen decisions — [T129] closed.** Transcribed into
  `EPA-PROGRAMME.md` § 3A-ii, each with the option chosen and its consequence.
  **[T120], [T121], [T122] and [T130] are all released.** Six needed a second pass — D6 "all
  rated" would have had trainees rating themselves; D12 answered a different pairing; D13 blank;
  D8 "not sure"; D7 unanswered; D15 ambiguous. All put back with the reason, and answered.
- **Renumbered [T121]'s local `D1`–`D4`** to the College's **D9, D8, D10, D11**; they collided.
- **[T126] — an activity can now say which ladder it was rated against.** `rated_level_field`
  on `FormSchema`, following T119's pointer exactly; eight seeds declare it, six abstain. D30's
  off-ladder rule now works. **The composite FK the task specified was NOT built** — Activity's
  snapshot columns deliberately carry no FK and nothing filters on a rating. No migration.
- **[T132] — MSF expiry reminders mailed the token *hash* in a relative URL.** Now re-issued.
- Filed [T130]/[T131]; re-rated the queue EPA-first; [T128] to `blocked/`; retitled [T102].

### Filed, not fixed

- **[T133] P1 — the visual builder erases every root schema pointer**, so an operator draft save
  silently drops `observation_date_field` (a live T119 regression) and `rated_level_field`. After
  T134 that un-declares a committee statistic. **Carry-through alone is not enough** — read the task.
- **[T135] P2** — drafts count as rated evidence, and an unparseable row is in neither the
  numerator nor the withheld count while `EvidenceComplete` reads true.

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
3. **[T133]** — P1, and now load-bearing: after T134 a builder save that erases `rated_level_field`
   un-declares a committee statistic. The family arm masks it for the ten known families.
4. **[T128]** — still blocked on you: destination + `age` key holder.

### Traps

- **`counts_for` is permanent per pinned version.** `Activity.SchemaVersion` is assigned once, there
  is no re-pin path, and a rebuild replays against the pinned version. "Ship `[]` now and switch
  later" was never available — which is why D8 had to be right first time. It is: MSF gets `[]`.
- **Nothing this session was browser-checked, and no database was queried.** T126's chart change and
  T134's report both reason from code. T134's disjunction was chosen partly so it works either way.
- **Seeded types republish at next boot** (eight schema files changed), stranding in-flight
  activities by design. `OffLadder = false` means "no disagreement established", not "on the ladder".
- **`lint` tells you to run `trim` when STATE/HANDOFF overflow, and `trim` reports "moved 0".** The
  advice is unactionable; both files were cut by hand. Worth fixing upstream next time you are there.
- **The family arm in `RatedActivityTypes` is INTERIM.** It retires when [T133] makes the pointer
  authorable and [T122]'s `WbaToolKey` lands. Tests pin it so removing it is a decision.
- Hooks hard-code `C:\dev\rcl_execution\bin\harness.py` (`main`, not `master`). Solution vs per-project
  builds resolve different Release trees — never pass `--no-build`.

### Verification status

- `dotnet build Wombat.sln -c Release` — **0 warnings, 0 errors**.
- Suites green, no `--no-build`: Domain **78**, Application **544**, Infrastructure **207**,
  Architecture **23**, Web **103** — **955 total**, up from 875 at session start. Integration is
  Docker-gated and was not run.
- T126, T132 and T134 were each **verified to fail against the unfixed code** before being called
  done — T134 three separate ways, including one that proves the denominator ordering is load-bearing.
- `harness.py lint --strict` clean; harness 2.4.3 pushed upstream, suite 127. Not re-verified:
  `drift-check.sh`, `restore-rehearsal.sh` — unchanged since 2026-09-20a.
