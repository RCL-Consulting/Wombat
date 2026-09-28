# T339 step F — rules common to every build lane (flow 02, sign-in and account)

Adapted from `design/pilot/build-lanes.md` (T335). You are one lane of the build of flow 02 of Wombat's GUI
restructure, task T339 (`execution/tasks/in_progress/T339-*.md`). The design is final: Claude Design canvas round 3.

## Read first (in your worktree)
- `design/flows/02-sign-in-and-account/README.md` — the design record.
- `design/flows/02-sign-in-and-account/round-3/*.dc.html` — the accepted boards. `R3-Spec.dc.html` is the contract:
  every message with its code, owner, words and kind; the focus rule; the six password rules; the DESIGN.md changes;
  the new parts. `R3-Steps.dc.html` lists the runbook Expects that change. Round 3 copied only changed boards: for a
  screen not in `round-3/`, read `round-2/` (same name with `R2-`). The boards load Google Fonts and inline hex: they
  are a picture, not code to copy. Read their text and inline styles for sizes, copy and colours.
- `design/flows/02-sign-in-and-account/round-2-review.md` — corrections C1–C9, decisions E1–E11 (all accepted), and
  "For the build": the route collision, T287 in the address, the toggle's four traps, the Remove endpoint's checks,
  the tests to change deliberately.
- `design/flows/02-sign-in-and-account/round-3-check.md` — § Carried into the build.
- `execution/architecture/DESIGN.md` — mandatory before touching any Razor or CSS. CLAUDE.md — conventions and
  footguns.

## Where you work
- Only in your own worktree directory (given in your prompt), on its branch. Use absolute paths for every command.
  Never touch `C:\Users\Renier\Wombat` itself (master), never merge, never push, never switch branch.
- Commit in your worktree as you go. Write each message to a file and use `git commit -F <file>`. End every message
  with the line `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
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
- Use the names on the boards' invented people (Tumi Moloi, Karabo Sithole) in tests or the runbook.

## How
- Test-first for behaviour: write or change the test, see it fail for the right reason, then build.
- Follow the surrounding code's idiom, naming and comment density (this codebase comments *why*, with task ids; cite
  T339 and the round-2 review's ids, e.g. "(T339, B1)").
- The audit pipeline commits a failed handler's staged rows (`AuditPipelineBehavior`): do every throwable check before
  any mutation.
- Change a test deliberately, never loosen it to pass. Keep the Design tests (tests/Wombat.Web.Tests/Design/*) truthful.
- Do not edit the runbook (`execution/knowledge/scenario-paediatrics/*.md`) or `design/flows/*.md`: the integrator
  does both after merging. T294's guard (tests/Wombat.Web.Tests/Scenario) must stay green; if a new route makes it
  fail, add the route to `coverage.md` § Pages only, and say so.
- Before finishing: `dotnet build Wombat.sln -c Release` clean (no new warnings), then all six suites, each with its own
  build: Domain, Application, Infrastructure, Architecture, Web, Integration
  (`dotnet test tests/<Project>/<Project>.csproj -c Release`). Quote the pass/fail/skip counts.

## Your report (your final message is returned to the integrator, not shown to a person)
- Branch and commit hashes; files changed (grouped).
- Each suite's counts.
- Decisions you made that the design left open, and why.
- What you did not do, and anything another lane or the integrator must know (contracts you rely on, files you touched
  outside your list, runbook steps whose Expect your change alters).
