---
id: T346
title: The completed activity's status card lacks the semester count (1 of 3 this semester)
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-29
---

# T346 — The completed activity's status card lacks the semester count (1 of 3 this semester)

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low: the design's words, not a defect.
**Surfaced:** 2026-09-29, flow 03's build review (T342, `design/flows/03-trainee-files-activity/build-review.md`), filed under the agreed stopping line.

## Symptom

The completed status card reads "Rated 4. Credited 1 item to PAED-001." The accepted design (flow 03 round 3, Step 3.6's board) adds ": 1 of 3 this semester".

## Root cause

The activity page makes no progress read (T342 lane C left it out; build review D2).

## What to build

Read the credited item's window count (the per-semester target, D39) for the subject on the completed page, cheaply, or move the count to flow 05's progress work. Update Step 3.6's Expect.

## Verification

- [ ] The completed card shows the semester count — checked by a page test and the replay of Step 3.6

## Related

_Task ids, decisions (`D<n>` for product, `W-<n>` for process), files, commits._

## Notes

_Observed facts, inferences, and unknowns. Label them when the difference matters._
