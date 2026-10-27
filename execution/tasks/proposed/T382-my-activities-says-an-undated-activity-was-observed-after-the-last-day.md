---
id: T382
title: My activities says an undated activity was observed after the last day
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-10-27
---

# T382 — My activities says an undated activity was observed after the last day

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. A wrong sentence on a read-only page; no credit is affected.
**Surfaced:** 2026-10-27, T379's replay, Step 5.28 (du Plessis's My activities).

## Why

The Act 5 replay: "the cancelled DOPS lists as 'no date' … It still carries the 'Observed after your last day…' note
row." Checked in the replay database: activity 8 (`dops_cpsa`, cancelled) has `DataJson` `{"epa_id": "3"}` and
`ObservedOn` 2026-10-27, the day it was created.

## Symptom

On a former trainee's My activities, a row that reads "no date" carries "Observed after your last day in the programme,
‹day›, so it counts towards nothing."

## Root cause

Observed: `ListActivitiesBySubjectQuery.cs:240` sets `ObservedAfterEnd = TraineeProfile.IsAfterEnd(endedOn,
row.ObservedOn)`, and an activity with no encounter date stores its creation day as `ObservedOn`, while the row's shown
date comes from the schema's encounter field.

## What to build

The note row only on a row that has an encounter date after the last day (the shown date), or a different sentence
for an undated one (e.g. "Filed after your last day…"); decide which. Check `IsAfterEnd`'s credit use is unaffected
(T281).

## Verification

- [ ] A Web/Application test: an undated activity created after the last day carries no "Observed after" note (or the
  decided sentence); a dated one does.

## Related

T379 (flow 13, lane R's note row), T281, Step 5.28.

## Notes
