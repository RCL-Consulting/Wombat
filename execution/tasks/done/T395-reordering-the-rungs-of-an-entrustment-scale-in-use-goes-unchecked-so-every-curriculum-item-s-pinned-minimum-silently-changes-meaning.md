---
id: T395
title: Reordering the rungs of an entrustment scale in use goes unchecked, so every curriculum item's pinned minimum silently changes meaning
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-11-04
promoted: 2026-11-05
started: 2026-11-05
completed: 2026-11-05
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** _High / Medium / Low, and why — this is what `priority` above is derived from._
**Surfaced:** _When and how it was found. "While fixing T100" is worth more than a date alone._

## Why

Found by flow 16's round 1 assessment (T394, `design/flows/16-college-catalogue/round-1-assessment.md`, Q16) and checked
2026-11-04: `UpdateEntrustmentScaleCommandHandler.cs:61-84` runs its guards only when a rung is removed. A save that
keeps every rung but changes their `Order` (the editor's Up and Down) passes, yet a curriculum item pins its minimum by
rank (`MinimumLevelOrder`, `MinimumLevelByStageJson`), so on the CPSA ladder a swap of 4 and 5 changes what nine items'
minimum means in every adopting institution, with no refusal and no word. The assessment recommends offering Up, Down
and Remove only on a scale nothing uses, with the same refusal in the handler. Flow 16's scale page (part 1, C1) will
draw whichever the operator chooses.

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

## Closed

2026-11-05: built in flow 16 (T394, lane B; squashed onto master as `de4cc718`), as the operator decided (Q16, P2): Up,
Down and Remove are offered only on a scale nothing uses, and `UpdateEntrustmentScaleCommand` refuses a reorder or a
removal on one in use (`EntrustmentScaleUses`, the delete's seven uses), after Q6's named refusals (Q18). Proved by
`EntrustmentScaleReorderGuardTests` (nine tests) and `CatalogueReadsPostgresTests`. Its PostgreSQL half (a reorder on an
unused scale) was T397.
