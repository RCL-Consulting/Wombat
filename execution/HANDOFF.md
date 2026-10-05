# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-10-04 (Opus): T358, flow 06, started; step 1 (the brief) done

The operator said "proceed with the next task". **The repo is `C:\dev\Wombat`, reconnected to its remote (2026-10-05, the operator): commit locally; push when the operator asks.** The harness is `python C:\dev\rcl_execution\bin\harness.py` (the repo holds no copy).

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

### Next: T290 and T298 (the operator's Q8: before flow 06's build), then T358 step 6, the build. Opus.

- **Flow 06's design is final** (`round-3-check.md`): A, two lists (Programme trainees, Waiting for assessors), a staff
  registrar page, Send a reminder; E1–E6 in `round-2-review.md`. The build carries the check's six items (no pronouns,
  the ended page from `EndedItemCard`, steps A.5.14/A.5.16/4.3/4.4, r8's trail, no inline styles, board detail).
- **First T290 and T298**, each reviewed against its own risk: the committee card reads the institution's current
  registrars, ties by surname then first name. T290's file holds more than the committee item: land that item, say so.
- **The build** (step 6, `design/flows/05-trainee-progress/build-lanes.md` as template): a new table and migration for
  the reminder (review items 1–3), an Application-side recipient policy, a role-explicit waiting query, new progress
  readers, `EpaProgressTable` names-as-text, `EndedView` extracted, the nav limit (E2) and DESIGN.md.

### For the operator

- **P2s before real users (15):** T288, T289, T311–T313, T315, T316, T319, T320, T329, T333, T334, T341, T356, T358.
- **T157 (deploy), T128 (backup), the College questions:** unchanged. The deploy carries flows 02–05, T304 and T305.
- **Tasks flow 05's build closed in part** (still in `queued/`): T306 (Home's half), T298 (Upcoming deadlines; its tie
  order is flow 06's), T346, T323 (the chart), T328 (its dashboard parts too).

### Environment left

- **The replay app on `:5180`** runs `wombat_scenario_t355` (the appendix's end state), published from `b389f7d2`.
- **Replay scratch:** T355's scratchpad folder `g355` (`replay-pw.env`, act and states scripts). Khumalo's and du
  Plessis's pre-appendix passwords are the `_A` keys.
- **Droppable databases:** `wombat_scenario_t355_s518{4,6,6b,8,8b}`, `_t304`, `_t350` (dumps kept), `_t350_s518*`,
  `_t342*`, `_t339*`, `_t335*`, `_states*`, `_rc*`, `_t292`.
- **Branches:** `t355*`, `t304`, `t350*`, `t342*` kept locally (squashed); worktrees removed. `verify-master` stays.

### Traps

- **A design-system publish:** read each published file and compare it with HEAD; send the files with `root` = a folder
  holding `project/…`; read the index again, send it last.
- **Copied replay scripts carry stale paths, ids and date helpers;** compute every date from D.
- **Full-page captures from the top;** a page taller than ~16,384 px must be captured in parts.
- **Paths with `\0…` in a Python string** become a NUL byte: write such files with the Write tool, not a heredoc.
- **Task titles** over ~150 characters break Windows paths. The audit pipeline commits a failed handler's staged rows.
