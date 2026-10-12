# State — wombat

Cap: 60 lines. `harness.py lint` enforces it; `harness.py trim` moves the overflow into `log/`.
This is a reset point, not a diary.

## Focus

Wombat is **deployed but not in service**: scenario data only. **The EPA v11.1 stream is built**, the runbook is a
replayed journey catalogue with a screenshot baseline, and **seven redesign flows are done**: 01 the shell (T335,
`b347e11c`), 02 sign-in (T339, `f50dffb2`), 03 filing an activity (T342, `725237ee`), 04 an assessor's inbox (T350,
`06aa51d7`), 05 a registrar's progress (T355, `b020c942`) 06 programme oversight (T358, `85d5a508`) and 07 the review sitting (T361, `44de180b`, with D53), each designed, replayed
and in the design system (version 19). Left: flows 08–20, the deploy, the College's, the P2s, P3 polish. The repo is on GitHub (RCL-Consulting/Wombat, public) since 2026-10-05.

## Now

- **The runbook** (`knowledge/scenario-paediatrics/`): 331 steps, last replayed against flow 07 (T361,
  `wombat_scenario_t361`: 267 no gap, no open regression); `coverage.md`, `states.md`, `design/baseline/`.
- **Open: 78 queued; T290 (its other items) in progress; 1 blocked.**
  - **P1:** T157, the deploy, which waits for the operator.
  - **P2, before real users:** T288, T289, T311–T313, T315, T316, T319, T320, T329, T333, T334, T341.
  - **P3:** the presentation debt the redesign absorbs (BRIEF § 6–7), and polish.
- **Gated on the College or the operator:** T139, T170, T146, T152, T153, T171 (`knowledge/college-message-2026-09.md`).

## Blockers

- **T128:** the off-host backup destination. It needs one operator decision: where the encrypted copy goes, and who
  holds the `age` private key.

## Next

- **T157, the deploy (operator).**
  - Take a `pg_dump` first. The migrations since the 16 September build are many, and T130, T219 and T281 empty the
    progress table for the bootstrapper to refill.
  - Check that production sets `Email__SmtpHost` (T157's note).
- Send the College the § 3F questions, and decide T128.
- **The redesign:** flow 08 next (BRIEF § 8), when the operator says. T362 and T363 (flow 07's follow-ups) wait in proposed.

## Open questions

- **Defaults adopted on recommendation (the operator may overrule).** All are in EPA-PROGRAMME § 3:
  - D33 part 1, D34, D35, D38(a), D44–D52 (D51: appeals are Dismissed or Remitted; D52: the College authors its types);
  - T131's O1 and O3–O8 (O2 overruled by the operator's D53, 2026-10-12: no deferral), T237 (no Trainee in any panel seat), T240 (the draft reminder is a digest);
  - T258 (erasure may be requested whatever the reviews' state), T268 (a locked account is not current);
  - T273, T281 (no credit after the last day), T284 (opening a draft is new work); flow 03's 3.10 (T342);
  - T305: an archived record has no correction path yet (a start found wrong after graduation): open.
- **Dev database:** snapshots `recovery/pre-*.dump`; replays' `scenario-*post-act*.dump`. Dev PG 16, production 18 (T275).

## Files to open first

- `CLAUDE.md`: conventions, footguns, and the "nothing is live" section.
- `execution/knowledge/EPA-PROGRAMME.md`: § 2A (what is left), § 3 (decisions D1–D52), and § 3F (College asks).
- `execution/architecture/DESIGN.md`: mandatory before any Razor work.
- `design/BRIEF.md` and `execution/knowledge/scenario-paediatrics/coverage.md`: the redesign brief and the journeys.
- `deploy/verify/drift-check.sh`: run before trusting any claim about the server.

## Recent

- **2026-10-12:** T361, flow 07, done (`44de180b`; D53; replay 331 steps, five fixes; states 82/83; design system v19); T356, T273 closed; T363 proposed.
- **10-04 to 10-05:** T358, flow 06, done (`85d5a508`, design system v17); T355, flow 05 (`b020c942`); T304, T305; the repo moved to GitHub.
- **10-03 and before:** T350 (flow 04), T342 (flow 03), T339 (flow 02), T335 (flow 01). Earlier: `git log`.
