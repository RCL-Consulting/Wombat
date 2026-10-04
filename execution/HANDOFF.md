# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-10-03/04 (Opus): T350 finished; T304, T305 done; T355, flow 05, done

The operator asked to carry on with the GUI redesign. **The repo is now `C:\dev\Wombat`** (moved from
`C:\Users\Renier\Wombat`), and **it is disconnected from its remote: commit locally, never push.**

### Done

- **T350, flow 04, closed:** replay (325 steps, 249 no gap, no regression), states (34 hold), design system version 13
  (`4f855cd3`), BRIEF § 11 "Flow 04". T353, T354 filed (P3).
- **T304 and T305, done first** (the operator's order): `95d4e950`, `a27983fd`; verified by replaying acts 5 and 6 to
  6.38 on `wombat_scenario_t304` (`1b44bfaf`).
- **T355, flow 05 (a registrar reads where they stand):**
  - Steps 1–5: brief (`45caa7e8`), canvas https://claude.ai/artifact/VkYiJc32Tn8USxWMrftScj, round 1 (V1: an index and
    a page per EPA, with V2's Recent decisions; Q8 no menu change; Q9 rebuilt state only, T304 first; O1, O2), round 2
    reviewed from four sides (C1–C13, E1–E6 accepted), round 3 checked: final (`c1ac0fd5`).
  - Step 6, the build: `b020c942` (lanes A1, A2, B, C, D, runbook-and-docs; `build-review.md`: 1 high, 4 medium; one
    fix pass of 17). D1 ISO dates on screens, D2 the committee chart over the review window.
  - Step 7, the replay on `wombat_scenario_t355`: 325 steps, 256 no gap. Two regressions found and fixed in the browser
    (4.15's committee chart section, `db4c2d0a`, `5f8e6639`; A.7.3's phone chart at 322 px, `b389f7d2`). T306's Home
    half, T298, T346, T280 at 3.12, T328, T323 and F-6.37a/T304 no longer occur. T356 filed (P2: a scrolled table
    paints over the fixed top bar, flow 01's). States: 36 of 37 hold (December unreachable in October).
    Dumps `scenario-t355-post-act{1..6,A}`.
  - Step 8: the design system re-synced and published as **version 15** (files 14, the index last; `f2a22044`): six
    records in a new Progress group, `target.svg` uploaded; one Sonnet review. T357 filed (P3).
  - Step 9: BRIEF § 11 "Flow 05" (`29d013d2`). **T355 is done**; T358, flow 06, filed (P2).

### Decisions (the operator's)

- Resume T350's act 2 on its own database (2026-10-03); no push (2026-10-03).
- Flow 05: V1, Q8, Q9 (T304 first), O1, O2; E1–E6; D1, D2; T304 before the build (2026-10-04).
- CLAUDE.md § Multi-agent workflows (the operator's edit, still uncommitted, theirs to commit).

### Next: T358, flow 06 (programme oversight). **Model: Opus.**

- `harness.py task start T358`, then step 1: restate `design/flows/06-programme-oversight.md` as flow 05's was
  (`design/flows/05-trainee-progress/` is the latest model; BRIEF § 11 "Flow 05"). Stage with `stage_upload.ps1 -Flow 06`.
- Step 3 needs the operator: a new canvas from the main app's Design page on the design system **version 15**.
- **Tasks the build closed in part** (for the operator to close or trim): T306 (Home's half), T298 (Upcoming
  deadlines), T346, T323 (the chart), T328 (rating links) no longer occur; their files are still in `queued/`.

### For the operator

- **P2s before real users (15):** T288, T289, T311–T313, T315, T316, T319, T320, T329, T333, T334, T341, T356, T358.
- **T157 (deploy), T128 (backup), the College questions:** unchanged. The deploy now carries flows 02–05, T304 and T305.
- **The design system** is https://claude.ai/artifact/RsbreZ2d94q2NUNQMLch18 (version 15).

### Environment left

- **The replay app on `:5180`** runs `wombat_scenario_t355` (the appendix's end state), published from `b389f7d2`.
- **Replay scratch:** this session's scratchpad folder `g355` (`replay-pw.env`, act and states scripts); T350's in the
  earlier session's `g350`. Khumalo's and du Plessis's pre-appendix passwords are the `_A` keys.
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
