---
id: T238
title: An erased trainee's pseudonymised profile stays in committee scope, and the schedule command accepts it
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T238 — An erased trainee's pseudonymised profile stays in committee scope, and the schedule command accepts it

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. It needs a crafted request, and the pseudonym names no one.
**Surfaced:** 2026-09-25, the T194 and T216 reviews.

## Symptom

`ErasureExecutor` replaces an erased trainee's profile user id with a pseudonym that matches no account. The profile
lookup includes inactive profiles, so the pseudonym stays in scope. The schedule command accepts it through a crafted
request (the picker and, since T194, the panel list leave it out). A profile that outlives its user's Trainee role has
the same shape.

## What to build

One rule for "a trainee this caller may act on": an active profile whose user exists and still holds Trainee. Used by
`CommitteeTraineeScope` (scheduling and its commands) and by the trainee lists, which drop stale and erased profiles.
Check T113's other trainee lists against the same rule.

## Verification

- [ ] Scheduling an erased trainee's pseudonym is refused before any write. Handler test.
- [ ] The trainee lists omit erased and stale profiles. Tests.

## Related

T194, T216, T113, T026 (erasure).
