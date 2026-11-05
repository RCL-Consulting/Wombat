---
id: T397
title: Reordering the rungs of an unused entrustment scale fails on PostgreSQL: EF reports a circular dependency on the unique (scale, order) index
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-11-05
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** _High / Medium / Low, and why — this is what `priority` above is derived from._
**Surfaced:** _When and how it was found. "While fixing T100" is worth more than a date alone._

## Why

Found 2026-11-05 by flow 16's fix pass (T394, adding the PostgreSQL tests the build review's S-8 asked for). A save
that swaps two rungs' `Order` on a scale nothing uses (the only case flow 16's Up and Down are offered, Q16/T395) makes
EF Core order the two updates against the unique (scale, order) index and give up with a circular dependency; the
in-memory provider has no such index, so every unit test passes. Reported as already on master (inferred from the fix
pass's report; needs confirmation by a PostgreSQL test on master). It blocks flow 16's rung editor in real use; no runbook
step reorders a rung, so the replay would not show it. The usual fix: write the moved rungs in two steps (to free
orders, then to their places) in one transaction, or make the index deferrable.

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
