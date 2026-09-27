# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-09-26 (Opus): groundwork for a Claude Design GUI redesign

The operator asked whether to brief Claude Design with a workflow list. The answer was yes, because Claude Design
builds a design system from code but not an app's journeys. Then: "do it all" (the runbook covering every path), and
"fix these defects first before designing?" → "yes, do both".

### Done

- **The runbook is now the journey catalogue.**
  - T292–T296: 325 steps playing all 80 page templates, `coverage.md`, `states.md` with 577 states, T294's guard, and a
    replay end to end.
  - The replay tool is `tools/scenario-replay.ps1`.
  - Findings are filed as T297–T331 and T333–T334.
- **The brief** is T332: `design/BRIEF.md`, 20 flow briefs, `design/research/` and `design/tools/`.
- **Group 1 fixed before designing:** T297, T300, T302, T303 and T307, each browser-verified on its act's replay.
- **2026-09-27: the pilot set up** (T335, in progress).
  - W-008: a restructure, with UX, emails and PDFs in scope.
  - W-009: Wombat's own design system (https://claude.ai/artifact/RsbreZ2d94q2NUNQMLch18, source in `design/system/`).
  - The mail and PDF baseline (T336), and `design/pilot/README.md` steps A–H.
- **Then, 2026-09-27: flow 01 designed and accepted, and step F started.**
  - The canvas's round 3 was accepted with D1–D11. It is recorded in `design/flows/01-shell/`, and the pilot's rules
    are W-010 (the GET role switch) and W-011 (OFL fonts).
  - D1 is built as a column on the account, carried as a claim. A cookie deleted at sign-out cannot remember a choice
    across sign-ins.
  - **Step F is in flight on branch `t335`.**
    - Four lanes run in worktrees `.claude/worktrees/t335-{tokens,role,reconnect,titles}`, each on its own branch.
    - Next: merge them into `t335`, then the shell lane and the Home/failure-pages lane, then a review, then one
      squashed T335 commit on master.
    - The shared rules for the lanes are in the job's tmp directory (`t335-common.md`), and the plan is
      `round-2-review.md` § Step F.

### For the operator

- **The pilot, next:** nothing is needed from you until step G. That step replays in the browser, and the Playwright
  MCP failed to connect this session; reconnect it (`/mcp`) before then.
- **The redesign after the pilot:** one `design/flows/NN-*.md` at a time, attaching its listed screenshots. They are
    in the gitignored `design/baseline/`, so they exist only on this machine.
  - Upload tracked files only (BRIEF § 3). Crop invitation captures that show a registration link.
  - A flow is done when its runbook steps replay; BRIEF § 9 says how.
- **P2s before real users (15):** T288, T289, T304, T305, T311–T313, T315, T316, T319, T320, T322, T329, T333, T334.
  The worst:
  - T320: no activity or committee mail is ever sent.
  - T333: SSO can grant Trainee round admission.
  - T334: the College can edit the system-managed MSF types.
- **Decisions adopted on recommendation,** which you may overrule: D51 (Upheld removed) and D52 (the College authors
  its disciplines' activity types). Open question on T290: a graduate has no way into a second programme.
- **T157 (deploy), T128 (backup), the College questions:** unchanged. The deploy now carries T307's migration.

### Environment left

- **The replay app on `:5180`** (`wombat_scenario`, the story's end state, fixed build). Stop it with
  `tools/scenario-replay.ps1 stop 5180`.
- **Droppable scratch databases:** `wombat_scenario_states*`, `_rc*` and `_t292`. The tool never drops a database.
- **Snapshots:** `recovery/scenario-post-act*.dump`. The cast's passwords are in `pwd_DO_NOT_COMMIT.txt`.
- **The dev app on `:5080`** runs the build from before today's fixes, against its own database. The next restart
  applies T307's migration there.

### Traps

- **Dev settings.** They send mail to `localhost:25` and links to `:5080`. `tools/scenario-replay.ps1 start` overrides
  both, plus `MsfRespondUrl`.
- **Downloads.** A download manager (IDM) takes downloads into `Downloads\Compressed`; check them in the page.
- **Quoting.** From bash, `… sql` needs a `.sql` file, or the quotes of `"Table"` identifiers are lost.
- **Task titles** over ~150 characters break Windows' path limit (the harness names the file after the title).
- **The flow briefs quote runbook steps.** After changing a step, run `design/tools/sync_verbatim_steps.py`, then
  `check_verbatim_steps.py`.
- **Earlier traps hold:** the audit pipeline commits a failed handler's staged rows (T201); merges are squashed per task.
  `browser_run_code_unsafe` with a filename echoes the file (and any password in it) into the transcript.
