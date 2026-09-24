---
id: T192
title: The late-filing warning says a date 'can still be filed' when the server will refuse it as before the programme
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

**Severity:** Low. Two messages on one screen contradict each other.
**Surfaced:** 2026-09-24, the T160 browser check.

## Symptom

On a crediting type, entering an encounter date before the trainee's `ProgrammeStartDate` shows the lateness warning
("This encounter was 267 days ago. It can still be filed, but …"). After Submit the warning stays beside the refusal
"cannot be before the trainee's programme started". The form's lateness check (`FilingLateness`, `ActivityForm`) does
not know the programme-start bound.

## What to build

Give the form the subject's programme start (the page resolves it once, the way `FiledOn` is passed), and warn about
lateness only for a date the server would accept. A date before the start should say so instead, as a client hint only;
the server stays the rule.

## Verification

- [ ] A pre-programme date shows the programme-start hint, not the lateness warning. bUnit test.

## Related

T160.
