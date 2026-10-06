# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-10-04/05 (Opus): T358, flow 06, done end to end; T298 and T290's committee item; repo to GitHub

The operator said "proceed with the next task". **The repo is `C:\dev\Wombat`, now on GitHub,
`https://github.com/RCL-Consulting/Wombat.git` (public), `origin`; the old server stays as remote `rcl` (its SSH port is
closed). Commit locally; push when the operator asks.** The harness is `harness.py`; its path is in the hooks in `.claude/settings.local.json`.

### Done

- **T358, flow 06 (programme oversight), all nine steps** (`design/flows/06-programme-oversight/`):
  - Brief `2f29c158`; canvas https://claude.ai/artifact/97WQWEuzMYHcKngPnNdHkV; round 1 picked A ("two lists and a
    record"); round 2 reviewed from three sides (`round-2-review.md`, E1–E6 the operator's); round 3 checked
    (`round-3-check.md`).
  - Build `85d5a508`: lanes A0, A1, A2, B, C, D, runbook (`build-lanes.md`, `build-contracts.md`); three Sonnet reviews,
    one fix pass (`build-review.md`). Programme trainees, the staff registrar page, Waiting for assessors, Send a reminder
    (ActivityReminders + AdmittedOn, one migration), the four Homes, the menus (E2's flat limit), a not-found page lights
    nothing.
  - Replay on `wombat_scenario_t358`: 332 steps, 266 no gap, every other gap a filed task; A.7.8a's 768 px regression
    fixed (`94edf2b7`); states re-taken. Design system re-synced (`1fed3a9a`), published as **version 17**. Lessons in
    BRIEF § 11 "Flow 06". T358 closed (`6b9ae3ae`).
- **T298 done, T290's committee item landed** (`14dac868`), before the build (the operator's Q8).
- **Filed:** T359 (Reassign; carries the review's R1), T360 (P3 layout nits, the trainees form without the circuit),
  T361 (flow 07).
- **Repo housekeeping:** README rewritten for the rebuilt app; old Azure workflow removed; `.gitignore` covers dumps,
  env files and keys; the five old `codex/*` branches deleted (their PRs were already merged or closed); the
  credentials survey found only dev-only or rotated values in history, left as is (the operator: dev only, not in use).
  CLAUDE.md's Multi-agent workflows section committed. The contributor rewrite (fulasht) is deferred by the operator.

### Next: T361, flow 07 (the review sitting), step 1: the brief. Opus.

- Run the loop as flow 06's record shows. Step 3 needs the operator's new canvas on design system version 17.
- **T290 stays in progress** for its other items only (the harness cannot requeue a task).

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
