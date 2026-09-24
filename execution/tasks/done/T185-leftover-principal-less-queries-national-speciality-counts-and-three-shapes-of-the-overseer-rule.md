---
id: T185
title: Leftover principal-less queries, national speciality counts, and three shapes of the overseer rule
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-25
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

- [x] Each item fixed has a test, or a line here saying why it was left.

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

## Note, 2026-09-24 (T169 review)

The profile picks still disagree three ways:
- The portfolio cover and T169's section use `TraineeScopeResolver.PreferredProfiles` (active, then highest id).
- Credit (`CreditTargetResolver.PickProfileAsync`) and the progress page use active, then latest start.
- The trajectory's ladder (`GetEpaTrajectoryForTraineeQuery.cs:326-332`) uses active, then latest start, with no id
  tie-break.

With two active profiles (the database forbids that, but past profiles tie), credit can land on one curriculum while
the export reads another. Make it one pick.

---

## As built — 2026-09-25 (`fbd1525`)

- **Principal-less queries:** the last ones take the caller, and `ListExpiringDecisionsQuery` is handled.
- **Dashboards:** the speciality and sub-speciality counts conjoin the institution.
- **Profile pick:** it is one rule, `PreferredProfiles`.
- **Trainee first:** `TraineeScopeResolver.ActsAsTrainee` makes a Trainee who also holds an admin role a trainee first
  on the entrustment admin list, revoke and the committee card. An Administrator who is also a Trainee no longer learns
  which decision ids exist.

Handler tests throughout (dev has no user holding both roles). Filed [T216]: committee scheduling and the agenda
preview do not ask the trainee-first rule yet.
