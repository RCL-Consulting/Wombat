# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-11-04/05 (Opus): T394, flow 16, designed and built (steps 1–6)

The operator said "Drop the scratch db's and start the next flow", then "yes" at each decision (§ 6 Q1–Q14; round 1's
picks and Q15–Q17; round 2's Q18–Q21; round 3's decisions 1–5; the build's P1–P3; F1 and F2), "done" after each round
(round 3 "required a new canvas"), and "Fix T397 now".
- **Design:** canvases "Wombat 16" https://claude.ai/artifact/AqaH447KL91WCTbjQFJnWf (rounds 1–2) and "Wombat 16 · Round 3"
  https://claude.ai/artifact/W1R56z3QGxfK2GPMEcp3c9. D56 (deactivation asks first) and D57 (the College reads the scales).
- **Built and squashed onto master `de4cc718`** (§ 0, lanes A–D, the sweep, lane W, two Sonnet reviews, one fix pass,
  T397's fix): suites 836/4411/1035/52/4675/530; master builds with 0 warnings; baseline missing 0. T395 and T397 done.
- **For review:** T396 (proposed: the Q16/Q19 guards' check-then-save race); the lanes' departures from the boards and the
  ~38 words on no board, accepted (F2); Colleges with five rows (R3-B16) not swept. `scenario-replay.ps1 stop` misses the
  app it started (twice this session): stop by process id after reading its command line. bUnit's
  `blazor:elementReference` guids can break a "markup holds no N" assertion.
- **Step 7 started** ("proceed"): `wombat_scenario_t394` on :5180 from master `c2de9b7b`; the replay brief and act
  prompts being written (scratch `...\67560d182d14635f\28969ed0-c979-487c-82e2-606d8331c3a3\scratchpad\g394`).
- **Next:** acts 1–3 done (`c414a804`, `5f03a68f`, `7d728f5c`: no flow 16 regression; act 1's two 1280px column faults fixed on
  master `a2878763`, not yet on :5180); act 4 running; then 5, 6 and A, one agent each, in order (an act whose
  post-act dump in recovery is absent did not finish: resume it from its out files in the g394 scratch); then
  publish `a2878763` to :5180, re-take 1.2 and 1.12, the states; then steps 8 and 9. Not pushed.
- **Environment:** the replay app runs on :5180; lane branches `t394*` kept, worktrees removed; scratch DBs of this flow dropped.

## Session 2026-11-02/03 (Opus): T392's replay, re-sync and lessons; T392 done

The operator said "the replay was interrupted, can you continue?", "drop the scratch databases", "finalise the session",
then "do step 8 & 9 and then push", "close the replay app", "finalise the session". Act 4 resumed at 4.49; acts 4–A and both states passes ran (one Opus agent each);
two flow 15 layout faults fixed here (`e10a5f22`, `50ca6470`). Step 8: re-synced (`9e3a357d`), reviewed (0 must, all
fixed, `20bbab12`), published as design system **version 45**. Step 9: BRIEF § 11 "Flow 15" (`3c44b4b6`). **T392 done.**
Pushed master to origin.
- **For review:** T291 items 1/4, T301, T304, T323's item-editor half, T326's scales half no longer occur (T392's
  replay): close or trim them? Unfiled: flow 16's CurriculumEdit (validator messages twice and unlinked, 36-40 px
  controls at 390, no result after a create or clone); the EPA list's Create EPA under the long subtitle; four page
  faults in the design system README's known gaps (a national EPA's record without cards; the EPA list's empty state
  by MayCreate alone, inferred; the profile's start-date check without a role; the Administrator's institution form
  unnamed). Three EPA list states keep the old column widths (`epas-list--college`, `--institution`, `--inactive`).
- **Next:** flow 16 on the operator's word, or a P2 (T320 the largest).
- **Environment left:** the replay app stopped (2026-11-03); `wombat_scenario_t392` kept at the appendix's end;
  dumps `recovery/scenario-t392-post-act{1..6,A}`, `-pre-a515-A`; T392's scratch DBs dropped. Old scratch DBs of
  earlier replays (`wombat_scenario_t387_s*` and older) still exist.

## Session 2026-11-02 (Opus): T392, flow 15 (an institution adopts the catalogue), design to build

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
  from `50ca6470`. Its scratch DBs were dropped 2026-11-03.
- **Next:** step 8 (re-sync the design system, read the live files first) and step 9 (lessons), on the operator's
  word; then close T392 (T291 items 1/4, T301, T304, T326's scales half no longer occur: the operator's call to close).
- **Replay scratch:** `...\ecc5a9d5-...\scratchpad\g392` (prompts, act dirs, `replay-pw.env`: never print it).
- **Traps:** the harness is at `C:\dev\rcl_execution\bin\harness.py`, not `bin/`. Canvas scratch:
  `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\ecc5a9d5-2a6e-43d8-a55a-e55b7e48f8ab\scratchpad\c15`.
