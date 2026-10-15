# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

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
- Not pushed: the D55 commit.

## Session 2026-10-14 (Opus): T366 (flow 09) done, with T367

The machine reset overnight; nothing was in flight. The operator said "tie up the loose ends", then "lets go",
"proceed" and "push": **T366, flow 09** (outcomes, STARs and appeals) ran the whole loop in the day, and is done.

- **Design:** brief (`0058adf3`), canvas "Wombat 09" (https://claude.ai/artifact/5LfLPdcXJ2iRX6GSm2bbKP), rounds 1–3,
  the operator's F1–F9; round 3 checked final (`13057800`). The canvas had no tokens stylesheet (bland previews):
  written by hand (canvas v18); the memory note and the traps say so now.
- **T367 done** (`e9b29081`, `5c183bb9`): `AccountNotices` (the one-off notice path T319's data-rights half reuses),
  the revocation mail, `GetDecisionsForTraineeQuery` (Active, Revoked, Expired).
- **Build** (`49e7cf5e`): lanes T, S, then C; the runbook lane; three Sonnet reviews (`build-review.md`, no high); one
  fix pass. Six suites green: 815 / 3809 / 1023 / 52 / 3735 / 491. **`a7f790b3` pins Pkcs 10.0.13** (GHSA-846f-f764-76mx,
  GHSA-2fmv-rv2c-59x7, published 10-13; NuGetAudit failed a fresh restore): **the deploy (T157) carries it.**
- **Replay** (`wombat_scenario_t366`): acts 1–6 and the appendix, 331 steps, 273 with no gap, no flow 09 fault; every
  browser check passed; states 57 rows, 2 corrected (`851d398f`). **T368 proposed** (A.5.10, unconfirmed).
- **Design system:** re-synced (`e87e9627`), the View 44 px fix it found (`9ba80586`), review fixes (`63dd5d36`),
  published as **version 23** (1791997100-4a21; v22 put the index beside `project/`, fixed in the next call).
  **Lessons** in BRIEF § 11 "Flow 09" (`c9d33455`); the replay prompt now says the remit is every step.
- **Pushed** through `7b421079`; the replay, design-system and lessons commits after it are **not pushed**.

### Environment left

- **The replay app on `:5180`** runs `wombat_scenario_t366` (the appendix's end state), published from `49e7cf5e`.
  Dumps `recovery/scenario-t366-post-act{1..6,A}`, `-pre-a515-A`. **Scratch DBs to drop on the operator's word:**
  `wombat_scenario_t366_s515`, `_s515b`, `_st1`..`_st3`, and the older `wombat_scenario_t364*`, `wombat_scenario_t361`.
  Kept beside them: `wombat_scenario`, `_pdf`, and the non-scenario ones (`wombat`, `wombat_snapshot_pre_t130_*`, ...).
- **Branches** `t366`, `t366-*` (and older flows' `t3xx*`) kept locally; `t366*` worktrees under `.claude/worktrees/`.
- **Scratch:** this session's scratchpad `g366` (`replay-pw.env`; act and states scripts; `prompt-*.txt`).

## Sessions 2026-10-11 to 10-13 (Opus): harness 3.4; 111 done tasks archived; T361 (flow 07) and T364 (flow 08) done

The operator said "upgrade the execution workspace", "move the done tasks" and "proceed". **The repo is `C:\dev\Wombat`,
on GitHub (`origin`, RCL-Consulting/Wombat, public). Commit locally; push when the operator asks.** The harness is
`C:\dev\rcl_execution\bin\harness.py` (the hooks in `.claude/settings.local.json`).

### For the operator

- **Decide:** whether to rewrite history to remove the contributor (deferred).
- **T157 (deploy), T128 (backup), the College questions:** unchanged. The deploy now carries flows 02–09 and the Pkcs pin.
- **Tasks flow 05's build closed in part** (still in `queued/`): T306 (Home's half), T346, T323 (the chart), T328.

### Traps

- **A design-system publish:** `list` the files and `read` the artifact itself first (a typed artifact refuses a
  publish otherwise); `file_path` = the index, the rest in `files`, `root` = a folder holding `project/…`, one call.
  A new canvas needs its tokens stylesheet written by hand (the system serves none): Wombat 09 lacked it until v18.
- **A script must pass its base address explicitly** (a helper reading `BASE` at import wrote to the replay database).
- **A killed background wait** (memory pressure): stop and report, never restart it unasked.
- **Check new tables at 768 px** as well as 390 and 1280.
- **Copied replay scripts carry stale paths, ids and date helpers;** compute every date from D.
- **Paths with `\0…` in a Python string** become a NUL byte; task titles over ~150 characters break Windows paths.
