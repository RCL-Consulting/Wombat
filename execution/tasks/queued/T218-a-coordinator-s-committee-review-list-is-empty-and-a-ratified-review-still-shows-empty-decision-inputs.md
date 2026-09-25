---
id: T218
title: A Coordinator's committee review list is empty, and a ratified review still shows empty decision inputs
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T218 — A Coordinator's committee review list is empty, and a ratified review still shows empty decision inputs

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The Coordinator can schedule a review and then cannot find it.
**Surfaced:** 2026-09-25, the T131 slices 5–6 browser check. Both predate T131.

## Symptom

- As the Coordinator, `/committee/reviews` reads "No reviews yet", even for a review they scheduled.
  `ListReviewsForPanelQuery` lists only reviews on panels the caller sits on, unless the caller is an Administrator or
  InstitutionalAdmin.
- A ratified review's Decision card still shows empty Rationale and Conditions inputs, with no Record button.

## What to build

List, for a scheduling role, the reviews they may open at their institution (T182's scope, trainee first per T216), and
render a decided review's decision read-only.

## Verification

- [ ] The Coordinator's list shows review 5. bUnit and browser.
- [ ] A ratified review shows no decision inputs. bUnit.

## Related

T131, T182, T212.
