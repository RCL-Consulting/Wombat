---
id: T178
title: DESIGN.md's nav table is stale for several roles, and PlaceholderPage still maps pages that exist
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The design contract disagrees with the navigation, and dead routes linger.
**Surfaced:** 2026-09-24, the T141 review.

## Symptom

- DESIGN.md § The NavMenu's role table: Everyone lacks Data Rights, Coordinator lacks MSF Campaigns and Committee
  Reviews, and Decision Panels is missing from the rows that have it. T141 corrected only the Trainee and
  PendingTrainee rows.
- `PlaceholderPage.razor` still maps `activities`, `my-activities`, `activity-inbox`, `specialities` and
  `activity-types`, all of which exist as real pages now.

## What to build

Regenerate the table from `NavMenu.razor` (each `AuthorizeView` block and its links). Delete the dead placeholder arms
once nothing links to them (grep). Run T141's `EveryLinkOffered_OpensAPageTheRoleIsAdmittedTo` for every role, not
only Trainee and PendingTrainee.

## Verification

- [ ] DESIGN.md's table matches NavMenu.razor for every role.
- [ ] No placeholder route shadows a real page, and `EveryLinkOffered…` runs for all nine roles.

## Related

T141.

Note, 2026-09-25: the Coordinator's nav lists "Data Rights" twice.
