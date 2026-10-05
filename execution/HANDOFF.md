# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-10-04 (Opus): T358, flow 06, started; step 1 (the brief) done

The operator said "proceed with the next task". **The repo is `C:\dev\Wombat`, disconnected from its remote: commit
locally, never push.** The harness is `python C:\dev\rcl_execution\bin\harness.py` (the repo holds no copy).

### Done

- **T358 started** (`task start`), the only task in progress.
- **Step 1, the brief restated:** `design/flows/06-programme-oversight.md` in flow 05's form, checked at `ef65c336`
  with file:line (`2f29c158`).
  - 20 steps (2.32 and 3.33 added); seven screens: the four Homes; Programme trainees, one registrar for staff and a
    list of what waits for a reviewer, all PROPOSED; Entrustment decisions in the speciality admins' menus.
  - Ten questions; **8–10 are the operator's**: T290 and T298 first as their own task (flow 05's T304 pattern) or
    folded in; triage built here or filed; what "inactive" counts.
  - Found at HEAD: T297's fixes hold; T290, T298 still open; slashes ("semester 0/10", "5 active / 0 inactive")
    against "n of m"; one component with two titles; "26 Sept" and an invitation's role as its key; the stall capped
    at 10 unsaid; nobody may change a request's named assessor (the registrar can only cancel and file again).
  - `design/flows/06-programme-oversight/round-1-ask.txt` is § 1 + § 8 as one paste; BRIEF § 8's row updated.
  - `check_flow_completeness`, `check_verbatim_steps` and `check_baseline_paths` exit 0.
- **Step 2, staged:** `stage_upload.ps1 -Flow 06`, 88 files (2.98 MB); flow 05's boards R3-E-typical, R3-P-standing,
  R3-T-committee (both widths) and `flow05-r3.css` copied by hand into `design/upload/flow05-boards/`.

### Next: T358 step 5, round 2. **The operator sends it.**

- Round 1 (`a1471073`): three structures. **The operator picked A** (two lists and a record) and answered Q8–Q10 as
  recommended: T290 and T298 land first as their own task, before the build; Send a reminder is built in flow 06,
  Reassign filed (**T359**, P3); no "inactive" count anywhere.
- Round 1's canvas said the screenshots and boards never reached it. `design/upload/round-2-ask.txt` (decisions, C1–C11,
  the states) goes with the 11 screenshots and `design/upload/flow05-boards/` AGAIN, as one message.
- Then read the boards into `round-2/`, review them four-sided (one reviewer on the code, one on the cast; Sonnet),
  put the decisions to the operator, one correction round (step 5's end). **Before step 6: T290 and T298.** Opus.

### For the operator

- **P2s before real users (15):** T288, T289, T311–T313, T315, T316, T319, T320, T329, T333, T334, T341, T356, T358.
- **T157 (deploy), T128 (backup), the College questions:** unchanged. The deploy carries flows 02–05, T304 and T305.
- **Tasks flow 05's build closed in part** (still in `queued/`): T306 (Home's half), T298 (Upcoming deadlines; its tie
  order is flow 06's), T346, T323 (the chart), T328 (its dashboard parts too).
- CLAUDE.md § Multi-agent workflows: the operator's edit, still uncommitted, theirs to commit.

### Environment left

- **The replay app on `:5180`** runs `wombat_scenario_t355` (the appendix's end state), published from `b389f7d2`.
- **Replay scratch:** T355's scratchpad folder `g355` (`replay-pw.env`, act and states scripts). Khumalo's and du
  Plessis's pre-appendix passwords are the `_A` keys.
- **Droppable databases:** `wombat_scenario_t355_s518{4,6,6b,8,8b}`, `_t304`, `_t350` (dumps kept), `_t350_s518*`,
  `_t342*`, `_t339*`, `_t335*`, `_states*`, `_rc*`, `_t292`.
- **Branches:** `t355*`, `t304`, `t350*`, `t342*` kept locally (squashed); worktrees removed. `verify-master` stays.

### Traps

- **No push.** The remote is disconnected.
- **A design-system publish:** read each published file and compare it with HEAD; send the files with `root` = a folder
  holding `project/…`; read the index again, send it last.
- **Copied replay scripts carry stale paths, ids and date helpers;** compute every date from D.
- **Full-page captures from the top;** a page taller than ~16,384 px must be captured in parts.
- **Paths with `\0…` in a Python string** become a NUL byte: write such files with the Write tool, not a heredoc.
- **Task titles** over ~150 characters break Windows paths. The audit pipeline commits a failed handler's staged rows.
