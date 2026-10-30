# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-10-30 (Opus): T387, flow 14 (data rights), started

The operator said "start flow 14". T387 created and started (P2), written from T379's shape.

- **Steps 3–4 done:** canvas "Wombat 14" https://claude.ai/artifact/FrvjJMZLwRd2Y3AMDEJeGs; design system v37
  (1793144981-e3ba) installed, `tokens.css` by hand (54 properties match app.css), the mark at
  `/_blob/616538c681cf4475edf7c466864e408d`. Canvas files in scratchpad `e81867ab-…/scratchpad/c14`.
- **Steps 1–2 done** (`2bc97e30`): brief restated, Q1–Q16 in its § 6 with recommendations, `round-1-ask.txt`
  written (mark filled in); completeness 0, verbatim 424/0, baseline missing 0; 113 files staged.
- **Round 1 back** (18 boards, `42366e44`); Q1–Q16 taken as recommended by the boards (confirm). Assessed (`2881b938`):
  picks My data rights A, Request A, Queue V1, Decide V1; M1–M6; Q17–Q28 put to the operator.
- **Q1–Q28 taken as recommended** (2026-10-30). `round-2-ask.txt` written (`61a92b31`, 509 lines, ~137 boards).
  Trap for the build: a correction's `TargetId` is a Guid but a trainee profile's id is an int.
- **Round 2 back** (138 boards, `a9904964`). Reviewed and merged (`e1227d44`, `round-2-review.md`: 6 must, 24
  should, 18 nits).
- **F1–F10 taken as recommended**; T388, T389 proposed. `round-3-ask.txt` written (`0c6f3882`, ~148 boards).
- **Round 3 final** (149 boards `d47ab4ea`; check `10d92875`: 59/61 done, 2 partly, 0 regressed).
- **Pushed** master through `c47bc6e0` (the operator's "yes"). **Step 6 started:** § 0 and the plan on `t387` (`7a155e37`;
  worktree `.claude/worktrees/agent-a0cbe10cd07555864`), six suites green in Release.
- **Next:** the operator approves the ten PROPOSED items and the wave (lanes D, K, M, P, then W).

## Session 2026-10-27/28 (Opus): T379, flow 13 (graduation and exit), done; T312 before it

The operator said "push and lets start flow 13", then took every set of questions as recommended (Q1–Q16, Q17–Q27,
F1–F11, B5 option 1), "proceed" (T312), "yes" (the wave), "yes" (the replay). Pushed through `c0801106`.

- **Design:** canvas "Wombat 13" https://claude.ai/artifact/SujH2iB2snNe7VANE4SxdK; rounds 1–3 (27, 168, 192 boards;
  final, `aaa3634b`). The canvas never received the captures in any round (BRIEF § 11 "Flow 13").
- **T312 landed** (`c9d6fe13`; the ended record keeps its list; B5: a STAR counts when a review of the programme issued
  it); confirmed in the browser at 6.21, 6.40, 6.41. Master's Release build had broken on its tests: `232e7701`.
- **Built:** `5cc10c66` (planner and § 0, lanes R/E/V/P, runbook lane, three Sonnet reviews 0 must, one fix pass).
- **Replay:** 331 steps, 286 clean, no regression; 2.28 and 5.28 corrected; one fault fixed (`920f778c`: an erased
  trainee's review offered a dead-end export). States 64 rows; one left (`trainee-profile-edit--end-failed`).
- **Re-sync:** design system **version 37**; three page faults found and fixed (`73baa556`, browser-checked).
- **Lessons:** BRIEF § 11 "Flow 13". **T379 done.**
- **Closed on the operator's word:** T380, T311, T313 (flow 13 did their work). **For review:** the blue "until" at 390 (T385's note) needs a real phone. New proposals T381–T386.
- **Next:** flow 14 (`design/flows/14-data-rights.md`) on the operator's word.

### Environment left

- **The replay app on `:5180`** runs `wombat_scenario_t379` (the appendix's end state, with `73baa556`). Dumps
  `recovery/scenario-t379-post-act{1..6,A}`, `-pre-a515-A`. The six `_s*` scratch DBs were dropped on the operator's
  word (2026-10-28). Old: `wombat_scenario_t376`, `_t373`, `_t369`.
- Scratch `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\67ce8168-e9a2-4dd0-acb1-eab0ccee0868\scratchpad\g379`
  (act, states and step 8 scripts; `replay-pw.env`: never print it).
- Branches `t312`, `t379*` and their worktrees kept.

