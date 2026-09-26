---
id: T294
title: Nothing checks that every page is exercised by a scenario step, or that a step's route exists
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-26
started: 2026-09-26
completed: 2026-09-26
---

# T294 — Nothing checks that every page is exercised by a scenario step, or that a step's route exists

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Without a check, the runbook drifts the way it did before T293: 29 of 68 pages had no step, and
Acts 4–5 named routes that do not exist (`/admin/committee-reviews/new`, `/admin/jobs/{key}/run`). A page added later
would fall outside the journey catalogue the GUI redesign is briefed from, and nobody would notice.
**Surfaced:** 2026-09-26, while scoping T293.

## Symptom

The runbook is prose. Nothing ties its `Route:` lines to the pages the app serves.

## Root cause

No test reads the runbook.

## What to build

A test in `Wombat.Web.Tests` that reads `execution/knowledge/scenario-paediatrics/` and the app's routes:
- **Every page is played or excused.** Every `@page` template of a routable component appears in some step's `Route:`
  line (the act and appendix files), or in `coverage.md`'s `## Not played` table with a reason.
- **Every route a step names exists.** Every token in a `Route:` line is a component template or a mapped endpoint,
  spelled exactly.
- **No stale excuse.** A template in `## Not played` is a real template, and no step plays it.
- **coverage.md's `## Pages` table** names every template.
- **The step format holds.** Every `### Step` block carries `Role:`, `Route:`, `Do:`, `Expect:`, `Actual:` and `Gap:`,
  in that order.

## Verification

- [x] The test passes on the T293 runbook — `dotnet test tests/Wombat.Web.Tests` (Debug), 2026-09-26: 49/49 scenario
  tests, 324 steps, 80 of 80 templates played.
- [x] Each rule fails when broken: a step's route misspelt, a page's only step removed, a stale `Not played` row, a
  template missing from `## Pages`, a step without `Expect:` — kept as fixture tests in `ScenarioRulesTests`
  (`AMisspeltRoute_IsCaught_AndNamesItsStep`, `APageWhoseOnlyStepIsRemoved_IsCaught`,
  `AStaleNotPlayedRow_IsCaught_WhenAStepPlaysIt_OrNoPageDeclaresIt_OrItGivesNoReason`,
  `ATemplateMissingFromPages_IsCaught`, `AStepWithoutExpect_IsCaught_AndNamesItsStep`). Before T293's files existed,
  all six real-corpus tests failed, each on its guard.
- [x] Web suite green — 2,056/2,056.

## As built — 2026-09-26

`tests/Wombat.Web.Tests/Scenario/`:
- `ScenarioRunbook` parses the step files (`act-*.md`, `appendix*.md`): each step's id, fields in order and Route tokens,
  and coverage.md's `## Pages` and `## Not played` tables. The tokeniser is documented on `RouteTokens`: a token starts
  with `/` after whitespace, a comma, `→`, `(` or a backtick; braces are kept whole; a query or fragment is cut;
  comparison is exact, case included.
- `ScenarioRoutes`: the page templates by reflection over `RouteAttribute` (`PageAccess.Pages`, now shared), and the
  mapped endpoints by scanning `src/Wombat.Web` for `Map*` calls, reading constants (`SessionEnd.Path`,
  `RegisterOutcome.SubmitPath`, `ChangePasswordOutcome.SubmitPath`) by reflection. A host was not used: `Program`
  migrates PostgreSQL before `Run`, and the suite has no database. 11 endpoints today.
- `ScenarioRules` and `ScenarioRunbookTests`: the five rules, plus a sixth, `ThePagesTable_NamesNoPageThatDoesNotExist`.
  Each test runs guards first (step files present, at least 60 templates, a clean endpoint scan), so it cannot pass on
  nothing.
- `ScenarioRulesTests`: 42 fixture tests, including the five mutation checks and 12 tokeniser cases.
- Not checked: coverage.md's `## Endpoints` table and its `Steps` column, which are an index kept by hand.

## Related

T293 (the runbook), T295 (the replay), `tests/Wombat.Web.Tests/Navigation/PageAccess*.cs` (how the suite finds pages).
