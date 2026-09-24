---
id: T150
title: Sampling and trajectory count assessors from drafts and cancelled requests, by a hard-coded field key
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# T150 — Sampling and trajectory count assessors from drafts and cancelled requests, by a hard-coded field key

**Severity:** Low-Medium. The committee's sampling signal can be padded without any assessment happening.
**Surfaced:** 2026-09-24, T102 mapping (consumers lens). Not fixed by T102, which judges who may be named, not
what a named assessor counts for.

## Symptom

`GetSamplingConcentrationWarnings.cs` (around :126-152 and :254-288) and `GetEpaTrajectoryForTraineeQuery.cs` (around
:484) read the literal key `assessor_user_id` out of `DataJson`, with no state filter and no rating requirement. A
trainee who names five different, perfectly eligible assessors on five drafts (or requests they then cancel) raises
the distinct-assessor count and clears the concentration warning without anyone assessing anything. Both also trim the
value, while the actor grammar and T102's gate compare it exactly.

A builder-made type whose assessor field has another key is invisible to both.

## What to build

Count only activities that reached a crediting terminal state (or carry a rating), and read the schema's declared
nominee fields (`ActorFieldRules.RequiredRolesByNomineeField`, pinned version) rather than a literal key. Compare ids
exactly.

## Verification

- [ ] Drafts and cancelled requests do not change the distinct-assessor count — test.
- [ ] A type with a differently named user field is counted — test.

## Related

T102, T131.
