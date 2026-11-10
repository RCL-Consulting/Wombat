# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-11-09/10 (Opus): T398, flow 17, designed and built (steps 1–6)

The operator said "flow 17", then "Done"/"done" after each round and "yes" at each decision (§ 6 Q1–Q12; the pick and
Q13–Q16; Q17–Q20; round 3's two; the build's P1–P2; F1).
- **Design:** canvases "Wombat 17" https://claude.ai/artifact/AQNS1QZDaUNizG7dzKPSGs (rounds 1–2) and "Wombat 17 · Round 3"
  https://claude.ai/artifact/Cpyw4vwX7UodgJUdKLF7Zw; round 3 checked 82 of 86 done, no round 4.
- **Built and squashed onto master `84bcffeb`** (§ 0, lanes A–C, the sweep, lane W, two Sonnet reviews, one fix pass):
  suites 899/4466/1035/52/4811/532, 0 warnings, no migration. DESIGN.md's banner lists flow 17. T334 folded in (MSF and
  Learner Feedback read-only; F1: a leftover draft may be discarded).
- **For review:** T399 proposed (P1: the other pages' `RefusalText.Of`); the lanes' departures and 32 NEW word groups
  (`build-contracts.md` § 5), accepted by the reviews.
- **Step 7 done** (2026-11-10): acts 1–6, A and both states passes on `wombat_scenario_t398`; three flow 17 faults found
  and fixed on master (`c9742a18` the scale's seed key and the count's space; `277e4a78` the list's one-word columns, the
  list now stacking to 1150px); no regression left; baseline missing 0. Known-task Gaps only otherwise.
- **Next:** steps 8 (re-sync the design system; read the live files first) and 9 (lessons), on the operator's word.
- **Environment:** the replay app on :5180 (`wombat_scenario_t398`, master `277e4a78`) left running; scratch DBs
  `_sweep`, `_s10`, `_s11`, `_s20`, `_s515` kept; dumps `recovery/scenario-t398-post-act{1..6,A}`; worktrees `t398*` kept.
- **Pushed** 2026-11-10 (`399a99be`); the replay's commits since are not pushed.

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
