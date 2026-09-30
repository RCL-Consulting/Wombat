# State — wombat

Cap: 60 lines. `harness.py lint` enforces it; `harness.py trim` moves the overflow into `log/`.
This is a reset point, not a diary.

## Focus

Wombat is **deployed but not in service**: scenario data only. **The EPA v11.1 stream is built**, the runbook is a
replayed journey catalogue with a screenshot baseline, and **three redesign flows are done**: flow 01, the shell (T335,
`b347e11c`), flow 02, sign-in and account (T339, `f50dffb2`), and flow 03, filing an activity (T342, `725237ee`), each
designed, built, replayed and in the design system (version 10). Left: flows 04–20, the deploy, the College's answers,
15 P2s, and P3 polish.

## Now

- **The runbook** (`knowledge/scenario-paediatrics/`): 325 steps, all 80 page templates, last replayed whole against
  flow 03 (T342, `wombat_scenario_t342b`: 254 with no gap); `coverage.md`, `states.md`; baseline in `design/baseline/`.
- **Open: 77 queued (1 P1, 16 P2, 60 P3), 1 blocked.**
  - **P1:** T157, the deploy, which waits for the operator.
  - **P2, before real users:** T288, T289, T304, T305, T311–T313, T315, T316, T319, T320, T329, T333, T334, T341;
    and T350, flow 04.
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
- **The GUI redesign: T350, flow 04, an assessor's inbox (queued). Model: Opus.** Flow 03's loop is its model.

## Open questions

- **Defaults adopted on recommendation (the operator may overrule).** All are in EPA-PROGRAMME § 3:
  - D33 part 1, D34, D35, D38(a), D44–D52 (D51: appeals are Dismissed or Remitted; D52: the College authors its types);
  - T131's O1–O8, T237 (no Trainee in any panel seat), T240 (the draft reminder is a digest);
  - T258 (erasure may be requested whatever the reviews' state), T268 (a locked account is not current);
  - T273, T281 (no credit after the last day) and T284 (opening a draft is new work);
  - flow 03's 3.10: the refusal summary keeps a corrected date's line until the next submit (T342).
- **Dev database:** snapshots `recovery/pre-*.dump`; replays' `scenario-*post-act*.dump`. Dev PG 16, production 18 (T275).

## Files to open first

- `CLAUDE.md`: conventions, footguns, and the "nothing is live" section.
- `execution/knowledge/EPA-PROGRAMME.md`: § 2A (what is left), § 3 (decisions D1–D52), and § 3F (College asks).
- `execution/architecture/DESIGN.md`: mandatory before any Razor work.
- `design/BRIEF.md` and `execution/knowledge/scenario-paediatrics/coverage.md`: the redesign brief and the journeys.
- `deploy/verify/drift-check.sh`: run before trusting any claim about the server.

## Recent

- **2026-09-30:** T342, flow 03: replayed (recovered after a shutdown), 3 fixes, design system v10; T348–T350 filed.
- **2026-09-28:** T339, flow 02 (`f50dffb2`, v7). 09-27: T335, flow 01. 09-26: group 1. Earlier: `git log`, "As built".
