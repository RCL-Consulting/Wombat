# State — wombat

Cap: 60 lines. `harness.py lint` enforces it; `harness.py trim` moves the overflow into `log/`.
This is a reset point, not a diary.

## Focus

Wombat is **deployed but not in service**: scenario data only. **The EPA v11.1 stream is built**, the runbook is a
replayed journey catalogue with a screenshot baseline, and **ten redesign flows are done**: 01 the shell (T335),
02 sign-in (T339), 03 filing an activity (T342), 04 an assessor's inbox (T350), 05 a registrar's progress (T355), 06
programme oversight (T358), 07 the review sitting (T361, D53), 08 the committee calendar (T364), 09 outcomes, STARs
and appeals (T366, with T367) and 10 the MSF campaign (T369, `68327c9d`), each designed, replayed and in the design
system (version 25). Left: flows 11–20, the deploy, the College's, the P2s, P3 polish. On GitHub (RCL-Consulting/Wombat, public).

## Now

- **The runbook** (`knowledge/scenario-paediatrics/`): last replayed whole against flow 10 (T369,
  `wombat_scenario_t369`, 2026-10-19: no open regression); `coverage.md`, `states.md`, `design/baseline/`.
- **Open: 76 queued; none in progress; 1 blocked; 7 proposed (T362, T363, T365, T368, T370, T371, T372).**
  - **P1:** T157, the deploy, which waits for the operator.
  - **P2, before real users:** T288, T289, T311–T313, T315, T316, T319, T320, T329, T333, T334, T341.
  - **P3:** the presentation debt the redesign absorbs (BRIEF § 6–7), and polish.
- **Gated on the College or the operator:** T139, T170, T146, T152, T153, T171 (`knowledge/college-message-2026-09.md`).

## Blockers

- **T128:** the off-host backup destination: where the encrypted copy goes, and who holds the `age` private key.

## Next

- **T157, the deploy (operator).**
  - Take a `pg_dump` first. The migrations since the 16 September build are many, and T130, T219 and T281 empty the
    progress table for the bootstrapper to refill.
  - Check that production sets `Email__SmtpHost` (T157's note).
- Send the College the § 3F questions, and decide T128.
- **The redesign:** flow 11 (`design/flows/11-onboarding.md`), when the operator says. T371 needs an answer; six more wait in proposed.

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
- `execution/knowledge/EPA-PROGRAMME.md`: § 2A (what is left), § 3 (decisions D1–D55), and § 3F (College asks).
- `execution/architecture/DESIGN.md`: mandatory before any Razor work.
- `design/BRIEF.md` and `execution/knowledge/scenario-paediatrics/coverage.md`: the redesign brief and the journeys.
- `deploy/verify/drift-check.sh`: run before trusting any claim about the server.

## Recent

- **2026-10-19:** T369, flow 10, done (`68327c9d`; replay 331 steps, 281 clean, no regression; states 62; fixes `e4b480e2`; design system v25); T370–T372 proposed.
- **2026-10-15:** T290 done (D54); D55, only a running programme files; Act 5 replayed (29/29), the other D54/D55 steps checked.
- **10-14 and before:** T366 (flow 09), T364 (flow 08), T361 (flow 07), T358, T355, T350, T342, T339, T335 (flows 06 to 01); the repo moved to GitHub. Earlier: `git log`.
