# State — wombat

Cap: 60 lines. `harness.py lint` enforces it; `harness.py trim` moves the overflow into `log/`.
This is a reset point, not a diary.

## Focus

Wombat is **deployed but not in service**: scenario data only. **The EPA v11.1 stream is built**, the runbook is a
replayed journey catalogue with a screenshot baseline, and **the redesign's pilot is done**: flow 01, the shell, is
designed, built (`b347e11c`), replayed and in the design system (T335). Left: flows 02–20, the deploy, the College's
answers, 14 P2s, and P3 polish.

## Now

- **The runbook** (`knowledge/scenario-paediatrics/`): 325 steps playing all 80 page templates, last replayed whole
  against `b347e11c` (T335 step G, `wombat_scenario_t335`); `coverage.md` indexes every role's jobs, `states.md` its
  states; T294's test holds it to the routes. Baseline in gitignored `design/baseline/`.
- **Group 1 fixed before designing:** T297, T300 (D52), T302, T303, T307 (D51), each browser-verified on its act.
- **Open: 67 queued (1 P1, 14 P2, 52 P3), 1 blocked.**
  - **P1:** T157, the deploy, which waits for the operator.
  - **P2, before real users:** T288, T289, T304, T305, T311–T313, T315, T316, T319, T320, T329, T333, T334.
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
- **The GUI redesign: T339, flow 02 (in progress). Model: Opus.** Follow BRIEF § 2.3 as § 11 corrects it: a new canvas, the mark first.

## Open questions

- **Defaults adopted on recommendation (the operator may overrule).** All are in EPA-PROGRAMME § 3:
  - D33 part 1, D34, D35, D38(a), D44–D52 (D51: appeals are Dismissed or Remitted; D52: the College authors its types);
  - T131's O1–O8, T237 (no Trainee in any panel seat), T240 (the draft reminder is a digest);
  - T258 (erasure may be requested whatever the reviews' state), T268 (a locked account is not current);
  - T273, T281 (no credit after the last day) and T284 (opening a draft is new work).
- **Dev database:** snapshots `recovery/pre-*.dump`; replays' `scenario-*post-act*.dump`. Dev PG 16, production 18 (T275).

## Files to open first

- `CLAUDE.md`: conventions, footguns, and the "nothing is live" section.
- `execution/knowledge/EPA-PROGRAMME.md`: § 2A (what is left), § 3 (decisions D1–D52), and § 3F (College asks).
- `execution/architecture/DESIGN.md`: mandatory before any Razor work.
- `design/BRIEF.md` and `execution/knowledge/scenario-paediatrics/coverage.md`: the redesign brief and the journeys.
- `deploy/verify/drift-check.sh`: run before trusting any claim about the server.

## Recent

- **2026-09-27:** T335, the pilot: flow 01 designed, built (`b347e11c`), replayed, lessons in BRIEF § 11; T190, T321,
  T322, T330, T331 closed by it. 2026-09-26: T292–T296, T332, group 1. Earlier: see `git log` and each "As built".
