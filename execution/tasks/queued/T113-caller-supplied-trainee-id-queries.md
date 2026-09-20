---
id: T113
title: "Two more queries trust a caller-supplied trainee id"
status: queued
priority: P2
created: 2026-09-19
---
# T113 — Two more queries trust a caller-supplied trainee id

**Status:** open
**Surfaced:** 2026-09-19, closing the read boundary for T101.
**Severity:** Medium — progress and committee history about a named trainee, readable by anyone who can
name them.

## Symptom

T101 added a `ClaimsPrincipal` and scope filtering to `ListActivitiesBySubjectQuery`,
`GetEpaTrajectoryForTraineeQuery`, the coordinator dashboard's stalled-activity query and
`GetSamplingConcentrationWarnings`. Two queries of exactly the same shape were left, because they read a
different aggregate and `ActivityReadScope.WhereReadableBy` does not apply to them:

- `src/Wombat.Application/Features/Curricula/GetCurriculumProgressForTrainee.cs:17` —
  `GetCurriculumProgressForTraineeQuery(string TraineeUserId)`, no principal. Called from
  `MyProgress.razor:81`, directly beside the trajectory query that T101 *did* scope.
- `src/Wombat.Application/Features/CommitteeDecisions/ListReviewsForTrainee.cs:8` —
  `ListReviewsForTraineeQuery(string TraineeUserId)`, no principal.

Both filter on the string the caller passes, so passing another trainee's id returns their data:
curriculum progress (counts, minimum-level achievement, credited items) and committee review history.

## Why they were not done with T101

They read `CurriculumItemProgress` and `CommitteeReview`, neither of which carries the T101 scope stamps.
Scoping them means either resolving the trainee's scope through `TraineeProfile` (the
`ExportPortfolio.ResolveTraineeScopeAsync` pattern) or stamping those aggregates too.

Prefer the resolver: unlike `Activity`, these rows are *about a trainee by definition*, so there is no
case where the subject has no profile and the row still means something.

## Also in scope

`GetSamplingConcentrationWarnings` was given a principal by T101, but it scopes the **rows** it reads,
not the **review** it is asked about. Reaching another institution's review id returns an empty report
rather than a refusal. The proper review-level ladder already exists in `GetCommitteeReviewById.cs:35-63`
(trainee self + state gate / InstitutionalAdmin institution / panel membership). Duplicating it would be
a third copy — extract it into one shared helper and call it from both.

## Related

[T101] closed the activity surface. [T112] is the data-rights equivalent.
