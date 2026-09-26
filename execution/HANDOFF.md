# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-09-26 (Opus): groundwork for a Claude Design GUI redesign

The operator asked whether to brief Claude Design with a workflow list. The answer was yes, because Claude Design
builds a design system from code but not an app's journeys. Then: "do it all" (the runbook covering every path), and
"fix these defects first before designing?" → "yes, do both".

### Done

- **The runbook as the journey catalogue.**
  - T292: `devadmin@wombat.local` is a dev-only Administrator.
  - T293: the runbook (`knowledge/scenario-paediatrics/`) now has 325 intent-and-outcome steps playing all 80 page
    templates. `coverage.md` indexes every role's jobs and `states.md` lists 577 states.
  - T294: a test holds the runbook to the routes.
  - T295: replayed end to end, and every state captured on scratch copies.
  - T296: docs corrected (ten roles).
  - Replay tool: `tools/scenario-replay.ps1`.
- **Findings, filed rather than fixed:** T297–T331, and T333–T334 from the fixes; 46+ notes on open tasks.
- **The brief (T332):**
  - `design/BRIEF.md`: how to run it, what to upload and never upload, the constraints, the requirements from the P3
    backlog, the screens that do not exist yet, and the reskin-or-restructure decision (it recommends restructuring the
    frame and reskinning the pages).
  - 18 paste-ready `design/flows/*.md`.
  - The research in `design/research/`, and the checks in `design/tools/`.
- **Group 1 fixed before designing,** because the baseline showed these screens wrong. Each was built test-first in a
  worktree, adversarially reviewed and fixed, squash-merged with all suites green (7,725 tests), and browser-verified on
  a replay of its act, with its screenshots refreshed:
  - T297: dashboards read what is waiting from the pinned workflow.
  - T300: the builder follows its guard and admits the College (D52).
  - T302: an institution's state belongs to the Administrator alone.
  - T303: Trainee is system-managed.
  - T307: appeals are Dismissed or Remitted (D51), with a migration.

### For the operator

- **The redesign:**
  - Decide reskin or restructure (BRIEF § 4).
  - Then run `/design` in this repo, one `design/flows/NN-*.md` at a time, attaching its listed screenshots. They are
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
- **Task titles.** Keep them under ~150 characters, or the harness's file name breaks Windows' path limit.
- **The flow briefs quote runbook steps.** After changing a step, run `design/tools/sync_verbatim_steps.py`, then
  `check_verbatim_steps.py`.
- **Passwords.** `browser_run_code_unsafe` with a filename echoes the file into the transcript.
- **Harness lint** warns that `done/` holds 211 tasks, over its 200 guide: move the oldest to `log/tasks/`.
- **Earlier traps hold:** the audit pipeline commits a failed handler's staged rows (T201), and merges are squashed per
  task.
