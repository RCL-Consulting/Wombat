# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Sessions 2026-10-11 to 10-12 (Opus): the harness upgraded to 3.4; 111 done tasks archived; T361 (flow 07) done

The operator said "upgrade the execution workspace", "move the done tasks" and "proceed". **The repo is `C:\dev\Wombat`,
on GitHub (`origin`, RCL-Consulting/Wombat, public). Commit locally; push when the operator asks.** The harness is
`C:\dev\rcl_execution\bin\harness.py` (the hooks in `.claude/settings.local.json`).

### Done

- **Harness 3.4** (`c5c4d41d`): `upgrade --apply`, five conflicts settled (this repo's `## Verification` rule kept; the
  generic multi-agent block points at CLAUDE.md's own section); `execution/harness.json` records the merge base;
  the template's empty `execution/ref/` added. `lint` clean; `upgrade` says up to date.
- **Archive** (`bc9a6c4b`): the 50 undated tasks and every task completed by 2026-09-24 moved from `tasks/done/` to
  `log/tasks/` (226 to 115); four links followed. CLAUDE.md's books-rag bullet names the medical-education sources
  (ten Cate et al. 2024 on EPAs, the `medicine` category), per the operator.
- **T361 steps 1–2** (flow 07, the review sitting): the brief restated in flow 06's layout (`9a204a5c`), from two
  read-only audits (the review page's code at `bc9a6c4b`; the captures). Completeness exit 0, baseline paths missing 0,
  all 26 verbatim steps match. Staged with `stage_upload.ps1 -Flow 07` (88 files, 18.7 MB);
  `design/flows/07-review-sitting/round-1-ask.txt` is § 1 + § 8 as one paste. T361's Verification lines are now
  flow 07's own.

- **T361 steps 3–5** (2026-10-12): the canvas and the mark (`025da574`); round 1's 13 boards recorded (`7b00819a`):
  A (the agenda is the page), B (three steps), C (a workspace per EPA). The operator took every recommendation:
  **A**, with C's per-EPA content in an opened line and its evidence reader inside a staging line; the decisions and
  C1–C9 are `design/flows/07-review-sitting/round-2-ask.txt` and T361's Notes. **D53** (EPA-PROGRAMME § 3D, the
  operator's): no deferral; unstaged closing lines are recorded "Not decided at this sitting", overruling T131's O2.
  Live following for members proposed as **T362**.

### T361 done (2026-10-12): flow 07 built, replayed, in the design system (version 19)

- **Build** `44de180b` (D53 with it), reviewed by three Sonnet lenses; six suites green (814 / 3750 / 1022 / 52 / 3557 /
  489). **Pushed** to origin 2026-10-12. Branches `t361`, `t361-*` kept locally.
- **Replay** (`wombat_scenario_t361`, one agent per act): 331 steps, 267 with no gap; five flow-07 faults found and
  fixed in the session, each re-checked in the browser: the return link's `#snap-n` (`59bb72fd`), the foot form's
  Issued on (`6ad6c57a`), 21 px snapshot links at 390 (`6b149fff`), the stacked caption and a pick line's space
  (`14378856`). States: 82 of 83 hold (`cdd12d9a`), the 83rd fixed. All other gaps are open tasks the act files cite.
- **Closed by flow 07:** T356 (the shell's stacking), T273 (the "no longer in use" mark). Still open with their review
  halves confirmed fixed: T309 (item 2 done), T329, T260, T324 (the form's half).
- **Design system** re-synced (`design/system/`, one Sonnet review) and published as **version 19** (1791816174-d002);
  the live v18 matched HEAD first. **Lessons** in BRIEF § 11 "Flow 07".
- **Proposed:** T363 (the page's four data gaps). T362 (live following) still waits. **T290** stays in progress.
- **Next in the redesign:** flow 08, started (below).

### T364, flow 08 (panels, what is due, the schedule): steps 1–2 done, 2026-10-12

- The operator said "flow 08". T364 filed and started (`b610cabd`). Brief restated in flow 07's layout (`c6740da1`) from
  two read-only audits; all checkers clean; staged (`stage_upload.ps1 -Flow 08`, 87 files). `round-1-ask.txt` holds
  `/_blob/MARK` until the mark is copied. **Waits for the operator's new canvas link** (main app's Design page, design
  system v19); then copy the mark, send round 1, record it in `design/flows/08-committee-calendar/`.

### For the operator

- **Decide:** whether to rewrite history to remove the contributor (deferred).
- **T157 (deploy), T128 (backup), the College questions:** unchanged. The deploy now carries flows 02–07.
- **Tasks flow 05's build closed in part** (still in `queued/`): T306 (Home's half), T346, T323 (the chart), T328.

### Environment left

- **The replay app on `:5180`** runs `wombat_scenario_t361` (flow 07's appendix end state), published from `14378856`.
  Dumps `recovery/scenario-t361-post-act{1..6,A}`, `scenario-t361-pre-a515`.
- **Replay scratch:** this session's scratchpad `g361` (`replay-pw.env`; act and states scripts). Khumalo's and du
  Plessis's pre-appendix passwords are the `_A` keys.
- **Databases:** 77 old `wombat_scenario_*` replay and scratch databases dropped 2026-10-12 (the operator's word);
  every earlier flow's act dumps stay in `recovery/`. Left: `wombat_scenario_t361`, `wombat_scenario`, `_pdf`, and the
  non-scenario ones (`wombat`, `wombat_snapshot_pre_t130_*`, `wombat_t002_verify`, `wombat_t159`, `ClinicAssist*`).
- **Branches:** `t358`, `t358-*` (merged, squashed into `85d5a508`), `t355*`, `t304`, `t350*`, `t342*` kept locally;
  worktrees removed. The orphaned `verify-master` worktree folder (pointing at the pre-move repo path) was deleted 2026-10-06.

### Traps

- **A design-system publish:** `list` the files and `read` the artifact itself first (a typed artifact refuses a
  publish otherwise); `file_path` = the index, the rest in `files`, `root` = a folder holding `project/…`, one call.
- **A script must pass its base address explicitly** (a helper reading `BASE` at import wrote to the replay database).
- **A killed background wait** (memory pressure): stop and report, never restart it unasked.
- **Check new tables at 768 px** as well as 390 and 1280.
- **Copied replay scripts carry stale paths, ids and date helpers;** compute every date from D.
- **Paths with `\0…` in a Python string** become a NUL byte; task titles over ~150 characters break Windows paths.
