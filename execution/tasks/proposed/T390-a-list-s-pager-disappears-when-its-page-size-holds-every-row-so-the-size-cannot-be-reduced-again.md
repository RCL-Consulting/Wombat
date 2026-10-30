---
id: T390
title: A list's pager disappears when its page size holds every row, so the size cannot be reduced again
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-10-30
---

# T390 — A list's pager disappears when its page size holds every row

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. A keyboard or pointer user who picks a large page size loses the pager and its select until the
page reloads; nothing is lost, and a reload restores the default size.
**Surfaced:** 2026-10-30, T387 step 7, the states pass on `/admin/data-rights` (`data-rights-requests--paged`): 50
requests, "Requests per page" set to 50 or 100, and the pager and its select are gone.

## Why

Found by the states pass's agent and reproduced by the integrator on scratch `wombat_scenario_t387_s10`. Five list pages
draw `PagerControls` only when `TotalCount > PageSize` (flows 11–14's F9/F10 rule): `MyActivities.razor:111`,
`AssessorsList.razor:129`, `RequestsList.razor:145`, `PendingTraineesList.razor:314` and `UsersList.razor` (F9). The
rule hides the select that set the size, so the only way back to a smaller size is a reload.

## Symptom

With 50 rows, choosing 50 or 100 per page removes the pager; there is no control left to choose 10 or 20.

## Root cause

The pager's visibility is judged against the chosen size, not against the smallest size the select offers.

## What to build

Draw the pager whenever the rows run past the smallest offered size (10), so the select stays while it can change the
view; or keep the select alone when one page holds every row. One rule for all five pages, in `PagerControls` or a
shared helper, with DESIGN.md's pager rule amended.

## Verification

_This repo's completion signal. Each item names how it will be checked — a command, a test,
an observation. "Code exists" is not a completion signal._

- [ ] _criterion_ — checked by _command or observation_

## Related

_Task ids, decisions (`D<n>` for product, `W-<n>` for process), files, commits._

## Notes

_Observed facts, inferences, and unknowns. Label them when the difference matters._
