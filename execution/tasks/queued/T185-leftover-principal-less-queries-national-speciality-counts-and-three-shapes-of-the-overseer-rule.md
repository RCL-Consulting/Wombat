---
id: T185
title: Leftover principal-less queries, national speciality counts, and three shapes of the overseer rule
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

**Severity:** Low. Each item is narrow or has no hostile caller today; together they keep T113's rule from being
uniform.
**Surfaced:** 2026-09-24, the T113 review (nits and notes).

## Items

1. `GetProgramScaleIdForReviewQuery`, `GetCurrentUserProfileQuery(string UserId)` and
   `UpdateCurrentUserProfileCommand(string UserId, …)` take no principal. The profile pages pass the signed-in user's
   own id, so the fix is to derive it from the principal.
2. `ListExpiringDecisionsQuery` is unscoped and has no caller. Delete it, or scope it.
3. `GetSpecialityAdminDashboardSummaryQuery.cs:47`, and the SubSpeciality version, count by the national speciality id
   without the institution (the catalogue memory: always conjoin the institution).
4. The overseer rule has three shapes: `ActivityService.IsScopedOverseerOf`, `ActivityReadScope.WhereReadableBy` and
   `TraineeScopeResolver.IsOverseenBy`. Make it one predicate, with an expression form for queries.
5. `CreditTargetResolver.PickProfileAsync`, `TraineeQuotaProgressReader` and
   `GetEpaTrajectoryForTraineeQuery.ResolvePinnedLadders` still pick a profile by programme start. They agree with the
   resolver whenever the trainee has an active profile; align them, or say why not.

## Verification

- [ ] Each item fixed has a test, or a line here saying why it was left.

## Related

T113, T153, T056.

## Note, 2026-09-24 (T183 review)

- `MayReadAsync` has no trainee-first rule. `CommitteeDecisionAuthorization.DemandReviewAccess` stops a Trainee who also
  holds an oversight role from reading other trainees' records; `MayReadAsync` does not.
- The entrustment admin list loads every decision at the institution for a Speciality or SubSpecialityAdmin, then
  filters in memory. Nothing leaks, but it is inefficient.
- After T183 the predicates nest: `IsAdministeredBy` ⊂ `IsAdministeredOrCoordinatedBy` ⊂ `IsOverseenBy`. Item 4
  (three shapes of the overseer rule) now means folding `ActivityService.IsScopedOverseerOf` and
  `ActivityReadScope.WhereReadableBy` into these.

