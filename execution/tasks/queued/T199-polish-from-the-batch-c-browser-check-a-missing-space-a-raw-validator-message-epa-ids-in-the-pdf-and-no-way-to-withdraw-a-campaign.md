---
id: T199
title: Polish from the batch-C browser check: a missing space, a raw validator message, EPA ids in the PDF, and no way to withdraw a campaign
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

**Severity:** Low. Each is small.
**Surfaced:** 2026-09-24, the browser check of T142/T158/T161/T173/T176/T182/T183.

## Items

1. On the curriculum items page, "…end-of-life care(inactive: not in force)" has no space before the marker.
2. The panel form shows a raw FluentValidation string for the Institution scope ("Validation failed: -- : …
   Severity: Error"), while the Speciality scope gets a friendly message.
3. The portfolio PDF prints the EPA as an internal id ("EPA: 3") and "MSF campaign: 3". Use the EPA code and title
   (`Activity.EpaId` is stamped since T137) and the campaign's name. Check this against T169's changes first.
4. EpaEdit still offers "Deactivate" on an EPA that is already inactive (also listed in T196).
5. An MSF campaign left UnderReview has no Withdraw button in the UI. Dev campaign 4 is such a campaign, and review 1
   shows T173's notice for it.

## Verification

- [ ] Each item is fixed, with a bUnit test or a browser check recorded here.

## Related

T158, T182, T169, T173.
