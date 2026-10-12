# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-10-11 (Opus): the harness upgraded to 3.4; 111 done tasks archived; T361 started

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

### Next: T361 step 5, round 3 (the correction round) in the canvas

- **Round 2** (`83ba4dc7`) reviewed from three sides: `design/flows/07-review-sitting/round-2-review.md` (`f1e193c8`).
  The operator took E1–E9 as recommended (2026-10-12). `round-3-ask.txt` carries them, corrections 7–31 by the
  review's numbers, and the fills. The operator pastes it as ONE message; then read the boards and check round 3 item
  by item (flow 06's `round-3-check.md`), then the build.
- **The build is not presentation only:** D53 changes the domain (the review's findings 1–6). Plan its review as one.
- **T290 stays in progress** for its other items only.

### For the operator

- **Decide:** drop the scratch databases below; whether to rewrite history to remove the contributor (deferred).
- **T157 (deploy), T128 (backup), the College questions:** unchanged. The deploy now carries flows 02–06.
- **Tasks flow 05's build closed in part** (still in `queued/`): T306 (Home's half), T346, T323 (the chart), T328.

### Environment left

- **The replay app on `:5180`** runs `wombat_scenario_t358_end` (flow 06's appendix end state), published from
  `94edf2b7`. Dumps `recovery/scenario-t358-post-act{1..6,A}`, `scenario-t358-pre-a515`.
- **Replay scratch:** this session's scratchpad `g358` (`replay-pw.env`; act and states scripts). Khumalo's and du
  Plessis's pre-appendix passwords are the `_A` keys.
- **Droppable databases:** `wombat_scenario_t358` (polluted by the states agent), `_t358_s515`, `_t358_states*`,
  `_t290`, `_t355`, `_t355_s518*`, `_t304`, `_t350*`, `_t342*`, `_t339*`, `_t335*`, `_states*`, `_rc*`, `_t292`.
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
