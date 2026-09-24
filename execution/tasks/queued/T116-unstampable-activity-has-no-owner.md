---
id: T116
title: "An activity whose scope cannot be resolved has no one who can move it"
status: queued
priority: P3
created: 2026-09-19
---
# T116 — An activity whose scope cannot be resolved has no one who can move it

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Status:** open
**Surfaced:** 2026-09-19, adversarial review of T101.
**Severity:** Low after T101's identity fallback; it was High before it.

## Background

T101 made `scope:` actor rules resolve from `Activity.InstitutionId` / `SpecialityId` /
`SubSpecialityId`, stamped at creation. A null stamp matches no `scope:` rule, for anybody.

That created a trap the review caught: an invited user is given an institution and speciality scopes by
`InvitedUserProvisioner` at acceptance, while their `TraineeProfile` is created only later by
`AdmitTrainee`. In between they can file a `reflective_note` (`/activities/new` is `[Authorize]` only,
and `ListActivityTypesQuery` offers speciality-scoped types on the claim). Out of `submitted` those
seeds offer only `approve` / `decline`, both `role:SpecialityAdmin+scope:speciality`. With null stamps
that matched nobody — and neither `WorkflowEvaluator.Evaluate` nor `ActivityService.TransitionAsync` has
an Administrator bypass — so the row was frozen for ever. Same for `research_output`,
`teaching_session`, `qi_project`.

**T101 fixed the reachable case** by falling back to the subject's own identity record
(`ActivityService.ResolveScopeFromIdentityAsync`): institution from `WombatIdentityUser.InstitutionId`,
speciality from `WombatIdentityUserSpecialityScope` when the user has exactly one.

## What is left

Two residual shapes still stamp null and can still freeze:

1. A subject with **no `InstitutionId` at all** on their identity record.
2. A subject holding **more than one** speciality scope — the column holds one id, and T101 deliberately
   refuses to pick arbitrarily.

Neither is reachable in the current scenario data, which is why this is Low rather than High. But the
structural point stands: **a row can exist that no role can move.**

## Options

1. **A narrow escape hatch.** Let a global `Administrator` take a transition on an activity whose scope
   is unresolvable. Deliberately NOT done in T101: a blanket Administrator bypass in `WorkflowEvaluator`
   would let an administrator complete an assessment as though they were the assessor, authoring
   clinical content under someone else's transition. If this route is taken, scope it to activities with
   a null stamp, and record the actor honestly in the transition.
2. **Refuse the creation instead.** Reject a create when the subject's scope cannot be resolved and the
   type's workflow needs a scoped actor to leave the initial state. Prevents the state arising, at the
   cost of a confusing refusal at the point of filing.
3. **Make the stamp repairable.** A re-stamp path — the same thing [T107] needs for re-pinning a
   superseded schema version. Both are "this row was written under facts that have since changed";
   solving them together is probably cheaper than either alone.

Option 3 is the most useful long-term and shares its machinery with [T107].

## Related

Fallout from [T101]. Shares a remedy with [T107].
