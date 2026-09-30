# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-09-30 (Opus): flow 03 finished (T342 done)

The operator asked to recover the task a machine shutdown had killed (T342 step 7, the replay), then to proceed.

### Done

- **Recovery.** Acts 1–2 had finished on `wombat_scenario_t342`; act 3's agent died about 45 steps in, having written
  nothing. Post-act-2 was restored into `wombat_scenario_t342b` and acts 3–6 and A replayed there (D = 2026-09-30).
- **Step 7, the replay:** 325 steps, 254 with no gap (T339: 246), no flow 03 regression; commits `3ae97b4b`…`bf84372a`;
  dumps `recovery/scenario-t342-post-act{1..6,A}.dump`. **65 flow 03 states** re-taken (`design/baseline/states/`);
  `check_baseline_paths` missing 0.
- **Fixed from the replay and the states** (each with a mutation-checked bUnit test; Web 2,860 green):
  `daaf6386` About marks a paused EPA "(no longer in use)" (6.18); `5fd5abcd` a refused pre-programme date is not
  hinted twice; `a341b44e` the marker's missing space.
- **Step 8:** `design/system/` re-synced by one agent, reviewed by one Sonnet agent (2 must-fix, 4 nits, all applied);
  `copy.svg` uploaded; **published as version 9 (files) and 10 (the index, last)**; `b05f3862`. The bundle's lost
  `:root` brace (from `56628efb`) is closed again.
- **Step 9:** BRIEF § 11 "Flow 03" (six lessons); the replay brief now keeps every password per act and writes Actual
  lines per phase.
- **Filed:** T348 (a Log-type draft says "submit"), T349 (flow 03 polish: developer text, "Please", small mismatches),
  T350 (flow 04, P2, Opus). T342 moved to done.

### Decisions

- **3.10 kept as the default** (the operator said "proceed" after the recommendation; recorded in STATE's adopted
  defaults, may be overruled): the refusal summary keeps a corrected date's line until the next submit.
- **CLAUDE.md § Multi-agent workflows** (the operator's edit, uncommitted, left for them): followed from step 8 on.

### Flow 04 started (T350, same session)

- Brief restated (`30c29c31`), canvas https://claude.ai/artifact/8JnYLZp6CTR1Mg38a5v7DX with the mark; round 1 (A picked,
  Q7: no year minimum for the rater), round 2 (four-sided review, C1–C13, E1–E5), round 3 checked: **the design is
  final** (`design/flows/04-assessor-inbox/`). The review's 15 build notes are in `round-2-review.md`.

### Next session: T350 step 6, the build. **Model: Opus.**

- Build from `round-3/` in waves (flow 03's `build-lanes.md` as template), with `round-2-review.md`'s build notes;
  integrate, review, one fix pass, squash; DESIGN.md amended (R1–R4). Then replay, re-sync, lessons (steps 7–9).
- Or T157 (deploy) if the operator is ready: it now carries flows 02 and 03, neither with a migration.

### For the operator

- **P2s before real users (16):** T288, T289, T304, T305, T311–T313, T315, T316, T319, T320, T329, T333, T334, T341, T350.
- **T157 (deploy), T128 (backup), the College questions:** unchanged.
- **Your CLAUDE.md edit** (Multi-agent workflows) is uncommitted in the worktree.

### Environment left

- **No replay app runs.** `.scenario-app/bin` is published from `a341b44e`. The final state is `wombat_scenario_t342b`
  (restart: `tools/scenario-replay.ps1 start wombat_scenario_t342b 5180`).
- **Droppable databases:** `wombat_scenario_t342` (the half-played one), `wombat_scenario_t342_states_518{4,6,8}*` (11),
  and the older `wombat_scenario_t339*`, `_t335*`, `_states*`, `_rc*`, `_t292`.
- **Replay scratch** (passwords in `replay-pw.env`, untracked): the old session's scratchpad `…/6961d967-…/scratchpad/g342`.
- **The dev app on `:5080`** still runs a build from before 2026-09-26 and locks the Web project's Release output: run
  suites from `.claude/worktrees/verify-master` (detached; `git checkout --detach master` first).
- **Branches/worktrees:** the `t342*` lanes (squashed into `725237ee`) can be removed with their worktrees.

### Traps

- **A design-system publish:** upload first, then `read` the artifact itself (no path), read each file to change, send
  the files in one publish with `root` = a folder holding `project/…`, then read the index again and send it last.
- **Replays keep capture names**; still run `check_baseline_paths.py` and open the cited images.
- **Republish before the states capture**, so fixes made during the replay are in the baseline.
- **Task titles** over ~150 characters break Windows paths. **Two test helpers** named `StyleSheet.cs` and
  `Stylesheet.cs` are one file on Windows.
- **Earlier traps hold:** the audit pipeline commits a failed handler's staged rows (T201); merges are squashed per task.
