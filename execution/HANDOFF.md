# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-11-04 (Opus): scratch DBs dropped; T394, flow 16, started

The operator said "what is next", then "Drop the scratch db's and start the next flow".
- Dropped `wombat_scenario_t387_s10`, `_s20`, `_s21`, `_s22`, `_s515`. Kept the replays' end-state DBs
  (`wombat_scenario_t290`…`_t392`, `_pdf`, bare `wombat_scenario`): the operator's call whether they go too.
- **T394 filed and started** (flow 16, the College keeps the national catalogue; `2bbc0768`). Steps 3–4 done: canvas
  "Wombat 16" https://claude.ai/artifact/AqaH447KL91WCTbjQFJnWf, design system v45 installed, `tokens.css` by hand,
  the mark at `/_blob/ce366cc30297c5a90255d5d0a88fa6b5`. Canvas scratch: `...ƽ60d182d14635f8969ed0-...\scratchpad\c16`.
- **In flight:** one Opus agent runs steps 1–2 (restate `design/flows/16-college-catalogue.md`, the two check scripts,
  `stage_upload.ps1 -Flow 16`, `round-1-ask.txt` with a `MARK:` placeholder) and commits them itself. If the session
  ended before it reported: check `git log` for "flow 16 brief restated"; if absent, rerun steps 1–2.
- **Next:** fill the mark's URL into `round-1-ask.txt`, bring the operator the agent's Q1…, then round 1 (fidelity,
  part 1). Not pushed.

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
