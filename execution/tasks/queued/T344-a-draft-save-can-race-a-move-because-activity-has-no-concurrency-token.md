---
id: T344
title: A draft save can race a move because Activity has no concurrency token
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-29
---

# T344 — A draft save can race a move because Activity has no concurrency token

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low: a milliseconds window, two tabs.
**Surfaced:** 2026-09-29, flow 03's build review (T342, `design/flows/03-trainee-files-activity/build-review.md`), filed under the agreed stopping line.

## Symptom

A draft save (`ActivityService.SaveDraftAsync`, T342) and a submit of the same activity from another tab can interleave: the save reads the draft, the submit commits, then the save writes the whole `DataJson`, `EpaId` and `ObservedOn`, leaving a Requested row whose data differs from the submit's snapshot.

## Root cause

`Activity` has no concurrency token; `MsfCampaign` and `CommitteeReview` use `xmin`.

## What to build

Add an `xmin` concurrency token to `Activity` (migration), and turn a `DbUpdateConcurrencyException` on save or move into a refusal the page shows ("This activity changed while you were editing it. Reload it and try again.").

## Verification

- [ ] A save racing a move is refused, not merged — checked by a Postgres integration test with two contexts

## Related

_Task ids, decisions (`D<n>` for product, `W-<n>` for process), files, commits._

## Notes

_Observed facts, inferences, and unknowns. Label them when the difference matters._
