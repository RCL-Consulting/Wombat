---
id: T210
title: Staff cannot see across a programme which trainees an MSF covered this semester
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

**Severity:** Low. Each trainee's own page and each review show coverage (T168). A coordinator planning campaigns
cannot see, per EPA and semester, "n of m trainees covered".
**Surfaced:** 2026-09-24, the T168 review (finding 5, a scope reading left open).

## What to build

A coordinator-facing view, institution-scoped (T113's rules), reusing `GetMsfCoverageForTraineeQuery`'s rule (after
[T186]'s fix derives coverage from the evidence rows): per EPA and semester, how many of the programme's trainees a
released campaign covered.

## Verification

- [ ] The counts match the per-trainee cards for dev's trainees. Test and browser.

## Related

T168, T113, T186.
