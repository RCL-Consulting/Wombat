---
id: T401
title: The College, speciality and sub-speciality records keep an Active box where flow 18's institution record reads Status out with dialogs
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-11-11
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. A consistency gap between record pages; nothing is refused or lost.
**Surfaced:** 2026-11-11, flow 18's round 1 assessment (T400, Q16).

## Symptom

Flow 18 (Q5) gives the Administrator's institution record its Status read out, with Deactivate…/Reactivate… behind
dialogs and no Active box. Flow 16's College, speciality and sub-speciality records, which Q5 cites as its model, still
keep an Active box saved with the form.

## Why

The operator took Q16 as recommended ("yes", 2026-11-11): keep Q5, and propose this task for the College's records to
follow it.

## Root cause

_Why it happens. If this is not yet known, say so rather than guessing._

## What to build

_The intended shape of the work. Enough that someone else could start it._

## Verification

_This repo's completion signal. Each item names how it will be checked — a command, a test,
an observation. "Code exists" is not a completion signal._

- [ ] _criterion_ — checked by _command or observation_

## Related

_Task ids, decisions (`D<n>` for product, `W-<n>` for process), files, commits._

## Notes

_Observed facts, inferences, and unknowns. Label them when the difference matters._
