# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-11-02 (Opus): T392, flow 15 (an institution adopts the catalogue), in progress

The operator said "lets do flow 15", then "Done" (round 1), "yes" (picks, Q18–Q21), "done" (round 2), "yes" (F1–F9 as
recommended). T392's Notes hold every step; not pushed (master ahead of origin since `a87041ac`: ask before pushing).
- **Design:** canvas "Wombat 15" https://claude.ai/artifact/QvQL26tCCJh6S9zoLBywdZ; brief restated (`1b0676a2`);
  round 1 (17 boards, `ff17241b`), round 2 (168 boards, `9c37e0ea`; review `2780557d`: 6 must, 25 should, 18 nit).
- **Round 3's ask** written (`5fe15328`, `round-3-ask.txt`: 93 redrawn, 12 new screens, 11 captures to reopen,
  the canvas saves its stylesheet as f15r3.css); wording the agent assumed is marked NEW in it.
- **Round 3** drawn (111 boards, `7ee1206a`) and checked (`round-3-check.md`, `e91f5c7f`: 45/48 done, 2 partly, 1 not).
- **Step 6, built:** `c931d56f` (lanes A–D, W; two Sonnet reviews, one fix pass, all but P5 left to T390; six suites
  green). T393 proposed (two edit pages print exception text).
- **Step 7, replay, done** (T392's Notes have the counts): acts 1-6 and A replayed, no flow 15 regression left; two flow
  15 layout faults found and fixed (`e10a5f22` the items table, `50ca6470` the EPA list); states 98 rows re-taken
  (`dff8b957`, `649eb643`); baseline missing 0. The replay app on `:5180` runs the appendix's end state, published
  from `50ca6470`. Scratch DBs `wombat_scenario_t392_s10`, `_s11`, `_s20`-`_s25`, `_s515` kept (drop on the word).
- **Next:** step 8 (re-sync the design system, read the live files first) and step 9 (lessons), on the operator's
  word; then close T392 (T291 items 1/4, T301, T304, T326's scales half no longer occur: the operator's call to close).
- **Replay scratch:** `...\ecc5a9d5-...\scratchpad\g392` (prompts, act dirs, `replay-pw.env`: never print it).
- **Traps:** the harness is at `C:\dev\rcl_execution\bin\harness.py`, not `bin/`. Canvas scratch:
  `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\ecc5a9d5-2a6e-43d8-a55a-e55b7e48f8ab\scratchpad\c15`.

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
