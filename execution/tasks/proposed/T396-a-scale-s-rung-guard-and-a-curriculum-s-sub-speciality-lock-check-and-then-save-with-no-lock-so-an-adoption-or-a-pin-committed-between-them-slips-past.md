---
id: T396
title: A scale's rung guard and a curriculum's sub-speciality lock check and then save with no lock, so an adoption or a pin committed between them slips past
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

Found by flow 16's build code review (T394, `design/flows/16-college-catalogue/build-review-code.md`, S-1) and taken
out of the fix pass by the operator on 2026-11-05 (F1: "yes"). `UpdateEntrustmentScaleCommandHandler` reads whether the
scale is in use (Q16, T395) and `UpdateCurriculumCommand` whether the version has an item or an adoption (Q19), then
save; nothing holds the scale or the curriculum between the read and the commit, so a concurrent pin (an item's minimum
set to the scale) or a concurrent adoption commits unseen and the reorder or the sub-speciality move goes through. The
fix reaches flows 15 and 17 (adoption, the item editor) and should follow the credit locks' pattern
(`IEpaCreditLock.HoldForChangeAsync`, `ITraineeCreditLock`), taken before the read and committed with the save.

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
