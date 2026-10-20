# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-10-20 (Opus): T373, flow 11, step 7 (the replay)

The operator said "proceed" (push and start the replay). Pushed through `9d979834`. **Not pushed:** `c0e5d0cc` onwards.

- **Step 7, the replay** (fresh `wombat_scenario_t373`, D = 2026-10-20; one Sonnet agent an act; `replay-brief.md`
  `c0e5d0cc`): **no flow 11 regression in any act.** Acts 1-2 `42f171b5`, 3 `40525efc`, 4 `c1f03dd1`, 5 `783af7b5`,
  6 `d771aa91`, A `f67ade0c`; every gap a known task (T370 at 4.25/4.29/4.41/4.42/5.8). Act 3's 3.1-3.42 and
  3.50-3.54c Actuals were re-based from T369's, the rest played fresh. Runbook fixes `a70f030b` (2.29, Act 6 handoff).
  Dumps `recovery/scenario-t373-post-act{1..6,A}`, `-pre-a515-A`.
- **States pass**, two agents in turn (`g373/prompt-states-{1,2}.txt`): half 1 (Invitations, Invite a person, 39 rows)
  all hold, 5 rows' words corrected in `states.md` (uncommitted); **T375 proposed** (`11fd5203`: a described radio's
  hit area). **Half 2 (Register, Home, Trainees, Admit) is running.** Then: commit states.md, `check_baseline_paths.py`.
- **Next:** step 8 (re-sync the design system; read the live files first), step 9 (BRIEF § 11), `task done T373`.

### Environment left

- The replay app on `:5180` runs `wombat_scenario_t373` (the appendix's end state). Scratch DBs `wombat_scenario_t373_s1{0,1,2}`
  (and half 2's `_s2N`), `_s515`: drop on the operator's word. Old: `wombat_scenario_t369` and T369's dumps.
- Scratch: this session's `g373` (`replay-pw.env` must never be printed; act and states scripts).

## Session 2026-10-19 (Opus): T369, flow 10, done

The operator said "proceed". Pushed `e825e2e2..e598c490`, then (10-20, the operator's word) `..69a7df20`.

- **Step 7, the replay** (`d3379233` brief; fresh `wombat_scenario_t369`, published from `e598c490`; one agent an act):
  331 steps, 281 with no gap, **no flow 10 regression**; the rest known tasks, or the panel's member order at 4.25, 4.41,
  4.42, 5.8 (**T370 proposed**). Flow 10's checks pass, anonymity holds; T368 did not recur (5 of 5). States: 62 rows
  hold, 5 corrected; 3.43's stand-in SQL fixed. Results `3a91991e`, `652749d0`, `72aaf968`.
- **Step 8:** the live system matched HEAD; re-synced (`da19e284`), its three page faults fixed (`e4b480e2`: the campaign
  form's card gap, the respondent's legend, two "Please"s; Web 3837 green; browser-checked, two states re-taken), one
  Sonnet review (`be67b0a7`), icons uploaded; **design system version 25** (1792413020-e2bb, `1f639c60`).
- **Step 9:** BRIEF § 11 "Flow 10" (`324c6abf`); the replay brief keeps scratch files out of the repo (`a81b0b73`).
- **Proposed:** T370 (panel member order), T371 (dropped 10-20: the operator said no, order is not identifying), T372 (an empty band above the campaign page's status card).

- **Then "carry on": T373 (flow 11, onboarding) started.** Brief restated (`11a77b0c`), canvas "Wombat 11"
  https://claude.ai/artifact/SFn6dDmJnoww2fhYrQD416 (v25 installed, `tokens.css` by hand, mark
  `/_blob/b3cd1c9ecfc87765138aec8743e383bc`, `9f81d865`), staged in `design/upload/`. Rounds 1 and 2 drawn and
  reviewed; the operator took Q1–Q18 and F1–F10 as recommended (`f5ed1cc5`). Round 3 drawn (203 boards),
  checked final. **Built** (`69a17c70`; six suites green; T373's step-6 note has the replay's cautions). **Next:**
  step 7, the replay on a fresh database (flow 10's `replay-brief.md`), on the operator's word.

### Environment left

- **The replay app on `:5180`** runs `wombat_scenario_t369` (the appendix's end state), republished from `e4b480e2`.
  Dumps `recovery/scenario-t369-post-act{1..6,A}`, `-pre-a515-A`. Its live DB holds 2 extra failed adoption audit rows.
- **Scratch DBs dropped** (10-20, the operator's word): T369's `_st0`..`_st4`, `_s515`, every `t366*`, `t364*`, `t361`. Kept:
  `wombat_scenario_t369` (the replay app), `_t290`, `wombat_scenario`, `_pdf`, and the non-scenario ones.
- **Scratch:** this session's scratchpad `g369` (`replay-pw.env`, the act, states and step 8 scripts, prompts).
