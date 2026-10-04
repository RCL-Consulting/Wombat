# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-10-03 (Opus): T350, flow 04, finished (replay, states, design system v13, lessons)

The operator asked to carry on with the GUI redesign. **The repo is now `C:\dev\Wombat`** (moved from
`C:\Users\Renier\Wombat`), and **it is disconnected from its remote: commit locally, never push.**

### Done

- **Step 7, the replay.** The previous session's act-2 agent died in Step 2.26's hour's wait, but `wombat_scenario_t350`
  stood exactly at the end of 2.25, so act 2 resumed there (the operator's choice; `90cd0112`, `60854d97`). Acts 3–6
  and A followed (`4b7a04d0`, `9bc09e15`, `8f634de9`, `7c30795d`, `d5f9dccd`). **325 steps, 249 with no gap, every
  other gap an open task, 0 regression, 0 new defect**; `check_baseline_paths` missing 0. Act 5's script slip filed an
  extra request (#24, cancelled), so later ids are one higher than T342's lines; harmless (the runbook keys by email).
  6.2 and 6.14's restart claims, skipped by the agent, were played by the integrator: both rows unchanged.
- **The states:** flow 04's 34 rows re-taken on scratch copies (`95ccb4b0`), all hold; 17 reused from the replay after
  checking each. One row reworded.
- **Step 8, the design system** re-synced and republished as **version 13** (files as 12, the index last; `4f855cd3`):
  4 components added (WaitingList, WayOn, OtherRoleLine, RatedLevelPicker), 19 revised, move labels recorded in
  sentence case, no icon added. One Sonnet review; its two must-fixes applied, one also in DESIGN.md (`Home.razor`
  places the other-role line for every role, not the committee Home only).
- **Step 9:** BRIEF § 11 "Flow 04" (seven lessons); the replay brief gained three (`930377cd`).
- **Filed:** T353 (P3, 3.30's ageing leaves a request filed before its encounter), T354 (P3, the rung picker's help is
  `small.page-subtitle`, not `p.field-help`), **T355 (P2, flow 05, the next flow)**. T350 is done.

### Decisions (the operator's)

- Resume act 2 from 2.26 on the same database rather than restore post-act1 (2026-10-03).
- The remote is disconnected: no push (2026-10-03).
- CLAUDE.md § Multi-agent workflows (the operator's edit, still uncommitted, theirs to commit).

### Then: T355, flow 05 (a registrar reads where they stand), started. **Model: Opus.**

- **Steps 1–2 done** (`45caa7e8`): the brief restated against `f913dda3` (nine questions; 8 and 9 are the operator's:
  My progress and My authorisations into the Trainee's menu group, and a "being rebuilt" notice after a version move);
  `round-1-ask.txt`; staged 90 files plus flow 03's trainee boards by hand (`design/upload/flow03-boards/`, gitignored).
  Flows 01–04's quoted runbook steps re-synced (77 stale after T350's replay).
- **Steps 3–4 done** (`578281c3`): canvas https://claude.ai/artifact/VkYiJc32Tn8USxWMrftScj (before round 1: no
  `project/canvas.json`, version `1791075615-bb97`); mark `/_blob/5e32abd7ee425f64c0db739c82470df3`, written into the
  ask. Record: `design/flows/05-trainee-progress/README.md`.
- **Round 1 waits for the operator:** paste `round-1-ask.txt` into the canvas, attaching `design/upload/` (with
  `flow03-boards/`). Once it has drawn (the version changes), save the boards to `round-1/` and review them as flow 04's
  `round-1-review.md` did.

### For the operator

- **P2s before real users (16):** T288, T289, T304, T305, T311–T313, T315, T316, T319, T320, T329, T333, T334, T341, T355.
- **T157 (deploy), T128 (backup), the College questions:** unchanged. The deploy now carries flows 02–04 (no migration).
- **The design system** is https://claude.ai/artifact/RsbreZ2d94q2NUNQMLch18 (version 13).

### Environment left

- **The replay app on `:5180`** runs `wombat_scenario_t350` (the appendix's end state), published from `5aad596e`. Stop
  it with `tools/scenario-replay.ps1 stop 5180` when no longer wanted.
- **Replay scratch:** `C:\Users\Renier\AppData\Local\Temp\claude\c--Users-Renier-Wombat\b4d7c8bc-…\scratchpad\g350`
  (`replay-pw.env`, the act and states scripts). Khumalo's and du Plessis's pre-appendix passwords are the `_A` keys.
- **Droppable databases:** `wombat_scenario_t350_s5184`, `_s5186`, `_s5186b`, `_s5186c`, `_s5188b`; `wombat_scenario_t342`,
  `_t342b`, `_t342_states_518{4,6,8}*`, and the older `_t339*`, `_t335*`, `_states*`, `_rc*`, `_t292`.
- **Branches:** `t350*` and `t342*` lanes kept locally (squashed). `verify-master` stays for test runs.

### Traps

- **No push.** The remote is disconnected.
- **A design-system publish:** read each published file and compare it with HEAD (a build commit may have edited the repo
  copy unpublished); send the files with `root` = a folder holding `project/…`; read the index again, send it last.
- **Copied replay scripts carry stale paths and date helpers;** compute every date from D.
- **Full-page captures from the top,** or the states capture cannot reuse them.
- **Task titles** over ~150 characters break Windows paths. **Two test helpers** named `StyleSheet.cs` and
  `Stylesheet.cs` are one file on Windows. The audit pipeline commits a failed handler's staged rows (T201).
