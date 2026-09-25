---
id: T289
title: The assessor-profile form lets a trainee-admin move a panel chair, and lets an admin pull an Assessor from another institution
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-25
---

# T289 — The assessor-profile form lets a trainee-admin move a panel chair, and lets an admin pull an Assessor from another institution

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium.
**Surfaced:** 2026-09-25, the T278 review.

## Symptom

- `CreateOrUpdateAssessorProfile` never asks the trainee-first rule. A Trainee who is also an Administrator can move their
  panel's chair to another institution through `UpdateScopeAsync`, and so stall their own review.
- The new-profile branch never checks the user's current institution. A forged form value lets an admin at A pull an
  Assessor from B, and the move drops that user's external logins.

## What to build

Refuse anyone who holds Trainee first. Require the user to be at the caller's institution before creating or moving a
profile, with an Administrator bound by the same rule for moves between institutions, or naming both institutions
explicitly. Handler tests and the audit trap.

## Verification

- [ ] Both paths are refused. Tests.

## Related

T278, T256, T237.

Note, 2026-09-25 (the T159 replay, step 2.4): the `Assessor user` picker never narrows. A user who already has a profile is
still offered, and the page stays on `/admin/assessors/edit` after a save. `ListAssessorUsersQuery` has no exclusion, and
`CreateOrUpdateAssessorProfile` looks a profile up by user, so creating one for someone already profiled would overwrite
theirs (inferred, not tried). T064 is marked done; this is its gap.
