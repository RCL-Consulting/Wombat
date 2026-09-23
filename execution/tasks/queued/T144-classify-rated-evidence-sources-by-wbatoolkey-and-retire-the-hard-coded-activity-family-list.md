---
id: T144
title: Classify rated evidence sources by WbaToolKey and retire the hard-coded activity-family list
status: queued
priority: P3
owner: agent
model: sonnet
depends_on: []
created: 2026-09-23
---

# T144 — Rated evidence is still categorised by guessing from its key, although types now say which instrument they are

**Severity:** Low. It only changes labels: the trajectory's source label and the committee sampling report's
`DistinctSourceCount` / `SingleSource`. What is rated and what credits are unaffected.
**Surfaced:** 2026-09-23, closing [T122], whose design promised this retirement and deliberately did not do it.

## Symptom

`RatedActivityTypes.SourceByActivityFamily` (`src/Wombat.Application/Features/Activities/Services/RatedActivityTypes.cs`)
still maps an activity type to an evidence category (Direct observation, Conversation, Case analysis) by exact key or
by a `"<family>_"` prefix. A builder-made rated type under an unfamiliar key reads as its own raw key on the trajectory
chart and counts as a separate source on the sampling report.

## Root cause

[T122] added `ActivityType.WbaToolKey`, the declared identity this map should be keyed on, and spelled the vocabulary
keys like the families (`mini_cex`, `dops`, `direct_observation`, `cbd`, `chart_stimulated_recall`, `cca`, `rca`) so
the switch would be a lookup swap. It did not make the swap, because:
- the key → category mapping needs a home: a `Category` column on `WbaTools` or a static map, and either is a new
  vocabulary decision;
- `acat` is not a College instrument and carries no key, so the family fallback cannot simply be deleted;
- filing `chart_stimulated_recall` under Conversation or Case analysis is the College's call;
- it changes a committee-facing number, which T122 did not verify.

## What to build

`Classify(key, wbaToolKey, schemaJson)` prefers the tool key's category and falls back to the family match only for
unkeyed types. `LoadAsync` and the trajectory query select `WbaToolKey`. Decide where the category lives and record it.
Update the comments that name this task (`RatedActivityTypes.cs`, `GetEpaTrajectoryForTraineeQuery.cs`,
`RatedActivityTypesTests.cs`).

## Verification

- [ ] A builder-made rated type keyed `mini_cex` under an unfamiliar key charts as Direct observation —
      `RatedActivityTypesTests`
- [ ] The seeded CPSA tools classify exactly as today — `RatedActivityTypesTests`, `SeedScaleKeyTests`
- [ ] The committee sampling report's source counts are unchanged on the scenario corpus — browser check

## Related

[T122] (introduced `WbaToolKey` and the `WbaTools` vocabulary), [T134] (consolidated the family map), [T120].
