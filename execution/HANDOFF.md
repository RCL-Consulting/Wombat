# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-10-14 (Opus): the reset checked; T366 (flow 09) started

The machine reset overnight; nothing was in flight (tree clean, T364 closed at `c368a54f`). The operator said "tie up
the loose ends": pushed through `adfa3700`, the replay app restarted. Then "lets go": **T366, flow 09** (outcomes,
STARs and appeals), filed and started.

- **Step 1** (`0058adf3`): the brief restated in flow 08's layout from two read-only audits at `adfa3700`. T308 and
  T319 are unbuilt; the revoke's "The trainee is notified." is false (nothing mails). Eleven questions, five the
  operator's (§ 6); 153 stale step pointers in flows 01–07 re-synced. Completeness 0, verbatim 0 problems, missing 0.
- **Steps 2–4** (`47f2850d`): staged (106 files; four long captures cropped to the Appeals section in the staged copy);
  canvas **"Wombat 09"** https://claude.ai/artifact/5LfLPdcXJ2iRX6GSm2bbKP on design system v20, mark copied in.
- **Round 1** (`58f8dc87`, assessed `71e02aa2`): **B** picked with A's inline resolve; the operator took every
  recommendation (T366 Notes). **T367** (T319's revocation half) filed, to land before the build; § 3F question 13 added.
- **Round 2** (`1221c005`) reviewed from three sides (`4da096d1`); the operator said "yes" to F1–F9 (T366 Notes).
- **Round 3** sent by the operator. Pushed through `01b4c1ed` (2026-10-14); later commits not pushed.
- **Later (2026-10-14):** round 3's previews were bland: "Wombat 09" had no tokens stylesheet (fixed, canvas v18).
  **Round 3 checked, the design final** (`4c0912ac`, `13057800`). **T367 built** (`e9b29081`, six suites green, one
  review clean); it stays in progress until flow 09's replay sees step 4.36's mail. **Build started** (step 6): branch
  `t366`, wave 1 lanes T and S running (T366 Notes). **`a7f790b3` pins Pkcs 10.0.13** (two advisories of 10-13): the deploy carries it.

## Sessions 2026-10-11 to 10-13 (Opus): harness 3.4; 111 done tasks archived; T361 (flow 07) and T364 (flow 08) done

The operator said "upgrade the execution workspace", "move the done tasks" and "proceed". **The repo is `C:\dev\Wombat`,
on GitHub (`origin`, RCL-Consulting/Wombat, public). Commit locally; push when the operator asks.** The harness is
`C:\dev\rcl_execution\bin\harness.py` (the hooks in `.claude/settings.local.json`).

### T361 done (2026-10-12): flow 07 built, replayed, in the design system (version 19)

- **Build** `44de180b` (D53 with it), three Sonnet reviews, six suites green; pushed 2026-10-12; branches `t361*` kept.
- **Replay** (`wombat_scenario_t361`, one agent per act): 331 steps, 267 with no gap; five flow-07 faults found and
  fixed in the session, each re-checked in the browser: the return link's `#snap-n` (`59bb72fd`), the foot form's
  Issued on (`6ad6c57a`), 21 px snapshot links at 390 (`6b149fff`), the stacked caption and a pick line's space
  (`14378856`). States: 82 of 83 hold (`cdd12d9a`), the 83rd fixed. All other gaps are open tasks the act files cite.
- **Closed by flow 07:** T356 (the shell's stacking), T273 (the "no longer in use" mark). Still open with their review
  halves confirmed fixed: T309 (item 2 done), T329, T260, T324 (the form's half).
- **Design system** re-synced and published as **version 19** (1791816174-d002). **Lessons** in BRIEF § 11 "Flow 07".
- **Proposed:** T363 (the page's four data gaps). T362 (live following) still waits. **T290** stays in progress.

### T364 done (2026-10-12 to 10-13): flow 08 built, replayed, in the design system (version 20)

- **Build** `622c5ad1` (six suites green), pushed with `74ffe579`. **Replay** on `wombat_scenario_t364`: acts 1–6 and
  the appendix (`b4dc3cd1`..`6a1daf0d`), one flow 08 fault fixed and re-measured (19 px sentence links, `1ec7a352`);
  states 56 of 59 hold, 3 rows corrected (`88f545f9`); missing 0. **Closed** T309, T259; flow 08 halves noted on T260,
  T264, T272, T280, T324, T329, T338 (`a21f890e`). **Proposed** T365 (an S1 agenda's "Due by year end", older).
- **Design system** re-synced (`84e22b0e`, one Sonnet review, no must-fix) and published as **version 20**
  (1791917863-8f48); the live v19 matched HEAD first. **Lessons** in BRIEF § 11 "Flow 08".
- **Pushed** to origin 2026-10-14 (through `c368a54f`). **Next in the redesign:** flow 09, when the operator says.

### For the operator

- **Decide:** whether to rewrite history to remove the contributor (deferred).
- **T157 (deploy), T128 (backup), the College questions:** unchanged. The deploy now carries flows 02–07.
- **Tasks flow 05's build closed in part** (still in `queued/`): T306 (Home's half), T346, T323 (the chart), T328.

### Environment left

- **The replay app on `:5180`** runs `wombat_scenario_t364` (flow 08's appendix end state), published from `1ec7a352`; restarted 2026-10-14 after the machine reset.
  Dumps `recovery/scenario-t364-post-act{1..6,A}`, `scenario-t364-pre-a515`. Scratch DBs `wombat_scenario_t364_s515`,
  `_st1`..`_st6` (states), and `wombat_scenario_t361` (flow 07's), can be dropped on the operator's word.
- **Replay scratch:** this session's scratchpad `g364` (`replay-pw.env`; act, states scripts; `prompt-*.txt`).
  Khumalo's and du Plessis's pre-appendix passwords are the `_A` keys.
- **Databases:** 77 old `wombat_scenario_*` replay and scratch databases dropped 2026-10-12 (the operator's word);
  every earlier flow's act dumps stay in `recovery/`. Left: the T364 and T361 ones above, `wombat_scenario`, `_pdf`, and the
  non-scenario ones (`wombat`, `wombat_snapshot_pre_t130_*`, `wombat_t002_verify`, `wombat_t159`, `ClinicAssist*`).
- **Branches:** `t358`, `t358-*` (merged, squashed into `85d5a508`), `t355*`, `t304`, `t350*`, `t342*` kept locally;
  worktrees removed. The orphaned `verify-master` worktree folder (pointing at the pre-move repo path) was deleted 2026-10-06.

### Traps

- **A design-system publish:** `list` the files and `read` the artifact itself first (a typed artifact refuses a
  publish otherwise); `file_path` = the index, the rest in `files`, `root` = a folder holding `project/…`, one call.
  A new canvas needs its tokens stylesheet written by hand (the system serves none): Wombat 09 lacked it until v18.
- **A script must pass its base address explicitly** (a helper reading `BASE` at import wrote to the replay database).
- **A killed background wait** (memory pressure): stop and report, never restart it unasked.
- **Check new tables at 768 px** as well as 390 and 1280.
- **Copied replay scripts carry stale paths, ids and date helpers;** compute every date from D.
- **Paths with `\0…` in a Python string** become a NUL byte; task titles over ~150 characters break Windows paths.
