---
id: T146
title: A crediting activity type from another discipline can credit a trainee's curriculum
status: queued
priority: P3
owner: agent
model: opus
depends_on: []
created: 2026-09-23
---

# T146 — A type scoped to one discipline credits whatever curriculum its subject is on

**Severity:** Low today, and a design question rather than a live leak. The type menu hides the case that matters on
the seeded data. Nothing on the write path refuses it.
**Surfaced:** 2026-09-23, by [T122]'s design critique; its premise was then corrected on the dev database.

## Symptom

`ActivityService.CreateDraftAsync` never checks that the subject trains inside the activity type's scope.
(`EnsureSubjectIsInTypeScope` exists, but only the MSF staged path calls it.) The credit engine resolves items from
the SUBJECT's curriculum, whatever the type's scope. So a Speciality-scoped General Medicine type filed about a
paediatric trainee credits the paediatric curriculum.

After [T122]:
- The generic `mini_cex`, `dops` and `cbd` carry their instrument keys, so they obey each EPA's tool list like the CPSA
  seeds.
- **`acat` has no College name, so it is unkeyed and unrestricted (D21)**, and a General Medicine ACAT credits any
  paediatric EPA.
- **The menu hides it on dev:** the generic seeds' `scale_key` resolves to the O-R Scale, and [T123] d3 drops a type
  whose ladder no item of the subject's curriculum is pinned to.
- So the case is reachable only below the UI: a crafted request, or a trainee holding claims in two disciplines on a
  curriculum with unpinned items.

## Root cause

A type's scope has only ever been a menu filter. D20 and D21 decided the tool question; nobody has decided whether a
type's scope should bind what it may credit.

## What to build

Decide the rule first, and record it as a product decision. The candidate: a type that declares credit may be filed
about a subject with a profile only if the subject's curriculum lies inside the type's scope. The comparisons:
- Global passes.
- Institution: the profile's `InstitutionId`.
- Speciality: the curriculum's sub-speciality's speciality.
- SubSpeciality: the curriculum's sub-speciality.
Refuse at create, before `Add`. Add the same conjunct to `ListActivityTypesQuery`, so the menu does not offer what
create refuses. Non-crediting types, and subjects with no profile, are unaffected.

## Verification

- [ ] A General Medicine `acat` filed about a paediatric-profile subject is refused at create, with a message carrying
      no raw ids — `ActivityService` test
- [ ] The CPSA and institution-scoped types are unaffected — tests on the seeded catalogue
- [ ] The menu and the write path agree — `ListActivityTypesNarrowingTests`

## Related

[T122], D20, D21, [T123] d3, [T121] (`EnsureSubjectIsInTypeScope`).
