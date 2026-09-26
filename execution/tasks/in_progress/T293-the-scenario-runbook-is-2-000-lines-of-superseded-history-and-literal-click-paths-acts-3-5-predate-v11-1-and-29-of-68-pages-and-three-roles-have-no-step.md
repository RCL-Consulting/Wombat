---
id: T293
title: The scenario runbook is 2,000 lines of superseded history and literal click paths, Acts 3-5 predate v11.1, and 29 of 68 pages and three roles have no step
status: in_progress
priority: P2
owner: agent
depends_on: []
created: 2026-09-26
started: 2026-09-26
---

# T293 — The scenario runbook is 2,000 lines of superseded history and literal click paths, Acts 3–5 predate v11.1, and 29 of 68 pages and three roles have no step

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. The runbook is the test corpus and, from 2026-09-26, the flow inventory the GUI redesign is briefed
from (Claude Design reads a design system from code but does not enumerate an app's journeys). As it stands it cannot
be either.
**Surfaced:** 2026-09-26, a read-only inventory of `scenario-paediatrics.md` against the 68 routable pages, made while
answering whether to brief the redesign from a workflow list.

## Symptom

- **Coverage.** 39 of 68 routable page files are touched by a step; 29 are not (all six MSF pages, the account pages,
  every catalogue editor, the institutions list, audit detail, the progress rebuild, entrustment decisions, the staff
  data-rights queue, the activity inbox and My Activities, My authorisations, portfolio verify, decisions due, and the
  system pages). The cast has no SpecialityAdmin and no SubSpecialityAdmin, and the CollegeAdmin only reads. The
  graduate's read-only record (T252), the MSF campaign lifecycle, and the assessor's decline and return have no step.
- **Currency.** Only Acts 1–2 are current (T159). Acts 3–5 carry "not restated" banners, name the pre-v11.1 world, and
  several step routes are wrong (`/admin/committee-reviews/new`, `/admin/trainees/{id}` → Export, `/admin/jobs/{key}/run`).
  Step 3.9 is superseded and 3.12 targets a page that does not exist.
- **Form.** Four warning banners and months of superseded `Actual:`/`Gap:` history sit inline. Steps are literal click
  paths ("click `Save`"), so any redesign would break every one.

## Root cause

The runbook grew by accretion: each re-baseline (T091, T130, T102, T159) added a banner and kept the history inline, and
new features shipped with bUnit tests and browser checks but no scenario step.

## What to build

A folder, `execution/knowledge/scenario-paediatrics/`, replacing the single file:
- `README.md`: purpose, how to play, the step format, the cast (adding a SpecialityAdmin, a SubSpecialityAdmin, a real
  CollegeAdmin and MSF respondents), the world and the findings policy.
- One file per act: `act-1-setup.md`, `act-2-onboarding.md`, `act-3-operations.md`, `act-4-annual-review.md`,
  `act-5-graduation.md`, `act-6-catalogue.md` (new: the College and KGK maintain the catalogue), and
  `appendix-cross-cutting.md`.
- `coverage.md`: every routable page mapped to the steps that play it, the pages deliberately not played and why, and a
  role-by-job index of every journey (the flow inventory for the redesign brief).
- `states.md`: for each page, its states (loaded, empty, loading, error, validation, refusal, not found, confirmation,
  narrow viewport) and how to reach each from a named step's database state.

Steps are written as **intent and outcome**, not click paths: `Role`, `Route` (the page templates exactly as `@page`
declares them), `Do`, `Expect`, `Actual`, `Gap`. Expectations are what the product decided (EPA-PROGRAMME § 3 and the
task files' As-built sections) checked against the code.

The old file moves whole to `execution/log/scenario-paediatrics-history-2026-09.md`, so every recorded line survives.

## Verification

- [ ] Every one of the 68 routable page files is played by a step or listed as not played with a reason — T294's guard
  test.
- [ ] Every route a step names is a real page template or endpoint — T294's guard test.
- [ ] Every role (10, plus the anonymous visitor and the former trainee) has at least one journey in `coverage.md`'s index.
- [ ] Each act's expectations were checked against the code by an independent reviewer; corrections applied.
- [ ] References to the old path are updated (CLAUDE.md, knowledge/README.md, HANDOVER.md, EPA-PROGRAMME.md).
- [ ] The rewrite is replayed end to end — T295.

## Related

T159 (Acts 1–2 retarget), T291 (its replay's gaps), T292 (the dev Administrator), T294 (the coverage guard), T295 (the
replay and screenshot baseline), W-006, W-007.

## Notes

- Decided 2026-09-26 by the operator: "Nothing is live or in use, do it all": move the history out, rewrite the steps
  as intent and outcome, cover every path.
