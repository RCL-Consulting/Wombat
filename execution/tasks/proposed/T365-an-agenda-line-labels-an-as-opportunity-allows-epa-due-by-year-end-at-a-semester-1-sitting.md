---
id: T365
title: An agenda line labels an as-opportunity-allows EPA Due by year end at a semester 1 sitting
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-10-13
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. A wrong label on optional lines; nothing is held back or recorded wrongly (since D53 a closing line
holds nothing back, and these lines are not closing).
**Surfaced:** 2026-10-13, T364 step 7, the replay of step 4.2: the schedule page's Agenda aside for 2026 S1.

## Why

The act 4 replay reported: "The schedule page's Agenda for 2026 S1 lists the optional PAED-008, 009 and 013 as 'Due by
year end'. Decisions due reads them 'As opportunity allows' for the same period, and so does the runbook." The 2026 S2
Agenda reads them correctly.

## Symptom

At a semester 1 sitting, an annual EPA the College decides as opportunity allows (PAED-008, 009, 013) is labelled "Due by
year end" on an agenda: the schedule page's Agenda aside (flow 08) and, by the same rule, the review page's agenda.
Decisions due labels the same EPA "As opportunity allows". At the semester 2 sitting both agree.

## Root cause

`GetCommitteeAgendaQueryHandler.StatusOf` (`GetCommitteeAgenda.cs`, written in T131 slice 4, `c3e6a182`) reads only the
stored line: a cadence line that is not closing and whose window ends after the sitting is `DueByYearEnd`. The line does
not record `CurriculumItem.DecisionIsOpportunistic`, which only feeds `IsClosing` (`CommitteeAgendaLine.ClosesAt`). At
S2 the window's last semester is the sitting, so the line falls through to `AsOpportunityAllows`; at S1 it does not.
Decisions due checks `isOpportunistic` first (`CommitteeAgendaDtos.cs`, the due-status rule).

## What to build

Freeze the flag on the line, as its code and title are: `CommitteeAgendaLine.IsOpportunistic`, written by the planner
from `DecisionIsOpportunistic`, a migration (Designer and snapshot), and `StatusOf` checks it before `DueByYearEnd`.
Or, if a frozen flag is judged wrong, read it from the curriculum item at query time; say which and why.

## Verification

- [ ] `StatusOf` returns `AsOpportunityAllows` for an opportunistic annual line at S1: a unit test.
- [ ] The S1 Agenda aside and the S1 review's agenda read "As opportunity allows" for PAED-008, 009 and 013: a browser
  check on a replay database.

## Related

T131 (O7, slice 4), T364 (flow 08; step 4.2 of `act-4-annual-review.md`), D53.

## Notes

- Observed in the T364 replay (`wombat_scenario_t364`, 2026-10-13). That the review page's agenda shows it too is
  inferred from the shared `StatusOf`, not yet seen.
