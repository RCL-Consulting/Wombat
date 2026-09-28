<!-- Kept from T335 (the flow 01 pilot) as the template for a later flow's build lanes. Replace "T335", "flow 01", the
     boards and the review file with the new flow's; point the worktree and scratch paths at the new session's. -->

# T335 step F — rules common to every build lane

You are one lane of the build of flow 01 (the shell) of Wombat's GUI restructure, pilot task T335
(`execution/tasks/in_progress/T335-*.md`). The design is accepted: Claude Design canvas round 3.

## Read first (in your worktree)
- `design/flows/01-shell/README.md` — the design record, the pick, D1–D11, round 3's acceptance.
- `design/flows/01-shell/round-3/*.dc.html` — the accepted boards. They are HTML mockups written by the design tool;
  they load Google Fonts (Wombat's CSP forbids that), so they are a picture of the design, not code to copy. Read their
  text and inline styles for sizes, copy and colours. `R2-Rules.dc.html` holds the shell's rules; `R2-Tokens.dc.html`
  the token sheet, contrast table, type, spacing, radii, shadows and motion.
- `design/flows/01-shell/round-2-review.md` — the review: should-fix S1–S22, decisions D1–D11 (all accepted), nits
  N1–N7, and "Step F build plan (condensed)" at the end (its commit list, DESIGN.md sections, tests, risks).
- `execution/DECISIONS.md` W-008 (restructure; DESIGN.md's structural lock is lifted for this work — amend it
  deliberately), W-010 (the GET role switch), W-011 (OFL fonts; dark mode later).
- `execution/architecture/DESIGN.md` — mandatory before touching any Razor or CSS.

## Where you work
- Only in your own worktree directory (given in your prompt), on its branch. Use absolute paths or `cd` into it for
  every command. Never touch `C:\Users\Renier\Wombat` itself (master), never merge, never push, never switch branch.
- Commit in your worktree as you go (small commits are fine; the integrator squashes). Write each message to a file and
  use `git commit -F <file>`. End every message with the line
  `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- Other lanes run in parallel on other branches. Stay inside the files your prompt gives you. If you must touch a file
  another lane owns, keep the change minimal and name it in your report.

## Never
- Touch the apps running on ports 5080 and 5180, or their databases, or start anything on those ports.
- Read `pwd_DO_NOT_COMMIT.txt`, the admin credential, `recovery/`, or user secrets' values. Integration tests read the
  connection themselves; each works in its own throwaway `it_<guid>` schema, which is expected.
- Pass `--no-build` to `dotnet test` or `dotnet ef` (CLAUDE.md explains why).
- Upgrade MediatR, add Bootstrap/MudBlazor/Radzen/jQuery, put raw hex colours outside `:root`, or use `<i class="bi">`.
- Load anything from a third-party host at runtime (the CSP allows only 'self' for scripts, styles, fonts).

## How
- Test-first for behaviour: write or change the test, see it fail for the right reason, then build.
- Follow the surrounding code's idiom, naming and comment density (this codebase comments *why*, with task ids).
- Amend `execution/architecture/DESIGN.md` for what you change, with a dated note "(2026-09-27, T335, flow 01)".
  Keep its Design tests (tests/Wombat.Web.Tests/Design/*) truthful: change a test deliberately, never loosen it to pass.
- Runbook (`execution/knowledge/scenario-paediatrics/*.md`): where your change alters what a step's **Expect** (or
  Route) says the screen shows, rewrite that line to the new truth in the same step format. Do not run
  `design/tools/sync_verbatim_steps.py` (the integrator runs it after merging). T294's guard
  (tests/Wombat.Web.Tests/Scenario) must stay green.
- Before finishing: `dotnet build Wombat.sln -c Release` clean (no new warnings), then all six suites, each with its own
  build: Domain, Application, Infrastructure, Architecture, Web, Integration
  (`dotnet test tests/<Project>/<Project>.csproj -c Release`). Quote the pass/fail/skip counts.

## Your report (your final message is returned to the integrator, not shown to a person)
- Branch and commit hashes; files changed (grouped).
- Each suite's counts.
- Decisions you made that the design left open, and why.
- What you did not do, and anything another lane or the integrator must know (tokens you rely on, files you touched
  outside your list, runbook steps whose Expect you changed).
