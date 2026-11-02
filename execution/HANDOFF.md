# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-11-02 (Opus): housekeeping

The operator said "push and close the tasks". Pushed master (`75eefc6c..f0348410`). Closed **T316** and **T319** as done:
flow 14 (T387) built them, and its replay records them as no longer occurring (a closing note in each). No code changed.
- **Next:** flow 15 (an institution adopting the catalogue) on the operator's word, or a P2 (T320 the largest).
- Still the operator's: T157 the deploy, T128's backup destination, the College's § 3F questions, dropping the
  `wombat_scenario_t387_s*` scratch DBs; the unfiled waiting-feedback and `aria-busy` gaps from flow 14.

## Session 2026-10-30 (Opus): T387, flow 14 (data rights), done

The operator said "start flow 14", took Q1–Q28 and F1–F10 as recommended, chose S1 (scrub the erased person's reason
and correction values, keep the note), "yes" (push, the wave), "proceed" (push and the replay). Pushed through `75eefc6c`.

- **Design:** canvas "Wombat 14" https://claude.ai/artifact/FrvjJMZLwRd2Y3AMDEJeGs; rounds 1–3 (18, 138, 149 boards;
  final, `10d92875`: 59/61 done, 2 partly).
- **Built:** `e8eea1d2` (planner and § 0, lanes D/K/M/P, runbook lane W, three Sonnet reviews, one fix pass; P5 made
  ConfirmDialog's action mode open on Cancel app-wide); six suites green.
- **Replay** (fresh `wombat_scenario_t387`, D = 2026-10-30): 332 steps, 295 clean, no flow 14 regression
  (`31b1a85a`–`3db79b0f`); 5.20's Expect and the appendix's outcome counts corrected. States 65 rows (`13e6325e`,
  `698beeff`); one fault fixed (`5b5aab80`: a reason counter after Blazor empties its box).
- **Re-sync:** design system **version 41** (`a6b8750e`, review `resync-review.md` 0 must; the faults it found fixed,
  `1c11d1f6`). **Lessons:** BRIEF § 11 "Flow 14". **T387 done.**
- **For review:** T316 and T319 no longer occur on the data-rights pages (A.1.8a's correction completes; every decision
  mails the requester): close them? T264's other half (the entrustment scale's Delete has no dialog, 6.8) stays open.
  Not filed, the operator's call: a failed submit or decision takes about a minute with nothing said meanwhile; My data
  rights' skeleton has no `aria-busy`. New proposals T388–T391 (T390 the pager hidden by a size holding every row, five
  lists; T391 the replay tool's stop missing a live listener).
- **Next:** flow 15 (an institution adopting the catalogue) on the operator's word; master is ahead of origin (ask
  before pushing).

### Environment left

- **The replay app on `:5180`** runs `wombat_scenario_t387` (the appendix's end state, published with `5b5aab80`; not
  republished with `1c11d1f6`, a words and comment change). Dumps `recovery/scenario-t387-post-act{1..6,A}`,
  `-pre-a515-A`. Scratch DBs `wombat_scenario_t387_s515`, `_s10`, `_s20`–`_s22` kept (drop on the operator's word); nothing
  runs on `:5181`. Old: `wombat_scenario_t379`, `_t376`, `_t373`, `_t369`.
- Scratch `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\e81867ab-c351-40bf-9d41-d1d61fe30f43\scratchpad`:
  `pw/` (Playwright 1.56.1 and `lib.mjs`, for the next replay), `g387/` (act, states and step 8 scripts;
  `replay-pw.env`: never print it). Branches `t387*` and their worktrees kept.

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

