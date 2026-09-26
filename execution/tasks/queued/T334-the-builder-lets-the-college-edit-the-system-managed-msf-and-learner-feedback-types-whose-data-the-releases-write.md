---
id: T334
title: The builder lets the College edit the system-managed MSF and learner-feedback types, whose data the releases write
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-26
---

# T334 — The builder lets the College edit the system-managed MSF and learner-feedback types, whose data the releases write

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** High, before real users. `msf_cpsa` and `learner_feedback_cpsa` are system-managed (T162, T164): the MSF
release and the learner-feedback release write their activities' data, so their schema and workflow are a contract
with code. Since T300 admitted the College to the builder (D52), its CollegeAdmin can open either and save a draft that
breaks that contract.
**Surfaced:** 2026-09-26, T300's adversarial review (its nit 8, deferred by the fix).

## Symptom

As a CollegeAdmin of the CPSA, open `/admin/activity-types`, then `msf_cpsa` or `learner_feedback_cpsa`: the builder
offers Save draft and Publish, and `SaveActivityTypeDraftCommand` accepts the save, because `ActivityTypeScopeGuard`
admits the owning College for any Speciality-scoped type and nothing reads `ActivityType.SystemManaged` there.

## Root cause

`ActivityTypeAdminScope.MayWrite` (the guard, the editor's `CanWrite` and the list's Edit) judges scope only. The
`SystemManaged` flag is read by the type picker and the create path (`ListActivityTypesQuery`, `IActivityService`), not
by the builder or its commands. Before T300 the College could not reach the builder, so only the Administrator could.

## What to build

- A system-managed type is read-only in the builder for everyone: `MayWrite` (and so the editor, the list's View, and
  the save, discard and publish commands) refuses it, with a refusal naming why ("Its data is written by the MSF
  release; its form changes only with the code").
- Its schema and workflow change only by migration or seed, as its tool key does.

## Verification

- [ ] Application test: Save draft, Discard draft and Publish on `msf_cpsa` and `learner_feedback_cpsa` are refused for
  the Administrator and the owning CollegeAdmin, and nothing is written after the audit pipeline's save.
- [ ] bUnit: the builder opens either read-only, and the list offers View, for every caller.
- [ ] Browser: as Dr Kruger, `msf_cpsa` opens read-only (a runbook step in Act 6).

## Related

T300 (D52), T162, T164, T091.
