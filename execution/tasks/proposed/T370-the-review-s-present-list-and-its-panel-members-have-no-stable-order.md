---
id: T370
title: The review's Present list and its panel members have no stable order
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-10-19
---

# T370 — The review's Present list and its panel members have no stable order

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The names are right; only their order moves between databases, so a decision's Present line and
the runbook's quotes of it disagree run to run.
**Surfaced:** 2026-10-19, T369's replay, Act 4: Steps 4.25, 4.41 and 4.42 read "Thandi Zulu (chair), Sarah Botha,
David Naidoo"; T366's replay of the same steps read Naidoo before Botha.

## Symptom

On a fresh replay, Decision panels, the review page's present picker and a recorded decision's Present line list the
panel's members in a different order from the last replay (Botha before Naidoo, then Naidoo before Botha), with the
same data played the same way.

## Root cause

Inferred, not proven: `PanelSeat.SittingAt` (`src/Wombat.Application/Features/CommitteeDecisions/PanelSeat.cs:135`)
filters `review.Panel.Members` with no `OrderBy`, so the order is whatever PostgreSQL returns for the navigation
collection. The other readers of `Panel.Members` (`ListDecisionPanels`, `GetDecisionPanelById`) were not checked.

## What to build

One stated order for a panel's members everywhere they are listed (chair first, then members by display name, then
externals, say), and the Present line stored in that order; the runbook's quotes follow it.

## Verification

- [ ] Every reader of a panel's members orders them the same way — a test per reader with members inserted out of order.
- [ ] Steps 4.25, 4.41 and 4.42 pass on the next replay — their Actual lines.

## Related

T369 (the replay that found it), T366, T361 (flow 07, the review sitting).

## Notes

- Observed: T369 replay, act-4-annual-review.md, Steps 4.25, 4.41, 4.42 (Gap: runbook: order).
