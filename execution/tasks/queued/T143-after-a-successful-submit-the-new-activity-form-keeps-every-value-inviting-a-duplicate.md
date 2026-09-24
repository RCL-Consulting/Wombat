---
id: T143
title: After a successful submit the new-activity form keeps every value, inviting a duplicate
status: queued
priority: P3
owner: agent
model: sonnet
depends_on: []
created: 2026-09-23
---

# T143 — After a successful submit the new-activity form keeps every value, inviting a duplicate

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. OBSERVED that the values stay; that a second press creates a duplicate is INFERRED, not tested.
**Surfaced:** 2026-09-23, observed by the [T130] staging run.

## Symptom

After "Activity submitted." on `/activities/new`, every field keeps its value. Pressing Submit again would, as far as
the code reads, create a second, identical activity.

## Root cause

Not yet traced. The page does not reset its model after a successful submit.

## What to build

On success, navigate to the new activity (or reset the form), so a second press cannot file the same encounter twice.
Check the interaction with [T127] (a failed submit leaves an orphan draft).

## Verification

- [ ] After a successful submit the form is empty or the page has moved on — checked by a bUnit test and in the browser
- [ ] Pressing Submit twice files one activity — checked in the browser

## Related

[T127].

## Update 2026-09-24 — EPA-stream survey

Lands with [T127], in the same change to `CreateOrTransitionAsync`: navigate to `/activities/{id}` after a successful
create. Closes with T127.
