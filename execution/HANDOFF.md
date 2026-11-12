# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-11-11 (Opus): T400, flow 18, designed, built and reviewed; waiting on F1

The machine restarted at 22:31, seven minutes after the last commit (`05b1a0de`, 22:23). Checked 2026-11-12: master and
the five `t400*` worktrees are clean, nothing was in flight, no app is running. This block was written after the restart
from T400's notes, which are complete.
- **Design:** canvases "Wombat 18" https://claude.ai/artifact/FE7WSRNTmSp88bvS4dZRGV (rounds 1–2) and "Wombat 18 · Round 3";
  Q1–Q18, T1–T24 and P1 taken as recommended; no round 4. Records in `design/flows/18-platform-operations/`.
- **Built on branch `t400`** (`f26cd81c`, 16 commits ahead of master, not squashed): § 0 with migration
  `20261111182215_T400_EmailOutcomes`, lanes A–C, the sweep, lane W, two Sonnet reviews (`build-review.md`). Suites
  907/4553/1039/52/4950/541, 0 warnings.
- **2026-11-12:** F1 taken as recommended; the fix pass done; **squashed onto master `34b5077e`** (suites
  907/4560/1039/52/4952/542, 0 warnings; DESIGN.md's banner lists flow 18).
- **Step 7 done** (brief `b331c64e`; acts `d5e676ff`…`3fbdd3b0`; states `559c9cee`, `c6d0642b`): acts 1–6 and A
  clean but known-task Gaps (T365, T320, T382, T286, T385); four flow 18 faults found and fixed on master (`4454e915`
  the jobs' times, `565b978a` Run history's select and widths, `a7d14b19` Home's times), re-taken and measured; flows
  01–07's step pointers re-synced (`1c06aa31`). Baseline missing 0. T402 proposed.
- **For review:** Institutions' 200-character name over 7 lines (not fixed: any share suits one email length); the audit
  log's "TransitionActivityCommand" over two lines with failures; `audit-detail--subject` reachable only by a staged row.
- **Next:** steps 8 (design system re-sync and republish) and 9 (lessons) on the operator's word.
- **Environment:** the replay app on :5180 (`wombat_scenario_t400`, master `1c06aa31`) left running; scratch DBs
  `wombat_scenario_t400_{sweep,s10,s11,s20,s21,s22,s23,s515}` kept; dumps `recovery/scenario-t400-post-act{1..6,A}`.
- **Lane and sweep notes** in the earlier session's scratchpad
  (`…\5ae5486c-8726-4e89-ae35-c60429e28267\scratchpad\b400`), which survived.
- **Not pushed:** `337a6675` onward. Ask before pushing.

## Session 2026-11-09 to 11 (Opus): T398, flow 17, designed, built, replayed and synced; T398 done

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
- **Steps 8 and 9 done** (2026-11-11, "proceed"): the live system matched `f98a61f0` but for CRLF; re-synced (`9c12fd44`,
  `31e8e9c5`: an "Activity types" group of four records, TabBar rewritten); review `design/flows/17-activity-type-builder/
  resync-review.md` 0 must, 1 should, 4 nits, fixed (`1f708307`, `edb4e5e2`); published as **version 52** (1794377277-5687;
  50 was the type's). BRIEF § 11 "Flow 17", six lessons (`a78003a2`). **T398 done.**
- **For review (new):** the two page faults in the system README's flow 17 known gaps, not filed (the pinned line's
  "as they filed it" with plural fields; the builder's empty result row, inferred).
- **Next:** flow 18 on the operator's word, or a P2 (T320 the largest).
- **Environment:** the replay app on :5180 (`wombat_scenario_t398`, master `277e4a78`) left running; scratch DBs
  `_sweep`, `_s10`, `_s11`, `_s20`, `_s515` kept; dumps `recovery/scenario-t398-post-act{1..6,A}`; worktrees `t398*` kept.
- **Pushed** 2026-11-10 (`399a99be`); everything since (the replay, steps 8–9) is not pushed. Ask before pushing.

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
