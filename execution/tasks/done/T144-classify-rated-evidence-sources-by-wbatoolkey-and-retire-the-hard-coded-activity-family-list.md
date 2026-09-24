---
id: T144
title: Classify rated evidence sources by WbaToolKey and retire the hard-coded activity-family list
status: done
priority: P3
owner: agent
model: sonnet
depends_on: []
created: 2026-09-23
started: 2026-09-24
completed: 2026-09-24
---

# T144 — Rated evidence is still categorised by guessing from its key, although types now say which instrument they are

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

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

- [x] A builder-made rated type keyed `mini_cex` under an unfamiliar key charts as Direct observation —
      `RatedActivityTypesTests`
- [x] The seeded CPSA tools classify exactly as today — `RatedActivityTypesTests`, `SeedScaleKeyTests`
- [x] The committee sampling report's source counts are unchanged on the scenario corpus. Closed on database
      evidence rather than in the browser: dev holds exactly the 19 seeded types, each with its catalogue tool key,
      no builder types, and zero `CommitteeReviews`, so there is no report to render. Each seeded type's bucket is
      pinned by `SeedScaleKeyTests.ExpectedSourceBucketBySeed`.

## Related

[T122] (introduced `WbaToolKey` and the `WbaTools` vocabulary), [T134] (consolidated the family map), [T120].

---

## As built — 2026-09-24

- `RatedActivityTypes.Classify(key, wbaToolKey, schemaJson)` resolves the evidence category from a **static map keyed
  on the instrument** (`WbaToolKey`), with no migration. It falls back to today's family match only for an unkeyed
  type; `acat` is the only such fallback. `msf`, `reflective_exercise`, `clinical_audit`, `portfolio_review` and
  `learner_feedback` have no category. A type with no category counts as a source under its own key, which is how
  `msf_cpsa` would appear. `chart_stimulated_recall` stays under Conversation (College question 8 may move it).
- `LoadAsync` projects `WbaToolKey`, and the trajectory passes the type's key. The T144 comments are gone.
- Tests:
  - A builder type keyed `mini_cex` under an unfamiliar type key classifies as Mini-CEX.
  - Each seeded tool's category is pinned per seed.
  - An unkeyed `*_paed`-style type still buckets by its family.
  - All three are mutation-checked.
- **Documented, not fixed:** the category follows the live `WbaToolKey`, which a draft save writes. Filed as [T179].
