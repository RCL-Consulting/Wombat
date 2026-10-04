# State — wombat

Cap: 60 lines. `harness.py lint` enforces it; `harness.py trim` moves the overflow into `log/`.
This is a reset point, not a diary.

## Focus

Wombat is **deployed but not in service**: scenario data only. **The EPA v11.1 stream is built**, the runbook is a
replayed journey catalogue with a screenshot baseline, and **four redesign flows are done**: flow 01, the shell (T335,
`b347e11c`), flow 02, sign-in and account (T339, `f50dffb2`), flow 03, filing an activity (T342, `725237ee`), and flow
04, an assessor's inbox (T350, `06aa51d7`), each designed, built, replayed and in the design system (version 13). Left:
flows 05–20 (T355 next), the deploy, the College's answers, 14 P2s, and P3 polish. **No push: the remote is cut.**

## Now

- **The runbook** (`knowledge/scenario-paediatrics/`): 325 steps, 80 page templates, last replayed against flow 04
  (T350, `wombat_scenario_t350`: 249 no gap, no regression); `coverage.md`, `states.md`; baseline `design/baseline/`.
- **Open: 78 queued (1 P1, 13 P2, 64 P3), T355 in progress, 1 blocked.**
  - **P1:** T157, the deploy, which waits for the operator.
  - **P2, before real users:** T288, T289, T311–T313, T315, T316, T319, T320, T329, T333, T334, T341, T355.
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
- **The GUI redesign: T355, flow 05 (a registrar reads where they stand), step 7, the replay. Opus.** Built: `b020c942`.

## Open questions

- **Defaults adopted on recommendation (the operator may overrule).** All are in EPA-PROGRAMME § 3:
  - D33 part 1, D34, D35, D38(a), D44–D52 (D51: appeals are Dismissed or Remitted; D52: the College authors its types);
  - T131's O1–O8, T237 (no Trainee in any panel seat), T240 (the draft reminder is a digest);
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

- **2026-10-04:** T304, T305 done (`95d4e950`, `a27983fd`); T355, flow 05, designed and built (`b020c942`).
- **2026-10-03:** T350, flow 04, done: replay (no regression), states, design system v13, lessons; T353–T355 filed.
- **09-30 and before:** T342 (flow 03), T339 (flow 02), T335 (flow 01). Earlier: `git log`.
