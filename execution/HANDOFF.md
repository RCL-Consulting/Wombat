# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-09-26 (Opus): the runbook as the GUI redesign's journey catalogue

The operator asked whether to brief Claude Design with an exhaustive workflow list, then: "Should we not update the
scenarios to exercise all the paths?", and "Nothing is live or in use, do it all".

### Done

- **The answer, researched:** Claude Design reads a design system (tokens, type, components) from a codebase, but does
  not enumerate an app's journeys; `/design-sync` is React-only. So the flow list is ours to supply.
- **T292:** `devadmin@wombat.local` (dev only) is a global Administrator, so agents can play Administrator steps.
- **T293:** the runbook is now `knowledge/scenario-paediatrics/`: README, Acts 1–6 (Act 6, catalogue maintenance, is
  new) and an appendix. It has 324 intent-and-outcome steps playing all 80 page templates, `coverage.md` (pages to
  steps, and every role's jobs as goals), and `states.md` (577 states and how to reach each). The old file is whole in
  `log/scenario-paediatrics-history-2026-09.md`.
- **T294:** `tests/Wombat.Web.Tests/Scenario/` fails the build if a page has no step, a step names a route that does not exist,
  or a step breaks the format.
- **T295:** replayed end to end on a fresh database (`wombat_scenario`): 324 of 324 steps played, and every outcome check
  matched. 109 more states were captured on scratch copies. The baseline is in `design/baseline/` (gitignored: 672
  step and 578 state screenshots). The tool is `tools/scenario-replay.ps1`.
- **Findings filed:** T297–T331 (15 P2, 20 P3), plus 46 notes on open tasks. Every Gap line cites its task.
- **T296:** CLAUDE.md and DOMAIN.md now say ten roles, and give the STAR glossary.
- Commits `0c1c110` … `0cf1f10`, pushed.

### For the operator

- **The Claude Design brief:**
  - `coverage.md` § Journeys by role is the flow inventory; brief one flow at a time.
  - `states.md` and `design/baseline/` are the visual baseline.
  - Link only `src/Wombat.Web/wwwroot` (tokens, fonts, icons), never the whole working tree: it holds
    `pwd_DO_NOT_COMMIT.txt` and `recovery/`.
  - Decide first whether this is a reskin or a restructure, because `DESIGN.md` freezes the structure.
- **P2s before real users:** T297, T302–T305, T307, T311–T313, T315, T316, T319, T320, T322 and T329, beside T288 and
  T289. The worst are T302 (an InstitutionalAdmin can deactivate her own institution), T303 (Add role makes a Trainee
  with no profile) and T320 (no activity or committee mail is ever sent).
- **T157 (deploy), T128 (backup), the College questions:** unchanged; see STATE.

### Environment left running

- **The replay app** on `:5180` (database `wombat_scenario`, the story's end state), so the replay can be browsed.
  Stop it with `tools/scenario-replay.ps1 stop 5180`, and restart it with `start wombat_scenario 5180`.
- **Scratch databases, droppable:** `wombat_scenario_states`, `wombat_scenario_states4` and `wombat_scenario_t292`.
  The script never drops a database.
- **Snapshots:** `recovery/scenario-post-act{1..6,A}.dump`. The passwords of the cast registered in the replay are in
  `pwd_DO_NOT_COMMIT.txt`.
- **The dev app on `:5080` and its database were not touched.** Port 25's temporary SMTP sink is stopped.

### Traps

- **Dev mail settings.** `appsettings.Development.json` sends mail to `localhost:25` and links to `:5080`. The replay
  tool overrides both; anything else run on another port needs the same overrides (`BaseUrl`, `MsfRespondUrl`,
  `Email:SmtpHost`).
- **Downloads.** The browser the agents drive has a download manager, which takes over downloads (saving them to
  `Downloads\Compressed`). Check a download from inside the page.
- **Quoting.** From bash, `powershell -File … sql` needs a `.sql` file, or the quotes of `"Table"` identifiers are
  lost.
- **Task titles.** The harness names the file after the title, so a title over about 150 characters breaks Windows'
  path limit.
- **Harness lint** warns that `done/` holds 205 tasks, over its 200 guide: move the oldest to `log/tasks/`.
- **Earlier traps still hold:** the audit pipeline commits a failed handler's staged rows (T201), and merge lanes are
  squashed per task.
