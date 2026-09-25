---
id: T249
title: The portfolio PDF prints the response count of an MSF group whose results are suppressed, as "Nurse: 1 responses"
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T249 — The portfolio PDF prints the response count of an MSF group whose results are suppressed, as "Nurse: 1 responses"

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. It is a count, not an answer, and it is ungrammatical.
**Surfaced:** 2026-09-25, the T225 review. It predates T225.

## Symptom

`MsfSectionComponent` prints the exact count for a group whose results are suppressed below the anonymity threshold.
The trainee's web copy (`MyMsfReports.razor`) leaves suppressed groups out. "1 responses" is also wrong.

## What to build

Print what the web copy shows: leave suppressed groups out, or say "fewer than N responses, not shown", once. Pluralise
the counts.

## Verification

- [ ] PDF text tests: a suppressed group has no count, and "1 response" is singular.

## Related

T225, T217, T205, T169.
