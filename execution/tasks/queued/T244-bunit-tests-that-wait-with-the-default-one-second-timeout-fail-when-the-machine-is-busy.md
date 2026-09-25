---
id: T244
title: bUnit tests that wait with the default one-second timeout fail when the machine is busy
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T244 — bUnit tests that wait with the default one-second timeout fail when the machine is busy

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Occasional red runs erode trust in the suite.
**Surfaced:** 2026-09-25. `CurriculumItemsRemoveTests.ASecondConfirmWhileTheFirstRemoveRuns_SendsNothing_AndRemoveIsOffMeanwhile(heldAt: Command)`
(T222) and `DecisionsDuePageTests.WhileTheQueryRuns_ThePageShowsSkeletons` (T194 run) each failed once under load and
passed on a rerun.

## What to build

Find why each needs more than a second. Where the test waits for a render that genuinely follows an awaited task, give
`WaitForAssertion`/`WaitForState` an explicit timeout in one shared constant (e.g. 5s), in the Web test base class. Do
not hide a real race: first check each flaky test for an unawaited task in the component.

## Verification

- [ ] The Web suite passes 10 runs in a row while the Integration suite runs beside it.

## Related

T227, T222, T194.
