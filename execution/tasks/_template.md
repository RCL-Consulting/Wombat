---
id:
title:
status: queued
priority: P2
owner: agent
depends_on: []
created:
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** _High / Medium / Low, and why — this is what `priority` above is derived from._
**Surfaced:** _When and how it was found. "While fixing T100" is worth more than a date alone._

## Symptom

_What is observably wrong, or what is missing. Written so someone who has never seen the code
can tell whether it is still true._

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
