---
id: T345
title: An activity's E7 name can differ by viewer; an assessor may not see the nominee suffix
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-29
---

# T345 — An activity's E7 name can differ by viewer; an assessor may not see the nominee suffix

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low: cosmetic, no leak.
**Surfaced:** 2026-09-29, flow 03's build review (T342, `design/flows/03-trainee-files-activity/build-review.md`), filed under the agreed stopping line.

## Symptom

An activity's E7 name adds " · <nominee>" when another of the subject's activities shares type, EPA and date. On the detail page the siblings are counted through `WhereReadableBy`, which leaves out the `field:` and transition-actor arms the single-activity gate admits, so an assessor may see the name without the suffix the registrar sees.

## Root cause

`GetActivityByIdQuery` counts siblings by the viewer's readable set, not the subject's (T342 lane A1; build review R7).

## What to build

Count siblings for the name by the subject's own activities (the names reveal only type, EPA and date the viewer already sees), or document that the suffix is viewer-relative.

## Verification

- [ ] The same activity's h1 reads the same for its registrar and its assessor on a collision — checked by an Application test

## Related

_Task ids, decisions (`D<n>` for product, `W-<n>` for process), files, commits._

## Notes

_Observed facts, inferences, and unknowns. Label them when the difference matters._
