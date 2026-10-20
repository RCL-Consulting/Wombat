# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

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
  saved and checked final (`round-3-check.md`). **Next:** step 6, the build (flow 10's build lanes as template).

### Environment left

- **The replay app on `:5180`** runs `wombat_scenario_t369` (the appendix's end state), republished from `e4b480e2`.
  Dumps `recovery/scenario-t369-post-act{1..6,A}`, `-pre-a515-A`. Its live DB holds 2 extra failed adoption audit rows.
- **Scratch DBs dropped** (10-20, the operator's word): T369's `_st0`..`_st4`, `_s515`, every `t366*`, `t364*`, `t361`. Kept:
  `wombat_scenario_t369` (the replay app), `_t290`, `wombat_scenario`, `_pdf`, and the non-scenario ones.
- **Scratch:** this session's scratchpad `g369` (`replay-pw.env`, the act, states and step 8 scripts, prompts).

## Session 2026-10-15 (Opus): pushed; T290 closed

The operator asked "what is next", then "push then close T290". Pushed `7b421079..d42e11b6`. **T290 done** (commit below):
every remaining item, with tests; the task file's closing note lists them.

- **D54 (the operator):** withdrawal takes the Trainee role away, as completion does. CLAUDE.md § Roles and the runbook
  follow; **Steps 5.27, 5.28, 6.41, A.1.6 and A.4.6 have new Expects whose Actuals are T366's: the next replay checks them.**
- Suites: Domain 815, Application 3820, Infrastructure 1023, Architecture 52, Web 3737, Integration 491; all green.
- The disconnected-circuit fix (`CircuitConnection`) is unit-tested only; a replay's log should show no
  "Navigation failed" after a role change near a sign-out.
- Pushed (`f3471780`). Then "deal with D54": **D55 (the operator, two answers):** only a registrar whose programme is
  running files. `/activities/new` admits only Trainee; `CreateDraftAsync` and a draft's filing move refuse a subject
  with no active profile (`ActivityFilingRules`); PendingTrainee's menu is Home alone. T153's create half settled (note
  there; its MSF notes remain). One agent repaired ~90 fixtures (`NomineeSeed.AddCurrentProgramme`); suites green:
  Application 3823, Infrastructure 1023, Web 3740, Architecture 52, Integration 491. Runbook 2.19 and 5.22 rewritten.
- Pushed `fb6e465e`. Then **the runbook check** (one agent): Act 5 replayed on `wombat_scenario_t290` (from T366's
  post-act4; D = 10-15), **29/29 as expected**, gaps only known tasks (T311, T313, T287, T314) and 5.8's issue day (Act 4
  played a day earlier); snapshot `scenario-t290-post-act5`. On that state, without rewriting their Actuals: 6.41
  (bar KGK-001), A.1.6, A.4.5–A.4.6 and a 2.19 stand-in (a new pending trainee) all pass. The log: no "Navigation failed"
  after du Plessis lost Trainee 15–30 s after signing out. Two wording fixes after it (not replayed): Deactivate's
  dialog and result now say the role goes; My activities' empty state tells only a Trainee to log one.
- Environment: the replay app on `:5180` runs `wombat_scenario_t290` (old build: before the wording fixes).
  The 2.19 stand-in account (pending290@kgk.wombat.local) exists in that database only.
- Pushed through `cc80d5b3`. **T369 (flow 10) started:** canvas "Wombat 10" https://claude.ai/artifact/CPUXS577gX2yo79AryKmVE
  (v23 installed, `tokens.css` by hand, mark `/_blob/d5b5890c1eb0efaad13d0d347d1ebf4c`). Brief restated (`ded43219`); round 1 drawn, assessed (`b389a526`); the operator
  picked RA + MA and Q8–Q10 as recommended (Q7, Q11 taken as recommended). Round 2 (159 boards) reviewed from three
  sides (`5cc29ec7`); the operator took F1–F11 as recommended (`f79586b5`). Round 3 drawn (canvas 1792071007-7a95, 188
  boards, saved in `round-3/`); checked final (`e825e2e2`). **Built** (`68327c9d`, six suites green; T369's notes); **next:** step 7, the replay on a fresh database
  (flow 09's `replay-brief.md`), then the design system and lessons. Branches `t369*` and their worktrees kept.
- **Session finalised.** **Not pushed:** `6e842de3`, `f825fe60`, `68327c9d`, `d2d7f1dc` (push on the operator's word).
  The replay app on `:5180` still runs `wombat_scenario_t290` (pre-flow-10 build); step 7 starts from a fresh database.
  The replay's cautions are in T369's step-6 note (stub-mail counts +3, T282's log line, D1 margins at 1280/390).
