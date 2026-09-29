---
id: T343
title: My activities reads every row with its clinical text to serve one page; collision keys and paging belong in SQL
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-29
---

# T343 — My activities reads every row with its clinical text to serve one page; collision keys and paging belong in SQL

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low: performance only; correct results.
**Surfaced:** 2026-09-29, flow 03's build review (T342, `design/flows/03-trainee-files-activity/build-review.md`), filed under the agreed stopping line.

## Symptom

My activities (`ListActivitiesBySubjectQuery`) loads every readable row of the subject, with its `DataJson` and two correlated transition subqueries, then pages in memory. A registrar with hundreds of activities pays for all of them on every page.

## Root cause

E7's collision key needs every row, so the query read them all (T342 lane A1, decision 6). Only `ActivityTypeId`, `EpaId`, `ObservedOn` and whether the date was declared are needed for it.

## What to build

Compute the collision keys with a SQL `GroupBy` on those columns (count > 1); page with `Skip`/`Take` in SQL; project `DataJson` and the last move for the page's rows only.

## Verification

- [ ] The query issues no read of `DataJson` for rows outside the page — checked by a Postgres integration test that counts rows materialised or inspects the SQL
- [ ] E7 names and paging unchanged — the existing `NeedsYouAndMyActivitiesTests` and My activities tests pass

## Related

_Task ids, decisions (`D<n>` for product, `W-<n>` for process), files, commits._

## Notes

_Observed facts, inferences, and unknowns. Label them when the difference matters._
