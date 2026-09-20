# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-09-20b (Opus) — the College's answers, and T110/T126/T132/T133/T134

Five application tasks shipped, fourteen product decisions closed, one upstream harness fix.

### Done

- **[T133]** — the visual builder erased every root schema pointer on every operator draft save.
  Carry-through, an orphaned-pointer guard, and a Form-tab control. Browser-verified.
- **[T110]** — four generic seeds declared `scale_key: "or_scale"` and bound to nothing. 23 fields
  rebound, plus the corpus guard whose absence was the root cause.
- **[T134]** — every v11.1 trainee's committee report read zero rated evidence, because the gate
  matched exact keys over four legacy tools. One shared classifier answers it now. Browser-verified.
- **The College answered all fourteen decisions — [T129] closed**, transcribed into
  `EPA-PROGRAMME.md` § 3A-ii. **[T120]/[T121]/[T122]/[T130] released.** Six needed a second pass;
  D6's "all rated" would have had trainees rating themselves. Two residual questions below.
- **Renumbered [T121]'s local `D1`–`D4`** to the College's **D9, D8, D10, D11**; they collided.
- **[T126]** — an activity can say which ladder it was rated against (`rated_level_field`); D30's
  off-ladder rule works. No migration and no FK.
- **[T132]** — MSF expiry reminders mailed the token *hash*; now re-issued. Filed [T130]/[T131];
  re-rated the queue EPA-first; [T128] to `blocked/`; retitled [T102].
- **Emptied the dev database and removed three compatibility hedges (W-006).** The operator
  restated that nothing is live. An audit found three places today's work preserved existing rows:
  T134's disjunctive gate, T126's off-ladder rule being inert, and D25's unmerged ladder. Dropping
  and rebuilding dev removed what all three were protecting, in one act.
  **18 types → 14, 52 versions → 14 (all v1 with pointers), 15 activities → 0, 3 scales → 2,
  the four `*_paed` types gone, 16/16 curriculum items pinned.** Then: the gate became the declared
  pointer alone, the trajectory's **KNOWN LIMITATION closed**, and the sampling fixture — which
  seeded types with a null `SchemaJson`, a shape unpublishable since T126 — now models a real type.
  Suites **1009**. **Production deliberately untouched.**

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
3. **[T135]** (sampling denominator/numerator) and **[T136]** (a curriculum scale change fails
   silently — no message, correct refusal, found browser-verifying T126).
4. **[T128]** — still blocked on you: destination + `age` key holder.

### Traps

- **`counts_for` is permanent per pinned version** — no re-pin path, and a rebuild replays against
  the pinned one. That is why D8 had to be right first time. MSF gets `[]`.
- **The dev database is empty of activities and users beyond the three seeded ones.** Anything you
  remember about dev data is stale, including activity 15 and the scenario logins. Re-run the
  runbooks if you need a populated corpus; the seeded admin/trainee/committee accounts work.
- **`OffLadder = false` means "no disagreement established"**, never "on the ladder".
- **`RatedActivityTypes`' family map now only LABELS** — it no longer decides what is rated.
  It retires with [T122]'s `WbaToolKey`.
- Hooks hard-code `C:\dev\rcl_execution\bin\harness.py` (`main`). Never pass `--no-build`.

### Verification status

- `dotnet build Wombat.sln -c Release` — **0 warnings, 0 errors**.
- Suites green, no `--no-build`: Domain **78**, Application **546**, Infrastructure **251**,
  Architecture **23**, Web **111** — **1009 total**, up from 875 at session start. Integration is
  Docker-gated and was not run.
- T126, T132, T133 and T134 each **verified to fail against the unfixed code**, and T126, T133 and
  T134 **browser-verified on dev** before the rebuild. T126's off-ladder rule was shown firing end
  to end; that took a hand-filed activity and a temporary curriculum re-pin then, and takes neither
  now that every type is at v1 with its pointer.
- `harness.py lint --strict` clean; harness 2.4.3 pushed upstream, suite 127. Not re-verified:
  `drift-check.sh`, `restore-rehearsal.sh` — unchanged since 2026-09-20a.
