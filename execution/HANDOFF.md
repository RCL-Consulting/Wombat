# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-11-09/10 (Opus): T398, flow 17, steps 1–5 (rounds 1–3)

The operator said "flow 17", then "Done" (sent the round-1 ask; taken as "yes" to brief § 6 Q1–Q12 as recommended).
- **T398 in progress.** Canvas "Wombat 17" https://claude.ai/artifact/AQNS1QZDaUNizG7dzKPSGs (design system v49, mark
  `/_blob/a89fec160a8a8c0b20116ccc3634e0bc`). Brief restated and staged (`7d8ce207`); round 1 (17 boards, `f17r1.css`)
  saved in `design/flows/17-activity-type-builder/round-1/` (`465480c7`): A tabs, B one page, C a stepped path.
- **Round 1 assessed** (`ab13dd3d`): pick A; M1–M7; Q13–Q16 put to the operator. Captures never reached the canvas.
- **Round 3 drawn and saved** (`684119ce`, 87 boards in `round-3/`) on "Wombat 17 · Round 3" https://claude.ai/artifact/Cpyw4vwX7UodgJUdKLF7Zw. Checked (`round-3-check.md`): 82 of 86 done, 4 partly, one regression (`li role="group"`). Decisions taken (no round 4). Build plan `build-lanes.md`/`build-contracts.md` (`af28d571`): § 0, lanes A/B/C, sweep, W, two reviews, fix pass; no migration. P1–P2 taken (T399 proposed for P1's other pages). § 0 done (`d3c98db5`, notes `08d1bbda` on `t398`). Lanes A, B, C merged on `t398`
  (suites 895/4459/1035/52/4806/532). Sweep passed (3 fixes), lane W merged (`68e127d8`).
  Reviewed (`build-review.md`, `ffe7c9f8`): 1 must, 7 should.
  F1 as recommended. **The fix pass running** (`t398-fix`); then the squash.
- **Trap:** the boards keep the workflow JSON "as typed"; jsonb re-renders it, so that is a stored text column (DATA).
- **Not pushed:** master ahead of origin with flows 16–17. Ask before pushing.

## Session 2026-11-09 (Opus): T394 steps 8 and 9; T394 done

The operator asked "what is next??", then "yes" to steps 8 and 9.
- **Step 8:** one Opus agent re-synced `design/system/` to flow 16 (`0c6af0c1`, `36b3bc04`; the live system matched HEAD
  but for CRLF). A Sonnet review (`design/flows/16-college-catalogue/resync-review.md`) found 0 must, 3 should and 4
  nits, all fixed here (`0034f728`, `f98a61f0`). Published: the files as version 48, the index alone as **version 49**
  (1794239792-3119). Versions 46 and 47 were not ours (presumably the type's releases).
- **Step 9:** BRIEF § 11 "Flow 16", six lessons (`74989833`). **T394 done.**
- **For review:**
  - The page faults the re-sync found are in the system README's flow 16 known gaps, not filed. Among them:
    - Colleges' and the tree's widths at 1025–1150px (inferred);
    - the College record's empty result row;
    - the tree's trail while it reads.
  - T393 (curriculum and scale pages), T324, T264's scales half, T291 item 2, T326 and T331 no longer occur: close them?
  - T396 is still proposed.
  - The runbook's cast cell for Dr Kruger ("CMSA College registrar") reads as a College to a reader.
- **Not pushed:** master is ahead of origin with all of flow 16. Ask before pushing.
- **Next:** flow 17 on the operator's word, or a P2 (T320 the largest).

## Session 2026-11-04/05 (Opus): T394, flow 16, designed and built (steps 1–6)

The operator said "Drop the scratch db's and start the next flow", then "yes" at each decision (§ 6 Q1–Q14; round 1's
picks and Q15–Q17; round 2's Q18–Q21; round 3's decisions 1–5; the build's P1–P3; F1 and F2), "done" after each round
(round 3 "required a new canvas"), "Fix T397 now", "proceed" (step 7), then "close the replay app and finalise
the session".
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
- **Step 7 done** (T394's Notes): acts 1-6 and A and both states passes, no flow 16 regression left; the catalogue
  lists' 1280 column widths fixed in `a2878763`, `7d4c4490`, `f7cb1903`, `c6bbb641`, `2c93b247` (Curricula and the
  items now stack to 1150px; DESIGN.md amended); captures re-taken, baseline missing 0.
- **Next:** step 8 (re-sync the design system: read the live files first) and step 9 (lessons, BRIEF § 11); then close
  T394. Not pushed.
- **Environment:** the replay app stopped (2026-11-05); `wombat_scenario_t394` kept at the appendix's end; lane branches `t394*` kept; this flow's scratch DBs dropped; dumps `recovery/scenario-t394-post-act{1..6,A}`.

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
