---
id: T193
title: Refusal alerts are not announced, and field help text is not linked to its input
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

**Severity:** Low. Accessibility: a screen reader may miss a refusal, and it never reads a field's help text.
**Surfaced:** 2026-09-24, the T160 browser check.

## Symptom

- `Alert.razor` renders `<div class="alert alert-danger">` with no `role="alert"` (or a live region), so a refusal
  that appears after Submit may not be announced. T125 gave `Alert` an optional `Role`; nothing sets it for errors.
- In `ActivityForm`, a field's help text ("When the encounter happened, not when this form is completed.", the Assessor
  hint) is not referenced by the input's `aria-describedby`. On the date field, `aria-describedby` names only T160's
  lateness region.

## What to build

Danger alerts default to `role="alert"`, and warning alerts to `role="status"`, unless the caller overrides. Every
field's help text gets an id, and the input's `aria-describedby` lists the help and any warning region.

## Verification

- [ ] A danger `Alert` renders with `role="alert"`. bUnit test.
- [ ] Every ActivityForm input with help text references it. bUnit test over the seed schemas.

## Related

T160, T177, T125.
