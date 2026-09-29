# T342 step 6 — rules common to every build lane (flow 03, a registrar files an activity)

Adapted from `design/flows/02-sign-in-and-account/build-lanes.md` (T339). You are one lane of the build of flow 03 of
Wombat's GUI restructure, task T342 (`execution/tasks/in_progress/T342-*.md`). The design is final: Claude Design canvas
round 3.

## Read first (in your worktree)
- `design/flows/03-trainee-files-activity/README.md` — the design record.
- `design/flows/03-trainee-files-activity/round-3/project/` — the accepted boards. The substance is in
  `R3-C-Log.dc.html`, `R3-C-Activity.dc.html` and `R3-C-Mine.dc.html`: each is one component with a `state` prop. Read
  its markup and its `renderVals` script, which holds each state's data and words. The `R3-A-*`, `R3-L-*` and `R3-M-*`
  files only pick a state and a width.
  - `R3-Spec.dc.html` is the contract: every message by kind and its words, the DESIGN.md changes, the NEW components
    and their classes, the contrast figures.
  - `R3-Steps.dc.html` lists the runbook Expects that change.
  - `flow03.css` and `flow03-r3.css` are the canvas's proposal for the new classes. They are a picture, not code to
    copy: rebuild them in `app.css` as the round-2 review says.
- `design/flows/03-trainee-files-activity/round-2-review.md` — corrections C1–C15, decisions E1–E9 (all accepted), and
  **§ For the build**: your lane's items are there, with the reviewers' evidence (file:line).
- `design/flows/03-trainee-files-activity/round-3-check.md`.
- `execution/architecture/DESIGN.md` — mandatory before touching any Razor or CSS. CLAUDE.md — conventions and
  footguns, and the Activity platform section (the gates, `ActorRuleMatcher`, `FieldPermissionEvaluator`).

## Where you work
- Only in your own worktree directory (given in your prompt), on its branch. Use absolute paths for every command.
  - Never touch `C:\Users\Renier\Wombat` itself (master).
  - Never merge, push or switch branch.
- Commit in your worktree as you go.
  - Write each message to a file and use `git commit -F <file>`.
  - End every message with the line `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- Other lanes run in parallel on other branches. Stay inside the files your prompt gives you. If you must touch a file
  another lane owns, keep the change minimal and name it in your report.

## Never
- Touch the apps running on ports 5080 and 5180, or their databases, or start anything on those ports.
- Read `pwd_DO_NOT_COMMIT.txt`, any admin credential, `recovery/`, or user secrets' values. Integration tests read the
  connection themselves; each works in its own throwaway `it_<guid>` schema, which is expected.
- Pass `--no-build` to `dotnet test` or `dotnet ef`.
- Upgrade MediatR, add Bootstrap/MudBlazor/Radzen/jQuery, put raw hex colours outside `:root`, or use `<i class="bi">`.
- Load anything from a third-party host at runtime (the CSP allows only 'self'); no inline scripts or `onclick`.
- Name a test helper `StyleSheet.cs` (it is the same file as `Design/Stylesheet.cs` on Windows).
- Change a seed's field order or labels (E6: the College's seeds stay as they are).

## How
- Test-first for behaviour: write or change the test, see it fail for the right reason, then build.
- Follow the surrounding code's idiom, naming and comment density. This codebase comments *why*, with task ids: cite
  T342 and the round-2 review's ids, e.g. "(T342, B6)".
- The audit pipeline commits a failed handler's staged rows (`AuditPipelineBehavior`): do every throwable check before
  any mutation.
- Application handlers go through `IApplicationDbContext`, never EF types. Web components use `IScopedSender` and DTOs,
  never Domain types in `.razor`.
- Change a test deliberately, never loosen it to pass. Keep the Design tests (`tests/Wombat.Web.Tests/Design/*`)
  truthful.
- Do not edit the runbook (`execution/knowledge/scenario-paediatrics/*.md`) or `design/flows/*.md`: the integrator does
  both after merging. T294's guard (`tests/Wombat.Web.Tests/Scenario`) must stay green. If a new route makes it fail,
  add the route to `coverage.md` § Pages only, and say so.
- Before finishing:
  - `dotnet build Wombat.sln -c Release` clean, with no new warnings.
  - Then all six suites, each with its own build: Domain, Application, Infrastructure, Architecture, Web, Integration
    (`dotnet test tests/<Project>/<Project>.csproj -c Release`). Quote the pass/fail/skip counts.

## Your report (your final message is returned to the integrator, not shown to a person)
- Branch and commit hashes; files changed (grouped).
- Each suite's counts.
- Decisions you made that the design left open, and why.
- What you did not do, and anything another lane or the integrator must know: the contracts you provide or rely on
  (exact type, member and parameter names), files you touched outside your list, runbook steps whose Expect your change
  alters.
